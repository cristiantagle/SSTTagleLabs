using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;

namespace TagleLabsGestorSST.UI.Services;

/// <summary>
/// Service for managing application theme (Light/Dark mode).
/// Updates both Material Design theme and custom app brushes.
/// </summary>
public class ThemeService
{
    private readonly PaletteHelper _paletteHelper = new();
    
    public bool IsDarkMode { get; private set; }

    /// <summary>
    /// Toggles between Light and Dark theme.
    /// </summary>
    public void ToggleTheme()
    {
        SetTheme(!IsDarkMode);
    }

    /// <summary>
    /// Sets the theme to Light or Dark.
    /// </summary>
    public void SetTheme(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        
        // Update Material Design theme
        var theme = _paletteHelper.GetTheme();
        theme.SetBaseTheme(isDarkMode ? BaseTheme.Dark : BaseTheme.Light);
        _paletteHelper.SetTheme(theme);
        
        // Update custom app brushes
        UpdateAppBrushes(isDarkMode);
    }

    private void UpdateAppBrushes(bool isDarkMode)
    {
        var app = Application.Current;
        if (app?.Resources == null) return;

        if (isDarkMode)
        {
            // Dark Mode Colors
            app.Resources["BrushFondo"] = new SolidColorBrush(Color.FromRgb(15, 23, 42));      // Slate 900
            app.Resources["BrushCardBackground"] = new SolidColorBrush(Color.FromRgb(30, 41, 59));  // Slate 800
            app.Resources["BrushTextPrimary"] = new SolidColorBrush(Color.FromRgb(248, 250, 252)); // Slate 50
            app.Resources["BrushTextSecondary"] = new SolidColorBrush(Color.FromRgb(148, 163, 184)); // Slate 400
            app.Resources["BrushBorder"] = new SolidColorBrush(Color.FromRgb(51, 65, 85));    // Slate 700
        }
        else
        {
            // Light Mode Colors
            app.Resources["BrushFondo"] = new LinearGradientBrush(
                Color.FromRgb(238, 242, 247), 
                Color.FromRgb(248, 250, 252), 
                45);
            app.Resources["BrushCardBackground"] = new SolidColorBrush(Colors.White);
            app.Resources["BrushTextPrimary"] = new SolidColorBrush(Color.FromRgb(15, 23, 42));   // Slate 900
            app.Resources["BrushTextSecondary"] = new SolidColorBrush(Color.FromRgb(100, 116, 139)); // Slate 500
            app.Resources["BrushBorder"] = new SolidColorBrush(Color.FromRgb(226, 232, 240));  // Slate 200
        }
    }
}
