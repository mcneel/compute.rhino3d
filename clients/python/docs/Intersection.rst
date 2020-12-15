Intersection
============

.. py:module:: compute_rhino3d.Intersection

.. py:function:: CurvePlane(curve, plane, tolerance, multiple=False)

   Intersects a curve with an (infinite) plane.

   :param rhino3dm.Curve curve: Curve to intersect.
   :param rhino3dm.Plane plane: Plane to intersect with.
   :param float tolerance: Tolerance to use during intersection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A list of intersection events or None if no intersections were recorded.
   :rtype: CurveIntersections
.. py:function:: MeshPlane(mesh, plane, multiple=False)

   Intersects a mesh with an (infinite) plane.

   :param rhino3dm.Mesh mesh: Mesh to intersect.
   :param rhino3dm.Plane plane: Plane to intersect with.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of polylines describing the intersection loops or None (Nothing in Visual Basic) if no intersections could be found.
   :rtype: rhino3dm.Polyline[]
.. py:function:: MeshPlane1(mesh, planes, multiple=False)

   Intersects a mesh with a collection of (infinite) planes.

   :param rhino3dm.Mesh mesh: Mesh to intersect.
   :param list[rhino3dm.Plane] planes: Planes to intersect with.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of polylines describing the intersection loops or None (Nothing in Visual Basic) if no intersections could be found.
   :rtype: rhino3dm.Polyline[]
.. py:function:: BrepPlane(brep, plane, tolerance, multiple=False)

   Intersects a Brep with an (infinite) plane.

   :param rhino3dm.Brep brep: Brep to intersect.
   :param rhino3dm.Plane plane: Plane to intersect with.
   :param float tolerance: Tolerance to use for intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: CurveSelf(curve, tolerance, multiple=False)

   Finds the places where a curve intersects itself.

   :param rhino3dm.Curve curve: Curve for self-intersections.
   :param float tolerance: Intersection tolerance. If the curve approaches itself to within tolerance, \
      an intersection is assumed.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A collection of intersection events.
   :rtype: CurveIntersections
.. py:function:: CurveCurve(curveA, curveB, tolerance, overlapTolerance, multiple=False)

   Finds the intersections between two curves.

   :param rhino3dm.Curve curveA: First curve for intersection.
   :param rhino3dm.Curve curveB: Second curve for intersection.
   :param float tolerance: Intersection tolerance. If the curves approach each other to within tolerance, an intersection is assumed.
   :param float overlapTolerance: The tolerance with which the curves are tested.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A collection of intersection events.
   :rtype: CurveIntersections
.. py:function:: CurveCurveValidate(curveA, curveB, tolerance, overlapTolerance, multiple=False)

   Finds the intersections between two curves.

   :param rhino3dm.Curve curveA: First curve for intersection.
   :param rhino3dm.Curve curveB: Second curve for intersection.
   :param float tolerance: Intersection tolerance. If the curves approach each other to within tolerance, an intersection is assumed.
   :param float overlapTolerance: The tolerance with which the curves are tested.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A collection of intersection events.
   :rtype: CurveIntersections
.. py:function:: CurveLine(curve, line, tolerance, overlapTolerance, multiple=False)

   Intersects a curve and an infinite line.

   :param rhino3dm.Curve curve: Curve for intersection.
   :param Line line: Infinite line to intersect.
   :param float tolerance: Intersection tolerance. If the curves approach each other to within tolerance, an intersection is assumed.
   :param float overlapTolerance: The tolerance with which the curves are tested.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A collection of intersection events.
   :rtype: CurveIntersections
.. py:function:: CurveSurface(curve, surface, tolerance, overlapTolerance, multiple=False)

   Intersects a curve and a surface.

   :param rhino3dm.Curve curve: Curve for intersection.
   :param rhino3dm.Surface surface: Surface for intersection.
   :param float tolerance: Intersection tolerance. If the curve approaches the surface to within tolerance, an intersection is assumed.
   :param float overlapTolerance: The tolerance with which the curves are tested.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A collection of intersection events.
   :rtype: CurveIntersections
.. py:function:: CurveSurfaceValidate(curve, surface, tolerance, overlapTolerance, multiple=False)

   Intersects a curve and a surface.

   :param rhino3dm.Curve curve: Curve for intersection.
   :param rhino3dm.Surface surface: Surface for intersection.
   :param float tolerance: Intersection tolerance. If the curve approaches the surface to within tolerance, an intersection is assumed.
   :param float overlapTolerance: The tolerance with which the curves are tested.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A collection of intersection events.
   :rtype: CurveIntersections
