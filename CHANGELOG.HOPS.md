# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- rhino.compute no longer sends requests to a compute.geometry child whose Rhino failed to start, for example because of a licensing problem. Such a child kept running and answered every request with an "An error occurred while starting the application" page until the server was restarted. rhino.compute now checks each new child's `/healthcheck` before using it, stops a child that fails, logs why, and starts a replacement with the next request.

### Security

- rhino.compute now ships with .NET 10.0.14, which fixes a Brotli decompression vulnerability (CVE-2026-111497) in the .NET 10.0.0 runtime it bundled.
- rhino.compute and compute.geometry now accept only gzip-compressed uploads. Uploads compressed any other way, such as with Brotli, are no longer unpacked.

## [9.0.0] - 2026-10-27

### Added

- New `RHINO_COMPUTE_LOG_LEVEL` environment variable (`Verbose`, `Debug`, `Information`, `Warning`, `Error` or `Fatal`) sets how much rhino.compute, compute.geometry and Hops write to their logs. Unlike `RHINO_COMPUTE_DEBUG`, it doesn't change what callers get back (stack traces in errors, Grasshopper warnings in answers). At `Debug`, the logs also record each definition's Grasshopper warnings and remarks.
- Hops and Rhino.Compute for Rhino 9 now send inputs and outputs in Grasshopper's own data format instead of converting each value to JSON. Anything Grasshopper can save inside a definition now reaches the server and comes back intact, including model objects (Rhino objects with their attributes, such as layer, name and color) and data types from third-party plugins, as long as the plugin is installed on the server too.
    - Hops uses the new format only when the server says it supports it, so Rhino 7 and 8 servers keep getting values as JSON. Other clients, such as the Python and JavaScript libraries, still send JSON and get JSON back.
    - Both sides need Rhino 9, because the format relies on changes to how Grasshopper in Rhino 9 saves data.
    - Model objects that reference the Rhino document are sent as copies, since the server doesn't have the document.
    - The Hops component can now show Model Object inputs and outputs.

### Changed

- Hops version numbers now start with the version of Rhino they're designed to work with, so this release, the first for Rhino 9, is 9.0.0 rather than 0.18.0. 

## [0.17.1] - 2026-10-13

### Added

- rhino.compute and compute.geometry now gzip answers of 1 KB or more for clients that ask for it (`Accept-Encoding: gzip`), and accept gzipped uploads (`Content-Encoding: gzip`). On one customer's batch of 41 requests, the data sent and received fell from 489 MB to 81 MB.
- Hops now asks for compressed answers and compresses its uploads to servers that accept them. Requests to a server on the same machine, including nested Hops calls, stay uncompressed both ways. Set the `HOPS_COMPRESSION` environment variable to `false` to turn this off.
- New `--child-startup-timeout` flag and `RHINO_COMPUTE_CHILD_STARTUP_TIMEOUT` environment variable (5-3600 seconds) set how long rhino.compute waits for a child compute.geometry to start.
- New `RHINO_COMPUTE_CHILDCOUNT` and `RHINO_COMPUTE_IDLESPAN` environment variables do the same as `--childcount` and `--idlespan`, so a deployed server can be configured without editing `web.config`.

### Changed

