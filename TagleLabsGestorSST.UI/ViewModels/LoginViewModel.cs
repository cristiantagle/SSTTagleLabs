using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ISessionService _sessionService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public LoginViewModel(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [RelayCommand]
    private void Login()
    {
        ErrorMessage = string.Empty;
        HasError = false;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Ingrese usuario y contraseña";
            HasError = true;
            return;
        }

        // Simulación de autenticación
        // En producción esto iría contra la base de datos (tabla Usuarios con hash)
        bool isValid = false;
        string rol = "";
        string nombre = "";

        if (Username.ToLower() == "admin" && Password == "admin")
        {
            isValid = true;
            rol = "Administrador";
            nombre = "Administrador del Sistema";
        }
        else if (Username.ToLower() == "prevencionista" && Password == "1234")
        {
            isValid = true;
            rol = "Prevencionista";
            nombre = "Juan Pérez (APR)";
        }

        if (isValid)
        {
            _sessionService.IniciarSesion(Username, rol, nombre);
            // La navegación se maneja via evento en MainViewModel
        }
        else
        {
            ErrorMessage = "Credenciales incorrectas";
            HasError = true;
        }
    }
}
