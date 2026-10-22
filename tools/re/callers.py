"""Find direct E8/E9 call sites to given RVAs in GameAssembly .text; report containing method."""
import sys, json, glob, struct, bisect
import pefile
pe = pefile.PE(r"D:/Games/Tale of Immortal/GameAssembly.dll", fast_load=True)
data = bytes(pe.__data__)
names = {}
for f in glob.glob('rva_*.json'):
    for k, v in json.load(open(f)).items():
        for pair in v:
            if pair:
                names[int(pair[0], 16)] = k
starts = sorted(names)
targets = {int(a, 16) for a in sys.argv[1:]}
text = next(s for s in pe.sections if s.Name.startswith(b'il2cpp'))
t0 = text.PointerToRawData; tv = text.VirtualAddress; n = text.SizeOfRawData
buf = data[t0:t0 + n]
import re
for m in re.finditer(rb'[\xe8\xe9]', buf):
    i = m.start()
    if i + 5 > n:
        break
    rel = struct.unpack_from('<i', buf, i + 1)[0]
    dst = tv + i + 5 + rel
    if dst in targets:
        src = tv + i
        k = bisect.bisect_right(starts, src) - 1
        print(f'{src:#x} -> {dst:#x} ({names.get(dst)})  in {names[starts[k]]} @ {starts[k]:#x}')
