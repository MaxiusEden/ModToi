import json, sys, re
types = json.load(open('meta_types.json', encoding='utf-8'))
pat = re.compile(sys.argv[1])
out = []
for t in types:
    if pat.search(t['name']):
        out.append(f"== {t['ns']}.{t['name']}  fields={len(t['fields'])} methods={len(t['methods'])}")
        lim = int(sys.argv[2]) if len(sys.argv) > 2 else 80
        out.append('  F: ' + ', '.join(f"{n}={v}" if v is not None else n for n, v in t['fields'][:lim]))
        out.append('  M: ' + ', '.join(f"{n}({p})" for n, p in t['methods'][:lim]))
open('showtypes.txt', 'w', encoding='utf-8').write('\n'.join(out))
