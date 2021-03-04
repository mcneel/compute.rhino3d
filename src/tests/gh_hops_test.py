import gh_hops_lib as Hops
import rhino3dm

class AdditionComponent(Hops.Component):
    def __init__(self):
        super().__init__("Add", "Add", "Add numbers with CPython", "Maths", "CPython")

    def register_input_params(self, inputs):
        inputs.add_number_parameter("A", "A", "First number", Hops.ParamAccess.ITEM)
        inputs.add_number_parameter("B", "B", "Second number", Hops.ParamAccess.ITEM)

    def register_output_params(self, outputs):
        outputs.add_number_parameter("Sum", "S", "A + B", Hops.ParamAccess.ITEM)

    def solve_instance(self, data_access):
        success, a = data_access.getdata(0)
        if not success:
            return
        success, b = data_access.getdata(1)
        if not success:
            return
        sum = a + b
        data_access.setdata(0, sum)


class PointAtComponent(Hops.Component):
    def __init__(self):
        super().__init__("PointAt", "PtAt", "Get point along curve", "Curve", "Analysis")

    def register_input_params(self, inputs):
        inputs.add_curve_parameter("Curve", "C", "Curve to evaluate", Hops.ParamAccess.ITEM)
        inputs.add_number_parameter("t", "t", "Parameter on Curve to evaluate", Hops.ParamAccess.ITEM)

    def register_output_params(self, outputs):
        outputs.add_point_parameter("P", "P", "Point on curve at t", Hops.ParamAccess.ITEM)

    def solve_instance(self, data_access):
        success, curve = data_access.getdata(0)
        if not success:
            return
        success, t = data_access.getdata(1)
        if not success:
            return
        point = curve.PointAt(t)
        data_access.setdata(0, point)


components = [
    AdditionComponent(),
    PointAtComponent()
    ]
Hops.start_server(components, 5000)
