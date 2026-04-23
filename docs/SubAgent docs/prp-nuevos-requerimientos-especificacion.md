# Especificación de integración de nuevos requerimientos al PRP

Fecha: 2026-04-23
Alcance: definición documental para incorporar auditoría persistente, rol Administrador, jerarquía de aprobaciones configurable, delegaciones y nuevos catálogos/estados organizacionales.

## 1. Resumen ejecutivo

- El PRP principal que debe actualizarse es `docs/PRP.md`.
- La razón es que `docs/PRP.md` es la versión más reciente y operativa del producto: incluye estado actual del prototipo, estructura del proyecto, checklist extendido y notas de implementación alineadas con el repositorio actual.
- `PRP_Justificacion_Marcas.md` debe mantenerse consistente como documento secundario porque todavía funciona como referencia de base para artefactos técnicos, en especial `docs/db/001_init_integra_cnp.sql`, cuyo encabezado indica que está basado en ese archivo.
- Los nuevos requerimientos no caben como simples notas marginales. Cambian modelo de roles, reglas de autorización, flujo de aprobación, modelo de datos y alcance del módulo administrativo. Deben integrarse en varias secciones y no solamente agregarse al final.

## 2. Documento PRP principal y documentos secundarios

### 2.1 PRP principal

- Archivo principal: `docs/PRP.md`
- Criterios:
  - versión 1.1 y fecha de actualización Abril 2026;
  - describe el estado actual del prototipo y la estructura real del repositorio;
  - contiene checklist y notas para desarrollo que reflejan mejor el estado vigente del proyecto.

### 2.2 Documentos secundarios que deben mantenerse consistentes

- `PRP_Justificacion_Marcas.md`
  - Debe reflejar el mismo alcance funcional y de datos del PRP principal para evitar dos líneas base contradictorias.
- `docs/db/001_init_integra_cnp.sql`
  - Debe alinearse con los nuevos roles, catálogos y tablas transversales, porque hoy materializa el modelo inicial de BD derivado del PRP secundario.
- `README.md`
  - Debe actualizarse cuando se formalicen nuevos endpoints, headers/roles soportados y pasos de inicialización de BD.
- `docs/manual-tecnico.md`
  - Debe alinearse con el nuevo catálogo de roles, nuevas restricciones administrativas y nuevos módulos/endpoint administrativos.
- `docs/Guia_Implementacion_Dev_Prod.md`
  - Debe reflejar las nuevas tablas/scripts y la eventual validación de rol Administrador.
- `docs/SubAgent docs/verificacion-roles-acceso.md`
  - Debe rehacerse o marcarse como desactualizado cuando Administrador quede formalizado, porque hoy afirma un universo de roles incompleto.

## 3. Análisis de la estructura actual del PRP

La estructura actual de `docs/PRP.md` es correcta para integrar los cambios sin rehacer el documento completo. Tiene secciones adecuadas para absorber impacto funcional, técnico y de datos:

1. Resumen del producto
2. Contexto y problema
3. Estado actual del prototipo
4. Stack tecnológico
5. Roles del sistema
6. Requerimientos funcionales
7. Requerimientos no funcionales
8. Arquitectura del software
9. Modelo de base de datos
10. Lógica de negocio y flujos
11. Integraciones externas
12. Estructura del proyecto
13. Checklist de progreso

### Observaciones de brecha actual

- El modelo de roles solo contempla Funcionario, Jefatura y RRHH.
- La aprobación está definida únicamente sobre subordinación directa por `JefaturaID`.
- La auditoría actual es mínima y documentalmente se limita a campos de trazabilidad de registro/modificación, no a una bitácora funcional persistente.
- No existe módulo administrativo formal en requisitos funcionales.
- El modelo de BD no contempla catálogos para eventos de auditoría, resultados, jerarquías, niveles de aprobación, delegaciones ni estados activo/inactivo.
- Las reglas de negocio fijan supuestos que los nuevos requerimientos invalidan, por ejemplo: "Solo ROL_JEFE puede cambiar el estado" y "Jefatura solo ve subordinados directos".

## 4. Estrategia de incorporación lógica y ordenada

La integración debe hacerse por sustitución controlada del modelo actual, no por anexos aislados.

### 4.1 Principio editorial

- Mantener la narrativa del sistema centrada en justificación de marcas.
- Introducir un subdominio administrativo transversal para configuración de seguridad, jerarquías, delegaciones y auditoría.
- Reemplazar referencias a jefatura directa como única fuente de aprobación por un modelo jerárquico configurable, usando la jefatura directa de WIZDOM solo como dato base de apoyo, no como regla suficiente.

