from . import Util
try:
    from itertools import izip as zip # python 2
except ImportError:
    pass # python 3


def CreateFromPlane(plane, xInterval, yInterval, xCount, yCount, multiple=False):
    """
    Constructs a planar mesh grid.

    Args:
        plane (Plane): Plane of mesh.
        xInterval (Interval): Interval describing size and extends of mesh along plane x-direction.
        yInterval (Interval): Interval describing size and extends of mesh along plane y-direction.
        xCount (int): Number of faces in x-direction.
        yCount (int): Number of faces in y-direction.
    """
    url = "rhino/geometry/mesh/createfromplane-plane_interval_interval_int_int"
    if multiple: url += "?multiple=true"
    args = [plane, xInterval, yInterval, xCount, yCount]
    if multiple: args = list(zip(plane, xInterval, yInterval, xCount, yCount))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromFilteredFaceList(original, inclusion, multiple=False):
    """
    Constructs a sub-mesh, that contains a filtered list of faces.

    Args:
        original (Mesh): The mesh to copy. This item can be null, and in this case an empty mesh is returned.
        inclusion (IEnumerable<bool>): A series of True and False values, that determine if each face is used in the new mesh.
            If the amount does not match the length of the face list, the pattern is repeated. If it exceeds the amount
            of faces in the mesh face list, the pattern is truncated. This items can be None or empty, and the mesh will simply be duplicated.
    """
    url = "rhino/geometry/mesh/createfromfilteredfacelist-mesh_boolarray"
    if multiple: url += "?multiple=true"
    args = [original, inclusion]
    if multiple: args = list(zip(original, inclusion))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromBox(box, xCount, yCount, zCount, multiple=False):
    """
    Constructs new mesh that matches a bounding box.

    Args:
        box (BoundingBox): A box to use for creation.
        xCount (int): Number of faces in x-direction.
        yCount (int): Number of faces in y-direction.
        zCount (int): Number of faces in z-direction.

    Returns:
        Mesh: A new brep, or None on failure.
    """
    url = "rhino/geometry/mesh/createfrombox-boundingbox_int_int_int"
    if multiple: url += "?multiple=true"
    args = [box, xCount, yCount, zCount]
    if multiple: args = list(zip(box, xCount, yCount, zCount))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromBox1(box, xCount, yCount, zCount, multiple=False):
    """
    Constructs new mesh that matches an aligned box.

    Args:
        box (Box): Box to match.
        xCount (int): Number of faces in x-direction.
        yCount (int): Number of faces in y-direction.
        zCount (int): Number of faces in z-direction.
    """
    url = "rhino/geometry/mesh/createfrombox-box_int_int_int"
    if multiple: url += "?multiple=true"
    args = [box, xCount, yCount, zCount]
    if multiple: args = list(zip(box, xCount, yCount, zCount))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromBox2(corners, xCount, yCount, zCount, multiple=False):
    """
    Constructs new mesh from 8 corner points.

    Args:
        corners (IEnumerable<Point3d>): 8 points defining the box corners arranged as the vN labels indicate.
            v7_____________v6|\             |\| \            | \|  \ _____________\|   v4         |   v5|   |          |   ||   |          |   |v3--|----------v2  | \  |           \  |  \ |            \ |   \|             \|    v0_____________v1
        xCount (int): Number of faces in x-direction.
        yCount (int): Number of faces in y-direction.
        zCount (int): Number of faces in z-direction.

    Returns:
        Mesh: A new brep, or None on failure.

    Returns:
        Mesh: A new box mesh, on None on error.
    """
    url = "rhino/geometry/mesh/createfrombox-point3darray_int_int_int"
    if multiple: url += "?multiple=true"
    args = [corners, xCount, yCount, zCount]
    if multiple: args = list(zip(corners, xCount, yCount, zCount))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromSphere(sphere, xCount, yCount, multiple=False):
    """
    Constructs a mesh sphere.

    Args:
        sphere (Sphere): Base sphere for mesh.
        xCount (int): Number of faces in the around direction.
        yCount (int): Number of faces in the top-to-bottom direction.
    """
    url = "rhino/geometry/mesh/createfromsphere-sphere_int_int"
    if multiple: url += "?multiple=true"
    args = [sphere, xCount, yCount]
    if multiple: args = list(zip(sphere, xCount, yCount))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateIcoSphere(sphere, subdivisions, multiple=False):
    """
    Constructs a icospherical mesh. A mesh icosphere differs from a standard
    UV mesh sphere in that it's vertices are evenly distributed. A mesh icosphere
    starts from an icosahedron (a regular polyhedron with 20 equilateral triangles).
    It is then refined by splitting each triangle into 4 smaller triangles.
    This splitting can be done several times.

    Args:
        sphere (Sphere): The input sphere provides the orienting plane and radius.
        subdivisions (int): The number of times you want the faces split, where 0  <= subdivisions <= 7.
            Note, the total number of mesh faces produces is: 20 * (4 ^ subdivisions)

    Returns:
        Mesh: A welded mesh icosphere if successful, or None on failure.
    """
    url = "rhino/geometry/mesh/createicosphere-sphere_int"
    if multiple: url += "?multiple=true"
    args = [sphere, subdivisions]
    if multiple: args = list(zip(sphere, subdivisions))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateQuadSphere(sphere, subdivisions, multiple=False):
    """
    Constructs a quad mesh sphere. A quad mesh sphere differs from a standard
    UV mesh sphere in that it's vertices are evenly distributed. A quad mesh sphere
    starts from a cube (a regular polyhedron with 6 square sides).
    It is then refined by splitting each quad into 4 smaller quads.
    This splitting can be done several times.

    Args:
        sphere (Sphere): The input sphere provides the orienting plane and radius.
        subdivisions (int): The number of times you want the faces split, where 0  <= subdivisions <= 8.
            Note, the total number of mesh faces produces is: 6 * (4 ^ subdivisions)

    Returns:
        Mesh: A welded quad mesh sphere if successful, or None on failure.
    """
    url = "rhino/geometry/mesh/createquadsphere-sphere_int"
    if multiple: url += "?multiple=true"
    args = [sphere, subdivisions]
    if multiple: args = list(zip(sphere, subdivisions))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromCylinder(cylinder, vertical, around, multiple=False):
    """
    Constructs a capped mesh cylinder.

    Args:
        vertical (int): Number of faces in the top-to-bottom direction.
        around (int): Number of faces around the cylinder.

    Returns:
        Mesh: Returns a mesh cylinder if successful, None otherwise.
    """
    url = "rhino/geometry/mesh/createfromcylinder-cylinder_int_int"
    if multiple: url += "?multiple=true"
    args = [cylinder, vertical, around]
    if multiple: args = list(zip(cylinder, vertical, around))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromCylinder1(cylinder, vertical, around, capBottom, capTop, multiple=False):
    """
    Constructs a mesh cylinder.

    Args:
        vertical (int): Number of faces in the top-to-bottom direction.
        around (int): Number of faces around the cylinder.
        capBottom (bool): If True end at Cylinder.Height1 should be capped.
        capTop (bool): If True end at Cylinder.Height2 should be capped.

    Returns:
        Mesh: Returns a mesh cylinder if successful, None otherwise.
    """
    url = "rhino/geometry/mesh/createfromcylinder-cylinder_int_int_bool_bool"
    if multiple: url += "?multiple=true"
    args = [cylinder, vertical, around, capBottom, capTop]
    if multiple: args = list(zip(cylinder, vertical, around, capBottom, capTop))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromCylinder2(cylinder, vertical, around, capBottom, capTop, quadCaps, multiple=False):
    """
    Constructs a mesh cylinder.

    Args:
        vertical (int): Number of faces in the top-to-bottom direction.
        around (int): Number of faces around the cylinder.
        capBottom (bool): If True end at Cylinder.Height1 should be capped.
        capTop (bool): If True end at Cylinder.Height2 should be capped.
        quadCaps (bool): If True and it's possible to make quad caps, i.e.. around is even, then caps will have quad faces.

    Returns:
        Mesh: Returns a mesh cylinder if successful, None otherwise.
    """
    url = "rhino/geometry/mesh/createfromcylinder-cylinder_int_int_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [cylinder, vertical, around, capBottom, capTop, quadCaps]
    if multiple: args = list(zip(cylinder, vertical, around, capBottom, capTop, quadCaps))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromCone(cone, vertical, around, multiple=False):
    """
    Constructs a solid mesh cone.

    Args:
        vertical (int): Number of faces in the top-to-bottom direction.
        around (int): Number of faces around the cone.

    Returns:
        Mesh: A valid mesh if successful.
    """
    url = "rhino/geometry/mesh/createfromcone-cone_int_int"
    if multiple: url += "?multiple=true"
    args = [cone, vertical, around]
    if multiple: args = list(zip(cone, vertical, around))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromCone1(cone, vertical, around, solid, multiple=False):
    """
    Constructs a mesh cone.

    Args:
        vertical (int): Number of faces in the top-to-bottom direction.
        around (int): Number of faces around the cone.
        solid (bool): If False the mesh will be open with no faces on the circular planar portion.

    Returns:
        Mesh: A valid mesh if successful.
    """
    url = "rhino/geometry/mesh/createfromcone-cone_int_int_bool"
    if multiple: url += "?multiple=true"
    args = [cone, vertical, around, solid]
    if multiple: args = list(zip(cone, vertical, around, solid))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromCone2(cone, vertical, around, solid, quadCaps, multiple=False):
    """
    Constructs a mesh cone.

    Args:
        vertical (int): Number of faces in the top-to-bottom direction.
        around (int): Number of faces around the cone.
        solid (bool): If False the mesh will be open with no faces on the circular planar portion.
        quadCaps (bool): If True and it's possible to make quad caps, i.e.. around is even, then caps will have quad faces.

    Returns:
        Mesh: A valid mesh if successful.
    """
    url = "rhino/geometry/mesh/createfromcone-cone_int_int_bool_bool"
    if multiple: url += "?multiple=true"
    args = [cone, vertical, around, solid, quadCaps]
    if multiple: args = list(zip(cone, vertical, around, solid, quadCaps))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromTorus(torus, vertical, around, multiple=False):
    """
    Constructs a mesh torus.

    Args:
        torus (Torus): The torus.
        vertical (int): Number of faces in the top-to-bottom direction.
        around (int): Number of faces around the torus.

    Returns:
        Mesh: Returns a mesh torus if successful, None otherwise.
    """
    url = "rhino/geometry/mesh/createfromtorus-torus_int_int"
    if multiple: url += "?multiple=true"
    args = [torus, vertical, around]
    if multiple: args = list(zip(torus, vertical, around))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromPlanarBoundary(boundary, parameters, multiple=False):
    """
    Do not use this overload. Use version that takes a tolerance parameter instead.

    Args:
        boundary (Curve): Do not use.
        parameters (MeshingParameters): Do not use.

    Returns:
        Mesh: Do not use.
    """
    url = "rhino/geometry/mesh/createfromplanarboundary-curve_meshingparameters"
    if multiple: url += "?multiple=true"
    args = [boundary, parameters]
    if multiple: args = list(zip(boundary, parameters))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromPlanarBoundary1(boundary, parameters, tolerance, multiple=False):
    """
    Attempts to construct a mesh from a closed planar curve.RhinoMakePlanarMeshes

    Args:
        boundary (Curve): must be a closed planar curve.
        parameters (MeshingParameters): parameters used for creating the mesh.
        tolerance (double): Tolerance to use during operation.

    Returns:
        Mesh: New mesh on success or None on failure.
    """
    url = "rhino/geometry/mesh/createfromplanarboundary-curve_meshingparameters_double"
    if multiple: url += "?multiple=true"
    args = [boundary, parameters, tolerance]
    if multiple: args = list(zip(boundary, parameters, tolerance))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromClosedPolyline(polyline, multiple=False):
    """
    Attempts to create a Mesh that is a triangulation of a simple closed polyline that projects onto a plane.

    Args:
        polyline (Polyline): must be closed

    Returns:
        Mesh: New mesh on success or None on failure.
    """
    url = "rhino/geometry/mesh/createfromclosedpolyline-polyline"
    if multiple: url += "?multiple=true"
    args = [polyline]
    if multiple: args = [[item] for item in polyline]
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromTessellation(points, edges, plane, allowNewVertices, multiple=False):
    """
    Attempts to create a mesh that is a triangulation of a list of points, projected on a plane,
    including its holes and fixed edges.

    Args:
        points (IEnumerable<Point3d>): A list, an array or any enumerable of points.
        plane (Plane): A plane.
        allowNewVertices (bool): If true, the mesh might have more vertices than the list of input points,
            if doing so will improve long thin triangles.
        edges (IEnumerable<IEnumerable<Point3d>>): A list of polylines, or other lists of points representing edges.
            This can be null. If nested enumerable items are null, they will be discarded.

    Returns:
        Mesh: A new mesh, or None if not successful.
    """
    url = "rhino/geometry/mesh/createfromtessellation-point3darray_ienumerable<point3d>array_plane_bool"
    if multiple: url += "?multiple=true"
    args = [points, edges, plane, allowNewVertices]
    if multiple: args = list(zip(points, edges, plane, allowNewVertices))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromBrep(brep, multiple=False):
    """
    Constructs a mesh from a brep.

    Args:
        brep (Brep): Brep to approximate.

    Returns:
        Mesh[]: An array of meshes.
    """
    url = "rhino/geometry/mesh/createfrombrep-brep"
    if multiple: url += "?multiple=true"
    args = [brep]
    if multiple: args = [[item] for item in brep]
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromBrep1(brep, meshingParameters, multiple=False):
    """
    Constructs a mesh from a brep.

    Args:
        brep (Brep): Brep to approximate.
        meshingParameters (MeshingParameters): Parameters to use during meshing.

    Returns:
        Mesh[]: An array of meshes.
    """
    url = "rhino/geometry/mesh/createfrombrep-brep_meshingparameters"
    if multiple: url += "?multiple=true"
    args = [brep, meshingParameters]
    if multiple: args = list(zip(brep, meshingParameters))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromSurface(surface, multiple=False):
    """
    Constructs a mesh from a surface

    Args:
        surface (Surface): Surface to approximate

    Returns:
        Mesh: New mesh representing the surface
    """
    url = "rhino/geometry/mesh/createfromsurface-surface"
    if multiple: url += "?multiple=true"
    args = [surface]
    if multiple: args = [[item] for item in surface]
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromSurface1(surface, meshingParameters, multiple=False):
    """
    Constructs a mesh from a surface

    Args:
        surface (Surface): Surface to approximate
        meshingParameters (MeshingParameters): settings used to create the mesh

    Returns:
        Mesh: New mesh representing the surface
    """
    url = "rhino/geometry/mesh/createfromsurface-surface_meshingparameters"
    if multiple: url += "?multiple=true"
    args = [surface, meshingParameters]
    if multiple: args = list(zip(surface, meshingParameters))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromSubD(subd, displayDensity, multiple=False):
    """
    Create a mesh from a SubD limit surface
    """
    url = "rhino/geometry/mesh/createfromsubd-subd_int"
    if multiple: url += "?multiple=true"
    args = [subd, displayDensity]
    if multiple: args = list(zip(subd, displayDensity))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreatePatch(outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions, multiple=False):
    """
    Construct a mesh patch from a variety of input geometry.

    Args:
        outerBoundary (Polyline): (optional: can be null) Outer boundary
            polyline, if provided this will become the outer boundary of the
            resulting mesh. Any of the input that is completely outside the outer
            boundary will be ignored and have no impact on the result. If any of
            the input intersects the outer boundary the result will be
            unpredictable and is likely to not include the entire outer boundary.
        angleToleranceRadians (double): Maximum angle between unit tangents and adjacent vertices. Used to
            divide curve inputs that cannot otherwise be represented as a polyline.
        innerBoundaryCurves (IEnumerable<Curve>): (optional: can be null) Polylines to create holes in the output mesh.
            If innerBoundaryCurves are the only input then the result may be null
            if trimback is set to False (see comments for trimback) because the
            resulting mesh could be invalid (all faces created contained vertexes
            from the perimeter boundary).
        pullbackSurface (Surface): (optional: can be null) Initial surface where 3d input will be pulled
            to make a 2d representation used by the function that generates the mesh.
            Providing a pullbackSurface can be helpful when it is similar in shape
            to the pattern of the input, the pulled 2d points will be a better
            representation of the 3d points. If all of the input is more or less
            coplanar to start with, providing pullbackSurface has no real benefit.
        innerBothSideCurves (IEnumerable<Curve>): (optional: can be null) These polylines will create faces on both sides
            of the edge. If there are only input points(innerPoints) there is no
            way to guarantee a triangulation that will create an edge between two
            particular points. Adding a line, or polyline, to innerBothsideCurves
            that includes points from innerPoints will help guide the triangulation.
        innerPoints (IEnumerable<Point3d>): (optional: can be null) Points to be used to generate the mesh. If
            outerBoundary is not null, points outside of that boundary after it has
            been pulled to pullbackSurface (or the best plane through the input if
            pullbackSurface is null) will be ignored.
        trimback (bool): Only used when a outerBoundary has not been provided. When that is the
            case, the function uses the perimeter of the surface as the outer boundary
            instead. If true, any face of the resulting triangulated mesh that
            contains a vertex of the perimeter boundary will be removed.
        divisions (int): Only used when a outerBoundary has not been provided. When that is the
            case, division becomes the number of divisions each side of the surface's
            perimeter will be divided into to create an outer boundary to work with.

    Returns:
        Mesh: mesh on success; None on failure
    """
    url = "rhino/geometry/mesh/createpatch-polyline_double_surface_curvearray_curvearray_point3darray_bool_int"
    if multiple: url += "?multiple=true"
    args = [outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions]
    if multiple: args = list(zip(outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateBooleanUnion(meshes, multiple=False):
    """
    Computes the solid union of a set of meshes.

    Args:
        meshes (IEnumerable<Mesh>): Meshes to union.

    Returns:
        Mesh[]: An array of Mesh results or None on failure.
    """
    url = "rhino/geometry/mesh/createbooleanunion-mesharray"
    if multiple: url += "?multiple=true"
    args = [meshes]
    if multiple: args = [[item] for item in meshes]
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateBooleanDifference(firstSet, secondSet, multiple=False):
    """
    Computes the solid difference of two sets of Meshes.

    Args:
        firstSet (IEnumerable<Mesh>): First set of Meshes (the set to subtract from).
        secondSet (IEnumerable<Mesh>): Second set of Meshes (the set to subtract).

    Returns:
        Mesh[]: An array of Mesh results or None on failure.
    """
    url = "rhino/geometry/mesh/createbooleandifference-mesharray_mesharray"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet]
    if multiple: args = list(zip(firstSet, secondSet))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateBooleanIntersection(firstSet, secondSet, multiple=False):
    """
    Computes the solid intersection of two sets of meshes.

    Args:
        firstSet (IEnumerable<Mesh>): First set of Meshes.
        secondSet (IEnumerable<Mesh>): Second set of Meshes.

    Returns:
        Mesh[]: An array of Mesh results or None on failure.
    """
    url = "rhino/geometry/mesh/createbooleanintersection-mesharray_mesharray"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet]
    if multiple: args = list(zip(firstSet, secondSet))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateBooleanSplit(meshesToSplit, meshSplitters, multiple=False):
    """
    Splits a set of meshes with another set.

    Args:
        meshesToSplit (IEnumerable<Mesh>): A list, an array, or any enumerable set of meshes to be split. If this is null, None will be returned.
        meshSplitters (IEnumerable<Mesh>): A list, an array, or any enumerable set of meshes that cut. If this is null, None will be returned.

    Returns:
        Mesh[]: A new mesh array, or None on error.
    """
    url = "rhino/geometry/mesh/createbooleansplit-mesharray_mesharray"
    if multiple: url += "?multiple=true"
    args = [meshesToSplit, meshSplitters]
    if multiple: args = list(zip(meshesToSplit, meshSplitters))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromCurvePipe(curve, radius, segments, accuracy, capType, faceted, intervals, multiple=False):
    """
    Constructs a new mesh pipe from a curve.

    Args:
        curve (Curve): A curve to pipe.
        radius (double): The radius of the pipe.
        segments (int): The number of segments in the pipe.
        accuracy (int): The accuracy of the pipe.
        capType (MeshPipeCapStyle): The type of cap to be created at the end of the pipe.
        faceted (bool): Specifies whether the pipe is faceted, or not.
        intervals (IEnumerable<Interval>): A series of intervals to pipe. This value can be null.

    Returns:
        Mesh: A new mesh, or None on failure.
    """
    url = "rhino/geometry/mesh/createfromcurvepipe-curve_double_int_int_meshpipecapstyle_bool_intervalarray"
    if multiple: url += "?multiple=true"
    args = [curve, radius, segments, accuracy, capType, faceted, intervals]
    if multiple: args = list(zip(curve, radius, segments, accuracy, capType, faceted, intervals))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromCurveExtrusion(curve, direction, parameters, boundingBox, multiple=False):
    """
    Constructs a new extrusion from a curve.

    Args:
        curve (Curve): A curve to extrude.
        direction (Vector3d): The direction of extrusion.
        parameters (MeshingParameters): The parameters of meshing.
        boundingBox (BoundingBox): The bounding box controls the length of the extrusion.

    Returns:
        Mesh: A new mesh, or None on failure.
    """
    url = "rhino/geometry/mesh/createfromcurveextrusion-curve_vector3d_meshingparameters_boundingbox"
    if multiple: url += "?multiple=true"
    args = [curve, direction, parameters, boundingBox]
    if multiple: args = list(zip(curve, direction, parameters, boundingBox))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateFromIterativeCleanup(meshes, tolerance, multiple=False):
    """
    Repairs meshes with vertices that are too near, using a tolerance value.

    Args:
        meshes (IEnumerable<Mesh>): The meshes to be repaired.
        tolerance (double): A minimum distance for clean vertices.

    Returns:
        Mesh[]: A valid meshes array if successful. If no change was required, some meshes can be null. Otherwise, null, when no changes were done.
    """
    url = "rhino/geometry/mesh/createfromiterativecleanup-mesharray_double"
    if multiple: url += "?multiple=true"
    args = [meshes, tolerance]
    if multiple: args = list(zip(meshes, tolerance))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def RequireIterativeCleanup(meshes, tolerance, multiple=False):
    """
    Analyzes some meshes, and determines if a pass of CreateFromIterativeCleanup would change the array.
    All available cleanup steps are used. Currently available cleanup steps are:- mending of single precision coincidence even though double precision vertices differ.- union of nearly identical vertices, irrespectively of their origin.- removal of t-joints along edges.

    Args:
        meshes (IEnumerable<Mesh>): A list, and array or any enumerable of meshes.
        tolerance (double): A 3d distance. This is usually a value of about 10e-7 magnitude.

    Returns:
        bool: True if meshes would be changed, otherwise false.
    """
    url = "rhino/geometry/mesh/requireiterativecleanup-mesharray_double"
    if multiple: url += "?multiple=true"
    args = [meshes, tolerance]
    if multiple: args = list(zip(meshes, tolerance))
    response = Util.ComputeFetch(url, args)
    return response


