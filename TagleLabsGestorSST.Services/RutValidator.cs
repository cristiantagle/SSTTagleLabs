namespace TagleLabsGestorSST.Services;

/// <summary>
/// Validador de RUT chileno usando algoritmo módulo 11
/// </summary>
public static class RutValidator
{
    /// <summary>
    /// Valida un RUT chileno completo (ej: "12.345.678-5" o "12345678-5")
    /// </summary>
    public static bool EsValido(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut))
            return false;

        // Limpiar RUT: remover puntos y guiones, convertir a mayúsculas
        var rutLimpio = rut.Replace(".", "").Replace("-", "").ToUpperInvariant().Trim();

        if (rutLimpio.Length < 2)
            return false;

        // Separar cuerpo y dígito verificador
        var cuerpo = rutLimpio[..^1];
        var digitoVerificadorIngresado = rutLimpio[^1];

        // Validar que el cuerpo sea numérico
        if (!long.TryParse(cuerpo, out var numeroCuerpo) || numeroCuerpo <= 0)
            return false;

        // Calcular dígito verificador
        var digitoCalculado = CalcularDigitoVerificador(cuerpo);

        return digitoVerificadorIngresado == digitoCalculado;
    }

    /// <summary>
    /// Calcula el dígito verificador de un RUT usando módulo 11
    /// </summary>
    public static char CalcularDigitoVerificador(string cuerpo)
    {
        // Algoritmo módulo 11
        var suma = 0;
        var multiplicador = 2;

        // Recorrer de derecha a izquierda
        for (var i = cuerpo.Length - 1; i >= 0; i--)
        {
            suma += (cuerpo[i] - '0') * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        var resto = 11 - (suma % 11);

        return resto switch
        {
            11 => '0',
            10 => 'K',
            _ => (char)('0' + resto)
        };
    }

    /// <summary>
    /// Formatea un RUT a formato estándar (12.345.678-9)
    /// </summary>
    public static string Formatear(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut))
            return string.Empty;

        var rutLimpio = rut.Replace(".", "").Replace("-", "").ToUpperInvariant().Trim();

        if (rutLimpio.Length < 2)
            return rut;

        var cuerpo = rutLimpio[..^1];
        var dv = rutLimpio[^1];

        // Formatear con puntos
        if (long.TryParse(cuerpo, out var numero))
        {
            return $"{numero:N0}".Replace(",", ".") + "-" + dv;
        }

        return rut;
    }

    /// <summary>
    /// Obtiene mensaje de error si el RUT es inválido
    /// </summary>
    public static string? ObtenerMensajeError(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut))
            return "El RUT es requerido";

        var rutLimpio = rut.Replace(".", "").Replace("-", "").ToUpperInvariant().Trim();

        if (rutLimpio.Length < 2)
            return "El RUT es muy corto";

        var cuerpo = rutLimpio[..^1];

        if (!long.TryParse(cuerpo, out var numero) || numero <= 0)
            return "El RUT contiene caracteres inválidos";

        if (!EsValido(rut))
            return "El dígito verificador no es correcto";

        return null; // Sin errores
    }
}
