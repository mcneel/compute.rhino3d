import json
import base64
from collections import namedtuple

from flask import Flask


HopsComponent = namedtuple(
    "HopsComponent", ["uri", "name", "desc", "icon", "inputs", "outputs", "view_func"]
)


class HopsBase:
    def __init__(self, app):
        self.app = app
        self.components = {"/": self.GET_comp_names}

    def GET_comp_names(self):
        return [c.name for c in self.components]

    def prepare_icon(self, icon_file_path):
        with open(icon_file_path, "rb") as image_file:
            base64_bytes = base64.b64encode(image_file.read())
            return base64_bytes.decode("ascii")

    def prepare_inputs(self, environ, component):
        return [10, 20]

    def solve(self, component, *args):
        return component.view_func(*args)

    def prepare_outputs(self, comp_returns):
        return json.dumps(comp_returns)

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
            uri = rule or f"/{f.__qualname__}"
            self.components[uri] = HopsComponent(
                uri=uri,
                name=f.__qualname__,
                desc=f.__doc__,
                icon=None,
                inputs=inputs,
                outputs=outputs,
                view_func=f,
            )
            return f

        return __func_wrapper__
