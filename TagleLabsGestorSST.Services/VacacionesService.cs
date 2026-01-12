using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public interface IVacacionesService
{
    Task<decimal> CalcularDiasGanadosAsync(int trabajadorId);
    Task<decimal> CalcularDiasUsadosAsync(int trabajadorId);
    Task<decimal> ObtenerSaldoAsync(int trabajadorId);
    Task<SolicitudVacaciones> RegistrarSolicitudAsync(SolicitudVacaciones solicitud);
    Task<List<MovimientoVacaciones>> ObtenerMovimientosAsync(int trabajadorId);
    Task<List<SolicitudVacaciones>> ObtenerSolicitudesAsync(int trabajadorId);
}

public class VacacionesService : IVacacionesService
{
    private readonly TagleLabsContext _context;

    public VacacionesService(TagleLabsContext context)
    {
        _context = context;
    }

    public async Task<decimal> CalcularDiasGanadosAsync(int trabajadorId)
    {
        var trabajador = await _context.Trabajadores.FindAsync(trabajadorId);
        if (trabajador == null) return 0;

        // Cálculo base: 1.25 días por mes trabajado
        var fechaIngreso = trabajador.FechaIngreso;
        var fechaCalculo = DateTime.Now;

        // Diferencia en meses
        var meses = ((fechaCalculo.Year - fechaIngreso.Year) * 12) + fechaCalculo.Month - fechaIngreso.Month;
        
        // Ajuste por día del mes (si no ha cumplido el mes completo, no se cuenta el último)
        if (fechaCalculo.Day < fechaIngreso.Day)
        {
            meses--;
        }

        if (meses < 0) meses = 0;

        decimal diasGanados = (decimal)(meses * 1.25);

        // TODO: Agregar lógica para días progresivos (vacaciones adicionales por antigüedad)
        
        return diasGanados;
    }

    public async Task<decimal> CalcularDiasUsadosAsync(int trabajadorId)
    {
        // Sumar todos los movimientos negativos (cargos)
        // Nota: SQLite no soporta Sum sobre decimales directamente en todas las versiones/configuraciones
        // Traemos los datos a memoria primero
        var movimientos = await _context.MovimientosVacaciones
            .Where(m => m.TrabajadorId == trabajadorId && m.Dias < 0)
            .ToListAsync();

        var total = movimientos.Sum(m => m.Dias);

        return Math.Abs(total); // Retornar positivo para visualización
    }

    public async Task<decimal> ObtenerSaldoAsync(int trabajadorId)
    {
        var ganados = await CalcularDiasGanadosAsync(trabajadorId);
        
        // Sumar todos los movimientos (Cargos son negativos, Ajustes pueden ser +/-)
        var movimientos = await _context.MovimientosVacaciones
            .Where(m => m.TrabajadorId == trabajadorId)
            .ToListAsync();

        var totalMovimientos = movimientos.Sum(m => m.Dias);

        // Saldo = Ganados (teóricos) + Movimientos (que incluyen los descuentos)
        return ganados + totalMovimientos;
    }

    public async Task<SolicitudVacaciones> RegistrarSolicitudAsync(SolicitudVacaciones solicitud)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Guardar Solicitud
            _context.SolicitudesVacaciones.Add(solicitud);
            await _context.SaveChangesAsync();

            // 2. Crear Movimiento de Cargo (Descuento)
            var movimiento = new MovimientoVacaciones
            {
                TrabajadorId = solicitud.TrabajadorId,
                FechaMovimiento = DateTime.Now,
                Tipo = TipoMovimientoVacaciones.CargoSolicitud,
                Dias = -solicitud.DiasHabiles, // Negativo para descontar
                Descripcion = $"Solicitud de vacaciones del {solicitud.FechaInicio:dd/MM/yyyy} al {solicitud.FechaFin:dd/MM/yyyy}",
                SolicitudVacacionesId = solicitud.Id
            };

            _context.MovimientosVacaciones.Add(movimiento);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return solicitud;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<MovimientoVacaciones>> ObtenerMovimientosAsync(int trabajadorId)
    {
        return await _context.MovimientosVacaciones
            .Where(m => m.TrabajadorId == trabajadorId)
            .OrderByDescending(m => m.FechaMovimiento)
            .ToListAsync();
    }

    public async Task<List<SolicitudVacaciones>> ObtenerSolicitudesAsync(int trabajadorId)
    {
        return await _context.SolicitudesVacaciones
            .Where(s => s.TrabajadorId == trabajadorId)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();
    }
}
