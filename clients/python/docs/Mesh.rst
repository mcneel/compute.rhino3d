Mesh
====

.. py:module:: compute_rhino3d.Mesh

.. py:function:: CreateFromPlane(plane, xInterval, yInterval, xCount, yCount, multiple=False)

   Constructs a planar mesh grid.

   :param rhino3dm.Plane plane: Plane of mesh.
   :param rhino3dm.Interval xInterval: Interval describing size and extends of mesh along plane x-direction.
   :param rhino3dm.Interval yInterval: Interval describing size and extends of mesh along plane y-direction.
   :param int xCount: Number of faces in x-direction.
   :param int yCount: Number of faces in y-direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromFilteredFaceList(original, inclusion, multiple=False)

   Constructs a sub-mesh, that contains a filtered list of faces.

   :param rhino3dm.Mesh original: The mesh to copy. This item can be null, and in this case an empty mesh is returned.
   :param IEnumerable<bool> inclusion: A series of True and False values, that determine if each face is used in the new mesh. \
      If the amount does not match the length of the face list, the pattern is repeated. If it exceeds the amount \
      of faces in the mesh face list, the pattern is truncated. This items can be None or empty, and the mesh will simply be duplicated.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromBox(box, xCount, yCount, zCount, multiple=False)

   Constructs new mesh that matches a bounding box.

   :param rhino3dm.BoundingBox box: A box to use for creation.
   :param int xCount: Number of faces in x-direction.
   :param int yCount: Number of faces in y-direction.
   :param int zCount: Number of faces in z-direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new brep, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromBox1(box, xCount, yCount, zCount, multiple=False)

   Constructs new mesh that matches an aligned box.

   :param rhino3dm.Box box: Box to match.
   :param int xCount: Number of faces in x-direction.
   :param int yCount: Number of faces in y-direction.
   :param int zCount: Number of faces in z-direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromBox2(corners, xCount, yCount, zCount, multiple=False)

   Constructs new mesh from 8 corner points.

   :param list[rhino3dm.Point3d] corners: 8 points defining the box corners arranged as the vN labels indicate. \
      v7_____________v6|\             |\| \            | \|  \ _____________\|   v4         |   v5|   |          |   ||   |          |   |v3--|----------v2  | \  |           \  |  \ |            \ |   \|             \|    v0_____________v1
   :param int xCount: Number of faces in x-direction.
   :param int yCount: Number of faces in y-direction.
   :param int zCount: Number of faces in z-direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new brep, or None on failure. \
      A new box mesh, on None on error.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromSphere(sphere, xCount, yCount, multiple=False)

   Constructs a mesh sphere.

   :param rhino3dm.Sphere sphere: Base sphere for mesh.
   :param int xCount: Number of faces in the around direction.
   :param int yCount: Number of faces in the top-to-bottom direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: CreateIcoSphere(sphere, subdivisions, multiple=False)

   Constructs a icospherical mesh. A mesh icosphere differs from a standard
   UV mesh sphere in that it's vertices are evenly distributed. A mesh icosphere
   starts from an icosahedron (a regular polyhedron with 20 equilateral triangles).
   It is then refined by splitting each triangle into 4 smaller triangles.
   This splitting can be done several times.

   :param rhino3dm.Sphere sphere: The input sphere provides the orienting plane and radius.
   :param int subdivisions: The number of times you want the faces split, where 0  <= subdivisions <= 7. \
      Note, the total number of mesh faces produces is: 20 * (4 ^ subdivisions)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A welded mesh icosphere if successful, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateQuadSphere(sphere, subdivisions, multiple=False)

   Constructs a quad mesh sphere. A quad mesh sphere differs from a standard
   UV mesh sphere in that it's vertices are evenly distributed. A quad mesh sphere
   starts from a cube (a regular polyhedron with 6 square sides).
   It is then refined by splitting each quad into 4 smaller quads.
   This splitting can be done several times.

   :param rhino3dm.Sphere sphere: The input sphere provides the orienting plane and radius.
   :param int subdivisions: The number of times you want the faces split, where 0  <= subdivisions <= 8. \
      Note, the total number of mesh faces produces is: 6 * (4 ^ subdivisions)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A welded quad mesh sphere if successful, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromCylinder(cylinder, vertical, around, multiple=False)

   Constructs a capped mesh cylinder.

   :param int vertical: Number of faces in the top-to-bottom direction.
   :param int around: Number of faces around the cylinder.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Returns a mesh cylinder if successful, None otherwise.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromCylinder1(cylinder, vertical, around, capBottom, capTop, multiple=False)

   Constructs a mesh cylinder.

   :param int vertical: Number of faces in the top-to-bottom direction.
   :param int around: Number of faces around the cylinder.
   :param bool capBottom: If True end at Cylinder.Height1 should be capped.
   :param bool capTop: If True end at Cylinder.Height2 should be capped.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Returns a mesh cylinder if successful, None otherwise.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromCylinder2(cylinder, vertical, around, capBottom, capTop, quadCaps, multiple=False)

   Constructs a mesh cylinder.

   :param int vertical: Number of faces in the top-to-bottom direction.
   :param int around: Number of faces around the cylinder.
   :param bool capBottom: If True end at Cylinder.Height1 should be capped.
   :param bool capTop: If True end at Cylinder.Height2 should be capped.
   :param bool quadCaps: If True and it's possible to make quad caps, i.e.. around is even, then caps will have quad faces.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Returns a mesh cylinder if successful, None otherwise.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromCone(cone, vertical, around, multiple=False)

   Constructs a solid mesh cone.

   :param int vertical: Number of faces in the top-to-bottom direction.
   :param int around: Number of faces around the cone.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A valid mesh if successful.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromCone1(cone, vertical, around, solid, multiple=False)

   Constructs a mesh cone.

   :param int vertical: Number of faces in the top-to-bottom direction.
   :param int around: Number of faces around the cone.
   :param bool solid: If False the mesh will be open with no faces on the circular planar portion.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A valid mesh if successful.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromCone2(cone, vertical, around, solid, quadCaps, multiple=False)

   Constructs a mesh cone.

   :param int vertical: Number of faces in the top-to-bottom direction.
   :param int around: Number of faces around the cone.
   :param bool solid: If False the mesh will be open with no faces on the circular planar portion.
   :param bool quadCaps: If True and it's possible to make quad caps, i.e.. around is even, then caps will have quad faces.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A valid mesh if successful.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromTorus(torus, vertical, around, multiple=False)

   Constructs a mesh torus.

   :param Torus torus: The torus.
   :param int vertical: Number of faces in the top-to-bottom direction.
   :param int around: Number of faces around the torus.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Returns a mesh torus if successful, None otherwise.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromPlanarBoundary(boundary, parameters, multiple=False)

   Do not use this overload. Use version that takes a tolerance parameter instead.

   :param rhino3dm.Curve boundary: Do not use.
   :param rhino3dm.MeshingParameters parameters: Do not use.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Do not use.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromPlanarBoundary1(boundary, parameters, tolerance, multiple=False)

   Attempts to construct a mesh from a closed planar curve.RhinoMakePlanarMeshes

   :param rhino3dm.Curve boundary: must be a closed planar curve.
   :param rhino3dm.MeshingParameters parameters: parameters used for creating the mesh.
   :param float tolerance: Tolerance to use during operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New mesh on success or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromClosedPolyline(polyline, multiple=False)

   Attempts to create a Mesh that is a triangulation of a simple closed polyline that projects onto a plane.

   :param rhino3dm.Polyline polyline: must be closed
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New mesh on success or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromTessellation(points, edges, plane, allowNewVertices, multiple=False)

   Attempts to create a mesh that is a triangulation of a list of points, projected on a plane,
   including its holes and fixed edges.

   :param list[rhino3dm.Point3d] points: A list, an array or any enumerable of points.
   :param rhino3dm.Plane plane: A plane.
   :param bool allowNewVertices: If true, the mesh might have more vertices than the list of input points, \
      if doing so will improve long thin triangles.
   :param IEnumerable<IEnumerable<Point3d>> edges: A list of polylines, or other lists of points representing edges. \
      This can be null. If nested enumerable items are null, they will be discarded.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh, or None if not successful.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromBrep(brep, multiple=False)

   Constructs a mesh from a brep.

   :param rhino3dm.Brep brep: Brep to approximate.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of meshes.
   :rtype: rhino3dm.Mesh[]
