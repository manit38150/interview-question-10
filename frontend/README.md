# MiniExam frontend — example.com

Angular UI for the IT 10-1 exam and IT 10-2 result screens in `docs/No10.docx`.
See [the project README](../README.md) for requirements, API and database details.

Run the backend first, then run these commands from `frontend`:

```powershell
npm ci
npm start
```

Open `http://localhost:4200`. `proxy.conf.json` forwards `/api/**` to port 5242.
The form loads questions from SQLite through the API, accepts one option per
question, shows the saved score and selections, and clears inputs for another
attempt. Failed requests show a retry message while preserving entered answers.

```powershell
npm run build
npm run test:browser
```

The browser check requires a built backend and Edge (or a Chromium executable
specified by `MINIEXAM_BROWSER`). It starts its own isolated servers and temporary
database, then checks the complete flow. It creates no `.spec.ts` files.

Production files are emitted to `dist/frontend/browser`. Configure the host to
route `/api` to the ASP.NET Core API; the development proxy is not bundled.
