using System.Net;

namespace Babble.Avalonia.Scripts;

internal class Validation
{
    internal static bool IsIpValid(string ip) => IPAddress.TryParse(ip, out _);
    internal static bool IsPortValid(int port) => port > 8000;
    internal static bool IsGreaterThanZero(int num) => num > 0;
    internal static bool IsGreaterThanZero(float num) => num > 0;
    internal static bool IsGreaterThanZero(double num) => num > 0;
}
