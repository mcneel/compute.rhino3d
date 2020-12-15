Extrusion
=========

.. py:module:: compute_rhino3d.Extrusion

.. py:function:: GetWireframe(thisExtrusion, multiple=False)

   Constructs all the Wireframe curves for this Extrusion.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Wireframe curves.
   :rtype: rhino3dm.Curve[]
