# Especificacion de completado de fechas - Cronograma Justificacion de Marcas

Fecha de analisis: 24/04/2026
Fuente: docs/Cronograma_Justificacion_Marcas.csv

## 1) Reglas aplicadas

1. Se preservan sin cambios las 3 fechas ya existentes en el CSV:
   - T001 / Fecha Inicio Baseline = 06/10/2025
   - T001 / Fecha Inicio Real = 06/10/2025
   - T038 / Fecha Fin Baseline = 31/03/2026
2. Baseline global usada como ancla de inicio: 06/10/2025, con secuenciacion por dependencia y duracion en dias habiles (lunes-viernes).
3. Para tareas bloqueadas/en espera con fechas proyectadas en comentarios, se usan esas fechas en reales.
4. Si una fecha real no aparece explicita, se infiere por dependencia y continuidad laboral; si solo existe fin proyectado, inicio se calcula hacia atras por dias habiles.
5. Se cumple consistencia por fila en fechas reales: Fecha Inicio Real <= Fecha Fin Real <= Fecha Entrega Real (se usa Entrega Real = Fin Real).
6. Inconsistencia detectada de origen en baseline: la suma de duraciones es 150 dias habiles, mayor al rango habil entre 06/10/2025 y 31/03/2026. Por eso, al respetar la fecha fija existente de T038 (Fin Baseline = 31/03/2026), T038 se deja con Inicio Baseline = 30/03/2026 para consistencia de fila, aunque rompe continuidad estricta de dependencia al cierre.

## 2) Tabla de fechas resultante (T001-T038)

