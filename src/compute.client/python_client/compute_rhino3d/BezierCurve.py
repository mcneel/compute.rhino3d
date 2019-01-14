from . import Util


def CreateCubicBeziers(sourceCurve, distanceTolerance, kinkTolerance, multiple=False):
    url = "rhino/geometry/beziercurve/createcubicbeziers-curve_double_double"
    if multiple: url += "?multiple=true"
    args = [sourceCurve, distanceTolerance, kinkTolerance]
    if multiple: args = zip(sourceCurve, distanceTolerance, kinkTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBeziers(sourceCurve, multiple=False):
    url = "rhino/geometry/beziercurve/createbeziers-curve"
    if multiple: url += "?multiple=true"
    args = [sourceCurve]
    if multiple: args = zip(sourceCurve)
    response = Util.ComputeFetch(url, args)
    return response

