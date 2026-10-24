# /// script
# requires-python = ">=3.11"
# dependencies = ["httpx>=0.27", "mcp>=1.2,<2"]
# ///
"""Fila diária de tradução (roda sem o Claude).

Percorre as entradas sem pt por prioridade de categoria e distribui lotes entre os modelos de
tools/ptbr/modelos.json, em paralelo (uma linha por grupo de cota). Cada modelo para no primeiro
limite (429) e passa a vez. No fim junta as saídas (merge), roda validate e build e grava um relatório.

Uso: uv run --script tools/ptbr/fila.py [--simular] [--limite N] [--modelos id,id]
"""
import argparse
import datetime as dt
import importlib.util
import json
import os
import sys
import threading
import time
from pathlib import Path

import httpx

sys.path.insert(0, str(Path(__file__).resolve().parent))
import ptbr  # noqa: E402

BRIDGE_PATH = Path(os.environ.get('OMNI_BRIDGE_SERVER', Path.home() / 'tools/omni-bridge/server.py'))
_spec = importlib.util.spec_from_file_location('omni_bridge', BRIDGE_PATH)
bridge = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(bridge)

MODELOS = Path(__file__).resolve().parent / 'modelos.json'
FILA_DIR = ptbr.REPO / 'traducao/auto/fila'

# Prioridade por prefixo de chave (a primeira que casar vale). O que não casar entra como "interface (resto)",
# antes das narrativas.
PRIORIDADES = [
    ('configurações', ('setting_', 'gameSetting_', 'shezhi_', 'game_', 'diff', 'login_', 'defaultKeys',
                       'mobilePaper_', 'gameLevel_')),
    ('botões', ('tiaoguo', 'ditu', 'gaiming', 'map_', 'task_', 'ui_', 'common_', 'careerTime', 'log_riqi', 'system_')),
    ('reinos', ('role_grade', 'D_role_grade', 'upGrade')),
    ('status', ('player_', 'playerInfo_', 'PlayerInfo_', 'role_hobby_name', 'role_character_name', 'role_race',
                'schoolPostName', 'npcInfo_')),
    ('nomes de item', ('item_name', 'itemType_', 'item_type_name', 'item_clickToUse', 'item_useType')),
    ('Olho da Providência', ('GodEyeSkills_name', 'intoGame_text', 'battle_unit_name', 'potmonRace')),
    ('artefatos', ('artifactShape_', 'artifact_', 'artifactSprite')),
    ('reputação', ('role_reputation',)),
]
NARRATIVA = ('drama_dialogue', 'drama_option', 'item_desc', 'startGameTipDesc', 'taskDesc', 'SchoolTaskAllotTaskDesc',
             'fateStoryDesc', 'role_feature', 'spriteTalent', 'skill_attack_desc', 'prefixDescS', 'AppellationTitle',
             'item_book_desc', 'effect_desc', 'effect_value_desc', 'potmonSkillLifeDesc', 'GodEyeSkills_desc')
NAO_TRADUZIR = ('school_name_first', 'school_name_second')  # ver glossario/decisoes.md
INTERFACE_CURTA = 40      # caracteres do en para os modelos só de interface
LOTE = {'gemini': 40, 'cohere': 20, 'groq': 30}
MAX_TOKENS = 8000
TEMPO_MAXIMO = 4 * 3600
ERROS_SEGUIDOS = 3
PEDIDOS_MAXIMOS = 400     # por modelo e rodada; trava contra laço gastando cota
CAMPOS_PT = ('pt', 'pt-br', 'pt_br', 'pt-BR', 'pt_BR', 'tradução', 'traducao', 'translation')
# Mesmo texto do omni_translate da ponte, para as duas rotas traduzirem igual.
SYSTEM = ("Você traduz o jogo chinês de cultivo (xianxia) Tale of Immortal para português do Brasil. "
          "Traduza a partir do inglês ('en'); use o chinês ('ch') para entender nomes e termos que o inglês só romanizou. "
          "Regras: preserve exatamente todos os marcadores ({0}, {name|A}, &x&, $x$, <color=...>, </color>, \\n); "
          "textos curtos de interface continuam curtos; não explique nada. "
          "Responda SOMENTE com um objeto JSON {\"chave\": \"tradução\"} com as mesmas chaves recebidas.")


