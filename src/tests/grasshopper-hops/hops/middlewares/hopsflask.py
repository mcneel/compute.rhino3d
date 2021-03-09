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
        method = environ.get("REQUEST_METHOD", "GET")
        comp_uri = environ.get("REQUEST_URI", "/")
        if method == "GET":
            res, results = self.query(uri=comp_uri)
            if res:
                res = Response("Success", mimetype="application/json", status=200)
                res.data = results
            else:
                res = Response("Unknown URI", mimetype="application/json", status=404)
            return res(environ, start_response)
        elif method == "POST":
            # TODO: grab data and execute
            pass
            # res, results = self.solve(uri=comp_uri, payload=data)
            # if res:
            #     res = Response("Success", mimetype="application/json", status=200)
            #     res.data = results
            # else:
            #     res = Response("Unknown URI", mimetype="application/json", status=404)
            #     res.data = results
        return self.wsgi_app(environ, start_response)