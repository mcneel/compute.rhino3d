import json
import base64
from collections import namedtuple

from flask import Flask


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
        metadata = self.__dict__.copy()
        metadata.pop("handler")
        return metadata


class HopsBase:
    def __init__(self, app):
        self.app = app
        # components dict store each components two times under
        # two keys get uri and solve uri, for faster lookups in query and solve
        # it is assumed that uri and solve uri and both unique to the component
        self._components: dict[str, HopsComponent] = {}

    def query(self, uri) -> tuple[bool, str]:
        if uri == "/":
            return True, self._get_comps_data()
        elif comp := self._components.get(uri, None):
            return True, self._get_comp_data(comp)
        return False, "Unknown uri"

    def solve(self, uri, payload) -> tuple[bool, str]:
        if uri == "/":
            return False, ""

        elif comp := self._components.get(uri, None):
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
        return False, "Unknown uri"

    def _get_comps_data(self):
        return json.dumps(list(self._components.values()), cls=_HopsEncoder)

    def _get_comp_data(self, comp):
        return json.dumps(comp)

    def _prepare_icon(self, icon_file_path):
        with open(icon_file_path, "rb") as image_file:
            base64_bytes = base64.b64encode(image_file.read())
            return base64_bytes.decode("ascii")

    def _prepare_inputs(self, comp, payload) -> tuple[bool, list]:
        # parse input payload
        data = json.loads(payload)

        # grab input param data and value items
        # FIXME: this works on a single branch only? ["0"][0]
        param_values = {}
        for d in data["values"]:
            param_values[d["ParamName"]] = d

        inputs = []
        registered_input_names = [x.name for x in comp.inputs]
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
        return True, json.dumps(outputs)

    def component(
        self,
        rule=None,
        name=None,
        nickname=None,
        description=None,
        category=None,
        subcategory=None,
        icon=None,
        inputs=[],
        outputs=[],
    ):
        def __func_wrapper__(f):
            # register inputs and outputs
            comp_name = name or f.__qualname__
            uri = rule or f"/{comp_name}"
            comp = HopsComponent(
                uri=uri,
                name=comp_name,
                nickname=nickname,
                desc=description or f.__doc__,
                cat=category or DEFAULT_CATEGORY,
                subcat=subcategory or DEFAULT_SUBCATEGORY,
                icon=self._prepare_icon(icon) if icon is not None else None,
                inputs=inputs,
                outputs=outputs,
                handler=f,
            )
            self._components[uri] = comp
            self._components[comp.solve_uri] = comp
            return f

        return __func_wrapper__
