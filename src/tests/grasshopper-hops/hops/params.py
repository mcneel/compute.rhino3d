from enum import Enum


class GHParamAccess(Enum):
    ITEM = 0
    LIST = 1
    TREE = 2


class _GHParam:
    def __init__(self, name):
        self.name = name


class HInt(_GHParam):
    def __init__(self, name):
        super(HInt, self).__init__(name)


class HStr(_GHParam):
    def __init__(self, name):
        super(HStr, self).__init__(name)


class HBrep(_GHParam):
    def __init__(self, name):
        super(HBrep, self).__init__(name)
