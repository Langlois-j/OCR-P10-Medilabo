# Green Code – Règles et bonnes pratiques

Document de référence pour la conception, le développement et l'exploitation de la solution de dépistage du diabète de type 2 (microservices ASP.NET Core).

- **Périmètre** : microservice Patient (SQL), microservice Notes (MongoDB), microservice Risque (sans base), Gateway Ocelot, Front (micro-frontend), images Docker.
- **Public** : développeurs, relecteurs, équipe d'exploitation.
- **Statut** : document vivant, révisé à chaque sprint ou évolution majeure d'architecture.

---

## 1. Objectif

Réduire l'empreinte environnementale du logiciel sur tout son cycle de vie en limitant la consommation de **CPU, mémoire, réseau et stockage**, sans dégrader le service rendu ni la sécurité des données patients.

L'impact d'un logiciel vient de deux sources : l'énergie consommée à l'exécution (serveurs, réseau, terminaux) et la fabrication du matériel qui l'héberge. Un code sobre réduit les deux : moins de ressources sollicitées signifie moins de serveurs à dimensionner et à renouveler.

## 2. Principes directeurs

1. **Sobriété fonctionnelle** : ne développer que ce qui est demandé. Une fonctionnalité non utilisée est du code, des données et des ressources gaspillés.
2. **Mesurer avant d'optimiser** : toute optimisation « verte » doit s'appuyer sur une mesure avant/après. Pas d'optimisation prématurée qui dégrade la lisibilité sans gain démontré.
3. **Moins de calcul, moins de données, moins d'appels** : privilégier l'algorithme le plus simple, ne transférer et ne stocker que le nécessaire.
4. **Sécurité et conformité prioritaires** : aucune règle verte ne justifie d'affaiblir l'authentification, le chiffrement ou la qualité des données (3NF).
5. **Lisibilité d'abord** : à gain équivalent, le code le plus simple à maintenir est le plus sobre à long terme.

## 3. Règles par couche

### 3.1 Architecture

| Règle | Détail |
|---|---|
| Justifier chaque service | Un microservice a un coût fixe (conteneur, runtime .NET, réseau, supervision). Tout nouveau service doit répondre à un besoin d'isolation, de scalabilité ou de technologie de stockage distincte. |
| Services sans état | Le service Risque ne possède aucune base : il peut être démarré, arrêté ou répliqué indépendamment. |
| Un service = une base | Pas de base partagée entre services. SQL (3NF) pour les patients, MongoDB pour les notes non structurées. |
| Communication minimale | Limiter les appels synchrones en chaîne ; ne pas faire transiter de données qui ne servent pas au traitement. |
| Réévaluation | Si la mesure montre un surcoût disproportionné lié au découpage, étudier le regroupement de services (monolithe modulaire). |

### 3.2 Code et algorithmique

- **Complexité** : viser O(n) pour les traitements sur les notes d'un patient. Un seul parcours pour compter les termes déclencheurs, pas une boucle par terme.
- **Recherche de termes** : stocker les termes dans un `HashSet<string>` (comparaison ordinale insensible à la casse) plutôt que des `List.Contains` répétés.
- **Chaînes** : utiliser `StringComparison.OrdinalIgnoreCase` plutôt que `ToLower()` / `ToUpper()` (une allocation par appel). Pour les concaténations en boucle, `StringBuilder`.
- **Expressions régulières** : éviter si une comparaison simple suffit. Sinon, instance réutilisée (`static readonly`) avec `RegexOptions.Compiled` ou `[GeneratedRegex]`.
- **LINQ** : ne pas enchaîner les énumérations multiples sur la même source (`Count()`, `ToList()`, `Any()` en cascade). Matérialiser une fois si nécessaire.
- **Allocations** : éviter de créer des objets dans les boucles chaudes ; préférer `Span<T>`, `ArrayPool<T>` ou des structures réutilisées uniquement là où un profilage l'a justifié.
- **Asynchronisme** : `async/await` sur toutes les opérations d'I/O (base, HTTP) pour ne pas bloquer de threads.
- **Code mort** : supprimer les dépendances NuGet, usings, classes et endpoints inutilisés.

### 3.3 Appels inter-services

