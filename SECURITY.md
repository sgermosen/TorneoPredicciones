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

Si `Tokens:Key` no está configurada, en desarrollo se deriva una clave efímera
solo-desarrollo y se emite una advertencia. En producción la aplicación **no
arranca** sin una clave fuerte configurada.

### Variables de entorno en producción

```
ConnectionStrings__SqlServerCnn=...
Database__Provider=SqlServer
Tokens__Key=...
Mail__Password=...
Cors__AllowedOrigins__0=https://tu-dominio
```

## Credenciales previamente expuestas

Versiones anteriores de este repositorio contenían secretos reales embebidos en
el código (una clave de firma JWT y una cadena de conexión de Azure Notification
Hub). Esos valores se consideran **comprometidos** por haber estado en un
repositorio público y **deben rotarse/revocarse** en el proveedor
correspondiente, aunque ya no aparezcan en el código actual.

## Reporte de vulnerabilidades

Si encuentras una vulnerabilidad, por favor abre un issue privado o contacta al
mantenedor del repositorio antes de divulgarla públicamente.
