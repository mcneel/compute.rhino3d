import Util


def GetConicSectionType(curve):
    args = [curve]
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


def DuplicateSegments(curve):
    args = [curve]
    response = Util.ComputeFetch("rhino/geometry/curve/duplicatesegments-curve", args)
    return response


def Smooth(curve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem):
    args = [curve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem]
    response = Util.ComputeFetch("rhino/geometry/curve/smooth-curve_double_bool_bool_bool_bool_smoothingcoordinatesystem", args)
    return response


def Smooth1(curve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane):
    args = [curve, smoothFactor, bXSmooth, bYSmooth, bZSmooth, bFixBoundaries, coordinateSystem, plane]
    response = Util.ComputeFetch("rhino/geometry/curve/smooth-curve_double_bool_bool_bool_bool_smoothingcoordinatesystem_plane", args)
    return response


def MakeClosed(curve, tolerance):
    args = [curve, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/makeclosed-curve_double", args)
    return response


def LcoalClosestPoint(curve, testPoint, seed, t):
    args = [curve, testPoint, seed, t]
    response = Util.ComputeFetch("rhino/geometry/curve/lcoalclosestpoint-curve_point3d_double_double", args)
    return response


def ClosestPoint(curve, testPoint, t):
    args = [curve, testPoint, t]
    response = Util.ComputeFetch("rhino/geometry/curve/closestpoint-curve_point3d_double", args)
    return response


def ClosestPoint1(curve, testPoint, t, maximumDistance):
    args = [curve, testPoint, t, maximumDistance]
    response = Util.ComputeFetch("rhino/geometry/curve/closestpoint-curve_point3d_double_double", args)
    return response


def Contains(curve, testPoint):
    args = [curve, testPoint]
    response = Util.ComputeFetch("rhino/geometry/curve/contains-curve_point3d", args)
    return response


def Contains1(curve, testPoint, plane):
    args = [curve, testPoint, plane]
    response = Util.ComputeFetch("rhino/geometry/curve/contains-curve_point3d_plane", args)
    return response


def Contains2(curve, testPoint, plane, tolerance):
    args = [curve, testPoint, plane, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/contains-curve_point3d_plane_double", args)
    return response


def ExtremeParameters(curve, direction):
    args = [curve, direction]
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


def PointAtLength(curve, length):
    args = [curve, length]
    response = Util.ComputeFetch("rhino/geometry/curve/pointatlength-curve_double", args)
    return response


def PointAtNormalizedLength(curve, length):
    args = [curve, length]
    response = Util.ComputeFetch("rhino/geometry/curve/pointatnormalizedlength-curve_double", args)
    return response


def PerpendicularFrameAt(curve, t, plane):
    args = [curve, t, plane]
    response = Util.ComputeFetch("rhino/geometry/curve/perpendicularframeat-curve_double_plane", args)
    return response


def GetPerpendicularFrames(curve, parameters):
    args = [curve, parameters]
    response = Util.ComputeFetch("rhino/geometry/curve/getperpendicularframes-curve_doublearray", args)
    return response


def GetLength(curve):
    args = [curve]
    response = Util.ComputeFetch("rhino/geometry/curve/getlength-curve", args)
    return response


def GetLength1(curve, fractionalTolerance):
    args = [curve, fractionalTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/getlength-curve_double", args)
    return response


def GetLength2(curve, subdomain):
    args = [curve, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/getlength-curve_interval", args)
    return response


def GetLength3(curve, fractionalTolerance, subdomain):
    args = [curve, fractionalTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/getlength-curve_double_interval", args)
    return response


def IsShort(curve, tolerance):
    args = [curve, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/isshort-curve_double", args)
    return response


def IsShort1(curve, tolerance, subdomain):
    args = [curve, tolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/isshort-curve_double_interval", args)
    return response


def RemoveShortSegments(curve, tolerance):
    args = [curve, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/removeshortsegments-curve_double", args)
    return response


def LengthParameter(curve, segmentLength, t):
    args = [curve, segmentLength, t]
    response = Util.ComputeFetch("rhino/geometry/curve/lengthparameter-curve_double_double", args)
    return response


def LengthParameter1(curve, segmentLength, t, fractionalTolerance):
    args = [curve, segmentLength, t, fractionalTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/lengthparameter-curve_double_double_double", args)
    return response


def LengthParameter2(curve, segmentLength, t, subdomain):
    args = [curve, segmentLength, t, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/lengthparameter-curve_double_double_interval", args)
    return response


def LengthParameter3(curve, segmentLength, t, fractionalTolerance, subdomain):
    args = [curve, segmentLength, t, fractionalTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/lengthparameter-curve_double_double_double_interval", args)
    return response


def NormalizedLengthParameter(curve, s, t):
    args = [curve, s, t]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameter-curve_double_double", args)
    return response


def NormalizedLengthParameter1(curve, s, t, fractionalTolerance):
    args = [curve, s, t, fractionalTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameter-curve_double_double_double", args)
    return response


def NormalizedLengthParameter2(curve, s, t, subdomain):
    args = [curve, s, t, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameter-curve_double_double_interval", args)
    return response


def NormalizedLengthParameter3(curve, s, t, fractionalTolerance, subdomain):
    args = [curve, s, t, fractionalTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameter-curve_double_double_double_interval", args)
    return response


def NormalizedLengthParameters(curve, s, absoluteTolerance):
    args = [curve, s, absoluteTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double", args)
    return response


def NormalizedLengthParameters1(curve, s, absoluteTolerance, fractionalTolerance):
    args = [curve, s, absoluteTolerance, fractionalTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_double", args)
    return response


def NormalizedLengthParameters2(curve, s, absoluteTolerance, subdomain):
    args = [curve, s, absoluteTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_interval", args)
    return response


def NormalizedLengthParameters3(curve, s, absoluteTolerance, fractionalTolerance, subdomain):
    args = [curve, s, absoluteTolerance, fractionalTolerance, subdomain]
    response = Util.ComputeFetch("rhino/geometry/curve/normalizedlengthparameters-curve_doublearray_double_double_interval", args)
    return response


def DivideByCount(curve, segmentCount, includeEnds):
    args = [curve, segmentCount, includeEnds]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebycount-curve_int_bool", args)
    return response


def DivideByCount1(curve, segmentCount, includeEnds, points):
    args = [curve, segmentCount, includeEnds, points]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebycount-curve_int_bool_point3darray", args)
    return response


def DivideByLength(curve, segmentLength, includeEnds):
    args = [curve, segmentLength, includeEnds]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebylength-curve_double_bool", args)
    return response


def DivideByLength1(curve, segmentLength, includeEnds, reverse):
    args = [curve, segmentLength, includeEnds, reverse]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebylength-curve_double_bool_bool", args)
    return response


def DivideByLength2(curve, segmentLength, includeEnds, points):
    args = [curve, segmentLength, includeEnds, points]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebylength-curve_double_bool_point3darray", args)
    return response


def DivideByLength3(curve, segmentLength, includeEnds, reverse, points):
    args = [curve, segmentLength, includeEnds, reverse, points]
    response = Util.ComputeFetch("rhino/geometry/curve/dividebylength-curve_double_bool_bool_point3darray", args)
    return response


def DivideEquidistant(curve, distance):
    args = [curve, distance]
    response = Util.ComputeFetch("rhino/geometry/curve/divideequidistant-curve_double", args)
    return response


def DivideAsContour(curve, contourStart, contourEnd, interval):
    args = [curve, contourStart, contourEnd, interval]
    response = Util.ComputeFetch("rhino/geometry/curve/divideascontour-curve_point3d_point3d_double", args)
    return response


def Trim(curve, side, length):
    args = [curve, side, length]
    response = Util.ComputeFetch("rhino/geometry/curve/trim-curve_curveend_double", args)
    return response


def Split(curve, cutter, tolerance):
    args = [curve, cutter, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/split-curve_brep_double", args)
    return response


def Split1(curve, cutter, tolerance, angleToleranceRadians):
    args = [curve, cutter, tolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/split-curve_brep_double_double", args)
    return response


def Split2(curve, cutter, tolerance):
    args = [curve, cutter, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/split-curve_surface_double", args)
    return response


def Split3(curve, cutter, tolerance, angleToleranceRadians):
    args = [curve, cutter, tolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/split-curve_surface_double_double", args)
    return response


def Extend(curve, t0, t1):
    args = [curve, t0, t1]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_double_double", args)
    return response


def Extend1(curve, domain):
    args = [curve, domain]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_interval", args)
    return response


def Extend2(curve, side, length, style):
    args = [curve, side, length, style]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_curveend_double_curveextensionstyle", args)
    return response


def Extend3(curve, side, style, geometry):
    args = [curve, side, style, geometry]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_curveend_curveextensionstyle_geometrybasearray", args)
    return response


def Extend4(curve, side, style, endPoint):
    args = [curve, side, style, endPoint]
    response = Util.ComputeFetch("rhino/geometry/curve/extend-curve_curveend_curveextensionstyle_point3d", args)
    return response


def ExtendOnSurface(curve, side, surface):
    args = [curve, side, surface]
    response = Util.ComputeFetch("rhino/geometry/curve/extendonsurface-curve_curveend_surface", args)
    return response


def ExtendOnSurface1(curve, side, face):
    args = [curve, side, face]
    response = Util.ComputeFetch("rhino/geometry/curve/extendonsurface-curve_curveend_brepface", args)
    return response


def ExtendByLine(curve, side, geometry):
    args = [curve, side, geometry]
    response = Util.ComputeFetch("rhino/geometry/curve/extendbyline-curve_curveend_geometrybasearray", args)
    return response


def ExtendByArc(curve, side, geometry):
    args = [curve, side, geometry]
    response = Util.ComputeFetch("rhino/geometry/curve/extendbyarc-curve_curveend_geometrybasearray", args)
    return response


def Simplify(curve, options, distanceTolerance, angleToleranceRadians):
    args = [curve, options, distanceTolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/simplify-curve_curvesimplifyoptions_double_double", args)
    return response


def SimplifyEnd(curve, end, options, distanceTolerance, angleToleranceRadians):
    args = [curve, end, options, distanceTolerance, angleToleranceRadians]
    response = Util.ComputeFetch("rhino/geometry/curve/simplifyend-curve_curveend_curvesimplifyoptions_double_double", args)
    return response


def Fair(curve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations):
    args = [curve, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations]
    response = Util.ComputeFetch("rhino/geometry/curve/fair-curve_double_double_int_int_int", args)
    return response


def Fit(curve, degree, fitTolerance, angleTolerance):
    args = [curve, degree, fitTolerance, angleTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/fit-curve_int_double_double", args)
    return response


def Rebuild(curve, pointCount, degree, preserveTangents):
    args = [curve, pointCount, degree, preserveTangents]
    response = Util.ComputeFetch("rhino/geometry/curve/rebuild-curve_int_int_bool", args)
    return response


def ToPolyline(curve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint):
    args = [curve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint]
    response = Util.ComputeFetch("rhino/geometry/curve/topolyline-curve_int_int_double_double_double_double_double_double_bool", args)
    return response


def ToPolyline1(curve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain):
    args = [curve, mainSegmentCount, subSegmentCount, maxAngleRadians, maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, keepStartPoint, curveDomain]
    response = Util.ComputeFetch("rhino/geometry/curve/topolyline-curve_int_int_double_double_double_double_double_double_bool_interval", args)
    return response


def ToPolyline2(curve, tolerance, angleTolerance, minimumLength, maximumLength):
    args = [curve, tolerance, angleTolerance, minimumLength, maximumLength]
    response = Util.ComputeFetch("rhino/geometry/curve/topolyline-curve_double_double_double_double", args)
    return response


def ToArcsAndLines(curve, tolerance, angleTolerance, minimumLength, maximumLength):
    args = [curve, tolerance, angleTolerance, minimumLength, maximumLength]
    response = Util.ComputeFetch("rhino/geometry/curve/toarcsandlines-curve_double_double_double_double", args)
    return response


def PullToMesh(curve, mesh, tolerance):
    args = [curve, mesh, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/pulltomesh-curve_mesh_double", args)
    return response


def Offset(curve, plane, distance, tolerance, cornerStyle):
    args = [curve, plane, distance, tolerance, cornerStyle]
    response = Util.ComputeFetch("rhino/geometry/curve/offset-curve_plane_double_double_curveoffsetcornerstyle", args)
    return response


def Offset1(curve, directionPoint, normal, distance, tolerance, cornerStyle):
    args = [curve, directionPoint, normal, distance, tolerance, cornerStyle]
    response = Util.ComputeFetch("rhino/geometry/curve/offset-curve_point3d_vector3d_double_double_curveoffsetcornerstyle", args)
    return response


def OffsetOnSurface(curve, face, distance, fittingTolerance):
    args = [curve, face, distance, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_brepface_double_double", args)
    return response


def OffsetOnSurface1(curve, face, throughPoint, fittingTolerance):
    args = [curve, face, throughPoint, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_brepface_point2d_double", args)
    return response


def OffsetOnSurface2(curve, face, curveParameters, offsetDistances, fittingTolerance):
    args = [curve, face, curveParameters, offsetDistances, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_brepface_doublearray_doublearray_double", args)
    return response


def OffsetOnSurface3(curve, surface, distance, fittingTolerance):
    args = [curve, surface, distance, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_surface_double_double", args)
    return response


def OffsetOnSurface4(curve, surface, throughPoint, fittingTolerance):
    args = [curve, surface, throughPoint, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_surface_point2d_double", args)
    return response


def OffsetOnSurface5(curve, surface, curveParameters, offsetDistances, fittingTolerance):
    args = [curve, surface, curveParameters, offsetDistances, fittingTolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetonsurface-curve_surface_doublearray_doublearray_double", args)
    return response


def PullToBrepFace(curve, face, tolerance):
    args = [curve, face, tolerance]
    response = Util.ComputeFetch("rhino/geometry/curve/pulltobrepface-curve_brepface_double", args)
    return response


def OffsetNormalToSurface(curve, surface, height):
    args = [curve, surface, height]
    response = Util.ComputeFetch("rhino/geometry/curve/offsetnormaltosurface-curve_surface_double", args)
    return response