### 4.2 Orden recomendado de actualización

1. Actualizar tabla de contenidos.
2. Formalizar rol Administrador en Roles del Sistema.
3. Ajustar RF existentes impactados.
4. Crear nuevos RF para auditoría, jerarquías y delegaciones.
5. Ajustar RNF de seguridad, trazabilidad y disponibilidad del flujo.
6. Ampliar arquitectura con módulo administrativo y servicio de auditoría.
7. Rediseñar el modelo de base de datos a nivel conceptual.
8. Reescribir reglas de negocio y flujos.
9. Aclarar en integraciones externas el límite de WIZDOM.
10. Actualizar checklist de desarrollo.

## 5. Secciones a crear o modificar en el PRP principal

## 5.1 `docs/PRP.md` - Tabla de contenidos

### Acción

- Modificar entradas existentes para reflejar nuevos apartados internos en Roles, RF, BD y Flujos.

### Redacción propuesta resumida

- Añadir referencias internas para:
  - Rol Administrador.
  - RF de auditoría.
  - RF de configuración de jerarquías de aprobación.
  - RF de gestión de delegados/subaprobadores.
  - Nuevas estructuras y catálogos organizacionales.

## 5.2 `docs/PRP.md` - Sección 5 Roles del Sistema

### Acción

- Modificar la matriz de roles para formalizar `ROL_ADMIN`.
- Ajustar descripciones de `ROL_JEFE` y `ROL_RRHH` para evitar superposición de privilegios.

### Redacción propuesta resumida

- Administrador (`ROL_ADMIN`): administra catálogos, configuraciones de jerarquía de aprobación, asignación de delegados/subaprobadores, habilitación e inhabilitación de estructuras organizacionales de soporte y consulta exclusiva de auditoría del sistema.
- Jefatura (`ROL_JEFE`): aprueba o rechaza solicitudes de acuerdo con la jerarquía de aprobación configurada o delegación vigente, no únicamente por subordinación directa.
- RRHH (`ROL_RRHH`): mantiene acceso de consulta global a justificaciones, sin acceso al módulo administrativo ni a la bitácora de auditoría.

### Nota clave a insertar

- El rol Administrador es independiente del rol funcional de RRHH o Jefatura y su visibilidad está restringida a capacidades administrativas y de auditoría.

## 5.3 `docs/PRP.md` - Sección 6 Requerimientos Funcionales

### Acción

- Modificar RF-03.
- Crear RF-07, RF-08, RF-09 y RF-10.

### RF-03 Aprobación / Rechazo por Jefatura

#### Cambio requerido

- Sustituir la regla de "subordinados directos" por "alcance definido por jerarquía de aprobación activa y delegaciones vigentes".

#### Redacción propuesta resumida

- Un usuario con permisos de aprobación podrá visualizar y resolver únicamente las solicitudes que le correspondan según la configuración jerárquica activa del sistema.
- La resolución debe registrar aprobador efectivo, rol con el que actúa, fecha/hora y resultado.
- El flujo no debe quedar bloqueado si existe delegación activa válida.

### RF-07 Auditoría persistente del sistema

#### Crear nueva sección

- El sistema debe registrar de forma persistente en base de datos todos los eventos relevantes de operación y administración.
- Cada registro debe almacenar al menos: fecha/hora, identificador del usuario, nombre del usuario, rol, tipo de evento catalogado, descripción catalogada o normalizada, resultado del evento y referencia funcional cuando aplique.
- Solo el rol Administrador podrá consultar la auditoría.
- El módulo de auditoría debe permitir visualización paginada, filtros por fecha, usuario, rol, tipo de evento y resultado, además de descarga de resultados filtrados.

### RF-08 Configuración de jerarquía de aprobaciones

#### Crear nueva sección

- El sistema debe permitir definir una jerarquía de aprobación configurable, desacoplada de la estructura limitada proveniente de las bases actuales.
- Debe soportar relaciones horizontales y verticales.
- Debe permitir que un aprobador superior visualice y resuelva solicitudes de unidades o subunidades bajo su alcance configurado.
- La configuración será administrada solo por Administrador.

### RF-09 Gestión de delegados y subaprobadores

#### Crear nueva sección

- El sistema debe permitir registrar delegados o subaprobadores para cubrir ausencias u otros escenarios operativos.
- Los delegados tendrán exactamente el mismo alcance de aprobación de la jefatura delegante durante la vigencia habilitada.
- El alta, baja, habilitación y deshabilitación de delegaciones solo podrá realizarla el rol Administrador y únicamente como gestión administrativa fuera del flujo ordinario de aprobación.
- La existencia de delegaciones no debe interrumpir ni bloquear la continuidad del flujo.

