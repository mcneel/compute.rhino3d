import ghhops_server as ghhs

import rhino3dm


hops = ghhs.Hops(None)


@hops.component(
    "/binsum",
    name="Binary Sum",
    nickname="BSum",
    inputs=[
        ghhs.HopsNumber("A"),
        ghhs.HopsNumber("B", access=ghhs.HopsParamAccess.LIST),
    ],
    outputs=[ghhs.HopsNumber("Sum")],
)
def BinarySum(a, b_list):
    b_sum = 0
    for b in b_list:
        b_sum += b
    return a + b_sum


@hops.component(
    "/binmult",
    inputs=[ghhs.HopsNumber("A"), ghhs.HopsNumber("B")],
    outputs=[ghhs.HopsNumber("Multiply")],
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
        ghhs.HopsNumber("A", "A", "First number", ghhs.HopsParamAccess.ITEM),
        ghhs.HopsNumber("B", "B", "Second number", ghhs.HopsParamAccess.ITEM),
    ],
    outputs=[ghhs.HopsNumber("Sum", "S", "A + B", ghhs.HopsParamAccess.ITEM)],
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
        ghhs.HopsCurve(
            "Curve", "C", "Curve to evaluate", ghhs.HopsParamAccess.ITEM
        ),
        ghhs.HopsNumber(
            "t",
            "t",
            "Parameter on Curve to evaluate",
            ghhs.HopsParamAccess.ITEM,
        ),
    ],
    outputs=[
        ghhs.HopsPoint(
            "P", "P", "Point on curve at t", ghhs.HopsParamAccess.ITEM
        )
    ],
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
    outputs=[
        HopsSurface("Surface", "S", "Resulting surface", HopsParamAccess.ITEM)
    ],
)
def ruled_surface(a, b, c, d):
    edge1 = rhino3dm.LineCurve(a, b)
    edge2 = rhino3dm.LineCurve(c, d)
    return rhino3dm.NurbsSurface.CreateRuledSurface(edge1, edge2)


hops.start()
