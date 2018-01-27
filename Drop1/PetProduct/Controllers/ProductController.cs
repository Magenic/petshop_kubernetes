using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NpgsqlTypes;
using PetProduct.Configuration;
using ServiceSharedCore.Utilities;
using ServiceSharedCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductServiceCore.Controllers
{
    public class ProductController : Controller
    {
        public ProductController(IOptions<ConnectionSettings> settings)
        {
            ConfigSettings = settings.Value;
        }

        public ConnectionSettings ConfigSettings { get; private set; }

        private const string SQL_SELECT_PRODUCTS_BY_CATEGORY = "SELECT Product.ProductId AS Id, Product.Name, Product.Descn AS Description, Product.Image, Product.CategoryId FROM \"MSPETSHOP4\".Product WHERE Product.CategoryId = :categoryId";
        [HttpGet]
        [Route("product/productbycategory/{category}")]
        public IList<ProductInfo> GetProductsByCategory(string category)
        {
            return DBFacilitator.GetList<ProductInfo>(
            ConfigSettings.PostgreSQLConnectionString,
                SQL_SELECT_PRODUCTS_BY_CATEGORY,
                new List<Tuple<string, string, NpgsqlDbType>>() {
                    { new Tuple<string, string, NpgsqlDbType>(":categoryId", category, NpgsqlDbType.Text) } });
        }
        private const string SQL_SELECT_PRODUCT = "SELECT Product.ProductId AS Id, Product.Name, Product.Descn AS Description, Product.Image, Product.CategoryId FROM \"MSPETSHOP4\".Product WHERE Product.ProductId  = :productId";
        [HttpGet]
        [Route("product/{productId}")]
        public ProductInfo GetProduct(string productId)
        {
            return DBFacilitator.GetOne<ProductInfo>(
            ConfigSettings.PostgreSQLConnectionString,
                SQL_SELECT_PRODUCT,
                new List<Tuple<string, string, NpgsqlDbType>>() {
                    { new Tuple<string, string, NpgsqlDbType>(":productId", productId, NpgsqlDbType.Text) } });
        }

        private const string SQL_SELECT_PRODUCTS_BY_SEARCH1 = "SELECT ProductId, Name, Descn, Product.Image, Product.CategoryId FROM Product WHERE ((";
        private const string SQL_SELECT_PRODUCTS_BY_SEARCH2 = "LOWER(Name) LIKE '%' || {0} || '%' OR LOWER(CategoryId) LIKE '%' || {0} || '%'";
        private const string SQL_SELECT_PRODUCTS_BY_SEARCH3 = ") OR (";
        private const string SQL_SELECT_PRODUCTS_BY_SEARCH4 = "))";
        [HttpGet]
        [Route("product/productbykeyword/{terms}")]
        public IList<ProductInfo> GetProductsBySearch(string terms)
        {
            var keywords = terms.Split(',');
            var sb = new StringBuilder(SQL_SELECT_PRODUCTS_BY_SEARCH1);
            for (int i = 0; i < keywords.Length; i++)
            {
                sb.Append(String.Format(SQL_SELECT_PRODUCTS_BY_SEARCH2, keywords[i]));
                if (i < keywords.Length - 1) sb.Append(SQL_SELECT_PRODUCTS_BY_SEARCH3);
            }
            sb.Append(SQL_SELECT_PRODUCTS_BY_SEARCH4);

            return DBFacilitator.GetList<ProductInfo>(
            ConfigSettings.PostgreSQLConnectionString,
                sb.ToString(),
                new List<Tuple<string, string, NpgsqlDbType>>());
        }
    }
}