def Volume(thisMesh, multiple=False):
    """
    Compute volume of the mesh.

    Returns:
        double: Volume of the mesh.
    """
    url = "rhino/geometry/mesh/volume-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = [[item] for item in thisMesh]
    response = Util.ComputeFetch(url, args)
    return response


def IsPointInside(thisMesh, point, tolerance, strictlyIn, multiple=False):
    """
    Determines if a point is inside a solid mesh.

    Args:
        point (Point3d): 3d point to test.
        tolerance (double): (>=0) 3d distance tolerance used for ray-mesh intersection
            and determining strict inclusion.
        strictlyIn (bool): If strictlyIn is true, then point must be inside mesh by at least
            tolerance in order for this function to return true.
            If strictlyIn is false, then this function will return True if
            point is inside or the distance from point to a mesh face is <= tolerance.

    Returns:
        bool: True if point is inside the solid mesh, False if not.
    """
    url = "rhino/geometry/mesh/ispointinside-mesh_point3d_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, point, tolerance, strictlyIn]
    if multiple: args = list(zip(thisMesh, point, tolerance, strictlyIn))
    response = Util.ComputeFetch(url, args)
    return response


def Smooth(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, multiple=False):
    """
    Smooths a mesh by averaging the positions of mesh vertices in a specified region.

    Args:
        smoothFactor (double): The smoothing factor, which controls how much vertices move towards the average of the neighboring vertices.
        bXSmooth (bool): When True vertices move in X axis direction.
        bYSmooth (bool): When True vertices move in Y axis direction.
        bZSmooth (bool): When True vertices move in Z axis direction.
        bFixBoundaries (bool): When True vertices along naked edges will not be modified.
        coordinateSystem (SmoothingCoordinateSystem): The coordinates to determine the direction of the smoothing.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/mesh/smooth-mesh_double_bool_bool_bool_bool_smoothingcoordinatesystem"
    if multiple: url += "?multiple=true"
    args = [thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem]
    if multiple: args = list(zip(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem))
    response = Util.ComputeFetch(url, args)
    return response


def Smooth1(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False):
    """
    Smooths a mesh by averaging the positions of mesh vertices in a specified region.

    Args:
        smoothFactor (double): The smoothing factor, which controls how much vertices move towards the average of the neighboring vertices.
        bXSmooth (bool): When True vertices move in X axis direction.
        bYSmooth (bool): When True vertices move in Y axis direction.
        bZSmooth (bool): When True vertices move in Z axis direction.
        bFixBoundaries (bool): When True vertices along naked edges will not be modified.
        coordinateSystem (SmoothingCoordinateSystem): The coordinates to determine the direction of the smoothing.
        plane (Plane): If SmoothingCoordinateSystem.CPlane specified, then the construction plane.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/mesh/smooth-mesh_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    if multiple: args = list(zip(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane))
    response = Util.ComputeFetch(url, args)
    return response


