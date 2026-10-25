"""Teste do `local --continuo` com a fila rodando no meio, sem o KoboldCpp e o OmniRoute de verdade.

Monta um repositório temporário (MODTOI_REPO) com 120 entradas, sobe um servidor falso que responde como o
KoboldCpp e como o OmniRoute e roda: o local contínuo em lotes de 20; com ele no meio do lote 002, uma rodada da
fila (APIs reservando 15 por vez); e, depois que o local acaba, outra rodada para juntar o resto.
Confere que toda entrada foi traduzida uma vez só: nenhuma em duas saídas e nenhuma perdida.

Uso: python tools/ptbr/teste_local_continuo.py
"""
import json
import os
import shutil
import subprocess
import sys
import tempfile
import threading
import time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

HERE = Path(__file__).resolve().parent
REAL_REPO = HERE.parents[1]
TMP = Path(tempfile.mkdtemp(prefix='modtoi_teste_'))
os.environ.update({'MODTOI_REPO': str(TMP), 'PTBR_LOTE_LOCAL': '20', 'PTBR_RESERVA_APIS': '15'})
sys.path.insert(0, str(HERE))
import ptbr  # noqa: E402  (depois do MODTOI_REPO: os caminhos apontam para TMP)

KEYS = ([f'setting_t{i:03d}' for i in range(30)] + [f'item_name_t{i:03d}' for i in range(30)]
        + [f'drama_dialogue_t{i:03d}' for i in range(60)])


class Fake(BaseHTTPRequestHandler):
    def log_message(self, *a):
        pass

    def _send(self, obj):
        body = json.dumps(obj).encode()
        self.send_response(200)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Content-Length', str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        self._send({'result': ptbr.LOCAL_ID} if self.path == '/api/v1/model' else {'data': []})

    def do_POST(self):
        req = json.loads(self.rfile.read(int(self.headers['Content-Length'])))
        msgs = req['messages']
        if any(m['role'] == 'system' for m in msgs):  # fila -> "OmniRoute": JSON {chave: tradução}
            batch = json.loads(msgs[-1]['content'].rsplit('\n', 1)[-1])
            time.sleep(0.3)
            content = json.dumps({k: 'API ' + v['en'] for k, v in batch.items()})
        else:  # local -> "KoboldCpp": só o texto
            time.sleep(0.15)
            content = 'LOCAL ' + msgs[0]['content'].rsplit('\n\n', 1)[-1]
        self._send({'choices': [{'message': {'content': content}, 'finish_reason': 'stop'}], 'usage': {}})


def wait(cond, timeout, what):
    end = time.time() + timeout
    while time.time() < end:
        if cond():
            return
        time.sleep(0.2)
    raise SystemExit(f'FALHOU: tempo esgotado esperando {what}')


def main():
    sys.stdout.reconfigure(encoding='utf-8')
    shutil.copytree(REAL_REPO / 'glossario', TMP / 'glossario')
    ptbr.save_work({k: {'ch': '', 'en': f'Line {k}', 'pt': ''} for k in KEYS})
    server = ThreadingHTTPServer(('127.0.0.1', 0), Fake)
    threading.Thread(target=server.serve_forever, daemon=True).start()
    url = f'http://127.0.0.1:{server.server_address[1]}'
    env = {**os.environ, 'HYMT2_URL': url, 'OMNIROUTE_URL': url + '/v1', 'PYTHONIOENCODING': 'utf-8'}
    uv = shutil.which('uv') or str(Path.home() / '.local/bin/uv.exe')
    fila_cmd = [uv, 'run', '--quiet', '--script', str(HERE / 'fila.py'), '--modelos', 'gemini/gemini-3-flash-preview']

    local = subprocess.Popen([sys.executable, '-u', str(HERE / 'ptbr.py'), 'local', '--continuo',
                              '--log', str(TMP / 'local_teste.log')], env=env)
    wait(lambda: (ptbr.LOCAL_DIR / 'src_lote002.json').exists(), 60, 'o local abrir o lote 002')
    print('local no lote 002; rodando a fila no meio')
    r1 = subprocess.run(fila_cmd, env=env, capture_output=True, text=True, encoding='utf-8')
    wait(lambda: local.poll() is not None, 120, 'o local terminar')
    r2 = subprocess.run(fila_cmd, env=env, capture_output=True, text=True, encoding='utf-8')
    server.shutdown()

    erros = []
    log1, log_local = r1.stdout + r1.stderr, (TMP / 'local_teste.log').read_text(encoding='utf-8')
    if r1.returncode or r2.returncode:
        erros.append(f'fila saiu com {r1.returncode}/{r2.returncode}:\n{log1[-2000:]}\n{(r2.stdout + r2.stderr)[-2000:]}')
    if 'já rodando' not in log1:
        erros.append('a primeira fila não viu o local rodando (poderia ter iniciado um segundo)')
    if 'sem pendentes livres' not in log_local:
        erros.append('o local não parou por falta de pendentes')
    if ptbr.API_RESERVE.exists():
        erros.append('a reserva das APIs ficou para trás')

    lotes = [set(ptbr.read_json(s)) for s in ptbr.local_batches()]
    if len(lotes) < 3:
        erros.append(f'esperava 3 ou mais lotes locais, houve {len(lotes)}')
    for i, a in enumerate(lotes):
        for b in lotes[i + 1:]:
            if a & b:
                erros.append(f'entrada em dois lotes locais: {sorted(a & b)[:5]}')
    donos = {}
    saidas = [ptbr.batch_output(s) for s in ptbr.local_batches()] + list(ptbr.FILA_DIR.glob('*/*.json'))
    for out in saidas:
        for k, v in ptbr.read_json(out, {}).items():
            donos.setdefault(k, []).append((out.name, v['pt']))
    work = ptbr.load_work()
    for k in KEYS:
        quem = donos.get(k, [])
        if len(quem) != 1:
            erros.append(f'{k}: {len(quem)} saídas {quem}')
        elif work[k].get('pt') != quem[0][1]:
            erros.append(f'{k}: pt no traducao/ ({work[k].get("pt")!r}) difere da saída {quem[0]}')
    n_local = sum(1 for q in donos.values() if q and q[0][0].startswith('out_lote'))
    n_api = len(donos) - n_local
    if not n_local or not n_api:
        erros.append(f'esperava as duas rotas trabalhando: local {n_local}, APIs {n_api}')

    print(f'{len(KEYS)} entradas | local: {n_local} em {len(lotes)} lotes | APIs: {n_api} | diretório: {TMP}')
    if erros:
        print('FALHOU:\n- ' + '\n- '.join(erros))
        return 1
    print('ok: nenhuma entrada duplicada, nenhuma perdida')
    shutil.rmtree(TMP, ignore_errors=True)
    return 0


if __name__ == '__main__':
    sys.exit(main())
