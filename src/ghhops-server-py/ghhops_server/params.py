"""Hops Component Parameter wrappers"""
import json
from enum import Enum
import inspect
from ghhops_server.base import _HopsEncoder
from ghhops_server.logger import hlogger


__all__ = (
    "HopsParamAccess",
    # "HopsArc",
    "HopsBoolean",
    # "HopsBox",
    "HopsBrep",
    # "HopsCircle",
    # "HopsColour",
    # "HopsComplex"
    # "HopsCulture",
    "HopsCurve",
    # "HopsField",
    # "HopsFilePath",
    # "HopsGeometry",
    "HopsInteger",
    # "HopsInterval",
    # "HopsInterval2D"
    "HopsLine",
    # "HopsMatrix",
    "HopsMesh",
    # "HopsMeshFace",
    "HopsNumber",
    # "HopsPlane",
    "HopsPoint",
    # "HopsRectangle",
    "HopsString",
    # "HopsStructurePath",
    "HopsSubD",
    "HopsSurface",
    # "HopsTime",
    # "HopsTransform",
    "HopsVector",
)


RHINO = None
RHINO_FROMJSON = None
RHINO_TOJSON = None
RHINO_GEOM = None


def _init_rhinoinside():
    global RHINO
    global RHINO_FROMJSON
    global RHINO_TOJSON
    global RHINO_GEOM

    # initialize with Rhino.Inside Cpython ==========
    import clr

    clr.AddReference("System.Collections")
    clr.AddReference("Newtonsoft.Json.Rhino")
    import Newtonsoft.Json as NJ
    from System.Collections.Generic import Dictionary

    def from_json(json_obj):
        """Convert to RhinoCommon from json"""
        data_dict = Dictionary[str, str]()
        for k, v in json_obj.items():
            data_dict[k] = str(v)
        return RHINO.Runtime.CommonObject.FromJSON(data_dict)

    def to_json(value):
        """Convert RhinoCommon object to json"""
        return NJ.JsonConvert.SerializeObject(value)

    RHINO_FROMJSON = from_json
    RHINO_TOJSON = to_json

    import Rhino

    RHINO = Rhino
    RHINO_GEOM = Rhino.Geometry


def _init_rhino3dm():
    global RHINO
    global RHINO_FROMJSON
    global RHINO_TOJSON
    global RHINO_GEOM

    import rhino3dm

    def from_json(json_obj):
        """Convert to rhino3dm from json"""
        return rhino3dm.CommonObject.Decode(json_obj)

    def to_json(value):
        """Convert rhino3dm object to json"""
        return json.dumps(value, cls=_HopsEncoder)

    RHINO_FROMJSON = from_json
    RHINO_TOJSON = to_json

    RHINO_GEOM = rhino3dm


class HopsParamAccess(Enum):
    """GH Item Access"""

    ITEM = 0
    LIST = 1
    TREE = 2


# TODO:
# - params can have icons too
# cast methods
class _GHParam:
    coercers = []
    param_type = None
    result_type = None

    def __init__(
        self,
        name,
        nickname=None,
        desc=None,
        access: HopsParamAccess = HopsParamAccess.ITEM,
        optional=False,
        default=None,
    ):
        self.name = name
        self.nickname = nickname
        self.description = desc
        self.access: HopsParamAccess = access or HopsParamAccess.ITEM
        self.optional = optional
        self.default = default or inspect.Parameter.empty

    def _coerce_value(self, param_type, param_data):
        # get data as dict
        data = json.loads(param_data)
        # parse data
        if isinstance(self.coercers, dict):
            coercer = self.coercers.get(param_type, None)
            if coercer:
                return coercer(data)
        elif param_type.startswith("Rhino.Geometry."):
            return RHINO_FROMJSON(data)
        return param_data

    def encode(self):
        """Parameter serializer"""
        param_def = {
            "Name": self.name,
            "Nickname": self.nickname,
            "Description": self.description,
            "ParamType": self.param_type,
            "ResultType": self.result_type,
            "AtLeast": 1,
        }
        if HopsParamAccess.ITEM == self.access:
            param_def["AtMost"] = 1
        if HopsParamAccess.LIST == self.access:
            param_def["AtMost"] = 2147483647  # Max 32 bit integer value
        if HopsParamAccess.TREE == self.access:
            param_def["AtLeast"] = -1
            param_def["AtMost"] = -1
        if self.default != inspect.Parameter.empty:
            param_def["Default"] = self.default
        return param_def

    def from_input(self, input_data):
        """Extract parameter data from serialized input"""
        if self.access == HopsParamAccess.TREE:
            paths = input_data["InnerTree"]
            tree = {}
            for k, v in paths.items():
                data = []
                for param_value_item in v:
                    param_type = param_value_item["type"]
                    param_value = param_value_item["data"]
                    data.append(self._coerce_value(param_type, param_value))
                tree[k] = data
            return tree

        data = []
        for param_value_item in input_data["InnerTree"]["0"]:
            param_type = param_value_item["type"]
            param_value = param_value_item["data"]
            data.append(self._coerce_value(param_type, param_value))
        if self.access == HopsParamAccess.ITEM:
            return data[0]
        return data

    def from_result(self, value):
        """Serialize parameter with given value for output"""
        if self.access == HopsParamAccess.TREE and isinstance(value, dict):
            tree = {}
            for key in value.keys():
                branch_data = [
                    {
                        "type": self.result_type,
                        "data": RHINO_TOJSON(v)
                    } for v in value[key]
                ]
                tree[key] = branch_data
            output = {
                "ParamName": self.name,
                "InnerTree": tree,
            }
            return output
        
        if not isinstance(value, tuple) and not isinstance(value, list):
            value = (value,)

        output_list = [
            {
                "type": self.result_type,
                "data": RHINO_TOJSON(v)
            } for v in value
        ]

        output = {
            "ParamName": self.name,
            "InnerTree": {
                "0": output_list
            },
        }
        return output


