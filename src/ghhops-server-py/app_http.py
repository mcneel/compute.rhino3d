from hops import Hops
from hops import middlewares as mw
from hops.params import *

import rhino3dm


hops: mw.HopsDefault = Hops(None)


@hops.component(
    "/binsum",
    name="Binary Sum",
    nickname="BSum",
    inputs=[HopsNumber("A"), HopsNumber("B", access=HopsParamAccess.LIST)],
    outputs=[HopsNumber("Sum")],
)
def BinarySum(a, b_list):
    b_sum = 0
    for b in b_list:
        b_sum += b
    return a + b_sum


@hops.component(
    "/binmult",
    inputs=[HopsNumber("A"), HopsNumber("B")],
    outputs=[HopsNumber("Multiply")],
)
def BinaryMultiply(a, b):
    return a * b


@hops.component(
    "/add",
    name="Add",
    nickname="Add",
    description="Add numbers with CPython",
    category="Maths",
    subcategory="CPython",
    inputs=[
        HopsNumber("A", "A", "First number", HopsParamAccess.ITEM),
        HopsNumber("B", "B", "Second number", HopsParamAccess.ITEM),
    ],
    outputs=[HopsNumber("Sum", "S", "A + B", HopsParamAccess.ITEM)],
)
def add(a, b):
    return a + b


@hops.component(
    "/pointat",
    name="PointAt",
    nickname="PtAt",
    description="Get point along curve",
    category="Curve",
    subcategory="Analysis",
    icon="pointat.png",
    inputs=[
        HopsCurve("Curve", "C", "Curve to evaluate", HopsParamAccess.ITEM),
        HopsNumber("t", "t", "Parameter on Curve to evaluate", HopsParamAccess.ITEM),
    ],
    outputs=[HopsPoint("P", "P", "Point on curve at t", HopsParamAccess.ITEM)],
)
def pointat(curve, t):
    return curve.PointAt(t)


@hops.component(
    "/srf4pt",
    name="4Point Surface",
    nickname="Srf4Pt",
    description="Create ruled surface from four points",
    category="Surface",
    subcategory="Freeform",
    inputs=[
        HopsPoint("Corner A", "A", "First corner", HopsParamAccess.ITEM),
        HopsPoint("Corner B", "B", "Second corner", HopsParamAccess.ITEM),
        HopsPoint("Corner C", "C", "Third corner", HopsParamAccess.ITEM),
        HopsPoint("Corner D", "D", "Fourth corner", HopsParamAccess.ITEM),
    ],
    outputs=[HopsSurface("Surface", "S", "Resulting surface", HopsParamAccess.ITEM)],
)
def ruled_surface(a, b, c, d):
    edge1 = rhino3dm.LineCurve(a, b)
    edge2 = rhino3dm.LineCurve(c, d)
    return rhino3dm.NurbsSurface.CreateRuledSurface(edge1, edge2)


hops.start()  # pylint: disable=no-member
