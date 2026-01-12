-- Script para poblar la base de datos con datos reales de "JUAN GONZALEZ VALENZUELA SPA"
-- Basado en RESUMEN_IMPLEMENTACION_DS44.txt

-- 1. Actualizar Empresa Principal (o insertar si no existe)
INSERT OR REPLACE INTO Empresas (Id, Rut, RazonSocial, Giro, RubroPrincipal, NumeroTrabajadores, Mutual, Direccion, Region, Ciudad, Telefono, EmailContacto, RepresentanteLegal)
VALUES (1, '76.825.693-4', 'JUAN GONZALEZ VALENZUELA SPA', 'Servicios de mantención y reparación industrial', 'Servicios Industriales', 15, 'ACHS', 'Pedro de Miranda #183, Lo Miranda', 'O''Higgins', 'Donihue', '+56 9 1234 5678', 'contacto@jgvalenzuela.cl', 'Juan González Valenzuela');

-- 2. Asegurar Centro de Trabajo
INSERT OR REPLACE INTO CentrosTrabajo (Id, EmpresaId, Nombre, Direccion, Region, Ciudad, Sector)
VALUES (1, 1, 'Taller Principal', 'Pedro de Miranda #183, Lo Miranda', 'O''Higgins', 'Donihue', 'Operaciones');

-- 3. Insertar Rubro y Actividades Reales
INSERT OR IGNORE INTO Rubros (Id, Codigo, Nombre, Descripcion) VALUES (2, 'IND-MANT', 'Servicios Industriales', 'Mantención y reparación de maquinaria y equipos industriales');

INSERT OR IGNORE INTO Actividades (Id, RubroId, Nombre, Descripcion) VALUES 
(2, 2, 'Soldadura y Corte', 'Procesos de unión y corte de metales'),
(3, 2, 'Mantenimiento Mecánico', 'Reparación y ajuste de maquinaria'),
(4, 2, 'Logística y Carga', 'Movimiento manual y mecánico de materiales'),
(5, 2, 'Uso de Herramientas', 'Operación de herramientas manuales y eléctricas'),
(6, 2, 'Trabajos en Altura', 'Labores sobre 1.8 metros');

-- 4. Insertar Tareas Específicas
INSERT OR IGNORE INTO Tareas (Id, ActividadId, Nombre, Descripcion) VALUES
(3, 2, 'Soldadura al arco', 'Unión de piezas metálicas mediante arco eléctrico'),
(4, 3, 'Desarme de equipos', 'Desmontaje de componentes mecánicos para revisión'),
(5, 4, 'Carga manual de materiales', 'Levantamiento y traslado de sacos y cajas'),
(6, 5, 'Esmerilado angular', 'Uso de galletera para desbaste y corte'),
(7, 6, 'Reparación de techumbres', 'Cambio de planchas de zinc en altura');

-- 5. Insertar Peligros Reales (DS44 / ACHS)
INSERT OR IGNORE INTO Peligros (Id, Categoria, Descripcion) VALUES
(4, 'Químico', 'Humos metálicos de soldadura'),
(5, 'Físico', 'Radiación UV (arco eléctrico)'),
(6, 'Mecánico', 'Atrapamiento por partes móviles'),
(7, 'Mecánico', 'Cortes con herramientas o bordes filosos'),
(8, 'Ergonómico', 'Trastornos Musculoesqueléticos (TME) por sobreesfuerzo'),
(9, 'Eléctrico', 'Contacto eléctrico directo o indirecto'),
(10, 'Gravedad', 'Caída a distinto nivel');

-- 6. Insertar Controles (Jerarquía)
INSERT OR IGNORE INTO Controles (Id, Tipo, Descripcion) VALUES
(4, 'Ingeniería', 'Sistema de extracción localizada de humos'),
(5, 'EPP', 'Máscara de soldar, coleto, polainas y guantes de cuero'),
(6, 'Administrativo', 'Procedimiento de bloqueo y etiquetado (LOTO)'),
(7, 'EPP', 'Guantes anticorte nivel 5'),
(8, 'Ingeniería', 'Uso de ayudas mecánicas (tecles, carretillas)'),
(9, 'Administrativo', 'Inspección pre-uso de herramientas y cables'),
(10, 'Ingeniería', 'Líneas de vida y puntos de anclaje certificados'),
(11, 'EPP', 'Arnés de seguridad de cuerpo completo con doble cabo');

-- 7. Poblar Matriz de Riesgos (Ejemplos)
-- Soldadura -> Humos -> Extracción (Riesgo Medio)
INSERT OR IGNORE INTO MatrizRiesgoEmpresa (CentroTrabajoId, TareaId, PeligroId, ControlId, Probabilidad, Consecuencia, NivelRiesgo, MedidaControlEspecifica)
VALUES (1, 3, 4, 4, 3, 3, 'Medio', 'Instalar brazo extractor articulado en puesto de soldadura');

-- Soldadura -> UV -> EPP (Riesgo Bajo si se usa bien)
INSERT OR IGNORE INTO MatrizRiesgoEmpresa (CentroTrabajoId, TareaId, PeligroId, ControlId, Probabilidad, Consecuencia, NivelRiesgo, MedidaControlEspecifica)
VALUES (1, 3, 5, 5, 2, 4, 'Medio', 'Uso obligatorio de EPP completo y biombos para proteger entorno');

-- Mantenimiento -> Atrapamiento -> LOTO (Riesgo Alto sin control)
INSERT OR IGNORE INTO MatrizRiesgoEmpresa (CentroTrabajoId, TareaId, PeligroId, ControlId, Probabilidad, Consecuencia, NivelRiesgo, MedidaControlEspecifica)
VALUES (1, 4, 6, 6, 2, 5, 'Alto', 'Aplicación estricta de protocolo de bloqueo de energías peligrosas');

-- Altura -> Caída -> Arnés (Riesgo Crítico)
INSERT OR IGNORE INTO MatrizRiesgoEmpresa (CentroTrabajoId, TareaId, PeligroId, ControlId, Probabilidad, Consecuencia, NivelRiesgo, MedidaControlEspecifica)
VALUES (1, 7, 10, 11, 3, 5, 'Alto', 'Uso de SPDC permanente y supervisión constante');

