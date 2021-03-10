"""Grasshopper Hops Server"""
import ghhops_server.base as base
import ghhops_server.middlewares as hmw

import flask


__all__ = ["Hops"]
__version__ = "1.0.0"


class Hops(base.HopsBase):
    """Hops Middleware"""

    def __new__(cls, app, *args, **kwargs) -> base.HopsBase:
        if app is None:
            return hmw.HopsDefault()
        elif isinstance(app, flask.Flask):
            return hmw.HopsFlask(app, *args, **kwargs)

        raise Exception("Unsupported app")
