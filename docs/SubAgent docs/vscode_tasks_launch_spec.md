# VS Code Tasks & Launch Configuration Spec

**Date:** 3 de mayo de 2026  
**Status:** Ready for Implementation  
**Target:** Practical GUI-based debugging/running without terminal commands

---

## 1. Current State Analysis

### Existing .vscode Configuration
- **Files Present:** Only `settings.json` exists (minimal configuration)
- **Missing:** `tasks.json`, `launch.json`, `extensions.json`
- **Current settings.json:** Only chat configuration, no build/debug tooling

### Backend Architecture
- **Framework:** ASP.NET Core 8.0 (Minimal APIs)
- **Structure:** 
  - `IntegradorMarcas.Api` (entry point, controllers, Swagger)
  - `IntegradorMarcas.Application` (services, DTOs, validation)
  - `IntegradorMarcas.Domain` (entities, constants)
  - `IntegradorMarcas.Infrastructure` (data access, repositories)
  - `IntegradorMarcas.Tests` (XUnit tests)
- **Default Port:** 5093 (HTTP), 7129 (HTTPS)
- **API Profile:** Swagger enabled in Development
- **Environment:** Development requires SQL Server LocalDB (SQLEXPRESS) with trusted connection

### Frontend Architecture
- **Type:** Multi-page HTML/JavaScript (no build step)
- **Entry Points:** `index.html`, `dashboard.html`
- **Assets:** `app.js`, `style.css`
- **Pattern:** Vanilla JS with toast notification system, no bundler required

### Test Configuration
- **Framework:** XUnit 2.5.3
- **SDK:** `Microsoft.NET.Test.Sdk` 17.8.0
- **Coverage:** coverlet.collector 6.0.0 available

---

## 2. Recommended tasks.json Structure

### File Location
`/.vscode/tasks.json`

### Task Groups

