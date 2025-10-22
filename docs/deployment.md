# Despliegue QA / Producción

Este documento resume los recursos cloud disponibles y cómo el pipeline automatiza los despliegues de QA y Producción.

## 1. Recursos en la nube

| Recurso | Entorno | Tipo | Propósito | Configuración relevante |
| --- | --- | --- | --- | --- |
| `tp05-backend-qa` | QA | Azure Web App (App Service) | Aloja la API .NET conectada a la base QA. | `ASPNETCORE_ENVIRONMENT=QA`, `ConnectionStrings__MongoDb` (cadena QA), `MongoDbSettings__DatabaseName=MyDB`, regla CORS con el origen del front QA. |
| `tp05-backend-prod` | Producción | Azure Web App (App Service) | Aloja la API productiva. | `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__MongoDb` (cadena PROD), `MongoDbSettings__DatabaseName=MyDB-PROD`, CORS con el origen del front PROD. |
| Static Web App QA | QA | Azure Static Web App | Publica el build Angular que consume la API QA. | Token `SWA_TOKEN_QA`, URL de API seteada por pipeline (`API_URL_QA`). |
| Static Web App PROD | Producción | Azure Static Web App | Publica el build Angular productivo. | Token `SWA_TOKEN_PROD`, URL de API seteada por pipeline (`API_URL_PROD`). |
| MongoDB Atlas `MyDB` | QA | Base de datos | Almacena datos de pruebas QA. | Usuario `pau`, password gestionado en Atlas. |
| MongoDB Atlas `MyDB-PROD` | Producción | Base de datos | Datos productivos. | Usuario `pauli`, password gestionado en Atlas. |

> ⚠️ Todos los secretos (passwords, connection strings, tokens) viven en variable groups seguros (`StaticWebAppTokens`) o en la configuración de cada App Service. No se commitean al repositorio.

## 2. Release Pipeline (`azure-pipelines-merged.yml`)

El pipeline está definido en la raíz del repo y consume los artefactos generados por el build del TP04. Está compuesto por cuatro stages:

1. **BuildAndTest**: restaura dependencias, ejecuta pruebas de .NET y Angular, y publica un artefacto con la API ya compilada.
2. **Deploy_QA**: despliega el backend a `tp05-backend-qa`, configura variables de entorno (cadena Mongo, base, `ASPNETCORE_ENVIRONMENT=QA`) y publica el frontend QA usando la configuración `qa` de Angular. La URL de la API se inyecta reemplazando el placeholder de `environment.qa.ts`.
3. **Approve_PROD**: etapa sin despliegue que contiene una tarea `ManualValidation@0`. El pipeline queda pausado hasta que el QA Lead y el Product Owner confirmen que QA está aprobado.
4. **Deploy_PROD**: reutiliza el artefacto del backend, configura la App Service productiva (`ConnectionStrings__MongoDb`, `MongoDbSettings__DatabaseName`, `ASPNETCORE_ENVIRONMENT=Production`) y publica la build Angular productiva reemplazando el placeholder de `environment.prod.ts`.

Variables requeridas en la definición de pipeline:

- `API_URL_QA` / `API_URL_PROD`: URL pública del backend para cada entorno.
- `MONGO_CONNSTR_QA` / `MONGO_CONNSTR_PROD`: cadenas de conexión copiadas desde MongoDB Atlas.
- `RG_QA` / `RG_PROD`: resource groups que contienen las Web Apps.
- `FRONT_ORIGIN_QA` / `FRONT_ORIGIN_PROD`: URLs permitidas para CORS.
- Variable group `StaticWebAppTokens`: expone `SWA_TOKEN_QA` y `SWA_TOKEN_PROD`.

## 3. Aprobaciones y responsables

1. **QA Lead (María López)** revisa la web QA y valida funcional/regresión.
2. **Product Owner (Juan Pérez)** confirma el OK de negocio luego de la aprobación de QA.
3. Ambos registran la aprobación dentro de la tarea `ManualValidation@0` en el stage `Approve_PROD`. Si no se recibe confirmación en 72 horas, el pipeline expira automáticamente (`onTimeout: reject`).

Una vez aceptado, la etapa `Deploy_PROD` se ejecuta automáticamente usando el mismo artefacto validado en QA, garantizando paridad entre entornos.
