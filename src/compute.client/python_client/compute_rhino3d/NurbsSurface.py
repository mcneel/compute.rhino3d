from . import Util


def CreateCurveOnSurfacePoints(surface, fixedPoints, tolerance, periodic, initCount, levels, multiple=False):
    """
    Computes a discrete spline curve on the surface. In other words, computes a sequence
    of points on the surface, each with a corresponding parameter value.

    Args:
        surface (Surface): The surface on which the curve is constructed. The surface should be G1 continuous.
            If the surface is closed in the u or v direction and is G1 at the seam, the
            function will construct point sequences that cross over the seam.
        fixedPoints (IEnumerable<Point2d>): Surface points to interpolate given by parameters. These must be distinct.
        tolerance (double): Relative tolerance used by the solver. When in doubt, use a tolerance of 0.0.
        periodic (bool): When True constructs a smoothly closed curve.
        initCount (int): Maximum number of points to insert beteween fixed points on the first level.
        levels (int): The number of levels (between 1 and 3) to be used in multi-level solver. Use 1 for single level solve.

    Returns:
        Point2d[]: A sequence of surface points, given by surface parameters, if successful.
        The number of output points is approximatelely: 2 ^ (level-1) * initCount * fixedPoints.Count.
    """
    url = "rhino/geometry/nurbssurface/createcurveonsurfacepoints-surface_point2darray_double_bool_int_int"
    if multiple: url += "?multiple=true"
    args = [surface, fixedPoints, tolerance, periodic, initCount, levels]
    if multiple: args = zip(surface, fixedPoints, tolerance, periodic, initCount, levels)
    response = Util.ComputeFetch(url, args)
    return response


def CreateCurveOnSurface(surface, points, tolerance, periodic, multiple=False):
    """
    Fit a sequence of 2d points on a surface to make a curve on the surface.

    Args:
        surface (Surface): Surface on which to construct curve.
        points (IEnumerable<Point2d>): Parameter space coodinates of the points to interpolate.
        tolerance (double): Curve should be within tolerance of surface and points.
        periodic (bool): When True make a periodic curve.

    Returns:
        NurbsCurve: A curve interpolating the points if successful, None on error.
    """
    url = "rhino/geometry/nurbssurface/createcurveonsurface-surface_point2darray_double_bool"
    if multiple: url += "?multiple=true"
    args = [surface, points, tolerance, periodic]
    if multiple: args = zip(surface, points, tolerance, periodic)
    response = Util.ComputeFetch(url, args)
    return response


def MakeCompatible(surface0, surface1, nurb0, nurb1, multiple=False):
    """
    For expert use only. Makes a pair of compatible NURBS surfaces based on two input surfaces.

    Args:
        surface0 (Surface): The first surface.
        surface1 (Surface): The second surface.
        nurb0 (NurbsSurface): The first output NURBS surface.
        nurb1 (NurbsSurface): The second output NURBS surface.

    Returns:
        bool: True if successsful, False on failure.
    """
    url = "rhino/geometry/nurbssurface/makecompatible-surface_surface_nurbssurface_nurbssurface"
    if multiple: url += "?multiple=true"
    args = [surface0, surface1, nurb0, nurb1]
    if multiple: args = zip(surface0, surface1, nurb0, nurb1)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromPoints(points, uCount, vCount, uDegree, vDegree, multiple=False):
    """
    Constructs a NURBS surface from a 2D grid of control points.

    Args:
        points (IEnumerable<Point3d>): Control point locations.
        uCount (int): Number of points in U direction.
        vCount (int): Number of points in V direction.
        uDegree (int): Degree of surface in U direction.
        vDegree (int): Degree of surface in V direction.

    Returns:
        NurbsSurface: A NurbsSurface on success or None on failure.
    """
    url = "rhino/geometry/nurbssurface/createfrompoints-point3darray_int_int_int_int"
    if multiple: url += "?multiple=true"
    args = [points, uCount, vCount, uDegree, vDegree]
    if multiple: args = zip(points, uCount, vCount, uDegree, vDegree)
    response = Util.ComputeFetch(url, args)
    return response


