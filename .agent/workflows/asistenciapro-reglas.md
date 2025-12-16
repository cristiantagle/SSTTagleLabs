---
description: Reglas de desarrollo para AsistenciaPro - SIEMPRE consultar antes de cualquier desarrollo
---

# Reglas de Oro - AsistenciaPro

## 🚫 NO USAR PRISMA

> **CRÍTICO:** Prisma fue removido completamente del proyecto porque causó problemas graves en el pasado.

### Qué NO hacer:
- ❌ `npx prisma migrate`
- ❌ `npx prisma generate`
- ❌ `@prisma/client`
- ❌ Cualquier comando o dependencia de Prisma

### Qué SÍ hacer:
- ✅ Repositorios SQL propios en `src/lib/repos/`
- ✅ Migraciones SQL manuales en `prisma/migrations/*/migration.sql` (solo los archivos .sql)
- ✅ Aplicar migraciones con `psql` o Supabase SQL Editor

---

## Base de Datos

- **Motor:** PostgreSQL via Supabase
- **ORM:** Ninguno - SQL directo con `pg` (node-postgres)
- **Migraciones:** Archivos `.sql` aplicados manualmente

### Aplicar nueva migración:
```bash
# Opción 1: Supabase SQL Editor (recomendado)
# Copiar contenido de migration.sql y ejecutar en el dashboard

# Opción 2: psql directo
psql $DATABASE_URL -f prisma/migrations/20251216120000_audit_log/migration.sql
```

---

## Estructura de Repositorios

```
src/lib/repos/
├── employees.ts      # Trabajadores
├── companies.ts      # Empresas
├── time-records.ts   # Marcaciones
├── fuel-records.ts   # Combustible
├── vehicles.ts       # Vehículos
├── audit.ts          # Historial de cambios
└── ...
```

Cada repo exporta funciones que usan `runQuery` y `runSingle` de `@/lib/db`.
