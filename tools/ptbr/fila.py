# /// script
# requires-python = ">=3.11"
# dependencies = ["httpx>=0.27", "mcp>=1.2,<2"]
# ///
"""Fila diária de tradução (roda sem o Claude).

1. Hy-MT2 local (tradutor principal): junta o que o `ptbr.py local` já traduziu em todos os lotes (ok=true)
   e, se nenhum `local` estiver vivo, inicia o `local --continuo`, que segue lote atrás de lote sozinho.
2. As linhas que o Hy-MT2 devolveu com ok=false passam pelos modelos de API na ordem de modelos.json.
3. Os modelos de API adiantam, em paralelo (uma linha por grupo de cota), entradas que reservam para si
   (ptbr.reserve_for_apis), nunca as dos lotes do local. Cada modelo para no primeiro limite (429) e passa a vez.
No fim junta as saídas (merge), roda validate e build, solta a reserva e grava um relatório. Só a fila escreve
em traducao/LocalText.

Uso: uv run --script tools/ptbr/fila.py [--simular] [--limite N] [--modelos id,id] [--sem-local]
"""
import argparse
import collections
import datetime as dt
import importlib.util
import json
import os
import subprocess
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
RESERVA_APIS = int(os.environ.get('PTBR_RESERVA_APIS', 1000))  # entradas por reserva; amplia quando acaba

PRIORIDADES, NARRATIVA, NAO_TRADUZIR = ptbr.PRIORIDADES, ptbr.NARRATIVA, ptbr.NAO_TRADUZIR
prioridade, pendentes = ptbr.prioridade, ptbr.pendentes
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


def carregar_modelos(filtro=None):
    """Modelos de API, na ordem de modelos.json (o local é tratado à parte, em etapa_local)."""
    data = json.loads(MODELOS.read_text(encoding='utf-8'))
    proibidos = data.get('proibidos', [])
    modelos = []
    for m in data['modelos']:
        if any(m['id'].startswith(p.rstrip('*')) for p in proibidos):
            raise SystemExit(f"modelo proibido em modelos.json: {m['id']}")
        if m.get('tipo') == 'local' or (filtro and m['id'] not in filtro):
            continue
        modelos.append(m)
    return modelos


class Fila:
    def __init__(self, work, chaves, primeiras=(), reservar=None):
        """`chaves`: reservadas para as APIs; `primeiras` vão antes (falhas do Hy-MT2).
        `reservar(n)` reserva mais n quando a fila acaba (None: não amplia, como em --limite)."""
        self.work = work
        pend, prim = set(pendentes(work)), set(primeiras)
        self.chaves = [k for k in primeiras if k in pend] + [k for k in chaves if k not in prim]
        self.reservar = reservar
        self.pegas, self.feitas = set(), set()
        self.so, self.fora = None, set()  # só estas chaves / nunca estas (ver main)
        self.lock = threading.Lock()

    def pegar(self, n, so_curta, evitar=()):
        with self.lock:
            lote = self._pegar(n, so_curta, evitar)
            # Reserva acabou: pede mais (os modelos só de interface curta não ampliam, senão reservariam tudo).
            if not lote and not so_curta and self.so is None and self.reservar:
                novas = self.reservar(RESERVA_APIS)
                if novas:
                    self.chaves += novas
                    lote = self._pegar(n, so_curta, evitar)
            return lote

    def _pegar(self, n, so_curta, evitar):
        lote = []
        for k in self.chaves:
            if k in self.pegas or k in self.feitas or k in evitar or k in self.fora:
                continue
            if self.so is not None and k not in self.so:
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
    if pt.strip() == en and len(en) > 4 and any(c.islower() for c in en) and not ptbr.igual_permitido(en):
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


def omni_no_ar():
    """Qualquer resposta HTTP (até 401) conta como no ar, como no Iniciar-Traducao.bat; só falha de conexão não."""
    try:
        httpx.get(f'{bridge.BASE_URL}/models', timeout=5)
        return True
    except httpx.TransportError:
        return False


