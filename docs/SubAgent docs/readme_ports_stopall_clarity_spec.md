# README.md: Puertos, Startup y Apagado - Especificación de Mejora

**Fecha:** 13 de mayo de 2026  
**Objetivo:** Detectar ambigüedades en puertos, pasos incompletos y ausencia de instrucciones de apagado.

---

## 1. Problemas Detectados

### 1.1 Ambigüedades de Puertos

- **API (5093):** Mencionado en múltiples secciones (Checklist, Swagger, REST Client) pero sin tabla centralizada
- **Frontend (8000):** Solo en checklist y fallback, no documentado en un lugar único
- **IIS local (8080):** Solo mencionado en "Validacion local de IIS" sin relación clara con desarrollo local
- **No hay mapping:** Los puertos no están organizados por contexto (desarrollo, IIS local, producción)

### 1.2 Pasos Incompletos en Startup

- **Checklist de primer arranque:** Dice ejecutar `build-api` y `run-api` pero no especifica cómo verificar estado de startup
- **"Inicio en un click":** Recomienda `start-full-stack` pero sin detalles de qué hace realmente
- **Fallback por terminal:** El paso 3 (`dotnet run --project...`) no explica cómo saber que está listo (ej: esperar línea de "ready" en console)
- **Orden de levantamiento:** No queda claro si frontend debe levantarse antes/después de API

### 1.3 Ausencia Total de Instrucciones de Apagado

- **No existe sección "Detener todo":** El README no menciona cómo parar la aplicación
- **Servidor Frontend:** No hay instrucción para detener `serve-frontend` 
- **API en ejecución:** No hay instrucción para parar `dotnet run` o el `run-api` task
- **Liberación de puertos:** No se documenta cómo liberar 5093 y 8000
- **Tarea no documentada:** `stop-api-on-5093` existe en tasks.json pero no está mencionada en README
- **Escenario de error:** Si la aplicación no arranca, no hay guide para "limpiar" puertos

### 1.4 Flujo Recomendado Poco Claro

- **Dos caminos:** "start-full-stack" vs. fallback con comandos manuales, sin indicar cuál es preferido
- **Debug mode:** Mencionado brevemente pero sin pasos claros
- **Falta transición:** No hay indicación de cuándo cambiar de un flujo a otro (ej: si tasks no funcionan)

---

## 2. Cambios Puntuales Propuestos

### 2.1 Agregar Tabla de Puertos (Nueva Subsección)

**Ubicación:** Después de "Que es esta app", antes de "Mapa rapido de arquitectura"

**Acción:** Insertar tabla centralizada con puertos por contexto.

### 2.2 Mejorar "Checklist de primer arranque"

**Ubicación:** Misma sección, mejorar pasos 3 y 6

**Cambios:**
- Paso 3: Añadir indicador de cuándo está listo (ej: "esperando mensaje 'ready'" en console)
- Paso 6: Aclarar que es alternativa a Swagger (no obligatorio)

### 2.3 Agregar Sección "Detener Todo"

**Ubicación:** Después de "Inicio en un click desde VS Code"

**Acción:** Nueva subsección con 3 métodos (Task, Terminal, Puertos)

### 2.4 Aclarar Orden de Levantamiento en Fallback

**Ubicación:** "Fallback por terminal"

**Cambios:** Añadir nota sobre paralelismo o secuencia recomendada

---

## 3. Texto Sugerido para Mejoras

### 3.1 Tabla de Puertos Oficiales

**Ubicación:** Nueva subsección "Puertos en Uso" (después de "Que es esta app")

```markdown
## Puertos en Uso

| Componente | Puerto | Contexto | Nota |
|---|---|---|---|
| API (.NET) | 5093 | Desarrollo local | Default, configurable en launchSettings.json |
| Frontend (Python HTTP) | 8000 | Desarrollo local | Servido por `serve-frontend` task |
| IIS Local | 8080 | Validación local pre-producción | Para probar en entorno IIS sin publicar a servidor |
| SQL Server | 1433 | Producción/Infraestructura | Puerto estándar, puede variar por infraestructura |

**Notas:**
- En desarrollo local, API + Frontend corren en `localhost:5093` y `localhost:8000`.
- Los puertos 5093 y 8000 pueden cambiar si hay conflictos; revisar `.vscode/launch.json` y `tasks.json`.
```

### 3.2 Flujo Recomendado Único para Levantar

**Ubicación:** Remodelar "Inicio en un click desde VS Code" + "Fallback por terminal"

