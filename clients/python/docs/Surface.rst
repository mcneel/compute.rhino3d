Surface
=======

.. py:module:: compute_rhino3d.Surface

.. py:function:: CreateRollingBallFillet(surfaceA, surfaceB, radius, tolerance, multiple=False)

   Constructs a rolling ball fillet between two surfaces.

   :param rhino3dm.Surface surfaceA: A first surface.
   :param rhino3dm.Surface surfaceB: A second surface.
   :param float radius: A radius value.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of rolling ball fillet surfaces; this array can be empty on failure.
   :rtype: rhino3dm.Surface[]
.. py:function:: CreateRollingBallFillet1(surfaceA, flipA, surfaceB, flipB, radius, tolerance, multiple=False)

   Constructs a rolling ball fillet between two surfaces.

   :param rhino3dm.Surface surfaceA: A first surface.
   :param bool flipA: A value that indicates whether A should be used in flipped mode.
   :param rhino3dm.Surface surfaceB: A second surface.
   :param bool flipB: A value that indicates whether B should be used in flipped mode.
   :param float radius: A radius value.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of rolling ball fillet surfaces; this array can be empty on failure.
   :rtype: rhino3dm.Surface[]
.. py:function:: CreateRollingBallFillet2(surfaceA, uvA, surfaceB, uvB, radius, tolerance, multiple=False)

   Constructs a rolling ball fillet between two surfaces.

   :param rhino3dm.Surface surfaceA: A first surface.
   :param rhino3dm.Point2d uvA: A point in the parameter space of FaceA near where the fillet is expected to hit the surface.
   :param rhino3dm.Surface surfaceB: A second surface.
   :param rhino3dm.Point2d uvB: A point in the parameter space of FaceB near where the fillet is expected to hit the surface.
   :param float radius: A radius value.
   :param float tolerance: A tolerance value used for approximating and intersecting offset surfaces.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of rolling ball fillet surfaces; this array can be empty on failure.
   :rtype: rhino3dm.Surface[]
.. py:function:: CreateExtrusion(profile, direction, multiple=False)

   Constructs a surface by extruding a curve along a vector.

   :param rhino3dm.Curve profile: Profile curve to extrude.
   :param rhino3dm.Vector3d direction: Direction and length of extrusion.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A surface on success or None on failure.
   :rtype: rhino3dm.Surface
.. py:function:: CreateExtrusionToPoint(profile, apexPoint, multiple=False)

   Constructs a surface by extruding a curve to a point.

   :param rhino3dm.Curve profile: Profile curve to extrude.
   :param rhino3dm.Point3d apexPoint: Apex point of extrusion.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A Surface on success or None on failure.
   :rtype: rhino3dm.Surface
.. py:function:: CreatePeriodicSurface(surface, direction, multiple=False)

   Constructs a periodic surface from a base surface and a direction.

   :param rhino3dm.Surface surface: The surface to make periodic.
   :param int direction: The direction to make periodic, either 0 = U, or 1 = V.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A Surface on success or None on failure.
   :rtype: rhino3dm.Surface
.. py:function:: CreatePeriodicSurface1(surface, direction, bSmooth, multiple=False)

   Constructs a periodic surface from a base surface and a direction.

   :param rhino3dm.Surface surface: The surface to make periodic.
   :param int direction: The direction to make periodic, either 0 = U, or 1 = V.
   :param bool bSmooth: Controls kink removal. If true, smooths any kinks in the surface and moves control points \
      to make a smooth surface. If false, control point locations are not changed or changed minimally \
      (only one point may move) and only the knot vector is altered.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A periodic surface if successful, None on failure.
   :rtype: rhino3dm.Surface
