import zipfile,re,sys
z=zipfile.ZipFile(sys.argv[1]); x=z.read('word/document.xml').decode('utf-8')
x=re.sub(r'</w:p>','\n',x); x=re.sub(r'<[^>]+>','',x)
open(sys.argv[2],"w",encoding="utf-8").write(x)