| ID | Fecha Inicio Baseline | Fecha Fin Baseline | Fecha Inicio Real | Fecha Fin Real | Fecha Entrega Real |
|---|---|---|---|---|---|
| T001 | 06/10/2025 | 08/10/2025 | 06/10/2025 | 08/10/2025 | 08/10/2025 |
| T002 | 09/10/2025 | 14/10/2025 | 09/10/2025 | 14/10/2025 | 14/10/2025 |
| T003 | 15/10/2025 | 16/10/2025 | 15/10/2025 | 16/10/2025 | 16/10/2025 |
| T004 | 17/10/2025 | 21/10/2025 | 17/10/2025 | 21/10/2025 | 21/10/2025 |
| T005 | 22/10/2025 | 31/10/2025 | 22/10/2025 | 31/10/2025 | 31/10/2025 |
| T006 | 03/11/2025 | 06/11/2025 | 03/11/2025 | 06/11/2025 | 06/11/2025 |
| T007 | 07/11/2025 | 12/11/2025 | 07/11/2025 | 12/11/2025 | 12/11/2025 |
| T008 | 13/11/2025 | 18/11/2025 | 13/11/2025 | 18/11/2025 | 18/11/2025 |
| T009 | 19/11/2025 | 21/11/2025 | 19/11/2025 | 21/11/2025 | 21/11/2025 |
| T010 | 24/11/2025 | 01/12/2025 | 24/11/2025 | 01/12/2025 | 01/12/2025 |
| T011 | 02/12/2025 | 08/12/2025 | 02/12/2025 | 08/12/2025 | 08/12/2025 |
| T012 | 09/12/2025 | 16/12/2025 | 09/12/2025 | 16/12/2025 | 16/12/2025 |
| T013 | 17/12/2025 | 23/12/2025 | 17/12/2025 | 23/12/2025 | 23/12/2025 |
| T014 | 24/12/2025 | 26/12/2025 | 24/12/2025 | 26/12/2025 | 26/12/2025 |
| T015 | 29/12/2025 | 01/01/2026 | 29/12/2025 | 01/01/2026 | 01/01/2026 |
| T016 | 02/01/2026 | 06/01/2026 | 02/01/2026 | 06/01/2026 | 06/01/2026 |
| T017 | 07/01/2026 | 08/01/2026 | 07/01/2026 | 08/01/2026 | 08/01/2026 |
| T018 | 09/01/2026 | 13/01/2026 | 09/01/2026 | 13/01/2026 | 13/01/2026 |
| T019 | 14/01/2026 | 19/01/2026 | 14/01/2026 | 19/01/2026 | 19/01/2026 |
| T020 | 20/01/2026 | 22/01/2026 | 20/01/2026 | 22/01/2026 | 22/01/2026 |
| T021 | 23/01/2026 | 30/01/2026 | 08/04/2026 | 15/04/2026 | 15/04/2026 |
| T022 | 02/02/2026 | 05/02/2026 | 16/04/2026 | 20/04/2026 | 20/04/2026 |
| T023 | 06/02/2026 | 10/02/2026 | 21/04/2026 | 25/04/2026 | 25/04/2026 |
| T024 | 11/02/2026 | 12/02/2026 | 24/04/2026 | 27/04/2026 | 27/04/2026 |
| T025 | 13/02/2026 | 16/02/2026 | 28/04/2026 | 30/04/2026 | 30/04/2026 |
| T026 | 17/02/2026 | 23/02/2026 | 01/05/2026 | 08/05/2026 | 08/05/2026 |
| T027 | 24/02/2026 | 02/03/2026 | 09/05/2026 | 15/05/2026 | 15/05/2026 |
| T028 | 03/03/2026 | 09/03/2026 | 16/05/2026 | 22/05/2026 | 22/05/2026 |
| T029 | 10/03/2026 | 16/03/2026 | 23/05/2026 | 29/05/2026 | 29/05/2026 |
| T030 | 17/03/2026 | 23/03/2026 | 30/05/2026 | 05/06/2026 | 05/06/2026 |
| T031 | 24/03/2026 | 30/03/2026 | 06/06/2026 | 12/06/2026 | 12/06/2026 |
| T032 | 31/03/2026 | 06/04/2026 | 13/06/2026 | 19/06/2026 | 19/06/2026 |
| T033 | 07/04/2026 | 09/04/2026 | 20/06/2026 | 22/06/2026 | 22/06/2026 |
| T034 | 10/04/2026 | 15/04/2026 | 23/06/2026 | 27/06/2026 | 27/06/2026 |
| T035 | 16/04/2026 | 20/04/2026 | 28/06/2026 | 01/07/2026 | 01/07/2026 |
| T036 | 21/04/2026 | 24/04/2026 | 02/07/2026 | 06/07/2026 | 06/07/2026 |
| T037 | 27/04/2026 | 29/04/2026 | 07/07/2026 | 11/07/2026 | 11/07/2026 |
| T038 | 30/03/2026 | 31/03/2026 | 12/07/2026 | 14/07/2026 | 14/07/2026 |

## 3) Listado de celdas a actualizar (solo vacias)

Nota: no se incluyen las 3 celdas existentes que deben quedar intactas.