.. py:function:: CreateFromBrep1(brep, meshingParameters, multiple=False)

   Constructs a mesh from a brep.

   :param rhino3dm.Brep brep: Brep to approximate.
   :param rhino3dm.MeshingParameters meshingParameters: Parameters to use during meshing.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of meshes.
   :rtype: rhino3dm.Mesh[]
.. py:function:: CreateFromSurface(surface, multiple=False)

   Constructs a mesh from a surface

   :param rhino3dm.Surface surface: Surface to approximate
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New mesh representing the surface
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromSurface1(surface, meshingParameters, multiple=False)

   Constructs a mesh from a surface

   :param rhino3dm.Surface surface: Surface to approximate
   :param rhino3dm.MeshingParameters meshingParameters: settings used to create the mesh
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New mesh representing the surface
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromSubD(subd, displayDensity, multiple=False)

   Create a mesh from a SubD limit surface

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: CreatePatch(outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions, multiple=False)

   Construct a mesh patch from a variety of input geometry.

   :param rhino3dm.Polyline outerBoundary: (optional: can be null) Outer boundary \
      polyline, if provided this will become the outer boundary of the \
      resulting mesh. Any of the input that is completely outside the outer \
      boundary will be ignored and have no impact on the result. If any of \
      the input intersects the outer boundary the result will be \
      unpredictable and is likely to not include the entire outer boundary.
   :param float angleToleranceRadians: Maximum angle between unit tangents and adjacent vertices. Used to \
      divide curve inputs that cannot otherwise be represented as a polyline.
   :param list[rhino3dm.Curve] innerBoundaryCurves: (optional: can be null) Polylines to create holes in the output mesh. \
      If innerBoundaryCurves are the only input then the result may be null \
      if trimback is set to False (see comments for trimback) because the \
      resulting mesh could be invalid (all faces created contained vertexes \
      from the perimeter boundary).
   :param rhino3dm.Surface pullbackSurface: (optional: can be null) Initial surface where 3d input will be pulled \
      to make a 2d representation used by the function that generates the mesh. \
      Providing a pullbackSurface can be helpful when it is similar in shape \
      to the pattern of the input, the pulled 2d points will be a better \
      representation of the 3d points. If all of the input is more or less \
      coplanar to start with, providing pullbackSurface has no real benefit.
   :param list[rhino3dm.Curve] innerBothSideCurves: (optional: can be null) These polylines will create faces on both sides \
      of the edge. If there are only input points(innerPoints) there is no \
      way to guarantee a triangulation that will create an edge between two \
      particular points. Adding a line, or polyline, to innerBothsideCurves \
      that includes points from innerPoints will help guide the triangulation.
   :param list[rhino3dm.Point3d] innerPoints: (optional: can be null) Points to be used to generate the mesh. If \
      outerBoundary is not null, points outside of that boundary after it has \
      been pulled to pullbackSurface (or the best plane through the input if \
      pullbackSurface is null) will be ignored.
   :param bool trimback: Only used when a outerBoundary has not been provided. When that is the \
      case, the function uses the perimeter of the surface as the outer boundary \
      instead. If true, any face of the resulting triangulated mesh that \
      contains a vertex of the perimeter boundary will be removed.
   :param int divisions: Only used when a outerBoundary has not been provided. When that is the \
      case, division becomes the number of divisions each side of the surface's \
      perimeter will be divided into to create an outer boundary to work with.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: mesh on success; None on failure
   :rtype: rhino3dm.Mesh
