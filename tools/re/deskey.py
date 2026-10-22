import json, re
from Crypto.Cipher import DES
lits = json.load(open('literals.json', encoding='utf-8'))
cands = set()
for s in lits:
    bs = s.encode('utf-8', 'ignore')
    if len(bs) == 8:
        cands.add(bs)
    elif 8 < len(bs) <= 64:
        cands.add(bs[:8])
print('candidates', len(cands))
blocks = [bytes.fromhex(h) for h in ['b287a09d727ae6a5', '3b7ddaffd7e487ef', '5b9866668a2742c7', 'd55e62efddccd8e97b1ff081a5c2eef8'[:16]]]
pr = re.compile(rb'^[\x20-\x7e]{6,8}')
hits = []
for k in cands:
    c = DES.new(k, DES.MODE_ECB)
    for mode in ('ecb', 'cbc_iv_key'):
        ok = 0; outs = []
        for blk in blocks:
            p = c.decrypt(blk)
            if mode == 'cbc_iv_key':
                p = bytes(a ^ b for a, b in zip(p, k))
            outs.append(p)
            if pr.match(p):
                ok += 1
        if ok >= 3:
            hits.append((k, mode, outs))
for h in hits[:20]:
    print(h)
print('hits', len(hits))
