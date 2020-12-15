RhinoCompute.Brep
=================

.. js:module:: RhinoCompute

.. js:function:: RhinoCompute.Brep.changeSeam(face, direction, parameter, tolerance, multiple=false)

   Change the seam of a closed trimmed surface.

   :param rhino3dm.BrepFace face: A Brep face with a closed underlying surface.
   :param int direction: The parameter direction (0 = U, 1 = V). The face's underlying surface must be closed in this direction.
   :param float parameter: The parameter at which to place the seam.
   :param float tolerance: Tolerance used to cut up surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new Brep that has the same geometry as the face with a relocated seam if successful, or None on failure.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.copyTrimCurves(trimSource, surfaceSource, tolerance, multiple=false)

   Copy all trims from a Brep face onto a surface.

   :param rhino3dm.BrepFace trimSource: Brep face which defines the trimming curves.
   :param rhino3dm.Surface surfaceSource: The surface to trim.
   :param float tolerance: Tolerance to use for rebuilding 3D trim curves.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A brep with the shape of surfaceSource and the trims of trimSource or None on failure.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createBaseballSphere(center, radius, tolerance, multiple=false)

   Creates a brep representation of the sphere with two similar trimmed NURBS surfaces, and no singularities.

   :param rhino3dm.Point3d center: The center of the sphere.
   :param float radius: The radius of the sphere.
   :param float tolerance: Used in computing 2d trimming curves. If >= 0.0, then the max of \
      ON_0.0001 * radius and RhinoMath.ZeroTolerance will be used.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new brep, or None on error.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createDevelopableLoft(crv0, crv1, reverse0, reverse1, density, multiple=false)

   Creates a single developable surface between two curves.

   :param rhino3dm.Curve crv0: The first rail curve.
   :param rhino3dm.Curve crv1: The second rail curve.
   :param bool reverse0: Reverse the first rail curve.
   :param bool reverse1: Reverse the second rail curve
   :param int density: The number of rulings across the surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The output Breps if successful, otherwise an empty array.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createDevelopableLoft1(rail0, rail1, fixedRulings, multiple=false)

   Creates a single developable surface between two curves.

   :param rhino3dm.NurbsCurve rail0: The first rail curve.
   :param rhino3dm.NurbsCurve rail1: The second rail curve.
   :param list[rhino3dm.Point2d] fixedRulings: Rulings define lines across the surface that define the straight sections on the developable surface, \
      where rulings[i].X = parameter on first rail curve, and rulings[i].Y = parameter on second rail curve. \
      Note, rulings will be automatically adjusted to minimum twist.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The output Breps if successful, otherwise an empty array.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarBreps(inputLoops, multiple=false)

   Constructs a set of planar breps as outlines by the loops.

   :param list[rhino3dm.Curve] inputLoops: Curve loops that delineate the planar boundaries.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Planar Breps.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarBreps1(inputLoops, tolerance, multiple=false)

   Constructs a set of planar breps as outlines by the loops.

   :param list[rhino3dm.Curve] inputLoops: Curve loops that delineate the planar boundaries.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Planar Breps.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarBreps2(inputLoop, multiple=false)

   Constructs a set of planar breps as outlines by the loops.

   :param rhino3dm.Curve inputLoop: A curve that should form the boundaries of the surfaces or polysurfaces.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Planar Breps.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarBreps3(inputLoop, tolerance, multiple=false)

   Constructs a set of planar breps as outlines by the loops.

   :param rhino3dm.Curve inputLoop: A curve that should form the boundaries of the surfaces or polysurfaces.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Planar Breps.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createTrimmedSurface(trimSource, surfaceSource, multiple=false)

   Constructs a Brep using the trimming information of a brep face and a surface.
   Surface must be roughly the same shape and in the same location as the trimming brep face.

   :param rhino3dm.BrepFace trimSource: BrepFace which contains trimmingSource brep.
   :param rhino3dm.Surface surfaceSource: Surface that trims of BrepFace will be applied to.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A brep with the shape of surfaceSource and the trims of trimSource or None on failure.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createTrimmedSurface1(trimSource, surfaceSource, tolerance, multiple=false)

   Constructs a Brep using the trimming information of a brep face and a surface.
   Surface must be roughly the same shape and in the same location as the trimming brep face.

   :param rhino3dm.BrepFace trimSource: BrepFace which contains trimmingSource brep.
   :param rhino3dm.Surface surfaceSource: Surface that trims of BrepFace will be applied to.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A brep with the shape of surfaceSource and the trims of trimSource or None on failure.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createFromCornerPoints(corner1, corner2, corner3, tolerance, multiple=false)

   Makes a Brep with one face from three corner points.

   :param rhino3dm.Point3d corner1: A first corner.
   :param rhino3dm.Point3d corner2: A second corner.
   :param rhino3dm.Point3d corner3: A third corner.
   :param float tolerance: Minimum edge length allowed before collapsing the side into a singularity.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A boundary representation, or None on error.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createFromCornerPoints1(corner1, corner2, corner3, corner4, tolerance, multiple=false)

   Makes a Brep with one face from four corner points.

   :param rhino3dm.Point3d corner1: A first corner.
   :param rhino3dm.Point3d corner2: A second corner.
   :param rhino3dm.Point3d corner3: A third corner.
   :param rhino3dm.Point3d corner4: A fourth corner.
   :param float tolerance: Minimum edge length allowed before collapsing the side into a singularity.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A boundary representation, or None on error.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createEdgeSurface(curves, multiple=false)

   Constructs a coons patch from 2, 3, or 4 curves.

   :param list[rhino3dm.Curve] curves: A list, an array or any enumerable set of curves.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: resulting brep or None on failure.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createPlanarBreps4(inputLoops, multiple=false)

   Constructs a set of planar Breps as outlines by the loops.

   :param Rhino.Collections.CurveList inputLoops: Curve loops that delineate the planar boundaries.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Planar Breps or None on error.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarBreps5(inputLoops, tolerance, multiple=false)

   Constructs a set of planar Breps as outlines by the loops.

   :param Rhino.Collections.CurveList inputLoops: Curve loops that delineate the planar boundaries.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Planar Breps.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromOffsetFace(face, offsetDistance, offsetTolerance, bothSides, createSolid, multiple=false)

   Offsets a face including trim information to create a new brep.

   :param rhino3dm.BrepFace face: the face to offset.
   :param float offsetDistance: An offset distance.
   :param float offsetTolerance: Use 0.0 to make a loose offset. Otherwise, the document's absolute tolerance is usually sufficient.
   :param bool bothSides: When true, offset to both sides of the input face.
   :param bool createSolid: When true, make a solid object.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new brep if successful. The brep can be disjoint if bothSides is True and createSolid is false, \
      or if createSolid is True and connecting the offsets with side surfaces fails. \
      None if unsuccessful.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createSolid(breps, tolerance, multiple=false)

   Constructs closed polysurfaces from surfaces and polysurfaces that bound a region in space.

   :param list[rhino3dm.Brep] breps: The intersecting surfaces and polysurfaces to automatically trim and join into closed polysurfaces.
   :param float tolerance: The trim and join tolerance. If set to RhinoMath.UnsetValue, Rhino's global absolute tolerance is used.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The resulting polysurfaces on success or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.mergeSurfaces(surface0, surface1, tolerance, angleToleranceRadians, multiple=false)

   Merges two surfaces into one surface at untrimmed edges.

   :param rhino3dm.Surface surface0: The first surface to merge.
   :param rhino3dm.Surface surface1: The second surface to merge.
   :param float tolerance: Surface edges must be within this tolerance for the two surfaces to merge.
   :param float angleToleranceRadians: Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The merged surfaces as a Brep if successful, None if not successful.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.mergeSurfaces1(brep0, brep1, tolerance, angleToleranceRadians, multiple=false)

   Merges two surfaces into one surface at untrimmed edges. Both surfaces must be untrimmed and share an edge.

   :param rhino3dm.Brep brep0: The first single-face Brep to merge.
   :param rhino3dm.Brep brep1: The second single-face Brep to merge.
   :param float tolerance: Surface edges must be within this tolerance for the two surfaces to merge.
   :param float angleToleranceRadians: Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The merged Brep if successful, None if not successful.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.mergeSurfaces2(brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth, multiple=false)

   Merges two surfaces into one surface at untrimmed edges. Both surfaces must be untrimmed and share an edge.

   :param rhino3dm.Brep brep0: The first single-face Brep to merge.
   :param rhino3dm.Brep brep1: The second single-face Brep to merge.
   :param float tolerance: Surface edges must be within this tolerance for the two surfaces to merge.
   :param float angleToleranceRadians: Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.
   :param rhino3dm.Point2d point0: 2D pick point on the first single-face Brep. The value can be unset.
   :param rhino3dm.Point2d point1: 2D pick point on the second single-face Brep. The value can be unset.
   :param float roundness: Defines the roundness of the merge. Acceptable values are between 0.0 (sharp) and 1.0 (smooth).
   :param bool smooth: The surface will be smooth. This makes the surface behave better for control point editing, but may alter the shape of both surfaces.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The merged Brep if successful, None if not successful.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createPatch(geometry, startingSurface, tolerance, multiple=false)

   Constructs a brep patch.
   This is the simple version of fit that uses a specified starting surface.

   :param list[rhino3dm.GeometryBase] geometry: Combination of Curves, BrepTrims, Points, PointClouds or Meshes. \
      Curves and trims are sampled to get points. Trims are sampled for \
      points and normals.
   :param rhino3dm.Surface startingSurface: A starting surface (can be null).
   :param float tolerance: Tolerance used by input analysis functions for loop finding, trimming, etc.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Brep fit through input on success, or None on error.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createPatch1(geometry, uSpans, vSpans, tolerance, multiple=false)

   Constructs a brep patch.
   This is the simple version of fit that uses a plane with u x v spans.
   It makes a plane by fitting to the points from the input geometry to use as the starting surface.
   The surface has the specified u and v span count.

   :param list[rhino3dm.GeometryBase] geometry: A combination of curves, brep trims, \
      points, point clouds or meshes. \
      Curves and trims are sampled to get points. Trims are sampled for \
      points and normals.
   :param int uSpans: The number of spans in the U direction.
   :param int vSpans: The number of spans in the V direction.
   :param float tolerance: Tolerance used by input analysis functions for loop finding, trimming, etc.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A brep fit through input on success, or None on error.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createPatch2(geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance, multiple=false)

   Constructs a brep patch using all controls

   :param list[rhino3dm.GeometryBase] geometry: A combination of curves, brep trims, \
      points, point clouds or meshes. \
      Curves and trims are sampled to get points. Trims are sampled for \
      points and normals.
   :param rhino3dm.Surface startingSurface: A starting surface (can be null).
   :param int uSpans: Number of surface spans used when a plane is fit through points to start in the U direction.
   :param int vSpans: Number of surface spans used when a plane is fit through points to start in the U direction.
   :param bool trim: If true, try to find an outer loop from among the input curves and trim the result to that loop
   :param bool tangency: If true, try to find brep trims in the outer loop of curves and try to \
      fit to the normal direction of the trim's surface at those locations.
   :param float pointSpacing: Basic distance between points sampled from input curves.
   :param float flexibility: Determines the behavior of the surface in areas where its not otherwise \
      controlled by the input.  Lower numbers make the surface behave more \
      like a stiff material; higher, less like a stiff material.  That is, \
      each span is made to more closely match the spans adjacent to it if there \
      is no input geometry mapping to that area of the surface when the \
      flexibility value is low.  The scale is logarithmic. Numbers around 0.001 \
      or 0.1 make the patch pretty stiff and numbers around 10 or 100 make the \
      surface flexible.
   :param float surfacePull: Tends to keep the result surface where it was before the fit in areas where \
      there is on influence from the input geometry
   :param bool[] fixEdges: Array of four elements. Flags to keep the edges of a starting (untrimmed) \
      surface in place while fitting the interior of the surface.  Order of \
      flags is left, bottom, right, top
   :param float tolerance: Tolerance used by input analysis functions for loop finding, trimming, etc.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A brep fit through input on success, or None on error.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createPipe(rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians, multiple=false)

   Creates a single walled pipe.

   :param rhino3dm.Curve rail: The rail, or path, curve.
   :param float radius: The radius of the pipe.
   :param bool localBlending: The shape blending. \
      If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied. \
      If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
   :param PipeCapMode cap: The end cap mode.
   :param bool fitRail: If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created; \
      otherwise the result is a Brep with joined surfaces created from the polycurve segments.
   :param float absoluteTolerance: The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
   :param float angleToleranceRadians: The angle tolerance. When in doubt, use the document's angle tolerance in radians.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps success.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPipe1(rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians, multiple=false)

   Creates a single walled pipe.

   :param rhino3dm.Curve rail: The rail, or path, curve.
   :param list[float] railRadiiParameters: One or more normalized curve parameters where changes in radius occur. \
      Important: curve parameters must be normalized - ranging between 0.0 and 1.0. \
      Use Interval.NormalizedParameterAt to calculate these.
   :param list[float] radii: One or more radii - one at each normalized curve parameter in railRadiiParameters.
   :param bool localBlending: The shape blending. \
      If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied. \
      If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
   :param PipeCapMode cap: The end cap mode.
   :param bool fitRail: If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created; \
      otherwise the result is a Brep with joined surfaces created from the polycurve segments.
   :param float absoluteTolerance: The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
   :param float angleToleranceRadians: The angle tolerance. When in doubt, use the document's angle tolerance in radians.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps success.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createThickPipe(rail, radius0, radius1, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians, multiple=false)

   Creates a double-walled pipe.

   :param rhino3dm.Curve rail: The rail, or path, curve.
   :param float radius0: The first radius of the pipe.
   :param float radius1: The second radius of the pipe.
   :param bool localBlending: The shape blending. \
      If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied. \
      If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
   :param PipeCapMode cap: The end cap mode.
   :param bool fitRail: If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created; \
      otherwise the result is a Brep with joined surfaces created from the polycurve segments.
   :param float absoluteTolerance: The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
   :param float angleToleranceRadians: The angle tolerance. When in doubt, use the document's angle tolerance in radians.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps success.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createThickPipe1(rail, railRadiiParameters, radii0, radii1, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians, multiple=false)

   Creates a double-walled pipe.

   :param rhino3dm.Curve rail: The rail, or path, curve.
   :param list[float] railRadiiParameters: One or more normalized curve parameters where changes in radius occur. \
      Important: curve parameters must be normalized - ranging between 0.0 and 1.0. \
      Use Interval.NormalizedParameterAt to calculate these.
   :param list[float] radii0: One or more radii for the first wall - one at each normalized curve parameter in railRadiiParameters.
   :param list[float] radii1: One or more radii for the second wall - one at each normalized curve parameter in railRadiiParameters.
   :param bool localBlending: The shape blending. \
      If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied. \
      If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied.
   :param PipeCapMode cap: The end cap mode.
   :param bool fitRail: If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created; \
      otherwise the result is a Brep with joined surfaces created from the polycurve segments.
   :param float absoluteTolerance: The sweeping and fitting tolerance. When in doubt, use the document's absolute tolerance.
   :param float angleToleranceRadians: The angle tolerance. When in doubt, use the document's angle tolerance in radians.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps success.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweep(rail, shape, closed, tolerance, multiple=false)

   Sweep1 function that fits a surface through a profile curve that define the surface cross-sections
   and one curve that defines a surface edge.

   :param rhino3dm.Curve rail: Rail to sweep shapes along
   :param rhino3dm.Curve shape: Shape curve
   :param bool closed: Only matters if shape is closed
   :param float tolerance: Tolerance for fitting surface and rails
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweep1(rail, shapes, closed, tolerance, multiple=false)

   Sweep1 function that fits a surface through profile curves that define the surface cross-sections
   and one curve that defines a surface edge.

   :param rhino3dm.Curve rail: Rail to sweep shapes along
   :param list[rhino3dm.Curve] shapes: Shape curves
   :param bool closed: Only matters if shapes are closed
   :param float tolerance: Tolerance for fitting surface and rails
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweep2(rail, shapes, startPoint, endPoint, frameType, roadlikeNormal, closed, blendType, miterType, tolerance, rebuildType, rebuildPointCount, refitTolerance, multiple=false)

   Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
   and one curve that defines a surface edge.

   :param rhino3dm.Curve rail: Rail to sweep shapes along.
   :param list[rhino3dm.Curve] shapes: Shape curves.
   :param rhino3dm.Point3d startPoint: Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.
   :param rhino3dm.Point3d endPoint: Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.
   :param SweepFrame frameType: The frame type.
   :param rhino3dm.Vector3d roadlikeNormal: The roadlike normal directoion. Use Vector3d.Unset if the frame type is not set to roadlike.
   :param bool closed: Only matters if shapes are closed.
   :param SweepBlend blendType: The shape blending type.
   :param SweepMiter miterType: The mitering type.
   :param SweepRebuild rebuildType: The rebuild style.
   :param int rebuildPointCount: If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.
   :param float refitTolerance: If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweepSegmented(rail, shape, closed, tolerance, multiple=false)

   Sweep1 function that fits a surface through a profile curve that define the surface cross-sections
   and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
   and sweeps each piece separately, then put the results together into a Brep.

   :param rhino3dm.Curve rail: Rail to sweep shapes along
   :param rhino3dm.Curve shape: Shape curve
   :param bool closed: Only matters if shape is closed
   :param float tolerance: Tolerance for fitting surface and rails
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweepSegmented1(rail, shapes, closed, tolerance, multiple=false)

   Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
   and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
   and sweeps each piece separately, then put the results together into a Brep.

   :param rhino3dm.Curve rail: Rail to sweep shapes along.
   :param list[rhino3dm.Curve] shapes: Shape curves.
   :param bool closed: Only matters if shapes are closed.
   :param float tolerance: Tolerance for fitting surface and rails.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweepSegmented2(rail, shapes, startPoint, endPoint, frameType, roadlikeNormal, closed, blendType, miterType, tolerance, rebuildType, rebuildPointCount, refitTolerance, multiple=false)

   Sweep1 function that fits a surface through a series of profile curves that define the surface cross-sections
   and one curve that defines a surface edge. The Segmented version breaks the rail at curvature kinks
   and sweeps each piece separately, then put the results together into a Brep.

   :param rhino3dm.Curve rail: Rail to sweep shapes along.
   :param list[rhino3dm.Curve] shapes: Shape curves.
   :param rhino3dm.Point3d startPoint: Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.
   :param rhino3dm.Point3d endPoint: Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.
   :param SweepFrame frameType: The frame type.
   :param rhino3dm.Vector3d roadlikeNormal: The roadlike normal directoion. Use Vector3d.Unset if the frame type is not set to roadlike.
   :param bool closed: Only matters if shapes are closed.
   :param SweepBlend blendType: The shape blending type.
   :param SweepMiter miterType: The mitering type.
   :param SweepRebuild rebuildType: The rebuild style.
   :param int rebuildPointCount: If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.
   :param float refitTolerance: If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweep3(rail1, rail2, shape, closed, tolerance, multiple=false)

   General 2 rail sweep. If you are not producing the sweep results that you are after, then
   use the SweepTwoRail class with options to generate the swept geometry.

   :param rhino3dm.Curve rail1: Rail to sweep shapes along
   :param rhino3dm.Curve rail2: Rail to sweep shapes along
   :param rhino3dm.Curve shape: Shape curve
   :param bool closed: Only matters if shape is closed
   :param float tolerance: Tolerance for fitting surface and rails
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweep4(rail1, rail2, shapes, closed, tolerance, multiple=false)

   General 2 rail sweep. If you are not producing the sweep results that you are after, then
   use the SweepTwoRail class with options to generate the swept geometry.

   :param rhino3dm.Curve rail1: Rail to sweep shapes along
   :param rhino3dm.Curve rail2: Rail to sweep shapes along
   :param list[rhino3dm.Curve] shapes: Shape curves
   :param bool closed: Only matters if shapes are closed
   :param float tolerance: Tolerance for fitting surface and rails
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweep5(rail1, rail2, shapes, start, end, closed, tolerance, rebuild, rebuildPointCount, refitTolerance, preserveHeight, multiple=false)

   Sweep2 function that fits a surface through profile curves that define the surface cross-sections
   and two curves that defines the surface edges.

   :param rhino3dm.Curve rail1: Rail to sweep shapes along
   :param rhino3dm.Curve rail2: Rail to sweep shapes along
   :param list[rhino3dm.Curve] shapes: Shape curves
   :param rhino3dm.Point3d start: Optional starting point of sweep. Use Point3d.Unset if you do not want to include a start point.
   :param rhino3dm.Point3d end: Optional ending point of sweep. Use Point3d.Unset if you do not want to include an end point.
   :param bool closed: Only matters if shapes are closed.
   :param float tolerance: Tolerance for fitting surface and rails.
   :param SweepRebuild rebuild: The rebuild style.
   :param int rebuildPointCount: If rebuild == SweepRebuild.Rebuild, the number of points. Otherwise specify 0.
   :param float refitTolerance: If rebuild == SweepRebuild.Refit, the refit tolerance. Otherwise, specify 0.0
   :param bool preserveHeight: Removes the association between the height scaling from the width scaling
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromSweepInParts(rail1, rail2, shapes, rail_params, closed, tolerance, multiple=false)

   Makes a 2 rail sweep. Like CreateFromSweep but the result is split where parameterization along a rail changes abruptly.

   :param rhino3dm.Curve rail1: Rail to sweep shapes along
   :param rhino3dm.Curve rail2: Rail to sweep shapes along
   :param list[rhino3dm.Curve] shapes: Shape curves
   :param list[rhino3dm.Point2d] rail_params: Shape parameters
   :param bool closed: Only matters if shapes are closed
   :param float tolerance: Tolerance for fitting surface and rails
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Brep sweep results
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromTaperedExtrude(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians, multiple=false)

   Extrude a curve to a taper making a brep (potentially more than 1)

   :param rhino3dm.Curve curveToExtrude: the curve to extrude
   :param float distance: the distance to extrude
   :param rhino3dm.Vector3d direction: the direction of the extrusion
   :param rhino3dm.Point3d basePoint: the base point of the extrusion
   :param float draftAngleRadians: angle of the extrusion
   :param float tolerance: tolerance to use for the extrusion
   :param float angleToleranceRadians: angle tolerance to use for the extrusion
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: array of breps on success
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromTaperedExtrude1(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, multiple=false)

   Extrude a curve to a taper making a brep (potentially more than 1)

   :param rhino3dm.Curve curveToExtrude: the curve to extrude
   :param float distance: the distance to extrude
   :param rhino3dm.Vector3d direction: the direction of the extrusion
   :param rhino3dm.Point3d basePoint: the base point of the extrusion
   :param float draftAngleRadians: angle of the extrusion
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: array of breps on success
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromTaperedExtrudeWithRef(curve, direction, distance, draftAngle, plane, tolerance, multiple=false)

   Creates one or more Breps by extruding a curve a distance along an axis with draft angle.

   :param rhino3dm.Curve curve: The curve to extrude.
   :param rhino3dm.Vector3d direction: The extrusion direction.
   :param float distance: The extrusion distance.
   :param float draftAngle: The extrusion draft angle in radians.
   :param rhino3dm.Plane plane: The end of the extrusion will be parallel to this plane, and "distance" from the plane's origin. \
      The plane's origin is generally be a point on the curve. For planar curves, a natural choice for the \
      plane's normal direction will be the normal direction of the curve's plane. In any case, \
      plane.Normal = direction may make sense.
   :param float tolerance: The intersecting and trimming tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Breps if successful.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBlendSurface(face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1, multiple=false)

   Makes a surface blend between two surface edges.

   :param rhino3dm.BrepFace face0: First face to blend from.
   :param rhino3dm.BrepEdge edge0: First edge to blend from.
   :param rhino3dm.Interval domain0: The domain of edge0 to use.
   :param bool rev0: If false, edge0 will be used in its natural direction. If true, edge0 will be used in the reversed direction.
   :param BlendContinuity continuity0: Continuity for the blend at the start.
   :param rhino3dm.BrepFace face1: Second face to blend from.
   :param rhino3dm.BrepEdge edge1: Second edge to blend from.
   :param rhino3dm.Interval domain1: The domain of edge1 to use.
   :param bool rev1: If false, edge1 will be used in its natural direction. If true, edge1 will be used in the reversed direction.
   :param BlendContinuity continuity1: Continuity for the blend at the end.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps if successful.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBlendShape(face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1, multiple=false)

   Makes a curve blend between points on two surface edges. The blend will be tangent to the surfaces and perpendicular to the edges.

   :param rhino3dm.BrepFace face0: First face to blend from.
   :param rhino3dm.BrepEdge edge0: First edge to blend from.
   :param float t0: Location on first edge for first end of blend curve.
   :param bool rev0: If false, edge0 will be used in its natural direction. If true, edge0 will be used in the reversed direction.
   :param BlendContinuity continuity0: Continuity for the blend at the start.
   :param rhino3dm.BrepFace face1: Second face to blend from.
   :param rhino3dm.BrepEdge edge1: Second edge to blend from.
   :param float t1: Location on second edge for second end of blend curve.
   :param bool rev1: If false, edge1 will be used in its natural direction. If true, edge1 will be used in the reversed direction.
   :param BlendContinuity continuity1: >Continuity for the blend at the end.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The blend curve on success. None on failure
   :rtype: rhino3dm.Curve
