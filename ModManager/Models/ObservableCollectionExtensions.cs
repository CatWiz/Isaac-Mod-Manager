using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

internal static class ObservableStaticCollectionExtensions
{
    public static bool BinaryRemove<T>(this ObservableCollection<T> collection, T value, IComparer<T> comparer)
    {
        int index = collection.BinarySearch(value, comparer);
        if (index < 0)
        {
            return false;
        }

        collection.RemoveAt(index);
        return true;
    }
    
    public static void BinaryInsert<T>(this ObservableCollection<T> collection, T value, IComparer<T> comparer)
    {
        int index = collection.BinarySearch(value, comparer);
        if (index < 0)
        {
            index = ~index;
        }

        collection.Insert(index, value);
    }
    public static int BinarySearch<T>(this ObservableCollection<T> collection, T value, IComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);

        int low = 0;
        int high = collection.Count - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            int compResult = comparer.Compare(value, collection[mid]);
            if (compResult == 0)
            {
                return mid;
            }

            if (compResult < 0)
            {
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }

        return ~low;
    }
}