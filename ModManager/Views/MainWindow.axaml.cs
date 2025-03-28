using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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
                Vm.SelectedPath = File.Exists(gameExecutablePath) ? selectedPath : "Invalid path";
            }
        }
        catch (InvalidOperationException exception)
        {
            Vm.SelectedPath = "Invalid path";
        }
    }
    
    private void RefreshModsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Vm.SelectedPath) || Vm.SelectedPath == "Invalid path")
        {
            // show a message box
            var box = MessageBoxManager.GetMessageBoxStandard(
            "No path",
            "Specify a valid path to the game folder"
            );
            box.ShowAsync();
            return;
        }
        Vm.UpdateModsList(Vm.SelectedPath);
    }

    private void DisableModButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectedModsIndexes = EnabledModsListBox.Selection.SelectedIndexes;
        var selectedMods = selectedModsIndexes.Select(index => Vm.EnabledMods[index]).ToList();
        Vm.DisableMods(selectedMods);
    }

    private void EnableModButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectedModsIndexes = DisabledModsListBox.Selection.SelectedIndexes;
        var selectedMods = selectedModsIndexes.Select(index => Vm.DisabledMods[index]).ToList();
        Vm.EnableMods(selectedMods);
    }
}