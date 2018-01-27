using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Text;
using PetShop.Model;
using PetShop.IDAL;
using PetShop.DBUtility;

namespace PetShop.OracleDAL {

    public class Order : IOrder {

        // Static strings
        private const string SQL_GET_ORDERNUM = "SELECT ORDERNUM.NEXTVAL FROM DUAL";
		private const string SQL_INSERT_ORDER = "INSERT INTO Orders VALUES(:OrderId, :UserId, :OrderDate, :ShipAddress1, :ShipAddress2, :ShipCity, :ShipState, :ShipZip, :ShipCountry, :BillAddress1, :BillAddress2, :BillCity, :BillState, :BillZip, :BillCountry, 'UPS', :Total, :BillFirstName, :BillLastName, :ShipFirstName, :ShipLastName, :AuthorizationNumber, 'US_en')";
		private const string SQL_INSERT_STATUS = "INSERT INTO OrderStatus VALUES(:OrderId, 0, sysdate, 'P')";
        private const string SQL_INSERT_ITEM = "INSERT INTO LineItem VALUES(:OrderId{0}, :LineNumber{0}, :ItemId{0}, :Quantity{0}, :Price{0})";
        private const string SQL_SELECT_ORDER = "SELECT o.OrderDate, o.UserId, o.CardType, o.CreditCard, o.ExprDate, o.BillToFirstName, o.BillToLastName, o.BillAddr1, o.BillAddr2, o.BillCity, o.BillState, o.BillZip, o.BillCountry, o.ShipToFirstName, o.ShipToLastName, o.ShipAddr1, o.ShipAddr2, o.ShipCity, o.ShipState, o.ShipZip, o.ShipCountry, o.TotalPrice, l.ItemId, l.LineNum, l.Quantity, l.UnitPrice FROM Orders o, lineitem l WHERE o.OrderId = :OrderId AND o.orderid = l.orderid";
        private const string PARM_USER_ID = ":UserId";
        private const string PARM_DATE = ":OrderDate";
        private const string PARM_SHIP_ADDRESS1 = ":ShipAddress1";
        private const string PARM_SHIP_ADDRESS2 = ":ShipAddress2";
        private const string PARM_SHIP_CITY = ":ShipCity";
        private const string PARM_SHIP_STATE = ":ShipState";
        private const string PARM_SHIP_ZIP = ":ShipZip";
        private const string PARM_SHIP_COUNTRY = ":ShipCountry";
        private const string PARM_BILL_ADDRESS1 = ":BillAddress1";
        private const string PARM_BILL_ADDRESS2 = ":BillAddress2";
        private const string PARM_BILL_CITY = ":BillCity";
        private const string PARM_BILL_STATE = ":BillState";
        private const string PARM_BILL_ZIP = ":BillZip";
        private const string PARM_BILL_COUNTRY = ":BillCountry";
        private const string PARM_TOTAL = ":Total";
        private const string PARM_BILL_FIRST_NAME = ":BillFirstName";
        private const string PARM_BILL_LAST_NAME = ":BillLastName";
        private const string PARM_SHIP_FIRST_NAME = ":ShipFirstName";
        private const string PARM_SHIP_LAST_NAME = ":ShipLastName";	  
		private const string PARM_AUTHORIZATION_NUMBER = ":AuthorizationNumber";
        private const string PARM_ORDER_ID = ":OrderId";
        private const string PARM_LINE_NUMBER = ":LineNumber";
        private const string PARM_ITEM_ID = ":ItemId";
        private const string PARM_QUANTITY = ":Quantity";
        private const string PARM_PRICE = ":Price";

