Presentación Técnica de Framework consumido por la solución: BaseCore.Framework
- Resumen Ejecutivo
BaseCore.Framework es un marco de trabajo (framework) corporativo diseñado para .NET 10 enfocado en la construcción de aplicaciones empresariales robustas y auditables. Basado en principios de Arquitectura Limpia (Clean Architecture) y Diseño Orientado al Dominio (DDD) el proyecto centraliza transversalmente todas las necesidades de seguridad, observabilidad y acceso a datos.

El proyecto está fragmentado en múltiples módulos (NuGet packages) independientes lo que permite a las aplicaciones adherirse al principio de pay-for-play (instalar solo lo que se necesita).

- Arquitectura y Módulos Principales
A continuación se detalla cada uno de los seis pilares principales del framework y cómo implementar lógicamente cada funcionalidad para obtener los resultados esperados.

1. BaseCore.Framework.Domain
Contiene las interfaces fundamentales y entidades base del DDD como BaseCoreDto clases de ordenamiento (SortingParameterModel), paginación y el corazón de la lógica de negocio: BaseCoreService<TDto, TEntity, TId>. Este servicio genérico expone e implementa un patrón estandarizado para operaciones CRUD (Create, Read, Update, Delete) integrando de forma automatizada trazabilidad, auditoría, paginado, filtrado y validación de entidades.

 Cómo implementarlo lógicamente:

 1: Crea tu DTO heredando de BaseCoreDto<TId>.
 2: Crea tu entidad de base de datos (por ejemplo UserEntity).
 3: Crea tu servicio de negocio heredando de BaseCoreService<TuDto, TuEntidad, TuId>. El constructor te pedirá inyectar las dependencias base (IBaseCoreLogger, BaseCoreRepository, IMapper y un validador genérico).
 Resultado: Cuentas inmediatamente con métodos como TuServicio.GetAll(), TuServicio.Add(dto), TuServicio.Update(dto), y TuServicio.Delete(id). Todos retornarán un objeto encapsulado genérico BaseCoreServiceResult o BaseCoreCollectionServiceResult con la propiedad Validation (información de si es válido o no) y el objeto devuelto, registrando automáticamente la pista de auditoría.
2. BaseCore.Framework.Infrastructure
Implementa el acceso a base de datos mediante Entity Framework Core. Una de sus características más potentes y peculiares es BaseCoreContextExtensions el cual inyecta dinámicamente comandos SQL Server en tiempo de ejecución para crear Triggers (Disparadores) Automáticos (trAfterUpd[Tabla]) en las tablas. Esto asegura de forma inquebrantable a nivel de base de datos que la columna LogTimeStamp se actualice en las operaciones INSERT y UPDATE.

 Cómo implementarlo lógicamente:

 1: Configura tu clase de contexto de Base de Datos que herede de DbContext (Entity Framework).
 2: Define todos tus DbSet<TuEntidad> dentro del contexto. Asegúrate de que tus entidades contengan la columna/propiedad LogTimeStamp.
 3: En el método de arranque de la aplicación o al finalizar la migración de la base de datos, llama al método de extensión provisto por el framework: context.ConfigureDataBase(); (el cual internamente invocará a SeedTriggersSqlServer()).
 Resultado: El framework evaluará cada colección DbSet usando reflexion, inspeccionará SQL Server para localizar la clave primaria (Primary Key) y automáticamente ejecutará un script CREATE TRIGGER si la tabla cuenta con el campo LogTimeStamp. Tus entidades guardarán la fecha exacta de modificación impidiendo que se falseé dicha fecha a nivel de código.
3. BaseCore.Framework.Observability (Auditoría y Trazabilidad)
 Un ecosistema completo de diagnóstico y registro encabezado por BaseCoreAuditEvent
. Registra con gran detalle cada operación del sistema: nombre de la clase, método llamador (CallerMethodName), capa arquitectónica, dirección IP, identificadores de sesión, duraciones (inicio/fin) e incluso interacciones Front-End. Mediante la clase TracingUtil y la librería Audit.NET, todos los parámetros de entrada y salida son rastreados.

 Cómo implementarlo lógicamente:

