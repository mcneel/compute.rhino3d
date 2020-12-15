SubD
====

.. py:module:: compute_rhino3d.SubD

.. py:function:: ToBrep(thisSubD, options, multiple=False)

   Create a Brep based on this SubD geometry.

   :param SubDToBrepOptions options: The SubD to Brep conversion options. Use SubDToBrepOptions.Default \
      for sensible defaults. Currently, these return unpacked faces \
      and locally-G1 vertices in the output Brep.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new Brep if successful, or None on failure.
   :rtype: rhino3dm.Brep
.. py:function:: CreateFromMesh(mesh, multiple=False)

   Create a new SubD from a mesh.

   :param rhino3dm.Mesh mesh: The input mesh.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new SubD if successful, or None on failure.
   :rtype: SubD
.. py:function:: CreateFromMesh1(mesh, options, multiple=False)

   Create a new SubD from a mesh.

   :param rhino3dm.Mesh mesh: The input mesh.
   :param SubDCreationOptions options: The SubD creation options.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new SubD if successful, or None on failure.
   :rtype: SubD
.. py:function:: Offset(thisSubD, distance, solidify, multiple=False)

   Makes a new SubD with vertices offset at distance in the direction of the control net vertex normals.
   Optionally, based on the value of solidify, adds the input SubD and a ribbon of faces along any naked edges.

   :param float distance: The distance to offset.
   :param bool solidify: True if the output SubD should be turned into a closed SubD.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new SubD if successful, or None on failure.
   :rtype: SubD
.. py:function:: CreateFromLoft(curves, closed, addCorners, addCreases, divisions, multiple=False)

   Creates a SubD lofted through shape curves.

   :param list[rhino3dm.NurbsCurve] curves: An enumeration of SubD-friendly NURBS curves to loft through.
   :param bool closed: Creates a SubD that is closed in the lofting direction. Must have three or more shape curves.
   :param bool addCorners: With open curves, adds creased vertices to the SubD at both ends of the first and last curves.
   :param bool addCreases: With kinked curves, adds creased edges to the SubD along the kinks.
   :param int divisions: The segment number between adjacent input curves.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new SubD if successful, or None on failure.
   :rtype: SubD
.. py:function:: CreateFromSweep(rail1, shapes, closed, addCorners, roadlikeFrame, roadlikeNormal, multiple=False)

   Fits a SubD through a series of profile curves that define the SubD cross-sections and one curve that defines a SubD edge.

   :param rhino3dm.NurbsCurve rail1: A SubD-friendly NURBS curve to sweep along.
   :param list[rhino3dm.NurbsCurve] shapes: An enumeration of SubD-friendly NURBS curves to sweep through.
   :param bool closed: Creates a SubD that is closed in the rail curve direction.
   :param bool addCorners: With open curves, adds creased vertices to the SubD at both ends of the first and last curves.
   :param bool roadlikeFrame: Determines how sweep frame rotations are calculated. \
      If False (Freeform), frame are propogated based on a refrence direction taken from the rail curve curvature direction. \
      If True (Roadlike), frame rotations are calculated based on a vector supplied in "roadlikeNormal" and the world coordinate system.
   :param rhino3dm.Vector3d roadlikeNormal: If roadlikeFrame = true, provide 3D vector used to calculate the frame rotations for sweep shapes. \
      If roadlikeFrame = false, then pass .
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new SubD if successful, or None on failure.
   :rtype: SubD
.. py:function:: CreateFromSweep1(rail1, rail2, shapes, closed, addCorners, multiple=False)

   Fits a SubD through a series of profile curves that define the SubD cross-sections and two curves that defines SubD edges.

   :param rhino3dm.NurbsCurve rail1: The first SubD-friendly NURBS curve to sweep along.
   :param rhino3dm.NurbsCurve rail2: The second SubD-friendly NURBS curve to sweep along.
   :param list[rhino3dm.NurbsCurve] shapes: An enumeration of SubD-friendly NURBS curves to sweep through.
   :param bool closed: Creates a SubD that is closed in the rail curve direction.
   :param bool addCorners: With open curves, adds creased vertices to the SubD at both ends of the first and last curves.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new SubD if successful, or None on failure.
   :rtype: SubD
.. py:function:: InterpolateSurfacePoints(thisSubD, surfacePoints, multiple=False)

   Modifies the SubD so that the SubD vertex limit surface points are
   equal to surface_points[]

   :param rhino3dm.Point3d[] surfacePoints: point for limit surface to interpolate. surface_points[i] is the \
      location for the i-th vertex returned by SubVertexIterator vit(this)
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: bool