- Compression is on by default. To turn it off, for example for a client that asks for gzip but can't unpack it, set the `RHINO_COMPUTE_COMPRESSION` environment variable to `false`. On IIS, add `<environmentVariable name="RHINO_COMPUTE_COMPRESSION" value="false" />` to `web.config`.
- Browsers, JavaScript `fetch` and Python `requests`, including the compute-rhino3d JavaScript and Python clients, already ask for gzip, so they now receive compressed answers and unpack them automatically. Clients that don't ask, such as the .NET client and older Hops versions, are unaffected.
- The default child startup timeout is now 300 seconds instead of 60, because a child's first load on a new cloud server often takes more than a minute.
- Requests that arrive while the first child is still starting now wait for it, instead of each starting another child.
- `/activechildren` and `/active-children` now count only children that are ready to serve, and no longer start children. The `?initialize` parameter added in 0.17.0 has been removed; use `POST /launch-children` or `POST /launch-child` to start children.
- rhino.compute now sends each request to an idle child, or to the least busy one when all are busy. It used to take turns, so a request could wait behind a long solve while other children sat idle.
- rhino.compute notices that a starting child is ready within 100 ms instead of up to a second, so cold starts are quicker.
- Hops components inside a definition that Rhino.Compute is solving now send their requests back to that server, with the server's API key, instead of starting a separate local rhino.compute. A server set in the server's Hops settings still takes precedence.
- Requests with a mistake in them now get a 400 with a message saying what's wrong, instead of a 500. This covers malformed JSON, an input of the wrong type, an `algo` that isn't a valid definition, a pointer that doesn't load, and geometry endpoint arguments that don't match.
- Requests that send their definition inline (`algo`) now reuse the definition already loaded from the same `algo` instead of building it again each time.
- `module_update_compute.ps1`, which updates Rhino.Compute on a VM, is now all or nothing:
    - It checks the download before stopping IIS, and puts the previous version back if any step fails.
    - It keeps the server's `web.config`, so settings such as child count and timeouts survive an update.
    - It does nothing when the latest build is already installed; `-Force` reinstalls.
    - It logs every run to `update_compute_log.txt` and exits with 0 (updated), 1 (failed, nothing changed) or 2 (failed, needs attention).
- `module_update_rhino.ps1` now stops Rhino.Compute while Rhino installs and starts it again afterwards, even if the install fails. It does nothing when Rhino is current, checks that the new version installed, logs every run to `update_rhino_log.txt`, and no longer adds a firewall rule each time.

### Fixed

- Fixed a memory leak in requests that send their definition inline (`algo`). Each one left another solved copy of the definition in memory until the child exited: about 9 MB per request for a small definition, and hundreds of MB for large models.
- Fixed definitions that drop out of the definition cache, for example after a local file changes, staying in memory.
- Fixed child processes shutting down in the middle of a long request, such as a long solve or a slow upload, which then failed with a 500.
- Fixed crashes and mixed-up inputs when several nested Hops solves ran at once in one process.
- Fixed nested Hops solves sometimes getting empty inputs, and Hops components sometimes sending only some of their inputs, when several loaded at once.
- Fixed a request with a bad input value returning 200 with the previous request's results when sent again.
- Fixed `GET /io`, which always returned 500, so Hops components that point to a component ID couldn't load.
- Fixed `/io` sometimes failing when it ran alongside a solve or another `/io` of the same definition.
- Fixed a rare error, when requests asked for two different Rhino file versions at once, that made every later solve in that child fail until it restarted.
- Fixed first loads of several local definition files at once failing with 500, and occasionally ending the process.
- Fixed children that were still starting surviving `/shutdown-children` and rhino.compute's own shutdown. If the first child failed to start, every later request also failed until rhino.compute restarted.
- Fixed standalone compute.geometry ignoring `-apikey` and `-load-grasshopper:false`.

## [0.17.0] - 2026-06-05

### Added

- Redesigned the Hops preferences. You now choose **Use local rhino.compute instance** or **Use remote server**, and the local-only options gray out when remote is selected. On macOS, remote is the default, since a local server can't be started there.
- The remote server field shows a status dot that tests the server and API key as you type; click it to test again. An "Advanced multi-server setup" button manages several server URLs.
- A "Manage sources..." dialog replaces the Function Sources list in the Hops preferences, fixing its layout at display scales above 100%.
- New `/validate` endpoint on rhino.compute and compute.geometry checks the API key without waking child processes. It returns 200 when the key matches or no key is set, and 401 otherwise.
- New `--load-children-sequentially` flag starts rhino.compute's children one at a time, for VMs short on memory.
- New rhino.compute endpoints for managing children. All are `POST` and need the API key:
    - `/shutdown-children` stops every child, or one with `?port=N`.
    - `/recycle-children` replaces the children one at a time.
    - `/launch-children` fills the pool up to `--childcount`.
    - `/launch-child` adds one child, optionally on `?port=N`, up to 64 children.
