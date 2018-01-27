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
    public class CategoryController : ApiController
    {
        private ICategory _categoryDAL = new Category();
        [HttpGet]
        [Route("category")]
        public List<CategoryInfo> Get()
        {
            return _categoryDAL.GetCategories().ToList();
        }
        [HttpGet]
        [Route("category/{categoryId}")]
        public CategoryInfo Get(string categoryId)
        {
            return _categoryDAL.GetCategory(categoryId);
        }
    }
}
