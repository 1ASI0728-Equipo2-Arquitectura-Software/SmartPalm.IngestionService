# Servicio de Ingestión SmartPalm

Microservicio dueño de lecturas sensoriales, umbrales agronómicos, ingestión idempotente y cálculo de salud de sectores. Conserva el contrato utilizado por el Edge API Python y usa inbox/outbox para integrarse con Device, Crop y Alert.

## Rol en la plataforma

Ingestion es el límite de persistencia del flujo físico de sensores. Recibe lotes desde Edge API, valida la proyección del registry de gateway/dispositivo, comprueba el usuario propietario, la plantación, el sector y el plan activo, aplica los umbrales configurados y almacena únicamente lecturas que pertenecen a la cadena de propiedad registrada. Un lote válido puede producir un evento de violación de umbral para Alert.

El servicio es deliberadamente independiente del buffer local del Edge Node: Edge Node reintenta después de fallos temporales de conectividad, mientras que Ingestion proporciona idempotencia para que un reintento no duplique una lectura. Los clientes web y móvil consultan lecturas, salud de sectores y umbrales a través del API Gateway.

## Relaciones de integración

- **Device Service:** fuente de los eventos de registro y propiedad de gateway/dispositivo.
- **Crop Service:** fuente de las proyecciones de plantación, sector y relación dispositivo-sector.
- **Identity Service:** fuente del estado de cuenta y suscripción utilizado por la autorización y validación de negocio.
- **Alert Service:** recibe eventos de violación de umbral mediante el flujo outbox/inbox.
- **Edge API/Edge Node:** envía el lote físico usando el contrato del gateway; ninguno escribe directamente en esta base de datos.

## Ejecución local

1. Copiar `.env.example` a `.env`.
2. Reemplazar los valores placeholder y usar el mismo `JWT_KEY` configurado en Identity Service.
3. Levantar PostgreSQL y RabbitMQ: `docker compose -f compose.local.yaml --env-file .env --profile isolated up -d`.
4. Levantar la API: `docker compose -f compose.local.yaml --env-file .env --profile api up -d --build`.

El Compose despliega únicamente hacia los servicios configurados. Para probar con PostgreSQL y RabbitMQ remotos, sobrescribe `POSTGRES_CONNECTION_STRING` y `RABBITMQ_URI` en `.env`.

## Despliegue en Vultr con Coolify

Coolify debe clonar este repositorio desde la rama de despliegue y usar `Dockerfile` como build pack. El puerto interno es `8080`; el health check debe ser `/health/ready` y el entorno debe ser `Production`.

Registra las siguientes variables como secretos de la aplicación en Coolify:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__IngestionDatabase=Host=<IP privada Vultr 2>;Port=5432;Database=smartpalm_ingestion;Username=smartpalm_ingestion;Password=<secreto>
Jwt__Issuer=smartpalm.identity
Jwt__Audience=smartpalm.clients
Jwt__Key=<misma clave compartida de Identity Service>
RabbitMq__Enabled=true
RabbitMq__Uri=amqp://<usuario>:<password>@<IP privada Vultr 2>:5672/
RabbitMq__Exchange=smartpalm.events
RabbitMq__Queue=smartpalm.ingestion
```

No publiques PostgreSQL, RabbitMQ ni los puertos internos del microservicio. La base y el broker deben estar restringidos a la red privada. El servicio aplica las migraciones EF Core al iniciar y reintenta hasta diez veces una conexión fallida.

Como alternativa operativa, `compose.deploy.yaml` ejecuta la imagen desde Docker Compose con las mismas variables. Las cadenas del archivo pueden resolverse desde `.env`; nunca almacenes credenciales reales en el repositorio.

## Contrato Edge API

`POST /api/v1/edge-gateways/{gatewayMac}/sensor-readings` acepta `devices[]`, cada uno con `deviceMac` y `readings[]`, además de `syncedAt`. Un reenvío idéntico se reconoce mediante fingerprint y no duplica lecturas.

## Verificación

```powershell
dotnet restore SmartPalm.IngestionService.slnx
dotnet build SmartPalm.IngestionService.slnx
dotnet test SmartPalm.IngestionService.slnx
```