### RF-10 Catálogos y estructuras de soporte organizacional

#### Crear nueva sección

- El sistema debe contar con catálogos y estructuras propias para soportar la lógica organizacional que no existe en WIZDOM ni en las bases históricas.
- Deben existir estados de activo/inactivo para entidades configurables relevantes.
- Deben contemplarse estructuras para jerarquías, niveles, delegaciones, tipos de evento de auditoría y resultados de auditoría.

## 5.4 `docs/PRP.md` - Sección 7 Requerimientos No Funcionales

### Acción

- Modificar RNF-01 y agregar nuevos RNF.

### Redacción propuesta resumida

- RNF-01 Seguridad: acceso basado en roles incluyendo `ROL_ADMIN`; segregación estricta entre operaciones administrativas, operativas y de consulta; auditoría visible solo a Administrador.
- Nuevo RNF de Trazabilidad: toda acción crítica y administrativa debe dejar evidencia persistente, consultable y exportable.
- Nuevo RNF de Continuidad Operativa: la lógica de delegaciones debe evitar bloqueo del flujo de aprobación ante ausencias.
- Nuevo RNF de Configurabilidad: la jerarquía de aprobación debe poder ajustarse sin depender de cambios en fuentes externas ni despliegues de código.

## 5.5 `docs/PRP.md` - Sección 8 Arquitectura del Software

### Acción

- Ampliar la descripción de la capa de lógica de negocio y de acceso a datos.

### Redacción propuesta resumida

- Incorporar un módulo administrativo con servicios de:
  - administración de jerarquías de aprobación;
  - administración de delegaciones;
  - administración de catálogos organizacionales;
  - consulta de auditoría.
- Incorporar un servicio transversal de auditoría persistente.
- Aclarar que la estructura organizacional importada de WIZDOM es un insumo de referencia, pero la matriz efectiva de aprobación reside en `INTEGRA_CNP`.

## 5.6 `docs/PRP.md` - Sección 9 Modelo de Base de Datos

### Acción

- Rediseñar el modelo conceptual para reflejar entidades nuevas.
- No es necesario fijar todavía el DDL definitivo dentro del PRP, pero sí el inventario mínimo de tablas/catálogos y sus responsabilidades.

### Estructuras nuevas propuestas

- `Roles`
  - agregar semilla de Administrador.
- `Cat_EstadosRegistro`
  - catálogo genérico para activo/inactivo.
- `Cat_TiposEventoAuditoria`
  - catálogo de eventos auditables.
- `Cat_ResultadosAuditoria`
  - éxito, fallo u otros estados formalizados.
- `Auditoria_Eventos`
  - bitácora persistente de acciones del sistema.
- `Estructuras_Organizacionales`
  - unidades o nodos configurables requeridos por la lógica interna.
- `Jerarquias_Aprobacion`
  - relaciones configurables entre aprobador, alcance, nivel, vigencia y estado.
- `Delegaciones_Aprobacion`
  - delegante, delegado, vigencia, estado, motivo y trazabilidad administrativa.

### Ajustes a tablas existentes

- `Usuarios`
  - conservar jefatura directa de origen como dato de referencia, pero no como única regla de autorización.
- `Justificaciones_Encabezado`
  - considerar campos adicionales de resolución y trazabilidad del aprobador efectivo si el modelo los requiere.
- `Estados`
  - evaluar si el catálogo actual de proceso debe ampliarse para contemplar estados más ricos del flujo, sin mezclarlo con activo/inactivo de entidades administrativas.

### Redacción propuesta resumida

- El sistema almacenará en `INTEGRA_CNP` sus propias estructuras de aprobación y delegación, independientes de las fuentes externas de consulta.
- La auditoría de sistema se implementará como un subsistema persistente con catálogos y registros históricos consultables solo por Administrador.

## 5.7 `docs/PRP.md` - Sección 10 Lógica de Negocio y Flujos

### Acción

- Reescribir el flujo principal y las reglas RN-03, RN-06 y relacionadas.
- Añadir flujo administrativo.

### Cambios de reglas propuestos

- Reemplazar "Solo `ROL_JEFE` puede cambiar el estado" por una regla del tipo: solo usuarios con autorización activa de aprobación, ya sea por jerarquía configurada o delegación vigente, pueden resolver una boleta.
- Reemplazar "Jefatura solo ve justificaciones donde `Usuarios.JefaturaID = jefaturaActual.UsuarioID`" por una regla basada en la matriz de alcance configurada en las tablas de jerarquía.
- Agregar una regla que indique que toda operación administrativa y toda resolución de boleta genera evento de auditoría persistente.
- Agregar una regla que indique que las delegaciones solo pueden ser creadas, modificadas o desactivadas por Administrador.

