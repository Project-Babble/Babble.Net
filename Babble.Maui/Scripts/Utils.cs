using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Babble.Maui.Scripts;

internal static class Utils
{
    internal const int THREAD_TIMEOUT_MS = 10;

    // https://stackoverflow.com/questions/1398664/enum-getvalues-return-type
    public static IEnumerable<TEnum> EnumerateEnum<TEnum>()
        where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        }
}
