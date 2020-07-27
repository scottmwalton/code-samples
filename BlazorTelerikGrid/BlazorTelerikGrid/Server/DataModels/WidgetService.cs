using BlazorTelerikGrid.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorTelerikGrid.Server.DataModels
{
    public class WidgetService : IWidgetService
    {
        public List<Widget> GetWidgets()
        {
            var context = new WidgetContext();
            return context.Widgets;
        }
    }
}
