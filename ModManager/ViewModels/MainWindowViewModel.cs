using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ModManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
    
    private string _selectedPath = string.Empty;

    public string SelectedPath
    {
        get => string.IsNullOrEmpty(_selectedPath) ? "No path selected" : _selectedPath;
        set => SetProperty(ref _selectedPath, value);
    }
}