def iniciar_local():
    """Deixa o `ptbr.py local --continuo` rodando sozinho: sem console e sem handles herdados, ele continua se a
    fila ou a janela do Iniciar-Traducao.bat fecharem. A saída vai para o log que ele mesmo abre (--log)."""
    cmd = [sys.executable, '-u', str(Path(ptbr.__file__)), 'local', '--continuo', '--log', str(ptbr.LOCAL_LOG)]
    flags = getattr(subprocess, 'DETACHED_PROCESS', 0) | getattr(subprocess, 'CREATE_NEW_PROCESS_GROUP', 0)
    # CREATE_BREAKAWAY_FROM_JOB tira o local do job do `uv run`; se o job não permitir, roda sem ele.
    for extra in (getattr(subprocess, 'CREATE_BREAKAWAY_FROM_JOB', 0), 0):
        try:
            proc = subprocess.Popen(cmd, stdin=None, stdout=None, stderr=None, close_fds=True,
                                    cwd=ptbr.REPO, creationflags=flags | extra)
            break
        except OSError as e:
            erro = e
    else:
        raise erro
    return f'`local --continuo` iniciado (pid {proc.pid}); log em {ptbr.LOCAL_LOG.name}'


def etapa_local(work, simular, hymt2_no_ar):
    """Junta as ok=true de todos os lotes do Hy-MT2 e garante um `local --continuo` vivo.

    Devolve (falhas, linhas): chaves que o Hy-MT2 devolveu com ok=false (vão para as APIs) e o resumo para log
    e relatório. As chaves dos lotes sem saída continuam do local (claimed_keys), e as APIs não as pegam."""
    ptbr.LOCAL_DIR.mkdir(parents=True, exist_ok=True)
    falhas, juntar, sem_saida = [], {}, {}
    for src in ptbr.local_batches():
        out = ptbr.read_json(ptbr.batch_output(src), {})
        for k in ptbr.read_json(src, {}):
            e = work.get(k)
            if e is None or e.get('pt'):
                continue
            o = out.get(k)
            if o is None:
                sem_saida[src.name] = sem_saida.get(src.name, 0) + 1
            elif o.get('ok') and ptbr.local_judge(e['en'], o.get('pt', '')):
                juntar[k] = o
            else:
                falhas.append(k)
    linhas = []
    if juntar and not simular:
        st = collections.Counter()
        ptbr.merge_outputs(work, juntar, st)
        ptbr.save_work(work)
        linhas.append('merge: ' + ', '.join(f'{k}: {v}' for k, v in st.items()))
    elif juntar:
        linhas.append(f'merge (simulado): {len(juntar)} ok=true a juntar')
    linhas.append(f'ok=false para as APIs: {len(falhas)}')
    linhas.append(f'linhas do local ainda sem saída: {sum(sem_saida.values())} {sem_saida}')

    if ptbr.local_running():
        linhas.append('`local` já rodando; não inicio outro')
    elif not hymt2_no_ar:
        linhas.append('local parado até o Hy-MT2 voltar')
    elif simular:
        linhas.append('`local --continuo` seria iniciado (simulado)')
    else:
        # Reserva o próximo lote antes de iniciar, para o Hy-MT2 (principal) ficar com o topo da fila.
        src = None if sem_saida else ptbr.new_local_batch()
        if src:
            linhas.append(f'novo lote {src.name}')
        if sem_saida or src:
            linhas.append(iniciar_local())
        else:
            linhas.append('sem pendentes livres para o local')
    return falhas, linhas


