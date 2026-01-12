# AGENTS.md

Este archivo proporciona contexto e instrucciones para ayudar a los agentes de IA a trabajar en este proyecto.

## Contexto del Proyecto

**SST TagleLabs** es un sistema multi-componente para gestión de Seguridad y Salud en el Trabajo (SST) y control de asistencia. El proyecto incluye:

1. **AsistenciaPro** (`AsistenciaPro/`) - Plataforma web multiempresa para control de asistencia, horas extra y cálculo automático de sueldos
   - Stack: Next.js 16 (App Router), TypeScript, Tailwind CSS, Supabase
   - PWA optimizada para móviles con reconocimiento facial local
   - Backend: Next.js API routes + repositorios SQL propios (sin Prisma)

2. **TagleLabsGestorSST** - Aplicación de escritorio para gestión SST
   - Stack: .NET 8, C#, WPF (MVVM)
   - Integración con IA remota (Gemini)
   - Generación de documentos SST (RIOHS, IRL, etc.)

## Tips del Entorno de Desarrollo

### Para AsistenciaPro

#### Configuración inicial
- Usa Node.js 20+ y npm
- Las migraciones SQL están en `prisma/migrations/` (solo archivos SQL, sin Prisma CLI)
- Todas las consultas usan repositorios SQL directos en `src/lib/repos/*`
- La zona horaria es fija: `America/Santiago` (configurada en `src/lib/timezone.ts`)

#### Variables de entorno requeridas
Crea un archivo `.env` en `AsistenciaPro/` con:
```env
DATABASE_URL="postgresql://USER.PROJECT:PASSWORD@aws-REGION.pooler.supabase.com:6543/postgres?sslmode=require"
JWT_SECRET="define-un-secreto-aqui"
SUPABASE_URL="https://tu-proyecto.supabase.co"
SUPABASE_ANON_KEY="tu-clave-anon"
SUPABASE_SERVICE_ROLE_KEY="tu-clave-service-role"
SUPABASE_DB_URL="postgresql://USER.PROJECT:PASSWORD@aws-REGION.pooler.supabase.com:6543/postgres?sslmode=require"
NEXT_PUBLIC_SUPABASE_URL="https://tu-proyecto.supabase.co"
NEXT_PUBLIC_SUPABASE_ANON_KEY="tu-clave-anon"
```

#### Comandos útiles
```bash
# Desarrollo
cd AsistenciaPro
npm run dev              # Servidor Next.js en desarrollo (puerto 3000)

# Base de datos
npm run db:seed          # Poblar BD con datos demo (superadmin, empresas, trabajadores)
npm run db:migrate       # Aplicar migraciones SQL (loop psql)

# Testing y calidad
npm run lint             # Ejecutar ESLint
npm test                 # Ejecutar tests con Vitest
npm run build            # Compilar para producción
npm run prepush          # Lint + test + build (usado en pre-push hook)
```

#### Estructura de carpetas clave
- `src/app/api/*` - Route handlers (autenticación, empresas, trabajadores, marcaciones, reportes)
- `src/app/empresa`, `src/app/superadmin`, `src/app/trabajador` - Dashboards por rol
- `src/lib/repos/*` - Repositorios SQL (companies, employees, time-records, etc.)
- `src/lib/time-calculations.ts` - Lógica de negocio para cálculo de horas
- `src/components/kiosk/` - Componentes del kiosco PWA
- `public/face-models/` - Modelos de reconocimiento facial (face-api)

#### Convenciones importantes
- **Multi-tenant seguro**: Cada request valida `companyId` y rol antes de acceder a datos
- **Sin Prisma**: Usa repositorios SQL directos (`pg` pool) y Supabase REST client
- **Realtime**: Supabase Realtime habilitado para tabla `TimeRecord` (sincronización de kioscos)
- **Autenticación**: JWT con cookies HTTP-only (`src/lib/auth.ts`)
- **Validaciones**: Zod para validación de payloads

#### Trabajar con repositorios
Los repositorios están en `src/lib/repos/`:
- `companies.ts` - CRUD empresas, actualización PIN, slugs
- `employees.ts` - Listados activos, horarios, actualización sueldos
- `time-records.ts` - Marcaciones, estados del día, reportes
- `kiosk-devices.ts` - Autorización de tablets, tokens
- `employee-faces.ts` - Descriptores faciales (reconocimiento)

Todos usan el pool de `src/lib/db.ts` (conexión a `SUPABASE_DB_URL`).

### Para TagleLabsGestorSST

#### Configuración inicial
- Requiere .NET 8 SDK
- Solución: `TagleLabsGestorSST.sln`
- Arquitectura MVVM con WPF

#### Estructura de la solución
- `TagleLabsGestorSST.UI/` - Interfaz WPF (Views, ViewModels)
- `TagleLabsGestorSST.Services/` - Lógica de negocio (MatrizRiesgoService, LocalAiService, etc.)
- `TagleLabsGestorSST.Data/` - Acceso a datos (Entity Framework)
- `TagleLabsGestorSST.Tests/` - Tests unitarios

#### Integración con IA
- **LocalAiService**: Integración con Gemini (API remota)
- Variables de entorno: `GEMINI_API_KEY`, `GEMINI_MODEL` (por defecto: `gemini-2.5-flash-lite`)
- Archivo `gemini.key` en raíz del proyecto (distribuido con instalador)
- El servicio también soporta Claude y Groq como fallback opcional (requieren `CLAUDE_API_KEY` y `GROQ_API_KEY`)
- Rate limiting: Throttle de 6.5 segundos entre llamadas para respetar límites de Gemini Free Tier (10 RPM)

