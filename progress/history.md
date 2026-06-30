# Bitácora de features terminadas

> Append-only y compacto: una línea por feature (qué · cuándo · approved_by · resumen).

<!-- - [YYYY-MM-DD] <feature> · aprobó <humano> · <resumen 1-2 líneas> -->
- [2026-06-28] cierre-jerarquia-aprobacion-avanzada (F-003, cronograma T026) · aprobó Raul OA · Cerró casos edge de resolución de aprobadores (sin aprobador, niveles múltiples, prioridad de delegación, vigencia) y validaciones de alta/edición; única pieza nueva: anti-duplicidad vigente (AppException 409). Revisado y aprobado, init.ps1 verde (27 unitarios). Hallazgo menor `ModificadoPor` → backlog (nueva).
- [2026-06-30] delegaciones-subaprobadores-reglas-completas (F-004, cronograma T027) · aprobó Raul OA · Cerró las reglas de delegación end-to-end sobre la base existente: restricciones del delegado (no aprueba al titular/a sí mismo/fuera de rango, expiración), anti-sub-delegación (409), borrado físico alineado a la UI con auditoría previa, vistas del delegado (función + registro de solo lectura), soberanía del titular (PATCH revisar-titular) y columnas de auditoría en `DelegacionAprobacion`. Revisado en 2 vueltas (brecha R1 —exigir VigenciaDesde— corregida) y aprobado, init.ps1 verde (54 unitarios). Deuda abierta aparte: `ModificadoPor` en `JerarquiaAprobacion` (findings/backlog).
