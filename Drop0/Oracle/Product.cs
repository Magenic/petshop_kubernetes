using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using PetShop.Model;
using PetShop.IDAL;
using PetShop.DBUtility;

namespace PetShop.OracleDAL {

    public class Product : IProduct {

        // Static constants
        private const string SQL_SELECT_PRODUCTS_BY_CATEGORY = "SELECT Product.ProductId, Product.Name, Product.Descn, Product.Image, Product.CategoryId FROM Product WHERE Product.CategoryId = :Category";
        private const string SQL_SELECT_PRODUCTS_BY_SEARCH1 = "SELECT ProductId, Name, Descn, Product.Image, Product.CategoryId FROM Product WHERE ((";
        private const string SQL_SELECT_PRODUCTS_BY_SEARCH2 = "LOWER(Name) LIKE '%' || {0} || '%' OR LOWER(CategoryId) LIKE '%' || {0} || '%'";
        private const string SQL_SELECT_PRODUCTS_BY_SEARCH3 = ") OR (";
        private const string SQL_SELECT_PRODUCTS_BY_SEARCH4 = "))";
        private const string SQL_SELECT_PRODUCT = "SELECT Product.ProductId, Product.Name, Product.Descn, Product.Image, Product.CategoryId FROM Product WHERE Product.ProductId  = :ProductId";
        private const string PARM_CATEGORY = ":Category";
        private const string PARM_KEYWORD = ":Keyword";
        private const string PARM_PRODUCTID = ":ProductId";

        /// <summary>
        /// Query for products by category
        /// </summary>
        /// <param name="category">category name</param>
        /// <returns>A Generic List of ProductInfo</returns>
        public IList<ProductInfo> GetProductsByCategory(string category) {

            IList<ProductInfo> productsByCategory = new List<ProductInfo>();

            OracleParameter parm = new OracleParameter(PARM_CATEGORY, OracleType.Char, 10);
            parm.Value = category;

            //Execute a query to read the products
            using (OracleDataReader rdr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringLocalTransaction, CommandType.Text, SQL_SELECT_PRODUCTS_BY_CATEGORY, parm)) {
                while (rdr.Read()) {
                    ProductInfo product = new ProductInfo(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3), rdr.GetString(4));
                    productsByCategory.Add(product);
                }
            }

            return productsByCategory;
        }


        /// <summary>
        /// Query for products by keywords. 
        /// The results will include any product where the keyword appears in the category name or product name
        /// </summary>
        /// <param name="keywords">string array of keywords</param>
        /// <returns>A Generic List of ProductInfo</returns>
        public IList<ProductInfo> GetProductsBySearch(string[] keywords) {

            IList<ProductInfo> productsBySearch = new List<ProductInfo>();

            //Create a new query string
            int numKeywords = keywords.Length;
            StringBuilder sql = new StringBuilder(SQL_SELECT_PRODUCTS_BY_SEARCH1);

            //Add each keyword to the query
            for (int i = 0; i < numKeywords; i++) {
                sql.Append(string.Format(SQL_SELECT_PRODUCTS_BY_SEARCH2, PARM_KEYWORD + i));
                sql.Append(i + 1 < numKeywords ? SQL_SELECT_PRODUCTS_BY_SEARCH3 : SQL_SELECT_PRODUCTS_BY_SEARCH4);
            }

            //See if we have a set of cached parameters based on a similar qquery
            string sqlProductsBySearch = sql.ToString();
            OracleParameter[] parms = OracleHelper.GetCachedParameters(sqlProductsBySearch);

            // If the parameters are null build a new set
            if (parms == null) {
                parms = new OracleParameter[numKeywords];

                for (int i = 0; i < numKeywords; i++)
                    parms[i] = new OracleParameter(PARM_KEYWORD + i, OracleType.VarChar, 80);

                // Cache the new parameters
                OracleHelper.CacheParameters(sqlProductsBySearch, parms);
            }

            // Bind the new parameters
            for (int i = 0; i < numKeywords; i++)
                parms[i].Value = keywords[i];

            //Finally execute the query
            using (OracleDataReader rdr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringLocalTransaction, CommandType.Text, sqlProductsBySearch, parms)) {
                while (rdr.Read()) {
                    ProductInfo product = new ProductInfo(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3), rdr.GetString(4));
                    productsBySearch.Add(product);
                }
            }

            return productsBySearch;
        }

        /// <summary>
        /// Query for a product
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>ProductInfo object for requested product</returns>
        public ProductInfo GetProduct(string productId) {
            ProductInfo product = null;
            OracleParameter parm = new OracleParameter(PARM_PRODUCTID, OracleType.Char, 10);
            parm.Value = productId;

            using (OracleDataReader rdr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringLocalTransaction, CommandType.Text, SQL_SELECT_PRODUCT, parm))
                if (rdr.Read())
                    product = new ProductInfo(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3), rdr.GetString(4));
                else
                    product = new ProductInfo();

            return product;
        }
    }
}
