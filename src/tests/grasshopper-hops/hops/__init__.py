"""Grasshopper Hops, Duh!"""
import flask

import hops.base as base
import hops.middlewares as hmw


__all__ = ["Hops"]


class Hops(base.HopsBase):
    """Hops Middleware"""

    def __new__(cls, app, *args, **kwargs) -> base.HopsBase:
        if app is None:
            return hmw.HopsDefault()
        elif isinstance(app, flask.Flask):
            return hmw.HopsFlask(app, *args, **kwargs)

        raise Exception("Unsupported app")