.. py:function:: CreateBooleanUnion(meshes, multiple=False)

   Computes the solid union of a set of meshes.

   :param list[rhino3dm.Mesh] meshes: Meshes to union.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Mesh results or None on failure.
   :rtype: rhino3dm.Mesh[]
.. py:function:: CreateBooleanDifference(firstSet, secondSet, multiple=False)

   Computes the solid difference of two sets of Meshes.

   :param list[rhino3dm.Mesh] firstSet: First set of Meshes (the set to subtract from).
   :param list[rhino3dm.Mesh] secondSet: Second set of Meshes (the set to subtract).
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Mesh results or None on failure.
   :rtype: rhino3dm.Mesh[]
.. py:function:: CreateBooleanIntersection(firstSet, secondSet, multiple=False)

   Computes the solid intersection of two sets of meshes.

   :param list[rhino3dm.Mesh] firstSet: First set of Meshes.
   :param list[rhino3dm.Mesh] secondSet: Second set of Meshes.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Mesh results or None on failure.
   :rtype: rhino3dm.Mesh[]
.. py:function:: CreateBooleanSplit(meshesToSplit, meshSplitters, multiple=False)

   Splits a set of meshes with another set.

   :param list[rhino3dm.Mesh] meshesToSplit: A list, an array, or any enumerable set of meshes to be split. If this is null, None will be returned.
   :param list[rhino3dm.Mesh] meshSplitters: A list, an array, or any enumerable set of meshes that cut. If this is null, None will be returned.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh array, or None on error.
   :rtype: rhino3dm.Mesh[]
.. py:function:: CreateFromCurvePipe(curve, radius, segments, accuracy, capType, faceted, intervals, multiple=False)

   Constructs a new mesh pipe from a curve.

   :param rhino3dm.Curve curve: A curve to pipe.
   :param float radius: The radius of the pipe.
   :param int segments: The number of segments in the pipe.
   :param int accuracy: The accuracy of the pipe.
   :param MeshPipeCapStyle capType: The type of cap to be created at the end of the pipe.
   :param bool faceted: Specifies whether the pipe is faceted, or not.
   :param list[rhino3dm.Interval] intervals: A series of intervals to pipe. This value can be null.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromCurveExtrusion(curve, direction, parameters, boundingBox, multiple=False)

   Constructs a new extrusion from a curve.

   :param rhino3dm.Curve curve: A curve to extrude.
   :param rhino3dm.Vector3d direction: The direction of extrusion.
   :param rhino3dm.MeshingParameters parameters: The parameters of meshing.
   :param rhino3dm.BoundingBox boundingBox: The bounding box controls the length of the extrusion.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: CreateFromIterativeCleanup(meshes, tolerance, multiple=False)

   Repairs meshes with vertices that are too near, using a tolerance value.

   :param list[rhino3dm.Mesh] meshes: The meshes to be repaired.
   :param float tolerance: A minimum distance for clean vertices.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A valid meshes array if successful. If no change was required, some meshes can be null. Otherwise, null, when no changes were done.
   :rtype: rhino3dm.Mesh[]
.. py:function:: RequireIterativeCleanup(meshes, tolerance, multiple=False)

   Analyzes some meshes, and determines if a pass of CreateFromIterativeCleanup would change the array.
   All available cleanup steps are used. Currently available cleanup steps are:- mending of single precision coincidence even though double precision vertices differ.- union of nearly identical vertices, irrespectively of their origin.- removal of t-joints along edges.

   :param list[rhino3dm.Mesh] meshes: A list, and array or any enumerable of meshes.
   :param float tolerance: A 3d distance. This is usually a value of about 10e-7 magnitude.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if meshes would be changed, otherwise false.
   :rtype: bool
.. py:function:: Volume(thisMesh, multiple=False)

   Compute volume of the mesh.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Volume of the mesh.
   :rtype: float
.. py:function:: IsPointInside(thisMesh, point, tolerance, strictlyIn, multiple=False)

   Determines if a point is inside a solid mesh.

   :param rhino3dm.Point3d point: 3d point to test.
   :param float tolerance: (>=0) 3d distance tolerance used for ray-mesh intersection \
      and determining strict inclusion.
   :param bool strictlyIn: If strictlyIn is true, then point must be inside mesh by at least \
      tolerance in order for this function to return true. \
      If strictlyIn is false, then this function will return True if \
      point is inside or the distance from point to a mesh face is <= tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if point is inside the solid mesh, False if not.
   :rtype: bool
