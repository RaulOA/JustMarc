# Análisis de avance real para actualizar Cronograma_Final_Marcas.csv

Fecha de corte: 2026-05-05

## Alcance y criterio

Este análisis se basa solo en evidencia verificable dentro del repositorio:

- Código fuente backend y frontend.
- Scripts SQL y semillas en docs/db.
- Documentación operativa y técnica en README.md, docs y .vscode.
- Resultado de la suite actual de pruebas ejecutada el 2026-05-05: 8/8 pruebas correctas.

No se asumió avance por trabajo no reflejado en el repositorio. Cuando una tarea describe ejecución real fuera del repo, por ejemplo UAT con usuarios, go-live o soporte post-despliegue, la recomendación se mantiene baja o en cero salvo que exista evidencia documental explícita de esa ejecución.

## Resumen ejecutivo

- T001-T020: no encontré evidencia que obligue a reducir el estado actual. El repositorio sostiene una base funcional de backend, frontend, scripts BD, tareas VS Code y pruebas ejecutables.
- T021: la implementación base de jerarquía configurable ya existe y está conectada al flujo real de aprobación.
- T022-T028: hay trabajo sustancial ya implementado en delegaciones, auditoría y panel admin, pero no todo está cerrado end to end.
- T029-T034: no hay evidencia suficiente de UAT formal, capacitaciones ni manuales finales derivados de retroalimentación.
- T035-T036: hay documentación y configuración preparatoria para producción, pero no evidencia de ejecución real del despliegue.
- T037-T038: no hay evidencia de go-live ni operación post-producción.

## Validación de tareas previas

Recomendación para T001-T020: mantener los valores actuales.

Evidencia resumida:

- La solución compila y las pruebas pasan en backend/tests/IntegradorMarcas.Tests.
- Existen controladores, servicios, repositorios y consultas para los flujos principales.
- Existen scripts base y objetos SQL para esquema, catálogos, auditoría e integración.
- El frontend y las tareas de VS Code permiten levantar API y UI localmente.
- README.md ya documenta onboarding, Swagger, REST Client, base de datos y despliegue IIS.

## Recomendación detallada T021-T038

