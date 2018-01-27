using System;
using System.Data;
using System.Data.OracleClient;
using System.Text;
using PetShop.Model;
using PetShop.IDAL;
using PetShop.DBUtility;

namespace PetShop.OracleDAL {

    public class Inventory : IInventory {

        //Static constants
        private const string SQL_SELECT_INVENTORY = "SELECT Qty FROM Inventory WHERE ItemId = :ItemId";
        private const string SQL_TAKE_INVENTORY = "UPDATE Inventory SET Qty = Qty - :Quantity{0} WHERE ItemId = :ItemId{0}";

        /// <summary>
        /// Function to get the current quantity in stock
        /// </summary>
        /// <param name="ItemId">Unique identifier for an item</param>
        /// <returns>Current Qty in Stock</returns>
        public int CurrentQtyInStock(string itemId) {

            int qty = 0;
            // Prepare parameters
            OracleParameter parm = new OracleParameter(":ItemId", OracleType.Char, 10);
            // Bind a value to the parameter
            parm.Value = itemId;

            // Execute a query to return the current quantity in stock
            qty = Convert.ToInt32(OracleHelper.ExecuteScalar(OracleHelper.ConnectionStringLocalTransaction, CommandType.Text, SQL_SELECT_INVENTORY, parm));

            return qty;
        }

        /// <summary>
        /// Function to update inventory based on purchased items
        /// Internally the function uses a batch query so the command is only sent to the database once
        /// </summary>
        /// <param name="items">Array of items purchased</param>
        public void TakeStock(LineItemInfo[] items) {

            // Total number of parameters = (2 * number of lines)
            int numberOfParameters = 2 * items.Length;

            // Create a parameters array based on the number of items in the array
            OracleParameter[] completeOrderParms = new OracleParameter[numberOfParameters];

            // Create a string builder to hold the entire query
            // Start the PL/SQL block
            StringBuilder finalSQLQuery = new StringBuilder("BEGIN ");

            int index = 0;

            int i = 1;

            // go through each item and bind parametes for the batch statement
            foreach (LineItemInfo item in items) {

                completeOrderParms[index] = new OracleParameter(":Quantity" + i, OracleType.Number);
                completeOrderParms[index++].Value = item.Quantity;
                completeOrderParms[index] = new OracleParameter(":ItemId" + i, OracleType.Char, 10);
                completeOrderParms[index++].Value = item.ItemId;

                // Append the current item query to the batch statement
                finalSQLQuery.Append(string.Format(SQL_TAKE_INVENTORY, i));
                finalSQLQuery.Append("; ");
                i++;
            }

            // Close the PL/SQL block
            finalSQLQuery.Append("END;");

            // Finally execute the query
            OracleHelper.ExecuteNonQuery(OracleHelper.ConnectionStringInventoryDistributedTransaction, CommandType.Text, finalSQLQuery.ToString(), completeOrderParms);
        }
    }
}

