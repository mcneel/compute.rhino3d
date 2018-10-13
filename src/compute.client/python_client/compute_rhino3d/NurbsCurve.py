import Util


def MakeCompatible(curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance):
    args = [curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/makecompatible-ienumerable<curve>_point3d_point3d_int_int_double_double", args)
    return response


def CreateParabolaFromVertex(vertex, startPoint, endPoint):
    args = [vertex, startPoint, endPoint]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/createparabolafromvertex-point3d_point3d_point3d", args)
    return response


def CreateParabolaFromFocus(focus, startPoint, endPoint):
    args = [focus, startPoint, endPoint]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/createparabolafromfocus-point3d_point3d_point3d", args)
    return response


def CreateFromArc(arc, degree, cvCount):
    args = [arc, degree, cvCount]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/createfromarc-arc_int_int", args)
    return response


def CreateFromCircle(circle, degree, cvCount):
    args = [circle, degree, cvCount]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/createfromcircle-circle_int_int", args)
    return response


def SetEndCondition(nurbscurve, bSetEnd, continuity, point, tangent):
    args = [nurbscurve, bSetEnd, continuity, point, tangent]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/setendcondition-nurbscurve_bool_nurbscurveendconditiontype_point3d_vector3d", args)
    return response


def SetEndCondition1(nurbscurve, bSetEnd, continuity, point, tangent, curvature):
    args = [nurbscurve, bSetEnd, continuity, point, tangent, curvature]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/setendcondition-nurbscurve_bool_nurbscurveendconditiontype_point3d_vector3d_vector3d", args)
    return response


def SetGrevillePoints(nurbscurve, points):
    args = [nurbscurve, points]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/setgrevillepoints-nurbscurve_ienumerable<point3d>", args)
    return response


def CreateSpiral(axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1):
    args = [axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/createspiral-point3d_vector3d_point3d_double_double_double_double", args)
    return response


def CreateSpiral1(railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn):
    args = [railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn]
    response = Util.ComputeFetch("rhino/geometry/nurbscurve/createspiral-curve_double_double_point3d_double_double_double_double_int", args)
    return response

