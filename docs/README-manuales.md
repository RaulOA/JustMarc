# Portal Documental - Integrador Marcas

Indice maestro de documentacion tecnica y funcional del repositorio.

## 1. Proposito
Centralizar documentos, audiencia, frecuencia de actualizacion y owner tecnico para evitar duplicidad y desalineacion con el codigo.

## 2. Mapa documental
| Documento | Audiencia | Frecuencia | Owner tecnico |
|---|---|---|---|
| [manual-tecnico.md](manual-tecnico.md) | Desarrollo, soporte, QA | Por release | Backend + Operacion |
| [Guia_Implementacion_Dev_Prod.md](Guia_Implementacion_Dev_Prod.md) | DevOps, infraestructura | Por cambio de despliegue | Operacion |
| [api-endpoints-reference.md](api-endpoints-reference.md) | Backend, frontend, QA | Por cambio de endpoint/contrato | Backend |
| [arquitectura-codigo-actual.md](arquitectura-codigo-actual.md) | Arquitectura, liderazgo tecnico | Mensual o por cambio estructural | Backend |
| [frontend-modulos-y-flujos.md](frontend-modulos-y-flujos.md) | Frontend, QA | Por cambio en app.js/dashboard | Frontend |
| [flujos-datos-end-to-end.md](flujos-datos-end-to-end.md) | Backend, frontend, QA, soporte | Por cambio de flujo de negocio | Backend + Frontend |
| [convenciones-codigo-y-documentacion.md](convenciones-codigo-y-documentacion.md) | Todo el equipo | Trimestral | Arquitectura/Tech Lead |
| [pruebas-estrategia-y-cobertura.md](pruebas-estrategia-y-cobertura.md) | QA, desarrollo | Por sprint | QA + Backend |
| [manual-usuario-funcionario.md](manual-usuario-funcionario.md) | Funcionario | Por cambio de UX funcional | Frontend/Producto |
| [manual-usuario-jefatura.md](manual-usuario-jefatura.md) | Jefaturas | Por cambio de UX funcional | Frontend/Producto |
| [manual-usuario-rrhh.md](manual-usuario-rrhh.md) | RRHH | Por cambio de UX funcional | Frontend/Producto |
| [PRP.md](PRP.md) | Producto y gestion | Por hito | Producto |

## 3. Guia de uso rapido
- Si cambia API o validaciones: actualizar api-endpoints-reference y flujos-datos-end-to-end.
- Si cambia estructura de proyectos o DI: actualizar arquitectura-codigo-actual.
- Si cambia UI/roles/eventos: actualizar frontend-modulos-y-flujos y manuales de usuario.
- Si cambia proceso de despliegue: actualizar Guia_Implementacion_Dev_Prod.

## 4. Convencion de versionado documental
- Registrar fecha y resumen en la seccion Historial de cambios de cada documento.
- Evitar duplicar contratos API en multiples archivos; usar referencias cruzadas.
- Mantener este portal como fuente principal de navegacion.

## 5. Historial de cambios
- 2026-04-23: Portal convertido a indice maestro con audiencia, frecuencia y owners.
