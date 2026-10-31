"""Lotes das falas de traducao/RoleLogLocal.json sem tradução, para a sessão na nuvem (traducao/nuvem/LEIA.md).

Os lotes saem em ordem: primeiro os diálogos, na sequência dos ids para a conversa ficar junta; depois o resto. A sessão
para onde o crédito acabar. Não entram as linhas sem inglês. O `ch` fica de fora para o lote custar menos.

Uso: python tools/ptbr/nuvem_falas.py            gera traducao/nuvem/falas/src_fNN.json
     python tools/ptbr/nuvem_falas.py --juntar   leva os out_fNN.json sem erro para traducao/RoleLogLocal.json
Gerar recusa rodar se já houver saída (out_f*.json), para não trocar os lotes de lugar.
"""
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import nuvem_checar  # noqa: E402

REPO = Path(__file__).resolve().parents[2]
ROLELOG = REPO / 'traducao' / 'RoleLogLocal.json'
OUT = REPO / 'traducao' / 'nuvem' / 'falas'
MAX_CHARS = 12000
AUTO = 'nuvem/falas'


def ordem(item):
    key, entry = item
    dialogo = (entry.get('keyID') or '').startswith(('drama_dialogue', 'drama_option'))
    return (0 if dialogo else 1, int(key))


def gerar():
    if list(OUT.glob('out_f*.json')):
        sys.exit(f'{OUT} já tem saídas; apague-as de propósito antes de gerar os lotes de novo.')
    OUT.mkdir(parents=True, exist_ok=True)
    for path in OUT.glob('src_f*.json'):
        path.unlink()
    work = json.loads(ROLELOG.read_text(encoding='utf-8'))
    pendentes = [(k, v) for k, v in work.items() if not (v.get('pt') or '').strip() and (v.get('en') or '').strip()]
    lotes, atual, tamanho = [], {}, 0
    for key, entry in sorted(pendentes, key=ordem):
        if atual and tamanho + len(entry['en']) > MAX_CHARS:
            lotes.append(atual)
            atual, tamanho = {}, 0
        atual[key] = {'en': entry['en']}
        tamanho += len(entry['en'])
    if atual:
        lotes.append(atual)
    for i, lote in enumerate(lotes, 1):
        path = OUT / f'src_f{i:02}.json'
        path.write_text(json.dumps(lote, ensure_ascii=False, indent=1) + '\n', encoding='utf-8', newline='\n')
    print(f'{len(lotes)} lotes, {len(pendentes)} entradas, {sum(len(v["en"]) for _, v in pendentes)} caracteres')


def juntar():
    work = json.loads(ROLELOG.read_text(encoding='utf-8'))
    feitas = fora = 0
    for src_path in sorted(OUT.glob('src_f*.json')):
        out_path = src_path.with_name(src_path.name.replace('src_', 'out_'))
        if not out_path.exists():
            continue
        src = json.loads(src_path.read_text(encoding='utf-8'))
        for key, pt in json.loads(out_path.read_text(encoding='utf-8')).items():
            erros, _ = nuvem_checar.problemas(key, src[key], pt) if key in src else (['chave fora do lote'], [])
            if erros:
                print(f'{out_path.name}\t{key}\t{erros}')
                fora += 1
            elif not (work[key].get('pt') or '').strip():
                work[key]['pt'] = pt
                work[key]['auto'] = AUTO
                feitas += 1
    ROLELOG.write_text(json.dumps(work, ensure_ascii=False, indent=1) + '\n', encoding='utf-8', newline='\n')
    print(f'{feitas} juntadas, {fora} fora por erro')


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8')
    juntar() if '--juntar' in sys.argv[1:] else gerar()
