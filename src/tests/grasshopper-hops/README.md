
navigate to grasshopper-hops/

run:
`pipenv install`

test http server:
`pipenv run hops_http`

test flask server:
`pipenv run hops_flask`


Notes:

- `grasshopper-hops` module supports builtin http or flask as server backend
- Exising hops API calls `/solve` and provides component name in the payload
- Test components are copied from previous python examples
  - `app.py` is the flask app. `flask.env` is the flask env vars
  - `app_http.py` is the http app
- `pyproject.toml` is prepared for publishing the `grasshopper-hops` module to pypi later
- `# TODO:` and `# FIXME:` are added for work to be done
  

Hops module structure:

- `base.py` base class for the Hops middleware:
  - component registration
  - input processing
  - solving
  - output processing
- `params.py` wrappers for supported params
- `middleware/` supported server backends:
  - handle http GET and POST in each framework