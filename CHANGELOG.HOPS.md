# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
