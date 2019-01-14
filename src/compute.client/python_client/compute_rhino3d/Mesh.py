from . import Util


def CreateFromPlane(plane, xInterval, yInterval, xCount, yCount, multiple=False):
    url = "rhino/geometry/mesh/createfromplane-plane_interval_interval_int_int"
    if multiple: url += "?multiple=true"
    args = [plane, xInterval, yInterval, xCount, yCount]
    if multiple: args = zip(plane, xInterval, yInterval, xCount, yCount)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromBox(box, xCount, yCount, zCount, multiple=False):
    url = "rhino/geometry/mesh/createfrombox-boundingbox_int_int_int"
    if multiple: url += "?multiple=true"
    args = [box, xCount, yCount, zCount]
    if multiple: args = zip(box, xCount, yCount, zCount)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromBox1(box, xCount, yCount, zCount, multiple=False):
    url = "rhino/geometry/mesh/createfrombox-box_int_int_int"
    if multiple: url += "?multiple=true"
    args = [box, xCount, yCount, zCount]
    if multiple: args = zip(box, xCount, yCount, zCount)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromBox2(corners, xCount, yCount, zCount, multiple=False):
    url = "rhino/geometry/mesh/createfrombox-point3darray_int_int_int"
    if multiple: url += "?multiple=true"
    args = [corners, xCount, yCount, zCount]
    if multiple: args = zip(corners, xCount, yCount, zCount)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSphere(sphere, xCount, yCount, multiple=False):
    url = "rhino/geometry/mesh/createfromsphere-sphere_int_int"
    if multiple: url += "?multiple=true"
    args = [sphere, xCount, yCount]
    if multiple: args = zip(sphere, xCount, yCount)
    response = Util.ComputeFetch(url, args)
    return response


def CreateIcoSphere(sphere, subdivisions, multiple=False):
    url = "rhino/geometry/mesh/createicosphere-sphere_int"
    if multiple: url += "?multiple=true"
    args = [sphere, subdivisions]
    if multiple: args = zip(sphere, subdivisions)
    response = Util.ComputeFetch(url, args)
    return response


