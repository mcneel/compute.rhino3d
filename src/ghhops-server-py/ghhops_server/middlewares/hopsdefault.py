"""Hops builtin HTTP server"""
import ghhops_server.base as base

from http.server import HTTPServer, BaseHTTPRequestHandler


class HopsDefault(base.HopsBase):
    """Hops builtin HTTP server implementation"""

    def __init__(self):
        super(HopsDefault, self).__init__(None)

    def start(self, address="localhost", port=5000):
        """Start hops builtin http server on given address:port"""
        _HopsHTTPHandler.hops = self
        httpd = HTTPServer((address, port), _HopsHTTPHandler)
        print(f"Starting hops python server on {address}:{port}")
        httpd.serve_forever()


class _HopsHTTPHandler(BaseHTTPRequestHandler):
    hops: HopsDefault = None

    def _get_comp_uri(self):
        return self.path.split("?")[0]

    def _set_headers(self):
        self.send_response(200)
        self.send_header("Content-type", "application/json")
        self.end_headers()

    def do_GET(self):
        # grab the path before url params
        comp_uri = self._get_comp_uri()
        res, results = self.hops.query(uri=comp_uri)
        if res:
            self._set_headers()
            self.wfile.write(results.encode(encoding="utf_8"))
        else:
            self.send_response(404)

    def do_HEAD(self):
        self._set_headers()

    def do_POST(self):
        # read the message and convert it into a python dictionary
        comp_uri = self._get_comp_uri()
        length = int(self.headers.get("Content-Length"))
        data = self.rfile.read(length)
        res, results = self.hops.solve(uri=comp_uri, payload=data)
        if res:
            self._set_headers()
            self.wfile.write(results.encode(encoding="utf_8"))
        else:
            # TODO: write proper errors
            self.send_response(500, "Execution Error")
            self.wfile.write(results.encode(encoding="utf_8"))
