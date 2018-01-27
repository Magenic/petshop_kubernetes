using PetShop.IDAL;
using PetShop.Model;
using PetShop.OracleDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ProductService.Controllers
{
    public class ProductController : ApiController
    {
        private IProduct _dal = new Product();
        [HttpGet]
        [Route("product/productbycategory/{category}")]
        public IList<ProductInfo> GetProductsByCategory(string category)
        {
            return _dal.GetProductsByCategory(category);
        }
        [HttpGet]
        [Route("product/{productId}")]
        public ProductInfo GetProduct(string productId)
        {
            return _dal.GetProduct(productId);
        }
        [HttpPost]
        [Route("product/productbykeyword")]
        public IList<ProductInfo> GetProductsBySearch(string[] keywords)
        {
            return _dal.GetProductsBySearch(keywords);
        }
    }
}
