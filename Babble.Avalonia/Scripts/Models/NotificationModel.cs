namespace Babble.Avalonia.Scripts.Models;

public class NotificationModel
{
    public string Title;
    public string Body;
    public string? BodyImagePath;
    public string? BodyAltText;
    public List<(string Title, string ActionId)?> ActionButtons;
    public DateTimeOffset? OptionalScheduledTime;
    public DateTimeOffset? OptionalExpirationTime;
}