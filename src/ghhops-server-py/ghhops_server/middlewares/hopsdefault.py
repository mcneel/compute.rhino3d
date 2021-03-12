"""Hops builtin HTTP server"""
import ghhops_server.base as base
from ghhops_server.logger import logging, hlogger

from http.server import ThreadingHTTPServer, BaseHTTPRequestHandler


class HopsDefault(base.HopsBase):
    """Hops builtin HTTP server implementation"""

    def __init__(self):
        super(HopsDefault, self).__init__(None)

    def start(self, address="localhost", port=5000, debug=False):
        """Start hops builtin http server on given address:port"""
        # setup logging
        hlogger.setLevel(logging.DEBUG if debug else logging.INFO)
        # start ther server
        _HopsHTTPHandler.hops = self
        httpd = ThreadingHTTPServer((address, port), _HopsHTTPHandler)
        hlogger.info("Starting hops python server on %s:%s", address, port)
        httpd.serve_forever()


class _HopsHTTPHandler(BaseHTTPRequestHandler):
    hops: HopsDefault = None

    def __init__(self, request, client_address, server):
        super(_HopsHTTPHandler, self).__init__(request, client_address, server)

    def log_message(self, format, *args):
        """Overriding BaseHTTPRequestHandler.log_message"""
        hlogger.info(
            "%s - - [%s] %s"
            % (
                self.address_string(),
                self.log_date_time_string(),
                format % args,
            )
        )

    def _get_comp_uri(self):
        return self.path.split("?")[0]

    def _prep_response(self, status=200, msg=None):
        self.send_response(status, msg if msg else "Success")
        self.send_header("Content-type", "application/json")
        self.end_headers()

    def do_GET(self):
        # grab the path before url params
        comp_uri = self._get_comp_uri()
        res, results = self.hops.query(uri=comp_uri)
        hlogger.debug(f"{res} : {results}")
        if res:
            self._prep_response()
            self.wfile.write(results.encode(encoding="utf_8"))
        else:
            self._prep_response(status=404)

    def do_HEAD(self):
        self._prep_response()

    def do_POST(self):
        # read the message and convert it into a python dictionary
        comp_uri = self._get_comp_uri()
        length = int(self.headers.get("Content-Length"))
        data = self.rfile.read(length)
        res, results = self.hops.solve(uri=comp_uri, payload=data)
        hlogger.debug(f"{res} : {results}")
        if res:
            self._prep_response()
            self.wfile.write(results.encode(encoding="utf_8"))
        else:
            # TODO: write proper errors
            self._prep_response(500, "Execution Error")
            self.wfile.write(results.encode(encoding="utf_8"))
