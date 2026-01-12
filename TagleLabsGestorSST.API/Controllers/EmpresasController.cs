using Microsoft.AspNetCore.Mvc;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresasController : ControllerBase
{
    private readonly IEmpresaService _empresaService;

    public EmpresasController(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
    {
        var empresas = await _empresaService.ObtenerEmpresasAsync();
        return Ok(empresas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Empresa>> GetEmpresa(int id)
    {
        var empresa = await _empresaService.ObtenerPorIdAsync(id);
        if (empresa == null) return NotFound();
        return Ok(empresa);
    }

    [HttpPost]
    public async Task<ActionResult<Empresa>> CreateEmpresa(Empresa empresa)
    {
        try 
        {
            var created = await _empresaService.CrearEmpresaAsync(empresa);
            return CreatedAtAction(nameof(GetEmpresa), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmpresa(int id, Empresa empresa)
    {
        if (id != empresa.Id) return BadRequest(new { message = "ID mismatch" });
        
        try
        {
            await _empresaService.ActualizarEmpresaAsync(empresa);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmpresa(int id)
    {
        await _empresaService.EliminarEmpresaAsync(id);
        return NoContent();
    }
}
