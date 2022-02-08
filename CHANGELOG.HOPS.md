# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.11.1] - 2022-02-08
### Fixed
- A small fix was made to the rhino.compute project to change how child processes (ie. compute.geometry) get spawn whenever the rhino.compute.exe is launched. This change was made because of how a production environment handles http requests, but this change also effected how Hops is launched from Grasshopper. Rhino.compute.exe now has a command line option called --spawn-on-startup whose default value is false. If you include this argument when you launch rhino.compute.exe then it will automatically launch a child process on startup.

## [0.11.0] - 2022-01-25
### Added
- You can now export the last API request/response made from the Hops component to the compute server. There are two endpoints that are hit during any Hops routine. In the first API call, Hops sends a request to '/io' which uploads the referenced grasshopper file to the server. Compute processes the file and returns a response with necessary information to populate the inputs and outputs on the Hops component. The Hops component gets this information and builds the inputs and outputs. It then determines what values to pass in as the input values to the definition and sends that information over to the '/solve' endpoint. Compute checks its cache to grab the right grasshopper definition and then feeds in these new input values to the definition. Once it gets a result it send it back to the Hops component which then feeds it out to the appropriate output parameter. Each of these requests and responses can now be exports (.json) so that the process can be inspected and debugged.

-An API Key input was added under the Hops preferences section. The API key is a string of text that is secret to your compute server and your applications that are using the compute API e.g. b8f91f04-3782-4f1c-87ac-8682f865bf1b. It is optional if you are testing locally, but should be used in a production environment. It is basically how the compute server ensures that the API calls are coming from your apps only. You can enter any string that is unique and secret to you and your compute apps. Make sure to keep this in a safe place.

### Fixed
- Logging was cleaned up in the compute console window. Lines that are generated from the compute.geometry project are prefixed with the letters "CG", otherwise you can assume the line was generated from the rhino.compute project. Port numbers (ie. 6001, 6002, etc.) for child processes are now also added to the compute.geometry lines as soon as they are assigned so it is easier to see which child process is handling a given request.

## [0.10.1] - 2021-11-17
### Added
- Hops output parameters can now be created using either the Context Print or Context Bake components. The nickname for these components will be used as the name of the output parameter in Hops. Note: All names should be unique (ie. no duplicates).

### Fixed
- A bug was fixed where output parameters did not retain the connection to their recipient parameters when saved/reopened.

## [0.9.0] - 2021-11-08
### Added
- Document tolerances (ie. absolute distance and angle tolerances) are now passed to rhino.compute as part of the JSON request.
- Get Boolean and Get File components are now correctly interpretted by Hops.

### Fixed
- Hops input and output parameters are now ordered based on the Y-canvas-position of the Get Components in the referenced definition.
- The maximum request body size was increased to from approximately 28.6mb to 50mb.

## [0.8.0] - 2021-08-31
### Added
- Export python sample added to Hops component context menu
- Export JSON added to Hops component context menu

### Fixed
- Default values for "Get" components can be resolved when relays are in between the component and upstream data
- Custom icons for hops components are always resized to 24x24

## [0.7.0] - 2021-06-23
### Added
- Nested hops calls now permitted with a recursion limit of 10. This limit can be modified by changing a GH app setting
- Components in named RH_OUT groups are now supported

### Fixed
- String inputs/outputs were not getting unescaped when passed back and forth between hops and compute
- Points, lines, circles were not getting converted to geometry when input to a "Get Geometry" component
- Improved error messages by forwarding errors from remote solved components
- 0.7.2: attempt to fix handling different forms of input strings in compute
- 0.7.3: handle exception in a plug-in's DocumentAdded event handler and allow request to continue

## [0.6.0] - 2021-05-30
### Added
- Path input (optional)
- Enabled input (optional)
- Asynchronous solving. Components can solve without blocking GH user interface. Asynchronous operation mode is optional per component.
- Maximum concurrent request setting to preferences

### Fixed
- Hid some preference controls on Mac that are only meant for Windows
- Component attempts to detect when inputs/outputs on a server have changed and will rebuild itself
- Removed parallel computing and variable parameters context menu items as they were unnecessary
- [0.6.2] Planes were not supported as a data type to be passed between processes

## [0.5.0] - 2021-05-18
### Added
- rhino.compute.exe now shipping with Hops on Windows and acts as a top level reverse proxy server to solve definitions.
- tree input support for python servers

### Fixed
- data output params are now passed back to hops

## [0.3.3] - 2021-03-03
## [0.1.0] - 2021-02-10
