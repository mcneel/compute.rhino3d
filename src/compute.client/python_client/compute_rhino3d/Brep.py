import Util


def ChangeSeam(face, direction, parameter, tolerance):
    args = [face, direction, parameter, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/changeseam-brepface_int_double_double", args)
    return response


def CopyTrimCurves(trimSource, surfaceSource, tolerance):
    args = [trimSource, surfaceSource, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/copytrimcurves-brepface_surface_double", args)
    return response


def CreateBaseballSphere(center, radius, tolerance):
    args = [center, radius, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createbaseballsphere-point3d_double_double", args)
    return response


def CreateDevelopableLoft(crv0, crv1, reverse0, reverse1, density):
    args = [crv0, crv1, reverse0, reverse1, density]
    response = Util.ComputeFetch("rhino/geometry/brep/createdevelopableloft-curve_curve_bool_bool_int", args)
    return response


def CreateDevelopableLoft1(rail0, rail1, fixedRulings):
    args = [rail0, rail1, fixedRulings]
    response = Util.ComputeFetch("rhino/geometry/brep/createdevelopableloft-nurbscurve_nurbscurve_point2darray", args)
    return response


def CreatePlanarBreps(inputLoops):
    args = [inputLoops]
    response = Util.ComputeFetch("rhino/geometry/brep/createplanarbreps-curvearray", args)
    return response


def CreatePlanarBreps1(inputLoops, tolerance):
    args = [inputLoops, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createplanarbreps-curvearray_double", args)
    return response


def CreatePlanarBreps2(inputLoop):
    args = [inputLoop]
    response = Util.ComputeFetch("rhino/geometry/brep/createplanarbreps-curve", args)
    return response


def CreatePlanarBreps3(inputLoop, tolerance):
    args = [inputLoop, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createplanarbreps-curve_double", args)
    return response


def CreateTrimmedSurface(trimSource, surfaceSource):
    args = [trimSource, surfaceSource]
    response = Util.ComputeFetch("rhino/geometry/brep/createtrimmedsurface-brepface_surface", args)
    return response


def CreateTrimmedSurface1(trimSource, surfaceSource, tolerance):
    args = [trimSource, surfaceSource, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createtrimmedsurface-brepface_surface_double", args)
    return response


def CreateFromCornerPoints(corner1, corner2, corner3, tolerance):
    args = [corner1, corner2, corner3, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromcornerpoints-point3d_point3d_point3d_double", args)
    return response


def CreateFromCornerPoints1(corner1, corner2, corner3, corner4, tolerance):
    args = [corner1, corner2, corner3, corner4, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromcornerpoints-point3d_point3d_point3d_point3d_double", args)
    return response


def CreateEdgeSurface(curves):
    args = [curves]
    response = Util.ComputeFetch("rhino/geometry/brep/createedgesurface-curvearray", args)
    return response


def CreatePlanarBreps(inputLoops):
    args = [inputLoops]
    response = Util.ComputeFetch("rhino/geometry/brep/createplanarbreps-rhino.collections.curvelist", args)
    return response


def CreatePlanarBreps1(inputLoops, tolerance):
    args = [inputLoops, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createplanarbreps-rhino.collections.curvelist_double", args)
    return response


def CreateFromOffsetFace(face, offsetDistance, offsetTolerance, bothSides, createSolid):
    args = [face, offsetDistance, offsetTolerance, bothSides, createSolid]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromoffsetface-brepface_double_double_bool_bool", args)
    return response


def CreateSolid(breps, tolerance):
    args = [breps, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createsolid-breparray_double", args)
    return response


def MergeSurfaces(surface0, surface1, tolerance, angleToleranceRadians):
    args = [surface0, surface1, tolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/brep/mergesurfaces-surface_surface_double_double", args)
    return response


def MergeSurfaces1(brep0, brep1, tolerance, angleToleranceRadians):
    args = [brep0, brep1, tolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/brep/mergesurfaces-brep_brep_double_double", args)
    return response


def MergeSurfaces2(brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth):
    args = [brep0, brep1, tolerance, angleToleranceRadians, point0, point1, roundness, smooth]
    response = Util.ComputeFetch("rhino/geometry/brep/mergesurfaces-brep_brep_double_double_point2d_point2d_double_bool", args)
    return response


def CreatePatch(geometry, startingSurface, tolerance):
    args = [geometry, startingSurface, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createpatch-geometrybasearray_surface_double", args)
    return response


def CreatePatch1(geometry, uSpans, vSpans, tolerance):
    args = [geometry, uSpans, vSpans, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createpatch-geometrybasearray_int_int_double", args)
    return response


def CreatePatch2(geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance):
    args = [geometry, startingSurface, uSpans, vSpans, trim, tangency, pointSpacing, flexibility, surfacePull, fixEdges, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createpatch-geometrybasearray_surface_int_int_bool_bool_double_double_double_boolarray_double", args)
    return response


def CreatePipe(rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians):
    args = [rail, radius, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/brep/createpipe-curve_double_bool_pipecapmode_bool_double_double", args)
    return response


def CreatePipe1(rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians):
    args = [rail, railRadiiParameters, radii, localBlending, cap, fitRail, absoluteTolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/brep/createpipe-curve_doublearray_doublearray_bool_pipecapmode_bool_double_double", args)
    return response


def CreateFromSweep(rail, shape, closed, tolerance):
    args = [rail, shape, closed, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromsweep-curve_curve_bool_double", args)
    return response


def CreateFromSweep1(rail, shapes, closed, tolerance):
    args = [rail, shapes, closed, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromsweep-curve_curvearray_bool_double", args)
    return response


def CreateFromSweep2(rail1, rail2, shape, closed, tolerance):
    args = [rail1, rail2, shape, closed, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromsweep-curve_curve_curve_bool_double", args)
    return response


def CreateFromSweep3(rail1, rail2, shapes, closed, tolerance):
    args = [rail1, rail2, shapes, closed, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromsweep-curve_curve_curvearray_bool_double", args)
    return response


def CreateFromSweepInParts(rail1, rail2, shapes, rail_params, closed, tolerance):
    args = [rail1, rail2, shapes, rail_params, closed, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromsweepinparts-curve_curve_curvearray_point2darray_bool_double", args)
    return response


def CreateFromTaperedExtrude(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians):
    args = [curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType, tolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromtaperedextrude-curve_double_vector3d_point3d_double_extrudecornertype_double_double", args)
    return response


def CreateFromTaperedExtrude1(curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType):
    args = [curveToExtrude, distance, direction, basePoint, draftAngleRadians, cornerType]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromtaperedextrude-curve_double_vector3d_point3d_double_extrudecornertype", args)
    return response


def CreateBlendSurface(face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1):
    args = [face0, edge0, domain0, rev0, continuity0, face1, edge1, domain1, rev1, continuity1]
    response = Util.ComputeFetch("rhino/geometry/brep/createblendsurface-brepface_brepedge_interval_bool_blendcontinuity_brepface_brepedge_interval_bool_blendcontinuity", args)
    return response


def CreateBlendShape(face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1):
    args = [face0, edge0, t0, rev0, continuity0, face1, edge1, t1, rev1, continuity1]
    response = Util.ComputeFetch("rhino/geometry/brep/createblendshape-brepface_brepedge_double_bool_blendcontinuity_brepface_brepedge_double_bool_blendcontinuity", args)
    return response


def CreateFilletSurface(face0, uv0, face1, uv1, radius, extend, tolerance):
    args = [face0, uv0, face1, uv1, radius, extend, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfilletsurface-brepface_point2d_brepface_point2d_double_bool_double", args)
    return response


def CreateChamferSurface(face0, uv0, radius0, face1, uv1, radius1, extend, tolerance):
    args = [face0, uv0, radius0, face1, uv1, radius1, extend, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createchamfersurface-brepface_point2d_double_brepface_point2d_double_bool_double", args)
    return response


def CreateFilletEdges(brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance):
    args = [brep, edgeIndices, startRadii, endRadii, blendType, railType, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfilletedges-brep_intarray_doublearray_doublearray_blendtype_railtype_double", args)
    return response


def CreateFromJoinedEdges(brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance):
    args = [brep0, edgeIndex0, brep1, edgeIndex1, joinTolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromjoinededges-brep_int_brep_int_double", args)
    return response


def CreateFromLoft(curves, start, end, loftType, closed):
    args = [curves, start, end, loftType, closed]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromloft-curvearray_point3d_point3d_lofttype_bool", args)
    return response


def CreateFromLoftRebuild(curves, start, end, loftType, closed, rebuildPointCount):
    args = [curves, start, end, loftType, closed, rebuildPointCount]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromloftrebuild-curvearray_point3d_point3d_lofttype_bool_int", args)
    return response


def CreateFromLoftRefit(curves, start, end, loftType, closed, refitTolerance):
    args = [curves, start, end, loftType, closed, refitTolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createfromloftrefit-curvearray_point3d_point3d_lofttype_bool_double", args)
    return response


def CreateBooleanUnion(breps, tolerance):
    args = [breps, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleanunion-breparray_double", args)
    return response


def CreateBooleanUnion1(breps, tolerance, manifoldOnly):
    args = [breps, tolerance, manifoldOnly]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleanunion-breparray_double_bool", args)
    return response


def CreateBooleanIntersection(firstSet, secondSet, tolerance):
    args = [firstSet, secondSet, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleanintersection-breparray_breparray_double", args)
    return response


def CreateBooleanIntersection1(firstSet, secondSet, tolerance, manifoldOnly):
    args = [firstSet, secondSet, tolerance, manifoldOnly]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleanintersection-breparray_breparray_double_bool", args)
    return response


def CreateBooleanIntersection2(firstBrep, secondBrep, tolerance):
    args = [firstBrep, secondBrep, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleanintersection-brep_brep_double", args)
    return response


def CreateBooleanIntersection3(firstBrep, secondBrep, tolerance, manifoldOnly):
    args = [firstBrep, secondBrep, tolerance, manifoldOnly]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleanintersection-brep_brep_double_bool", args)
    return response


def CreateBooleanDifference(firstSet, secondSet, tolerance):
    args = [firstSet, secondSet, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleandifference-breparray_breparray_double", args)
    return response


def CreateBooleanDifference1(firstSet, secondSet, tolerance, manifoldOnly):
    args = [firstSet, secondSet, tolerance, manifoldOnly]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleandifference-breparray_breparray_double_bool", args)
    return response


def CreateBooleanDifference2(firstBrep, secondBrep, tolerance):
    args = [firstBrep, secondBrep, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleandifference-brep_brep_double", args)
    return response


def CreateBooleanDifference3(firstBrep, secondBrep, tolerance, manifoldOnly):
    args = [firstBrep, secondBrep, tolerance, manifoldOnly]
    response = Util.ComputeFetch("rhino/geometry/brep/createbooleandifference-brep_brep_double_bool", args)
    return response


def CreateShell(brep, facesToRemove, distance, tolerance):
    args = [brep, facesToRemove, distance, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/createshell-brep_intarray_double_double", args)
    return response


def JoinBreps(brepsToJoin, tolerance):
    args = [brepsToJoin, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/joinbreps-breparray_double", args)
    return response


def MergeBreps(brepsToMerge, tolerance):
    args = [brepsToMerge, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/mergebreps-breparray_double", args)
    return response


def CreateContourCurves(brepToContour, contourStart, contourEnd, interval):
    args = [brepToContour, contourStart, contourEnd, interval]
    response = Util.ComputeFetch("rhino/geometry/brep/createcontourcurves-brep_point3d_point3d_double", args)
    return response


def CreateContourCurves1(brepToContour, sectionPlane):
    args = [brepToContour, sectionPlane]
    response = Util.ComputeFetch("rhino/geometry/brep/createcontourcurves-brep_plane", args)
    return response


def CreateCurvatureAnalysisMesh(brep, state):
    args = [brep, state]
    response = Util.ComputeFetch("rhino/geometry/brep/createcurvatureanalysismesh-brep_rhino.applicationsettings.curvatureanalysissettingsstate", args)
    return response


def GetRegions(brep):
    args = [brep]
    response = Util.ComputeFetch("rhino/geometry/brep/getregions-brep", args)
    return response


def GetWireframe(brep, density):
    args = [brep, density]
    response = Util.ComputeFetch("rhino/geometry/brep/getwireframe-brep_int", args)
    return response


def ClosestPoint(brep, testPoint):
    args = [brep, testPoint]
    response = Util.ComputeFetch("rhino/geometry/brep/closestpoint-brep_point3d", args)
    return response


def IsPointInside(brep, point, tolerance, strictlyIn):
    args = [brep, point, tolerance, strictlyIn]
    response = Util.ComputeFetch("rhino/geometry/brep/ispointinside-brep_point3d_double_bool", args)
    return response


def CapPlanarHoles(brep, tolerance):
    args = [brep, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/capplanarholes-brep_double", args)
    return response


def Join(brep, otherBrep, tolerance, compact):
    args = [brep, otherBrep, tolerance, compact]
    response = Util.ComputeFetch("rhino/geometry/brep/join-brep_brep_double_bool", args)
    return response


def JoinNakedEdges(brep, tolerance):
    args = [brep, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/joinnakededges-brep_double", args)
    return response


def MergeCoplanarFaces(brep, tolerance):
    args = [brep, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/mergecoplanarfaces-brep_double", args)
    return response


def MergeCoplanarFaces1(brep, tolerance, angleTolerance):
    args = [brep, tolerance, angleTolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/mergecoplanarfaces-brep_double_double", args)
    return response


def Split(brep, splitter, intersectionTolerance):
    args = [brep, splitter, intersectionTolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/split-brep_brep_double", args)
    return response


def Split1(brep, splitter, intersectionTolerance, toleranceWasRaised):
    args = [brep, splitter, intersectionTolerance, toleranceWasRaised]
    response = Util.ComputeFetch("rhino/geometry/brep/split-brep_brep_double_bool", args)
    return response


def Trim(brep, cutter, intersectionTolerance):
    args = [brep, cutter, intersectionTolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/trim-brep_brep_double", args)
    return response


def Trim1(brep, cutter, intersectionTolerance):
    args = [brep, cutter, intersectionTolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/trim-brep_plane_double", args)
    return response


def UnjoinEdges(brep, edgesToUnjoin):
    args = [brep, edgesToUnjoin]
    response = Util.ComputeFetch("rhino/geometry/brep/unjoinedges-brep_intarray", args)
    return response


def JoinEdges(brep, edgeIndex0, edgeIndex1, joinTolerance, compact):
    args = [brep, edgeIndex0, edgeIndex1, joinTolerance, compact]
    response = Util.ComputeFetch("rhino/geometry/brep/joinedges-brep_int_int_double_bool", args)
    return response


def TransformComponent(brep, components, xform, tolerance, timeLimit, useMultipleThreads):
    args = [brep, components, xform, tolerance, timeLimit, useMultipleThreads]
    response = Util.ComputeFetch("rhino/geometry/brep/transformcomponent-brep_componentindexarray_transform_double_double_bool", args)
    return response


def GetArea(brep):
    args = [brep]
    response = Util.ComputeFetch("rhino/geometry/brep/getarea-brep", args)
    return response


def GetArea1(brep, relativeTolerance, absoluteTolerance):
    args = [brep, relativeTolerance, absoluteTolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/getarea-brep_double_double", args)
    return response


def GetVolume(brep):
    args = [brep]
    response = Util.ComputeFetch("rhino/geometry/brep/getvolume-brep", args)
    return response


def GetVolume1(brep, relativeTolerance, absoluteTolerance):
    args = [brep, relativeTolerance, absoluteTolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/getvolume-brep_double_double", args)
    return response


def RebuildTrimsForV2(brep, face, nurbsSurface):
    args = [brep, face, nurbsSurface]
    response = Util.ComputeFetch("rhino/geometry/brep/rebuildtrimsforv2-brep_brepface_nurbssurface", args)
    return response


def MakeValidForV2(brep):
    args = [brep]
    response = Util.ComputeFetch("rhino/geometry/brep/makevalidforv2-brep", args)
    return response


def Repair(brep, tolerance):
    args = [brep, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/repair-brep_double", args)
    return response


def RemoveHoles(brep, tolerance):
    args = [brep, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/removeholes-brep_double", args)
    return response


def RemoveHoles1(brep, loops, tolerance):
    args = [brep, loops, tolerance]
    response = Util.ComputeFetch("rhino/geometry/brep/removeholes-brep_componentindexarray_double", args)
    return response