.. js:function:: RhinoCompute.Brep.createFilletSurface(face0, uv0, face1, uv1, radius, extend, tolerance, multiple=false)

   Creates a constant-radius round surface between two surfaces.

   :param rhino3dm.BrepFace face0: First face to fillet from.
   :param rhino3dm.Point2d uv0: A parameter face0 at the side you want to keep after filleting.
   :param rhino3dm.BrepFace face1: Second face to fillet from.
   :param rhino3dm.Point2d uv1: A parameter face1 at the side you want to keep after filleting.
   :param float radius: The fillet radius.
   :param bool extend: If true, then when one input surface is longer than the other, the fillet surface is extended to the input surface edges.
   :param float tolerance: The tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps if successful.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFilletSurface1(face0, uv0, face1, uv1, radius, trim, extend, tolerance, multiple=false)

   Creates a constant-radius round surface between two surfaces.

   :param rhino3dm.BrepFace face0: First face to fillet from.
   :param rhino3dm.Point2d uv0: A parameter face0 at the side you want to keep after filleting.
   :param rhino3dm.BrepFace face1: Second face to fillet from.
   :param rhino3dm.Point2d uv1: A parameter face1 at the side you want to keep after filleting.
   :param float radius: The fillet radius.
   :param bool trim: If true, the input faces will be trimmed, if false, the input faces will be split.
   :param bool extend: If true, then when one input surface is longer than the other, the fillet surface is extended to the input surface edges.
   :param float tolerance: The tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps if successful.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createChamferSurface(face0, uv0, radius0, face1, uv1, radius1, extend, tolerance, multiple=false)

   Creates a ruled surface as a bevel between two input surface edges.

   :param rhino3dm.BrepFace face0: First face to chamfer from.
   :param rhino3dm.Point2d uv0: A parameter face0 at the side you want to keep after chamfering.
   :param float radius0: The distance from the intersection of face0 to the edge of the chamfer.
   :param rhino3dm.BrepFace face1: Second face to chamfer from.
   :param rhino3dm.Point2d uv1: A parameter face1 at the side you want to keep after chamfering.
   :param float radius1: The distance from the intersection of face1 to the edge of the chamfer.
   :param bool extend: If true, then when one input surface is longer than the other, the chamfer surface is extended to the input surface edges.
   :param float tolerance: The tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps if successful.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createChamferSurface1(face0, uv0, radius0, face1, uv1, radius1, trim, extend, tolerance, multiple=false)

   Creates a ruled surface as a bevel between two input surface edges.

   :param rhino3dm.BrepFace face0: First face to chamfer from.
   :param rhino3dm.Point2d uv0: A parameter face0 at the side you want to keep after chamfering.
   :param float radius0: The distance from the intersection of face0 to the edge of the chamfer.
   :param rhino3dm.BrepFace face1: Second face to chamfer from.
   :param rhino3dm.Point2d uv1: A parameter face1 at the side you want to keep after chamfering.
   :param float radius1: The distance from the intersection of face1 to the edge of the chamfer.
   :param bool trim: If true, the input faces will be trimmed, if false, the input faces will be split.
   :param bool extend: If true, then when one input surface is longer than the other, the chamfer surface is extended to the input surface edges.
   :param float tolerance: The tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps if successful.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFilletEdges(brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance, multiple=false)

   Fillets, chamfers, or blends the edges of a brep.

   :param rhino3dm.Brep brep: The brep to fillet, chamfer, or blend edges.
   :param list[int] edgeIndices: An array of one or more edge indices where the fillet, chamfer, or blend will occur.
   :param list[float] startRadii: An array of starting fillet, chamfer, or blend radaii, one for each edge index.
   :param list[float] endRadii: An array of ending fillet, chamfer, or blend radaii, one for each edge index.
   :param BlendType blendType: The blend type.
   :param RailType railType: The rail type.
   :param float tolerance: The tolerance to be used to perform calculations.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps if successful.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createOffsetBrep(brep, distance, solid, extend, tolerance, multiple=false)

   Offsets a Brep.

   :param rhino3dm.Brep brep: The Brep to offset.
   :param float distance: The distance to offset. This is a signed distance value with respect to \
      face normals and flipped faces.
   :param bool solid: If true, then the function makes a closed solid from the input and offset \
      surfaces by lofting a ruled surface between all of the matching edges.
   :param bool extend: If true, then the function maintains the sharp corners when the original \
      surfaces have sharps corner. If False, then the function creates fillets \
      at sharp corners in the original surfaces.
   :param float tolerance: The offset tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Array of Breps if successful. If the function succeeds in offsetting, a \
      single Brep will be returned. Otherwise, the array will contain the \
      offset surfaces, outBlends will contain the set of blends used to fill \
      in gaps (if extend is false), and outWalls will contain the set of wall \
      surfaces that was supposed to join the offset to the original (if solid \
      is true).
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.removeFins(thisBrep, multiple=false)

   Recursively removes any Brep face with a naked edge. This function is only useful for non-manifold Breps.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False if everything is removed or if the result has any Brep edges with more than two Brep trims.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.createFromJoinedEdges(brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance, multiple=false)

   Joins two naked edges, or edges that are coincident or close together, from two Breps.

   :param rhino3dm.Brep brep0: The first Brep.
   :param int edgeIndex0: The edge index on the first Brep.
   :param rhino3dm.Brep brep1: The second Brep.
   :param int edgeIndex1: The edge index on the second Brep.
   :param float joinTolerance: The join tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The resulting Brep if successful, None on failure.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createFromLoft(curves, start, end, loftType, closed, multiple=false)

   Constructs one or more Breps by lofting through a set of curves.

   :param list[rhino3dm.Curve] curves: The curves to loft through. This function will not perform any curve sorting. You must pass in \
      curves in the order you want them lofted. This function will not adjust the directions of open \
      curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves. \
      This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to \
      adjust the seam of closed curves.
   :param rhino3dm.Point3d start: Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
   :param rhino3dm.Point3d end: Optional ending point of loft. Use Point3d.Unset if you do not want to include an end point.
   :param LoftType loftType: type of loft to perform.
   :param bool closed: True if the last curve in this loft should be connected back to the first one.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Constructs a closed surface, continuing the surface past the last curve around to the \
      first curve. Available when you have selected three shape curves.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromLoftRebuild(curves, start, end, loftType, closed, rebuildPointCount, multiple=false)

   Constructs one or more Breps by lofting through a set of curves. Input for the loft is simplified by
   rebuilding to a specified number of control points.

   :param list[rhino3dm.Curve] curves: The curves to loft through. This function will not perform any curve sorting. You must pass in \
      curves in the order you want them lofted. This function will not adjust the directions of open \
      curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves. \
      This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to \
      adjust the seam of closed curves.
   :param rhino3dm.Point3d start: Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
   :param rhino3dm.Point3d end: Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point.
   :param LoftType loftType: type of loft to perform.
   :param bool closed: True if the last curve in this loft should be connected back to the first one.
   :param int rebuildPointCount: A number of points to use while rebuilding the curves. 0 leaves turns this parameter off.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Constructs a closed surface, continuing the surface past the last curve around to the \
      first curve. Available when you have selected three shape curves.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromLoftRefit(curves, start, end, loftType, closed, refitTolerance, multiple=false)

   Constructs one or more Breps by lofting through a set of curves. Input for the loft is simplified by
   refitting to a specified tolerance.

   :param list[rhino3dm.Curve] curves: The curves to loft through. This function will not perform any curve sorting. You must pass in \
      curves in the order you want them lofted. This function will not adjust the directions of open \
      curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves. \
      This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to \
      adjust the seam of closed curves.
   :param rhino3dm.Point3d start: Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
   :param rhino3dm.Point3d end: Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point.
   :param LoftType loftType: type of loft to perform.
   :param bool closed: True if the last curve in this loft should be connected back to the first one.
   :param float refitTolerance: A distance to use in refitting, or 0 if you want to turn this parameter off.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Constructs a closed surface, continuing the surface past the last curve around to the \
      first curve. Available when you have selected three shape curves.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createFromLoft1(curves, start, end, StartTangent, EndTangent, StartTrim, EndTrim, loftType, closed, multiple=false)

   Constructs one or more Breps by lofting through a set of curves, optionally matching start and
   end tangents of surfaces when first and/or last loft curves are surface edges

   :param list[rhino3dm.Curve] curves: The curves to loft through. This function will not perform any curve sorting. You must pass in \
      curves in the order you want them lofted. This function will not adjust the directions of open \
      curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves. \
      This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to \
      adjust the seam of closed curves.
   :param rhino3dm.Point3d start: Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point. \
      "start" and "StartTangent" cannot both be true.
   :param rhino3dm.Point3d end: Optional ending point of loft. Use Point3d.Unset if you do not want to include an end point. \
      "end and "EndTangent" cannot both be true.
   :param bool StartTangent: If StartTangent is True and the first loft curve is a surface edge, the loft will match the tangent \
      of the surface behind that edge.
   :param bool EndTangent: If EndTangent is True and the first loft curve is a surface edge, the loft will match the tangent \
      of the surface behind that edge.
   :param BrepTrim StartTrim: BrepTrim from the surface edge where start tangent is to be matched
   :param BrepTrim EndTrim: BrepTrim from the surface edge where end tangent is to be matched
   :param LoftType loftType: type of loft to perform.
   :param bool closed: True if the last curve in this loft should be connected back to the first one.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Constructs a closed surface, continuing the surface past the last curve around to the \
      first curve. Available when you have selected three shape curves.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarUnion(breps, plane, tolerance, multiple=false)

   CreatePlanarUnion

   :param list[rhino3dm.Brep] breps: The planar regions on which to preform the union operation.
   :param rhino3dm.Plane plane: The plane in which all the input breps lie
   :param float tolerance: Tolerance to use for union operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarUnion1(b0, b1, plane, tolerance, multiple=false)

   CreatePlanarUnion

   :param rhino3dm.Brep b0: The first brep to union.
   :param rhino3dm.Brep b1: The first brep to union.
   :param rhino3dm.Plane plane: The plane in which all the input breps lie
   :param float tolerance: Tolerance to use for union operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarDifference(b0, b1, plane, tolerance, multiple=false)

   CreatePlanarDifference

   :param rhino3dm.Brep b0: The first brep to intersect.
   :param rhino3dm.Brep b1: The first brep to intersect.
   :param rhino3dm.Plane plane: The plane in which all the input breps lie
   :param float tolerance: Tolerance to use for Difference operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createPlanarIntersection(b0, b1, plane, tolerance, multiple=false)

   CreatePlanarIntersection

   :param rhino3dm.Brep b0: The first brep to intersect.
   :param rhino3dm.Brep b1: The first brep to intersect.
   :param rhino3dm.Plane plane: The plane in which all the input breps lie
   :param float tolerance: Tolerance to use for intersection operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanUnion(breps, tolerance, multiple=false)

   Compute the Boolean Union of a set of Breps.

   :param list[rhino3dm.Brep] breps: Breps to union.
   :param float tolerance: Tolerance to use for union operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanUnion1(breps, tolerance, manifoldOnly, multiple=false)

   Compute the Boolean Union of a set of Breps.

   :param list[rhino3dm.Brep] breps: Breps to union.
   :param float tolerance: Tolerance to use for union operation.
   :param bool manifoldOnly: If true, non-manifold input breps are ignored.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanIntersection(firstSet, secondSet, tolerance, multiple=false)

   Compute the Solid Intersection of two sets of Breps.

   :param list[rhino3dm.Brep] firstSet: First set of Breps.
   :param list[rhino3dm.Brep] secondSet: Second set of Breps.
   :param float tolerance: Tolerance to use for intersection operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanIntersection1(firstSet, secondSet, tolerance, manifoldOnly, multiple=false)

   Compute the Solid Intersection of two sets of Breps.

   :param list[rhino3dm.Brep] firstSet: First set of Breps.
   :param list[rhino3dm.Brep] secondSet: Second set of Breps.
   :param float tolerance: Tolerance to use for intersection operation.
   :param bool manifoldOnly: If true, non-manifold input breps are ignored.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanIntersection2(firstBrep, secondBrep, tolerance, multiple=false)

   Compute the Solid Intersection of two Breps.

   :param rhino3dm.Brep firstBrep: First Brep for boolean intersection.
   :param rhino3dm.Brep secondBrep: Second Brep for boolean intersection.
   :param float tolerance: Tolerance to use for intersection operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanIntersection3(firstBrep, secondBrep, tolerance, manifoldOnly, multiple=false)

   Compute the Solid Intersection of two Breps.

   :param rhino3dm.Brep firstBrep: First Brep for boolean intersection.
   :param rhino3dm.Brep secondBrep: Second Brep for boolean intersection.
   :param float tolerance: Tolerance to use for intersection operation.
   :param bool manifoldOnly: If true, non-manifold input breps are ignored.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanDifference(firstSet, secondSet, tolerance, multiple=false)

   Compute the Solid Difference of two sets of Breps.

   :param list[rhino3dm.Brep] firstSet: First set of Breps (the set to subtract from).
   :param list[rhino3dm.Brep] secondSet: Second set of Breps (the set to subtract).
   :param float tolerance: Tolerance to use for difference operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanDifference1(firstSet, secondSet, tolerance, manifoldOnly, multiple=false)

   Compute the Solid Difference of two sets of Breps.

   :param list[rhino3dm.Brep] firstSet: First set of Breps (the set to subtract from).
   :param list[rhino3dm.Brep] secondSet: Second set of Breps (the set to subtract).
   :param float tolerance: Tolerance to use for difference operation.
   :param bool manifoldOnly: If true, non-manifold input breps are ignored.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanDifference2(firstBrep, secondBrep, tolerance, multiple=false)

   Compute the Solid Difference of two Breps.

   :param rhino3dm.Brep firstBrep: First Brep for boolean difference.
   :param rhino3dm.Brep secondBrep: Second Brep for boolean difference.
   :param float tolerance: Tolerance to use for difference operation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanDifference3(firstBrep, secondBrep, tolerance, manifoldOnly, multiple=false)

   Compute the Solid Difference of two Breps.

   :param rhino3dm.Brep firstBrep: First Brep for boolean difference.
   :param rhino3dm.Brep secondBrep: Second Brep for boolean difference.
   :param float tolerance: Tolerance to use for difference operation.
   :param bool manifoldOnly: If true, non-manifold input breps are ignored.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanSplit(firstBrep, secondBrep, tolerance, multiple=false)

   Splits shared areas of Breps and creates separate Breps from the shared and unshared parts.

   :param rhino3dm.Brep firstBrep: The Brep to split.
   :param rhino3dm.Brep secondBrep: The cutting Brep.
   :param float tolerance: Tolerance to use for splitting operation. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep if successful, an empty array on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createBooleanSplit1(firstSet, secondSet, tolerance, multiple=false)

   Splits shared areas of Breps and creates separate Breps from the shared and unshared parts.

   :param list[rhino3dm.Brep] firstSet: The Breps to split.
   :param list[rhino3dm.Brep] secondSet: The cutting Breps.
   :param float tolerance: Tolerance to use for splitting operation. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep if successful, an empty array on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.createShell(brep, facesToRemove, distance, tolerance, multiple=false)

   Creates a hollowed out shell from a solid Brep. Function only operates on simple, solid, manifold Breps.

   :param rhino3dm.Brep brep: The solid Brep to shell.
   :param list[int] facesToRemove: The indices of the Brep faces to remove. These surfaces are removed and the remainder is offset inward, using the outer parts of the removed surfaces to join the inner and outer parts.
   :param float distance: The distance, or thickness, for the shell. This is a signed distance value with respect to face normals and flipped faces.
   :param float tolerance: The offset tolerance. When in doubt, use the document's absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Brep results or None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.joinBreps(brepsToJoin, tolerance, multiple=false)

   Joins the breps in the input array at any overlapping edges to form
   as few as possible resulting breps. There may be more than one brep in the result array.

   :param list[rhino3dm.Brep] brepsToJoin: A list, an array or any enumerable set of breps to join.
   :param float tolerance: 3d distance tolerance for detecting overlapping edges.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: new joined breps on success, None on failure.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.mergeBreps(brepsToMerge, tolerance, multiple=false)

   Combines two or more breps into one. A merge is like a boolean union that keeps the inside pieces. This
   function creates non-manifold Breps which in general are unusual in Rhino. You may want to consider using
   JoinBreps or CreateBooleanUnion functions instead.

   :param list[rhino3dm.Brep] brepsToMerge: must contain more than one Brep.
   :param float tolerance: the tolerance to use when merging.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Single merged Brep on success. Null on error.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.createContourCurves(brepToContour, contourStart, contourEnd, interval, multiple=false)

   Constructs the contour curves for a brep at a specified interval.

   :param rhino3dm.Brep brepToContour: A brep or polysurface.
   :param rhino3dm.Point3d contourStart: A point to start.
   :param rhino3dm.Point3d contourEnd: A point to use as the end.
   :param float interval: The interaxial offset in world units.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array with intersected curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. js:function:: RhinoCompute.Brep.createContourCurves1(brepToContour, sectionPlane, multiple=false)

   Constructs the contour curves for a brep, using a slicing plane.

   :param rhino3dm.Brep brepToContour: A brep or polysurface.
   :param rhino3dm.Plane sectionPlane: A plane.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array with intersected curves. This array can be empty.
   :rtype: rhino3dm.Curve[]
