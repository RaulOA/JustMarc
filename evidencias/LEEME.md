# evidencias/ — Paquetes de evidencia para jefatura

Carpeta **independiente y autocontenida**: lo que jefatura recibe como prueba de avance de cada tarea del
cronograma (`docs/cronograma.xlsx`). Se mantiene con el ritual semanal `/cronograma`.

## Regla de oro: informe limpio y ejecutivo

Las fichas son de **negocio**, no técnicas. Demuestran que una tarea quedó **al 100%** (o justifican por
qué no). **PROHIBIDO** mencionar: inteligencia artificial, agentes, arnés, specs, identificadores de
feature (`F-xxx`), tests / `init.ps1`, commits, ramas o cualquier detalle del proceso de desarrollo.
Jefatura ve el desarrollo como tareas lineales; la ficha habla solo del **resultado funcional**.

## Estructura

```
evidencias/
  INDICE.md                       índice general (Tarea · Fecha entrega · Estado · ficha)
  LEEME.md                        este archivo
  _PLANTILLA-informe.md           plantilla base de cada informe.md
  <fecha-entrega>_<Txxx>_<slug>/  una carpeta por tarea entregada
    informe.md                    informe ejecutivo (capturas vinculadas)
    capturas/
      01-<descripcion>.png        capturas del frontend real
      02-<descripcion>.png
```

- `<fecha-entrega>` en formato `YYYY-MM-DD`. Ej.: `2026-06-30_T026_cierre-jerarquia/`.
- Las capturas se enlazan **relativas** desde `informe.md`: `![desc](capturas/01-...png)`.

## Cómo se generan las capturas

Frontend real, con el método nativo ya usado en `docs/manual-*/capturas/`: **Microsoft Edge en
`--headless=new` dirigido por CDP desde Node** (sin dependencias). Se levanta el stack
(`build-api` → `run-api` → `serve-frontend`) y se inyecta `sessionStorage.sjm_session` para entrar a cada
panel por rol. Detalle operativo en el comando `/cronograma`.
