<!--
=============================================================================
SYNC IMPACT REPORT
=============================================================================
Version change   : N/A → 1.0.0 (ratificación inicial del proyecto)
Modified         : Ajustes aplicados antes de ratificación final

Added sections:
  - § 1. Misión
  - § 2. Principios de Producto  (PP1 – PP5)
  - § 3. Principios Técnicos     (TP1 – TP10)
  - § 4. Gobernanza              (4.1 Enmienda · 4.2 Versionado · 4.3 Cumplimiento)

Removed sections : (ninguna — plantilla base reemplazada)

Templates requiring review:
  ⚠ .specify/templates/plan-template.md   — revisar alineación con esta constitución
  ⚠ .specify/templates/spec-template.md   — revisar alineación con esta constitución
  ⚠ .specify/templates/tasks-template.md  — revisar alineación con esta constitución
  ⚠ .github/prompts/                      — revisar comandos Copilot generados por Spec Kit
  ⚠ .github/copilot-instructions.md       — revisar que refleje los principios del proyecto

Follow-up TODOs:
  - TODO(AUTHOR_LIST): Confirmar lista oficial de maintainers/autores del proyecto.
  - TODO(REVIEW_CADENCE): Definir cadencia periódica formal de revisión constitucional
    (sugerido: trimestral o al inicio de cada bounded context nuevo).
  - TODO(COVERAGE_THRESHOLD): Validar umbral obligatorio de cobertura de dominio contra
    la línea base real del proyecto antes de hacerlo vinculante en CI.
=============================================================================
-->

# Constitución del Proyecto: MiKompri

**Versión**: 1.0.0 | **Ratificada**: 2026-06-30 | **Última Enmienda**: 2026-06-30

---

## 1. Misión

MiKompri es una plataforma colaborativa para la gestión de listas de compra, diseñada para familias, parejas y grupos pequeños. Su propósito fundamental es simplificar la coordinación conjunta de compras, permitiendo que múltiples miembros operen sobre las mismas listas con transparencia, trazabilidad y una experiencia móvil sencilla.

El producto crece de forma incremental y deliberada: cada entrega DEBE ser funcional por sí misma y aportar valor real a los usuarios antes de añadir complejidad adicional.

---

## 2. Principios de Producto

### PP1 · Valor de Usuario Primero

La funcionalidad real para el usuario DEBE tener prioridad sobre la elegancia técnica, la complejidad arquitectural o la deuda de infraestructura. Toda decisión de producto —prioridad de backlog, alcance de MVP, criterios de aceptación— DEBE justificarse en términos de valor entregado al usuario final.

**Verificable**: Una feature sin historia de usuario o caso de uso explícito NO DEBE promoverse a implementación.

**Rationale**: La complejidad técnica acumulada sin correlato de valor entregado es una de las principales causas de abandono en proyectos en etapa temprana.

---

### PP2 · Autonomía de MVP

Cada MVP o iteración de producto DEBE ser autónoma y desplegable de forma independiente. Un MVP no puede quedar bloqueado por funcionalidades planificadas para iteraciones futuras.

**Verificable**: Antes de cerrar un MVP, DEBE existir un criterio de "done" explícito en el `tasks.md` correspondiente, y todos sus ítems DEBEN estar completados.

**Rationale**: MVPs acoplados generan retrasos en cadena y dificultan la validación temprana con usuarios reales.

---

### PP3 · Listas de Compra como Núcleo

La funcionalidad de creación y gestión de listas de compra —crear lista, agregar ítems, marcar como comprado, editar y eliminar— es el núcleo irrenunciable del producto. Toda extensión futura —usuarios, grupos, permisos, notificaciones, estadísticas o integraciones— DEBE diseñarse alrededor de este núcleo y no al revés.

**Verificable**: El bounded context `ShoppingList` DEBE tener reglas de dominio cubiertas por tests y estar en estado desplegable antes de que otros bounded contexts avancen significativamente.