.. js:function:: RhinoCompute.Brep.createCurvatureAnalysisMesh(brep, state, multiple=false)

   Create an array of analysis meshes for the brep using the specified settings.
   Meshes aren't set on the brep.

   :param Rhino.ApplicationSettings.CurvatureAnalysisSettingsState state: CurvatureAnalysisSettingsState
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if meshes were created
   :rtype: rhino3dm.Mesh[]
.. js:function:: RhinoCompute.Brep.getRegions(thisBrep, multiple=false)

   Gets an array containing all regions in this brep.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of regions in this brep. This array can be empty, but not null.
   :rtype: BrepRegion[]
.. js:function:: RhinoCompute.Brep.getWireframe(thisBrep, density, multiple=false)

   Constructs all the Wireframe curves for this Brep.

   :param int density: Wireframe density. Valid values range between -1 and 99.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: An array of Wireframe curves or None on failure.
   :rtype: rhino3dm.Curve[]
.. js:function:: RhinoCompute.Brep.closestPoint(thisBrep, testPoint, multiple=false)

   Finds a point on the brep that is closest to testPoint.

   :param rhino3dm.Point3d testPoint: Base point to project to brep.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The point on the Brep closest to testPoint or Point3d.Unset if the operation failed.
   :rtype: rhino3dm.Point3d
