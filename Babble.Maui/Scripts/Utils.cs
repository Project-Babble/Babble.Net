namespace Babble.Maui.Scripts;

/// <summary>
/// Utility class with a number of useful constants and methods
/// </summary>
public static class Utils
{
    public const int THREAD_TIMEOUT_MS = 10;

    /// <summary>
    /// Gets all enums as an IEnumerable
    /// https://stackoverflow.com/questions/1398664/enum-getvalues-return-type
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <returns></returns>
    public static IEnumerable<TEnum> EnumerateEnum<TEnum>()
        where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        }
}
