"""Pipeline da tradução PT-BR de LocalText: extract (dump de runtime -> traducao/), validate, build (-> ModAssets)."""
import argparse
import collections
import json
import os
import re
import sys
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
GAME_DIR = Path(os.environ.get('GAME_DIR', r'C:\Games\Tale of Immortal'))
MOD_ID = '6b2SYs'
KIT_JSON = GAME_DIR / 'Mod/modFQA/配置修改教程/配置（只读）Json格式/LocalText.json'
DUMP = GAME_DIR / f'ModExportData/Mod_{MOD_ID}/ptbr_missing_LocalText.txt'
WORK_DIR = REPO / 'traducao/LocalText'
OUT_JSON = REPO / f'ModProject_{MOD_ID}/ModProject/ModAssets/ptbr/LocalText.json'

# Tokens que o jogo substitui em runtime e precisam sobreviver intactos à tradução.
TOKEN_RE = re.compile(r'\{[^{}\s]*\}|&[A-Za-z0-9_.%+\-*/|]+&|\$[A-Za-z0-9_]+\$|%[0-9.]*[sdf]')
TAG_RE = re.compile(r'</?[A-Za-z][^<>]*>')
CJK_RE = re.compile(r'[\u3400-\u9fff]')
# Chaves de 8 caracteres alfanuméricos aleatórios vêm de outros mods (ModExcel), não do jogo base.
MOD_KEY_RE = re.compile(r'(?=.*\d)(?=.*[A-Z])[A-Za-z0-9]{8}')


MIN_FILE = 100


def fine_category(key):
    if MOD_KEY_RE.fullmatch(key):
        return '_outros_mods'
    return re.sub(r'\d+', '#', re.sub(r'[\d_]+$', '', key))


def coarse_category(key):
    m = re.match(r'[A-Za-z]+', key)
    return m.group(0) if m else ''


def file_names(keys):
    """Uma categoria por arquivo; as pequenas caem para o prefixo de letras e depois para _diversos."""
    fine = {k: fine_category(k) for k in keys}
    nfine = collections.Counter(fine.values())
    mid = {k: (c if nfine[c] >= MIN_FILE else coarse_category(k)) for k, c in fine.items()}
    nmid = collections.Counter(mid.values())
    names = {}
    for k, c in mid.items():
        if nmid[c] < MIN_FILE or not c:
            c = '_diversos'
        names[k] = re.sub(r'[^A-Za-z0-9_#.-]', '_', c)
    return names


def unescape_dump(s):
    out, i = [], 0
    while i < len(s):
        if s[i] == '\\' and i + 1 < len(s):
            out.append({'n': '\n', 'r': '\r', 't': '\t', '\\': '\\'}.get(s[i + 1], s[i:i + 2]))
            i += 2
        else:
            out.append(s[i])
            i += 1
    return ''.join(out)


def unescape_kit(s):
    return s.replace('\\n', '\n').replace("\\'", "'").replace('\\"', '"')


def load_kit():
    rows = json.loads(KIT_JSON.read_text(encoding='utf-8-sig'))
    return {r['key']: {'ch': unescape_kit(r['ch']), 'en': unescape_kit(r['en'])} for r in rows}


def load_dump(path):
    dump = {}
    for line in path.read_text(encoding='utf-8').split('\n'):
        if line:
            key, en = line.split('\t', 1)
            dump[key] = unescape_dump(en)
    return dump


def load_work():
    work = {}
    if WORK_DIR.exists():
        for f in sorted(WORK_DIR.glob('*.json')):
            for key, entry in json.loads(f.read_text(encoding='utf-8')).items():
                work[key] = entry
    return work


def save_work(work):
    names = file_names(work)
    by_cat = collections.defaultdict(dict)
    for key in sorted(work):
        by_cat[names[key]][key] = work[key]
    WORK_DIR.mkdir(parents=True, exist_ok=True)
    for cat, entries in by_cat.items():
        (WORK_DIR / f'{cat}.json').write_text(
            json.dumps(entries, ensure_ascii=False, indent=1) + '\n', encoding='utf-8', newline='\n')
    for f in WORK_DIR.glob('*.json'):
        if f.stem not in by_cat:
            f.unlink()
    return by_cat


