using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    private MainWindowViewModel Vm => DataContext as MainWindowViewModel ?? throw new System.InvalidCastException();
    public MainWindow()
    {
        InitializeComponent();
        
        Closed += (sender, args) =>
        {
            Vm.Settings.Save(Settings.DefaultPath);
        };
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
            if (!string.IsNullOrEmpty(selectedPath))
            {
                selectedPath = selectedPath.Replace("%20", " ");
                var gameExecutablePath = Path.Combine(selectedPath, "isaac-ng.exe");
                Vm.GamePath = File.Exists(gameExecutablePath) ? selectedPath : string.Empty;
                Vm.UpdateModsList(Vm.GamePath);
            }
        }
        catch (InvalidOperationException exception)
        {
            // show a message box
            var box = MessageBoxManager.GetMessageBoxStandard(
            "Error",
            "Invalid path"
            );
            await box.ShowAsync();
        }
    }
    
    private void RefreshModsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Vm.GamePath) || Vm.GamePath == "Invalid path")
        {
            // show a message box
            var box = MessageBoxManager.GetMessageBoxStandard(
            "No path",
            "Specify a valid path to the game folder"
            );
            box.ShowAsync();
            return;
        }
        Vm.UpdateModsList(Vm.GamePath);
    }

    private void DisableModButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectedModsIndexes = EnabledModsListBox.Selection.SelectedIndexes;
        var selectedMods = selectedModsIndexes.Select(index => Vm.EnabledMods[index]).ToList();
        Vm.DisableMods(selectedMods);
        Vm.LastSelectedMod = null;
    }

    private void EnableModButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectedModsIndexes = DisabledModsListBox.Selection.SelectedIndexes;
        var selectedMods = selectedModsIndexes.Select(index => Vm.DisabledMods[index]).ToList();
        Vm.EnableMods(selectedMods);
        Vm.LastSelectedMod = null;
    }

    private void ApplyModsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Vm.ApplyMods();
    }

    private void EnabledModsListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            DisabledModsListBox.Selection.Clear();
            var lastSelected = EnabledModsListBox.SelectedItems?.OfType<Mod>().Last();
            if (lastSelected != null)
            {
                Vm.LastSelectedMod = lastSelected;
            }
        }
    }

    private void DisabledModsListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            EnabledModsListBox.Selection.Clear();
            var lastSelected = DisabledModsListBox.SelectedItems?.OfType<Mod>().Last();
            if (lastSelected != null)
            {
                Vm.LastSelectedMod = lastSelected;
            }
        }
    }
}