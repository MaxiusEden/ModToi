import re,sys,collections
b=open(sys.argv[1],'rb').read()
strs=set(m.group().decode('utf-8','replace') for m in re.finditer(rb'[\x20-\x7e]{4,200}',b))
pat=re.compile(sys.argv[2])
hits=sorted(s for s in strs if pat.search(s))
print(len(strs),len(hits))
for h in hits[:int(sys.argv[3]) if len(sys.argv)>3 else 300]: print(h)
