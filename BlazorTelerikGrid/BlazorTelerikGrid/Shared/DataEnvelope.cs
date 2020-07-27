using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BlazorTelerikGrid.Shared
{
    public class DataEnvelope<T>
    {
        public List<T> CurrentPageData { get; set; }
        public int TotalItemCount { get; set; }
    }
}
