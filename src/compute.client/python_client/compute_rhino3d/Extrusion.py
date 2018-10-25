from . import Util


def GetWireframe(thisExtrusion):
    args = [thisExtrusion]
    response = Util.ComputeFetch("rhino/geometry/extrusion/getwireframe-extrusion", args)
    return response

