"""Hops Component Parameter wrappers"""
import json
from enum import Enum

import rhino3dm

from hops.base import _HopsEncoder

__all__ = [
    "HopsParamAccess",
    "HopsNumber",
    "HopsCurve",
    "HopsPoint",
    "HopsSurface",
]


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

    def __init__(self, name, nickname, desc, access: HopsParamAccess, optional=False):
        self.name = name
        self.nickname = nickname
        self.description = desc
        self.access: HopsParamAccess = access or HopsParamAccess.ITEM
        self.optional = optional

    def _coerce_value(self, param_type, param_data):
        data = json.loads(param_data)
        if isinstance(self.coercers, dict):
            if coercer := self.coercers.get(param_type, None):
                return coercer(data)
        elif param_type.startswith("Rhino.Geometry."):
            return rhino3dm.CommonObject.Decode(data)
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
        output = {
            "ParamName": self.name,
            "InnerTree": {
                "0": [
                    {
                        "type": self.result_type,
                        "data": json.dumps(value, cls=_HopsEncoder),
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

    def __init__(
        self,
        name,
        nickname=None,
        desc=None,
        access: HopsParamAccess = None,
        optional=False,
    ):
        super(HopsNumber, self).__init__(name, nickname, desc, access, optional)


class HopsCurve(_GHParam):
    """Wrapper for GH Curve"""

    param_type = "Curve"
    result_type = "Rhino.Geometry.Curve"

    def __init__(
        self,
        name,
        nickname=None,
        desc=None,
        access: HopsParamAccess = None,
        optional=False,
    ):
        super(HopsCurve, self).__init__(name, nickname, desc, access, optional)


class HopsPoint(_GHParam):
    """Wrapper for GH Point"""

    param_type = "Point"
    result_type = "Rhino.Geometry.Point3d"

    coercers = {
        "Rhino.Geometry.Point2d": lambda d: rhino3dm.Point2d(d["X"], d["Y"]),
        "Rhino.Geometry.Point3d": lambda d: rhino3dm.Point3d(d["X"], d["Y"], d["Z"]),
        "Rhino.Geometry.Vector3d": lambda d: rhino3dm.Vector3d(d["X"], d["Y"], d["Z"]),
    }

    def __init__(
        self,
        name,
        nickname=None,
        desc=None,
        access: HopsParamAccess = None,
        optional=False,
    ):
        super(HopsPoint, self).__init__(name, nickname, desc, access, optional)


class HopsSurface(_GHParam):
    """Wrapper for GH Surface"""

    param_type = "Surface"
    result_type = "Rhino.Geometry.Brep"

    def __init__(
        self,
        name,
        nickname=None,
        desc=None,
        access: HopsParamAccess = None,
        optional=False,
    ):
        super(HopsSurface, self).__init__(name, nickname, desc, access, optional)
