BezierCurve
===========

.. py:module:: compute_rhino3d.BezierCurve

.. py:function:: CreateCubicBeziers(sourceCurve, distanceTolerance, kinkTolerance, multiple=False)

   Constructs an array of cubic, non-rational Beziers that fit a curve to a tolerance.

   :param rhino3dm.Curve sourceCurve: A curve to approximate.
   :param float distanceTolerance: The max fitting error. Use RhinoMath.SqrtEpsilon as a minimum.
   :param float kinkTolerance: If the input curve has a g1-discontinuity with angle radian measure \
      greater than kinkTolerance at some point P, the list of beziers will \
      also have a kink at P.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of bezier curves. The array can be empty and might contain None items.
   :rtype: rhino3dm.BezierCurve[]
.. py:function:: CreateBeziers(sourceCurve, multiple=False)

   Create an array of Bezier curves that fit to an existing curve. Please note, these
   Beziers can be of any order and may be rational.

   :param rhino3dm.Curve sourceCurve: The curve to fit Beziers to
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of Bezier curves
   :rtype: rhino3dm.BezierCurve[]
