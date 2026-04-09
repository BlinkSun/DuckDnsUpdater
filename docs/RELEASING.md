# Release Guide

This project uses tag-based releases.

## 1. Prepare
- Ensure `dotnet build` and `dotnet test` pass
- Update `CHANGELOG.md`
- Bump version in `DuckDnsUpdater.csproj`

## 2. Commit and Tag
```powershell
git add .
git commit -m "release: vX.Y.Z"
git tag vX.Y.Z
git push origin main --tags
```

## 3. GitHub Release
Create a GitHub release from tag `vX.Y.Z` with:
- Title: `vX.Y.Z`
- Notes from `CHANGELOG.md`
- Optional artifacts:
  - zipped `bin/Release/net10.0-windows`

## 4. Post-release
- Verify README badge and CI status
- Announce release in project channels
