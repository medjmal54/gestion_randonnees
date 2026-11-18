# Migration Assessment — L'Échappée Sauvage (PHP → ASP.NET Core MVC)

**Date:** 2026-11-18  
**Workspace:** `C:\Users\user\Documents\DotNet Projects\Gestion-randonnees`  
**Target baseline in workspace:** `Gestion-randonnees.slnx` / `Gestion-randonnees/Gestion-randonnees.csproj` (`net10.0`, default ASP.NET Core MVC scaffold, no EF Core/Identity yet)

## 1. Reconnaissance summary

| Area | Finding | Status |
|---|---|---|
| Git | No `.git` directory, no remotes/branches configured | confirmed |
| .NET SDK | .NET SDK `10.0.301` installed; project targets `net10.0` | confirmed |
| Existing .NET app | Default template: `Program.cs`, `HomeController`, `Views/Home|Shared`, Bootstrap assets, no data/auth packages, no tests | confirmed |
| Legacy PHP source in workspace | Not present as application files; only prompts/skill docs copied locally (`START_PROMPT.md`, `SKILL (1).md`, `php-project-opencode-skill/*`) | confirmed |
| Legacy source inspected | Fetched and reviewed from `https://github.com/medjmal54/php_project` (main): `projet/database.sql`, `index.php`, `config/db.php`, all controllers/models, key views (home, auth, randonnee list/details, user dashboard/settings/favorites, admin dashboard/manage randonnees/participations/buses/settings), layout/header actions, and README/docs metadata | confirmed from source (remote) |
| Runtime behavior | Not executed locally (no PHP/MySQL runtime configured in workspace) | needs runtime verification |

## 2. Existing architecture (legacy)

- **Pattern:** Custom PHP "front controller" + manual routing (`projet/index.php`) dispatching by `page`/`action` to controller classes; controllers include models/views directly.
- **Data access:** PDO via `config/db.php` (`mysql`, host `localhost`, db `randonnee_tunisie`, user `root`, empty password). Models use `include 'config/db.php'` per method and run SQL directly; some models wrap rows in simple objects.
- **Auth/session:** Native PHP sessions (`session_start()` in `index.php`), role check helper `verifierAcces()`; login stores `user_id`, `user_name`, `email`, `role`, `photo_url` in session.
- **UI:** Server-rendered PHP views with shared layouts/sidebars/headers, CSS under `public/assets/css`, image uploads under `public/uploads/{profiles,randonnees}`. Home/rando/favorites use card layouts; admin/user areas include tables for management/history.
- **Schema source:** `projet/database.sql` (MySQL, InnoDB, utf8mb4) with sample admin/bus/randonnee rows.

## 3. Feature inventory (route/action map)

