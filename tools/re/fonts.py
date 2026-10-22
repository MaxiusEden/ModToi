import UnityPy, sys, io
from fontTools.ttLib import TTFont
env = UnityPy.load(sys.argv[1])
need="áàâãéêíóôõúüçÁÀÂÃÉÊÍÓÔÕÚÜÇ"
for obj in env.objects:
    if obj.type.name!='Font': continue
    d=obj.read(); data=bytes(d.m_FontData)
    n=d.m_Name
    if not data: print(n,'no data'); continue
    open(f'fonts/{n}.ttf','wb').write(data)
    try:
        f=TTFont(io.BytesIO(data),fontNumber=0); cmap=f.getBestCmap()
        miss=[c for c in need if ord(c) not in cmap]
        print(n,len(data),'glyphs',len(cmap),'missing PT chars:',''.join(miss) or 'none')
    except Exception as e: print(n,'ERR',e)
