from . import Util


def GetConicSectionType(thisCurve, multiple=False):
    """
    Returns the type of conic section based on the curve's shape.
    """
    url = "rhino/geometry/curve/getconicsectiontype-curve"
    if multiple: url += "?multiple=true"
    args = [thisCurve]
    if multiple: args = [[item] for item in thisCurve]
    response = Util.ComputeFetch(url, args)
    return response


def CreateInterpolatedCurve(points, degree, multiple=False):
    """
    Interpolates a sequence of points. Used by InterpCurve Command
    This routine works best when degree=3.

    Args:
        degree (int): The degree of the curve >=1.  Degree must be odd.
        points (IEnumerable<Point3d>): Points to interpolate (Count must be >= 2)

    Returns:
        Curve: interpolated curve on success. None on failure.
    """
    url = "rhino/geometry/curve/createinterpolatedcurve-point3darray_int"
    if multiple: url += "?multiple=true"
    args = [points, degree]
    if multiple: args = zip(points, degree)
    response = Util.ComputeFetch(url, args)
    return response


def CreateInterpolatedCurve1(points, degree, knots, multiple=False):
    """
    Interpolates a sequence of points. Used by InterpCurve Command
    This routine works best when degree=3.

    Args:
        degree (int): The degree of the curve >=1.  Degree must be odd.
        points (IEnumerable<Point3d>): Points to interpolate. For periodic curves if the final point is a
            duplicate of the initial point it is  ignored. (Count must be >=2)
        knots (CurveKnotStyle): Knot-style to use  and specifies if the curve should be periodic.

    Returns:
        Curve: interpolated curve on success. None on failure.
    """
    url = "rhino/geometry/curve/createinterpolatedcurve-point3darray_int_curveknotstyle"
    if multiple: url += "?multiple=true"
    args = [points, degree, knots]
    if multiple: args = zip(points, degree, knots)
    response = Util.ComputeFetch(url, args)
    return response


def CreateInterpolatedCurve2(points, degree, knots, startTangent, endTangent, multiple=False):
    """
    Interpolates a sequence of points. Used by InterpCurve Command
    This routine works best when degree=3.

    Args:
        degree (int): The degree of the curve >=1.  Degree must be odd.
        points (IEnumerable<Point3d>): Points to interpolate. For periodic curves if the final point is a
            duplicate of the initial point it is  ignored. (Count must be >=2)
        knots (CurveKnotStyle): Knot-style to use  and specifies if the curve should be periodic.
        startTangent (Vector3d): A starting tangent.
        endTangent (Vector3d): An ending tangent.

    Returns:
        Curve: interpolated curve on success. None on failure.
    """
    url = "rhino/geometry/curve/createinterpolatedcurve-point3darray_int_curveknotstyle_vector3d_vector3d"
    if multiple: url += "?multiple=true"
    args = [points, degree, knots, startTangent, endTangent]
    if multiple: args = zip(points, degree, knots, startTangent, endTangent)
    response = Util.ComputeFetch(url, args)
    return response


