# Competitions And Tournaments — Copa Amistosa

App de "apuestas amistosas" basada en **predicciones** de partidos de torneos y
competencias. Un administrador configura ligas, equipos, torneos y partidos;
desde la app los usuarios predicen el marcador de cada partido y acumulan puntos:

- **3 puntos** si aciertan el marcador exacto.
- **1 punto** si aciertan solo el resultado (ganador o empate).

Este repositorio contiene el proyecto original modernizado y llevado a **.NET 10
(2026)**, con el backend como API + panel de administración y una nueva app
móvil en **.NET MAUI**.

> Proyecto inspirado en los tutoriales y mentoría de Juan Carlos Zuluaga
> (https://www.youtube.com/user/jzuluaga55).

## Estructura de la solución

| Proyecto | Descripción |
| --- | --- |
| `CompeTournament.Backend` | ASP.NET Core (.NET 10): panel MVC de administración + API REST con JWT. |
| `CompeTournament.Shared` | Contratos (DTOs) y lógica de puntuación compartidos entre backend y app. |
| `CompeTournament.Mobile.Core` | Lógica del cliente móvil sin dependencias de MAUI (cliente HTTP + ViewModels MVVM). |
| `CompeTournament.Mobile` | App .NET MAUI (Android/iOS/MacCatalyst/Windows). |
| `CompeTournament.Tests` | Pruebas de puntuación e integración de API y cliente (xUnit). |

## Novedades de la modernización

- Migración de ASP.NET Core 2.2 (fin de soporte) a **.NET 10 LTS**.
- **API REST** con autenticación JWT para la app móvil, además del panel MVC.
- Proveedor de base de datos configurable: **SQLite** por defecto para
  desarrollo/pruebas y **SQL Server** para producción.
- Patrones de seguridad: rate limiting en autenticación, cabeceras de seguridad,
  CORS restringido, bloqueo por intentos fallidos, política de contraseñas y
  resolución de la clave de firma desde configuración/secretos.
- Nueva app **.NET MAUI** con MVVM (CommunityToolkit.Mvvm) y un sistema visual
  cohesivo.
- Datos de ejemplo sembrados automáticamente y pruebas automatizadas.

## Requisitos

- SDK de **.NET 10**.
- Para compilar la app MAUI: `dotnet workload install maui`.

## Ejecutar el backend (SQLite, sin configuración externa)

```bash
cd CompeTournament.Backend
dotnet run
```

Al iniciar se crea y siembra una base SQLite local (`competournament.db`) con
estados, tipos, usuarios de ejemplo y un torneo demo. El documento OpenAPI queda
disponible en `/openapi/v1.json` en desarrollo.

### Usuarios de ejemplo

Se crean con la contraseña por defecto `Torneo2026` (configurable con
`Seed:DefaultPassword`):

- `sgrysoft@gmail.com` — Admin, miembro del torneo demo.
- `toreneo@gmail.com` — Admin.
- `elis@gmail.com`, `starling@gmail.com` — Usuarios.

## Configuración de secretos

Ningún secreto se versiona. En desarrollo usa User Secrets; en producción,
variables de entorno o un gestor de secretos. Ver [SECURITY.md](SECURITY.md).

```bash
cd CompeTournament.Backend
dotnet user-secrets set "Tokens:Key" "$(openssl rand -base64 48)"
```

Para producción con SQL Server:

```
Database__Provider=SqlServer
ConnectionStrings__SqlServerCnn=<tu-cadena>
Tokens__Key=<clave-fuerte>
```

## Ejecutar las pruebas

```bash
dotnet test CompeTournament.Tests
```

## App móvil

Ver [CompeTournament.Mobile/README.md](CompeTournament.Mobile/README.md) para la
arquitectura de la app y los pasos de compilación con MAUI.

## Endpoints principales de la API

| Método | Ruta | Descripción |
| --- | --- | --- |
| `POST` | `/api/auth/token` | Inicio de sesión (devuelve JWT). |
| `POST` | `/api/auth/register` | Registro de usuario. |
| `GET` | `/api/auth/me` | Perfil y puntos del usuario actual. |
| `GET` | `/api/groups` | Lista de torneos/grupos. |
| `GET` | `/api/groups/mine` | Grupos del usuario. |
| `GET` | `/api/groups/{id}` | Detalle con partidos y posiciones. |
| `POST` | `/api/groups/{id}/join` | Solicitud para unirse a un grupo. |
| `GET` | `/api/groups/{id}/leaderboard` | Ranking de miembros. |
| `GET` | `/api/matches/{id}` | Detalle de partido y mi predicción. |
| `POST` | `/api/predictions` | Crear/actualizar predicción. |
| `GET` | `/api/predictions/mine` | Mis predicciones. |
