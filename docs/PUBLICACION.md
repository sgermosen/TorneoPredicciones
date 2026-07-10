# Guía de publicación — Copa Amistosa

Guía **paso a paso** para poner en producción el backend y publicar la app móvil
en **Google Play** y **App Store**. Incluye configuración de servidor, firma,
iconos, capturas, textos listos para copiar, clasificación de contenido y
política de privacidad.

> Identidad de la app
> - **Nombre público:** Copa Amistosa
> - **Bundle / Application ID:** `com.competournament.mobile`
> - **Backend:** `CompeTournament.Backend` (.NET 10, API REST + SignalR + panel MVC)
> - **App:** `CompeTournament.Mobile` (.NET MAUI)

---

## 0. Antes de empezar: rotar credenciales (obligatorio)

Versiones antiguas del repositorio contuvieron secretos reales (clave JWT,
cadena de Azure Notification Hub y contraseñas de admin del panel legacy). Aunque
ya no están en el código actual, **estuvieron públicos y deben rotarse**:

1. **Azure Notification Hub / Service Bus:** regenera la SharedAccessKey en el
   portal de Azure (o elimina el recurso si no se usa).
2. **Clave de firma JWT:** genera una nueva (`openssl rand -base64 48`).
3. **Contraseñas de administrador** que hayas usado en el panel legacy.
4. Si usas Gmail para correo, usa una **App Password**, nunca la contraseña real.

---

## Parte 1 — Backend en el servidor

### 1.1 Requisitos

- **.NET 10 SDK/Runtime** en el servidor (o contenedor).
- Una base de datos: **SQL Server** (recomendado en producción) o SQLite.
- Un dominio con **HTTPS** (certificado TLS). JWT y SignalR requieren HTTPS en prod.

### 1.2 Base de datos

**Opción A — SQL Server (producción):**
1. Crea una base de datos vacía (Azure SQL, SQL Server on-prem o contenedor).
2. Obtén la cadena de conexión.
3. Configura `Database:Provider=SqlServer` y `ConnectionStrings:SqlServerCnn`.

**Opción B — SQLite (pruebas/uso ligero):** valor por defecto, sin configuración.

> **Esquema:** la app crea el esquema con `EnsureCreated` a partir del modelo
> (funciona con ambos proveedores). Para control de versiones del esquema en
> producción, considera añadir migraciones de EF Core por proveedor más adelante.

### 1.3 Variables de entorno (configuración)

Nunca pongas secretos en `appsettings.json`. Úsalos como variables de entorno
(en Linux se usan dobles guiones bajos para la jerarquía):

| Variable | Ejemplo / descripción |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Database__Provider` | `SqlServer` o `Sqlite` |
| `ConnectionStrings__SqlServerCnn` | Cadena de SQL Server (si aplica) |
| `Tokens__Key` | Clave de firma JWT **fuerte** (>= 32 bytes) |
| `Tokens__Issuer` | `CompeTournament` |
| `Tokens__Audience` | `CompeTournamentUsers` |
| `Tokens__AccessTokenMinutes` | `60` |
| `Tokens__RefreshTokenDays` | `30` |
| `Cors__AllowedOrigins__0` | Origen permitido (p. ej. tu dominio web si lo hubiera) |
| `Mail__From` | Remitente de correo |
| `Mail__Password` | App Password del correo |
| `Scheduler__Enabled` | `true` |
| `Scheduler__IntervalSeconds` | `60` |
| `Observability__OtlpEndpoint` | (opcional) endpoint OTLP de tu colector |
| `Seed__DefaultPassword` | (opcional) contraseña de los usuarios de ejemplo |

> En **producción la app no arranca** si `Tokens__Key` no está configurada o es
> débil. Es una protección intencional.

### 1.4 Publicar la aplicación

```bash
cd CompeTournament.Backend
dotnet publish -c Release -o ./publish
```

### 1.5 Hosting — Opción A: Azure App Service (recomendada)

1. Crea un **App Service** (Linux, .NET 10).
2. En **Configuración → Variables de entorno**, agrega las de la tabla 1.3.
3. En **Configuración → Configuración general**:
   - **Web sockets: ON** (imprescindible para SignalR).
   - **Always On: ON** (para que el `BackgroundService` del scheduler no se
     detenga por inactividad).
   - **HTTPS Only: ON**.
4. Despliega (`az webapp deploy`, GitHub Actions o Zip Deploy del `./publish`).
5. Si **escalas a varias instancias**, SignalR necesita un backplane
   (Azure SignalR Service o Redis) y sticky sessions. Con una sola instancia no
   hace falta.

### 1.6 Hosting — Opción B: Docker / VPS

Ejemplo de `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish CompeTournament.Backend -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CompeTournament.Backend.dll"]
```

- Pon un **reverse proxy con TLS** delante (Nginx, Caddy, Traefik) y habilita
  **WebSockets** en el proxy para `/hubs/tournament`.
- Pasa las variables de entorno con `-e` o `--env-file`.

### 1.7 CORS y la app móvil

La app móvil usa el token JWT en el header, no cookies, así que normalmente **no
necesita CORS**. Si además expones un cliente web, añade su origen en
`Cors__AllowedOrigins__N`.

### 1.8 Verificación post-despliegue

- `GET https://tu-dominio/health` → debe responder `Healthy`.
- `GET https://tu-dominio/openapi/v1.json` (solo si dejas Development) → documento OpenAPI.
- Prueba `POST /api/auth/token` con un usuario de ejemplo.
- Prueba una conexión al hub `/hubs/tournament` (WebSockets).

