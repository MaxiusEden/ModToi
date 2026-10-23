"""Pipeline da tradução PT-BR de LocalText: extract (dump de runtime -> traducao/), validate, build (-> ModAssets)."""
import argparse
import collections
import json
import os
import re
import sys
import unicodedata
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
GAME_DIR = Path(os.environ.get('GAME_DIR', r'C:\Games\Tale of Immortal'))
MOD_ID = '6b2SYs'
KIT_JSON = GAME_DIR / 'Mod/modFQA/配置修改教程/配置（只读）Json格式/LocalText.json'
DUMP = GAME_DIR / f'ModExportData/Mod_{MOD_ID}/ptbr_missing_LocalText.txt'
WORK_DIR = REPO / 'traducao/LocalText'
OUT_JSON = REPO / f'ModProject_{MOD_ID}/ModProject/ModAssets/ptbr/LocalText.json'
GLOSS_DIR = REPO / 'glossario'

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
        # O en do kit é a fonte padrão; o do dump traz o texto de outros mods ativos e só vale
        # para chaves fora do kit ou com en vazio no kit.
        if kit.get(key, {}).get('en'):
            en = kit[key]['en']
        ch = kit.get(key, {}).get('ch', '')
        entry = work.get(key)
        if entry is None:
            work[key] = {'ch': ch, 'en': en, 'pt': existing.get(key, '')}
            added += 1
            continue
        if entry['en'] != en:
            if entry.get('pt'):
                add_reason(entry, 'en mudou depois da tradução')
                changed += 1
            entry['en'] = en
            updated += 1
        if ch and not entry.get('ch'):
            entry['ch'] = ch
    by_cat = save_work(work)
    print(f'dump: {len(dump)} chaves | novas: {added} | en atualizado: {updated} (traduzidas a revisar: {changed})')
    print(f'{len(work)} entradas em {len(by_cat)} arquivos em {WORK_DIR.relative_to(REPO)}')


GLOSS_REASON = 'glossário: '


def add_reason(entry, reason):
    """`revisar` é a lista de motivos pendentes; quem revisa apaga o campo."""
    reasons = entry.get('revisar') or []
    if reason not in reasons:
        entry['revisar'] = reasons + [reason]


def fold(s):
    s = unicodedata.normalize('NFKD', s.lower())
    return ''.join(c for c in s if not unicodedata.combining(c))


_glossary = None
CONNECTORS = {'de', 'do', 'da', 'dos', 'das', 'e', 'em', 'no', 'na', 'o', 'a', 'os', 'as', 'sem'}
_PLURAL = [('ão', 'ões'), ('al', 'ais'), ('el', 'eis'), ('ol', 'ois'), ('ul', 'uis'), ('m', 'ns'),
           ('r', 'res'), ('z', 'zes')]


_abbrev = None


def abbreviate(folded):
    """Aplica glossario/abreviacoes.json (ex.: habilidade -> habil.) para aceitar a forma curta em UI apertada."""
    global _abbrev
    if _abbrev is None:
        _abbrev = {fold(k): fold(v) for k, v in load_glossary('abreviacoes.json').items()}
    return ' '.join(_abbrev.get(w, w) for w in folded.split())


def number_variant(text, plural):
    """Põe no plural (ou singular) as palavras antes do primeiro conectivo: 'Pedra Espiritual' <-> 'Pedras Espirituais'."""
    out, done = [], False
    for w in text.split():
        if done or w.lower() in CONNECTORS:
            done = done or w.lower() in CONNECTORS
            out.append(w)
            continue
        for sing, plur in _PLURAL:
            a, b = (sing, plur) if plural else (plur, sing)
            if w.lower().endswith(a):
                w = w[:len(w) - len(a)] + b
                break
        else:
            if plural and w[-1:].lower() in 'aeiouáéíóúâêôãõ':
                w += 's'
            elif not plural and w[-1:].lower() == 's':
                w = w[:-1]
        out.append(w)
    return ' '.join(out)


