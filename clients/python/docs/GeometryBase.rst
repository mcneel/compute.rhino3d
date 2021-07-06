GeometryBase
============

.. py:module:: compute_rhino3d.GeometryBase

.. py:function:: GetBoundingBox(thisGeometryBase, accurate, multiple=False)

   Bounding box solver. Gets the world axis aligned bounding box for the geometry.

   :param bool accurate: If true, a physically accurate bounding box will be computed. \
      If not, a bounding box estimate will be computed. For some geometry types there is no \
      difference between the estimate and the accurate bounding box. Estimated bounding boxes \
      can be computed much (much) faster than accurate (or "tight") bounding boxes. \
      Estimated bounding boxes are always similar to or larger than accurate bounding boxes.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The bounding box of the geometry in world coordinates or BoundingBox.Empty \
      if not bounding box could be found.
   :rtype: rhino3dm.BoundingBox
.. py:function:: GetBoundingBox1(thisGeometryBase, xform, multiple=False)

   Aligned Bounding box solver. Gets the world axis aligned bounding box for the transformed geometry.

   :param Transform xform: Transformation to apply to object prior to the BoundingBox computation. \
      The geometry itself is not modified.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The accurate bounding box of the transformed geometry in world coordinates \
      or BoundingBox.Empty if not bounding box could be found.
   :rtype: rhino3dm.BoundingBox
.. py:function:: GeometryEquals(first, second, multiple=False)

   Determines if two geometries equal one another, in pure geometrical shape.
   This version only compares the geometry itself and does not include any user
   data comparisons.
   This is a comparison by value: for two identical items it will be true, no matter
   where in memory they may be stored.

   :param rhino3dm.GeometryBase first: The first geometry
   :param rhino3dm.GeometryBase second: The second geometry
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The indication of equality
   :rtype: bool
