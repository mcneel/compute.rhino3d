import flask

import hops.base as base
import hops.middlewares as hmw


__all__ = ["Hops"]


class Hops:
    """Hops Middleware for Flask"""

    def __new__(cls, app, *args, **kwargs):
        if app is None:
            return hmw.HopsDefault()
        elif isinstance(app, flask.Flask):
            return hmw.HopsFlask(app, *args, **kwargs)

        raise Exception("Unsupported app")