**Rationale**: Concentrar los primeros esfuerzos en el núcleo garantiza que la propuesta de valor principal siempre esté disponible para los usuarios.

---

### PP4 · Transparencia Colaborativa

Toda acción colaborativa DEBE ser trazable al usuario que la realizó. Como mínimo, cada ítem en una lista compartida DEBE registrar quién lo agregó. Esta trazabilidad es un requisito funcional no negociable, no un nice-to-have.

**Verificable**: Los modelos de dominio que representen entidades colaborativas DEBEN incluir campos como `CreatedBy`, `UpdatedBy` o equivalentes cuando aplique, y exponer esa información en las APIs correspondientes.

**Rationale**: Sin trazabilidad, la colaboración se convierte en confusión. Las familias y grupos necesitan saber quién hizo qué para coordinarse efectivamente.

---

### PP5 · Móvil Primero

El diseño de experiencia de usuario DEBE tomar como referencia principal pantallas móviles, especialmente dispositivos de hasta 430 px de ancho. El cliente inicial es Android mediante .NET MAUI. Las APIs REST DEBEN estar optimizadas para el consumo desde clientes móviles: payloads controlados, paginación cuando aplique, baja latencia y respuestas claras.

**Verificable**: Todo criterio de aceptación de UI DEBE validarse primero en resolución móvil.

**Rationale**: El contexto de uso primario —listas de compra en supermercados, tiendas o desplazamientos— es inherentemente móvil.

---

## 3. Principios Técnicos

### TP1 · Backend en .NET

Todo el backend DEBE desarrollarse en .NET, con versión mínima actual .NET 8. No se permite introducir otros runtimes de servidor —Node.js, Python, Go, etc.— sin una enmienda constitucional aprobada y un ADR que la justifique.

**Verificable**: Todos los proyectos backend de la solución DEBEN referenciar un TFM `net8.0` o superior.

**Rationale**: Mantener un único runtime de servidor simplifica la operación, el CI/CD, el mantenimiento y el onboarding de nuevos colaboradores.

---

### TP2 · Arquitectura Modular por Bounded Contexts

El sistema DEBE organizarse como una arquitectura modular por bounded contexts, preparada para evolucionar hacia microservicios cuando el producto, la escala y la operación lo justifiquen.

Los bounded contexts inicialmente previstos son:

* `ShoppingList`
* `Users`

Cada bounded context DEBE mantener separación clara entre responsabilidades de `Api`, `Application`, `Domain` e `Infrastructure`, ya sea como proyectos separados o como módulos claramente delimitados dentro del monorepo.

La comunicación entre bounded contexts DEBE realizarse a través de contratos públicos —APIs REST, eventos o mensajería— y NO mediante acceso directo a la base de datos ni acoplamiento interno entre capas de dominio de contextos distintos.

**Verificable**: No DEBEN existir referencias directas entre capas `Domain` de bounded contexts distintos. Cualquier comunicación entre contextos DEBE estar documentada en el `plan.md` de la spec correspondiente o en un ADR.

**Rationale**: El desacoplamiento por bounded context permite empezar de forma simple, mantener claridad de dominio y evolucionar progresivamente hacia servicios independientes sin reescrituras prematuras.

---

### TP3 · Monorepo en GitHub

Todo el código fuente, configuración de CI/CD, documentación de specs, ADRs, infraestructura y cliente móvil DEBEN vivir en el mismo repositorio GitHub: `frankcval/MiKompri`.

No se permite fragmentar el monorepo en repositorios separados sin enmienda constitucional.

**Verificable**: No DEBEN existir referencias a código fuente en repositorios externos no declarados como dependencias públicas, paquetes NuGet, GitHub Packages u otros artefactos formalmente versionados.

**Rationale**: El monorepo facilita la visibilidad global, la revisión cruzada de cambios y la ejecución de pipelines unificados.

