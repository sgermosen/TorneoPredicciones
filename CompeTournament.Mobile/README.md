# Copa Amistosa — App MAUI

Cliente móvil (.NET MAUI) para el sistema de predicciones de torneos. Consume la
API REST de `CompeTournament.Backend` y comparte los contratos con
`CompeTournament.Shared`.

## Arquitectura

- **CompeTournament.Mobile.Core** — lógica sin dependencias de MAUI: cliente HTTP
  (`ApiClient`), abstracciones (`ITokenStore`, `ISession`, `INavigationService`)
  y los ViewModels (CommunityToolkit.Mvvm). Es la capa que se prueba de forma
  automatizada contra la API real.
- **CompeTournament.Mobile** — capa de presentación MAUI: páginas XAML, sistema
  visual (`Resources/Styles`), inyección de dependencias (`MauiProgram`) e
  implementaciones de plataforma (`SecureStorageTokenStore`,
  `ShellNavigationService`).

## Pantallas

- Login y registro (con refresh token y auto-refresh transparente).
- Torneos (listado de grupos, con filtro "solo mis grupos" y unión a un grupo o
  por código de invitación).
- Detalle de grupo: partidos, tabla de posiciones y ranking de amigos que se
  **actualizan en vivo** (SignalR), resumen de la jornada y compartir invitación.
- Predicción de partido: marcador con pasos +/−, opción **banker** (doble
  puntos) y **chat del partido** en tiempo real.
- Perfil con puntos acumulados, **estadísticas** (precisión y rachas) y cierre
  de sesión.

## Servicios de la capa Core

`ApiClient` (con auto-refresh), `ILiveTournamentClient` (SignalR),
`ITokenStore`, `ISession` e `INavigationService`. Todos los ViewModels se
prueban contra la API real en `CompeTournament.Tests`.

## Requisitos para compilar

La app usa la carga de trabajo de MAUI. En una máquina con el SDK de .NET 10:

```bash
dotnet workload install maui
dotnet build CompeTournament.Mobile/CompeTournament.Mobile.csproj -f net10.0-android
```

La URL base de la API se configura en `Services/ApiConstants.cs`
(`10.0.2.2` para el emulador de Android, `localhost` para el resto).

Los iconos y la pantalla de bienvenida usan SVG en `Resources/`; sustitúyelos por
los definitivos antes de publicar.
