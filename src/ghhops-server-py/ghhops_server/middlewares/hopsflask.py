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

    def _prep_response(self, status=200, msg=None):
        return Response(
            msg if msg else "Success",
            mimetype="application/json",
            status=status,
        )

    def __call__(self, environ, start_response):
        request = Request(environ)

        method = request.method

        # if hops app handled this request
        if self.handles(request):
            response = None

            if method == "HEAD":
                response = self.handle_HEAD(request)

            elif method == "GET":
                response = self.handle_GET(request)

            elif method == "POST":
                response = self.handle_POST(request)

            if response:
                return response(environ, start_response)

            # respond with 405 if method is not valid
            return self._return_method_not_allowed()

        # otherwise ask wapped app to process the call
        return self.wsgi_app(environ, start_response)
