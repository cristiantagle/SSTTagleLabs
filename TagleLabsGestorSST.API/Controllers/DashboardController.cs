using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;

namespace TagleLabsGestorSST.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly TagleLabsContext _context;

    public DashboardController(TagleLabsContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        // Obtener contadores básicos para el dashboard
        var totalEmpresas = await _context.Empresas.CountAsync();
        var totalTrabajadores = await _context.Trabajadores.CountAsync();
        var totalDocumentos = await _context.DocumentosGenerados.CountAsync();
        
        // Alertas: Contar registros de auditoría de tipo 'Error' o 'Warning' recientes (ejemplo)
        // O simplemente contar Matrices de Riesgo activas
        var totalMatrices = await _context.MatrizRiesgosEmpresa.CountAsync();

        return Ok(new 
        {
            Empresas = totalEmpresas,
            Trabajadores = totalTrabajadores,
            Documentos = totalDocumentos,
            Matrices = totalMatrices
        });
    }
}
