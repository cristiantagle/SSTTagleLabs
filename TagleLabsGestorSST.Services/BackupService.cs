using System;
using System.IO;

namespace TagleLabsGestorSST.Services;

public class BackupService
{
    private readonly string _dbPath;

    public BackupService(string dbPath)
    {
        _dbPath = dbPath;
    }

    public Task<string> CrearBackupAsync(string destinoCarpeta, CancellationToken ct = default)
    {
        Directory.CreateDirectory(destinoCarpeta);
        var destino = Path.Combine(destinoCarpeta, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        File.Copy(_dbPath, destino, overwrite: true);
        return Task.FromResult(destino);
    }

    public Task RestaurarBackupAsync(string backupPath, CancellationToken ct = default)
    {
        if (!File.Exists(backupPath)) throw new FileNotFoundException("Backup no encontrado", backupPath);
        File.Copy(backupPath, _dbPath, overwrite: true);
        return Task.CompletedTask;
    }
}
