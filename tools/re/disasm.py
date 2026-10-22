"""Disassemble IL2CPP functions in GameAssembly.dll with name + string-literal annotation."""
import sys; sys.stdout.reconfigure(encoding="utf-8")
import sys, json, struct, glob
import pefile
from capstone import Cs, CS_ARCH_X86, CS_MODE_64

GA = r"D:/Games/Tale of Immortal/GameAssembly.dll"
pe = pefile.PE(GA, fast_load=True)
data = bytes(pe.__data__)
base = pe.OPTIONAL_HEADER.ImageBase
names = {}
for f in glob.glob('rva_*.json'):
    for k, v in json.load(open(f)).items():
        for pair in v:
            if pair:
                names[int(pair[0], 16)] = k
lits = json.load(open('literals.json', encoding='utf-8'))
TYPES = {1: 'TypeInfo', 2: 'Il2CppType', 3: 'MethodDef', 4: 'FieldInfo', 5: 'String', 6: 'MethodRef'}
types = json.load(open('meta_types.json', encoding='utf-8'))

def off(rva):
    return pe.get_offset_from_rva(rva)

def usage(rva):
    try:
        v = struct.unpack_from('<Q', data, off(rva))[0]
    except Exception:
        return None
    if v & 1 and v < (1 << 32):
        t = (v & 0xE0000000) >> 29; idx = (v & 0x1FFFFFFE) >> 1
        if t == 5 and idx < len(lits):
            return 'STR ' + repr(lits[idx][:120])
        if t == 1 and idx < len(types):
            return 'TypeInfo ' + types[idx]['name']
        if t in TYPES:
            return f'{TYPES[t]}#{idx}'
    return None

md = Cs(CS_ARCH_X86, CS_MODE_64); md.detail = False
for rva_s in sys.argv[1:]:
    rva = int(rva_s, 16)
    print(f'===== {names.get(rva, "?")} @ {rva:#x}')
    code = data[off(rva):off(rva) + 0x1200]
    for ins in md.disasm(code, base + rva):
        ann = ''
        if ins.mnemonic in ('call', 'jmp') and ins.op_str.startswith('0x'):
            t = int(ins.op_str, 16) - base
            ann = names.get(t, '')
        if 'rip +' in ins.op_str or 'rip -' in ins.op_str:
            import re
            m = re.search(r'rip ([+-]) (0x[0-9a-f]+)', ins.op_str)
            disp = int(m.group(2), 16) * (1 if m.group(1) == '+' else -1)
            tgt = ins.address + ins.size + disp - base
            ann = usage(tgt) or f'[{tgt:#x}]'
        print(f'{ins.address - base:#x}: {ins.mnemonic:6s} {ins.op_str:45s} {ann}')
        if ins.mnemonic == 'int3':
            break
