from flask import Flask
from hops import Hops
from hops.params import *

# register hops app as middleware
app = Flask(__name__)
hops = Hops(app)


# flask app can be used for other stuff drectly
@app.route("/help")
def help():
    return f"Welcome to Grashopper Hops for CPython!"


# hops middleware is used to handle rhinocommon calls
@hops.component(
    "/binsum",
    name="Binrary Sum",
    nickname="BSum",
    inputs=[HInt("A"), HStr("B")],
    outputs=[HStr("Sum")],
)
def BinarySum(a, b):
    return f"Hello, {a + b}!"