- Le service Risque ne demande que les données nécessaires : sexe, date de naissance, et notes d'**un seul patient**.
- Interdire le motif **N+1** (une requête par élément d'une liste) ; préférer une requête groupée.
- Utiliser `IHttpClientFactory` (réutilisation des connexions, pas de `new HttpClient()` répété).
- Définir des **timeouts** et une politique de retry bornée (pas de retry infini qui multiplie les appels).
- Mettre en cache les réponses stables et peu coûteuses à invalider (voir 3.5).

### 3.4 Données

**SQL (relationnel, 3NF)**
- Lectures seules : `AsNoTracking()`.
- Projections (`Select`) vers des DTO plutôt que chargement d'entités complètes.
- Index sur les colonnes de recherche et de jointure ; pas d'index inutiles (coût en écriture et stockage).
- Pagination de la liste des patients.
- Types de colonnes adaptés (longueurs bornées, pas de `nvarchar(max)` par défaut).

**MongoDB (notes)**
- Index sur `PatientId`.
- Projection des seuls champs utiles ; ne pas charger tout l'historique lorsque seul un sous-ensemble est affiché.
- Pagination ou limite sur le nombre de notes retournées par défaut.
- Conserver le format d'origine des notes, sans duplication ni copie dans plusieurs collections.

**Général**
- Pas de volume de données de test supérieur au besoin.
- Politique de rétention documentée pour les logs et sauvegardes.

### 3.5 Gateway et réseau

- Compression des réponses (gzip/brotli) activée.
- Cache Ocelot (`FileCacheOptions`) avec durée courte sur les lectures répétées non sensibles aux mises à jour immédiates. Ne jamais mettre en cache de données patient sans analyse de confidentialité préalable.
- Charges utiles JSON sans champs superflus ni valeurs nulles inutiles.
- Niveau de logs `Warning` (ou `Information` ciblé) en production ; jamais `Debug`/`Trace` en continu.
- Éviter le polling ; préférer les appels déclenchés par l'action de l'utilisateur.

### 3.6 Front-end

- Interface sobre : pas de framework JavaScript lourd ni de bibliothèque ajoutée pour une seule fonction.
- Pas d'images, vidéos, polices ou animations décoratives.
- Minification et bundling des CSS/JS ; en-têtes de cache sur les ressources statiques.
- Pas de rafraîchissement automatique de la page ; chargement des notes et du risque uniquement sur la page patient consultée.
- Respect de l'accessibilité et de la légèreté des pages (un front léger est aussi plus rapide sur des terminaux modestes).

### 3.7 Docker et infrastructure

- **Builds multi-stage** : compilation dans l'image SDK, exécution dans une image runtime légère (`aspnet` en variante `alpine` ou `chiseled`, selon compatibilité).
- Un `.dockerignore` par service (exclure `bin/`, `obj/`, `.git`, fichiers de tests).
- Ordonner les instructions du Dockerfile pour maximiser le cache de build (restauration des dépendances avant la copie du code).
- Fixer des **limites CPU et mémoire** par conteneur dans `docker-compose.yml`.
- Ne pas embarquer d'outils de debug dans les images de production.
- Arrêter ou supprimer les environnements inutilisés (recette, démonstration).
- Un seul `docker-compose.yml` pour l'ensemble de la solution.

### 3.8 Sécurité compatible avec la sobriété

- Authentification via Identity : mots de passe hachés, jetons à durée de vie adaptée.
- Éviter de ré-authentifier inutilement entre services à chaque appel interne si un jeton valide existe.
- La compression et le cache ne doivent jamais exposer de données patient à des tiers non autorisés.

## 4. Mesure et suivi

### 4.1 Outils recommandés

| Domaine | Outil | Usage |
|---|---|---|
| Mémoire / GC / CPU | `dotnet-counters`, `dotnet-trace`, profileur Visual Studio | Détecter allocations excessives et points chauds |
| Micro-benchmarks | BenchmarkDotNet | Comparer deux implémentations de l'algorithme de risque |
| Conteneurs | `docker stats` | Suivre CPU/mémoire/réseau par service |
| Requêtes SQL | Logs EF Core (`LogTo`), plans d'exécution | Repérer N+1 et requêtes lourdes |
| Requêtes Mongo | `explain()`, profiler MongoDB | Vérifier l'usage des index |
| Web | Lighthouse, EcoIndex | Poids de page, nombre de requêtes |
| Énergie | Scaphandre, GreenFrame (à valider selon l'environnement) | Estimation de la consommation |

### 4.2 Indicateurs à suivre

- Taille des images Docker (par service).
- Consommation mémoire et CPU au repos et en charge (par conteneur).
- Temps de réponse et poids des réponses des endpoints principaux.
- Nombre d'appels inter-services par affichage de page patient.
- Poids de la page patient et nombre de requêtes front.

Établir une **mesure de référence (baseline)** à la fin de chaque sprint, puis comparer après chaque optimisation.

### 4.3 Signaux d'alerte en revue de code

- Boucles imbriquées sur des collections de taille variable.
- Requête base de données ou appel HTTP dans une boucle.
- Entité complète chargée pour lire un seul champ.
- Allocations de chaînes répétées (`ToLower`, concaténation en boucle).
- Énumérations LINQ multiples de la même source.
- Dépendance ajoutée pour une fonction triviale.
- Logs verbeux laissés actifs.

## 5. Plan d'actions priorisé

| # | Problème visé | Action | Indicateur de suivi | Effort |
|---|---|---|---|---|
| 1 | Parcours multiples des notes | Algorithme de risque en un seul passage avec `HashSet` des termes | Temps d'exécution (BenchmarkDotNet), allocations | Faible |
| 2 | Chargements de données superflus | Projections EF et Mongo, `AsNoTracking()`, index | Taille des réponses, durée des requêtes | Faible |
| 3 | Transferts réseau volumineux | Compression Ocelot, JSON allégé | Poids des réponses | Faible |
| 4 | Images lourdes | Multi-stage, images runtime allégées, `.dockerignore` | Taille des images | Faible |
| 5 | Listes non bornées | Pagination patients et notes | Mémoire du service, temps de réponse | Moyen |
| 6 | Appels répétés entre services | Cache court Ocelot sur lectures non sensibles | Nombre d'appels inter-services | Moyen |
| 7 | Ressources non plafonnées | Limites CPU/mémoire dans docker-compose | `docker stats` | Faible |
| 8 | Logs excessifs | Niveaux de logs par environnement | Volume de logs stockés | Faible |
| 9 | Surcoût du découpage | Réévaluer le regroupement de services après mesure | Mémoire totale, nombre de conteneurs | Élevé |

## 6. Intégration dans le processus de développement

**Definition of Done – volet Green Code** (à cocher pour chaque user story) :
- [ ] Aucune fonctionnalité, dépendance ou donnée non demandée ajoutée.
- [ ] Pas de requête dans une boucle ni de N+1.
- [ ] Lectures SQL en `AsNoTracking()` avec projection.
- [ ] Index vérifiés pour les nouvelles requêtes.
- [ ] Réponses API limitées aux champs nécessaires et paginées si la taille est variable.
- [ ] Dockerfile en multi-stage, `.dockerignore` à jour.
- [ ] Niveaux de logs conformes à l'environnement.

**Revue de code** : le relecteur applique la liste des signaux d'alerte (§4.3).

**Intégration continue** (pistes) : analyse statique (analyseurs Roslyn), suivi de la taille des images, benchmark sur l'algorithme de risque avec seuil de régression.

**Revue périodique** : à chaque fin de sprint, comparer les indicateurs à la baseline et réviser ce document.

## 7. Limites et points d'attention

- Les gains réels dépendent du contexte d'exécution (volumétrie, matériel, hébergeur) : ils doivent être mesurés, pas supposés.
- Les normes Green Code ne sont pas encore pleinement stabilisées ; ce document s'appuie sur des pratiques courantes et sera mis à jour avec les référentiels qui émergent.
- Certaines optimisations (cache, regroupement de services) ont un coût en complexité ou en sécurité : arbitrer au cas par cas.

## 8. Références

- Institut du Numérique Responsable – *Green code : écrivez du code vert !*
- Cours OpenClassrooms – *Appliquez les principes du Green IT dans votre entreprise*
- Scitepress – *How Green Are Java Best Coding Practices?* (2014)
- Documentation Microsoft : performances ASP.NET Core, Entity Framework Core (suivi des modifications et requêtes), Ocelot (cache)
- Documentation Docker : builds multi-stage et bonnes pratiques de Dockerfile