| Area | Route / action | Controller | Model(s) | View(s) | Access | Notes |
|---|---|---|---|---|---|---|
| Home | `home/index` (default) | `HomeController::index` | `RandonneeModel::getLatest()` | `home.php` | public | Shows up to 3 upcoming hikes (`date_debut > NOW()`), marketing/testimonial content (static in view) |
| Auth | `auth/login` | `AuthController::login` | `UserModel::findByEmail` | `auth/login.php` | public (POST/GET) | Accepts hashed or plaintext password match (legacy compatibility) |
| Auth | `auth/register` | `AuthController::register` | `UserModel::findByEmail`, `UserModel::create` | `auth/register.php` | public | Forces `role='user'`; requires email/telephone in code but validation/error handling is inconsistent |
| Auth | `auth/logout` | `AuthController::logout` | — | redirect | session user | `session_destroy()` then redirect home |
| Randonnée | `randonnee/index` | `RandonneeController::index` | `RandonneeModel::search`/`getAll` | `randonnee/index.php` | public | Filters: `region` keyword, `region_filter`, `categorie`, `difficulte`, `prix_min/max`; `getAll()` used when no filters |
| Randonnée | `randonnee/details` | `RandonneeController::details` | `RandonneeModel::findById`, `ParticipationModel::getCountByRandonnee`, `findByUserAndRandonnee` | `randonnee/details.php` | public (personalized if logged in) | Shows capacity/count, participation status/actions |
| Randonnée | `randonnee/participate` | `RandonneeController::participate` | `RandonneeModel::findById`, `ParticipationModel::{getCountByRandonnee,findByUserAndRandonnee,hasDateOverlap,create}` | redirect | logged-in user (GET) | Checks capacity, duplicate, accepted-date overlap; creates `en attente` |
| User | `user/dashboard` | `UserController::dashboard` | `ParticipationModel::getByUser` | `user/dashboard.php` | role `user` | Participation history/stats |
| User | `user/settings` | `UserController::settings` | `UserModel::{findById,updateProfile,updatePassword,updatePhoto}` | `user/settings.php` | role `user` | Profile, password change, photo upload |
| User | `user/favorites` | `UserController::favorites` | `FavoriteModel::getByUser` | `user/favorites.php` | logged-in (any role) | Lists favorite hikes |
| User | `user/toggle_favorite` | `UserController::toggle_favorite` | `FavoriteModel::toggle` | JSON | logged-in (any role), GET/AJAX | Add/remove favorite |
| User (unrouted) | `mark_read` method exists | `UserController::mark_read` | `ParticipationModel::markAsSeen` | JSON | intended logged-in | **Not wired in router** but called by `views/layout/user_header_actions.php` (`fetch` to `action=mark_read`) — likely broken |
| Admin | `admin/dashboard` | `AdminController::dashboard` | `RandonneeModel::getTotalRandonnees`, `UserModel::getTotalUsers`, `ParticipationModel::{getFilteredStats,getFilteredRevenue,getChartData}` | `admin/dashboard.php` | role `admin` | Date-filtered stats/chart, revenue (accepted) |
| Admin | `admin/manage_randonnees` | `AdminController::manageRandonnees` | `RandonneeModel::{getAll,create,update}`, `BusModel::getAll` | `admin/manage_randonnees.php` | role `admin` | List/add/edit; image upload |
| Admin | `admin/delete_randonnee` | `AdminController::deleteRandonnee` | `RandonneeModel::delete` | redirect (GET) | role `admin` | No confirmation server-side beyond UI |
| Admin | `admin/manage_participations` | `AdminController::manageParticipations` | `ParticipationModel::getRandonneeParticipations` | `admin/manage_participations.php` | role `admin` | Pending/treated split in view |
| Admin | `admin/update_participation_status` | `AdminController::updateParticipationStatus` | `ParticipationModel::{findById,updateStatus}`, `RandonneeModel::findById`, `ParticipationModel::getCountByRandonnee` | redirect (GET) | role `admin` | Accept checks capacity; reject/other status update |
| Admin | `admin/manage_buses` | `AdminController::manageBuses` | `BusModel::{getAll,create,update,delete}` | `admin/manage_buses.php` | role `admin` | CRUD via `subaction` + GET/POST mix |
| Admin | `admin/settings` | `AdminController::settings` | `UserModel::{findById,updateProfile,updatePassword,updatePhoto}` | `admin/settings.php` | role `admin` | Profile/password/photo; largely duplicated with user settings |

## 4. Data model & relationships (legacy schema)

**Source:** `projet/database.sql`

| Entity | Key fields | Constraints/notes |
|---|---|---|
| `user` | `id_user` PK AI, `nom`, `email` UNIQUE, `password`, `role` ENUM(`admin`,`user`) default `user`, `telephone`, `image_profile` | Sample admin row included; sample hash appears placeholder-like (comment claims password `password`) — **do not trust/migrate as valid credential** |
| `bus` | `id_bus` PK AI, `immat_bus`, `nb_places`, `chauffeur`, `tel_chauff` | Referenced by hikes |
| `randonnee` | `id_randonnee` PK AI, `categorie`, `titre`, `description`, `date_debut/end` DATETIME, `region`, `distance` FLOAT, `difficulte` ENUM(`Facile`,`Moyen`,`Difficile`), `capacite`, `image`, `id_bus` FK, `date_creation` TS, `statut` ENUM(`Prévue`,`En cours`,`Terminée`,`Annulée`) default `Prévue`, `prix` DECIMAL(10,2) | `id_bus` FK `ON DELETE SET NULL` |
| `participation` | `id_participation` PK AI, `id_user` FK, `id_randonnee` FK, `statut` ENUM(`en attente`,`acceptée`,`refusée`) default `en attente`, `is_seen_by_user` BOOL default false, `date_creation` TS | **UNIQUE(`id_user`,`id_randonnee`)**; both FKs `ON DELETE CASCADE` |
| `favoris` | `id_user`, `id_randonnee`, `date_creation` TS | Composite PK (`id_user`,`id_randonnee`); FKs `ON DELETE CASCADE` |