.. py:function:: Smooth(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, multiple=False)

   Smooths a mesh by averaging the positions of mesh vertices in a specified region.

   :param float smoothFactor: The smoothing factor, which controls how much vertices move towards the average of the neighboring vertices.
   :param bool bXSmooth: When True vertices move in X axis direction.
   :param bool bYSmooth: When True vertices move in Y axis direction.
   :param bool bZSmooth: When True vertices move in Z axis direction.
   :param bool bFixBoundaries: When True vertices along naked edges will not be modified.
   :param SmoothingCoordinateSystem coordinateSystem: The coordinates to determine the direction of the smoothing.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: Smooth1(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False)

   Smooths a mesh by averaging the positions of mesh vertices in a specified region.

   :param float smoothFactor: The smoothing factor, which controls how much vertices move towards the average of the neighboring vertices.
   :param bool bXSmooth: When True vertices move in X axis direction.
   :param bool bYSmooth: When True vertices move in Y axis direction.
   :param bool bZSmooth: When True vertices move in Z axis direction.
   :param bool bFixBoundaries: When True vertices along naked edges will not be modified.
   :param SmoothingCoordinateSystem coordinateSystem: The coordinates to determine the direction of the smoothing.
   :param rhino3dm.Plane plane: If SmoothingCoordinateSystem.CPlane specified, then the construction plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: Smooth2(thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False)

   Smooths part of a mesh by averaging the positions of mesh vertices in a specified region.

   :param list[int] vertexIndices: The mesh vertex indices that specify the part of the mesh to smooth.
   :param float smoothFactor: The smoothing factor, which controls how much vertices move towards the average of the neighboring vertices.
   :param bool bXSmooth: When True vertices move in X axis direction.
   :param bool bYSmooth: When True vertices move in Y axis direction.
   :param bool bZSmooth: When True vertices move in Z axis direction.
   :param bool bFixBoundaries: When True vertices along naked edges will not be modified.
   :param SmoothingCoordinateSystem coordinateSystem: The coordinates to determine the direction of the smoothing.
   :param rhino3dm.Plane plane: If SmoothingCoordinateSystem.CPlane specified, then the construction plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: Unweld(thisMesh, angleToleranceRadians, modifyNormals, multiple=False)

   Makes sure that faces sharing an edge and having a difference of normal greater
   than or equal to angleToleranceRadians have unique vertexes along that edge,
   adding vertices if necessary.

   :param float angleToleranceRadians: Angle at which to make unique vertices.
   :param bool modifyNormals: Determines whether new vertex normals will have the same vertex normal as the original (false) \
      or vertex normals made from the corresponding face normals (true)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: void
.. py:function:: UnweldEdge(thisMesh, edgeIndices, modifyNormals, multiple=False)

   Adds creases to a smooth mesh by creating coincident vertices along selected edges.

   :param list[int] edgeIndices: An array of mesh topology edge indices.
   :param bool modifyNormals: If true, the vertex normals on each side of the edge take the same value as the face to which they belong, giving the mesh a hard edge look. \
      If false, each of the vertex normals on either side of the edge is assigned the same value as the original normal that the pair is replacing, keeping a smooth look.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: UnweldVertices(thisMesh, topologyVertexIndices, modifyNormals, multiple=False)

   Ensures that faces sharing a common topological vertex have unique indices into the  collection.

   :param list[int] topologyVertexIndices: Topological vertex indices, from the  collection, to be unwelded. \
      Use  to convert from vertex indices to topological vertex indices.
   :param bool modifyNormals: If true, the new vertex normals will be calculated from the face normal.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: Weld(thisMesh, angleToleranceRadians, multiple=False)

   Makes sure that faces sharing an edge and having a difference of normal greater
   than or equal to angleToleranceRadians share vertexes along that edge, vertex normals
   are averaged.

   :param float angleToleranceRadians: Angle at which to weld vertices.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: void
.. py:function:: RebuildNormals(thisMesh, multiple=False)

   Removes mesh normals and reconstructs the face and vertex normals based
   on the orientation of the faces.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: void
.. py:function:: ExtractNonManifoldEdges(thisMesh, selective, multiple=False)

   Extracts, or removes, non-manifold mesh edges.

   :param bool selective: If true, then extract hanging faces only.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A mesh containing the extracted non-manifold parts if successful, None otherwise.
   :rtype: rhino3dm.Mesh
.. py:function:: HealNakedEdges(thisMesh, distance, multiple=False)

   Attempts to "heal" naked edges in a mesh based on a given distance.
   First attempts to move vertexes to neighboring vertexes that are within that
   distance away. Then it finds edges that have a closest point to the vertex within
   the distance and splits the edge. When it finds one it splits the edge and
   makes two new edges using that point.

   :param float distance: Distance to not exceed when modifying the mesh.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: FillHoles(thisMesh, multiple=False)

   Attempts to determine "holes" in the mesh by chaining naked edges together.
   Then it triangulates the closed polygons adds the faces to the mesh.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: FileHole(thisMesh, topologyEdgeIndex, multiple=False)

   Given a starting "naked" edge index, this function attempts to determine a "hole"
   by chaining additional naked edges together until if returns to the start index.
   Then it triangulates the closed polygon and either adds the faces to the mesh.

   :param int topologyEdgeIndex: Starting naked edge index.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: UnifyNormals(thisMesh, multiple=False)

   Attempts to fix inconsistencies in the directions of mesh faces in a mesh. This function
   does not modify mesh vertex normals, it rearranges the mesh face winding and face
   normals to make them all consistent. Note, you may want to call Mesh.Normals.ComputeNormals()
   to recompute vertex normals after calling this functions.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: number of faces that were modified.
   :rtype: int
.. py:function:: UnifyNormals1(thisMesh, countOnly, multiple=False)

   Attempts to fix inconsistencies in the directions of mesh faces in a mesh. This function
   does not modify mesh vertex normals, it rearranges the mesh face winding and face
   normals to make them all consistent. Note, you may want to call Mesh.Normals.ComputeNormals()
   to recompute vertex normals after calling this functions.

   :param bool countOnly: If true, then only the number of faces that would be modified is determined.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: If countOnly=false, the number of faces that were modified. If countOnly=true, the number of faces that would be modified.
   :rtype: int
.. py:function:: SplitDisjointPieces(thisMesh, multiple=False)

   Splits up the mesh into its unconnected pieces.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array containing all the disjoint pieces that make up this Mesh.
   :rtype: rhino3dm.Mesh[]
.. py:function:: Split(thisMesh, plane, multiple=False)

   Split a mesh by an infinite plane.

   :param rhino3dm.Plane plane: The splitting plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh array with the split result. This can be None if no result was found.
   :rtype: rhino3dm.Mesh[]
