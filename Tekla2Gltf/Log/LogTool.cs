using System;
using System.IO;

namespace ExportTekla2Gltf
{
  public class Logger
  {
    private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
    private readonly string _logFilePath;
    private static readonly object _lock = new object();

    private Logger()
    {
      _logFilePath = GetLogFilePath();
      EnsureLogDirectoryExists();
      DeleteExistingLogFile();
    }

    public static Logger GetInstance() => _instance.Value;

    public void Log(string message, LogLevel level = LogLevel.Info)
    {
      string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
      WriteLogEntry(logEntry);
    }

    public void Info(string message) => Log(message, LogLevel.Info);
    public void Warning(string message) => Log(message, LogLevel.Warning);
    public void Error(string message) => Log(message, LogLevel.Error);
    public void Debug(string message) => Log(message, LogLevel.Debug);

    private string GetLogFilePath()
    {
      return Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
          "Tekla2Gltf",
          "Tekla2Gltf.log");
    }

    private void EnsureLogDirectoryExists()
    {
      string logDirectory = Path.GetDirectoryName(_logFilePath);
      Directory.CreateDirectory(logDirectory);
    }

    private void DeleteExistingLogFile()
    {
      if (File.Exists(_logFilePath))
      {
        File.Delete(_logFilePath);
      }
    }

    private void WriteLogEntry(string logEntry)
    {
      lock (_lock)
      {
        try
        {
          using (StreamWriter sw = new StreamWriter(_logFilePath, true))
          {
            sw.WriteLine(logEntry);
          }
        }
        catch (Exception)
        {
          // Consider adding some form of error handling or reporting here
        }
      }
    }
  }

  public enum LogLevel
  {
    Info,
    Warning,
    Error,
    Debug
  }
}