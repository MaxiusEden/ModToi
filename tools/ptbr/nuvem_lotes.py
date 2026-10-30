"""Gera os lotes de revisão por prioridade para a sessão na nuvem (traducao/nuvem/LEIA.md).

Os lotes saem em ordem: o p001 é o mais importante, e a sessão para onde o crédito acabar. O grupo A é o que nunca
foi revisado (Cohere, Gemini e Hy-MT2 sem revisão); o grupo B é a segunda leitura das missões e da história.
Não entram as entradas revisadas à mão, as sem inglês e as que ficam em chinês de propósito.

Uso: python tools/ptbr/nuvem_lotes.py
Não altera traducao/LocalText. Recusa rodar se já houver saída (out_p*.json), para não trocar os lotes de lugar.
"""
import glob
import json
import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
LOCALTEXT = REPO / 'traducao' / 'LocalText'
NUVEM = REPO / 'traducao' / 'nuvem'
OUT = NUVEM / 'prioridade'
MAX_CHARS = 25000
HANZI = re.compile(r'[一-鿿]')

# Ordem de cada grupo: arquivo a arquivo, do que mais aparece na tela ao que menos aparece.
GRUPO_A = [
    # Interface
    'ui', 'common', 'school', 'player', 'npcInfo', 'town', 'battle', 'keyboard', 'item_type_name', 'D', 'role',
    '_diversos',
    # Nomes
    'item_name', 'effect_name', 'ability_name', 'GodEyeSkills_name', 'battle_unit_name', 'battle_dungeon_name',
    'dungenonSceneName', 'fortuitous_event_name', 'SchoolTaskAllotTaskName', 'MonthLog_title', 'achievementTitle',
    # Descrições
    'fateDesc', 'achievementDesc', 'battle_dungeon_info', 'battleAbility', 'item',
]
GRUPO_B = ['taskName', 'taskDesc', 'drama_dialogue', 'drama_option']
FORA = {'startGameTipDesc', '_outros_mods', 'wenjuan'}


def carregar():
    arquivos = {}
    for path in sorted(LOCALTEXT.glob('*.json')):
        arquivos[path.stem] = json.loads(path.read_text(encoding='utf-8'))
    return arquivos


def nomes_antigos():
    """Os nomes aprovados que os lotes anteriores já traziam para cada chave."""
    nomes = {}
    for pattern in ('src_*.json', 'revisao/src_rev*.json'):
        for path in glob.glob(str(NUVEM / pattern)):
            for key, entry in json.loads(Path(path).read_text(encoding='utf-8')).items():
                if entry.get('nomes'):
                    nomes[key] = entry['nomes']
    return nomes


def revisadas_na_nuvem():
    chaves = set()
    for path in glob.glob(str(NUVEM / 'revisao' / 'src_rev*.json')):
        chaves |= set(json.loads(Path(path).read_text(encoding='utf-8')))
    return chaves


def elegivel(entry):
    pt = (entry.get('pt') or '').strip() if isinstance(entry, dict) else ''
    en = (entry.get('en') or '').strip() if isinstance(entry, dict) else ''
    return bool(pt and en) and not entry.get('revisado') and not HANZI.search(pt)


def fila(arquivos):
    """Devolve [(grupo, chave, entrada)] na ordem de prioridade."""
    revisadas = revisadas_na_nuvem()

    def nunca_revisada(key, entry):
        return key not in revisadas and not str(entry.get('auto') or '').startswith('nuvem')

    itens = []
    vistos = set()
    resto = [n for n in sorted(arquivos) if n not in GRUPO_A and n not in FORA]
    for nome in GRUPO_A + resto:
        for key, entry in arquivos.get(nome, {}).items():
            if elegivel(entry) and nunca_revisada(key, entry):
                itens.append(('A', key, entry))
                vistos.add(key)
    for nome in GRUPO_B:
        for key, entry in arquivos.get(nome, {}).items():
            if elegivel(entry) and key not in vistos:
                itens.append(('B', key, entry))
    return itens


def montar(grupo, key, entry, nomes):
    lote = {'en': entry['en'], 'pt': entry['pt']}
    # Na história, o chinês pesa cerca de um terço do lote e quase nunca muda a correção.
    if grupo == 'A' and entry.get('ch'):
        lote['ch'] = entry['ch']
    if key in nomes:
        lote['nomes'] = nomes[key]
    return lote


def main():
    if list(OUT.glob('out_p*.json')):
        sys.exit(f'{OUT} já tem saídas; apague-as de propósito antes de gerar os lotes de novo.')
    OUT.mkdir(parents=True, exist_ok=True)
    for path in OUT.glob('src_p*.json'):
        path.unlink()
    nomes = nomes_antigos()
    lotes, atual, tamanho, grupo_atual = [], {}, 0, None
    for grupo, key, entry in fila(carregar()):
        lote = montar(grupo, key, entry, nomes)
        chars = sum(len(str(v)) for v in lote.values()) + len(key)
        if atual and (grupo != grupo_atual or tamanho + chars > MAX_CHARS):
            lotes.append((grupo_atual, atual, tamanho))
            atual, tamanho = {}, 0
        atual[key] = lote
        tamanho += chars
        grupo_atual = grupo
    if atual:
        lotes.append((grupo_atual, atual, tamanho))
    linhas = ['# Lotes por prioridade', '', 'Lote | Grupo | Entradas | Caracteres | Primeira chave', '--- | --- | --- | --- | ---']
    for i, (grupo, lote, tamanho) in enumerate(lotes, 1):
        path = OUT / f'src_p{i:03}.json'
        path.write_text(json.dumps(lote, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')
        linhas.append(f'p{i:03} | {grupo} | {len(lote)} | {tamanho} | {next(iter(lote))}')
    (OUT / 'INDICE.md').write_text('\n'.join(linhas) + '\n', encoding='utf-8')
    for grupo in 'AB':
        sel = [x for x in lotes if x[0] == grupo]
        print(f'grupo {grupo}: {len(sel)} lotes, {sum(len(x[1]) for x in sel)} entradas, '
              f'{sum(x[2] for x in sel)} caracteres')


if __name__ == '__main__':
    main()
