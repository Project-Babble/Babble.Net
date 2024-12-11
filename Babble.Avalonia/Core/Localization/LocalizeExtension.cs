using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Avalonia.Localizer.Core.Localization
{
    /// <summary>
    /// Markup extension for localization in XAML designers
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="LocalizeExtension"/> class.
    /// </remarks>
    /// <param name="key"> Key </param>
    internal sealed class LocalizeExtension(string key) : MarkupExtension
    {

        /// <summary>
        /// Gets or sets the localization key
        /// </summary>
        /// <value>The localization key.</value>
        public string Key { get; set; } = key;

        /// <summary>
        /// Gets or sets the context
        /// </summary>
        /// <value> The context </value>
        public string? Context { get; set; }

        /// <summary>
        /// Provide the localized value
        /// </summary>
        /// <param name="serviceProvider"> Service provider </param>
        /// <returns> Localized value </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var keyToUse = Key;

            if (!string.IsNullOrWhiteSpace(Context))
            {
                keyToUse = $"{Context}/{Key}";
            }

            var binding = new ReflectionBindingExtension($"[{keyToUse}]")
            {
                Mode = BindingMode.OneWay,
                Source = LocalizerCore.Localizer
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
