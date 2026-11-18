# L'Échappée Sauvage — Gestion de randonnées

Application web de gestion de randonnées (club/organisateur) migrée depuis un legacy PHP/MySQL vers **ASP.NET Core MVC (.NET 10)** avec **EF Core (code-first)** et **SQL Server**. Elle permet de consulter et filtrer les randonnées, gérer les inscriptions (demandes de participation), les favoris, un tableau de bord utilisateur et un espace d’administration (randonnées, bus, validation des participations).

> État : **baseline de migration en cours** (fonctionnel en développement, non durci pour la production). Base de données non appliquée automatiquement, pas de jeu de données seed admin par défaut.

## Fonctionnalités

### Public
- Accueil avec randonnées à venir (`Statut = Prevue`).
- Catalogue des randonnées avec filtres (recherche, région, difficulté, dates) — `Hikes/Index`.
- Détail d’une randonnée (infos, capacité, bus associé) — `Hikes/Details`.

### Utilisateur authentifié (cookie)
- Inscription / connexion / déconnexion (hachage mot de passe **BCrypt**), contrôle d’accès par rôles.
- Demande de participation à une randonnée avec règles métier :
  - unicité de demande par couple `(Utilisateur, Randonnée)` ;
  - blocage si **chevauchement de dates** avec une participation déjà **acceptée** ;
  - contrôle de capacité (places acceptées).
- Favoris (ajout/retrait), tableau de bord (demandes/statuts, notifications lues), profil (infos, changement de mot de passe, photo).

### Administration (`/Admin`, rôle `Admin`)
- Tableau de bord (indicateurs sur période : volumes, statuts, revenu accepté).
- CRUD randonnées (dont image) et bus.
- Revue des participations (accepter/refuser) avec contrôles de capacité et de chevauchement sur acceptation.

### API (JWT Bearer)
- `POST /api/Auth/token` — émission d’un JWT pour clients API (email/mot de passe). Le site MVC utilise l’authentification **cookie** par défaut ; le schéma **JWT Bearer** est configuré pour les scénarios API.

## Architecture et stack technique

- **Framework** : ASP.NET Core MVC, C#, .NET 10 (`net10.0`), Razor Views.
- **Données** : Entity Framework Core 10 (code-first), provider SQL Server, migrations dans `Gestion-randonnees/Data/Migrations/`.
- **Authentification/autorisation** : cookie auth custom (sans ASP.NET Core Identity), rôles (`User`, `Admin`), politique `AdminOnly` ; BCrypt pour les mots de passe ; JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) + service de génération de jeton.
- **Couche présentation** : Controllers MVC + API (`Controllers/`, `Controllers/Api/`), ViewModels/DTOs, Razor Views.

Diagrammes (C4, domaine, flux) : dossier [`architecture/`](architecture/) — fichier Draw.io ouvrable avec [diagrams.net / draw.io](https://app.diagrams.net/).

### Structure du dépôt

```text
.
├─ Gestion-randonnees.slnx
├─ Gestion-randonnees/                # Projet web ASP.NET Core MVC
│  ├─ Controllers/                    # MVC + Api/
│  ├─ Data/                           # ApplicationDbContext, Migrations
│  ├─ DTOs/                           # Objets de transfert (API/liste)
│  ├─ Models/Entities/                # Entités EF Core
│  ├─ Models/Enums/                   # Rôles, statuts, difficulté
│  ├─ Services/                       # Services applicatifs (ex. JWT)
│  ├─ ViewModels/                     # ViewModels par fonctionnalité
│  ├─ Views/                          # Razor Views
│  ├─ wwwroot/                        # Fichiers statiques
│  ├─ appsettings*.json               # Configuration (placeholders)
│  └─ Program.cs                      # DI, auth, pipeline, routes
├─ docs/                              # Évaluation de migration / notes
├─ architecture/                      # Diagrammes (Draw.io, C4/UML)
└─ README.md
```

### Modèle de domaine (aperçu)

- **User** (rôle, profil, hash mot de passe) — 1..N **Participation**, N..N **Hike** via **Favorite**.
- **Hike** (dates, région, difficulté, statut, capacité, prix, image) — N..1 **Bus** (optionnel).
- **Participation** (statut `EnAttente/Acceptee/Refusee`, dates) — contrainte d’unicité `(UserId, HikeId)`.
- **Favorite** — clé composite `(UserId, HikeId)`.

Voir `architecture/architecture.drawio` pour les vues C4 et le diagramme de domaine.

## Prérequis

- [.NET SDK 10.x](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB ou SQLEXPRESS) accessible depuis la machine de dev
- (Optionnel) `dotnet-ef` pour appliquer les migrations : `dotnet tool install --global dotnet-ef`

## Démarrage rapide

1. **Configurer la base de données** dans `Gestion-randonnees/appsettings.json` (ou via User Secrets / variables d’environnement) :
   - `ConnectionStrings:DefaultConnection` : pointez vers votre instance SQL Server.
2. **Configurer JWT** (pour l’API) :
   - `Jwt:Key` : secret fort (>= 32 caractères). **Ne pas committer de secret réel** ; utiliser User Secrets en dev.
   - `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiresMinutes` selon besoin.
3. **Restaurer et construire** :
   ```bash
   dotnet restore Gestion-randonnees.slnx
   dotnet build Gestion-randonnees.slnx
   ```
4. **Appliquer les migrations** (base créée/mise à jour) :
   ```bash
   dotnet ef database update --project Gestion-randonnees/Gestion-randonnees.csproj --startup-project Gestion-randonnees/Gestion-randonnees.csproj
   ```
5. **Lancer l’application** :
   ```bash
   dotnet run --project Gestion-randonnees/Gestion-randonnees.csproj
   ```
6. **Créer un administrateur** : inscrire un compte via l’UI puis promouvoir son rôle en `Admin` en base (table `Users`, colonne `Role`) — aucun seed admin automatique à ce stade.

## Configuration (notes)

- `appsettings.json` contient des valeurs **placeholder/dev** ; préférez `dotnet user-secrets` ou des variables d’environnement pour les secrets et chaînes sensibles.
- L’authentification principale de l’UI est basée sur les cookies ; les endpoints protégés par JWT doivent être configurés explicitement avec le schéma Bearer si ajoutés.

## État connu / travaux restants

- Migration EF générée mais application et smoke tests runtime à valider selon l’environnement SQL.
- Pas de seed admin/utilisateurs de démo, pas de suite de tests automatisés.
- Durcissement sécurité, gestion de concurrence/capacité avancée, parité UI avec le legacy et industrialisation (CI/CD, observabilité) à poursuivre.
- Détails d’analyse et décisions : [`docs/migration-assessment.md`](docs/migration-assessment.md).

## Licence

À définir.
