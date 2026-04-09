# Security Policy

## Supported Versions
Security updates are best effort for the latest `main` branch.

## Reporting a Vulnerability
Please do not disclose vulnerabilities publicly in issues.

Send a private report to:
- security contact: `security@blinksun.ca`
- general open-source contact: `opensource@blinksun.ca`

Include:
- Affected version/commit
- Reproduction steps
- Impact assessment
- Suggested mitigation (if known)

## Response Targets
- Initial acknowledgement: within 72 hours
- Triage status update: within 7 days
- Fix target: depends on severity and complexity

## Disclosure Process
1. We acknowledge and triage.
2. We validate and prepare a fix.
3. We release a patched version.
4. We publish a coordinated security note.

## Security Hardening Notes
Current security controls include:
- HTTPS-only communication
- DPAPI token protection (CurrentUser)
- Input validation and routable IP checks
- Retry/timeout strategy with controlled notifications
