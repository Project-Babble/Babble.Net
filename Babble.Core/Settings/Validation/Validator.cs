namespace Babble.Maui.Scripts.Validation;

internal abstract class Validator<T> where T : struct
{
    public static T Validate(string str)
    {
        if (string.IsNullOrEmpty(null))
            throw new ArgumentException("Invalid event args passed.");

        if (ParseValue<T>(str, out var newValue))
        {
            return newValue;
        }
        else
        {
            throw new ArgumentException();
        }
    }

    private static bool ParseValue<T>(string value, out T parsed) where T : struct
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            parsed = default;
            return false;
        }

        switch (Type.GetTypeCode(typeof(T)))
        {
            case TypeCode.Int32:
                if (int.TryParse(value, out var intValue))
                {
                    parsed = (T)(object)intValue;
                    return true;
                }
                else
                {
                    parsed = default;
                    return false;
                }
            case TypeCode.Double:
                if (double.TryParse(value, out var doubleValue))
                {
                    parsed = (T)(object)doubleValue;
                    return true;
                }
                else
                {
                    parsed = default;
                    return false;
                }
            case TypeCode.Decimal:
                if (decimal.TryParse(value, out var decimalValue))
                {
                    parsed = (T)(object)decimalValue;
                    return true;
                }
                else
                {
                    parsed = default;
                    return false;
                }

            // Add more cases as needed

            default:
                throw new NotSupportedException($"Type '{typeof(T)}' is not supported.");
        }
    }
}
