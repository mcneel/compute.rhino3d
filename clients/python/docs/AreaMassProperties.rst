AreaMassProperties
==================

.. py:module:: compute_rhino3d.AreaMassProperties

.. py:function:: Compute(closedPlanarCurve, multiple=False)

   Computes an AreaMassProperties for a closed planar curve.

   :param rhino3dm.Curve closedPlanarCurve: Curve to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given curve or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute1(closedPlanarCurve, planarTolerance, multiple=False)

   Computes an AreaMassProperties for a closed planar curve.

   :param rhino3dm.Curve closedPlanarCurve: Curve to measure.
   :param float planarTolerance: absolute tolerance used to insure the closed curve is planar
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given curve or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute2(hatch, multiple=False)

   Computes an AreaMassProperties for a hatch.

   :param Hatch hatch: Hatch to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given hatch or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute3(mesh, multiple=False)

   Computes an AreaMassProperties for a mesh.

   :param rhino3dm.Mesh mesh: Mesh to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Mesh or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute4(mesh, area, firstMoments, secondMoments, productMoments, multiple=False)

   Compute the AreaMassProperties for a single Mesh.

   :param rhino3dm.Mesh mesh: Mesh to measure.
   :param bool area: True to calculate area.
   :param bool firstMoments: True to calculate area first moments, area, and area centroid.
   :param bool secondMoments: True to calculate area second moments.
   :param bool productMoments: True to calculate area product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Mesh or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute5(brep, multiple=False)

   Computes an AreaMassProperties for a brep.

   :param rhino3dm.Brep brep: Brep to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Brep or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute6(brep, area, firstMoments, secondMoments, productMoments, multiple=False)

   Compute the AreaMassProperties for a single Brep.

   :param rhino3dm.Brep brep: Brep to measure.
   :param bool area: True to calculate area.
   :param bool firstMoments: True to calculate area first moments, area, and area centroid.
   :param bool secondMoments: True to calculate area second moments.
   :param bool productMoments: True to calculate area product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Brep or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute7(surface, multiple=False)

   Computes an AreaMassProperties for a surface.

   :param rhino3dm.Surface surface: Surface to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Surface or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute8(surface, area, firstMoments, secondMoments, productMoments, multiple=False)

   Compute the AreaMassProperties for a single Surface.

   :param rhino3dm.Surface surface: Surface to measure.
   :param bool area: True to calculate area.
   :param bool firstMoments: True to calculate area first moments, area, and area centroid.
   :param bool secondMoments: True to calculate area second moments.
   :param bool productMoments: True to calculate area product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The AreaMassProperties for the given Surface or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute9(geometry, multiple=False)

   Computes the Area properties for a collection of geometric objects.
   At present only Breps, Surfaces, Meshes and Planar Closed Curves are supported.

   :param list[rhino3dm.GeometryBase] geometry: Objects to include in the area computation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The Area properties for the entire collection or None on failure.
   :rtype: AreaMassProperties
.. py:function:: Compute10(geometry, area, firstMoments, secondMoments, productMoments, multiple=False)

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