class Limite(Exception):
    """429: a cota do modelo (ou do grupo de cota) acabou."""


def prioridade(key):
    if key.startswith(NARRATIVA):
        return len(PRIORIDADES) + 1, 'narrativas'
    for i, (nome, prefixos) in enumerate(PRIORIDADES):
        if key.startswith(prefixos):
            return i, nome
    return len(PRIORIDADES), 'interface (resto)'


def carregar_modelos(filtro=None):
    data = json.loads(MODELOS.read_text(encoding='utf-8'))
    proibidos = data.get('proibidos', [])
    modelos = []
    for m in data['modelos']:
        if any(m['id'].startswith(p.rstrip('*')) for p in proibidos):
            raise SystemExit(f"modelo proibido em modelos.json: {m['id']}")
        if filtro and m['id'] not in filtro:
            continue
        modelos.append(m)
    return modelos


class Fila:
    def __init__(self, work, limite):
        self.work = work
        self.chaves = sorted((k for k, e in work.items()
                              if not e.get('pt') and e['en'].strip() and not k.startswith(NAO_TRADUZIR)),
                             key=lambda k: (prioridade(k)[0], k))
        if limite:
            self.chaves = self.chaves[:limite]
        self.pegas, self.feitas = set(), set()
        self.lock = threading.Lock()

    def pegar(self, n, so_curta, evitar=()):
        with self.lock:
            lote = []
            for k in self.chaves:
                if k in self.pegas or k in self.feitas or k in evitar:
                    continue
                if so_curta and (prioridade(k)[1] == 'narrativas' or len(self.work[k]['en']) > INTERFACE_CURTA):
                    continue
                lote.append(k)
                self.pegas.add(k)
                if len(lote) == n:
                    break
            return lote

    def devolver(self, chaves, feitas):
        with self.lock:
            self.pegas.difference_update(chaves)
            self.feitas.update(feitas)


def chamar(model, user, max_tokens):
    body = {'model': model, 'messages': [{'role': 'system', 'content': SYSTEM}, {'role': 'user', 'content': user}],
            'max_tokens': max_tokens, 'temperature': 0.2, 'stream': False}
    r = httpx.post(f'{bridge.BASE_URL}/chat/completions', json=body,
                   headers={'Authorization': f'Bearer {bridge.api_key()}'}, timeout=180)
    if r.status_code == 429:
        raise Limite(r.text[:800])
    if r.status_code >= 400:
        raise RuntimeError(f'HTTP {r.status_code}: {r.text[:300]}')
    data = r.json()
    return (data.get('choices') or [{}])[0].get('message', {}).get('content') or '', data.get('usage') or {}


def ler_resposta(texto):
    """JSON {chave: tradução}. Aceita valores como objeto ({"pt-br": ...}), que o cohere às vezes devolve
    e o parse_json_object da ponte transforma em texto vazio."""
    a, b = texto.find('{'), texto.rfind('}')
    if a < 0 or b <= a:
        raise ValueError('resposta sem JSON')
    obj = json.loads(texto[a:b + 1])
    if isinstance(obj, dict) and len(obj) == 1 and isinstance(next(iter(obj.values())), dict):
        inner = next(iter(obj.values()))
        if inner and all(isinstance(v, (str, dict)) for v in inner.values()):
            obj = inner
    out = {}
    for k, v in obj.items():
        if isinstance(v, dict):
            v = next((v[c] for c in CAMPOS_PT if isinstance(v.get(c), str)), '')
        out[k] = v if isinstance(v, str) else ''
    return out


def julgar(entry, pt):
    """ok da ponte + regras do CLAUDE.md que a ponte não aplica (texto idêntico ao inglês, erro de validação)."""
    src = entry['en'] or entry['ch']
    if not bridge.placeholders_ok(src, pt):
        return False
    en = entry['en'].strip()
    if pt.strip() == en and len(en) > 4 and any(c.islower() for c in en):
        return False
    errors, _ = ptbr.check({**entry, 'pt': pt})
    return not errors