        /// <summary>
        /// With Oracle we can send a PL/SQL block to the database and 
        /// maintain ACID properties for the batch of statements
        /// The benefits with this is that ou increase performance by
        ///		reducing roundtrips to the database
        ///		gurantee that you only use one database connection and hence same construction costs
        ///	However there are limits to statement size which is based on the 
        ///	maximum size of VARCHAR2 parameters (approx 40,000 characters)
        /// </summary>
        /// <param name="order">Order details</param>
        /// <returns>OrderId</returns>
        public void Insert(OrderInfo order) {
            int orderId = 0;

            // Get the parameters
            OracleParameter[] completeOrderParms = null;
            OracleParameter[] orderParms = GetOrderParameters();
            OracleParameter statusParm = new OracleParameter(PARM_ORDER_ID, OracleType.Number);

            // Bind the parameters
            orderParms[1].Value = order.UserId;
            orderParms[2].Value = order.Date;
            orderParms[3].Value = order.ShippingAddress.Address1;
            orderParms[4].Value = order.ShippingAddress.Address2;
            orderParms[5].Value = order.ShippingAddress.City;
            orderParms[6].Value = order.ShippingAddress.State;
            orderParms[7].Value = order.ShippingAddress.Zip;
            orderParms[8].Value = order.ShippingAddress.Country;
            orderParms[9].Value = order.BillingAddress.Address1;
            orderParms[10].Value = order.BillingAddress.Address2;
            orderParms[11].Value = order.BillingAddress.City;
            orderParms[12].Value = order.BillingAddress.State;
            orderParms[13].Value = order.BillingAddress.Zip;
            orderParms[14].Value = order.BillingAddress.Country;
            orderParms[15].Value = order.OrderTotal;
            orderParms[16].Value = order.BillingAddress.FirstName;
            orderParms[17].Value = order.BillingAddress.LastName;
            orderParms[18].Value = order.ShippingAddress.FirstName;
            orderParms[19].Value = order.ShippingAddress.LastName; 
			orderParms[20].Value = order.AuthorizationNumber.Value;

            // Create the connection to the database
            using (OracleConnection conn = new OracleConnection(OracleHelper.ConnectionStringOrderDistributedTransaction)) {

                // Open the database connection
                conn.Open();

                // Get the order id for the order sequence
                orderId = Convert.ToInt32(OracleHelper.ExecuteScalar(conn, CommandType.Text, SQL_GET_ORDERNUM));

                orderParms[0].Value = orderId;
                statusParm.Value = orderId;

                // Total number of parameters = order parameters count + 1 + (5 * number of lines)
                int numberOfParameters = orderParms.Length + 1 + (5 * order.LineItems.Length);

                //Create a set of parameters
                completeOrderParms = new OracleParameter[numberOfParameters];

                //Copy the parameters to the execution parameters
                orderParms.CopyTo(completeOrderParms, 0);
				completeOrderParms[orderParms.Length] = statusParm;

                //Create a batch statement
                StringBuilder finalSQLQuery = new StringBuilder("BEGIN ");

                // Append the order header statements
                finalSQLQuery.Append(SQL_INSERT_ORDER);
                finalSQLQuery.Append("; ");
                finalSQLQuery.Append(SQL_INSERT_STATUS);
                finalSQLQuery.Append("; ");

				int index = orderParms.Length + 1;
                int i = 1;

                // Append each line item to the batch statement
                foreach (LineItemInfo item in order.LineItems) {

                    //Add the appropriate parameters
                    completeOrderParms[index] = new OracleParameter(PARM_ORDER_ID + i, OracleType.Number);
                    completeOrderParms[index++].Value = orderId;
                    completeOrderParms[index] = new OracleParameter(PARM_LINE_NUMBER + i, OracleType.Number);
                    completeOrderParms[index++].Value = item.Line;
                    completeOrderParms[index] = new OracleParameter(PARM_ITEM_ID + i, OracleType.Char, 10);
                    completeOrderParms[index++].Value = item.ItemId;
                    completeOrderParms[index] = new OracleParameter(PARM_QUANTITY + i, OracleType.Number);
                    completeOrderParms[index++].Value = item.Quantity;
                    completeOrderParms[index] = new OracleParameter(PARM_PRICE + i, OracleType.Number);
                    completeOrderParms[index++].Value = item.Price;

                    // Append the statement to the batch
                    finalSQLQuery.Append(string.Format(SQL_INSERT_ITEM, i));
                    finalSQLQuery.Append("; ");
                    i++;

                }

                //Close the PL/SQL block
                finalSQLQuery.Append("END;");

                // Finally execute the query
                OracleHelper.ExecuteNonQuery(conn, CommandType.Text, finalSQLQuery.ToString(), completeOrderParms);
            }


        }

