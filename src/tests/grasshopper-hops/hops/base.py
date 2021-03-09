"""Base types for Hops middleware"""
import json
import base64


DEFAULT_CATEGORY = "Hops"
DEFAULT_SUBCATEGORY = "Hops Python"


class _HopsEncoder(json.JSONEncoder):
    def default(self, o):
        if hasattr(o, "Encode"):
            return o.Encode()
        elif hasattr(o, "encode"):
            return o.encode()
        return json.JSONEncoder.default(self, o)


class HopsComponent:
    """Hops Component"""

    def __init__(
        self, uri, name, nickname, desc, cat, subcat, icon, inputs, outputs, handler
    ):
        self.uri = uri
        # TODO: customize solve uri?
        self.solve_uri = uri
        self.name = name
        self.nickname = nickname
        self.description = desc
        self.category = cat
        self.subcategory = subcat
        self.icon = icon
        self.inputs = inputs or []
        self.outputs = outputs or []
        self.handler = handler

    def encode(self):
        """Serializer"""
        metadata = {
            "Description": self.description,
            "Inputs": self.inputs,
            "Outputs": self.outputs,
        }
        if self.icon:
            metadata["Icon"] = self.icon
        return metadata


class HopsBase:
    """Base class for all Hops middleware implementations"""

    def __init__(self, app):
        self.app = app
        # components dict store each components two times under
        # two keys get uri and solve uri, for faster lookups in query and solve
        # it is assumed that uri and solve uri and both unique to the component
        self._components: dict[str, HopsComponent] = {}

    def query(self, uri) -> tuple[bool, str]:
        """Get information on given uri"""
        if uri == "/":
            return True, self._get_comps_data()
        elif comp := self._components.get(uri, None):
            return True, self._get_comp_data(comp)
        return False, "Unknown uri"

    def solve(self, uri, payload) -> tuple[bool, str]:
        """Perform Solve on given uri"""
        if uri == "/":
            return False, ""

        # FIXME: remove support for legacy solve behaviour
        elif uri == "/solve":
            data = json.loads(payload)
            comp_name = data["pointer"]
            for comp in self._components.values():
                if comp_name == comp.uri.replace("/", ""):
                    return self._process_solve_request(comp, payload)

        # FIXME: test this new api
        elif comp := self._components.get(uri, None):
            return self._process_solve_request(comp, payload)
        return False, "Unknown uri"

    def _get_comps_data(self):
        return json.dumps(list(self._components.values()), cls=_HopsEncoder)

    def _get_comp_data(self, comp):
        return json.dumps(comp, cls=_HopsEncoder)

    def _prepare_icon(self, icon_file_path):
        with open(icon_file_path, "rb") as image_file:
            base64_bytes = base64.b64encode(image_file.read())
            return base64_bytes.decode("ascii")

    def _process_solve_request(self, comp, payload) -> tuple[bool, str]:
        # parse payload for inputs
        res, inputs = self._prepare_inputs(comp, payload)
        if not res:
            return res, "Bad inputs"

        # run
        try:
            solve_returned = self._solve(comp, inputs)
            res, outputs = self._prepare_outputs(comp, solve_returned)
            return res, outputs if res else "Bad outputs"
        except Exception as solve_ex:  # pylint: disable=broad-except
            return False, str(solve_ex)

    def _prepare_inputs(self, comp, payload) -> tuple[bool, list]:
        # parse input payload
        data = json.loads(payload)

        # grab input param data and value items
        # FIXME: this works on a single branch only? ["0"][0]
        param_values = {}
        for item in data["values"]:
            param_values[item["ParamName"]] = item

        inputs = []
        for in_param in comp.inputs:
            if in_param.name not in param_values and not in_param.optional:
                return False, f"Missing value for required input {in_param.name}"
            in_param_data = param_values[in_param.name]
            value = in_param.from_input(in_param_data)
            inputs.append(value)

        return True, inputs

    def _solve(self, comp, inputs):
        return comp.handler(*inputs)

    def _prepare_outputs(self, comp, returns) -> tuple[bool, str]:
        outputs = []
        if not isinstance(returns, tuple):
            returns = (returns,)
        for out_param, out_result in zip(comp.outputs, returns):
            output_data = out_param.from_result(out_result)
            outputs.append(output_data)
        payload = {"values": outputs}
        return True, json.dumps(payload, cls=_HopsEncoder)

    def component(
        self,
        rule=None,
        name=None,
        nickname=None,
        description=None,
        category=None,
        subcategory=None,
        icon=None,
        inputs=None,
        outputs=None,
    ):
        """Decorator for Hops middleware"""

        def __func_wrapper__(comp_func):
            # register inputs and outputs
            comp_name = name or comp_func.__qualname__
            uri = rule or f"/{comp_name}"
            comp = HopsComponent(
                uri=uri,
                name=comp_name,
                nickname=nickname,
                desc=description or comp_func.__doc__,
                cat=category or DEFAULT_CATEGORY,
                subcat=subcategory or DEFAULT_SUBCATEGORY,
                icon=self._prepare_icon(icon) if icon is not None else None,
                inputs=inputs or [],
                outputs=outputs or [],
                handler=comp_func,
            )
            self._components[uri] = comp
            self._components[comp.solve_uri] = comp
            return comp_func

        return __func_wrapper__
