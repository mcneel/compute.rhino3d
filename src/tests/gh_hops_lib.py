from http.server import HTTPServer, BaseHTTPRequestHandler
from enum import Enum
import json


class ParamAccess(Enum):
    ITEM = 0
    LIST = 1
    TREE = 2


class ParamManager(object):
    def __init__(self):
        self._items = []

    def to_meta_list(self):
        return self._items

    def parameter_name(self, i: int):
        return self._items[i]['Name']

    def add_number_parameter(self, name: str, nickname: str, description: str,
        access: ParamAccess):
        param = {'Name': name, 'Description': description, 'AtLeast': 1}
        if access == ParamAccess.ITEM:
            param['AtMost'] = 1
        param['ParamType'] = 'Number'
        self._items.append(param)
        return len(self._items) - 1


class InputParamManager(ParamManager):
    def __init__(self):
        super().__init__()


class OutputParamManager(ParamManager):
    def __init__(self):
        super().__init__()


def _coerce_input(item):
    data = item['data']
    itemtype = item['type']
    coercers = {
        'System.Double': lambda x: float(x)
    }
    rc = coercers[itemtype](data)
    return rc


class DataAccess(object):
    def __init__(self, component, inputs):
        self._inputs = inputs
        self._component = component
        self._results = {}

    def getdata(self, i):
        name = i
        if isinstance(i, int):
            name = self._component._inputs.parameter_name(i)
        data = self._inputs[name]['0'][0]
        data = _coerce_input(data)
        return (True, data)

    def setdata(self, i, data):
        name = i
        if isinstance(i, int):
            name = self._component._outputs.parameter_name(i)
        self._results[name] = data

    def output_list(self):
        rc = []
        for key, value in self._results.items():
            entry = {
                'ParamName': key,
                'InnerTree': {
                    '0': [{
                        'type': 'System.Double',
                        'data': value
                    }]
                }
            }
            rc.append(entry)
        return rc


class Component(object):
    def __init__(self, name: str, nickname: str, description: str,
                 category: str, subcategory: str):
        self._name = name
        self._nickname = nickname
        self._description = description
        self._category = category
        self._subcategory = subcategory
        self._meta = None

    def build_params(self):
        self._inputs = InputParamManager()
        self.register_input_params(self._inputs)
        self._outputs = OutputParamManager()
        self.register_output_params(self._outputs)

    def name(self):
        return self._name

    def meta(self):
        if self._meta is None:
            meta = {'Description': self._description}
            meta['Inputs'] = self._inputs.to_meta_list()
            meta['Outputs'] = self._outputs.to_meta_list()
            self._meta = meta
        return self._meta

    def register_input_params(self, inputs):
        pass

    def register_output_params(self, outputs):
        pass

    def solve_instance(self, data_access):
        pass


class ComponentCollection(object):
    components = None

    @staticmethod
    def component_names():
        names = [c.name() for c in ComponentCollection.components]
        return names

    @staticmethod
    def find_component(name: str) -> Component:
        name = name.lower()
        for component in ComponentCollection.components:
            if name == component.name().lower():
                return component
        return None

    @staticmethod
    def component_description(name: str):
        component = ComponentCollection.find_component(name)
        description = component.meta()
        return description

    @staticmethod
    def solve_component(name: str, inputs: list):
        component = ComponentCollection.find_component(name)
        da = DataAccess(component, inputs)
        component.solve_instance(da)
        return da


class __HopsServer(BaseHTTPRequestHandler):
    def _set_headers(self):
        self.send_response(200)
        self.send_header("Content-type", "application/json")
        self.end_headers()

    def do_GET(self):
        tokens = self.path.split('?')
        path = tokens[0]
        if (path == '/'):
            self._set_headers()
            output = ComponentCollection.component_names()
            s = json.dumps(output)
            self.wfile.write(s.encode(encoding='utf_8'))
            return
        self._set_headers()
        component_name = tokens[0][1:]
        output = ComponentCollection.component_description(component_name)
        s = json.dumps(output)
        self.wfile.write(s.encode(encoding='utf_8'))

    def do_HEAD(self):
        self._set_headers()

    def do_POST(self):
        # read the message and convert it into a python dictionary
        length = int(self.headers.get('Content-Length'))
        jsoninput = self.rfile.read(length)
        data = json.loads(jsoninput)
        comp_name = data['pointer']
        values = {}
        for d in data['values']:
            paramname = d['ParamName']
            values[paramname] = d['InnerTree']
        da = ComponentCollection.solve_component(comp_name, values)
        output = da.output_list()
        output = {
            'values': output
        }
        self._set_headers()
        s = json.dumps(output)
        self.wfile.write(s.encode(encoding='utf_8'))


def start_server(components: list, port: int):
    for component in components:
        component.build_params()
    ComponentCollection.components = components

    location = ('localhost', port)
    httpd = HTTPServer(location, __HopsServer)
    print(f"Starting hops python server on {location[0]}:{location[1]}")
    httpd.serve_forever()
