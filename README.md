Saludos chicos, aquí les traigo el código Fuente de un app para realizar apuestas amistosas desarrollada con Xamarin Forms.

Esta app ha sido desarrollada siguiendo los tutoriales, consejos y mentoria del señor Juan Carlos Zuluaga en su canal de YouTube: https://www.youtube.com/user/jzuluaga55
y la publicacion del codigo fuente propietario se hace con su consentimiento mediante correo electronico

La app consta de un BackEnd, donde el administrador configura las ligas, los equipos, los torneos y los partidos, luego desde el APP los usuarios realizan sus predicciones respecto a cómo va a quedar el partido, si gana el equipo que el usuario escogió se le suma 1 punto, si el partido queda exactamente como el predijo, se le suman tres puntos.

Esta utiliza los conceptos de desarrollo MVVM y  Locator.

La misma funciona con un BackEnd desarrollado en ASP usando el patrón MVC hosteado en Azure usando SQL como motor de base de datos.

Segmentación del API, el POCO y el Proyecto web, para mayor seguridad y mantenibilidad en el futuro.

En el código fuente podremos encontrar buenas prácticas para lo siguiente:

------Backend------
•	Interacción e integración con servicios de Azure.
•	Consultas seguras a APIS usando token de seguridad.
•	Desarrollo de un API segura.
•	Creación de la base de datos mediante POCO, migraciones y manejo seguro de datos e información de usuarios.
•	Consultas entre varias tablas y devolver resultados en una sola consulta.
•	Utilización de JSON y listas Dinámicas.
•	Manejo de maestro detalle mediante un solo controlador.
•	Consultas Genéricas mediante controladores genéricos para el múltiple manejo de tablas.

-----App-------
•	Manejo de la cámara.
•	Manejo de internet.
•	Manejo de seguridad.
•	Almacenamiento de datos en SQLite.
•	Creación de usuarios Locales mediante servicios seguros.
•	Validación de injection.
•	Creación y extensión de Controles locales y personalizados.
•	Actualización y envió de datos mediante protocolos seguros.

---------Clases y otras vainas bonitas de regalo en el app------
•	FilesHelper
•	Response Generico
•	Token Response
•	User Request
•	Custom Control Bindable Picker
•	Interfaces Genericas
•	ApiService
•	DataService
•	DialogService
•	NavigationService

Aca les dejo el link de descarga del PlayStore

https://play.google.com/store/apps/details?id=com.praysoft_net.TorneosPredicciones&hl=es

Ayudarían bastante a que crezca la comunidad de desarrollo de Xamarin en la Republica Dominicana si la descargan además de puntearla con 5 Estrellas
