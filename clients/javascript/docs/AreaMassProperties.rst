RhinoCompute.AreaMassProperties
===============================

.. js:module:: RhinoCompute

.. js:function:: RhinoCompute.AreaMassProperties.compute(closedPlanarCurve, multiple=false)

   Computes an AreaMassProperties for a closed planar curve.

   :param rhino3dm.Curve closedPlanarCurve: Curve to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given curve or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute1(closedPlanarCurve, planarTolerance, multiple=false)

   Computes an AreaMassProperties for a closed planar curve.

   :param rhino3dm.Curve closedPlanarCurve: Curve to measure.
   :param float planarTolerance: absolute tolerance used to insure the closed curve is planar
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given curve or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute2(hatch, multiple=false)

   Computes an AreaMassProperties for a hatch.

   :param Hatch hatch: Hatch to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given hatch or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute3(mesh, multiple=false)

   Computes an AreaMassProperties for a mesh.

   :param rhino3dm.Mesh mesh: Mesh to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Mesh or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute4(mesh, area, firstMoments, secondMoments, productMoments, multiple=false)

   Compute the AreaMassProperties for a single Mesh.

   :param rhino3dm.Mesh mesh: Mesh to measure.
   :param bool area: True to calculate area.
   :param bool firstMoments: True to calculate area first moments, area, and area centroid.
   :param bool secondMoments: True to calculate area second moments.
   :param bool productMoments: True to calculate area product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Mesh or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute5(brep, multiple=false)

   Computes an AreaMassProperties for a brep.

   :param rhino3dm.Brep brep: Brep to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Brep or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute6(brep, area, firstMoments, secondMoments, productMoments, multiple=false)

   Compute the AreaMassProperties for a single Brep.

   :param rhino3dm.Brep brep: Brep to measure.
   :param bool area: True to calculate area.
   :param bool firstMoments: True to calculate area first moments, area, and area centroid.
   :param bool secondMoments: True to calculate area second moments.
   :param bool productMoments: True to calculate area product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Brep or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute7(surface, multiple=false)

   Computes an AreaMassProperties for a surface.

   :param rhino3dm.Surface surface: Surface to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Surface or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute8(surface, area, firstMoments, secondMoments, productMoments, multiple=false)

   Compute the AreaMassProperties for a single Surface.

   :param rhino3dm.Surface surface: Surface to measure.
   :param bool area: True to calculate area.
   :param bool firstMoments: True to calculate area first moments, area, and area centroid.
   :param bool secondMoments: True to calculate area second moments.
   :param bool productMoments: True to calculate area product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Surface or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute9(geometry, multiple=false)

   Computes the Area properties for a collection of geometric objects.
   At present only Breps, Surfaces, Meshes and Planar Closed Curves are supported.

   :param list[rhino3dm.GeometryBase] geometry: Objects to include in the area computation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The Area properties for the entire collection or None on failure.
   :rtype: AreaMassProperties
.. js:function:: RhinoCompute.AreaMassProperties.compute10(geometry, area, firstMoments, secondMoments, productMoments, multiple=false)

   Computes the AreaMassProperties for a collection of geometric objects.
   At present only Breps, Surfaces, Meshes and Planar Closed Curves are supported.

   :param list[rhino3dm.GeometryBase] geometry: Objects to include in the area computation.
   :param bool area: True to calculate area.
   :param bool firstMoments: True to calculate area first moments, area, and area centroid.
   :param bool secondMoments: True to calculate area second moments.
   :param bool productMoments: True to calculate area product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the entire collection or None on failure.
   :rtype: AreaMassProperties
