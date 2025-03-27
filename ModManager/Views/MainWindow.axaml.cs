using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ModManager.ViewModels;

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
                Vm.SelectedPath = selectedPath;
            }
        }
        catch (InvalidOperationException exception)
        {
            Vm.SelectedPath = "Invalid path";
        }
    }
}