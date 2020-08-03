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
    public class ProductsController : ControllerBase
    {
        protected IProductService productSrv;
        
        public ProductsController(IProductService productService)
        {
            productSrv = productService;
        }

        [HttpGet]
        public List<Product> GetProducts()
        {
            return productSrv.GetProducts();
        }

        [HttpPost]
        public async Task<ActionResult<DataEnvelope<Product>>> GetPagedProducts([FromBody] DataSourceRequest request)
        {
            DataSourceResult result = await productSrv.GetPagedProducts(request);
            var data = new DataEnvelope<Product>
            {
                CurrentPageData = result.Data.OfType<Product>().ToList(),
                TotalItemCount = result.Total
            };
            return data;
        }
    }
}
