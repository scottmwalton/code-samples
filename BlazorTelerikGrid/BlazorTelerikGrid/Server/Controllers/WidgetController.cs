using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorTelerikGrid.Server.DataModels;
using BlazorTelerikGrid.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorTelerikGrid.Server.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class WidgetController : ControllerBase
    {
        protected IWidgetService _widgetService;
        
        public WidgetController(IWidgetService widgetService)
        {
            _widgetService = widgetService;
        }

        [HttpGet]
        public List<Widget> GetWidgets()
        {
            return _widgetService.GetWidgets();
        }
    }
}
