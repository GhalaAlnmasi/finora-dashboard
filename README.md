# FINORA

FINORA is a standalone **.NET 10 Blazor WebAssembly** personal-finance dashboard. It is a portfolio demonstration: sample Saudi riyal (SAR) activity, monthly budgets, analytics, and preferences all run in the browser. There is no backend, no authentication, and no bank connection.

Data is stored in `localStorage` on the visitor’s device. Clearing site data, using a private window, or blocking storage will remove it.

## Screenshots

Add PNG captures here after you take them from a running build:

| Overview | Transactions |
| --- | --- |
| `docs/screenshots/overview.png` | `docs/screenshots/transactions.png` |

| Budgets | Analytics | Settings |
| --- | --- | --- |
| `docs/screenshots/budgets.png` | `docs/screenshots/analytics.png` | `docs/screenshots/settings.png` |

Suggested shots: Overview in light mode, Settings in dark mode, and a mobile navigation frame.

## Features

- **Overview** — monthly income, expenses, savings, balance, and charts from completed activity
- **Transactions** — search, filter, and CRUD against shared local state
- **Budgets** — category envelopes with on-track / warning / at-limit / over-budget health
- **Analytics** — lookback trends, category mix, and budget attention
- **Settings** — sample profile, light/dark theme, SAR and date display, JSON backup import/export, demo reset
- **Scheduled items** — dates after today stay out of completed totals until that date
- **Privacy-first demo** — everything stays in the browser; imports are validated before replacing data

## Tech stack

- C# / .NET 10
- Blazor WebAssembly
- Razor components and scoped CSS
- `System.Text.Json` + `localStorage`
- xUnit
- Static hosting (Vercel-ready `wwwroot` publish)

## Local installation

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone <your-repo-url>
cd finora-dashboard
dotnet restore
dotnet run --launch-profile http
```

Open `http://localhost:5097`.

## Build and test

```bash
dotnet build
dotnet test finora-dashboard.Tests/finora-dashboard.Tests.csproj
dotnet publish finora-dashboard.csproj -c Release -o publish
```

The static site is **`publish/wwwroot`**. That folder must contain `index.html`, `_framework/`, `css/`, and `js/`.

To preview the published files locally (SPA fallback included):

```bash
npx --yes serve publish/wwwroot -s -l 4173
```

Then open `http://localhost:4173` and refresh `/transactions`, `/budgets`, `/analytics`, and `/settings`.

## Architecture

| Area | Role |
| --- | --- |
| `Pages/` | Overview, Transactions, Budgets, Analytics, Settings |
| `Layout/` | Shell, sidebar, header |
| `Components/` | Charts, editors, dialogs, month selector |
| `Services/TransactionStateService` | Ledger, summaries, `finora.finance.v1` |
| `Services/BudgetStateService` | Envelopes, `finora.budgets.v1` |
| `Services/PreferencesService` | Profile, theme, formats, `finora.preferences.v1` |
| `Services/AnalyticsMath`, `BudgetMath`, `Money`, `Dates` | Pure calculations and display |
| `wwwroot/js/finora.js` | Storage, theme, download, focus trap |

Pages subscribe to shared `Changed` events so Overview, Budgets, and Analytics stay in sync with the ledger. Existing browser keys are loaded as-is; FINORA does not wipe a visitor’s stored demo on deploy.

## Demo data and privacy

- Default sample profile: **Lina Faris**, Riyadh (editable, not an account)
- Seed transactions: June–November 2026 in SAR, plus an opening cash figure
- November dates after “today” are treated as scheduled
- Backup files are versioned JSON (`format: FINORA`, version `1`). Invalid or unknown files are rejected; current data is left untouched
- Reset restores seed transactions and budgets only, after confirmation. Theme and display name stay as saved

FINORA has **no live bank feed, no login, and no server database**.

## Deployment (Vercel)

Vercel’s default build image does not include the .NET SDK. This repo’s `vercel.json` installs .NET 10 during the hosting build, publishes the WASM app, and serves `publish/wwwroot` with SPA rewrites so direct URLs and refresh work.

1. Push the repository to GitHub.
2. In Vercel, import the repo.
3. Confirm:
   - **Install command** is `true` (skips npm; the .NET SDK is installed by the build script)
   - **Build command** is `bash scripts/vercel-build.sh`
   - **Output directory** is `publish/wwwroot`
4. Deploy. After the live URL exists, open `/`, `/transactions`, `/budgets`, `/analytics`, and `/settings`, then refresh each route.

Alternative: publish locally and upload only `publish/wwwroot`, still using the rewrite `{ "source": "/(.*)", "destination": "/index.html" }`.

GitHub Actions in `.github/workflows/ci.yml` builds, tests, and publishes on push.

## Known limitations

- Demo only; not a banking or accounting product
- No multi-device sync, no accounts, no exchange rates
- Storage quota or private-mode blocks persist in-session only
- Backup import supports FINORA JSON version 1
- Hosted fonts load from Google Fonts (the UI still works if that request is blocked)
- A live production URL is not claimed until it has been opened and checked in a browser

## License

Use and adapt this project as a portfolio piece unless you add a different license file.