def trabalhar(grupo, modelos, fila, glossary, saida_dir, stats, fim):
    for m in modelos:
        st = stats[m['id']]
        provedor = m['id'].split('/')[0]
        n = LOTE.get(provedor, 20)
        max_tokens = min(m.get('max_tokens') or MAX_TOKENS, MAX_TOKENS)
        arquivo = saida_dir / (m['id'].replace('/', '__') + '.json')
        out = json.loads(arquivo.read_text(encoding='utf-8')) if arquivo.exists() else {}
        seguidos = 0
        sem_resposta = set()  # chaves que este modelo devolveu vazias: ficam para os outros modelos
        while time.time() < fim:
            if st['pedidos'] + len(st['erros']) >= PEDIDOS_MAXIMOS:
                st['fim'] = f'{PEDIDOS_MAXIMOS} pedidos (teto da rodada)'
                break
            lote = fila.pegar(n, m.get('so_interface_curta'), sem_resposta)
            if not lote:
                st['fim'] = 'fila vazia'
                break
            batch = {k: {'en': fila.work[k]['en'], 'ch': fila.work[k]['ch']} for k in lote}
            gl = bridge.glossary_for(batch, glossary)
            user = (f'Glossário obrigatório:\n{gl}\n\n' if gl else '') + json.dumps(batch, ensure_ascii=False)
            t0 = time.time()
            try:
                texto, usage = chamar(m['id'], user, max_tokens)
                got = ler_resposta(texto)
            except Limite as e:
                fila.devolver(lote, [])
                st['fim'] = 'limite (429)'
                st['erro_limite'] = str(e)
                # Grupo de cota compartilhada: o próximo modelo do grupo esbarraria no mesmo limite.
                return
            except Exception as e:  # noqa: BLE001 - qualquer falha do lote devolve as chaves à fila
                fila.devolver(lote, [])
                st['erros'].append(str(e)[:300])
                seguidos += 1
                if seguidos >= ERROS_SEGUIDOS:
                    st['fim'] = f'{ERROS_SEGUIDOS} erros seguidos'
                    break
                continue
            st['pedidos'] += 1
            st['segundos'] += time.time() - t0
            for k, u in usage.items():
                if isinstance(u, (int, float)):
                    st['tokens'][k] = st['tokens'].get(k, 0) + u
            feitas = []
            for k in lote:
                pt = got.get(k)
                if not isinstance(pt, str) or not pt.strip():
                    st['faltando'] += 1
                    sem_resposta.add(k)
                    continue
                ok = julgar(fila.work[k], pt)
                out[k] = {'pt': pt, 'ok': ok, 'model': m['id']}
                feitas.append(k)
                st['entradas'] += 1
                st['ok_false'] += not ok
            fila.devolver(lote, feitas)
            arquivo.write_text(json.dumps(out, ensure_ascii=False, indent=1), encoding='utf-8')
            if feitas:
                seguidos = 0
            else:
                st['erros'].append(f'lote sem nenhuma tradução aproveitável: {texto[:200]}')
                seguidos += 1
                if seguidos >= ERROS_SEGUIDOS:
                    st['fim'] = f'{ERROS_SEGUIDOS} lotes seguidos sem tradução'
                    break
        else:
            st['fim'] = 'tempo máximo'
            return


def relatorio(caminho, hoje, stats, fila, inicio, merge_txt, validate_txt):
    restantes = {}
    for k in fila.chaves:
        if k not in fila.feitas:
            nome = prioridade(k)[1]
            restantes[nome] = restantes.get(nome, 0) + 1
    feitas = {}
    for k in fila.feitas:
        nome = prioridade(k)[1]
        feitas[nome] = feitas.get(nome, 0) + 1
    linhas = [f'# Fila de tradução — {hoje}', '',
              f'Duração: {(time.time() - inicio) / 60:.0f} min. Entradas traduzidas: {len(fila.feitas)}. '
              f'Pendentes na fila: {len(fila.chaves) - len(fila.feitas)}.', '',
              '| Modelo | Pedidos | Entradas | ok=false | Faltando | s/pedido | Fim |', '|---|---|---|---|---|---|---|']
    for mid, st in stats.items():
        media = st['segundos'] / st['pedidos'] if st['pedidos'] else 0
        linhas.append(f"| `{mid}` | {st['pedidos']} | {st['entradas']} | {st['ok_false']} | {st['faltando']} | "
                      f"{media:.0f} | {st.get('fim', 'não rodou')} |")
    linhas += ['', '## Por categoria', '', '| Grupo | Traduzidas hoje | Restantes |', '|---|---|---|']
    for nome in [n for n, _ in PRIORIDADES] + ['interface (resto)', 'narrativas']:
        linhas.append(f'| {nome} | {feitas.get(nome, 0)} | {restantes.get(nome, 0)} |')
    linhas += ['', '## Limites e erros', '']
    for mid, st in stats.items():
        if st.get('erro_limite'):
            linhas.append(f"- `{mid}`: {st['erro_limite']}")
        for e in st['erros'][:3]:
            linhas.append(f'- `{mid}` (erro): {e}')
    linhas += ['', '## Merge e validate', '', '```', merge_txt.strip(), validate_txt.strip(), '```', '',
               'Nada foi commitado: revise e faça o commit (traducao/LocalText e ModAssets/ptbr/LocalText.json).']
    caminho.write_text('\n'.join(linhas) + '\n', encoding='utf-8')


