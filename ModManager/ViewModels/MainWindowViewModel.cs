using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using ModManager.Models;
using ReactiveUI;

namespace ModManager.ViewModels;

public class MainWindowViewModel : ReactiveObject, IDisposable
{
    private readonly ModFolderNameComparer _comparer = new();

    private readonly Settings _settings;
    private Mod? _lastSelectedMod;
    public Settings Settings => _settings;

    public string GamePath
    {
        get => _settings.GamePath;
        set
        {
            _settings.GamePath = value;
            this.RaisePropertyChanged();
        }
    }

    public Mod? LastSelectedMod
    {
        get => _lastSelectedMod;
        set => this.RaiseAndSetIfChanged(ref _lastSelectedMod, value);
    }

    private readonly SourceList<Mod> _enabledMods = new();
    private readonly SourceList<Mod> _disabledMods = new();

    public SearchableSortedModListViewModel SortedEnabledModsVm { get; }
    public SearchableSortedModListViewModel SortedDisabledModsVm { get; }
    
    public MainWindowViewModel()
    {
        _settings = new Settings();
        SortedEnabledModsVm = new SearchableSortedModListViewModel(_enabledMods, _comparer);
        SortedDisabledModsVm = new SearchableSortedModListViewModel(_disabledMods, _comparer);
    }
    public MainWindowViewModel(Settings settings)
    {
        _settings = settings;
        SortedEnabledModsVm = new SearchableSortedModListViewModel(_enabledMods, _comparer);
        SortedDisabledModsVm = new SearchableSortedModListViewModel(_disabledMods, _comparer);
    }

    
    #region ModManagementCommands
    
    public bool IsValidGamePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (!Directory.Exists(path)) return false;
        
        var gameExecutablePath = Path.Combine(path, "isaac-ng.exe");
        if (!File.Exists(gameExecutablePath)) return false;
        
        var modsFolderPath = Path.Combine(path, "mods");
        if (!Directory.Exists(modsFolderPath)) return false;
        
        return true;
    }
    public void UpdateModsList()
    {
        var modsFolderPath = Settings.ModsPath;
        var mods = Mod.GetModsList(modsFolderPath);
        
        _enabledMods.Clear();
        _disabledMods.Clear();
        foreach (var mod in mods)
        {
            var disablePath = Path.Combine(modsFolderPath, mod.FolderName, "disable.it");
            if (!File.Exists(disablePath))
            {
                _enabledMods.Add(mod);
            }
            else
            {
                _disabledMods.Add(mod);
            }
        }
    }
    
    public void DisableMods(IEnumerable<Mod> mods)
    {
        foreach (var mod in mods)
        {
            if (_enabledMods.Items.Contains(mod, _comparer))
            {
                _enabledMods.Remove(mod);
            }
            _disabledMods.Add(mod);
        }
    }
    
    public void EnableMods(IEnumerable<Mod> mods)
    {
        foreach (var mod in mods)
        {
            if (_disabledMods.Items.Contains(mod, _comparer))
            {
                _disabledMods.Remove(mod);
            }
            _enabledMods.Add(mod);
        }
    }

    public void ApplyMods()
    {
        var modsFolderPath = Path.Combine(GamePath, "mods");
        foreach (var mod in _enabledMods.Items)
        {
            var disablePath = Path.Combine(modsFolderPath, mod.FolderName, "disable.it");
            if (File.Exists(disablePath))
            {
                File.Delete(disablePath);
            }
        }
        foreach (var mod in _disabledMods.Items)
        {
            var disablePath = Path.Combine(modsFolderPath, mod.FolderName, "disable.it");
            if (!File.Exists(disablePath))
            {
                File.Create(disablePath).Close();
            }
        }
    }

    private void LoadModList(IEnumerable<string> modFolders)
    {
        _enabledMods.Clear();
        _disabledMods.Clear();

        var modsToEnable = new HashSet<string>(modFolders);
        var modsList = Mod.GetModsList(Settings.ModsPath);
        foreach (var mod in modsList)
        {
            if (modsToEnable.Contains(mod.FolderName))
            {
                _enabledMods.Add(mod);
            }
            else
            {
                _disabledMods.Add(mod);                
            }
        }
    }

    public void LoadModList(Stream stream)
    {
        var reader = new StreamReader(stream);
        var modFolders = reader.ReadToEnd().Split(Environment.NewLine);
        LoadModList(modFolders.ToList());        
    }

    private void SaveModList(IEnumerable<Mod> mods, Stream stream)
    {
        var modsStr = string.Join(Environment.NewLine, mods.Select(mod => mod.FolderName).Order());
        
        using var writer = new StreamWriter(stream);
        writer.Write(modsStr);
    }

    public void SaveCurrentModList(Stream stream)
    {
        SaveModList(_enabledMods.Items, stream);
    }
    #endregion

    public void Dispose()
    {
        SortedEnabledModsVm.Dispose();
        SortedDisabledModsVm.Dispose();
        _enabledMods.Dispose();
        _disabledMods.Dispose();
    }
}
