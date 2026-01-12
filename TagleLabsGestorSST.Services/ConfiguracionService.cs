using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public interface IConfiguracionService
{
    Task<string?> GetValorAsync(string clave);
    Task SetValorAsync(string clave, string valor);
}

public class ConfiguracionService : IConfiguracionService
{
    private readonly TagleLabsContext _db;

    public ConfiguracionService(TagleLabsContext db)
    {
        _db = db;
    }

    public async Task<string?> GetValorAsync(string clave)
    {
        var config = await _db.Configuraciones
            .FirstOrDefaultAsync(c => c.Clave == clave);
        return config?.Valor;
    }

    public async Task SetValorAsync(string clave, string valor)
    {
        var config = await _db.Configuraciones
            .FirstOrDefaultAsync(c => c.Clave == clave);

        if (config == null)
        {
            config = new Configuracion { Clave = clave, Valor = valor };
            _db.Configuraciones.Add(config);
        }
        else
        {
            config.Valor = valor;
            _db.Configuraciones.Update(config);
        }

        await _db.SaveChangesAsync();
    }
}