def capturar(func, *args):
    import contextlib
    import io
    buf = io.StringIO()
    with contextlib.redirect_stdout(buf):
        func(*args)
    return buf.getvalue()


def main():
    sys.stdout.reconfigure(encoding='utf-8')
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument('--simular', action='store_true', help='mostra o plano sem chamar modelos')
    ap.add_argument('--limite', type=int, default=0, help='traduz no máximo N entradas (teste)')
    ap.add_argument('--modelos', default='', help='só estes IDs, separados por vírgula (teste)')
    args = ap.parse_args()

    hoje = dt.date.today().isoformat()
    saida_dir = FILA_DIR / hoje
    saida_dir.mkdir(parents=True, exist_ok=True)
    log = (FILA_DIR / 'fila.log').open('a', encoding='utf-8')
    def registrar(msg):
        linha = f'{dt.datetime.now():%Y-%m-%d %H:%M:%S} {msg}'
        print(linha)
        log.write(linha + '\n')
        log.flush()

    # Saídas de rodadas anteriores que não chegaram ao merge (queda no meio) entram antes de montar a fila.
    antigas = sorted(str(p) for p in FILA_DIR.glob('*/*.json'))
    if antigas and not args.simular:
        registrar('merge de saídas anteriores: ' + capturar(ptbr.cmd_merge, argparse.Namespace(files=antigas)).strip())

    modelos = carregar_modelos(set(args.modelos.split(',')) if args.modelos else None)
    work = ptbr.load_work()
    fila = Fila(work, args.limite)
    grupos = {}
    for m in modelos:
        grupos.setdefault(m.get('cota') or m['id'], []).append(m)
    resumo = {}
    for k in fila.chaves:
        nome = prioridade(k)[1]
        resumo[nome] = resumo.get(nome, 0) + 1
    registrar(f'fila: {len(fila.chaves)} entradas {resumo}')
    registrar('grupos de cota: ' + '; '.join(f"{g}: {', '.join(m['id'] for m in ms)}" for g, ms in grupos.items()))
    if args.simular:
        return 0

    glossary = json.loads((ptbr.GLOSS_DIR / 'glossario.json').read_text(encoding='utf-8'))
    stats = {m['id']: {'pedidos': 0, 'entradas': 0, 'ok_false': 0, 'faltando': 0, 'segundos': 0.0, 'tokens': {},
                       'erros': []} for m in modelos}
    inicio = time.time()
    fim = inicio + TEMPO_MAXIMO
    threads = [threading.Thread(target=trabalhar, args=(g, ms, fila, glossary, saida_dir, stats, fim), daemon=True)
               for g, ms in grupos.items()]
    for t in threads:
        t.start()
    for t in threads:
        t.join()
    for mid, st in stats.items():
        registrar(f"{mid}: {st['pedidos']} pedidos, {st['entradas']} entradas, {st['ok_false']} ok=false, "
                  f"fim: {st.get('fim', 'não rodou')}")

    arquivos = sorted(str(p) for p in saida_dir.glob('*.json'))
    merge_txt = capturar(ptbr.cmd_merge, argparse.Namespace(files=arquivos)) if arquivos else 'nada a juntar\n'
    validate_txt = capturar(ptbr.cmd_validate, argparse.Namespace(report=str(ptbr.REPO / 'traducao/validacao.txt')))
    validate_txt += capturar(ptbr.cmd_build, argparse.Namespace())
    rel = FILA_DIR / f'relatorio-{hoje}.md'
    relatorio(rel, hoje, stats, fila, inicio, merge_txt, validate_txt)
    registrar(f'relatório: {rel}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