def Smooth2(thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False):
    """
    Smooths part of a mesh by averaging the positions of mesh vertices in a specified region.

    Args:
        vertexIndices (IEnumerable<int>): The mesh vertex indices that specify the part of the mesh to smooth.
        smoothFactor (double): The smoothing factor, which controls how much vertices move towards the average of the neighboring vertices.
        bXSmooth (bool): When True vertices move in X axis direction.
        bYSmooth (bool): When True vertices move in Y axis direction.
        bZSmooth (bool): When True vertices move in Z axis direction.
        bFixBoundaries (bool): When True vertices along naked edges will not be modified.
        coordinateSystem (SmoothingCoordinateSystem): The coordinates to determine the direction of the smoothing.
        plane (Plane): If SmoothingCoordinateSystem.CPlane specified, then the construction plane.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/mesh/smooth-mesh_intarray_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    if multiple: args = list(zip(thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane))
    response = Util.ComputeFetch(url, args)
    return response


def Unweld(thisMesh, angleToleranceRadians, modifyNormals, multiple=False):
    """
    Makes sure that faces sharing an edge and having a difference of normal greater
    than or equal to angleToleranceRadians have unique vertexes along that edge,
    adding vertices if necessary.

    Args:
        angleToleranceRadians (double): Angle at which to make unique vertices.
        modifyNormals (bool): Determines whether new vertex normals will have the same vertex normal as the original (false)
            or vertex normals made from the corresponding face normals (true)
    """
    url = "rhino/geometry/mesh/unweld-mesh_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, angleToleranceRadians, modifyNormals]
    if multiple: args = list(zip(thisMesh, angleToleranceRadians, modifyNormals))
    response = Util.ComputeFetch(url, args)
    return response


def UnweldEdge(thisMesh, edgeIndices, modifyNormals, multiple=False):
    """
    Adds creases to a smooth mesh by creating coincident vertices along selected edges.

    Args:
        edgeIndices (IEnumerable<int>): An array of mesh topology edge indices.
        modifyNormals (bool): If true, the vertex normals on each side of the edge take the same value as the face to which they belong, giving the mesh a hard edge look.
            If false, each of the vertex normals on either side of the edge is assigned the same value as the original normal that the pair is replacing, keeping a smooth look.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/mesh/unweldedge-mesh_intarray_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, edgeIndices, modifyNormals]
    if multiple: args = list(zip(thisMesh, edgeIndices, modifyNormals))
    response = Util.ComputeFetch(url, args)
    return response


