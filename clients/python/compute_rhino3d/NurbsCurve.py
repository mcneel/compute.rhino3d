from . import Util
try:
    from itertools import izip as zip # python 2
except ImportError:
    pass # python 3


def MakeCompatible(curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance, multiple=False):
    """
    For expert use only. From the input curves, make an array of compatible NURBS curves.

    Args:
        curves (IEnumerable<Curve>): The input curves.
        startPt (Point3d): The start point. To omit, specify Point3d.Unset.
        endPt (Point3d): The end point. To omit, specify Point3d.Unset.
        simplifyMethod (int): The simplify method.
        numPoints (int): The number of rebuild points.
        refitTolerance (double): The refit tolerance.
        angleTolerance (double): The angle tolerance in radians.

    Returns:
        NurbsCurve[]: The output NURBS surfaces if successful.
    """
    url = "rhino/geometry/nurbscurve/makecompatible-curvearray_point3d_point3d_int_int_double_double"
    if multiple: url += "?multiple=true"
    args = [curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance]
    if multiple: args = list(zip(curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateParabolaFromVertex(vertex, startPoint, endPoint, multiple=False):
    """
    Creates a parabola from vertex and end points.

    Args:
        vertex (Point3d): The vertex point.
        startPoint (Point3d): The start point.
        endPoint (Point3d): The end point

    Returns:
        NurbsCurve: A 2 degree NURBS curve if successful, False otherwise.
    """
    url = "rhino/geometry/nurbscurve/createparabolafromvertex-point3d_point3d_point3d"
    if multiple: url += "?multiple=true"
    args = [vertex, startPoint, endPoint]
    if multiple: args = list(zip(vertex, startPoint, endPoint))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateParabolaFromFocus(focus, startPoint, endPoint, multiple=False):
    """
    Creates a parabola from focus and end points.

    Args:
        focus (Point3d): The focal point.
        startPoint (Point3d): The start point.
        endPoint (Point3d): The end point

    Returns:
        NurbsCurve: A 2 degree NURBS curve if successful, False otherwise.
    """
    url = "rhino/geometry/nurbscurve/createparabolafromfocus-point3d_point3d_point3d"
    if multiple: url += "?multiple=true"
    args = [focus, startPoint, endPoint]
    if multiple: args = list(zip(focus, startPoint, endPoint))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromArc(arc, degree, cvCount, multiple=False):
    """
    Create a uniform non-rational cubic NURBS approximation of an arc.

    Args:
        degree (int): >=1
        cvCount (int): CV count >=5

    Returns:
        NurbsCurve: NURBS curve approximation of an arc on success
    """
    url = "rhino/geometry/nurbscurve/createfromarc-arc_int_int"
    if multiple: url += "?multiple=true"
    args = [arc, degree, cvCount]
    if multiple: args = list(zip(arc, degree, cvCount))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateHSpline(points, multiple=False):
    """
    Construct an H-spline from a sequence of interpolation points

    Args:
        points (IEnumerable<Point3d>): Points to interpolate
    """
    url = "rhino/geometry/nurbscurve/createhspline-point3darray"
    if multiple: url += "?multiple=true"
    args = [points]
    if multiple: args = [[item] for item in points]
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateHSpline1(points, startTangent, endTangent, multiple=False):
    """
    Construct an H-spline from a sequence of interpolation points and
    optional start and end derivative information

    Args:
        points (IEnumerable<Point3d>): Points to interpolate
        startTangent (Vector3d): Unit tangent vector or Unset
        endTangent (Vector3d): Unit tangent vector or Unset

    Returns:
        NurbsCurve: NURBS curve approximation of an arc on success
    """
    url = "rhino/geometry/nurbscurve/createhspline-point3darray_vector3d_vector3d"
    if multiple: url += "?multiple=true"
    args = [points, startTangent, endTangent]
    if multiple: args = list(zip(points, startTangent, endTangent))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateSubDFriendly(points, interpolatePoints, periodicClosedCurve, multiple=False):
    """
    Create a NURBS curve, that is suitable for calculations like lofting SubD objects, through a sequence of curves.

    Args:
        points (IEnumerable<Point3d>): An enumeration of points. Adjacent points must not be equal.
            If periodicClosedCurve is false, there must be at least two points.
            If periodicClosedCurve is true, there must be at least three points and it is not necessary to duplicate the first and last points.
            When periodicClosedCurve is True and the first and last points are equal, the duplicate last point is automatically ignored.
        interpolatePoints (bool): True if the curve should interpolate the points. False if points specify control point locations.
            In either case, the curve will begin at the first point and end at the last point.
        periodicClosedCurve (bool): True to create a periodic closed curve. Do not duplicate the start/end point in the point input.

    Returns:
        NurbsCurve: A SubD friendly NURBS curve is successful, None otherwise.
    """
    url = "rhino/geometry/nurbscurve/createsubdfriendly-point3darray_bool_bool"
    if multiple: url += "?multiple=true"
    args = [points, interpolatePoints, periodicClosedCurve]
    if multiple: args = list(zip(points, interpolatePoints, periodicClosedCurve))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateSubDFriendly1(curve, multiple=False):
    """
    Create a NURBS curve, that is suitable for calculations like lofting SubD objects, from an existing curve.

    Args:
        curve (Curve): Curve to rebuild as a SubD friendly curve.

    Returns:
        NurbsCurve: A SubD friendly NURBS curve is successful, None otherwise.
    """
    url = "rhino/geometry/nurbscurve/createsubdfriendly-curve"
    if multiple: url += "?multiple=true"
    args = [curve]
    if multiple: args = [[item] for item in curve]
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateSubDFriendly2(curve, pointCount, periodicClosedCurve, multiple=False):
    """
    Create a NURBS curve, that is suitable for calculations like lofting SubD objects, from an existing curve.

    Args:
        curve (Curve): Curve to rebuild as a SubD friendly curve.
        pointCount (int): Desired number of control points. If periodicClosedCurve is true, the number must be >= 6, otherwise the number must be >= 4.
        periodicClosedCurve (bool): True if the SubD friendly curve should be closed and periodic. False in all other cases.

    Returns:
        NurbsCurve: A SubD friendly NURBS curve is successful, None otherwise.
    """
    url = "rhino/geometry/nurbscurve/createsubdfriendly-curve_int_bool"
    if multiple: url += "?multiple=true"
    args = [curve, pointCount, periodicClosedCurve]
    if multiple: args = list(zip(curve, pointCount, periodicClosedCurve))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreatePlanarRailFrames(thisNurbsCurve, parameters, normal, multiple=False):
    """
    Computes planar rail sweep frames at specified parameters.

    Args:
        parameters (IEnumerable<double>): A collection of curve parameters.
        normal (Vector3d): Unit normal to the plane.

    Returns:
        Plane[]: An array of planes if successful, or an empty array on failure.
    """
    url = "rhino/geometry/nurbscurve/createplanarrailframes-nurbscurve_doublearray_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, parameters, normal]
    if multiple: args = list(zip(thisNurbsCurve, parameters, normal))
    response = Util.ComputeFetch(url, args)
    return response


def CreateRailFrames(thisNurbsCurve, parameters, multiple=False):
    """
    Computes relatively parallel rail sweep frames at specified parameters.

    Args:
        parameters (IEnumerable<double>): A collection of curve parameters.

    Returns:
        Plane[]: An array of planes if successful, or an empty array on failure.
    """
    url = "rhino/geometry/nurbscurve/createrailframes-nurbscurve_doublearray"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, parameters]
    if multiple: args = list(zip(thisNurbsCurve, parameters))
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCircle(circle, degree, cvCount, multiple=False):
    """
    Create a uniform non-rational cubic NURBS approximation of a circle.

    Args:
        degree (int): >=1
        cvCount (int): CV count >=5

    Returns:
        NurbsCurve: NURBS curve approximation of a circle on success
    """
    url = "rhino/geometry/nurbscurve/createfromcircle-circle_int_int"
    if multiple: url += "?multiple=true"
    args = [circle, degree, cvCount]
    if multiple: args = list(zip(circle, degree, cvCount))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def SetEndCondition(thisNurbsCurve, bSetEnd, continuity, point, tangent, multiple=False):
    """
    Set end condition of a NURBS curve to point, tangent and curvature.

    Args:
        bSetEnd (bool): true: set end of curve, false: set start of curve
        continuity (NurbsCurveEndConditionType): Position: set start or end point, Tangency: set point and tangent, Curvature: set point, tangent and curvature
        point (Point3d): point to set
        tangent (Vector3d): tangent to set

    Returns:
        bool: True on success, False on failure.
    """
    url = "rhino/geometry/nurbscurve/setendcondition-nurbscurve_bool_nurbscurveendconditiontype_point3d_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, bSetEnd, continuity, point, tangent]
    if multiple: args = list(zip(thisNurbsCurve, bSetEnd, continuity, point, tangent))
    response = Util.ComputeFetch(url, args)
    return response


def SetEndCondition1(thisNurbsCurve, bSetEnd, continuity, point, tangent, curvature, multiple=False):
    """
    Set end condition of a NURBS curve to point, tangent and curvature.

    Args:
        bSetEnd (bool): true: set end of curve, false: set start of curve
        continuity (NurbsCurveEndConditionType): Position: set start or end point, Tangency: set point and tangent, Curvature: set point, tangent and curvature
        point (Point3d): point to set
        tangent (Vector3d): tangent to set
        curvature (Vector3d): curvature to set

    Returns:
        bool: True on success, False on failure.
    """
    url = "rhino/geometry/nurbscurve/setendcondition-nurbscurve_bool_nurbscurveendconditiontype_point3d_vector3d_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, bSetEnd, continuity, point, tangent, curvature]
    if multiple: args = list(zip(thisNurbsCurve, bSetEnd, continuity, point, tangent, curvature))
    response = Util.ComputeFetch(url, args)
    return response


def GrevillePoints(thisNurbsCurve, all, multiple=False):
    """
    Gets Greville points for this curve.

    Args:
        all (bool): If true, then all Greville points are returns. If false, only edit points are returned.

    Returns:
        Point3dList: A list of points if successful, None otherwise.
    """
    url = "rhino/geometry/nurbscurve/grevillepoints-nurbscurve_bool"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, all]
    if multiple: args = list(zip(thisNurbsCurve, all))
    response = Util.ComputeFetch(url, args)
    return response


