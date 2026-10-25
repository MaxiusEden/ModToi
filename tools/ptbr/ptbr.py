"""Pipeline da tradução PT-BR de LocalText: extract (dump de runtime -> traducao/), validate, build (-> ModAssets)."""
import argparse
import collections
import json
import os
import re
import sys
import time
import unicodedata
import urllib.request
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


def merge_outputs(work, data, stats):
    """Junta uma saída de tradutor (chave -> {pt, ok, model}) em `work`, sem sobrescrever pt existente."""
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


def cmd_merge(args):
    work = load_work()
    stats = collections.Counter()
    for path in args.files:
        merge_outputs(work, json.loads(Path(path).read_text(encoding='utf-8')), stats)
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


# Hy-MT2 no KoboldCpp: tradutor principal, uma linha por pedido (o modelo foi treinado sem system prompt).
LOCAL_URL = os.environ.get('HYMT2_URL', 'http://127.0.0.1:5001')
LOCAL_ID = 'koboldcpp/Hy-MT2-30B-A3B-Q4_K_M'
LOCAL_MODEL = 'local/hy-mt2'
LOCAL_START = r'C:\Users\filip\Downloads\bot\Start-HyMT2.bat'
LOCAL_TIMEOUT = 120
LOCAL_SAVE_EVERY = 20
LOCAL_MAX_ERRORS = 10
LOCAL_GLOSSARY_MAX = 15

# Copiado de ~/tools/omni-bridge/server.py (PLACEHOLDER, TYPO_MAP, normalize_punct, placeholders_ok):
# o local e a ponte precisam julgar a saída do mesmo jeito. Mude lá e aqui juntos.
PLACEHOLDER = re.compile(r"\{[^{}]*\}|&[^&\s]+&|\$[^$\s]+\$|</?[A-Za-z][^<>]*>|\\n|\n|%[sdf]")
TYPO_MAP = {"\u201c": '"', "\u201d": '"', "\u201e": '"', "\u2018": "'", "\u2019": "'",
            "\u2026": "...", "\u2013": "-", "\u2014": "-", "\u2011": "-", "\u00a0": " ",
            "\u00ba": "o", "\u00aa": "a"}


def normalize_punct(src: str, dst: str) -> str:
    """Troca pontuacao tipografica por ASCII quando o original nao a usa (fontes do jogo sem esses glifos)."""
    if not dst:
        return dst
    allowed = set(src or "")
    return "".join(c if c in allowed else TYPO_MAP.get(c, c) for c in dst)


def placeholders_ok(src: str, dst: str) -> bool:
    if collections.Counter(PLACEHOLDER.findall(src or "")) != collections.Counter(PLACEHOLDER.findall(dst or "")):
        return False
    allowed = set(src or "")
    return all(ord(ch) < 128 or "\u00c0" <= ch <= "\u00ff" or ch in allowed for ch in dst or "")


IGUAIS_FILE = GLOSS_DIR / 'iguais_ao_ingles.json'
_iguais = (None, frozenset())


def iguais_ao_ingles():
    """glossario/iguais_ao_ingles.json, relido quando muda (o `local --continuo` roda por dias)."""
    global _iguais
    try:
        mtime = IGUAIS_FILE.stat().st_mtime
    except OSError:
        return frozenset()
    if _iguais[0] != mtime:
        _iguais = (mtime, frozenset(json.loads(IGUAIS_FILE.read_text(encoding='utf-8'))))
    return _iguais[1]


def igual_permitido(src):
    """Texto que pode sair igual ao inglês: inteiro (sem marcadores) em iguais_ao_ingles.json, ou só marcadores.
    A ponte (~/tools/omni-bridge/server.py) tem a mesma função e lê o mesmo arquivo."""
    texto = PLACEHOLDER.sub('', src or '').strip()
    return not texto or texto in iguais_ao_ingles()


def local_judge(en, pt, finish=None):
    """ok da saída do Hy-MT2, com os critérios da ponte; a fila reaplica no merge, então mudar aqui vale para saídas antigas."""
    if finish == 'length' or not placeholders_ok(en, pt):
        return False
    identical = len(en.strip()) > 4 and any(c.islower() for c in en) and pt.strip() == en.strip()
    return not identical or igual_permitido(en)


