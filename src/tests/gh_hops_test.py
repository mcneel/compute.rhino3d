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
        self.set_icon('pointat.png')

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


class RuledSurfaceComponent(Hops.Component):
    def __init__(self):
        super().__init__("4Point Surface", "Srf4Pt", "Create ruled surface from four points", "Surface", "Freeform")

    def register_input_params(self, inputs):
        inputs.add_point_parameter("Corner A", "A", "First corner", Hops.ParamAccess.ITEM)
        inputs.add_point_parameter("Corner B", "B", "Second corner", Hops.ParamAccess.ITEM)
        inputs.add_point_parameter("Corner C", "C", "Third corner", Hops.ParamAccess.ITEM)
        inputs.add_point_parameter("Corner D", "D", "Fourth corner", Hops.ParamAccess.ITEM)

    def register_output_params(self, outputs):
        outputs.add_surface_parameter("Surface", "S", "Resulting surface", Hops.ParamAccess.ITEM)

    def solve_instance(self, data_access):
        success, a = data_access.getdata(0)
        if not success:
            return
        success, b = data_access.getdata(1)
        if not success:
            return
        success, c = data_access.getdata(2)
        if not success:
            return
        success, d = data_access.getdata(3)
        if not success:
            return

        edge1 = rhino3dm.LineCurve(a, b)
        edge2 = rhino3dm.LineCurve(c, d)
        srf = rhino3dm.NurbsSurface.CreateRuledSurface(edge1, edge2)
        data_access.setdata(0, srf)


components = [
    AdditionComponent(),
    PointAtComponent(),
    RuledSurfaceComponent()
    ]
Hops.start_server(components, 5000)
