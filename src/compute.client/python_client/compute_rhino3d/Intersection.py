from . import Util


def CurvePlane(curve, plane, tolerance):
    args = [curve, plane, tolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/curveplane-curve_plane_double", args)
    return response


def MeshPlane(mesh, plane):
    args = [mesh, plane]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshplane-mesh_plane", args)
    return response


def MeshPlane1(mesh, planes):
    args = [mesh, planes]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshplane-mesh_planearray", args)
    return response


def CurveSelf(curve, tolerance):
    args = [curve, tolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/curveself-curve_double", args)
    return response


def CurveCurve(curveA, curveB, tolerance, overlapTolerance):
    args = [curveA, curveB, tolerance, overlapTolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/curvecurve-curve_curve_double_double", args)
    return response


def CurveLine(curve, line, tolerance, overlapTolerance):
    args = [curve, line, tolerance, overlapTolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/curveline-curve_line_double_double", args)
    return response


def CurveSurface(curve, surface, tolerance, overlapTolerance):
    args = [curve, surface, tolerance, overlapTolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/curvesurface-curve_surface_double_double", args)
    return response


def CurveSurface1(curve, curveDomain, surface, tolerance, overlapTolerance):
    args = [curve, curveDomain, surface, tolerance, overlapTolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/curvesurface-curve_interval_surface_double_double", args)
    return response


def CurveBrep(curve, brep, tolerance, angleTolerance, t):
    args = [curve, brep, tolerance, angleTolerance, t]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/curvebrep-curve_brep_double_double_doublearray", args)
    return response


def MeshMeshFast(meshA, meshB):
    args = [meshA, meshB]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshmeshfast-mesh_mesh", args)
    return response


def MeshMesh(meshes, tolerance, mode):
    args = [meshes, tolerance, mode]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshmesh-mesharray_double_setscombinations", args)
    return response


def MeshMeshAccurate(meshA, meshB, tolerance):
    args = [meshA, meshB, tolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshmeshaccurate-mesh_mesh_double", args)
    return response


def MeshRay(mesh, ray):
    args = [mesh, ray]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshray-mesh_ray3d", args)
    return response


def MeshRay1(mesh, ray, meshFaceIndices):
    args = [mesh, ray, meshFaceIndices]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshray-mesh_ray3d_intarray", args)
    return response


def MeshPolyline(mesh, curve, faceIds):
    args = [mesh, curve, faceIds]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshpolyline-mesh_polylinecurve_intarray", args)
    return response


def MeshLine(mesh, line, faceIds):
    args = [mesh, line, faceIds]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/meshline-mesh_line_intarray", args)
    return response


def RayShoot(ray, geometry, maxReflections):
    args = [ray, geometry, maxReflections]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/rayshoot-ray3d_geometrybasearray_int", args)
    return response


def ProjectPointsToMeshes(meshes, points, direction, tolerance):
    args = [meshes, points, direction, tolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/projectpointstomeshes-mesharray_point3darray_vector3d_double", args)
    return response


def ProjectPointsToMeshesEx(meshes, points, direction, tolerance, indices):
    args = [meshes, points, direction, tolerance, indices]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/projectpointstomeshesex-mesharray_point3darray_vector3d_double_intarray", args)
    return response


def ProjectPointsToBreps(breps, points, direction, tolerance):
    args = [breps, points, direction, tolerance]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/projectpointstobreps-breparray_point3darray_vector3d_double", args)
    return response


def ProjectPointsToBrepsEx(breps, points, direction, tolerance, indices):
    args = [breps, points, direction, tolerance, indices]
    response = Util.ComputeFetch("rhino/geometry/intersect/intersection/projectpointstobrepsex-breparray_point3darray_vector3d_double_intarray", args)
    return response