**Relationships:** User 1..* Participation *..1 Randonnee; User 1..* Favoris *..1 Randonnee; Bus 1..* Randonnee (optional).

## 5. Confirmed business rules (from source)

- **Roles/access:** `admin` and `user` roles; admin area guarded by `verifierAcces('admin')`; user dashboard/settings guarded by `verifierAcces('user')`; favorites/toggle require login but are allowed for any logged-in role (confirmed in router).
- **Registration:** Public registration sets role to `user`; login establishes session with role.
- **Hike discovery:** Public list/details; filters by region (keyword/selected), category, difficulty, min/max price; home shows latest upcoming 3; details show count of accepted participations vs capacity.
- **Participation request:** Must be logged in; rejected if hike full (`accepted count >= capacite`), if already requested for same hike, or if accepted participation overlaps requested hike dates; on success creates participation with status `en attente`.
- **Capacity semantics (legacy):** Count includes only participations with `statut = 'acceptée'` (`ParticipationModel::getCountByRandonnee`).
- **Date overlap semantics (legacy):** `hasDateOverlap` checks only user's **accepted** participations and uses range comparisons with `BETWEEN`/containment against existing hike dates.
- **Duplicate prevention:** DB unique `(id_user,id_randonnee)` and application pre-check.
- **Admin participation review:** Accept/refuse via status update; acceptance blocked if capacity reached at decision time; status update resets `is_seen_by_user = 0`.
- **Favorites:** Toggle add/remove per user+hike; favorites page lists user's favorite hikes.
- **Buses:** Admin can create/edit/delete buses; hike form assigns `id_bus`.
- **Notifications concept:** Unseen accepted/refused participation updates queried and header calls `mark_read` endpoint (endpoint routing incomplete).

## 6. Technical risks, defects, and security findings

| Severity | Finding | Evidence / status |
|---|---|---|
| Critical | **Plaintext password storage on registration**; login allows plaintext equality fallback | `AuthController::register` passes raw password to `UserModel::create` (no hash). `AuthController::login` accepts `password_verify` OR `$_POST['password'] === $user->getPassword()`. **confirmed** |
| Critical | **No CSRF protection** on state-changing forms/actions | Forms/actions are standard POST/GET without tokens. **confirmed** |
| Critical | **State-changing operations exposed via GET** (incl. from links/AJAX) | `participate`, `delete_randonnee`, `update_participation_status`, bus delete, `toggle_favorite` invoked via GET/links/fetch. **confirmed** |
| High | **Authorization gaps / routing issues** | `user/mark_read` implemented but not routed; favorites/toggle accessible to any logged-in role (may be intended but must be explicit); protected actions rely on UI + basic session checks only. **confirmed** |
| High | **Race conditions on capacity/enrollment** | Count-then-insert in participate; count-then-accept in admin update; no transaction/locking. Concurrent requests can exceed capacity. **confirmed** |
| High | **Broken/weak registration validation** | Email-uniqueness check compares string to object (`$_POST['email'] == UserModel::findByEmail(...)`) so it does not reliably detect duplicates; relies partly on DB exception behavior. Password confirmation checked but password not hashed. **confirmed** |
| High | **File upload validation insufficient** | Extension allowlist only (jpg/jpeg/png; gif allowed for user profile), no MIME/content/signature/size checks, no randomized storage isolation strategy beyond name pattern, stored paths hardcoded to `/projet/...`. **confirmed** |
| High | **Potential null dereference / missing input validation** | `updateParticipationStatus` fetches participation/rando without verifying existence before `getIdRandonnee()`/capacity checks; status parameter not validated against allowed values. **confirmed** |
| Medium | **Date-overlap logic/semantics ambiguous** | Overlap checks accepted only, uses `BETWEEN`-style conditions and may not match desired business rule for boundaries/pending requests; no overlap re-check on acceptance. **needs product decision + verification** |
| Medium | **Status and query inconsistencies** | Some revenue/accepted queries use `'Acceptée'` vs `'acceptée'`; randonnee `statut` lifecycle not visibly enforced. **confirmed** (risk/needs verification for MySQL behavior) |
| Medium | **Information exposure & insecure defaults** | DB connect dies with raw PDO exception; hardcoded DB creds in repo; sample SQL contains unusable/suspicious admin hash while claiming known password. **confirmed** |
| Medium | **Duplicate logic / maintainability** | Admin and user settings code largely duplicated; models open DB connection repeatedly; hardcoded `/projet/` URL/path assumptions. **confirmed** |
| Medium | **Incomplete/unclear product behaviors** | Can users request participation for past/cancelled hikes? Are pending overlaps allowed? Should admins be able to favorite/request? Should deleting a bus/hike be blocked in some states? Listing shows all hikes via `getAll()` when unfiltered. **product decision required** |
| Low | **Static/invented marketing claims in UI** | Home/testimonials include static claims (e.g. "Plus de 2,000 aventuriers…") not backed by data. **confirmed** (UI content) |

