from . import Util


def GetConicSectionType(thisCurve, multiple=False):
    url = "rhino/geometry/curve/getconicsectiontype-curve"
    if multiple: url += "?multiple=true"
    args = [thisCurve]
    if multiple: args = zip(thisCurve)
    response = Util.ComputeFetch(url, args)
    return response


def CreateInterpolatedCurve(points, degree, multiple=False):
    url = "rhino/geometry/curve/createinterpolatedcurve-point3darray_int"
    if multiple: url += "?multiple=true"
    args = [points, degree]
    if multiple: args = zip(points, degree)
    response = Util.ComputeFetch(url, args)
    return response


def CreateInterpolatedCurve1(points, degree, knots, multiple=False):
    url = "rhino/geometry/curve/createinterpolatedcurve-point3darray_int_curveknotstyle"
    if multiple: url += "?multiple=true"
    args = [points, degree, knots]
    if multiple: args = zip(points, degree, knots)
    response = Util.ComputeFetch(url, args)
    return response


def CreateInterpolatedCurve2(points, degree, knots, startTangent, endTangent, multiple=False):
    url = "rhino/geometry/curve/createinterpolatedcurve-point3darray_int_curveknotstyle_vector3d_vector3d"
    if multiple: url += "?multiple=true"
    args = [points, degree, knots, startTangent, endTangent]
    if multiple: args = zip(points, degree, knots, startTangent, endTangent)
    response = Util.ComputeFetch(url, args)
    return response