def obligatory_terms():
    """[(termo, tradução, regex na fonte, é_ch)], mais longos primeiro; inclui sugestões só para cobertura."""
    global _glossary
    if _glossary is None:
        obrig, sug = load_glossary('obrigatorios.json'), load_glossary('sugestoes.json')
        terms = []
        for term, pt in {**sug, **obrig}.items():
            is_ch = bool(CJK_RE.search(term))
            rx = re.compile(re.escape(term) if is_ch else
                            r'(?<![A-Za-z])' + re.escape(term) + r'(?:s|es)?(?![A-Za-z])')
            forms = {fold(f) for f in (pt, number_variant(pt, plural=True), number_variant(pt, plural=False))}
            forms |= {abbreviate(f) for f in forms}
            pt_rx = [re.compile(r'(?<![a-z0-9])' + r'\w*\s+'.join(map(re.escape, f.split()))) for f in forms]
            terms.append((term, pt_rx, rx, is_ch, term in obrig, pt))
        terms.sort(key=lambda t: -len(t[0]))
        _glossary = terms
    return _glossary


_exceptions = None


def exceptions():
    """glossario/excecoes.json: nomes próprios cujo interior não é conferido e termos ignorados por chave."""
    global _exceptions
    if _exceptions is None:
        data = load_glossary('excecoes.json') or {}
        names = [re.compile(re.escape(n) if CJK_RE.search(n) else r'(?<![A-Za-z])' + re.escape(n) + r'(?![A-Za-z])')
                 for n in data.get('nomes', [])]
        _exceptions = (names, {k: set(v) for k, v in data.get('por_chave', {}).items()})
    return _exceptions


def glossary_misses(entry, key=None):
    """Termos obrigatórios presentes na fonte cuja tradução aprovada falta no pt.

    Usa a correspondência mais longa: em "Taoist Mind" vale "Coração do Dao", não "Daoista".
    No en aceita plural (-s/-es). No pt ignora acento e maiúscula e aceita a tradução no singular
    ou no plural, exigindo só o início de cada palavra."""
    pt = fold(entry.get('pt', ''))
    names, by_key = exceptions()
    ignored = by_key.get(key, set())
    misses = []
    for source in ('en', 'ch'):
        text = entry.get(source, '')
        covered = [m.span() for rx in names for m in rx.finditer(text)]
        for term, pt_rx, rx, is_ch, required, tr in obligatory_terms():
            if is_ch != (source == 'ch'):
                continue
            for m in rx.finditer(text):
                if any(m.start() < e and s < m.end() for s, e in covered):
                    continue
                covered.append((m.start(), m.end()))
                # Termo excetuado na chave continua cobrindo o trecho (os termos menores dentro dele não são cobrados).
                if required and term not in ignored and not any(r.search(pt) for r in pt_rx):
                    misses.append(f'{term} → {tr}')
    return sorted(set(misses))


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
    warnings += [f'revisar: {r}' for r in entry.get('revisar') or []]
    return errors, warnings


WIDTH_REASON = 'largura: '
SHORT_UI = 20
_limits = None


def width_limit(key, en):
    """Limite de caracteres do pt: traducao/limites.json para a chave, senão regra geral para textos curtos de UI."""
    global _limits
    if _limits is None:
        path = REPO / 'traducao/limites.json'
        _limits = json.loads(path.read_text(encoding='utf-8')) if path.exists() else {}
    if key in _limits:
        return _limits[key]
    for pattern, limit in _limits.items():
        if pattern.endswith('*') and key and key.startswith(pattern[:-1]):
            return limit
    en = TAG_RE.sub('', en).strip()
    if not en or '\n' in en or len(en) > SHORT_UI or CJK_RE.search(en):
        return None
    return max(len(en) + 3, int(len(en) * 1.3 + 0.999))


def width_problem(entry, key):
    limit = width_limit(key, entry['en'])
    size = len(TAG_RE.sub('', entry.get('pt', '')).strip())
    return f'{size} > {limit} caracteres' if limit is not None and size > limit else None


