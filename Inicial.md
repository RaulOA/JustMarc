# 📑 Instrucciones para Claude Sonnet

## 🎯 Objetivo
Generar un prototipo visual del **Sistema de Justificación de Marcas**:
- Pantalla inicial de **Login** (`index.html`).
- Validación básica con credenciales genéricas quemadas en código (ejemplo: usuario=`admin`, contraseña=`1234`).
- Si las credenciales son correctas → redirigir al **Dashboard**.
- El Dashboard debe tener una **barra superior fija** con pestañas para todas las pantallas del sistema.
- Cada pestaña muestra su vista independiente, sin mezclar contenidos.

---

## 📂 Estructura de archivos
- `index.html` → Pantalla de Login.
- `dashboard.html` → Pantalla principal con barra superior y pestañas.
- `style.css` → Estilos visuales globales.
- `app.js` → Lógica básica de navegación y validación de login.

---

## 🖥 Pantalla Inicial: Login (index.html)
**Montar en HTML + CSS + JS**
- Formulario centrado en pantalla:
  - Campo Usuario (input text).
  - Campo Contraseña (input password).
  - Botón "Ingresar".
- Validación básica en `app.js` contra credenciales quemadas.
- Si son correctas → redirigir a `dashboard.html`.

---

## 🖥 Pantalla Dashboard (dashboard.html)
**Montar en HTML + CSS**
- Barra superior fija con pestañas:
  - Panel Funcionario
  - Panel Jefatura
  - Panel RRHH
  - Consulta Histórica (SIFCNP)
- Cada pestaña abre una sección independiente (mostrar/ocultar con JS).
- Estilo visual consistente con `style.css`.

---

## 🖥 Panel Funcionario
**Montar en HTML + CSS**
- Formulario de creación de justificación:
  - Motivo General (textarea).
  - Tipo de Justificación (select).
  - Fecha de Marca (date).
  - Observación Detalle (textarea).
  - Botón "Registrar".
- Tabla de historial personal:
  - Columnas: ID, Motivo, Tipo, Fecha, Estado.

---

## 🖥 Panel Jefatura
**Montar en HTML + CSS**
- Tabla de solicitudes pendientes:
  - Columnas: Funcionario, Motivo, Tipo, Fecha, Estado, Acciones.
- Vista de detalle expandida:
  - Motivo General.
  - Lista de Detalles (tipo, fecha, observación).
  - Botones de acción (Aprobar/Rechazar).

---

## 🖥 Panel RRHH
**Montar en HTML + CSS**
- Filtros: Funcionario, Estado, Rango de fechas.
- Tabla global de justificaciones:
  - Columnas: Funcionario, Motivo, Tipo, Fecha, Estado.
- Botón "Descargar Reporte".

---

## 🖥 Consulta Histórica (SIFCNP)
**Montar en HTML + CSS**
- Campo búsqueda por funcionario.
- Campo rango de fechas.
- Tabla de resultados históricos:
  - Funcionario, Concepto, Fecha, Observación, Estado.

---

## 📌 Notas
- El **Login** es la pantalla inicial y no aparece en la barra superior.
- El **Dashboard** es la segunda pantalla, con pestañas para todas las vistas.
- No implementar lógica avanzada aún → solo estructura visual y navegación básica.
- Usuario genérico puede ver todas las pestañas.
