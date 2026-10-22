import json,re,collections,sys
lits=json.load(open('literals.json',encoding='utf-8'))
L=set(lits)
base=sys.argv[1]
cjk=re.compile(r'[\u4e00-\u9fff]')
res={}
for t in ['LocalText','RoleLogLocal']:
    rows=json.load(open(f'{base}/{t}.json',encoding='utf-8-sig'))
    for col in ['key','keyID','ch','en']:
        vals=[r[col] for r in rows if col in r and r[col] not in ('','0')]
        if not vals: continue
        hit=sum(1 for v in vals if v in L)
        print(t,col,len(vals),'in-binary',hit,f'{100*hit/len(vals):.1f}%')
# literals with CJK not in any json ch
allch=set()
for t in ['LocalText','RoleLogLocal']:
    for r in json.load(open(f'{base}/{t}.json',encoding='utf-8-sig')):
        allch.add(r.get('ch','')); allch.add(r.get('tc',''))
cjklits=[s for s in L if cjk.search(s)]
missing=[s for s in cjklits if s not in allch]
print('CJK literals total',len(cjklits),'not in LocalText/RoleLogLocal ch/tc',len(missing))
json.dump(sorted(missing),open('cjk_not_in_json.json','w',encoding='utf-8'),ensure_ascii=False,indent=0)