def UnweldVertices(thisMesh, topologyVertexIndices, modifyNormals, multiple=False):
    """
    Ensures that faces sharing a common topological vertex have unique indices into the  collection.

    Args:
        topologyVertexIndices (IEnumerable<int>): Topological vertex indices, from the  collection, to be unwelded.
            Use  to convert from vertex indices to topological vertex indices.
        modifyNormals (bool): If true, the new vertex normals will be calculated from the face normal.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/mesh/unweldvertices-mesh_intarray_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, topologyVertexIndices, modifyNormals]
    if multiple: args = list(zip(thisMesh, topologyVertexIndices, modifyNormals))
    response = Util.ComputeFetch(url, args)
    return response


def Weld(thisMesh, angleToleranceRadians, multiple=False):
    """
    Makes sure that faces sharing an edge and having a difference of normal greater
    than or equal to angleToleranceRadians share vertexes along that edge, vertex normals
    are averaged.

    Args:
        angleToleranceRadians (double): Angle at which to weld vertices.
    """
    url = "rhino/geometry/mesh/weld-mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, angleToleranceRadians]
    if multiple: args = list(zip(thisMesh, angleToleranceRadians))
    response = Util.ComputeFetch(url, args)
    return response


def RebuildNormals(thisMesh, multiple=False):
    """
    Removes mesh normals and reconstructs the face and vertex normals based
    on the orientation of the faces.
    """
    url = "rhino/geometry/mesh/rebuildnormals-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = [[item] for item in thisMesh]
    response = Util.ComputeFetch(url, args)
    return response


def ExtractNonManifoldEdges(thisMesh, selective, multiple=False):
    """
    Extracts, or removes, non-manifold mesh edges.

    Args:
        selective (bool): If true, then extract hanging faces only.

    Returns:
        Mesh: A mesh containing the extracted non-manifold parts if successful, None otherwise.
    """
    url = "rhino/geometry/mesh/extractnonmanifoldedges-mesh_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, selective]
    if multiple: args = list(zip(thisMesh, selective))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def HealNakedEdges(thisMesh, distance, multiple=False):
    """
    Attempts to "heal" naked edges in a mesh based on a given distance.
    First attempts to move vertexes to neighboring vertexes that are within that
    distance away. Then it finds edges that have a closest point to the vertex within
    the distance and splits the edge. When it finds one it splits the edge and
    makes two new edges using that point.

    Args:
        distance (double): Distance to not exceed when modifying the mesh.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/mesh/healnakededges-mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance]
    if multiple: args = list(zip(thisMesh, distance))
    response = Util.ComputeFetch(url, args)
    return response


def FillHoles(thisMesh, multiple=False):
    """
    Attempts to determine "holes" in the mesh by chaining naked edges together.
    Then it triangulates the closed polygons adds the faces to the mesh.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/mesh/fillholes-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = [[item] for item in thisMesh]
    response = Util.ComputeFetch(url, args)
    return response


def FileHole(thisMesh, topologyEdgeIndex, multiple=False):
    """
    Given a starting "naked" edge index, this function attempts to determine a "hole"
    by chaining additional naked edges together until if returns to the start index.
    Then it triangulates the closed polygon and either adds the faces to the mesh.

    Args:
        topologyEdgeIndex (int): Starting naked edge index.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/mesh/filehole-mesh_int"
    if multiple: url += "?multiple=true"
    args = [thisMesh, topologyEdgeIndex]
    if multiple: args = list(zip(thisMesh, topologyEdgeIndex))
    response = Util.ComputeFetch(url, args)
    return response


def UnifyNormals(thisMesh, multiple=False):
    """
    Attempts to fix inconsistencies in the directions of mesh faces in a mesh. This function
    does not modify mesh vertex normals, it rearranges the mesh face winding and face
    normals to make them all consistent. Note, you may want to call Mesh.Normals.ComputeNormals()
    to recompute vertex normals after calling this functions.

    Returns:
        int: number of faces that were modified.
    """
    url = "rhino/geometry/mesh/unifynormals-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = [[item] for item in thisMesh]
    response = Util.ComputeFetch(url, args)
    return response


def UnifyNormals1(thisMesh, countOnly, multiple=False):
    """
    Attempts to fix inconsistencies in the directions of mesh faces in a mesh. This function
    does not modify mesh vertex normals, it rearranges the mesh face winding and face
    normals to make them all consistent. Note, you may want to call Mesh.Normals.ComputeNormals()
    to recompute vertex normals after calling this functions.

    Args:
        countOnly (bool): If true, then only the number of faces that would be modified is determined.

    Returns:
        int: If countOnly=false, the number of faces that were modified. If countOnly=true, the number of faces that would be modified.
    """
    url = "rhino/geometry/mesh/unifynormals-mesh_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, countOnly]
    if multiple: args = list(zip(thisMesh, countOnly))
    response = Util.ComputeFetch(url, args)
    return response


