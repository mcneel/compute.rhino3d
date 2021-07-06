VolumeMassProperties
====================

.. py:module:: compute_rhino3d.VolumeMassProperties

.. py:function:: Compute(mesh, multiple=False)

   Compute the VolumeMassProperties for a single Mesh.

   :param rhino3dm.Mesh mesh: Mesh to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Mesh or None on failure.
   :rtype: VolumeMassProperties
.. py:function:: Compute1(mesh, volume, firstMoments, secondMoments, productMoments, multiple=False)

   Compute the VolumeMassProperties for a single Mesh.

   :param rhino3dm.Mesh mesh: Mesh to measure.
   :param bool volume: True to calculate volume.
   :param bool firstMoments: True to calculate volume first moments, volume, and volume centroid.
   :param bool secondMoments: True to calculate volume second moments.
   :param bool productMoments: True to calculate volume product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Mesh or None on failure.
   :rtype: VolumeMassProperties
.. py:function:: Compute2(brep, multiple=False)

   Compute the VolumeMassProperties for a single Brep.

   :param rhino3dm.Brep brep: Brep to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Brep or None on failure.
   :rtype: VolumeMassProperties
.. py:function:: Compute3(brep, volume, firstMoments, secondMoments, productMoments, multiple=False)

   Compute the VolumeMassProperties for a single Brep.

   :param rhino3dm.Brep brep: Brep to measure.
   :param bool volume: True to calculate volume.
   :param bool firstMoments: True to calculate volume first moments, volume, and volume centroid.
   :param bool secondMoments: True to calculate volume second moments.
   :param bool productMoments: True to calculate volume product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Brep or None on failure.
   :rtype: VolumeMassProperties
.. py:function:: Compute4(surface, multiple=False)

   Compute the VolumeMassProperties for a single Surface.

   :param rhino3dm.Surface surface: Surface to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Surface or None on failure.
   :rtype: VolumeMassProperties
.. py:function:: Compute5(surface, volume, firstMoments, secondMoments, productMoments, multiple=False)

   Compute the VolumeMassProperties for a single Surface.

   :param rhino3dm.Surface surface: Surface to measure.
   :param bool volume: True to calculate volume.
   :param bool firstMoments: True to calculate volume first moments, volume, and volume centroid.
   :param bool secondMoments: True to calculate volume second moments.
   :param bool productMoments: True to calculate volume product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Surface or None on failure.
   :rtype: VolumeMassProperties
.. py:function:: Compute6(geometry, multiple=False)

   Computes the VolumeMassProperties for a collection of geometric objects.
   At present only Breps, Surfaces, and Meshes are supported.

   :param list[rhino3dm.GeometryBase] geometry: Objects to include in the area computation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the entire collection or None on failure.
   :rtype: VolumeMassProperties
.. py:function:: Compute7(geometry, volume, firstMoments, secondMoments, productMoments, multiple=False)

   Computes the VolumeMassProperties for a collection of geometric objects.
   At present only Breps, Surfaces, Meshes and Planar Closed Curves are supported.

   :param list[rhino3dm.GeometryBase] geometry: Objects to include in the area computation.
   :param bool volume: True to calculate volume.
   :param bool firstMoments: True to calculate volume first moments, volume, and volume centroid.
   :param bool secondMoments: True to calculate volume second moments.
   :param bool productMoments: True to calculate volume product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the entire collection or None on failure.
   :rtype: VolumeMassProperties
.. py:function:: Sum(thisVolumeMassProperties, summand, multiple=False)

   Sum mass properties together to get an aggregate mass.

   :param VolumeMassProperties summand: mass properties to add.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful.
   :rtype: bool
