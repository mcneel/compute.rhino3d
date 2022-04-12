"""Hops default HTTP server swith rhinoinside example"""
import rhinoinside
import ghhops_server as hs


rhinoinside.load()
# System and Rhino can only be loaded after rhinoinside is initialized
import System  # noqa
import Rhino  # noqa

hops = hs.Hops(app=rhinoinside)


@hops.component(
    "/add",
    name="Add",
    nickname="Add",
    description="Add numbers with CPython",
    category="Maths",
    subcategory="CPython",
    inputs=[
        hs.HopsNumber("A", "A", "First number"),
        hs.HopsNumber("B", "B", "Second number"),
    ],
    outputs=[hs.HopsNumber("Sum", "S", "A + B")],
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
    icon="pointat.png",
    inputs=[
        hs.HopsCurve("Curve", "C", "Curve to evaluate"),
        hs.HopsNumber("t", "t", "Parameter on Curve to evaluate"),
    ],
    outputs=[
        hs.HopsPoint(
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
        hs.HopsPoint("Corner A", "A", "First corner"),
        hs.HopsPoint("Corner B", "B", "Second corner"),
        hs.HopsPoint("Corner C", "C", "Third corner"),
        hs.HopsPoint("Corner D", "D", "Fourth corner"),
    ],
    outputs=[hs.HopsSurface("Surface", "S", "Resulting surface")],
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
        hs.HopsPoint("P1", "P1", "First point"),
        hs.HopsPoint("P2", "P2", "Second point"),
        hs.HopsPoint("P3", "P3", "Third point"),
    ],
    outputs=[hs.HopsNumber("Length", "L", "Interpolated curve length")],
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
