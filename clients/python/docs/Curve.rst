Curve
=====

.. py:module:: compute_rhino3d.Curve

.. py:function:: GetConicSectionType(thisCurve, multiple=False)

   Returns the type of conic section based on the curve's shape.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: ConicSectionType
.. py:function:: CreateInterpolatedCurve(points, degree, multiple=False)

   Interpolates a sequence of points. Used by InterpCurve Command
   This routine works best when degree=3.

   :param int degree: The degree of the curve >=1.  Degree must be odd.
   :param list[rhino3dm.Point3d] points: Points to interpolate (Count must be >= 2)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: interpolated curve on success. None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: CreateInterpolatedCurve1(points, degree, knots, multiple=False)

   Interpolates a sequence of points. Used by InterpCurve Command
   This routine works best when degree=3.

   :param int degree: The degree of the curve >=1.  Degree must be odd.
   :param list[rhino3dm.Point3d] points: Points to interpolate. For periodic curves if the final point is a \
      duplicate of the initial point it is  ignored. (Count must be >=2)
   :param CurveKnotStyle knots: Knot-style to use  and specifies if the curve should be periodic.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: interpolated curve on success. None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: CreateInterpolatedCurve2(points, degree, knots, startTangent, endTangent, multiple=False)

   Interpolates a sequence of points. Used by InterpCurve Command
   This routine works best when degree=3.

   :param int degree: The degree of the curve >=1.  Degree must be odd.
   :param list[rhino3dm.Point3d] points: Points to interpolate. For periodic curves if the final point is a \
      duplicate of the initial point it is  ignored. (Count must be >=2)
   :param CurveKnotStyle knots: Knot-style to use  and specifies if the curve should be periodic.
   :param rhino3dm.Vector3d startTangent: A starting tangent.
   :param rhino3dm.Vector3d endTangent: An ending tangent.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: interpolated curve on success. None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: CreateSoftEditCurve(curve, t, delta, length, fixEnds, multiple=False)

   Creates a soft edited curve from an existing curve using a smooth field of influence.

   :param rhino3dm.Curve curve: The curve to soft edit.
   :param float t: A parameter on the curve to move from. This location on the curve is moved, and the move \
      is smoothly tapered off with increasing distance along the curve from this parameter.
   :param rhino3dm.Vector3d delta: The direction and magnitude, or maximum distance, of the move.
   :param float length: The distance along the curve from the editing point over which the strength \
      of the editing falls off smoothly.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The soft edited curve if successful. None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: CreateFilletCornersCurve(curve, radius, tolerance, angleTolerance, multiple=False)

   Rounds the corners of a kinked curve with arcs of a single, specified radius.

   :param rhino3dm.Curve curve: The curve to fillet.
   :param float radius: The fillet radius.
   :param float tolerance: The tolerance. When in doubt, use the document's model space absolute tolerance.
   :param float angleTolerance: The angle tolerance in radians. When in doubt, use the document's model space angle tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The filleted curve if successful. None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: CreateArcBlend(startPt, startDir, endPt, endDir, controlPointLengthRatio, multiple=False)

   Creates a polycurve consisting of two tangent arc segments that connect two points and two directions.

   :param rhino3dm.Point3d startPt: Start of the arc blend curve.
   :param rhino3dm.Vector3d startDir: Start direction of the arc blend curve.
   :param rhino3dm.Point3d endPt: End of the arc blend curve.
   :param rhino3dm.Vector3d endDir: End direction of the arc blend curve.
   :param float controlPointLengthRatio: The ratio of the control polygon lengths of the two arcs. Note, a value of 1.0 \
      means the control polygon lengths for both arcs will be the same.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The arc blend curve, or None on error.
   :rtype: rhino3dm.Curve
.. py:function:: CreateMeanCurve(curveA, curveB, angleToleranceRadians, multiple=False)

   Constructs a mean, or average, curve from two curves.

   :param rhino3dm.Curve curveA: A first curve.
   :param rhino3dm.Curve curveB: A second curve.
   :param float angleToleranceRadians: The angle tolerance, in radians, used to match kinks between curves. \
      If you are unsure how to set this parameter, then either use the \
      document's angle tolerance RhinoDoc.AngleToleranceRadians, \
      or the default value (RhinoMath.UnsetValue)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The average curve, or None on error.
   :rtype: rhino3dm.Curve
.. py:function:: CreateMeanCurve1(curveA, curveB, multiple=False)

   Constructs a mean, or average, curve from two curves.

   :param rhino3dm.Curve curveA: A first curve.
   :param rhino3dm.Curve curveB: A second curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The average curve, or None on error.
   :rtype: rhino3dm.Curve
.. py:function:: CreateBlendCurve(curveA, curveB, continuity, multiple=False)

   Create a Blend curve between two existing curves.

   :param rhino3dm.Curve curveA: Curve to blend from (blending will occur at curve end point).
   :param rhino3dm.Curve curveB: Curve to blend to (blending will occur at curve start point).
   :param BlendContinuity continuity: Continuity of blend.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A curve representing the blend between A and B or None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: CreateBlendCurve1(curveA, curveB, continuity, bulgeA, bulgeB, multiple=False)

   Create a Blend curve between two existing curves.

   :param rhino3dm.Curve curveA: Curve to blend from (blending will occur at curve end point).
   :param rhino3dm.Curve curveB: Curve to blend to (blending will occur at curve start point).
   :param BlendContinuity continuity: Continuity of blend.
   :param float bulgeA: Bulge factor at curveA end of blend. Values near 1.0 work best.
   :param float bulgeB: Bulge factor at curveB end of blend. Values near 1.0 work best.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A curve representing the blend between A and B or None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: CreateBlendCurve2(curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1, multiple=False)

   Makes a curve blend between 2 curves at the parameters specified
   with the directions and continuities specified

   :param rhino3dm.Curve curve0: First curve to blend from
   :param float t0: Parameter on first curve for blend endpoint
   :param bool reverse0: If false, the blend will go in the natural direction of the curve. \
      If true, the blend will go in the opposite direction to the curve
   :param BlendContinuity continuity0: Continuity for the blend at the start
   :param rhino3dm.Curve curve1: Second curve to blend from
   :param float t1: Parameter on second curve for blend endpoint
   :param bool reverse1: If false, the blend will go in the natural direction of the curve. \
      If true, the blend will go in the opposite direction to the curve
   :param BlendContinuity continuity1: Continuity for the blend at the end
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The blend curve on success. None on failure
   :rtype: rhino3dm.Curve
