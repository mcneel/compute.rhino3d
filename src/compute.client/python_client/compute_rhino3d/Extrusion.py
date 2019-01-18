from . import Util


def GetWireframe(thisExtrusion, multiple=False):
    """
    Constructs all the Wireframe curves for this Extrusion.

    Returns:
        Curve[]: An array of Wireframe curves.
    """
    url = "rhino/geometry/extrusion/getwireframe-extrusion"
    if multiple: url += "?multiple=true"
    args = [thisExtrusion]
    if multiple: args = [[item] for item in thisExtrusion]
    response = Util.ComputeFetch(url, args)
    return response

