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
2. Usar el mismo `JWT_KEY` de Identity Service.
3. Levantar PostgreSQL con `docker compose -f compose.local.yaml --env-file .env up -d`.
4. Ejecutar `dotnet run --project SmartPalm.IngestionService.API`.

Los perfiles `isolated` y `api` permiten levantar RabbitMQ y la API en contenedores. El archivo raíz de `Code` no es necesario.

## Contrato Edge API

`POST /api/v1/edge-gateways/{gatewayMac}/sensor-readings` acepta `devices[]`, cada uno con `deviceMac` y `readings[]`, además de `syncedAt`. Un reenvío idéntico se reconoce mediante fingerprint y no duplica lecturas.

## Verificación

```powershell
dotnet restore SmartPalm.IngestionService.slnx
dotnet build SmartPalm.IngestionService.slnx
dotnet test SmartPalm.IngestionService.slnx
```

