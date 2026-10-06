using System.Text.Json;

namespace CoordinacionOrientadores.Services
{
    // Almacenamiento simple basado en archivos JSON (sin necesidad de un motor de base de datos
    // ni de paquetes NuGet externos). Suficiente para el volumen de datos de un programa
    // universitario de Orientación (un puñado de orientadores y sus procesos por cuatrimestre).
    public class JsonStore<T>
    {
        private readonly string _filePath;
        private readonly object _lock = new();
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true
        };

        public JsonStore(string fileName, IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, fileName);
        }

        public List<T> LoadAll()
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath))
                {
                    return new List<T>();
                }

                var json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<T>();
                }

                return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
            }
        }

        public void SaveAll(List<T> items)
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(items, Options);
                File.WriteAllText(_filePath, json);
            }
        }
    }
}
