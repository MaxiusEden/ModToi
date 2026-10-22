import UnityPy, sys, collections, json
env = UnityPy.load(sys.argv[1])
cnt=collections.Counter(); ta=[]; fonts=[]
for obj in env.objects:
    t=obj.type.name; cnt[t]+=1
    if t in ('TextAsset','Font','MonoBehaviour') and t!='MonoBehaviour':
        try:
            d=obj.read(); n=getattr(d,'m_Name','')
            if t=='TextAsset':
                sc=d.m_Script; b=sc.encode('utf-8','surrogateescape') if isinstance(sc,str) else bytes(sc)
                ta.append((n,len(b),b[:24].hex()))
            else: fonts.append(n)
        except Exception as e: ta.append(('ERR',str(e)[:80],''))
print(cnt.most_common(30))
print('fonts',fonts)
ta.sort(key=lambda x:-x[1] if isinstance(x[1],int) else 0)
print(len(ta))
for x in ta[:80]: print(x)