### Flujo resumido propuesto

1. Funcionario crea boleta.
2. Sistema determina ruta de aprobación según configuración jerárquica activa.
3. Si existe delegación vigente, el delegado puede actuar con el mismo alcance autorizado.
4. Aprobador efectivo resuelve la boleta.
5. Toda acción relevante genera evento de auditoría.
6. Administrador puede consultar auditoría y administrar jerarquías/delegaciones.

## 5.8 `docs/PRP.md` - Sección 11 Integraciones Externas

### Acción

- Aclarar límites de WIZDOM y SIFCNP frente al nuevo modelo.

### Redacción propuesta resumida

- WIZDOM seguirá siendo fuente de datos maestros de personal y jefatura directa, pero no de la lógica final de aprobaciones jerárquicas ni de delegaciones.
- La lógica de aprobaciones y delegaciones será propia de `INTEGRA_CNP`.
- SIFCNP permanece solo como fuente histórica sin participación en la jerarquía ni en auditoría operativa del nuevo sistema.

## 5.9 `docs/PRP.md` - Sección 13 Checklist de progreso

### Acción

- Añadir tareas explícitas para el módulo administrativo y la auditoría.

### Redacción propuesta resumida

- Crear catálogos de auditoría.
- Crear tablas de jerarquía y delegaciones.
- Sembrar rol Administrador.
- Implementar endpoints/pantallas administrativas.
- Implementar dashboard de auditoría con filtros y exportación.
- Agregar pruebas de autorización por jerarquía, delegación y restricción de auditoría.

## 6. Secciones equivalentes a actualizar en el PRP secundario

## 6.1 `PRP_Justificacion_Marcas.md`

### Acción

- Replicar el mismo contenido sustantivo de `docs/PRP.md` en las secciones equivalentes.
- Mantener consistencia de términos, códigos de rol, reglas de negocio y modelo conceptual de BD.

### Criterio de consistencia

- No deben quedar diferencias materiales en:
  - universo de roles;
  - definición del flujo de aprobación;
  - alcance del rol Administrador;
  - tablas/catálogos principales del modelo;
  - reglas RN afectadas;
  - definición de auditoría persistente.

## 7. Archivos exactos a editar

### 7.1 Edición obligatoria para actualizar la línea base PRP

- `docs/PRP.md`
- `PRP_Justificacion_Marcas.md`

### 7.2 Edición obligatoria para consistencia técnico-documental inmediata

- `docs/db/001_init_integra_cnp.sql`

### 7.3 Edición recomendada en la siguiente pasada documental

- `README.md`
- `docs/manual-tecnico.md`
- `docs/Guia_Implementacion_Dev_Prod.md`
- `docs/SubAgent docs/verificacion-roles-acceso.md`

## 8. Propuesta mínima para `docs/db/001_init_integra_cnp.sql`

Aunque el objetivo principal es documental, este script debe quedar alineado cuando se actualice el PRP porque hoy representa la materialización inicial del modelo.

### Ajustes mínimos esperados

- Agregar semilla de rol `Administrador`.
- Incorporar tablas/catálogos nuevos para:
  - auditoría persistente;
  - tipos y resultados de auditoría;
  - jerarquías de aprobación;
  - delegaciones;
  - estados activo/inactivo para entidades configurables.
- Agregar índices básicos para consultas administrativas y dashboard de auditoría.

## 9. Criterio para decidir la fuente de verdad

Se recomienda formalizar de inmediato la siguiente política editorial:

- `docs/PRP.md` = fuente primaria de verdad del PRP.
- `PRP_Justificacion_Marcas.md` = copia controlada o versión espejo resumida, mantenida consistente mientras existan dependencias heredadas.

Si no se define esta jerarquía documental, el repositorio seguirá generando desalineación entre requerimientos, scripts y documentos técnicos.

## 10. Resultado esperado después de la actualización

El PRP actualizado debe dejar explícito que:

- existe un rol Administrador formal y no implícito;
- la auditoría es funcional, persistente, filtrable y exportable;
- el modelo de aprobación ya no depende solamente de `JefaturaID`;
- las delegaciones son una capacidad administrativa controlada;
- `INTEGRA_CNP` incorpora estructuras propias para representar la lógica organizacional faltante en las fuentes actuales.