.. js:function:: RhinoCompute.Brep.isPointInside(thisBrep, point, tolerance, strictlyIn, multiple=false)

   Determines if point is inside a Brep.  This question only makes sense when
   the brep is a closed and manifold.  This function does not check for
   closed or manifold, so result is not valid in those cases.  Intersects
   a line through point with brep, finds the intersection point Q closest
   to point, and looks at face normal at Q.  If the point Q is on an edge
   or the intersection is not transverse at Q, then another line is used.

   :param rhino3dm.Point3d point: 3d point to test.
   :param float tolerance: 3d distance tolerance used for intersection and determining strict inclusion. \
      A good default is RhinoMath.SqrtEpsilon.
   :param bool strictlyIn: if true, point is in if inside brep by at least tolerance. \
      if false, point is in if truly in or within tolerance of boundary.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if point is in, False if not.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.getPointInside(thisBrep, tolerance, multiple=false)

   Finds a point inside of a solid Brep.

   :param float tolerance: Used for intersecting rays and is not necessarily related to the distance from the brep to the found point. \
      When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: Returns False if the input is not solid and manifold, if the Brep's bounding box is less than 2.0 * tolerance wide, \
      or if no point could be found due to ray shooting or other errors. Otherwise, True is returned.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.capPlanarHoles(thisBrep, tolerance, multiple=false)

   Returns a new Brep that is equivalent to this Brep with all planar holes capped.

   :param float tolerance: Tolerance to use for capping.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: New brep on success. None on error.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.join(thisBrep, otherBrep, tolerance, compact, multiple=false)

   If any edges of this brep overlap edges of otherBrep, merge a copy of otherBrep into this
   brep joining all edges that overlap within tolerance.

   :param rhino3dm.Brep otherBrep: Brep to be added to this brep.
   :param float tolerance: 3d distance tolerance for detecting overlapping edges.
   :param bool compact: if true, set brep flags and tolerances, remove unused faces and edges.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if any edges were joined.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.joinNakedEdges(thisBrep, tolerance, multiple=false)

   Joins naked edge pairs within the same brep that overlap within tolerance.

   :param float tolerance: The tolerance value.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: number of joins made.
   :rtype: int