### 1.9 Integraciones externas (opcionales)

- **Push real (FCM/APNs):** los dispositivos ya se registran vía `/api/devices`.
  Implementa un proveedor que lea esos tokens y envíe por FCM/APNs, reemplazando
  `LoggerNotificationService`.
- **Resultados automáticos:** implementa `IMatchResultsProvider` contra una API
  deportiva (API-Football, Football-Data) para que el scheduler cierre partidos
  con marcadores reales.
- **Recap con IA:** implementa `IJornadaRecapGenerator` contra un proveedor LLM
  si quieres resúmenes generativos en lugar de la plantilla.

---

## Parte 2 — Compilar y firmar la app móvil

### 2.1 Requisitos

```bash
dotnet workload install maui
```

- **Android:** Android SDK (se instala con el workload / Android Studio).
- **iOS:** un Mac con **Xcode** y una cuenta de **Apple Developer**.

### 2.2 Apuntar al API de producción

Edita `CompeTournament.Mobile/Services/ApiConstants.cs` y coloca la **URL HTTPS
pública** de tu backend (para Android físico/emulador usa el dominio real, no
`localhost`).

### 2.3 Firmar la app

**Android (keystore):**
```bash
keytool -genkey -v -keystore copa-amistosa.keystore -alias copa \
  -keyalg RSA -keysize 2048 -validity 10000
```
- Guarda el `.keystore` y sus contraseñas en un lugar **seguro** (no en el repo).
- Configura la firma en el `.csproj`/perfil de publicación o pásala por línea de
  comandos al publicar.

**iOS (certificados):**
- En **developer.apple.com**: crea el **App ID** `com.competournament.mobile`,
  un **certificado de distribución** y un **provisioning profile** de App Store.
- Xcode gestiona la firma automáticamente si inicias sesión con tu cuenta.

### 2.4 Compilar en release

```bash
# Android App Bundle (AAB) para Play
dotnet publish CompeTournament.Mobile -f net10.0-android -c Release

# iOS (IPA) — requiere Mac + Xcode
dotnet publish CompeTournament.Mobile -f net10.0-ios -c Release
```

---

## Parte 3 — Google Play Store

### 3.1 Cuenta

- Crea una **cuenta de desarrollador de Google Play** (pago único de **25 USD**).

### 3.2 Crear la app en Play Console

1. **Play Console → Crear app**.
2. Nombre: **Copa Amistosa**. Idioma por defecto: Español. Tipo: **App**. Gratis.
3. Acepta las declaraciones.

### 3.3 Ficha de Play Store (textos listos para copiar)

Ver **Parte 5** para las descripciones. En **Presencia en la tienda → Ficha
principal** coloca:
- Nombre (30 car.), descripción breve (80 car.), descripción completa (4000 car.).

### 3.4 Gráficos requeridos (Play)

| Recurso | Especificación |
| --- | --- |
| Icono de la app | **512×512 px**, PNG 32-bit, < 1 MB |
| Gráfico de funciones (feature graphic) | **1024×500 px**, PNG/JPG |
| Capturas de teléfono | mínimo **2** (recomendado 4–8), 16:9 o 9:16, lado 320–3840 px |
| Video (opcional) | enlace de YouTube |

> Usa el **artifact de screenshots** (ver enlace en el chat) exportando cada
> tarjeta a 1080×1920, o toma capturas reales del emulador.

### 3.5 Clasificación de contenido (cuestionario IARC)

**Importante (apuestas):** la app trata de "apuestas amistosas" entre amigos.
- Si **NO se maneja dinero real dentro de la app** (solo se registran
  predicciones y puntos; los premios se acuerdan entre amigos fuera de la app),
  respóndelo así en el cuestionario: **no hay juego con dinero real ni compras de
  azar**. Aun así, por el tema de apuestas, lo habitual es que quede
  **PEGI 12 / Teen** o superior. Sé honesto en cada pregunta.
