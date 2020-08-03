using BlazorTelerikGrid.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telerik.DataSource;
using Telerik.DataSource.Extensions;

namespace BlazorTelerikGrid.Server.DataModels
{
    public class ProductService : IProductService
    {
        public List<Product> GetProducts()
        {
            var context = new ProductContext();
            return context.Products;
        }

        public async Task<DataSourceResult> GetPagedProducts(DataSourceRequest request)
        {
            var context = new ProductContext();
            DataSourceResult result = await context.Products.ToDataSourceResultAsync(request);
            return result;
        }
    }
}