.. js:function:: RhinoCompute.Brep.mergeCoplanarFaces(thisBrep, tolerance, multiple=false)

   Merges adjacent coplanar faces into single faces.

   :param float tolerance: Tolerance for determining when edges are adjacent. \
      When in doubt, use the document's ModelAbsoluteTolerance property.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if faces were merged, False if no faces were merged.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.mergeCoplanarFaces1(thisBrep, tolerance, angleTolerance, multiple=false)

   Merges adjacent coplanar faces into single faces.

   :param float tolerance: Tolerance for determining when edges are adjacent. \
      When in doubt, use the document's ModelAbsoluteTolerance property.
   :param float angleTolerance: Angle tolerance, in radians, for determining when faces are parallel. \
      When in doubt, use the document's ModelAngleToleranceRadians property.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if faces were merged, False if no faces were merged.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.split(thisBrep, cutter, intersectionTolerance, multiple=false)

   Splits a Brep into pieces using a Brep as a cutter.

   :param rhino3dm.Brep cutter: The Brep to use as a cutter.
   :param float intersectionTolerance: The tolerance with which to compute intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of Breps. This array can be empty.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.split1(thisBrep, cutter, intersectionTolerance, multiple=false)

   Splits a Brep into pieces using a Brep as a cutter.

   :param rhino3dm.Brep cutter: The Brep to use as a cutter.
   :param float intersectionTolerance: The tolerance with which to compute intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of Breps. This array can be empty.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.split2(thisBrep, cutters, intersectionTolerance, multiple=false)

   Splits a Brep into pieces using Breps as cutters.

   :param list[rhino3dm.Brep] cutters: One or more Breps to use as cutters.
   :param float intersectionTolerance: The tolerance with which to compute intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of Breps. This array can be empty.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.split3(thisBrep, cutters, intersectionTolerance, multiple=false)

   Splits a Brep into pieces using curves, at least partially on the Brep, as cutters.

   :param list[rhino3dm.Curve] cutters: The splitting curves. Only the portion of the curve on the Brep surface will be used for cutting.
   :param float intersectionTolerance: The tolerance with which to compute intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of Breps. This array can be empty.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.split4(thisBrep, cutters, normal, planView, intersectionTolerance, multiple=false)

   Splits a Brep into pieces using a combination of curves, to be extruded, and Breps as cutters.

   :param list[rhino3dm.GeometryBase] cutters: The curves, surfaces, faces and Breps to be used as cutters. Any other geometry is ignored.
   :param rhino3dm.Vector3d normal: A construction plane normal, used in deciding how to extrude a curve into a cutter.
   :param bool planView: Set True if the assume view is a plan, or parallel projection, view.
   :param float intersectionTolerance: The tolerance with which to compute intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: A new array of Breps. This array can be empty.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.trim(thisBrep, cutter, intersectionTolerance, multiple=false)

   Trims a brep with an oriented cutter. The parts of the brep that lie inside
   (opposite the normal) of the cutter are retained while the parts to the
   outside (in the direction of the normal) are discarded.  If the Cutter is
   closed, then a connected component of the Brep that does not intersect the
   cutter is kept if and only if it is contained in the inside of cutter.
   That is the region bounded by cutter opposite from the normal of cutter,
   If cutter is not closed all these components are kept.

   :param rhino3dm.Brep cutter: A cutting brep.
   :param float intersectionTolerance: A tolerance value with which to compute intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: This Brep is not modified, the trim results are returned in an array.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.trim1(thisBrep, cutter, intersectionTolerance, multiple=false)

   Trims a Brep with an oriented cutter.  The parts of Brep that lie inside
   (opposite the normal) of the cutter are retained while the parts to the
   outside ( in the direction of the normal ) are discarded. A connected
   component of Brep that does not intersect the cutter is kept if and only
   if it is contained in the inside of Cutter.  That is the region bounded by
   cutter opposite from the normal of cutter, or in the case of a Plane cutter
   the half space opposite from the plane normal.

   :param rhino3dm.Plane cutter: A cutting plane.
   :param float intersectionTolerance: A tolerance value with which to compute intersections.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: This Brep is not modified, the trim results are returned in an array.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.unjoinEdges(thisBrep, edgesToUnjoin, multiple=false)

   Un-joins, or separates, edges within the Brep. Note, seams in closed surfaces will not separate.

   :param list[int] edgesToUnjoin: The indices of the Brep edges to un-join.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: This Brep is not modified, the trim results are returned in an array.
   :rtype: rhino3dm.Brep[]
