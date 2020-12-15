BrepFace
========

.. py:module:: compute_rhino3d.BrepFace

.. py:function:: PullPointsToFace(thisBrepFace, points, tolerance, multiple=False)

   Pulls one or more points to a brep face.

   :param list[rhino3dm.Point3d] points: Points to pull.
   :param float tolerance: Tolerance for pulling operation. Only points that are closer than tolerance will be pulled to the face.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of pulled points.
   :rtype: rhino3dm.Point3d[]
.. py:function:: DraftAnglePoint(thisBrepFace, testPoint, testAngle, pullDirection, edge, multiple=False)

   Returns the surface draft angle and point at a parameter.

   :param rhino3dm.Point2d testPoint: The u,v parameter on the face to evaluate.
   :param float testAngle: The angle in radians to test.
   :param rhino3dm.Vector3d pullDirection: The pull direction.
   :param bool edge: Restricts the point placement to an edge.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. py:function:: RemoveHoles(thisBrepFace, tolerance, multiple=False)

   Remove all inner loops, or holes, from a Brep face.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: rhino3dm.Brep
.. py:function:: ShrinkSurfaceToEdge(thisBrepFace, multiple=False)

   Shrinks the underlying untrimmed surface of this Brep face right to the trimming boundaries.
   Note, shrinking the trimmed surface can sometimes cause problems later since having
   the edges so close to the trimming boundaries can cause commands that use the surface
   edges as input to fail.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success, False on failure.
   :rtype: bool
.. py:function:: Split(thisBrepFace, curves, tolerance, multiple=False)

   Split this face using 3D trimming curves.

   :param list[rhino3dm.Curve] curves: Curves to split with.
   :param float tolerance: Tolerance for splitting, when in doubt use the Document Absolute Tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A brep consisting of all the split fragments, or None on failure.
   :rtype: rhino3dm.Brep
.. py:function:: IsPointOnFace(thisBrepFace, u, v, multiple=False)

   Tests if a parameter space point is in the active region of a face.

   :param float u: Parameter space point U value.
   :param float v: Parameter space point V value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A value describing the relationship between the point and the face.
   :rtype: PointFaceRelation
.. py:function:: IsPointOnFace1(thisBrepFace, u, v, tolerance, multiple=False)

   Tests if a parameter space point is in the active region of a face.

   :param float u: Parameter space point U value.
   :param float v: Parameter space point V value.
   :param float tolerance: 3D tolerance used when checking to see if the point is on a face or inside of a loop.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A value describing the relationship between the point and the face.
   :rtype: PointFaceRelation
.. py:function:: TrimAwareIsoIntervals(thisBrepFace, direction, constantParameter, multiple=False)

   Gets intervals where the iso curve exists on a BrepFace (trimmed surface)

   :param int direction: Direction of isocurve. \
      0 = Isocurve connects all points with a constant U value.1 = Isocurve connects all points with a constant V value.
   :param float constantParameter: Surface parameter that remains identical along the isocurves.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: If direction = 0, the parameter space iso interval connects the 2d points \
      (intervals[i][0],iso_constant) and (intervals[i][1],iso_constant). \
      If direction = 1, the parameter space iso interval connects the 2d points \
      (iso_constant,intervals[i][0]) and (iso_constant,intervals[i][1]).
   :rtype: rhino3dm.Interval[]
.. py:function:: TrimAwareIsoCurve(thisBrepFace, direction, constantParameter, multiple=False)

   Similar to IsoCurve function, except this function pays attention to trims on faces
   and may return multiple curves.

   :param int direction: Direction of isocurve. \
      0 = Isocurve connects all points with a constant U value.1 = Isocurve connects all points with a constant V value.
   :param float constantParameter: Surface parameter that remains identical along the isocurves.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Isoparametric curves connecting all points with the constantParameter value.
   :rtype: rhino3dm.Curve[]
.. py:function:: ChangeSurface(thisBrepFace, surfaceIndex, multiple=False)

   Expert user tool that replaces the 3d surface geometry use by the face.

   :param int surfaceIndex: brep surface index of new surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful.
   :rtype: bool
.. py:function:: RebuildEdges(thisBrepFace, tolerance, rebuildSharedEdges, rebuildVertices, multiple=False)

   Rebuild the edges used by a face so they lie on the surface.

   :param float tolerance: tolerance for fitting 3d edge curves.
   :param bool rebuildSharedEdges: if False and edge is used by this face and a neighbor, then the edge \
      will be skipped.
   :param bool rebuildVertices: if true, vertex locations are updated to lie on the surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success.
   :rtype: bool