def SplitDisjointPieces(thisMesh, multiple=False):
    """
    Splits up the mesh into its unconnected pieces.

    Returns:
        Mesh[]: An array containing all the disjoint pieces that make up this Mesh.
    """
    url = "rhino/geometry/mesh/splitdisjointpieces-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = [[item] for item in thisMesh]
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Split(thisMesh, plane, multiple=False):
    """
    Split a mesh by an infinite plane.

    Args:
        plane (Plane): The splitting plane.

    Returns:
        Mesh[]: A new mesh array with the split result. This can be None if no result was found.
    """
    url = "rhino/geometry/mesh/split-mesh_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, plane]
    if multiple: args = list(zip(thisMesh, plane))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Split1(thisMesh, mesh, multiple=False):
    """
    Split a mesh with another mesh. Suggestion: upgrade to overload with tolerance.

    Args:
        mesh (Mesh): Mesh to split with.

    Returns:
        Mesh[]: An array of mesh segments representing the split result.
    """
    url = "rhino/geometry/mesh/split-mesh_mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh, mesh]
    if multiple: args = list(zip(thisMesh, mesh))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Split2(thisMesh, meshes, multiple=False):
    """
    Split a mesh with a collection of meshes. Suggestion: upgrade to overload with tolerance.
    Does not split at coplanar intersections.

    Args:
        meshes (IEnumerable<Mesh>): Meshes to split with.

    Returns:
        Mesh[]: An array of mesh segments representing the split result.
    """
    url = "rhino/geometry/mesh/split-mesh_mesharray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshes]
    if multiple: args = list(zip(thisMesh, meshes))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Split3(thisMesh, meshes, tolerance, splitAtCoplanar, textLog, cancel, progress, multiple=False):
    """
    Split a mesh with a collection of meshes.

    Args:
        meshes (IEnumerable<Mesh>): Meshes to split with.
        tolerance (double): A value for intersection tolerance.
            WARNING! Correct values are typically in the (10e-8 - 10e-4) range.An option is to use the document tolerance diminished by a few orders or magnitude.
        splitAtCoplanar (bool): If false, coplanar areas will not be separated.
        textLog (TextLog): A text log to write onto.
        cancel (CancellationToken): A cancellation token.
        progress (IProgress<double>): A progress reporter item. This can be null.

    Returns:
        Mesh[]: An array of mesh parts representing the split result, or null: when no mesh intersected, or if a cancel stopped the computation.
    """
    url = "rhino/geometry/mesh/split-mesh_mesharray_double_bool_textlog_cancellationtoken_doublearray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshes, tolerance, splitAtCoplanar, textLog, cancel, progress]
    if multiple: args = list(zip(thisMesh, meshes, tolerance, splitAtCoplanar, textLog, cancel, progress))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Split4(thisMesh, meshes, tolerance, splitAtCoplanar, createNgons, textLog, cancel, progress, multiple=False):
    """
    Split a mesh with a collection of meshes.

    Args:
        meshes (IEnumerable<Mesh>): Meshes to split with.
        tolerance (double): A value for intersection tolerance.
            WARNING! Correct values are typically in the (10e-8 - 10e-4) range.An option is to use the document tolerance diminished by a few orders or magnitude.
        splitAtCoplanar (bool): If false, coplanar areas will not be separated.
        createNgons (bool): If true, creates ngons along the split ridge.
        textLog (TextLog): A text log to write onto.
        cancel (CancellationToken): A cancellation token.
        progress (IProgress<double>): A progress reporter item. This can be null.

    Returns:
        Mesh[]: An array of mesh parts representing the split result, or null: when no mesh intersected, or if a cancel stopped the computation.
    """
    url = "rhino/geometry/mesh/split-mesh_mesharray_double_bool_bool_textlog_cancellationtoken_doublearray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshes, tolerance, splitAtCoplanar, createNgons, textLog, cancel, progress]
    if multiple: args = list(zip(thisMesh, meshes, tolerance, splitAtCoplanar, createNgons, textLog, cancel, progress))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def GetOutlines(thisMesh, plane, multiple=False):
    """
    Constructs the outlines of a mesh projected against a plane.

    Args:
        plane (Plane): A plane to project against.

    Returns:
        Polyline[]: An array of polylines, or None on error.
    """
    url = "rhino/geometry/mesh/getoutlines-mesh_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, plane]
    if multiple: args = list(zip(thisMesh, plane))
    response = Util.ComputeFetch(url, args)
    return response


def GetOutlines1(thisMesh, viewport, multiple=False):
    """
    Constructs the outlines of a mesh. The projection information in the
    viewport is used to determine how the outlines are projected.

    Args:
        viewport (Display.RhinoViewport): A viewport to determine projection direction.

    Returns:
        Polyline[]: An array of polylines, or None on error.
    """
    url = "rhino/geometry/mesh/getoutlines-mesh_display.rhinoviewport"
    if multiple: url += "?multiple=true"
    args = [thisMesh, viewport]
    if multiple: args = list(zip(thisMesh, viewport))
    response = Util.ComputeFetch(url, args)
    return response


def GetOutlines2(thisMesh, viewportInfo, plane, multiple=False):
    """
    Constructs the outlines of a mesh.

    Args:
        viewportInfo (ViewportInfo): The viewport info that provides the outline direction.
        plane (Plane): Usually the view's construction plane. If a parallel projection and view plane is parallel to this, then project the results to the plane.

    Returns:
        Polyline[]: An array of polylines, or None on error.
    """
    url = "rhino/geometry/mesh/getoutlines-mesh_viewportinfo_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, viewportInfo, plane]
    if multiple: args = list(zip(thisMesh, viewportInfo, plane))
    response = Util.ComputeFetch(url, args)
    return response


def GetNakedEdges(thisMesh, multiple=False):
    """
    Returns all edges of a mesh that are considered "naked" in the
    sense that the edge only has one face.

    Returns:
        Polyline[]: An array of polylines, or None on error.
    """
    url = "rhino/geometry/mesh/getnakededges-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = [[item] for item in thisMesh]
    response = Util.ComputeFetch(url, args)
    return response


def ExplodeAtUnweldedEdges(thisMesh, multiple=False):
    """
    Explode the mesh into sub-meshes where a sub-mesh is a collection of faces that are contained
    within a closed loop of "unwelded" edges. Unwelded edges are edges where the faces that share
    the edge have unique mesh vertexes (not mesh topology vertexes) at both ends of the edge.

    Returns:
        Mesh[]: Array of sub-meshes on success; None on error. If the count in the returned array is 1, then
        nothing happened and the output is essentially a copy of the input.
    """
    url = "rhino/geometry/mesh/explodeatunweldededges-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = [[item] for item in thisMesh]
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def ClosestPoint(thisMesh, testPoint, multiple=False):
    """
    Gets the point on the mesh that is closest to a given test point.

    Args:
        testPoint (Point3d): Point to search for.

    Returns:
        Point3d: The point on the mesh closest to testPoint, or Point3d.Unset on failure.
    """
    url = "rhino/geometry/mesh/closestpoint-mesh_point3d"
    if multiple: url += "?multiple=true"
    args = [thisMesh, testPoint]
    if multiple: args = list(zip(thisMesh, testPoint))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToPoint3d(response)
    return response


def ClosestMeshPoint(thisMesh, testPoint, maximumDistance, multiple=False):
    """
    Gets the point on the mesh that is closest to a given test point. Similar to the
    ClosestPoint function except this returns a MeshPoint class which includes
    extra information beyond just the location of the closest point.

    Args:
        testPoint (Point3d): The source of the search.
        maximumDistance (double): Optional upper bound on the distance from test point to the mesh.
            If you are only interested in finding a point Q on the mesh when
            testPoint.DistanceTo(Q) < maximumDistance,
            then set maximumDistance to that value.
            This parameter is ignored if you pass 0.0 for a maximumDistance.

    Returns:
        MeshPoint: closest point information on success. None on failure.
    """
    url = "rhino/geometry/mesh/closestmeshpoint-mesh_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, testPoint, maximumDistance]
    if multiple: args = list(zip(thisMesh, testPoint, maximumDistance))
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint1(thisMesh, testPoint, maximumDistance, multiple=False):
    """
    Gets the point on the mesh that is closest to a given test point.

    Args:
        testPoint (Point3d): Point to search for.
        maximumDistance (double): Optional upper bound on the distance from test point to the mesh.
            If you are only interested in finding a point Q on the mesh when
            testPoint.DistanceTo(Q) < maximumDistance,
            then set maximumDistance to that value.
            This parameter is ignored if you pass 0.0 for a maximumDistance.

    Returns:
        int: Index of face that the closest point lies on if successful.
        -1 if not successful; the value of pointOnMesh is undefined.
        pointOnMesh (Point3d): Point on the mesh closest to testPoint.
    """
    url = "rhino/geometry/mesh/closestpoint-mesh_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, testPoint, maximumDistance]
    if multiple: args = list(zip(thisMesh, testPoint, maximumDistance))
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint2(thisMesh, testPoint, maximumDistance, multiple=False):
    """
    Gets the point on the mesh that is closest to a given test point.

    Args:
        testPoint (Point3d): Point to search for.
        maximumDistance (double): Optional upper bound on the distance from test point to the mesh.
            If you are only interested in finding a point Q on the mesh when
            testPoint.DistanceTo(Q) < maximumDistance,
            then set maximumDistance to that value.
            This parameter is ignored if you pass 0.0 for a maximumDistance.

    Returns:
        int: Index of face that the closest point lies on if successful.
        -1 if not successful; the value of pointOnMesh is undefined.
        pointOnMesh (Point3d): Point on the mesh closest to testPoint.
        normalAtPoint (Vector3d): The normal vector of the mesh at the closest point.
    """
    url = "rhino/geometry/mesh/closestpoint-mesh_point3d_point3d_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, testPoint, maximumDistance]
    if multiple: args = list(zip(thisMesh, testPoint, maximumDistance))
    response = Util.ComputeFetch(url, args)
    return response


def PointAt(thisMesh, meshPoint, multiple=False):
    """
    Evaluate a mesh at a set of barycentric coordinates.

    Args:
        meshPoint (MeshPoint): MeshPoint instance containing a valid Face Index and Barycentric coordinates.

    Returns:
        Point3d: A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.
    """
    url = "rhino/geometry/mesh/pointat-mesh_meshpoint"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshPoint]
    if multiple: args = list(zip(thisMesh, meshPoint))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToPoint3d(response)
    return response


def PointAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False):
    """
    Evaluates a mesh at a set of barycentric coordinates. Barycentric coordinates must
    be assigned in accordance with the rules as defined by MeshPoint.T.

    Args:
        faceIndex (int): Index of triangle or quad to evaluate.
        t0 (double): First barycentric coordinate.
        t1 (double): Second barycentric coordinate.
        t2 (double): Third barycentric coordinate.
        t3 (double): Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.

    Returns:
        Point3d: A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.
    """
    url = "rhino/geometry/mesh/pointat-mesh_int_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    if multiple: args = list(zip(thisMesh, faceIndex, t0, t1, t2, t3))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToPoint3d(response)
    return response


def NormalAt(thisMesh, meshPoint, multiple=False):
    """
    Evaluate a mesh normal at a set of barycentric coordinates.

    Args:
        meshPoint (MeshPoint): MeshPoint instance containing a valid Face Index and Barycentric coordinates.

    Returns:
        Vector3d: A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.
    """
    url = "rhino/geometry/mesh/normalat-mesh_meshpoint"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshPoint]
    if multiple: args = list(zip(thisMesh, meshPoint))
    response = Util.ComputeFetch(url, args)
    return response


def NormalAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False):
    """
    Evaluate a mesh normal at a set of barycentric coordinates. Barycentric coordinates must
    be assigned in accordance with the rules as defined by MeshPoint.T.

    Args:
        faceIndex (int): Index of triangle or quad to evaluate.
        t0 (double): First barycentric coordinate.
        t1 (double): Second barycentric coordinate.
        t2 (double): Third barycentric coordinate.
        t3 (double): Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.

    Returns:
        Vector3d: A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.
    """
    url = "rhino/geometry/mesh/normalat-mesh_int_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    if multiple: args = list(zip(thisMesh, faceIndex, t0, t1, t2, t3))
    response = Util.ComputeFetch(url, args)
    return response


def ColorAt(thisMesh, meshPoint, multiple=False):
    """
    Evaluate a mesh color at a set of barycentric coordinates.

    Args:
        meshPoint (MeshPoint): MeshPoint instance containing a valid Face Index and Barycentric coordinates.

    Returns:
        Color: The interpolated vertex color on the mesh or Color.Transparent if the faceIndex is not valid,
        if the barycentric coordinates could not be evaluated, or if there are no colors defined on the mesh.
    """
    url = "rhino/geometry/mesh/colorat-mesh_meshpoint"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshPoint]
    if multiple: args = list(zip(thisMesh, meshPoint))
    response = Util.ComputeFetch(url, args)
    return response


def ColorAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False):
    """
    Evaluate a mesh normal at a set of barycentric coordinates. Barycentric coordinates must
    be assigned in accordance with the rules as defined by MeshPoint.T.

    Args:
        faceIndex (int): Index of triangle or quad to evaluate.
        t0 (double): First barycentric coordinate.
        t1 (double): Second barycentric coordinate.
        t2 (double): Third barycentric coordinate.
        t3 (double): Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.

    Returns:
        Color: The interpolated vertex color on the mesh or Color.Transparent if the faceIndex is not valid,
        if the barycentric coordinates could not be evaluated, or if there are no colors defined on the mesh.
    """
    url = "rhino/geometry/mesh/colorat-mesh_int_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    if multiple: args = list(zip(thisMesh, faceIndex, t0, t1, t2, t3))
    response = Util.ComputeFetch(url, args)
    return response


def PullPointsToMesh(thisMesh, points, multiple=False):
    """
    Pulls a collection of points to a mesh.

    Args:
        points (IEnumerable<Point3d>): An array, a list or any enumerable set of points.

    Returns:
        Point3d[]: An array of points. This can be empty.
    """
    url = "rhino/geometry/mesh/pullpointstomesh-mesh_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, points]
    if multiple: args = list(zip(thisMesh, points))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToPoint3d(response)
    return response


def PullCurve(thisMesh, curve, tolerance, multiple=False):
    """
    Gets a polyline approximation of the input curve and then moves its control points to the closest point on the mesh.
    Then it "connects the points" over edges so that a polyline on the mesh is formed.

    Args:
        curve (Curve): A curve to pull.
        tolerance (double): A tolerance value.

    Returns:
        PolylineCurve: A polyline curve, or None if none could be constructed.
    """
    url = "rhino/geometry/mesh/pullcurve-mesh_curve_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, curve, tolerance]
    if multiple: args = list(zip(thisMesh, curve, tolerance))
    response = Util.ComputeFetch(url, args)
    return response


def SplitWithProjectedPolylines(thisMesh, curves, tolerance, multiple=False):
    """
    Splits a mesh by adding edges in correspondence with input polylines, and divides the mesh at partitioned areas.
    Polyline segments that are measured not to be on the mesh will be ignored.

    Args:
        curves (IEnumerable<PolylineCurve>): An array, a list or any enumerable of polyline curves.
        tolerance (double): A tolerance value.

    Returns:
        Mesh[]: An array of meshes, or None if no change would happen.
    """
    url = "rhino/geometry/mesh/splitwithprojectedpolylines-mesh_polylinecurvearray_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, curves, tolerance]
    if multiple: args = list(zip(thisMesh, curves, tolerance))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def SplitWithProjectedPolylines1(thisMesh, curves, tolerance, textLog, cancel, progress, multiple=False):
    """
    Splits a mesh by adding edges in correspondence with input polylines, and divides the mesh at partitioned areas.
    Polyline segments that are measured not to be on the mesh will be ignored.

    Args:
        curves (IEnumerable<PolylineCurve>): An array, a list or any enumerable of polyline curves.
        tolerance (double): A tolerance value.
        textLog (TextLog): A text log, or null.
        cancel (CancellationToken): A cancellation token to stop the computation at a given point.
        progress (IProgress<double>): A progress reporter to inform the user about progress. The reported value is indicative.

    Returns:
        Mesh[]: An array of meshes, or None if no change would happen.
    """
    url = "rhino/geometry/mesh/splitwithprojectedpolylines-mesh_polylinecurvearray_double_textlog_cancellationtoken_doublearray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, curves, tolerance, textLog, cancel, progress]
    if multiple: args = list(zip(thisMesh, curves, tolerance, textLog, cancel, progress))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Offset(thisMesh, distance, multiple=False):
    """
    Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
    Same as Mesh.Offset(distance, false)

    Args:
        distance (double): A distance value to use for offsetting.

    Returns:
        Mesh: A new mesh on success, or None on failure.
    """
    url = "rhino/geometry/mesh/offset-mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance]
    if multiple: args = list(zip(thisMesh, distance))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Offset1(thisMesh, distance, solidify, multiple=False):
    """
    Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
    Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
    If solidify is False it acts exactly as the Offset(distance) function.

    Args:
        distance (double): A distance value.
        solidify (bool): True if the mesh should be solidified.

    Returns:
        Mesh: A new mesh on success, or None on failure.
    """
    url = "rhino/geometry/mesh/offset-mesh_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance, solidify]
    if multiple: args = list(zip(thisMesh, distance, solidify))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Offset2(thisMesh, distance, solidify, direction, multiple=False):
    """
    Makes a new mesh with vertices offset a distance along the direction parameter.
    Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
    If solidify is False it acts exactly as the Offset(distance) function.

    Args:
        distance (double): A distance value.
        solidify (bool): True if the mesh should be solidified.
        direction (Vector3d): Direction of offset for all vertices.

    Returns:
        Mesh: A new mesh on success, or None on failure.
    """
    url = "rhino/geometry/mesh/offset-mesh_double_bool_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance, solidify, direction]
    if multiple: args = list(zip(thisMesh, distance, solidify, direction))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def Offset3(thisMesh, distance, solidify, direction, multiple=False):
    """
    Makes a new mesh with vertices offset a distance along the direction parameter.
    Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
    If solidify is False it acts exactly as the Offset(distance) function. Returns list of wall faces, i.e. the
    faces that connect original and offset mesh when solidified.

    Args:
        distance (double): A distance value.
        solidify (bool): True if the mesh should be solidified.
        direction (Vector3d): Direction of offset for all vertices.

    Returns:
        Mesh: A new mesh on success, or None on failure.
        wallFacesOut (List<int>): Returns list of wall faces.
    """
    url = "rhino/geometry/mesh/offset-mesh_double_bool_vector3d_intarray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance, solidify, direction]
    if multiple: args = list(zip(thisMesh, distance, solidify, direction))
    response = Util.ComputeFetch(url, args)
    return response