La magia automática: Si estás usando una clase derivada de BaseCoreService el registro lo hace por ti. Cada invocación a un CRUD envuelve la ejecución en un bloque using (new TracingUtil...) y CreateAuditScope().
Para métodos personalizados: Si creas un método de negocio personalizado en tu servicio o controlador, solo necesitas instanciar el log:
csharp
using (new TracingUtil<TuClase>("N/A", traceLogger, new object[] { param1, param2 }, "NombreDelMetodo"))
{ 
    // Tu lógica aquí 
}
Crear el rastro hijo-padre: Para que los eventos de trazabilidad del repositorio DB se agrupen debajo de una llamada principal (ejemplo: del Controlador Web) inyecta y asocia tu BaseCoreAuditEvent actual a la propiedad TrackLoggerParentEvent de la clase BaseCoreService.
Resultado: Sin escribir configuraciones SQL ni prints en la consola, se recolectan automáticamente en JSON o tu visor de logs las duraciones de los métodos, las llamadas anidadas y los argumentos pasados a la función.
4. BaseCore.Framework.Cryptography (Seguridad y Cifrado)
Un módulo especializado que implementa el contrato IEncryptor:

Simétrico (AES-128 CBC): Con derivación de clave a través de Rfc2898DeriveBytes requiriendo 310,000 ciclos de iteración (PBKDF2).
Asimétrico (RSA): Uso completo de Certificados X509 (X509Certificate2).
 Cómo implementarlo lógicamente:

Paso 1: Inyecta (usando DI) la interfaz genérica IEncryptor en tu clase constructora (El contenedor IoC deberá tener registrado a Encryptor).
Cifrado Simétrico (Contraseñas): Usa _encryptor.EncryptAES("TuTextoSecreto", "TuPasswordSalt"); y para descifrar usa DecryptAES. Usa un Salt constante y oculto por aplicación y el ID del usuario como contraseña secundaria para mayor seguridad.
Cifrado Asimétrico (Certificados): Si vas a firmar o mandar datos sensibles b2b carga un X509Certificate2 del repositorio de tu máquina o path y envíalo a: _encryptor.EncryptUsingPublicKey(certificate, Encoding.UTF8.GetBytes("Data"));. Devuelve un arreglo de bytes fuertemente cifrado.
Resultado: Cifrado militar/bancario implementado con menos de tres líneas de código protegido contra diccionarios y fuerza bruta merced de las más de 300 mil iteraciones PBKDF2 que ejecuta el framework por defecto.
5. BaseCore.Framework.ExceptionManager (Manejo de Excepciones)
Presenta BaseCoreException, una excepción súper vitaminada que recopila proactivamente el estado del entorno cuando ocurre un crash. Captura el MachineName, el nombre del AppDomain, la identidad del hilo (ThreadIdentityName) de forma segura, el usuario de Windows y genera un IdError único de correlación.

 Cómo implementarlo lógicamente:

1: Típicamente los desarrolladores atrapan errores haciendo throw new Exception("Error");. En este framework debes atrapar y elevar excepciones relativas a negocio con 
BaseCoreException
.
2: En tus bloques catch (Exception ex) (o middlewares globales de ASP.NET Core) eleva:
csharp
throw new BaseCoreException(_logger, "Ha ocurrido un error creando el usuario", ex);
Resultado: En el instante en que instancias esta excepción, el Logger (que deberás proveer en el argumento) inyectará automáticamente en el formato impreso o en tu ElasticSearch/FileLog un bloque trazado con el IdError, Clase Recursiva, Nombre del Dominio e Identidad del PC. Podrás decirle a tu usuario final: "Ocurrió un error. Comúniquese con el administrador mencionando el código: [20260303-1A2B]" y buscar ese mismo código directo en los logs.
6. BaseCore.Framework.Configuration (Gestión de Opciones)
Centraliza toda la configuración tipada de la aplicación (
BaseCoreApplicationSettings
). Gestiona URLs de servidores de identidad, cadenas de conexión múltiples, ajustes de TrackLogger, diagnóstico, certificados pre-cargados y variables codificadas propias.

Cómo implementarlo lógicamente:

Paso 1: En tu archivo de configuración de .NET (appsettings.json), crea una sección que se alinee con las propiedades del modelo 
BaseCoreApplicationSettings
 (Ej.: ApplicationName, ConnectionStrings, IdentityServerUrl).
Paso 2: Al momento de inicializar los servicios (Program.cs o Startup.cs), usa el motor nativo de .NET para "bindear" la sección JSON directamente a la clase. Es recomendable inyectarlo como un IOptions<BaseCoreApplicationSettings> de forma global.
Resultado: A través de la interfaz IBaseCoreApplicationSettings, cualquier servicio en tu aplicación podrá acceder fuertemente tipado (sin "Magic Strings" estáticos) al string de conexión, al endpoint de Oauth2 o saber si la aplicación tiene banderas de "DiagnosticsEnabled" en tiempo real.

