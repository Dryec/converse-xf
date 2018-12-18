using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Converse.Helpers
{
    static class Extensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection, bool Asc = true) where T : IComparable
        {
            List<T> sorted;
            if (Asc)
            {
                sorted = collection.OrderBy(x => x).ToList();

            }
            else
            {
                sorted = collection.OrderByDescending(x => x).ToList();
            }
            for (var i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
        }

        public static async Task SortAsync<T>(this ObservableCollection<T> collection, bool Asc = true, int delay = 10) where T : IComparable
        {
            List<T> sorted;
            if (Asc)
            {
                sorted = collection.OrderBy(x => x).ToList();

            }
            else
            {
                sorted = collection.OrderByDescending(x => x).ToList();
            }
            for (var i = 0; i < sorted.Count(); i++)
            {
                Debug.WriteLine($"{i}", "Sort");
                collection.Move(collection.IndexOf(sorted[i]), i);
                await Task.Delay(delay);
            }
        }
    }
}
