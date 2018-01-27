using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PetProduct.Configuration;
using Microsoft.Extensions.Options;
using ServiceSharedCore.Utilities;
using NpgsqlTypes;
using ServiceSharedCore.Models;

namespace ProductServiceCore.Controllers
{
    public class CategoryController : Controller
    {
        private const string SQL_GET_PRODUCTS = "SELECT CategoryId AS Id, Name, Descn AS Description FROM \"MSPETSHOP4\".category";
        private const string SQL_GET_PRODUCTS_BY_CATEGORY = "SELECT CategoryId AS Id, Name, Descn AS Description FROM \"MSPETSHOP4\".category WHERE CategoryId = :categoryId";
        private string PostgreSQLConnectionString { get; set; }

        public CategoryController(IOptions<ConnectionSettings> settings)
        {
            PostgreSQLConnectionString = settings.Value.PostgreSQLConnectionString;
        }
        // GET api/products
        [HttpGet]
        [Route("category")]
        public List<CategoryInfo> Get()
        {
            return DBFacilitator.GetList<CategoryInfo>(
                PostgreSQLConnectionString,
                SQL_GET_PRODUCTS);
        }
        [HttpGet]
        [Route("category/{categoryId}")]
        public CategoryInfo Get(string categoryId)
        {
            return DBFacilitator.GetList<CategoryInfo>(
                PostgreSQLConnectionString,
                SQL_GET_PRODUCTS_BY_CATEGORY,
                new List<Tuple<string, string, NpgsqlDbType>>() {
                    { new Tuple<string, string, NpgsqlDbType>("categoryId", categoryId, NpgsqlDbType.Text) } })
                .First();
            
        }
    }
}