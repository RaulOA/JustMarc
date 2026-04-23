# Especificacion de Reescritura de Manuales de Usuario para Publico Final

## Proposito de esta especificacion
Este documento define como reescribir los manuales de usuario de Funcionario, Jefatura y RRHH para personas sin conocimientos de informatica. La reescritura debe conservar con fidelidad lo que el sistema realmente permite hacer hoy, pero explicandolo con un lenguaje cercano, humano y didactico, sin tecnicismos ni estilo mecanico. El resultado esperado son manuales que acompanen a cada persona en su jornada real de trabajo, desde que ingresa al sistema hasta que termina sus tareas y cierra sesion.

## Lineamientos editoriales obligatorios
La redaccion debe sentirse como una guia conversada y clara, como si una persona experta acompanara a otra paso a paso durante su trabajo diario. Cada explicacion debe estar escrita en texto corrido, con transiciones naturales entre una accion y la siguiente, evitando por completo el formato de vinetas, listas numeradas y bloques de instrucciones cortadas. Tambien debe evitarse cualquier termino tecnico propio de desarrollo de software o integracion de sistemas. No se deben incluir referencias a API, endpoint, headers, codigos HTTP, backend, validaciones tecnicas internas o conceptos similares. Cuando sea necesario explicar un error o una situacion inesperada, se debe hacer en lenguaje cotidiano, explicando que significa para la persona usuaria y que puede intentar despues.

Cada manual debe tener una voz calmada, empatica y orientada a reducir ansiedad operativa. Se debe priorizar la claridad antes que la brevedad. Las frases deben ser concretas, pero lo suficientemente amplias para que una persona que nunca uso la plataforma pueda orientarse sin apoyo externo.

## Fidelidad funcional que debe respetarse
La reescritura no puede inventar pantallas, botones, estados o permisos que no existan en la version actual. Para el rol Funcionario, el contenido debe mantenerse centrado en ingresar, registrar una justificacion con motivo general y al menos una linea de detalle, y consultar el historial propio. Para el rol Jefatura, el contenido debe mantenerse en revisar pendientes de subordinados directos, entrar al detalle y resolver aprobando o rechazando. Para el rol RRHH, el contenido debe mantenerse en consultar de forma global, aplicar filtros, ajustar busquedas y limpiar filtros para nuevas consultas. En todos los casos se debe respetar la logica actual de estados y alcances por rol, explicada de forma no tecnica.

## Estructura narrativa propuesta para manual de Funcionario
El manual de Funcionario debe iniciar con una bienvenida breve que explique para que sirve el sistema en el dia a dia de una persona funcionaria. Despues debe guiar el inicio de sesion con una descripcion amable de lo que la persona ve al entrar y como confirmar que esta en su espacio correcto. A continuacion debe desarrollar el flujo principal de la jornada: preparar la justificacion, escribir el motivo general con orientaciones practicas, agregar cada detalle con su tipo y fecha, revisar con calma lo cargado y finalmente registrar la boleta. Luego debe continuar con el seguimiento natural del trabajo, explicando como consultar el historial y como interpretar si una solicitud sigue pendiente o ya fue resuelta.

La parte final del manual de Funcionario debe cubrir situaciones habituales de confusion con un tono tranquilizador, por ejemplo cuando falta informacion, cuando no aparece una boleta recien registrada o cuando no se puede ingresar. Debe cerrar con una seccion de cierre de jornada que indique como verificar que no quedan gestiones pendientes y como salir del sistema de forma ordenada.

## Estructura narrativa propuesta para manual de Jefatura
El manual de Jefatura debe abrir con una explicacion clara del objetivo de su rol: revisar y resolver solicitudes de su equipo directo con criterio y oportunidad. Luego debe describir el inicio de sesion y el ingreso al panel de pendientes como el primer paso de cada jornada, incluyendo como reconocer rapidamente que hay solicitudes por atender.

En el desarrollo principal debe narrar la revision de la bandeja de forma completa, explicando como identificar una solicitud, abrir su detalle, comprender la informacion disponible y tomar una decision. La secuencia de aprobacion y rechazo debe contarse como dos caminos posibles de una misma tarea, destacando que una vez resuelta deja de estar pendiente. El texto debe reforzar la importancia de revisar antes de decidir, sin introducir reglas tecnicas ni lenguaje de sistemas.

La parte de apoyo debe explicar que hacer cuando una solicitud no se puede resolver, cuando ya fue atendida por otra persona o cuando no aparecen pendientes esperados, siempre en lenguaje cotidiano y orientado a accion. El cierre de jornada debe incluir una verificacion final de bandeja y una salida responsable del sistema.

## Estructura narrativa propuesta para manual de RRHH
El manual de RRHH debe comenzar ubicando su tarea principal en terminos operativos: consultar el panorama general de justificaciones para seguimiento institucional. Debe continuar con el inicio de sesion y la llegada al panel global, explicando de forma sencilla como leer la vista general antes de aplicar filtros.

El cuerpo central debe narrar el proceso de consulta como una rutina practica: iniciar con una busqueda amplia, afinar por nombre o identificacion de persona, estado, compania y periodo de fechas, revisar resultados y ajustar cuando sea necesario. Debe explicarse que limpiar filtros permite volver a empezar una nueva consulta sin arrastrar condiciones anteriores. Tambien debe quedar claro, en lenguaje no tecnico, que este rol consulta informacion pero no resuelve solicitudes.

El tramo final debe abordar escenarios comunes como resultados vacios, filtros demasiado restrictivos o datos ingresados de forma inconsistente, siempre con indicaciones simples y humanas. El cierre de jornada debe orientar a dejar las consultas en orden y cerrar sesion.

## Longitud minima recomendada por manual
Para asegurar profundidad didactica real, el manual de Funcionario debe tener una extension minima recomendada de 1600 palabras. El manual de Jefatura debe tener una extension minima recomendada de 1700 palabras, considerando la necesidad de explicar la toma de decision con contexto. El manual de RRHH debe tener una extension minima recomendada de 1800 palabras por la amplitud de escenarios de consulta y combinacion de filtros. Estas longitudes son piso minimo y no limite maximo.

## Criterios de aceptacion
Se considerara aceptado el trabajo cuando cada manual pueda leerse de principio a fin como una guia continua, sin vinetas ni numeraciones, y con un tono cercano que resulte comprensible para personas sin formacion tecnica. Se considerara aceptado cuando el contenido cubra todo el recorrido de uso diario: ingreso, tareas habituales del rol, resolucion de situaciones comunes y cierre de jornada. Tambien se considerara aceptado cuando no aparezcan tecnicismos de desarrollo ni referencias internas de arquitectura.

Adicionalmente, la aceptacion exige que cada manual respete estrictamente las capacidades reales del rol en el sistema actual. Funcionario debe quedar limitado a crear justificaciones propias y revisar su historial. Jefatura debe quedar limitada a revisar y resolver pendientes de su equipo directo. RRHH debe quedar limitado a consulta global y uso de filtros sin acciones de aprobacion o rechazo. Si se detecta cualquier desvio funcional, la reescritura debe corregirse antes de publicarse.

Finalmente, se considerara aceptado cuando la lectura transmita acompanamiento practico y seguridad operativa, de manera que una persona usuaria pueda completar su jornada con autonomia basica solo con el manual, sin necesidad de interpretar lenguaje tecnico ni traducir instrucciones mecanicas.