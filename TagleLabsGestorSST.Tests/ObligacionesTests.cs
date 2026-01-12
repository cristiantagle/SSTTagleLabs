using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.Tests;

public class ObligacionesTests
{
    private TagleLabsContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<TagleLabsContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        var ctx = new TagleLabsContext(options);
        ctx.Database.OpenConnection();
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task AsignarObligaciones_CreaRegistrosPendientes()
    {
        using var ctx = BuildContext();
        var mockAuditoria = new MockAuditoriaService();
        var service = new ObligacionesService(ctx, mockAuditoria);
        await service.AsignarObligacionesAsync(1);

        var obligaciones = ctx.ObligacionesEmpresa.Where(o => o.EmpresaId == 1).ToList();
        obligaciones.Should().NotBeEmpty();
        obligaciones.Should().OnlyContain(o => o.Estado == Data.Entities.EstadoObligacion.Pendiente);
    }

    private class MockAuditoriaService : IAuditoriaService
    {
        public Task RegistrarAccionAsync(string accion, string entidad, string entidadId, string detalle, string? usuario = null)
        {
            return Task.CompletedTask;
        }
    }
}
