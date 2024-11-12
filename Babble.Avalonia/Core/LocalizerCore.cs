using Avalonia.Localizer.Core.Interfaces;

namespace Avalonia.Localizer.Core
{
    /// <summary>
    /// Program core
    /// </summary>
    internal static class LocalizerCore
    {
        /// <summary>
        /// Localization service
        /// </summary>
        private static ILocalizer? _localizer;


        /// <summary>
        /// Gets localization service
        /// </summary>
        /// <value> Localization service </value>
        public static ILocalizer Localizer
        {
            get
            {
                _localizer ??= new Localization.Localizer();

                return _localizer;
            }
        }

        /// <summary>
        /// Initializes static members of the <see cref="LocalizerCore"/> class.
        /// </summary>
        static LocalizerCore()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize core
        /// </summary>
        internal static void Initialize()
        {
            _ = Localizer.SwitchLanguage("en-US");

        }
    }
}