.. py:function:: Split1(thisMesh, mesh, multiple=False)

   Split a mesh with another mesh. Suggestion: upgrade to overload with tolerance.

   :param rhino3dm.Mesh mesh: Mesh to split with.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of mesh segments representing the split result.
   :rtype: rhino3dm.Mesh[]
.. py:function:: Split2(thisMesh, meshes, multiple=False)

   Split a mesh with a collection of meshes. Suggestion: upgrade to overload with tolerance.
   Does not split at coplanar intersections.

   :param list[rhino3dm.Mesh] meshes: Meshes to split with.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of mesh segments representing the split result.
   :rtype: rhino3dm.Mesh[]
.. py:function:: Split3(thisMesh, meshes, tolerance, splitAtCoplanar, textLog, cancel, progress, multiple=False)

   Split a mesh with a collection of meshes.

   :param list[rhino3dm.Mesh] meshes: Meshes to split with.
   :param float tolerance: A value for intersection tolerance. \
      WARNING! Correct values are typically in the (10e-8 - 10e-4) range.An option is to use the document tolerance diminished by a few orders or magnitude.
   :param bool splitAtCoplanar: If false, coplanar areas will not be separated.
   :param TextLog textLog: A text log to write onto.
   :param CancellationToken cancel: A cancellation token.
   :param IProgress<double> progress: A progress reporter item. This can be null.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of mesh parts representing the split result, or null: when no mesh intersected, or if a cancel stopped the computation.
   :rtype: rhino3dm.Mesh[]
.. py:function:: Split4(thisMesh, meshes, tolerance, splitAtCoplanar, createNgons, textLog, cancel, progress, multiple=False)

   Split a mesh with a collection of meshes.

   :param list[rhino3dm.Mesh] meshes: Meshes to split with.
   :param float tolerance: A value for intersection tolerance. \
      WARNING! Correct values are typically in the (10e-8 - 10e-4) range.An option is to use the document tolerance diminished by a few orders or magnitude.
   :param bool splitAtCoplanar: If false, coplanar areas will not be separated.
   :param bool createNgons: If true, creates ngons along the split ridge.
   :param TextLog textLog: A text log to write onto.
   :param CancellationToken cancel: A cancellation token.
   :param IProgress<double> progress: A progress reporter item. This can be null.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of mesh parts representing the split result, or null: when no mesh intersected, or if a cancel stopped the computation.
   :rtype: rhino3dm.Mesh[]
.. py:function:: GetOutlines(thisMesh, plane, multiple=False)

   Constructs the outlines of a mesh projected against a plane.

   :param rhino3dm.Plane plane: A plane to project against.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of polylines, or None on error.
   :rtype: rhino3dm.Polyline[]
.. py:function:: GetOutlines1(thisMesh, viewport, multiple=False)

   Constructs the outlines of a mesh. The projection information in the
   viewport is used to determine how the outlines are projected.

   :param Display.RhinoViewport viewport: A viewport to determine projection direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of polylines, or None on error.
   :rtype: rhino3dm.Polyline[]
.. py:function:: GetOutlines2(thisMesh, viewportInfo, plane, multiple=False)

   Constructs the outlines of a mesh.

   :param ViewportInfo viewportInfo: The viewport info that provides the outline direction.
   :param rhino3dm.Plane plane: Usually the view's construction plane. If a parallel projection and view plane is parallel to this, then project the results to the plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of polylines, or None on error.
   :rtype: rhino3dm.Polyline[]
.. py:function:: GetNakedEdges(thisMesh, multiple=False)

   Returns all edges of a mesh that are considered "naked" in the
   sense that the edge only has one face.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of polylines, or None on error.
   :rtype: rhino3dm.Polyline[]
.. py:function:: ExplodeAtUnweldedEdges(thisMesh, multiple=False)

   Explode the mesh into sub-meshes where a sub-mesh is a collection of faces that are contained
   within a closed loop of "unwelded" edges. Unwelded edges are edges where the faces that share
   the edge have unique mesh vertexes (not mesh topology vertexes) at both ends of the edge.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of sub-meshes on success; None on error. If the count in the returned array is 1, then \
      nothing happened and the output is essentially a copy of the input.
   :rtype: rhino3dm.Mesh[]
.. py:function:: ClosestPoint(thisMesh, testPoint, multiple=False)

   Gets the point on the mesh that is closest to a given test point.

   :param rhino3dm.Point3d testPoint: Point to search for.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The point on the mesh closest to testPoint, or Point3d.Unset on failure.
   :rtype: rhino3dm.Point3d
.. py:function:: ClosestMeshPoint(thisMesh, testPoint, maximumDistance, multiple=False)

   Gets the point on the mesh that is closest to a given test point. Similar to the
   ClosestPoint function except this returns a MeshPoint class which includes
   extra information beyond just the location of the closest point.

   :param rhino3dm.Point3d testPoint: The source of the search.
   :param float maximumDistance: Optional upper bound on the distance from test point to the mesh. \
      If you are only interested in finding a point Q on the mesh when \
      testPoint.DistanceTo(Q) < maximumDistance, \
      then set maximumDistance to that value. \
      This parameter is ignored if you pass 0.0 for a maximumDistance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: closest point information on success. None on failure.
   :rtype: MeshPoint
.. py:function:: ClosestPoint1(thisMesh, testPoint, maximumDistance, multiple=False)

   Gets the point on the mesh that is closest to a given test point.

   :param rhino3dm.Point3d testPoint: Point to search for.
   :param float maximumDistance: Optional upper bound on the distance from test point to the mesh. \
      If you are only interested in finding a point Q on the mesh when \
      testPoint.DistanceTo(Q) < maximumDistance, \
      then set maximumDistance to that value. \
      This parameter is ignored if you pass 0.0 for a maximumDistance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Index of face that the closest point lies on if successful. \
      -1 if not successful; the value of pointOnMesh is undefined.
   :rtype: int