.. py:function:: CreateTweenCurves(curve0, curve1, numCurves, multiple=False)

   Creates curves between two open or closed input curves. Uses the control points of the curves for finding tween curves.
   That means the first control point of first curve is matched to first control point of the second curve and so on.
   There is no matching of curves direction. Caller must match input curves direction before calling the function.

   :param rhino3dm.Curve curve0: The first, or starting, curve.
   :param rhino3dm.Curve curve1: The second, or ending, curve.
   :param int numCurves: Number of tween curves to create.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of joint curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateTweenCurves1(curve0, curve1, numCurves, tolerance, multiple=False)

   Creates curves between two open or closed input curves. Uses the control points of the curves for finding tween curves.
   That means the first control point of first curve is matched to first control point of the second curve and so on.
   There is no matching of curves direction. Caller must match input curves direction before calling the function.

   :param rhino3dm.Curve curve0: The first, or starting, curve.
   :param rhino3dm.Curve curve1: The second, or ending, curve.
   :param int numCurves: Number of tween curves to create.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of joint curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateTweenCurvesWithMatching(curve0, curve1, numCurves, multiple=False)

   Creates curves between two open or closed input curves. Make the structure of input curves compatible if needed.
   Refits the input curves to have the same structure. The resulting curves are usually more complex than input unless
   input curves are compatible and no refit is needed. There is no matching of curves direction.
   Caller must match input curves direction before calling the function.

   :param rhino3dm.Curve curve0: The first, or starting, curve.
   :param rhino3dm.Curve curve1: The second, or ending, curve.
   :param int numCurves: Number of tween curves to create.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of joint curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateTweenCurvesWithMatching1(curve0, curve1, numCurves, tolerance, multiple=False)

   Creates curves between two open or closed input curves. Make the structure of input curves compatible if needed.
   Refits the input curves to have the same structure. The resulting curves are usually more complex than input unless
   input curves are compatible and no refit is needed. There is no matching of curves direction.
   Caller must match input curves direction before calling the function.

   :param rhino3dm.Curve curve0: The first, or starting, curve.
   :param rhino3dm.Curve curve1: The second, or ending, curve.
   :param int numCurves: Number of tween curves to create.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of joint curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateTweenCurvesWithSampling(curve0, curve1, numCurves, numSamples, multiple=False)

   Creates curves between two open or closed input curves. Use sample points method to make curves compatible.
   This is how the algorithm works: Divides the two curves into an equal number of points, finds the midpoint between the
   corresponding points on the curves and interpolates the tween curve through those points. There is no matching of curves
   direction. Caller must match input curves direction before calling the function.

   :param rhino3dm.Curve curve0: The first, or starting, curve.
   :param rhino3dm.Curve curve1: The second, or ending, curve.
   :param int numCurves: Number of tween curves to create.
   :param int numSamples: Number of sample points along input curves.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: >An array of joint curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateTweenCurvesWithSampling1(curve0, curve1, numCurves, numSamples, tolerance, multiple=False)

   Creates curves between two open or closed input curves. Use sample points method to make curves compatible.
   This is how the algorithm works: Divides the two curves into an equal number of points, finds the midpoint between the
   corresponding points on the curves and interpolates the tween curve through those points. There is no matching of curves
   direction. Caller must match input curves direction before calling the function.

   :param rhino3dm.Curve curve0: The first, or starting, curve.
   :param rhino3dm.Curve curve1: The second, or ending, curve.
   :param int numCurves: Number of tween curves to create.
   :param int numSamples: Number of sample points along input curves.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: >An array of joint curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: JoinCurves(inputCurves, multiple=False)

   Joins a collection of curve segments together.

   :param list[rhino3dm.Curve] inputCurves: Curve segments to join.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of curves which contains.
   :rtype: rhino3dm.Curve[]
.. py:function:: JoinCurves1(inputCurves, joinTolerance, multiple=False)

   Joins a collection of curve segments together.

   :param list[rhino3dm.Curve] inputCurves: An array, a list or any enumerable set of curve segments to join.
   :param float joinTolerance: Joining tolerance, \
      i.e. the distance between segment end-points that is allowed.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of joint curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: JoinCurves2(inputCurves, joinTolerance, preserveDirection, multiple=False)

   Joins a collection of curve segments together.

   :param list[rhino3dm.Curve] inputCurves: An array, a list or any enumerable set of curve segments to join.
   :param float joinTolerance: Joining tolerance, \
      i.e. the distance between segment end-points that is allowed.
   :param bool preserveDirection: If true, curve endpoints will be compared to curve start points.If false, all start and endpoints will be compared and copies of input curves may be reversed in output.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of joint curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: MakeEndsMeet(curveA, adjustStartCurveA, curveB, adjustStartCurveB, multiple=False)

   Makes adjustments to the ends of one or both input curves so that they meet at a point.

   :param rhino3dm.Curve curveA: 1st curve to adjust.
   :param bool adjustStartCurveA: Which end of the 1st curve to adjust: True is start, False is end.
   :param rhino3dm.Curve curveB: 2nd curve to adjust.
   :param bool adjustStartCurveB: which end of the 2nd curve to adjust true==start, false==end.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success.
   :rtype: bool
.. py:function:: CreateFillet(curve0, curve1, radius, t0Base, t1Base, multiple=False)

   Computes the fillet arc for a curve filleting operation.

   :param rhino3dm.Curve curve0: First curve to fillet.
   :param rhino3dm.Curve curve1: Second curve to fillet.
   :param float radius: Fillet radius.
   :param float t0Base: Parameter on curve0 where the fillet ought to start (approximately).
   :param float t1Base: Parameter on curve1 where the fillet ought to end (approximately).
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The fillet arc on success, or Arc.Unset on failure.
   :rtype: Arc
.. py:function:: CreateFilletCurves(curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance, multiple=False)

   Creates a tangent arc between two curves and trims or extends the curves to the arc.

   :param rhino3dm.Curve curve0: The first curve to fillet.
   :param rhino3dm.Point3d point0: A point on the first curve that is near the end where the fillet will \
      be created.
   :param rhino3dm.Curve curve1: The second curve to fillet.
   :param rhino3dm.Point3d point1: A point on the second curve that is near the end where the fillet will \
      be created.
   :param float radius: The radius of the fillet.
   :param bool join: Join the output curves.
   :param bool trim: Trim copies of the input curves to the output fillet curve.
   :param bool arcExtension: Applies when arcs are filleted but need to be extended to meet the \
      fillet curve or chamfer line. If true, then the arc is extended \
      maintaining its validity. If false, then the arc is extended with a \
      line segment, which is joined to the arc converting it to a polycurve.
   :param float tolerance: The tolerance, generally the document's absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The results of the fillet operation. The number of output curves depends \
      on the input curves and the values of the parameters that were used \
      during the fillet operation. In most cases, the output array will contain \
      either one or three curves, although two curves can be returned if the \
      radius is zero and join = false. \
      For example, if both join and trim = true, then the output curve \
      will be a polycurve containing the fillet curve joined with trimmed copies \
      of the input curves. If join = False and trim = true, then three curves, \
      the fillet curve and trimmed copies of the input curves, will be returned. \
      If both join and trim = false, then just the fillet curve is returned.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanUnion(curves, multiple=False)

   Calculates the boolean union of two or more closed, planar curves.
   Note, curves must be co-planar.

   :param list[rhino3dm.Curve] curves: The co-planar curves to union.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Result curves on success, empty array if no union could be calculated.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanUnion1(curves, tolerance, multiple=False)

   Calculates the boolean union of two or more closed, planar curves.
   Note, curves must be co-planar.

   :param list[rhino3dm.Curve] curves: The co-planar curves to union.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Result curves on success, empty array if no union could be calculated.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanIntersection(curveA, curveB, multiple=False)

   Calculates the boolean intersection of two closed, planar curves.
   Note, curves must be co-planar.

   :param rhino3dm.Curve curveA: The first closed, planar curve.
   :param rhino3dm.Curve curveB: The second closed, planar curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Result curves on success, empty array if no intersection could be calculated.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanIntersection1(curveA, curveB, tolerance, multiple=False)

   Calculates the boolean intersection of two closed, planar curves.
   Note, curves must be co-planar.

   :param rhino3dm.Curve curveA: The first closed, planar curve.
   :param rhino3dm.Curve curveB: The second closed, planar curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Result curves on success, empty array if no intersection could be calculated.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanDifference(curveA, curveB, multiple=False)

   Calculates the boolean difference between two closed, planar curves.
   Note, curves must be co-planar.

   :param rhino3dm.Curve curveA: The first closed, planar curve.
   :param rhino3dm.Curve curveB: The second closed, planar curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Result curves on success, empty array if no difference could be calculated.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanDifference1(curveA, curveB, tolerance, multiple=False)

   Calculates the boolean difference between two closed, planar curves.
   Note, curves must be co-planar.

   :param rhino3dm.Curve curveA: The first closed, planar curve.
   :param rhino3dm.Curve curveB: The second closed, planar curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Result curves on success, empty array if no difference could be calculated.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanDifference2(curveA, subtractors, multiple=False)

   Calculates the boolean difference between a closed planar curve, and a list of closed planar curves.
   Note, curves must be co-planar.

   :param rhino3dm.Curve curveA: The first closed, planar curve.
   :param list[rhino3dm.Curve] subtractors: curves to subtract from the first closed curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Result curves on success, empty array if no difference could be calculated.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanDifference3(curveA, subtractors, tolerance, multiple=False)

   Calculates the boolean difference between a closed planar curve, and a list of closed planar curves.
   Note, curves must be co-planar.

   :param rhino3dm.Curve curveA: The first closed, planar curve.
   :param list[rhino3dm.Curve] subtractors: curves to subtract from the first closed curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Result curves on success, empty array if no difference could be calculated.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateBooleanRegions(curves, plane, points, combineRegions, tolerance, multiple=False)

   Curve Boolean method, which trims and splits curves based on their overlapping regions.

   :param list[rhino3dm.Curve] curves: The input curves.
   :param rhino3dm.Plane plane: Regions will be found in the projection of the curves to this plane.
   :param list[rhino3dm.Point3d] points: These points will be projected to plane. All regions that contain at least one of these points will be found.
   :param bool combineRegions: If true, then adjacent regions will be combined.
   :param float tolerance: Function tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The curve Boolean regions if successful, None of no successful.
   :rtype: CurveBooleanRegions
