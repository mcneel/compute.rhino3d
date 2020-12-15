import rhino3dm
import json
import requests

__version__ = '0.12.2'

url = 'https://compute.rhino3d.com/'
authToken = ''
apiKey = ''
stopat = 0


def ComputeFetch(endpoint, arglist):
    class __Rhino3dmEncoder(json.JSONEncoder):
        def default(self, o):
            if hasattr(o, "Encode"):
                return o.Encode()
            return json.JSONEncoder.default(self, o)
    global authToken
    global apiKey
    global url
    global stopat
    posturl = url + endpoint
    if(stopat>0):
        if(posturl.find('?')>0): posturl += '&stopat='
        else: posturl += '?stopat='
        posturl += str(stopat)
    postdata = json.dumps(arglist, cls=__Rhino3dmEncoder)
    headers = { 'User-Agent': 'compute.rhino3d.py/' + __version__ }
    if authToken:
        headers['Authorization'] = 'Bearer ' + authToken
    if apiKey:
        headers['RhinoComputeKey'] = apiKey
    r = requests.post(posturl, data=postdata, headers=headers)
    return r.json()


def PythonEvaluate(script, inputs, output_names):
    """
    Evaluate a python script on the compute server. The script can reference an
    `input` parameter which is passed as a dictionary. The script also has
    access to an 'output' parameter which is returned from the server.

    Args:
        script (str): the python script to evaluate
        inputs (dict): dictionary of data passed to the server for use by the
                       script as an input variable
        output_names (list): list of strings defining which variables in the
                       script to return
    Returns:
        dict: The script has access to an output dict variable that it can
              fill with values. This information is returned from the server
              to the client.
    """
    encodedInput = rhino3dm.ArchivableDictionary.EncodeDict(inputs)
    url = 'rhino/python/evaluate'
    args = [script, json.dumps(encodedInput), output_names]
    response = ComputeFetch(url, args)
    output = rhino3dm.ArchivableDictionary.DecodeDict(json.loads(response))
    return output


def DecodeToCommonObject(item):
    if item is None:
        return None
    if isinstance(item, list):
        return [DecodeToCommonObject(x) for x in item]
    return rhino3dm.CommonObject.Decode(item)


def DecodeToPoint3d(item):
    if item is None:
        return None
    if isinstance(item, list):
        return [DecodeToPoint3d(x) for x in item]
    return rhino3dm.Point3d(item['X'], item['Y'], item['Z'])


def DecodeToVector3d(item):
    if item is None:
        return None
    if isinstance(item, list):
        return [DecodeToVector3d(x) for x in item]
    return rhino3dm.Vector3d(item['X'], item['Y'], item['Z'])


def DecodeToLine(item):
    if item is None:
        return None
    if isinstance(item, list):
        return [DecodeToLine(x) for x in item]
    start = DecodeToPoint3d(item['From'])
    end = DecodeToPoint3d(item['To'])
    return rhino3dm.Line(start,end)


def DecodeToBoundingBox(item):
    if item is None:
        return None
    if isinstance(item, list):
        return [DecodeToBoundingBox(x) for x in item]
    return rhino3dm.BoundingBox(item['Min']['X'], item['Min']['Y'], item['Min']['Z'], item['Max']['X'], item['Max']['Y'], item['Max']['Z'])