- Si en algún momento **sí** procesas dinero real, entras en la **política de
  juego y apuestas de Google**: requiere solicitud especial, verificación,
  restricción por país y edad mínima. Evítalo salvo que cuentes con licencias.

### 3.6 Seguridad de los datos (Data safety)

Declara lo que la app recoge:
- **Correo electrónico y nombre** (para la cuenta) — *obligatorio*, no se comparte
  con terceros, se transmite cifrado, el usuario puede solicitar eliminación.
- **Identificadores de dispositivo** (token de push) si activas notificaciones.
- No se recopila ubicación, contactos ni datos financieros.

### 3.7 Público objetivo y política de privacidad

- **Público objetivo:** 13+ (o 18+ si enfatizas el tema de apuestas).
- **Política de privacidad:** URL obligatoria. Usa la plantilla de la **Parte 5**
  publicada en una web pública (GitHub Pages sirve).

### 3.8 Lanzar

1. **Prueba interna** (internal testing): sube el **AAB**, agrega testers por
   correo, valida el flujo completo con el backend de producción.
2. Promueve a **prueba cerrada/abierta** si quieres beta pública.
3. **Producción:** completa todas las secciones (que queden en verde), sube el
   AAB, revisa países y precios (gratis), y **envía a revisión**.

---

## Parte 4 — Apple App Store

### 4.1 Cuenta

- **Apple Developer Program**: **99 USD/año**.

### 4.2 Crear la app en App Store Connect

1. **App Store Connect → Mis apps → +** → Nueva app.
2. Plataforma iOS, nombre **Copa Amistosa**, idioma principal Español,
   **Bundle ID** `com.competournament.mobile`, un **SKU** (p. ej. `copa-amistosa-01`).

### 4.3 Ficha (textos listos en la Parte 5)

- **Nombre** (30), **subtítulo** (30), **descripción** (4000),
  **palabras clave** (100, separadas por comas), **texto promocional** (170).
- **URL de soporte** y **URL de marketing** (opcional).

### 4.4 Recursos gráficos (App Store)

| Recurso | Especificación |
| --- | --- |
| Icono de la app | **1024×1024 px**, PNG sin transparencia ni esquinas redondeadas |
| Capturas iPhone 6.7" | **1290×2796** (obligatorias) |
| Capturas iPhone 6.5" | **1242×2688** |
| Capturas iPhone 5.5" | **1242×2208** |
| Capturas iPad (si aplica) | 12.9" **2048×2732** |

### 4.5 Clasificación por edad

- Completa el cuestionario. Por el tema de **"apuestas/juego simulado"** marca
  la casilla correspondiente → normalmente queda **17+**. Si no hay dinero real,
  indícalo; si lo hubiera, aplica la política de gambling de Apple (más estricta).

### 4.6 Privacidad (App Privacy)

- Declara datos recopilados: **correo, nombre** (vinculados a la identidad, para
  funcionalidad de la app), **identificadores** si usas push.
- **URL de política de privacidad** obligatoria (plantilla en la Parte 5).

### 4.7 TestFlight y revisión

1. Sube el **IPA** (Xcode/Transporter). Prueba con **TestFlight**.
2. En **Información de revisión**, agrega una **cuenta demo** para el revisor:
   `sgrysoft@gmail.com` / `Torneo2026` (o la que definas en el seeding), y notas
   explicando que las "apuestas" no manejan dinero real dentro de la app.
3. **Enviar a revisión**. La primera revisión suele tardar 1–3 días.

---

## Parte 5 — Textos y assets listos para copiar

### 5.1 Nombre y subtítulo

- **Nombre:** Copa Amistosa
- **Subtítulo (App Store, 30):** Predicciones entre amigos
- **Descripción breve (Play, 80):** Predice los partidos, reta a tus amigos y sube en el ranking en vivo.

### 5.2 Descripción larga (ES)

```
Copa Amistosa convierte cualquier torneo en una competencia entre amigos.

Crea un grupo, invita a tus panas con un código y predice el marcador de cada
partido. Aciertas el resultado exacto y sumas 3 puntos; aciertas solo al ganador
y sumas 1. El ranking se actualiza EN VIVO cada vez que cierra un partido.

• Predicciones por partido con marcador
• Jugada "Banker": dobla los puntos del partido que más confías
• Ranking y posiciones en tiempo real (SignalR)
• Chat por partido para el trash-talk sano
• Invita a tu grupo con un simple código o link
• Estadísticas personales: precisión y rachas
• Resumen automático de cada jornada

Sin complicaciones: tú y tus amigos acuerdan la apuesta, la app lleva la cuenta.

Descarga Copa Amistosa y demuestra quién sabe más de tu deporte favorito.
```