def cmd_extract(args):
    kit = load_kit()
    dump = load_dump(Path(args.dump))
    work = load_work()
    existing = json.loads(OUT_JSON.read_text(encoding='utf-8')) if OUT_JSON.exists() else {}
    added = updated = changed = 0
    # Chaves já traduzidas não aparecem no dump; vêm do LocalText.json atual com o en do kit.
    sources = dict(dump)
    for key in existing:
        if key not in sources and key in kit:
            sources[key] = kit[key]['en']
    for key, en in sources.items():
        ch = kit.get(key, {}).get('ch', '')
        entry = work.get(key)
        if entry is None:
            work[key] = {'ch': ch, 'en': en, 'pt': existing.get(key, '')}
            added += 1
            continue
        if entry['en'] != en:
            if entry.get('pt'):
                entry['revisar'] = True
                changed += 1
            entry['en'] = en
            updated += 1
        if ch and not entry.get('ch'):
            entry['ch'] = ch
    by_cat = save_work(work)
    print(f'dump: {len(dump)} chaves | novas: {added} | en atualizado: {updated} (traduzidas a revisar: {changed})')
    print(f'{len(work)} entradas em {len(by_cat)} arquivos em {WORK_DIR.relative_to(REPO)}')


def check(entry):
    """Devolve (erros, avisos) da tradução de uma entrada."""
    en, pt = entry['en'], entry.get('pt', '')
    errors, warnings = [], []
    if not pt:
        return errors, warnings
    src = en or entry.get('ch', '')
    for name, rx in (('token', TOKEN_RE), ('tag', TAG_RE)):
        a, b = collections.Counter(rx.findall(src)), collections.Counter(rx.findall(pt))
        if a != b:
            missing, extra = list((a - b).elements()), list((b - a).elements())
            errors.append(f'{name}: faltando {missing} sobrando {extra}')
    if src.count('\n') != pt.count('\n'):
        warnings.append(f'quebras de linha: en={src.count(chr(10))} pt={pt.count(chr(10))}')
    if CJK_RE.search(pt):
        warnings.append('pt contém caracteres chineses')
    if entry.get('revisar'):
        warnings.append('en mudou depois da tradução')
    return errors, warnings


def validate(work):
    report, n_err, n_warn = [], 0, 0
    for key in sorted(work):
        errors, warnings = check(work[key])
        n_err += bool(errors)
        n_warn += bool(warnings)
        for msg in errors:
            report.append(f'ERRO\t{key}\t{msg}')
        for msg in warnings:
            report.append(f'AVISO\t{key}\t{msg}')
    return report, n_err, n_warn


def cmd_validate(args):
    work = load_work()
    report, n_err, n_warn = validate(work)
    done = sum(1 for e in work.values() if e.get('pt'))
    out = Path(args.report)
    out.write_text('\n'.join(report) + '\n', encoding='utf-8')
    print(f'{done}/{len(work)} traduzidas | {n_err} com erro | {n_warn} com aviso | relatório: {out}')
    return 1 if n_err else 0


def cmd_build(args):
    work = load_work()
    result, skipped = {}, []
    for key in sorted(work):
        entry = work[key]
        if not entry.get('pt'):
            continue
        errors, _ = check(entry)
        if errors:
            skipped.append(key)
        else:
            result[key] = entry['pt']
    if not result:
        print(f'nenhuma tradução válida em {WORK_DIR.relative_to(REPO)}; {OUT_JSON.name} não foi alterado')
        return 1
    OUT_JSON.parent.mkdir(parents=True, exist_ok=True)
    OUT_JSON.write_text(json.dumps(result, ensure_ascii=False, indent=1) + '\n', encoding='utf-8', newline='\n')
    print(f'{len(result)} traduções em {OUT_JSON.relative_to(REPO)}')
    if skipped:
        print(f'{len(skipped)} fora por erro de validação (rode validate): {skipped[:10]}')
        return 1
    return 0


def main():
    sys.stdout.reconfigure(encoding='utf-8')
    ap = argparse.ArgumentParser(description=__doc__)
    sub = ap.add_subparsers(dest='cmd', required=True)
    p = sub.add_parser('extract', help='mescla o dump de runtime em traducao/LocalText/')
    p.add_argument('--dump', default=str(DUMP))
    p = sub.add_parser('validate', help='confere tokens, tags e quebras de linha das traduções')
    p.add_argument('--report', default=str(REPO / 'traducao/validacao.txt'))
    sub.add_parser('build', help='gera o LocalText.json do mod com as traduções válidas')
    args = ap.parse_args()
    return {'extract': cmd_extract, 'validate': cmd_validate, 'build': cmd_build}[args.cmd](args) or 0


if __name__ == '__main__':
    sys.exit(main())
