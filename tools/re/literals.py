import struct,sys,json,re
b=open(sys.argv[1],'rb').read()
h=struct.unpack_from('<8I',b,0)
sanity,ver,slo,sls,sldo,slds,so,ss=h
lits=[]
for i in range(sls//8):
    ln,idx=struct.unpack_from('<II',b,slo+i*8)
    lits.append(b[sldo+idx:sldo+idx+ln].decode('utf-8','replace'))
json.dump(lits,open(sys.argv[2],'w',encoding='utf-8'),ensure_ascii=False)
print('version',ver,'literals',len(lits))