def CollapseFacesByEdgeLength(thisMesh, bGreaterThan, edgeLength, multiple=False):
    """
    Collapses multiple mesh faces, with greater/less than edge length, based on the principles
    found in Stan Melax's mesh reduction PDF,
    see http://pomax.nihongoresources.com/downloads/PolygonReduction.pdf

    Args:
        bGreaterThan (bool): Determines whether edge with lengths greater than or less than edgeLength are collapsed.
        edgeLength (double): Length with which to compare to edge lengths.

    Returns:
        int: Number of edges (faces) that were collapsed.
    """
    url = "rhino/geometry/mesh/collapsefacesbyedgelength-mesh_bool_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, bGreaterThan, edgeLength]
    if multiple: args = list(zip(thisMesh, bGreaterThan, edgeLength))
    response = Util.ComputeFetch(url, args)
    return response


def CollapseFacesByArea(thisMesh, lessThanArea, greaterThanArea, multiple=False):
    """
    Collapses multiple mesh faces, with areas less than LessThanArea and greater than GreaterThanArea,
    based on the principles found in Stan Melax's mesh reduction PDF,
    see http://pomax.nihongoresources.com/downloads/PolygonReduction.pdf

    Args:
        lessThanArea (double): Area in which faces are selected if their area is less than or equal to.
        greaterThanArea (double): Area in which faces are selected if their area is greater than or equal to.

    Returns:
        int: Number of faces that were collapsed in the process.
    """
    url = "rhino/geometry/mesh/collapsefacesbyarea-mesh_double_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, lessThanArea, greaterThanArea]
    if multiple: args = list(zip(thisMesh, lessThanArea, greaterThanArea))
    response = Util.ComputeFetch(url, args)
    return response


def CollapseFacesByByAspectRatio(thisMesh, aspectRatio, multiple=False):
    """
    Collapses a multiple mesh faces, determined by face aspect ratio, based on criteria found in Stan Melax's polygon reduction,
    see http://pomax.nihongoresources.com/downloads/PolygonReduction.pdf

    Args:
        aspectRatio (double): Faces with an aspect ratio less than aspectRatio are considered as candidates.

    Returns:
        int: Number of faces that were collapsed in the process.
    """
    url = "rhino/geometry/mesh/collapsefacesbybyaspectratio-mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, aspectRatio]
    if multiple: args = list(zip(thisMesh, aspectRatio))
    response = Util.ComputeFetch(url, args)
    return response


def GetUnsafeLock(thisMesh, writable, multiple=False):
    """
    Allows to obtain unsafe pointers to the underlying unmanaged data structures of the mesh.

    Args:
        writable (bool): True if user will need to write onto the structure. False otherwise.

    Returns:
        MeshUnsafeLock: A lock that needs to be released.
    """
    url = "rhino/geometry/mesh/getunsafelock-mesh_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, writable]
    if multiple: args = list(zip(thisMesh, writable))
    response = Util.ComputeFetch(url, args)
    return response


def ReleaseUnsafeLock(thisMesh, meshData, multiple=False):
    """
    Updates the Mesh data with the information that was stored via the .

    Args:
        meshData (MeshUnsafeLock): The data that will be unlocked.
    """
    url = "rhino/geometry/mesh/releaseunsafelock-mesh_meshunsafelock"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshData]
    if multiple: args = list(zip(thisMesh, meshData))
    response = Util.ComputeFetch(url, args)
    return response


