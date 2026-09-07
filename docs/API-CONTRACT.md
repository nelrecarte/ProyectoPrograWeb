# Contrato de API — ApagónYa

Backend en `http://localhost:5099`. Documentación navegable en `http://localhost:5099/scalar/v1`.

Este archivo es el contrato congelado contra el que trabaja el frontend. Si algo aquí
tiene que cambiar, se avisa en el grupo antes de cambiarlo.

> Este repositorio contiene **dos entregables**: el proyecto ApagónYa (todo lo de este
> documento) y la actividad semanal de clase (`/api/Note`, `Note.cs`, `NoteService`).
> No se mezclan.

---

## Puesta en marcha

```bash
cd ProyectoPrograWeb
dotnet restore

# La Web API Key de Firebase y la clave de setup NO van en appsettings.json
dotnet user-secrets init
dotnet user-secrets set "Firebase:ApiKey" "<Web API Key de la consola de Firebase>"
dotnet user-secrets set "ApagonYa:SetupKey" "<cualquier clave que acuerde el equipo>"

dotnet run
```

`firebase-key.json` va dentro de `ProyectoPrograWeb/` y no se sube al repositorio.

### Primer arranque, una sola vez

```bash
# 1. Crear el catálogo de zonas
curl -X POST http://localhost:5099/api/setup/seed-zones -H "X-Setup-Key: <clave>"

# 2. Registrarse normalmente desde /api/auth/register, y luego convertirse en admin
curl -X POST "http://localhost:5099/api/setup/promote-admin?email=tu@correo.com" \
     -H "X-Setup-Key: <clave>"
```

Después de `promote-admin` hay que **volver a iniciar sesión**: el rol viaja dentro del
token y el token viejo todavía dice "Ciudadano".

---

## Autenticación

Los tokens los emite Firebase Authentication. El backend solo los valida.

Todas las rutas excepto `/api/auth/*` y `/api/setup/*` piden:

```
Authorization: Bearer {idToken}
```

El rol viaja como custom claim `role` dentro del token. Los tres valores posibles son
`Administrador`, `Tecnico` y `Ciudadano`.

| Método | Ruta | Rol | Qué hace |
|---|---|---|---|
| POST | `/api/auth/register` | público | Registra un ciudadano. Devuelve `idToken`, `localId`, `email`, `role`. |
| POST | `/api/auth/login` | público | Inicia sesión. Mismo cuerpo de respuesta. |

**`RegisterDto`**: `email`, `password` (mín. 6), `displayName`, `username`, `phoneNumber`,
`birthDate`, `country`, `bio` (opcional), `zoneId` (opcional).

**`LoginDto`**: `email`, `password`.

---

## Usuarios

| Método | Ruta | Rol | Qué hace |
|---|---|---|---|
| GET | `/api/users/me` | cualquiera | Perfil del usuario autenticado, **con su rol**. Es la primera llamada después del login. |
| PUT | `/api/users/me` | cualquiera | Actualiza `displayName`, `phoneNumber`, `country`, `bio`, `zoneId`. |
| GET | `/api/users?role=Ciudadano` | Administrador | Lista de usuarios, filtrable por rol. |
| GET | `/api/users/{id}` | Administrador | Un usuario. |
| PUT | `/api/users/{id}/role` | Administrador | Cambia el rol. Cuerpo: `{ "role": "Tecnico" }`. |

---

## Zonas

| Método | Ruta | Rol | Qué hace |
|---|---|---|---|
| GET | `/api/zones?onlyActive=true` | cualquiera | Lista de zonas. **Cada zona trae `activeReportId`** si ya tiene un corte abierto. |
| GET | `/api/zones/{id}` | cualquiera | Una zona. |
| POST | `/api/zones` | Administrador | Crea. Cuerpo: `name`, `sector`, `description`. |
| PUT | `/api/zones/{id}` | Administrador | Actualiza. Agrega `isActive`. |
| DELETE | `/api/zones/{id}` | Administrador | Baja lógica: la zona conserva su historial. |

`activeReportId` sirve para que el formulario de reporte avise **antes** de mandar la
petición que la zona ya tiene un corte abierto.

---

## Reportes