class HopsBoolean(_GHParam):
    """Wrapper for GH_Boolean"""

    param_type = "Boolean"
    result_type = "System.Boolean"

    coercers = {"System.Boolean": lambda b: bool(b)}


class HopsBrep(_GHParam):
    """Wrapper for GH Brep"""

    param_type = "Brep"
    result_type = "Rhino.Geometry.Brep"


class HopsCurve(_GHParam):
    """Wrapper for GH Curve"""

    param_type = "Curve"
    result_type = "Rhino.Geometry.Curve"


class HopsInteger(_GHParam):
    """Wrapper for GH_Integer"""

    param_type = "Integer"
    result_type = "System.Int32"

    coercers = {"System.Int32": lambda i: int(i)}


class HopsLine(_GHParam):
    """Wrapper for GH_Line"""

    param_type = "Line"
    result_type = "Rhino.Geometry.Line"

    coercers = {
        "Rhino.Geometry.Line": lambda l: RHINO_GEOM.Line(
            HopsLine._make_point(l["From"]), HopsLine._make_point(l["To"])
        )
    }

    @staticmethod
    def _make_point(a):
        return RHINO_GEOM.Point3d(a["X"], a["Y"], a["Z"])


class HopsMesh(_GHParam):
    """Wrapper for GH Mesh"""

    param_type = "Mesh"
    result_type = "Rhino.Geometry.Mesh"


class HopsNumber(_GHParam):
    """Wrapper for GH Number"""

    param_type = "Number"
    result_type = "System.Double"

    coercers = {
        "System.Double": lambda d: float(d),
    }


class HopsPoint(_GHParam):
    """Wrapper for GH Point"""

    param_type = "Point"
    result_type = "Rhino.Geometry.Point3d"

    coercers = {
        "Rhino.Geometry.Point2d": lambda d: RHINO_GEOM.Point2d(d["X"], d["Y"]),
        "Rhino.Geometry.Point3d": lambda d: RHINO_GEOM.Point3d(
            d["X"], d["Y"], d["Z"]
        ),
        "Rhino.Geometry.Vector3d": lambda d: RHINO_GEOM.Vector3d(
            d["X"], d["Y"], d["Z"]
        ),
    }


class HopsString(_GHParam):
    """Wrapper for GH_String"""

    param_type = "Text"
    result_type = "System.String"

    coercers = {"System.String": lambda s: s}


class HopsSubD(_GHParam):
    """Wrapper for GH SubD"""

    param_type = "SubD"
    result_type = "Rhino.Geometry.SubD"


class HopsSurface(_GHParam):
    """Wrapper for GH Surface"""

    param_type = "Surface"
    result_type = "Rhino.Geometry.Brep"


class HopsVector(_GHParam):
    """Wrapper for GH Vector"""

    param_type = "Vector"
    result_type = "Rhino.Geometry.Vector3d"

    coercers = {
        "Rhino.Geometry.Point2d": lambda d: RHINO_GEOM.Point2d(d["X"], d["Y"]),
        "Rhino.Geometry.Point3d": lambda d: RHINO_GEOM.Point3d(
            d["X"], d["Y"], d["Z"]
        ),
        "Rhino.Geometry.Vector3d": lambda d: RHINO_GEOM.Vector3d(
            d["X"], d["Y"], d["Z"]
        ),
    }
