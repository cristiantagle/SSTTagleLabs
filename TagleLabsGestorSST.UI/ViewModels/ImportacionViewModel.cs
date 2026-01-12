using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class ImportacionViewModel : ObservableObject
{
    private readonly ImportacionService _importacionService;
    private readonly IEmpresaService _empresaService;

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();
    [ObservableProperty]
    private Empresa? _empresaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<CentroTrabajo> _centrosTrabajo = new();
    [ObservableProperty]
    private CentroTrabajo? _centroTrabajoSeleccionado;

    [ObservableProperty]
    private string _rutaArchivo = string.Empty;

    [ObservableProperty]
    private string _resultadoTexto = string.Empty;

    [ObservableProperty]
    private bool _estaProcesando;

    public ImportacionViewModel(ImportacionService importacionService, IEmpresaService empresaService)
    {
        _importacionService = importacionService;
        _empresaService = empresaService;
        CargarDatosInicialesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task CargarDatosIniciales()
    {
        var empresas = await _empresaService.ObtenerEmpresasAsync();
        Empresas = new ObservableCollection<Empresa>(empresas);
        if (Empresas.Any()) EmpresaSeleccionada = Empresas.First();
    }

    partial void OnEmpresaSeleccionadaChanged(Empresa? value)
    {
        if (value != null)
        {
            CentrosTrabajo = new ObservableCollection<CentroTrabajo>(value.CentrosTrabajo);
            if (CentrosTrabajo.Any()) CentroTrabajoSeleccionado = CentrosTrabajo.First();
        }
        else
        {
            CentrosTrabajo.Clear();
        }
    }

    [RelayCommand]
    private async Task ImportarTrabajadores()
    {
        if (string.IsNullOrWhiteSpace(RutaArchivo) || CentroTrabajoSeleccionado == null)
        {
            ResultadoTexto = "Por favor seleccione un archivo y un centro de trabajo.";
            return;
        }

        EstaProcesando = true;
        ResultadoTexto = "Procesando archivo...";

        try
        {
            var resultado = await _importacionService.ImportarTrabajadoresAsync(RutaArchivo, CentroTrabajoSeleccionado.Id);
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Proceso finalizado.");
            sb.AppendLine($"Total filas: {resultado.TotalProcesados}");
            sb.AppendLine($"Insertados: {resultado.Insertados}");
            sb.AppendLine($"Actualizados: {resultado.Actualizados}");
            sb.AppendLine($"Errores: {resultado.Errores}");
            
            if (resultado.Errores > 0)
            {
                sb.AppendLine("\nDetalle de Errores:");
                foreach (var err in resultado.MensajesError.Take(10)) // Mostrar primeros 10
                {
                    sb.AppendLine($"- {err}");
                }
                if (resultado.MensajesError.Count > 10)
                    sb.AppendLine("... y más errores.");
            }

            ResultadoTexto = sb.ToString();
        }
        catch (Exception ex)
        {
            ResultadoTexto = $"Error crítico: {ex.Message}";
        }
        finally
        {
            EstaProcesando = false;
        }
    }

    // ============================================
    // IMPORTACIÓN DE EMPRESAS DESDE EXCEL
    // ============================================
    
    [ObservableProperty]
    private string _rutaArchivoEmpresas = string.Empty;

    [ObservableProperty]
    private string _resultadoEmpresasTexto = string.Empty;

    [RelayCommand]
    private async Task ImportarEmpresas()
    {
        if (string.IsNullOrWhiteSpace(RutaArchivoEmpresas))
        {
            ResultadoEmpresasTexto = "Por favor seleccione un archivo Excel.";
            return;
        }

        EstaProcesando = true;
        ResultadoEmpresasTexto = "Procesando archivo de empresas...";

        try
        {
            var resultado = await _importacionService.ImportarEmpresasAsync(RutaArchivoEmpresas);
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"✅ Proceso finalizado.");
            sb.AppendLine($"📊 Total filas: {resultado.TotalProcesados}");
            sb.AppendLine($"➕ Insertadas: {resultado.Insertados}");
            sb.AppendLine($"🔄 Actualizadas: {resultado.Actualizados}");
            sb.AppendLine($"❌ Errores: {resultado.Errores}");
            
            if (resultado.Errores > 0)
            {
                sb.AppendLine("\n⚠️ Detalle de Errores:");
                foreach (var err in resultado.MensajesError.Take(10))
                {
                    sb.AppendLine($"  • {err}");
                }
                if (resultado.MensajesError.Count > 10)
                    sb.AppendLine("  ... y más errores.");
            }

            ResultadoEmpresasTexto = sb.ToString();
            
            // Recargar lista de empresas
            await CargarDatosIniciales();
        }
        catch (Exception ex)
        {
            ResultadoEmpresasTexto = $"❌ Error crítico: {ex.Message}";
        }
        finally
        {
            EstaProcesando = false;
        }
    }
}