| Método | Ruta | Rol | Qué hace |
|---|---|---|---|
| GET | `/api/reports` | cualquiera | Lista con filtros. Un técnico solo ve su zona. |
| GET | `/api/reports/mine` | cualquiera | Reportes creados por el usuario. |
| GET | `/api/reports/zone/{zoneId}` | cualquiera | Reportes de una zona. `?isActive=true` para los abiertos. |
| GET | `/api/reports/{id}` | cualquiera | Detalle, incluye la resolución si ya existe. |
| POST | `/api/reports` | cualquiera | **Crea un reporte.** Escenario 1. |
| POST | `/api/reports/{id}/confirm` | cualquiera | **"A mí también".** Escenario 3. |
| POST | `/api/reports/{id}/assign` | Administrador | Asigna técnico. Cuerpo: `{ "technicianId": "..." }`. |
| POST | `/api/reports/{id}/accept` | Tecnico | El técnico toma un reporte de su zona. |
| PATCH | `/api/reports/{id}/status` | Tecnico, Administrador | `{ "status": "en_verificacion" \| "confirmado" }`. |
| POST | `/api/reports/{id}/resolution` | Tecnico | **Cierra el corte.** Escenario 4. |
| GET | `/api/reports/{id}/resolution` | cualquiera | Resolución del reporte. |

**Filtros de `GET /api/reports`**: `zoneId`, `status`, `technicianId`, `isActive`, `from`, `to`.

**`CreateReportDto`**: `zoneId`, `address` (mín. 5), `startedAt` (opcional, por defecto ahora),
`evidenceUrl` (opcional).

**`CreateResolutionDto`**: `cause`, `detail` (mín. 10), `estimatedMinutes`, `restoredAt` (opcional).

### Estados

`nuevo` → `en_verificacion` → `confirmado` → `resuelto`

El salto a `confirmado` es **automático** al alcanzar el umbral de confirmaciones
(`ApagonYa:ConfirmationThreshold`, por defecto 1). El salto a `resuelto` solo ocurre al
registrar una resolución.

### Campos útiles de `ReportDto`

- `confirmedByMe` — `true` si el usuario ya confirmó. Sirve para desactivar el botón.
- `confirmationCount` — cuántos vecinos confirmaron.
- `isActive` — `true` mientras el corte no esté resuelto.
- `resolution` — objeto completo cuando el reporte ya está cerrado.

---

## Técnicos

| Método | Ruta | Rol | Qué hace |
|---|---|---|---|
| GET | `/api/technicians` | Administrador | Lista **con `activeReportCount`**, la carga de cada uno. |
| GET | `/api/technicians/me` | Tecnico | Datos del técnico que está usando la app. |
| GET | `/api/technicians/{id}` | Administrador | Un técnico. |
| POST | `/api/technicians` | Administrador | Registra. **También promueve al usuario al rol Tecnico.** |
| PUT | `/api/technicians/{id}` | Administrador | Actualiza nombre, zona y disponibilidad. |
| PATCH | `/api/technicians/{id}/deactivate` | Administrador | Baja lógica, conserva el historial. |
| PATCH | `/api/technicians/{id}/activate` | Administrador | Reactiva. |

**`CreateTechnicianDto`**: `userId` (UID de Firebase de un usuario **ya registrado**),
`fullName`, `zoneId`, `isAvailable`.

---

## Estadísticas

| Método | Ruta | Rol |
|---|---|---|
| GET | `/api/statistics?zoneId=&from=&to=` | Administrador |

Devuelve en una sola llamada todo lo que necesita el dashboard:

```jsonc
{
  "totalReports": 12,
  "activeReports": 3,
  "resolvedReports": 8,
  "unverifiedReports": 1,
  "averageResolutionMinutes": 143.5,

  // gráfico de barras
  "reportsByZone": [
    { "zoneId": "...", "zoneName": "Colonia Kennedy", "total": 5,
      "resolved": 4, "resolvedPercentage": 80.0, "averageResolutionMinutes": 120.0 }
  ],

  // gráfico circular — siempre trae los cuatro estados, aunque vayan en cero
  "reportsByStatus": [
    { "status": "nuevo", "total": 1 },
    { "status": "en_verificacion", "total": 2 },
    { "status": "confirmado", "total": 1 },
    { "status": "resuelto", "total": 8 }
  ],

  "technicianPerformance": [
    { "technicianId": "...", "technicianName": "Melvin",
      "assigned": 6, "resolved": 5, "resolvedPercentage": 83.33,
      "averageResolutionMinutes": 98.4 }
  ]
}
```