- Casos de Uso Ideales
Sistemas Financieros, Bancarios o FinTech: El framework brilla en entornos donde la pista de auditoría es material de vida o muerte legal. Los Triggers SQL duros junto con BaseCoreAuditEvent rastreando cada parámetro modificado, validan este punto.

Sistemas Altamente Regulados (Ej. Salud - HIPAA o ERPs Corporativos): Sistemas que deben probar de dónde y cuándo provino una modificación. El hecho de que cada llamada a API esté automatizada bajo su propio 
AuditScope y los metadatos de identidad (IP, Identidad de Windows/Hilos) se incrusten en BaseCoreExceptionb hace ideal este framework en auditorías.

Arquitectura Basadas en Microservicios Gubernamentales: A través de BaseCoreServiceResult permite mantener un estándar inter-departamental idéntico siendo predecible en su consumo para cualquier UI externa o Integrador.

- Fortalezas (Puntos a favor)
Auditoría Implacable e Inevitable: Es casi imposible para un desarrollador olvidar crear logs o rastrear un cambio de entidad; 
BaseCoreService los genera implícitamente mediante using (new TracingUtil...) y Entity Framework automáticamente añade los Triggers SQL. El seguimiento de auditoría está protegido contra errores humanos.
Tiempos de Resolución de Errores (MTTR) Mínimos: 
BaseCoreException escanea detalladamente el StackTrace extrayendo el método, clase y tipo anidado que falló y le estampa un IdError UUID, lo enviará directo a los Loggers para que el equipo de DevOps encuentre el bug sin esfuerzo.
Robusta Criptografía Core: Sus clases de encriptado no cometen los errores de "novato". Implementan vectores de inicialización (IV) dinámicos combinados con Entropy de 128 Bits, salts integrados a los arrays de salida, e itera la encriptación AES bajo un PBKDF2 extenso.
Calidad de Código y Privacidad Garantizadas: Reglas estrictas aplicadas al diseño con NetArchTest que aseguran que nadie corrompa la separación de capas a lo largo de los años. Configurado "Privacy by Design" donde el flag de EmbedAllSources=false mitiga la fuga de código fuente, pero reteniendo el "debug" en los Snupkg.

- Debilidades (Áreas de Mejora o "Trade-Offs")
Agonía de Rendimiento en Alta Frecuencia (Overhead): Cada método en el CRUD (GetAll, Add) de BaseCoreService empaqueta, rastrea con TracingUtil, valida y luego serializa por JSON todo el result set de la capa a la base de datos JsonConvert.SerializeObject(baseCoreCollectionServiceResult.ResultObject). Para cargas voluminosas en sistemas de alto tránsito, esta reflectividad abusiva impactará drástica y negativamente el performance y asignación de CPU/Memory heap (GC).
Acoso y Fuerte Acoplamiento a Microsoft SQL Server: El diseño de BaseCoreContextExtensions inserta RAW Strings directamente configurados para Dialecto T-SQL (Consultando sys.triggers e INFORMATION_SCHEMA). Aunque tiene un .IsSqlServer() para evitar crashes, si se quisiera portar el proyecto mañana mismo a PostgreSQL (múltiples nubes), dicha funcionalidad espectacular de Triggers se desvanecería o requeriría rehacerse manualmente.
Algoritmos Asimétricos Ligeramente Deprecados (Padding): En Encryptor.cs RSAEncryptionPadding.Pkcs1 está fijado por defecto. Aunque no está roto para datos heredados la industria actual estándar insta fuertemente el uso de Padding OaepSHA256 o superior debido a vulnerabilidades como Bleichenbacher conocidas en Pkcs1 en ciertas configuraciones.

Generación Constante de Excepciones Lenta: Llamar a Environment.MachineName, AppDomain e iterar el new StackTrace() es una operación muy costosa en el ciclo de vida de .NET. Si una aplicación que consume el framework eleva muchas 
BaseCoreException (inclusive manejadas por reglas tontas de negocio), afectará el flujo habitual.

BaseCore.Framework es un bloque fundacional y dictatorial. Pre-selecciona todos los aspectos (Logging, Auditoría de BD, DTO Mappers, Encripción y Servicios base) resolviéndolos con madurez corporativa. Está diseñado claramente para priorizar Seguridad y Control Cueste lo que Cueste sobre alto rendimiento asumiendo un enfoque que ama los ecosistemas puramente empresariales dependientes del stack profundo de Microsoft.

