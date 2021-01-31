'''
Poorly written example of a python script that could be called from Hops
without involving Rhino/Grasshopper at all
'''
from http.server import HTTPServer, BaseHTTPRequestHandler
import json


class HopsServer(BaseHTTPRequestHandler):
    def _set_headers(self):
        self.send_response(200)
        self.send_header("Content-type", "application/json")
        self.end_headers()

    def do_GET(self):
        # self.path = '/add'
        self._set_headers()
        data = {
            'Description': 'Add two numbers using cpython',
            'Inputs': [
                {'Name': 'a', 'AtLeast': 1, 'AtMost': 1, 'Description': 'First value for addition', 'ParamType': 'Number'},
                {'Name': 'b', 'AtLeast': 1, 'AtMost': 1, 'Description': 'Second value for addition', 'ParamType': 'Number'},
            ],
            'Outputs': [
                {'Name': 'sum', 'ParamType': 'Number'}
            ]
        }
        s = json.dumps(data)
        self.wfile.write(s.encode(encoding='utf_8'))

    def do_HEAD(self):
        self._set_headers()

    def do_POST(self):
        # read the message and convert it into a python dictionary
        length = int(self.headers.get('Content-Length'))
        jsoninput = self.rfile.read(length)
        data = json.loads(jsoninput)
        a = float(data['values'][0]['InnerTree']['0'][0]['data'])
        b = float(data['values'][1]['InnerTree']['0'][0]['data'])
        sum = a + b
        output = {
            'values': [
                {
                    'ParamName': 'sum',
                    'InnerTree': {
                        '0': [{
                            'type': 'System.Double',
                            'data': sum
                        }]
                    }
                }
            ]
        }
        self._set_headers()
        s = json.dumps(output)
        self.wfile.write(s.encode(encoding='utf_8'))


if __name__ == "__main__":
    location = ('localhost', 5000)
    httpd = HTTPServer(location, HopsServer)
    print(f"Starting httpd server on {location[0]}:{location[1]}")
    httpd.serve_forever()