def CreateQuadSphere(sphere, subdivisions, multiple=False):
    url = "rhino/geometry/mesh/createquadsphere-sphere_int"
    if multiple: url += "?multiple=true"
    args = [sphere, subdivisions]
    if multiple: args = zip(sphere, subdivisions)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCylinder(cylinder, vertical, around, multiple=False):
    url = "rhino/geometry/mesh/createfromcylinder-cylinder_int_int"
    if multiple: url += "?multiple=true"
    args = [cylinder, vertical, around]
    if multiple: args = zip(cylinder, vertical, around)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCone(cone, vertical, around, multiple=False):
    url = "rhino/geometry/mesh/createfromcone-cone_int_int"
    if multiple: url += "?multiple=true"
    args = [cone, vertical, around]
    if multiple: args = zip(cone, vertical, around)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCone1(cone, vertical, around, solid, multiple=False):
    url = "rhino/geometry/mesh/createfromcone-cone_int_int_bool"
    if multiple: url += "?multiple=true"
    args = [cone, vertical, around, solid]
    if multiple: args = zip(cone, vertical, around, solid)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromPlanarBoundary(boundary, parameters, multiple=False):
    url = "rhino/geometry/mesh/createfromplanarboundary-curve_meshingparameters"
    if multiple: url += "?multiple=true"
    args = [boundary, parameters]
    if multiple: args = zip(boundary, parameters)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromPlanarBoundary1(boundary, parameters, tolerance, multiple=False):
    url = "rhino/geometry/mesh/createfromplanarboundary-curve_meshingparameters_double"
    if multiple: url += "?multiple=true"
    args = [boundary, parameters, tolerance]
    if multiple: args = zip(boundary, parameters, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromClosedPolyline(polyline, multiple=False):
    url = "rhino/geometry/mesh/createfromclosedpolyline-polyline"
    if multiple: url += "?multiple=true"
    args = [polyline]
    if multiple: args = zip(polyline)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromTessellation(points, edges, plane, allowNewVertices, multiple=False):
    url = "rhino/geometry/mesh/createfromtessellation-point3darray_ienumerable<point3d>array_plane_bool"
    if multiple: url += "?multiple=true"
    args = [points, edges, plane, allowNewVertices]
    if multiple: args = zip(points, edges, plane, allowNewVertices)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromBrep(brep, multiple=False):
    url = "rhino/geometry/mesh/createfrombrep-brep"
    if multiple: url += "?multiple=true"
    args = [brep]
    if multiple: args = zip(brep)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromBrep1(brep, meshingParameters, multiple=False):
    url = "rhino/geometry/mesh/createfrombrep-brep_meshingparameters"
    if multiple: url += "?multiple=true"
    args = [brep, meshingParameters]
    if multiple: args = zip(brep, meshingParameters)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSurface(surface, multiple=False):
    url = "rhino/geometry/mesh/createfromsurface-surface"
    if multiple: url += "?multiple=true"
    args = [surface]
    if multiple: args = zip(surface)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromSurface1(surface, meshingParameters, multiple=False):
    url = "rhino/geometry/mesh/createfromsurface-surface_meshingparameters"
    if multiple: url += "?multiple=true"
    args = [surface, meshingParameters]
    if multiple: args = zip(surface, meshingParameters)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePatch(outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions, multiple=False):
    url = "rhino/geometry/mesh/createpatch-polyline_double_surface_curvearray_curvearray_point3darray_bool_int"
    if multiple: url += "?multiple=true"
    args = [outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions]
    if multiple: args = zip(outerBoundary, angleToleranceRadians, pullbackSurface, innerBoundaryCurves, innerBothSideCurves, innerPoints, trimback, divisions)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion(meshes, multiple=False):
    url = "rhino/geometry/mesh/createbooleanunion-mesharray"
    if multiple: url += "?multiple=true"
    args = [meshes]
    if multiple: args = zip(meshes)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference(firstSet, secondSet, multiple=False):
    url = "rhino/geometry/mesh/createbooleandifference-mesharray_mesharray"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet]
    if multiple: args = zip(firstSet, secondSet)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection(firstSet, secondSet, multiple=False):
    url = "rhino/geometry/mesh/createbooleanintersection-mesharray_mesharray"
    if multiple: url += "?multiple=true"
    args = [firstSet, secondSet]
    if multiple: args = zip(firstSet, secondSet)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanSplit(meshesToSplit, meshSplitters, multiple=False):
    url = "rhino/geometry/mesh/createbooleansplit-mesharray_mesharray"
    if multiple: url += "?multiple=true"
    args = [meshesToSplit, meshSplitters]
    if multiple: args = zip(meshesToSplit, meshSplitters)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCurvePipe(curve, radius, segments, accuracy, capType, faceted, intervals, multiple=False):
    url = "rhino/geometry/mesh/createfromcurvepipe-curve_double_int_int_meshpipecapstyle_bool_intervalarray"
    if multiple: url += "?multiple=true"
    args = [curve, radius, segments, accuracy, capType, faceted, intervals]
    if multiple: args = zip(curve, radius, segments, accuracy, capType, faceted, intervals)
    response = Util.ComputeFetch(url, args)
    return response


def Volume(thisMesh, multiple=False):
    url = "rhino/geometry/mesh/volume-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = zip(thisMesh)
    response = Util.ComputeFetch(url, args)
    return response


def Smooth(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, multiple=False):
    url = "rhino/geometry/mesh/smooth-mesh_double_bool_bool_bool_bool_smoothingcoordinatesystem"
    if multiple: url += "?multiple=true"
    args = [thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem]
    if multiple: args = zip(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem)
    response = Util.ComputeFetch(url, args)
    return response


def Smooth1(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False):
    url = "rhino/geometry/mesh/smooth-mesh_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    if multiple: args = zip(thisMesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane)
    response = Util.ComputeFetch(url, args)
    return response


def Smooth2(thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False):
    url = "rhino/geometry/mesh/smooth-mesh_intarray_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    if multiple: args = zip(thisMesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane)
    response = Util.ComputeFetch(url, args)
    return response


def Unweld(thisMesh, angleToleranceRadians, modifyNormals, multiple=False):
    url = "rhino/geometry/mesh/unweld-mesh_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, angleToleranceRadians, modifyNormals]
    if multiple: args = zip(thisMesh, angleToleranceRadians, modifyNormals)
    response = Util.ComputeFetch(url, args)
    return response


def UnweldEdge(thisMesh, edgeIndices, modifyNormals, multiple=False):
    url = "rhino/geometry/mesh/unweldedge-mesh_intarray_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, edgeIndices, modifyNormals]
    if multiple: args = zip(thisMesh, edgeIndices, modifyNormals)
    response = Util.ComputeFetch(url, args)
    return response


