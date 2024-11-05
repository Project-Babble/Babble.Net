using Babble.Maui.Locale;

namespace Babble.Maui.Scripts;

internal static class LocaleExtensions
{
    internal static void SetLocalizedText(Label textBlock, string localeKey, string? tooltipKey = null)
    {
        textBlock.Text = LocaleManager.Instance[localeKey];
        if (tooltipKey != null)
        {
            ToolTipProperties.SetText(textBlock, LocaleManager.Instance[tooltipKey]);
        }
    }
}
