RhinoCompute.NurbsSurface
=========================

.. js:module:: RhinoCompute

.. js:function:: RhinoCompute.NurbsSurface.createSubDFriendly(surface, multiple=false)

   Create a bi-cubic SubD friendly surface from a surface.

   :param rhino3dm.Surface surface: >Surface to rebuild as a SubD friendly surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A SubD friendly NURBS surface is successful, None otherwise.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createFromPlane(plane, uInterval, vInterval, uDegree, vDegree, uPointCount, vPointCount, multiple=false)

   Creates a NURBS surface from a plane and additonal parameters.

   :param rhino3dm.Plane plane: The plane.
   :param rhino3dm.Interval uInterval: The interval describing the extends of the output surface in the U direction.
   :param rhino3dm.Interval vInterval: The interval describing the extends of the output surface in the V direction.
   :param int uDegree: The degree of the output surface in the U direction.
   :param int vDegree: The degree of the output surface in the V direction.
   :param int uPointCount: The number of control points of the output surface in the U direction.
   :param int vPointCount: The number of control points of the output surface in the V direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A NURBS surface if successful, or None on failure.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createCurveOnSurfacePoints(surface, fixedPoints, tolerance, periodic, initCount, levels, multiple=false)

   Computes a discrete spline curve on the surface. In other words, computes a sequence
   of points on the surface, each with a corresponding parameter value.

   :param rhino3dm.Surface surface: The surface on which the curve is constructed. The surface should be G1 continuous. \
      If the surface is closed in the u or v direction and is G1 at the seam, the \
      function will construct point sequences that cross over the seam.
   :param list[rhino3dm.Point2d] fixedPoints: Surface points to interpolate given by parameters. These must be distinct.
   :param float tolerance: Relative tolerance used by the solver. When in doubt, use a tolerance of 0.0.
   :param bool periodic: When True constructs a smoothly closed curve.
   :param int initCount: Maximum number of points to insert between fixed points on the first level.
   :param int levels: The number of levels (between 1 and 3) to be used in multi-level solver. Use 1 for single level solve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A sequence of surface points, given by surface parameters, if successful. \
      The number of output points is approximately: 2 ^ (level-1) * initCount * fixedPoints.Count.
   :rtype: rhino3dm.Point2d[]
.. js:function:: RhinoCompute.NurbsSurface.createCurveOnSurface(surface, points, tolerance, periodic, multiple=false)

   Fit a sequence of 2d points on a surface to make a curve on the surface.

   :param rhino3dm.Surface surface: Surface on which to construct curve.
   :param list[rhino3dm.Point2d] points: Parameter space coordinates of the points to interpolate.
   :param float tolerance: Curve should be within tolerance of surface and points.
   :param bool periodic: When True make a periodic curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A curve interpolating the points if successful, None on error.
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsSurface.makeCompatible(surface0, surface1, multiple=false)

   For expert use only. Makes a pair of compatible NURBS surfaces based on two input surfaces.

   :param rhino3dm.Surface surface0: The first surface.
   :param rhino3dm.Surface surface1: The second surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False on failure.
   :rtype: bool
