"""Hops flask middleware implementation"""
import ghhops_server.base as base

from werkzeug.wrappers import Request, Response


class HopsFlask(base.HopsBase):
    """Hops Middleware for Flask"""

    ERROR_PAGE_405 = """<!doctype html>
    <html lang=en>
    <title>405 Method Not Allowed</title>
    <h1>Method Not Allowed</h1>
    <p>The method is not allowed for the requested URL.</p>"""

    def __init__(self, flask_app):
        # keep a ref to original flask app
        super(HopsFlask, self).__init__(flask_app)
        # and and wsgi_app
        self.wsgi_app = flask_app.wsgi_app
        # replace wsgi_app with self, this instance will call the bubble up
        # the unknown/unhandled messages to the original wsgi_app
        flask_app.wsgi_app = self

    def _prep_response(self, status=200, msg=None):
        return Response(
            msg if msg else "Success",
            mimetype="application/json",
            status=status,
        )

    def __call__(self, environ, start_response):
        request = Request(environ)

        method = request.method
        comp_uri = request.path

        # if uri is registered on the hops app
        if self.contains(comp_uri):
            if method == "GET":
                # if component exists
                res, results = self.query(uri=comp_uri)
                if res:
                    response = self._prep_response()
                    response.data = results
                else:
                    response = self._prep_response(404, "Unknown URI")
                return response(environ, start_response)

            elif method == "POST":
                data = request.data
                res, results = self.solve(uri=comp_uri, payload=data)
                if res:
                    response = self._prep_response()
                    response.data = results.encode(encoding="utf_8")
                else:
                    response = self._prep_response(404, "Execution Error")
                    response.data = results.encode(encoding="utf_8")
                return response(environ, start_response)

            # respond with 405 if method is not valid
            response = self._prep_response(405, "Method Not Allowed")
            response.data = HopsFlask.ERROR_PAGE_405.encode(encoding="utf_8")
            return response(environ, start_response)

        # otherwise ask wapped app to process the call
        return self.wsgi_app(environ, start_response)
