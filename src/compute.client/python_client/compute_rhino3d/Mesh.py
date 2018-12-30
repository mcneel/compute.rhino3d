from . import Util


def CreateFromPlane(plane, xInterval, yInterval, xCount, yCount):
    args = [plane, xInterval, yInterval, xCount, yCount]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromplane-plane_interval_interval_int_int", args)
    return response


def CreateFromBox(box, xCount, yCount, zCount):
    args = [box, xCount, yCount, zCount]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfrombox-boundingbox_int_int_int", args)
    return response


def CreateFromBox1(box, xCount, yCount, zCount):
    args = [box, xCount, yCount, zCount]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfrombox-box_int_int_int", args)
    return response


def CreateFromBox2(corners, xCount, yCount, zCount):
    args = [corners, xCount, yCount, zCount]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfrombox-point3darray_int_int_int", args)
    return response


def CreateFromSphere(sphere, xCount, yCount):
    args = [sphere, xCount, yCount]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromsphere-sphere_int_int", args)
    return response


def CreateIcoSphere(sphere, subdivisions):
    args = [sphere, subdivisions]
    response = Util.ComputeFetch("rhino/geometry/mesh/createicosphere-sphere_int", args)
    return response


def CreateQuadSphere(sphere, subdivisions):
    args = [sphere, subdivisions]
    response = Util.ComputeFetch("rhino/geometry/mesh/createquadsphere-sphere_int", args)
    return response


def CreateFromCylinder(cylinder, vertical, around):
    args = [cylinder, vertical, around]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromcylinder-cylinder_int_int", args)
    return response


def CreateFromCone(cone, vertical, around):
    args = [cone, vertical, around]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromcone-cone_int_int", args)
    return response


def CreateFromCone1(cone, vertical, around, solid):
    args = [cone, vertical, around, solid]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromcone-cone_int_int_bool", args)
    return response


def CreateFromPlanarBoundary(boundary, parameters):
    args = [boundary, parameters]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromplanarboundary-curve_meshingparameters", args)
    return response


def CreateFromPlanarBoundary1(boundary, parameters, tolerance):
    args = [boundary, parameters, tolerance]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromplanarboundary-curve_meshingparameters_double", args)
    return response


def CreateFromClosedPolyline(polyline):
    args = [polyline]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromclosedpolyline-polyline", args)
    return response


def CreateFromTessellation(points, edges, plane, allowNewVertices):
    args = [points, edges, plane, allowNewVertices]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromtessellation-point3darray_ienumerable<point3d>array_plane_bool", args)
    return response


def CreateFromBrep(brep):
    args = [brep]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfrombrep-brep", args)
    return response


def CreateFromBrep1(brep, meshingParameters):
    args = [brep, meshingParameters]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfrombrep-brep_meshingparameters", args)
    return response


def CreateFromSurface(surface):
    args = [surface]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromsurface-surface", args)
    return response


def CreateFromSurface1(surface, meshingParameters):
    args = [surface, meshingParameters]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromsurface-surface_meshingparameters", args)
    return response


def CreatePatch(outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions):
    args = [outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions]
    response = Util.ComputeFetch("rhino/geometry/mesh/createpatch-polyline_double_surface_curvearray_curvearray_point3darray_bool_int", args)
    return response


def CreateBooleanUnion(meshes):
    args = [meshes]
    response = Util.ComputeFetch("rhino/geometry/mesh/createbooleanunion-mesharray", args)
    return response


def CreateBooleanDifference(firstSet, secondSet):
    args = [firstSet, secondSet]
    response = Util.ComputeFetch("rhino/geometry/mesh/createbooleandifference-mesharray_mesharray", args)
    return response


def CreateBooleanIntersection(firstSet, secondSet):
    args = [firstSet, secondSet]
    response = Util.ComputeFetch("rhino/geometry/mesh/createbooleanintersection-mesharray_mesharray", args)
    return response


def CreateBooleanSplit(meshesToSplit, meshSplitters):
    args = [meshesToSplit, meshSplitters]
    response = Util.ComputeFetch("rhino/geometry/mesh/createbooleansplit-mesharray_mesharray", args)
    return response


def CreateFromCurvePipe(curve, radius, segments, accuracy, capType, faceted, intervals):
    args = [curve, radius, segments, accuracy, capType, faceted, intervals]
    response = Util.ComputeFetch("rhino/geometry/mesh/createfromcurvepipe-curve_double_int_int_meshpipecapstyle_bool_intervalarray", args)
    return response


