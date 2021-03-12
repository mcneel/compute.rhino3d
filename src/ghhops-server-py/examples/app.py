# add root repo path so module can be loaded
import os.path as op
import sys

sys.path.append(op.dirname(op.dirname(__file__)))
#

from flask import Flask
import ghhops_server as ghhs
import rhino3dm


# register hops app as middleware
app = Flask(__name__)
hops: ghhs.HopsFlask = ghhs.Hops(app)


# flask app can be used for other stuff drectly
@app.route("/help")
def help():
    return "Welcome to Grashopper Hops for CPython!"


# hops middleware is used to handle rhinocommon calls
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
    outputs=[ghhs.HopsPoint("P", "P", "Point on curve at t")],
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
    edge1 = rhino3dm.LineCurve(a, b)
    edge2 = rhino3dm.LineCurve(c, d)
    return rhino3dm.NurbsSurface.CreateRuledSurface(edge1, edge2)


if __name__ == "__main__":
    app.run(debug=True)