def Weld(thisMesh, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/mesh/weld-mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, angleToleranceRadians]
    if multiple: args = zip(thisMesh, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def RebuildNormals(thisMesh, multiple=False):
    url = "rhino/geometry/mesh/rebuildnormals-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = zip(thisMesh)
    response = Util.ComputeFetch(url, args)
    return response


def ExtractNonManifoldEdges(thisMesh, selective, multiple=False):
    url = "rhino/geometry/mesh/extractnonmanifoldedges-mesh_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, selective]
    if multiple: args = zip(thisMesh, selective)
    response = Util.ComputeFetch(url, args)
    return response


def HealNakedEdges(thisMesh, distance, multiple=False):
    url = "rhino/geometry/mesh/healnakededges-mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance]
    if multiple: args = zip(thisMesh, distance)
    response = Util.ComputeFetch(url, args)
    return response


def FillHoles(thisMesh, multiple=False):
    url = "rhino/geometry/mesh/fillholes-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = zip(thisMesh)
    response = Util.ComputeFetch(url, args)
    return response


def FileHole(thisMesh, topologyEdgeIndex, multiple=False):
    url = "rhino/geometry/mesh/filehole-mesh_int"
    if multiple: url += "?multiple=true"
    args = [thisMesh, topologyEdgeIndex]
    if multiple: args = zip(thisMesh, topologyEdgeIndex)
    response = Util.ComputeFetch(url, args)
    return response


def UnifyNormals(thisMesh, multiple=False):
    url = "rhino/geometry/mesh/unifynormals-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = zip(thisMesh)
    response = Util.ComputeFetch(url, args)
    return response


def UnifyNormals1(thisMesh, countOnly, multiple=False):
    url = "rhino/geometry/mesh/unifynormals-mesh_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, countOnly]
    if multiple: args = zip(thisMesh, countOnly)
    response = Util.ComputeFetch(url, args)
    return response


def SplitDisjointPieces(thisMesh, multiple=False):
    url = "rhino/geometry/mesh/splitdisjointpieces-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = zip(thisMesh)
    response = Util.ComputeFetch(url, args)
    return response


def Split(thisMesh, plane, multiple=False):
    url = "rhino/geometry/mesh/split-mesh_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, plane]
    if multiple: args = zip(thisMesh, plane)
    response = Util.ComputeFetch(url, args)
    return response


def Split1(thisMesh, mesh, multiple=False):
    url = "rhino/geometry/mesh/split-mesh_mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh, mesh]
    if multiple: args = zip(thisMesh, mesh)
    response = Util.ComputeFetch(url, args)
    return response


def Split2(thisMesh, meshes, multiple=False):
    url = "rhino/geometry/mesh/split-mesh_mesharray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshes]
    if multiple: args = zip(thisMesh, meshes)
    response = Util.ComputeFetch(url, args)
    return response


def GetOutlines(thisMesh, plane, multiple=False):
    url = "rhino/geometry/mesh/getoutlines-mesh_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, plane]
    if multiple: args = zip(thisMesh, plane)
    response = Util.ComputeFetch(url, args)
    return response


def GetOutlines1(thisMesh, viewport, multiple=False):
    url = "rhino/geometry/mesh/getoutlines-mesh_display.rhinoviewport"
    if multiple: url += "?multiple=true"
    args = [thisMesh, viewport]
    if multiple: args = zip(thisMesh, viewport)
    response = Util.ComputeFetch(url, args)
    return response


def GetOutlines2(thisMesh, viewportInfo, plane, multiple=False):
    url = "rhino/geometry/mesh/getoutlines-mesh_viewportinfo_plane"
    if multiple: url += "?multiple=true"
    args = [thisMesh, viewportInfo, plane]
    if multiple: args = zip(thisMesh, viewportInfo, plane)
    response = Util.ComputeFetch(url, args)
    return response


def GetNakedEdges(thisMesh, multiple=False):
    url = "rhino/geometry/mesh/getnakededges-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = zip(thisMesh)
    response = Util.ComputeFetch(url, args)
    return response


def ExplodeAtUnweldedEdges(thisMesh, multiple=False):
    url = "rhino/geometry/mesh/explodeatunweldededges-mesh"
    if multiple: url += "?multiple=true"
    args = [thisMesh]
    if multiple: args = zip(thisMesh)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint(thisMesh, testPoint, multiple=False):
    url = "rhino/geometry/mesh/closestpoint-mesh_point3d"
    if multiple: url += "?multiple=true"
    args = [thisMesh, testPoint]
    if multiple: args = zip(thisMesh, testPoint)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestMeshPoint(thisMesh, testPoint, maximumDistance, multiple=False):
    url = "rhino/geometry/mesh/closestmeshpoint-mesh_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, testPoint, maximumDistance]
    if multiple: args = zip(thisMesh, testPoint, maximumDistance)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint(thisMesh, testPoint, pointOnMesh, maximumDistance, multiple=False):
    url = "rhino/geometry/mesh/closestpoint-mesh_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, testPoint, pointOnMesh, maximumDistance]
    if multiple: args = zip(thisMesh, testPoint, pointOnMesh, maximumDistance)
    response = Util.ComputeFetch(url, args)
    return response


def PointAt(thisMesh, meshPoint, multiple=False):
    url = "rhino/geometry/mesh/pointat-mesh_meshpoint"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshPoint]
    if multiple: args = zip(thisMesh, meshPoint)
    response = Util.ComputeFetch(url, args)
    return response


def PointAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False):
    url = "rhino/geometry/mesh/pointat-mesh_int_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    if multiple: args = zip(thisMesh, faceIndex, t0, t1, t2, t3)
    response = Util.ComputeFetch(url, args)
    return response


def NormalAt(thisMesh, meshPoint, multiple=False):
    url = "rhino/geometry/mesh/normalat-mesh_meshpoint"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshPoint]
    if multiple: args = zip(thisMesh, meshPoint)
    response = Util.ComputeFetch(url, args)
    return response


def NormalAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False):
    url = "rhino/geometry/mesh/normalat-mesh_int_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    if multiple: args = zip(thisMesh, faceIndex, t0, t1, t2, t3)
    response = Util.ComputeFetch(url, args)
    return response


def ColorAt(thisMesh, meshPoint, multiple=False):
    url = "rhino/geometry/mesh/colorat-mesh_meshpoint"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshPoint]
    if multiple: args = zip(thisMesh, meshPoint)
    response = Util.ComputeFetch(url, args)
    return response


def ColorAt1(thisMesh, faceIndex, t0, t1, t2, t3, multiple=False):
    url = "rhino/geometry/mesh/colorat-mesh_int_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceIndex, t0, t1, t2, t3]
    if multiple: args = zip(thisMesh, faceIndex, t0, t1, t2, t3)
    response = Util.ComputeFetch(url, args)
    return response


def PullPointsToMesh(thisMesh, points, multiple=False):
    url = "rhino/geometry/mesh/pullpointstomesh-mesh_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, points]
    if multiple: args = zip(thisMesh, points)
    response = Util.ComputeFetch(url, args)
    return response


def Offset(thisMesh, distance, multiple=False):
    url = "rhino/geometry/mesh/offset-mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance]
    if multiple: args = zip(thisMesh, distance)
    response = Util.ComputeFetch(url, args)
    return response


def Offset1(thisMesh, distance, solidify, multiple=False):
    url = "rhino/geometry/mesh/offset-mesh_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance, solidify]
    if multiple: args = zip(thisMesh, distance, solidify)
    response = Util.ComputeFetch(url, args)
    return response


def Offset2(thisMesh, distance, solidify, direction, multiple=False):
    url = "rhino/geometry/mesh/offset-mesh_double_bool_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisMesh, distance, solidify, direction]
    if multiple: args = zip(thisMesh, distance, solidify, direction)
    response = Util.ComputeFetch(url, args)
    return response


def CollapseFacesByEdgeLength(thisMesh, bGreaterThan, edgeLength, multiple=False):
    url = "rhino/geometry/mesh/collapsefacesbyedgelength-mesh_bool_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, bGreaterThan, edgeLength]
    if multiple: args = zip(thisMesh, bGreaterThan, edgeLength)
    response = Util.ComputeFetch(url, args)
    return response


def CollapseFacesByArea(thisMesh, lessThanArea, greaterThanArea, multiple=False):
    url = "rhino/geometry/mesh/collapsefacesbyarea-mesh_double_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, lessThanArea, greaterThanArea]
    if multiple: args = zip(thisMesh, lessThanArea, greaterThanArea)
    response = Util.ComputeFetch(url, args)
    return response


def CollapseFacesByByAspectRatio(thisMesh, aspectRatio, multiple=False):
    url = "rhino/geometry/mesh/collapsefacesbybyaspectratio-mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, aspectRatio]
    if multiple: args = zip(thisMesh, aspectRatio)
    response = Util.ComputeFetch(url, args)
    return response


