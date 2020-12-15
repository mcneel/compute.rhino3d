from . import Util
try:
    from itertools import izip as zip # python 2
except ImportError:
    pass # python 3


def Compute(mesh, multiple=False):
    """
    Compute the VolumeMassProperties for a single Mesh.

    Args:
        mesh (Mesh): Mesh to measure.

    Returns:
        VolumeMassProperties: The VolumeMassProperties for the given Mesh or None on failure.
    """
    url = "rhino/geometry/volumemassproperties/compute-mesh"
    if multiple: url += "?multiple=true"
    args = [mesh]
    if multiple: args = [[item] for item in mesh]
    response = Util.ComputeFetch(url, args)
    return response


def Compute1(mesh, volume, firstMoments, secondMoments, productMoments, multiple=False):
    """
    Compute the VolumeMassProperties for a single Mesh.

    Args:
        mesh (Mesh): Mesh to measure.
        volume (bool): True to calculate volume.
        firstMoments (bool): True to calculate volume first moments, volume, and volume centroid.
        secondMoments (bool): True to calculate volume second moments.
        productMoments (bool): True to calculate volume product moments.

    Returns:
        VolumeMassProperties: The VolumeMassProperties for the given Mesh or None on failure.
    """
    url = "rhino/geometry/volumemassproperties/compute-mesh_bool_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [mesh, volume, firstMoments, secondMoments, productMoments]
    if multiple: args = list(zip(mesh, volume, firstMoments, secondMoments, productMoments))
    response = Util.ComputeFetch(url, args)
    return response


def Compute2(brep, multiple=False):
    """
    Compute the VolumeMassProperties for a single Brep.

    Args:
        brep (Brep): Brep to measure.

    Returns:
        VolumeMassProperties: The VolumeMassProperties for the given Brep or None on failure.
    """
    url = "rhino/geometry/volumemassproperties/compute-brep"
    if multiple: url += "?multiple=true"
    args = [brep]
    if multiple: args = [[item] for item in brep]
    response = Util.ComputeFetch(url, args)
    return response


def Compute3(brep, volume, firstMoments, secondMoments, productMoments, multiple=False):
    """
    Compute the VolumeMassProperties for a single Brep.

    Args:
        brep (Brep): Brep to measure.
        volume (bool): True to calculate volume.
        firstMoments (bool): True to calculate volume first moments, volume, and volume centroid.
        secondMoments (bool): True to calculate volume second moments.
        productMoments (bool): True to calculate volume product moments.

    Returns:
        VolumeMassProperties: The VolumeMassProperties for the given Brep or None on failure.
    """
    url = "rhino/geometry/volumemassproperties/compute-brep_bool_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [brep, volume, firstMoments, secondMoments, productMoments]
    if multiple: args = list(zip(brep, volume, firstMoments, secondMoments, productMoments))
    response = Util.ComputeFetch(url, args)
    return response


def Compute4(surface, multiple=False):
    """
    Compute the VolumeMassProperties for a single Surface.

    Args:
        surface (Surface): Surface to measure.

    Returns:
        VolumeMassProperties: The VolumeMassProperties for the given Surface or None on failure.
    """
    url = "rhino/geometry/volumemassproperties/compute-surface"
    if multiple: url += "?multiple=true"
    args = [surface]
    if multiple: args = [[item] for item in surface]
    response = Util.ComputeFetch(url, args)
    return response


def Compute5(surface, volume, firstMoments, secondMoments, productMoments, multiple=False):
    """
    Compute the VolumeMassProperties for a single Surface.

    Args:
        surface (Surface): Surface to measure.
        volume (bool): True to calculate volume.
        firstMoments (bool): True to calculate volume first moments, volume, and volume centroid.
        secondMoments (bool): True to calculate volume second moments.
        productMoments (bool): True to calculate volume product moments.

    Returns:
        VolumeMassProperties: The VolumeMassProperties for the given Surface or None on failure.
    """
    url = "rhino/geometry/volumemassproperties/compute-surface_bool_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [surface, volume, firstMoments, secondMoments, productMoments]
    if multiple: args = list(zip(surface, volume, firstMoments, secondMoments, productMoments))
    response = Util.ComputeFetch(url, args)
    return response


def Compute6(geometry, multiple=False):
    """
    Computes the VolumeMassProperties for a collection of geometric objects.
    At present only Breps, Surfaces, and Meshes are supported.

    Args:
        geometry (IEnumerable<GeometryBase>): Objects to include in the area computation.

    Returns:
        VolumeMassProperties: The VolumeMassProperties for the entire collection or None on failure.
    """
    url = "rhino/geometry/volumemassproperties/compute-geometrybasearray"
    if multiple: url += "?multiple=true"
    args = [geometry]
    if multiple: args = [[item] for item in geometry]
    response = Util.ComputeFetch(url, args)
    return response


def Compute7(geometry, volume, firstMoments, secondMoments, productMoments, multiple=False):
    """
    Computes the VolumeMassProperties for a collection of geometric objects.
    At present only Breps, Surfaces, Meshes and Planar Closed Curves are supported.

    Args:
        geometry (IEnumerable<GeometryBase>): Objects to include in the area computation.
        volume (bool): True to calculate volume.
        firstMoments (bool): True to calculate volume first moments, volume, and volume centroid.
        secondMoments (bool): True to calculate volume second moments.
        productMoments (bool): True to calculate volume product moments.

    Returns:
        VolumeMassProperties: The VolumeMassProperties for the entire collection or None on failure.
    """
    url = "rhino/geometry/volumemassproperties/compute-geometrybasearray_bool_bool_bool_bool"
    if multiple: url += "?multiple=true"
    args = [geometry, volume, firstMoments, secondMoments, productMoments]
    if multiple: args = list(zip(geometry, volume, firstMoments, secondMoments, productMoments))
    response = Util.ComputeFetch(url, args)
    return response


def Sum(thisVolumeMassProperties, summand, multiple=False):
    """
    Sum mass properties together to get an aggregate mass.

    Args:
        summand (VolumeMassProperties): mass properties to add.

    Returns:
        bool: True if successful.
    """
    url = "rhino/geometry/volumemassproperties/sum-volumemassproperties_volumemassproperties"
    if multiple: url += "?multiple=true"
    args = [thisVolumeMassProperties, summand]
    if multiple: args = list(zip(thisVolumeMassProperties, summand))
    response = Util.ComputeFetch(url, args)
    return response