def WithShutLining(thisMesh, faceted, tolerance, curves, multiple=False):
    """
    Constructs new mesh from the current one, with shut lining applied to it.

    Args:
        faceted (bool): Specifies whether the shutline is faceted.
        tolerance (double): The tolerance of the shutline.
        curves (IEnumerable<ShutLiningCurveInfo>): A collection of curve arguments.

    Returns:
        Mesh: A new mesh with shutlining. Null on failure.
    """
    url = "rhino/geometry/mesh/withshutlining-mesh_bool_double_shutliningcurveinfoarray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceted, tolerance, curves]
    if multiple: args = list(zip(thisMesh, faceted, tolerance, curves))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def WithDisplacement(thisMesh, displacement, multiple=False):
    """
    Constructs new mesh from the current one, with displacement applied to it.

    Args:
        displacement (MeshDisplacementInfo): Information on mesh displacement.

    Returns:
        Mesh: A new mesh with shutlining.
    """
    url = "rhino/geometry/mesh/withdisplacement-mesh_meshdisplacementinfo"
    if multiple: url += "?multiple=true"
    args = [thisMesh, displacement]
    if multiple: args = list(zip(thisMesh, displacement))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def WithEdgeSoftening(thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold, multiple=False):
    """
    Constructs new mesh from the current one, with edge softening applied to it.

    Args:
        softeningRadius (double): The softening radius.
        chamfer (bool): Specifies whether to chamfer the edges.
        faceted (bool): Specifies whether the edges are faceted.
        force (bool): Specifies whether to soften edges despite too large a radius.
        angleThreshold (double): Threshold angle (in degrees) which controls whether an edge is softened or not.
            The angle refers to the angles between the adjacent faces of an edge.

    Returns:
        Mesh: A new mesh with soft edges.
    """
    url = "rhino/geometry/mesh/withedgesoftening-mesh_double_bool_bool_bool_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold]
    if multiple: args = list(zip(thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def QuadRemeshBrep(brep, parameters, multiple=False):
    """
    Create QuadRemesh from a Brep
    Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
    """
    url = "rhino/geometry/mesh/quadremeshbrep-brep_quadremeshparameters"
    if multiple: url += "?multiple=true"
    args = [brep, parameters]
    if multiple: args = list(zip(brep, parameters))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def QuadRemeshBrep1(brep, parameters, guideCurves, multiple=False):
    """
    Create Quad Remesh from a Brep

    Args:
        brep (Brep): Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
        guideCurves (IEnumerable<Curve>): A curve array used to influence mesh face layout
            The curves should touch the input mesh
            Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
    """
    url = "rhino/geometry/mesh/quadremeshbrep-brep_quadremeshparameters_curvearray"
    if multiple: url += "?multiple=true"
    args = [brep, parameters, guideCurves]
    if multiple: args = list(zip(brep, parameters, guideCurves))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def QuadRemeshBrepAsync(brep, parameters, progress, cancelToken, multiple=False):
    """
    Quad remesh this Brep asynchronously.

    Args:
        brep (Brep): Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
    """
    url = "rhino/geometry/mesh/quadremeshbrepasync-brep_quadremeshparameters_intarray_cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [brep, parameters, progress, cancelToken]
    if multiple: args = list(zip(brep, parameters, progress, cancelToken))
    response = Util.ComputeFetch(url, args)
    return response


def QuadRemeshBrepAsync1(brep, parameters, guideCurves, progress, cancelToken, multiple=False):
    """
    Quad remesh this Brep asynchronously.

    Args:
        brep (Brep): Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
        guideCurves (IEnumerable<Curve>): A curve array used to influence mesh face layout
            The curves should touch the input mesh
            Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
    """
    url = "rhino/geometry/mesh/quadremeshbrepasync-brep_quadremeshparameters_curvearray_intarray_cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [brep, parameters, guideCurves, progress, cancelToken]
    if multiple: args = list(zip(brep, parameters, guideCurves, progress, cancelToken))
    response = Util.ComputeFetch(url, args)
    return response


def QuadRemesh(thisMesh, parameters, multiple=False):
    """
    Quad remesh this mesh.
    """
    url = "rhino/geometry/mesh/quadremesh-mesh_quadremeshparameters"
    if multiple: url += "?multiple=true"
    args = [thisMesh, parameters]
    if multiple: args = list(zip(thisMesh, parameters))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def QuadRemesh1(thisMesh, parameters, guideCurves, multiple=False):
    """
    Quad remesh this mesh.

    Args:
        guideCurves (IEnumerable<Curve>): A curve array used to influence mesh face layout
            The curves should touch the input mesh
            Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
    """
    url = "rhino/geometry/mesh/quadremesh-mesh_quadremeshparameters_curvearray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, parameters, guideCurves]
    if multiple: args = list(zip(thisMesh, parameters, guideCurves))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def QuadRemeshAsync(thisMesh, parameters, progress, cancelToken, multiple=False):
    """
    Quad remesh this mesh asynchronously.
    """
    url = "rhino/geometry/mesh/quadremeshasync-mesh_quadremeshparameters_intarray_cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [thisMesh, parameters, progress, cancelToken]
    if multiple: args = list(zip(thisMesh, parameters, progress, cancelToken))
    response = Util.ComputeFetch(url, args)
    return response


def QuadRemeshAsync1(thisMesh, parameters, guideCurves, progress, cancelToken, multiple=False):
    """
    Quad remesh this mesh asynchronously.

    Args:
        guideCurves (IEnumerable<Curve>): A curve array used to influence mesh face layout
            The curves should touch the input mesh
            Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
    """
    url = "rhino/geometry/mesh/quadremeshasync-mesh_quadremeshparameters_curvearray_intarray_cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [thisMesh, parameters, guideCurves, progress, cancelToken]
    if multiple: args = list(zip(thisMesh, parameters, guideCurves, progress, cancelToken))
    response = Util.ComputeFetch(url, args)
    return response


def QuadRemeshAsync2(thisMesh, faceBlocks, parameters, guideCurves, progress, cancelToken, multiple=False):
    """
    Quad remesh this mesh asynchronously.

    Args:
        guideCurves (IEnumerable<Curve>): A curve array used to influence mesh face layout
            The curves should touch the input mesh
            Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
    """
    url = "rhino/geometry/mesh/quadremeshasync-mesh_intarray_quadremeshparameters_curvearray_intarray_cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceBlocks, parameters, guideCurves, progress, cancelToken]
    if multiple: args = list(zip(thisMesh, faceBlocks, parameters, guideCurves, progress, cancelToken))
    response = Util.ComputeFetch(url, args)
    return response


def Reduce(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, multiple=False):
    """
    Reduce polygon count

    Args:
        desiredPolygonCount (int): desired or target number of faces
        allowDistortion (bool): If True mesh appearance is not changed even if the target polygon count is not reached
        accuracy (int): Integer from 1 to 10 telling how accurate reduction algorithm
            to use. Greater number gives more accurate results
        normalizeSize (bool): If True mesh is fitted to an axis aligned unit cube until reduction is complete

    Returns:
        bool: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
    """
    url = "rhino/geometry/mesh/reduce-mesh_int_bool_int_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize]
    if multiple: args = list(zip(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize))
    response = Util.ComputeFetch(url, args)
    return response


def Reduce1(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, threaded, multiple=False):
    """
    Reduce polygon count

    Args:
        desiredPolygonCount (int): desired or target number of faces
        allowDistortion (bool): If True mesh appearance is not changed even if the target polygon count is not reached
        accuracy (int): Integer from 1 to 10 telling how accurate reduction algorithm
            to use. Greater number gives more accurate results
        normalizeSize (bool): If True mesh is fitted to an axis aligned unit cube until reduction is complete
        threaded (bool): If True then will run computation inside a worker thread and ignore any provided CancellationTokens and ProgressReporters.
            If False then will run on main thread.

    Returns:
        bool: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
    """
    url = "rhino/geometry/mesh/reduce-mesh_int_bool_int_bool_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, threaded]
    if multiple: args = list(zip(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, threaded))
    response = Util.ComputeFetch(url, args)
    return response


def Reduce2(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, cancelToken, progress, multiple=False):
    """
    Reduce polygon count

    Args:
        desiredPolygonCount (int): desired or target number of faces
        allowDistortion (bool): If True mesh appearance is not changed even if the target polygon count is not reached
        accuracy (int): Integer from 1 to 10 telling how accurate reduction algorithm
            to use. Greater number gives more accurate results
        normalizeSize (bool): If True mesh is fitted to an axis aligned unit cube until reduction is complete

    Returns:
        bool: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
    """
    url = "rhino/geometry/mesh/reduce-mesh_int_bool_int_bool_cancellationtoken_doublearray_string"
    if multiple: url += "?multiple=true"
    args = [thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, cancelToken, progress]
    if multiple: args = list(zip(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, cancelToken, progress))
    response = Util.ComputeFetch(url, args)
    return response


def Reduce3(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, cancelToken, progress, threaded, multiple=False):
    """
    Reduce polygon count

    Args:
        desiredPolygonCount (int): desired or target number of faces
        allowDistortion (bool): If True mesh appearance is not changed even if the target polygon count is not reached
        accuracy (int): Integer from 1 to 10 telling how accurate reduction algorithm
            to use. Greater number gives more accurate results
        normalizeSize (bool): If True mesh is fitted to an axis aligned unit cube until reduction is complete
        threaded (bool): If True then will run computation inside a worker thread and ignore any provided CancellationTokens and ProgressReporters.
            If False then will run on main thread.

    Returns:
        bool: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
    """
    url = "rhino/geometry/mesh/reduce-mesh_int_bool_int_bool_cancellationtoken_doublearray_string_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, cancelToken, progress, threaded]
    if multiple: args = list(zip(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, cancelToken, progress, threaded))
    response = Util.ComputeFetch(url, args)
    return response


def Reduce4(thisMesh, parameters, multiple=False):
    """
    Reduce polygon count

    Args:
        parameters (ReduceMeshParameters): Parameters

    Returns:
        bool: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
    """
    url = "rhino/geometry/mesh/reduce-mesh_reducemeshparameters"
    if multiple: url += "?multiple=true"
    args = [thisMesh, parameters]
    if multiple: args = list(zip(thisMesh, parameters))
    response = Util.ComputeFetch(url, args)
    return response


def Reduce5(thisMesh, parameters, threaded, multiple=False):
    """
    Reduce polygon count

    Args:
        parameters (ReduceMeshParameters): Parameters
        threaded (bool): If True then will run computation inside a worker thread and ignore any provided CancellationTokens and ProgressReporters.
            If False then will run on main thread.

    Returns:
        bool: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
    """
    url = "rhino/geometry/mesh/reduce-mesh_reducemeshparameters_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, parameters, threaded]
    if multiple: args = list(zip(thisMesh, parameters, threaded))
    response = Util.ComputeFetch(url, args)
    return response


def ComputeThickness(meshes, maximumThickness, multiple=False):
    """
    Compute thickness metrics for this mesh.

    Args:
        meshes (IEnumerable<Mesh>): Meshes to include in thickness analysis.
        maximumThickness (double): Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.

    Returns:
        MeshThicknessMeasurement[]: Array of thickness measurements.
    """
    url = "rhino/geometry/mesh/computethickness-mesharray_double"
    if multiple: url += "?multiple=true"
    args = [meshes, maximumThickness]
    if multiple: args = list(zip(meshes, maximumThickness))
    response = Util.ComputeFetch(url, args)
    return response


def ComputeThickness1(meshes, maximumThickness, cancelToken, multiple=False):
    """
    Compute thickness metrics for this mesh.

    Args:
        meshes (IEnumerable<Mesh>): Meshes to include in thickness analysis.
        maximumThickness (double): Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.
        cancelToken (System.Threading.CancellationToken): Computation cancellation token.

    Returns:
        MeshThicknessMeasurement[]: Array of thickness measurements.
    """
    url = "rhino/geometry/mesh/computethickness-mesharray_double_system.threading.cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [meshes, maximumThickness, cancelToken]
    if multiple: args = list(zip(meshes, maximumThickness, cancelToken))
    response = Util.ComputeFetch(url, args)
    return response


def ComputeThickness2(meshes, maximumThickness, sharpAngle, cancelToken, multiple=False):
    """
    Compute thickness metrics for this mesh.

    Args:
        meshes (IEnumerable<Mesh>): Meshes to include in thickness analysis.
        maximumThickness (double): Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.
        sharpAngle (double): Sharpness angle in radians.
        cancelToken (System.Threading.CancellationToken): Computation cancellation token.

    Returns:
        MeshThicknessMeasurement[]: Array of thickness measurements.
    """
    url = "rhino/geometry/mesh/computethickness-mesharray_double_double_system.threading.cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [meshes, maximumThickness, sharpAngle, cancelToken]
    if multiple: args = list(zip(meshes, maximumThickness, sharpAngle, cancelToken))
    response = Util.ComputeFetch(url, args)
    return response


def CreateContourCurves(meshToContour, contourStart, contourEnd, interval, multiple=False):
    """
    Constructs contour curves for a mesh, sectioned along a linear axis.

    Args:
        meshToContour (Mesh): A mesh to contour.
        contourStart (Point3d): A start point of the contouring axis.
        contourEnd (Point3d): An end point of the contouring axis.
        interval (double): An interval distance.

    Returns:
        Curve[]: An array of curves. This array can be empty.
    """
    url = "rhino/geometry/mesh/createcontourcurves-mesh_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [meshToContour, contourStart, contourEnd, interval]
    if multiple: args = list(zip(meshToContour, contourStart, contourEnd, interval))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response


def CreateContourCurves1(meshToContour, sectionPlane, multiple=False):
    """
    Constructs contour curves for a mesh, sectioned at a plane.

    Args:
        meshToContour (Mesh): A mesh to contour.
        sectionPlane (Plane): A cutting plane.

    Returns:
        Curve[]: An array of curves. This array can be empty.
    """
    url = "rhino/geometry/mesh/createcontourcurves-mesh_plane"
    if multiple: url += "?multiple=true"
    args = [meshToContour, sectionPlane]
    if multiple: args = list(zip(meshToContour, sectionPlane))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToCommonObject(response)
    return response

