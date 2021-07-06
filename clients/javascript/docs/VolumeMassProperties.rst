RhinoCompute.VolumeMassProperties
=================================

.. js:module:: RhinoCompute

.. js:function:: RhinoCompute.VolumeMassProperties.compute(mesh, multiple=false)

   Compute the VolumeMassProperties for a single Mesh.

   :param rhino3dm.Mesh mesh: Mesh to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Mesh or None on failure.
   :rtype: VolumeMassProperties
.. js:function:: RhinoCompute.VolumeMassProperties.compute1(mesh, volume, firstMoments, secondMoments, productMoments, multiple=false)

   Compute the VolumeMassProperties for a single Mesh.

   :param rhino3dm.Mesh mesh: Mesh to measure.
   :param bool volume: True to calculate volume.
   :param bool firstMoments: True to calculate volume first moments, volume, and volume centroid.
   :param bool secondMoments: True to calculate volume second moments.
   :param bool productMoments: True to calculate volume product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Mesh or None on failure.
   :rtype: VolumeMassProperties
.. js:function:: RhinoCompute.VolumeMassProperties.compute2(brep, multiple=false)

   Compute the VolumeMassProperties for a single Brep.

   :param rhino3dm.Brep brep: Brep to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Brep or None on failure.
   :rtype: VolumeMassProperties
.. js:function:: RhinoCompute.VolumeMassProperties.compute3(brep, volume, firstMoments, secondMoments, productMoments, multiple=false)

   Compute the VolumeMassProperties for a single Brep.

   :param rhino3dm.Brep brep: Brep to measure.
   :param bool volume: True to calculate volume.
   :param bool firstMoments: True to calculate volume first moments, volume, and volume centroid.
   :param bool secondMoments: True to calculate volume second moments.
   :param bool productMoments: True to calculate volume product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Brep or None on failure.
   :rtype: VolumeMassProperties
.. js:function:: RhinoCompute.VolumeMassProperties.compute4(surface, multiple=false)

   Compute the VolumeMassProperties for a single Surface.

   :param rhino3dm.Surface surface: Surface to measure.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Surface or None on failure.
   :rtype: VolumeMassProperties
.. js:function:: RhinoCompute.VolumeMassProperties.compute5(surface, volume, firstMoments, secondMoments, productMoments, multiple=false)

   Compute the VolumeMassProperties for a single Surface.

   :param rhino3dm.Surface surface: Surface to measure.
   :param bool volume: True to calculate volume.
   :param bool firstMoments: True to calculate volume first moments, volume, and volume centroid.
   :param bool secondMoments: True to calculate volume second moments.
   :param bool productMoments: True to calculate volume product moments.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the given Surface or None on failure.
   :rtype: VolumeMassProperties
.. js:function:: RhinoCompute.VolumeMassProperties.compute6(geometry, multiple=false)

   Computes the VolumeMassProperties for a collection of geometric objects.
   At present only Breps, Surfaces, and Meshes are supported.

   :param list[rhino3dm.GeometryBase] geometry: Objects to include in the area computation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The VolumeMassProperties for the entire collection or None on failure.
   :rtype: VolumeMassProperties
.. js:function:: RhinoCompute.VolumeMassProperties.compute7(geometry, volume, firstMoments, secondMoments, productMoments, multiple=false)

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
.. js:function:: RhinoCompute.VolumeMassProperties.sum(thisVolumeMassProperties, summand, multiple=false)

   Sum mass properties together to get an aggregate mass.

   :param VolumeMassProperties summand: mass properties to add.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful.
   :rtype: bool
