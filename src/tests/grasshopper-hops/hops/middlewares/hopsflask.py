import hops.base as base

from werkzeug.wrappers import Request, Response, ResponseStream


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
        if comp_uri := environ.get("REQUEST_URI", None):
            if comp := self.components.get(comp_uri, None):
                outputs = self.solve(comp, *self.prepare_inputs(environ, comp))
                res = Response("Success", mimetype="application/json", status=200)
                res.data = self.prepare_outputs(outputs)
                return res(environ, start_response)
        return self.wsgi_app(environ, start_response)