.. js:function:: RhinoCompute.Brep.joinEdges(thisBrep, edgeIndex0, edgeIndex1, joinTolerance, compact, multiple=false)

   Joins two naked edges, or edges that are coincident or close together.

   :param int edgeIndex0: The first edge index.
   :param int edgeIndex1: The second edge index.
   :param float joinTolerance: The join tolerance.
   :param bool compact: If joining more than one edge pair and want the edge indices of subsequent pairs to remain valid, \
      set to false. But then call Brep.Compact() on the final result.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.transformComponent(thisBrep, components, xform, tolerance, timeLimit, useMultipleThreads, multiple=false)

   Transform an array of Brep components, bend neighbors to match, and leave the rest fixed.

   :param IEnumerable<ComponentIndex> components: The Brep components to transform.
   :param Transform xform: The transformation to apply.
   :param float tolerance: The desired fitting tolerance to use when bending faces that share edges with both fixed and transformed components.
   :param float timeLimit: If the deformation is extreme, it can take a long time to calculate the result. \
      If time_limit > 0, then the value specifies the maximum amount of time in seconds you want to spend before giving up.
   :param bool useMultipleThreads: True if multiple threads can be used.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True if successful, False otherwise.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.getArea(thisBrep, multiple=false)

   Compute the Area of the Brep. If you want proper Area data with moments
   and error information, use the AreaMassProperties class.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The area of the Brep.
   :rtype: float
