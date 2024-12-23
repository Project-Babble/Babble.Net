using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Avalonia.Localizer.Core.Converters
{
    /// <summary>
    /// Converter to use strings in XAML. Support localization.
    /// </summary>
    internal class StringLocalizationConverter : IValueConverter
    {
        /// <inheritdoc/>
        object IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string str)
            {
                throw new NotSupportedException("Value should be enum.");
            }

            return LocalizerCore.Localizer[str];
        }

        /// <inheritdoc/>
        object IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