| ID Tarea | Valor actual | Valor recomendado | Evidencia resumida |
| --- | --- | --- | --- |
| T021 | 70.00% / En Progreso / Jerarquía en desarrollo | 100.00% / Completado / Jerarquía configurable implementada en BD, backend y consultas operativas; cerrar solo cobertura adicional en tareas posteriores | docs/db/001_integra_marcas_base_inicial.sql crea Operacion.JerarquiaAprobacion e índices; docs/db/002_integra_marcas_objetos.sql crea fn_AprobadoresVigentesPorSolicitante; backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs expone CRUD base de jerarquías; backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs usa la función para aprobador actual, pendientes y resolver; backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs aplica esas reglas en runtime |
| T022 | 0.00% / Bloqueada / Sin comentario | 80.00% / En Progreso / Delegaciones implementadas en BD, backend y UI admin básica; faltan cierres end to end y alineación de acciones de borrado | docs/db/001_integra_marcas_base_inicial.sql crea Operacion.DelegacionAprobacion; docs/db/002_integra_marcas_objetos.sql resuelve delegación vigente en la función de aprobadores; backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs expone listar, crear, actualizar y cambiar estado; app.js y dashboard.html ya tienen panel de Delegaciones; backend/tests/IntegradorMarcas.Tests/JustificacionServiceCurrentApproverTests.cs cubre caso Delegacion; gap verificable: app.js intenta DELETE pero el controlador no publica HttpDelete |
| T023 | 0.00% / Bloqueada / Sin comentario | 75.00% / En Progreso / Logging de errores y eventos ya persiste; existe panel admin de registros con filtros y exportación CSV; falta consolidar auditoría detallada en la vista admin | backend/src/IntegradorMarcas.Api/Program.cs registra excepciones en Auditoria.ErrorApi; backend/src/IntegradorMarcas.Infrastructure/Repositories/AuditEventRepository.cs persiste EventoAuditoria; docs/db/008_admin_audit_and_alignment.sql crea Auditoria.AdminAccionAuditoria; backend/src/IntegradorMarcas.Api/Controllers/AdminMonitoringController.cs consulta ErrorApi y EventoAuditoria; app.js y dashboard.html ya renderizan panel Registros y descarga CSV; backend/tests/IntegradorMarcas.Tests/ErrorLogIntegrationTests.cs pasa contra Auditoria.ErrorApi |
| T024 | 50.00% / En Progreso / Sin comentario | 70.00% / En Progreso / Existe documentación técnica y operativa amplia, además de ayuda para API; faltan manuales formales por rol | README.md cubre onboarding, Swagger, REST Client, setup BD, pre-check productivo e IIS; backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.http aporta smoke tests manuales; controladores incluyen comentarios XML; no hay manuales de usuario separados en docs/ |
| T025 | 0.00% / En Espera de Debloqueo / Sin comentario | 35.00% / En Progreso / Hay correcciones y pulido incremental, pero no evidencia de pruebas cross-browser ni optimización formal de rendimiento | Existen múltiples specs y correcciones funcionales en docs/specs, más paginación en panel admin y varios índices SQL; docs/db/001_integra_marcas_base_inicial.sql crea índices relevantes; no hay matrices cross-browser, benchmarks ni reportes de performance en el repo |
| T026 | 0.00% / En Espera de Debloqueo / Sin comentario | 45.00% / En Progreso / La jerarquía ya opera con validaciones base, pero el cierre avanzado todavía no está completo | backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs valida permisos admin, referencias y consistencia; backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs valida scope de aprobación y evita autoaprobación; docs/db/007_seed_hierarquia_dependencias.sql agrega semillas jerárquicas complejas; persisten huecos de cierre como falta de endpoints DELETE y ausencia de pruebas específicas de CRUD/edge cases admin |
| T027 | 0.00% / En Espera de Debloqueo / Sin comentario | 40.00% / En Progreso / Delegación funcional disponible, pero reglas avanzadas y cierre total de sub-aprobación no están demostrados end to end | Hay tabla, consultas, controller, service y UI básica para delegaciones; la función de aprobadores prioriza Delegacion sobre Jerarquia; AdminAprobacionesService valida no auto-delegación y referencias; no hay evidencia de un módulo diferenciado de sub-aprobadores ni pruebas CRUD completas de delegación |
| T028 | 0.00% / En Espera de Debloqueo / Sin comentario | 65.00% / En Progreso / El sistema de monitoreo/auditoría ya es funcional para errores y eventos; falta terminar la capa de auditoría administrativa detallada en UI | AdminMonitoringController ya devuelve registros consolidados; app.js permite filtrar, ordenar y descargar CSV; docs/db/008_admin_audit_and_alignment.sql crea auditoría detallada admin; no existe evidencia de pantalla dedicada que consulte Auditoria.AdminAccionAuditoria |
| T029 | 0.00% / En Espera de Debloqueo / Sin comentario | 20.00% / En Progreso / El entorno y scripts de prueba local están listos, pero no hay preparación UAT formal con usuarios finales | .vscode/tasks.json y .vscode/launch.json facilitan entorno de prueba; README.md y el endpoint health/Swagger respaldan smoke testing; backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.http ofrece requests listas; no hay plan UAT, scripts de aceptación ni evidencias de coordinación con usuarios |
| T030 | 0.00% / En Espera de Debloqueo / Sin comentario | 0.00% / No Iniciado / No hay evidencia verificable de ejecución de UAT con usuarios finales ni de feedback recopilado | No se encontraron actas, reportes, issues UAT, minutas ni documentos de resultados en docs/ o tests/ |
| T031 | 0.00% / En Espera de Debloqueo / Sin comentario | 0.00% / No Iniciado / Hay correcciones técnicas en el repositorio, pero no están trazadas a defectos levantados en UAT formal | Existen correcciones y specs de fixes, pero no hay evidencia que las vincule a una ronda UAT ejecutada y revalidada |
| T032 | 0.00% / En Espera de Debloqueo / Sin comentario | 0.00% / No Iniciado / No hay evidencia de sesiones de capacitación a usuarios finales | No existen materiales de capacitación, listas de asistencia, agendas o manuales específicos para usuarios finales |
| T033 | 0.00% / En Espera de Debloqueo / Sin comentario | 0.00% / No Iniciado / No hay evidencia de capacitación formal a administradores | Aunque existe panel admin y documentación técnica parcial, no hay material o registro de capacitación administrativa |
| T034 | 0.00% / En Espera de Debloqueo / Sin comentario | 0.00% / No Iniciado / No hay evidencia de actualización documental basada en feedback de UAT/capacitación | README.md existe, pero no hay rastro verificable de iteración basada en UAT o sesiones de capacitación |
| T035 | 0.00% / En Espera de Debloqueo / Sin comentario | 55.00% / En Progreso / Existe plan de despliegue productivo documentado; falta un plan verificable de rollback y ejecución real | README.md documenta pre-check BD y despliegue a Producción en IIS; docs/specs/readme_iis_prod_sin_herramientas_spec.md muestra el análisis previo; falta evidencia explícita de rollback operativo detallado y de plan de migración ejecutado |
| T036 | 0.00% / En Espera de Debloqueo / Sin comentario | 15.00% / En Progreso / Hay preparación de configuración para producción en repo, pero no evidencia de entorno realmente configurado | backend/src/IntegradorMarcas.Api/appsettings.Production.json existe; backend/src/IntegradorMarcas.Api/Program.cs exige cadena de conexión fuera de Development; README.md describe variables IIS y Hosting Bundle; no hay evidencia del ambiente productivo configurado ni validado |
| T037 | 0.00% / En Espera de Debloqueo / Sin comentario | 0.00% / No Iniciado / No hay evidencia de go-live, migración productiva ni validación en producción | No existen actas de pase, scripts de release ejecutados, artefactos publish versionados ni documentación de validación post-release |
| T038 | 0.00% / En Espera de Debloqueo / Sin comentario | 0.00% / No Iniciado / Aún no hay evidencia de operación post-despliegue; solo existe capacidad técnica de monitoreo | El repo sí contiene monitoreo de errores y eventos, pero no evidencia de soporte real posterior a un despliegue productivo |

## Hallazgos clave que impactan el cronograma

1. La jerarquía de aprobación no está solo "en desarrollo": ya está modelada, sembrada y conectada a consultas críticas del sistema.
2. Delegaciones tampoco están bloqueadas en cero: ya tienen tabla, lógica, API y UI básica.
3. La auditoría existe en tres niveles: ErrorApi, EventoAuditoria y AdminAccionAuditoria; sin embargo, la UI admin actual solo consolida ErrorApi y EventoAuditoria.
4. El panel admin de jerarquías y delegaciones no está completamente cerrado end to end porque app.js invoca DELETE sobre rutas que el controlador actual no expone.
5. La documentación técnica está bastante más avanzada de lo que refleja T024.
6. UAT, capacitación, go-live y soporte post-producción no tienen respaldo verificable en el repositorio y no deberían inflarse.

## Recomendación operativa para actualizar el CSV

- Mantener T001-T020 como están.
- Actualizar T021-T028 para reflejar implementación real ya existente.
- Mantener T030-T034 y T037-T038 en cero hasta contar con evidencia documental verificable.
- Marcar T029, T035 y T036 como parciales por preparación/documentación, no como ejecutadas.