.. js:function:: RhinoCompute.Brep.getArea1(thisBrep, relativeTolerance, absoluteTolerance, multiple=false)

   Compute the Area of the Brep. If you want proper Area data with moments
   and error information, use the AreaMassProperties class.

   :param float relativeTolerance: Relative tolerance to use for area calculation.
   :param float absoluteTolerance: Absolute tolerance to use for area calculation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The area of the Brep.
   :rtype: float
.. js:function:: RhinoCompute.Brep.getVolume(thisBrep, multiple=false)

   Compute the Volume of the Brep. If you want proper Volume data with moments
   and error information, use the VolumeMassProperties class.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The volume of the Brep.
   :rtype: float
.. js:function:: RhinoCompute.Brep.getVolume1(thisBrep, relativeTolerance, absoluteTolerance, multiple=false)

   Compute the Volume of the Brep. If you want proper Volume data with moments
   and error information, use the VolumeMassProperties class.

   :param float relativeTolerance: Relative tolerance to use for area calculation.
   :param float absoluteTolerance: Absolute tolerance to use for area calculation.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The volume of the Brep.
   :rtype: float
.. js:function:: RhinoCompute.Brep.rebuildTrimsForV2(thisBrep, face, nurbsSurface, multiple=false)

   No support is available for this function.
   Expert user function used by MakeValidForV2 to convert trim
   curves from one surface to its NURBS form. After calling this function,
   you need to change the surface of the face to a NurbsSurface.

   :param rhino3dm.BrepFace face: Face whose underlying surface has a parameterization that is different \
      from its NURBS form.
   :param NurbsSurface nurbsSurface: NURBS form of the face's underlying surface.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: void