def Volume(thisMesh):
    args = [thisMesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/volume-mesh", args)
    return response


def Smooth(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem):
    args = [thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem]
    response = Util.ComputeFetch("rhino/geometry/mesh/smooth-mesh_double_bool_bool_bool_bool_smoothingcoordinatesystem", args)
    return response


def Smooth1(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane):
    args = [thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/smooth-mesh_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane", args)
    return response


def Smooth2(thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane):
    args = [thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/smooth-mesh_intarray_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane", args)
    return response


def Unweld(thisMesh, angleToleranceRadians, modifyNormals):
    args = [thisMesh, angleToleranceRadians, modifyNormals]
    response = Util.ComputeFetch("rhino/geometry/mesh/unweld-mesh_double_bool", args)
    return response


def UnweldEdge(thisMesh, edgeIndices, modifyNormals):
    args = [thisMesh, edgeIndices, modifyNormals]
    response = Util.ComputeFetch("rhino/geometry/mesh/unweldedge-mesh_intarray_bool", args)
    return response


def Weld(thisMesh, angleToleranceRadians):
    args = [thisMesh, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/mesh/weld-mesh_double", args)
    return response


def RebuildNormals(thisMesh):
    args = [thisMesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/rebuildnormals-mesh", args)
    return response


def ExtractNonManifoldEdges(thisMesh, selective):
    args = [thisMesh, selective]
    response = Util.ComputeFetch("rhino/geometry/mesh/extractnonmanifoldedges-mesh_bool", args)
    return response


def HealNakedEdges(thisMesh, distance):
    args = [thisMesh, distance]
    response = Util.ComputeFetch("rhino/geometry/mesh/healnakededges-mesh_double", args)
    return response


def FillHoles(thisMesh):
    args = [thisMesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/fillholes-mesh", args)
    return response


def FileHole(thisMesh, topologyEdgeIndex):
    args = [thisMesh, topologyEdgeIndex]
    response = Util.ComputeFetch("rhino/geometry/mesh/filehole-mesh_int", args)
    return response


def UnifyNormals(thisMesh):
    args = [thisMesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/unifynormals-mesh", args)
    return response


def UnifyNormals1(thisMesh, countOnly):
    args = [thisMesh, countOnly]
    response = Util.ComputeFetch("rhino/geometry/mesh/unifynormals-mesh_bool", args)
    return response


def SplitDisjointPieces(thisMesh):
    args = [thisMesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/splitdisjointpieces-mesh", args)
    return response


def Split(thisMesh, plane):
    args = [thisMesh, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/split-mesh_plane", args)
    return response


def Split1(thisMesh, mesh):
    args = [thisMesh, mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/split-mesh_mesh", args)
    return response


def Split2(thisMesh, meshes):
    args = [thisMesh, meshes]
    response = Util.ComputeFetch("rhino/geometry/mesh/split-mesh_mesharray", args)
    return response


def GetOutlines(thisMesh, plane):
    args = [thisMesh, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/getoutlines-mesh_plane", args)
    return response


def GetOutlines1(thisMesh, viewport):
    args = [thisMesh, viewport]
    response = Util.ComputeFetch("rhino/geometry/mesh/getoutlines-mesh_display.rhinoviewport", args)
    return response


def GetOutlines2(thisMesh, viewportInfo, plane):
    args = [thisMesh, viewportInfo, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/getoutlines-mesh_viewportinfo_plane", args)
    return response


def GetNakedEdges(thisMesh):
    args = [thisMesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/getnakededges-mesh", args)
    return response


def ExplodeAtUnweldedEdges(thisMesh):
    args = [thisMesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/explodeatunweldededges-mesh", args)
    return response


def ClosestPoint(thisMesh, testPoint):
    args = [thisMesh, testPoint]
    response = Util.ComputeFetch("rhino/geometry/mesh/closestpoint-mesh_point3d", args)
    return response


def ClosestMeshPoint(thisMesh, testPoint, maximumDistance):
    args = [thisMesh, testPoint, maximumDistance]
    response = Util.ComputeFetch("rhino/geometry/mesh/closestmeshpoint-mesh_point3d_double", args)
    return response


def ClosestPoint(thisMesh, testPoint, pointOnMesh, maximumDistance):
    args = [thisMesh, testPoint, pointOnMesh, maximumDistance]
    response = Util.ComputeFetch("rhino/geometry/mesh/closestpoint-mesh_point3d_point3d_double", args)
    return response


def PointAt(thisMesh, meshPoint):
    args = [thisMesh, meshPoint]
    response = Util.ComputeFetch("rhino/geometry/mesh/pointat-mesh_meshpoint", args)
    return response


def PointAt1(thisMesh, faceIndex, t0, t1, t2, t3):
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    response = Util.ComputeFetch("rhino/geometry/mesh/pointat-mesh_int_double_double_double_double", args)
    return response


def NormalAt(thisMesh, meshPoint):
    args = [thisMesh, meshPoint]
    response = Util.ComputeFetch("rhino/geometry/mesh/normalat-mesh_meshpoint", args)
    return response


def NormalAt1(thisMesh, faceIndex, t0, t1, t2, t3):
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    response = Util.ComputeFetch("rhino/geometry/mesh/normalat-mesh_int_double_double_double_double", args)
    return response


def ColorAt(thisMesh, meshPoint):
    args = [thisMesh, meshPoint]
    response = Util.ComputeFetch("rhino/geometry/mesh/colorat-mesh_meshpoint", args)
    return response


def ColorAt1(thisMesh, faceIndex, t0, t1, t2, t3):
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    response = Util.ComputeFetch("rhino/geometry/mesh/colorat-mesh_int_double_double_double_double", args)
    return response


def PullPointsToMesh(thisMesh, points):
    args = [thisMesh, points]
    response = Util.ComputeFetch("rhino/geometry/mesh/pullpointstomesh-mesh_point3darray", args)
    return response


def Offset(thisMesh, distance):
    args = [thisMesh, distance]
    response = Util.ComputeFetch("rhino/geometry/mesh/offset-mesh_double", args)
    return response


def Offset1(thisMesh, distance, solidify):
    args = [thisMesh, distance, solidify]
    response = Util.ComputeFetch("rhino/geometry/mesh/offset-mesh_double_bool", args)
    return response


def Offset2(thisMesh, distance, solidify, direction):
    args = [thisMesh, distance, solidify, direction]
    response = Util.ComputeFetch("rhino/geometry/mesh/offset-mesh_double_bool_vector3d", args)
    return response


def CollapseFacesByEdgeLength(thisMesh, bGreaterThan, edgeLength):
    args = [thisMesh, bGreaterThan, edgeLength]
    response = Util.ComputeFetch("rhino/geometry/mesh/collapsefacesbyedgelength-mesh_bool_double", args)
    return response


def CollapseFacesByArea(thisMesh, lessThanArea, greaterThanArea):
    args = [thisMesh, lessThanArea, greaterThanArea]
    response = Util.ComputeFetch("rhino/geometry/mesh/collapsefacesbyarea-mesh_double_double", args)
    return response


def CollapseFacesByByAspectRatio(thisMesh, aspectRatio):
    args = [thisMesh, aspectRatio]
    response = Util.ComputeFetch("rhino/geometry/mesh/collapsefacesbybyaspectratio-mesh_double", args)
    return response


def GetUnsafeLock(thisMesh, writable):
    args = [thisMesh, writable]
    response = Util.ComputeFetch("rhino/geometry/mesh/getunsafelock-mesh_bool", args)
    return response


def ReleaseUnsafeLock(thisMesh, meshData):
    args = [thisMesh, meshData]
    response = Util.ComputeFetch("rhino/geometry/mesh/releaseunsafelock-mesh_meshunsafelock", args)
    return response


def WithShutLining(thisMesh, faceted, tolerance, curves):
    args = [thisMesh, faceted, tolerance, curves]
    response = Util.ComputeFetch("rhino/geometry/mesh/withshutlining-mesh_bool_double_shutliningcurveinfoarray", args)
    return response


def WithDisplacement(thisMesh, displacement):
    args = [thisMesh, displacement]
    response = Util.ComputeFetch("rhino/geometry/mesh/withdisplacement-mesh_meshdisplacementinfo", args)
    return response


def WithEdgeSoftening(thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold):
    args = [thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold]
    response = Util.ComputeFetch("rhino/geometry/mesh/withedgesoftening-mesh_double_bool_bool_bool_double", args)
    return response


def Reduce(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize):
    args = [thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize]
    response = Util.ComputeFetch("rhino/geometry/mesh/reduce-mesh_int_bool_int_bool", args)
    return response


def Reduce1(thisMesh, parameters):
    args = [thisMesh, parameters]
    response = Util.ComputeFetch("rhino/geometry/mesh/reduce-mesh_reducemeshparameters", args)
    return response


def ComputeThickness(meshes, maximumThickness):
    args = [meshes, maximumThickness]
    response = Util.ComputeFetch("rhino/geometry/mesh/computethickness-mesharray_double", args)
    return response


def ComputeThickness1(meshes, maximumThickness, cancelToken):
    args = [meshes, maximumThickness, cancelToken]
    response = Util.ComputeFetch("rhino/geometry/mesh/computethickness-mesharray_double_system.threading.cancellationtoken", args)
    return response


def ComputeThickness2(meshes, maximumThickness, sharpAngle, cancelToken):
    args = [meshes, maximumThickness, sharpAngle, cancelToken]
    response = Util.ComputeFetch("rhino/geometry/mesh/computethickness-mesharray_double_double_system.threading.cancellationtoken", args)
    return response


def CreateContourCurves(meshToContour, contourStart, contourEnd, interval):
    args = [meshToContour, contourStart, contourEnd, interval]
    response = Util.ComputeFetch("rhino/geometry/mesh/createcontourcurves-mesh_point3d_point3d_double", args)
    return response


def CreateContourCurves1(meshToContour, sectionPlane):
    args = [meshToContour, sectionPlane]
    response = Util.ComputeFetch("rhino/geometry/mesh/createcontourcurves-mesh_plane", args)
    return response