def relatorio(caminho, hoje, stats, fila, inicio, merge_txt, validate_txt, local_linhas, servidores):
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
              '## Servidores', ''] + [f'- {l}' for l in servidores] + ['',
              '## Hy-MT2 local', ''] + [f'- {l}' for l in local_linhas] + ['',
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
    ap.add_argument('--sem-local', action='store_true', help='não mexe no lote do Hy-MT2 local')
    args = ap.parse_args()

    hoje = dt.date.today().isoformat()
    saida_dir = ptbr.FILA_DIR / hoje
    saida_dir.mkdir(parents=True, exist_ok=True)
    log = (ptbr.FILA_DIR / 'fila.log').open('a', encoding='utf-8')
    def registrar(msg):
        linha = f'{dt.datetime.now():%Y-%m-%d %H:%M:%S} {msg}'
        print(linha)
        log.write(linha + '\n')
        log.flush()

    # Saídas de rodadas anteriores que não chegaram ao merge (queda no meio) entram antes de montar a fila.
    antigas = sorted(str(p) for p in ptbr.FILA_DIR.glob('*/*.json'))
    if antigas and not args.simular:
        registrar('merge de saídas anteriores: ' + capturar(ptbr.cmd_merge, argparse.Namespace(files=antigas)).strip())

    # Servidor fora do ar pula só a etapa dele, sem gastar erros por modelo.
    omni_ok, hymt2_ok = omni_no_ar(), ptbr.local_alive() == ptbr.LOCAL_ID
    servidores = [f'OmniRoute {"no ar" if omni_ok else "fora do ar"} ({bridge.BASE_URL})',
                  f'Hy-MT2 {"no ar" if hymt2_ok else "fora do ar"} ({ptbr.LOCAL_URL})']
    for l in servidores:
        registrar(l)
    if not omni_ok:
        servidores.append('etapas das APIs puladas; suba o OmniRoute pelo Iniciar-Traducao.bat')
    if not hymt2_ok:
        servidores.append(f'tradução local pulada; rode {ptbr.LOCAL_START} ou o Iniciar-Traducao.bat')

    modelos = carregar_modelos(set(args.modelos.split(',')) if args.modelos else None)
    if not omni_ok:
        modelos = []
    work = ptbr.load_work()
    falhas, local_linhas = [], ['etapa local pulada (--sem-local)']
    if not args.sem_local:
        falhas, local_linhas = etapa_local(work, args.simular, hymt2_ok)
    for l in local_linhas:
        registrar('Hy-MT2: ' + l)
    if args.simular or not modelos:
        donos = ptbr.claimed_keys()
        livres = [k for k in pendentes(work) if k not in donos]
        fila = Fila(work, livres[:args.limite] if args.limite else livres, primeiras=falhas)
    else:
        fila = Fila(work, ptbr.reserve_for_apis(args.limite or RESERVA_APIS), primeiras=falhas,
                    reservar=None if args.limite else ptbr.reserve_for_apis)
    grupos = {}
    for m in modelos:
        grupos.setdefault(m.get('cota') or m['id'], []).append(m)
    resumo = {}
    for k in fila.chaves:
        nome = prioridade(k)[1]
        resumo[nome] = resumo.get(nome, 0) + 1
    if args.simular or not modelos:
        registrar(f'fila das APIs (sem reservar): {len(fila.chaves)} entradas livres {resumo}')
    else:
        registrar(f'fila das APIs: {len(fila.chaves)} entradas reservadas {resumo} (amplia de {RESERVA_APIS} em '
                  f'{RESERVA_APIS} se acabar)')
    registrar('grupos de cota: ' + '; '.join(f"{g}: {', '.join(m['id'] for m in ms)}" for g, ms in grupos.items()))
    if args.simular:
        return 0
    try:
        return rodar(args, hoje, saida_dir, registrar, modelos, grupos, fila, falhas, local_linhas, servidores)
    finally:
        ptbr.release_api_reserve()


def rodar(args, hoje, saida_dir, registrar, modelos, grupos, fila, falhas, local_linhas, servidores):
    glossary = json.loads((ptbr.GLOSS_DIR / 'glossario.json').read_text(encoding='utf-8'))
    stats = {m['id']: {'pedidos': 0, 'entradas': 0, 'ok_false': 0, 'faltando': 0, 'segundos': 0.0, 'tokens': {},
                       'erros': []} for m in modelos}
    inicio = time.time()
    fim = inicio + TEMPO_MAXIMO
    # Falhas do Hy-MT2: um modelo por vez, na ordem de modelos.json; um 429 esgota o grupo de cota inteiro.
    esgotadas = set()
    fila.so = set(falhas) & set(fila.chaves)
    for m in modelos:
        if not fila.so - fila.feitas:
            break
        cota = m.get('cota') or m['id']
        if cota not in esgotadas:
            trabalhar(cota, [m], fila, glossary, saida_dir, stats, fim)
            if stats[m['id']].get('fim') == 'limite (429)':
                esgotadas.add(cota)
    if fila.so:
        registrar(f'falhas do Hy-MT2 refeitas pelas APIs: {len(fila.so & fila.feitas)}/{len(fila.so)}')
    # As que nenhuma API refez ficam para amanhã; a etapa paralela não insiste nelas.
    fila.fora, fila.so = fila.so - fila.feitas, None
    grupos = {g: ms for g, ms in grupos.items() if g not in esgotadas}
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
    rel = ptbr.FILA_DIR / f'relatorio-{hoje}.md'
    relatorio(rel, hoje, stats, fila, inicio, merge_txt, validate_txt, local_linhas, servidores)
    registrar(f'relatório: {rel}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