def refresh_glossary_reasons(entry, key=None):
    """Recalcula os motivos automáticos de `revisar` ('glossário:' e 'largura:'); devolve True se o campo mudou."""
    before = entry.get('revisar') or []
    keep = [r for r in before if not r.startswith((GLOSS_REASON, WIDTH_REASON))]
    after = keep
    if entry.get('pt'):
        after = keep + [GLOSS_REASON + m for m in glossary_misses(entry, key)]
        width = width_problem(entry, key)
        if width:
            after.append(WIDTH_REASON + width)
    if after == before:
        return False
    if after:
        entry['revisar'] = after
    else:
        entry.pop('revisar', None)
    return True


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
    changed = sum(refresh_glossary_reasons(e, k) for k, e in work.items())
    if changed:
        save_work(work)
    report, n_err, n_warn = validate(work)
    done = sum(1 for e in work.values() if e.get('pt'))
    pending = sum(1 for e in work.values() if e.get('revisar'))
    count = lambda p: sum(1 for e in work.values() if any(r.startswith(p) for r in e.get('revisar') or []))
    out = Path(args.report)
    out.write_text('\n'.join(report) + '\n', encoding='utf-8')
    print(f'{done}/{len(work)} traduzidas | {n_err} com erro | {n_warn} com aviso | revisar: {pending} '
          f'(glossário: {count(GLOSS_REASON)}, largura: {count(WIDTH_REASON)}; {changed} atualizadas agora) | relatório: {out}')
    return 1 if n_err else 0


OMNI_BAD = 'omni ok=false'


def cmd_merge(args):
    work = load_work()
    stats = collections.Counter()
    for path in args.files:
        data = json.loads(Path(path).read_text(encoding='utf-8'))
        for key, out in data.items():
            entry = work.get(key)
            if entry is None:
                stats['chave desconhecida'] += 1
                continue
            if entry.get('pt'):
                stats['já tinha pt (mantido)'] += 1
                continue
            entry['pt'] = out.get('pt', '')
            entry['auto'] = out.get('model', '?')
            if out.get('ok'):
                stats['ok=true'] += 1
            else:
                add_reason(entry, OMNI_BAD)
                stats['ok=false (revisar)'] += 1
            refresh_glossary_reasons(entry, key)
    save_work(work)
    gloss = sum(1 for e in work.values() if e.get('auto') and any(r.startswith(GLOSS_REASON) for r in e.get('revisar') or []))
    print(', '.join(f'{k}: {v}' for k, v in stats.items()) or 'nada a juntar')
    print(f'traduções automáticas com motivo de glossário: {gloss}')
    return 0


def cmd_build(args):
    work = load_work()
    result, skipped, unreviewed = {}, [], 0
    for key in sorted(work):
        entry = work[key]
        if not entry.get('pt'):
            continue
        # Saída quebrada do modelo não vai para o jogo sem revisão humana.
        if OMNI_BAD in (entry.get('revisar') or []):
            unreviewed += 1
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
    if unreviewed:
        print(f'{unreviewed} fora por "{OMNI_BAD}" ainda não revisado')
    if skipped:
        print(f'{len(skipped)} fora por erro de validação (rode validate): {skipped[:10]}')
        return 1
    return 0


def load_glossary(name):
    path = GLOSS_DIR / name
    return json.loads(path.read_text(encoding='utf-8')) if path.exists() else {}


def cmd_glossario(args):
    obrig, sug = load_glossary('obrigatorios.json'), load_glossary('sugestoes.json')
    both = sorted(set(obrig) & set(sug))
    if both:
        print(f'termos nos dois níveis: {both}')
        return 1
    merged = {**obrig, **sug}
    (GLOSS_DIR / 'glossario.json').write_text(
        json.dumps(merged, ensure_ascii=False, indent=1) + '\n', encoding='utf-8', newline='\n')
    print(f'glossario.json: {len(merged)} termos ({len(obrig)} obrigatórios + {len(sug)} sugestões)')
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
    sub.add_parser('glossario', help='junta obrigatorios.json e sugestoes.json em glossario.json (entrada do omni_translate)')
    p = sub.add_parser('merge', help='traz saídas do omni_translate (chave -> {pt, ok, model}) para traducao/')
    p.add_argument('files', nargs='+')
    args = ap.parse_args()
    cmds = {'extract': cmd_extract, 'validate': cmd_validate, 'build': cmd_build, 'glossario': cmd_glossario,
            'merge': cmd_merge}
    return cmds[args.cmd](args) or 0


if __name__ == '__main__':
    sys.exit(main())
