Análisis Arquitectónico y Técnico (Code-Level): BaseCore.Framework
La siguiente disección técnica analiza el código fuente, los patrones de diseño implementados (Design Patterns), y el comportamiento algorítmico de bajo nivel (Low-Level Design) de los módulos que conforman la solución BaseCore.Framework (.NET 10). Se eliminan las descripciones de alto nivel para centrarse exclusivamente en la implementación.

1. Patrón Result y Entidades Base (BaseCore.Framework.Domain)
Implementación Técnica: El dominio impone el uso estricto del patrón Result Wrapper para aislar el flujo de control del throw Exception nativo del CLR de .NET. Esto previene el Stack Unwinding, una operación altamente costosa a nivel de CPU.

IBaseCoreService<TDto, TId>: Todo servicio de la capa Application debe implementar este contrato genérico. La interfaz expone métodos sobrecargados de GetAll que aceptan predicados dinámicos a través de Dictionary<string, object> y ordenamiento complejo usando List<BaseCoreSortingParameterModel>.
Encapsulación de Retorno: Métodos mutadores (
Add
, Update) no retornan el DTO o void, sino que están forzados a retornar BaseCoreServiceResult<TDto, TId>. Esta clase encapsula tres estados: la respuesta genérica (ResultObject), metadatos transaccionales y el estado del motor FluentValidation (mediante un nodo Validation con sus respectivos Errors).
Inyección de Correlación (
BaseCoreAuditEvent
): Los servicios intermedios permiten inyectar el evento TrackLoggerParentEvent proveniente de la capa web. Esto asegura que la traza jerárquica padre-hijo se mantenga estructurada para la posterior ingesta asíncrona hacia loggers o la propia BBDD por la infraestructura.
Ventajas Técnicas (Pro/Contra):

(Pro): Evita las First Chance Exceptions en el CLR, permitiendo escalar el servidor IIS/Kestrel a miles de RPS sin penalización de CPU por capturas de pila fallidas.
(Contra): Todo cliente HTTP que consuma los SDKs generados debe deconstruir forzosamente el objeto Result, incrementando la verbosidad de los clientes.
2. Abstracción EF Core y Triggers Dinámicos (BaseCore.Framework.Infrastructure)
Implementación Técnica: La capa de infraestructura envuelve Microsoft.EntityFrameworkCore y utiliza Metadatos (Reflection) para inyectar scripts T-SQL puros (System.Data.Common.DbCommand) directamente al motor SQL Server al momento del arranque.

Inyección en caliente (
BaseCoreContextExtensions
): El método de extensión ConfigureDataBase<TContext>(this TContext context) se ejecuta asíncronamente en el inyector del 
Startup
. Localiza todos los DbSet declarados por reflection: typeof(TContext).GetProperties().Where(x => x.PropertyType.Name.StartsWith("DbSet")).
Trampas SQL Anidadas: Por cada tabla, el framework abre una conexión ADO.NET (DbConnection) y lanza queries sobre INFORMATION_SCHEMA.TABLE_CONSTRAINTS para descubrir autodinámicamente la llave primaria (primaryKeyColumn). Luego, emite un comando CREATE TRIGGER trAfterUpd{tableName} ON {tableName} AFTER INSERT, UPDATE que forza a nivel de hardware la ejecución de SET LogTimeStamp = GETUTCDATE().
Integración Audit.NET: El repositorio primario intercepta la virtualización del comando SaveChanges(). Mediante el motor de Audit.NET, genera deltas comparativos de la entidad EF (pre y post-estado modificado), lo serializa a JSON y lo anida en la colección secundaria para generar un registro no repudiable de cambios de filas.
Ventajas Técnicas (Pro/Contra):

(Pro): Ni un DBA conectado por SSMS podría hacer operaciones UPDATE LogTimeStamp personalizadas sin que el motor dispare el trigger y sobreescriba su marca con el reloj atómico del disco.
(Contra): Altísima latencia transaccional durante los bulk inserts (inserciones masivas), dado que el Trigger SQL procesa un contexto transitorio interponiendo un lock físico de la tabla por cada registro ingresado.
3. Criptosistema Híbrido Asimétrico/Simétrico (BaseCore.Framework.Cryptography)
Implementación Técnica: La clase de orquestación 
EncryptorManager
 administra un motor dual para evitar almacenar secretos en texto planto y validar que los mensajes transitando entre microservicios no hayan sufrido Man-In-The-Middle (MitM). Requiere System.Security.Cryptography.X509Certificates.

