# Constitución del proyecto — Justificación de Marca (INTEGRA_CNP)

Versión: 1.0.0 — 2026-06-28

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

*(Sección sujeta a tu visto bueno — es contrato.)*

5. **Datos sensibles y credenciales.** La app maneja datos de RRHH. Las credenciales de conexión NO
   DEBEN versionarse (se inyectan por `ConnectionStrings__IntegraCnp`). Los logs NO DEBEN exponer datos
   sensibles más allá de lo necesario.
6. **Integridad de fuentes externas.** `WIZDOM`/`SIFCNP` son **solo lectura**: el sistema NO DEBE
   ejecutar INSERT/UPDATE/DELETE contra ellas. Toda escritura DEBE ocurrir en `INTEGRA_CNP`.
7. **Clean Architecture.** DEBE respetarse la regla de dependencia hacia adentro; el SQL NO DEBE vivir
   en controllers (va en `Queries/*Sql.cs` + Repository).
8. **Español y UTF-8.** El dominio, la UI, los mensajes y los specs DEBEN estar en español, cuidando
   UTF-8 para evitar mojibake.
9. **Compatibilidad de plataforma y target.** El tooling DEBE funcionar en Windows/PowerShell; los
   proyectos DEBEN target `net8.0` (no asumir net10).
10. **Convenciones de BD.** Todo objeto de BD nuevo DEBE seguir `docs/db/Convenciones_Nomeclatura_BD.md`
    (PascalCase, esquemas funcionales, PK `[Tabla]Id`, scripts idempotentes).

## Regla de enmienda

Cambiar esta constitución requiere **acción humana** y un **bump semver** (MAYOR para cambios
incompatibles de principio; MENOR para añadir principios; PATCH para aclaraciones). El agente nunca la
enmienda por su cuenta.