.. py:function:: CreateSoftEditSurface(surface, uv, delta, uLength, vLength, tolerance, fixEnds, multiple=False)

   Creates a soft edited surface from an existing surface using a smooth field of influence.

   :param rhino3dm.Surface surface: The surface to soft edit.
   :param rhino3dm.Point2d uv: A point in the parameter space to move from. This location on the surface is moved, \
      and the move is smoothly tapered off with increasing distance along the surface from \
      this parameter.
   :param rhino3dm.Vector3d delta: The direction and magnitude, or maximum distance, of the move.
   :param float uLength: The distance along the surface's u-direction from the editing point over which the \
      strength of the editing falls off smoothly.
   :param float vLength: The distance along the surface's v-direction from the editing point over which the \
      strength of the editing falls off smoothly.
   :param float tolerance: The active document's model absolute tolerance.
   :param bool fixEnds: Keeps edge locations fixed.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The soft edited surface if successful. None on failure.
   :rtype: rhino3dm.Surface
.. py:function:: Smooth(thisSurface, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, multiple=False)

   Smooths a surface by averaging the positions of control points in a specified region.

   :param float smoothFactor: The smoothing factor, which controls how much control points move towards the average of the neighboring control points.
   :param bool bXSmooth: When True control points move in X axis direction.
   :param bool bYSmooth: When True control points move in Y axis direction.
   :param bool bZSmooth: When True control points move in Z axis direction.
   :param bool bFixBoundaries: When True the surface edges don't move.
   :param SmoothingCoordinateSystem coordinateSystem: The coordinates to determine the direction of the smoothing.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The smoothed surface if successful, None otherwise.
   :rtype: rhino3dm.Surface
.. py:function:: Smooth1(thisSurface, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False)

   Smooths a surface by averaging the positions of control points in a specified region.

   :param float smoothFactor: The smoothing factor, which controls how much control points move towards the average of the neighboring control points.
   :param bool bXSmooth: When True control points move in X axis direction.
   :param bool bYSmooth: When True control points move in Y axis direction.
   :param bool bZSmooth: When True control points move in Z axis direction.
   :param bool bFixBoundaries: When True the surface edges don't move.
   :param SmoothingCoordinateSystem coordinateSystem: The coordinates to determine the direction of the smoothing.
   :param rhino3dm.Plane plane: If SmoothingCoordinateSystem.CPlane specified, then the construction plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The smoothed surface if successful, None otherwise.
   :rtype: rhino3dm.Surface
.. py:function:: VariableOffset(thisSurface, uMinvMin, uMinvMax, uMaxvMin, uMaxvMax, tolerance, multiple=False)

   Copies a surface so that all locations at the corners of the copied surface are specified distances from the original surface.

   :param float uMinvMin: Offset distance at Domain(0).Min, Domain(1).Min.
   :param float uMinvMax: Offset distance at Domain(0).Min, Domain(1).Max.
   :param float uMaxvMin: Offset distance at Domain(0).Max, Domain(1).Min.
   :param float uMaxvMax: Offset distance at Domain(0).Max, Domain(1).Max.
   :param float tolerance: The offset tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The offset surface if successful, None otherwise.
   :rtype: rhino3dm.Surface
.. py:function:: VariableOffset1(thisSurface, uMinvMin, uMinvMax, uMaxvMin, uMaxvMax, interiorParameters, interiorDistances, tolerance, multiple=False)

   Copies a surface so that all locations at the corners, and from specified interior locations, of the copied surface are specified distances from the original surface.

   :param float uMinvMin: Offset distance at Domain(0).Min, Domain(1).Min.
   :param float uMinvMax: Offset distance at Domain(0).Min, Domain(1).Max.
   :param float uMaxvMin: Offset distance at Domain(0).Max, Domain(1).Min.
   :param float uMaxvMax: Offset distance at Domain(0).Max, Domain(1).Max.
   :param list[rhino3dm.Point2d] interiorParameters: An array of interior UV parameters to offset from.
   :param list[float] interiorDistances: >An array of offset distances at the interior UV parameters.
   :param float tolerance: The offset tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The offset surface if successful, None otherwise.
   :rtype: rhino3dm.Surface
.. py:function:: GetSurfaceSize(thisSurface, multiple=False)

   Gets an estimate of the size of the rectangle that would be created
   if the 3d surface where flattened into a rectangle.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful.
   :rtype: bool
