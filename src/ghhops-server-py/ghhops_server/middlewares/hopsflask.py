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
        comp_uri = request.path

        # if uri is registered on the hops app
        if self.handles(comp_uri):
            if method == "GET":
                # if calling GET /solve, respond 405
                if self.is_solve_uri(comp_uri):
                    return self._return_method_not_allowed(
                        environ,
                        start_response
                        )

                # if component exists, return component data
                res, results = self.query(uri=comp_uri)
                if res:
                    response = self._prep_response()
                    response.data = results

                # otherwise return 404
                else:
                    response = self._prep_response(404, "Unknown URI")

                return response(environ, start_response)

            elif method == "POST":
                # if POST on component uri, return 405
                if self.is_comp_uri(comp_uri):
                    return self._return_method_not_allowed(
                        environ,
                        start_response
                        )

                # otherwise try to solve with payload
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
            return self._return_method_not_allowed(environ, start_response)

        # otherwise ask wapped app to process the call
        return self.wsgi_app(environ, start_response)
