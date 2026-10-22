import sys,re,html,glob,os
os.chdir(sys.argv[1]); pat=sys.argv[2]
out=[]
for f in glob.glob('*.htm'):
    t=open(f,encoding='utf-8',errors='replace').read()
    m=re.search(r'<title>([^<]*)</title>',t)
    if m and re.search(pat,m.group(1)):
        body=re.sub(r'<script.*?</script>','',t,flags=re.S)
        body=re.sub(r'<[^>]+>',' ',body); body=html.unescape(re.sub(r'\s+',' ',body))
        out.append('### '+m.group(1)+'\n'+body[:int(sys.argv[3])])
open(sys.argv[4],'w',encoding='utf-8').write('\n\n'.join(sorted(out)))
