using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ModManager.Models;
using Tmds.DBus.Protocol;

namespace ModManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private class ModFolderNameComparer : Comparer<Mod>
    {
        public override int Compare(Mod? x, Mod? y)
        {
            if (x == null || y == null) return 0;
            return string.Compare(x.FolderName, y.FolderName, StringComparison.OrdinalIgnoreCase);
        }
    }

    private ModFolderNameComparer _comparer = new();

    private Settings _settings;
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

    public MainWindowViewModel()
    {
        _settings = new Settings();
    }
    public MainWindowViewModel(Settings settings)
    {
        _settings = settings;
        if (!string.IsNullOrEmpty(_settings.GamePath))
        {
            UpdateModsList(_settings.GamePath);
        }
    }

    public ObservableCollection<Mod> EnabledMods { get; private set; } = new();
    public ObservableCollection<Mod> DisabledMods { get; private set; } = new();

    public void UpdateModsList(string basePath)
    {
        var modsFolderPath = Path.Combine(basePath, "mods");
        var mods = Mod.GetModsList(modsFolderPath);

        EnabledMods = new ObservableCollection<Mod>(mods.Where(m => m.Enabled));
        DisabledMods = new ObservableCollection<Mod>(mods.Where(m => !m.Enabled));
        
        OnPropertyChanged(nameof(EnabledMods));
        OnPropertyChanged(nameof(DisabledMods));
    }
    
    public void DisableMods(IEnumerable<Mod> mods)
    {
        foreach (var mod in mods)
        {
            mod.Enabled = false;
            int index = EnabledMods.BinarySearch(mod, _comparer);
            if (index >= 0)
            {
                EnabledMods.Remove(mod);
            }
            DisabledMods.BinaryInsert(mod, _comparer);
        }
        OnPropertyChanged(nameof(EnabledMods));
        OnPropertyChanged(nameof(DisabledMods));
    }
    
    public void EnableMods(IEnumerable<Mod> mods)
    {
        foreach (var mod in mods)
        {
            mod.Enabled = true;
            int index = DisabledMods.BinarySearch(mod, _comparer);
            if (index >= 0)
            {
                DisabledMods.Remove(mod);
            }
            EnabledMods.BinaryInsert(mod, _comparer);
        }
        OnPropertyChanged(nameof(EnabledMods));
        OnPropertyChanged(nameof(DisabledMods));
    }

    public void ApplyMods()
    {
        var modsFolderPath = Path.Combine(GamePath, "mods");
        foreach (var mod in EnabledMods)
        {
            var disablePath = Path.Combine(modsFolderPath, mod.FolderName, "disable.it");
            if (File.Exists(disablePath))
            {
                File.Delete(disablePath);
            }
        }
        foreach (var mod in DisabledMods)
        {
            var disablePath = Path.Combine(modsFolderPath, mod.FolderName, "disable.it");
            if (!File.Exists(disablePath))
            {
                File.Create(disablePath).Close();
            }
        }
    }
}
