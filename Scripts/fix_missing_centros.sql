-- Script para agregar centros de trabajo a empresas existentes que no los tienen
INSERT INTO CentrosTrabajo (Nombre, Direccion, Region, Ciudad, EmpresaId)
SELECT 
    'Sede Principal' as Nombre,
    COALESCE(Direccion, 'Sin dirección especificada') as Direccion,
    'Región Metropolitana' as Region,
    'Santiago' as Ciudad,
    Id as EmpresaId
FROM Empresas e
WHERE NOT EXISTS (
    SELECT 1 FROM CentrosTrabajo ct WHERE ct.EmpresaId = e.Id
);
