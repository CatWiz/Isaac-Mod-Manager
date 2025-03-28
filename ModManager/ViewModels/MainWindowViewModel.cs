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
    private string _selectedPath = string.Empty;
    private ObservableCollection<Mod> _enabledMods = new();
    private ObservableCollection<Mod> _disabledMods = new();

    public string SelectedPath
    {
        get => string.IsNullOrEmpty(_selectedPath) ? "" : _selectedPath;
        set => SetProperty(ref _selectedPath, value);
    }

    public ObservableCollection<Mod> EnabledMods
    {
        get => _enabledMods;
        private set
        {
            _enabledMods = new ObservableCollection<Mod>(value.OrderBy(m => m.FolderName));
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Mod> DisabledMods
    {
        get => _disabledMods;
        private set
        {
            _disabledMods = new ObservableCollection<Mod>(value.OrderBy(m => m.FolderName));
            OnPropertyChanged();
        }
    }

    public void UpdateModsList(string basePath)
    {
        var modsFolderPath = Path.Combine(basePath, "mods");
        var mods = Mod.GetModsList(modsFolderPath);

        EnabledMods = new ObservableCollection<Mod>(mods.Where(m => m.Enabled));
        DisabledMods = new ObservableCollection<Mod>(mods.Where(m => !m.Enabled));
    }
    
    public void DisableMods(IEnumerable<Mod> mods)
    {
        foreach (var mod in mods)
        {
            mod.Enabled = false;
            EnabledMods.Remove(mod);
            DisabledMods.Add(mod);
        }
        // Trigger the OnPropertyChanged event
        DisabledMods = DisabledMods;
    }
    
    public void EnableMods(IEnumerable<Mod> mods)
    {
        foreach (var mod in mods)
        {
            mod.Enabled = true;
            DisabledMods.Remove(mod);
            EnabledMods.Add(mod);
        }
        // Trigger the OnPropertyChanged event
        EnabledMods = EnabledMods;
    }
}
