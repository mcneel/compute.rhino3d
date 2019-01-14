from . import Util


def GetWireframe(thisExtrusion, multiple=False):
    url = "rhino/geometry/extrusion/getwireframe-extrusion"
    if multiple: url += "?multiple=true"
    args = [thisExtrusion]
    if multiple: args = zip(thisExtrusion)
    response = Util.ComputeFetch(url, args)
    return response