.. py:function:: CreateBooleanRegions1(curves, plane, combineRegions, tolerance, multiple=False)

   Calculates curve Boolean regions, which trims and splits curves based on their overlapping regions.

   :param list[rhino3dm.Curve] curves: The input curves.
   :param rhino3dm.Plane plane: Regions will be found in the projection of the curves to this plane.
   :param bool combineRegions: If true, then adjacent regions will be combined.
   :param float tolerance: Function tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The curve Boolean regions if successful, None of no successful.
   :rtype: CurveBooleanRegions
.. py:function:: CreateTextOutlines(text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance, multiple=False)

   Creates outline curves created from a text string. The functionality is similar to what you find in Rhino's TextObject command or TextEntity.Explode() in RhinoCommon.

   :param str text: The text from which to create outline curves.
   :param str font: The text font.
   :param float textHeight: The text height.
   :param int textStyle: The font style. The font style can be any number of the following: 0 - Normal, 1 - Bold, 2 - Italic
   :param bool closeLoops: Set this value to True when dealing with normal fonts and when you expect closed loops. You may want to set this to False when specifying a single-stroke font where you don't want closed loops.
   :param rhino3dm.Plane plane: The plane on which the outline curves will lie.
   :param float smallCapsScale: Displays lower-case letters as small caps. Set the relative text size to a percentage of the normal text.
   :param float tolerance: The tolerance for the operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array containing one or more curves if successful.
   :rtype: rhino3dm.Curve[]
.. py:function:: CreateCurve2View(curveA, curveB, vectorA, vectorB, tolerance, angleTolerance, multiple=False)

   Creates a third curve from two curves that are planar in different construction planes.
   The new curve looks the same as each of the original curves when viewed in each plane.

   :param rhino3dm.Curve curveA: The first curve.
   :param rhino3dm.Curve curveB: The second curve.
   :param rhino3dm.Vector3d vectorA: A vector defining the normal direction of the plane which the first curve is drawn upon.
   :param rhino3dm.Vector3d vectorB: A vector defining the normal direction of the plane which the second curve is drawn upon.
   :param float tolerance: The tolerance for the operation.
   :param float angleTolerance: The angle tolerance for the operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array containing one or more curves if successful.
   :rtype: rhino3dm.Curve[]
.. py:function:: DoDirectionsMatch(curveA, curveB, multiple=False)

   Determines whether two curves travel more or less in the same direction.

   :param rhino3dm.Curve curveA: First curve to test.
   :param rhino3dm.Curve curveB: Second curve to test.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if both curves more or less point in the same direction, \
      False if they point in the opposite directions.
   :rtype: bool
.. py:function:: ProjectToMesh(curve, mesh, direction, tolerance, multiple=False)

   Projects a curve to a mesh using a direction and tolerance.

   :param rhino3dm.Curve curve: A curve.
   :param rhino3dm.Mesh mesh: A mesh.
   :param rhino3dm.Vector3d direction: A direction vector.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A curve array.
   :rtype: rhino3dm.Curve[]
.. py:function:: ProjectToMesh1(curve, meshes, direction, tolerance, multiple=False)

   Projects a curve to a set of meshes using a direction and tolerance.

   :param rhino3dm.Curve curve: A curve.
   :param list[rhino3dm.Mesh] meshes: A list, an array or any enumerable of meshes.
   :param rhino3dm.Vector3d direction: A direction vector.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A curve array.
   :rtype: rhino3dm.Curve[]
.. py:function:: ProjectToMesh2(curves, meshes, direction, tolerance, multiple=False)

   Projects a curve to a set of meshes using a direction and tolerance.

   :param list[rhino3dm.Curve] curves: A list, an array or any enumerable of curves.
   :param list[rhino3dm.Mesh] meshes: A list, an array or any enumerable of meshes.
   :param rhino3dm.Vector3d direction: A direction vector.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A curve array.
   :rtype: rhino3dm.Curve[]
.. py:function:: ProjectToBrep(curve, brep, direction, tolerance, multiple=False)

   Projects a Curve onto a Brep along a given direction.

   :param rhino3dm.Curve curve: Curve to project.
   :param rhino3dm.Brep brep: Brep to project onto.
   :param rhino3dm.Vector3d direction: Direction of projection.
   :param float tolerance: Tolerance to use for projection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of projected curves or empty array if the projection set is empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: ProjectToBrep1(curve, breps, direction, tolerance, multiple=False)

   Projects a Curve onto a collection of Breps along a given direction.

   :param rhino3dm.Curve curve: Curve to project.
   :param list[rhino3dm.Brep] breps: Breps to project onto.
   :param rhino3dm.Vector3d direction: Direction of projection.
   :param float tolerance: Tolerance to use for projection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of projected curves or empty array if the projection set is empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: ProjectToBrep2(curve, breps, direction, tolerance, multiple=False)

   Projects a Curve onto a collection of Breps along a given direction.

   :param rhino3dm.Curve curve: Curve to project.
   :param list[rhino3dm.Brep] breps: Breps to project onto.
   :param rhino3dm.Vector3d direction: Direction of projection.
   :param float tolerance: Tolerance to use for projection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of projected curves or None if the projection set is empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: ProjectToBrep3(curves, breps, direction, tolerance, multiple=False)

   Projects a collection of Curves onto a collection of Breps along a given direction.

   :param list[rhino3dm.Curve] curves: Curves to project.
   :param list[rhino3dm.Brep] breps: Breps to project onto.
   :param rhino3dm.Vector3d direction: Direction of projection.
   :param float tolerance: Tolerance to use for projection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of projected curves or empty array if the projection set is empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: ProjectToBrep4(curves, breps, direction, tolerance, multiple=False)

   Projects a collection of Curves onto a collection of Breps along a given direction.

   :param list[rhino3dm.Curve] curves: Curves to project.
   :param list[rhino3dm.Brep] breps: Breps to project onto.
   :param rhino3dm.Vector3d direction: Direction of projection.
   :param float tolerance: Tolerance to use for projection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of projected curves. Array is empty if the projection set is empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: ProjectToPlane(curve, plane, multiple=False)

   Constructs a curve by projecting an existing curve to a plane.

   :param rhino3dm.Curve curve: A curve.
   :param rhino3dm.Plane plane: A plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The projected curve on success; None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: PullToBrepFace(curve, face, tolerance, multiple=False)

   Pull a curve to a BrepFace using closest point projection.

   :param rhino3dm.Curve curve: Curve to pull.
   :param rhino3dm.BrepFace face: Brep face that pulls.
   :param float tolerance: Tolerance to use for pulling.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of pulled curves, or an empty array on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: PlanarClosedCurveRelationship(curveA, curveB, testPlane, tolerance, multiple=False)

   Determines whether two coplanar simple closed curves are disjoint or intersect;
   otherwise, if the regions have a containment relationship, discovers
   which curve encloses the other.

   :param rhino3dm.Curve curveA: A first curve.
   :param rhino3dm.Curve curveB: A second curve.
   :param rhino3dm.Plane testPlane: A plane.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A value indicating the relationship between the first and the second curve.
   :rtype: RegionContainment