```markdown
## Flujo Recomendado: Levantar Aplicación Local

### Opción A: Usar Tasks (Recomendado)

1. En VS Code, abrir Command Palette: `Ctrl+Shift+P`
2. Ejecutar: **Terminal > Run Task > start-full-stack**
3. Esperar a ver en consola: `Application started. Press Ctrl+C to shut down.`
4. API estará en: **http://localhost:5093/health**
5. Frontend estará en: **http://localhost:8000/index.html**

Esta opción:
- Ejecuta `restore`, `build-api`, `run-api` y `serve-frontend` en secuencia.
- Es la forma recomendada. No requiere escribir comandos.

### Opción B: Fallback por Terminal Manual

Si las Tasks no funcionan:

```powershell
# Terminal 1: Restaurar y compilar
dotnet restore backend/
dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug

# Terminal 2: Ejecutar API
# Espera a ver: "Application started. Press Ctrl+C to shut down."
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --no-build

# Terminal 3: Servir Frontend (opcional, solo si deseas probar UI)
python -m http.server 8000 --directory .
```

Nota: Usa 3 terminales separadas para que cada componente se mantenga en ejecución.

### Opción C: Debug Completo (IDE)

1. Abrir Command Palette: `Ctrl+Shift+P`
2. Ejecutar: **Debug: Start Debugging** (o presionar `F5`)
3. Seleccionar: **Full Stack Debug (API + Frontend)**
4. VS Code pausará en breakpoints del código API.
```

### 3.3 Sección "Detener Todo"

**Ubicación:** Nueva sección después del flujo de levantar

```markdown
## Detener Todo

### Opción A: Desde VS Code Tasks

1. En la terminal donde corre `start-full-stack`:
   - Presionar **Ctrl+C** una o más veces hasta que todas las tareas se detengan.
2. Verificar que ambas terminales cierren (API y Frontend).

### Opción B: Detener Componentes Individuales

Si levantaste manualmente con 3 terminales:

| Componente | Cómo Detener |
|---|---|
| **API (.NET)** | En la terminal del dotnet run, presionar **Ctrl+C** |
| **Frontend (HTTP Server)** | En la terminal del python http.server, presionar **Ctrl+C** |

### Opción C: Liberar Puertos Ocupados (Si quedan procesos)

Si la API o Frontend no detienen limpiamente y los puertos quedan ocupados:

```powershell
# En PowerShell (Admin):

# Liberar puerto 5093 (API)
$conn = Get-NetTCPConnection -LocalPort 5093 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if ($conn) { 
    Stop-Process -Id $conn.OwningProcess -Force
    Write-Host "Puerto 5093 liberado."
} else { 
    Write-Host "Puerto 5093 ya está libre."
}

# Liberar puerto 8000 (Frontend)
$conn = Get-NetTCPConnection -LocalPort 8000 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1
if ($conn) { 
    Stop-Process -Id $conn.OwningProcess -Force
    Write-Host "Puerto 8000 liberado."
} else { 
    Write-Host "Puerto 8000 ya está libre."
}
```

**O usar la Task incluida:**

1. En VS Code, Command Palette: `Ctrl+Shift+P`
2. Ejecutar: **Terminal > Run Task > stop-api-on-5093**

Nota: Esta task solo libera 5093. Para un apagado limpio, prefiere Opción A (Ctrl+C).

### Verificación Post-Apagado

```powershell
# Confirmar que los puertos estén libres:
Get-NetTCPConnection -LocalPort 5093 -State Listen -ErrorAction SilentlyContinue
Get-NetTCPConnection -LocalPort 8000 -State Listen -ErrorAction SilentlyContinue

# Si no devuelven nada, los puertos están libres.
```
```

---

## 4. Resumen de Cambios

| Sección | Cambio | Razón |
|---|---|---|
| Nueva: "Puertos en Uso" | Tabla centralizada | Elimina ambigüedad sobre 5093, 8000, 8080 |
| Mejorar: "Checklist" | Aclarar qué significa "listo" | Evita confusión sobre cuándo la app está up |
| Remodelar: "Inicio en un click..." | 3 opciones claras (A/B/C) | Unifica flujos y da alternativas ordenadas |
| Nueva: "Detener Todo" | 3 opciones claras | Cierra vacío total sobre apagado |
| Mejorar: "Fallback por terminal" | Notas sobre paralelismo | Claridad en orden de ejecución |

---

## 5. Impacto

- **Usuarios nuevos:** Flujo claro, tabla de puertos, instrucciones de parada
- **Debugging:** Sección de liberación de puertos para recuperarse de fallos
- **Documentación interna:** Reducción de tickets sobre "no sé cómo apagar" o "puerto 5093 ocupado"

---

## 6. Notas de Implementación

- Los cambios son **puntuales**, no requieren reescritura de secciones completas.
- La tabla de puertos se agrega como sección nueva corta.
- El flujo de startup unifica contenido existente bajo estructura 3-opciones.
- La sección de apagado es completamente nueva pero usa tareas/comandos ya existentes.
- Se mantiene la estructura y tono del README original.

