# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

## [0.5.0] - 2021-05-18
### Added
- rhino.compute.exe now shipping with Hops on Windows and acts as a top level reverse proxy server to solve definitions.
- tree input support for python servers

### Fixed
- data output params are now passed back to hops

## [0.3.3] - 2021-03-03
## [0.1.0] - 2021-02-10