.. py:function:: ClosestPoint2(thisMesh, testPoint, maximumDistance, multiple=False)

   Gets the point on the mesh that is closest to a given test point.

   :param rhino3dm.Point3d testPoint: Point to search for.
   :param float maximumDistance: Optional upper bound on the distance from test point to the mesh. \
      If you are only interested in finding a point Q on the mesh when \
      testPoint.DistanceTo(Q) < maximumDistance, \
      then set maximumDistance to that value. \
      This parameter is ignored if you pass 0.0 for a maximumDistance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Index of face that the closest point lies on if successful. \
      -1 if not successful; the value of pointOnMesh is undefined.
   :rtype: int
.. py:function:: PointAt(thisMesh, meshPoint, multiple=False)

   Evaluate a mesh at a set of barycentric coordinates.

   :param MeshPoint meshPoint: MeshPoint instance containing a valid Face Index and Barycentric coordinates.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.
   :rtype: rhino3dm.Point3d
.. py:function:: PointAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False)

   Evaluates a mesh at a set of barycentric coordinates. Barycentric coordinates must
   be assigned in accordance with the rules as defined by MeshPoint.T.

   :param int faceIndex: Index of triangle or quad to evaluate.
   :param float t0: First barycentric coordinate.
   :param float t1: Second barycentric coordinate.
   :param float t2: Third barycentric coordinate.
   :param float t3: Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.
   :rtype: rhino3dm.Point3d
.. py:function:: NormalAt(thisMesh, meshPoint, multiple=False)

   Evaluate a mesh normal at a set of barycentric coordinates.

   :param MeshPoint meshPoint: MeshPoint instance containing a valid Face Index and Barycentric coordinates.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.
   :rtype: rhino3dm.Vector3d
.. py:function:: NormalAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False)

   Evaluate a mesh normal at a set of barycentric coordinates. Barycentric coordinates must
   be assigned in accordance with the rules as defined by MeshPoint.T.

   :param int faceIndex: Index of triangle or quad to evaluate.
   :param float t0: First barycentric coordinate.
   :param float t1: Second barycentric coordinate.
   :param float t2: Third barycentric coordinate.
   :param float t3: Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.
   :rtype: rhino3dm.Vector3d
.. py:function:: ColorAt(thisMesh, meshPoint, multiple=False)

   Evaluate a mesh color at a set of barycentric coordinates.

   :param MeshPoint meshPoint: MeshPoint instance containing a valid Face Index and Barycentric coordinates.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The interpolated vertex color on the mesh or Color.Transparent if the faceIndex is not valid, \
      if the barycentric coordinates could not be evaluated, or if there are no colors defined on the mesh.
   :rtype: Color
.. py:function:: ColorAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False)

   Evaluate a mesh normal at a set of barycentric coordinates. Barycentric coordinates must
   be assigned in accordance with the rules as defined by MeshPoint.T.

   :param int faceIndex: Index of triangle or quad to evaluate.
   :param float t0: First barycentric coordinate.
   :param float t1: Second barycentric coordinate.
   :param float t2: Third barycentric coordinate.
   :param float t3: Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The interpolated vertex color on the mesh or Color.Transparent if the faceIndex is not valid, \
      if the barycentric coordinates could not be evaluated, or if there are no colors defined on the mesh.
   :rtype: Color
.. py:function:: PullPointsToMesh(thisMesh, points, multiple=False)

   Pulls a collection of points to a mesh.

   :param list[rhino3dm.Point3d] points: An array, a list or any enumerable set of points.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points. This can be empty.
   :rtype: rhino3dm.Point3d[]
.. py:function:: PullCurve(thisMesh, curve, tolerance, multiple=False)

   Gets a polyline approximation of the input curve and then moves its control points to the closest point on the mesh.
   Then it "connects the points" over edges so that a polyline on the mesh is formed.

   :param rhino3dm.Curve curve: A curve to pull.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A polyline curve, or None if none could be constructed.
   :rtype: PolylineCurve
.. py:function:: SplitWithProjectedPolylines(thisMesh, curves, tolerance, multiple=False)

   Splits a mesh by adding edges in correspondence with input polylines, and divides the mesh at partitioned areas.
   Polyline segments that are measured not to be on the mesh will be ignored.

   :param IEnumerable<PolylineCurve> curves: An array, a list or any enumerable of polyline curves.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of meshes, or None if no change would happen.
   :rtype: rhino3dm.Mesh[]
.. py:function:: SplitWithProjectedPolylines1(thisMesh, curves, tolerance, textLog, cancel, progress, multiple=False)

   Splits a mesh by adding edges in correspondence with input polylines, and divides the mesh at partitioned areas.
   Polyline segments that are measured not to be on the mesh will be ignored.

   :param IEnumerable<PolylineCurve> curves: An array, a list or any enumerable of polyline curves.
   :param float tolerance: A tolerance value.
   :param TextLog textLog: A text log, or null.
   :param CancellationToken cancel: A cancellation token to stop the computation at a given point.
   :param IProgress<double> progress: A progress reporter to inform the user about progress. The reported value is indicative.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of meshes, or None if no change would happen.
   :rtype: rhino3dm.Mesh[]
