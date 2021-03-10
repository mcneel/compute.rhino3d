"""Hops flask middleware implementation"""
import ghhops_server.base as base

from werkzeug.wrappers import Request, Response


class HopsFlask(base.HopsBase):
    """Hops Middleware for Flask"""

    def __init__(self, flask_app):
        # keep a ref to original flask app
        super(HopsFlask, self).__init__(flask_app)
        # and and wsgi_app
        self.wsgi_app = flask_app.wsgi_app
        # replace wsgi_app with self, this instance will call the bubble up
        # the unknown/unhandled messages to the original wsgi_app
        flask_app.wsgi_app = self

    def __call__(self, environ, start_response):
        request = Request(environ)

        method = request.method
        comp_uri = request.path

        if method == "GET":
            res, results = self.query(uri=comp_uri)
            if res:
                response = Response(
                    "Success", mimetype="application/json", status=200
                )
                response.data = results
            else:
                response = Response(
                    "Unknown URI", mimetype="application/json", status=404
                )
            return response(environ, start_response)

        elif method == "POST":
            data = request.data
            res, results = self.solve(uri=comp_uri, payload=data)
            if res:
                response = Response(
                    "Success", mimetype="application/json", status=200
                )
                response.data = results.encode(encoding="utf_8")
            else:
                response = Response(
                    "Execution Error", mimetype="application/json", status=500
                )
                response.data = results.encode(encoding="utf_8")
            return response(environ, start_response)

        return self.wsgi_app(environ, start_response)
