using System;

namespace TagleLabsGestorSST.Data.Entities;

public class LogAuditoria
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Accion { get; set; } = string.Empty; // Crear, Editar, Eliminar
    public string Entidad { get; set; } = string.Empty; // Empresa, Trabajador, etc.
    public string EntidadId { get; set; } = string.Empty; // ID del registro afectado
    public string Detalle { get; set; } = string.Empty; // Descripción legible (ej. "Se creó empresa X")
    public string Usuario { get; set; } = "Sistema"; // Por ahora hardcoded, futuro: usuario logueado
}
