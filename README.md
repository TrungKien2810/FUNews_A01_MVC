# FUNews A01 - ASP.NET Core MVC

FUNewsManagementSystem built with ASP.NET Core MVC (.NET 8), EF Core and SQL Server.
Requirements: [docs/REQUIREMENTS.md](docs/REQUIREMENTS.md).

## Solution structure (`src/TrungKien_SE19C_A01.sln`)

| Project | Role |
|---|---|
| `BusinessObjects` | EF Core entities + `FUNewsManagementContext` (reads the connection string from `appsettings.json`) |
| `DataAccessObjects` | DAOs implemented as **Singletons** (`AccountDAO.Instance`, `CategoryDAO.Instance`, ...) |
| `Repositories` | **Repository pattern**: `IXxxRepository` / `XxxRepository` calling the DAOs |
| `Services` | Business rules (`IXxxService` / `XxxService`) calling repositories |
| `TrungKienMVC` | ASP.NET Core MVC web app. Controllers only use Services (never the DbContext) |

Flow: `Controller -> Service -> Repository -> DAO -> DbContext`.

## Run

1. Create the database: run `database/FUNewsManagement.sql` in SQL Server (creates `FUNewsManagement`).
   The script starts with a re-run guard: if `FUNewsManagement` already exists it is dropped and
   rebuilt, so running it again **resets the database to the seed data**. Run it only when you
   want that reset.
2. Edit `src/TrungKienMVC/appsettings.json` → `ConnectionStrings:DefaultConnection` for your SQL Server.
3. Open `src/TrungKien_SE19C_A01.sln` in Visual Studio and run `TrungKienMVC`
   (or `cd src/TrungKienMVC && dotnet run`). The Login page is the default page.

> **Which SQL Server?** `Server=(local)` is not portable across machines with more than one
> instance. On the machine this was verified on, port 1433 belonged to `SQLEXPRESS02`, so
> `(local)` reached that instance while `sqlcmd -S localhost` reached the default one over
> shared memory. If login fails with error 4060 ("Cannot open database"), the script and the
> app are talking to two different instances — point the connection string at the instance
> that holds the database, e.g. `Server=localhost\SQLEXPRESS02`.

## Accounts

| Role | Email | Password |
|---|---|---|
| Admin (from `appsettings.json`) | admin@FUNewsManagementSystem.org | @@abc123@@ |
| Staff (role 1) | IsabellaDavid@FUNewsManagement.org | @1 |
| Lecturer (role 2) | EmmaWilliam@FUNewsManagement.org | @1 |

## Features

- **Anonymous / Lecturer**: view and search active news articles (by keyword, category, tag) and read details.
- **Admin**: manage accounts (CRUD + search by name/email/role); statistic report by StartDate–EndDate on
  news created date, sorted descending (totals, by category, by author, by date, article list).
- **Staff**: manage categories (CRUD + search; a category used by news cannot be deleted), manage news
  articles including tags (CRUD + search by keyword/category/tag/status), manage own profile, view own news history.
- Create / Edit run in Bootstrap popup dialogs (AJAX, server + client validation); Delete always shows a confirmation popup.
- All inputs are validated with data annotations (`ViewModels/*`).

## Interface

The look is a deliberate choice, described below.

**Direction — "newsroom ledger".** Serif for the news itself (headlines, the wordmark),
a neutral sans for the chrome, and a monospaced face with tabular figures for every
identifier, count and date — the ruled columns of a story ledger. One accent colour
(spruce, hue 172) is spent on the primary action and the current nav mark and nothing
else. Surfaces are a flat system separated by hairlines; only the modal carries elevation.

**Files that carry it**

| File | What it holds |
|---|---|
| `wwwroot/css/tokens.css` | the whole system: type scale (ratio 1.25, 7 steps), one tinted neutral ramp, semantic colour layer, spacing scale in three tiers, radius by element size, motion by role, and the dark theme |
| `wwwroot/css/site.css` | components composed only from those tokens. Nothing here names a raw hex or a one-off pixel |
| `wwwroot/js/site.js` | the popup helper: loading state, in-place busy buttons, focus into the dialog, a Tab cycle, Escape, and focus returning to the trigger |

Screens reference the semantic layer (`--bg`, `--surface`, `--text`, `--accent`) only.
Dark mode is a second design pass — elevation goes lighter, chroma drops, text is never
pure white — driven by `prefers-color-scheme`, with `color-scheme` declared so the
browser stops painting native controls against a background we never chose.

> **Note on the vendored Bootstrap.** `wwwroot/lib/bootstrap` is **5.1.0**, which predates
> the `--bs-btn-*` variable system (that arrived in 5.2). Its variant rules write
> `color:#0d6efd` literally, so setting the variables alone changes nothing. `site.css`
> therefore sets the real properties *and* the variables, so a future upgrade keeps working.

### Verified

Checked by running the app against a real `FUNewsManagement` database on SQL Server:

- **Responsive layout** at 375 / 768 / 1440 px in both themes — no horizontal page
  overflow. The news, category and account ledgers are wide by nature and scroll inside
  their own container; `min-width: 64rem` is what makes them scroll rather than crush.
- **Keyboard:** Tab order starts at the skip link and stays visible throughout; the popup
  opens with `role="dialog"`, `aria-modal="true"` and an accessible name, moves focus to
  its first field, keeps Tab inside, closes on Escape, and returns focus to the button
  that opened it.
- **Popup scrolling:** all 12 popups at 1000×720, 1440×900, 390×700 and 1200×480 — the
  body scrolls internally, the header and the submit button never leave the viewport, and
  the overlay behind never has to scroll.
- **Coarse pointer:** every control reaches 44px under `@media (pointer: coarse)`; under a
  mouse the dense sizes stay, which is what a desk-bound console wants.

Not verified: no real screen reader, no cross-browser or real-device pass, no 200% zoom
sweep beyond the 375px renders, and no usability testing with actual staff.