- `/activechildren?initialize=false` returns the number of children without starting any.
- New `POST /cache/purge` endpoint on compute.geometry clears cached solve results and downloaded data, but not cached definitions. These caches also clear old entries automatically when memory use passes 70% (`RHINO_COMPUTE_CACHE_PHYSICAL_LIMIT_PERCENT` changes this).
- compute.geometry now only downloads `http` and `https` URLs, up to the request size limit. The new `--block-private-urls` flag (or `RHINO_COMPUTE_BLOCK_PRIVATE_URLS=true`) also blocks private and local addresses, such as cloud metadata endpoints. It's off by default but on in the `web.config` for new VMs, and recommended for any public server.

### Changed

- Removed the Carter dependency; endpoints now use ASP.NET Core routing.
- rhino.compute's root URL (`/`) now redirects to https://www.rhino3d.com/compute, like compute.geometry.
- The VM bootstrap scripts under `script/production/` now run in one pass with no restart, and install 9 IIS features instead of 35.
- rhino.compute starts at most 64 children; larger values are reduced to 64 with a warning.
- compute.geometry now limits request bodies to 50 MB by default. Change it with `RHINO_COMPUTE_MAX_REQUEST_SIZE` or `--max-request-size`.
- `RHINO_COMPUTE_TIMEOUT` now also applies to compute.geometry's own downloads, such as fetching a definition from a URL.

### Fixed

- Fixed compute.geometry ignoring `RHINO_COMPUTE_KEY`. POST requests now need the key; GET requests to informational endpoints such as `/version` stay open.
- Fixed Hops switching HTTPS server addresses to HTTP when solving.
- Hops now shows 401 Unauthorized as an error on the component, and both servers log rejected requests.
- Hops now shows a warning when it uploads a local definition to the server after a 500.
- Hops no longer reads local definition files over 100 MB, which could run Rhino out of memory. `HOPS_MAX_LOCAL_FILE_SIZE` changes the limit.
- Fixed Hops starting a duplicate rhino.compute, which flashed a console window, on every solve when one was already running.
- Fixed GET requests through rhino.compute always returning 200, even when compute.geometry returned an error.
- Fixed rhino.compute's proxy sending a misspelled content type (`applicaton/json`), labeling JSON answers as `text/plain`, and holding on to connections under load.
- Fixed Min/Max bounds on Context Get Number and Context Get Integer being ignored for tree access.
- Fixed an IndexOutOfRangeException in Hops when inputs have different numbers of branches.
- Fixed string inputs gaining quotes, and string outputs keeping doubled backslashes, in Hops.

## [0.16.28] - 2025-11-05

### Added

- New environment variable
    - `RHINO_COMPUTE_LOAD_GRASSHOPPER` - This variable controls whether or not the Grasshopper plugin is loaded (defaults to `True` for backward compatibility). Setting this to `False` will speed up the loading process, but you will not be able to solve Grasshopper definitions using Rhino.Compute. Instead, you will only be able to access standard SDK functions and/or any custom endpoints that you may have added through Rhino plugins.
- New command line arguments
    - `--load-grasshopper` `<true|false>` - Enable/disable Grasshopper plugin loading
    - `--max-request-size` `<bytes>` - Set the maximum acceptable request body size
    - `--apikey` `<key>` - Set an API key for authentication
    - `--timeout` `<seconds>` - Set the request timeout limit
    - `--create-headless-doc` `<true/false>` - Enable/disable whether to create a new headless Rhino document upon receiving a new request

## [0.16.27] - 2025-09-09

### Fixed

- Disabled parameters (i.e. Contextual Get Parameters, Context Bake/Print, RH_IN and/or RH_OUT groups) are now skipped during the IO search process.

