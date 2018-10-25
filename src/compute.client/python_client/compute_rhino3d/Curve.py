from . import Util


def GetConicSectionType(thisCurve):
    args = [thisCurve]
    response = Util.ComputeFetch("rhino/geometry/curve/getconicsectiontype-curve", args)
    return response


def CreateInterpolatedCurve(points, degree):
    args = [points, degree]
    response = Util.ComputeFetch("rhino/geometry/curve/createinterpolatedcurve-point3darray_int", args)
    return response


def CreateInterpolatedCurve1(points, degree, knots):
    args = [points, degree, knots]
    response = Util.ComputeFetch("rhino/geometry/curve/createinterpolatedcurve-point3darray_int_curveknotstyle", args)
    return response


def CreateInterpolatedCurve2(points, degree, knots, startTangent, endTangent):
    args = [points, degree, knots, startTangent, endTangent]
    response = Util.ComputeFetch("rhino/geometry/curve/createinterpolatedcurve-point3darray_int_curveknotstyle_vector3d_vector3d", args)
    return response


def CreateSoftEditCurve(curve, t, delta, length, fixEnds):
    args = [curve, t, delta, length, fixEnds]
    response = Util.ComputeFetch("rhino/geometry/curve/createsofteditcurve-curve_double_vector3d_double_bool", args)
    return response