.. py:function:: PlanarCurveCollision(curveA, curveB, testPlane, tolerance, multiple=False)

   Determines if two coplanar curves collide (intersect).

   :param rhino3dm.Curve curveA: A curve.
   :param rhino3dm.Curve curveB: Another curve.
   :param rhino3dm.Plane testPlane: A valid plane containing the curves.
   :param float tolerance: A tolerance value for intersection.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if the curves intersect, otherwise false
   :rtype: bool
.. py:function:: DuplicateSegments(thisCurve, multiple=False)

   Polylines will be exploded into line segments. ExplodeCurves will
   return the curves in topological order.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of all the segments that make up this curve.
   :rtype: rhino3dm.Curve[]
.. py:function:: Smooth(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, multiple=False)

   Smooths a curve by averaging the positions of control points in a specified region.

   :param float smoothFactor: The smoothing factor, which controls how much control points move towards the average of the neighboring control points.
   :param bool bXSmooth: When True control points move in X axis direction.
   :param bool bYSmooth: When True control points move in Y axis direction.
   :param bool bZSmooth: When True control points move in Z axis direction.
   :param bool bFixBoundaries: When True the curve ends don't move.
   :param SmoothingCoordinateSystem coordinateSystem: The coordinates to determine the direction of the smoothing.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The smoothed curve if successful, None otherwise.
   :rtype: rhino3dm.Curve
.. py:function:: Smooth1(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False)

   Smooths a curve by averaging the positions of control points in a specified region.

   :param float smoothFactor: The smoothing factor, which controls how much control points move towards the average of the neighboring control points.
   :param bool bXSmooth: When True control points move in X axis direction.
   :param bool bYSmooth: When True control points move in Y axis direction.
   :param bool bZSmooth: When True control points move in Z axis direction.
   :param bool bFixBoundaries: When True the curve ends don't move.
   :param SmoothingCoordinateSystem coordinateSystem: The coordinates to determine the direction of the smoothing.
   :param rhino3dm.Plane plane: If SmoothingCoordinateSystem.CPlane specified, then the construction plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The smoothed curve if successful, None otherwise.
   :rtype: rhino3dm.Curve
.. py:function:: GetLocalPerpPoint(thisCurve, testPoint, seedParmameter, multiple=False)

   Search for a location on the curve, near seedParmameter, that is perpendicular to a test point.

   :param rhino3dm.Point3d testPoint: The test point.
   :param float seedParmameter: A "seed" parameter on the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if a solution is found, False otherwise.
   :rtype: bool
.. py:function:: GetLocalPerpPoint1(thisCurve, testPoint, seedParmameter, subDomain, multiple=False)

   Search for a location on the curve, near seedParmameter, that is perpendicular to a test point.

   :param rhino3dm.Point3d testPoint: The test point.
   :param float seedParmameter: A "seed" parameter on the curve.
   :param rhino3dm.Interval subDomain: The sub-domain of the curve to search.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if a solution is found, False otherwise.
   :rtype: bool
.. py:function:: GetLocalTangentPoint(thisCurve, testPoint, seedParmameter, multiple=False)

   Search for a location on the curve, near seedParmameter, that is tangent to a test point.

   :param rhino3dm.Point3d testPoint: The test point.
   :param float seedParmameter: A "seed" parameter on the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if a solution is found, False otherwise.
   :rtype: bool
.. py:function:: GetLocalTangentPoint1(thisCurve, testPoint, seedParmameter, subDomain, multiple=False)

   Search for a location on the curve, near seedParmameter, that is tangent to a test point.

   :param rhino3dm.Point3d testPoint: The test point.
   :param float seedParmameter: A "seed" parameter on the curve.
   :param rhino3dm.Interval subDomain: The sub-domain of the curve to search.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if a solution is found, False otherwise.
   :rtype: bool
.. py:function:: InflectionPoints(thisCurve, multiple=False)

   Returns a curve's inflection points. An inflection point is a location on
   a curve at which the sign of the curvature (i.e., the concavity) changes.
   The curvature at these locations is always 0.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points if successful, None if not successful or on error.
   :rtype: rhino3dm.Point3d[]
.. py:function:: MaxCurvaturePoints(thisCurve, multiple=False)

   Returns a curve's maximum curvature points. The maximum curvature points identify
   where the curvature starts to decrease in both directions from the points.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points if successful, None if not successful or on error.
   :rtype: rhino3dm.Point3d[]
.. py:function:: MakeClosed(thisCurve, tolerance, multiple=False)

   If IsClosed, just return true. Otherwise, decide if curve can be closed as
   follows: Linear curves polylinear curves with 2 segments, NURBS with 3 or less
   control points cannot be made closed. Also, if tolerance > 0 and the gap between
   start and end is larger than tolerance, curve cannot be made closed.
   Adjust the curve's endpoint to match its start point.

   :param float tolerance: If nonzero, and the gap is more than tolerance, curve cannot be made closed.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: LcoalClosestPoint(thisCurve, testPoint, seed, multiple=False)

   Find parameter of the point on a curve that is locally closest to
   the testPoint.  The search for a local close point starts at
   a seed parameter.

   :param rhino3dm.Point3d testPoint: A point to test against.
   :param float seed: The seed parameter.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if the search is successful, False if the search fails.
   :rtype: bool
.. py:function:: LocalClosestPoint(thisCurve, testPoint, seed, multiple=False)

   Find parameter of the point on a curve that is locally closest to
   the testPoint.  The search for a local close point starts at
   a seed parameter.

   :param rhino3dm.Point3d testPoint: A point to test against.
   :param float seed: The seed parameter.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if the search is successful, False if the search fails.
   :rtype: bool
.. py:function:: ClosestPoint(thisCurve, testPoint, multiple=False)

   Finds parameter of the point on a curve that is closest to testPoint.
   If the maximumDistance parameter is > 0, then only points whose distance
   to the given point is <= maximumDistance will be returned.  Using a
   positive value of maximumDistance can substantially speed up the search.

   :param rhino3dm.Point3d testPoint: Point to search from.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: ClosestPoint1(thisCurve, testPoint, maximumDistance, multiple=False)

   Finds the parameter of the point on a curve that is closest to testPoint.
   If the maximumDistance parameter is > 0, then only points whose distance
   to the given point is <= maximumDistance will be returned.  Using a
   positive value of maximumDistance can substantially speed up the search.

   :param rhino3dm.Point3d testPoint: Point to project.
   :param float maximumDistance: The maximum allowed distance. \
      Past this distance, the search is given up and False is returned.Use 0 to turn off this parameter.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: ClosestPoints(thisCurve, otherCurve, multiple=False)

   Gets closest points between this and another curves.

   :param rhino3dm.Curve otherCurve: The other curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success; False on error.
   :rtype: bool
.. py:function:: Contains(thisCurve, testPoint, multiple=False)

   Computes the relationship between a point and a closed curve region.
   This curve must be closed or the return value will be Unset.
   Both curve and point are projected to the World XY plane.

   :param rhino3dm.Point3d testPoint: Point to test.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Relationship between point and curve region.
   :rtype: PointContainment
.. py:function:: Contains1(thisCurve, testPoint, plane, multiple=False)

   Computes the relationship between a point and a closed curve region.
   This curve must be closed or the return value will be Unset.

   :param rhino3dm.Point3d testPoint: Point to test.
   :param rhino3dm.Plane plane: Plane in which to compare point and region.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Relationship between point and curve region.
   :rtype: PointContainment
.. py:function:: Contains2(thisCurve, testPoint, plane, tolerance, multiple=False)

   Computes the relationship between a point and a closed curve region.
   This curve must be closed or the return value will be Unset.

   :param rhino3dm.Point3d testPoint: Point to test.
   :param rhino3dm.Plane plane: Plane in which to compare point and region.
   :param float tolerance: Tolerance to use during comparison.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Relationship between point and curve region.
   :rtype: PointContainment
.. py:function:: ExtremeParameters(thisCurve, direction, multiple=False)

   Returns the parameter values of all local extrema.
   Parameter values are in increasing order so consecutive extrema
   define an interval on which each component of the curve is monotone.
   Note, non-periodic curves always return the end points.

   :param rhino3dm.Vector3d direction: The direction in which to perform the calculation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The parameter values of all local extrema.
   :rtype: float[]