.. py:function:: CurveSurface1(curve, curveDomain, surface, tolerance, overlapTolerance, multiple=False)

   Intersects a sub-curve and a surface.

   :param rhino3dm.Curve curve: Curve for intersection.
   :param rhino3dm.Interval curveDomain: Domain of sub-curve to take into consideration for Intersections.
   :param rhino3dm.Surface surface: Surface for intersection.
   :param float tolerance: Intersection tolerance. If the curve approaches the surface to within tolerance, an intersection is assumed.
   :param float overlapTolerance: The tolerance with which the curves are tested.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A collection of intersection events.
   :rtype: CurveIntersections
.. py:function:: CurveSurfaceValidate1(curve, curveDomain, surface, tolerance, overlapTolerance, multiple=False)

   Intersects a sub-curve and a surface.

   :param rhino3dm.Curve curve: Curve for intersection.
   :param rhino3dm.Interval curveDomain: Domain of sub-curve to take into consideration for Intersections.
   :param rhino3dm.Surface surface: Surface for intersection.
   :param float tolerance: Intersection tolerance. If the curve approaches the surface to within tolerance, an intersection is assumed.
   :param float overlapTolerance: The tolerance with which the curves are tested.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A collection of intersection events.
   :rtype: CurveIntersections
.. py:function:: CurveBrep(curve, brep, tolerance, multiple=False)

   Intersects a curve with a Brep. This function returns the 3D points of intersection
   and 3D overlap curves. If an error occurs while processing overlap curves, this function
   will return false, but it will still provide partial results.

   :param rhino3dm.Curve curve: Curve for intersection.
   :param rhino3dm.Brep brep: Brep for intersection.
   :param float tolerance: Fitting and near miss tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: CurveBrep1(curve, brep, tolerance, angleTolerance, multiple=False)

   Intersect a curve with a Brep. This function returns the intersection parameters on the curve.

   :param rhino3dm.Curve curve: Curve.
   :param rhino3dm.Brep brep: Brep.
   :param float tolerance: Absolute tolerance for intersections.
   :param float angleTolerance: Angle tolerance in radians.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: CurveBrepFace(curve, face, tolerance, multiple=False)

   Intersects a curve with a Brep face.

   :param rhino3dm.Curve curve: A curve.
   :param rhino3dm.BrepFace face: A brep face.
   :param float tolerance: Fitting and near miss tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: SurfaceSurface(surfaceA, surfaceB, tolerance, multiple=False)

   Intersects two Surfaces.

   :param rhino3dm.Surface surfaceA: First Surface for intersection.
   :param rhino3dm.Surface surfaceB: Second Surface for intersection.
   :param float tolerance: Intersection tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: BrepBrep(brepA, brepB, tolerance, multiple=False)

   Intersects two Breps.

   :param rhino3dm.Brep brepA: First Brep for intersection.
   :param rhino3dm.Brep brepB: Second Brep for intersection.
   :param float tolerance: Intersection tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success; False on failure.
   :rtype: bool
.. py:function:: BrepSurface(brep, surface, tolerance, multiple=False)

   Intersects a Brep and a Surface.

   :param rhino3dm.Brep brep: A brep to be intersected.
   :param rhino3dm.Surface surface: A surface to be intersected.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success; False on failure.
   :rtype: bool
.. py:function:: MeshMeshFast(meshA, meshB, multiple=False)

   This is an old overload kept for compatibility. Overlaps and near misses are ignored.

   :param rhino3dm.Mesh meshA: First mesh for intersection.
   :param rhino3dm.Mesh meshB: Second mesh for intersection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of intersection line segments, or None if no intersections were found.
   :rtype: Line[]
.. py:function:: MeshMeshAccurate(meshA, meshB, tolerance, multiple=False)

   Intersects two meshes. Overlaps and near misses are handled. This is an old method kept for compatibility.

   :param rhino3dm.Mesh meshA: First mesh for intersection.
   :param rhino3dm.Mesh meshB: Second mesh for intersection.
   :param float tolerance: A tolerance value. If negative, the positive value will be used. \
      WARNING! Good tolerance values are in the magnitude of 10^-7, or RhinoMath.SqrtEpsilon*10.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of intersection and overlaps polylines.
   :rtype: rhino3dm.Polyline[]
.. py:function:: MeshRay(mesh, ray, multiple=False)

   Finds the first intersection of a ray with a mesh.

   :param rhino3dm.Mesh mesh: A mesh to intersect.
   :param Ray3d ray: A ray to be casted.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: >= 0.0 parameter along ray if successful. \
      < 0.0 if no intersection found.
   :rtype: float
.. py:function:: MeshRay1(mesh, ray, multiple=False)

   Finds the first intersection of a ray with a mesh.

   :param rhino3dm.Mesh mesh: A mesh to intersect.
   :param Ray3d ray: A ray to be casted.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: >= 0.0 parameter along ray if successful. \
      < 0.0 if no intersection found.
   :rtype: float
