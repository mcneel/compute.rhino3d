import json
from enum import Enum

import rhino3dm

from hops.base import _HopsEncoder


class HopsParamAccess(Enum):
    ITEM = 0
    LIST = 1
    TREE = 2


# TODO:
# - params can have icons too
# cast methods
class _GHParam:
    coercers = []

    def __init__(self, name, nickname, desc, access: HopsParamAccess):
        self.name = name
        self.nickname = nickname
        self.description = desc
        self.access: HopsParamAccess = access or HopsParamAccess.ITEM
        self.param_type: type = self.__class__.__name__
        self.result_type: type = self.__class__.__name__

    def _coerce_value(self, param_type, param_data):
        if isinstance(self.coercers, dict):
            if coercer := self.coercers.get(param_type, None):
                return coercer(param_data)
            elif param_type.startswith("Rhino.Geometry."):
                return rhino3dm.CommonObject.Decode(param_data)
        return param_data

    def encode(self):
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
        param_name = input_data["ParamName"]
        param_value_item = input_data["InnerTree"]["0"][0]
        param_type = param_value_item["type"]
        param_value = param_value_item["data"]
        return self._coerce_value(param_type, param_value)

    def from_result(self, value):
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
    coercers = {
        "System.Double": lambda d: float(d),
    }

    def __init__(self, name, nickname=None, desc=None, access: HopsParamAccess = None):
        super(HopsNumber, self).__init__(name, nickname, desc, access)


class HopsCurve(_GHParam):
    def __init__(self, name, nickname=None, desc=None, access: HopsParamAccess = None):
        super(HopsCurve, self).__init__(name, nickname, desc, access)


class HopsPoint(_GHParam):
    coercers = {
        "Rhino.Geometry.Point2d": lambda d: rhino3dm.Point2d(d["X"], d["Y"]),
        "Rhino.Geometry.Point3d": lambda d: rhino3dm.Point3d(d["X"], d["Y"], d["Z"]),
        "Rhino.Geometry.Vector3d": lambda d: rhino3dm.Vector3d(d["X"], d["Y"], d["Z"]),
    }

    def __init__(self, name, nickname=None, desc=None, access: HopsParamAccess = None):
        super(HopsPoint, self).__init__(name, nickname, desc, access)


class HopsSurface(_GHParam):
    def __init__(self, name, nickname=None, desc=None, access: HopsParamAccess = None):
        super(HopsSurface, self).__init__(name, nickname, desc, access)
