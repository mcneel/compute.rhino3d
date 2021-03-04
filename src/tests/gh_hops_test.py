import gh_hops_lib as Hops


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


components = [AdditionComponent()]
Hops.start_server(components, 5000)
