# FINORA — portfolio description

FINORA is a personal-finance dashboard built in **C# on .NET 10** as a standalone **Blazor WebAssembly** app. It runs entirely in the browser: there is no API, database, or authentication layer.

The UI is a responsive Razor workspace (Overview, Transactions, Budgets, Analytics, Settings) with a shared shell, light and dark themes, and reusable components for charts, dialogs, editors, and month navigation. Application state lives in injected services that persist a versioned JSON model to `localStorage`, so ledger edits, budget envelopes, and preferences stay consistent across pages and survive a refresh.

Analytics and budget health are calculated in testable C# (completed vs scheduled dates, savings rate, month-over-month change, category mix, at-limit vs over-budget). SVG/CSS visualizations render those results without paid chart libraries. xUnit covers the financial math, seed consistency, display formatting, and backup validation.

FINORA is a portfolio demonstration of client-side .NET, careful UX, and static deployment — not a live bank product.