.. py:function:: CreatePeriodicCurve(curve, multiple=False)

   Removes kinks from a curve. Periodic curves deform smoothly without kinks.

   :param rhino3dm.Curve curve: The curve to make periodic. Curve must have degree >= 2.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The resulting curve if successful, None otherwise.
   :rtype: rhino3dm.Curve
.. py:function:: CreatePeriodicCurve1(curve, smooth, multiple=False)

   Removes kinks from a curve. Periodic curves deform smoothly without kinks.

   :param rhino3dm.Curve curve: The curve to make periodic. Curve must have degree >= 2.
   :param bool smooth: If true, smooths any kinks in the curve and moves control points to make a smooth curve. \
      If false, control point locations are not changed or changed minimally (only one point may move) and only the knot vector is altered.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The resulting curve if successful, None otherwise.
   :rtype: rhino3dm.Curve
.. py:function:: PointAtLength(thisCurve, length, multiple=False)

   Gets a point at a certain length along the curve. The length must be
   non-negative and less than or equal to the length of the curve.
   Lengths will not be wrapped when the curve is closed or periodic.

   :param float length: Length along the curve between the start point and the returned point.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Point on the curve at the specified length from the start point or Poin3d.Unset on failure.
   :rtype: rhino3dm.Point3d
.. py:function:: PointAtNormalizedLength(thisCurve, length, multiple=False)

   Gets a point at a certain normalized length along the curve. The length must be
   between or including 0.0 and 1.0, where 0.0 equals the start of the curve and
   1.0 equals the end of the curve.

   :param float length: Normalized length along the curve between the start point and the returned point.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Point on the curve at the specified normalized length from the start point or Poin3d.Unset on failure.
   :rtype: rhino3dm.Point3d
.. py:function:: PerpendicularFrameAt(thisCurve, t, multiple=False)

   Return a 3d frame at a parameter. This is slightly different than FrameAt in
   that the frame is computed in a way so there is minimal rotation from one
   frame to the next.

   :param float t: Evaluation parameter.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: GetPerpendicularFrames(thisCurve, parameters, multiple=False)

   Gets a collection of perpendicular frames along the curve. Perpendicular frames
   are also known as 'Zero-twisting frames' and they minimize rotation from one frame to the next.

   :param list[float] parameters: A collection of strictly increasing curve parameters to place perpendicular frames on.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of perpendicular frames on success or None on failure.
   :rtype: rhino3dm.Plane[]
.. py:function:: GetLength(thisCurve, multiple=False)

   Gets the length of the curve with a fractional tolerance of 1.0e-8.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The length of the curve on success, or zero on failure.
   :rtype: float
.. py:function:: GetLength1(thisCurve, fractionalTolerance, multiple=False)

   Get the length of the curve.

   :param float fractionalTolerance: Desired fractional precision. \
      fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The length of the curve on success, or zero on failure.
   :rtype: float
.. py:function:: GetLength2(thisCurve, subdomain, multiple=False)

   Get the length of a sub-section of the curve with a fractional tolerance of 1e-8.

   :param rhino3dm.Interval subdomain: The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The length of the sub-curve on success, or zero on failure.
   :rtype: float
.. py:function:: GetLength3(thisCurve, fractionalTolerance, subdomain, multiple=False)

   Get the length of a sub-section of the curve.

   :param float fractionalTolerance: Desired fractional precision. \
      fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
   :param rhino3dm.Interval subdomain: The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The length of the sub-curve on success, or zero on failure.
   :rtype: float
.. py:function:: IsShort(thisCurve, tolerance, multiple=False)

   Used to quickly find short curves.

   :param float tolerance: Length threshold value for "shortness".
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if the length of the curve is <= tolerance.
   :rtype: bool
.. py:function:: IsShort1(thisCurve, tolerance, subdomain, multiple=False)

   Used to quickly find short curves.

   :param float tolerance: Length threshold value for "shortness".
   :param rhino3dm.Interval subdomain: The test is performed on the interval that is the intersection of sub-domain with Domain()
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if the length of the curve is <= tolerance.
   :rtype: bool
.. py:function:: RemoveShortSegments(thisCurve, tolerance, multiple=False)

   Looks for segments that are shorter than tolerance that can be removed.
   Does not change the domain, but it will change the relative parameterization.

   :param float tolerance: Tolerance which defines "short" segments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if removable short segments were found. \
      False if no removable short segments were found.
   :rtype: bool
.. py:function:: LengthParameter(thisCurve, segmentLength, multiple=False)

   Gets the parameter along the curve which coincides with a given length along the curve.
   A fractional tolerance of 1e-8 is used in this version of the function.

   :param float segmentLength: Length of segment to measure. Must be less than or equal to the length of the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: LengthParameter1(thisCurve, segmentLength, fractionalTolerance, multiple=False)

   Gets the parameter along the curve which coincides with a given length along the curve.

   :param float segmentLength: Length of segment to measure. Must be less than or equal to the length of the curve.
   :param float fractionalTolerance: Desired fractional precision. \
      fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: LengthParameter2(thisCurve, segmentLength, subdomain, multiple=False)

   Gets the parameter along the curve which coincides with a given length along the curve.
   A fractional tolerance of 1e-8 is used in this version of the function.

   :param float segmentLength: Length of segment to measure. Must be less than or equal to the length of the sub-domain.
   :param rhino3dm.Interval subdomain: The calculation is performed on the specified sub-domain of the curve rather than the whole curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: LengthParameter3(thisCurve, segmentLength, fractionalTolerance, subdomain, multiple=False)

   Gets the parameter along the curve which coincides with a given length along the curve.

   :param float segmentLength: Length of segment to measure. Must be less than or equal to the length of the sub-domain.
   :param float fractionalTolerance: Desired fractional precision. \
      fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
   :param rhino3dm.Interval subdomain: The calculation is performed on the specified sub-domain of the curve rather than the whole curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: NormalizedLengthParameter(thisCurve, s, multiple=False)

   Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
   A fractional tolerance of 1e-8 is used in this version of the function.

   :param float s: Normalized arc length parameter. \
      E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: NormalizedLengthParameter1(thisCurve, s, fractionalTolerance, multiple=False)

   Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.

   :param float s: Normalized arc length parameter. \
      E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
   :param float fractionalTolerance: Desired fractional precision. \
      fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: NormalizedLengthParameter2(thisCurve, s, subdomain, multiple=False)

   Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
   A fractional tolerance of 1e-8 is used in this version of the function.

   :param float s: Normalized arc length parameter. \
      E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
   :param rhino3dm.Interval subdomain: The calculation is performed on the specified sub-domain of the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: NormalizedLengthParameter3(thisCurve, s, fractionalTolerance, subdomain, multiple=False)

   Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.

   :param float s: Normalized arc length parameter. \
      E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
   :param float fractionalTolerance: Desired fractional precision. \
      fabs(("exact" length from start to t) - arc_length)/arc_length <= fractionalTolerance.
   :param rhino3dm.Interval subdomain: The calculation is performed on the specified sub-domain of the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: NormalizedLengthParameters(thisCurve, s, absoluteTolerance, multiple=False)

   Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
   A fractional tolerance of 1e-8 is used in this version of the function.

   :param float[] s: Array of normalized arc length parameters. \
      E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
   :param float absoluteTolerance: If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length \
      and the length of the curve segment from t[i] to t[i+1] will be <= absoluteTolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. \
      Null on failure.
   :rtype: float[]
.. py:function:: NormalizedLengthParameters1(thisCurve, s, absoluteTolerance, fractionalTolerance, multiple=False)

   Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.

   :param float[] s: Array of normalized arc length parameters. \
      E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
   :param float absoluteTolerance: If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length \
      and the length of the curve segment from t[i] to t[i+1] will be <= absoluteTolerance.
   :param float fractionalTolerance: Desired fractional precision for each segment. \
      fabs("true" length - actual length)/(actual length) <= fractionalTolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. \
      Null on failure.
   :rtype: float[]