def CreateSoftEditCurve(curve, t, delta, length, fixEnds, multiple=False):
    """
    Creates a soft edited curve from an exising curve using a smooth field of influence.

    Args:
        curve (Curve): The curve to soft edit.
        t (double): A parameter on the curve to move from. This location on the curve is moved, and the move
            is smoothly tapered off with increasing distance along the curve from this parameter.
        delta (Vector3d): The direction and magitude, or maximum distance, of the move.
        length (double): The distance along the curve from the editing point over which the strength
            of the editing falls off smoothly.

    Returns:
        Curve: The soft edited curve if successful. None on failure.
    """
    url = "rhino/geometry/curve/createsofteditcurve-curve_double_vector3d_double_bool"
    if multiple: url += "?multiple=true"
    args = [curve, t, delta, length, fixEnds]
    if multiple: args = zip(curve, t, delta, length, fixEnds)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFilletCornersCurve(curve, radius, tolerance, angleTolerance, multiple=False):
    """
    Rounds the corners of a kinked curve with arcs of a single, specified radius.

    Args:
        curve (Curve): The curve to fillet.
        radius (double): The fillet radius.
        tolerance (double): The tolerance. When in doubt, use the document's model space absolute tolerance.
        angleTolerance (double): The angle tolerance in radians. When in doubt, use the document's model space angle tolerance.

    Returns:
        Curve: The filleted curve if successful. None on failure.
    """
    url = "rhino/geometry/curve/createfilletcornerscurve-curve_double_double_double"
    if multiple: url += "?multiple=true"
    args = [curve, radius, tolerance, angleTolerance]
    if multiple: args = zip(curve, radius, tolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateArcBlend(startPt, startDir, endPt, endDir, controlPointLengthRatio, multiple=False):
    """
    Creates a polycurve consisting of two tangent arc segments that connect two points and two directions.

    Args:
        startPt (Point3d): Start of the arc blend curve.
        startDir (Vector3d): Start direction of the arc blend curve.
        endPt (Point3d): End of the arc blend curve.
        endDir (Vector3d): End direction of the arc blend curve.
        controlPointLengthRatio (double): The ratio of the control polygon lengths of the two arcs. Note, a value of 1.0
            means the control polygon lengths for both arcs will be the same.

    Returns:
        Curve: The arc blend curve, or None on error.
    """
    url = "rhino/geometry/curve/createarcblend-point3d_vector3d_point3d_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [startPt, startDir, endPt, endDir, controlPointLengthRatio]
    if multiple: args = zip(startPt, startDir, endPt, endDir, controlPointLengthRatio)
    response = Util.ComputeFetch(url, args)
    return response


def CreateMeanCurve(curveA, curveB, angleToleranceRadians, multiple=False):
    """
    Constructs a mean, or average, curve from two curves.

    Args:
        curveA (Curve): A first curve.
        curveB (Curve): A second curve.
        angleToleranceRadians (double): The angle tolerance, in radians, used to match kinks between curves.
            If you are unsure how to set this parameter, then either use the
            document's angle tolerance RhinoDoc.AngleToleranceRadians,
            or the default value (RhinoMath.UnsetValue)

    Returns:
        Curve: The average curve, or None on error.
    """
    url = "rhino/geometry/curve/createmeancurve-curve_curve_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, angleToleranceRadians]
    if multiple: args = zip(curveA, curveB, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def CreateMeanCurve1(curveA, curveB, multiple=False):
    """
    Constructs a mean, or average, curve from two curves.

    Args:
        curveA (Curve): A first curve.
        curveB (Curve): A second curve.

    Returns:
        Curve: The average curve, or None on error.
    """
    url = "rhino/geometry/curve/createmeancurve-curve_curve"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB]
    if multiple: args = zip(curveA, curveB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendCurve(curveA, curveB, continuity, multiple=False):
    """
    Create a Blend curve between two existing curves.

    Args:
        curveA (Curve): Curve to blend from (blending will occur at curve end point).
        curveB (Curve): Curve to blend to (blending will occur at curve start point).
        continuity (BlendContinuity): Continuity of blend.

    Returns:
        Curve: A curve representing the blend between A and B or None on failure.
    """
    url = "rhino/geometry/curve/createblendcurve-curve_curve_blendcontinuity"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, continuity]
    if multiple: args = zip(curveA, curveB, continuity)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendCurve1(curveA, curveB, continuity, bulgeA, bulgeB, multiple=False):
    """
    Create a Blend curve between two existing curves.

    Args:
        curveA (Curve): Curve to blend from (blending will occur at curve end point).
        curveB (Curve): Curve to blend to (blending will occur at curve start point).
        continuity (BlendContinuity): Continuity of blend.
        bulgeA (double): Bulge factor at curveA end of blend. Values near 1.0 work best.
        bulgeB (double): Bulge factor at curveB end of blend. Values near 1.0 work best.

    Returns:
        Curve: A curve representing the blend between A and B or None on failure.
    """
    url = "rhino/geometry/curve/createblendcurve-curve_curve_blendcontinuity_double_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, continuity, bulgeA, bulgeB]
    if multiple: args = zip(curveA, curveB, continuity, bulgeA, bulgeB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendCurve2(curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1, multiple=False):
    """
    Makes a curve blend between 2 curves at the parameters specified
    with the directions and continuities specified

    Args:
        curve0 (Curve): First curve to blend from
        t0 (double): Parameter on first curve for blend endpoint
        reverse0 (bool): If false, the blend will go in the natural direction of the curve.
            If true, the blend will go in the opposite direction to the curve
        continuity0 (BlendContinuity): Continuity for the blend at the start
        curve1 (Curve): Second curve to blend from
        t1 (double): Parameter on second curve for blend endpoint
        reverse1 (bool): If false, the blend will go in the natural direction of the curve.
            If true, the blend will go in the opposite direction to the curve
        continuity1 (BlendContinuity): Continuity for the blend at the end

    Returns:
        Curve: The blend curve on success. None on failure
    """
    url = "rhino/geometry/curve/createblendcurve-curve_double_bool_blendcontinuity_curve_double_bool_blendcontinuity"
    if multiple: url += "?multiple=true"
    args = [curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1]
    if multiple: args = zip(curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurves(curve0, curve1, numCurves, multiple=False):
    """
    Creates curves between two open or closed input curves. Uses the control points of the curves for finding tween curves.
    That means the first control point of first curve is matched to first control point of the second curve and so on.
    There is no matching of curves direction. Caller must match input curves direction before calling the function.

    Args:
        curve0 (Curve): The first, or starting, curve.
        curve1 (Curve): The second, or ending, curve.
        numCurves (int): Number of tween curves to create.

    Returns:
        Curve[]: An array of joint curves. This array can be empty.
    """
    url = "rhino/geometry/curve/createtweencurves-curve_curve_int"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves]
    if multiple: args = zip(curve0, curve1, numCurves)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurves1(curve0, curve1, numCurves, tolerance, multiple=False):
    """
    Creates curves between two open or closed input curves. Uses the control points of the curves for finding tween curves.
    That means the first control point of first curve is matched to first control point of the second curve and so on.
    There is no matching of curves direction. Caller must match input curves direction before calling the function.

    Args:
        curve0 (Curve): The first, or starting, curve.
        curve1 (Curve): The second, or ending, curve.
        numCurves (int): Number of tween curves to create.

    Returns:
        Curve[]: An array of joint curves. This array can be empty.
    """
    url = "rhino/geometry/curve/createtweencurves-curve_curve_int_double"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves, tolerance]
    if multiple: args = zip(curve0, curve1, numCurves, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurvesWithMatching(curve0, curve1, numCurves, multiple=False):
    """
    Creates curves between two open or closed input curves. Make the structure of input curves compatible if needed.
    Refits the input curves to have the same structure. The resulting curves are usually more complex than input unless
    input curves are compatible and no refit is needed. There is no matching of curves direction.
    Caller must match input curves direction before calling the function.

    Args:
        curve0 (Curve): The first, or starting, curve.
        curve1 (Curve): The second, or ending, curve.
        numCurves (int): Number of tween curves to create.

    Returns:
        Curve[]: An array of joint curves. This array can be empty.
    """
    url = "rhino/geometry/curve/createtweencurveswithmatching-curve_curve_int"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves]
    if multiple: args = zip(curve0, curve1, numCurves)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurvesWithMatching1(curve0, curve1, numCurves, tolerance, multiple=False):
    """
    Creates curves between two open or closed input curves. Make the structure of input curves compatible if needed.
    Refits the input curves to have the same structure. The resulting curves are usually more complex than input unless
    input curves are compatible and no refit is needed. There is no matching of curves direction.
    Caller must match input curves direction before calling the function.

    Args:
        curve0 (Curve): The first, or starting, curve.
        curve1 (Curve): The second, or ending, curve.
        numCurves (int): Number of tween curves to create.

    Returns:
        Curve[]: An array of joint curves. This array can be empty.
    """
    url = "rhino/geometry/curve/createtweencurveswithmatching-curve_curve_int_double"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves, tolerance]
    if multiple: args = zip(curve0, curve1, numCurves, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurvesWithSampling(curve0, curve1, numCurves, numSamples, multiple=False):
    """
    Creates curves between two open or closed input curves. Use sample points method to make curves compatible.
    This is how the algorithm workd: Divides the two curves into an equal number of points, finds the midpoint between the
    corresponding points on the curves and interpolates the tween curve through those points. There is no matching of curves
    direction. Caller must match input curves direction before calling the function.

    Args:
        curve0 (Curve): The first, or starting, curve.
        curve1 (Curve): The second, or ending, curve.
        numCurves (int): Number of tween curves to create.
        numSamples (int): Number of sample points along input curves.

    Returns:
        Curve[]: >An array of joint curves. This array can be empty.
    """
    url = "rhino/geometry/curve/createtweencurveswithsampling-curve_curve_int_int"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves, numSamples]
    if multiple: args = zip(curve0, curve1, numCurves, numSamples)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurvesWithSampling1(curve0, curve1, numCurves, numSamples, tolerance, multiple=False):
    """
    Creates curves between two open or closed input curves. Use sample points method to make curves compatible.
    This is how the algorithm workd: Divides the two curves into an equal number of points, finds the midpoint between the
    corresponding points on the curves and interpolates the tween curve through those points. There is no matching of curves
    direction. Caller must match input curves direction before calling the function.

    Args:
        curve0 (Curve): The first, or starting, curve.
        curve1 (Curve): The second, or ending, curve.
        numCurves (int): Number of tween curves to create.
        numSamples (int): Number of sample points along input curves.

    Returns:
        Curve[]: >An array of joint curves. This array can be empty.
    """
    url = "rhino/geometry/curve/createtweencurveswithsampling-curve_curve_int_int_double"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves, numSamples, tolerance]
    if multiple: args = zip(curve0, curve1, numCurves, numSamples, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def JoinCurves(inputCurves, multiple=False):
    """
    Joins a collection of curve segments together.

    Args:
        inputCurves (IEnumerable<Curve>): Curve segments to join.

    Returns:
        Curve[]: An array of curves which contains.
    """
    url = "rhino/geometry/curve/joincurves-curvearray"
    if multiple: url += "?multiple=true"
    args = [inputCurves]
    if multiple: args = [[item] for item in inputCurves]
    response = Util.ComputeFetch(url, args)
    return response


def JoinCurves1(inputCurves, joinTolerance, multiple=False):
    """
    Joins a collection of curve segments together.

    Args:
        inputCurves (IEnumerable<Curve>): An array, a list or any enumerable set of curve segments to join.
        joinTolerance (double): Joining tolerance,
            i.e. the distance between segment end-points that is allowed.

    Returns:
        Curve[]: An array of joint curves. This array can be empty.
    """
    url = "rhino/geometry/curve/joincurves-curvearray_double"
    if multiple: url += "?multiple=true"
    args = [inputCurves, joinTolerance]
    if multiple: args = zip(inputCurves, joinTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def JoinCurves2(inputCurves, joinTolerance, preserveDirection, multiple=False):
    """
    Joins a collection of curve segments together.

    Args:
        inputCurves (IEnumerable<Curve>): An array, a list or any enumerable set of curve segments to join.
        joinTolerance (double): Joining tolerance,
            i.e. the distance between segment end-points that is allowed.
        preserveDirection (bool): If true, curve endpoints will be compared to curve startpoints.If false, all start and endpoints will be compared and copies of input curves may be reversed in output.

    Returns:
        Curve[]: An array of joint curves. This array can be empty.
    """
    url = "rhino/geometry/curve/joincurves-curvearray_double_bool"
    if multiple: url += "?multiple=true"
    args = [inputCurves, joinTolerance, preserveDirection]
    if multiple: args = zip(inputCurves, joinTolerance, preserveDirection)
    response = Util.ComputeFetch(url, args)
    return response


def MakeEndsMeet(curveA, adjustStartCurveA, curveB, adjustStartCurveB, multiple=False):
    """
    Makes adjustments to the ends of one or both input curves so that they meet at a point.

    Args:
        curveA (Curve): 1st curve to adjust.
        adjustStartCurveA (bool): Which end of the 1st curve to adjust: True is start, False is end.
        curveB (Curve): 2nd curve to adjust.
        adjustStartCurveB (bool): which end of the 2nd curve to adjust true==start, false==end.

    Returns:
        bool: True on success.
    """
    url = "rhino/geometry/curve/makeendsmeet-curve_bool_curve_bool"
    if multiple: url += "?multiple=true"
    args = [curveA, adjustStartCurveA, curveB, adjustStartCurveB]
    if multiple: args = zip(curveA, adjustStartCurveA, curveB, adjustStartCurveB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFillet(curve0, curve1, radius, t0Base, t1Base, multiple=False):
    """
    Computes the fillet arc for a curve filleting operation.

    Args:
        curve0 (Curve): First curve to fillet.
        curve1 (Curve): Second curve to fillet.
        radius (double): Fillet radius.
        t0Base (double): Parameter on curve0 where the fillet ought to start (approximately).
        t1Base (double): Parameter on curve1 where the fillet ought to end (approximately).

    Returns:
        Arc: The fillet arc on success, or Arc.Unset on failure.
    """
    url = "rhino/geometry/curve/createfillet-curve_curve_double_double_double"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, radius, t0Base, t1Base]
    if multiple: args = zip(curve0, curve1, radius, t0Base, t1Base)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFilletCurves(curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance, multiple=False):
    """
    Creates a tangent arc between two curves and trims or extends the curves to the arc.

    Args:
        curve0 (Curve): The first curve to fillet.
        point0 (Point3d): A point on the first curve that is near the end where the fillet will
            be created.
        curve1 (Curve): The second curve to fillet.
        point1 (Point3d): A point on the second curve that is near the end where the fillet will
            be created.
        radius (double): The radius of the fillet.
        join (bool): Join the output curves.
        trim (bool): Trim copies of the input curves to the output fillet curve.
        arcExtension (bool): Applies when arcs are filleted but need to be extended to meet the
            fillet curve or chamfer line. If true, then the arc is extended
            maintaining its validity. If false, then the arc is extended with a
            line segment, which is joined to the arc converting it to a polycurve.
        tolerance (double): The tolerance, generally the document's absolute tolerance.

    Returns:
        Curve[]: The results of the fillet operation. The number of output curves depends
        on the input curves and the values of the parameters that were used
        during the fillet operation. In most cases, the output array will contain
        either one or three curves, although two curves can be returned if the
        radius is zero and join = false.
        For example, if both join and trim = true, then the output curve
        will be a polycurve containing the fillet curve joined with trimmed copies
        of the input curves. If join = False and trim = true, then three curves,
        the fillet curve and trimmed copies of the input curves, will be returned.
        If both join and trim = false, then just the fillet curve is returned.
    """
    url = "rhino/geometry/curve/createfilletcurves-curve_point3d_curve_point3d_double_bool_bool_bool_double_double"
    if multiple: url += "?multiple=true"
    args = [curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance]
    if multiple: args = zip(curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion(curves, multiple=False):
    """
    Calculates the boolean union of two or more closed, planar curves.
    Note, curves must be co-planar.

    Args:
        curves (IEnumerable<Curve>): The co-planar curves to union.

    Returns:
        Curve[]: Result curves on success, empty array if no union could be calculated.
    """
    url = "rhino/geometry/curve/createbooleanunion-curvearray"
    if multiple: url += "?multiple=true"
    args = [curves]
    if multiple: args = [[item] for item in curves]
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion1(curves, tolerance, multiple=False):
    """
    Calculates the boolean union of two or more closed, planar curves.
    Note, curves must be co-planar.

    Args:
        curves (IEnumerable<Curve>): The co-planar curves to union.

    Returns:
        Curve[]: Result curves on success, empty array if no union could be calculated.
    """
    url = "rhino/geometry/curve/createbooleanunion-curvearray_double"
    if multiple: url += "?multiple=true"
    args = [curves, tolerance]
    if multiple: args = zip(curves, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection(curveA, curveB, multiple=False):
    """
    Calculates the boolean intersection of two closed, planar curves.
    Note, curves must be co-planar.

    Args:
        curveA (Curve): The first closed, planar curve.
        curveB (Curve): The second closed, planar curve.

    Returns:
        Curve[]: Result curves on success, empty array if no intersection could be calculated.
    """
    url = "rhino/geometry/curve/createbooleanintersection-curve_curve"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB]
    if multiple: args = zip(curveA, curveB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection1(curveA, curveB, tolerance, multiple=False):
    """
    Calculates the boolean intersection of two closed, planar curves.
    Note, curves must be co-planar.

    Args:
        curveA (Curve): The first closed, planar curve.
        curveB (Curve): The second closed, planar curve.

    Returns:
        Curve[]: Result curves on success, empty array if no intersection could be calculated.
    """
    url = "rhino/geometry/curve/createbooleanintersection-curve_curve_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, tolerance]
    if multiple: args = zip(curveA, curveB, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference(curveA, curveB, multiple=False):
    """
    Calculates the boolean difference between two closed, planar curves.
    Note, curves must be co-planar.

    Args:
        curveA (Curve): The first closed, planar curve.
        curveB (Curve): The second closed, planar curve.

    Returns:
        Curve[]: Result curves on success, empty array if no difference could be calculated.
    """
    url = "rhino/geometry/curve/createbooleandifference-curve_curve"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB]
    if multiple: args = zip(curveA, curveB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference1(curveA, curveB, tolerance, multiple=False):
    """
    Calculates the boolean difference between two closed, planar curves.
    Note, curves must be co-planar.

    Args:
        curveA (Curve): The first closed, planar curve.
        curveB (Curve): The second closed, planar curve.

    Returns:
        Curve[]: Result curves on success, empty array if no difference could be calculated.
    """
    url = "rhino/geometry/curve/createbooleandifference-curve_curve_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, tolerance]
    if multiple: args = zip(curveA, curveB, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference2(curveA, subtractors, multiple=False):
    """
    Calculates the boolean difference between a closed planar curve, and a list of closed planar curves.
    Note, curves must be co-planar.

    Args:
        curveA (Curve): The first closed, planar curve.
        subtractors (IEnumerable<Curve>): curves to subtract from the first closed curve.

    Returns:
        Curve[]: Result curves on success, empty array if no difference could be calculated.
    """
    url = "rhino/geometry/curve/createbooleandifference-curve_curvearray"
    if multiple: url += "?multiple=true"
    args = [curveA, subtractors]
    if multiple: args = zip(curveA, subtractors)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference3(curveA, subtractors, tolerance, multiple=False):
    """
    Calculates the boolean difference between a closed planar curve, and a list of closed planar curves.
    Note, curves must be co-planar.

    Args:
        curveA (Curve): The first closed, planar curve.
        subtractors (IEnumerable<Curve>): curves to subtract from the first closed curve.

    Returns:
        Curve[]: Result curves on success, empty array if no difference could be calculated.
    """
    url = "rhino/geometry/curve/createbooleandifference-curve_curvearray_double"
    if multiple: url += "?multiple=true"
    args = [curveA, subtractors, tolerance]
    if multiple: args = zip(curveA, subtractors, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTextOutlines(text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance, multiple=False):
    """
    Creates outline curves created from a text string. The functionality is similar to what you find in Rhino's TextObject command or TextEntity.Explode() in RhinoCommon.

    Args:
        text (string): The text from which to create outline curves.
        font (string): The text font.
        textHeight (double): The text height.
        textStyle (int): The font style. The font style can be any number of the following: 0 - Normal, 1 - Bold, 2 - Italic
        closeLoops (bool): Set this value to True when dealing with normal fonts and when you expect closed loops. You may want to set this to False when specifying a single-stroke font where you don't want closed loops.
        plane (Plane): The plane on which the outline curves will lie.
        smallCapsScale (double): Displays lower-case letters as small caps. Set the relative text size to a percentage of the normal text.
        tolerance (double): The tolerance for the operation.

    Returns:
        Curve[]: An array containing one or more curves if successful.
    """
    url = "rhino/geometry/curve/createtextoutlines-string_string_double_int_bool_plane_double_double"
    if multiple: url += "?multiple=true"
    args = [text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance]
    if multiple: args = zip(text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateCurve2View(curveA, curveB, vectorA, vectorB, tolerance, angleTolerance, multiple=False):
    """
    Creates a third curve from two curves that are planar in different construction planes.
    The new curve looks the same as each of the original curves when viewed in each plane.

    Args:
        curveA (Curve): The first curve.
        curveB (Curve): The second curve.
        vectorA (Vector3d): A vector defining the normal direction of the plane which the first curve is drawn upon.
        vectorB (Vector3d): A vector defining the normal direction of the plane which the seconf curve is drawn upon.
        tolerance (double): The tolerance for the operation.
        angleTolerance (double): The angle tolerance for the operation.

    Returns:
        Curve[]: An array containing one or more curves if successful.
    """
    url = "rhino/geometry/curve/createcurve2view-curve_curve_vector3d_vector3d_double_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, vectorA, vectorB, tolerance, angleTolerance]
    if multiple: args = zip(curveA, curveB, vectorA, vectorB, tolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def DoDirectionsMatch(curveA, curveB, multiple=False):
    """
    Determines whether two curves travel more or less in the same direction.

    Args:
        curveA (Curve): First curve to test.
        curveB (Curve): Second curve to test.

    Returns:
        bool: True if both curves more or less point in the same direction,
        False if they point in the opposite directions.
    """
    url = "rhino/geometry/curve/dodirectionsmatch-curve_curve"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB]
    if multiple: args = zip(curveA, curveB)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToMesh(curve, mesh, direction, tolerance, multiple=False):
    """
    Projects a curve to a mesh using a direction and tolerance.

    Args:
        curve (Curve): A curve.
        mesh (Mesh): A mesh.
        direction (Vector3d): A direction vector.
        tolerance (double): A tolerance value.

    Returns:
        Curve[]: A curve array.
    """
    url = "rhino/geometry/curve/projecttomesh-curve_mesh_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curve, mesh, direction, tolerance]
    if multiple: args = zip(curve, mesh, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToMesh1(curve, meshes, direction, tolerance, multiple=False):
    """
    Projects a curve to a set of meshes using a direction and tolerance.

    Args:
        curve (Curve): A curve.
        meshes (IEnumerable<Mesh>): A list, an array or any enumerable of meshes.
        direction (Vector3d): A direction vector.
        tolerance (double): A tolerance value.

    Returns:
        Curve[]: A curve array.
    """
    url = "rhino/geometry/curve/projecttomesh-curve_mesharray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curve, meshes, direction, tolerance]
    if multiple: args = zip(curve, meshes, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToMesh2(curves, meshes, direction, tolerance, multiple=False):
    """
    Projects a curve to a set of meshes using a direction and tolerance.

    Args:
        curves (IEnumerable<Curve>): A list, an array or any enumerable of curves.
        meshes (IEnumerable<Mesh>): A list, an array or any enumerable of meshes.
        direction (Vector3d): A direction vector.
        tolerance (double): A tolerance value.

    Returns:
        Curve[]: A curve array.
    """
    url = "rhino/geometry/curve/projecttomesh-curvearray_mesharray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curves, meshes, direction, tolerance]
    if multiple: args = zip(curves, meshes, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToBrep(curve, brep, direction, tolerance, multiple=False):
    """
    Projects a Curve onto a Brep along a given direction.

    Args:
        curve (Curve): Curve to project.
        brep (Brep): Brep to project onto.
        direction (Vector3d): Direction of projection.
        tolerance (double): Tolerance to use for projection.

    Returns:
        Curve[]: An array of projected curves or empty array if the projection set is empty.
    """
    url = "rhino/geometry/curve/projecttobrep-curve_brep_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curve, brep, direction, tolerance]
    if multiple: args = zip(curve, brep, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToBrep1(curve, breps, direction, tolerance, multiple=False):
    """
    Projects a Curve onto a collection of Breps along a given direction.

    Args:
        curve (Curve): Curve to project.
        breps (IEnumerable<Brep>): Breps to project onto.
        direction (Vector3d): Direction of projection.
        tolerance (double): Tolerance to use for projection.

    Returns:
        Curve[]: An array of projected curves or empty array if the projection set is empty.
    """
    url = "rhino/geometry/curve/projecttobrep-curve_breparray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curve, breps, direction, tolerance]
    if multiple: args = zip(curve, breps, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToBrep2(curve, breps, direction, tolerance, multiple=False):
    """
    Projects a Curve onto a collection of Breps along a given direction.

    Args:
        curve (Curve): Curve to project.
        breps (IEnumerable<Brep>): Breps to project onto.
        direction (Vector3d): Direction of projection.
        tolerance (double): Tolerance to use for projection.

    Returns:
        Curve[]: An array of projected curves or None if the projection set is empty.
        brepIndices (int[]): (out) Integers that identify for each resulting curve which Brep it was projected onto.
    """
    url = "rhino/geometry/curve/projecttobrep-curve_breparray_vector3d_double_intarray"
    if multiple: url += "?multiple=true"
    args = [curve, breps, direction, tolerance]
    if multiple: args = zip(curve, breps, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToBrep3(curves, breps, direction, tolerance, multiple=False):
    """
    Projects a collection of Curves onto a collection of Breps along a given direction.

    Args:
        curves (IEnumerable<Curve>): Curves to project.
        breps (IEnumerable<Brep>): Breps to project onto.
        direction (Vector3d): Direction of projection.
        tolerance (double): Tolerance to use for projection.

    Returns:
        Curve[]: An array of projected curves or empty array if the projection set is empty.
    """
    url = "rhino/geometry/curve/projecttobrep-curvearray_breparray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curves, breps, direction, tolerance]
    if multiple: args = zip(curves, breps, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToPlane(curve, plane, multiple=False):
    """
    Constructs a curve by projecting an existing curve to a plane.

    Args:
        curve (Curve): A curve.
        plane (Plane): A plane.

    Returns:
        Curve: The projected curve on success; None on failure.
    """
    url = "rhino/geometry/curve/projecttoplane-curve_plane"
    if multiple: url += "?multiple=true"
    args = [curve, plane]
    if multiple: args = zip(curve, plane)
    response = Util.ComputeFetch(url, args)
    return response


def PullToBrepFace(curve, face, tolerance, multiple=False):
    """
    Pull a curve to a BrepFace using closest point projection.

    Args:
        curve (Curve): Curve to pull.
        face (BrepFace): Brepface that pulls.
        tolerance (double): Tolerance to use for pulling.

    Returns:
        Curve[]: An array of pulled curves, or an empty array on failure.
    """
    url = "rhino/geometry/curve/pulltobrepface-curve_brepface_double"
    if multiple: url += "?multiple=true"
    args = [curve, face, tolerance]
    if multiple: args = zip(curve, face, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def PlanarClosedCurveRelationship(curveA, curveB, testPlane, tolerance, multiple=False):
    """
    Determines whether two coplanar simple closed curves are disjoint or intersect;
    otherwise, if the regions have a containment relationship, discovers
    which curve encloses the other.

    Args:
        curveA (Curve): A first curve.
        curveB (Curve): A second curve.
        testPlane (Plane): A plane.
        tolerance (double): A tolerance value.

    Returns:
        RegionContainment: A value indicating the relationship between the first and the second curve.
    """
    url = "rhino/geometry/curve/planarclosedcurverelationship-curve_curve_plane_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, testPlane, tolerance]
    if multiple: args = zip(curveA, curveB, testPlane, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def PlanarCurveCollision(curveA, curveB, testPlane, tolerance, multiple=False):
    """
    Determines if two coplanar curves collide (intersect).

    Args:
        curveA (Curve): A curve.
        curveB (Curve): Another curve.
        testPlane (Plane): A valid plane containing the curves.
        tolerance (double): A tolerance value for intersection.

    Returns:
        bool: True if the curves intersect, otherwise false
    """
    url = "rhino/geometry/curve/planarcurvecollision-curve_curve_plane_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, testPlane, tolerance]
    if multiple: args = zip(curveA, curveB, testPlane, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def DuplicateSegments(thisCurve, multiple=False):
    """
    Polylines will be exploded into line segments. ExplodeCurves will
    return the curves in topological order.

    Returns:
        Curve[]: An array of all the segments that make up this curve.
    """
    url = "rhino/geometry/curve/duplicatesegments-curve"
    if multiple: url += "?multiple=true"
    args = [thisCurve]
    if multiple: args = [[item] for item in thisCurve]
    response = Util.ComputeFetch(url, args)
    return response


def Smooth(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, multiple=False):
    """
    Smooths a curve by averaging the positions of control points in a specified region.

    Args:
        smoothFactor (double): The smoothing factor, which controls how much control points move towards the average of the neighboring control points.
        bXSmooth (bool): When True control points move in X axis direction.
        bYSmooth (bool): When True control points move in Y axis direction.
        bZSmooth (bool): When True control points move in Z axis direction.
        bFixBoundaries (bool): When True the curve ends don't move.
        coordinateSystem (SmoothingCoordinateSystem): The coordinates to determine the direction of the smoothing.

    Returns:
        Curve: The smoothed curve if successful, None otherwise.
    """
    url = "rhino/geometry/curve/smooth-curve_double_bool_bool_bool_bool_smoothingcoordinatesystem"
    if multiple: url += "?multiple=true"
    args = [thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem]
    if multiple: args = zip(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem)
    response = Util.ComputeFetch(url, args)
    return response


def Smooth1(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False):
    """
    Smooths a curve by averaging the positions of control points in a specified region.

    Args:
        smoothFactor (double): The smoothing factor, which controls how much control points move towards the average of the neighboring control points.
        bXSmooth (bool): When True control points move in X axis direction.
        bYSmooth (bool): When True control points move in Y axis direction.
        bZSmooth (bool): When True control points move in Z axis direction.
        bFixBoundaries (bool): When True the curve ends don't move.
        coordinateSystem (SmoothingCoordinateSystem): The coordinates to determine the direction of the smoothing.
        plane (Plane): If SmoothingCoordinateSystem.CPlane specified, then the construction plane.

    Returns:
        Curve: The smoothed curve if successful, None otherwise.
    """
    url = "rhino/geometry/curve/smooth-curve_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane"
    if multiple: url += "?multiple=true"
    args = [thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    if multiple: args = zip(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane)
    response = Util.ComputeFetch(url, args)
    return response


def MakeClosed(thisCurve, tolerance, multiple=False):
    """
    If IsClosed, just return true. Otherwise, decide if curve can be closed as
    follows: Linear curves polylinear curves with 2 segments, Nurbs with 3 or less
    control points cannot be made closed. Also, if tolerance > 0 and the gap between
    start and end is larger than tolerance, curve cannot be made closed.
    Adjust the curve's endpoint to match its start point.

    Args:
        tolerance (double): If nonzero, and the gap is more than tolerance, curve cannot be made closed.

    Returns:
        bool: True on success, False on failure.
    """
    url = "rhino/geometry/curve/makeclosed-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance]
    if multiple: args = zip(thisCurve, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def LcoalClosestPoint(thisCurve, testPoint, seed, multiple=False):
    """
    Find parameter of the point on a curve that is locally closest to
    the testPoint.  The search for a local close point starts at
    a seed parameter.

    Args:
        testPoint (Point3d): A point to test against.
        seed (double): The seed parameter.

    Returns:
        bool: True if the search is successful, False if the search fails.
        t (double): >Parameter of the curve that is closest to testPoint.
    """
    url = "rhino/geometry/curve/lcoalclosestpoint-curve_point3d_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, seed]
    if multiple: args = zip(thisCurve, testPoint, seed)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint(thisCurve, testPoint, multiple=False):
    """
    Finds parameter of the point on a curve that is closest to testPoint.
    If the maximumDistance parameter is > 0, then only points whose distance
    to the given point is <= maximumDistance will be returned.  Using a
    positive value of maximumDistance can substantially speed up the search.

    Args:
        testPoint (Point3d): Point to search from.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter of local closest point.
    """
    url = "rhino/geometry/curve/closestpoint-curve_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint]
    if multiple: args = zip(thisCurve, testPoint)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint1(thisCurve, testPoint, maximumDistance, multiple=False):
    """
    Finds the parameter of the point on a curve that is closest to testPoint.
    If the maximumDistance parameter is > 0, then only points whose distance
    to the given point is <= maximumDistance will be returned.  Using a
    positive value of maximumDistance can substantially speed up the search.

    Args:
        testPoint (Point3d): Point to project.
        maximumDistance (double): The maximum allowed distance.
            Past this distance, the search is given up and False is returned.Use 0 to turn off this parameter.

    Returns:
        bool: True on success, False on failure.
        t (double): parameter of local closest point returned here.
    """
    url = "rhino/geometry/curve/closestpoint-curve_point3d_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, maximumDistance]
    if multiple: args = zip(thisCurve, testPoint, maximumDistance)
    response = Util.ComputeFetch(url, args)
    return response


def Contains(thisCurve, testPoint, multiple=False):
    """
    Computes the relationship between a point and a closed curve region.
    This curve must be closed or the return value will be Unset.
    Both curve and point are projected to the World XY plane.

    Args:
        testPoint (Point3d): Point to test.

    Returns:
        PointContainment: Relationship between point and curve region.
    """
    url = "rhino/geometry/curve/contains-curve_point3d"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint]
    if multiple: args = zip(thisCurve, testPoint)
    response = Util.ComputeFetch(url, args)
    return response


def Contains1(thisCurve, testPoint, plane, multiple=False):
    """
    Computes the relationship between a point and a closed curve region.
    This curve must be closed or the return value will be Unset.

    Args:
        testPoint (Point3d): Point to test.
        plane (Plane): Plane in in which to compare point and region.

    Returns:
        PointContainment: Relationship between point and curve region.
    """
    url = "rhino/geometry/curve/contains-curve_point3d_plane"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, plane]
    if multiple: args = zip(thisCurve, testPoint, plane)
    response = Util.ComputeFetch(url, args)
    return response


def Contains2(thisCurve, testPoint, plane, tolerance, multiple=False):
    """
    Computes the relationship between a point and a closed curve region.
    This curve must be closed or the return value will be Unset.

    Args:
        testPoint (Point3d): Point to test.
        plane (Plane): Plane in in which to compare point and region.
        tolerance (double): Tolerance to use during comparison.

    Returns:
        PointContainment: Relationship between point and curve region.
    """
    url = "rhino/geometry/curve/contains-curve_point3d_plane_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, plane, tolerance]
    if multiple: args = zip(thisCurve, testPoint, plane, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ExtremeParameters(thisCurve, direction, multiple=False):
    """
    Returns the parameter values of all local extrema.
    Parameter values are in increasing order so consecutive extrema
    define an interval on which each component of the curve is monotone.
    Note, non-periodic curves always return the end points.

    Args:
        direction (Vector3d): The direction in which to perform the calculation.

    Returns:
        double[]: The parameter values of all local extrema.
    """
    url = "rhino/geometry/curve/extremeparameters-curve_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisCurve, direction]
    if multiple: args = zip(thisCurve, direction)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePeriodicCurve(curve, multiple=False):
    """
    Removes kinks from a curve. Periodic curves deform smoothly without kinks.

    Args:
        curve (Curve): The curve to make periodic. Curve must have degree >= 2.

    Returns:
        Curve: The resulting curve if successful, None otherwise.
    """
    url = "rhino/geometry/curve/createperiodiccurve-curve"
    if multiple: url += "?multiple=true"
    args = [curve]
    if multiple: args = [[item] for item in curve]
    response = Util.ComputeFetch(url, args)
    return response


def CreatePeriodicCurve1(curve, smooth, multiple=False):
    """
    Removes kinks from a curve. Periodic curves deform smoothly without kinks.

    Args:
        curve (Curve): The curve to make periodic. Curve must have degree >= 2.
        smooth (bool): If true, smooths any kinks in the curve and moves control points to make a smooth curve.
            If false, control point locations are not changed or changed minimally (only one point may move) and only the knot vector is altered.

    Returns:
        Curve: The resulting curve if successful, None otherwise.
    """
    url = "rhino/geometry/curve/createperiodiccurve-curve_bool"
    if multiple: url += "?multiple=true"
    args = [curve, smooth]
    if multiple: args = zip(curve, smooth)
    response = Util.ComputeFetch(url, args)
    return response


def PointAtLength(thisCurve, length, multiple=False):
    """
    Gets a point at a certain length along the curve. The length must be
    non-negative and less than or equal to the length of the curve.
    Lengths will not be wrapped when the curve is closed or periodic.

    Args:
        length (double): Length along the curve between the start point and the returned point.

    Returns:
        Point3d: Point on the curve at the specified length from the start point or Poin3d.Unset on failure.
    """
    url = "rhino/geometry/curve/pointatlength-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, length]
    if multiple: args = zip(thisCurve, length)
    response = Util.ComputeFetch(url, args)
    return response


def PointAtNormalizedLength(thisCurve, length, multiple=False):
    """
    Gets a point at a certain normalized length along the curve. The length must be
    between or including 0.0 and 1.0, where 0.0 equals the start of the curve and
    1.0 equals the end of the curve.

    Args:
        length (double): Normalized length along the curve between the start point and the returned point.

    Returns:
        Point3d: Point on the curve at the specified normalized length from the start point or Poin3d.Unset on failure.
    """
    url = "rhino/geometry/curve/pointatnormalizedlength-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, length]
    if multiple: args = zip(thisCurve, length)
    response = Util.ComputeFetch(url, args)
    return response


def PerpendicularFrameAt(thisCurve, t, multiple=False):
    """
    Return a 3d frame at a parameter. This is slightly different than FrameAt in
    that the frame is computed in a way so there is minimal rotation from one
    frame to the next.

    Args:
        t (double): Evaluation parameter.

    Returns:
        bool: True on success, False on failure.
        plane (Plane): The frame is returned here.
    """
    url = "rhino/geometry/curve/perpendicularframeat-curve_double_plane"
    if multiple: url += "?multiple=true"
    args = [thisCurve, t]
    if multiple: args = zip(thisCurve, t)
    response = Util.ComputeFetch(url, args)
    return response


def GetPerpendicularFrames(thisCurve, parameters, multiple=False):
    """
    Gets a collection of perpendicular frames along the curve. Perpendicular frames
    are also known as 'Zero-twisting frames' and they minimize rotation from one frame to the next.

    Args:
        parameters (IEnumerable<double>): A collection of strictly increasing curve parameters to place perpendicular frames on.

    Returns:
        Plane[]: An array of perpendicular frames on success or None on failure.
    """
    url = "rhino/geometry/curve/getperpendicularframes-curve_doublearray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, parameters]
    if multiple: args = zip(thisCurve, parameters)
    response = Util.ComputeFetch(url, args)
    return response


def GetLength(thisCurve, multiple=False):
    """
    Gets the length of the curve with a fractional tolerance of 1.0e-8.

    Returns:
        double: The length of the curve on success, or zero on failure.
    """
    url = "rhino/geometry/curve/getlength-curve"
    if multiple: url += "?multiple=true"
    args = [thisCurve]
    if multiple: args = [[item] for item in thisCurve]
    response = Util.ComputeFetch(url, args)
    return response


def GetLength1(thisCurve, fractionalTolerance, multiple=False):
    """
    Get the length of the curve.

    Args:
        fractionalTolerance (double): Desired fractional precision.
            fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.

    Returns:
        double: The length of the curve on success, or zero on failure.
    """
    url = "rhino/geometry/curve/getlength-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, fractionalTolerance]
    if multiple: args = zip(thisCurve, fractionalTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def GetLength2(thisCurve, subdomain, multiple=False):
    """
    Get the length of a sub-section of the curve with a fractional tolerance of 1e-8.

    Args:
        subdomain (Interval): The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).

    Returns:
        double: The length of the sub-curve on success, or zero on failure.
    """
    url = "rhino/geometry/curve/getlength-curve_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, subdomain]
    if multiple: args = zip(thisCurve, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def GetLength3(thisCurve, fractionalTolerance, subdomain, multiple=False):
    """
    Get the length of a sub-section of the curve.

    Args:
        fractionalTolerance (double): Desired fractional precision.
            fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
        subdomain (Interval): The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).

    Returns:
        double: The length of the sub-curve on success, or zero on failure.
    """
    url = "rhino/geometry/curve/getlength-curve_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, fractionalTolerance, subdomain]
    if multiple: args = zip(thisCurve, fractionalTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def IsShort(thisCurve, tolerance, multiple=False):
    """
    Used to quickly find short curves.

    Args:
        tolerance (double): Length threshold value for "shortness".

    Returns:
        bool: True if the length of the curve is <= tolerance.
    """
    url = "rhino/geometry/curve/isshort-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance]
    if multiple: args = zip(thisCurve, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def IsShort1(thisCurve, tolerance, subdomain, multiple=False):
    """
    Used to quickly find short curves.

    Args:
        tolerance (double): Length threshold value for "shortness".
        subdomain (Interval): The test is performed on the interval that is the intersection of subdomain with Domain()

    Returns:
        bool: True if the length of the curve is <= tolerance.
    """
    url = "rhino/geometry/curve/isshort-curve_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance, subdomain]
    if multiple: args = zip(thisCurve, tolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def RemoveShortSegments(thisCurve, tolerance, multiple=False):
    """
    Looks for segments that are shorter than tolerance that can be removed.
    Does not change the domain, but it will change the relative parameterization.

    Args:
        tolerance (double): Tolerance which defines "short" segments.

    Returns:
        bool: True if removable short segments were found.
        False if no removable short segments were found.
    """
    url = "rhino/geometry/curve/removeshortsegments-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance]
    if multiple: args = zip(thisCurve, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def LengthParameter(thisCurve, segmentLength, multiple=False):
    """
    Gets the parameter along the curve which coincides with a given length along the curve.
    A fractional tolerance of 1e-8 is used in this version of the function.

    Args:
        segmentLength (double): Length of segment to measure. Must be less than or equal to the length of the curve.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter such that the length of the curve from the curve start point to t equals length.
    """
    url = "rhino/geometry/curve/lengthparameter-curve_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength]
    if multiple: args = zip(thisCurve, segmentLength)
    response = Util.ComputeFetch(url, args)
    return response


def LengthParameter1(thisCurve, segmentLength, fractionalTolerance, multiple=False):
    """
    Gets the parameter along the curve which coincides with a given length along the curve.

    Args:
        segmentLength (double): Length of segment to measure. Must be less than or equal to the length of the curve.
        fractionalTolerance (double): Desired fractional precision.
            fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter such that the length of the curve from the curve start point to t equals s.
    """
    url = "rhino/geometry/curve/lengthparameter-curve_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, fractionalTolerance]
    if multiple: args = zip(thisCurve, segmentLength, fractionalTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def LengthParameter2(thisCurve, segmentLength, subdomain, multiple=False):
    """
    Gets the parameter along the curve which coincides with a given length along the curve.
    A fractional tolerance of 1e-8 is used in this version of the function.

    Args:
        segmentLength (double): Length of segment to measure. Must be less than or equal to the length of the subdomain.
        subdomain (Interval): The calculation is performed on the specified sub-domain of the curve rather than the whole curve.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter such that the length of the curve from the start of the subdomain to t is s.
    """
    url = "rhino/geometry/curve/lengthparameter-curve_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, subdomain]
    if multiple: args = zip(thisCurve, segmentLength, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def LengthParameter3(thisCurve, segmentLength, fractionalTolerance, subdomain, multiple=False):
    """
    Gets the parameter along the curve which coincides with a given length along the curve.

    Args:
        segmentLength (double): Length of segment to measure. Must be less than or equal to the length of the subdomain.
        fractionalTolerance (double): Desired fractional precision.
            fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
        subdomain (Interval): The calculation is performed on the specified sub-domain of the curve rather than the whole curve.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter such that the length of the curve from the start of the subdomain to t is s.
    """
    url = "rhino/geometry/curve/lengthparameter-curve_double_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, fractionalTolerance, subdomain]
    if multiple: args = zip(thisCurve, segmentLength, fractionalTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameter(thisCurve, s, multiple=False):
    """
    Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
    A fractional tolerance of 1e-8 is used in this version of the function.

    Args:
        s (double): Normalized arc length parameter.
            E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter such that the length of the curve from its start to t is arc_length.
    """
    url = "rhino/geometry/curve/normalizedlengthparameter-curve_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s]
    if multiple: args = zip(thisCurve, s)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameter1(thisCurve, s, fractionalTolerance, multiple=False):
    """
    Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.

    Args:
        s (double): Normalized arc length parameter.
            E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        fractionalTolerance (double): Desired fractional precision.
            fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter such that the length of the curve from its start to t is arc_length.
    """
    url = "rhino/geometry/curve/normalizedlengthparameter-curve_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, fractionalTolerance]
    if multiple: args = zip(thisCurve, s, fractionalTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameter2(thisCurve, s, subdomain, multiple=False):
    """
    Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
    A fractional tolerance of 1e-8 is used in this version of the function.

    Args:
        s (double): Normalized arc length parameter.
            E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        subdomain (Interval): The calculation is performed on the specified sub-domain of the curve.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter such that the length of the curve from its start to t is arc_length.
    """
    url = "rhino/geometry/curve/normalizedlengthparameter-curve_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, subdomain]
    if multiple: args = zip(thisCurve, s, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameter3(thisCurve, s, fractionalTolerance, subdomain, multiple=False):
    """
    Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.

    Args:
        s (double): Normalized arc length parameter.
            E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        fractionalTolerance (double): Desired fractional precision.
            fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
        subdomain (Interval): The calculation is performed on the specified sub-domain of the curve.

    Returns:
        bool: True on success, False on failure.
        t (double): Parameter such that the length of the curve from its start to t is arc_length.
    """
    url = "rhino/geometry/curve/normalizedlengthparameter-curve_double_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, fractionalTolerance, subdomain]
    if multiple: args = zip(thisCurve, s, fractionalTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameters(thisCurve, s, absoluteTolerance, multiple=False):
    """
    Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
    A fractional tolerance of 1e-8 is used in this version of the function.

    Args:
        s (double[]): Array of normalized arc length parameters.
            E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        absoluteTolerance (double): If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length
            and the length of the curve segment from t[i] to t[i+1] will be <= absoluteTolerance.

    Returns:
        double[]: If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length.
        Null on failure.
    """
    url = "rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, absoluteTolerance]
    if multiple: args = zip(thisCurve, s, absoluteTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameters1(thisCurve, s, absoluteTolerance, fractionalTolerance, multiple=False):
    """
    Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.

    Args:
        s (double[]): Array of normalized arc length parameters.
            E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        absoluteTolerance (double): If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length
            and the length of the curve segment from t[i] to t[i+1] will be <= absoluteTolerance.
        fractionalTolerance (double): Desired fractional precision for each segment.
            fabs("true" length - actual length)/(actual length) <= fractionalTolerance.

    Returns:
        double[]: If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length.
        Null on failure.
    """
    url = "rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, absoluteTolerance, fractionalTolerance]
    if multiple: args = zip(thisCurve, s, absoluteTolerance, fractionalTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameters2(thisCurve, s, absoluteTolerance, subdomain, multiple=False):
    """
    Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
    A fractional tolerance of 1e-8 is used in this version of the function.

    Args:
        s (double[]): Array of normalized arc length parameters.
            E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        absoluteTolerance (double): If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length
            and the length of the curve segment from t[i] to t[i+1] will be <= absoluteTolerance.
        subdomain (Interval): The calculation is performed on the specified sub-domain of the curve.
            A 0.0 s value corresponds to subdomain->Min() and a 1.0 s value corresponds to subdomain->Max().

    Returns:
        double[]: If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length.
        Null on failure.
    """
    url = "rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, absoluteTolerance, subdomain]
    if multiple: args = zip(thisCurve, s, absoluteTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameters3(thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain, multiple=False):
    """
    Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.

    Args:
        s (double[]): Array of normalized arc length parameters.
            E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
        absoluteTolerance (double): If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length
            and the length of the curve segment from t[i] to t[i+1] will be <= absoluteTolerance.
        fractionalTolerance (double): Desired fractional precision for each segment.
            fabs("true" length - actual length)/(actual length) <= fractionalTolerance.
        subdomain (Interval): The calculation is performed on the specified sub-domain of the curve.
            A 0.0 s value corresponds to subdomain->Min() and a 1.0 s value corresponds to subdomain->Max().

    Returns:
        double[]: If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length.
        Null on failure.
    """
    url = "rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain]
    if multiple: args = zip(thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByCount(thisCurve, segmentCount, includeEnds, multiple=False):
    """
    Divide the curve into a number of equal-length segments.

    Args:
        segmentCount (int): Segment count. Note that the number of division points may differ from the segment count.
        includeEnds (bool): If true, then the point at the start of the first division segment is returned.

    Returns:
        double[]: List of curve parameters at the division points on success, None on failure.
    """
    url = "rhino/geometry/curve/dividebycount-curve_int_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentCount, includeEnds]
    if multiple: args = zip(thisCurve, segmentCount, includeEnds)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByCount1(thisCurve, segmentCount, includeEnds, multiple=False):
    """
    Divide the curve into a number of equal-length segments.

    Args:
        segmentCount (int): Segment count. Note that the number of division points may differ from the segment count.
        includeEnds (bool): If true, then the point at the start of the first division segment is returned.

    Returns:
        double[]: Array containing division curve parameters on success, None on failure.
        points (Point3d[]): A list of division points. If the function returns successfully, this point-array will be filled in.
    """
    url = "rhino/geometry/curve/dividebycount-curve_int_bool_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentCount, includeEnds]
    if multiple: args = zip(thisCurve, segmentCount, includeEnds)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByLength(thisCurve, segmentLength, includeEnds, multiple=False):
    """
    Divide the curve into specific length segments.

    Args:
        segmentLength (double): The length of each and every segment (except potentially the last one).
        includeEnds (bool): If true, then the point at the start of the first division segment is returned.

    Returns:
        double[]: Array containing division curve parameters if successful, None on failure.
    """
    url = "rhino/geometry/curve/dividebylength-curve_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, includeEnds]
    if multiple: args = zip(thisCurve, segmentLength, includeEnds)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByLength1(thisCurve, segmentLength, includeEnds, reverse, multiple=False):
    """
    Divide the curve into specific length segments.

    Args:
        segmentLength (double): The length of each and every segment (except potentially the last one).
        includeEnds (bool): If true, then the point at the start of the first division segment is returned.
        reverse (bool): If true, then the divisions start from the end of the curve.

    Returns:
        double[]: Array containing division curve parameters if successful, None on failure.
    """
    url = "rhino/geometry/curve/dividebylength-curve_double_bool_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, includeEnds, reverse]
    if multiple: args = zip(thisCurve, segmentLength, includeEnds, reverse)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByLength2(thisCurve, segmentLength, includeEnds, multiple=False):
    """
    Divide the curve into specific length segments.

    Args:
        segmentLength (double): The length of each and every segment (except potentially the last one).
        includeEnds (bool): If true, then the point at the start of the first division segment is returned.

    Returns:
        double[]: Array containing division curve parameters if successful, None on failure.
        points (Point3d[]): If function is successful, points at each parameter value are returned in points.
    """
    url = "rhino/geometry/curve/dividebylength-curve_double_bool_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, includeEnds]
    if multiple: args = zip(thisCurve, segmentLength, includeEnds)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByLength3(thisCurve, segmentLength, includeEnds, reverse, multiple=False):
    """
    Divide the curve into specific length segments.

    Args:
        segmentLength (double): The length of each and every segment (except potentially the last one).
        includeEnds (bool): If true, then the point at the start of the first division segment is returned.
        reverse (bool): If true, then the divisions start from the end of the curve.

    Returns:
        double[]: Array containing division curve parameters if successful, None on failure.
        points (Point3d[]): If function is successful, points at each parameter value are returned in points.
    """
    url = "rhino/geometry/curve/dividebylength-curve_double_bool_bool_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, includeEnds, reverse]
    if multiple: args = zip(thisCurve, segmentLength, includeEnds, reverse)
    response = Util.ComputeFetch(url, args)
    return response


def DivideEquidistant(thisCurve, distance, multiple=False):
    """
    Calculates 3d points on a curve where the linear distance between the points is equal.

    Args:
        distance (double): The distance betwen division points.

    Returns:
        Point3d[]: An array of equidistant points, or None on error.
    """
    url = "rhino/geometry/curve/divideequidistant-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, distance]
    if multiple: args = zip(thisCurve, distance)
    response = Util.ComputeFetch(url, args)
    return response


def DivideAsContour(thisCurve, contourStart, contourEnd, interval, multiple=False):
    """
    Divides this curve at fixed steps along a defined contour line.

    Args:
        contourStart (Point3d): The start of the contouring line.
        contourEnd (Point3d): The end of the contouring line.
        interval (double): A distance to measure on the contouring axis.

    Returns:
        Point3d[]: An array of points; or None on error.
    """
    url = "rhino/geometry/curve/divideascontour-curve_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, contourStart, contourEnd, interval]
    if multiple: args = zip(thisCurve, contourStart, contourEnd, interval)
    response = Util.ComputeFetch(url, args)
    return response


def Trim(thisCurve, side, length, multiple=False):
    """
    Shortens a curve by a given length

    Returns:
        Curve: Trimmed curve if successful, None on failure.
    """
    url = "rhino/geometry/curve/trim-curve_curveend_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, length]
    if multiple: args = zip(thisCurve, side, length)
    response = Util.ComputeFetch(url, args)
    return response


def Split(thisCurve, cutter, tolerance, multiple=False):
    """
    Splits a curve into pieces using a polysurface.

    Args:
        cutter (Brep): A cutting surface or polysurface.
        tolerance (double): A tolerance for computing intersections.

    Returns:
        Curve[]: An array of curves. This array can be empty.
    """
    url = "rhino/geometry/curve/split-curve_brep_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, cutter, tolerance]
    if multiple: args = zip(thisCurve, cutter, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Split1(thisCurve, cutter, tolerance, angleToleranceRadians, multiple=False):
    """
    Splits a curve into pieces using a polysurface.

    Args:
        cutter (Brep): A cutting surface or polysurface.
        tolerance (double): A tolerance for computing intersections.

    Returns:
        Curve[]: An array of curves. This array can be empty.
    """
    url = "rhino/geometry/curve/split-curve_brep_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, cutter, tolerance, angleToleranceRadians]
    if multiple: args = zip(thisCurve, cutter, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def Split2(thisCurve, cutter, tolerance, multiple=False):
    """
    Splits a curve into pieces using a surface.

    Args:
        cutter (Surface): A cutting surface or polysurface.
        tolerance (double): A tolerance for computing intersections.

    Returns:
        Curve[]: An array of curves. This array can be empty.
    """
    url = "rhino/geometry/curve/split-curve_surface_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, cutter, tolerance]
    if multiple: args = zip(thisCurve, cutter, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Split3(thisCurve, cutter, tolerance, angleToleranceRadians, multiple=False):
    """
    Splits a curve into pieces using a surface.

    Args:
        cutter (Surface): A cutting surface or polysurface.
        tolerance (double): A tolerance for computing intersections.

    Returns:
        Curve[]: An array of curves. This array can be empty.
    """
    url = "rhino/geometry/curve/split-curve_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, cutter, tolerance, angleToleranceRadians]
    if multiple: args = zip(thisCurve, cutter, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def Extend(thisCurve, t0, t1, multiple=False):
    """
    Where possible, analytically extends curve to include the given domain.
    This will not work on closed curves. The original curve will be identical to the
    restriction of the resulting curve to the original curve domain.

    Args:
        t0 (double): Start of extension domain, if the start is not inside the
            Domain of this curve, an attempt will be made to extend the curve.
        t1 (double): End of extension domain, if the end is not inside the
            Domain of this curve, an attempt will be made to extend the curve.

    Returns:
        Curve: Extended curve on success, None on failure.
    """
    url = "rhino/geometry/curve/extend-curve_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, t0, t1]
    if multiple: args = zip(thisCurve, t0, t1)
    response = Util.ComputeFetch(url, args)
    return response


def Extend1(thisCurve, domain, multiple=False):
    """
    Where possible, analytically extends curve to include the given domain.
    This will not work on closed curves. The original curve will be identical to the
    restriction of the resulting curve to the original curve domain.

    Args:
        domain (Interval): Extension domain.

    Returns:
        Curve: Extended curve on success, None on failure.
    """
    url = "rhino/geometry/curve/extend-curve_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, domain]
    if multiple: args = zip(thisCurve, domain)
    response = Util.ComputeFetch(url, args)
    return response


def Extend2(thisCurve, side, length, style, multiple=False):
    """
    Extends a curve by a specific length.

    Args:
        side (CurveEnd): Curve end to extend.
        length (double): Length to add to the curve end.
        style (CurveExtensionStyle): Extension style.

    Returns:
        Curve: A curve with extended ends or None on failure.
    """
    url = "rhino/geometry/curve/extend-curve_curveend_double_curveextensionstyle"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, length, style]
    if multiple: args = zip(thisCurve, side, length, style)
    response = Util.ComputeFetch(url, args)
    return response


def Extend3(thisCurve, side, style, geometry, multiple=False):
    """
    Extends a curve until it intersects a collection of objects.

    Args:
        side (CurveEnd): The end of the curve to extend.
        style (CurveExtensionStyle): The style or type of extension to use.
        geometry (System.Collections.Generic.IEnumerable<GeometryBase>): A collection of objects. Allowable object types are Curve, Surface, Brep.

    Returns:
        Curve: New extended curve result on success, None on failure.
    """
    url = "rhino/geometry/curve/extend-curve_curveend_curveextensionstyle_geometrybasearray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, style, geometry]
    if multiple: args = zip(thisCurve, side, style, geometry)
    response = Util.ComputeFetch(url, args)
    return response


def Extend4(thisCurve, side, style, endPoint, multiple=False):
    """
    Extends a curve to a point.

    Args:
        side (CurveEnd): The end of the curve to extend.
        style (CurveExtensionStyle): The style or type of extension to use.
        endPoint (Point3d): A new end point.

    Returns:
        Curve: New extended curve result on success, None on failure.
    """
    url = "rhino/geometry/curve/extend-curve_curveend_curveextensionstyle_point3d"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, style, endPoint]
    if multiple: args = zip(thisCurve, side, style, endPoint)
    response = Util.ComputeFetch(url, args)
    return response


def ExtendOnSurface(thisCurve, side, surface, multiple=False):
    """
    Extends a curve on a surface.

    Args:
        side (CurveEnd): The end of the curve to extend.
        surface (Surface): Surface that contains the curve.

    Returns:
        Curve: New extended curve result on success, None on failure.
    """
    url = "rhino/geometry/curve/extendonsurface-curve_curveend_surface"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, surface]
    if multiple: args = zip(thisCurve, side, surface)
    response = Util.ComputeFetch(url, args)
    return response


def ExtendOnSurface1(thisCurve, side, face, multiple=False):
    """
    Extends a curve on a surface.

    Args:
        side (CurveEnd): The end of the curve to extend.
        face (BrepFace): BrepFace that contains the curve.

    Returns:
        Curve: New extended curve result on success, None on failure.
    """
    url = "rhino/geometry/curve/extendonsurface-curve_curveend_brepface"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, face]
    if multiple: args = zip(thisCurve, side, face)
    response = Util.ComputeFetch(url, args)
    return response


def ExtendByLine(thisCurve, side, geometry, multiple=False):
    """
    Extends a curve by a line until it intersects a collection of objects.

    Args:
        side (CurveEnd): The end of the curve to extend.
        geometry (System.Collections.Generic.IEnumerable<GeometryBase>): A collection of objects. Allowable object types are Curve, Surface, Brep.

    Returns:
        Curve: New extended curve result on success, None on failure.
    """
    url = "rhino/geometry/curve/extendbyline-curve_curveend_geometrybasearray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, geometry]
    if multiple: args = zip(thisCurve, side, geometry)
    response = Util.ComputeFetch(url, args)
    return response


def ExtendByArc(thisCurve, side, geometry, multiple=False):
    """
    Extends a curve by an Arc until it intersects a collection of objects.

    Args:
        side (CurveEnd): The end of the curve to extend.
        geometry (System.Collections.Generic.IEnumerable<GeometryBase>): A collection of objects. Allowable object types are Curve, Surface, Brep.

    Returns:
        Curve: New extended curve result on success, None on failure.
    """
    url = "rhino/geometry/curve/extendbyarc-curve_curveend_geometrybasearray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, geometry]
    if multiple: args = zip(thisCurve, side, geometry)
    response = Util.ComputeFetch(url, args)
    return response


def Simplify(thisCurve, options, distanceTolerance, angleToleranceRadians, multiple=False):
    """
    Returns a geometrically equivalent PolyCurve.
    The PolyCurve has the following properties
    1. All the PolyCurve segments are LineCurve, PolylineCurve, ArcCurve, or NurbsCurve.
    
    2. The Nurbs Curves segments do not have fully multiple interior knots.
    
    3. Rational Nurbs curves do not have constant weights.
    
    4. Any segment for which IsLinear() or IsArc() is True is a Line,
    Polyline segment, or an Arc.
    
    5. Adjacent Colinear or Cocircular segments are combined.
    
    6. Segments that meet with G1-continuity have there ends tuned up so
    that they meet with G1-continuity to within machine precision.

    Args:
        options (CurveSimplifyOptions): Simplification options.
        distanceTolerance (double): A distance tolerance for the simplification.
        angleToleranceRadians (double): An angle tolerance for the simplification.

    Returns:
        Curve: New simplified curve on success, None on failure.
    """
    url = "rhino/geometry/curve/simplify-curve_curvesimplifyoptions_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, options, distanceTolerance, angleToleranceRadians]
    if multiple: args = zip(thisCurve, options, distanceTolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def SimplifyEnd(thisCurve, end, options, distanceTolerance, angleToleranceRadians, multiple=False):
    """
    Same as SimplifyCurve, but simplifies only the last two segments at "side" end.

    Args:
        end (CurveEnd): If CurveEnd.Start the function simplifies the last two start
            side segments, otherwise if CurveEnd.End the last two end side segments are simplified.
        options (CurveSimplifyOptions): Simplification options.
        distanceTolerance (double): A distance tolerance for the simplification.
        angleToleranceRadians (double): An angle tolerance for the simplification.

    Returns:
        Curve: New simplified curve on success, None on failure.
    """
    url = "rhino/geometry/curve/simplifyend-curve_curveend_curvesimplifyoptions_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, end, options, distanceTolerance, angleToleranceRadians]
    if multiple: args = zip(thisCurve, end, options, distanceTolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def Fair(thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations, multiple=False):
    """
    Fairs a curve object. Fair works best on degree 3 (cubic) curves. Attempts to
    remove large curvature variations while limiting the geometry changes to be no
    more than the specified tolerance.

    Args:
        distanceTolerance (double): Maximum allowed distance the faired curve is allowed to deviate from the input.
        angleTolerance (double): (in radians) kinks with angles <= angleTolerance are smoothed out 0.05 is a good default.
        clampStart (int): The number of (control vertices-1) to preserve at start.
            0 = preserve start point1 = preserve start point and 1st derivative2 = preserve start point, 1st and 2nd derivative
        clampEnd (int): Same as clampStart.
        iterations (int): The number of iteratoins to use in adjusting the curve.

    Returns:
        Curve: Returns new faired Curve on success, None on failure.
    """
    url = "rhino/geometry/curve/fair-curve_double_double_int_int_int"
    if multiple: url += "?multiple=true"
    args = [thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations]
    if multiple: args = zip(thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations)
    response = Util.ComputeFetch(url, args)
    return response


def Fit(thisCurve, degree, fitTolerance, angleTolerance, multiple=False):
    """
    Fits a new curve through an existing curve.

    Args:
        degree (int): The degree of the returned Curve. Must be bigger than 1.
        fitTolerance (double): The fitting tolerance. If fitTolerance is RhinoMath.UnsetValue or <=0.0,
            the document absolute tolerance is used.
        angleTolerance (double): The kink smoothing tolerance in radians.
            If angleTolerance is 0.0, all kinks are smoothedIf angleTolerance is >0.0, kinks smaller than angleTolerance are smoothedIf angleTolerance is RhinoMath.UnsetValue or <0.0, the document angle tolerance is used for the kink smoothing

    Returns:
        Curve: Returns a new fitted Curve if successful, None on failure.
    """
    url = "rhino/geometry/curve/fit-curve_int_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, degree, fitTolerance, angleTolerance]
    if multiple: args = zip(thisCurve, degree, fitTolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Rebuild(thisCurve, pointCount, degree, preserveTangents, multiple=False):
    """
    Rebuild a curve with a specific point count.

    Args:
        pointCount (int): Number of control points in the rebuild curve.
        degree (int): Degree of curve. Valid values are between and including 1 and 11.
        preserveTangents (bool): If true, the end tangents of the input curve will be preserved.

    Returns:
        NurbsCurve: A Nurbs curve on success or None on failure.
    """
    url = "rhino/geometry/curve/rebuild-curve_int_int_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, pointCount, degree, preserveTangents]
    if multiple: args = zip(thisCurve, pointCount, degree, preserveTangents)
    response = Util.ComputeFetch(url, args)
    return response


def ToPolyline(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, multiple=False):
    """
    Gets a polyline approximation of a curve.

    Args:
        mainSegmentCount (int): If mainSegmentCount <= 0, then both subSegmentCount and mainSegmentCount are ignored.
            If mainSegmentCount > 0, then subSegmentCount must be >= 1. In this
            case the nurb will be broken into mainSegmentCount equally spaced
            chords. If needed, each of these chords can be split into as many
            subSegmentCount sub-parts if the subdivision is necessary for the
            mesh to meet the other meshing constraints. In particular, if
            subSegmentCount = 0, then the curve is broken into mainSegmentCount
            pieces and no further testing is performed.
        subSegmentCount (int): An amount of subsegments.
        maxAngleRadians (double): ( 0 to pi ) Maximum angle (in radians) between unit tangents at
            adjacent vertices.
        maxChordLengthRatio (double): Maximum permitted value of
            (distance chord midpoint to curve) / (length of chord).
        maxAspectRatio (double): If maxAspectRatio < 1.0, the parameter is ignored.
            If 1 <= maxAspectRatio < sqrt(2), it is treated as if maxAspectRatio = sqrt(2).
            This parameter controls the maximum permitted value of
            (length of longest chord) / (length of shortest chord).
        tolerance (double): If tolerance = 0, the parameter is ignored.
            This parameter controls the maximum permitted value of the
            distance from the curve to the polyline.
        minEdgeLength (double): The minimum permitted edge length.
        maxEdgeLength (double): If maxEdgeLength = 0, the parameter
            is ignored. This parameter controls the maximum permitted edge length.
        keepStartPoint (bool): If True the starting point of the curve
            is added to the polyline. If False the starting point of the curve is
            not added to the polyline.

    Returns:
        PolylineCurve: PolylineCurve on success, None on error.
    """
    url = "rhino/geometry/curve/topolyline-curve_int_int_double_double_double_double_double_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint]
    if multiple: args = zip(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint)
    response = Util.ComputeFetch(url, args)
    return response


def ToPolyline1(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain, multiple=False):
    """
    Gets a polyline approximation of a curve.

    Args:
        mainSegmentCount (int): If mainSegmentCount <= 0, then both subSegmentCount and mainSegmentCount are ignored.
            If mainSegmentCount > 0, then subSegmentCount must be >= 1. In this
            case the nurb will be broken into mainSegmentCount equally spaced
            chords. If needed, each of these chords can be split into as many
            subSegmentCount sub-parts if the subdivision is necessary for the
            mesh to meet the other meshing constraints. In particular, if
            subSegmentCount = 0, then the curve is broken into mainSegmentCount
            pieces and no further testing is performed.
        subSegmentCount (int): An amount of subsegments.
        maxAngleRadians (double): ( 0 to pi ) Maximum angle (in radians) between unit tangents at
            adjacent vertices.
        maxChordLengthRatio (double): Maximum permitted value of
            (distance chord midpoint to curve) / (length of chord).
        maxAspectRatio (double): If maxAspectRatio < 1.0, the parameter is ignored.
            If 1 <= maxAspectRatio < sqrt(2), it is treated as if maxAspectRatio = sqrt(2).
            This parameter controls the maximum permitted value of
            (length of longest chord) / (length of shortest chord).
        tolerance (double): If tolerance = 0, the parameter is ignored.
            This parameter controls the maximum permitted value of the
            distance from the curve to the polyline.
        minEdgeLength (double): The minimum permitted edge length.
        maxEdgeLength (double): If maxEdgeLength = 0, the parameter
            is ignored. This parameter controls the maximum permitted edge length.
        keepStartPoint (bool): If True the starting point of the curve
            is added to the polyline. If False the starting point of the curve is
            not added to the polyline.
        curveDomain (Interval): This subdomain of the NURBS curve is approximated.

    Returns:
        PolylineCurve: PolylineCurve on success, None on error.
    """
    url = "rhino/geometry/curve/topolyline-curve_int_int_double_double_double_double_double_double_bool_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain]
    if multiple: args = zip(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain)
    response = Util.ComputeFetch(url, args)
    return response


def ToPolyline2(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength, multiple=False):
    """
    Gets a polyline approximation of a curve.

    Args:
        tolerance (double): The tolerance. This is the maximum deviation from line midpoints to the curve. When in doubt, use the document's model space absolute tolerance.
        angleTolerance (double): The angle tolerance in radians. This is the maximum deviation of the line directions. When in doubt, use the document's model space angle tolerance.
        minimumLength (double): The minimum segment length.
        maximumLength (double): The maximum segment length.

    Returns:
        PolylineCurve: PolyCurve on success, None on error.
    """
    url = "rhino/geometry/curve/topolyline-curve_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance, angleTolerance, minimumLength, maximumLength]
    if multiple: args = zip(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength)
    response = Util.ComputeFetch(url, args)
    return response


def ToArcsAndLines(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength, multiple=False):
    """
    Converts a curve into polycurve consisting of arc segments. Sections of the input curves that are nearly straight are converted to straight-line segments.

    Args:
        tolerance (double): The tolerance. This is the maximum deviation from arc midpoints to the curve. When in doubt, use the document's model space absolute tolerance.
        angleTolerance (double): The angle tolerance in radians. This is the maximum deviation of the arc end directions from the curve direction. When in doubt, use the document's model space angle tolerance.
        minimumLength (double): The minimum segment length.
        maximumLength (double): The maximum segment length.

    Returns:
        PolyCurve: PolyCurve on success, None on error.
    """
    url = "rhino/geometry/curve/toarcsandlines-curve_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance, angleTolerance, minimumLength, maximumLength]
    if multiple: args = zip(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength)
    response = Util.ComputeFetch(url, args)
    return response


def PullToMesh(thisCurve, mesh, tolerance, multiple=False):
    """
    Makes a polyline approximation of the curve and gets the closest point on the mesh for each point on the curve.
    Then it "connects the points" so that you have a polyline on the mesh.

    Args:
        mesh (Mesh): Mesh to project onto.
        tolerance (double): Input tolerance (RhinoDoc.ModelAbsoluteTolerance is a good default)

    Returns:
        PolylineCurve: A polyline curve on success, None on failure.
    """
    url = "rhino/geometry/curve/pulltomesh-curve_mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, mesh, tolerance]
    if multiple: args = zip(thisCurve, mesh, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Offset(thisCurve, plane, distance, tolerance, cornerStyle, multiple=False):
    """
    Offsets this curve. If you have a nice offset, then there will be one entry in
    the array. If the original curve had kinks or the offset curve had self
    intersections, you will get multiple segments in the offset_curves[] array.

    Args:
        plane (Plane): Offset solution plane.
        distance (double): The positive or negative distance to offset.
        tolerance (double): The offset or fitting tolerance.
        cornerStyle (CurveOffsetCornerStyle): Corner style for offset kinks.

    Returns:
        Curve[]: Offset curves on success, None on failure.
    """
    url = "rhino/geometry/curve/offset-curve_plane_double_double_curveoffsetcornerstyle"
    if multiple: url += "?multiple=true"
    args = [thisCurve, plane, distance, tolerance, cornerStyle]
    if multiple: args = zip(thisCurve, plane, distance, tolerance, cornerStyle)
    response = Util.ComputeFetch(url, args)
    return response


def Offset1(thisCurve, directionPoint, normal, distance, tolerance, cornerStyle, multiple=False):
    """
    Offsets this curve. If you have a nice offset, then there will be one entry in
    the array. If the original curve had kinks or the offset curve had self
    intersections, you will get multiple segments in the offset_curves[] array.

    Args:
        directionPoint (Point3d): A point that indicates the direction of the offset.
        normal (Vector3d): The normal to the offset plane.
        distance (double): The positive or negative distance to offset.
        tolerance (double): The offset or fitting tolerance.
        cornerStyle (CurveOffsetCornerStyle): Corner style for offset kinks.

    Returns:
        Curve[]: Offset curves on success, None on failure.
    """
    url = "rhino/geometry/curve/offset-curve_point3d_vector3d_double_double_curveoffsetcornerstyle"
    if multiple: url += "?multiple=true"
    args = [thisCurve, directionPoint, normal, distance, tolerance, cornerStyle]
    if multiple: args = zip(thisCurve, directionPoint, normal, distance, tolerance, cornerStyle)
    response = Util.ComputeFetch(url, args)
    return response


def RibbonOffset(thisCurve, distance, blendRadius, directionPoint, normal, tolerance, multiple=False):
    """
    Offsets a closed curve in the following way: pProject the curve to a plane with given normal.
    Then, loose Offset the projection by distance + blend_radius and trim off self-intersection.
    THen, Offset the remaining curve back in the opposite direction by blend_radius, filling gaps with blends.
    Finally, use the elevations of the input curve to get the correct elevations of the result.

    Args:
        distance (double): The positive distance to offset the curve.
        blendRadius (double): Positive, typically the same as distance. When the offset results in a self-intersection
            that gets trimmed off at a kink, the kink will be blended out using this radius.
        directionPoint (Point3d): A point that indicates the direction of the offset. If the offset is inward,
            the point's projection to the plane should be well within the curve.
            It will be used to decide which part of the offset to keep if there are self-intersections.
        normal (Vector3d): A vector that indicates the normal of the plane in which the offset will occur.
        tolerance (double): Used to determine self-intersections, not offset error.

    Returns:
        Curve: The offset curve if successful.
    """
    url = "rhino/geometry/curve/ribbonoffset-curve_double_double_point3d_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, distance, blendRadius, directionPoint, normal, tolerance]
    if multiple: args = zip(thisCurve, distance, blendRadius, directionPoint, normal, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface(thisCurve, face, distance, fittingTolerance, multiple=False):
    """
    Offset this curve on a brep face surface. This curve must lie on the surface.

    Args:
        face (BrepFace): The brep face on which to offset.
        distance (double): A distance to offset (+)left, (-)right.
        fittingTolerance (double): A fitting tolerance.

    Returns:
        Curve[]: Offset curves on success, or None on failure.
    """
    url = "rhino/geometry/curve/offsetonsurface-curve_brepface_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, face, distance, fittingTolerance]
    if multiple: args = zip(thisCurve, face, distance, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface1(thisCurve, face, throughPoint, fittingTolerance, multiple=False):
    """
    Offset a curve on a brep face surface. This curve must lie on the surface.
    This overload allows to specify a surface point at which the offset will pass.

    Args:
        face (BrepFace): The brep face on which to offset.
        throughPoint (Point2d): 2d point on the brep face to offset through.
        fittingTolerance (double): A fitting tolerance.

    Returns:
        Curve[]: Offset curves on success, or None on failure.
    """
    url = "rhino/geometry/curve/offsetonsurface-curve_brepface_point2d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, face, throughPoint, fittingTolerance]
    if multiple: args = zip(thisCurve, face, throughPoint, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface2(thisCurve, face, curveParameters, offsetDistances, fittingTolerance, multiple=False):
    """
    Offset a curve on a brep face surface. This curve must lie on the surface.
    This overload allows to specify different offsets for different curve parameters.

    Args:
        face (BrepFace): The brep face on which to offset.
        curveParameters (double[]): Curve parameters corresponding to the offset distances.
        offsetDistances (double[]): distances to offset (+)left, (-)right.
        fittingTolerance (double): A fitting tolerance.

    Returns:
        Curve[]: Offset curves on success, or None on failure.
    """
    url = "rhino/geometry/curve/offsetonsurface-curve_brepface_doublearray_doublearray_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, face, curveParameters, offsetDistances, fittingTolerance]
    if multiple: args = zip(thisCurve, face, curveParameters, offsetDistances, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface3(thisCurve, surface, distance, fittingTolerance, multiple=False):
    """
    Offset a curve on a surface. This curve must lie on the surface.

    Args:
        surface (Surface): A surface on which to offset.
        distance (double): A distance to offset (+)left, (-)right.
        fittingTolerance (double): A fitting tolerance.

    Returns:
        Curve[]: Offset curves on success, or None on failure.
    """
    url = "rhino/geometry/curve/offsetonsurface-curve_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, surface, distance, fittingTolerance]
    if multiple: args = zip(thisCurve, surface, distance, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface4(thisCurve, surface, throughPoint, fittingTolerance, multiple=False):
    """
    Offset a curve on a surface. This curve must lie on the surface.
    This overload allows to specify a surface point at which the offset will pass.

    Args:
        surface (Surface): A surface on which to offset.
        throughPoint (Point2d): 2d point on the brep face to offset through.
        fittingTolerance (double): A fitting tolerance.

    Returns:
        Curve[]: Offset curves on success, or None on failure.
    """
    url = "rhino/geometry/curve/offsetonsurface-curve_surface_point2d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, surface, throughPoint, fittingTolerance]
    if multiple: args = zip(thisCurve, surface, throughPoint, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface5(thisCurve, surface, curveParameters, offsetDistances, fittingTolerance, multiple=False):
    """
    Offset this curve on a surface. This curve must lie on the surface.
    This overload allows to specify different offsets for different curve parameters.

    Args:
        surface (Surface): A surface on which to offset.
        curveParameters (double[]): Curve parameters corresponding to the offset distances.
        offsetDistances (double[]): Distances to offset (+)left, (-)right.
        fittingTolerance (double): A fitting tolerance.

    Returns:
        Curve[]: Offset curves on success, or None on failure.
    """
    url = "rhino/geometry/curve/offsetonsurface-curve_surface_doublearray_doublearray_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, surface, curveParameters, offsetDistances, fittingTolerance]
    if multiple: args = zip(thisCurve, surface, curveParameters, offsetDistances, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def PullToBrepFace(thisCurve, face, tolerance, multiple=False):
    """
    Pulls this curve to a brep face and returns the result of that operation.

    Args:
        face (BrepFace): A brep face.
        tolerance (double): A tolerance value.

    Returns:
        Curve[]: An array containing the resulting curves after pulling. This array could be empty.
    """
    url = "rhino/geometry/curve/pulltobrepface-curve_brepface_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, face, tolerance]
    if multiple: args = zip(thisCurve, face, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetNormalToSurface(thisCurve, surface, height, multiple=False):
    """
    Finds a curve by offsetting an existing curve normal to a surface.
    The caller is responsible for ensuring that the curve lies on the input surface.

    Args:
        surface (Surface): Surface from which normals are calculated.
        height (double): offset distance (distance from surface to result curve)

    Returns:
        Curve: Offset curve at distance height from the surface.  The offset curve is
        interpolated through a small number of points so if the surface is irregular
        or complicated, the result will not be a very accurate offset.
    """
    url = "rhino/geometry/curve/offsetnormaltosurface-curve_surface_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, surface, height]
    if multiple: args = zip(thisCurve, surface, height)
    response = Util.ComputeFetch(url, args)
    return response

