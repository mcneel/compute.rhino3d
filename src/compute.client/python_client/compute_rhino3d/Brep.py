from . import Util


def ChangeSeam(face, direction, parameter, tolerance, multiple=False):
    url = "rhino/geometry/brep/changeseam-brepface_int_double_double"
    if multiple: url += "?multiple=true"
    args = [face, direction, parameter, tolerance]
    if multiple: args = zip(face, direction, parameter, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CopyTrimCurves(trimSource, surfaceSource, tolerance, multiple=False):
    url = "rhino/geometry/brep/copytrimcurves-brepface_surface_double"
    if multiple: url += "?multiple=true"
    args = [trimSource, surfaceSource, tolerance]
    if multiple: args = zip(trimSource, surfaceSource, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBaseballSphere(center, radius, tolerance, multiple=False):
    url = "rhino/geometry/brep/createbaseballsphere-point3d_double_double"
    if multiple: url += "?multiple=true"
    args = [center, radius, tolerance]
    if multiple: args = zip(center, radius, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateDevelopableLoft(crv0, crv1, reverse0, reverse1, density, multiple=False):
    url = "rhino/geometry/brep/createdevelopableloft-curve_curve_bool_bool_int"
    if multiple: url += "?multiple=true"
    args = [crv0, crv1, reverse0, reverse1, density]
    if multiple: args = zip(crv0, crv1, reverse0, reverse1, density)
    response = Util.ComputeFetch(url, args)
    return response


def CreateDevelopableLoft1(rail0, rail1, fixedRulings, multiple=False):
    url = "rhino/geometry/brep/createdevelopableloft-nurbscurve_nurbscurve_point2darray"
    if multiple: url += "?multiple=true"
    args = [rail0, rail1, fixedRulings]
    if multiple: args = zip(rail0, rail1, fixedRulings)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps(inputLoops, multiple=False):
    url = "rhino/geometry/brep/createplanarbreps-curvearray"
    if multiple: url += "?multiple=true"
    args = [inputLoops]
    if multiple: args = zip(inputLoops)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps1(inputLoops, tolerance, multiple=False):
    url = "rhino/geometry/brep/createplanarbreps-curvearray_double"
    if multiple: url += "?multiple=true"
    args = [inputLoops, tolerance]
    if multiple: args = zip(inputLoops, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps2(inputLoop, multiple=False):
    url = "rhino/geometry/brep/createplanarbreps-curve"
    if multiple: url += "?multiple=true"
    args = [inputLoop]
    if multiple: args = zip(inputLoop)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps3(inputLoop, tolerance, multiple=False):
    url = "rhino/geometry/brep/createplanarbreps-curve_double"
    if multiple: url += "?multiple=true"
    args = [inputLoop, tolerance]
    if multiple: args = zip(inputLoop, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTrimmedSurface(trimSource, surfaceSource, multiple=False):
    url = "rhino/geometry/brep/createtrimmedsurface-brepface_surface"
    if multiple: url += "?multiple=true"
    args = [trimSource, surfaceSource]
    if multiple: args = zip(trimSource, surfaceSource)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTrimmedSurface1(trimSource, surfaceSource, tolerance, multiple=False):
    url = "rhino/geometry/brep/createtrimmedsurface-brepface_surface_double"
    if multiple: url += "?multiple=true"
    args = [trimSource, surfaceSource, tolerance]
    if multiple: args = zip(trimSource, surfaceSource, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCornerPoints(corner1, corner2, corner3, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfromcornerpoints-point3d_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [corner1, corner2, corner3, tolerance]
    if multiple: args = zip(corner1, corner2, corner3, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCornerPoints1(corner1, corner2, corner3, corner4, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfromcornerpoints-point3d_point3d_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [corner1, corner2, corner3, corner4, tolerance]
    if multiple: args = zip(corner1, corner2, corner3, corner4, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateEdgeSurface(curves, multiple=False):
    url = "rhino/geometry/brep/createedgesurface-curvearray"
    if multiple: url += "?multiple=true"
    args = [curves]
    if multiple: args = zip(curves)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps(inputLoops, multiple=False):
    url = "rhino/geometry/brep/createplanarbreps-rhino.collections.curvelist"
    if multiple: url += "?multiple=true"
    args = [inputLoops]
    if multiple: args = zip(inputLoops)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePlanarBreps1(inputLoops, tolerance, multiple=False):
    url = "rhino/geometry/brep/createplanarbreps-rhino.collections.curvelist_double"
    if multiple: url += "?multiple=true"
    args = [inputLoops, tolerance]
    if multiple: args = zip(inputLoops, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromOffsetFace(face, offsetDistance, offsetTolerance, bothSides, createSolid, multiple=False):
    url = "rhino/geometry/brep/createfromoffsetface-brepface_double_double_bool_bool"
    if multiple: url += "?multiple=true"
    args = [face, offsetDistance, offsetTolerance, bothSides, createSolid]
    if multiple: args = zip(face, offsetDistance, offsetTolerance, bothSides, createSolid)
    response = Util.ComputeFetch(url, args)
    return response


def CreateSolid(breps, tolerance, multiple=False):
    url = "rhino/geometry/brep/createsolid-breparray_double"
    if multiple: url += "?multiple=true"
    args = [breps, tolerance]
    if multiple: args = zip(breps, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MergeSurfaces(surface0, surface1, tolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/brep/mergesurfaces-surface_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [surface0, surface1, tolerance, angleToleranceRadians]
    if multiple: args = zip(surface0, surface1, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def MergeSurfaces1(brep0, brep1, tolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/brep/mergesurfaces-brep_brep_double_double"
    if multiple: url += "?multiple=true"
    args = [brep0, brep1, tolerance, angleToleranceRadians]
    if multiple: args = zip(brep0, brep1, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def MergeSurfaces2(brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth, multiple=False):
    url = "rhino/geometry/brep/mergesurfaces-brep_brep_double_double_point2d_point2d_double_bool"
    if multiple: url += "?multiple=true"
    args = [brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth]
    if multiple: args = zip(brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePatch(geometry, startingSurface, tolerance, multiple=False):
    url = "rhino/geometry/brep/createpatch-geometrybasearray_surface_double"
    if multiple: url += "?multiple=true"
    args = [geometry, startingSurface, tolerance]
    if multiple: args = zip(geometry, startingSurface, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePatch1(geometry, uSpans, vSpans, tolerance, multiple=False):
    url = "rhino/geometry/brep/createpatch-geometrybasearray_int_int_double"
    if multiple: url += "?multiple=true"
    args = [geometry, uSpans, vSpans, tolerance]
    if multiple: args = zip(geometry, uSpans, vSpans, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePatch2(geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance, multiple=False):
    url = "rhino/geometry/brep/createpatch-geometrybasearray_surface_int_int_bool_bool_double_double_double_boolarray_double"
    if multiple: url += "?multiple=true"
    args = [geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance]
    if multiple: args = zip(geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePipe(rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/brep/createpipe-curve_double_bool_pipecapmode_bool_double_double"
    if multiple: url += "?multiple=true"
    args = [rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians]
    if multiple: args = zip(rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePipe1(rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/brep/createpipe-curve_doublearray_doublearray_bool_pipecapmode_bool_double_double"
    if multiple: url += "?multiple=true"
    args = [rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians]
    if multiple: args = zip(rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweep(rail, shape, closed, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfromsweep-curve_curve_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail, shape, closed, tolerance]
    if multiple: args = zip(rail, shape, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweep1(rail, shapes, closed, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfromsweep-curve_curvearray_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail, shapes, closed, tolerance]
    if multiple: args = zip(rail, shapes, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweep2(rail1, rail2, shape, closed, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfromsweep-curve_curve_curve_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail1, rail2, shape, closed, tolerance]
    if multiple: args = zip(rail1, rail2, shape, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweep3(rail1, rail2, shapes, closed, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfromsweep-curve_curve_curvearray_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail1, rail2, shapes, closed, tolerance]
    if multiple: args = zip(rail1, rail2, shapes, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSweepInParts(rail1, rail2, shapes, rail_params, closed, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfromsweepinparts-curve_curve_curvearray_point2darray_bool_double"
    if multiple: url += "?multiple=true"
    args = [rail1, rail2, shapes, rail_params, closed, tolerance]
    if multiple: args = zip(rail1, rail2, shapes, rail_params, closed, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromTaperedExtrude(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/brep/createfromtaperedextrude-curve_double_vector3d_point3d_double_extrudecornertype_double_double"
    if multiple: url += "?multiple=true"
    args = [curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians]
    if multiple: args = zip(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromTaperedExtrude1(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, multiple=False):
    url = "rhino/geometry/brep/createfromtaperedextrude-curve_double_vector3d_point3d_double_extrudecornertype"
    if multiple: url += "?multiple=true"
    args = [curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType]
    if multiple: args = zip(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendSurface(face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1, multiple=False):
    url = "rhino/geometry/brep/createblendsurface-brepface_brepedge_interval_bool_blendcontinuity_brepface_brepedge_interval_bool_blendcontinuity"
    if multiple: url += "?multiple=true"
    args = [face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1]
    if multiple: args = zip(face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendShape(face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1, multiple=False):
    url = "rhino/geometry/brep/createblendshape-brepface_brepedge_double_bool_blendcontinuity_brepface_brepedge_double_bool_blendcontinuity"
    if multiple: url += "?multiple=true"
    args = [face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1]
    if multiple: args = zip(face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFilletSurface(face0, uv0, face1, uv1, radius, extend, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfilletsurface-brepface_point2d_brepface_point2d_double_bool_double"
    if multiple: url += "?multiple=true"
    args = [face0, uv0, face1, uv1, radius, extend, tolerance]
    if multiple: args = zip(face0, uv0, face1, uv1, radius, extend, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateChamferSurface(face0, uv0, radius0, face1, uv1, radius1, extend, tolerance, multiple=False):
    url = "rhino/geometry/brep/createchamfersurface-brepface_point2d_double_brepface_point2d_double_bool_double"
    if multiple: url += "?multiple=true"
    args = [face0, uv0, radius0, face1, uv1, radius1, extend, tolerance]
    if multiple: args = zip(face0, uv0, radius0, face1, uv1, radius1, extend, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFilletEdges(brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance, multiple=False):
    url = "rhino/geometry/brep/createfilletedges-brep_intarray_doublearray_doublearray_blendtype_railtype_double"
    if multiple: url += "?multiple=true"
    args = [brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance]
    if multiple: args = zip(brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromJoinedEdges(brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance, multiple=False):
    url = "rhino/geometry/brep/createfromjoinededges-brep_int_brep_int_double"
    if multiple: url += "?multiple=true"
    args = [brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance]
    if multiple: args = zip(brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromLoft(curves, start, end, loftType, closed, multiple=False):
    url = "rhino/geometry/brep/createfromloft-curvearray_point3d_point3d_lofttype_bool"
    if multiple: url += "?multiple=true"
    args = [curves, start, end, loftType, closed]
    if multiple: args = zip(curves, start, end, loftType, closed)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromLoftRebuild(curves, start, end, loftType, closed, rebuildPointCount, multiple=False):
    url = "rhino/geometry/brep/createfromloftrebuild-curvearray_point3d_point3d_lofttype_bool_int"
    if multiple: url += "?multiple=true"
    args = [curves, start, end, loftType, closed, rebuildPointCount]
    if multiple: args = zip(curves, start, end, loftType, closed, rebuildPointCount)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromLoftRefit(curves, start, end, loftType, closed, refitTolerance, multiple=False):
    url = "rhino/geometry/brep/createfromloftrefit-curvearray_point3d_point3d_lofttype_bool_double"
    if multiple: url += "?multiple=true"
    args = [curves, start, end, loftType, closed, refitTolerance]
    if multiple: args = zip(curves, start, end, loftType, closed, refitTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion(breps, tolerance, multiple=False):
    url = "rhino/geometry/brep/createbooleanunion-breparray_double"
    if multiple: url += "?multiple=true"
    args = [breps, tolerance]
    if multiple: args = zip(breps, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion1(breps, tolerance, manifoldOnly, multiple=False):
    url = "rhino/geometry/brep/createbooleanunion-breparray_double_bool"
    if multiple: url += "?multiple=true"
    args = [breps, tolerance, manifoldOnly]
    if multiple: args = zip(breps, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection(firstSet, secondSet, tolerance, multiple=False):
    url = "rhino/geometry/brep/createbooleanintersection-breparray_breparray_double"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet, tolerance]
    if multiple: args = zip(firstSet, secondSet, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection1(firstSet, secondSet, tolerance, manifoldOnly, multiple=False):
    url = "rhino/geometry/brep/createbooleanintersection-breparray_breparray_double_bool"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet, tolerance, manifoldOnly]
    if multiple: args = zip(firstSet, secondSet, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection2(firstBrep, secondBrep, tolerance, multiple=False):
    url = "rhino/geometry/brep/createbooleanintersection-brep_brep_double"
    if multiple: url += "?multiple=true"
    args = [firstBrep, secondBrep, tolerance]
    if multiple: args = zip(firstBrep, secondBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection3(firstBrep, secondBrep, tolerance, manifoldOnly, multiple=False):
    url = "rhino/geometry/brep/createbooleanintersection-brep_brep_double_bool"
    if multiple: url += "?multiple=true"
    args = [firstBrep, secondBrep, tolerance, manifoldOnly]
    if multiple: args = zip(firstBrep, secondBrep, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference(firstSet, secondSet, tolerance, multiple=False):
    url = "rhino/geometry/brep/createbooleandifference-breparray_breparray_double"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet, tolerance]
    if multiple: args = zip(firstSet, secondSet, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference1(firstSet, secondSet, tolerance, manifoldOnly, multiple=False):
    url = "rhino/geometry/brep/createbooleandifference-breparray_breparray_double_bool"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet, tolerance, manifoldOnly]
    if multiple: args = zip(firstSet, secondSet, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference2(firstBrep, secondBrep, tolerance, multiple=False):
    url = "rhino/geometry/brep/createbooleandifference-brep_brep_double"
    if multiple: url += "?multiple=true"
    args = [firstBrep, secondBrep, tolerance]
    if multiple: args = zip(firstBrep, secondBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference3(firstBrep, secondBrep, tolerance, manifoldOnly, multiple=False):
    url = "rhino/geometry/brep/createbooleandifference-brep_brep_double_bool"
    if multiple: url += "?multiple=true"
    args = [firstBrep, secondBrep, tolerance, manifoldOnly]
    if multiple: args = zip(firstBrep, secondBrep, tolerance, manifoldOnly)
    response = Util.ComputeFetch(url, args)
    return response


def CreateShell(brep, facesToRemove, distance, tolerance, multiple=False):
    url = "rhino/geometry/brep/createshell-brep_intarray_double_double"
    if multiple: url += "?multiple=true"
    args = [brep, facesToRemove, distance, tolerance]
    if multiple: args = zip(brep, facesToRemove, distance, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def JoinBreps(brepsToJoin, tolerance, multiple=False):
    url = "rhino/geometry/brep/joinbreps-breparray_double"
    if multiple: url += "?multiple=true"
    args = [brepsToJoin, tolerance]
    if multiple: args = zip(brepsToJoin, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MergeBreps(brepsToMerge, tolerance, multiple=False):
    url = "rhino/geometry/brep/mergebreps-breparray_double"
    if multiple: url += "?multiple=true"
    args = [brepsToMerge, tolerance]
    if multiple: args = zip(brepsToMerge, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateContourCurves(brepToContour, contourStart, contourEnd, interval, multiple=False):
    url = "rhino/geometry/brep/createcontourcurves-brep_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [brepToContour, contourStart, contourEnd, interval]
    if multiple: args = zip(brepToContour, contourStart, contourEnd, interval)
    response = Util.ComputeFetch(url, args)
    return response


def CreateContourCurves1(brepToContour, sectionPlane, multiple=False):
    url = "rhino/geometry/brep/createcontourcurves-brep_plane"
    if multiple: url += "?multiple=true"
    args = [brepToContour, sectionPlane]
    if multiple: args = zip(brepToContour, sectionPlane)
    response = Util.ComputeFetch(url, args)
    return response


def CreateCurvatureAnalysisMesh(brep, state, multiple=False):
    url = "rhino/geometry/brep/createcurvatureanalysismesh-brep_rhino.applicationsettings.curvatureanalysissettingsstate"
    if multiple: url += "?multiple=true"
    args = [brep, state]
    if multiple: args = zip(brep, state)
    response = Util.ComputeFetch(url, args)
    return response


def GetRegions(thisBrep, multiple=False):
    url = "rhino/geometry/brep/getregions-brep"
    if multiple: url += "?multiple=true"
    args = [thisBrep]
    if multiple: args = zip(thisBrep)
    response = Util.ComputeFetch(url, args)
    return response


def GetWireframe(thisBrep, density, multiple=False):
    url = "rhino/geometry/brep/getwireframe-brep_int"
    if multiple: url += "?multiple=true"
    args = [thisBrep, density]
    if multiple: args = zip(thisBrep, density)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint(thisBrep, testPoint, multiple=False):
    url = "rhino/geometry/brep/closestpoint-brep_point3d"
    if multiple: url += "?multiple=true"
    args = [thisBrep, testPoint]
    if multiple: args = zip(thisBrep, testPoint)
    response = Util.ComputeFetch(url, args)
    return response


def IsPointInside(thisBrep, point, tolerance, strictlyIn, multiple=False):
    url = "rhino/geometry/brep/ispointinside-brep_point3d_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, point, tolerance, strictlyIn]
    if multiple: args = zip(thisBrep, point, tolerance, strictlyIn)
    response = Util.ComputeFetch(url, args)
    return response


def CapPlanarHoles(thisBrep, tolerance, multiple=False):
    url = "rhino/geometry/brep/capplanarholes-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Join(thisBrep, otherBrep, tolerance, compact, multiple=False):
    url = "rhino/geometry/brep/join-brep_brep_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, otherBrep, tolerance, compact]
    if multiple: args = zip(thisBrep, otherBrep, tolerance, compact)
    response = Util.ComputeFetch(url, args)
    return response


def JoinNakedEdges(thisBrep, tolerance, multiple=False):
    url = "rhino/geometry/brep/joinnakededges-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MergeCoplanarFaces(thisBrep, tolerance, multiple=False):
    url = "rhino/geometry/brep/mergecoplanarfaces-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MergeCoplanarFaces1(thisBrep, tolerance, angleTolerance, multiple=False):
    url = "rhino/geometry/brep/mergecoplanarfaces-brep_double_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance, angleTolerance]
    if multiple: args = zip(thisBrep, tolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Split(thisBrep, splitter, intersectionTolerance, multiple=False):
    url = "rhino/geometry/brep/split-brep_brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, splitter, intersectionTolerance]
    if multiple: args = zip(thisBrep, splitter, intersectionTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Split1(thisBrep, splitter, intersectionTolerance, toleranceWasRaised, multiple=False):
    url = "rhino/geometry/brep/split-brep_brep_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, splitter, intersectionTolerance, toleranceWasRaised]
    if multiple: args = zip(thisBrep, splitter, intersectionTolerance, toleranceWasRaised)
    response = Util.ComputeFetch(url, args)
    return response


def Trim(thisBrep, cutter, intersectionTolerance, multiple=False):
    url = "rhino/geometry/brep/trim-brep_brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, cutter, intersectionTolerance]
    if multiple: args = zip(thisBrep, cutter, intersectionTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Trim1(thisBrep, cutter, intersectionTolerance, multiple=False):
    url = "rhino/geometry/brep/trim-brep_plane_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, cutter, intersectionTolerance]
    if multiple: args = zip(thisBrep, cutter, intersectionTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def UnjoinEdges(thisBrep, edgesToUnjoin, multiple=False):
    url = "rhino/geometry/brep/unjoinedges-brep_intarray"
    if multiple: url += "?multiple=true"
    args = [thisBrep, edgesToUnjoin]
    if multiple: args = zip(thisBrep, edgesToUnjoin)
    response = Util.ComputeFetch(url, args)
    return response


def JoinEdges(thisBrep, edgeIndex0, edgeIndex1, joinTolerance, compact, multiple=False):
    url = "rhino/geometry/brep/joinedges-brep_int_int_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, edgeIndex0, edgeIndex1, joinTolerance, compact]
    if multiple: args = zip(thisBrep, edgeIndex0, edgeIndex1, joinTolerance, compact)
    response = Util.ComputeFetch(url, args)
    return response


def TransformComponent(thisBrep, components, xform, tolerance, timeLimit, useMultipleThreads, multiple=False):
    url = "rhino/geometry/brep/transformcomponent-brep_componentindexarray_transform_double_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisBrep, components, xform, tolerance, timeLimit, useMultipleThreads]
    if multiple: args = zip(thisBrep, components, xform, tolerance, timeLimit, useMultipleThreads)
    response = Util.ComputeFetch(url, args)
    return response


def GetArea(thisBrep, multiple=False):
    url = "rhino/geometry/brep/getarea-brep"
    if multiple: url += "?multiple=true"
    args = [thisBrep]
    if multiple: args = zip(thisBrep)
    response = Util.ComputeFetch(url, args)
    return response


def GetArea1(thisBrep, relativeTolerance, absoluteTolerance, multiple=False):
    url = "rhino/geometry/brep/getarea-brep_double_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, relativeTolerance, absoluteTolerance]
    if multiple: args = zip(thisBrep, relativeTolerance, absoluteTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def GetVolume(thisBrep, multiple=False):
    url = "rhino/geometry/brep/getvolume-brep"
    if multiple: url += "?multiple=true"
    args = [thisBrep]
    if multiple: args = zip(thisBrep)
    response = Util.ComputeFetch(url, args)
    return response


def GetVolume1(thisBrep, relativeTolerance, absoluteTolerance, multiple=False):
    url = "rhino/geometry/brep/getvolume-brep_double_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, relativeTolerance, absoluteTolerance]
    if multiple: args = zip(thisBrep, relativeTolerance, absoluteTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def RebuildTrimsForV2(thisBrep, face, nurbsSurface, multiple=False):
    url = "rhino/geometry/brep/rebuildtrimsforv2-brep_brepface_nurbssurface"
    if multiple: url += "?multiple=true"
    args = [thisBrep, face, nurbsSurface]
    if multiple: args = zip(thisBrep, face, nurbsSurface)
    response = Util.ComputeFetch(url, args)
    return response


def MakeValidForV2(thisBrep, multiple=False):
    url = "rhino/geometry/brep/makevalidforv2-brep"
    if multiple: url += "?multiple=true"
    args = [thisBrep]
    if multiple: args = zip(thisBrep)
    response = Util.ComputeFetch(url, args)
    return response


def Repair(thisBrep, tolerance, multiple=False):
    url = "rhino/geometry/brep/repair-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def RemoveHoles(thisBrep, tolerance, multiple=False):
    url = "rhino/geometry/brep/removeholes-brep_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, tolerance]
    if multiple: args = zip(thisBrep, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def RemoveHoles1(thisBrep, loops, tolerance, multiple=False):
    url = "rhino/geometry/brep/removeholes-brep_componentindexarray_double"
    if multiple: url += "?multiple=true"
    args = [thisBrep, loops, tolerance]
    if multiple: args = zip(thisBrep, loops, tolerance)
    response = Util.ComputeFetch(url, args)
    return response