def SetGrevillePoints(thisNurbsCurve, points, multiple=False):
    """
    Sets all Greville edit points for this curve.

    Args:
        points (IEnumerable<Point3d>): The new point locations. The number of points should match
            the number of point returned by NurbsCurve.GrevillePoints(false).

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/nurbscurve/setgrevillepoints-nurbscurve_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, points]
    if multiple: args = list(zip(thisNurbsCurve, points))
    response = Util.ComputeFetch(url, args)
    return response


def CreateSpiral(axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1, multiple=False):
    """
    Creates a C1 cubic NURBS approximation of a helix or spiral. For a helix,
    you may have radius0 == radius1. For a spiral radius0 == radius1 produces
    a circle. Zero and negative radii are permissible.

    Args:
        axisStart (Point3d): Helix's axis starting point or center of spiral.
        axisDir (Vector3d): Helix's axis vector or normal to spiral's plane.
        radiusPoint (Point3d): Point used only to get a vector that is perpendicular to the axis. In
            particular, this vector must not be (anti)parallel to the axis vector.
        pitch (double): The pitch, where a spiral has a pitch = 0, and pitch > 0 is the distance
            between the helix's "threads".
        turnCount (double): The number of turns in spiral or helix. Positive
            values produce counter-clockwise orientation, negative values produce
            clockwise orientation. Note, for a helix, turnCount * pitch = length of
            the helix's axis.
        radius0 (double): The starting radius.
        radius1 (double): The ending radius.

    Returns:
        NurbsCurve: NurbsCurve on success, None on failure.
    """
    url = "rhino/geometry/nurbscurve/createspiral-point3d_vector3d_point3d_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1]
    if multiple: args = list(zip(axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateSpiral1(railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn, multiple=False):
    """
    Create a C2 non-rational uniform cubic NURBS approximation of a swept helix or spiral.

    Args:
        railCurve (Curve): The rail curve.
        t0 (double): Starting portion of rail curve's domain to sweep along.
        t1 (double): Ending portion of rail curve's domain to sweep along.
        radiusPoint (Point3d): Point used only to get a vector that is perpendicular to the axis. In
            particular, this vector must not be (anti)parallel to the axis vector.
        pitch (double): The pitch. Positive values produce counter-clockwise orientation,
            negative values produce clockwise orientation.
        turnCount (double): The turn count. If != 0, then the resulting helix will have this many
            turns. If = 0, then pitch must be != 0 and the approximate distance
            between turns will be set to pitch. Positive values produce counter-clockwise
            orientation, negative values produce clockwise orientation.
        radius0 (double): The starting radius. At least one radii must be nonzero. Negative values
            are allowed.
        radius1 (double): The ending radius. At least one radii must be nonzero. Negative values
            are allowed.
        pointsPerTurn (int): Number of points to interpolate per turn. Must be greater than 4.
            When in doubt, use 12.

    Returns:
        NurbsCurve: NurbsCurve on success, None on failure.
    """
    url = "rhino/geometry/nurbscurve/createspiral-curve_double_double_point3d_double_double_double_double_int"
    if multiple: url += "?multiple=true"
    args = [railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn]
    if multiple: args = list(zip(railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response

