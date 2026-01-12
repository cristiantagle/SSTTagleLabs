using System.Windows;
using System.Windows.Controls;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;
using TagleLabsGestorSST.UI.ViewModels;

namespace TagleLabsGestorSST.UI.Views;

public partial class EmpresasView : UserControl
{
    public EmpresasView()
    {
        InitializeComponent();
        DataContextChanged += EmpresasView_DataContextChanged;
    }

    private void EmpresasView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (DataContext is EmpresasViewModel vm)
        {
            vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(vm.EmpresaSeleccionada))
                {
                    ActualizarSeleccionRubros();
                    // Validar RUT inicial al cambiar empresa
                    if (TxtRut != null)
                    {
                        ValidarRut(TxtRut.Text);
                    }
                }
            };
        }
    }

    private void ActualizarSeleccionRubros()
    {
        if (DataContext is not EmpresasViewModel vm || vm.EmpresaSeleccionada == null) return;
        
        RubrosListBox.SelectionChanged -= RubrosListBox_SelectionChanged;
        RubrosListBox.SelectedItems.Clear();
        
        foreach (var rubro in vm.EmpresaSeleccionada.Rubros)
        {
            var match = vm.Rubros.FirstOrDefault(r => r.Id == rubro.Id);
            if (match != null)
            {
                RubrosListBox.SelectedItems.Add(match);
            }
        }
        
        RubrosListBox.SelectionChanged += RubrosListBox_SelectionChanged;
    }

    private void RubrosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not EmpresasViewModel vm || vm.EmpresaSeleccionada == null) return;

        // Validar máximo 3 rubros
        if (RubrosListBox.SelectedItems.Count > 3)
        {
            MessageBox.Show("Puede seleccionar máximo 3 rubros.", "Límite alcanzado", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            
            // Revertir la última selección
            foreach (var item in e.AddedItems)
            {
                RubrosListBox.SelectedItems.Remove(item);
            }
            return;
        }

        // Sincronizar con la colección de la empresa
        vm.EmpresaSeleccionada.Rubros.Clear();
        foreach (Rubro rubro in RubrosListBox.SelectedItems)
        {
            vm.EmpresaSeleccionada.Rubros.Add(rubro);
        }
    }

    private void TxtRut_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox txt)
        {
            ValidarRut(txt.Text);
        }
    }

    private void ValidarRut(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut))
        {
            IconRutValid.Visibility = Visibility.Collapsed;
            IconRutInvalid.Visibility = Visibility.Collapsed;
            TxtRutError.Visibility = Visibility.Collapsed;
            return;
        }

        var error = RutValidator.ObtenerMensajeError(rut);
        
        if (error == null)
        {
            // RUT válido
            IconRutValid.Visibility = Visibility.Visible;
            IconRutInvalid.Visibility = Visibility.Collapsed;
            TxtRutError.Visibility = Visibility.Collapsed;
        }
        else
        {
            // RUT inválido
            IconRutValid.Visibility = Visibility.Collapsed;
            IconRutInvalid.Visibility = Visibility.Visible;
            TxtRutError.Text = error;
            TxtRutError.Visibility = Visibility.Visible;
        }
    }
}

