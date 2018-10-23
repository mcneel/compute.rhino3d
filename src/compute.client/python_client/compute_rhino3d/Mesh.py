import Util


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


def Volume(mesh):
    args = [mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/volume-mesh", args)
    return response


def Smooth(mesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem):
    args = [mesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem]
    response = Util.ComputeFetch("rhino/geometry/mesh/smooth-mesh_double_bool_bool_bool_bool_smoothingcoordinatesystem", args)
    return response


def Smooth1(mesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane):
    args = [mesh, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/smooth-mesh_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane", args)
    return response


def Smooth2(mesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane):
    args = [mesh, vertexIndices, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/smooth-mesh_intarray_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane", args)
    return response


def Unweld(mesh, angleToleranceRadians, modifyNormals):
    args = [mesh, angleToleranceRadians, modifyNormals]
    response = Util.ComputeFetch("rhino/geometry/mesh/unweld-mesh_double_bool", args)
    return response


def UnweldEdge(mesh, edgeIndices, modifyNormals):
    args = [mesh, edgeIndices, modifyNormals]
    response = Util.ComputeFetch("rhino/geometry/mesh/unweldedge-mesh_intarray_bool", args)
    return response


def Weld(mesh, angleToleranceRadians):
    args = [mesh, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/mesh/weld-mesh_double", args)
    return response


def RebuildNormals(mesh):
    args = [mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/rebuildnormals-mesh", args)
    return response


def ExtractNonManifoldEdges(mesh, selective):
    args = [mesh, selective]
    response = Util.ComputeFetch("rhino/geometry/mesh/extractnonmanifoldedges-mesh_bool", args)
    return response


def HealNakedEdges(mesh, distance):
    args = [mesh, distance]
    response = Util.ComputeFetch("rhino/geometry/mesh/healnakededges-mesh_double", args)
    return response


def FillHoles(mesh):
    args = [mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/fillholes-mesh", args)
    return response


def FileHole(mesh, topologyEdgeIndex):
    args = [mesh, topologyEdgeIndex]
    response = Util.ComputeFetch("rhino/geometry/mesh/filehole-mesh_int", args)
    return response


def UnifyNormals(mesh):
    args = [mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/unifynormals-mesh", args)
    return response


def UnifyNormals1(mesh, countOnly):
    args = [mesh, countOnly]
    response = Util.ComputeFetch("rhino/geometry/mesh/unifynormals-mesh_bool", args)
    return response


def SplitDisjointPieces(mesh):
    args = [mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/splitdisjointpieces-mesh", args)
    return response


def Split(mesh, plane):
    args = [mesh, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/split-mesh_plane", args)
    return response


def Split1(mesh, mesh):
    args = [mesh, mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/split-mesh_mesh", args)
    return response


def Split2(mesh, meshes):
    args = [mesh, meshes]
    response = Util.ComputeFetch("rhino/geometry/mesh/split-mesh_mesharray", args)
    return response


def GetOutlines(mesh, plane):
    args = [mesh, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/getoutlines-mesh_plane", args)
    return response


def GetOutlines1(mesh, viewport):
    args = [mesh, viewport]
    response = Util.ComputeFetch("rhino/geometry/mesh/getoutlines-mesh_display.rhinoviewport", args)
    return response


def GetOutlines2(mesh, viewportInfo, plane):
    args = [mesh, viewportInfo, plane]
    response = Util.ComputeFetch("rhino/geometry/mesh/getoutlines-mesh_viewportinfo_plane", args)
    return response


def GetNakedEdges(mesh):
    args = [mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/getnakededges-mesh", args)
    return response


def ExplodeAtUnweldedEdges(mesh):
    args = [mesh]
    response = Util.ComputeFetch("rhino/geometry/mesh/explodeatunweldededges-mesh", args)
    return response


def ClosestPoint(mesh, testPoint):
    args = [mesh, testPoint]
    response = Util.ComputeFetch("rhino/geometry/mesh/closestpoint-mesh_point3d", args)
    return response


def ClosestMeshPoint(mesh, testPoint, maximumDistance):
    args = [mesh, testPoint, maximumDistance]
    response = Util.ComputeFetch("rhino/geometry/mesh/closestmeshpoint-mesh_point3d_double", args)
    return response


def ClosestPoint(mesh, testPoint, pointOnMesh, maximumDistance):
    args = [mesh, testPoint, pointOnMesh, maximumDistance]
    response = Util.ComputeFetch("rhino/geometry/mesh/closestpoint-mesh_point3d_point3d_double", args)
    return response


def PointAt(mesh, meshPoint):
    args = [mesh, meshPoint]
    response = Util.ComputeFetch("rhino/geometry/mesh/pointat-mesh_meshpoint", args)
    return response


def PointAt1(mesh, faceIndex, t0, t1, t2, t3):
    args = [mesh, faceIndex, t0, t1, t2, t3]
    response = Util.ComputeFetch("rhino/geometry/mesh/pointat-mesh_int_double_double_double_double", args)
    return response


def NormalAt(mesh, meshPoint):
    args = [mesh, meshPoint]
    response = Util.ComputeFetch("rhino/geometry/mesh/normalat-mesh_meshpoint", args)
    return response


def NormalAt1(mesh, faceIndex, t0, t1, t2, t3):
    args = [mesh, faceIndex, t0, t1, t2, t3]
    response = Util.ComputeFetch("rhino/geometry/mesh/normalat-mesh_int_double_double_double_double", args)
    return response


def ColorAt(mesh, meshPoint):
    args = [mesh, meshPoint]
    response = Util.ComputeFetch("rhino/geometry/mesh/colorat-mesh_meshpoint", args)
    return response


def ColorAt1(mesh, faceIndex, t0, t1, t2, t3):
    args = [mesh, faceIndex, t0, t1, t2, t3]
    response = Util.ComputeFetch("rhino/geometry/mesh/colorat-mesh_int_double_double_double_double", args)
    return response


def PullPointsToMesh(mesh, points):
    args = [mesh, points]
    response = Util.ComputeFetch("rhino/geometry/mesh/pullpointstomesh-mesh_point3darray", args)
    return response


def Offset(mesh, distance):
    args = [mesh, distance]
    response = Util.ComputeFetch("rhino/geometry/mesh/offset-mesh_double", args)
    return response


def Offset1(mesh, distance, solidify):
    args = [mesh, distance, solidify]
    response = Util.ComputeFetch("rhino/geometry/mesh/offset-mesh_double_bool", args)
    return response


def Offset2(mesh, distance, solidify, direction):
    args = [mesh, distance, solidify, direction]
    response = Util.ComputeFetch("rhino/geometry/mesh/offset-mesh_double_bool_vector3d", args)
    return response


def CollapseFacesByEdgeLength(mesh, bGreaterThan, edgeLength):
    args = [mesh, bGreaterThan, edgeLength]
    response = Util.ComputeFetch("rhino/geometry/mesh/collapsefacesbyedgelength-mesh_bool_double", args)
    return response


def CollapseFacesByArea(mesh, lessThanArea, greaterThanArea):
    args = [mesh, lessThanArea, greaterThanArea]
    response = Util.ComputeFetch("rhino/geometry/mesh/collapsefacesbyarea-mesh_double_double", args)
    return response


def CollapseFacesByByAspectRatio(mesh, aspectRatio):
    args = [mesh, aspectRatio]
    response = Util.ComputeFetch("rhino/geometry/mesh/collapsefacesbybyaspectratio-mesh_double", args)
    return response


def GetUnsafeLock(mesh, writable):
    args = [mesh, writable]
    response = Util.ComputeFetch("rhino/geometry/mesh/getunsafelock-mesh_bool", args)
    return response


def ReleaseUnsafeLock(mesh, meshData):
    args = [mesh, meshData]
    response = Util.ComputeFetch("rhino/geometry/mesh/releaseunsafelock-mesh_meshunsafelock", args)
    return response


def WithShutLining(mesh, faceted, tolerance, curves):
    args = [mesh, faceted, tolerance, curves]
    response = Util.ComputeFetch("rhino/geometry/mesh/withshutlining-mesh_bool_double_shutliningcurveinfoarray", args)
    return response


def WithDisplacement(mesh, displacement):
    args = [mesh, displacement]
    response = Util.ComputeFetch("rhino/geometry/mesh/withdisplacement-mesh_meshdisplacementinfo", args)
    return response


def WithEdgeSoftening(mesh, softeningRadius, chamfer, faceted, force, angleThreshold):
    args = [mesh, softeningRadius, chamfer, faceted, force, angleThreshold]
    response = Util.ComputeFetch("rhino/geometry/mesh/withedgesoftening-mesh_double_bool_bool_bool_double", args)
    return response