Proceso de Encriptación (
GenerateEncodedString
): Recupera del Trusted Store un X509Certificate2. Usa el helper local AesPasswordGenerator para sembrar un array pseudo-aleatorio atado al tiempo del procesador creando una clave volátil de 50 caracteres (llave AES de un solo uso). Mediante encriptación simétrica AES, codifica el Payload (ej. String de base de datos) usando dicha llave. Luego usa la clase nativa RSA.Encrypt() extrayendo la clave matemática pública del certificado sobreescribiendo en bytes encriptados la llave de 50 caracteres AES.
Payload Concatenado: Genera un solo stream de salida uniendo el array del RSA (llave protegida) + un separador estricto (Encoding.UTF8.GetBytes("|||")) + el array AES cifrado del payload, codificando el flujo completo final en Convert.ToBase64String.
Firmas Digitales Hash (
GenerateSignatureString
): Recibe el payload enrutado, invoca al hardware (certificate!.GetRSAPrivateKey()) y aplica un Hash SHA-512 rellenado criptográfico seguro firmando el paquete (rsa.SignData(..., HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1)).
Ventajas Técnicas (Pro/Contra):

(Pro): Resistencia absouta al vector de ataque por exfiltración. Si se obtienen los AppSettings, sin el certificado PFX exportable alojado en la RAM física del servidor origen o el clúster TPM contenedor, las cadenas son chatarra.
(Contra): Rotar (renovar) certificados caducos exige ineludiblemente herramientas de recifrado de configuraciones antes de desplegar nodos nuevos.
4. Gestor Global de Excepciones Forense (BaseCore.Framework.ExceptionManager)
Implementación Técnica: Constituye el "crash-handler" más atómico del conjunto, heredando la clase universal abstracta de C# pero imponiendo reglas para desenredar fallos fatales absorbiendo el entorno volátil originador.

Identidad Cruzada (
InitializeEnvironmentInformation
): Mediante bloques Try/Catch de contención (evitando SecurityException en nodos Linux), recolecta variables de solo-lectura: Environment.MachineName, aisla el proceso del Application Domain (AppDomain.CurrentDomain.FriendlyName), clona la identidad del hilo paralelizado actual HTTP (Thread.CurrentPrincipal.Identity.Name) e intercepta al usuario anfitrión WindowsIdentity.GetCurrent().Name.
Aplanamiento de Pila (
LogException()
): Instancia la clase nativa System.Diagnostics.StackTrace(). A través de un bucle for, recorre todos los stackTrace.FrameCount. Por cada frame extrae el DeclaringType.Name fusionándolos con un 
StringBuilder
.
Zero-Allocation String Building: Optimizado masivamente en .NET 10 mediante StringBuilder.AppendInterpolatedStringHandler, permitiendo empalmar texto forense sin generar alojamientos de montones adicionales explícitos (allocations) evadiendo picos del recolector de basura (Garbage Collector) en momentos en el que el servidor está a punto de un StackOverflow o un Out Of Memory.
Ventajas Técnicas (Pro/Contra):

(Pro): Entrega a la plataforma Elastic/Logstash el StackTrace real no solo del error, sino formateado explícitamente y empaquetado con el estado exacto procesal sin usar plugins pesados como Serilog Enrichers.
(Contra): Alto uso de Reflexión y lectura de Pila (StackTrace) nativa en instante de colisión, lo que incrementa el consumo de los Ms exactos del Crash por unos nanosegundos adicionales.
5. Orquestador de Observabilidad Audit.Net y NLog (BaseCore.Framework.Observability)
Implementación Técnica: Módulo middleware que se ubica entre las API internas y el proveedor de recolección de archivos y bases de datos. Construido sobre NLog y Audit.Core.

Jerarquía de Contextos (
BaseCoreAuditEvent.cs
): Modela el ciclo del loggeador instanciando diccionarios de correlación: ChildEvents = new Dictionary<int, BaseCoreAuditEvent>();. Su constructor auto-genera un correlativo único combinando utcNow("yyyyMMddHHmmssfff") y un pseudo-aleatorio new Random().Next(100, 1000).
Adaptador Log Nativo (Logger.cs : IBaseCoreLogger): Carga el parser XML local 
Start(string configurationFilePath)
 para anidar a NLog.LogManager.Configuration.
Polimorfismo Bidireccional de LogLevel: Dado a que Microsoft.Extensions implementa 
LogLevel
 abstractos, este logger posee algoritmos switch explícitos (
GetNLogLevel
) adaptando al instante tipos nativos por sus análogos en memoria: LogLevel.Information ==> Priority.Info, lo que encapsula NLog impidiendo dependencias cruzadas en 
Presentation
.
6. Ecosistema de Seguridad Identitaria OIDC (BaseCore.Framework.Security.*)
Implementación Técnica: Estos cuatro ensamblados forman una solución Single Sign-On (SSO) de delegación de credenciales distribuidas construida fundamentalmente sobre OpenIddict.Server.AspNetCore.

