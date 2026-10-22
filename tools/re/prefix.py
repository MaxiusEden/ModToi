import json,re,collections
base="D:/Games/Tale of Immortal/Mod/modFQA/配置修改教程/配置（只读）Json格式"
rows=json.load(open(base+'/LocalText.json',encoding='utf-8-sig'))
cjk=re.compile(r'[\u4e00-\u9fff]')
c=collections.Counter(); w=collections.Counter(); bad=collections.Counter()
for r in rows:
    p=re.sub(r'[\d_]+$','',r['key']); p=re.sub(r'\d+','#',p)
    c[p]+=1; w[p]+=len(r['en'].split())
    if r['en'] in('','0') or cjk.search(r['en']): bad[p]+=1
lines=[f'{p:45s} n={c[p]:6d} enWords={w[p]:7d} semEN/CJK={bad[p]}' for p,_ in w.most_common(45)]
lines.append('--- prefixes with untranslated EN:')
lines+= [f'{p:45s} {n}' for p,n in bad.most_common(25)]
# RoleLogLocal
rl=json.load(open(base+'/RoleLogLocal.json',encoding='utf-8-sig'))
c2=collections.Counter(re.sub(r'\d+$','',r['keyID']) for r in rl)
lines.append('--- RoleLogLocal keyID prefixes'); lines+=[f'{p:45s} {n}' for p,n in c2.most_common(15)]
# repetitiveness sample: npc chat lines
open('prefix.txt','w',encoding='utf-8').write('\n'.join(lines))