        /// <summary>
        /// Read an order from the database
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <returns>Details of the Order</returns>
        public OrderInfo GetOrder(int orderId) {

            //Create a parameter
            OracleParameter parm = new OracleParameter(PARM_ORDER_ID, OracleType.Number);
            parm.Value = orderId;

            //Execute a query to read the order
            using (OracleDataReader rdr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringOrderDistributedTransaction, CommandType.Text, SQL_SELECT_ORDER, parm)) {
                if (rdr.Read()) {
                    //Generate an order header from the first row
                    AddressInfo billingAddress = new AddressInfo(rdr.GetString(5), rdr.GetString(6), rdr.GetString(7), rdr.GetString(8), rdr.GetString(9), rdr.GetString(10), rdr.GetString(11), rdr.GetString(12), null, "email");
                    AddressInfo shippingAddress = new AddressInfo(rdr.GetString(13), rdr.GetString(14), rdr.GetString(15), rdr.GetString(16), rdr.GetString(17), rdr.GetString(18), rdr.GetString(19), rdr.GetString(20), null, "email");

					OrderInfo order = new OrderInfo(orderId, rdr.GetDateTime(0), rdr.GetString(1), null, billingAddress, shippingAddress, rdr.GetDecimal(21), null, null);

                    IList<LineItemInfo> lineItems = new List<LineItemInfo>();
                    LineItemInfo item = null;

                    //Create the lineitems from the first row and subsequent rows
                    do {
                        item = new LineItemInfo(rdr.GetString(22), string.Empty, rdr.GetInt32(23), rdr.GetInt32(24), rdr.GetDecimal(25));
                        lineItems.Add(item);
                    } while (rdr.Read());

                    order.LineItems = new LineItemInfo[lineItems.Count];
                    lineItems.CopyTo(order.LineItems, 0);

                    return order;
                }
            }

            return null;
        }

        /// <summary>
        /// Internal function to get cached parameters
        /// </summary>
        /// <returns></returns>
        private static OracleParameter[] GetOrderParameters() {
            OracleParameter[] parms = OracleHelper.GetCachedParameters(SQL_INSERT_ORDER);

            if (parms == null) {
                parms = new OracleParameter[] {												 
					new OracleParameter(PARM_ORDER_ID, OracleType.Number, 10),
					new OracleParameter(PARM_USER_ID, OracleType.VarChar, 80),
					new OracleParameter(PARM_DATE, OracleType.DateTime),
					new OracleParameter(PARM_SHIP_ADDRESS1, OracleType.VarChar, 80),
					new OracleParameter(PARM_SHIP_ADDRESS2, OracleType.VarChar, 80),
					new OracleParameter(PARM_SHIP_CITY, OracleType.VarChar, 80),
					new OracleParameter(PARM_SHIP_STATE, OracleType.VarChar, 80),
					new OracleParameter(PARM_SHIP_ZIP, OracleType.VarChar, 50),
					new OracleParameter(PARM_SHIP_COUNTRY, OracleType.VarChar, 50),
					new OracleParameter(PARM_BILL_ADDRESS1, OracleType.VarChar, 80),
					new OracleParameter(PARM_BILL_ADDRESS2, OracleType.VarChar, 80),
					new OracleParameter(PARM_BILL_CITY, OracleType.VarChar, 80),
					new OracleParameter(PARM_BILL_STATE, OracleType.VarChar, 80),
					new OracleParameter(PARM_BILL_ZIP, OracleType.VarChar, 50),
					new OracleParameter(PARM_BILL_COUNTRY, OracleType.VarChar, 50),
					new OracleParameter(PARM_TOTAL, OracleType.Number),
					new OracleParameter(PARM_BILL_FIRST_NAME, OracleType.VarChar, 80),
					new OracleParameter(PARM_BILL_LAST_NAME, OracleType.VarChar, 80),
					new OracleParameter(PARM_SHIP_FIRST_NAME, OracleType.VarChar, 80),
					new OracleParameter(PARM_SHIP_LAST_NAME, OracleType.VarChar, 80),
				    new OracleParameter(PARM_AUTHORIZATION_NUMBER, OracleType.Int32)};

                OracleHelper.CacheParameters(SQL_INSERT_ORDER, parms);
            }

            return parms;
        }
    }
}
