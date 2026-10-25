"""Confere a saída de um lote traduzido na sessão na nuvem (traducao/nuvem/LEIA.md).

Entrada: o lote (chave -> {en, ch, nomes?}) e a saída (chave -> tradução). Usa os mesmos critérios da fila:
local_judge (marcadores, caracteres fora do ASCII e dos acentos latinos, texto idêntico ao inglês, barras),
o check do validate e o glossário obrigatório. Não altera nada.

Uso: python tools/ptbr/nuvem_checar.py [--parcial] traducao/nuvem/src_piloto.json traducao/nuvem/out_piloto.json
--parcial: a saída só tem as entradas corrigidas (revisão); as que faltam não contam como erro.
Sai com 1 se houver ERRO (a sessão corrige antes do commit); AVISO não bloqueia, mas deve ser lido.
"""
import collections
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import ptbr  # noqa: E402


def problemas(key, entry, pt):
    """(erros, avisos) de uma tradução."""
    en = entry['en']
    erros, avisos = [], []
    if not isinstance(pt, str) or not pt.strip():
        return ['tradução vazia'], []
    a = collections.Counter(ptbr.PLACEHOLDER.findall(en))
    b = collections.Counter(ptbr.PLACEHOLDER.findall(pt))
    if a != b:
        erros.append(f'marcadores: faltando {list((a - b).elements())} sobrando {list((b - a).elements())}')
    fora = sorted({c for c in pt if ord(c) >= 128 and not 'À' <= c <= 'ÿ' and c not in en})
    if fora:
        erros.append(f'caracteres não permitidos: {fora} (use aspas, reticências e hífen do teclado)')
    if '/' in pt and '/' not in en:
        erros.append('barra que o en não tem (alternativas?)')
    if not erros and not ptbr.local_judge(en, pt, key=key):
        erros.append('idêntico ao inglês')
    check_erros, check_avisos = ptbr.check({'en': en, 'ch': entry.get('ch', ''), 'pt': pt})
    # Divergência de marcador já relatada acima não se repete como erro de token/tag do validate.
    erros += [e for e in check_erros if not (a != b and e.startswith(('token', 'tag')))]
    avisos += check_avisos
    avisos += [f'glossário: {m}' for m in ptbr.glossary_misses({'en': en, 'ch': entry.get('ch', ''), 'pt': pt}, key)]
    for nome_en, nome_pt in (entry.get('nomes') or {}).items():
        if ptbr.fold(nome_pt) not in ptbr.fold(pt):
            avisos.append(f'nome dado não usado: {nome_en} → {nome_pt}')
    return erros, avisos


def main():
    sys.stdout.reconfigure(encoding='utf-8')
    args = [a for a in sys.argv[1:] if a != '--parcial']
    parcial = '--parcial' in sys.argv[1:]
    if len(args) != 2:
        print(__doc__)
        return 2
    src = json.loads(Path(args[0]).read_text(encoding='utf-8'))
    out_path = Path(args[1])
    out = json.loads(out_path.read_text(encoding='utf-8')) if out_path.exists() else {}
    n_err = n_av = 0
    for key in src:
        if key not in out and parcial:
            continue
        if key not in out:
            print(f'ERRO\t{key}\tsem tradução na saída')
            n_err += 1
            continue
        erros, avisos = problemas(key, src[key], out[key])
        n_err += bool(erros)
        n_av += bool(avisos)
        for e in erros:
            print(f'ERRO\t{key}\t{e}')
        for a in avisos:
            print(f'AVISO\t{key}\t{a}')
    for key in out:
        if key not in src:
            print(f'ERRO\t{key}\tchave que não está no lote')
            n_err += 1
    n = len(out) if parcial else len(src)
    print(f'{n} entradas conferidas | {n - n_err} sem erro | {n_err} com erro | {n_av} com aviso')
    return 1 if n_err else 0


if __name__ == '__main__':
    sys.exit(main())
