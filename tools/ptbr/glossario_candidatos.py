"""Gera glossario/candidatos.tsv: termos recorrentes do en (n-gramas com maiúscula) e nomes das tabelas do kit."""
import collections
import json
import re
import sys

import ptbr

OUT = ptbr.REPO / 'glossario/candidatos.tsv'
KIT_DIR = ptbr.KIT_JSON.parent
MIN_FREQ = 5
MAX_NGRAM = 5

# Tabela do kit -> (tipo, campos que apontam para chaves de LocalText com nomes).
TABLES = {
    'RoleGrade': ('reino', ['gradeName', 'phaseName']),
    'DLCUnitGrade': ('reino', ['gradeName', 'fullname']),
    'SchoolName': ('seita', ['name1', 'name2']),
    'ItemType': ('tipo de item', ['name', 'uiLabel']),
    'DLCItemType': ('tipo de item', ['name', 'uiLabel']),
    'BattleEffect': ('efeito', ['name']),
    'SkillEffectDesc': ('efeito', ['key']),
    'ModRoleEffectType1': ('atributo', ['attrText']),
    'DLCRoleStandardAttr': ('atributo', ['name']),
    'AppellationTitle': ('título', ['name']),
    'RoleCreateFeature': ('destino/traço', ['name']),
    'DLCUnitFateFeature': ('destino/traço', ['name']),
    'BattleSkillAttack': ('técnica', ['name']),
    'DLCBattleSkillAttack': ('técnica', ['name']),
    'PassiveMethod': ('técnica', ['name']),
    'GodEyeSkills': ('técnica', ['name']),
    'TownName': ('lugar', ['name']),
}
# Palavras que só começam frase ou são pronomes; sozinhas não são termo.
STOP = set('''I You He She It We They The A An This That These Those My Your His Her Its Our Their
Me Him Us Them What Who Why How When Where Which If But And Or So Then Now Yes No Not Do Does Did
Is Are Was Were Be Been Am Will Would Can Could Should Shall May Might Must Let Please Oh Ah Hmm Well
There Here All Some Any Each Every One Two Three After Before While Since As At In On Of To For From
With By Just Even Also Only Still Very Too Okay OK Hey Hello Thank Thanks Sorry Haha Huh Eh Alright
Lv Lvl Max Min HP MP SP'''.split())
CONNECT = {'of', 'the', 'and'}
# Tratamentos costumam vir em minúscula no en, então os n-gramas capitalizados não os pegam.
TRATAMENTO = '''fellow daoist|senior brother|senior sister|junior brother|junior sister|senior|junior|
master|disciple|elder|sect leader|young master|young lady|my lord|lord|venerable|patriarch|ancestor|
grandmaster|little friend|benefactor|this old man|this lady|daoist|immortal|fairy|sister|brother|
husband|wife|dao companion|dao partner|teacher|apprentice|sect master|grand elder|young friend|
fellow cultivator|my friend|friend|kid|brat|young man|girl|old man|grandpa|madam|miss|leader'''
CULTIVO = '''cultivator|cultivation|mortal|demon|demonic|righteous|evil|qi|spirit stones|spirit stone|dao|
breakthrough|tribulation|meridians|dantian|spiritual root|lifespan'''
SEEDS = {t.strip(): 'tratamento' for t in TRATAMENTO.replace('\n', '').split('|')}
SEEDS.update({t.strip(): 'cultivo' for t in CULTIVO.replace('\n', '').split('|')})
WORD_RE = re.compile(r"[A-Za-z][A-Za-z'\-]*")
CLEAN_RE = re.compile(r'<[^<>]*>|\{[^{}]*\}|&[^&\s]+&|\$[^$\s]+\$')


def words(text):
    return WORD_RE.findall(CLEAN_RE.sub(' ', text))


def cap_ngrams(text):
    """N-gramas de palavras capitalizadas (permite 'of/the/and' no meio), sem a 1ª palavra de cada frase."""
    found = set()
    for sentence in re.split(r'[.!?\n:;"“”()\[\]]+', CLEAN_RE.sub(' ', text)):
        ws = WORD_RE.findall(sentence)
        for i in range(len(ws)):
            if not ws[i][0].isupper():
                continue
            for n in range(1, MAX_NGRAM + 1):
                seq = ws[i:i + n]
                if len(seq) < n or not seq[-1][0].isupper():
                    if len(seq) < n:
                        break
                    if seq[-1].lower() in CONNECT:
                        continue
                    break
                if any(not w[0].isupper() and w.lower() not in CONNECT for w in seq):
                    break
                if n == 1 and (i == 0 or seq[0] in STOP):
                    continue
                if seq[0] in STOP and n <= 2:
                    continue
                found.add(' '.join(seq))
    return found