def GetUnsafeLock(thisMesh, writable, multiple=False):
    url = "rhino/geometry/mesh/getunsafelock-mesh_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, writable]
    if multiple: args = zip(thisMesh, writable)
    response = Util.ComputeFetch(url, args)
    return response


def ReleaseUnsafeLock(thisMesh, meshData, multiple=False):
    url = "rhino/geometry/mesh/releaseunsafelock-mesh_meshunsafelock"
    if multiple: url += "?multiple=true"
    args = [thisMesh, meshData]
    if multiple: args = zip(thisMesh, meshData)
    response = Util.ComputeFetch(url, args)
    return response


def WithShutLining(thisMesh, faceted, tolerance, curves, multiple=False):
    url = "rhino/geometry/mesh/withshutlining-mesh_bool_double_shutliningcurveinfoarray"
    if multiple: url += "?multiple=true"
    args = [thisMesh, faceted, tolerance, curves]
    if multiple: args = zip(thisMesh, faceted, tolerance, curves)
    response = Util.ComputeFetch(url, args)
    return response


def WithDisplacement(thisMesh, displacement, multiple=False):
    url = "rhino/geometry/mesh/withdisplacement-mesh_meshdisplacementinfo"
    if multiple: url += "?multiple=true"
    args = [thisMesh, displacement]
    if multiple: args = zip(thisMesh, displacement)
    response = Util.ComputeFetch(url, args)
    return response


def WithEdgeSoftening(thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold, multiple=False):
    url = "rhino/geometry/mesh/withedgesoftening-mesh_double_bool_bool_bool_double"
    if multiple: url += "?multiple=true"
    args = [thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold]
    if multiple: args = zip(thisMesh, softeningRadius, chamfer, faceted, force, angleThreshold)
    response = Util.ComputeFetch(url, args)
    return response


def Reduce(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize, multiple=False):
    url = "rhino/geometry/mesh/reduce-mesh_int_bool_int_bool"
    if multiple: url += "?multiple=true"
    args = [thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize]
    if multiple: args = zip(thisMesh, desiredPolygonCount, allowDistortion, accuracy, normalizeSize)
    response = Util.ComputeFetch(url, args)
    return response


def Reduce1(thisMesh, parameters, multiple=False):
    url = "rhino/geometry/mesh/reduce-mesh_reducemeshparameters"
    if multiple: url += "?multiple=true"
    args = [thisMesh, parameters]
    if multiple: args = zip(thisMesh, parameters)
    response = Util.ComputeFetch(url, args)
    return response


def ComputeThickness(meshes, maximumThickness, multiple=False):
    url = "rhino/geometry/mesh/computethickness-mesharray_double"
    if multiple: url += "?multiple=true"
    args = [meshes, maximumThickness]
    if multiple: args = zip(meshes, maximumThickness)
    response = Util.ComputeFetch(url, args)
    return response


def ComputeThickness1(meshes, maximumThickness, cancelToken, multiple=False):
    url = "rhino/geometry/mesh/computethickness-mesharray_double_system.threading.cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [meshes, maximumThickness, cancelToken]
    if multiple: args = zip(meshes, maximumThickness, cancelToken)
    response = Util.ComputeFetch(url, args)
    return response


def ComputeThickness2(meshes, maximumThickness, sharpAngle, cancelToken, multiple=False):
    url = "rhino/geometry/mesh/computethickness-mesharray_double_double_system.threading.cancellationtoken"
    if multiple: url += "?multiple=true"
    args = [meshes, maximumThickness, sharpAngle, cancelToken]
    if multiple: args = zip(meshes, maximumThickness, sharpAngle, cancelToken)
    response = Util.ComputeFetch(url, args)
    return response


def CreateContourCurves(meshToContour, contourStart, contourEnd, interval, multiple=False):
    url = "rhino/geometry/mesh/createcontourcurves-mesh_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [meshToContour, contourStart, contourEnd, interval]
    if multiple: args = zip(meshToContour, contourStart, contourEnd, interval)
    response = Util.ComputeFetch(url, args)
    return response


def CreateContourCurves1(meshToContour, sectionPlane, multiple=False):
    url = "rhino/geometry/mesh/createcontourcurves-mesh_plane"
    if multiple: url += "?multiple=true"
    args = [meshToContour, sectionPlane]
    if multiple: args = zip(meshToContour, sectionPlane)
    response = Util.ComputeFetch(url, args)
    return response

