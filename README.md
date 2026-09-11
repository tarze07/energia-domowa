# WattDom — zarządzanie zużyciem energii w domu

Aplikacja webowa: frontend TypeScript (React + Vite) i backend ASP.NET Core 8.

## Funkcje

- pulpit: zużycie dziś / w miesiącu, koszt, budżet, alerty, wykres 30 dni
- urządzenia i pomieszczenia z szacunkiem kWh i kosztu
- dzienne zużycie oraz odczyty licznika
- taryfa (zł/kWh), limit dzienny i budżet miesięczny
- dane demo (SQLite) przy pierwszym uruchomieniu

## Uruchomienie

Terminal 1 — API (`http://localhost:5235`):

```bash
cd backend
dotnet run
```

Terminal 2 — UI (`http://localhost:5173`):

```bash
cd frontend
npm install
npm run dev
```

Frontend proxy’uje `/api` do backendu.
