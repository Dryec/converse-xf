using System;
using System.Collections.Generic;
using System.Diagnostics;
using Converse.Models;
using Syncfusion.DataSource;

namespace Converse.Comparators
{
    public class ChatEntryComparer : IComparer<object>, ISortDirection
    {
        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            if (x is ChatEntry entryX && y is ChatEntry entryY)
            {
                try
                {
                    var compareResult = entryX.LastMessage.Timestamp.CompareTo(entryY.LastMessage.Timestamp);
                    return compareResult * (SortDirection == ListSortDirection.Ascending ? 1 : -1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return 0;
        }
    }
}
