# DuckDNS Updater

[![CI](https://github.com/BlinkSun/DuckDnsUpdater/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/BlinkSun/DuckDnsUpdater/actions/workflows/ci.yml)
[![Version](https://img.shields.io/github/v/tag/BlinkSun/DuckDnsUpdater?sort=semver)](https://github.com/BlinkSun/DuckDnsUpdater/tags)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/BlinkSun/DuckDnsUpdater/blob/master/LICENSE)
[![Tests](https://img.shields.io/badge/tests-23%20passing-brightgreen)](https://github.com/BlinkSun/DuckDnsUpdater/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

A lightweight Windows system tray app that keeps your DuckDNS record updated with your current public IP. 🦆

## Summary
- Tray-only WinForms app (no main window)
- Automatic and on-demand DuckDNS updates
- Local token protection with DPAPI (CurrentUser)
- Health status in tray menu
- Built-in configuration test
- Retry, timeout, fallback IP providers, local logs

## Features ✨
- HTTPS-only network calls
- Public IP validation (routable IPv4/IPv6 only)
- Silent failure mode with recovery notification
- Start with Windows option
- Structured changelog and unit tests

## Requirements 📋
- Windows 10/11
- .NET 10 Runtime (for running from source build output)
- A DuckDNS account with at least one domain and token

## Quick Start 🚀
1. Build and run:

```powershell
dotnet build DuckDnsUpdater.sln
dotnet run --project DuckDnsUpdater.csproj
```

2. Right-click the tray icon and open `DuckDNS Settings`.
3. Enter:
- Domain(s): one or more DuckDNS subdomains, comma-separated
- Token: your DuckDNS token
- Refresh interval: 5 to 60 minutes
4. Optionally enable `Start with Windows`.
5. Use `Test Config Now` to verify setup.

## Security Notes 🔒
- The DuckDNS token is stored encrypted using Windows DPAPI for the current user profile.
- Network calls use HTTPS.
- Security policy and reporting process are available in `SECURITY.md`.

## Logs and Local State 📝
- Log file: `%LOCALAPPDATA%\\DuckDnsUpdater\\duckdns-updater.log`
- Runtime state: `%LOCALAPPDATA%\\DuckDnsUpdater\\duckdns-updater.state`

## FAQ ❓
### Why is there no main window?
This app is designed to live in the tray and stay focused on one task: keeping DuckDNS up to date.

### Can I use multiple domains?
Yes. Use comma-separated DuckDNS subdomains in the settings.

### Does this app update when IP did not change?
No. It skips updates when IP is unchanged, unless you force an update.

### Why did I get a warning notification once, then no spam?
The app uses silent failure mode and only notifies when an issue starts and when it recovers.

## Development 🛠️
- Main app: `DuckDnsUpdater.csproj`
- Tests: `DuckDnsUpdater.Tests/DuckDnsUpdater.Tests.csproj`

Run tests:

```powershell
dotnet test DuckDnsUpdater.sln
```

## Contributing 🤝
Please read `CONTRIBUTING.md` and `CODE_OF_CONDUCT.md` before submitting changes.

## Contacts 📬
- Security: `security@blinksun.ca`
- Open-source: `opensource@blinksun.ca`

## Legal Disclaimer ⚖️
This project is an independent community tool and is not affiliated with DuckDNS.
You are responsible for your own DNS/account usage and compliance.

## License 📄
This project is licensed under the MIT License. See `LICENSE`.
