import json, os, re, sys, collections
d = sys.argv[1]
cjk = re.compile(r'[\u4e00-\u9fff]')
lang_tables = []
cjk_cols = collections.Counter()
total = {'rows':0,'ch_chars':0,'en_words':0,'en_missing':0,'en_same_as_ch':0}
keyrefs = 0
for f in sorted(os.listdir(d)):
    if not f.endswith('.json'): continue
    try:
        data = json.load(open(os.path.join(d,f),encoding='utf-8-sig'))
    except Exception as e:
        print('ERR',f,e); continue
    if not isinstance(data,list) or not data: continue
    cols = list(data[0].keys())
    if 'ch' in cols and 'en' in cols:
        n=len(data); chc=sum(len(r.get('ch','')) for r in data)
        enw=sum(len(r.get('en','').split()) for r in data)
        miss=sum(1 for r in data if not r.get('en') or r.get('en') in ('0',''))
        same=sum(1 for r in data if r.get('en') and cjk.search(r.get('en','')))
        lang_tables.append((f,n,chc,enw,miss,same,cols))
        total['rows']+=n; total['ch_chars']+=chc; total['en_words']+=enw; total['en_missing']+=miss; total['en_same_as_ch']+=same
    else:
        for c in cols:
            s=sum(1 for r in data if isinstance(r.get(c),str) and cjk.search(r[c]))
            if s: cjk_cols[(f,c)]+=s
for t in sorted(lang_tables,key=lambda x:-x[1]):
    print(f'{t[0]:40s} rows={t[1]:7d} chChars={t[2]:9d} enWords={t[3]:8d} enMissing={t[4]:6d} enHasCJK={t[5]:5d} cols={[c for c in t[6] if c not in ("ch","tc","en","kr")]}')
print(total)
print('--- non-language tables with CJK in columns:')
for (f,c),n in cjk_cols.most_common(60): print(f,c,n)
