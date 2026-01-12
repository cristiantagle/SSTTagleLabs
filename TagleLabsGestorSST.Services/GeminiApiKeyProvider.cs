using System;
using System.IO;

namespace TagleLabsGestorSST.Services
{
    public static class GeminiApiKeyProvider
    {
        private const string EnvKey = "GEMINI_API_KEY";
        private const string FileName = "gemini.key";

        /// <summary>
        /// Devuelve la API key desde variable de entorno GEMINI_API_KEY o desde el archivo gemini.key copiado con el instalador.
        /// </summary>
        public static string? GetApiKey()
        {
            var fromEnv = Environment.GetEnvironmentVariable(EnvKey);
            if (!string.IsNullOrWhiteSpace(fromEnv))
                return fromEnv!.Trim();

            try
            {
                // Busca el archivo junto al ejecutable.
                var baseDir = AppContext.BaseDirectory;
                var path = Path.Combine(baseDir, FileName);
                if (File.Exists(path))
                {
                    var key = File.ReadAllText(path).Trim();
                    return string.IsNullOrWhiteSpace(key) ? null : key;
                }
            }
            catch
            {
                // Ignorar errores de lectura y devolver null
            }

            return null;
        }
    }
}