.. py:function:: NormalizedLengthParameters2(thisCurve, s, absoluteTolerance, subdomain, multiple=False)

   Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
   A fractional tolerance of 1e-8 is used in this version of the function.

   :param float[] s: Array of normalized arc length parameters. \
      E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
   :param float absoluteTolerance: If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length \
      and the length of the curve segment from t[i] to t[i+1] will be <= absoluteTolerance.
   :param rhino3dm.Interval subdomain: The calculation is performed on the specified sub-domain of the curve. \
      A 0.0 s value corresponds to sub-domain->Min() and a 1.0 s value corresponds to sub-domain->Max().
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. \
      Null on failure.
   :rtype: float[]
.. py:function:: NormalizedLengthParameters3(thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain, multiple=False)

   Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.

   :param float[] s: Array of normalized arc length parameters. \
      E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
   :param float absoluteTolerance: If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length \
      and the length of the curve segment from t[i] to t[i+1] will be <= absoluteTolerance.
   :param float fractionalTolerance: Desired fractional precision for each segment. \
      fabs("true" length - actual length)/(actual length) <= fractionalTolerance.
   :param rhino3dm.Interval subdomain: The calculation is performed on the specified sub-domain of the curve. \
      A 0.0 s value corresponds to sub-domain->Min() and a 1.0 s value corresponds to sub-domain->Max().
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. \
      Null on failure.
   :rtype: float[]
.. py:function:: DivideByCount(thisCurve, segmentCount, includeEnds, multiple=False)

   Divide the curve into a number of equal-length segments.

   :param int segmentCount: Segment count. Note that the number of division points may differ from the segment count.
   :param bool includeEnds: If true, then the point at the start of the first division segment is returned.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: List of curve parameters at the division points on success, None on failure.
   :rtype: float[]
.. py:function:: DivideByCount1(thisCurve, segmentCount, includeEnds, multiple=False)

   Divide the curve into a number of equal-length segments.

   :param int segmentCount: Segment count. Note that the number of division points may differ from the segment count.
   :param bool includeEnds: If true, then the point at the start of the first division segment is returned.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array containing division curve parameters on success, None on failure.
   :rtype: float[]
.. py:function:: DivideByLength(thisCurve, segmentLength, includeEnds, multiple=False)

   Divide the curve into specific length segments.

   :param float segmentLength: The length of each and every segment (except potentially the last one).
   :param bool includeEnds: If true, then the point at the start of the first division segment is returned.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array containing division curve parameters if successful, None on failure.
   :rtype: float[]
.. py:function:: DivideByLength1(thisCurve, segmentLength, includeEnds, reverse, multiple=False)

   Divide the curve into specific length segments.

   :param float segmentLength: The length of each and every segment (except potentially the last one).
   :param bool includeEnds: If true, then the point at the start of the first division segment is returned.
   :param bool reverse: If true, then the divisions start from the end of the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array containing division curve parameters if successful, None on failure.
   :rtype: float[]
.. py:function:: DivideByLength2(thisCurve, segmentLength, includeEnds, multiple=False)

   Divide the curve into specific length segments.

   :param float segmentLength: The length of each and every segment (except potentially the last one).
   :param bool includeEnds: If true, then the point at the start of the first division segment is returned.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array containing division curve parameters if successful, None on failure.
   :rtype: float[]
.. py:function:: DivideByLength3(thisCurve, segmentLength, includeEnds, reverse, multiple=False)

   Divide the curve into specific length segments.

   :param float segmentLength: The length of each and every segment (except potentially the last one).
   :param bool includeEnds: If true, then the point at the start of the first division segment is returned.
   :param bool reverse: If true, then the divisions start from the end of the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array containing division curve parameters if successful, None on failure.
   :rtype: float[]
.. py:function:: DivideEquidistant(thisCurve, distance, multiple=False)

   Calculates 3d points on a curve where the linear distance between the points is equal.

   :param float distance: The distance between division points.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of equidistant points, or None on error.
   :rtype: rhino3dm.Point3d[]
.. py:function:: DivideAsContour(thisCurve, contourStart, contourEnd, interval, multiple=False)

   Divides this curve at fixed steps along a defined contour line.

   :param rhino3dm.Point3d contourStart: The start of the contouring line.
   :param rhino3dm.Point3d contourEnd: The end of the contouring line.
   :param float interval: A distance to measure on the contouring axis.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of points; or None on error.
   :rtype: rhino3dm.Point3d[]
.. py:function:: Trim(thisCurve, side, length, multiple=False)

   Shortens a curve by a given length

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Trimmed curve if successful, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Split(thisCurve, cutter, tolerance, multiple=False)

   Splits a curve into pieces using a polysurface.

   :param rhino3dm.Brep cutter: A cutting surface or polysurface.
   :param float tolerance: A tolerance for computing intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: Split1(thisCurve, cutter, tolerance, angleToleranceRadians, multiple=False)

   Splits a curve into pieces using a polysurface.

   :param rhino3dm.Brep cutter: A cutting surface or polysurface.
   :param float tolerance: A tolerance for computing intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: Split2(thisCurve, cutter, tolerance, multiple=False)

   Splits a curve into pieces using a surface.

   :param rhino3dm.Surface cutter: A cutting surface or polysurface.
   :param float tolerance: A tolerance for computing intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: Split3(thisCurve, cutter, tolerance, angleToleranceRadians, multiple=False)

   Splits a curve into pieces using a surface.

   :param rhino3dm.Surface cutter: A cutting surface or polysurface.
   :param float tolerance: A tolerance for computing intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: Extend(thisCurve, t0, t1, multiple=False)

   Where possible, analytically extends curve to include the given domain.
   This will not work on closed curves. The original curve will be identical to the
   restriction of the resulting curve to the original curve domain.

   :param float t0: Start of extension domain, if the start is not inside the \
      Domain of this curve, an attempt will be made to extend the curve.
   :param float t1: End of extension domain, if the end is not inside the \
      Domain of this curve, an attempt will be made to extend the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Extended curve on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Extend1(thisCurve, domain, multiple=False)

   Where possible, analytically extends curve to include the given domain.
   This will not work on closed curves. The original curve will be identical to the
   restriction of the resulting curve to the original curve domain.

   :param rhino3dm.Interval domain: Extension domain.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Extended curve on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Extend2(thisCurve, side, length, style, multiple=False)

   Extends a curve by a specific length.

   :param CurveEnd side: Curve end to extend.
   :param float length: Length to add to the curve end.
   :param CurveExtensionStyle style: Extension style.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A curve with extended ends or None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Extend3(thisCurve, side, style, geometry, multiple=False)

   Extends a curve until it intersects a collection of objects.

   :param CurveEnd side: The end of the curve to extend.
   :param CurveExtensionStyle style: The style or type of extension to use.
   :param System.Collections.Generic.IEnumerable<GeometryBase> geometry: A collection of objects. Allowable object types are Curve, Surface, Brep.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New extended curve result on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Extend4(thisCurve, side, style, endPoint, multiple=False)

   Extends a curve to a point.

   :param CurveEnd side: The end of the curve to extend.
   :param CurveExtensionStyle style: The style or type of extension to use.
   :param rhino3dm.Point3d endPoint: A new end point.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New extended curve result on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: ExtendOnSurface(thisCurve, side, surface, multiple=False)

   Extends a curve on a surface.

   :param CurveEnd side: The end of the curve to extend.
   :param rhino3dm.Surface surface: Surface that contains the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New extended curve result on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: ExtendOnSurface1(thisCurve, side, face, multiple=False)

   Extends a curve on a surface.

   :param CurveEnd side: The end of the curve to extend.
   :param rhino3dm.BrepFace face: BrepFace that contains the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New extended curve result on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: ExtendByLine(thisCurve, side, geometry, multiple=False)

   Extends a curve by a line until it intersects a collection of objects.

   :param CurveEnd side: The end of the curve to extend.
   :param System.Collections.Generic.IEnumerable<GeometryBase> geometry: A collection of objects. Allowable object types are Curve, Surface, Brep.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New extended curve result on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: ExtendByArc(thisCurve, side, geometry, multiple=False)

   Extends a curve by an Arc until it intersects a collection of objects.

   :param CurveEnd side: The end of the curve to extend.
   :param System.Collections.Generic.IEnumerable<GeometryBase> geometry: A collection of objects. Allowable object types are Curve, Surface, Brep.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New extended curve result on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Simplify(thisCurve, options, distanceTolerance, angleToleranceRadians, multiple=False)

   Returns a geometrically equivalent PolyCurve.
   The PolyCurve has the following properties
   1. All the PolyCurve segments are LineCurve, PolylineCurve, ArcCurve, or NurbsCurve.
   
   2. The NURBS Curves segments do not have fully multiple interior knots.
   
   3. Rational NURBS curves do not have constant weights.
   
   4. Any segment for which IsLinear() or IsArc() is True is a Line,
   Polyline segment, or an Arc.
   
   5. Adjacent co-linear or co-circular segments are combined.
   
   6. Segments that meet with G1-continuity have there ends tuned up so
   that they meet with G1-continuity to within machine precision.

   :param CurveSimplifyOptions options: Simplification options.
   :param float distanceTolerance: A distance tolerance for the simplification.
   :param float angleToleranceRadians: An angle tolerance for the simplification.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New simplified curve on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: SimplifyEnd(thisCurve, end, options, distanceTolerance, angleToleranceRadians, multiple=False)

   Same as SimplifyCurve, but simplifies only the last two segments at "side" end.

   :param CurveEnd end: If CurveEnd.Start the function simplifies the last two start \
      side segments, otherwise if CurveEnd.End the last two end side segments are simplified.
   :param CurveSimplifyOptions options: Simplification options.
   :param float distanceTolerance: A distance tolerance for the simplification.
   :param float angleToleranceRadians: An angle tolerance for the simplification.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New simplified curve on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Fair(thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations, multiple=False)

   Fairs a curve object. Fair works best on degree 3 (cubic) curves. Attempts to
   remove large curvature variations while limiting the geometry changes to be no
   more than the specified tolerance.

   :param float distanceTolerance: Maximum allowed distance the faired curve is allowed to deviate from the input.
   :param float angleTolerance: (in radians) kinks with angles <= angleTolerance are smoothed out 0.05 is a good default.
   :param int clampStart: The number of (control vertices-1) to preserve at start. \
      0 = preserve start point1 = preserve start point and 1st derivative2 = preserve start point, 1st and 2nd derivative
   :param int clampEnd: Same as clampStart.
   :param int iterations: The number of iterations to use in adjusting the curve.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Returns new faired Curve on success, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Fit(thisCurve, degree, fitTolerance, angleTolerance, multiple=False)

   Fits a new curve through an existing curve.

   :param int degree: The degree of the returned Curve. Must be bigger than 1.
   :param float fitTolerance: The fitting tolerance. If fitTolerance is RhinoMath.UnsetValue or <=0.0, \
      the document absolute tolerance is used.
   :param float angleTolerance: The kink smoothing tolerance in radians. \
      If angleTolerance is 0.0, all kinks are smoothedIf angleTolerance is >0.0, kinks smaller than angleTolerance are smoothedIf angleTolerance is RhinoMath.UnsetValue or <0.0, the document angle tolerance is used for the kink smoothing
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Returns a new fitted Curve if successful, None on failure.
   :rtype: rhino3dm.Curve