- T001: Fecha Fin Baseline=08/10/2025; Fecha Fin Real=08/10/2025; Fecha Entrega Real=08/10/2025
- T002: Fecha Inicio Baseline=09/10/2025; Fecha Fin Baseline=14/10/2025; Fecha Inicio Real=09/10/2025; Fecha Fin Real=14/10/2025; Fecha Entrega Real=14/10/2025
- T003: Fecha Inicio Baseline=15/10/2025; Fecha Fin Baseline=16/10/2025; Fecha Inicio Real=15/10/2025; Fecha Fin Real=16/10/2025; Fecha Entrega Real=16/10/2025
- T004: Fecha Inicio Baseline=17/10/2025; Fecha Fin Baseline=21/10/2025; Fecha Inicio Real=17/10/2025; Fecha Fin Real=21/10/2025; Fecha Entrega Real=21/10/2025
- T005: Fecha Inicio Baseline=22/10/2025; Fecha Fin Baseline=31/10/2025; Fecha Inicio Real=22/10/2025; Fecha Fin Real=31/10/2025; Fecha Entrega Real=31/10/2025
- T006: Fecha Inicio Baseline=03/11/2025; Fecha Fin Baseline=06/11/2025; Fecha Inicio Real=03/11/2025; Fecha Fin Real=06/11/2025; Fecha Entrega Real=06/11/2025
- T007: Fecha Inicio Baseline=07/11/2025; Fecha Fin Baseline=12/11/2025; Fecha Inicio Real=07/11/2025; Fecha Fin Real=12/11/2025; Fecha Entrega Real=12/11/2025
- T008: Fecha Inicio Baseline=13/11/2025; Fecha Fin Baseline=18/11/2025; Fecha Inicio Real=13/11/2025; Fecha Fin Real=18/11/2025; Fecha Entrega Real=18/11/2025
- T009: Fecha Inicio Baseline=19/11/2025; Fecha Fin Baseline=21/11/2025; Fecha Inicio Real=19/11/2025; Fecha Fin Real=21/11/2025; Fecha Entrega Real=21/11/2025
- T010: Fecha Inicio Baseline=24/11/2025; Fecha Fin Baseline=01/12/2025; Fecha Inicio Real=24/11/2025; Fecha Fin Real=01/12/2025; Fecha Entrega Real=01/12/2025
- T011: Fecha Inicio Baseline=02/12/2025; Fecha Fin Baseline=08/12/2025; Fecha Inicio Real=02/12/2025; Fecha Fin Real=08/12/2025; Fecha Entrega Real=08/12/2025
- T012: Fecha Inicio Baseline=09/12/2025; Fecha Fin Baseline=16/12/2025; Fecha Inicio Real=09/12/2025; Fecha Fin Real=16/12/2025; Fecha Entrega Real=16/12/2025
- T013: Fecha Inicio Baseline=17/12/2025; Fecha Fin Baseline=23/12/2025; Fecha Inicio Real=17/12/2025; Fecha Fin Real=23/12/2025; Fecha Entrega Real=23/12/2025
- T014: Fecha Inicio Baseline=24/12/2025; Fecha Fin Baseline=26/12/2025; Fecha Inicio Real=24/12/2025; Fecha Fin Real=26/12/2025; Fecha Entrega Real=26/12/2025
- T015: Fecha Inicio Baseline=29/12/2025; Fecha Fin Baseline=01/01/2026; Fecha Inicio Real=29/12/2025; Fecha Fin Real=01/01/2026; Fecha Entrega Real=01/01/2026
- T016: Fecha Inicio Baseline=02/01/2026; Fecha Fin Baseline=06/01/2026; Fecha Inicio Real=02/01/2026; Fecha Fin Real=06/01/2026; Fecha Entrega Real=06/01/2026
- T017: Fecha Inicio Baseline=07/01/2026; Fecha Fin Baseline=08/01/2026; Fecha Inicio Real=07/01/2026; Fecha Fin Real=08/01/2026; Fecha Entrega Real=08/01/2026
- T018: Fecha Inicio Baseline=09/01/2026; Fecha Fin Baseline=13/01/2026; Fecha Inicio Real=09/01/2026; Fecha Fin Real=13/01/2026; Fecha Entrega Real=13/01/2026
- T019: Fecha Inicio Baseline=14/01/2026; Fecha Fin Baseline=19/01/2026; Fecha Inicio Real=14/01/2026; Fecha Fin Real=19/01/2026; Fecha Entrega Real=19/01/2026
- T020: Fecha Inicio Baseline=20/01/2026; Fecha Fin Baseline=22/01/2026; Fecha Inicio Real=20/01/2026; Fecha Fin Real=22/01/2026; Fecha Entrega Real=22/01/2026
- T021: Fecha Inicio Baseline=23/01/2026; Fecha Fin Baseline=30/01/2026; Fecha Inicio Real=08/04/2026; Fecha Fin Real=15/04/2026; Fecha Entrega Real=15/04/2026
- T022: Fecha Inicio Baseline=02/02/2026; Fecha Fin Baseline=05/02/2026; Fecha Inicio Real=16/04/2026; Fecha Fin Real=20/04/2026; Fecha Entrega Real=20/04/2026
- T023: Fecha Inicio Baseline=06/02/2026; Fecha Fin Baseline=10/02/2026; Fecha Inicio Real=21/04/2026; Fecha Fin Real=25/04/2026; Fecha Entrega Real=25/04/2026
- T024: Fecha Inicio Baseline=11/02/2026; Fecha Fin Baseline=12/02/2026; Fecha Inicio Real=24/04/2026; Fecha Fin Real=27/04/2026; Fecha Entrega Real=27/04/2026
- T025: Fecha Inicio Baseline=13/02/2026; Fecha Fin Baseline=16/02/2026; Fecha Inicio Real=28/04/2026; Fecha Fin Real=30/04/2026; Fecha Entrega Real=30/04/2026
- T026: Fecha Inicio Baseline=17/02/2026; Fecha Fin Baseline=23/02/2026; Fecha Inicio Real=01/05/2026; Fecha Fin Real=08/05/2026; Fecha Entrega Real=08/05/2026
- T027: Fecha Inicio Baseline=24/02/2026; Fecha Fin Baseline=02/03/2026; Fecha Inicio Real=09/05/2026; Fecha Fin Real=15/05/2026; Fecha Entrega Real=15/05/2026
- T028: Fecha Inicio Baseline=03/03/2026; Fecha Fin Baseline=09/03/2026; Fecha Inicio Real=16/05/2026; Fecha Fin Real=22/05/2026; Fecha Entrega Real=22/05/2026
- T029: Fecha Inicio Baseline=10/03/2026; Fecha Fin Baseline=16/03/2026; Fecha Inicio Real=23/05/2026; Fecha Fin Real=29/05/2026; Fecha Entrega Real=29/05/2026
- T030: Fecha Inicio Baseline=17/03/2026; Fecha Fin Baseline=23/03/2026; Fecha Inicio Real=30/05/2026; Fecha Fin Real=05/06/2026; Fecha Entrega Real=05/06/2026
- T031: Fecha Inicio Baseline=24/03/2026; Fecha Fin Baseline=30/03/2026; Fecha Inicio Real=06/06/2026; Fecha Fin Real=12/06/2026; Fecha Entrega Real=12/06/2026
- T032: Fecha Inicio Baseline=31/03/2026; Fecha Fin Baseline=06/04/2026; Fecha Inicio Real=13/06/2026; Fecha Fin Real=19/06/2026; Fecha Entrega Real=19/06/2026
- T033: Fecha Inicio Baseline=07/04/2026; Fecha Fin Baseline=09/04/2026; Fecha Inicio Real=20/06/2026; Fecha Fin Real=22/06/2026; Fecha Entrega Real=22/06/2026
- T034: Fecha Inicio Baseline=10/04/2026; Fecha Fin Baseline=15/04/2026; Fecha Inicio Real=23/06/2026; Fecha Fin Real=27/06/2026; Fecha Entrega Real=27/06/2026
- T035: Fecha Inicio Baseline=16/04/2026; Fecha Fin Baseline=20/04/2026; Fecha Inicio Real=28/06/2026; Fecha Fin Real=01/07/2026; Fecha Entrega Real=01/07/2026
- T036: Fecha Inicio Baseline=21/04/2026; Fecha Fin Baseline=24/04/2026; Fecha Inicio Real=02/07/2026; Fecha Fin Real=06/07/2026; Fecha Entrega Real=06/07/2026
- T037: Fecha Inicio Baseline=27/04/2026; Fecha Fin Baseline=29/04/2026; Fecha Inicio Real=07/07/2026; Fecha Fin Real=11/07/2026; Fecha Entrega Real=11/07/2026
- T038: Fecha Inicio Baseline=30/03/2026; Fecha Inicio Real=12/07/2026; Fecha Fin Real=14/07/2026; Fecha Entrega Real=14/07/2026
