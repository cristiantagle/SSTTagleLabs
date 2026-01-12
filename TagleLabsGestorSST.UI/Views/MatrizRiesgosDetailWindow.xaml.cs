using System.Windows;

namespace TagleLabsGestorSST.UI.Views;

    public partial class MatrizRiesgosDetailWindow : Window
{
        public TagleLabsGestorSST.UI.ViewModels.MatrizRiesgosViewModel? ParentViewModel { get; set; }
    private string _originalSnapshotJson = string.Empty;

    public MatrizRiesgosDetailWindow()
    {
        InitializeComponent();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        // ensure we unlock the item in parent VM when window is closed
        try
        {
            if (ParentViewModel != null && this.DataContext is TagleLabsGestorSST.Data.Entities.MatrizRiesgoEmpresa it)
            {
                ParentViewModel.UnlockItem(it.Id);
            }
        }
        catch { }
        this.Close();
    }

    public void CaptureOriginalSnapshot()
    {
        try
        {
            if (this.DataContext is TagleLabsGestorSST.Data.Entities.MatrizRiesgoEmpresa item && item.Id != 0)
            {
                _originalSnapshotJson = Newtonsoft.Json.JsonConvert.SerializeObject(item);
            }
            else
            {
                _originalSnapshotJson = string.Empty;
            }
        }
        catch { _originalSnapshotJson = string.Empty; }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        // Save changes by asking parent VM to persist the current item
        try
        {
            if (ParentViewModel != null && this.DataContext is TagleLabsGestorSST.Data.Entities.MatrizRiesgoEmpresa item)
            {
                // Delegate to parent's ConfirmAndSaveFromChildAsync to handle conflicts and persistence
                var saved = await ParentViewModel.ConfirmAndSaveFromChildAsync(item, _originalSnapshotJson);
                if (!saved)
                {
                    // user cancelled or save failed - just unlock and return
                    ParentViewModel.UnlockItem(item.Id);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error guardando: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Ensure unlock on close as well
            try { if (ParentViewModel != null && this.DataContext is TagleLabsGestorSST.Data.Entities.MatrizRiesgoEmpresa it) ParentViewModel.UnlockItem(it.Id); } catch { }
            this.Close();
        }
    }
}