.. py:function:: MeshPolyline(mesh, curve, multiple=False)

   Finds the intersection of a mesh and a polyline. Points are not guaranteed to be sorted along the polyline.

   :param rhino3dm.Mesh mesh: A mesh to intersect.
   :param PolylineCurve curve: A polyline curves to intersect.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points: one for each face that was passed by the faceIds out reference.
   :rtype: rhino3dm.Point3d[]
.. py:function:: MeshPolylineSorted(mesh, curve, multiple=False)

   Finds the intersection of a mesh and a polyline. Points are guaranteed to be sorted along the polyline.

   :param rhino3dm.Mesh mesh: A mesh to intersect.
   :param PolylineCurve curve: A polyline curves to intersect.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points: one for each face that was passed by the faceIds out reference.
   :rtype: rhino3dm.Point3d[]
.. py:function:: MeshLine(mesh, line, multiple=False)

   Finds the intersections of a mesh and a line. The points are not necessarily sorted.

   :param rhino3dm.Mesh mesh: A mesh to intersect
   :param Line line: The line to intersect with the mesh
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points: one for each face that was passed by the faceIds out reference.
   :rtype: rhino3dm.Point3d[]
.. py:function:: MeshLineSorted(mesh, line, multiple=False)

   Finds the intersections of a mesh and a line. Points are sorted along the line.

   :param rhino3dm.Mesh mesh: A mesh to intersect
   :param Line line: The line to intersect with the mesh
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points: one for each face that was passed by the faceIds out reference.
   :rtype: rhino3dm.Point3d[]
.. py:function:: RayShoot(ray, geometry, maxReflections, multiple=False)

   Computes point intersections that occur when shooting a ray to a collection of surfaces and Breps.

   :param Ray3d ray: A ray used in intersection.
   :param list[rhino3dm.GeometryBase] geometry: Only Surface and Brep objects are currently supported. Trims are ignored on Breps.
   :param int maxReflections: The maximum number of reflections. This value should be any value between 1 and 1000, inclusive.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points: one for each surface or Brep face that was hit, or an empty array on failure.
   :rtype: rhino3dm.Point3d[]
.. py:function:: RayShoot1(geometry, ray, maxReflections, multiple=False)

   Computes point intersections that occur when shooting a ray to a collection of surfaces and Breps.

   :param list[rhino3dm.GeometryBase] geometry: The collection of surfaces and Breps to intersect. Trims are ignored on Breps.
   :param Ray3d ray: >A ray used in intersection.
   :param int maxReflections: The maximum number of reflections. This value should be any value between 1 and 1000, inclusive.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of RayShootEvent structs if successful, or an empty array on failure.
   :rtype: RayShootEvent[]
.. py:function:: ProjectPointsToMeshes(meshes, points, direction, tolerance, multiple=False)

   Projects points onto meshes.

   :param list[rhino3dm.Mesh] meshes: the meshes to project on to.
   :param list[rhino3dm.Point3d] points: the points to project.
   :param rhino3dm.Vector3d direction: the direction to project.
   :param float tolerance: Projection tolerances used for culling close points and for line-mesh intersection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of projected points, or None in case of any error or invalid input.
   :rtype: rhino3dm.Point3d[]
.. py:function:: ProjectPointsToMeshesEx(meshes, points, direction, tolerance, multiple=False)

   Projects points onto meshes.

   :param list[rhino3dm.Mesh] meshes: the meshes to project on to.
   :param list[rhino3dm.Point3d] points: the points to project.
   :param rhino3dm.Vector3d direction: the direction to project.
   :param float tolerance: Projection tolerances used for culling close points and for line-mesh intersection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of projected points, or None in case of any error or invalid input.
   :rtype: rhino3dm.Point3d[]
.. py:function:: ProjectPointsToBreps(breps, points, direction, tolerance, multiple=False)

   Projects points onto breps.

   :param list[rhino3dm.Brep] breps: The breps projection targets.
   :param list[rhino3dm.Point3d] points: The points to project.
   :param rhino3dm.Vector3d direction: The direction to project.
   :param float tolerance: The tolerance used for intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of projected points, or None in case of any error or invalid input.
   :rtype: rhino3dm.Point3d[]
.. py:function:: ProjectPointsToBrepsEx(breps, points, direction, tolerance, multiple=False)

   Projects points onto breps.

   :param list[rhino3dm.Brep] breps: The breps projection targets.
   :param list[rhino3dm.Point3d] points: The points to project.
   :param rhino3dm.Vector3d direction: The direction to project.
   :param float tolerance: The tolerance used for intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of projected points, or None in case of any error or invalid input.
   :rtype: rhino3dm.Point3d[]
