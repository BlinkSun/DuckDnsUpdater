# DuckDnsUpdater Modernization Checklist

Ce fichier suit la progression par rounds. On coche uniquement les éléments réellement terminés.

## Étapes

- [x] 1. Passer tous les appels réseau en HTTPS
- [x] 2. Ajouter un HttpClient unique + structure propre d'usage
- [x] 3. Ajouter timeout + retry intelligent
- [x] 4. Valider strictement les inputs
- [x] 5. Chiffrer le token localement (DPAPI Windows)
- [x] 6. Ajouter des logs locaux lisibles
- [x] 7. Ajouter un statut health dans le menu tray
- [x] 8. Option démarrage avec Windows
- [x] 9. Min interval guard
- [x] 10. Optimiser la logique update sur changement d'IP
- [x] 11. Silent failures + notif de reprise
- [x] 12. Moderniser visuellement le SettingsForm
- [x] 13. Ajouter des tests unitaires
- [x] 14. Séparer UI et logique (petite architecture clean)
- [x] 15. Ajouter endpoint IP de secours
- [x] 16. Ajouter "Test config maintenant"
- [x] 17. Versioning/release propre
- [x] 18. Vérifier IP routable valide avant update
- [x] 19. Créer un README public professionnel (FR/EN, usage, sécurité, FAQ)
- [x] 20. Ajouter LICENSE + notice légale + disclaimers
- [x] 21. Ajouter CONTRIBUTING + CODE_OF_CONDUCT
- [x] 22. Ajouter issue templates + PR template GitHub
- [x] 23. Ajouter CI GitHub Actions (build + tests)
- [x] 24. Ajouter guide de release (tags, changelog, artefacts)
- [x] 25. Ajouter SECURITY.md (politique de vulnérabilités)

## Journal des rounds

- Round 1:
  - Checklist créée au root.
  - Réseau migré vers HTTPS (`duckdns` + providers IP publics).
  - `HttpClient` partagé introduit (évite recréation à chaque appel).
  - Timeout (10s) + retry avec backoff léger ajoutés.
  - Fallback provider IP ajouté (`checkip.amazonaws.com` puis `api.ipify.org`).
  - Logging local ajouté: `%LOCALAPPDATA%\\DuckDnsUpdater\\duckdns-updater.log`.
  - Build validé (`dotnet build` OK, 0 erreurs, 0 warnings).

- Round 2:
  - Validation stricte ajoutée côté Settings:
    - domaine DuckDNS (subdomain(s) valides, séparés par virgules),
    - token au format GUID,
    - normalisation trim/lowercase pour domaine.
  - Validation runtime ajoutée côté updater (si config legacy invalide, update bloquée proprement avec log + notification).
  - Garde-fou intervalle ajouté:
    - intervalle forcé entre 5 et 60 minutes,
    - correction automatique des anciennes valeurs hors plage.
  - Vérification IP publique routable ajoutée (IPv4/IPv6), avec rejet des plages privées/réservées avant update.
  - Build validé (`dotnet build` OK, 0 erreurs, 0 warnings).

- Round 3:
  - Health menu ajouté dans le tray:
    - `Status`,
    - `Last check`,
    - `Last success`,
    - `Last IP`,
    - `Last error`,
    - `Next check in`.
  - Nouvel item `Test Config Now` ajouté:
    - valide la config,
    - teste l'IP publique,
    - teste la réponse DuckDNS,
    - affiche un résultat explicite succès/échec.
  - Le health est mis à jour dynamiquement pendant les checks/update/tests.
  - Build validé (`dotnet build` OK, 0 erreurs, 0 warnings).

- Round 4:
  - Optimisation update:
    - persistance locale de la dernière IP réussie et du dernier succès (`%LOCALAPPDATA%\\DuckDnsUpdater\\duckdns-updater.state`),
    - restauration au démarrage pour éviter un update inutile après redémarrage si l'IP n'a pas changé.
  - Mode échecs silencieux:
    - plus de spam de notifications identiques en boucle,
    - une alerte discrète au début d'un incident,
    - notification de reprise automatique quand la santé revient OK.
  - Build validé (`dotnet build` OK, 0 erreurs, 0 warnings).

