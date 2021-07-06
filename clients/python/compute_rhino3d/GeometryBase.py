from . import Util
try:
    from itertools import izip as zip # python 2
except ImportError:
    pass # python 3


def GetBoundingBox(thisGeometryBase, accurate, multiple=False):
    """
    Bounding box solver. Gets the world axis aligned bounding box for the geometry.

    Args:
        accurate (bool): If true, a physically accurate bounding box will be computed.
            If not, a bounding box estimate will be computed. For some geometry types there is no
            difference between the estimate and the accurate bounding box. Estimated bounding boxes
            can be computed much (much) faster than accurate (or "tight") bounding boxes.
            Estimated bounding boxes are always similar to or larger than accurate bounding boxes.

    Returns:
        BoundingBox: The bounding box of the geometry in world coordinates or BoundingBox.Empty
        if not bounding box could be found.
    """
    url = "rhino/geometry/geometrybase/getboundingbox-geometrybase_bool"
    if multiple: url += "?multiple=true"
    args = [thisGeometryBase, accurate]
    if multiple: args = list(zip(thisGeometryBase, accurate))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToBoundingBox(response)
    return response


def GetBoundingBox1(thisGeometryBase, xform, multiple=False):
    """
    Aligned Bounding box solver. Gets the world axis aligned bounding box for the transformed geometry.

    Args:
        xform (Transform): Transformation to apply to object prior to the BoundingBox computation.
            The geometry itself is not modified.

    Returns:
        BoundingBox: The accurate bounding box of the transformed geometry in world coordinates
        or BoundingBox.Empty if not bounding box could be found.
    """
    url = "rhino/geometry/geometrybase/getboundingbox-geometrybase_transform"
    if multiple: url += "?multiple=true"
    args = [thisGeometryBase, xform]
    if multiple: args = list(zip(thisGeometryBase, xform))
    response = Util.ComputeFetch(url, args)
    response = Util.DecodeToBoundingBox(response)
    return response


def GeometryEquals(first, second, multiple=False):
    """
    Determines if two geometries equal one another, in pure geometrical shape.
    This version only compares the geometry itself and does not include any user
    data comparisons.
    This is a comparison by value: for two identical items it will be true, no matter
    where in memory they may be stored.

    Args:
        first (GeometryBase): The first geometry
        second (GeometryBase): The second geometry

    Returns:
        bool: The indication of equality
    """
    url = "rhino/geometry/geometrybase/geometryequals-geometrybase_geometrybase"
    if multiple: url += "?multiple=true"
    args = [first, second]
    if multiple: args = list(zip(first, second))
    response = Util.ComputeFetch(url, args)
    return response

