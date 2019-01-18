from . import Util


def CurvePlane(curve, plane, tolerance, multiple=False):
    """
    Intersects a curve with an (infinite) plane.

    Args:
        curve (Curve): Curve to intersect.
        plane (Plane): Plane to intersect with.
        tolerance (double): Tolerance to use during intersection.

    Returns:
        CurveIntersections: A list of intersection events or None if no intersections were recorded.
    """
    url = "rhino/geometry/intersect/intersection/curveplane-curve_plane_double"
    if multiple: url += "?multiple=true"
    args = [curve, plane, tolerance]
    if multiple: args = zip(curve, plane, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MeshPlane(mesh, plane, multiple=False):
    """
    Intersects a mesh with an (infinite) plane.

    Args:
        mesh (Mesh): Mesh to intersect.
        plane (Plane): Plane to intersect with.

    Returns:
        Polyline[]: An array of polylines describing the intersection loops or None (Nothing in Visual Basic) if no intersections could be found.
    """
    url = "rhino/geometry/intersect/intersection/meshplane-mesh_plane"
    if multiple: url += "?multiple=true"
    args = [mesh, plane]
    if multiple: args = zip(mesh, plane)
    response = Util.ComputeFetch(url, args)
    return response


def MeshPlane1(mesh, planes, multiple=False):
    """
    Intersects a mesh with a collection of (infinite) planes.

    Args:
        mesh (Mesh): Mesh to intersect.
        planes (IEnumerable<Plane>): Planes to intersect with.

    Returns:
        Polyline[]: An array of polylines describing the intersection loops or None (Nothing in Visual Basic) if no intersections could be found.
    """
    url = "rhino/geometry/intersect/intersection/meshplane-mesh_planearray"
    if multiple: url += "?multiple=true"
    args = [mesh, planes]
    if multiple: args = zip(mesh, planes)
    response = Util.ComputeFetch(url, args)
    return response


def CurveSelf(curve, tolerance, multiple=False):
    """
    Finds the places where a curve intersects itself.

    Args:
        curve (Curve): Curve for self-intersections.
        tolerance (double): Intersection tolerance. If the curve approaches itself to within tolerance,
            an intersection is assumed.

    Returns:
        CurveIntersections: A collection of intersection events.
    """
    url = "rhino/geometry/intersect/intersection/curveself-curve_double"
    if multiple: url += "?multiple=true"
    args = [curve, tolerance]
    if multiple: args = zip(curve, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveCurve(curveA, curveB, tolerance, overlapTolerance, multiple=False):
    """
    Finds the intersections between two curves.

    Args:
        curveA (Curve): First curve for intersection.
        curveB (Curve): Second curve for intersection.
        tolerance (double): Intersection tolerance. If the curves approach each other to within tolerance,
            an intersection is assumed.
        overlapTolerance (double): The tolerance with which the curves are tested.

    Returns:
        CurveIntersections: A collection of intersection events.
    """
    url = "rhino/geometry/intersect/intersection/curvecurve-curve_curve_double_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, tolerance, overlapTolerance]
    if multiple: args = zip(curveA, curveB, tolerance, overlapTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveLine(curve, line, tolerance, overlapTolerance, multiple=False):
    """
    Intersects a curve and an infinite line.

    Args:
        curve (Curve): Curve for intersection.
        line (Line): Infinite line to intesect.
        tolerance (double): Intersection tolerance. If the curves approach each other to within tolerance,
            an intersection is assumed.
        overlapTolerance (double): The tolerance with which the curves are tested.

    Returns:
        CurveIntersections: A collection of intersection events.
    """
    url = "rhino/geometry/intersect/intersection/curveline-curve_line_double_double"
    if multiple: url += "?multiple=true"
    args = [curve, line, tolerance, overlapTolerance]
    if multiple: args = zip(curve, line, tolerance, overlapTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveSurface(curve, surface, tolerance, overlapTolerance, multiple=False):
    """
    Intersects a curve and a surface.

    Args:
        curve (Curve): Curve for intersection.
        surface (Surface): Surface for intersection.
        tolerance (double): Intersection tolerance. If the curve approaches the surface to within tolerance,
            an intersection is assumed.
        overlapTolerance (double): The tolerance with which the curves are tested.

    Returns:
        CurveIntersections: A collection of intersection events.
    """
    url = "rhino/geometry/intersect/intersection/curvesurface-curve_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [curve, surface, tolerance, overlapTolerance]
    if multiple: args = zip(curve, surface, tolerance, overlapTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveSurface1(curve, curveDomain, surface, tolerance, overlapTolerance, multiple=False):
    """
    Intersects a (sub)curve and a surface.

    Args:
        curve (Curve): Curve for intersection.
        curveDomain (Interval): Domain of surbcurve to take into consideration for Intersections.
        surface (Surface): Surface for intersection.
        tolerance (double): Intersection tolerance. If the curve approaches the surface to within tolerance,
            an intersection is assumed.
        overlapTolerance (double): The tolerance with which the curves are tested.

    Returns:
        CurveIntersections: A collection of intersection events.
    """
    url = "rhino/geometry/intersect/intersection/curvesurface-curve_interval_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [curve, curveDomain, surface, tolerance, overlapTolerance]
    if multiple: args = zip(curve, curveDomain, surface, tolerance, overlapTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveBrep(curve, brep, tolerance, angleTolerance, t, multiple=False):
    url = "rhino/geometry/intersect/intersection/curvebrep-curve_brep_double_double_doublearray"
    if multiple: url += "?multiple=true"
    args = [curve, brep, tolerance, angleTolerance, t]
    if multiple: args = zip(curve, brep, tolerance, angleTolerance, t)
    response = Util.ComputeFetch(url, args)
    return response


def MeshMeshFast(meshA, meshB, multiple=False):
    """
    Quickly intersects two meshes. Overlaps and near misses are ignored.

    Args:
        meshA (Mesh): First mesh for intersection.
        meshB (Mesh): Second mesh for intersection.

    Returns:
        Line[]: An array of intersection line segments, or null.
    """
    url = "rhino/geometry/intersect/intersection/meshmeshfast-mesh_mesh"
    if multiple: url += "?multiple=true"
    args = [meshA, meshB]
    if multiple: args = zip(meshA, meshB)
    response = Util.ComputeFetch(url, args)
    return response


def MeshMesh(meshes, tolerance, mode, multiple=False):
    """
    Intersects two meshes. Overlaps and perforations are handled in the output list.

    Args:
        meshes (IEnumerable<Mesh>): The mesh input list. It cannot be null.
        tolerance (double): A tolerance value. If negative, the positive value will be used.
        mode (SetsCombinations): The required working mode.

    Returns:
        Polyline[]: An array of both intersects, and overlaps.
    """
    url = "rhino/geometry/intersect/intersection/meshmesh-mesharray_double_setscombinations"
    if multiple: url += "?multiple=true"
    args = [meshes, tolerance, mode]
    if multiple: args = zip(meshes, tolerance, mode)
    response = Util.ComputeFetch(url, args)
    return response


def MeshMeshAccurate(meshA, meshB, tolerance, multiple=False):
    """
    Intersects two meshes. Overlaps and near misses are handled.

    Args:
        meshA (Mesh): First mesh for intersection.
        meshB (Mesh): Second mesh for intersection.
        tolerance (double): Intersection tolerance.

    Returns:
        Polyline[]: An array of intersection polylines.
    """
    url = "rhino/geometry/intersect/intersection/meshmeshaccurate-mesh_mesh_double"
    if multiple: url += "?multiple=true"
    args = [meshA, meshB, tolerance]
    if multiple: args = zip(meshA, meshB, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MeshRay(mesh, ray, multiple=False):
    """
    Finds the first intersection of a ray with a mesh.

    Args:
        mesh (Mesh): A mesh to intersect.
        ray (Ray3d): A ray to be casted.

    Returns:
        double: >= 0.0 parameter along ray if successful.
        < 0.0 if no intersection found.
    """
    url = "rhino/geometry/intersect/intersection/meshray-mesh_ray3d"
    if multiple: url += "?multiple=true"
    args = [mesh, ray]
    if multiple: args = zip(mesh, ray)
    response = Util.ComputeFetch(url, args)
    return response


def MeshRay1(mesh, ray, meshFaceIndices, multiple=False):
    """
    Finds the first intersection of a ray with a mesh.

    Args:
        mesh (Mesh): A mesh to intersect.
        ray (Ray3d): A ray to be casted.
        meshFaceIndices (int[]): faces on mesh that ray intersects.

    Returns:
        double: >= 0.0 parameter along ray if successful.
        < 0.0 if no intersection found.
    """
    url = "rhino/geometry/intersect/intersection/meshray-mesh_ray3d_intarray"
    if multiple: url += "?multiple=true"
    args = [mesh, ray, meshFaceIndices]
    if multiple: args = zip(mesh, ray, meshFaceIndices)
    response = Util.ComputeFetch(url, args)
    return response


def MeshPolyline(mesh, curve, faceIds, multiple=False):
    """
    Finds the intersection of a mesh and a polyline.

    Args:
        mesh (Mesh): A mesh to intersect.
        curve (PolylineCurve): A polyline curves to intersect.
        faceIds (int[]): The indices of the intersecting faces. This out reference is assigned during the call.

    Returns:
        Point3d[]: An array of points: one for each face that was passed by the faceIds out reference.
    """
    url = "rhino/geometry/intersect/intersection/meshpolyline-mesh_polylinecurve_intarray"
    if multiple: url += "?multiple=true"
    args = [mesh, curve, faceIds]
    if multiple: args = zip(mesh, curve, faceIds)
    response = Util.ComputeFetch(url, args)
    return response


def MeshLine(mesh, line, faceIds, multiple=False):
    """
    Finds the intersection of a mesh and a line

    Args:
        mesh (Mesh): A mesh to intersect
        line (Line): The line to intersect with the mesh
        faceIds (int[]): The indices of the intersecting faces. This out reference is assigned during the call.

    Returns:
        Point3d[]: An array of points: one for each face that was passed by the faceIds out reference.
    """
    url = "rhino/geometry/intersect/intersection/meshline-mesh_line_intarray"
    if multiple: url += "?multiple=true"
    args = [mesh, line, faceIds]
    if multiple: args = zip(mesh, line, faceIds)
    response = Util.ComputeFetch(url, args)
    return response


def RayShoot(ray, geometry, maxReflections, multiple=False):
    """
    Computes point intersections that occur when shooting a ray to a collection of surfaces.

    Args:
        ray (Ray3d): A ray used in intersection.
        geometry (IEnumerable<GeometryBase>): Only Surface and Brep objects are currently supported. Trims are ignored on Breps.
        maxReflections (int): The maximum number of reflections. This value should be any value between 1 and 1000, inclusive.

    Returns:
        Point3d[]: An array of points: one for each face that was passed by the faceIds out reference.
    """
    url = "rhino/geometry/intersect/intersection/rayshoot-ray3d_geometrybasearray_int"
    if multiple: url += "?multiple=true"
    args = [ray, geometry, maxReflections]
    if multiple: args = zip(ray, geometry, maxReflections)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectPointsToMeshes(meshes, points, direction, tolerance, multiple=False):
    """
    Projects points onto meshes.

    Args:
        meshes (IEnumerable<Mesh>): the meshes to project on to.
        points (IEnumerable<Point3d>): the points to project.
        direction (Vector3d): the direction to project.
        tolerance (double): Projection tolerances used for culling close points and for line-mesh intersection.

    Returns:
        Point3d[]: Array of projected points, or None in case of any error or invalid input.
    """
    url = "rhino/geometry/intersect/intersection/projectpointstomeshes-mesharray_point3darray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [meshes, points, direction, tolerance]
    if multiple: args = zip(meshes, points, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectPointsToMeshesEx(meshes, points, direction, tolerance, indices, multiple=False):
    """
    Projects points onto meshes.

    Args:
        meshes (IEnumerable<Mesh>): the meshes to project on to.
        points (IEnumerable<Point3d>): the points to project.
        direction (Vector3d): the direction to project.
        tolerance (double): Projection tolerances used for culling close points and for line-mesh intersection.
        indices (int[]): Return points[i] is a projection of points[indices[i]]

    Returns:
        Point3d[]: Array of projected points, or None in case of any error or invalid input.
    """
    url = "rhino/geometry/intersect/intersection/projectpointstomeshesex-mesharray_point3darray_vector3d_double_intarray"
    if multiple: url += "?multiple=true"
    args = [meshes, points, direction, tolerance, indices]
    if multiple: args = zip(meshes, points, direction, tolerance, indices)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectPointsToBreps(breps, points, direction, tolerance, multiple=False):
    """
    Projects points onto breps.

    Args:
        breps (IEnumerable<Brep>): The breps projection targets.
        points (IEnumerable<Point3d>): The points to project.
        direction (Vector3d): The direction to project.
        tolerance (double): The tolerance used for intersections.

    Returns:
        Point3d[]: Array of projected points, or None in case of any error or invalid input.
    """
    url = "rhino/geometry/intersect/intersection/projectpointstobreps-breparray_point3darray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [breps, points, direction, tolerance]
    if multiple: args = zip(breps, points, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectPointsToBrepsEx(breps, points, direction, tolerance, indices, multiple=False):
    """
    Projects points onto breps.

    Args:
        breps (IEnumerable<Brep>): The breps projection targets.
        points (IEnumerable<Point3d>): The points to project.
        direction (Vector3d): The direction to project.
        tolerance (double): The tolerance used for intersections.
        indices (int[]): Return points[i] is a projection of points[indices[i]]

    Returns:
        Point3d[]: Array of projected points, or None in case of any error or invalid input.
    """
    url = "rhino/geometry/intersect/intersection/projectpointstobrepsex-breparray_point3darray_vector3d_double_intarray"
    if multiple: url += "?multiple=true"
    args = [breps, points, direction, tolerance, indices]
    if multiple: args = zip(breps, points, direction, tolerance, indices)
    response = Util.ComputeFetch(url, args)
    return response

