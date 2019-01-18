from . import Util


def CreateCubicBeziers(sourceCurve, distanceTolerance, kinkTolerance, multiple=False):
    """
    Constructs an array of cubic, non-rational beziers that fit a curve to a tolerance.

    Args:
        sourceCurve (Curve): A curve to approximate.
        distanceTolerance (double): The max fitting error. Use RhinoMath.SqrtEpsilon as a minimum.
        kinkTolerance (double): If the input curve has a g1-discontinuity with angle radian measure
            greater than kinkTolerance at some point P, the list of beziers will
            also have a kink at P.

    Returns:
        BezierCurve[]: A new array of bezier curves. The array can be empty and might contain None items.
    """
    url = "rhino/geometry/beziercurve/createcubicbeziers-curve_double_double"
    if multiple: url += "?multiple=true"
    args = [sourceCurve, distanceTolerance, kinkTolerance]
    if multiple: args = zip(sourceCurve, distanceTolerance, kinkTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBeziers(sourceCurve, multiple=False):
    """
    Create an array of Bezier curves that fit to an existing curve. Please note, these
    Beziers can be of any order and may be rational.

    Args:
        sourceCurve (Curve): The curve to fit Beziers to

    Returns:
        BezierCurve[]: A new array of Bezier curves
    """
    url = "rhino/geometry/beziercurve/createbeziers-curve"
    if multiple: url += "?multiple=true"
    args = [sourceCurve]
    if multiple: args = [[item] for item in sourceCurve]
    response = Util.ComputeFetch(url, args)
    return response

