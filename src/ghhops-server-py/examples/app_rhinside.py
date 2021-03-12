"""Hops default HTTP server swith rhinoinside example"""
import rhinoinside
import ghhops_server as ghhs


rhinoinside.load()
# System and Rhino can only be loaded after rhinoinside is initialized
import System  # noqa
import Rhino  # noqa

hops = ghhs.Hops(app=rhinoinside)


@hops.component(
    "/add",
    name="Add",
    nickname="Add",
    description="Add numbers with CPython",
    category="Maths",
    subcategory="CPython",
    inputs=[
        ghhs.HopsNumber("A", "A", "First number"),
        ghhs.HopsNumber("B", "B", "Second number"),
    ],
    outputs=[ghhs.HopsNumber("Sum", "S", "A + B")],
)
def add(a, b):
    # testing error report
    f = 12 / 0
    return a + b


@hops.component(
    "/pointat",
    name="PointAt",
    nickname="PtAt",
    description="Get point along curve",
    category="Curve",
    subcategory="Analysis",
    icon="examples/pointat.png",
    inputs=[
        ghhs.HopsCurve("Curve", "C", "Curve to evaluate"),
        ghhs.HopsNumber("t", "t", "Parameter on Curve to evaluate"),
    ],
    outputs=[
        ghhs.HopsPoint(
            "P",
            "P",
            "Point on curve at t",
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
        ghhs.HopsPoint("Corner A", "A", "First corner"),
        ghhs.HopsPoint("Corner B", "B", "Second corner"),
        ghhs.HopsPoint("Corner C", "C", "Third corner"),
        ghhs.HopsPoint("Corner D", "D", "Fourth corner"),
    ],
    outputs=[ghhs.HopsSurface("Surface", "S", "Resulting surface")],
)
def ruled_surface(a, b, c, d):
    edge1 = Rhino.Geometry.LineCurve(a, b)
    edge2 = Rhino.Geometry.LineCurve(c, d)
    return Rhino.Geometry.NurbsSurface.CreateRuledSurface(edge1, edge2)


@hops.component(
    "/interplength",
    name="InterpCurve Length",
    nickname="ICL",
    inputs=[
        ghhs.HopsPoint("P1", "P1", "First point"),
        ghhs.HopsPoint("P2", "P2", "Second point"),
        ghhs.HopsPoint("P3", "P3", "Third point"),
    ],
    outputs=[ghhs.HopsNumber("Length", "L", "Interpolated curve length")],
)
def interp_length(p1, p2, p3):
    print(p1, p2, p3)
    pts = System.Collections.Generic.List[Rhino.Geometry.Point3d]()
    pts.Add(p1)
    pts.Add(p2)
    pts.Add(p3)
    crv = Rhino.Geometry.Curve.CreateInterpolatedCurve(pts, 3)
    return crv.GetLength()


if __name__ == "__main__":
    hops.start(debug=True)
