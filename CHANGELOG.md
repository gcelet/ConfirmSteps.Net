# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Expose per-phase step timings and scenario duration
- Let a scenario release the service provider it built
- Add IScenarioObserver<T> for step-by-step progress
- Let a host observe the steps that follow a failure
- Optionally release the buffered responses a step retains
- Refuse to build a request whose variables have no value **BREAKING**
- Repeat a query parameter once per value its variable carries
- Describe a request body as a JSON structure, not as text
- Load an http step from a JSON description
- Let the host supply the root a description does not name
- A body property can be left out when its variable has no value
- A step description may carry comments and a trailing comma
- Run and verify a SQL command against any ADO.NET provider

### Changed

- Streamline WireMock assertions and remove null suppressions

### Fixed

- Make the shared Random behind wait steps thread-safe
- Stop reporting a requested cancellation as a step failure

## [0.2.1] - 2026-05-03

### Fixed

- Support non rfc compliant http header
- Push package to nuget only on tag

## [0.2.0] - 2026-04-27

### Added

- Add support for dotnet 8 and 10
- Add step title
- Expose http response headers
- Add reporting of execution summary of a scenario

### Changed

- Allow multiple http verifications

### Fixed

- Remove warnings introduced by analyzers

## [0.1.2] - 2023-08-15

### Added

- Add jsonpath array selector methods
- Add jmespath object transform methods
- Add jsonpath array selector methods
- Add jmespath object transform methods

## [0.1.0] - 2023-05-09

### Added

- Add core files, start coding! [ci] (#1)
- Enable renovate bot (#2)
- Scenario api design (#8)

### Fixed

- Build script need execution permission (#6)
- Build script need execution permission (#7)
- Remove assert
- Local build working again
- Change default value to nuget feed parameter
- Enable publish artifacts for github action
- Select nuget package files only
