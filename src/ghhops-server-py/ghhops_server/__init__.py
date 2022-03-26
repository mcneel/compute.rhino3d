"""Grasshopper Hops Server"""
import importlib
import ghhops_server.base as base
import ghhops_server.middlewares as hmw
from ghhops_server import params
from ghhops_server.logger import logging, hlogger

# import all supported servers for easy typehinting
from ghhops_server.middlewares import *  # noqa

# import all supported parameter types for easy access
from ghhops_server.params import *  # noqa


# main module version for pypi build
__version__ = "1.5.2"


class Hops(base.HopsBase):
    """Hops Middleware"""

    def __new__(cls, app=None, debug=False, *args, **kwargs) -> base.HopsBase:
        # set logger level
        hlogger.setLevel(logging.DEBUG if debug else logging.INFO)

        # determine the correct middleware base on the source app being wrapped
        # when running standalone with no source apps
        if app is None:
            hlogger.debug("Using Hops default http server")
            params._init_rhino3dm()
            return hmw.HopsDefault()

        # if wrapping another app
        app_type = repr(app)
        # if app is Flask
        if app_type.startswith("<Flask"):
            hlogger.debug("Using Hops Flask middleware")
            params._init_rhino3dm()
            return hmw.HopsFlask(app, *args, **kwargs)

        # if wrapping rhinoinside
        # paractically this is not necessary. it is implemented this way
        # mostly to provide a level of consistency on how Hops is used
        elif app_type.startswith("<module 'rhinoinside'"):
            # detemine if running with rhino.inside.cpython
            # and init the param module accordingly
            if not Hops.is_inside():
                raise Exception("rhinoinside is not loaded yet")
            hlogger.debug("Using Hops default http server with rhinoinside")
            params._init_rhinoinside()
            return hmw.HopsDefault(*args, **kwargs)

        raise Exception("Unsupported app")

    @classmethod
    def is_inside(cls):
        try:
            ri = importlib.import_module("rhinoinside")
            ri_core = getattr(ri, "__rhino_core", None)
            return ri_core is not None
        except Exception:
            return False