---

### TP4 · Docker Obligatorio

Docker y Docker Compose DEBEN usarse para:

* Levantamiento del entorno de desarrollo local.
* Bases de datos y servicios externos usados en desarrollo.
* Ejecución de migraciones de base de datos.
* Tests de integración.
* Validación de imagen previa a despliegue.

No DEBE existir un workflow de CI/CD que despliegue directamente desde código fuente sin construir y validar previamente una imagen Docker.

**Verificable**: El pipeline de CI DEBE incluir, como mínimo, un paso de `docker build` y un paso de tests automatizados. Cuando existan tests de integración, estos DEBEN ejecutarse contra servicios levantados en contenedores o entorno equivalente reproducible.

**Rationale**: Docker garantiza mayor paridad entre entornos —local, CI y producción— y reduce defectos del tipo "funciona en mi máquina".

---

### TP5 · Despliegue Objetivo en Azure

La plataforma de despliegue objetivo es Microsoft Azure. Todas las decisiones de infraestructura —base de datos gestionada, registro de contenedores, runtime de API, redes, secretos, observabilidad— DEBEN evaluarse en el contexto de los servicios disponibles en Azure.

Se DEBE evitar incurrir en dependencias de infraestructura que impidan la migración o el despliegue en Azure.

**Verificable**: El pipeline de CD DEBE apuntar a un entorno Azure, como Azure Container Apps, Azure App Service for Containers, AKS u otro servicio Azure equivalente declarado en el ADR correspondiente.

**Rationale**: Azure ofrece integración directa con el ecosistema .NET, GitHub Actions, contenedores, observabilidad y servicios gestionados, reduciendo la fricción operativa.

---

### TP6 · Cliente Android con .NET MAUI

El cliente móvil inicial DEBE desarrollarse con .NET MAUI targeting Android. No DEBE introducirse un segundo framework de cliente móvil —React Native, Flutter, Kotlin nativo, etc.— sin enmienda constitucional y ADR aprobado.

**Verificable**: El proyecto cliente DEBE ser un proyecto .NET MAUI dentro del monorepo, compilable con `dotnet build` para un TFM Android, por ejemplo `net8.0-android` o superior.

**Rationale**: .NET MAUI permite reutilizar el ecosistema .NET en el cliente, mantener C# como lenguaje principal en todo el stack y simplificar el onboarding.

---

### TP7 · APIs REST con OpenAPI/Swagger

Todas las APIs HTTP DEBEN seguir principios REST y DEBEN exponer documentación OpenAPI/Swagger actualizada en cada despliegue no productivo.

Los contratos de API DEBEN versionarse —por ejemplo `/api/v1/`, `/api/v2/`— cuando introduzcan cambios incompatibles con versiones anteriores.

**Verificable**: El endpoint `/swagger` o equivalente DEBE estar disponible en ambientes no productivos. Los builds de CI DEBEN fallar si el esquema OpenAPI generado no es válido, cuando esta validación esté incorporada al pipeline.

**Rationale**: La documentación automática de APIs elimina fricción de integración entre backend, cliente MAUI y futuros consumidores externos.

---

### TP8 · Testing Obligatorio

Se DEBEN mantener tests automatizados para, al menos, los siguientes niveles:

* **Dominio**: reglas de negocio críticas en agregados, entidades y value objects.
* **Casos de uso**: command handlers, query handlers o servicios de aplicación críticos para el negocio.
* **Integración de API**: endpoints principales de cada bounded context.

El pipeline de CI DEBE ejecutar los tests automatizados disponibles y DEBE bloquearse si alguno falla.

La cobertura de código en la capa de dominio DEBE tener una línea base medida en CI. El objetivo inicial recomendado es 80 %, pero el umbral obligatorio se fijará después de validar la cobertura real del proyecto.

**Verificable**: El archivo CI DEBE incluir pasos de test. Cuando se defina el umbral obligatorio de cobertura, el pipeline DEBE fallar si la cobertura queda por debajo de ese valor.

