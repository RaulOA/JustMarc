# Pruebas: Estrategia y Cobertura

## 1. Proposito
Documentar el estado actual de pruebas automatizadas y definir una estrategia incremental de cobertura para backend y frontend.

## 2. Alcance
- Proyecto backend/tests/IntegradorMarcas.Tests.
- Cobertura actual observada en el repositorio.
- Backlog inicial de casos prioritarios.

## 3. Fuente de verdad
- backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj
- backend/tests/IntegradorMarcas.Tests/UnitTest1.cs
- backend/src/IntegradorMarcas.Application/**
- backend/src/IntegradorMarcas.Api/**
- app.js

## 4. Estado actual
- Framework de pruebas: xUnit.
- Paquetes de testing configurados:
  - Microsoft.NET.Test.Sdk
  - xunit
  - xunit.runner.visualstudio
  - coverlet.collector
- Cobertura implementada hoy: minima (solo UnitTest1 placeholder sin aserciones).

## 5. Objetivo de cobertura (faseada)

### 5.1 Meta inicial
- Tener pruebas unitarias reales para validadores y servicios criticos.
- Probar reglas de autorizacion y validacion de negocio por rol.

### 5.2 Meta intermedia
- Agregar pruebas de integracion de repositorio contra base local de pruebas.
- Agregar pruebas API smoke para rutas criticas.

### 5.3 Meta objetivo
- Piramide balanceada:
  - Unitarias: 70%
  - Integracion: 20%
  - End-to-end/smoke: 10%

## 6. Backlog prioritario de casos

### 6.1 JustificacionValidator
1. ValidateCreate falla si MotivoGeneral vacio.
2. ValidateCreate falla si MotivoGeneral > 500.
3. ValidateCreate falla sin detalles (RN-01).
4. ValidateAccion acepta APROBAR/RECHAZAR y rechaza otros.
5. NormalizeComentarioResolucion retorna null para string vacio.
6. ValidateRangoFechas falla cuando desde > hasta.
7. ValidateCompania acepta CNP/FANAL y rechaza otros valores.
8. ValidateTextoBusqueda falla >150 caracteres.

### 6.2 JustificacionService
9. CreateAsync rechaza rol no funcionario.
10. ListPendientesJefaturaAsync rechaza rol no jefatura.
11. ListRrhhAsync rechaza rol no RRHH.
12. ResolverAsync devuelve 409 cuando estado no pendiente.
13. ResolverAsync devuelve 409 cuando affected=0.

### 6.3 API smoke (minimo)
14. /health responde 200.
15. /api/justificaciones sin headers retorna 401.
16. /api/rrhh/justificaciones con rol no RRHH retorna 403.

## 7. Estrategia de implementacion
- Priorizar pruebas unitarias sin dependencias de BD (Validator + Service con mocks).
- Introducir dobles de prueba para IJustificacionRepository.
- Agregar categorias o nomenclatura por tipo de prueba en nombres de clases.
- Integrar ejecucion de pruebas en pipeline CI antes de merge.

## 8. Comandos recomendados

Ejecutar todas las pruebas:
```powershell
dotnet test backend/IntegradorMarcas.slnx
```

Cobertura (si se habilita coleccion por proyecto):
```powershell
dotnet test backend/tests/IntegradorMarcas.Tests --collect:"XPlat Code Coverage"
```

## 9. Criterios de aceptacion
- Cada regla critica de negocio tiene al menos una prueba positiva y una negativa.
- Errores de autorizacion por rol cubiertos por pruebas.
- PR no se aprueba si falla suite de pruebas automatizadas.

## 10. Checklist de validacion
- Estado actual de cobertura coincide con el repositorio.
- Backlog contiene al menos 10 casos prioritarios.
- Comandos son ejecutables con la solucion actual.

## 11. Historial de cambios
- 2026-04-23: Documento creado con estrategia incremental de cobertura.