.. js:function:: RhinoCompute.Brep.makeValidForV2(thisBrep, multiple=false)

   No support is available for this function.
   Expert user function that converts all geometry in Brep to NURB form.

   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :rtype: bool
.. js:function:: RhinoCompute.Brep.repair(thisBrep, tolerance, multiple=false)

   Fills in missing or fixes incorrect component information from a Brep.
   Useful when reading Brep information from other file formats that do not
   provide as complete of a Brep definition as required by Rhino.

   :param float tolerance: The repair tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: True on success.
   :rtype: bool
.. js:function:: RhinoCompute.Brep.removeHoles(thisBrep, tolerance, multiple=false)

   Remove all inner loops, or holes, in a Brep.

   :param float tolerance: The tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The Brep without holes if successful, None otherwise.
   :rtype: rhino3dm.Brep
.. js:function:: RhinoCompute.Brep.removeHoles1(thisBrep, loops, tolerance, multiple=false)

   Removes inner loops, or holes, in a Brep.

   :param IEnumerable<ComponentIndex> loops: A list of BrepLoop component indexes, where BrepLoop.LoopType == Rhino.Geometry.BrepLoopType.Inner.
   :param float tolerance: The tolerance. When in doubt, use the document's model absolute tolerance.
   :param bool multiple: (default False) If True, all parameters are expected as lists of equal length and input will be batch processed

   :return: The Brep without holes if successful, None otherwise.
   :rtype: rhino3dm.Brep
