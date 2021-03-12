"""Hops Component Parameter wrappers"""
import json
from enum import Enum

from ghhops_server.base import _HopsEncoder
from ghhops_server.logger import hlogger


__all__ = (
    "HopsParamAccess",
    "HopsNumber",
    "HopsCurve",
    "HopsPoint",
    "HopsSurface",
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
    ):
        self.name = name
        self.nickname = nickname
        self.description = desc
        self.access: HopsParamAccess = access or HopsParamAccess.ITEM
        self.optional = optional

    def _coerce_value(self, param_type, param_data):
        # get data as dict
        data = json.loads(param_data)
        # parse data
        if isinstance(self.coercers, dict):
            if coercer := self.coercers.get(param_type, None):
                return coercer(data)
        elif param_type.startswith("Rhino.Geometry."):
            return RHINO_FROMJSON(data)
        return param_data

    def encode(self):
        """Parameter serializer"""
        param_def = {
            "Name": self.name,
            "Description": self.description,
            "ParamType": self.param_type,
            "ResultType": self.result_type,
            "AtLeast": 1,
        }
        if HopsParamAccess.ITEM == self.access:
            param_def["AtMost"] = 1
        return param_def

    def from_input(self, input_data):
        """Extract parameter data from serialized input"""
        # param_name = input_data["ParamName"]
        param_value_item = input_data["InnerTree"]["0"][0]
        param_type = param_value_item["type"]
        param_value = param_value_item["data"]
        return self._coerce_value(param_type, param_value)

    def from_result(self, value):
        """Serialize parameter with given value for output"""
        json_data = RHINO_TOJSON(value)

        output = {
            "ParamName": self.name,
            "InnerTree": {
                "0": [
                    {
                        "type": self.result_type,
                        "data": json_data,
                    }
                ]
            },
        }
        return output


class HopsNumber(_GHParam):
    """Wrapper for GH Number"""

    param_type = "Number"
    result_type = "System.Double"

    coercers = {
        "System.Double": lambda d: float(d),
    }


class HopsCurve(_GHParam):
    """Wrapper for GH Curve"""

    param_type = "Curve"
    result_type = "Rhino.Geometry.Curve"


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


class HopsSurface(_GHParam):
    """Wrapper for GH Surface"""

    param_type = "Surface"
    result_type = "Rhino.Geometry.Brep"
