using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ModManager.Models;
using Tmds.DBus.Protocol;

namespace ModManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private ModFolderNameComparer _comparer = new();

    private Settings _settings;
    private Mod? _lastSelectedMod;
    public Settings Settings => _settings;

    public string GamePath
    {
        get => _settings.GamePath;
        set
        {
            _settings.GamePath = value;
            OnPropertyChanged();
        }
    }

    public Mod? LastSelectedMod
    {
        get => _lastSelectedMod;
        set
        {
            if (Equals(value, _lastSelectedMod)) return;
            _lastSelectedMod = value;
            OnPropertyChanged();
        }
    }

    private readonly SourceList<Mod> _enabledMods = new();
    private readonly SourceList<Mod> _disabledMods = new();

    private ReadOnlyObservableCollection<Mod> _sortedEnabledMods = null!;
    private ReadOnlyObservableCollection<Mod> _sortedDisabledMods = null!;
    
    public ReadOnlyObservableCollection<Mod> SortedEnabledMods => _sortedEnabledMods;
    public ReadOnlyObservableCollection<Mod> SortedDisabledMods => _sortedDisabledMods;
    
    private IDisposable _sortedEnabledModsDisposable = null!;
    private IDisposable _sortedDisabledModsDisposable = null!;

    private void Initialize()
    {
        _sortedEnabledModsDisposable = _enabledMods
            .Connect()
            .Sort(_comparer)
            .Bind(out _sortedEnabledMods)
            .Subscribe();
        
        _sortedDisabledModsDisposable = _disabledMods
            .Connect()
            .Sort(_comparer)
            .Bind(out _sortedDisabledMods)
            .Subscribe();
    }
    
    public MainWindowViewModel()
    {
        _settings = new Settings();
        Initialize();
    }
    public MainWindowViewModel(Settings settings)
    {
        _settings = settings;
        if (!string.IsNullOrEmpty(_settings.GamePath))
        {
            UpdateModsList();
        }
        Initialize();
    }

    
    #region ModManagementCommands
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

    public void LoadModList(IEnumerable<string> modFolders)
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
        _sortedEnabledModsDisposable.Dispose();
        _sortedDisabledModsDisposable.Dispose();
        _enabledMods.Dispose();
        _disabledMods.Dispose();
    }
}
