from hops import Hops
from hops import middlewares as mw
from hops.params import *

hops: mw.HopsDefault = Hops(None)


@hops.component(
    "/binsum",
    name="Binrary Sum",
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


hops.start()  # pylint: disable=no-member