#### Task 1: Restore
```json
{
  "label": "restore",
  "type": "shell",
  "command": "dotnet",
  "args": ["restore", "backend/"],
  "group": "build",
  "problemMatcher": "$msCompile",
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

#### Task 2: Build Backend
```json
{
  "label": "build-api",
  "type": "shell",
  "command": "dotnet",
  "args": ["build", "backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj", "--configuration", "Debug"],
  "group": {
    "kind": "build",
    "isDefault": true
  },
  "problemMatcher": ["$msCompile"],
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

#### Task 3: Run API
```json
{
  "label": "run-api",
  "type": "shell",
  "command": "dotnet",
  "args": ["run", "--project", "backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj", "--no-build"],
  "isBackground": true,
  "problemMatcher": {
    "pattern": {
      "regexp": "^.*$",
      "file": 1,
      "location": 2,
      "message": 3
    },
    "background": {
      "activeOnStart": true,
      "beginsPattern": "^\\s*Now listening on:",
      "endsPattern": "^\\s*Application started. Press Ctrl\\+C to shut down\\.|^\\s*Shutting down"
    }
  },
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

#### Task 4: Run Tests
```json
{
  "label": "test",
  "type": "shell",
  "command": "dotnet",
  "args": ["test", "backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj", "--verbosity=normal", "--logger=console;verbosity=detailed"],
  "group": {
    "kind": "test",
    "isDefault": true
  },
  "problemMatcher": "$msCompile",
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

#### Task 5: Run Tests with Coverage
```json
{
  "label": "test-coverage",
  "type": "shell",
  "command": "dotnet",
  "args": ["test", "backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj", "/p:CollectCoverage=true", "/p:CoverageFormat=opencover"],
  "group": "test",
  "problemMatcher": "$msCompile",
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

#### Task 6: Serve Frontend
```json
{
  "label": "serve-frontend",
  "type": "shell",
  "command": "python",
  "args": ["-m", "http.server", "8000", "--directory", "."],
  "isBackground": true,
  "problemMatcher": {
    "pattern": {
      "regexp": "^.*$"
    },
    "background": {
      "activeOnStart": true,
      "beginsPattern": "^Serving HTTP",
      "endsPattern": "^.*Keyboard interrupt.*"
    }
  },
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

#### Task 7: Clean Build
```json
{
  "label": "clean",
  "type": "shell",
  "command": "dotnet",
  "args": ["clean", "backend/", "--configuration", "Debug"],
  "group": "build",
  "problemMatcher": "$msCompile",
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

#### Task 8: Build & Run API
```json
{
  "label": "build-and-run-api",
  "dependsOn": ["build-api", "run-api"],
  "group": "build",
  "presentation": {
    "reveal": "always",
    "panel": "shared"
  }
}
```

---

## 3. Recommended launch.json Configuration

### File Location
`/.vscode/launch.json`

### Configuration Profiles

#### Profile 1: Debug API (.NET)
```json
{
  "name": "Debug API (.NET)",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "build-api",
  "program": "${workspaceFolder}/backend/src/IntegradorMarcas.Api/bin/Debug/net8.0/IntegradorMarcas.Api.dll",
  "args": [],
  "cwd": "${workspaceFolder}/backend/src/IntegradorMarcas.Api",
  "stopAtEntry": false,
  "serverReadyAction": {
    "action": "openExternally",
    "pattern": "Now listening on:\\s+(https?://\\S+)",
    "uriFormat": "$1/swagger"
  },
  "env": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  },
  "console": "integratedTerminal",
  "internalConsoleOptions": "neverOpen"
}
```

#### Profile 2: Attach to Process
```json
{
  "name": "Attach to Process",
  "type": "coreclr",
  "request": "attach",
  "processId": "${command:pickProcess}",
  "console": "integratedTerminal"
}
```

#### Profile 3: Unit Tests (Debug)
```json
{
  "name": "Debug Tests",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "build-api",
  "program": "${workspaceFolder}/backend/tests/IntegradorMarcas.Tests/bin/Debug/net8.0/IntegradorMarcas.Tests.dll",
  "args": [],
  "cwd": "${workspaceFolder}/backend/tests/IntegradorMarcas.Tests",
  "stopAtEntry": false,
  "console": "integratedTerminal",
  "internalConsoleOptions": "neverOpen"
}
```

#### Profile 4: Frontend + API Stack
```json
{
  "name": "Frontend + API",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "build-and-run-api",
  "serverReadyAction": {
    "action": "openExternally",
    "pattern": "Now listening on:\\s+(https?://\\S+)",
    "uriFormat": "http://localhost:8000"
  },
  "console": "integratedTerminal",
  "internalConsoleOptions": "neverOpen"
}
```

---

## 4. Recommended Extensions

### Category: C# & .NET Development
| Extension ID | Purpose |
|---|---|
| `ms-dotnettools.csharp` | Official C# support, IntelliSense, debugging |
| `ms-dotnettools.vscode-dotnet-runtime` | Managed .NET runtime for scripts |

### Category: API & REST Testing
| Extension ID | Purpose |
|---|---|
| `humao.rest-client` | Send HTTP requests directly from editor (`.http` files) |
| `Swagger.swaggerviewer` | Swagger UI integration for API testing |

### Category: Database & SQL
| Extension ID | Purpose |
|---|---|
| `ms-mssql.mssql` | SQL Server connection and query tools |
| `mtxr.sqltools` | Universal SQL tools (optional companion) |

### Category: Debugging & Diagnostics
| Extension ID | Purpose |
|---|---|
| `ms-vscode.vscode-dotnet-pack` | Optimized .NET workflow pack |

### Category: Frontend & Editor Quality
| Extension ID | Purpose |
|---|---|
| `esbenp.prettier-vscode` | Code formatter (HTML, CSS, JS) |
| `dbaeumer.vscode-eslint` | JavaScript linting (if needed for app.js) |

### Installation Command
```shell
code --install-extension ms-dotnettools.csharp
code --install-extension humao.rest-client
code --install-extension ms-mssql.mssql
code --install-extension esbenp.prettier-vscode
```

---

## 5. Integration Points & Keyboard Shortcuts

### Suggested Keyboard Shortcuts (`keybindings.json` additions)
```json
[
  {
    "key": "ctrl+shift+b",
    "command": "workbench.action.tasks.runTask",
    "args": "build-api"
  },
  {
    "key": "ctrl+shift+d",
    "command": "workbench.action.debug.start"
  },
  {
    "key": "f5",
    "command": "workbench.action.debug.start"
  },
  {
    "key": "shift+f5",
    "command": "workbench.action.tasks.runTask",
    "args": "run-api"
  },
  {
    "key": "ctrl+shift+t",
    "command": "workbench.action.tasks.runTask",
    "args": "test"
  },
  {
    "key": "ctrl+k ctrl+c",
    "command": "workbench.action.tasks.runTask",
    "args": "clean"
  }
]
```

### Default Keybindings in VS Code
- **F5:** Start/continue debugging (works with launch.json profiles)
- **Shift+F5:** Stop debugging
- **Ctrl+Shift+D:** Open Debug view
- **Ctrl+Shift+`:** New terminal
- **Ctrl+Shift+B:** Run build task

### Custom Workflow Integration
1. **Quick Build & Debug:** `F5` → Runs "Debug API (.NET)" profile → builds → launches → opens Swagger
2. **Run Tests:** `Ctrl+Shift+T` → Runs XUnit tests with console output
3. **Attach to Running Process:** Open Debug view → "Attach to Process" → select dotnet process
4. **API Testing:** Open `.http` file → Use REST Client extension to test endpoints

---

## 6. Folder Structure & File Organization

### Expected Directory Tree (Post-Implementation)
```
.vscode/
├── tasks.json           [NEW] Build/test automation
├── launch.json          [NEW] Debugging configurations
├── extensions.json      [NEW] Recommended extensions list
├── settings.json        [EXISTING] Editor settings
└── keybindings.json     [OPTIONAL] Custom shortcuts

backend/
├── IntegradorMarcas.slnx
└── src/
    ├── IntegradorMarcas.Api/
    │   ├── Program.cs
    │   ├── IntegradorMarcas.Api.http  [Use with REST Client]
    │   └── Properties/launchSettings.json
    └── ...

docs/
└── SubAgent docs/
    └── vscode_tasks_launch_spec.md   [THIS FILE]
```

---

## 7. Implementation Prerequisites

### System Requirements
- **.NET SDK 8.0+** installed and in PATH
- **SQL Server Express LocalDB** with database `INTEGRA_CNP` created
- **Python 3.7+** (for frontend HTTP server fallback; optional)
- **VS Code Extensions** installed (see Section 4)

### Configuration Checklist
- [ ] SQL Server connection string verified in `appsettings.Development.json`
- [ ] Port 5093 (API) not in use
- [ ] Port 8000 (frontend fallback) not in use
- [ ] VS Code C# extension installed and activated
- [ ] Workspace .NET version resolved (should auto-detect net8.0)

---

## 8. Integration Workflow Examples

### Example 1: Debug Full Stack
1. Open Debug view (`Ctrl+Shift+D`)
2. Select "Frontend + API" configuration
3. Press `F5`
4. API builds and launches → Swagger opens in browser
5. Frontend loads at `http://localhost:8000`
6. Set breakpoints in `Program.cs` or controllers → triggers on requests

### Example 2: Unit Test Debugging
1. Open test file: `backend/tests/IntegradorMarcas.Tests/UnitTest1.cs`
2. Set breakpoints in test methods
3. Open Debug view → Select "Debug Tests"
4. Press `F5`
5. Tests run with debugger attached

### Example 3: REST API Testing
1. Open `IntegradorMarcas.Api.http` file
2. REST Client extension recognizes HTTP request blocks
3. Click "Send Request" button above any request
4. Response displays in side panel

---

## 9. Notes & Considerations

- **Swagger UI:** Auto-opens at `http://localhost:5093/swagger` when debugging API
- **Frontend:** Currently no build step; serves static files directly. Add webpack/vite if components become complex
- **Environment:** Default is Development; Production requires SQL connection string validation (see Program.cs)
- **Database:** Ensure `INTEGRA_CNP` database exists; migrations not yet visible in workspace structure
- **Error Handling:** API has global exception handler with correlation IDs for traceability
- **CORS:** Configured for local development with origin `*` (restrict in production)

