from . import Util


def CurvePlane(curve, plane, tolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/curveplane-curve_plane_double"
    if multiple: url += "?multiple=true"
    args = [curve, plane, tolerance]
    if multiple: args = zip(curve, plane, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MeshPlane(mesh, plane, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshplane-mesh_plane"
    if multiple: url += "?multiple=true"
    args = [mesh, plane]
    if multiple: args = zip(mesh, plane)
    response = Util.ComputeFetch(url, args)
    return response


def MeshPlane1(mesh, planes, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshplane-mesh_planearray"
    if multiple: url += "?multiple=true"
    args = [mesh, planes]
    if multiple: args = zip(mesh, planes)
    response = Util.ComputeFetch(url, args)
    return response


def CurveSelf(curve, tolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/curveself-curve_double"
    if multiple: url += "?multiple=true"
    args = [curve, tolerance]
    if multiple: args = zip(curve, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveCurve(curveA, curveB, tolerance, overlapTolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/curvecurve-curve_curve_double_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, tolerance, overlapTolerance]
    if multiple: args = zip(curveA, curveB, tolerance, overlapTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveLine(curve, line, tolerance, overlapTolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/curveline-curve_line_double_double"
    if multiple: url += "?multiple=true"
    args = [curve, line, tolerance, overlapTolerance]
    if multiple: args = zip(curve, line, tolerance, overlapTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveSurface(curve, surface, tolerance, overlapTolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/curvesurface-curve_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [curve, surface, tolerance, overlapTolerance]
    if multiple: args = zip(curve, surface, tolerance, overlapTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveSurface1(curve, curveDomain, surface, tolerance, overlapTolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/curvesurface-curve_interval_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [curve, curveDomain, surface, tolerance, overlapTolerance]
    if multiple: args = zip(curve, curveDomain, surface, tolerance, overlapTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CurveBrep(curve, brep, tolerance, angleTolerance, t, multiple=False):
    url = "rhino/geometry/intersect/intersection/curvebrep-curve_brep_double_double_doublearray"
    if multiple: url += "?multiple=true"
    args = [curve, brep, tolerance, angleTolerance, t]
    if multiple: args = zip(curve, brep, tolerance, angleTolerance, t)
    response = Util.ComputeFetch(url, args)
    return response


def MeshMeshFast(meshA, meshB, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshmeshfast-mesh_mesh"
    if multiple: url += "?multiple=true"
    args = [meshA, meshB]
    if multiple: args = zip(meshA, meshB)
    response = Util.ComputeFetch(url, args)
    return response


def MeshMesh(meshes, tolerance, mode, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshmesh-mesharray_double_setscombinations"
    if multiple: url += "?multiple=true"
    args = [meshes, tolerance, mode]
    if multiple: args = zip(meshes, tolerance, mode)
    response = Util.ComputeFetch(url, args)
    return response


def MeshMeshAccurate(meshA, meshB, tolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshmeshaccurate-mesh_mesh_double"
    if multiple: url += "?multiple=true"
    args = [meshA, meshB, tolerance]
    if multiple: args = zip(meshA, meshB, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def MeshRay(mesh, ray, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshray-mesh_ray3d"
    if multiple: url += "?multiple=true"
    args = [mesh, ray]
    if multiple: args = zip(mesh, ray)
    response = Util.ComputeFetch(url, args)
    return response


def MeshRay1(mesh, ray, meshFaceIndices, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshray-mesh_ray3d_intarray"
    if multiple: url += "?multiple=true"
    args = [mesh, ray, meshFaceIndices]
    if multiple: args = zip(mesh, ray, meshFaceIndices)
    response = Util.ComputeFetch(url, args)
    return response


def MeshPolyline(mesh, curve, faceIds, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshpolyline-mesh_polylinecurve_intarray"
    if multiple: url += "?multiple=true"
    args = [mesh, curve, faceIds]
    if multiple: args = zip(mesh, curve, faceIds)
    response = Util.ComputeFetch(url, args)
    return response


def MeshLine(mesh, line, faceIds, multiple=False):
    url = "rhino/geometry/intersect/intersection/meshline-mesh_line_intarray"
    if multiple: url += "?multiple=true"
    args = [mesh, line, faceIds]
    if multiple: args = zip(mesh, line, faceIds)
    response = Util.ComputeFetch(url, args)
    return response


def RayShoot(ray, geometry, maxReflections, multiple=False):
    url = "rhino/geometry/intersect/intersection/rayshoot-ray3d_geometrybasearray_int"
    if multiple: url += "?multiple=true"
    args = [ray, geometry, maxReflections]
    if multiple: args = zip(ray, geometry, maxReflections)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectPointsToMeshes(meshes, points, direction, tolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/projectpointstomeshes-mesharray_point3darray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [meshes, points, direction, tolerance]
    if multiple: args = zip(meshes, points, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectPointsToMeshesEx(meshes, points, direction, tolerance, indices, multiple=False):
    url = "rhino/geometry/intersect/intersection/projectpointstomeshesex-mesharray_point3darray_vector3d_double_intarray"
    if multiple: url += "?multiple=true"
    args = [meshes, points, direction, tolerance, indices]
    if multiple: args = zip(meshes, points, direction, tolerance, indices)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectPointsToBreps(breps, points, direction, tolerance, multiple=False):
    url = "rhino/geometry/intersect/intersection/projectpointstobreps-breparray_point3darray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [breps, points, direction, tolerance]
    if multiple: args = zip(breps, points, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectPointsToBrepsEx(breps, points, direction, tolerance, indices, multiple=False):
    url = "rhino/geometry/intersect/intersection/projectpointstobrepsex-breparray_point3darray_vector3d_double_intarray"
    if multiple: url += "?multiple=true"
    args = [breps, points, direction, tolerance, indices]
    if multiple: args = zip(breps, points, direction, tolerance, indices)
    response = Util.ComputeFetch(url, args)
    return response

