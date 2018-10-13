import Util


def GetWireframe(extrusion):
    args = [extrusion]
    response = Util.ComputeFetch("rhino/geometry/extrusion/getwireframe-extrusion", args)
    return response

