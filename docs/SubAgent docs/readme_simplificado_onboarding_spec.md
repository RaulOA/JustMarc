# Spec: README simplificado para onboarding (principiantes)

Fecha: 2026-05-03
Objetivo: reemplazar el README actual por una guia corta, intuitiva y completa para abrir, ejecutar y usar herramientas del proyecto (VS Code Tasks/Debug, Swagger y REST Client).

## 1) Findings (estado actual)

- El README actual es completo pero demasiado largo para primer contacto (muchas secciones avanzadas y repetidas).
- El workspace ya trae automatizacion lista en VS Code:
  - Tasks en `.vscode/tasks.json`.
  - Debug profiles en `.vscode/launch.json`.
  - Variables REST Client en `.vscode/settings.json`.
- La API ya expone salud y Swagger:
  - `GET /health`
  - `/swagger` cuando `Swagger:Enabled=true` (default true).
- Existe archivo de pruebas HTTP listo para uso:
  - `backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.http`.
- El frontend es estatico (sin npm ni bundler) y se sirve por tarea `serve-frontend` en puerto 8000.

## 2) Estructura final propuesta del README

Mantener solo estas 9 secciones, en este orden:

1. Titulo + que hace la app (3-5 lineas)
2. Requisitos minimos
3. Inicio rapido (VS Code en 3 pasos)
4. Uso de Tasks (sin terminal manual)
5. Debug rapido (API y full stack)
6. Probar API con Swagger
7. Probar API con REST Client (.http)
8. Mapa rapido de arquitectura
9. Troubleshooting (Top 5)

## 3) Que eliminar del README actual

Eliminar o mover fuera del README principal:

- Explicacion extensa de scripts SQL y notas legacy (dejar referencia breve a `docs/db/`).
- Detalle largo de pruebas y comandos avanzados de filtrado/cobertura.
- Secciones duplicadas de comandos manuales (restore/build/run repetidos en varias partes).
- Listados extensos de atajos y descripciones largas por perfil.
- Narrativa larga de comportamiento de sesion por inactividad (mover a doc funcional dedicado).

Regla de simplificacion:
- Si no ayuda a "abrir, correr o probar" en los primeros 10 minutos, no va en README principal.

## 4) Comandos minimos exactos

Incluir solo este bloque (copiar/pegar):

```powershell
# 1) Restaurar dependencias backend
dotnet restore backend/

# 2) Compilar API
dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug

# 3) Ejecutar API
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --no-build

# 4) (Opcional) Servir frontend estatico en :8000
python -m http.server 8000 --directory .
```

Nota de UX recomendada para README:
- "Si usas VS Code, preferi Tasks y Debug; estos comandos son fallback."

## 5) Checklist primer arranque (first-run)

Checklist breve para nuevos devs:

- Tener .NET SDK 8 (`dotnet --version`).
- Abrir carpeta del workspace en VS Code.
- Ejecutar Task `build-api`.
- Ejecutar Task `run-api`.
- Verificar `http://localhost:5093/health`.
- Abrir `http://localhost:5093/swagger`.
- (Opcional UI) Ejecutar Task `serve-frontend` y abrir `http://localhost:8000/index.html`.
- (Opcional API tests) abrir `.http` y ejecutar request de health.

## 6) Mapa rapido de arquitectura (1 pantalla)

Usar formato compacto:

- Frontend (estatico): `index.html`, `dashboard.html`, `app.js`, `style.css`.
- API: `backend/src/IntegradorMarcas.Api` (controllers + config + Swagger).
- Application: `backend/src/IntegradorMarcas.Application` (casos de uso/servicios).
- Domain: `backend/src/IntegradorMarcas.Domain` (entidades/constantes).
- Infrastructure: `backend/src/IntegradorMarcas.Infrastructure` (repositorios/SQL).
- Tests: `backend/tests/IntegradorMarcas.Tests`.
- SQL docs/scripts: `docs/db/`.

## 7) Flujo de uso: Swagger (beginner)

Flujo recomendado (5 pasos):

1. Levantar API (`run-api` task o comando).
2. Abrir `http://localhost:5093/swagger`.
3. Ejecutar `GET /health` (sin headers).
4. Probar endpoint de negocio con headers:
   - `X-User-Id: 6`
   - `X-User-Role: ROL_RRHH`
5. Si responde 401, validar ambos headers.

## 8) Flujo de uso: REST Client (.http)

Flujo recomendado (6 pasos):

1. Instalar extension `humao.rest-client` (si falta).
2. Levantar API.
3. Abrir `backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.http`.
4. Confirmar entorno `local` en REST Client.
5. Ejecutar `GET /health`.
6. Ejecutar request con identidad (`/api/rrhh/justificaciones`) usando:
   - `{{userId}}`
   - `{{userRole}}`

Variables base que ya existen en `.vscode/settings.json`:
- `apiBaseUrl: http://localhost:5093`
- `userId: 6`
- `userRole: ROL_RRHH`

## 9) Troubleshooting Top 5 (solo sintomas + accion)

1. API no levanta
- Accion: correr `restore` y `build-api`; revisar SDK 8.

2. 401 en endpoints
- Accion: enviar `X-User-Id` y `X-User-Role` validos.

3. Swagger no abre
- Accion: validar API en `http://localhost:5093/health`; luego `/swagger`.

4. REST Client no resuelve variables
- Accion: seleccionar entorno `local` y verificar `.vscode/settings.json`.

5. Frontend no conecta a API
- Accion: confirmar API en :5093 y frontend en :8000; revisar URL base en `app.js`.

## 10) Criterios de aceptacion para el nuevo README

- Lectura total <= 3-5 minutos.
- Onboarding ejecutable sin conocimiento previo de .NET.
- Incluye una sola ruta recomendada: VS Code Tasks + Debug.
- Incluye fallback por terminal (minimo).
- Incluye flujos concretos de Swagger y REST Client.
- Evita detalle profundo de SQL/tests avanzados en documento principal.
