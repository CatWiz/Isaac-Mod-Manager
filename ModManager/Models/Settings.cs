using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModManager.Models;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Settings))]
public partial class SettingsJsonSerializerContext : JsonSerializerContext
{
    
}

public sealed class Settings : INotifyPropertyChanged
{
    [JsonIgnore]
    public static readonly string DefaultStoragePath = Path.Combine(AppContext.BaseDirectory, "settings.json");
    
    [JsonIgnore]
    public static readonly string ModListPath = Path.Combine(AppContext.BaseDirectory, "modlists");
    
    [JsonIgnore]
    public string ModsPath => Path.Combine(GamePath, "mods");
    
    private string _gamePath = string.Empty;
    private bool _applyModlistOnLoad = true;

    [JsonInclude]
    public string GamePath
    {
        get => _gamePath;
        set
        {
            if (value == _gamePath) return;
            _gamePath = value;
            OnPropertyChanged();
        }
    }
    
    [JsonInclude]
    public bool ApplyModlistOnLoad
    {
        get => _applyModlistOnLoad;
        set
        {
            if (value == _applyModlistOnLoad) return;
            _applyModlistOnLoad = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public static Settings Load(string path)
    {
        if (!File.Exists(path))
        {
            return new Settings();
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Settings>(json, SettingsJsonSerializerContext.Default.Settings) ?? new Settings();
    }
    
    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this, SettingsJsonSerializerContext.Default.Settings);
        File.WriteAllText(path, json);
    }
}