#### Comandos útiles
```bash
# Compilar
dotnet build TagleLabsGestorSST.sln

# Ejecutar
dotnet run --project TagleLabsGestorSST.UI

# Tests
dotnet test TagleLabsGestorSST.Tests
```

#### Convenciones importantes
- **MVVM**: Separación clara entre Views (XAML) y ViewModels (C#)
- **Servicios de IA**: `LocalAiService` expone métodos para análisis, mejora de redacción, extracción de datos
- **Base de datos**: SQLite (`TagleLabs.db`) en raíz del proyecto
- **Templates**: Plantillas de documentos en `Templates/`
- **Outputs**: Documentos generados en `Outputs/`

## Instrucciones de Testing

### AsistenciaPro
```bash
cd AsistenciaPro
npm test                 # Ejecutar todos los tests con Vitest
npm run lint             # Verificar ESLint y TypeScript
npm run build            # Verificar que compila sin errores
```

**Antes de commit:**
- Siempre ejecuta `npm run prepush` (lint + test + build)
- Si hay errores de tipo o lint, corrígelos antes de commitear
- Agrega o actualiza tests para el código que cambies

### TagleLabsGestorSST
```bash
dotnet test TagleLabsGestorSST.Tests
dotnet build TagleLabsGestorSST.sln
```

**Antes de commit:**
- Compila sin errores ni advertencias
- Ejecuta tests relevantes
- Verifica que la UI responde correctamente

## Instrucciones de PR

### Formato de títulos
- Para AsistenciaPro: `[AsistenciaPro] <Descripción del cambio>`
- Para TagleLabsGestorSST: `[TagleLabsGestorSST] <Descripción del cambio>`
- Para cambios generales: `[General] <Descripción>`

### Checklist antes de commit
- [ ] Código compila sin errores
- [ ] Tests pasan (`npm test` o `dotnet test`)
- [ ] Linter pasa (`npm run lint` para AsistenciaPro)
- [ ] No hay advertencias críticas
- [ ] Se agregaron/actualizaron tests si es necesario
- [ ] Documentación actualizada si aplica

### Convenciones de código

#### AsistenciaPro
- **TypeScript strict mode**: Habilitado en `tsconfig.json`
- **Rutas API**: Usar Next.js App Router (`src/app/api/*/route.ts`)
- **Componentes**: React Server Components cuando sea posible
- **Validaciones**: Usar Zod para validar payloads de API
- **Manejo de errores**: Retornar códigos HTTP apropiados y mensajes claros

#### TagleLabsGestorSST
- **C# convenciones**: Seguir guías de estilo de C#
- **MVVM**: No poner lógica de negocio en code-behind de Views
- **Async/await**: Usar correctamente para operaciones asíncronas
- **Manejo de errores**: Usar try-catch y mostrar mensajes al usuario

## Convenciones Específicas del Proyecto

### Zona horaria
- **Fija**: `America/Santiago` en todo el backend
- No configurar zona horaria por empresa
- Helpers en `AsistenciaPro/src/lib/datetime.ts` y `AsistenciaPro/src/lib/timezone.ts`

### Multi-tenant
- Cada request debe validar `companyId` y rol antes de acceder a datos
- Usar `assertRole()` y `getSession()` de `src/lib/auth.ts` en AsistenciaPro
- Nunca exponer datos de una empresa a otra

### Repositorios SQL
- **Sin Prisma**: Usar repositorios SQL directos en `src/lib/repos/*`
- Pool de conexiones en `src/lib/db.ts` (pg)
- Cliente Supabase REST en `src/lib/supabase.ts` para operaciones simples

### Integración con IA
- **TagleLabsGestorSST**: `LocalAiService` con soporte para Gemini (principal)
- Temperatura configurable según caso de uso (0.2 para análisis legal, 0.7 para creatividad)
- Modelos dinámicos: `ObtenerModelosDisponiblesAsync()`, `SetModeloAsync()`
- Fallback opcional: Claude y Groq (requieren API keys configuradas)
- Embeddings: Usa `text-embedding-004` de Gemini para búsqueda semántica (RAG)

### Reconocimiento facial (AsistenciaPro)
- Modelos en `public/face-models/`
- Descriptores almacenados en tabla `EmployeeFace` (TEXT)
- Cálculos locales con `@vladmandic/face-api` + TensorFlow
- Endpoints: `GET/POST /api/kiosk/[slug]/faces`

### Base de datos
- **AsistenciaPro**: PostgreSQL/Supabase
- **TagleLabsGestorSST**: SQLite (`TagleLabs.db`)
- Migraciones SQL en `AsistenciaPro/prisma/migrations/` (aplicar con `psql`)

## Recursos Adicionales

- **Documentación AsistenciaPro**: `AsistenciaPro/README.md` y `AsistenciaPro/ai.md`
- **Análisis de IA**: `ANALISIS_OLLAMA_BRECHAS_Y_MEJORAS.md`
- **Roadmap**: `HOJA_DE_RUTA_MEJORAS_FUTURAS.md`
- **Plan de Implementación Detallado**: `PLAN_MEJORAS_IMPLEMENTACION.md` - Plan de acción estructurado con pasos específicos, código de ejemplo y criterios de aceptación para implementar mejoras prioritarias
- **Documentación maestra**: `Docs/MASTER_COMPENDIO.md`

## Notas Importantes

- **No usar Prisma**: El proyecto AsistenciaPro fue migrado completamente a Supabase nativo
- **PWA**: El kiosco es una PWA instalable con service worker
- **Realtime**: Habilitar Realtime en Supabase para tabla `TimeRecord` para sincronización de kioscos
- **Branding**: Tagle Labs - usar assets en `AsistenciaPro/public/tagle-labs-*`
- **Contacto**: WhatsApp +56 9 5680 4513, cristian.gonzalez.gt@gmail.com

