"""Grasshopper Hops Server"""

import ghhops_server.base as base
import ghhops_server.middlewares as hmw
from ghhops_server.middlewares.hopsflask import flask
from ghhops_server.logger import hlogger

# import all supported servers for easy typehinting
from ghhops_server.middlewares import *  # noqa

# import all supported parameter types for easy access
from ghhops_server.params import *  # noqa


__version__ = "1.0.0"


class Hops(base.HopsBase):
    """Hops Middleware"""

    def __new__(cls, app=None, debug=False, *args, **kwargs) -> base.HopsBase:
        # determine the correct middleware base on the source app being wrapped
        if app is None:
            hlogger.debug("Using Hops default http server")
            return hmw.HopsDefault()
        elif isinstance(app, flask.Flask):
            hlogger.debug("Using Hops Flask middleware")
            return hmw.HopsFlask(app, *args, **kwargs)
        raise Exception("Unsupported app")
