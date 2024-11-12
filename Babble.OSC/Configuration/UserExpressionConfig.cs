using Babble.OSC.Enums;
using System.Text.Json;

namespace Babble.OSC.Configuration;

public class UserExpressionConfig
{
    public Dictionary<string, ExpressionPriority> ExpressionPriorities { get; set; } = new();

    public void Save(string filePath)
    {
        var json = JsonSerializer.Serialize(this);
        File.WriteAllText(filePath, json);
    }

    public static UserExpressionConfig Load(string filePath)
    {
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<UserExpressionConfig>(json) ?? new UserExpressionConfig();
        }
        return new UserExpressionConfig();
    }
}
