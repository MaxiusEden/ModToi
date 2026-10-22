import os, sys, collections, math, base64
d = sys.argv[1]
files = sorted(os.listdir(d))
data = {f: open(os.path.join(d, f), 'rb').read() for f in files}
for f, b in data.items():
    ent = 0
    if b:
        c = collections.Counter(b)
        ent = -sum(v / len(b) * math.log2(v / len(b)) for v in c.values())
    print(f"{f:45s} len={len(b):7d} mod8={len(b)%8} entropy={ent:.2f} head={b[:40]!r}")
# common prefix across all
bs = [b for b in data.values() if len(b) > 40]
p = 0
while all(b[p] == bs[0][p] for b in bs):
    p += 1
print('common prefix len', p, bs[0][:p])
# filename decoding attempt (custom base64: @ for /)
for f in files:
    n = f.replace('.cache', '').replace('@', '/')
    try:
        raw = base64.b64decode(n + '=' * (-len(n) % 4))
        print(f, len(raw), raw.hex())
    except Exception as e:
        print(f, 'b64 err')
