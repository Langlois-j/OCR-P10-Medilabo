## Contexte

<!-- Quelle user story / quel besoin ? Sprint concerné. -->

## Changements

<!-- Ce qui est ajouté, modifié ou supprimé. -->

## Tests

<!-- Comment vérifier : requêtes Bruno (collection du dépôt), tests unitaires, lancement Docker. -->

## Checklist

- [ ] La solution compile (`dotnet build`) sans avertissement
- [ ] Les tests ont été écrits avant le code (TDD) et passent
- [ ] Aucune fonctionnalité, dépendance ou donnée non demandée
- [ ] Pas de secret ni de mot de passe en clair

### Green Code

- [ ] Pas de requête ni d'appel HTTP dans une boucle (pas de N+1)
- [ ] Lectures SQL en `AsNoTracking()` avec projection DTO
- [ ] Index vérifiés pour les nouvelles requêtes
- [ ] Réponses limitées aux champs nécessaires, paginées si taille variable
- [ ] Dockerfile multi-stage, `.dockerignore` à jour
- [ ] Logs en `Warning` hors développement
- [ ] Le projet compile et les endpoints concernés répondent sous Bruno