**Rationale**: Los tests son la red de seguridad que permite evolucionar el producto con confianza. Un dominio sin tests es código de producción sin contrato verificable.

---

### TP9 · Decisiones Documentadas mediante ADR

Toda decisión técnica significativa —elección de librería, cambio de patrón arquitectural, migración de tecnología, estrategia de autenticación, estrategia de despliegue, base de datos, mensajería, observabilidad, etc.— DEBE documentarse como un ADR o dentro del `plan.md` de la spec que la origina.

**Verificable**: Todo PR que introduzca un cambio técnico significativo DEBE referenciar el ADR o la sección del `plan.md` que lo justifica. Los revisores DEBEN rechazar PRs que omitan esta justificación cuando el cambio lo requiera.

**Rationale**: Sin registro de decisiones, el equipo pierde contexto histórico y repite debates ya resueltos. Los ADRs son la memoria técnica del proyecto.

---

### TP10 · Desarrollo Spec-First

Ninguna feature nueva DEBE comenzar su implementación sin que existan previamente los tres artefactos de especificación:

| Artefacto  | Propósito                                          | Ubicación sugerida         |
| ---------- | -------------------------------------------------- | -------------------------- |
| `spec.md`  | Qué se construye y por qué: requisitos y contexto  | `specs/<feature>/spec.md`  |
| `plan.md`  | Cómo se construye y decisiones técnicas relevantes | `specs/<feature>/plan.md`  |
| `tasks.md` | Ítems ejecutables con criterios de done            | `specs/<feature>/tasks.md` |

**Verificable**: Todo PR de feature DEBE referenciar los tres artefactos correspondientes en `specs/<feature>/`. Los revisores DEBEN rechazar PRs de feature que omitan estos artefactos.

**Rationale**: La especificación previa a la implementación reduce el retrabajo, alinea expectativas y garantiza que cada feature tenga criterios de aceptación claros antes de escribir código.

---

## 4. Gobernanza

### 4.1 Procedimiento de Enmienda

1. Cualquier colaborador puede proponer una enmienda abriendo un issue en GitHub con el label `constitution-amendment`.
2. La propuesta DEBE incluir:

   * Principio o principios afectados.
   * Justificación.
   * Impacto en artefactos dependientes.
   * Bump de versión propuesto.
3. La enmienda DEBE ser revisada y aprobada por al menos un maintainer del repositorio mediante PR.
4. Una vez aprobado el PR, se ejecuta el comando `/speckit.constitution` para actualizar este documento y propagar cambios a las plantillas dependientes cuando aplique.
5. El commit de enmienda DEBE seguir el formato:

   `docs: amend constitution to vX.Y.Z (<descripción breve del cambio>)`

---

### 4.2 Política de Versionado

La versión de la constitución sigue Semantic Versioning —SemVer—:

| Tipo    | Cuándo se aplica                                                         |
| ------- | ------------------------------------------------------------------------ |
| `MAJOR` | Eliminación o redefinición incompatible de principios existentes         |
| `MINOR` | Adición de un nuevo principio o sección con guía material nueva          |
| `PATCH` | Clarificaciones, correcciones de redacción o refinamientos no semánticos |

---

### 4.3 Revisión de Cumplimiento

* La constitución DEBE revisarse al inicio de cada nuevo bounded context o feature mayor.
* El cumplimiento de los principios técnicos `TP1`–`TP10` DEBE verificarse como parte del checklist de revisión de PRs.
* Las specs nuevas DEBEN demostrar alineación con esta constitución en su `plan.md`.
* Las excepciones a esta constitución DEBEN documentarse mediante ADR y, si cambian un principio obligatorio, mediante enmienda constitucional.
* TODO(REVIEW_CADENCE): Definir cadencia periódica formal de revisión constitucional, sugerido trimestral.

