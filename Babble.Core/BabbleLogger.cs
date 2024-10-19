using Babble.Core.Enums;

namespace Babble.Core;

public class BabbleLogger
{
    private readonly BabbleLogger _instance;
    private readonly string _logFilePath;
    private readonly LogLevel _minLogLevel;

    public BabbleLogger(string logFilePath, LogLevel minLogLevel = LogLevel.Info)
    {
        if (_instance != null)
            throw new InvalidOperationException();

        _instance = this;
        _logFilePath = logFilePath;
        _minLogLevel = minLogLevel;
    }

    public void Log(LogLevel level, string message)
    {
        if (level >= _minLogLevel)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            Console.WriteLine(logEntry);
            // File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
    }
}