def local_alive():
    """ID do modelo carregado no KoboldCpp, ou None se o servidor não responder."""
    try:
        with urllib.request.urlopen(f'{LOCAL_URL}/api/v1/model', timeout=5) as r:
            return json.loads(r.read().decode('utf-8')).get('result')
    except (OSError, ValueError):
        return None


def local_glossary(text, glossary):
    """Termos do glossário presentes no texto, o mais longo vencendo sobreposições; no máximo LOCAL_GLOSSARY_MAX.

    Sem diferenciar maiúsculas, como a ponte: na narrativa os termos aparecem em minúscula ("your breakthrough").
    Devolve pares (en, pt) para o prompt; termo em minúscula no texto vai em minúscula também no pt, senão o
    modelo copia a maiúscula do glossário para o meio da frase ("um profundo Revigoramento")."""
    found, taken = {}, []
    for term in sorted((t for t in glossary if not CJK_RE.search(t)), key=len, reverse=True):
        for m in re.finditer(r'(?<![A-Za-z])' + re.escape(term) + r'(?:s|es)?(?![A-Za-z])', text, re.IGNORECASE):
            if not any(m.start() < e and s < m.end() for s, e in taken):
                taken.append(m.span())
                if term not in found:
                    surface, pt = text[m.start():m.start() + len(term)], glossary[term]
                    if surface.islower() and not term.islower():
                        pt = ' '.join(w[:1].lower() + w[1:] for w in pt.split())
                    found[term] = (surface if surface.islower() else term, pt)
        if len(found) >= LOCAL_GLOSSARY_MAX:
            break
    return list(found.values())


def local_prompt(en, glossary):
    terms = local_glossary(en, glossary)
    head = ('Reference the following translations:\n'
            + '\n'.join(f'{t} translates to {pt}' for t, pt in terms) + '\n\n') if terms else ''
    instr = 'Translate the following text into Brazilian Portuguese.'
    if PLACEHOLDER.search(en):
        instr += (' You must retain the exact same number of delimiters in the translation, such as {0}, <color=...>,'
                  ' </color>, &x&, $x$ and \\n, without omitting, escaping or translating them.')
    instr += ' Output only the translation, without additional explanation.'
    return head + instr + '\n\n' + en


def local_chat(prompt):
    body = {'model': 'hy-mt2', 'messages': [{'role': 'user', 'content': prompt}],
            'max_tokens': 1024, 'temperature': 0.7, 'top_p': 1.0}
    req = urllib.request.Request(f'{LOCAL_URL}/v1/chat/completions', data=json.dumps(body).encode('utf-8'),
                                 headers={'Content-Type': 'application/json'})
    with urllib.request.urlopen(req, timeout=LOCAL_TIMEOUT) as r:
        data = json.loads(r.read().decode('utf-8'))
    choice = (data.get('choices') or [{}])[0]
    return (choice.get('message') or {}).get('content') or '', choice.get('finish_reason')


def pid_alive(pid):
    if os.name != 'nt':
        try:
            os.kill(pid, 0)
        except OSError:
            return False
        return True
    # No Windows os.kill(pid, 0) encerra o processo; consulta o código de saída.
    import ctypes
    k32 = ctypes.windll.kernel32
    handle = k32.OpenProcess(0x1000, False, pid)  # PROCESS_QUERY_LIMITED_INFORMATION
    if not handle:
        return False
    code = ctypes.c_ulong()
    k32.GetExitCodeProcess(handle, ctypes.byref(code))
    k32.CloseHandle(handle)
    return code.value == 259  # STILL_ACTIVE


def local_running(out_path):
    """True se já há um `local` gravando nesta saída (arquivo .pid ao lado dela)."""
    pid_file = Path(out_path).with_suffix('.pid')
    try:
        return pid_alive(int(pid_file.read_text()))
    except (OSError, ValueError):
        return False


