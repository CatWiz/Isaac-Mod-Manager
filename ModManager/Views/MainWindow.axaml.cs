using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ModManager.Models;
using ModManager.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ModManager.Views;

public partial class MainWindow : Window
{
    private readonly FilePickerFileType[] _openModlistFilePickerFileTypes =
    {
        new("Text files") { Patterns = new [] {"*.txt"} }
    };
    
    private readonly MainWindowViewModel _vm;
    public MainWindow(MainWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = _vm = vm;
        
        Closing += (sender, args) =>
        {
            _vm.Settings.Save(Settings.DefaultStoragePath);
        };
        
        if (vm.IsValidGamePath(vm.Settings.GamePath))
        {
            try
            {
                _vm.UpdateModsList();
            }
            catch (Exception e)
            {
                MessageBoxManager.GetMessageBoxStandard(
                "Error",
                "An error occurred while updating the mods list: " + e.Message,
                    icon: MsBox.Avalonia.Enums.Icon.Error
                ).ShowAsync();
            }
        }
    }

    private async void SelectGamePathButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var storageProvider = StorageProvider;
        if (!storageProvider.CanPickFolder) return;
        
        var selection = await storageProvider.OpenFolderPickerAsync(new()
        {
            Title = "Select the game folder",
            AllowMultiple = false,
        });

        var storageFolder = selection.FirstOrDefault();

        try
        {
            var selectedPath = storageFolder?.Path.AbsolutePath;
            if (string.IsNullOrEmpty(selectedPath)) return;
            
            selectedPath = selectedPath.Replace("%20", " ");
            if (_vm.IsValidGamePath(selectedPath))
            {
                _vm.GamePath = selectedPath;
                try
                {
                    _vm.UpdateModsList();
                }
                catch (Exception exception)
                {
                    await MessageBoxManager.GetMessageBoxStandard(
                        "Error",
                        "An error occurred while updating the mods list: " + exception.Message,
                            icon: MsBox.Avalonia.Enums.Icon.Error
                    ).ShowAsync();
                }
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Invalid path",
                    "The selected path is not a valid game folder",
                        icon: MsBox.Avalonia.Enums.Icon.Error
                ).ShowAsync();
            }
        }
        catch (InvalidOperationException exception)
        {
            await MessageBoxManager.GetMessageBoxStandard(
            "Error",
            "Invalid operation: " + exception.Message,
                icon: MsBox.Avalonia.Enums.Icon.Error
            ).ShowAsync();
        }
    }
    
    private void RefreshModsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_vm.GamePath))
        {
            MessageBoxManager.GetMessageBoxStandard(
            "No path",
            "Specify a valid path to the game folder",
                icon: MsBox.Avalonia.Enums.Icon.Warning
            ).ShowAsync();
            return;
        }
        _vm.UpdateModsList();
    }

    private void DisableModButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectedMods = EnabledModsListBox.Selection.SelectedItems
            .OfType<Mod>()
            .ToList();
        
        _vm.DisableMods(selectedMods);
        _vm.LastSelectedMod = null;
    }

    private void EnableModButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectedMods = DisabledModsListBox.Selection.SelectedItems
            .OfType<Mod>()
            .ToList();
        
        _vm.EnableMods(selectedMods);
        _vm.LastSelectedMod = null;
    }

    private void ApplyModsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _vm.ApplyMods();
    }

    private void EnabledModsListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count <= 0) return;
        
        DisabledModsListBox.Selection.Clear();
        var lastSelected = EnabledModsListBox.SelectedItems?.OfType<Mod>().Last();
        if (lastSelected != null)
        {
            _vm.LastSelectedMod = lastSelected;
        }
    }

    private void DisabledModsListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count <= 0) return;
        
        EnabledModsListBox.Selection.Clear();
        var lastSelected = DisabledModsListBox.SelectedItems?.OfType<Mod>().Last();
        if (lastSelected != null)
        {
            _vm.LastSelectedMod = lastSelected;
        }
    }

    private async void SaveModListButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var provider = StorageProvider;
        if (!provider.CanSave) return;

        if (!Directory.Exists(Settings.ModListPath))
        {
            Directory.CreateDirectory(Settings.ModListPath);
        }

        var modListFolder = await provider.TryGetFolderFromPathAsync(Settings.ModListPath);
        var storageFile = await provider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save the mod list",
            DefaultExtension = "txt",
            ShowOverwritePrompt = true,
            SuggestedStartLocation = modListFolder,
            SuggestedFileName = "modlist"
        });
        
        if (storageFile == null) return;

        await using var writeStream = await storageFile.OpenWriteAsync();
        _vm.SaveCurrentModList(writeStream);
    }

    private async void LoadModListButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var defaultPath = Settings.ModListPath;
        if (!Directory.Exists(defaultPath))
        {
            defaultPath = Settings.DefaultStoragePath;
        }
        
        var provider = StorageProvider;
        if (!provider.CanOpen) return;
        
        var modListFolder = await provider.TryGetFolderFromPathAsync(defaultPath);
        var selection = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Select the mod list",
            AllowMultiple = false,
            SuggestedStartLocation = modListFolder,
            FileTypeFilter = _openModlistFilePickerFileTypes
        });

        var storageFile = selection.FirstOrDefault();
        if (storageFile == null) return;
        
        await using var readStream = await storageFile.OpenReadAsync();
        _vm.LoadModList(readStream);
    }

    private void ClearEnabledSearchQueryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _vm.SortedEnabledModsVm.SearchText = string.Empty;
    }
    private void ClearDisabledSearchQueryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _vm.SortedDisabledModsVm.SearchText = string.Empty;
    }

}