# Constitución del proyecto — Justificación de Marca (INTEGRA_CNP)

Versión: 1.1.0 — 2026-06-28

> Principios **inmutables**. Cambiarlos requiere acción humana + bump semver (ver Regla de enmienda). El
> agente nunca la enmienda solo.

## Núcleo invariante

1. **Verificación antes de cerrar.** Ninguna feature es `done` sin `init.ps1` en verde y trazabilidad
   `R<n>→test` completa. NO DEBE marcarse `done` de otra forma.
2. **La spec es el contrato.** Sin spec aprobado por un humano (`approved_by`/`approved_at`) NO DEBE
   escribirse código de una feature `sdd`.
3. **Una feature a la vez; el estado vive en archivos.** DEBE haber a lo sumo una feature `in_progress`,
   y el estado DEBE persistir en archivos versionados (no en el chat).
4. **Sin workarounds ante fallos.** Ante un fallo de herramienta el agente NO DEBE improvisar: para,
   marca `blocked`, documenta.

## Principios del proyecto

5. **Datos sensibles y credenciales.** La app maneja datos de RRHH. Las credenciales de conexión NO
   DEBEN versionarse (se inyectan por `ConnectionStrings__IntegraCnp`). Los logs NO DEBEN exponer datos
   sensibles más allá de lo necesario.
6. **Integridad de fuentes externas.** `WIZDOM`/`SIFCNP` son **solo lectura**: el sistema NO DEBE
   ejecutar INSERT/UPDATE/DELETE contra ellas. Toda escritura DEBE ocurrir en `INTEGRA_CNP`.
7. **Operaciones destructivas sobre bases reales.** Ninguna operación destructiva o irreversible sobre
   las bases reales (`WIZDOM`, `SIFCNP`, `INTEGRA_CNP`) —DROP, DELETE/TRUNCATE masivo, ALTER con pérdida
   de datos, restore/overwrite— DEBE ejecutarse sin **aprobación humana explícita**.
8. **Clean Architecture.** DEBE respetarse la regla de dependencia hacia adentro; el SQL NO DEBE vivir
   en controllers (va en `Queries/*Sql.cs` + Repository).
9. **Español y UTF-8.** El dominio, la UI, los mensajes y los specs DEBEN estar en español, cuidando
   UTF-8 para evitar mojibake.
10. **Compatibilidad de plataforma y target.** El tooling DEBE funcionar en Windows/PowerShell; los
    proyectos DEBEN target `net8.0` (no asumir net10).
11. **Convenciones de BD.** Todo objeto de BD nuevo DEBE seguir `docs/db/Convenciones_Nomeclatura_BD.md`
    (PascalCase, esquemas funcionales, PK `[Tabla]Id`, scripts idempotentes).

## Regla de enmienda

Cambiar esta constitución requiere **acción humana** y un **bump semver** (MAYOR para cambios
incompatibles de principio; MENOR para añadir principios; PATCH para aclaraciones). El agente nunca la
enmienda por su cuenta.

## Historial de enmiendas

- **v1.1.0** (2026-06-28): añade el principio 7 (operaciones destructivas/irreversibles sobre bases
  reales requieren aprobación humana explícita). Aprobado por el humano.
- **v1.0.0** (2026-06-28): versión inicial.
