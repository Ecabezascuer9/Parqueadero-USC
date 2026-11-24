# Parqueadero USC

Sistema de control de aforo para el parqueadero universitario. La solución usa ASP.NET Core MVC, ASP.NET Core Identity, Entity Framework Core con SQLite y una aplicación de escritorio WPF. La consulta pública se actualiza cada tres segundos.

## Requisitos

- .NET SDK 10
- Windows para ejecutar la aplicación de escritorio WPF

## Ejecutar la aplicación web

Desde la raíz del proyecto:

```powershell
dotnet run --project Parqueadero.Web
```

La base de datos se crea y prepara automáticamente en `%LOCALAPPDATA%\ParqueaderoUSC\parqueadero.db`. Para usar otra ubicación, define `PARQUEADERO_DB` antes de iniciar la web y la aplicación de escritorio. Ambos clientes comparten la misma base de datos.

La primera ejecución crea los roles, cuentas de demostración, tarjetas de muestra y una capacidad inicial de 60 cupos.

## Cuentas de demostración

| Perfil | Usuario | Contraseña |
|---|---|---|
| Administrador | `admin@usc.edu.co` | `Admin123!` |
| Empleado | `empleado@usc.edu.co` | `Empleado123!` |
| Estudiante | `estudiante@usc.edu.co` | `Estudiante123!` |
| Docente | `docente@usc.edu.co` | `Docente123!` |
| Egresado | `egresado@usc.edu.co` | `Egresado123!` |

Cambiar estas contraseñas de demostración antes de publicar el sistema.

## Permisos

- **Administrador:** gestiona tarjetas y usuarios, consulta reportes y registra movimientos.
- El administrador también ajusta la capacidad máxima; no puede reducirla por debajo de los vehículos actualmente dentro ni de las reservas que coinciden en su horario de mayor demanda.
- **Empleado:** registra entradas y salidas y consulta la ocupación.
- **Estudiante y docente:** consultan el aforo público.
- **Estudiante:** consulta el Bloque 6, atrás de las canchas y la Arena USC.
- **Docente:** consulta el Bloque 8 y la Arena USC.
- **Egresado:** puede registrarse con correo personal y correo institucional `@usc.edu.co`, y consulta la plazoleta y la Arena USC.
- La Arena USC puede abrirse o cerrarse desde la configuración administrativa.
- **Estudiante:** además solicita cupo con fecha/hora deseada y duración estimada; la solicitud asigna una tarjeta física y genera un ticket. No puede definir ni editar la entrada o salida reales.
- La pantalla pública no requiere iniciar sesión.

## Operación

El inventario inicial incluye tarjetas `USC-001` a `USC-010`; `USC-010` está inactiva. Las reservas confirmadas no pueden solaparse para el mismo estudiante, ni reutilizan una tarjeta asignada a otra reserva en ese intervalo. El estudiante puede cancelar una solicitud antes de su hora programada.

El empleado o administrador registra entradas y salidas reales al presentar la tarjeta, desde la web o el panel WPF. El estudiante no introduce esas horas; el sistema guarda automáticamente cada movimiento. Una tarjeta no puede mantener más de una entrada activa y se rechazan entradas cuando se agotan los cupos. El historial conserva entrada, salida y duración.

La solución se organiza en `Parqueadero.Core` (dominio y reglas), `Parqueadero.Data` (EF Core, SQLite, Identity, migraciones y repositorios), `Parqueadero.Web` (MVC) y `Parqueadero.Desktop` (WPF). Las migraciones se aplican automáticamente al iniciar la web o el panel de escritorio.

Las entradas activas se consultan desde SQLite en cada operación; un índice único filtrado impide duplicar tarjetas aun si web y escritorio intentan registrar al mismo tiempo. Al solicitar una reserva para una llegada dentro de la próxima hora, el sistema suma los vehículos que siguen dentro y las reservas confirmadas que se cruzan con ese horario; cuando esa demanda alcanza la capacidad, rechaza la solicitud e informa que se agotaron los cupos. Las reservas cuya tarjeta ya registró entrada no se cuentan dos veces. Para búsquedas dentro de la capacidad acotada del parqueadero se usan `HashSet` (tarjetas activas) y `Dictionary` (registros activos por código); el historial y los reportes se procesan con listas y LINQ. La web local sirve Bootstrap, el logo USC proporcionado y las fuentes Arimo/Playfair Display incluidas con sus licencias OFL.

## Compilar

```powershell
dotnet build ParqueaderoUSC.slnx
```