.. py:function:: Rebuild(thisCurve, pointCount, degree, preserveTangents, multiple=False)

   Rebuild a curve with a specific point count.

   :param int pointCount: Number of control points in the rebuild curve.
   :param int degree: Degree of curve. Valid values are between and including 1 and 11.
   :param bool preserveTangents: If true, the end tangents of the input curve will be preserved.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A NURBS curve on success or None on failure.
   :rtype: rhino3dm.NurbsCurve
.. py:function:: ToPolyline(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, multiple=False)

   Gets a polyline approximation of a curve.

   :param int mainSegmentCount: If mainSegmentCount <= 0, then both subSegmentCount and mainSegmentCount are ignored. \
      If mainSegmentCount > 0, then subSegmentCount must be >= 1. In this \
      case the NURBS will be broken into mainSegmentCount equally spaced \
      chords. If needed, each of these chords can be split into as many \
      subSegmentCount sub-parts if the subdivision is necessary for the \
      mesh to meet the other meshing constraints. In particular, if \
      subSegmentCount = 0, then the curve is broken into mainSegmentCount \
      pieces and no further testing is performed.
   :param int subSegmentCount: An amount of subsegments.
   :param float maxAngleRadians: ( 0 to pi ) Maximum angle (in radians) between unit tangents at \
      adjacent vertices.
   :param float maxChordLengthRatio: Maximum permitted value of \
      (distance chord midpoint to curve) / (length of chord).
   :param float maxAspectRatio: If maxAspectRatio < 1.0, the parameter is ignored. \
      If 1 <= maxAspectRatio < sqrt(2), it is treated as if maxAspectRatio = sqrt(2). \
      This parameter controls the maximum permitted value of \
      (length of longest chord) / (length of shortest chord).
   :param float tolerance: If tolerance = 0, the parameter is ignored. \
      This parameter controls the maximum permitted value of the \
      distance from the curve to the polyline.
   :param float minEdgeLength: The minimum permitted edge length.
   :param float maxEdgeLength: If maxEdgeLength = 0, the parameter \
      is ignored. This parameter controls the maximum permitted edge length.
   :param bool keepStartPoint: If True the starting point of the curve \
      is added to the polyline. If False the starting point of the curve is \
      not added to the polyline.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: PolylineCurve on success, None on error.
   :rtype: PolylineCurve
.. py:function:: ToPolyline1(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain, multiple=False)

   Gets a polyline approximation of a curve.

   :param int mainSegmentCount: If mainSegmentCount <= 0, then both subSegmentCount and mainSegmentCount are ignored. \
      If mainSegmentCount > 0, then subSegmentCount must be >= 1. In this \
      case the NURBS will be broken into mainSegmentCount equally spaced \
      chords. If needed, each of these chords can be split into as many \
      subSegmentCount sub-parts if the subdivision is necessary for the \
      mesh to meet the other meshing constraints. In particular, if \
      subSegmentCount = 0, then the curve is broken into mainSegmentCount \
      pieces and no further testing is performed.
   :param int subSegmentCount: An amount of subsegments.
   :param float maxAngleRadians: ( 0 to pi ) Maximum angle (in radians) between unit tangents at \
      adjacent vertices.
   :param float maxChordLengthRatio: Maximum permitted value of \
      (distance chord midpoint to curve) / (length of chord).
   :param float maxAspectRatio: If maxAspectRatio < 1.0, the parameter is ignored. \
      If 1 <= maxAspectRatio < sqrt(2), it is treated as if maxAspectRatio = sqrt(2). \
      This parameter controls the maximum permitted value of \
      (length of longest chord) / (length of shortest chord).
   :param float tolerance: If tolerance = 0, the parameter is ignored. \
      This parameter controls the maximum permitted value of the \
      distance from the curve to the polyline.
   :param float minEdgeLength: The minimum permitted edge length.
   :param float maxEdgeLength: If maxEdgeLength = 0, the parameter \
      is ignored. This parameter controls the maximum permitted edge length.
   :param bool keepStartPoint: If True the starting point of the curve \
      is added to the polyline. If False the starting point of the curve is \
      not added to the polyline.
   :param rhino3dm.Interval curveDomain: This sub-domain of the NURBS curve is approximated.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: PolylineCurve on success, None on error.
   :rtype: PolylineCurve
.. py:function:: ToPolyline2(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength, multiple=False)

   Gets a polyline approximation of a curve.

   :param float tolerance: The tolerance. This is the maximum deviation from line midpoints to the curve. When in doubt, use the document's model space absolute tolerance.
   :param float angleTolerance: The angle tolerance in radians. This is the maximum deviation of the line directions. When in doubt, use the document's model space angle tolerance.
   :param float minimumLength: The minimum segment length.
   :param float maximumLength: The maximum segment length.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: PolyCurve on success, None on error.
   :rtype: PolylineCurve