Security.Identity (Base de Datos Identidades): Implementación especializada de Entity Framework (
BaseCoreIdentityDbContext
) que obliga, bajo el método sobrescrito 
OnModelCreating
, a encapsular llamadas .ToTable("ARQ_SEC_USERS"). Añade DbSet relacionales pesadas impuestas por su Security.Business:
UserOldPasswords: Auditoría física forzada (Anti-Rotación pasiva).
SessionPolicies / 
UserSessionEntity
: Previene ataques de replicación de Token obligando chequeos (HeartBeat) sobre LastHeartbeat e IpAddress. Al cambiar abruptamente, invalida el SecurityToken y revoca el pipeline en red bloqueando Session Hijacking.
IdentityServer (Startup Pipeline): Instanciación limpia en el inyector IoC: Inicia un contexto general (AddDbContext), invoca la lógica paralela genérica 
AddEnterpriseSecurity
, y por último anexa sobre el host services.AddBaseCoreIdentityServer<AppIdentityDbContext>(coreBuilderObj => { coreBuilder.UseEntityFrameworkCore().UseDbContext<AppIdentityDbContext>(); }). Este es el emisor final que crea los flujos RSA JWT que consumirán los clientes.
SecurityIdentityClient (Delegate Consumer): Un cliente desacoplado (Relying Party). Permite que cualquier microservicio externo anexe la dll para poder comprobar firmas JSON Web Tokens (Públicas), descifrar la clave AES y conectarse bajo REST al control delegativo en el Server Controllers/SessionController.cs forzando deslogueos transversales de clústeres de servidores desde el host principal.
7. Middleware Interceptor HTTP y Embudo Transaccional (BaseCore.Framework.Web)
Implementación Técnica: Front-end físico de back-end. El pipeline ASP.NET. Construido usando patrones de localización pasiva de servicios e invalidación masiva controlada de flujos en la respuesta HTTP saliente.

El BaseController y Patrón Service Locator: Para evitar la Inyección de Constructores Pesada (Donde un Programador añadiría docenas de dependencias solo a un controlador base), expone un localizador diferido y seguro: protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>();. Esto invoca el ensamblado de mapas (AutoMapper) únicamente en tiempo de ejecución (JIT) la primera vez que se solicita el Parse de DTOs, descargando uso de memoria efímera al controlador durante la recepción del stream HTTP inicial.
Embudo ResultObject (
CreateResponse
): Recibe desde Application un BaseCoreServiceResult. Automáticamente testea la condición flag .Validation.IsValid. Si resulta falso en los dominios profundos, fuerza nativamente sobre-escritura en red HTTP regresando genérico BadRequest(result.Validation) y si es verdadero sin payload retorna un HTTP 204 NoContent(). La estandarización API forzada.
ExceptionMiddleware Catch-All: Implementación nativa que intercepta al RequestDelegate _next(httpContext) bajo delegados Try/Catch envolventes C#. Si el árbol entero estalla con expeciones irreconocidas o genéricas nativas (Null Reference, Invalid Operation), se incauta el flujo de salida y se reescribe de urgencia el contexto HTTP instanciando un tipo anónimo en anonimato (Type-Erased): var errorResponse = new { ResultObject = (object?)null, Validation = new { IsValid = false, Errors = new[] { new { ErrorMessage = exception.Message } } } };. La instancia anterior se vuelca de facto sobre la tubería de salida context.Response.WriteAsync(json) tapando silenciosamente el colapso subyacente impidiéndole cruzar los cortafuegos WAF perimetrales al no poder enviar huellas forenses StackTrace de memoria y bases de código, devolviendo código HTTP 500 pre-estructurado como el de 
CreateResponse
. En paralelo y antes del volcamiento json, detona la escritura asíncrona hacia el Logger en máxima escala de criticidad y trazabilidad de identidad (el Módulo ExceptionManager entra en juego recursivo aquí).
8. Arquitectura y Restricción por Testing (BaseCore.Framework.ArchitectureTest)
Implementación Técnica: Una medida compilatoria de validación y coerción continua de Top-Level. Creado bajo TestRunner (XUnit) invocando clases estáticas validadoras del Namespace reflexivo (NetArchTest.Rules).

Aserción Reflexiva Estructural: Las directivas como Types.InAssembly(typeof(Domain.AssemblyReference).Assembly).ShouldNot().HaveDependencyOn("BaseCore.Framework.Infrastructure") aplican Pattern Matching en los metadatos compilados del dll. No validan algoritmos, validan el Árbol de Importación de las Clases y el compilador (using imports...).
Mecanismo coercitivo de Integración CI/CD: Debido a que los procesos estándar de DevOps (ej: Github Actions, Azure Pipelines) incluyen en su pipeline los pasos dotnet restore -> dotnet build -> dotnet test, si un desarrollador por desconocimiento acopla las dependencias inyectando algo del ensamblado FrontEnd o EF Core al código puro anémico de DDD, este módulo lanzará un Assert.Fail deteniendo implacablemente el build automático y rechazando el Push o el Merge Request (MR). Establece código inquebrantable no suceptible a excepciones humanas.

Comment
Ctrl+Alt+M
