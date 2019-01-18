from . import Util


def ChangeSeam(face, direction, parameter, tolerance, multiple=False):
    """
    Change the seam of a closed trimmed surface.

    Args:
        face (BrepFace): A Brep face with a closed underlying surface.
        direction (int): The parameter direction (0 = U, 1 = V). The face's underlying surface must be closed in this direction.
        parameter (double): The parameter at which to place the seam.
        tolerance (double): Tolerance used to cut up surface.

    Returns:
        Brep: A new Brep that has the same geoemtry as the face with a relocated seam if successful, or None on failure.
    """
    url = "rhino/geometry/brep/changeseam-brepface_int_double_double"
    if multiple: url += "?multiple=true"
    args = [face, direction, parameter, tolerance]
    if multiple: args = zip(face, direction, parameter, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CopyTrimCurves(trimSource, surfaceSource, tolerance, multiple=False):
    """
    Copy all trims from a Brep face onto a surface.

    Args:
        trimSource (BrepFace): Brep face which defines the trimming curves.
        surfaceSource (Surface): The surface to trim.
        tolerance (double): Tolerance to use for rebuilding 3D trim curves.

    Returns:
        Brep: A brep with the shape of surfaceSource and the trims of trimSource or None on failure.
    """
    url = "rhino/geometry/brep/copytrimcurves-brepface_surface_double"
    if multiple: url += "?multiple=true"
    args = [trimSource, surfaceSource, tolerance]
    if multiple: args = zip(trimSource, surfaceSource, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBaseballSphere(center, radius, tolerance, multiple=False):
    """
    Creates a brep representation of the sphere with two similar trimmed NURBS surfaces, and no singularities.

    Args:
        center (Point3d): The center of the sphere.
        radius (double): The radius of the sphere.
        tolerance (double): Used in computing 2d trimming curves. If >= 0.0, then the max of
            ON_0.0001 * radius and RhinoMath.ZeroTolerance will be used.

    Returns:
        Brep: A new brep, or None on error.
    """
    url = "rhino/geometry/brep/createbaseballsphere-point3d_double_double"
    if multiple: url += "?multiple=true"
    args = [center, radius, tolerance]
    if multiple: args = zip(center, radius, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateDevelopableLoft(crv0, crv1, reverse0, reverse1, density, multiple=False):
    """
    Creates a single developable surface between two curves.

    Args:
        crv0 (Curve): The first rail curve.
        crv1 (Curve): The second rail curve.
        reverse0 (bool): Reverse the first rail curve.
        reverse1 (bool): Reverse the second rail curve
        density (int): The number of rulings across the surface.

    Returns:
        Brep[]: The output Breps if successful, otherwise an empty array.
    """
    url = "rhino/geometry/brep/createdevelopableloft-curve_curve_bool_bool_int"
    if multiple: url += "?multiple=true"
    args = [crv0, crv1, reverse0, reverse1, density]
    if multiple: args = zip(crv0, crv1, reverse0, reverse1, density)
    response = Util.ComputeFetch(url, args)
    return response


def CreateDevelopableLoft1(rail0, rail1, fixedRulings, multiple=False):
    """
    Creates a single developable surface between two curves.

    Args:
        rail0 (NurbsCurve): The first rail curve.
        rail1 (NurbsCurve): The second rail curve.
        fixedRulings (IEnumerable<Point2d>): Rulings define lines across the surface that define the straight sections on the developable surface,
            where rulings[i].X = parameter on first rail curve, and rulings[i].Y = parameter on second rail curve.
            Note, rulings will be automatically adjusted to minimum twist.

    Returns:
        Brep[]: The output Breps if successful, otherwise an empty array.
    """
    url = "rhino/geometry/brep/createdevelopableloft-nurbscurve_nurbscurve_point2darray"
    if multiple: url += "?multiple=true"
    args = [rail0, rail1, fixedRulings]
    if multiple: args = zip(rail0, rail1, fixedRulings)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps(inputLoops, multiple=False):
    """
    Constructs a set of planar breps as outlines by the loops.

    Args:
        inputLoops (IEnumerable<Curve>): Curve loops that delineate the planar boundaries.

    Returns:
        Brep[]: An array of Planar Breps.
    """
    url = "rhino/geometry/brep/createplanarbreps-curvearray"
    if multiple: url += "?multiple=true"
    args = [inputLoops]
    if multiple: args = [[item] for item in inputLoops]
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps1(inputLoops, tolerance, multiple=False):
    """
    Constructs a set of planar breps as outlines by the loops.

    Args:
        inputLoops (IEnumerable<Curve>): Curve loops that delineate the planar boundaries.

    Returns:
        Brep[]: An array of Planar Breps.
    """
    url = "rhino/geometry/brep/createplanarbreps-curvearray_double"
    if multiple: url += "?multiple=true"
    args = [inputLoops, tolerance]
    if multiple: args = zip(inputLoops, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps2(inputLoop, multiple=False):
    """
    Constructs a set of planar breps as outlines by the loops.

    Args:
        inputLoop (Curve): A curve that should form the boundaries of the surfaces or polysurfaces.

    Returns:
        Brep[]: An array of Planar Breps.
    """
    url = "rhino/geometry/brep/createplanarbreps-curve"
    if multiple: url += "?multiple=true"
    args = [inputLoop]
    if multiple: args = [[item] for item in inputLoop]
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps3(inputLoop, tolerance, multiple=False):
    """
    Constructs a set of planar breps as outlines by the loops.

    Args:
        inputLoop (Curve): A curve that should form the boundaries of the surfaces or polysurfaces.

    Returns:
        Brep[]: An array of Planar Breps.
    """
    url = "rhino/geometry/brep/createplanarbreps-curve_double"
    if multiple: url += "?multiple=true"
    args = [inputLoop, tolerance]
    if multiple: args = zip(inputLoop, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTrimmedSurface(trimSource, surfaceSource, multiple=False):
    """
    Constructs a Brep using the trimming information of a brep face and a surface.
    Surface must be roughly the same shape and in the same location as the trimming brep face.

    Args:
        trimSource (BrepFace): BrepFace which contains trimmingSource brep.
        surfaceSource (Surface): Surface that trims of BrepFace will be applied to.

    Returns:
        Brep: A brep with the shape of surfaceSource and the trims of trimSource or None on failure.
    """
    url = "rhino/geometry/brep/createtrimmedsurface-brepface_surface"
    if multiple: url += "?multiple=true"
    args = [trimSource, surfaceSource]
    if multiple: args = zip(trimSource, surfaceSource)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTrimmedSurface1(trimSource, surfaceSource, tolerance, multiple=False):
    """
    Constructs a Brep using the trimming information of a brep face and a surface.
    Surface must be roughly the same shape and in the same location as the trimming brep face.

    Args:
        trimSource (BrepFace): BrepFace which contains trimmingSource brep.
        surfaceSource (Surface): Surface that trims of BrepFace will be applied to.

    Returns:
        Brep: A brep with the shape of surfaceSource and the trims of trimSource or None on failure.
    """
    url = "rhino/geometry/brep/createtrimmedsurface-brepface_surface_double"
    if multiple: url += "?multiple=true"
    args = [trimSource, surfaceSource, tolerance]
    if multiple: args = zip(trimSource, surfaceSource, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCornerPoints(corner1, corner2, corner3, tolerance, multiple=False):
    """
    Makes a brep with one face.

    Args:
        corner1 (Point3d): A first corner.
        corner2 (Point3d): A second corner.
        corner3 (Point3d): A third corner.
        tolerance (double): Minimum edge length without collapsing to a singularity.

    Returns:
        Brep: A boundary representation, or None on error.
    """
    url = "rhino/geometry/brep/createfromcornerpoints-point3d_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [corner1, corner2, corner3, tolerance]
    if multiple: args = zip(corner1, corner2, corner3, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCornerPoints1(corner1, corner2, corner3, corner4, tolerance, multiple=False):
    """
    make a Brep with one face.

    Args:
        corner1 (Point3d): A first corner.
        corner2 (Point3d): A second corner.
        corner3 (Point3d): A third corner.
        corner4 (Point3d): A fourth corner.
        tolerance (double): Minimum edge length allowed before collapsing the side into a singularity.

    Returns:
        Brep: A boundary representation, or None on error.
    """
    url = "rhino/geometry/brep/createfromcornerpoints-point3d_point3d_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [corner1, corner2, corner3, corner4, tolerance]
    if multiple: args = zip(corner1, corner2, corner3, corner4, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateEdgeSurface(curves, multiple=False):
    """
    Constructs a coons patch from 2, 3, or 4 curves.

    Args:
        curves (IEnumerable<Curve>): A list, an array or any enumerable set of curves.

    Returns:
        Brep: resulting brep or None on failure.
    """
    url = "rhino/geometry/brep/createedgesurface-curvearray"
    if multiple: url += "?multiple=true"
    args = [curves]
    if multiple: args = [[item] for item in curves]
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps(inputLoops, multiple=False):
    """
    Constructs a set of planar Breps as outlines by the loops.

    Args:
        inputLoops (Rhino.Collections.CurveList): Curve loops that delineate the planar boundaries.

    Returns:
        Brep[]: An array of Planar Breps or None on error.
    """
    url = "rhino/geometry/brep/createplanarbreps-rhino.collections.curvelist"
    if multiple: url += "?multiple=true"
    args = [inputLoops]
    if multiple: args = [[item] for item in inputLoops]
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps1(inputLoops, tolerance, multiple=False):
    """
    Constructs a set of planar Breps as outlines by the loops.

    Args:
        inputLoops (Rhino.Collections.CurveList): Curve loops that delineate the planar boundaries.

    Returns:
        Brep[]: An array of Planar Breps.
    """
    url = "rhino/geometry/brep/createplanarbreps-rhino.collections.curvelist_double"
    if multiple: url += "?multiple=true"
    args = [inputLoops, tolerance]
    if multiple: args = zip(inputLoops, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromOffsetFace(face, offsetDistance, offsetTolerance, bothSides, createSolid, multiple=False):
    """
    Offsets a face including trim information to create a new brep.

    Args:
        face (BrepFace): the face to offset.
        offsetDistance (double): An offset distance.
        offsetTolerance (double): Use 0.0 to make a loose offset. Otherwise, the document's absolute tolerance is usually sufficient.
        bothSides (bool): When true, offset to both sides of the input face.
        createSolid (bool): When true, make a solid object.

    Returns:
        Brep: A new brep if successful. The brep can be disjoint if bothSides is True and createSolid is false,
        or if createSolid is True and connecting the offsets with side surfaces fails.
        None if unsuccessful.
    """
    url = "rhino/geometry/brep/createfromoffsetface-brepface_double_double_bool_bool"
    if multiple: url += "?multiple=true"
    args = [face, offsetDistance, offsetTolerance, bothSides, createSolid]
    if multiple: args = zip(face, offsetDistance, offsetTolerance, bothSides, createSolid)
    response = Util.ComputeFetch(url, args)
    return response


def CreateSolid(breps, tolerance, multiple=False):
    """
    Constructs closed polysurfaces from surfaces and polysurfaces that bound a region in space.

    Args:
        breps (IEnumerable<Brep>): The intersecting surfaces and polysurfaces to automatically trim and join into closed polysurfaces.
        tolerance (double): The trim and join tolerance. If set to RhinoMath.UnsetValue, Rhino's global absolute tolerance is used.

    Returns:
        Brep[]: The resulting polysurfaces on success or None on failure.
    """
    url = "rhino/geometry/brep/createsolid-breparray_double"
    if multiple: url += "?multiple=true"
    args = [breps, tolerance]
    if multiple: args = zip(breps, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MergeSurfaces(surface0, surface1, tolerance, angleToleranceRadians, multiple=False):
    """
    Merges two surfaces into one surface at untrimmed edges.

    Args:
        surface0 (Surface): The first surface to merge.
        surface1 (Surface): The second surface to merge.
        tolerance (double): Surface edges must be within this tolerance for the two surfaces to merge.
        angleToleranceRadians (double): Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.

    Returns:
        Brep: The merged surfaces as a Brep if successful, None if not successful.
    """
    url = "rhino/geometry/brep/mergesurfaces-surface_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [surface0, surface1, tolerance, angleToleranceRadians]
    if multiple: args = zip(surface0, surface1, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def MergeSurfaces1(brep0, brep1, tolerance, angleToleranceRadians, multiple=False):
    """
    Merges two surfaces into one surface at untrimmed edges. Both surfaces must be untrimmed and share an edge.

    Args:
        brep0 (Brep): The first single-face Brep to merge.
        brep1 (Brep): The second single-face Brep to merge.
        tolerance (double): Surface edges must be within this tolerance for the two surfaces to merge.
        angleToleranceRadians (double): Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.

    Returns:
        Brep: The merged Brep if successful, None if not successful.
    """
    url = "rhino/geometry/brep/mergesurfaces-brep_brep_double_double"
    if multiple: url += "?multiple=true"
    args = [brep0, brep1, tolerance, angleToleranceRadians]
    if multiple: args = zip(brep0, brep1, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def MergeSurfaces2(brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth, multiple=False):
    """
    Merges two surfaces into one surface at untrimmed edges. Both surfaces must be untrimmed and share an edge.

    Args:
        brep0 (Brep): The first single-face Brep to merge.
        brep1 (Brep): The second single-face Brep to merge.
        tolerance (double): Surface edges must be within this tolerance for the two surfaces to merge.
        angleToleranceRadians (double): Edge must be within this angle tolerance in order for contiguous edges to be combined into a single edge.
        point0 (Point2d): 2D pick point on the first single-face Brep. The value can be unset.
        point1 (Point2d): 2D pick point on the second single-face Brep. The value can be unset.
        roundness (double): Defines the roundness of the merge. Acceptable values are between 0.0 (sharp) and 1.0 (smooth).
        smooth (bool): The surface will be smooth. This makes the surface behave better for control point editing, but may alter the shape of both surfaces.

    Returns:
        Brep: The merged Brep if successful, None if not successful.
    """
    url = "rhino/geometry/brep/mergesurfaces-brep_brep_double_double_point2d_point2d_double_bool"
    if multiple: url += "?multiple=true"
    args = [brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth]
    if multiple: args = zip(brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePatch(geometry, startingSurface, tolerance, multiple=False):
    """
    Constructs a brep patch.
    This is the simple version of fit that uses a specified starting surface.

    Args:
        geometry (IEnumerable<GeometryBase>): Combination of Curves, BrepTrims, Points, PointClouds or Meshes.
            Curves and trims are sampled to get points. Trims are sampled for
            points and normals.
        startingSurface (Surface): A starting surface (can be null).
        tolerance (double): Tolerance used by input analysis functions for loop finding, trimming, etc.

    Returns:
        Brep: Brep fit through input on success, or None on error.
    """
    url = "rhino/geometry/brep/createpatch-geometrybasearray_surface_double"
    if multiple: url += "?multiple=true"
    args = [geometry, startingSurface, tolerance]
    if multiple: args = zip(geometry, startingSurface, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePatch1(geometry, uSpans, vSpans, tolerance, multiple=False):
    """
    Constructs a brep patch.
    This is the simple version of fit that uses a plane with u x v spans.
    It makes a plane by fitting to the points from the input geometry to use as the starting surface.
    The surface has the specified u and v span count.

    Args:
        geometry (IEnumerable<GeometryBase>): A combination of curves, brep trims,
            points, point clouds or meshes.
            Curves and trims are sampled to get points. Trims are sampled for
            points and normals.
        uSpans (int): The number of spans in the U direction.
        vSpans (int): The number of spans in the V direction.
        tolerance (double): Tolerance used by input analysis functions for loop finding, trimming, etc.

    Returns:
        Brep: A brep fit through input on success, or None on error.
    """
    url = "rhino/geometry/brep/createpatch-geometrybasearray_int_int_double"
    if multiple: url += "?multiple=true"
    args = [geometry, uSpans, vSpans, tolerance]
    if multiple: args = zip(geometry, uSpans, vSpans, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePatch2(geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance, multiple=False):
    """
    Constructs a brep patch using all controls

    Args:
        geometry (IEnumerable<GeometryBase>): A combination of curves, brep trims,
            points, point clouds or meshes.
            Curves and trims are sampled to get points. Trims are sampled for
            points and normals.
        startingSurface (Surface): A starting surface (can be null).
        uSpans (int): Number of surface spans used when a plane is fit through points to start in the U direction.
        vSpans (int): Number of surface spans used when a plane is fit through points to start in the U direction.
        trim (bool): If true, try to find an outer loop from among the input curves and trim the result to that loop
        tangency (bool): If true, try to find brep trims in the outer loop of curves and try to
            fit to the normal direction of the trim's surface at those locations.
        pointSpacing (double): Basic distance between points sampled from input curves.
        flexibility (double): Determines the behavior of the surface in areas where its not otherwise
            controlled by the input.  Lower numbers make the surface behave more
            like a stiff material; higher, less like a stiff material.  That is,
            each span is made to more closely match the spans adjacent to it if there
            is no input geometry mapping to that area of the surface when the
            flexibility value is low.  The scale is logrithmic. Numbers around 0.001
            or 0.1 make the patch pretty stiff and numbers around 10 or 100 make the
            surface flexible.
        surfacePull (double): Tends to keep the result surface where it was before the fit in areas where
            there is on influence from the input geometry
        fixEdges (bool[]): Array of four elements. Flags to keep the edges of a starting (untrimmed)
            surface in place while fitting the interior of the surface.  Order of
            flags is left, bottom, right, top
        tolerance (double): Tolerance used by input analysis functions for loop finding, trimming, etc.

    Returns:
        Brep: A brep fit through input on success, or None on error.
    """
    url = "rhino/geometry/brep/createpatch-geometrybasearray_surface_int_int_bool_bool_double_double_double_boolarray_double"
    if multiple: url += "?multiple=true"
    args = [geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance]
    if multiple: args = zip(geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePipe(rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians, multiple=False):
    """
    Creates a single walled pipe

    Args:
        rail (Curve): the path curve for the pipe
        radius (double): radius of the pipe
        localBlending (bool): If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
            If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied
        cap (PipeCapMode): end cap mode
        fitRail (bool): If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
            otherwise the result is a polysurface with joined surfaces created from the polycurve segments.
        absoluteTolerance (double): The sweeping and fitting tolerance. If you are unsure what to use, then use the document's absolute tolerance
        angleToleranceRadians (double): The angle tolerance. If you are unsure what to use, then either use the document's angle tolerance in radians

    Returns:
        Brep[]: Array of created pipes on success
    """
    url = "rhino/geometry/brep/createpipe-curve_double_bool_pipecapmode_bool_double_double"
    if multiple: url += "?multiple=true"
    args = [rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians]
    if multiple: args = zip(rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePipe1(rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians, multiple=False):
    """
    Creates a single walled pipe

    Args:
        rail (Curve): the path curve for the pipe
        railRadiiParameters (IEnumerable<double>): one or more normalized curve parameters where changes in radius occur.
            Important: curve parameters must be normalized - ranging between 0.0 and 1.0.
        radii (IEnumerable<double>): An array of radii - one at each normalized curve parameter in railRadiiParameters.
        localBlending (bool): If True, Local (pipe radius stays constant at the ends and changes more rapidly in the middle) is applied.
            If False, Global (radius is linearly blended from one end to the other, creating pipes that taper from one radius to the other) is applied
        cap (PipeCapMode): end cap mode
        fitRail (bool): If the curve is a polycurve of lines and arcs, the curve is fit and a single surface is created;
            otherwise the result is a polysurface with joined surfaces created from the polycurve segments.
        absoluteTolerance (double): The sweeping and fitting tolerance. If you are unsure what to use, then use the document's absolute tolerance
        angleToleranceRadians (double): The angle tolerance. If you are unsure what to use, then either use the document's angle tolerance in radians

    Returns:
        Brep[]: Array of created pipes on success
    """
    url = "rhino/geometry/brep/createpipe-curve_doublearray_doublearray_bool_pipecapmode_bool_double_double"
    if multiple: url += "?multiple=true"
    args = [rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians]
    if multiple: args = zip(rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweep(rail, shape, closed, tolerance, multiple=False):
    """
    General 1 rail sweep. If you are not producing the sweep results that you are after, then
    use the SweepOneRail class with options to generate the swept geometry

    Args:
        rail (Curve): Rail to sweep shapes along
        shape (Curve): Shape curve
        closed (bool): Only matters if shape is closed
        tolerance (double): Tolerance for fitting surface and rails

    Returns:
        Brep[]: Array of Brep sweep results
    """
    url = "rhino/geometry/brep/createfromsweep-curve_curve_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail, shape, closed, tolerance]
    if multiple: args = zip(rail, shape, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweep1(rail, shapes, closed, tolerance, multiple=False):
    """
    General 1 rail sweep. If you are not producing the sweep results that you are after, then
    use the SweepOneRail class with options to generate the swept geometry

    Args:
        rail (Curve): Rail to sweep shapes along
        shapes (IEnumerable<Curve>): Shape curves
        closed (bool): Only matters if shapes are closed
        tolerance (double): Tolerance for fitting surface and rails

    Returns:
        Brep[]: Array of Brep sweep results
    """
    url = "rhino/geometry/brep/createfromsweep-curve_curvearray_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail, shapes, closed, tolerance]
    if multiple: args = zip(rail, shapes, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweep2(rail1, rail2, shape, closed, tolerance, multiple=False):
    """
    General 2 rail sweep. If you are not producing the sweep results that you are after, then
    use the SweepTwoRail class with options to generate the swept geometry

    Args:
        rail1 (Curve): Rail to sweep shapes along
        rail2 (Curve): Rail to sweep shapes along
        shape (Curve): Shape curve
        closed (bool): Only matters if shape is closed
        tolerance (double): Tolerance for fitting surface and rails

    Returns:
        Brep[]: Array of Brep sweep results
    """
    url = "rhino/geometry/brep/createfromsweep-curve_curve_curve_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail1, rail2, shape, closed, tolerance]
    if multiple: args = zip(rail1, rail2, shape, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweep3(rail1, rail2, shapes, closed, tolerance, multiple=False):
    """
    General 2 rail sweep. If you are not producing the sweep results that you are after, then
    use the SweepTwoRail class with options to generate the swept geometry

    Args:
        rail1 (Curve): Rail to sweep shapes along
        rail2 (Curve): Rail to sweep shapes along
        shapes (IEnumerable<Curve>): Shape curves
        closed (bool): Only matters if shapes are closed
        tolerance (double): Tolerance for fitting surface and rails

    Returns:
        Brep[]: Array of Brep sweep results
    """
    url = "rhino/geometry/brep/createfromsweep-curve_curve_curvearray_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail1, rail2, shapes, closed, tolerance]
    if multiple: args = zip(rail1, rail2, shapes, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweepInParts(rail1, rail2, shapes, rail_params, closed, tolerance, multiple=False):
    """
    Makes a 2 rail sweep.  Like CreateFromSweep but the result is split where parameterization along a rail changes abruptly

    Args:
        rail1 (Curve): Rail to sweep shapes along
        rail2 (Curve): Rail to sweep shapes along
        shapes (IEnumerable<Curve>): Shape curves
        rail_params (IEnumerable<Point2d>): Shape parameters
        closed (bool): Only matters if shapes are closed
        tolerance (double): Tolerance for fitting surface and rails

    Returns:
        Brep[]: Array of Brep sweep results
    """
    url = "rhino/geometry/brep/createfromsweepinparts-curve_curve_curvearray_point2darray_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail1, rail2, shapes, rail_params, closed, tolerance]
    if multiple: args = zip(rail1, rail2, shapes, rail_params, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromTaperedExtrude(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians, multiple=False):
    """
    Extrude a curve to a taper making a brep (potentially more than 1)

    Args:
        curveToExtrude (Curve): the curve to extrude
        distance (double): the distance to extrude
        direction (Vector3d): the direction of the extrusion
        basePoint (Point3d): the basepoint of the extrusion
        draftAngleRadians (double): angle of the extrusion
        tolerance (double): tolerance to use for the extrusion
        angleToleranceRadians (double): angle tolerance to use for the extrusion

    Returns:
        Brep[]: array of breps on success
    """
    url = "rhino/geometry/brep/createfromtaperedextrude-curve_double_vector3d_point3d_double_extrudecornertype_double_double"
    if multiple: url += "?multiple=true"
    args = [curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians]
    if multiple: args = zip(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromTaperedExtrude1(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, multiple=False):
    """
    Extrude a curve to a taper making a brep (potentially more than 1)

    Args:
        curveToExtrude (Curve): the curve to extrude
        distance (double): the distance to extrude
        direction (Vector3d): the direction of the extrusion
        basePoint (Point3d): the basepoint of the extrusion
        draftAngleRadians (double): angle of the extrusion

    Returns:
        Brep[]: array of breps on success
    """
    url = "rhino/geometry/brep/createfromtaperedextrude-curve_double_vector3d_point3d_double_extrudecornertype"
    if multiple: url += "?multiple=true"
    args = [curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType]
    if multiple: args = zip(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendSurface(face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1, multiple=False):
    """
    Makes a surface blend between two surface edges.

    Args:
        face0 (BrepFace): First face to blend from.
        edge0 (BrepEdge): First edge to blend from.
        domain0 (Interval): The domain of edge0 to use.
        rev0 (bool): If false, edge0 will be used in its natural direction. If true, edge0 will be used in the reversed direction.
        continuity0 (BlendContinuity): Continuity for the blend at the start.
        face1 (BrepFace): Second face to blend from.
        edge1 (BrepEdge): Second edge to blend from.
        domain1 (Interval): The domain of edge1 to use.
        rev1 (bool): If false, edge1 will be used in its natural direction. If true, edge1 will be used in the reversed direction.
        continuity1 (BlendContinuity): Continuity for the blend at the end.

    Returns:
        Brep[]: Array of Breps if successful.
    """
    url = "rhino/geometry/brep/createblendsurface-brepface_brepedge_interval_bool_blendcontinuity_brepface_brepedge_interval_bool_blendcontinuity"
    if multiple: url += "?multiple=true"
    args = [face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1]
    if multiple: args = zip(face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendShape(face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1, multiple=False):
    """
    Makes a curve blend between points on two surface edges. The blend will be tangent to the surfaces and perpendicular to the edges.

    Args:
        face0 (BrepFace): First face to blend from.
        edge0 (BrepEdge): First edge to blend from.
        t0 (double): Location on first edge for first end of blend curve.
        rev0 (bool): If false, edge0 will be used in its natural direction. If true, edge0 will be used in the reversed direction.
        continuity0 (BlendContinuity): Continuity for the blend at the start.
        face1 (BrepFace): Second face to blend from.
        edge1 (BrepEdge): Second edge to blend from.
        t1 (double): Location on second edge for second end of blend curve.
        rev1 (bool): If false, edge1 will be used in its natural direction. If true, edge1 will be used in the reversed direction.
        continuity1 (BlendContinuity): >Continuity for the blend at the end.

    Returns:
        Curve: The blend curve on success. None on failure
    """
    url = "rhino/geometry/brep/createblendshape-brepface_brepedge_double_bool_blendcontinuity_brepface_brepedge_double_bool_blendcontinuity"
    if multiple: url += "?multiple=true"
    args = [face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1]
    if multiple: args = zip(face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFilletSurface(face0, uv0, face1, uv1, radius, extend, tolerance, multiple=False):
    """
    Creates a constant-radius round surface between two surfaces.

    Args:
        face0 (BrepFace): First face to fillet from.
        uv0 (Point2d): A parameter face0 at the side you want to keep after filleting.
        face1 (BrepFace): Second face to fillet from.
        uv1 (Point2d): A parameter face1 at the side you want to keep after filleting.
        radius (double): The fillet radius.
        extend (bool): If true, then when one input surface is longer than the other, the fillet surface is extended to the input surface edges.
        tolerance (double): The tolerance. In in doubt, the the document's model absolute tolerance.

    Returns:
        Brep[]: Array of Breps if successful.
    """
    url = "rhino/geometry/brep/createfilletsurface-brepface_point2d_brepface_point2d_double_bool_double"
    if multiple: url += "?multiple=true"
    args = [face0, uv0, face1, uv1, radius, extend, tolerance]
    if multiple: args = zip(face0, uv0, face1, uv1, radius, extend, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateChamferSurface(face0, uv0, radius0, face1, uv1, radius1, extend, tolerance, multiple=False):
    """
    Creates a ruled surface as a bevel between two input surface edges.

    Args:
        face0 (BrepFace): First face to chamfer from.
        uv0 (Point2d): A parameter face0 at the side you want to keep after chamfering.
        radius0 (double): The distance from the intersection of face0 to the edge of the chamfer.
        face1 (BrepFace): Second face to chamfer from.
        uv1 (Point2d): A parameter face1 at the side you want to keep after chamfering.
        radius1 (double): The distance from the intersection of face1 to the edge of the chamfer.
        extend (bool): If true, then when one input surface is longer than the other, the chamfer surface is extended to the input surface edges.
        tolerance (double): The tolerance. In in doubt, the the document's model absolute tolerance.

    Returns:
        Brep[]: Array of Breps if successful.
    """
    url = "rhino/geometry/brep/createchamfersurface-brepface_point2d_double_brepface_point2d_double_bool_double"
    if multiple: url += "?multiple=true"
    args = [face0, uv0, radius0, face1, uv1, radius1, extend, tolerance]
    if multiple: args = zip(face0, uv0, radius0, face1, uv1, radius1, extend, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFilletEdges(brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance, multiple=False):
    """
    Fillets, chamfers, or blends the edges of a brep.

    Args:
        brep (Brep): The brep to fillet, chamfer, or blend edges.
        edgeIndices (IEnumerable<int>): An array of one or more edge indices where the fillet, chamfer, or blend will occur.
        startRadii (IEnumerable<double>): An array of starting fillet, chamfer, or blend radaii, one for each edge index.
        endRadii (IEnumerable<double>): An array of ending fillet, chamfer, or blend radaii, one for each edge index.
        blendType (BlendType): The blend type.
        railType (RailType): The rail type.
        tolerance (double): The tolerance to be used to perform calculations.

    Returns:
        Brep[]: Array of Breps if successful.
    """
    url = "rhino/geometry/brep/createfilletedges-brep_intarray_doublearray_doublearray_blendtype_railtype_double"
    if multiple: url += "?multiple=true"
    args = [brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance]
    if multiple: args = zip(brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromJoinedEdges(brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance, multiple=False):
    """
    Joins two naked edges, or edges that are coincident or close together, from two Breps.

    Args:
        brep0 (Brep): The first Brep.
        edgeIndex0 (int): The edge index on the first Brep.
        brep1 (Brep): The second Brep.
        edgeIndex1 (int): The edge index on the second Brep.
        joinTolerance (double): The join tolerance.

    Returns:
        Brep: The resulting Brep if successful, None on failure.
    """
    url = "rhino/geometry/brep/createfromjoinededges-brep_int_brep_int_double"
    if multiple: url += "?multiple=true"
    args = [brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance]
    if multiple: args = zip(brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromLoft(curves, start, end, loftType, closed, multiple=False):
    """
    Constructs one or more Breps by lofting through a set of curves.

    Args:
        curves (IEnumerable<Curve>): The curves to loft through. This function will not perform any curve sorting. You must pass in
            curves in the order you want them lofted. This function will not adjust the directions of open
            curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
            This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
            adjust the seam of closed curves.
        start (Point3d): Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        end (Point3d): Optional ending point of loft. Use Point3d.Unset if you do not want to include an end point.
        loftType (LoftType): type of loft to perform.
        closed (bool): True if the last curve in this loft should be connected back to the first one.

    Returns:
        Brep[]: Constructs a closed surface, continuing the surface past the last curve around to the
        first curve. Available when you have selected three shape curves.
    """
    url = "rhino/geometry/brep/createfromloft-curvearray_point3d_point3d_lofttype_bool"
    if multiple: url += "?multiple=true"
    args = [curves, start, end, loftType, closed]
    if multiple: args = zip(curves, start, end, loftType, closed)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromLoftRebuild(curves, start, end, loftType, closed, rebuildPointCount, multiple=False):
    """
    Constructs one or more Breps by lofting through a set of curves. Input for the loft is simplified by
    rebuilding to a specified number of control points.

    Args:
        curves (IEnumerable<Curve>): The curves to loft through. This function will not perform any curve sorting. You must pass in
            curves in the order you want them lofted. This function will not adjust the directions of open
            curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
            This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
            adjust the seam of closed curves.
        start (Point3d): Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        end (Point3d): Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point.
        loftType (LoftType): type of loft to perform.
        closed (bool): True if the last curve in this loft should be connected back to the first one.
        rebuildPointCount (int): A number of points to use while rebuilding the curves. 0 leaves turns this parameter off.

    Returns:
        Brep[]: Constructs a closed surface, continuing the surface past the last curve around to the
        first curve. Available when you have selected three shape curves.
    """
    url = "rhino/geometry/brep/createfromloftrebuild-curvearray_point3d_point3d_lofttype_bool_int"
    if multiple: url += "?multiple=true"
    args = [curves, start, end, loftType, closed, rebuildPointCount]
    if multiple: args = zip(curves, start, end, loftType, closed, rebuildPointCount)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromLoftRefit(curves, start, end, loftType, closed, refitTolerance, multiple=False):
    """
    Constructs one or more Breps by lofting through a set of curves. Input for the loft is simplified by
    refitting to a specified tolerance.

    Args:
        curves (IEnumerable<Curve>): The curves to loft through. This function will not perform any curve sorting. You must pass in
            curves in the order you want them lofted. This function will not adjust the directions of open
            curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
            This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
            adjust the seam of closed curves.
        start (Point3d): Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point.
        end (Point3d): Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point.
        loftType (LoftType): type of loft to perform.
        closed (bool): True if the last curve in this loft should be connected back to the first one.
        refitTolerance (double): A distance to use in refitting, or 0 if you want to turn this parameter off.

    Returns:
        Brep[]: Constructs a closed surface, continuing the surface past the last curve around to the
        first curve. Available when you have selected three shape curves.
    """
    url = "rhino/geometry/brep/createfromloftrefit-curvearray_point3d_point3d_lofttype_bool_double"
    if multiple: url += "?multiple=true"
    args = [curves, start, end, loftType, closed, refitTolerance]
    if multiple: args = zip(curves, start, end, loftType, closed, refitTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion(breps, tolerance, multiple=False):
    """
    Compute the Boolean Union of a set of Breps.

    Args:
        breps (IEnumerable<Brep>): Breps to union.
        tolerance (double): Tolerance to use for union operation.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleanunion-breparray_double"
    if multiple: url += "?multiple=true"
    args = [breps, tolerance]
    if multiple: args = zip(breps, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion1(breps, tolerance, manifoldOnly, multiple=False):
    """
    Compute the Boolean Union of a set of Breps.

    Args:
        breps (IEnumerable<Brep>): Breps to union.
        tolerance (double): Tolerance to use for union operation.
        manifoldOnly (bool): If true, non-manifold input breps are ignored.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleanunion-breparray_double_bool"
    if multiple: url += "?multiple=true"
    args = [breps, tolerance, manifoldOnly]
    if multiple: args = zip(breps, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection(firstSet, secondSet, tolerance, multiple=False):
    """
    Compute the Solid Intersection of two sets of Breps.

    Args:
        firstSet (IEnumerable<Brep>): First set of Breps.
        secondSet (IEnumerable<Brep>): Second set of Breps.
        tolerance (double): Tolerance to use for intersection operation.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleanintersection-breparray_breparray_double"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet, tolerance]
    if multiple: args = zip(firstSet, secondSet, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection1(firstSet, secondSet, tolerance, manifoldOnly, multiple=False):
    """
    Compute the Solid Intersection of two sets of Breps.

    Args:
        firstSet (IEnumerable<Brep>): First set of Breps.
        secondSet (IEnumerable<Brep>): Second set of Breps.
        tolerance (double): Tolerance to use for intersection operation.
        manifoldOnly (bool): If true, non-manifold input breps are ignored.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleanintersection-breparray_breparray_double_bool"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet, tolerance, manifoldOnly]
    if multiple: args = zip(firstSet, secondSet, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection2(firstBrep, secondBrep, tolerance, multiple=False):
    """
    Compute the Solid Intersection of two Breps.

    Args:
        firstBrep (Brep): First Brep for boolean intersection.
        secondBrep (Brep): Second Brep for boolean intersection.
        tolerance (double): Tolerance to use for intersection operation.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleanintersection-brep_brep_double"
    if multiple: url += "?multiple=true"
    args = [firstBrep, secondBrep, tolerance]
    if multiple: args = zip(firstBrep, secondBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection3(firstBrep, secondBrep, tolerance, manifoldOnly, multiple=False):
    """
    Compute the Solid Intersection of two Breps.

    Args:
        firstBrep (Brep): First Brep for boolean intersection.
        secondBrep (Brep): Second Brep for boolean intersection.
        tolerance (double): Tolerance to use for intersection operation.
        manifoldOnly (bool): If true, non-manifold input breps are ignored.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleanintersection-brep_brep_double_bool"
    if multiple: url += "?multiple=true"
    args = [firstBrep, secondBrep, tolerance, manifoldOnly]
    if multiple: args = zip(firstBrep, secondBrep, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference(firstSet, secondSet, tolerance, multiple=False):
    """
    Compute the Solid Difference of two sets of Breps.

    Args:
        firstSet (IEnumerable<Brep>): First set of Breps (the set to subtract from).
        secondSet (IEnumerable<Brep>): Second set of Breps (the set to subtract).
        tolerance (double): Tolerance to use for difference operation.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleandifference-breparray_breparray_double"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet, tolerance]
    if multiple: args = zip(firstSet, secondSet, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference1(firstSet, secondSet, tolerance, manifoldOnly, multiple=False):
    """
    Compute the Solid Difference of two sets of Breps.

    Args:
        firstSet (IEnumerable<Brep>): First set of Breps (the set to subtract from).
        secondSet (IEnumerable<Brep>): Second set of Breps (the set to subtract).
        tolerance (double): Tolerance to use for difference operation.
        manifoldOnly (bool): If true, non-manifold input breps are ignored.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleandifference-breparray_breparray_double_bool"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet, tolerance, manifoldOnly]
    if multiple: args = zip(firstSet, secondSet, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference2(firstBrep, secondBrep, tolerance, multiple=False):
    """
    Compute the Solid Difference of two Breps.

    Args:
        firstBrep (Brep): First Brep for boolean difference.
        secondBrep (Brep): Second Brep for boolean difference.
        tolerance (double): Tolerance to use for difference operation.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleandifference-brep_brep_double"
    if multiple: url += "?multiple=true"
    args = [firstBrep, secondBrep, tolerance]
    if multiple: args = zip(firstBrep, secondBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference3(firstBrep, secondBrep, tolerance, manifoldOnly, multiple=False):
    """
    Compute the Solid Difference of two Breps.

    Args:
        firstBrep (Brep): First Brep for boolean difference.
        secondBrep (Brep): Second Brep for boolean difference.
        tolerance (double): Tolerance to use for difference operation.
        manifoldOnly (bool): If true, non-manifold input breps are ignored.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createbooleandifference-brep_brep_double_bool"
    if multiple: url += "?multiple=true"
    args = [firstBrep, secondBrep, tolerance, manifoldOnly]
    if multiple: args = zip(firstBrep, secondBrep, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateShell(brep, facesToRemove, distance, tolerance, multiple=False):
    """
    Creates a hollowed out shell from a solid Brep. Function only operates on simple, solid, manifold Breps.

    Args:
        brep (Brep): The solid Brep to shell.
        facesToRemove (IEnumerable<int>): The indices of the Brep faces to remove. These surfaces are removed and the remainder is offset inward, using the outer parts of the removed surfaces to join the inner and outer parts.
        distance (double): The distance, or thickness, for the shell. This is a signed distance value with respect to face normals and flipped faces.
        tolerance (double): The offset tolerane. When in doubt, use the document's absolute tolerance.

    Returns:
        Brep[]: An array of Brep results or None on failure.
    """
    url = "rhino/geometry/brep/createshell-brep_intarray_double_double"
    if multiple: url += "?multiple=true"
    args = [brep, facesToRemove, distance, tolerance]
    if multiple: args = zip(brep, facesToRemove, distance, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def JoinBreps(brepsToJoin, tolerance, multiple=False):
    """
    Joins the breps in the input array at any overlapping edges to form
    as few as possible resulting breps. There may be more than one brep in the result array.

    Args:
        brepsToJoin (IEnumerable<Brep>): A list, an array or any enumerable set of breps to join.
        tolerance (double): 3d distance tolerance for detecting overlapping edges.

    Returns:
        Brep[]: new joined breps on success, None on failure.
    """
    url = "rhino/geometry/brep/joinbreps-breparray_double"
    if multiple: url += "?multiple=true"
    args = [brepsToJoin, tolerance]
    if multiple: args = zip(brepsToJoin, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MergeBreps(brepsToMerge, tolerance, multiple=False):
    """
    Combines two or more breps into one. A merge is like a boolean union that keeps the inside pieces. This
    function creates non-manifold Breps which in general are unusual in Rhino. You may want to consider using
    JoinBreps or CreateBooleanUnion functions instead.

    Args:
        brepsToMerge (IEnumerable<Brep>): must contain more than one Brep.
        tolerance (double): the tolerance to use when merging.

    Returns:
        Brep: Single merged Brep on success. Null on error.
    """
    url = "rhino/geometry/brep/mergebreps-breparray_double"
    if multiple: url += "?multiple=true"
    args = [brepsToMerge, tolerance]
    if multiple: args = zip(brepsToMerge, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateContourCurves(brepToContour, contourStart, contourEnd, interval, multiple=False):
    """
    Constructs the contour curves for a brep at a specified interval.

    Args:
        brepToContour (Brep): A brep or polysurface.
        contourStart (Point3d): A point to start.
        contourEnd (Point3d): A point to use as the end.
        interval (double): The interaxial offset in world units.

    Returns:
        Curve[]: An array with intersected curves. This array can be empty.
    """
    url = "rhino/geometry/brep/createcontourcurves-brep_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [brepToContour, contourStart, contourEnd, interval]
    if multiple: args = zip(brepToContour, contourStart, contourEnd, interval)
    response = Util.ComputeFetch(url, args)
    return response


def CreateContourCurves1(brepToContour, sectionPlane, multiple=False):
    """
    Constructs the contour curves for a brep, using a slicing plane.

    Args:
        brepToContour (Brep): A brep or polysurface.
        sectionPlane (Plane): A plane.

    Returns:
        Curve[]: An array with intersected curves. This array can be empty.
    """
    url = "rhino/geometry/brep/createcontourcurves-brep_plane"
    if multiple: url += "?multiple=true"
    args = [brepToContour, sectionPlane]
    if multiple: args = zip(brepToContour, sectionPlane)
    response = Util.ComputeFetch(url, args)
    return response


def CreateCurvatureAnalysisMesh(brep, state, multiple=False):
    """
    Create an array of analysis meshes for the brep using the specified settings.
    Meshes aren't set on the brep.

    Args:
        state (Rhino.ApplicationSettings.CurvatureAnalysisSettingsState): CurvatureAnalysisSettingsState

    Returns:
        Mesh[]: True if meshes were created
    """
    url = "rhino/geometry/brep/createcurvatureanalysismesh-brep_rhino.applicationsettings.curvatureanalysissettingsstate"
    if multiple: url += "?multiple=true"
    args = [brep, state]
    if multiple: args = zip(brep, state)
    response = Util.ComputeFetch(url, args)
    return response


def GetRegions(thisBrep, multiple=False):
    """
    Gets an array containing all regions in this brep.

    Returns:
        BrepRegion[]: An array of regions in this brep. This array can be empty, but not null.
    """
    url = "rhino/geometry/brep/getregions-brep"
    if multiple: url += "?multiple=true"
    args = [thisBrep]
    if multiple: args = [[item] for item in thisBrep]
    response = Util.ComputeFetch(url, args)
    return response


def GetWireframe(thisBrep, density, multiple=False):
    """
    Constructs all the Wireframe curves for this Brep.

    Args:
        density (int): Wireframe density. Valid values range between -1 and 99.

    Returns:
        Curve[]: An array of Wireframe curves or None on failure.
    """
    url = "rhino/geometry/brep/getwireframe-brep_int"
    if multiple: url += "?multiple=true"
    args = [thisBrep, density]
    if multiple: args = zip(thisBrep, density)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint(thisBrep, testPoint, multiple=False):
    """
    Finds a point on the brep that is closest to testPoint.

    Args:
        testPoint (Point3d): Base point to project to brep.

    Returns:
        Point3d: The point on the Brep closest to testPoint or Point3d.Unset if the operation failed.
    """
    url = "rhino/geometry/brep/closestpoint-brep_point3d"
    if multiple: url += "?multiple=true"
    args = [thisBrep, testPoint]
    if multiple: args = zip(thisBrep, testPoint)
    response = Util.ComputeFetch(url, args)
    return response


def IsPointInside(thisBrep, point, tolerance, strictlyIn, multiple=False):
    """
    Determines if point is inside Brep.  This question only makes sense when
    the brep is a closed manifold.  This function does not not check for
    closed or manifold, so result is not valid in those cases.  Intersects
    a line through point with brep, finds the intersection point Q closest
    to point, and looks at face normal at Q.  If the point Q is on an edge
    or the intersection is not transverse at Q, then another line is used.

    Args:
        point (Point3d): 3d point to test.
        tolerance (double): 3d distance tolerance used for intersection and determining strict inclusion.
            A good default is RhinoMath.SqrtEpsilon.
        strictlyIn (bool): if true, point is in if inside brep by at least tolerance.
            if false, point is in if truly in or within tolerance of boundary.

    Returns:
        bool: True if point is in, False if not.
    """
    url = "rhino/geometry/brep/ispointinside-brep_point3d_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, point, tolerance, strictlyIn]
    if multiple: args = zip(thisBrep, point, tolerance, strictlyIn)
    response = Util.ComputeFetch(url, args)
    return response


def CapPlanarHoles(thisBrep, tolerance, multiple=False):
    """
    Returns a new Brep that is equivalent to this Brep with all planar holes capped.

    Args:
        tolerance (double): Tolerance to use for capping.

    Returns:
        Brep: New brep on success. None on error.
    """
    url = "rhino/geometry/brep/capplanarholes-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Join(thisBrep, otherBrep, tolerance, compact, multiple=False):
    """
    If any edges of this brep overlap edges of otherBrep, merge a copy of otherBrep into this
    brep joining all edges that overlap within tolerance.

    Args:
        otherBrep (Brep): Brep to be added to this brep.
        tolerance (double): 3d distance tolerance for detecting overlapping edges.
        compact (bool): if true, set brep flags and tolerances, remove unused faces and edges.

    Returns:
        bool: True if any edges were joined.
    """
    url = "rhino/geometry/brep/join-brep_brep_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, otherBrep, tolerance, compact]
    if multiple: args = zip(thisBrep, otherBrep, tolerance, compact)
    response = Util.ComputeFetch(url, args)
    return response


def JoinNakedEdges(thisBrep, tolerance, multiple=False):
    """
    Joins naked edge pairs within the same brep that overlap within tolerance.

    Args:
        tolerance (double): The tolerance value.

    Returns:
        int: number of joins made.
    """
    url = "rhino/geometry/brep/joinnakededges-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MergeCoplanarFaces(thisBrep, tolerance, multiple=False):
    """
    Merges adjacent coplanar faces into single faces.

    Args:
        tolerance (double): Tolerance for determining when edges are adjacent.
            When in doubt, use the document's ModelAbsoluteTolerance property.

    Returns:
        bool: True if faces were merged, False if no faces were merged.
    """
    url = "rhino/geometry/brep/mergecoplanarfaces-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MergeCoplanarFaces1(thisBrep, tolerance, angleTolerance, multiple=False):
    """
    Merges adjacent coplanar faces into single faces.

    Args:
        tolerance (double): Tolerance for determining when edges are adjacent.
            When in doubt, use the document's ModelAbsoluteTolerance property.
        angleTolerance (double): Angle tolerance, in radians, for determining when faces are parallel.
            When in doubt, use the document's ModelAngleToleranceRadians property.

    Returns:
        bool: True if faces were merged, False if no faces were merged.
    """
    url = "rhino/geometry/brep/mergecoplanarfaces-brep_double_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance, angleTolerance]
    if multiple: args = zip(thisBrep, tolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Split(thisBrep, splitter, intersectionTolerance, multiple=False):
    """
    Splits a Brep into pieces.

    Args:
        splitter (Brep): A splitting surface or polysurface.
        intersectionTolerance (double): The tolerance with which to compute intersections.

    Returns:
        Brep[]: A new array of breps. This array can be empty.
    """
    url = "rhino/geometry/brep/split-brep_brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, splitter, intersectionTolerance]
    if multiple: args = zip(thisBrep, splitter, intersectionTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Split1(thisBrep, splitter, intersectionTolerance, multiple=False):
    """
    Splits a Brep into pieces.

    Args:
        splitter (Brep): The splitting polysurface.
        intersectionTolerance (double): The tolerance with which to compute intersections.

    Returns:
        Brep[]: A new array of breps. This array can be empty.
        toleranceWasRaised (bool): set to True if the split failed at intersectionTolerance but succeeded
            when the tolerance was increased to twice intersectionTolerance.
    """
    url = "rhino/geometry/brep/split-brep_brep_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, splitter, intersectionTolerance]
    if multiple: args = zip(thisBrep, splitter, intersectionTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Trim(thisBrep, cutter, intersectionTolerance, multiple=False):
    """
    Trims a brep with an oriented cutter. The parts of the brep that lie inside
    (opposite the normal) of the cutter are retained while the parts to the
    outside (in the direction of the normal) are discarded.  If the Cutter is
    closed, then a connected component of the Brep that does not intersect the
    cutter is kept if and only if it is contained in the inside of cutter.
    That is the region bounded by cutter opposite from the normal of cutter,
    If cutter is not closed all these components are kept.

    Args:
        cutter (Brep): A cutting brep.
        intersectionTolerance (double): A tolerance value with which to compute intersections.

    Returns:
        Brep[]: This Brep is not modified, the trim results are returned in an array.
    """
    url = "rhino/geometry/brep/trim-brep_brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, cutter, intersectionTolerance]
    if multiple: args = zip(thisBrep, cutter, intersectionTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Trim1(thisBrep, cutter, intersectionTolerance, multiple=False):
    """
    Trims a Brep with an oriented cutter.  The parts of Brep that lie inside
    (opposite the normal) of the cutter are retained while the parts to the
    outside ( in the direction of the normal ) are discarded. A connected
    component of Brep that does not intersect the cutter is kept if and only
    if it is contained in the inside of Cutter.  That is the region bounded by
    cutter opposite from the normal of cutter, or in the case of a Plane cutter
    the halfspace opposite from the plane normal.

    Args:
        cutter (Plane): A cutting plane.
        intersectionTolerance (double): A tolerance value with which to compute intersections.

    Returns:
        Brep[]: This Brep is not modified, the trim results are returned in an array.
    """
    url = "rhino/geometry/brep/trim-brep_plane_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, cutter, intersectionTolerance]
    if multiple: args = zip(thisBrep, cutter, intersectionTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def UnjoinEdges(thisBrep, edgesToUnjoin, multiple=False):
    """
    Unjoins, or separates, edges within the Brep. Note, seams in closed surfaces will not separate.

    Args:
        edgesToUnjoin (IEnumerable<int>): The indices of the Brep edges to unjoin.

    Returns:
        Brep[]: This Brep is not modified, the trim results are returned in an array.
    """
    url = "rhino/geometry/brep/unjoinedges-brep_intarray"
    if multiple: url += "?multiple=true"
    args = [thisBrep, edgesToUnjoin]
    if multiple: args = zip(thisBrep, edgesToUnjoin)
    response = Util.ComputeFetch(url, args)
    return response


def JoinEdges(thisBrep, edgeIndex0, edgeIndex1, joinTolerance, compact, multiple=False):
    """
    Joins two naked edges, or edges that are coincident or close together.

    Args:
        edgeIndex0 (int): The first edge index.
        edgeIndex1 (int): The second edge index.
        joinTolerance (double): The join tolerance.
        compact (bool): If joining more than one edge pair and want the edge indices of subsequent pairs to remain valid,
            set to false. But then call Brep.Compact() on the final result.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/brep/joinedges-brep_int_int_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, edgeIndex0, edgeIndex1, joinTolerance, compact]
    if multiple: args = zip(thisBrep, edgeIndex0, edgeIndex1, joinTolerance, compact)
    response = Util.ComputeFetch(url, args)
    return response


def TransformComponent(thisBrep, components, xform, tolerance, timeLimit, useMultipleThreads, multiple=False):
    """
    Transform an array of Brep components, bend neighbors to match, and leave the rest fixed.

    Args:
        components (IEnumerable<ComponentIndex>): The Brep components to transform.
        xform (Transform): The transformation to apply.
        tolerance (double): The desired fitting tolerance to use when bending faces that share edges with both fixed and transformed components.
        timeLimit (double): If the deformation is extreme, it can take a long time to calculate the result.
            If time_limit > 0, then the value specifies the maximum amount of time in seconds you want to spend before giving up.
        useMultipleThreads (bool): True if multiple threads can be used.

    Returns:
        bool: True if successful, False otherwise.
    """
    url = "rhino/geometry/brep/transformcomponent-brep_componentindexarray_transform_double_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, components, xform, tolerance, timeLimit, useMultipleThreads]
    if multiple: args = zip(thisBrep, components, xform, tolerance, timeLimit, useMultipleThreads)
    response = Util.ComputeFetch(url, args)
    return response


def GetArea(thisBrep, multiple=False):
    """
    Compute the Area of the Brep. If you want proper Area data with moments
    and error information, use the AreaMassProperties class.

    Returns:
        double: The area of the Brep.
    """
    url = "rhino/geometry/brep/getarea-brep"
    if multiple: url += "?multiple=true"
    args = [thisBrep]
    if multiple: args = [[item] for item in thisBrep]
    response = Util.ComputeFetch(url, args)
    return response


def GetArea1(thisBrep, relativeTolerance, absoluteTolerance, multiple=False):
    """
    Compute the Area of the Brep. If you want proper Area data with moments
    and error information, use the AreaMassProperties class.

    Args:
        relativeTolerance (double): Relative tolerance to use for area calculation.
        absoluteTolerance (double): Absolute tolerance to use for area calculation.

    Returns:
        double: The area of the Brep.
    """
    url = "rhino/geometry/brep/getarea-brep_double_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, relativeTolerance, absoluteTolerance]
    if multiple: args = zip(thisBrep, relativeTolerance, absoluteTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def GetVolume(thisBrep, multiple=False):
    """
    Compute the Volume of the Brep. If you want proper Volume data with moments
    and error information, use the VolumeMassProperties class.

    Returns:
        double: The volume of the Brep.
    """
    url = "rhino/geometry/brep/getvolume-brep"
    if multiple: url += "?multiple=true"
    args = [thisBrep]
    if multiple: args = [[item] for item in thisBrep]
    response = Util.ComputeFetch(url, args)
    return response


def GetVolume1(thisBrep, relativeTolerance, absoluteTolerance, multiple=False):
    """
    Compute the Volume of the Brep. If you want proper Volume data with moments
    and error information, use the VolumeMassProperties class.

    Args:
        relativeTolerance (double): Relative tolerance to use for area calculation.
        absoluteTolerance (double): Absolute tolerance to use for area calculation.

    Returns:
        double: The volume of the Brep.
    """
    url = "rhino/geometry/brep/getvolume-brep_double_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, relativeTolerance, absoluteTolerance]
    if multiple: args = zip(thisBrep, relativeTolerance, absoluteTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def RebuildTrimsForV2(thisBrep, face, nurbsSurface, multiple=False):
    """
    No support is available for this function.
    Expert user function used by MakeValidForV2 to convert trim
    curves from one surface to its NURBS form. After calling this function,
    you need to change the surface of the face to a NurbsSurface.

    Args:
        face (BrepFace): Face whose underlying surface has a parameterization that is different
            from its NURBS form.
        nurbsSurface (NurbsSurface): NURBS form of the face's underlying surface.
    """
    url = "rhino/geometry/brep/rebuildtrimsforv2-brep_brepface_nurbssurface"
    if multiple: url += "?multiple=true"
    args = [thisBrep, face, nurbsSurface]
    if multiple: args = zip(thisBrep, face, nurbsSurface)
    response = Util.ComputeFetch(url, args)
    return response


def MakeValidForV2(thisBrep, multiple=False):
    """
    No support is available for this function.
    Expert user function that converts all geometry in brep to nurbs form.
    """
    url = "rhino/geometry/brep/makevalidforv2-brep"
    if multiple: url += "?multiple=true"
    args = [thisBrep]
    if multiple: args = [[item] for item in thisBrep]
    response = Util.ComputeFetch(url, args)
    return response


def Repair(thisBrep, tolerance, multiple=False):
    """
    Fills in missing or fixes incorrect component information from a Brep.
    Useful when reading Brep information from other file formats that do not
    provide as complete of a Brep definition as requried by Rhino.

    Args:
        tolerance (double): The repair tolerance. When in doubt, use the document's model absolute tolerance.

    Returns:
        bool: True on success.
    """
    url = "rhino/geometry/brep/repair-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def RemoveHoles(thisBrep, tolerance, multiple=False):
    """
    Remove all inner loops, or holes, in a Brep.

    Args:
        tolerance (double): The tolerance. When in doubt, use the document's model absolute tolerance.

    Returns:
        Brep: The Brep without holes if successful, None otherwise.
    """
    url = "rhino/geometry/brep/removeholes-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def RemoveHoles1(thisBrep, loops, tolerance, multiple=False):
    """
    Removes inner loops, or holes, in a Brep.

    Args:
        loops (IEnumerable<ComponentIndex>): A list of BrepLoop component indexes, where BrepLoop.LoopType == Rhino.Geometry.BrepLoopType.Inner.
        tolerance (double): The tolerance. When in doubt, use the document's model absolute tolerance.

    Returns:
        Brep: The Brep without holes if successful, None otherwise.
    """
    url = "rhino/geometry/brep/removeholes-brep_componentindexarray_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, loops, tolerance]
    if multiple: args = zip(thisBrep, loops, tolerance)
    response = Util.ComputeFetch(url, args)
    return response

