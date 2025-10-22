# Decisiones de Testing y Evidencias

## Frameworks elegidos y justificación

- Backend (.NET 8):
  - xUnit: framework de pruebas moderno, bien soportado en .NET y ampliamente utilizado en la comunidad.
  - Moq: mocking framework para aislar dependencias (por ejemplo, `ILogger<T>` y repositorios/DbContext en escenarios específicos).
  - EF Core InMemory: proveedor en memoria para testear lógica de datos sin requerir una base real; rápido y determinista.
  - Coverlet (XPlat Code Coverage): recolección de cobertura integrada con `dotnet test` y compatible con Azure DevOps.

- Frontend (Angular 17):
  - Karma + Jasmine: stack por defecto en Angular para unit tests; integración directa con @angular-devkit y ChromeHeadless.
  - Headless Chrome: ejecución rápida y estable en CI; evita UI y reduce flakiness.
  - Reporte JUnit: facilita publicar resultados en Azure DevOps.
  - Istanbul (coverage): cobertura de líneas, funciones y ramas con reporte HTML y texto.

## Estrategia de mocking

- Backend:
  - `ILogger<ProductController>`: mockeado con Moq para verificar que ciertos eventos (errores, duplicados, etc.) registran logs.
  - DbContext: en general se usa `ApplicationDbContext` con EF Core InMemory para escenarios de integración ligera (CRUD y validaciones). Para casos de error controlado se usa un DbContext "throwing" para verificar propagación/gestión de excepciones.

- Frontend:
  - `HttpClientTestingModule`: intercepta llamadas HTTP en specs de servicios (`product.service.spec.ts`), validando verbo, URL y payload sin pegarle a la API real.
  - `RouterTestingModule`/mocks de `ActivatedRoute`: para tests de componentes de creación/edición donde se depende de parámetros de ruta.
  - Spies de servicios (Toast/Modal): se espían métodos para comprobar que se muestran pop-ups en éxito/error y en confirmaciones de borrado.

## Casos de prueba relevantes

- Backend:
  - Creación/actualización con validaciones de negocio (nombre sin dígitos/repeticiones excesivas, unicidad de nombre, stock dentro de rango, etc.).
  - Endpoints de stock: `SetStock`, `IncrementStock`, `DecrementStock` con límites y mensajes de error adecuados.
  - Borrado de entidad inexistente -> `NotFound`.
  - Manejo de errores y logging: uso de `ILogger` para registrar eventos esperados.

- Frontend:
  - Servicio HTTP: verifica métodos GET/POST/PUT/DELETE, URLs correctas, y mapeo de fechas/propiedades.
  - Componente de alta/edición: validación de nombre (dígitos, repetidos), validación de stock (0..100), detección de duplicados y manejo de errores del API mostrando pop-ups.
  - Componente de listado: eliminación con confirmación y manejo de error mostrando pop-up.
  - Toast/Modal: se comprueba que las notificaciones bloqueantes se invoquen y que delegan correctamente a la capa de UI.

## Evidencias de ejecución

- Azure Pipelines:
  - Publicación de resultados .NET (TRX) y Angular (JUnit) en cada run.
  - Artefacto de cobertura Angular: carpeta `angular-coverage` con el HTML.

- Local:
  - .NET: `dotnet test -v minimal` en `Backend.Tests` -> 20/20 PASS.
  - Angular: `npm run test:ci` en `Frontend` -> `TOTAL: 20 SUCCESS` y cobertura disponible en `Frontend/coverage/html/index.html`.

## Notas operativas

- Para evitar que los tests de Angular queden corriendo, usar `npm run test:once` o `npm run test:ci` (ambos `--watch=false`).
- En CI, se usa ChromeHeadless y se publica JUnit + cobertura para facilitar trazabilidad.
- Se agregó caché de `.angular/cache` en Azure DevOps para acelerar ejecuciones repetidas.