---

## Errores

**Todos** los errores tienen la misma forma:

```json
{ "error": "mensaje para mostrarle al usuario", "code": "codigo_estable", "data": null }
```

El frontend muestra `error` y reacciona según `code`. Nunca hay que leer el mensaje
para decidir qué hacer.

| HTTP | `code` | Cuándo |
|---|---|---|
| 400 | `peticion_invalida`, `hora_invalida`, `estado_invalido`, `rol_invalido` | Datos malos |
| 400 | `error_de_firebase` | Correo repetido, contraseña débil, credenciales malas |
| 403 | `no_asignado` | No es el técnico asignado a ese reporte |
| 403 | `fuera_de_zona` | El reporte no es de su zona |
| 403 | `tecnico_no_registrado` | El usuario tiene rol Tecnico pero no tiene ficha |
| 404 | `zona_no_encontrada`, `reporte_no_encontrado`, `usuario_no_encontrado` | No existe |
| 409 | **`reporte_duplicado`** | La zona ya tiene un corte abierto |
| 409 | `confirmacion_duplicada` | Ya confirmó ese reporte |
| 409 | `autor_del_reporte` | Intenta confirmar su propio reporte |
| 409 | `resolucion_inmutable` | Ese reporte ya tiene resolución |
| 409 | `reporte_resuelto` | El corte ya está cerrado |

### El caso importante: `reporte_duplicado` (escenario 2)

`POST /api/reports` responde **409** cuando la zona ya tiene un corte abierto, y el
campo `data` trae lo necesario para ofrecer confirmarlo en vez de crear otro:

```json
{
  "error": "La zona 'Colonia Kennedy' ya tiene un reporte de corte abierto. En lugar de crear otro, confirma el que ya existe.",
  "code": "reporte_duplicado",
  "data": {
    "existingReportId": "abc123",
    "zoneId": "z1",
    "zoneName": "Colonia Kennedy",
    "status": "nuevo",
    "confirmationCount": 0,
    "alreadyConfirmedByMe": false
  }
}
```

Flujo esperado en Angular: atrapar el 409, leer `data.existingReportId`, y mostrar
"ya hay un reporte en tu zona, ¿te afecta a vos también?" con un botón que llame a
`POST /api/reports/{existingReportId}/confirm`.

---

## Las tres reglas que se resuelven con transacciones

No son validaciones normales: son transacciones de Firestore, para que aguanten
dos personas actuando al mismo tiempo.

1. **Un solo reporte activo por zona.** `ReportService.CreateAsync` busca dentro de la
   transacción y rechaza antes de escribir.
2. **Umbral de confirmaciones.** `ReportService.ConfirmAsync` lee reporte y confirmación,
   suma y cambia el estado en la misma transacción. El id del documento de confirmación
   es `{reportId}_{userId}`, así que Firestore mismo impide confirmar dos veces.
3. **Resolución inmutable.** `ResolutionService.CreateAsync` usa el id del reporte como id
   de la resolución y verifica que no exista antes de escribir. Una resolución registrada
   no se modifica nunca.

---

## Colecciones en Firestore

| Colección | Contenido |
|---|---|
| `users` | Perfiles. El id del documento es el UID de Firebase. |
| `zones` | Catálogo de zonas. |
| `reports` | Reportes de corte. |
| `confirmations` | Una por (reporte, usuario). Id: `{reportId}_{userId}`. |
| `resolutions` | Una por reporte. Id: el id del reporte. |
| `technicians` | Fichas de técnicos. |
| `notes` | De la actividad semanal, no de ApagónYa. |

Si Firestore devuelve `FAILED_PRECONDITION` pidiendo un índice, el mensaje trae un
enlace que lo crea con un clic. Tarda unos minutos en construirse.
