# Contributing Guide

Thanks for contributing to DuckDNS Updater.

## Ground Rules
- Keep the app tray-focused and lightweight.
- Preserve security defaults (HTTPS, token protection, safe validation).
- Prefer small, reviewable pull requests.

## Development Setup
```powershell
dotnet restore DuckDnsUpdater.sln
dotnet build DuckDnsUpdater.sln
dotnet test DuckDnsUpdater.sln
```

## Branch and Commit Style
- Branch naming: `feature/...`, `fix/...`, `docs/...`, `chore/...`
- Commit style: clear imperative summary, for example:
  - `fix: prevent duplicate warning notifications`
  - `docs: add security policy`

## Pull Request Checklist
- Build succeeds locally
- Tests pass locally
- Changelog updated when behavior changes
- Security impact considered
- UI text kept clear and concise

## Scope Guidance
Please avoid unrelated refactors in the same PR.

## Reporting Security Issues
Do not open public issues for security vulnerabilities.
Use the process in `SECURITY.md`.

General contributor contact: `opensource@blinksun.ca`
