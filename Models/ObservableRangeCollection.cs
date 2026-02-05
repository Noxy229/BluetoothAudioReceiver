using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BluetoothAudioReceiver.Models;

/// <summary>
/// A collection that supports bulk addition of items, raising a single notification.
/// This improves performance when adding many items at once by avoiding UI thrashing.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    /// <summary>
    /// Adds a range of items to the collection and notifies observers once.
    /// </summary>
    /// <param name="collection">The items to add.</param>
    public void AddRange(IEnumerable<T> collection)
    {
        var newItems = new List<T>(collection);
        if (newItems.Count == 0) return;

        CheckReentrancy();

        foreach (var item in newItems)
        {
            Items.Add(item);
        }

        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
    }
}