## 7. Proposed ASP.NET Core target architecture

- **Style:** Pragmatic modular monolith (single ASP.NET Core MVC app) on **.NET 10 / ASP.NET Core (current installed SDK)**, C#, Razor Views.
- **Suggested structure:**
  - `Controllers/` — thin MVC endpoints (Hikes, Account/Profile, Participations, Favorites, Admin/Hikes/Buses/Participations/Dashboard)
  - `Domain/` or `Models/Domain/` — entities/value concepts (`ApplicationUser` profile extension or related profile entity, `Hike`/`Randonnee`, `Bus`, `Participation`, `Favorite`) and enums (`UserRole` via Identity roles, `Difficulty`, `HikeStatus`, `ParticipationStatus`)
  - `Data/` — `ApplicationDbContext` (IdentityDbContext), EF Core entity configurations (`IEntityTypeConfiguration<T>`), migrations, seed helpers
  - `ViewModels/` — page/form/filter models and validation attributes
  - `Services/` — focused application services (e.g. `ParticipationRequestService`, `ParticipationReviewService`, `HikeQueryService`, `FileStorageService/UploadValidator`) to enforce invariants and transactions
  - `Authorization/` — policies/constants (`AdminOnly`, ownership policies if needed)
  - `wwwroot/` — migrated/adapted static assets; uploads stored outside webroot or in controlled path with safe serving rules
  - `tests/` — xUnit test project(s) for domain/service rules and authorization-critical flows
- **Tech choices:** EF Core + SQL Server provider, ASP.NET Core Identity (roles), built-in model validation/antiforgery, DI, configuration/user-secrets for connection strings, logging.
- **Data/mapping principles:** `decimal` for `prix`, `int` for capacities/seats, explicit DateTime policy (recommend storing UTC or documented local convention for Tunisia and comparing consistently), unique index `(UserId,HikeId)` for participations, composite key for favorites, explicit FK delete behaviors, indexes for common filters/dates/status.
- **Security/behavior by design:** `[Authorize]`/policies on endpoints, no role binding from public input, POST+antiforgery for mutations, server-side validation, safe upload pipeline, parameterized EF queries, concurrency-safe participation/capacity enforcement (transaction + uniqueness/capacity checks, consider row version/concurrency token strategy where appropriate).

## 8. Phased implementation plan

| Phase | Scope | Key outputs |
|---|---|---|
| **0. Assessment (current)** | Reconnaissance, mapping, risks, decisions | `docs/migration-assessment.md`, reviewed milestone plan |
| **1. Foundation** | Add EF Core/Identity/SQL Server packages; configure DbContext, Identity, authz policies, antiforgery defaults; model core entities/enums/configurations; initial migration; safe dev seeding approach; basic layout/nav shell | Buildable app with secure baseline and schema |
| **2. Core catalog & admin CRUD** | Hike/Bus management (admin), public hike listing/details/filters, image storage service | Catalog parity with corrected validation/authorization |
| **3. User participation workflow** | Request/approve/reject flow, duplicate/capacity/overlap rules, transactional/concurrency handling, user dashboard/history/notifications semantics | Core business invariants enforced and tested |
| **4. Favorites & profile** | Favorites toggle/list, user/admin profile & password/photo flows via Identity-compatible approach | Account-related features |
| **5. Admin dashboard & reporting** | Stats/charts/revenue with correct status semantics and date filtering | Admin insights parity/improvements |
| **6. Hardening & UX polish** | Responsive/accessible UI refinement, error handling, logging, upload/security hardening, test coverage expansion, docs | Release-ready increment; legacy remains untouched |

