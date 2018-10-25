from . import Util


def CreateCubicBeziers(sourceCurve, distanceTolerance, kinkTolerance):
    args = [sourceCurve, distanceTolerance, kinkTolerance]
    response = Util.ComputeFetch("rhino/geometry/beziercurve/createcubicbeziers-curve_double_double", args)
    return response


def CreateBeziers(sourceCurve):
    args = [sourceCurve]
    response = Util.ComputeFetch("rhino/geometry/beziercurve/createbeziers-curve", args)
    return response