.. py:function:: ClosestSide(thisSurface, u, v, multiple=False)

   Gets the side that is closest, in terms of 3D-distance, to a U and V parameter.

   :param float u: A u parameter.
   :param float v: A v parameter.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A side.
   :rtype: IsoStatus
.. py:function:: Extend(thisSurface, edge, extensionLength, smooth, multiple=False)

   Extends an untrimmed surface along one edge.

   :param IsoStatus edge: Edge to extend.  Must be North, South, East, or West.
   :param float extensionLength: distance to extend.
   :param bool smooth: True for smooth (C-infinity) extension. \
      False for a C1- ruled extension.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New extended surface on success.
   :rtype: rhino3dm.Surface
.. py:function:: Rebuild(thisSurface, uDegree, vDegree, uPointCount, vPointCount, multiple=False)

   Rebuilds an existing surface to a given degree and point count.

   :param int uDegree: the output surface u degree.
   :param int vDegree: the output surface u degree.
   :param int uPointCount: The number of points in the output surface u direction. Must be bigger \
      than uDegree (maximum value is 1000)
   :param int vPointCount: The number of points in the output surface v direction. Must be bigger \
      than vDegree (maximum value is 1000)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: new rebuilt surface on success. None on failure.
   :rtype: NurbsSurface
.. py:function:: RebuildOneDirection(thisSurface, direction, pointCount, loftType, refitTolerance, multiple=False)

   Rebuilds an existing surface with a new surface to a given point count in either the u or v directions independently.

   :param int direction: The direction (0 = U, 1 = V).
   :param int pointCount: The number of points in the output surface in the "direction" direction.
   :param LoftType loftType: The loft type
   :param float refitTolerance: The refit tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: new rebuilt surface on success. None on failure.
   :rtype: NurbsSurface
.. py:function:: ClosestPoint(thisSurface, testPoint, multiple=False)

   Input the parameters of the point on the surface that is closest to testPoint.

   :param rhino3dm.Point3d testPoint: A point to test against.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: LocalClosestPoint(thisSurface, testPoint, seedU, seedV, multiple=False)

   Find parameters of the point on a surface that is locally closest to
   the testPoint. The search for a local close point starts at seed parameters.

   :param rhino3dm.Point3d testPoint: A point to test against.
   :param float seedU: The seed parameter in the U direction.
   :param float seedV: The seed parameter in the V direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if the search is successful, False if the search fails.
   :rtype: bool
.. py:function:: Offset(thisSurface, distance, tolerance, multiple=False)

   Constructs a new surface which is offset from the current surface.

   :param float distance: Distance (along surface normal) to offset.
   :param float tolerance: Offset accuracy.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The offset surface or None on failure.
   :rtype: rhino3dm.Surface
.. py:function:: Fit(thisSurface, uDegree, vDegree, fitTolerance, multiple=False)

   Fits a new surface through an existing surface.

   :param int uDegree: the output surface U degree. Must be bigger than 1.
   :param int vDegree: the output surface V degree. Must be bigger than 1.
   :param float fitTolerance: The fitting tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A surface, or None on error.
   :rtype: rhino3dm.Surface
.. py:function:: InterpolatedCurveOnSurfaceUV(thisSurface, points, tolerance, multiple=False)

   Returns a curve that interpolates points on a surface. The interpolant lies on the surface.

   :param System.Collections.Generic.IEnumerable<Point2d> points: List of at least two UV parameter locations on the surface.
   :param float tolerance: Tolerance used for the fit of the push-up curve. Generally, the resulting interpolating curve will be within tolerance of the surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new NURBS curve if successful, or None on error.
   :rtype: rhino3dm.NurbsCurve