### 5.3 Descripción larga (EN)

```
Copa Amistosa turns any tournament into a friendly competition.

Create a group, invite your friends with a code, and predict the score of every
match. Nail the exact score for 3 points; get just the winner for 1. The
leaderboard updates LIVE the moment a match ends.

• Score predictions for every match
• "Banker" pick: double the points on your most confident match
• Real-time leaderboard and standings (SignalR)
• Per-match chat for friendly trash-talk
• Invite your group with a simple code or link
• Personal stats: accuracy and streaks
• Automatic recap of every round

Simple: you and your friends agree on the stakes, the app keeps score.

Download Copa Amistosa and prove who really knows the game.
```

### 5.4 Palabras clave (App Store, 100 car.)

```
predicciones,quiniela,torneo,futbol,apuestas amigos,ranking,deportes,liga,polla,pronosticos
```

### 5.5 Notas de la versión (v1.0)

```
¡Primera versión de Copa Amistosa! Crea grupos, predice partidos, juega tu
Banker y sube en el ranking en vivo con tus amigos.
```

### 5.6 Plantilla de Política de Privacidad

> Publícala en una URL pública (p. ej. GitHub Pages) y usa esa URL en ambas tiendas.

```
Política de Privacidad — Copa Amistosa

Última actualización: [FECHA]

Copa Amistosa ("la app") respeta tu privacidad. Esta política explica qué datos
recopilamos y cómo los usamos.

1. Datos que recopilamos
- Datos de cuenta: nombre, apellido, correo electrónico y teléfono (opcional),
  que proporcionas al registrarte.
- Contenido que generas: predicciones, comentarios y pertenencia a grupos.
- Identificador de dispositivo para notificaciones push (si las activas).

2. Cómo usamos los datos
- Para crear y gestionar tu cuenta y tu participación en los grupos.
- Para calcular puntos, rankings y estadísticas.
- Para enviarte notificaciones relacionadas con la app.

3. Compartir datos
No vendemos ni compartimos tus datos personales con terceros con fines
publicitarios. Los datos se procesan únicamente para operar la app.

4. Seguridad
La comunicación con nuestros servidores usa cifrado (HTTPS). Las contraseñas se
almacenan de forma segura (hash).

5. Tus derechos
Puedes solicitar el acceso, la corrección o la eliminación de tus datos
escribiendo a [CORREO DE CONTACTO].

6. Menores
La app no está dirigida a menores de la edad mínima indicada en la ficha de la
tienda.

7. Contacto
Para cualquier duda: [CORREO DE CONTACTO].
```

### 5.7 Especificaciones de iconos (resumen)

| Uso | Tamaño | Notas |
| --- | --- | --- |
| Play — icono | 512×512 | PNG 32-bit |
| App Store — icono | 1024×1024 | Sin transparencia ni esquinas redondeadas |
| App (fuente) | `Resources/AppIcon/appicon.svg` + `appiconfg.svg` | MAUI genera todas las densidades |

> Reemplaza los SVG placeholder en `CompeTournament.Mobile/Resources/AppIcon/`
> y `Resources/Splash/` por el arte final antes de compilar el release.

---

## Parte 6 — Después del lanzamiento

- **Monitoreo:** `/health`, y OpenTelemetry hacia tu colector (Grafana/Datadog/…)
  si configuras `Observability__OtlpEndpoint`.
- **Crash reporting móvil:** considera integrar App Center/Sentry/Firebase
  Crashlytics.
- **Actualizaciones:** sube nuevas builds incrementando `ApplicationVersion`
  (Android versionCode) y `ApplicationDisplayVersion`.
- **Rotación periódica** de la clave JWT y secretos.

---

## Checklist rápido

- [ ] Credenciales antiguas **rotadas** (JWT, Azure, admin, correo).
- [ ] Backend desplegado con HTTPS y WebSockets, `/health` en verde.
- [ ] Variables de entorno de producción configuradas (incl. `Tokens__Key`).
- [ ] `ApiConstants.cs` apuntando al dominio de producción.
- [ ] App firmada (keystore Android / certificados iOS) y **respaldada**.
- [ ] Iconos y splash finales colocados.
- [ ] Capturas y textos cargados en ambas tiendas.
- [ ] Clasificación de contenido y formularios de privacidad completados.
- [ ] Política de privacidad publicada y enlazada.
- [ ] Cuenta demo para el revisor de Apple.
- [ ] Builds subidos (AAB / IPA) y enviados a revisión.
