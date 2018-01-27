using System;
using System.Data;
using System.Data.OracleClient;
using PetShop.Model;
using PetShop.IDAL;
using System.Collections.Generic;
using PetShop.DBUtility;

namespace PetShop.OracleDAL {

    public class Item : IItem {

        // Static constants
        private const string SQL_SELECT_ITEMS_BY_PRODUCT = "SELECT Item.ItemId, Item.Name, Inventory.Qty, Item.ListPrice, Product.Name, Item.Image, Product.CategoryId, Product.ProductId FROM Item, Product, Inventory WHERE Item.ProductId = Product.ProductId AND Item.ItemId = Inventory.ItemId AND Item.ProductId = :ProductId";

        private const string SQL_SELECT_ITEM = "SELECT Item.ItemId, Item.Name, Item.ListPrice, Product.Name, Item.Image, Product.CategoryId, Product.ProductId FROM Item, Product WHERE Item.ProductId = Product.ProductId AND Item.ItemId = :ItemId";

        private const string PARM_ITEM_ID = ":ItemId";
		private const string PARM_PRODUCT_ID = ":ProductId";

        /// <summary>
        /// Function to get a list of items within a product group
        /// </summary>
        /// <param name="productId">Product Id</param>
        /// <returns>A Generic List of ItemInfo</returns>
		public IList<ItemInfo> GetItemsByProduct(string productId) {

            // Declare array to return
			IList<ItemInfo> itemsByProduct = new List<ItemInfo>();

            // Create a database parameter
            OracleParameter parm = new OracleParameter(PARM_PRODUCT_ID, OracleType.Char, 10);
            //Set the parameter value
            parm.Value = productId;

            //Execute the query
			using(OracleDataReader rdr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringLocalTransaction, CommandType.Text, SQL_SELECT_ITEMS_BY_PRODUCT, parm)) {

                // Scroll through the results
                while (rdr.Read()) {
                    ItemInfo item = new ItemInfo(rdr.GetString(0), rdr.GetString(1), rdr.GetInt32(2), rdr.GetDecimal(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7));
                    //Add each item to the arraylist
                    itemsByProduct.Add(item);
                }
            }

            return itemsByProduct;
        }

        /// <summary>
        /// Get an individual item based on a the unique key
        /// </summary>
        /// <param name="itemId">unique key</param>
        /// <returns>Details of the Item</returns>
        public ItemInfo GetItem(string itemId) {

            //Set up a return value
            ItemInfo item = null;

            //Create a parameter
            OracleParameter parm = new OracleParameter(PARM_ITEM_ID, OracleType.Char, 10);
            //Bind the parameter
            parm.Value = itemId;

            //Execute a query
            using (OracleDataReader rdr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringLocalTransaction, CommandType.Text, SQL_SELECT_ITEM, parm)) {

                //Query will only return one record
                if (rdr.Read())
                    item = new ItemInfo(rdr.GetString(0), rdr.GetString(1), 0, rdr.GetDecimal(2), rdr.GetString(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6));
                else
                    item = new ItemInfo();
            }
            return item;
        }
    }
}
