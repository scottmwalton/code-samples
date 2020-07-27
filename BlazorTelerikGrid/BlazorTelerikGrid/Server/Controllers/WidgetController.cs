using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorTelerikGrid.Server.DataModels;
using BlazorTelerikGrid.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telerik.DataSource;

namespace BlazorTelerikGrid.Server.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class WidgetController : ControllerBase
    {
        protected IWidgetService widgetSrv;
        
        public WidgetController(IWidgetService widgetService)
        {
            widgetSrv = widgetService;
        }

        [HttpGet]
        public List<Widget> GetWidgets()
        {
            return widgetSrv.GetWidgets();
        }

        [HttpPost]
        public async Task<ActionResult<DataEnvelope<Widget>>> GetPagedWidgets([FromBody] DataSourceRequest request)
        {
            DataSourceResult result = await widgetSrv.GetPagedWidgets(request);
            var data = new DataEnvelope<Widget>
            {
                CurrentPageData = result.Data.OfType<Widget>().ToList(),
                TotalItemCount = result.Total
            };
            return data;
        }
    }
}
