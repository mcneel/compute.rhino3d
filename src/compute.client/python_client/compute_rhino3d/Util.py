import json
import urllib2

url = "https://compute.rhino3d.com/"
authToken = None

def ComputeFetch(endpoint, arglist):
    args = []
    for item in arglist:
        if hasattr(item, 'Encode'):
            args.append(item.Encode())
        else:
            args.append(item)
    req = urllib2.Request(url + endpoint)
    req.add_header('Content-Type', 'application/json')
    req.add_header('Authorization', 'Bearer ' + authToken)
    response = urllib2.urlopen(req, json.dumps(args))
    return json.loads(response.read())

