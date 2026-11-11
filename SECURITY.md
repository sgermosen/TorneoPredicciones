# Security Policy

## Manejo de secretos

Este repositorio es público. **Ningún secreto real debe vivir en el código ni en
archivos versionados.** Toda credencial se resuelve en tiempo de ejecución desde
la configuración (variables de entorno, User Secrets en desarrollo o un gestor de
secretos como Azure Key Vault en producción).

### Configuración local de desarrollo

El backend lee la clave de firma de los tokens desde `Tokens:Key`. En desarrollo,
usa User Secrets para no exponerla:

```bash
cd CompeTournament.Backend
dotnet user-secrets init
dotnet user-secrets set "Tokens:Key" "$(openssl rand -base64 48)"
dotnet user-secrets set "Mail:Password" "<tu-app-password>"
```

Si `Tokens:Key` no está configurada, en desarrollo se usa una clave fija solo
para desarrollo. En producción la aplicación **no arranca** sin una clave fuerte
propia, y rechaza las claves de ejemplo.

### Variables de entorno en producción

```
ConnectionStrings__SqlServerCnn=...
Database__Provider=SqlServer
Tokens__Key=...
Seed__DefaultPassword=...
Mail__Password=...
Cors__AllowedOrigins__0=https://tu-dominio
```

## Reporte de vulnerabilidades

Si encuentras una vulnerabilidad, repórtala de forma privada desde la pestaña
«Security» del repositorio («Report a vulnerability»). No abras un issue público.
