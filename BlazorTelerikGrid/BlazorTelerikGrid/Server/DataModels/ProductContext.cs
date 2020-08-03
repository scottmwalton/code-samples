using BlazorTelerikGrid.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorTelerikGrid.Server.DataModels
{
    public class ProductContext
    {
        public ProductContext()
        {
            SeedData();
        }

        public List<Product> Products { get; set; }

        private void SeedData()
        {
            // Load products from the json file
            Products = JsonConvert.DeserializeObject<List<Product>>(File.ReadAllText("Products.json"));
        }
    }
}
