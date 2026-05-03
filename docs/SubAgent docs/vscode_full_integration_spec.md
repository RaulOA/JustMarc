# VS Code Full Integration Spec (One-Click Startup)

**Date:** 3 de mayo de 2026  
**Status:** Ready for implementation  
**Goal:** One-click startup for backend API + frontend server from Tasks and Run and Debug.

## 1) Findings Summary (Current State)

### .vscode/tasks.json
- Existing tasks: `restore`, `build-api`, `run-api`, `test`, `test-coverage`, `serve-frontend`, `clean`, `build-and-run-api`.
- `build-and-run-api` only starts backend (`build-api` + `run-api`), not frontend.
- `run-api` is already configured as background with a background problem matcher.
- `serve-frontend` is already configured as background with a background problem matcher.

### .vscode/launch.json
- Existing debug profiles: `Debug API (.NET)`, `Attach to Process`, `Debug Tests`, `Frontend + API`.
- There is no `compounds` section.
- Current `Frontend + API` profile does not start frontend server task; it only runs backend prelaunch and opens browser URL.

### README usage
- VS Code section exists and documents tasks/profiles.
- It does not document a single one-click full stack task.
- It does not document a compound profile for Run and Debug full stack startup.

## 2) Required Changes

## 2.1 Update .vscode/tasks.json

### A) Add one new task: `start-full-stack`
Add this task to `tasks` array:

```json
{
  "label": "start-full-stack",
  "dependsOn": ["build-api", "run-api", "serve-frontend"],
  "dependsOrder": "sequence",
  "group": "build",
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

Rationale:
- Ensures backend build runs first.
- Then starts API in background and waits for readiness.
- Then starts frontend server in background.
- Provides one-click startup from `Terminal > Run Task`.

### B) Tighten background matcher for run-api (adjust existing task)
Replace only `problemMatcher.background` in `run-api` with:

```json
"background": {
  "activeOnStart": true,
  "beginsPattern": "^\\s*Now listening on:\\s+https?://\\S+",
  "endsPattern": "^\\s*Application started\\. Press Ctrl\\+C to shut down\\."
}
```

### C) Tighten background matcher for serve-frontend (adjust existing task)
Replace only `problemMatcher.background` in `serve-frontend` with:

```json
"background": {
  "activeOnStart": true,
  "beginsPattern": "^(Serving HTTP on|HTTP server serving|Serving on)",
  "endsPattern": "^(Keyboard interrupt received, exiting\\.|.*Keyboard interrupt.*)$"
}
```

## 2.2 Update .vscode/launch.json

### A) Add browser debug profile for frontend (new configuration)
Add this configuration in `configurations`:

```json
{
  "name": "Frontend Browser",
  "type": "pwa-chrome",
  "request": "launch",
  "preLaunchTask": "serve-frontend",
  "url": "http://localhost:8000/index.html",
  "webRoot": "${workspaceFolder}",
  "presentation": {
    "group": "full-stack",
    "order": 2
  }
}
```

### B) Add compound profile to launch everything from Run and Debug
Add top-level `compounds` section (new if missing):

```json
"compounds": [
  {
    "name": "Full Stack Debug (API + Frontend)",
    "configurations": ["Debug API (.NET)", "Frontend Browser"],
    "stopAll": true
  }
]
```

Notes:
- `Debug API (.NET)` continues debugging backend process directly (best for breakpoints).
- `Frontend Browser` ensures frontend server is up and opens browser debug session.
- This gives one-click from `Run and Debug` dropdown using the compound.

### C) Deprecate old `Frontend + API` profile (optional but recommended)
- Option 1: remove it to avoid confusion.
- Option 2: rename to `Legacy Frontend + API` and keep temporary compatibility.

Recommended final state: remove old `Frontend + API` once compound is available.

## 2.3 Update README.md (concise usage)

In section "Ejecutar desde VS Code":

### A) Add task description line
Under task list, add:
- `start-full-stack`: build + API + frontend en un solo click.

### B) Add compound debug line
Under debug profiles, add:
- `Full Stack Debug (API + Frontend)`: inicia depuracion API y frontend desde Run and Debug.

### C) Add quick one-click steps (4 lines max)
Add a short subsection:

```md
### Inicio en un click
1. Terminal > Run Task > start-full-stack
2. Run and Debug > Full Stack Debug (API + Frontend) > F5
```

Keep this concise and avoid duplicating full manual command sections.

## 3) Acceptance Criteria

1. Running `start-full-stack` starts backend API and frontend server without extra manual commands.
2. Both long-running tasks remain non-blocking and marked as background correctly.
3. `Run and Debug` shows compound `Full Stack Debug (API + Frontend)` and starts both backend debug + frontend browser session.
4. README documents the one-click task and compound profile clearly in the VS Code section.

## 4) Validation Steps

1. `Terminal > Run Task > start-full-stack`.
2. Confirm API is reachable (`/health`) and frontend is reachable at `http://localhost:8000/index.html`.
3. `Run and Debug > Full Stack Debug (API + Frontend) > F5`.
4. Hit breakpoint in backend controller and confirm browser opens frontend URL.
5. Stop debugging and verify both sessions stop (`stopAll: true`).

## 5) Risks / Notes

- `pwa-chrome` requires JS debugger support in VS Code (normally built-in).
- If Python output differs by environment, matcher regex may need minor tuning.
- If port 8000 is occupied, frontend task fails; document fallback in troubleshooting if needed.