.. js:function:: RhinoCompute.NurbsSurface.createFromPoints(points, uCount, vCount, uDegree, vDegree, multiple=false)

   Constructs a NURBS surface from a 2D grid of control points.

   :param list[rhino3dm.Point3d] points: Control point locations.
   :param int uCount: Number of points in U direction.
   :param int vCount: Number of points in V direction.
   :param int uDegree: Degree of surface in U direction.
   :param int vDegree: Degree of surface in V direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A NurbsSurface on success or None on failure.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createThroughPoints(points, uCount, vCount, uDegree, vDegree, uClosed, vClosed, multiple=false)

   Constructs a NURBS surface from a 2D grid of points.

   :param list[rhino3dm.Point3d] points: Control point locations.
   :param int uCount: Number of points in U direction.
   :param int vCount: Number of points in V direction.
   :param int uDegree: Degree of surface in U direction.
   :param int vDegree: Degree of surface in V direction.
   :param bool uClosed: True if the surface should be closed in the U direction.
   :param bool vClosed: True if the surface should be closed in the V direction.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A NurbsSurface on success or None on failure.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createFromCorners(corner1, corner2, corner3, corner4, multiple=false)

   Makes a surface from 4 corner points.
   This is the same as calling  with tolerance 0.

   :param rhino3dm.Point3d corner1: The first corner.
   :param rhino3dm.Point3d corner2: The second corner.
   :param rhino3dm.Point3d corner3: The third corner.
   :param rhino3dm.Point3d corner4: The fourth corner.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: the resulting surface or None on error.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createFromCorners1(corner1, corner2, corner3, corner4, tolerance, multiple=false)

   Makes a surface from 4 corner points.

   :param rhino3dm.Point3d corner1: The first corner.
   :param rhino3dm.Point3d corner2: The second corner.
   :param rhino3dm.Point3d corner3: The third corner.
   :param rhino3dm.Point3d corner4: The fourth corner.
   :param float tolerance: Minimum edge length without collapsing to a singularity.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The resulting surface or None on error.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createFromCorners2(corner1, corner2, corner3, multiple=false)

   Makes a surface from 3 corner points.

   :param rhino3dm.Point3d corner1: The first corner.
   :param rhino3dm.Point3d corner2: The second corner.
   :param rhino3dm.Point3d corner3: The third corner.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The resulting surface or None on error.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createRailRevolvedSurface(profile, rail, axis, scaleHeight, multiple=false)

   Constructs a railed Surface-of-Revolution.

   :param rhino3dm.Curve profile: Profile curve for revolution.
   :param rhino3dm.Curve rail: Rail curve for revolution.
   :param Line axis: Axis of revolution.
   :param bool scaleHeight: If true, surface will be locally scaled.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A NurbsSurface or None on failure.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createNetworkSurface(uCurves, uContinuityStart, uContinuityEnd, vCurves, vContinuityStart, vContinuityEnd, edgeTolerance, interiorTolerance, angleTolerance, multiple=false)

   Builds a surface from an ordered network of curves/edges.

   :param list[rhino3dm.Curve] uCurves: An array, a list or any enumerable set of U curves.
   :param int uContinuityStart: continuity at first U segment, 0 = loose, 1 = position, 2 = tan, 3 = curvature.
   :param int uContinuityEnd: continuity at last U segment, 0 = loose, 1 = position, 2 = tan, 3 = curvature.
   :param list[rhino3dm.Curve] vCurves: An array, a list or any enumerable set of V curves.
   :param int vContinuityStart: continuity at first V segment, 0 = loose, 1 = position, 2 = tan, 3 = curvature.
   :param int vContinuityEnd: continuity at last V segment, 0 = loose, 1 = position, 2 = tan, 3 = curvature.
   :param float edgeTolerance: tolerance to use along network surface edge.
   :param float interiorTolerance: tolerance to use for the interior curves.
   :param float angleTolerance: angle tolerance to use.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A NurbsSurface or None on failure.
   :rtype: NurbsSurface
.. js:function:: RhinoCompute.NurbsSurface.createNetworkSurface1(curves, continuity, edgeTolerance, interiorTolerance, angleTolerance, multiple=false)

   Builds a surface from an auto-sorted network of curves/edges.

   :param list[rhino3dm.Curve] curves: An array, a list or any enumerable set of curves/edges, sorted automatically into U and V curves.
   :param int continuity: continuity along edges, 0 = loose, 1 = position, 2 = tan, 3 = curvature.
   :param float edgeTolerance: tolerance to use along network surface edge.
   :param float interiorTolerance: tolerance to use for the interior curves.
   :param float angleTolerance: angle tolerance to use.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A NurbsSurface or None on failure.
   :rtype: NurbsSurface
