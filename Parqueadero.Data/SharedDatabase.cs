namespace Parqueadero.Data;

public static class SharedDatabase
{
    public static string GetConnectionString()
    {
        var configuredPath = Environment.GetEnvironmentVariable("PARQUEADERO_DB");
        var databasePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ParqueaderoUSC",
                "parqueadero.db")
            : Path.GetFullPath(configuredPath);

        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return $"Data Source={databasePath}";
    }
}
