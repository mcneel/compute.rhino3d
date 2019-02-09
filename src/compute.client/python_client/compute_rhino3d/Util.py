import rhino3dm
import json
import requests

__version__ = '0.5.0'

url = "https://compute.rhino3d.com/"
authToken = None
stopat = 0

def ComputeFetch(endpoint, arglist) :
    class __Rhino3dmEncoder(json.JSONEncoder):
        def default(self, o):
            if hasattr(o, "Encode") :
                return o.Encode()
            return json.JSONEncoder.default(self, o)
    global authToken
    global url
    global stopat
    posturl = url + endpoint
    if(stopat>0):
        if(posturl.find('?')>0): posturl += '&stopat='
        else: posturl += '?stopat='
        posturl += str(stopat)
    postdata = json.dumps(arglist, cls = __Rhino3dmEncoder)
    headers = {
        'Authorization': 'Bearer ' + authToken,
        'User-Agent': 'compute.rhino3d.py/' + __version__
    }
    r = requests.post(posturl, data=postdata, headers=headers)
    return r.json()