def CreateThroughPoints(points, uCount, vCount, uDegree, vDegree, uClosed, vClosed, multiple=False):
    """
    Constructs a NURBS surface from a 2D grid of points.

    Args:
        points (IEnumerable<Point3d>): Control point locations.
        uCount (int): Number of points in U direction.
        vCount (int): Number of points in V direction.
        uDegree (int): Degree of surface in U direction.
        vDegree (int): Degree of surface in V direction.
        uClosed (bool): True if the surface should be closed in the U direction.
        vClosed (bool): True if the surface should be closed in the V direction.

    Returns:
        NurbsSurface: A NurbsSurface on success or None on failure.
    """
    url = "rhino/geometry/nurbssurface/createthroughpoints-point3darray_int_int_int_int_bool_bool"
    if multiple: url += "?multiple=true"
    args = [points, uCount, vCount, uDegree, vDegree, uClosed, vClosed]
    if multiple: args = zip(points, uCount, vCount, uDegree, vDegree, uClosed, vClosed)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCorners(corner1, corner2, corner3, corner4, multiple=False):
    """
    Makes a surface from 4 corner points.
    This is the same as calling  with tolerance 0.

    Args:
        corner1 (Point3d): The first corner.
        corner2 (Point3d): The second corner.
        corner3 (Point3d): The third corner.
        corner4 (Point3d): The fourth corner.

    Returns:
        NurbsSurface: the resulting surface or None on error.
    """
    url = "rhino/geometry/nurbssurface/createfromcorners-point3d_point3d_point3d_point3d"
    if multiple: url += "?multiple=true"
    args = [corner1, corner2, corner3, corner4]
    if multiple: args = zip(corner1, corner2, corner3, corner4)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCorners1(corner1, corner2, corner3, corner4, tolerance, multiple=False):
    """
    Makes a surface from 4 corner points.

    Args:
        corner1 (Point3d): The first corner.
        corner2 (Point3d): The second corner.
        corner3 (Point3d): The third corner.
        corner4 (Point3d): The fourth corner.
        tolerance (double): Minimum edge length without collapsing to a singularity.

    Returns:
        NurbsSurface: The resulting surface or None on error.
    """
    url = "rhino/geometry/nurbssurface/createfromcorners-point3d_point3d_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [corner1, corner2, corner3, corner4, tolerance]
    if multiple: args = zip(corner1, corner2, corner3, corner4, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCorners2(corner1, corner2, corner3, multiple=False):
    """
    Makes a surface from 3 corner points.

    Args:
        corner1 (Point3d): The first corner.
        corner2 (Point3d): The second corner.
        corner3 (Point3d): The third corner.

    Returns:
        NurbsSurface: The resulting surface or None on error.
    """
    url = "rhino/geometry/nurbssurface/createfromcorners-point3d_point3d_point3d"
    if multiple: url += "?multiple=true"
    args = [corner1, corner2, corner3]
    if multiple: args = zip(corner1, corner2, corner3)
    response = Util.ComputeFetch(url, args)
    return response


def CreateRailRevolvedSurface(profile, rail, axis, scaleHeight, multiple=False):
    """
    Constructs a railed Surface-of-Revolution.

    Args:
        profile (Curve): Profile curve for revolution.
        rail (Curve): Rail curve for revolution.
        axis (Line): Axis of revolution.
        scaleHeight (bool): If true, surface will be locally scaled.

    Returns:
        NurbsSurface: A NurbsSurface or None on failure.
    """
    url = "rhino/geometry/nurbssurface/createrailrevolvedsurface-curve_curve_line_bool"
    if multiple: url += "?multiple=true"
    args = [profile, rail, axis, scaleHeight]
    if multiple: args = zip(profile, rail, axis, scaleHeight)
    response = Util.ComputeFetch(url, args)
    return response


def CreateNetworkSurface(uCurves, uContinuityStart, uContinuityEnd, vCurves, vContinuityStart, vContinuityEnd, edgeTolerance, interiorTolerance, angleTolerance, error, multiple=False):
    """
    Builds a surface from an ordered network of curves/edges.

    Args:
        uCurves (IEnumerable<Curve>): An array, a list or any enumerable set of U curves.
        uContinuityStart (int): continuity at first U segment, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
        uContinuityEnd (int): continuity at last U segment, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
        vCurves (IEnumerable<Curve>): An array, a list or any enumerable set of V curves.
        vContinuityStart (int): continuity at first V segment, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
        vContinuityEnd (int): continuity at last V segment, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
        edgeTolerance (double): tolerance to use along network surface edge.
        interiorTolerance (double): tolerance to use for the interior curves.
        angleTolerance (double): angle tolerance to use.
        error (int): If the NurbsSurface could not be created, the error value describes where
            the failure occured.  0 = success,  1 = curve sorter failed, 2 = network initializing failed,
            3 = failed to build surface, 4 = network surface is not valid.

    Returns:
        NurbsSurface: A NurbsSurface or None on failure.
    """
    url = "rhino/geometry/nurbssurface/createnetworksurface-curvearray_int_int_curvearray_int_int_double_double_double_int"
    if multiple: url += "?multiple=true"
    args = [uCurves, uContinuityStart, uContinuityEnd, vCurves, vContinuityStart, vContinuityEnd, edgeTolerance, interiorTolerance, angleTolerance, error]
    if multiple: args = zip(uCurves, uContinuityStart, uContinuityEnd, vCurves, vContinuityStart, vContinuityEnd, edgeTolerance, interiorTolerance, angleTolerance, error)
    response = Util.ComputeFetch(url, args)
    return response


def CreateNetworkSurface1(curves, continuity, edgeTolerance, interiorTolerance, angleTolerance, error, multiple=False):
    """
    Builds a surface from an autosorted network of curves/edges.

    Args:
        curves (IEnumerable<Curve>): An array, a list or any enumerable set of curves/edges, sorted automatically into U and V curves.
        continuity (int): continuity along edges, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
        edgeTolerance (double): tolerance to use along network surface edge.
        interiorTolerance (double): tolerance to use for the interior curves.
        angleTolerance (double): angle tolerance to use.
        error (int): If the NurbsSurface could not be created, the error value describes where
            the failure occured.  0 = success,  1 = curve sorter failed, 2 = network initializing failed,
            3 = failed to build surface, 4 = network surface is not valid.

    Returns:
        NurbsSurface: A NurbsSurface or None on failure.
    """
    url = "rhino/geometry/nurbssurface/createnetworksurface-curvearray_int_double_double_double_int"
    if multiple: url += "?multiple=true"
    args = [curves, continuity, edgeTolerance, interiorTolerance, angleTolerance, error]
    if multiple: args = zip(curves, continuity, edgeTolerance, interiorTolerance, angleTolerance, error)
    response = Util.ComputeFetch(url, args)
    return response