.. py:function:: Offset(thisMesh, distance, multiple=False)

   Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
   Same as Mesh.Offset(distance, false)

   :param float distance: A distance value to use for offsetting.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh on success, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: Offset1(thisMesh, distance, solidify, multiple=False)

   Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
   Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
   If solidify is False it acts exactly as the Offset(distance) function.

   :param float distance: A distance value.
   :param bool solidify: True if the mesh should be solidified.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh on success, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: Offset2(thisMesh, distance, solidify, direction, multiple=False)

   Makes a new mesh with vertices offset a distance along the direction parameter.
   Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
   If solidify is False it acts exactly as the Offset(distance) function.

   :param float distance: A distance value.
   :param bool solidify: True if the mesh should be solidified.
   :param rhino3dm.Vector3d direction: Direction of offset for all vertices.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh on success, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: Offset3(thisMesh, distance, solidify, direction, multiple=False)

   Makes a new mesh with vertices offset a distance along the direction parameter.
   Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
   If solidify is False it acts exactly as the Offset(distance) function. Returns list of wall faces, i.e. the
   faces that connect original and offset mesh when solidified.

   :param float distance: A distance value.
   :param bool solidify: True if the mesh should be solidified.
   :param rhino3dm.Vector3d direction: Direction of offset for all vertices.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh on success, or None on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: CollapseFacesByEdgeLength(thisMesh, bGreaterThan, edgeLength, multiple=False)

   Collapses multiple mesh faces, with greater/less than edge length, based on the principles
   found in Stan Melax's mesh reduction PDF,
   see http://pomax.nihongoresources.com/downloads/PolygonReduction.pdf

   :param bool bGreaterThan: Determines whether edge with lengths greater than or less than edgeLength are collapsed.
   :param float edgeLength: Length with which to compare to edge lengths.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Number of edges (faces) that were collapsed.
   :rtype: int
.. py:function:: CollapseFacesByArea(thisMesh, lessThanArea, greaterThanArea, multiple=False)

   Collapses multiple mesh faces, with areas less than LessThanArea and greater than GreaterThanArea,
   based on the principles found in Stan Melax's mesh reduction PDF,
   see http://pomax.nihongoresources.com/downloads/PolygonReduction.pdf

   :param float lessThanArea: Area in which faces are selected if their area is less than or equal to.
   :param float greaterThanArea: Area in which faces are selected if their area is greater than or equal to.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Number of faces that were collapsed in the process.
   :rtype: int
.. py:function:: CollapseFacesByByAspectRatio(thisMesh, aspectRatio, multiple=False)

   Collapses a multiple mesh faces, determined by face aspect ratio, based on criteria found in Stan Melax's polygon reduction,
   see http://pomax.nihongoresources.com/downloads/PolygonReduction.pdf

   :param float aspectRatio: Faces with an aspect ratio less than aspectRatio are considered as candidates.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Number of faces that were collapsed in the process.
   :rtype: int
.. py:function:: GetUnsafeLock(thisMesh, writable, multiple=False)

   Allows to obtain unsafe pointers to the underlying unmanaged data structures of the mesh.

   :param bool writable: True if user will need to write onto the structure. False otherwise.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A lock that needs to be released.
   :rtype: MeshUnsafeLock
.. py:function:: ReleaseUnsafeLock(thisMesh, meshData, multiple=False)

   Updates the Mesh data with the information that was stored via the .

   :param MeshUnsafeLock meshData: The data that will be unlocked.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: void
.. py:function:: WithShutLining(thisMesh, faceted, tolerance, curves, multiple=False)

   Constructs new mesh from the current one, with shut lining applied to it.

   :param bool faceted: Specifies whether the shutline is faceted.
   :param float tolerance: The tolerance of the shutline.
   :param IEnumerable<ShutLiningCurveInfo> curves: A collection of curve arguments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh with shutlining. Null on failure.
   :rtype: rhino3dm.Mesh
.. py:function:: WithDisplacement(thisMesh, displacement, multiple=False)

   Constructs new mesh from the current one, with displacement applied to it.

   :param MeshDisplacementInfo displacement: Information on mesh displacement.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh with shutlining.
   :rtype: rhino3dm.Mesh
.. py:function:: WithEdgeSoftening(thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold, multiple=False)

   Constructs new mesh from the current one, with edge softening applied to it.

   :param float softeningRadius: The softening radius.
   :param bool chamfer: Specifies whether to chamfer the edges.
   :param bool faceted: Specifies whether the edges are faceted.
   :param bool force: Specifies whether to soften edges despite too large a radius.
   :param float angleThreshold: Threshold angle (in degrees) which controls whether an edge is softened or not. \
      The angle refers to the angles between the adjacent faces of an edge.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new mesh with soft edges.
   :rtype: rhino3dm.Mesh
.. py:function:: QuadRemeshBrep(brep, parameters, multiple=False)

   Create QuadRemesh from a Brep
   Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: QuadRemeshBrep1(brep, parameters, guideCurves, multiple=False)

   Create Quad Remesh from a Brep

   :param rhino3dm.Brep brep: Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
   :param list[rhino3dm.Curve] guideCurves: A curve array used to influence mesh face layout \
      The curves should touch the input mesh \
      Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: QuadRemeshBrepAsync(brep, parameters, progress, cancelToken, multiple=False)

   Quad remesh this Brep asynchronously.

   :param rhino3dm.Brep brep: Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: Task<Mesh>
.. py:function:: QuadRemeshBrepAsync1(brep, parameters, guideCurves, progress, cancelToken, multiple=False)

   Quad remesh this Brep asynchronously.

   :param rhino3dm.Brep brep: Set Brep Face Mode by setting QuadRemeshParameters.PreserveMeshArrayEdgesMode
   :param list[rhino3dm.Curve] guideCurves: A curve array used to influence mesh face layout \
      The curves should touch the input mesh \
      Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: Task<Mesh>
.. py:function:: QuadRemesh(thisMesh, parameters, multiple=False)

   Quad remesh this mesh.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: QuadRemesh1(thisMesh, parameters, guideCurves, multiple=False)

   Quad remesh this mesh.

   :param list[rhino3dm.Curve] guideCurves: A curve array used to influence mesh face layout \
      The curves should touch the input mesh \
      Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Mesh
