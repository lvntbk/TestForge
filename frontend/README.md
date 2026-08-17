# TestForge Frontend

React and TypeScript dashboard for TestForge.

## Features

- Submit a public GitHub repository for testing
- Track pipeline status with automatic polling
- Display passed, failed and skipped test counts
- View build and test durations
- Inspect build and test logs
- Responsive dashboard layout

## Local development

The frontend proxies `/api` requests to the TestForge API running at `http://127.0.0.1:5080`.

```bash
npm install
npm run dev
```

## Verification

```bash
npm run lint
npm run build
```
