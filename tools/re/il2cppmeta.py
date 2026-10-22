"""Minimal IL2CPP v27 global-metadata.dat parser: types, fields, methods, enum values."""
import struct, sys, json

HDR = ["stringLiteral", "stringLiteralData", "string", "events", "properties", "methods",
       "parameterDefaultValues", "fieldDefaultValues", "fieldAndParameterDefaultValueData",
       "fieldMarshaledSizes", "parameters", "fields", "genericParameters",
       "genericParameterConstraints", "genericContainers", "nestedTypes", "interfaces",
       "vtableMethods", "interfaceOffsets", "typeDefinitions", "images", "assemblies"]

b = open(sys.argv[1], 'rb').read()
sanity, ver = struct.unpack_from('<II', b, 0)
assert ver == 27, ver
h = {}
for i, n in enumerate(HDR):
    h[n] = struct.unpack_from('<II', b, 8 + i * 8)

so = h['string'][0]
def S(idx):
    e = b.index(b'\0', so + idx)
    return b[so + idx:e].decode('utf-8', 'replace')

TD = 88
off, size = h['typeDefinitions']
assert size % TD == 0, (size, size % TD)
ntypes = size // TD
fo, fs = h['fields']; assert fs % 12 == 0
mo, ms = h['methods']; assert ms % 32 == 0
dvo, dvs = h['fieldDefaultValues']
dataoff = h['fieldAndParameterDefaultValueData'][0]
defaults = {}
for i in range(dvs // 12):
    fi, ti, di = struct.unpack_from('<iii', b, dvo + i * 12)
    defaults[fi] = di

types = []
for t in range(ntypes):
    v = struct.unpack_from('<16i8HII', b, off + t * TD)
    name, ns = S(v[0]), S(v[1])
    declaring, parent = v[3], v[4]
    fieldStart, methodStart = v[8], v[9]
    mcount, pcount, fcount = v[16], v[17], v[18]
    fields = []
    for k in range(fcount):
        fi = fieldStart + k
        fn = S(struct.unpack_from('<i', b, fo + fi * 12)[0])
        dv = None
        if fi in defaults and defaults[fi] >= 0:
            dv = struct.unpack_from('<i', b, dataoff + defaults[fi])[0]
        fields.append([fn, dv])
    methods = []
    for k in range(mcount):
        mi = methodStart + k
        mv = struct.unpack_from('<6i4H', b, mo + mi * 32)
        methods.append([S(mv[0]), mv[9]])
    types.append({'ns': ns, 'name': name, 'declaring': declaring, 'fields': fields, 'methods': methods})

json.dump(types, open(sys.argv[2], 'w', encoding='utf-8'), ensure_ascii=False)
print('types', ntypes)
