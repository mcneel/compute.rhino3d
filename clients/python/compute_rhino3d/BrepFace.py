from . import Util
try:
    from itertools import izip as zip # python 2
except ImportError:
    pass # python 3


def PullPointsToFace(thisBrepFace, points, tolerance, multiple=False):
    """
    Pulls one or more points to a brep face.

    Args:
        points (IEnumerable<Point3d>): Points to pull.
        tolerance (double): Tolerance for pulling operation. Only points that are closer than tolerance will be pulled to the face.

    Returns:
        Point3d[]: An array of pulled points.
    """
    url = "rhino/geometry/brepface/pullpointstoface-brepface_point3darray_double"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, points, tolerance]
    if multiple: args = list(zip(thisBrepFace, points, tolerance))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToPoint3d(response)
    return response


def DraftAnglePoint(thisBrepFace, testPoint, testAngle, pullDirection, edge, multiple=False):
    """
    Returns the surface draft angle and point at a parameter.

    Args:
        testPoint (Point2d): The u,v parameter on the face to evaluate.
        testAngle (double): The angle in radians to test.
        pullDirection (Vector3d): The pull direction.
        edge (bool): Restricts the point placement to an edge.

    Returns:
        bool: True if successful, False otherwise.
        draftPoint (Point3d): The draft angle point.
        draftAngle (double): The draft angle in radians.
    """
    url = "rhino/geometry/brepface/draftanglepoint-brepface_point2d_double_vector3d_bool_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, testPoint, testAngle, pullDirection, edge]
    if multiple: args = list(zip(thisBrepFace, testPoint, testAngle, pullDirection, edge))
    response = Util.ComputeFetch(url, args)
    return response


def RemoveHoles(thisBrepFace, tolerance, multiple=False):
    """
    Remove all inner loops, or holes, from a Brep face.
    """
    url = "rhino/geometry/brepface/removeholes-brepface_double"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, tolerance]
    if multiple: args = list(zip(thisBrepFace, tolerance))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def ShrinkSurfaceToEdge(thisBrepFace, multiple=False):
    """
    Shrinks the underlying untrimmed surface of this Brep face right to the trimming boundaries.
    Note, shrinking the trimmed surface can sometimes cause problems later since having
    the edges so close to the trimming boundaries can cause commands that use the surface
    edges as input to fail.

    Returns:
        bool: True on success, False on failure.
    """
    url = "rhino/geometry/brepface/shrinksurfacetoedge-brepface"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace]
    if multiple: args = [[item] for item in thisBrepFace]
    response = Util.ComputeFetch(url, args)
    return response


def Split(thisBrepFace, curves, tolerance, multiple=False):
    """
    Split this face using 3D trimming curves.

    Args:
        curves (IEnumerable<Curve>): Curves to split with.
        tolerance (double): Tolerance for splitting, when in doubt use the Document Absolute Tolerance.

    Returns:
        Brep: A brep consisting of all the split fragments, or None on failure.
    """
    url = "rhino/geometry/brepface/split-brepface_curvearray_double"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, curves, tolerance]
    if multiple: args = list(zip(thisBrepFace, curves, tolerance))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def IsPointOnFace(thisBrepFace, u, v, multiple=False):
    """
    Tests if a parameter space point is in the active region of a face.

    Args:
        u (double): Parameter space point U value.
        v (double): Parameter space point V value.

    Returns:
        PointFaceRelation: A value describing the relationship between the point and the face.
    """
    url = "rhino/geometry/brepface/ispointonface-brepface_double_double"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, u, v]
    if multiple: args = list(zip(thisBrepFace, u, v))
    response = Util.ComputeFetch(url, args)
    return response


def IsPointOnFace1(thisBrepFace, u, v, tolerance, multiple=False):
    """
    Tests if a parameter space point is in the active region of a face.

    Args:
        u (double): Parameter space point U value.
        v (double): Parameter space point V value.
        tolerance (double): 3D tolerance used when checking to see if the point is on a face or inside of a loop.

    Returns:
        PointFaceRelation: A value describing the relationship between the point and the face.
    """
    url = "rhino/geometry/brepface/ispointonface-brepface_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, u, v, tolerance]
    if multiple: args = list(zip(thisBrepFace, u, v, tolerance))
    response = Util.ComputeFetch(url, args)
    return response


def TrimAwareIsoIntervals(thisBrepFace, direction, constantParameter, multiple=False):
    """
    Gets intervals where the iso curve exists on a BrepFace (trimmed surface)

    Args:
        direction (int): Direction of isocurve.
            0 = Isocurve connects all points with a constant U value.1 = Isocurve connects all points with a constant V value.
        constantParameter (double): Surface parameter that remains identical along the isocurves.

    Returns:
        Interval[]: If direction = 0, the parameter space iso interval connects the 2d points
        (intervals[i][0],iso_constant) and (intervals[i][1],iso_constant).
        If direction = 1, the parameter space iso interval connects the 2d points
        (iso_constant,intervals[i][0]) and (iso_constant,intervals[i][1]).
    """
    url = "rhino/geometry/brepface/trimawareisointervals-brepface_int_double"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, direction, constantParameter]
    if multiple: args = list(zip(thisBrepFace, direction, constantParameter))
    response = Util.ComputeFetch(url, args)
    return response


def TrimAwareIsoCurve(thisBrepFace, direction, constantParameter, multiple=False):
    """
    Similar to IsoCurve function, except this function pays attention to trims on faces
    and may return multiple curves.

    Args:
        direction (int): Direction of isocurve.
            0 = Isocurve connects all points with a constant U value.1 = Isocurve connects all points with a constant V value.
        constantParameter (double): Surface parameter that remains identical along the isocurves.

    Returns:
        Curve[]: Isoparametric curves connecting all points with the constantParameter value.
    """
    url = "rhino/geometry/brepface/trimawareisocurve-brepface_int_double"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, direction, constantParameter]
    if multiple: args = list(zip(thisBrepFace, direction, constantParameter))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def ChangeSurface(thisBrepFace, surfaceIndex, multiple=False):
    """
    Expert user tool that replaces the 3d surface geometry use by the face.

    Args:
        surfaceIndex (int): brep surface index of new surface.

    Returns:
        bool: True if successful.
    """
    url = "rhino/geometry/brepface/changesurface-brepface_int"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, surfaceIndex]
    if multiple: args = list(zip(thisBrepFace, surfaceIndex))
    response = Util.ComputeFetch(url, args)
    return response


def RebuildEdges(thisBrepFace, tolerance, rebuildSharedEdges, rebuildVertices, multiple=False):
    """
    Rebuild the edges used by a face so they lie on the surface.

    Args:
        tolerance (double): tolerance for fitting 3d edge curves.
        rebuildSharedEdges (bool): if False and edge is used by this face and a neighbor, then the edge
            will be skipped.
        rebuildVertices (bool): if true, vertex locations are updated to lie on the surface.

    Returns:
        bool: True on success.
    """
    url = "rhino/geometry/brepface/rebuildedges-brepface_double_bool_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrepFace, tolerance, rebuildSharedEdges, rebuildVertices]
    if multiple: args = list(zip(thisBrepFace, tolerance, rebuildSharedEdges, rebuildVertices))
    response = Util.ComputeFetch(url, args)
    return response

