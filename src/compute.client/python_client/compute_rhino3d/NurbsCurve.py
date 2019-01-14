from . import Util


def MakeCompatible(curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance, multiple=False):
    url = "rhino/geometry/nurbscurve/makecompatible-curvearray_point3d_point3d_int_int_double_double"
    if multiple: url += "?multiple=true"
    args = [curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance]
    if multiple: args = zip(curves, startPt, endPt, simplifyMethod, numPoints, refitTolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateParabolaFromVertex(vertex, startPoint, endPoint, multiple=False):
    url = "rhino/geometry/nurbscurve/createparabolafromvertex-point3d_point3d_point3d"
    if multiple: url += "?multiple=true"
    args = [vertex, startPoint, endPoint]
    if multiple: args = zip(vertex, startPoint, endPoint)
    response = Util.ComputeFetch(url, args)
    return response


def CreateParabolaFromFocus(focus, startPoint, endPoint, multiple=False):
    url = "rhino/geometry/nurbscurve/createparabolafromfocus-point3d_point3d_point3d"
    if multiple: url += "?multiple=true"
    args = [focus, startPoint, endPoint]
    if multiple: args = zip(focus, startPoint, endPoint)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromArc(arc, degree, cvCount, multiple=False):
    url = "rhino/geometry/nurbscurve/createfromarc-arc_int_int"
    if multiple: url += "?multiple=true"
    args = [arc, degree, cvCount]
    if multiple: args = zip(arc, degree, cvCount)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFromCircle(circle, degree, cvCount, multiple=False):
    url = "rhino/geometry/nurbscurve/createfromcircle-circle_int_int"
    if multiple: url += "?multiple=true"
    args = [circle, degree, cvCount]
    if multiple: args = zip(circle, degree, cvCount)
    response = Util.ComputeFetch(url, args)
    return response


def SetEndCondition(thisNurbsCurve, bSetEnd, continuity, point, tangent, multiple=False):
    url = "rhino/geometry/nurbscurve/setendcondition-nurbscurve_bool_nurbscurveendconditiontype_point3d_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, bSetEnd, continuity, point, tangent]
    if multiple: args = zip(thisNurbsCurve, bSetEnd, continuity, point, tangent)
    response = Util.ComputeFetch(url, args)
    return response


def SetEndCondition1(thisNurbsCurve, bSetEnd, continuity, point, tangent, curvature, multiple=False):
    url = "rhino/geometry/nurbscurve/setendcondition-nurbscurve_bool_nurbscurveendconditiontype_point3d_vector3d_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, bSetEnd, continuity, point, tangent, curvature]
    if multiple: args = zip(thisNurbsCurve, bSetEnd, continuity, point, tangent, curvature)
    response = Util.ComputeFetch(url, args)
    return response


def SetGrevillePoints(thisNurbsCurve, points, multiple=False):
    url = "rhino/geometry/nurbscurve/setgrevillepoints-nurbscurve_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisNurbsCurve, points]
    if multiple: args = zip(thisNurbsCurve, points)
    response = Util.ComputeFetch(url, args)
    return response


def CreateSpiral(axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1, multiple=False):
    url = "rhino/geometry/nurbscurve/createspiral-point3d_vector3d_point3d_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1]
    if multiple: args = zip(axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1)
    response = Util.ComputeFetch(url, args)
    return response


def CreateSpiral1(railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn, multiple=False):
    url = "rhino/geometry/nurbscurve/createspiral-curve_double_double_point3d_double_double_double_double_int"
    if multiple: url += "?multiple=true"
    args = [railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn]
    if multiple: args = zip(railCurve, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn)
    response = Util.ComputeFetch(url, args)
    return response