def cmd_local(args):
    if args.log:
        # Iniciado pela fila sem console nem handles herdados: a saída vai direto para o log.
        sys.stdout = sys.stderr = open(args.log, 'a', encoding='utf-8', buffering=1)
    model = local_alive()
    if model != LOCAL_ID:
        print(f'Hy-MT2 não está no ar em {LOCAL_URL} (resposta: {model or "nenhuma"}). '
              f'Rode {LOCAL_START} e espere ~1 min para o modelo carregar.')
        return 2
    out_path = Path(args.out)
    if local_running(out_path):
        print(f'já há um processo gravando em {out_path}; não rodo dois ao mesmo tempo')
        return 2
    src = json.loads(Path(args.src).read_text(encoding='utf-8'))
    out = json.loads(out_path.read_text(encoding='utf-8')) if out_path.exists() else {}
    todo = [k for k, v in src.items()
            if not (v.get('pt') or '').strip() and (v.get('en') or '').strip() and k not in out]
    if args.limit:
        todo = todo[:args.limit]
    glossary = load_glossary('glossario.json')
    print(f'{len(todo)} linhas a traduzir ({len(out)} já na saída) com {LOCAL_MODEL}')

    def save():
        out_path.parent.mkdir(parents=True, exist_ok=True)
        out_path.write_text(json.dumps(out, ensure_ascii=False, indent=1), encoding='utf-8')

    pid_file = out_path.with_suffix('.pid')
    pid_file.parent.mkdir(parents=True, exist_ok=True)
    pid_file.write_text(str(os.getpid()))
    n_ok = n_bad = n_err = in_a_row = 0
    server_gone = False
    start = time.time()
    try:
        for i, key in enumerate(todo, 1):
            en = src[key]['en']
            t0 = time.time()
            try:
                pt, finish = local_chat(local_prompt(en, glossary))
                if not pt.strip():
                    raise ValueError('resposta vazia')
            except Exception as e:  # noqa: BLE001 - registra e segue; a linha fica para a próxima rodada
                if local_alive() != LOCAL_ID:
                    # KoboldCpp fechado (para liberar a placa): para sem contar erro; a linha não foi gravada.
                    server_gone = True
                    print(f'{i}/{len(todo)} Hy-MT2 fechado no meio; parando. Retome com o mesmo comando ou pela fila.',
                          flush=True)
                    break
                n_err += 1
                in_a_row += 1
                print(f'{i}/{len(todo)} ERRO {key}: {str(e)[:200]}', flush=True)
                if in_a_row >= LOCAL_MAX_ERRORS:
                    print(f'parando: {LOCAL_MAX_ERRORS} erros seguidos')
                    break
                continue
            in_a_row = 0
            # O modelo apara espaços e quebras das pontas; devolve as do en para não contar como marcador perdido.
            lead, trail = en[:len(en) - len(en.lstrip())], en[len(en.rstrip()):]
            pt = normalize_punct(en, lead + pt.strip() + trail)
            ok = local_judge(en, pt, finish)
            out[key] = {'pt': pt, 'ok': ok, 'model': LOCAL_MODEL}
            n_ok += ok
            n_bad += not ok
            print(f'{i}/{len(todo)} {"ok" if ok else "ok=false"} {time.time() - t0:.1f}s {key}', flush=True)
            if (n_ok + n_bad) % LOCAL_SAVE_EVERY == 0:
                save()
                rate = (n_ok + n_bad) / ((time.time() - start) / 60)
                print(f'-- {n_ok + n_bad}/{len(todo)} | {rate:.1f} linhas/min | ok {n_ok} | ok=false {n_bad} | erros {n_err}',
                      flush=True)
    finally:
        save()
        pid_file.unlink(missing_ok=True)
    minutes = (time.time() - start) / 60
    done = n_ok + n_bad
    print(f'resumo: {done} traduzidas em {minutes:.1f} min ({done / minutes if minutes else 0:.1f} linhas/min) | '
          f'ok {n_ok} | ok=false {n_bad} | erros {n_err} | faltam {len(todo) - done} | saída: {out_path}')
    return 3 if server_gone else 1 if in_a_row >= LOCAL_MAX_ERRORS else 0


def main():
    if sys.stdout:  # None quando a fila inicia o local sem handles (ver cmd_local --log)
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
    p = sub.add_parser('local', help='traduz com o Hy-MT2 local (KoboldCpp) um arquivo chave -> {ch, en, pt}; retomável')
    p.add_argument('--src', required=True)
    p.add_argument('--out', required=True)
    p.add_argument('--limit', type=int, default=0)
    p.add_argument('--log', help='grava a saída neste arquivo (usado quando a fila inicia o local em segundo plano)')
    args = ap.parse_args()
    cmds = {'extract': cmd_extract, 'validate': cmd_validate, 'build': cmd_build, 'glossario': cmd_glossario,
            'merge': cmd_merge, 'local': cmd_local}
    return cmds[args.cmd](args) or 0


if __name__ == '__main__':
    sys.exit(main())