def CreateSoftEditCurve(curve, t, delta, length, fixEnds, multiple=False):
    url = "rhino/geometry/curve/createsofteditcurve-curve_double_vector3d_double_bool"
    if multiple: url += "?multiple=true"
    args = [curve, t, delta, length, fixEnds]
    if multiple: args = zip(curve, t, delta, length, fixEnds)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFilletCornersCurve(curve, radius, tolerance, angleTolerance, multiple=False):
    url = "rhino/geometry/curve/createfilletcornerscurve-curve_double_double_double"
    if multiple: url += "?multiple=true"
    args = [curve, radius, tolerance, angleTolerance]
    if multiple: args = zip(curve, radius, tolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateArcBlend(startPt, startDir, endPt, endDir, controlPointLengthRatio, multiple=False):
    url = "rhino/geometry/curve/createarcblend-point3d_vector3d_point3d_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [startPt, startDir, endPt, endDir, controlPointLengthRatio]
    if multiple: args = zip(startPt, startDir, endPt, endDir, controlPointLengthRatio)
    response = Util.ComputeFetch(url, args)
    return response


def CreateMeanCurve(curveA, curveB, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/curve/createmeancurve-curve_curve_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, angleToleranceRadians]
    if multiple: args = zip(curveA, curveB, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def CreateMeanCurve1(curveA, curveB, multiple=False):
    url = "rhino/geometry/curve/createmeancurve-curve_curve"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB]
    if multiple: args = zip(curveA, curveB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendCurve(curveA, curveB, continuity, multiple=False):
    url = "rhino/geometry/curve/createblendcurve-curve_curve_blendcontinuity"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, continuity]
    if multiple: args = zip(curveA, curveB, continuity)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendCurve1(curveA, curveB, continuity, bulgeA, bulgeB, multiple=False):
    url = "rhino/geometry/curve/createblendcurve-curve_curve_blendcontinuity_double_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, continuity, bulgeA, bulgeB]
    if multiple: args = zip(curveA, curveB, continuity, bulgeA, bulgeB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBlendCurve2(curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1, multiple=False):
    url = "rhino/geometry/curve/createblendcurve-curve_double_bool_blendcontinuity_curve_double_bool_blendcontinuity"
    if multiple: url += "?multiple=true"
    args = [curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1]
    if multiple: args = zip(curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurves(curve0, curve1, numCurves, multiple=False):
    url = "rhino/geometry/curve/createtweencurves-curve_curve_int"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves]
    if multiple: args = zip(curve0, curve1, numCurves)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurves1(curve0, curve1, numCurves, tolerance, multiple=False):
    url = "rhino/geometry/curve/createtweencurves-curve_curve_int_double"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves, tolerance]
    if multiple: args = zip(curve0, curve1, numCurves, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurvesWithMatching(curve0, curve1, numCurves, multiple=False):
    url = "rhino/geometry/curve/createtweencurveswithmatching-curve_curve_int"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves]
    if multiple: args = zip(curve0, curve1, numCurves)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurvesWithMatching1(curve0, curve1, numCurves, tolerance, multiple=False):
    url = "rhino/geometry/curve/createtweencurveswithmatching-curve_curve_int_double"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves, tolerance]
    if multiple: args = zip(curve0, curve1, numCurves, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurvesWithSampling(curve0, curve1, numCurves, numSamples, multiple=False):
    url = "rhino/geometry/curve/createtweencurveswithsampling-curve_curve_int_int"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves, numSamples]
    if multiple: args = zip(curve0, curve1, numCurves, numSamples)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTweenCurvesWithSampling1(curve0, curve1, numCurves, numSamples, tolerance, multiple=False):
    url = "rhino/geometry/curve/createtweencurveswithsampling-curve_curve_int_int_double"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, numCurves, numSamples, tolerance]
    if multiple: args = zip(curve0, curve1, numCurves, numSamples, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def JoinCurves(inputCurves, multiple=False):
    url = "rhino/geometry/curve/joincurves-curvearray"
    if multiple: url += "?multiple=true"
    args = [inputCurves]
    if multiple: args = zip(inputCurves)
    response = Util.ComputeFetch(url, args)
    return response


def JoinCurves1(inputCurves, joinTolerance, multiple=False):
    url = "rhino/geometry/curve/joincurves-curvearray_double"
    if multiple: url += "?multiple=true"
    args = [inputCurves, joinTolerance]
    if multiple: args = zip(inputCurves, joinTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def JoinCurves2(inputCurves, joinTolerance, preserveDirection, multiple=False):
    url = "rhino/geometry/curve/joincurves-curvearray_double_bool"
    if multiple: url += "?multiple=true"
    args = [inputCurves, joinTolerance, preserveDirection]
    if multiple: args = zip(inputCurves, joinTolerance, preserveDirection)
    response = Util.ComputeFetch(url, args)
    return response


def MakeEndsMeet(curveA, adjustStartCurveA, curveB, adjustStartCurveB, multiple=False):
    url = "rhino/geometry/curve/makeendsmeet-curve_bool_curve_bool"
    if multiple: url += "?multiple=true"
    args = [curveA, adjustStartCurveA, curveB, adjustStartCurveB]
    if multiple: args = zip(curveA, adjustStartCurveA, curveB, adjustStartCurveB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFillet(curve0, curve1, radius, t0Base, t1Base, multiple=False):
    url = "rhino/geometry/curve/createfillet-curve_curve_double_double_double"
    if multiple: url += "?multiple=true"
    args = [curve0, curve1, radius, t0Base, t1Base]
    if multiple: args = zip(curve0, curve1, radius, t0Base, t1Base)
    response = Util.ComputeFetch(url, args)
    return response


def CreateFilletCurves(curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance, multiple=False):
    url = "rhino/geometry/curve/createfilletcurves-curve_point3d_curve_point3d_double_bool_bool_bool_double_double"
    if multiple: url += "?multiple=true"
    args = [curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance]
    if multiple: args = zip(curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion(curves, multiple=False):
    url = "rhino/geometry/curve/createbooleanunion-curvearray"
    if multiple: url += "?multiple=true"
    args = [curves]
    if multiple: args = zip(curves)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanUnion1(curves, tolerance, multiple=False):
    url = "rhino/geometry/curve/createbooleanunion-curvearray_double"
    if multiple: url += "?multiple=true"
    args = [curves, tolerance]
    if multiple: args = zip(curves, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection(curveA, curveB, multiple=False):
    url = "rhino/geometry/curve/createbooleanintersection-curve_curve"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB]
    if multiple: args = zip(curveA, curveB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanIntersection1(curveA, curveB, tolerance, multiple=False):
    url = "rhino/geometry/curve/createbooleanintersection-curve_curve_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, tolerance]
    if multiple: args = zip(curveA, curveB, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference(curveA, curveB, multiple=False):
    url = "rhino/geometry/curve/createbooleandifference-curve_curve"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB]
    if multiple: args = zip(curveA, curveB)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference1(curveA, curveB, tolerance, multiple=False):
    url = "rhino/geometry/curve/createbooleandifference-curve_curve_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, tolerance]
    if multiple: args = zip(curveA, curveB, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference2(curveA, subtractors, multiple=False):
    url = "rhino/geometry/curve/createbooleandifference-curve_curvearray"
    if multiple: url += "?multiple=true"
    args = [curveA, subtractors]
    if multiple: args = zip(curveA, subtractors)
    response = Util.ComputeFetch(url, args)
    return response


def CreateBooleanDifference3(curveA, subtractors, tolerance, multiple=False):
    url = "rhino/geometry/curve/createbooleandifference-curve_curvearray_double"
    if multiple: url += "?multiple=true"
    args = [curveA, subtractors, tolerance]
    if multiple: args = zip(curveA, subtractors, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateTextOutlines(text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance, multiple=False):
    url = "rhino/geometry/curve/createtextoutlines-string_string_double_int_bool_plane_double_double"
    if multiple: url += "?multiple=true"
    args = [text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance]
    if multiple: args = zip(text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def CreateCurve2View(curveA, curveB, vectorA, vectorB, tolerance, angleTolerance, multiple=False):
    url = "rhino/geometry/curve/createcurve2view-curve_curve_vector3d_vector3d_double_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, vectorA, vectorB, tolerance, angleTolerance]
    if multiple: args = zip(curveA, curveB, vectorA, vectorB, tolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def DoDirectionsMatch(curveA, curveB, multiple=False):
    url = "rhino/geometry/curve/dodirectionsmatch-curve_curve"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB]
    if multiple: args = zip(curveA, curveB)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToMesh(curve, mesh, direction, tolerance, multiple=False):
    url = "rhino/geometry/curve/projecttomesh-curve_mesh_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curve, mesh, direction, tolerance]
    if multiple: args = zip(curve, mesh, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToMesh1(curve, meshes, direction, tolerance, multiple=False):
    url = "rhino/geometry/curve/projecttomesh-curve_mesharray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curve, meshes, direction, tolerance]
    if multiple: args = zip(curve, meshes, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToMesh2(curves, meshes, direction, tolerance, multiple=False):
    url = "rhino/geometry/curve/projecttomesh-curvearray_mesharray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curves, meshes, direction, tolerance]
    if multiple: args = zip(curves, meshes, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToBrep(curve, brep, direction, tolerance, multiple=False):
    url = "rhino/geometry/curve/projecttobrep-curve_brep_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curve, brep, direction, tolerance]
    if multiple: args = zip(curve, brep, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToBrep1(curve, breps, direction, tolerance, multiple=False):
    url = "rhino/geometry/curve/projecttobrep-curve_breparray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curve, breps, direction, tolerance]
    if multiple: args = zip(curve, breps, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToBrep2(curve, breps, direction, tolerance, brepIndices, multiple=False):
    url = "rhino/geometry/curve/projecttobrep-curve_breparray_vector3d_double_intarray"
    if multiple: url += "?multiple=true"
    args = [curve, breps, direction, tolerance, brepIndices]
    if multiple: args = zip(curve, breps, direction, tolerance, brepIndices)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToBrep3(curves, breps, direction, tolerance, multiple=False):
    url = "rhino/geometry/curve/projecttobrep-curvearray_breparray_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [curves, breps, direction, tolerance]
    if multiple: args = zip(curves, breps, direction, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ProjectToPlane(curve, plane, multiple=False):
    url = "rhino/geometry/curve/projecttoplane-curve_plane"
    if multiple: url += "?multiple=true"
    args = [curve, plane]
    if multiple: args = zip(curve, plane)
    response = Util.ComputeFetch(url, args)
    return response


def PullToBrepFace(curve, face, tolerance, multiple=False):
    url = "rhino/geometry/curve/pulltobrepface-curve_brepface_double"
    if multiple: url += "?multiple=true"
    args = [curve, face, tolerance]
    if multiple: args = zip(curve, face, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def PlanarClosedCurveRelationship(curveA, curveB, testPlane, tolerance, multiple=False):
    url = "rhino/geometry/curve/planarclosedcurverelationship-curve_curve_plane_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, testPlane, tolerance]
    if multiple: args = zip(curveA, curveB, testPlane, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def PlanarCurveCollision(curveA, curveB, testPlane, tolerance, multiple=False):
    url = "rhino/geometry/curve/planarcurvecollision-curve_curve_plane_double"
    if multiple: url += "?multiple=true"
    args = [curveA, curveB, testPlane, tolerance]
    if multiple: args = zip(curveA, curveB, testPlane, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def DuplicateSegments(thisCurve, multiple=False):
    url = "rhino/geometry/curve/duplicatesegments-curve"
    if multiple: url += "?multiple=true"
    args = [thisCurve]
    if multiple: args = zip(thisCurve)
    response = Util.ComputeFetch(url, args)
    return response


def Smooth(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, multiple=False):
    url = "rhino/geometry/curve/smooth-curve_double_bool_bool_bool_bool_smoothingcoordinatesystem"
    if multiple: url += "?multiple=true"
    args = [thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem]
    if multiple: args = zip(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem)
    response = Util.ComputeFetch(url, args)
    return response


def Smooth1(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane, multiple=False):
    url = "rhino/geometry/curve/smooth-curve_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane"
    if multiple: url += "?multiple=true"
    args = [thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    if multiple: args = zip(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane)
    response = Util.ComputeFetch(url, args)
    return response


def MakeClosed(thisCurve, tolerance, multiple=False):
    url = "rhino/geometry/curve/makeclosed-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance]
    if multiple: args = zip(thisCurve, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def LcoalClosestPoint(thisCurve, testPoint, seed, t, multiple=False):
    url = "rhino/geometry/curve/lcoalclosestpoint-curve_point3d_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, seed, t]
    if multiple: args = zip(thisCurve, testPoint, seed, t)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint(thisCurve, testPoint, t, multiple=False):
    url = "rhino/geometry/curve/closestpoint-curve_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, t]
    if multiple: args = zip(thisCurve, testPoint, t)
    response = Util.ComputeFetch(url, args)
    return response


def ClosestPoint1(thisCurve, testPoint, t, maximumDistance, multiple=False):
    url = "rhino/geometry/curve/closestpoint-curve_point3d_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, t, maximumDistance]
    if multiple: args = zip(thisCurve, testPoint, t, maximumDistance)
    response = Util.ComputeFetch(url, args)
    return response


def Contains(thisCurve, testPoint, multiple=False):
    url = "rhino/geometry/curve/contains-curve_point3d"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint]
    if multiple: args = zip(thisCurve, testPoint)
    response = Util.ComputeFetch(url, args)
    return response


def Contains1(thisCurve, testPoint, plane, multiple=False):
    url = "rhino/geometry/curve/contains-curve_point3d_plane"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, plane]
    if multiple: args = zip(thisCurve, testPoint, plane)
    response = Util.ComputeFetch(url, args)
    return response


def Contains2(thisCurve, testPoint, plane, tolerance, multiple=False):
    url = "rhino/geometry/curve/contains-curve_point3d_plane_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, testPoint, plane, tolerance]
    if multiple: args = zip(thisCurve, testPoint, plane, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def ExtremeParameters(thisCurve, direction, multiple=False):
    url = "rhino/geometry/curve/extremeparameters-curve_vector3d"
    if multiple: url += "?multiple=true"
    args = [thisCurve, direction]
    if multiple: args = zip(thisCurve, direction)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePeriodicCurve(curve, multiple=False):
    url = "rhino/geometry/curve/createperiodiccurve-curve"
    if multiple: url += "?multiple=true"
    args = [curve]
    if multiple: args = zip(curve)
    response = Util.ComputeFetch(url, args)
    return response


def CreatePeriodicCurve1(curve, smooth, multiple=False):
    url = "rhino/geometry/curve/createperiodiccurve-curve_bool"
    if multiple: url += "?multiple=true"
    args = [curve, smooth]
    if multiple: args = zip(curve, smooth)
    response = Util.ComputeFetch(url, args)
    return response


def PointAtLength(thisCurve, length, multiple=False):
    url = "rhino/geometry/curve/pointatlength-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, length]
    if multiple: args = zip(thisCurve, length)
    response = Util.ComputeFetch(url, args)
    return response


def PointAtNormalizedLength(thisCurve, length, multiple=False):
    url = "rhino/geometry/curve/pointatnormalizedlength-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, length]
    if multiple: args = zip(thisCurve, length)
    response = Util.ComputeFetch(url, args)
    return response


def PerpendicularFrameAt(thisCurve, t, plane, multiple=False):
    url = "rhino/geometry/curve/perpendicularframeat-curve_double_plane"
    if multiple: url += "?multiple=true"
    args = [thisCurve, t, plane]
    if multiple: args = zip(thisCurve, t, plane)
    response = Util.ComputeFetch(url, args)
    return response


def GetPerpendicularFrames(thisCurve, parameters, multiple=False):
    url = "rhino/geometry/curve/getperpendicularframes-curve_doublearray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, parameters]
    if multiple: args = zip(thisCurve, parameters)
    response = Util.ComputeFetch(url, args)
    return response


def GetLength(thisCurve, multiple=False):
    url = "rhino/geometry/curve/getlength-curve"
    if multiple: url += "?multiple=true"
    args = [thisCurve]
    if multiple: args = zip(thisCurve)
    response = Util.ComputeFetch(url, args)
    return response


def GetLength1(thisCurve, fractionalTolerance, multiple=False):
    url = "rhino/geometry/curve/getlength-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, fractionalTolerance]
    if multiple: args = zip(thisCurve, fractionalTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def GetLength2(thisCurve, subdomain, multiple=False):
    url = "rhino/geometry/curve/getlength-curve_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, subdomain]
    if multiple: args = zip(thisCurve, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def GetLength3(thisCurve, fractionalTolerance, subdomain, multiple=False):
    url = "rhino/geometry/curve/getlength-curve_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, fractionalTolerance, subdomain]
    if multiple: args = zip(thisCurve, fractionalTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def IsShort(thisCurve, tolerance, multiple=False):
    url = "rhino/geometry/curve/isshort-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance]
    if multiple: args = zip(thisCurve, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def IsShort1(thisCurve, tolerance, subdomain, multiple=False):
    url = "rhino/geometry/curve/isshort-curve_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance, subdomain]
    if multiple: args = zip(thisCurve, tolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def RemoveShortSegments(thisCurve, tolerance, multiple=False):
    url = "rhino/geometry/curve/removeshortsegments-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance]
    if multiple: args = zip(thisCurve, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def LengthParameter(thisCurve, segmentLength, t, multiple=False):
    url = "rhino/geometry/curve/lengthparameter-curve_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, t]
    if multiple: args = zip(thisCurve, segmentLength, t)
    response = Util.ComputeFetch(url, args)
    return response


def LengthParameter1(thisCurve, segmentLength, t, fractionalTolerance, multiple=False):
    url = "rhino/geometry/curve/lengthparameter-curve_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, t, fractionalTolerance]
    if multiple: args = zip(thisCurve, segmentLength, t, fractionalTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def LengthParameter2(thisCurve, segmentLength, t, subdomain, multiple=False):
    url = "rhino/geometry/curve/lengthparameter-curve_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, t, subdomain]
    if multiple: args = zip(thisCurve, segmentLength, t, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def LengthParameter3(thisCurve, segmentLength, t, fractionalTolerance, subdomain, multiple=False):
    url = "rhino/geometry/curve/lengthparameter-curve_double_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, t, fractionalTolerance, subdomain]
    if multiple: args = zip(thisCurve, segmentLength, t, fractionalTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameter(thisCurve, s, t, multiple=False):
    url = "rhino/geometry/curve/normalizedlengthparameter-curve_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, t]
    if multiple: args = zip(thisCurve, s, t)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameter1(thisCurve, s, t, fractionalTolerance, multiple=False):
    url = "rhino/geometry/curve/normalizedlengthparameter-curve_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, t, fractionalTolerance]
    if multiple: args = zip(thisCurve, s, t, fractionalTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameter2(thisCurve, s, t, subdomain, multiple=False):
    url = "rhino/geometry/curve/normalizedlengthparameter-curve_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, t, subdomain]
    if multiple: args = zip(thisCurve, s, t, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameter3(thisCurve, s, t, fractionalTolerance, subdomain, multiple=False):
    url = "rhino/geometry/curve/normalizedlengthparameter-curve_double_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, t, fractionalTolerance, subdomain]
    if multiple: args = zip(thisCurve, s, t, fractionalTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameters(thisCurve, s, absoluteTolerance, multiple=False):
    url = "rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, absoluteTolerance]
    if multiple: args = zip(thisCurve, s, absoluteTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameters1(thisCurve, s, absoluteTolerance, fractionalTolerance, multiple=False):
    url = "rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, absoluteTolerance, fractionalTolerance]
    if multiple: args = zip(thisCurve, s, absoluteTolerance, fractionalTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameters2(thisCurve, s, absoluteTolerance, subdomain, multiple=False):
    url = "rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, absoluteTolerance, subdomain]
    if multiple: args = zip(thisCurve, s, absoluteTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def NormalizedLengthParameters3(thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain, multiple=False):
    url = "rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_double_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain]
    if multiple: args = zip(thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByCount(thisCurve, segmentCount, includeEnds, multiple=False):
    url = "rhino/geometry/curve/dividebycount-curve_int_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentCount, includeEnds]
    if multiple: args = zip(thisCurve, segmentCount, includeEnds)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByCount1(thisCurve, segmentCount, includeEnds, points, multiple=False):
    url = "rhino/geometry/curve/dividebycount-curve_int_bool_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentCount, includeEnds, points]
    if multiple: args = zip(thisCurve, segmentCount, includeEnds, points)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByLength(thisCurve, segmentLength, includeEnds, multiple=False):
    url = "rhino/geometry/curve/dividebylength-curve_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, includeEnds]
    if multiple: args = zip(thisCurve, segmentLength, includeEnds)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByLength1(thisCurve, segmentLength, includeEnds, reverse, multiple=False):
    url = "rhino/geometry/curve/dividebylength-curve_double_bool_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, includeEnds, reverse]
    if multiple: args = zip(thisCurve, segmentLength, includeEnds, reverse)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByLength2(thisCurve, segmentLength, includeEnds, points, multiple=False):
    url = "rhino/geometry/curve/dividebylength-curve_double_bool_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, includeEnds, points]
    if multiple: args = zip(thisCurve, segmentLength, includeEnds, points)
    response = Util.ComputeFetch(url, args)
    return response


def DivideByLength3(thisCurve, segmentLength, includeEnds, reverse, points, multiple=False):
    url = "rhino/geometry/curve/dividebylength-curve_double_bool_bool_point3darray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, segmentLength, includeEnds, reverse, points]
    if multiple: args = zip(thisCurve, segmentLength, includeEnds, reverse, points)
    response = Util.ComputeFetch(url, args)
    return response


def DivideEquidistant(thisCurve, distance, multiple=False):
    url = "rhino/geometry/curve/divideequidistant-curve_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, distance]
    if multiple: args = zip(thisCurve, distance)
    response = Util.ComputeFetch(url, args)
    return response


def DivideAsContour(thisCurve, contourStart, contourEnd, interval, multiple=False):
    url = "rhino/geometry/curve/divideascontour-curve_point3d_point3d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, contourStart, contourEnd, interval]
    if multiple: args = zip(thisCurve, contourStart, contourEnd, interval)
    response = Util.ComputeFetch(url, args)
    return response


def Trim(thisCurve, side, length, multiple=False):
    url = "rhino/geometry/curve/trim-curve_curveend_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, length]
    if multiple: args = zip(thisCurve, side, length)
    response = Util.ComputeFetch(url, args)
    return response


def Split(thisCurve, cutter, tolerance, multiple=False):
    url = "rhino/geometry/curve/split-curve_brep_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, cutter, tolerance]
    if multiple: args = zip(thisCurve, cutter, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Split1(thisCurve, cutter, tolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/curve/split-curve_brep_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, cutter, tolerance, angleToleranceRadians]
    if multiple: args = zip(thisCurve, cutter, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def Split2(thisCurve, cutter, tolerance, multiple=False):
    url = "rhino/geometry/curve/split-curve_surface_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, cutter, tolerance]
    if multiple: args = zip(thisCurve, cutter, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Split3(thisCurve, cutter, tolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/curve/split-curve_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, cutter, tolerance, angleToleranceRadians]
    if multiple: args = zip(thisCurve, cutter, tolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def Extend(thisCurve, t0, t1, multiple=False):
    url = "rhino/geometry/curve/extend-curve_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, t0, t1]
    if multiple: args = zip(thisCurve, t0, t1)
    response = Util.ComputeFetch(url, args)
    return response


def Extend1(thisCurve, domain, multiple=False):
    url = "rhino/geometry/curve/extend-curve_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, domain]
    if multiple: args = zip(thisCurve, domain)
    response = Util.ComputeFetch(url, args)
    return response


def Extend2(thisCurve, side, length, style, multiple=False):
    url = "rhino/geometry/curve/extend-curve_curveend_double_curveextensionstyle"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, length, style]
    if multiple: args = zip(thisCurve, side, length, style)
    response = Util.ComputeFetch(url, args)
    return response


def Extend3(thisCurve, side, style, geometry, multiple=False):
    url = "rhino/geometry/curve/extend-curve_curveend_curveextensionstyle_geometrybasearray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, style, geometry]
    if multiple: args = zip(thisCurve, side, style, geometry)
    response = Util.ComputeFetch(url, args)
    return response


def Extend4(thisCurve, side, style, endPoint, multiple=False):
    url = "rhino/geometry/curve/extend-curve_curveend_curveextensionstyle_point3d"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, style, endPoint]
    if multiple: args = zip(thisCurve, side, style, endPoint)
    response = Util.ComputeFetch(url, args)
    return response


def ExtendOnSurface(thisCurve, side, surface, multiple=False):
    url = "rhino/geometry/curve/extendonsurface-curve_curveend_surface"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, surface]
    if multiple: args = zip(thisCurve, side, surface)
    response = Util.ComputeFetch(url, args)
    return response


def ExtendOnSurface1(thisCurve, side, face, multiple=False):
    url = "rhino/geometry/curve/extendonsurface-curve_curveend_brepface"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, face]
    if multiple: args = zip(thisCurve, side, face)
    response = Util.ComputeFetch(url, args)
    return response


def ExtendByLine(thisCurve, side, geometry, multiple=False):
    url = "rhino/geometry/curve/extendbyline-curve_curveend_geometrybasearray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, geometry]
    if multiple: args = zip(thisCurve, side, geometry)
    response = Util.ComputeFetch(url, args)
    return response


def ExtendByArc(thisCurve, side, geometry, multiple=False):
    url = "rhino/geometry/curve/extendbyarc-curve_curveend_geometrybasearray"
    if multiple: url += "?multiple=true"
    args = [thisCurve, side, geometry]
    if multiple: args = zip(thisCurve, side, geometry)
    response = Util.ComputeFetch(url, args)
    return response


def Simplify(thisCurve, options, distanceTolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/curve/simplify-curve_curvesimplifyoptions_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, options, distanceTolerance, angleToleranceRadians]
    if multiple: args = zip(thisCurve, options, distanceTolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def SimplifyEnd(thisCurve, end, options, distanceTolerance, angleToleranceRadians, multiple=False):
    url = "rhino/geometry/curve/simplifyend-curve_curveend_curvesimplifyoptions_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, end, options, distanceTolerance, angleToleranceRadians]
    if multiple: args = zip(thisCurve, end, options, distanceTolerance, angleToleranceRadians)
    response = Util.ComputeFetch(url, args)
    return response


def Fair(thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations, multiple=False):
    url = "rhino/geometry/curve/fair-curve_double_double_int_int_int"
    if multiple: url += "?multiple=true"
    args = [thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations]
    if multiple: args = zip(thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations)
    response = Util.ComputeFetch(url, args)
    return response


def Fit(thisCurve, degree, fitTolerance, angleTolerance, multiple=False):
    url = "rhino/geometry/curve/fit-curve_int_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, degree, fitTolerance, angleTolerance]
    if multiple: args = zip(thisCurve, degree, fitTolerance, angleTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Rebuild(thisCurve, pointCount, degree, preserveTangents, multiple=False):
    url = "rhino/geometry/curve/rebuild-curve_int_int_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, pointCount, degree, preserveTangents]
    if multiple: args = zip(thisCurve, pointCount, degree, preserveTangents)
    response = Util.ComputeFetch(url, args)
    return response


def ToPolyline(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, multiple=False):
    url = "rhino/geometry/curve/topolyline-curve_int_int_double_double_double_double_double_double_bool"
    if multiple: url += "?multiple=true"
    args = [thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint]
    if multiple: args = zip(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint)
    response = Util.ComputeFetch(url, args)
    return response


def ToPolyline1(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain, multiple=False):
    url = "rhino/geometry/curve/topolyline-curve_int_int_double_double_double_double_double_double_bool_interval"
    if multiple: url += "?multiple=true"
    args = [thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain]
    if multiple: args = zip(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain)
    response = Util.ComputeFetch(url, args)
    return response


def ToPolyline2(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength, multiple=False):
    url = "rhino/geometry/curve/topolyline-curve_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance, angleTolerance, minimumLength, maximumLength]
    if multiple: args = zip(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength)
    response = Util.ComputeFetch(url, args)
    return response


def ToArcsAndLines(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength, multiple=False):
    url = "rhino/geometry/curve/toarcsandlines-curve_double_double_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, tolerance, angleTolerance, minimumLength, maximumLength]
    if multiple: args = zip(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength)
    response = Util.ComputeFetch(url, args)
    return response


def PullToMesh(thisCurve, mesh, tolerance, multiple=False):
    url = "rhino/geometry/curve/pulltomesh-curve_mesh_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, mesh, tolerance]
    if multiple: args = zip(thisCurve, mesh, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Offset(thisCurve, plane, distance, tolerance, cornerStyle, multiple=False):
    url = "rhino/geometry/curve/offset-curve_plane_double_double_curveoffsetcornerstyle"
    if multiple: url += "?multiple=true"
    args = [thisCurve, plane, distance, tolerance, cornerStyle]
    if multiple: args = zip(thisCurve, plane, distance, tolerance, cornerStyle)
    response = Util.ComputeFetch(url, args)
    return response


def Offset1(thisCurve, directionPoint, normal, distance, tolerance, cornerStyle, multiple=False):
    url = "rhino/geometry/curve/offset-curve_point3d_vector3d_double_double_curveoffsetcornerstyle"
    if multiple: url += "?multiple=true"
    args = [thisCurve, directionPoint, normal, distance, tolerance, cornerStyle]
    if multiple: args = zip(thisCurve, directionPoint, normal, distance, tolerance, cornerStyle)
    response = Util.ComputeFetch(url, args)
    return response


def RibbonOffset(thisCurve, distance, blendRadius, directionPoint, normal, tolerance, multiple=False):
    url = "rhino/geometry/curve/ribbonoffset-curve_double_double_point3d_vector3d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, distance, blendRadius, directionPoint, normal, tolerance]
    if multiple: args = zip(thisCurve, distance, blendRadius, directionPoint, normal, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface(thisCurve, face, distance, fittingTolerance, multiple=False):
    url = "rhino/geometry/curve/offsetonsurface-curve_brepface_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, face, distance, fittingTolerance]
    if multiple: args = zip(thisCurve, face, distance, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface1(thisCurve, face, throughPoint, fittingTolerance, multiple=False):
    url = "rhino/geometry/curve/offsetonsurface-curve_brepface_point2d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, face, throughPoint, fittingTolerance]
    if multiple: args = zip(thisCurve, face, throughPoint, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface2(thisCurve, face, curveParameters, offsetDistances, fittingTolerance, multiple=False):
    url = "rhino/geometry/curve/offsetonsurface-curve_brepface_doublearray_doublearray_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, face, curveParameters, offsetDistances, fittingTolerance]
    if multiple: args = zip(thisCurve, face, curveParameters, offsetDistances, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface3(thisCurve, surface, distance, fittingTolerance, multiple=False):
    url = "rhino/geometry/curve/offsetonsurface-curve_surface_double_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, surface, distance, fittingTolerance]
    if multiple: args = zip(thisCurve, surface, distance, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface4(thisCurve, surface, throughPoint, fittingTolerance, multiple=False):
    url = "rhino/geometry/curve/offsetonsurface-curve_surface_point2d_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, surface, throughPoint, fittingTolerance]
    if multiple: args = zip(thisCurve, surface, throughPoint, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetOnSurface5(thisCurve, surface, curveParameters, offsetDistances, fittingTolerance, multiple=False):
    url = "rhino/geometry/curve/offsetonsurface-curve_surface_doublearray_doublearray_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, surface, curveParameters, offsetDistances, fittingTolerance]
    if multiple: args = zip(thisCurve, surface, curveParameters, offsetDistances, fittingTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def PullToBrepFace(thisCurve, face, tolerance, multiple=False):
    url = "rhino/geometry/curve/pulltobrepface-curve_brepface_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, face, tolerance]
    if multiple: args = zip(thisCurve, face, tolerance)
    response = Util.ComputeFetch(url, args)
    return response


def OffsetNormalToSurface(thisCurve, surface, height, multiple=False):
    url = "rhino/geometry/curve/offsetnormaltosurface-curve_surface_double"
    if multiple: url += "?multiple=true"
    args = [thisCurve, surface, height]
    if multiple: args = zip(thisCurve, surface, height)
    response = Util.ComputeFetch(url, args)
    return response

