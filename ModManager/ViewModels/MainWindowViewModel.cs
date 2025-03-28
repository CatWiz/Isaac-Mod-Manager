﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ModManager.Models;

namespace ModManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string _selectedPath = string.Empty;
    public string SelectedPath
    {
        get => string.IsNullOrEmpty(_selectedPath) ? "" : _selectedPath;
        set => SetProperty(ref _selectedPath, value);
    }
    
    public ObservableCollection<Mod> EnabledMods { get; private set; } = new();
    public ObservableCollection<Mod> DisabledMods { get; private set; } = new();

    public void UpdateModsList(string basePath)
    {
        var modsFolderPath = Path.Combine(basePath, "mods");
        var mods = Mod.GetModsList(modsFolderPath);
        
        EnabledMods.Clear();
        DisabledMods.Clear();

        foreach (var mod in mods)
        {
            if (mod.Enabled)
            {
                EnabledMods.Add(mod);
            }
            else
            {
                DisabledMods.Add(mod);
            }
        }
    }
    
    public void DisableMods(IEnumerable<Mod> mods)
    {
        foreach (var mod in mods)
        {
            mod.Enabled = false;
            EnabledMods.Remove(mod);
            DisabledMods.Add(mod);
        }
    }
    
    public void EnableMods(IEnumerable<Mod> mods)
    {
        foreach (var mod in mods)
        {
            mod.Enabled = true;
            DisabledMods.Remove(mod);
            EnabledMods.Add(mod);
        }
    }
}
