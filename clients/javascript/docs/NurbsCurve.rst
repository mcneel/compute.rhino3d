RhinoCompute.NurbsCurve
=======================

.. js:module:: RhinoCompute

.. js:function:: RhinoCompute.NurbsCurve.makeCompatible(curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance, multiple=false)

   For expert use only. From the input curves, make an array of compatible NURBS curves.

   :param list[rhino3dm.Curve] curves: The input curves.
   :param rhino3dm.Point3d startPt: The start point. To omit, specify Point3d.Unset.
   :param rhino3dm.Point3d endPt: The end point. To omit, specify Point3d.Unset.
   :param int simplifyMethod: The simplify method.
   :param int numPoints: The number of rebuild points.
   :param float refitTolerance: The refit tolerance.
   :param float angleTolerance: The angle tolerance in radians.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The output NURBS surfaces if successful.
   :rtype: rhino3dm.NurbsCurve[]
.. js:function:: RhinoCompute.NurbsCurve.createParabolaFromVertex(vertex, startPoint, endPoint, multiple=false)

   Creates a parabola from vertex and end points.

   :param rhino3dm.Point3d vertex: The vertex point.
   :param rhino3dm.Point3d startPoint: The start point.
   :param rhino3dm.Point3d endPoint: The end point
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A 2 degree NURBS curve if successful, False otherwise.
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createParabolaFromFocus(focus, startPoint, endPoint, multiple=false)

   Creates a parabola from focus and end points.

   :param rhino3dm.Point3d focus: The focal point.
   :param rhino3dm.Point3d startPoint: The start point.
   :param rhino3dm.Point3d endPoint: The end point
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A 2 degree NURBS curve if successful, False otherwise.
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createFromArc(arc, degree, cvCount, multiple=false)

   Create a uniform non-rational cubic NURBS approximation of an arc.

   :param int degree: >=1
   :param int cvCount: CV count >=5
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: NURBS curve approximation of an arc on success
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createHSpline(points, multiple=false)

   Construct an H-spline from a sequence of interpolation points

   :param list[rhino3dm.Point3d] points: Points to interpolate
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createHSpline1(points, startTangent, endTangent, multiple=false)

   Construct an H-spline from a sequence of interpolation points and
   optional start and end derivative information

   :param list[rhino3dm.Point3d] points: Points to interpolate
   :param rhino3dm.Vector3d startTangent: Unit tangent vector or Unset
   :param rhino3dm.Vector3d endTangent: Unit tangent vector or Unset
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: NURBS curve approximation of an arc on success
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createSubDFriendly(points, interpolatePoints, periodicClosedCurve, multiple=false)

   Create a NURBS curve, that is suitable for calculations like lofting SubD objects, through a sequence of curves.

   :param list[rhino3dm.Point3d] points: An enumeration of points. Adjacent points must not be equal. \
      If periodicClosedCurve is false, there must be at least two points. \
      If periodicClosedCurve is true, there must be at least three points and it is not necessary to duplicate the first and last points. \
      When periodicClosedCurve is True and the first and last points are equal, the duplicate last point is automatically ignored.
   :param bool interpolatePoints: True if the curve should interpolate the points. False if points specify control point locations. \
      In either case, the curve will begin at the first point and end at the last point.
   :param bool periodicClosedCurve: True to create a periodic closed curve. Do not duplicate the start/end point in the point input.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A SubD friendly NURBS curve is successful, None otherwise.
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createSubDFriendly1(curve, multiple=false)

   Create a NURBS curve, that is suitable for calculations like lofting SubD objects, from an existing curve.

   :param rhino3dm.Curve curve: Curve to rebuild as a SubD friendly curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A SubD friendly NURBS curve is successful, None otherwise.
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createSubDFriendly2(curve, pointCount, periodicClosedCurve, multiple=false)

   Create a NURBS curve, that is suitable for calculations like lofting SubD objects, from an existing curve.

   :param rhino3dm.Curve curve: Curve to rebuild as a SubD friendly curve.
   :param int pointCount: Desired number of control points. If periodicClosedCurve is true, the number must be >= 6, otherwise the number must be >= 4.
   :param bool periodicClosedCurve: True if the SubD friendly curve should be closed and periodic. False in all other cases.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A SubD friendly NURBS curve is successful, None otherwise.
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createPlanarRailFrames(thisNurbsCurve, parameters, normal, multiple=false)

   Computes planar rail sweep frames at specified parameters.

   :param list[float] parameters: A collection of curve parameters.
   :param rhino3dm.Vector3d normal: Unit normal to the plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of planes if successful, or an empty array on failure.
   :rtype: rhino3dm.Plane[]
