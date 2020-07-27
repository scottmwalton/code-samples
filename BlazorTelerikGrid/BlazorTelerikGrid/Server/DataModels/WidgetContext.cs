using BlazorTelerikGrid.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorTelerikGrid.Server.DataModels
{
    public class WidgetContext
    {
        public WidgetContext()
        {
            SeedData();
        }

        public List<Widget> Widgets { get; set; }

        private void SeedData()
        {
            // Load widgets from the json file
            Widgets = JsonConvert.DeserializeObject<List<Widget>>(File.ReadAllText("Widgets.json"));
        }
    }
}