def one_line(s, limit=110):
    s = s.replace('\t', ' ').replace('\n', ' / ')
    return s if len(s) <= limit else s[:limit - 1] + '…'


def main():
    sys.stdout.reconfigure(encoding='utf-8')
    kit = ptbr.load_kit()
    work = ptbr.load_work()
    corpus = {k: e['en'] for k, e in work.items() if e['en']}

    # Nomes das tabelas: en exato -> (tipo, ch, tabelas).
    named = {}
    for table, (kind, fields) in TABLES.items():
        rows = json.loads((KIT_DIR / f'{table}.json').read_text(encoding='utf-8-sig'))
        for r in rows:
            for f in fields:
                key = r.get(f)
                if key in kit and 0 < len(kit[key]['ch']) <= 12:
                    en = kit[key]['en'].strip().rstrip(':：').strip()
                    if not en or ptbr.CJK_RE.search(en):
                        continue
                    item = named.setdefault(en, {'tipo': kind, 'ch': collections.Counter(), 'fontes': set()})
                    item['ch'][kit[key]['ch']] += 1
                    item['fontes'].add(table)

    # en exato de entradas curtas -> ch mais comum (para dar ch aos n-gramas do corpus).
    exact_ch = collections.defaultdict(collections.Counter)
    exact_cat = collections.defaultdict(collections.Counter)
    for k, en in corpus.items():
        if len(en.split()) <= MAX_NGRAM and kit.get(k, {}).get('ch'):
            exact_ch[en.strip()][kit[k]['ch']] += 1
            exact_cat[en.strip()][ptbr.fine_category(k)] += 1

    # Frequência = nº de entradas cujo en contém o termo. Nomes de uma palavra só contam pelos
    # n-gramas capitalizados (senão sílabas de seita como "You"/"An" contam toda frase).
    doc_freq = collections.Counter()
    term_docs = collections.defaultdict(list)
    named_multi = {tuple(words(t)): t for t in named if len(words(t)) > 1}
    for k, en in corpus.items():
        terms = cap_ngrams(en)
        ws = words(en)
        seqs = {tuple(ws[i:i + n]) for n in range(2, 9) for i in range(len(ws) - n + 1)}
        terms |= {named_multi[s] for s in seqs if s in named_multi}
        for t in terms:
            doc_freq[t] += 1
            if len(term_docs[t]) < 40:
                term_docs[t].append(k)

    for seed, kind in SEEDS.items():
        rx = re.compile(r'\b' + re.escape(seed) + r'\b', re.I)
        term = seed.title()
        docs = [k for k, en in corpus.items() if rx.search(en)]
        if not docs:
            continue
        named.setdefault(term, {'tipo': kind, 'ch': collections.Counter(), 'fontes': {'semente'}})
        named[term]['tipo'] = kind
        doc_freq[term] = len(docs)
        term_docs[term] = docs[:40]

    rows = []
    for term, freq in doc_freq.items():
        info = named.get(term)
        if freq < MIN_FREQ and not info:
            continue
        chs = collections.Counter(exact_ch.get(term, {}))
        if info:
            chs.update(info['ch'])
            kind, src = info['tipo'], ','.join(sorted(info['fontes']))
        else:
            kind, src = '', 'corpus'
        ch = ' / '.join(one_line(c) for c, _ in chs.most_common(3))
        cats = collections.Counter(ptbr.fine_category(k) for k in term_docs[term])
        if exact_cat.get(term):
            cats = exact_cat[term]
        docs = sorted(term_docs[term], key=lambda k: (len(corpus[k]) < 25, len(corpus[k])))
        examples = [f'{k}: {one_line(corpus[k])}' for k in docs[:2]]
        examples += [''] * (2 - len(examples))
        rows.append((term, freq, ch, kind, cats.most_common(1)[0][0], src, *examples))

    rows.sort(key=lambda r: (-r[1], r[0]))
    OUT.parent.mkdir(parents=True, exist_ok=True)
    header = ('termo', 'frequencia', 'ch', 'tipo', 'categoria', 'fonte', 'exemplo1', 'exemplo2')
    with OUT.open('w', encoding='utf-8', newline='\n') as f:
        f.write('\t'.join(header) + '\n')
        for r in rows:
            f.write('\t'.join(str(x) for x in r) + '\n')
    by_kind = collections.Counter(r[3] or '(corpus)' for r in rows)
    print(f'{len(rows)} candidatos em {OUT.relative_to(ptbr.REPO)} | com ch: {sum(1 for r in rows if r[2])}')
    print('por tipo:', dict(by_kind.most_common()))


if __name__ == '__main__':
    main()
