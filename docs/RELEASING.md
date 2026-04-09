# Release Guide

This project uses tag-based releases with automatic binary assets.

## 1. Prepare
- Ensure `dotnet build` and `dotnet test` pass
- Update `CHANGELOG.md`
- Bump version in `DuckDnsUpdater.csproj`

## 2. Commit and Tag
```powershell
git add .
git commit -m "release: vX.Y.Z"
git tag vX.Y.Z
git push origin master --tags
```

## 3. Automated Release Assets
When a tag matching `v*` is pushed:
- GitHub Actions workflow `release.yml` runs
- Builds and tests the solution
- Publishes a `win-x64` self-contained single-file app
- Packages and uploads:
  - `DuckDnsUpdater-win-x64-vX.Y.Z.zip`
  - `DuckDnsUpdater-win-x64-vX.Y.Z.zip.sha256.txt`
- Attaches those files to the matching GitHub Release

## 4. GitHub Release Notes
Create or edit the release for tag `vX.Y.Z`:
- Title: `vX.Y.Z`
- Notes based on `CHANGELOG.md`

## 5. Post-release
- Verify release assets and checksum
- Verify README badges and CI status
- Announce release in project channels
