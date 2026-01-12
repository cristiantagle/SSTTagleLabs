using System;

namespace TagleLabsGestorSST.Services;

public class UsuarioSesion
{
    public string Username { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
}

public interface ISessionService
{
    UsuarioSesion? UsuarioActual { get; }
    bool IsLoggedIn { get; }
    void IniciarSesion(string username, string rol, string nombre);
    void CerrarSesion();
    event Action? OnLoginSuccess;
    event Action? OnLogout;
}

public class SessionService : ISessionService
{
    public UsuarioSesion? UsuarioActual { get; private set; }

    public bool IsLoggedIn => UsuarioActual != null;
    
    public event Action? OnLoginSuccess;
    public event Action? OnLogout;

    public void IniciarSesion(string username, string rol, string nombre)
    {
        UsuarioActual = new UsuarioSesion
        {
            Username = username,
            Rol = rol,
            NombreCompleto = nombre
        };
        OnLoginSuccess?.Invoke();
    }

    public void CerrarSesion()
    {
        UsuarioActual = null;
        OnLogout?.Invoke();
    }
}