.. py:function:: InterpolatedCurveOnSurfaceUV1(thisSurface, points, tolerance, closed, closedSurfaceHandling, multiple=False)

   Returns a curve that interpolates points on a surface. The interpolant lies on the surface.

   :param System.Collections.Generic.IEnumerable<Point2d> points: List of at least two UV parameter locations on the surface.
   :param float tolerance: Tolerance used for the fit of the push-up curve. Generally, the resulting interpolating curve will be within tolerance of the surface.
   :param bool closed: If false, the interpolating curve is not closed. If true, the interpolating curve is closed, and the last point and first point should generally not be equal.
   :param int closedSurfaceHandling: If 0, all points must be in the rectangular domain of the surface. If the surface is closed in some direction, \
      then this routine will interpret each point and place it at an appropriate location in the covering space. \
      This is the simplest option and should give good results. \
      If 1, then more options for more control of handling curves going across seams are available. \
      If the surface is closed in some direction, then the points are taken as points in the covering space. \
      Example, if srf.IsClosed(0)=True and srf.IsClosed(1)=False and srf.Domain(0)=srf.Domain(1)=Interval(0,1) \
      then if closedSurfaceHandling=1 a point(u, v) in points can have any value for the u coordinate, but must have 0<=v<=1. \
      In particular, if points = { (0.0,0.5), (2.0,0.5) } then the interpolating curve will wrap around the surface two times in the closed direction before ending at start of the curve. \
      If closed=True the last point should equal the first point plus an integer multiple of the period on a closed direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new NURBS curve if successful, or None on error.
   :rtype: rhino3dm.NurbsCurve
.. py:function:: InterpolatedCurveOnSurface(thisSurface, points, tolerance, multiple=False)

   Constructs an interpolated curve on a surface, using 3D points.

   :param System.Collections.Generic.IEnumerable<Point3d> points: A list, an array or any enumerable set of points.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new NURBS curve, or None on error.
   :rtype: rhino3dm.NurbsCurve
.. py:function:: ShortPath(thisSurface, start, end, tolerance, multiple=False)

   Constructs a geodesic between 2 points, used by ShortPath command in Rhino.

   :param rhino3dm.Point2d start: start point of curve in parameter space. Points must be distinct in the domain of the surface.
   :param rhino3dm.Point2d end: end point of curve in parameter space. Points must be distinct in the domain of the surface.
   :param float tolerance: tolerance used in fitting discrete solution.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: a geodesic curve on the surface on success. None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Pushup(thisSurface, curve2d, tolerance, curve2dSubdomain, multiple=False)

   Computes a 3d curve that is the composite of a 2d curve and the surface map.

   :param rhino3dm.Curve curve2d: a 2d curve whose image is in the surface's domain.
   :param float tolerance: the maximum acceptable distance from the returned 3d curve to the image of curve_2d on the surface.
   :param rhino3dm.Interval curve2dSubdomain: The curve interval (a sub-domain of the original curve) to use.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: 3d curve.
   :rtype: rhino3dm.Curve
.. py:function:: Pushup1(thisSurface, curve2d, tolerance, multiple=False)

   Computes a 3d curve that is the composite of a 2d curve and the surface map.

   :param rhino3dm.Curve curve2d: a 2d curve whose image is in the surface's domain.
   :param float tolerance: the maximum acceptable distance from the returned 3d curve to the image of curve_2d on the surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: 3d curve.
   :rtype: rhino3dm.Curve
.. py:function:: Pullback(thisSurface, curve3d, tolerance, multiple=False)

   Pulls a 3d curve back to the surface's parameter space.

   :param rhino3dm.Curve curve3d: The curve to pull.
   :param float tolerance: the maximum acceptable 3d distance between from surface(curve_2d(t)) \
      to the locus of points on the surface that are closest to curve_3d.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: 2d curve.
   :rtype: rhino3dm.Curve
.. py:function:: Pullback1(thisSurface, curve3d, tolerance, curve3dSubdomain, multiple=False)

   Pulls a 3d curve back to the surface's parameter space.

   :param rhino3dm.Curve curve3d: A curve.
   :param float tolerance: the maximum acceptable 3d distance between from surface(curve_2d(t)) \
      to the locus of points on the surface that are closest to curve_3d.
   :param rhino3dm.Interval curve3dSubdomain: A sub-domain of the curve to sample.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: 2d curve.
   :rtype: rhino3dm.Curve
