import rhino3dm
import json
import requests

url = "https://compute.rhino3d.com/"
authToken = None

def ComputeFetch(endpoint, arglist) :
    class __Rhino3dmEncoder(json.JSONEncoder):
        def default(self, o):
            if hasattr(o, "Encode") :
                return o.Encode()
            return json.JSONEncoder.default(self, o)
    global authToken
    postdata = json.dumps(arglist, cls = __Rhino3dmEncoder)
    headers = {'Authorization': 'Bearer ' + authToken}
    r = requests.post(url+endpoint, data=postdata, headers=headers)
    return r.json()