.. js:function:: RhinoCompute.NurbsCurve.createRailFrames(thisNurbsCurve, parameters, multiple=false)

   Computes relatively parallel rail sweep frames at specified parameters.

   :param list[float] parameters: A collection of curve parameters.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of planes if successful, or an empty array on failure.
   :rtype: rhino3dm.Plane[]
.. js:function:: RhinoCompute.NurbsCurve.createFromCircle(circle, degree, cvCount, multiple=false)

   Create a uniform non-rational cubic NURBS approximation of a circle.

   :param int degree: >=1
   :param int cvCount: CV count >=5
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: NURBS curve approximation of a circle on success
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.setEndCondition(thisNurbsCurve, bSetEnd, continuity, point, tangent, multiple=false)

   Set end condition of a NURBS curve to point, tangent and curvature.

   :param bool bSetEnd: true: set end of curve, false: set start of curve
   :param NurbsCurveEndConditionType continuity: Position: set start or end point, Tangency: set point and tangent, Curvature: set point, tangent and curvature
   :param rhino3dm.Point3d point: point to set
   :param rhino3dm.Vector3d tangent: tangent to set
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. js:function:: RhinoCompute.NurbsCurve.setEndCondition1(thisNurbsCurve, bSetEnd, continuity, point, tangent, curvature, multiple=false)

   Set end condition of a NURBS curve to point, tangent and curvature.

   :param bool bSetEnd: true: set end of curve, false: set start of curve
   :param NurbsCurveEndConditionType continuity: Position: set start or end point, Tangency: set point and tangent, Curvature: set point, tangent and curvature
   :param rhino3dm.Point3d point: point to set
   :param rhino3dm.Vector3d tangent: tangent to set
   :param rhino3dm.Vector3d curvature: curvature to set
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. js:function:: RhinoCompute.NurbsCurve.grevillePoints(thisNurbsCurve, all, multiple=false)

   Gets Greville points for this curve.

   :param bool all: If true, then all Greville points are returns. If false, only edit points are returned.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A list of points if successful, None otherwise.
   :rtype: Point3dList
.. js:function:: RhinoCompute.NurbsCurve.setGrevillePoints(thisNurbsCurve, points, multiple=false)

   Sets all Greville edit points for this curve.

   :param list[rhino3dm.Point3d] points: The new point locations. The number of points should match \
      the number of point returned by NurbsCurve.GrevillePoints(false).
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. js:function:: RhinoCompute.NurbsCurve.createSpiral(axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1, multiple=false)

   Creates a C1 cubic NURBS approximation of a helix or spiral. For a helix,
   you may have radius0 == radius1. For a spiral radius0 == radius1 produces
   a circle. Zero and negative radii are permissible.

   :param rhino3dm.Point3d axisStart: Helix's axis starting point or center of spiral.
   :param rhino3dm.Vector3d axisDir: Helix's axis vector or normal to spiral's plane.
   :param rhino3dm.Point3d radiusPoint: Point used only to get a vector that is perpendicular to the axis. In \
      particular, this vector must not be (anti)parallel to the axis vector.
   :param float pitch: The pitch, where a spiral has a pitch = 0, and pitch > 0 is the distance \
      between the helix's "threads".
   :param float turnCount: The number of turns in spiral or helix. Positive \
      values produce counter-clockwise orientation, negative values produce \
      clockwise orientation. Note, for a helix, turnCount * pitch = length of \
      the helix's axis.
   :param float radius0: The starting radius.
   :param float radius1: The ending radius.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: NurbsCurve on success, None on failure.
   :rtype: rhino3dm.NurbsCurve
.. js:function:: RhinoCompute.NurbsCurve.createSpiral1(railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn, multiple=false)

   Create a C2 non-rational uniform cubic NURBS approximation of a swept helix or spiral.

   :param rhino3dm.Curve railCurve: The rail curve.
   :param float t0: Starting portion of rail curve's domain to sweep along.
   :param float t1: Ending portion of rail curve's domain to sweep along.
   :param rhino3dm.Point3d radiusPoint: Point used only to get a vector that is perpendicular to the axis. In \
      particular, this vector must not be (anti)parallel to the axis vector.
   :param float pitch: The pitch. Positive values produce counter-clockwise orientation, \
      negative values produce clockwise orientation.
   :param float turnCount: The turn count. If != 0, then the resulting helix will have this many \
      turns. If = 0, then pitch must be != 0 and the approximate distance \
      between turns will be set to pitch. Positive values produce counter-clockwise \
      orientation, negative values produce clockwise orientation.
   :param float radius0: The starting radius. At least one radii must be nonzero. Negative values \
      are allowed.
   :param float radius1: The ending radius. At least one radii must be nonzero. Negative values \
      are allowed.
   :param int pointsPerTurn: Number of points to interpolate per turn. Must be greater than 4. \
      When in doubt, use 12.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: NurbsCurve on success, None on failure.
   :rtype: rhino3dm.NurbsCurve
