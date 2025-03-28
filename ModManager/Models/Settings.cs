using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModManager.Models;

[JsonSerializable(typeof(Settings))]
public class Settings : INotifyPropertyChanged
{
    [JsonIgnore]
    public static string DefaultPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
    private string _gamePath = string.Empty;

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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public static Settings Load(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            return new Settings();
        }

        var json = System.IO.File.ReadAllText(path);
        return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
    }
    
    public void Save(string path)
    {
        var json = JsonSerializer.Serialize(this);
        System.IO.File.WriteAllText(path, json);
    }
}