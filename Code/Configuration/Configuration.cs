using System.IO;
using System.Text.Json;
using Godot;

public static class Configuration
{
    private static ConfigValues _configValues;
    public static ConfigValues ConfigValues { get => _configValues; }

    public static void SetUpConfiguration()
    {
        var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration.json");
        string configJsonString = "";
        try
        {
            configJsonString = File.ReadAllText(configFilePath);
        }
        catch
        {
            GD.PrintErr($"Couldn't read file at {configFilePath}");
        }
        _configValues = JsonSerializer.Deserialize<ConfigValues>(configJsonString)!;
        GD.Print($"Configuration setup with {JsonSerializer.Serialize(_configValues)}");
    }
}

public record ConfigValues(
    bool IsDebug,
    int ScoreToWin,
    int NumOfStartingDice,
    int ScoreTriesPerRound,
    int RerollsPerStage
);