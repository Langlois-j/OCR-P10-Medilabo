# OCR-P10-Medilabo

Application de dépistage du risque de diabète de type 2, développée en architecture **microservices** (ASP.NET Core / .NET 8) pour un client du secteur de la santé.

## Architecture

## Lancer le projet

## Workflow Git

### Branches

- `main` : version livrable. Reçoit uniquement les branches `release/*`. Chaque fin de sprint est taguée (`v1.0-s1`, `v2.0-s2`, `v3.0-s3`).
- `develop` : branche d'intégration.
- Branches de travail créées depuis `develop`, supprimées après merge.

| Préfixe | Usage |
|---|---|
| `feature/` | nouvelle fonctionnalité |
| `test/` | tests seuls |
| `docs/` | documentation |
| `chore/` | configuration, outillage |
| `refactor/` | refactoring sans changement fonctionnel |
| `release/` | clôture d'un sprint |

Nommage : `feature/s<N>-<service>-<sujet>`, par exemple `feature/s1-patient-seed`.

### Commits

Format `type(scope): message`, en anglais, à l'impératif. Exemple : `test(patient): add Bruno requests for list and detail`.


### Pull requests

- Une PR par branche, vers `develop`, mergée avec **Create a merge commit**.
- La description suit le modèle du dépôt (contexte, changements, tests, checklist dont Green Code).
- Les tests sont écrits avant le code (TDD).

### Fin de sprint

1. Création de `release/sN` depuis `develop`.
2. Test complet : `docker compose up` depuis zéro, démonstration de bout en bout.
3. Merge dans `main`, puis tag `vN.0-sN`.
4. Merge retour dans `develop`.

## Green Code
