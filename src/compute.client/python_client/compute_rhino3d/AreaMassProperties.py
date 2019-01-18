from . import Util


def Compute(closedPlanarCurve, multiple=False):
    """
    Computes an AreaMassProperties for a closed planar curve.

    Args:
        closedPlanarCurve (Curve): Curve to measure.

    Returns:
        AreaMassProperties: The AreaMassProperties for the given curve or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-curve"
    if multiple: url += "?multiple=true"
    args = [closedPlanarCurve]
    if multiple: args = [[item] for item in closedPlanarCurve]
    response = Util.ComputeFetch(url, args)
    return response


def Compute1(closedPlanarCurve, planarTolerance, multiple=False):
    """
    Computes an AreaMassProperties for a closed planar curve.

    Args:
        closedPlanarCurve (Curve): Curve to measure.
        planarTolerance (double): absolute tolerance used to insure the closed curve is planar

    Returns:
        AreaMassProperties: The AreaMassProperties for the given curve or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-curve_double"
    if multiple: url += "?multiple=true"
    args = [closedPlanarCurve, planarTolerance]
    if multiple: args = zip(closedPlanarCurve, planarTolerance)
    response = Util.ComputeFetch(url, args)
    return response


def Compute2(hatch, multiple=False):
    """
    Computes an AreaMassProperties for a hatch.

    Args:
        hatch (Hatch): Hatch to measure.

    Returns:
        AreaMassProperties: The AreaMassProperties for the given hatch or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-hatch"
    if multiple: url += "?multiple=true"
    args = [hatch]
    if multiple: args = [[item] for item in hatch]
    response = Util.ComputeFetch(url, args)
    return response


def Compute3(mesh, multiple=False):
    """
    Computes an AreaMassProperties for a mesh.

    Args:
        mesh (Mesh): Mesh to measure.

    Returns:
        AreaMassProperties: The AreaMassProperties for the given Mesh or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-mesh"
    if multiple: url += "?multiple=true"
    args = [mesh]
    if multiple: args = [[item] for item in mesh]
    response = Util.ComputeFetch(url, args)
    return response


def Compute4(mesh, area, firstMoments, secondMoments, productMoments, multiple=False):
    """
    Compute the AreaMassProperties for a single Mesh.

    Args:
        mesh (Mesh): Mesh to measure.
        area (bool): True to calculate area.
        firstMoments (bool): True to calculate area first moments, area, and area centroid.
        secondMoments (bool): True to calculate area second moments.
        productMoments (bool): True to calculate area product moments.

    Returns:
        AreaMassProperties: The AreaMassProperties for the given Mesh or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-mesh_bool_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [mesh, area, firstMoments, secondMoments, productMoments]
    if multiple: args = zip(mesh, area, firstMoments, secondMoments, productMoments)
    response = Util.ComputeFetch(url, args)
    return response


def Compute5(brep, multiple=False):
    """
    Computes an AreaMassProperties for a brep.

    Args:
        brep (Brep): Brep to measure.

    Returns:
        AreaMassProperties: The AreaMassProperties for the given Brep or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-brep"
    if multiple: url += "?multiple=true"
    args = [brep]
    if multiple: args = [[item] for item in brep]
    response = Util.ComputeFetch(url, args)
    return response


def Compute6(brep, area, firstMoments, secondMoments, productMoments, multiple=False):
    """
    Compute the AreaMassProperties for a single Brep.

    Args:
        brep (Brep): Brep to measure.
        area (bool): True to calculate area.
        firstMoments (bool): True to calculate area first moments, area, and area centroid.
        secondMoments (bool): True to calculate area second moments.
        productMoments (bool): True to calculate area product moments.

    Returns:
        AreaMassProperties: The AreaMassProperties for the given Brep or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-brep_bool_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [brep, area, firstMoments, secondMoments, productMoments]
    if multiple: args = zip(brep, area, firstMoments, secondMoments, productMoments)
    response = Util.ComputeFetch(url, args)
    return response


def Compute7(surface, multiple=False):
    """
    Computes an AreaMassProperties for a surface.

    Args:
        surface (Surface): Surface to measure.

    Returns:
        AreaMassProperties: The AreaMassProperties for the given Surface or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-surface"
    if multiple: url += "?multiple=true"
    args = [surface]
    if multiple: args = [[item] for item in surface]
    response = Util.ComputeFetch(url, args)
    return response


def Compute8(surface, area, firstMoments, secondMoments, productMoments, multiple=False):
    """
    Compute the AreaMassProperties for a single Surface.

    Args:
        surface (Surface): Surface to measure.
        area (bool): True to calculate area.
        firstMoments (bool): True to calculate area first moments, area, and area centroid.
        secondMoments (bool): True to calculate area second moments.
        productMoments (bool): True to calculate area product moments.

    Returns:
        AreaMassProperties: The AreaMassProperties for the given Surface or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-surface_bool_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [surface, area, firstMoments, secondMoments, productMoments]
    if multiple: args = zip(surface, area, firstMoments, secondMoments, productMoments)
    response = Util.ComputeFetch(url, args)
    return response


def Compute9(geometry, multiple=False):
    """
    Computes the Area properties for a collection of geometric objects.
    At present only Breps, Surfaces, Meshes and Planar Closed Curves are supported.

    Args:
        geometry (IEnumerable<GeometryBase>): Objects to include in the area computation.

    Returns:
        AreaMassProperties: The Area properties for the entire collection or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-geometrybasearray"
    if multiple: url += "?multiple=true"
    args = [geometry]
    if multiple: args = [[item] for item in geometry]
    response = Util.ComputeFetch(url, args)
    return response


def Compute10(geometry, area, firstMoments, secondMoments, productMoments, multiple=False):
    """
    Computes the AreaMassProperties for a collection of geometric objects.
    At present only Breps, Surfaces, Meshes and Planar Closed Curves are supported.

    Args:
        geometry (IEnumerable<GeometryBase>): Objects to include in the area computation.
        area (bool): True to calculate area.
        firstMoments (bool): True to calculate area first moments, area, and area centroid.
        secondMoments (bool): True to calculate area second moments.
        productMoments (bool): True to calculate area product moments.

    Returns:
        AreaMassProperties: The AreaMassProperties for the entire collection or None on failure.
    """
    url = "rhino/geometry/areamassproperties/compute-geometrybasearray_bool_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [geometry, area, firstMoments, secondMoments, productMoments]
    if multiple: args = zip(geometry, area, firstMoments, secondMoments, productMoments)
    response = Util.ComputeFetch(url, args)
    return response


