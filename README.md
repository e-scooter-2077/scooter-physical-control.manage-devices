# Scooter Physical Control/Manage Devices
[![Continuous Integration](https://github.com/e-scooter-2077/scooter-physical-control.manage-devices/actions/workflows/ci.yml/badge.svg?event=push)](https://github.com/e-scooter-2077/scooter-physical-control.manage-devices/actions/workflows/ci.yml)
[![GitHub issues](https://img.shields.io/github/issues-raw/e-scooter-2077/scooter-physical-control.manage-devices?style=plastic)](https://github.com/e-scooter-2077/scooter-physical-control.manage-devices/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr-raw/e-scooter-2077/scooter-physical-control.manage-devices?style=plastic)](https://github.com/e-scooter-2077/scooter-physical-control.manage-devices/pulls)
[![GitHub](https://img.shields.io/github/license/e-scooter-2077/scooter-physical-control.manage-devices?style=plastic)](/LICENSE)
[![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/e-scooter-2077/scooter-physical-control.manage-devices?include_prereleases&style=plastic)](https://github.com/e-scooter-2077/scooter-physical-control.manage-devices/releases)
[![Documentation](https://img.shields.io/badge/documentation-click%20here-informational?style=plastic)](https://e-scooter-2077.github.io/documentation/implementation/index.html#event-handling)

This is a Service Bus triggered Azure Function that reacts to the events of the Scooter Data signaling the creation or removal of a Scooter identity and propagate the changes on the IoTHub and the Digital Twins Graph.
