"""Recover additive-cipher key for EncryptMult files via printable-text scoring."""
import os, sys
d = sys.argv[1]
HL = 21
blobs = []
for f in os.listdir(d):
    b = open(os.path.join(d, f), 'rb').read()
    if b.startswith(b'1b8bg-/%ePe]P[-/921CG'):
        blobs.append(b[HL:])
def score(byte):
    if byte in (9, 10, 13) or 32 <= byte < 127:
        return 1.0
    return 0.0
best = []
for L in range(1, 65):
    key = []; tot = 0; cnt = 0
    for j in range(L):
        col = [c for blob in blobs for c in blob[j::L]]
        bk, bs = 0, -1
        for k in range(256):
            s = sum(score((c - k) & 0xff) for c in col)
            if s > bs:
                bk, bs = k, s
        key.append(bk); tot += bs; cnt += len(col)
    best.append((tot / cnt, L, bytes(key)))
best.sort(reverse=True)
for r in best[:6]:
    print(round(r[0], 4), r[1], r[2])
s, L, key = best[0]
p = bytes((c - key[i % L]) & 0xff for i, c in enumerate(blobs[0][:300]))
print(p)
