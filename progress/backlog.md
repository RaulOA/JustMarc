# Backlog de ideas

> Append-only. Una línea con fecha por idea. Lo llena `/idea` (en "Ideas propias"), el `reviewer` (en
> "Ideas derivadas de hallazgos") y lo procesa `/planificar`.

## Ideas propias

> Ideas que vos anotás (vía `/idea`). Una línea con fecha.

<!-- - [YYYY-MM-DD] <idea> -->
- [2026-06-28] Reglas de negocio de delegación (relacionado a F-004/T027): 1. los aprovadores delegados son temporales, y deben ser asignados por el aprobador titular/jefe (el aprobador titular/jefe lo asigna un admin) quien debe colocar la fecha de inicio en la que se le concede el permiso a dicho delegado asi como la de fin, y la app habilitara y desabilitara dicho permiso segun esas fechas. 2. el jefe podra anular o detener este acceso en cualquier momento. 3. los delegados no podran delegar a nadie. 4. cualquier admin puede ayudar igualmente a los titulares en este proceso. 5. los delegados no pueden aprobar justificaciones al titular, a si mismas o a personal que pertenece a dependencias fuera del rango jerarquico de su titular. 6. el delegado debe poder visualizar su nueva funcion, quien se la asigno, y el alcance de a que dependencias podra aprobar. 7. las aprobaciones que hayan sido dirigidas a un delegado y no se hayan tramitado a tiempo por parte del mismo se bloquearan en la fecha de fin del permiso indicada por el titular. 8. el titular siempre tendra permiso de ver y modificar las justificaciones que hayan sido creadas en su ausencia por niveles inferiores. 9. los delegados podran tener un registro de solo lectura de las justificaciones delegadas a ellos y este registro solo registrara las justificaciones que se les haya delegado en los tiempos en que se les concedio el permiso.

## Ideas derivadas de hallazgos

> Mejoras detectadas por el `reviewer` durante la revisión (no son brechas que bloquean; esas se
> rechazan). Cada idea cita la **feature** a la que se relaciona: un `id` existente (p. ej. `F-003`) o
> `nueva`. El hallazgo de origen queda en `progress/findings.md`.

<!-- - [YYYY-MM-DD] (F-xxx | nueva) <mejora derivada del hallazgo> -->
- [2026-06-28] (nueva) Auditoría "modificado por" en jerarquías de aprobación: o se agrega la columna `ModificadoPor` (+`FechaHoraModificacion`) a `Operacion.JerarquiaAprobacion`, o se deja de enviar ese dato en `UpdateJerarquia`. Herencia previa. Origen: `progress/findings.md`.
