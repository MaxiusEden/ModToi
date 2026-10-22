"""Minimal ECMA-335 reader: map MethodDef -> RVA from Cpp2IL AddressAttribute on a dummy DLL."""
import struct, sys, json, re
import pefile

path, want = sys.argv[1], re.compile(sys.argv[2])
pe = pefile.PE(path, fast_load=True)
data = bytes(pe.__data__)
clidir = pe.OPTIONAL_HEADER.DATA_DIRECTORY[14]
cli_off = pe.get_offset_from_rva(clidir.VirtualAddress)
md_rva = struct.unpack_from('<I', data, cli_off + 8)[0]
md_off = pe.get_offset_from_rva(md_rva)
assert data[md_off:md_off + 4] == b'BSJB'
vlen = struct.unpack_from('<I', data, md_off + 12)[0]
p = md_off + 16 + vlen
flags, nstreams = struct.unpack_from('<HH', data, p); p += 4
streams = {}
for _ in range(nstreams):
    o, s = struct.unpack_from('<II', data, p); p += 8
    e = data.index(b'\0', p); name = data[p:e].decode(); p = (e + 4) & ~3
    streams[name] = (md_off + o, s)
tb, _ = streams['#~']
hs = data[tb + 6]
valid, = struct.unpack_from('<Q', data, tb + 8)
p = tb + 24
rows = {}
for i in range(64):
    if valid >> i & 1:
        rows[i] = struct.unpack_from('<I', data, p)[0]; p += 4
SI = 4 if hs & 1 else 2; GI = 4 if hs & 2 else 2; BI = 4 if hs & 4 else 2
def idx(t): return 4 if rows.get(t, 0) > 0xFFFF else 2
def cidx(tables, bits):
    m = max(rows.get(t, 0) for t in tables if t is not None)
    return 2 if m < (1 << (16 - bits)) else 4
TDOR = cidx([2, 1, 0x1b], 2); RS = cidx([0, 0x1a, 0x23, 1], 2)
HCA = cidx([6, 4, 1, 2, 8, 9, 10, 0, 0xe, 0x17, 0x14, 0x11, 0x1a, 0x1b, 0x20, 0x23, 0x26, 0x27, 0x28, 0x2a, 0x2c, 0x2b], 5)
CAT = cidx([None, None, 6, 10, None], 3)
HC = cidx([4, 8, 0x17], 2); HFM = cidx([4, 8], 1); HDS = cidx([2, 6, 0x20], 2)
MRP = cidx([2, 1, 0x1a, 6, 0x1b], 3); HS = cidx([0x14, 0x17], 1); MDOR = cidx([6, 10], 1)
MF = cidx([4, 6], 1); IMPL = cidx([0x26, 0x23, 0x27], 2); TOMD = cidx([2, 6], 1)
sizes = {
 0: 2 + SI + GI * 3, 1: RS + SI * 2, 2: 4 + SI * 2 + TDOR + idx(4) + idx(6), 3: idx(4), 4: 2 + SI + BI,
 5: idx(6), 6: 4 + 2 + 2 + SI + BI + idx(8), 7: idx(8), 8: 2 + 2 + SI, 9: idx(2) + TDOR,
 10: MRP + SI + BI, 11: 2 + HC + BI, 12: HCA + CAT + BI,
}
off = {}
q = p
for t in sorted(rows):
    if t not in sizes:
        break
    off[t] = q; q += sizes[t] * rows[t]
strings_off = streams['#Strings'][0]; blob_off = streams['#Blob'][0]
def rd(o, n): return struct.unpack_from('<H' if n == 2 else '<I', data, o)[0]
def S(i): e = data.index(b'\0', strings_off + i); return data[strings_off + i:e].decode('utf-8', 'replace')
def blob(i):
    o = blob_off + i; b0 = data[o]
    if b0 & 0x80 == 0: return data[o + 1:o + 1 + b0]
    if b0 & 0xC0 == 0x80: n = ((b0 & 0x3f) << 8) | data[o + 1]; return data[o + 2:o + 2 + n]
    n = ((b0 & 0x1f) << 24) | (data[o + 1] << 16) | (data[o + 2] << 8) | data[o + 3]; return data[o + 4:o + 4 + n]
# TypeDef: flags, name, ns, extends, fieldlist, methodlist
types = []
for r in range(rows[2]):
    o = off[2] + r * sizes[2]
    name = S(rd(o + 4, SI)); ns = S(rd(o + 4 + SI, SI))
    ml = rd(o + 4 + SI * 2 + TDOR + idx(4), idx(6))
    types.append((ns, name, ml))
def meth_type(mi):
    lo, hi = 0, len(types) - 1
    while lo < hi:
        mid = (lo + hi + 1) // 2
        if types[mid][2] <= mi: lo = mid
        else: hi = mid - 1
    return types[lo]
mnames = []
for r in range(rows[6]):
    o = off[6] + r * sizes[6]
    mnames.append(S(rd(o + 8, SI)))
# find AddressAttribute ctor MethodDef row
addr_ctor = None
print({hex(k):v for k,v in rows.items()}, len(types), len(mnames)); print([ (t, mnames[t[2]-1:t[2]+8]) for t in types if t[1]=="AddressAttribute"])
for ti, (ns, name, ml) in enumerate(types):
    if name == 'AddressAttribute':
        pass
import collections
cc=collections.Counter()
for r in range(rows[12]):
    o = off[12] + r * sizes[12]
    parent = rd(o, HCA); ctype = rd(o + HCA, CAT)
    if parent & 31 == 0: cc[ctype]+=1
for ct,n in cc.most_common(5):
    mi=ct>>3; print('ctype tag',ct&7,'idx',mi,n, meth_type(mi)[:2] if ct&7==2 else '', mnames[mi-1] if ct&7==2 else '')
addr_ctor = [ct>>3 for ct,_ in cc.most_common(2) if meth_type(ct>>3)[1]=="AddressAttribute"][0]
out = {}
for r in range(rows[12]):
    o = off[12] + r * sizes[12]
    parent = rd(o, HCA); ctype = rd(o + HCA, CAT); val = rd(o + HCA + CAT, BI)
    if parent & 31 != 0:  # MethodDef
        continue
    if ctype & 7 != 2 or (ctype >> 3) != addr_ctor:
        continue
    mi = parent >> 5
    ns, tn, _ = meth_type(mi)
    full = f"{ns}.{tn}::{mnames[mi - 1]}" if ns else f"{tn}::{mnames[mi - 1]}"
    if not want.search(full):
        continue
    b = blob(val)
    txt = re.findall(rb'0x[0-9A-Fa-f]+', b)
    out.setdefault(full, []).append([t.decode() for t in txt])
json.dump(out, open(sys.argv[3], 'w'), indent=0)
print('matches', len(out))
