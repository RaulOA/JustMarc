# Ajuste de fechas reales sin vacios notorios

Fecha de analisis: 28/04/2026
Fuente: docs/Cronograma_Justificacion_Marcas.csv
Alcance: T001-T038

## 1. Supuestos aplicados

1. Solo se analizaron y se proponen cambios sobre las columnas Fecha Inicio Real y Fecha Fin Real.
2. No se modifica Fecha Entrega Real ni ninguna columna baseline.
3. La referencia del usuario a "entre T001 y T038" se interpreta como "dentro del tramo T001-T038". El hueco reportado de 22/01/2026 a 08/04/2026 corresponde en realidad a la transicion T020 -> T021, no a una relacion directa T001 -> T038.
4. Cuando un comentario contiene una proyeccion explicita de fin o de inicio por dependencia, esa proyeccion tiene prioridad para evitar una reescritura artificial del tramo abril-julio.
5. Se privilegia el cambio minimo que elimine vacios grandes no justificados sin romper la secuencia explicitamente proyectada en T021-T038.
6. Un desfase corto por fin de semana no se considera hueco notorio. Por eso no se corrigen pausas de 1 o 2 dias calendario cuando la secuencia general ya es continua.

## 2. Validacion del hueco reportado

Hallazgo principal validado en el archivo:

- T020 tiene Fecha Fin Real = 22/01/2026.
- T021 tiene Fecha Inicio Real = 08/04/2026.
- La diferencia es de 76 dias calendario.

Interpretacion semantica mas probable:

- El usuario no se equivoco en las fechas, sino en los IDs de referencia: el hueco real esta entre T020 y T021.
- El propio CSV muestra que desde T021 en adelante existe una cadena completa de fechas reales y comentarios con proyecciones explicitas entre abril y julio de 2026.
- Por eso, la correccion menos invasiva no es mover todo el bloque T021-T038 hacia enero-marzo, sino reinterpretar T021 como una tarea que comenzo al terminar T020 y permanecio abierta hasta su fin proyectado del 15/04/2026.

## 3. Huecos notorios detectados

### Hueco 1: T020 -> T021

- Fin previo: T020 / 22/01/2026
- Inicio siguiente: T021 / 08/04/2026
- Magnitud: 76 dias calendario
- Evaluacion: hueco notorio e injustificado usando solo fechas reales. Ningun comentario explica una pausa formal de ese tamano; el comentario de T021 solo confirma que la tarea estaba en desarrollo y con fin proyectado al 15/04/2026.

### No se detectan otros huecos notorios

- De T021 en adelante la secuencia queda encadenada por comentarios de dependencia y fechas proyectadas consecutivas.
- Antes de T021, el cronograma real avanza sin vacios grandes; solo aparecen saltos normales de calendario entre tareas consecutivas.
- Existe un solape menor entre T023 y T024: T023 termina el 25/04/2026 y T024 inicia el 24/04/2026. No es un hueco y no se corrige en esta propuesta porque T024 ya figura En Progreso con avance real, y cambiarla forzaria inconsistencias mayores con su estado/comentario.

## 4. Redistribucion propuesta de fechas reales

### T021

- Fecha Inicio Real actual: 08/04/2026
- Fecha Fin Real actual: 15/04/2026
- Fecha Inicio Real propuesta: 23/01/2026
- Fecha Fin Real propuesta: 15/04/2026

Justificacion:

- T021 depende de T020, cuyo fin real es 22/01/2026.
- Cambiar solo el inicio real de T021 elimina el vacio de 76 dias con un ajuste minimo de una sola celda.
- Se preserva el fin real del 15/04/2026 porque esta respaldado explicitamente por el comentario: "proyeccion fin 15-04-26".
- Tambien se preserva toda la cadena T022-T038, ya que sus comentarios dependen de ese hito del 15/04/2026 y luego encadenan sus propios inicios y fines proyectados.
- Esta lectura es compatible con el estado En Progreso y con 70% de avance real en T021: la tarea pudo iniciar en enero, extenderse por atraso operativo y continuar abierta hasta abril.

## 5. Nuevas fechas reales propuestas por tarea afectada

| Tarea | Fecha Inicio Real actual | Fecha Fin Real actual | Fecha Inicio Real propuesta | Fecha Fin Real propuesta |
|---|---|---|---|---|
| T021 | 08/04/2026 | 15/04/2026 | 23/01/2026 | 15/04/2026 |

## 6. Lista exacta de celdas a editar

Editar solo esta celda en el CSV:

- Fila T021, columna Fecha Inicio Real: cambiar 08/04/2026 por 23/01/2026.

No editar:

- T021 / Fecha Fin Real
- Cualquier celda de Fecha Entrega Real
- Cualquier celda baseline
- T022-T038, porque sus fechas reales actuales ya estan alineadas con comentarios de dependencia y proyecciones explicitas

## 7. Resultado esperado de la correccion

- Se elimina el unico vacio grande e injustificado de la secuencia real.
- La cronologia T020 -> T021 -> T022 ... -> T038 queda continua sin reescribir el bloque abril-julio.
- La propuesta mantiene la coherencia con los comentarios operativos que si contienen proyecciones explicitas.