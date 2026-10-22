import json, re, os, random
base = "D:/Games/Tale of Immortal/Mod/modFQA/配置修改教程/配置（只读）Json格式"
BS = chr(92)

def un(s):
    return (s.replace(BS + 'n', '\n').replace(BS + "'", "'").replace(BS + '"', '"')
             .replace(BS + 't', '\t').replace(BS + BS, BS))

known = set()
for f in os.listdir(base):
    if not f.endswith('.json'):
        continue
    try:
        rows = json.load(open(os.path.join(base, f), encoding='utf-8-sig'))
    except Exception:
        continue
    if isinstance(rows, list):
        for r in rows:
            if isinstance(r, dict):
                for v in r.values():
                    if isinstance(v, str):
                        known.add(v); known.add(un(v))
lits = json.load(open('literals.json', encoding='utf-8'))
L = set(lits)
rt = json.load(open(base + '/LocalText.json', encoding='utf-8-sig'))
vals = [un(r['en']) for r in rt if r['en'] not in ('', '0')]
print('LocalText en match after unescape', round(sum(1 for v in vals if v in L) / len(vals), 4))
sent = re.compile(r"^[A-Z][a-z']+( [A-Za-z',.!?]+){2,}")
eng = [s for s in L if sent.match(s) and s not in known and not re.search(r'[\u4e00-\u9fff]', s)]
print('english sentence-like literals outside tables', len(eng))
random.seed(3); random.shuffle(eng)
open('eng_extra.txt', 'w', encoding='utf-8').write('\n'.join(x.replace('\n', ' / ')[:160] for x in eng))
