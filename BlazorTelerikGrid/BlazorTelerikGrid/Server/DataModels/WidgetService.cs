using BlazorTelerikGrid.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telerik.DataSource;
using Telerik.DataSource.Extensions;

namespace BlazorTelerikGrid.Server.DataModels
{
    public class WidgetService : IWidgetService
    {
        public List<Widget> GetWidgets()
        {
            var context = new WidgetContext();
            return context.Widgets;
        }

        public async Task<DataSourceResult> GetPagedWidgets(DataSourceRequest request)
        {
            var context = new WidgetContext();
            DataSourceResult result = await context.Widgets.ToDataSourceResultAsync(request);
            return result;
        }
    }
}