- Round 5:
  - Sécurisation token:
    - ajout de `TokenProtector` (DPAPI `CurrentUser`),
    - stockage du token en format protégé (`dpapi:...`) dans les settings,
    - compatibilité rétroactive avec anciens tokens en clair (migration à la sauvegarde).
  - Démarrage Windows:
    - ajout de `StartupManager` (registre `HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run`),
    - option `Start with Windows` ajoutée dans `SettingsForm`.
  - Polish visuel Settings:
    - police `Segoe UI`,
    - layout élargi et plus lisible,
    - champ token masqué (`UseSystemPasswordChar`),
    - boutons Save/Cancel configurés avec `AcceptButton`/`CancelButton`.
  - `MainForm` utilise désormais le token déchiffré en mémoire pour les appels DuckDNS.
  - Build validé (`dotnet build` OK, 0 erreurs, 0 warnings).

- Round 6:
  - Checklist enrichie pour publication publique:
    - étapes 19 à 25 ajoutées (README pro, LICENSE, CONTRIBUTING, templates GitHub, CI, release guide, SECURITY).
  - Séparation UI/logique (étape 14):
    - création de `Core/DuckDnsConfigurationRules.cs` (normalisation + validation config + garde-fou intervalle),
    - création de `Core/PublicIpAddressRules.cs` (règles IP publique routable),
    - `MainForm` et `SettingsForm` branchés sur ces règles partagées (suppression de duplication).
  - Tests unitaires (étape 13):
    - nouveau projet `DuckDnsUpdater.Tests` ajouté à la solution,
    - tests xUnit pour validation config/domain/token/intervalle,
    - tests xUnit pour validation IPv4/IPv6 routable.
  - Validation: `dotnet test DuckDnsUpdater.sln` OK (23 tests pass).

- Round 7:
  - Ménage de solution/projets:
    - exclusion complète de `DuckDnsUpdater.Tests/**` du projet principal (`Compile`, `None`, `Content`, `EmbeddedResource`) pour éviter tout mélange visuel dans l'explorateur du projet WinForms.
  - Versioning/release (étape 17) rendu concret:
    - version centralisée dans `DuckDnsUpdater.csproj` (`Version`, `AssemblyVersion`, `FileVersion`, `InformationalVersion`),
    - `About` utilise désormais la version assembly dynamique (plus de version hardcodée),
    - ajout d'un `CHANGELOG.md` initial pour tracer les releases.
  - Validation: `dotnet build DuckDnsUpdater.sln` OK et `dotnet test DuckDnsUpdater.sln` OK (23 tests pass).

- Round 8:
  - Publication publique (étapes 19 a 25) completee:
    - `README.md` pro FR/EN (usage, securite, FAQ, logs, dev),
    - `LICENSE` (MIT) + `NOTICE.md` (disclaimers),
    - `CONTRIBUTING.md` + `CODE_OF_CONDUCT.md`,
    - templates GitHub: bug/feature issue templates + PR template,
    - CI GitHub Actions: `.github/workflows/ci.yml` (restore/build/test sur Windows),
    - guide release: `docs/RELEASING.md`,
    - politique securite: `SECURITY.md`.
  - Validation: `dotnet build DuckDnsUpdater.sln` OK et `dotnet test DuckDnsUpdater.sln` OK (23 tests pass).

- Round 9:
  - Personnalisation finale pre-publication:
    - contact securite passe a `security@blinksun.ca`,
    - contact general open-source ajoute: `opensource@blinksun.ca`,
    - docs alignees (`README.md`, `CONTRIBUTING.md`, `SECURITY.md`, `.github/ISSUE_TEMPLATE/config.yml`).
  - Validation finale: `dotnet build DuckDnsUpdater.sln` OK et `dotnet test DuckDnsUpdater.sln` OK (23 tests pass).