## [0.16.26] - 2025-07-31

### Fixed

- Fixed a bug that was reported [here](https://github.com/mcneel/compute.rhino3d/issues/730) which created a race condition and lead to more compute.geometry processes than specified being created.

## [0.16.25] - 2025-07-14

### Fixed

- Fixed a bug where the maximum value, when set on a contextual number or integer parameter, was not being set properly during serialization.

## [0.16.24] - 2025-07-14

### Added

- The rhino.compute and compute.geometry Visual Studio projects now target the .NET 8.0 Core Runtimes. This change has been made because as of release of Rhino 8.21, the Rhino installer will now automatically install the ASP .NET Core Runtimes as part of it's installation process. Using Hops 0.16.24 (or higher) and Rhino 8.21 (or higher) together should eliminate the need for users to maunally install the appropriate .NET Runtimes in order to get Rhino.Compute to work.

## [0.16.23] - 2025-03-04

### Added

- Downgrading Serilog references to the minimum necessary in order to maintain compatability with Revit in the Hops project.

## [0.16.22] - 2025-02-24

### Added

- The Hops component will now display the Minimum and Maximum values on the input parameter tooltip. In addition, the error message that is displayed if a user exceeds the Minimum or Maximum bounds will also indicate what the numeric bounds are for further clarification.

## [0.16.21] - 2025-02-21

### Added

- Added more detailed logging information to capture the idlespan value and the shutdown procedure. If the environment variable RHINO_COMPUTE_DEBUG is set to True, then you should see additional information regarding the registered idlespan value and how it is used during the shutdown procedure.

## [0.16.20] - 2025-02-07

### Fixed

- Logging has been refactored in all three projects (ie. Rhino.Compute, Compute.Geometry, and Hops) to be more consistent and robust. Setting an environment variable RHINO_COMPUTE_DEBUG to True (default is False) will now add much more verbose logging information to both the console and file outputs. A log output has also been added to the Hops project to receive logging information relevant to the Hops component. You can customize where these logs are saved by setting an environment variable RHINO_COMPUTE_LOG_PATH with the directory path for the log files. If this variable is left blank, the default log path will be C:\Users\UserName\AppData\Local\Temp\Compute\Logs.

## [0.16.19] - 2025-01-27

### Added

- It is now possible for Rhino.Compute to create a headless document to be used during the solve routine. To use this feature, create a local environment variable called RHINO_COMPUTE_CREATE_HEADLESS_DOC.The value of this variable can be set to either true or false with false being the default. Each time the GrasshopperSolveHelper method of the ResthopperEndpointsModule is invoked, it checks whether Config.CreateHeadlessDoc is set to true. If it is, a brand new headless document is instantiated with tolerances and units determined by the input object. This feature is useful for third party plugins which may make calls to the Rhino Document to retrieve properties such as tolerances or units. Prior to this feature, these requests would result in an error. Now, a plugin may utilize this information for calculations.

## [0.16.18] - 2025-01-06

### Fixed

- Fixed a bug where custom endpoints which pointed to functions which did not contain an argument would result in a duplicate. When this endpoint would be called, the duplicate would cause an AmbiguousMatchException to be thrown.
- Changed the `RollForward` value in both the Rhino.Compute and Compute.Geometry csproj files to use `Major` instead of `LatestMinor`. This change means that these projects will use .NET 7 if the framework is installed, but if not it will roll forward to .NET 8 or 9 (which ever is the latest major version installed on the machine). Read more at https://learn.microsoft.com/en-us/dotnet/core/versions/selection.

## [0.16.17] - 2024-11-19

### Fixed

- Startup.cs now loads the Commands.rhp so that certain features such as the gltf exporter will work properly inside rhino.compute. This is a temporary solution until the gltf exporter code is moved into Rhinocommon or Rhino.UI.

## [0.16.16] - 2024-11-07

### Fixed

- Fixed a bug where the /launch endpoint would return a NullReference exception.
- Added a line to the ComputeChildren.cs to clear the startInfo environmentVariable for "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES" when Rhino.Compute starts up the child process Compute.Geometry. This was causing issues while debugging in newer version of Visual Studio.
- Fixed a bug where the ComputeRecursionLevel property was not always being set.

## [0.16.15] - 2024-11-07

### Fixed

- Fixed a bug where system directory paths were not being parsed correctly as a command line argument.

## [0.16.14] - 2024-10-30

### Fixed

- Minor bug fixes. Fixed a bug where the open file icon shown in the Set Definition dialog was blurry.

## [0.16.13] - 2024-08-29

### Fixed

- Fixed a bug in the Function Source Manager which was causing the document to expire incorrectly when multiple functions were instantiated by the Hops component.

## [0.16.12] - 2024-08-28

### Fixed

- In the change made to version 0.16.11, the schema of the default input values were changed to a DataTree<ResthopperObject>. However, this change broke some code in instances where people would be sending the older style of values as defaults. The changes made in this build (0.16.12) will now properly handle the deserialization of values sent either as a DataTree<ResthopperObject> or as the older style of default values.

## [0.16.11] - 2024-08-20

### Fixed

- Fixed an issue with default values. Previously, default values would only work on some parameter types and only if a single value was set via persistent data. Now, any parameter type (except for model objects) will work as setting a default value and you can pass in an item, list, or data tree as your default value.

### Added

- Added the ability to make some parameters act as Optional inputs. If you use a Context Get component and set the AtLeast value to 0, then that parameter will be treated as an Optional input.

## [0.16.10] - 2024-08-08

### Fixed

- Adding a fix to enfoce the localization culture of Compute.Geometry to use the CultureInfo("en-US"). This should hopefully fix some issues where users from various countries have trouble with strings converting to numbers because of comma and/or decimal separators.

## [0.16.9] - 2024-07-03

### Fixed

- Fixed a bug where setting the environment variable RHINO_COMPUTE_DEBUG to True would not in effect add any additional logging information to the console output. The default for this is True when running in debug mode, and False when running Rhino.Compute in release mode. Override this setting by creating an environment variable called RHINO_COMPUTE_DEBUG and setting the value to True or False. Note, this change will only work when targeting builds of Rhino >= 8.10.

## [0.16.8] - 2024-06-05

### Fixed

- The Rhino.Inside.Resolver was set to automatically look for the latest build of Rhino which was causing some conflicts if users had installed Rhino 9 WIP. The default path for the 8.x build of rhino.compute should now look for that latest build of Rhino 8 when trying to resolve Rhino.Inside.

### Added

- A few additional lines of logging information were added to display the the current build of Rhino.Compute and Rhino as well as the working system path.

## [0.16.7] - 2024-04-22

### Added

- Added the RhinoCompute.cs file to the compute.geometry 8.x repo. This is referenced in several guides and tutorials and had been omitted during the refactoring process when we moved from 7.x to 8.x. In addition, a new endpoint was added called `/sdk/csharp` which will allow you to view and/or download the contents of this file for use in your own projects.

## [0.16.6] - 2024-03-15

### Fixed

- Fixing another bug to handle how the RhinoCode scripting plugin is loaded in Rhino.Compute. This plugin is now loaded first before Grasshopper or other compute related plugins.

## [0.16.5] - 2024-02-29

### Fixed

- Calling the /sdk endpoint in Rhino.Compute would not return anything. This has now been fixed and it will now return a full list of all SDK methods available in Rhino.Compute.
- Fixed the module_update_compute.ps1 powershell script which can be run to update existing production environments to the latest version of Rhino.Compute.
- Fixed a bug where the RhinoCode scripting plugin was not being loaded properly in Rhino.Compute.
- Added some parameters to the launch settings to be able to debug Rhino.Compute in .NET 4.8.

## [0.16.4] - 2023-11-27

### Added

- Added support for the Box as a data type returned from rhino.compute.

## [0.16.3] - 2023-11-17

### Added

- Added a support in rhino.compute for the new Get Plane context component available in Rhino 8.

## [0.16.2] - 2023-08-23

### Added

- Added support for Rectangle3d as a data type returned from rhino.compute.

### Fixed

- Rhino.Compute and Compute.Geometry now target the .NET 7 Framework. This makes compute now compatible with Rhino 8.
- Fixed many minor bugs.

## [0.16.1] - 2023-03-23

### Added

- Two endpoints to return information about the plugins installed on the machine running the rhino.compute server.
    - /plugins/rhino/installed - will return a sorted dictionary of Rhino plugins which are installed.
    - /plugins/gh/installed - will return a sorted dictionary of Grasshopper plugins which are installed.

### Fixed

- Compute would not return the correct minimum and maximum value for the Context Get Integer component if a slider was connected as the default value.
- Compute would not return the correct default value for the Context Get Boolean if a boolean toggle was connected as the default value.
- Fixed a bug which prevented the minimum and maximum values to be set on a Contextual Number/Integer parameter if a number slider was used to define the default value. Now, the minimum and maximum value of the slider should be used for that input parameter.
- Fixed a rather serious bug which prevented the data structure from the output of the Hops component to match the same data structure returned from running native Grasshopper components. This fix should ensure that the data structures returned from Hops are consistent with examples which do not use Hops but contain the same underlying components. [See this thread for more information](https://discourse.mcneel.com/t/hops-output-tree-mismatch/155361).
- If a user set the At Least to something > 0 (ie. list access), the Hops component wouldn't detect the number of incoming items and would throw an error.

## [0.15.4] - 2022-11-23

### Fixed

- Fixing release bug to properly bundle all necessary dependencies into a self-contained exectuable

## [0.15.3] - 2022-11-05

### Fixed

- A bug where warnings were always being produced with input set to list access

## [0.15.2] - 2022-10-26

### Added

- An environment variable called `RHINO_COMPUTE_MAX_REQUEST_SIZE` can now be set to configure the maximum allowed size of any HTTP request body in bytes. The default value is set to 52428800 bytes (approx. 50mb).

## [0.15.1] - 2022-08-04

### Added

- A textbox was added to the Hops App Settings dialog (under Preferences/Solver) which allows you to set the timeout (in seconds) for the HTTP client that Hops uses to send/receive data to/from the rhino.compute server. The default is 100s but for long running calculations you may need to increase this value.

## [0.15.0] - 2022-06-10

### Added

- Hops can now internalize it's referenced definition if it is saved on a local file system. Similar to internalizing geometry, when you internalize a definition it means that it removes the reference to the external file and instead serializes the definition inside the Hops component. This means that once a referenced file is internalized, you no longer have to share the accompanying referenced definition along with the Hops file.

## [0.14.1] - 2022-05-26

### Fixed

- A bug was fixed which caused an error when parsing the numeric minimum and maximum values on the Get Number or Get Integer components.

## [0.14.0] - 2022-05-19

### Added

- DataTrees are now supported in Rhino builds >= 7.19.22130.15001 and 8.0.22131.04306. All of the contextual getter components now have a menu item which lets you set whether you want that parameter to have DataTree access. When set to true, this setting will override the AtMost value which is what Hops uses for item or list access. The DataTree access setting has no effect on the GH Player command. 
- The function manager is now available in Rhino for mac on builds >= 7.19.22126.15001 and 8.0.22126.04306. See the update below for hops build 0.13.1 for more information about how the function manager works.
- The bootstrap script which is used when deploying Rhino.Compute for a production environment has been update to follow a two-step installation process (with a restart in between). The installation is largely the same, however a restart is now required to finish the installation of the IIS modules.

### Fixed

- A bug was fixed if the Hops referenced file was not found. Now, if the referenced file is at least in the same folder as the Hops definition then it should find and load it upon opening the file.
- An error would occur when exporting last HTTP request and/or response if more than one hops component was being used in a definition.
- The Minimum and Maximum values on the contextual Get Number and Get Integer components are now used in Hops. If either of those values are set on the getter parameter and the input value exceeds that limit, an error will be thrown in Hops. 

## [0.13.1] - 2022-04-13

### Added

- A function manager was added to the Hops preferenced UI. This interface allows you to add function sources (ie. local folder, localhost, and remote server locations). Once a valid source is added to the Hops preferences, a new menu item will be added to the Hops component. This menu item will enumerate valid functions (either grasshopper files or function endpoints). Right-clicking on a menu item will open the file while left-clicking will reference the file in the Hops component (ie. like setting the path). For  now, this feature is only available when running Grasshopper for Windows.

### Fixed

- A bug was fixed which prevented the rhino.compute server from launching on local machines which had the checkbox turned on for loading .gha assemblies using COFF byte arrays in the Grasshopper developer settings.

## [0.12.0] - 2022-03-10

### Added

- The current document units (ie. feet, inches, millimeters, etc.) are now included as part of the API request from Hops.
- The Hops component now displays an error message if:
    - one or more inputs have the same name
    - one or more outputs have the same name
    - rhino.compute is missing a component. This would likely be caused if rhino.compute is running on a remote server and it is missing a 3rd party plugin.
- An environment variable called `RHINO_COMPUTE_TIMEOUT` can now be set to configure the request timeout (in seconds) for the HttpClient in the reverse proxy module.
- The bootstrap script (for deployment to production environments) has been updated to handle the installation of 3rd party plugins. The script will now create a new local user account, called `RhinoComputeUser` and auto-generate a unique password. It will add this user to the RDP group and assign this identity to the RhinoComputeAppPool which is what IIS uses to run rhino.compute.exe. The script will print the username and password at the end for your records. To install 3rd party plugins after the bootstrap script has been run, follow these steps:
    1. Log into your VM using these credentials (write these down)
        - User Name: RhinoComputeUser
        - Password: #This will be some unique password generated by the bootstrap script
    1. Install plugins using the Rhino package manager or
    1. Copy/paste plugin files to C:\Users\RhinoComputeUser\AppData\Roaming\Grasshopper\Libraries
    1. Restart the VM

### Fixed

- A bug was fixed preventing Hops from running properly in Rhino 8 WIP.

## [0.11.1] - 2022-02-08

### Fixed

- A small fix was made to the rhino.compute project to change how child processes (ie. compute.geometry) get spawn whenever the rhino.compute.exe is launched. This change was made because of how a production environment handles http requests, but this change also effected how Hops is launched from Grasshopper. Rhino.compute.exe now has a command line option called --spawn-on-startup whose default value is false. If you include this argument when you launch rhino.compute.exe then it will automatically launch a child process on startup.

## [0.11.0] - 2022-01-25

### Added

- You can now export the last API request/response made from the Hops component to the compute server. There are two endpoints that are hit during any Hops routine. In the first API call, Hops sends a request to '/io' which uploads the referenced grasshopper file to the server. Compute processes the file and returns a response with necessary information to populate the inputs and outputs on the Hops component. The Hops component gets this information and builds the inputs and outputs. It then determines what values to pass in as the input values to the definition and sends that information over to the '/solve' endpoint. Compute checks its cache to grab the right grasshopper definition and then feeds in these new input values to the definition. Once it gets a result it send it back to the Hops component which then feeds it out to the appropriate output parameter. Each of these requests and responses can now be exports (.json) so that the process can be inspected and debugged.
- An API Key input was added under the Hops preferences section. The API key is a string of text that is secret to your compute server and your applications that are using the compute API e.g. b8f91f04-3782-4f1c-87ac-8682f865bf1b. It is optional if you are testing locally, but should be used in a production environment. It is basically how the compute server ensures that the API calls are coming from your apps only. You can enter any string that is unique and secret to you and your compute apps. Make sure to keep this in a safe place.

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