def CreateFilletCornersCurve(curve, radius, tolerance, angleTolerance):
    args = [curve, radius, tolerance, angleTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createfilletcornerscurve-curve_double_double_double", args)
    return response


def CreateArcBlend(startPt, startDir, endPt, endDir, controlPointLengthRatio):
    args = [startPt, startDir, endPt, endDir, controlPointLengthRatio]
    response = Util.ComputeFetch("rhino/geometry/curve/createarcblend-point3d_vector3d_point3d_vector3d_double", args)
    return response


def CreateMeanCurve(curveA, curveB, angleToleranceRadians):
    args = [curveA, curveB, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/createmeancurve-curve_curve_double", args)
    return response


def CreateMeanCurve1(curveA, curveB):
    args = [curveA, curveB]
    response = Util.ComputeFetch("rhino/geometry/curve/createmeancurve-curve_curve", args)
    return response


def CreateBlendCurve(curveA, curveB, continuity):
    args = [curveA, curveB, continuity]
    response = Util.ComputeFetch("rhino/geometry/curve/createblendcurve-curve_curve_blendcontinuity", args)
    return response


def CreateBlendCurve1(curveA, curveB, continuity, bulgeA, bulgeB):
    args = [curveA, curveB, continuity, bulgeA, bulgeB]
    response = Util.ComputeFetch("rhino/geometry/curve/createblendcurve-curve_curve_blendcontinuity_double_double", args)
    return response


def CreateBlendCurve2(curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1):
    args = [curve0, t0, reverse0, continuity0, curve1, t1, reverse1, continuity1]
    response = Util.ComputeFetch("rhino/geometry/curve/createblendcurve-curve_double_bool_blendcontinuity_curve_double_bool_blendcontinuity", args)
    return response


def CreateTweenCurves(curve0, curve1, numCurves):
    args = [curve0, curve1, numCurves]
    response = Util.ComputeFetch("rhino/geometry/curve/createtweencurves-curve_curve_int", args)
    return response


def CreateTweenCurves1(curve0, curve1, numCurves, tolerance):
    args = [curve0, curve1, numCurves, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createtweencurves-curve_curve_int_double", args)
    return response


def CreateTweenCurvesWithMatching(curve0, curve1, numCurves):
    args = [curve0, curve1, numCurves]
    response = Util.ComputeFetch("rhino/geometry/curve/createtweencurveswithmatching-curve_curve_int", args)
    return response


def CreateTweenCurvesWithMatching1(curve0, curve1, numCurves, tolerance):
    args = [curve0, curve1, numCurves, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createtweencurveswithmatching-curve_curve_int_double", args)
    return response


def CreateTweenCurvesWithSampling(curve0, curve1, numCurves, numSamples):
    args = [curve0, curve1, numCurves, numSamples]
    response = Util.ComputeFetch("rhino/geometry/curve/createtweencurveswithsampling-curve_curve_int_int", args)
    return response


def CreateTweenCurvesWithSampling1(curve0, curve1, numCurves, numSamples, tolerance):
    args = [curve0, curve1, numCurves, numSamples, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createtweencurveswithsampling-curve_curve_int_int_double", args)
    return response


def JoinCurves(inputCurves):
    args = [inputCurves]
    response = Util.ComputeFetch("rhino/geometry/curve/joincurves-curvearray", args)
    return response


def JoinCurves1(inputCurves, joinTolerance):
    args = [inputCurves, joinTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/joincurves-curvearray_double", args)
    return response


def JoinCurves2(inputCurves, joinTolerance, preserveDirection):
    args = [inputCurves, joinTolerance, preserveDirection]
    response = Util.ComputeFetch("rhino/geometry/curve/joincurves-curvearray_double_bool", args)
    return response


def MakeEndsMeet(curveA, adjustStartCurveA, curveB, adjustStartCurveB):
    args = [curveA, adjustStartCurveA, curveB, adjustStartCurveB]
    response = Util.ComputeFetch("rhino/geometry/curve/makeendsmeet-curve_bool_curve_bool", args)
    return response


def CreateFillet(curve0, curve1, radius, t0Base, t1Base):
    args = [curve0, curve1, radius, t0Base, t1Base]
    response = Util.ComputeFetch("rhino/geometry/curve/createfillet-curve_curve_double_double_double", args)
    return response


def CreateFilletCurves(curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance):
    args = [curve0, point0, curve1, point1, radius, join, trim, arcExtension, tolerance, angleTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createfilletcurves-curve_point3d_curve_point3d_double_bool_bool_bool_double_double", args)
    return response


def CreateBooleanUnion(curves):
    args = [curves]
    response = Util.ComputeFetch("rhino/geometry/curve/createbooleanunion-curvearray", args)
    return response


def CreateBooleanUnion1(curves, tolerance):
    args = [curves, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createbooleanunion-curvearray_double", args)
    return response


def CreateBooleanIntersection(curveA, curveB):
    args = [curveA, curveB]
    response = Util.ComputeFetch("rhino/geometry/curve/createbooleanintersection-curve_curve", args)
    return response


def CreateBooleanIntersection1(curveA, curveB, tolerance):
    args = [curveA, curveB, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createbooleanintersection-curve_curve_double", args)
    return response


def CreateBooleanDifference(curveA, curveB):
    args = [curveA, curveB]
    response = Util.ComputeFetch("rhino/geometry/curve/createbooleandifference-curve_curve", args)
    return response


def CreateBooleanDifference1(curveA, curveB, tolerance):
    args = [curveA, curveB, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createbooleandifference-curve_curve_double", args)
    return response


def CreateBooleanDifference2(curveA, subtractors):
    args = [curveA, subtractors]
    response = Util.ComputeFetch("rhino/geometry/curve/createbooleandifference-curve_curvearray", args)
    return response


def CreateBooleanDifference3(curveA, subtractors, tolerance):
    args = [curveA, subtractors, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createbooleandifference-curve_curvearray_double", args)
    return response


def CreateTextOutlines(text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance):
    args = [text, font, textHeight, textStyle, closeLoops, plane, smallCapsScale, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createtextoutlines-string_string_double_int_bool_plane_double_double", args)
    return response


def CreateCurve2View(curveA, curveB, vectorA, vectorB, tolerance, angleTolerance):
    args = [curveA, curveB, vectorA, vectorB, tolerance, angleTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/createcurve2view-curve_curve_vector3d_vector3d_double_double", args)
    return response


def DoDirectionsMatch(curveA, curveB):
    args = [curveA, curveB]
    response = Util.ComputeFetch("rhino/geometry/curve/dodirectionsmatch-curve_curve", args)
    return response


def ProjectToMesh(curve, mesh, direction, tolerance):
    args = [curve, mesh, direction, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/projecttomesh-curve_mesh_vector3d_double", args)
    return response


def ProjectToMesh1(curve, meshes, direction, tolerance):
    args = [curve, meshes, direction, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/projecttomesh-curve_mesharray_vector3d_double", args)
    return response


def ProjectToMesh2(curves, meshes, direction, tolerance):
    args = [curves, meshes, direction, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/projecttomesh-curvearray_mesharray_vector3d_double", args)
    return response


def ProjectToBrep(curve, brep, direction, tolerance):
    args = [curve, brep, direction, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/projecttobrep-curve_brep_vector3d_double", args)
    return response


def ProjectToBrep1(curve, breps, direction, tolerance):
    args = [curve, breps, direction, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/projecttobrep-curve_breparray_vector3d_double", args)
    return response


def ProjectToBrep2(curve, breps, direction, tolerance, brepIndices):
    args = [curve, breps, direction, tolerance, brepIndices]
    response = Util.ComputeFetch("rhino/geometry/curve/projecttobrep-curve_breparray_vector3d_double_intarray", args)
    return response


def ProjectToBrep3(curves, breps, direction, tolerance):
    args = [curves, breps, direction, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/projecttobrep-curvearray_breparray_vector3d_double", args)
    return response


def ProjectToPlane(curve, plane):
    args = [curve, plane]
    response = Util.ComputeFetch("rhino/geometry/curve/projecttoplane-curve_plane", args)
    return response


def PullToBrepFace(curve, face, tolerance):
    args = [curve, face, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/pulltobrepface-curve_brepface_double", args)
    return response


def PlanarClosedCurveRelationship(curveA, curveB, testPlane, tolerance):
    args = [curveA, curveB, testPlane, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/planarclosedcurverelationship-curve_curve_plane_double", args)
    return response


def PlanarCurveCollision(curveA, curveB, testPlane, tolerance):
    args = [curveA, curveB, testPlane, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/planarcurvecollision-curve_curve_plane_double", args)
    return response


def DuplicateSegments(thisCurve):
    args = [thisCurve]
    response = Util.ComputeFetch("rhino/geometry/curve/duplicatesegments-curve", args)
    return response


def Smooth(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem):
    args = [thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem]
    response = Util.ComputeFetch("rhino/geometry/curve/smooth-curve_double_bool_bool_bool_bool_smoothingcoordinatesystem", args)
    return response


def Smooth1(thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane):
    args = [thisCurve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    response = Util.ComputeFetch("rhino/geometry/curve/smooth-curve_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane", args)
    return response


def MakeClosed(thisCurve, tolerance):
    args = [thisCurve, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/makeclosed-curve_double", args)
    return response


def LcoalClosestPoint(thisCurve, testPoint, seed, t):
    args = [thisCurve, testPoint, seed, t]
    response = Util.ComputeFetch("rhino/geometry/curve/lcoalclosestpoint-curve_point3d_double_double", args)
    return response


def ClosestPoint(thisCurve, testPoint, t):
    args = [thisCurve, testPoint, t]
    response = Util.ComputeFetch("rhino/geometry/curve/closestpoint-curve_point3d_double", args)
    return response


def ClosestPoint1(thisCurve, testPoint, t, maximumDistance):
    args = [thisCurve, testPoint, t, maximumDistance]
    response = Util.ComputeFetch("rhino/geometry/curve/closestpoint-curve_point3d_double_double", args)
    return response


def Contains(thisCurve, testPoint):
    args = [thisCurve, testPoint]
    response = Util.ComputeFetch("rhino/geometry/curve/contains-curve_point3d", args)
    return response


def Contains1(thisCurve, testPoint, plane):
    args = [thisCurve, testPoint, plane]
    response = Util.ComputeFetch("rhino/geometry/curve/contains-curve_point3d_plane", args)
    return response


def Contains2(thisCurve, testPoint, plane, tolerance):
    args = [thisCurve, testPoint, plane, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/contains-curve_point3d_plane_double", args)
    return response


def ExtremeParameters(thisCurve, direction):
    args = [thisCurve, direction]
    response = Util.ComputeFetch("rhino/geometry/curve/extremeparameters-curve_vector3d", args)
    return response


def CreatePeriodicCurve(curve):
    args = [curve]
    response = Util.ComputeFetch("rhino/geometry/curve/createperiodiccurve-curve", args)
    return response


def CreatePeriodicCurve1(curve, smooth):
    args = [curve, smooth]
    response = Util.ComputeFetch("rhino/geometry/curve/createperiodiccurve-curve_bool", args)
    return response


def PointAtLength(thisCurve, length):
    args = [thisCurve, length]
    response = Util.ComputeFetch("rhino/geometry/curve/pointatlength-curve_double", args)
    return response


def PointAtNormalizedLength(thisCurve, length):
    args = [thisCurve, length]
    response = Util.ComputeFetch("rhino/geometry/curve/pointatnormalizedlength-curve_double", args)
    return response


def PerpendicularFrameAt(thisCurve, t, plane):
    args = [thisCurve, t, plane]
    response = Util.ComputeFetch("rhino/geometry/curve/perpendicularframeat-curve_double_plane", args)
    return response


def GetPerpendicularFrames(thisCurve, parameters):
    args = [thisCurve, parameters]
    response = Util.ComputeFetch("rhino/geometry/curve/getperpendicularframes-curve_doublearray", args)
    return response


def GetLength(thisCurve):
    args = [thisCurve]
    response = Util.ComputeFetch("rhino/geometry/curve/getlength-curve", args)
    return response


def GetLength1(thisCurve, fractionalTolerance):
    args = [thisCurve, fractionalTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/getlength-curve_double", args)
    return response


def GetLength2(thisCurve, subdomain):
    args = [thisCurve, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/getlength-curve_interval", args)
    return response


def GetLength3(thisCurve, fractionalTolerance, subdomain):
    args = [thisCurve, fractionalTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/getlength-curve_double_interval", args)
    return response


def IsShort(thisCurve, tolerance):
    args = [thisCurve, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/isshort-curve_double", args)
    return response


def IsShort1(thisCurve, tolerance, subdomain):
    args = [thisCurve, tolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/isshort-curve_double_interval", args)
    return response


def RemoveShortSegments(thisCurve, tolerance):
    args = [thisCurve, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/removeshortsegments-curve_double", args)
    return response


def LengthParameter(thisCurve, segmentLength, t):
    args = [thisCurve, segmentLength, t]
    response = Util.ComputeFetch("rhino/geometry/curve/lengthparameter-curve_double_double", args)
    return response


def LengthParameter1(thisCurve, segmentLength, t, fractionalTolerance):
    args = [thisCurve, segmentLength, t, fractionalTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/lengthparameter-curve_double_double_double", args)
    return response


def LengthParameter2(thisCurve, segmentLength, t, subdomain):
    args = [thisCurve, segmentLength, t, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/lengthparameter-curve_double_double_interval", args)
    return response


def LengthParameter3(thisCurve, segmentLength, t, fractionalTolerance, subdomain):
    args = [thisCurve, segmentLength, t, fractionalTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/lengthparameter-curve_double_double_double_interval", args)
    return response


def NormalizedLengthParameter(thisCurve, s, t):
    args = [thisCurve, s, t]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameter-curve_double_double", args)
    return response


def NormalizedLengthParameter1(thisCurve, s, t, fractionalTolerance):
    args = [thisCurve, s, t, fractionalTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameter-curve_double_double_double", args)
    return response


def NormalizedLengthParameter2(thisCurve, s, t, subdomain):
    args = [thisCurve, s, t, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameter-curve_double_double_interval", args)
    return response


def NormalizedLengthParameter3(thisCurve, s, t, fractionalTolerance, subdomain):
    args = [thisCurve, s, t, fractionalTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameter-curve_double_double_double_interval", args)
    return response


def NormalizedLengthParameters(thisCurve, s, absoluteTolerance):
    args = [thisCurve, s, absoluteTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double", args)
    return response


def NormalizedLengthParameters1(thisCurve, s, absoluteTolerance, fractionalTolerance):
    args = [thisCurve, s, absoluteTolerance, fractionalTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_double", args)
    return response


def NormalizedLengthParameters2(thisCurve, s, absoluteTolerance, subdomain):
    args = [thisCurve, s, absoluteTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_interval", args)
    return response


def NormalizedLengthParameters3(thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain):
    args = [thisCurve, s, absoluteTolerance, fractionalTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_double_interval", args)
    return response


def DivideByCount(thisCurve, segmentCount, includeEnds):
    args = [thisCurve, segmentCount, includeEnds]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebycount-curve_int_bool", args)
    return response


def DivideByCount1(thisCurve, segmentCount, includeEnds, points):
    args = [thisCurve, segmentCount, includeEnds, points]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebycount-curve_int_bool_point3darray", args)
    return response


def DivideByLength(thisCurve, segmentLength, includeEnds):
    args = [thisCurve, segmentLength, includeEnds]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebylength-curve_double_bool", args)
    return response


def DivideByLength1(thisCurve, segmentLength, includeEnds, reverse):
    args = [thisCurve, segmentLength, includeEnds, reverse]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebylength-curve_double_bool_bool", args)
    return response


def DivideByLength2(thisCurve, segmentLength, includeEnds, points):
    args = [thisCurve, segmentLength, includeEnds, points]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebylength-curve_double_bool_point3darray", args)
    return response


def DivideByLength3(thisCurve, segmentLength, includeEnds, reverse, points):
    args = [thisCurve, segmentLength, includeEnds, reverse, points]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebylength-curve_double_bool_bool_point3darray", args)
    return response


def DivideEquidistant(thisCurve, distance):
    args = [thisCurve, distance]
    response = Util.ComputeFetch("rhino/geometry/curve/divideequidistant-curve_double", args)
    return response


def DivideAsContour(thisCurve, contourStart, contourEnd, interval):
    args = [thisCurve, contourStart, contourEnd, interval]
    response = Util.ComputeFetch("rhino/geometry/curve/divideascontour-curve_point3d_point3d_double", args)
    return response


def Trim(thisCurve, side, length):
    args = [thisCurve, side, length]
    response = Util.ComputeFetch("rhino/geometry/curve/trim-curve_curveend_double", args)
    return response


def Split(thisCurve, cutter, tolerance):
    args = [thisCurve, cutter, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/split-curve_brep_double", args)
    return response


def Split1(thisCurve, cutter, tolerance, angleToleranceRadians):
    args = [thisCurve, cutter, tolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/split-curve_brep_double_double", args)
    return response


def Split2(thisCurve, cutter, tolerance):
    args = [thisCurve, cutter, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/split-curve_surface_double", args)
    return response


def Split3(thisCurve, cutter, tolerance, angleToleranceRadians):
    args = [thisCurve, cutter, tolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/split-curve_surface_double_double", args)
    return response


def Extend(thisCurve, t0, t1):
    args = [thisCurve, t0, t1]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_double_double", args)
    return response


def Extend1(thisCurve, domain):
    args = [thisCurve, domain]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_interval", args)
    return response


def Extend2(thisCurve, side, length, style):
    args = [thisCurve, side, length, style]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_curveend_double_curveextensionstyle", args)
    return response


def Extend3(thisCurve, side, style, geometry):
    args = [thisCurve, side, style, geometry]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_curveend_curveextensionstyle_geometrybasearray", args)
    return response


def Extend4(thisCurve, side, style, endPoint):
    args = [thisCurve, side, style, endPoint]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_curveend_curveextensionstyle_point3d", args)
    return response


def ExtendOnSurface(thisCurve, side, surface):
    args = [thisCurve, side, surface]
    response = Util.ComputeFetch("rhino/geometry/curve/extendonsurface-curve_curveend_surface", args)
    return response


def ExtendOnSurface1(thisCurve, side, face):
    args = [thisCurve, side, face]
    response = Util.ComputeFetch("rhino/geometry/curve/extendonsurface-curve_curveend_brepface", args)
    return response


def ExtendByLine(thisCurve, side, geometry):
    args = [thisCurve, side, geometry]
    response = Util.ComputeFetch("rhino/geometry/curve/extendbyline-curve_curveend_geometrybasearray", args)
    return response


def ExtendByArc(thisCurve, side, geometry):
    args = [thisCurve, side, geometry]
    response = Util.ComputeFetch("rhino/geometry/curve/extendbyarc-curve_curveend_geometrybasearray", args)
    return response


def Simplify(thisCurve, options, distanceTolerance, angleToleranceRadians):
    args = [thisCurve, options, distanceTolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/simplify-curve_curvesimplifyoptions_double_double", args)
    return response


def SimplifyEnd(thisCurve, end, options, distanceTolerance, angleToleranceRadians):
    args = [thisCurve, end, options, distanceTolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/simplifyend-curve_curveend_curvesimplifyoptions_double_double", args)
    return response


def Fair(thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations):
    args = [thisCurve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations]
    response = Util.ComputeFetch("rhino/geometry/curve/fair-curve_double_double_int_int_int", args)
    return response


def Fit(thisCurve, degree, fitTolerance, angleTolerance):
    args = [thisCurve, degree, fitTolerance, angleTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/fit-curve_int_double_double", args)
    return response


def Rebuild(thisCurve, pointCount, degree, preserveTangents):
    args = [thisCurve, pointCount, degree, preserveTangents]
    response = Util.ComputeFetch("rhino/geometry/curve/rebuild-curve_int_int_bool", args)
    return response


def ToPolyline(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint):
    args = [thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint]
    response = Util.ComputeFetch("rhino/geometry/curve/topolyline-curve_int_int_double_double_double_double_double_double_bool", args)
    return response


def ToPolyline1(thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain):
    args = [thisCurve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain]
    response = Util.ComputeFetch("rhino/geometry/curve/topolyline-curve_int_int_double_double_double_double_double_double_bool_interval", args)
    return response


def ToPolyline2(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength):
    args = [thisCurve, tolerance, angleTolerance, minimumLength, maximumLength]
    response = Util.ComputeFetch("rhino/geometry/curve/topolyline-curve_double_double_double_double", args)
    return response


def ToArcsAndLines(thisCurve, tolerance, angleTolerance, minimumLength, maximumLength):
    args = [thisCurve, tolerance, angleTolerance, minimumLength, maximumLength]
    response = Util.ComputeFetch("rhino/geometry/curve/toarcsandlines-curve_double_double_double_double", args)
    return response


def PullToMesh(thisCurve, mesh, tolerance):
    args = [thisCurve, mesh, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/pulltomesh-curve_mesh_double", args)
    return response


def Offset(thisCurve, plane, distance, tolerance, cornerStyle):
    args = [thisCurve, plane, distance, tolerance, cornerStyle]
    response = Util.ComputeFetch("rhino/geometry/curve/offset-curve_plane_double_double_curveoffsetcornerstyle", args)
    return response


def Offset1(thisCurve, directionPoint, normal, distance, tolerance, cornerStyle):
    args = [thisCurve, directionPoint, normal, distance, tolerance, cornerStyle]
    response = Util.ComputeFetch("rhino/geometry/curve/offset-curve_point3d_vector3d_double_double_curveoffsetcornerstyle", args)
    return response


def OffsetOnSurface(thisCurve, face, distance, fittingTolerance):
    args = [thisCurve, face, distance, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_brepface_double_double", args)
    return response


def OffsetOnSurface1(thisCurve, face, throughPoint, fittingTolerance):
    args = [thisCurve, face, throughPoint, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_brepface_point2d_double", args)
    return response


def OffsetOnSurface2(thisCurve, face, curveParameters, offsetDistances, fittingTolerance):
    args = [thisCurve, face, curveParameters, offsetDistances, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_brepface_doublearray_doublearray_double", args)
    return response


def OffsetOnSurface3(thisCurve, surface, distance, fittingTolerance):
    args = [thisCurve, surface, distance, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_surface_double_double", args)
    return response


def OffsetOnSurface4(thisCurve, surface, throughPoint, fittingTolerance):
    args = [thisCurve, surface, throughPoint, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_surface_point2d_double", args)
    return response


def OffsetOnSurface5(thisCurve, surface, curveParameters, offsetDistances, fittingTolerance):
    args = [thisCurve, surface, curveParameters, offsetDistances, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_surface_doublearray_doublearray_double", args)
    return response


def PullToBrepFace(thisCurve, face, tolerance):
    args = [thisCurve, face, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/pulltobrepface-curve_brepface_double", args)
    return response


def OffsetNormalToSurface(thisCurve, surface, height):
    args = [thisCurve, surface, height]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetnormaltosurface-curve_surface_double", args)
    return response