.. py:function:: QuadRemeshAsync(thisMesh, parameters, progress, cancelToken, multiple=False)

   Quad remesh this mesh asynchronously.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: Task<Mesh>
.. py:function:: QuadRemeshAsync1(thisMesh, parameters, guideCurves, progress, cancelToken, multiple=False)

   Quad remesh this mesh asynchronously.

   :param list[rhino3dm.Curve] guideCurves: A curve array used to influence mesh face layout \
      The curves should touch the input mesh \
      Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: Task<Mesh>
.. py:function:: QuadRemeshAsync2(thisMesh, faceBlocks, parameters, guideCurves, progress, cancelToken, multiple=False)

   Quad remesh this mesh asynchronously.

   :param list[rhino3dm.Curve] guideCurves: A curve array used to influence mesh face layout \
      The curves should touch the input mesh \
      Set Guide Curve Influence by using QuadRemeshParameters.GuideCurveInfluence
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: Task<Mesh>
.. py:function:: Reduce(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, multiple=False)

   Reduce polygon count

   :param int desiredPolygonCount: desired or target number of faces
   :param bool allowDistortion: If True mesh appearance is not changed even if the target polygon count is not reached
   :param int accuracy: Integer from 1 to 10 telling how accurate reduction algorithm \
      to use. Greater number gives more accurate results
   :param bool normalizeSize: If True mesh is fitted to an axis aligned unit cube until reduction is complete
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
   :rtype: bool
.. py:function:: Reduce1(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, threaded, multiple=False)

   Reduce polygon count

   :param int desiredPolygonCount: desired or target number of faces
   :param bool allowDistortion: If True mesh appearance is not changed even if the target polygon count is not reached
   :param int accuracy: Integer from 1 to 10 telling how accurate reduction algorithm \
      to use. Greater number gives more accurate results
   :param bool normalizeSize: If True mesh is fitted to an axis aligned unit cube until reduction is complete
   :param bool threaded: If True then will run computation inside a worker thread and ignore any provided CancellationTokens and ProgressReporters. \
      If False then will run on main thread.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
   :rtype: bool
.. py:function:: Reduce2(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, cancelToken, progress, multiple=False)

   Reduce polygon count

   :param int desiredPolygonCount: desired or target number of faces
   :param bool allowDistortion: If True mesh appearance is not changed even if the target polygon count is not reached
   :param int accuracy: Integer from 1 to 10 telling how accurate reduction algorithm \
      to use. Greater number gives more accurate results
   :param bool normalizeSize: If True mesh is fitted to an axis aligned unit cube until reduction is complete
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
   :rtype: bool
.. py:function:: Reduce3(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, cancelToken, progress, threaded, multiple=False)

   Reduce polygon count

   :param int desiredPolygonCount: desired or target number of faces
   :param bool allowDistortion: If True mesh appearance is not changed even if the target polygon count is not reached
   :param int accuracy: Integer from 1 to 10 telling how accurate reduction algorithm \
      to use. Greater number gives more accurate results
   :param bool normalizeSize: If True mesh is fitted to an axis aligned unit cube until reduction is complete
   :param bool threaded: If True then will run computation inside a worker thread and ignore any provided CancellationTokens and ProgressReporters. \
      If False then will run on main thread.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
   :rtype: bool
.. py:function:: Reduce4(thisMesh, parameters, multiple=False)

   Reduce polygon count

   :param ReduceMeshParameters parameters: Parameters
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
   :rtype: bool
.. py:function:: Reduce5(thisMesh, parameters, threaded, multiple=False)

   Reduce polygon count

   :param ReduceMeshParameters parameters: Parameters
   :param bool threaded: If True then will run computation inside a worker thread and ignore any provided CancellationTokens and ProgressReporters. \
      If False then will run on main thread.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if mesh is successfully reduced and False if mesh could not be reduced for some reason.
   :rtype: bool
.. py:function:: ComputeThickness(meshes, maximumThickness, multiple=False)

   Compute thickness metrics for this mesh.

   :param list[rhino3dm.Mesh] meshes: Meshes to include in thickness analysis.
   :param float maximumThickness: Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of thickness measurements.
   :rtype: MeshThicknessMeasurement[]
.. py:function:: ComputeThickness1(meshes, maximumThickness, cancelToken, multiple=False)

   Compute thickness metrics for this mesh.

   :param list[rhino3dm.Mesh] meshes: Meshes to include in thickness analysis.
   :param float maximumThickness: Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.
   :param System.Threading.CancellationToken cancelToken: Computation cancellation token.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of thickness measurements.
   :rtype: MeshThicknessMeasurement[]
.. py:function:: ComputeThickness2(meshes, maximumThickness, sharpAngle, cancelToken, multiple=False)

   Compute thickness metrics for this mesh.

   :param list[rhino3dm.Mesh] meshes: Meshes to include in thickness analysis.
   :param float maximumThickness: Maximum thickness to consider. Use as small a thickness as possible to speed up the solver.
   :param float sharpAngle: Sharpness angle in radians.
   :param System.Threading.CancellationToken cancelToken: Computation cancellation token.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of thickness measurements.
   :rtype: MeshThicknessMeasurement[]
.. py:function:: CreateContourCurves(meshToContour, contourStart, contourEnd, interval, multiple=False)

   Constructs contour curves for a mesh, sectioned along a linear axis.

   :param rhino3dm.Mesh meshToContour: A mesh to contour.
   :param rhino3dm.Point3d contourStart: A start point of the contouring axis.
   :param rhino3dm.Point3d contourEnd: An end point of the contouring axis.
   :param float interval: An interval distance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateContourCurves1(meshToContour, sectionPlane, multiple=False)

   Constructs contour curves for a mesh, sectioned at a plane.

   :param rhino3dm.Mesh meshToContour: A mesh to contour.
   :param rhino3dm.Plane sectionPlane: A cutting plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
