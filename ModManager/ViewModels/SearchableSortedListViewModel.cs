using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using ModManager.Models;
using ReactiveUI;

namespace ModManager.ViewModels;

public class SearchableSortedModListViewModel : ReactiveObject, IDisposable
{
    private string _searchText = string.Empty;
    private readonly SourceList<Mod> _sourceList;
    private ReadOnlyObservableCollection<Mod> _sortedList = null!;
    private IDisposable _sortedListDisposable = null!;
    private readonly IComparer<Mod> _comparer;

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }
    public ReadOnlyObservableCollection<Mod> SortedList => _sortedList;

    public SearchableSortedModListViewModel(SourceList<Mod> sourceList, IComparer<Mod> comparer)
    {
        _sourceList = sourceList;
        _comparer = comparer;

        var searchTextFilter = this
            .WhenAnyValue(x => x.SearchText)
            .Select(searchText => (Func<Mod, bool>)(mod =>
                    string.IsNullOrWhiteSpace(searchText)
                    || mod.DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                ))
            .StartWith(mod => true);

        _sortedListDisposable = _sourceList
            .Connect()
            .Filter(searchTextFilter)
            .Sort(_comparer)
            .Bind(out _sortedList)
            .Subscribe();
    }

    public void Dispose()
    {
        _sortedListDisposable.Dispose();
    }
}