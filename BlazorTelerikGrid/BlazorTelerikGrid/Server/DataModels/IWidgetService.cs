using BlazorTelerikGrid.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telerik.DataSource;

namespace BlazorTelerikGrid.Server.DataModels
{
    public interface IWidgetService
    {
        List<Widget> GetWidgets();
        Task<DataSourceResult> GetPagedWidgets(DataSourceRequest request);
    }
}