.. py:function:: ToArcsAndLines(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength, multiple=False)

   Converts a curve into polycurve consisting of arc segments. Sections of the input curves that are nearly straight are converted to straight-line segments.

   :param float tolerance: The tolerance. This is the maximum deviation from arc midpoints to the curve. When in doubt, use the document's model space absolute tolerance.
   :param float angleTolerance: The angle tolerance in radians. This is the maximum deviation of the arc end directions from the curve direction. When in doubt, use the document's model space angle tolerance.
   :param float minimumLength: The minimum segment length.
   :param float maximumLength: The maximum segment length.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: PolyCurve on success, None on error.
   :rtype: PolyCurve
.. py:function:: PullToMesh(thisCurve, mesh, tolerance, multiple=False)

   Makes a polyline approximation of the curve and gets the closest point on the mesh for each point on the curve.
   Then it "connects the points" so that you have a polyline on the mesh.

   :param rhino3dm.Mesh mesh: Mesh to project onto.
   :param float tolerance: Input tolerance (RhinoDoc.ModelAbsoluteTolerance is a good default)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A polyline curve on success, None on failure.
   :rtype: PolylineCurve
.. py:function:: Offset(thisCurve, plane, distance, tolerance, cornerStyle, multiple=False)

   Offsets this curve. If you have a nice offset, then there will be one entry in
   the array. If the original curve had kinks or the offset curve had self
   intersections, you will get multiple segments in the output array.

   :param rhino3dm.Plane plane: Offset solution plane.
   :param float distance: The positive or negative distance to offset.
   :param float tolerance: The offset or fitting tolerance.
   :param CurveOffsetCornerStyle cornerStyle: Corner style for offset kinks.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: Offset1(thisCurve, directionPoint, normal, distance, tolerance, cornerStyle, multiple=False)

   Offsets this curve. If you have a nice offset, then there will be one entry in
   the array. If the original curve had kinks or the offset curve had self
   intersections, you will get multiple segments in the output array.

   :param rhino3dm.Point3d directionPoint: A point that indicates the direction of the offset.
   :param rhino3dm.Vector3d normal: The normal to the offset plane.
   :param float distance: The positive or negative distance to offset.
   :param float tolerance: The offset or fitting tolerance.
   :param CurveOffsetCornerStyle cornerStyle: Corner style for offset kinks.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: Offset2(thisCurve, directionPoint, normal, distance, tolerance, angleTolerance, loose, cornerStyle, endStyle, multiple=False)

   Offsets this curve. If you have a nice offset, then there will be one entry in
   the array. If the original curve had kinks or the offset curve had self
   intersections, you will get multiple segments in the output array.

   :param rhino3dm.Point3d directionPoint: A point that indicates the direction of the offset.
   :param rhino3dm.Vector3d normal: The normal to the offset plane.
   :param float distance: The positive or negative distance to offset.
   :param float tolerance: The offset or fitting tolerance.
   :param float angleTolerance: The angle tolerance, in radians, used to decide whether to split at kinks.
   :param bool loose: If false, offset within tolerance. If true, offset by moving edit points.
   :param CurveOffsetCornerStyle cornerStyle: Corner style for offset kinks.
   :param CurveOffsetEndStyle endStyle: End style for non-loose, non-closed curve offsets.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: RibbonOffset(thisCurve, distance, blendRadius, directionPoint, normal, tolerance, multiple=False)

   Offsets a closed curve in the following way: pProject the curve to a plane with given normal.
   Then, loose Offset the projection by distance + blend_radius and trim off self-intersection.
   THen, Offset the remaining curve back in the opposite direction by blend_radius, filling gaps with blends.
   Finally, use the elevations of the input curve to get the correct elevations of the result.

   :param float distance: The positive distance to offset the curve.
   :param float blendRadius: Positive, typically the same as distance. When the offset results in a self-intersection \
      that gets trimmed off at a kink, the kink will be blended out using this radius.
   :param rhino3dm.Point3d directionPoint: A point that indicates the direction of the offset. If the offset is inward, \
      the point's projection to the plane should be well within the curve. \
      It will be used to decide which part of the offset to keep if there are self-intersections.
   :param rhino3dm.Vector3d normal: A vector that indicates the normal of the plane in which the offset will occur.
   :param float tolerance: Used to determine self-intersections, not offset error.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The offset curve if successful.
   :rtype: rhino3dm.Curve
.. py:function:: OffsetOnSurface(thisCurve, face, distance, fittingTolerance, multiple=False)

   Offset this curve on a brep face surface. This curve must lie on the surface.

   :param rhino3dm.BrepFace face: The brep face on which to offset.
   :param float distance: A distance to offset (+)left, (-)right.
   :param float fittingTolerance: A fitting tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, or None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: OffsetOnSurface1(thisCurve, face, throughPoint, fittingTolerance, multiple=False)

   Offset a curve on a brep face surface. This curve must lie on the surface.
   This overload allows to specify a surface point at which the offset will pass.

   :param rhino3dm.BrepFace face: The brep face on which to offset.
   :param rhino3dm.Point2d throughPoint: 2d point on the brep face to offset through.
   :param float fittingTolerance: A fitting tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, or None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: OffsetOnSurface2(thisCurve, face, curveParameters, offsetDistances, fittingTolerance, multiple=False)

   Offset a curve on a brep face surface. This curve must lie on the surface.
   This overload allows to specify different offsets for different curve parameters.

   :param rhino3dm.BrepFace face: The brep face on which to offset.
   :param float[] curveParameters: Curve parameters corresponding to the offset distances.
   :param float[] offsetDistances: distances to offset (+)left, (-)right.
   :param float fittingTolerance: A fitting tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, or None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: OffsetOnSurface3(thisCurve, surface, distance, fittingTolerance, multiple=False)

   Offset a curve on a surface. This curve must lie on the surface.

   :param rhino3dm.Surface surface: A surface on which to offset.
   :param float distance: A distance to offset (+)left, (-)right.
   :param float fittingTolerance: A fitting tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, or None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: OffsetOnSurface4(thisCurve, surface, throughPoint, fittingTolerance, multiple=False)

   Offset a curve on a surface. This curve must lie on the surface.
   This overload allows to specify a surface point at which the offset will pass.

   :param rhino3dm.Surface surface: A surface on which to offset.
   :param rhino3dm.Point2d throughPoint: 2d point on the brep face to offset through.
   :param float fittingTolerance: A fitting tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, or None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: OffsetOnSurface5(thisCurve, surface, curveParameters, offsetDistances, fittingTolerance, multiple=False)

   Offset this curve on a surface. This curve must lie on the surface.
   This overload allows to specify different offsets for different curve parameters.

   :param rhino3dm.Surface surface: A surface on which to offset.
   :param float[] curveParameters: Curve parameters corresponding to the offset distances.
   :param float[] offsetDistances: Distances to offset (+)left, (-)right.
   :param float fittingTolerance: A fitting tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curves on success, or None on failure.
   :rtype: rhino3dm.Curve[]
.. py:function:: PullToBrepFace1(thisCurve, face, tolerance, multiple=False)

   Pulls this curve to a brep face and returns the result of that operation.

   :param rhino3dm.BrepFace face: A brep face.
   :param float tolerance: A tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array containing the resulting curves after pulling. This array could be empty.
   :rtype: rhino3dm.Curve[]
.. py:function:: OffsetNormalToSurface(thisCurve, surface, height, multiple=False)

   Finds a curve by offsetting an existing curve normal to a surface.
   The caller is responsible for ensuring that the curve lies on the input surface.

   :param rhino3dm.Surface surface: Surface from which normals are calculated.
   :param float height: offset distance (distance from surface to result curve)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Offset curve at distance height from the surface.  The offset curve is \
      interpolated through a small number of points so if the surface is irregular \
      or complicated, the result will not be a very accurate offset.
   :rtype: rhino3dm.Curve