## 9. Open questions / product decisions required

1. **Date-overlap rule:** Use strict interval overlap (`existingStart < requestedEnd && requestedStart < existingEnd`) and include boundary-touching as overlap or not? Should **pending** requests block new requests and/or be considered in conflicts, or only **accepted**?
2. **Participation eligibility:** Can users request participation for hikes with status `Annulée/Terminée/En cours` or with past dates? Should capacity/status be revalidated on approval and on request?
3. **Identity/account model:** Adopt ASP.NET Core Identity fully (recommended) vs custom auth table; for migrated/existing users later, how will passwords be handled (no trustworthy legacy hashes available)?
4. **Admin capabilities:** May admins favorite/request hikes or are these user-only actions? Is self-service profile management for admins required in same way as legacy?
5. **Data migration:** Is legacy MySQL data to be imported into SQL Server in a later task? If yes, source/target mapping and handling of sample/invalid records must be specified (separate reversible process).
6. **Hike visibility/listing:** Unfiltered list currently returns all hikes (`getAll`) while home shows upcoming only—should public catalog default to upcoming/non-cancelled only?
7. **UI/content fidelity:** Preserve existing visual style/assets closely vs redesign within "outdoors/Tunisia" identity; keep or remove static testimonial/stat claims?
8. **Uploads/storage:** Store uploads under `wwwroot` with strict controls or outside webroot with a file-serving endpoint; max sizes and allowed image types/content checks?

## 10. Recommended first implementation milestone (after review)

**Milestone 1 — Secure foundation & domain/data baseline (small, coherent):**
- Add NuGet packages: EF Core (`SqlServer`, `Tools`/design as needed), ASP.NET Core Identity EF stores; keep target `net10.0` aligned with installed SDK.
- Configure `ApplicationDbContext` (+ Identity), connection string via configuration/user-secrets placeholder, DI, authentication/authorization middleware, global antiforgery for forms.
- Implement core domain entities and EF configurations/enums reflecting verified schema and invariants (unique participation per user+hike, favorite composite key, decimal price, required fields/lengths/indexes, explicit relationships/delete behavior) with documented assumptions for open decisions.
- Create and verify initial EF Core migration for SQL Server (no legacy DB mutation/import).
- Add role/policy constants and a development-only seeding strategy outline (no hardcoded real secrets; admin bootstrap via secrets/config), plus minimal layout/nav placeholders for auth-aware areas.
- Introduce test project scaffolding and initial tests for key rule calculations/constraints where practical.

**Rationale:** establishes a correct, secure base (authz, schema, invariants) before feature parity work; avoids copying legacy auth/upload/race-condition flaws into the new app. Implementation should begin only after you review/confirm the above assumptions and open decisions that affect semantics.
## Update (2026-11-18, decisions confirmed)
- **Auth approach:** custom implementation (no ASP.NET Core Identity), BCrypt already installed, JWT planned later by user. Cookie-based auth/antiforgery/authorization hardening to be handled per project needs (acknowledged by user).
- **Participation duplicate rule:** any prior request for the same hike blocks a new request (keep unique `(UserId, HikeId)`).
- **Overlap rule (different hike):** only participations with status `acceptée` block overlapping date requests/approvals; `en attente` and `refusée` do not block.
- **DateTime policy (initial assumption):** store hike/participation timestamps as SQL `datetime2` and treat as application-local (Tunisia) unless revised; comparisons must be consistent.
- **Naming convention (initial):** internal/domain identifiers in English (`Hike`, `Participation`, `Favorite`, `Bus`, `User`), French user-facing labels preserved in UI later.
