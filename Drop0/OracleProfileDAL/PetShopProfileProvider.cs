using System;
using System.Data;
using System.Data.OracleClient;
using PetShop.Model;
using PetShop.IProfileDAL;
using System.Collections.Generic;
using PetShop.DBUtility;
using System.Text;

namespace PetShop.OracleProfileDAL {
	class PetShopProfileProvider : IPetShopProfileProvider {
		// Contst matching System.Web.Profile.ProfileAuthenticationOption.Anonymous
		private const int AUTH_ANONYMOUS = 0;

		// Contst matching System.Web.Profile.ProfileAuthenticationOption.Authenticated
		private const int AUTH_AUTHENTICATED = 1;

		// Contst matching System.Web.Profile.ProfileAuthenticationOption.All
		private const int AUTH_ALL = 2;

        /// <summary>
        /// Retrieve account information for current username and application.
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="appName">Application Name</param>
        /// <returns>Account information for current user</returns>
		public AddressInfo GetAccountInfo(string userName, string appName) {
			string sqlSelect = "SELECT Account.Email, Account.FirstName, Account.LastName, Account.Address1, Account.Address2, Account.City, Account.State, Account.Zip, Account.Country, Account.Phone FROM Account, Profiles WHERE Account.UniqueID = Profiles.UniqueID AND Profiles.Username = :Username AND Profiles.ApplicationName = :ApplicationName";
			OracleParameter[] parms = {					   
				new OracleParameter(":Username", OracleType.VarChar, 256),
				new OracleParameter(":ApplicationName", OracleType.VarChar, 256)};
			parms[0].Value = userName;
			parms[1].Value = appName;

			AddressInfo addressInfo = null;

			OracleDataReader dr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlSelect, parms);
            while (dr.Read()) {
                string address2 = string.Empty;
                if (!dr.IsDBNull(4))
                    address2 = dr.GetString(4);
                addressInfo = new AddressInfo(dr.GetString(1), dr.GetString(2), dr.GetString(3), address2, dr.GetString(5), dr.GetString(6), dr.GetString(7), dr.GetString(8), dr.GetString(9), dr.GetString(0));
            }
			dr.Close();

			return addressInfo;
		}

        /// <summary>
        /// Update account for current user
        /// </summary>
        /// <param name="uniqueID">User id</param>
        /// <param name="addressInfo">Account information for current user</param>   
		public void SetAccountInfo(int uniqueID, AddressInfo addressInfo) {
			string sqlDelete = "DELETE FROM Account WHERE UniqueID = :UniqueID";
			OracleParameter param = new OracleParameter(":UniqueID", OracleType.Int32);
			param.Value = uniqueID;

			string sqlInsert = "INSERT INTO Account (UniqueID, Email, FirstName, LastName, Address1, Address2, City, State, Zip, Country, Phone) VALUES (:UniqueID, :Email, :FirstName, :LastName, :Address1, :Address2, :City, :State, :Zip, :Country, :Phone)";

			OracleParameter[] parms = {					   
			new OracleParameter(":UniqueID", OracleType.Number, 10),
			new OracleParameter(":Email", OracleType.VarChar, 80),
			new OracleParameter(":FirstName", OracleType.VarChar, 80),
			new OracleParameter(":LastName", OracleType.VarChar, 80),
			new OracleParameter(":Address1", OracleType.VarChar, 80),
			new OracleParameter(":Address2", OracleType.VarChar, 80),
			new OracleParameter(":City", OracleType.VarChar, 80),
			new OracleParameter(":State", OracleType.VarChar, 80),
			new OracleParameter(":Zip", OracleType.VarChar, 80),
			new OracleParameter(":Country", OracleType.VarChar, 80),
			new OracleParameter(":Phone", OracleType.VarChar, 80)};

			parms[0].Value = uniqueID;
			parms[1].Value = addressInfo.Email;
			parms[2].Value = addressInfo.FirstName;
			parms[3].Value = addressInfo.LastName;
			parms[4].Value = addressInfo.Address1;
			parms[5].Value = addressInfo.Address2;
			parms[6].Value = addressInfo.City;
			parms[7].Value = addressInfo.State;
			parms[8].Value = addressInfo.Zip;
			parms[9].Value = addressInfo.Country;
			parms[10].Value = addressInfo.Phone;

			OracleConnection conn = new OracleConnection(OracleHelper.ConnectionStringProfile);
			conn.Open();
			OracleTransaction trans = conn.BeginTransaction(IsolationLevel.ReadCommitted);

			try {
				OracleHelper.ExecuteNonQuery(trans, CommandType.Text, sqlDelete, param);
				OracleHelper.ExecuteNonQuery(trans, CommandType.Text, sqlInsert, parms);
				trans.Commit();
			}
			catch(Exception e) {
				trans.Rollback();
				throw new ApplicationException(e.Message);
			}
			finally {
				conn.Close();
			}
		}

        /// <summary>
        /// Retrieve collection of shopping cart items
        /// </summary>
        /// <param name="userName">User Name</param>
        /// <param name="appName">Application Name</param>
        /// <param name="isShoppingCart">Shopping cart flag</param>
        /// <returns>Collection of shopping cart items</returns>
		public IList<CartItemInfo> GetCartItems(string userName, string appName, bool isShoppingCart) {
			string sqlSelect = "SELECT Cart.ItemId, Cart.Name, Cart.Type, Cart.Price, Cart.CategoryId, Cart.ProductId, Cart.Quantity FROM Profiles, Cart WHERE Profiles.UniqueID = Cart.UniqueID AND Profiles.Username = :Username AND Profiles.ApplicationName = :ApplicationName AND IsShoppingCart = :IsShoppingCart";

			OracleParameter[] parms = {						   
				new OracleParameter(":Username", OracleType.VarChar, 256),
				new OracleParameter(":ApplicationName", OracleType.VarChar, 256),
				new OracleParameter(":IsShoppingCart", OracleType.VarChar, 1)};
			parms[0].Value = userName;
			parms[1].Value = appName;
			parms[2].Value = OracleHelper.OraBit(isShoppingCart);

			OracleDataReader dr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlSelect, parms);

			IList<CartItemInfo> cartItems = new List<CartItemInfo>();

			while(dr.Read()) {
				CartItemInfo cartItem = new CartItemInfo(dr.GetString(0), dr.GetString(1), dr.GetInt32(6), dr.GetDecimal(3), dr.GetString(2), dr.GetString(4), dr.GetString(5));
				cartItems.Add(cartItem);
			}
			dr.Close();
			return cartItems;
		}

        /// <summary>
        /// Update shopping cart for current user
        /// </summary>
        /// <param name="uniqueID">User id</param>
        /// <param name="cartItems">Collection of shopping cart items</param>
        /// <param name="isShoppingCart">Shopping cart flag</param>
		public void SetCartItems(int uniqueID, ICollection<CartItemInfo> cartItems, bool isShoppingCart) {
			string sqlDelete = "DELETE FROM Cart WHERE UniqueID = :UniqueID AND IsShoppingCart = :IsShoppingCart";

			OracleParameter[] parms1 = {				   
				new OracleParameter(":UniqueID", OracleType.Number, 10),
				new OracleParameter(":IsShoppingCart", OracleType.VarChar, 1)};
			parms1[0].Value = uniqueID;
			parms1[1].Value = OracleHelper.OraBit(isShoppingCart);

            if (cartItems.Count > 0) {

                // update cart using SqlTransaction
                string sqlInsert = "INSERT INTO Cart (UniqueID, ItemId, Name, Type, Price, CategoryId, ProductId, IsShoppingCart, Quantity) VALUES (:UniqueID, :ItemId, :Name, :Type, :Price, :CategoryId, :ProductId, :IsShoppingCart, :Quantity)";

                OracleParameter[] parms2 = {				   
				new OracleParameter(":UniqueID", OracleType.Number, 10),	
				new OracleParameter(":IsShoppingCart", OracleType.VarChar, 1),
				new OracleParameter(":ItemId", OracleType.VarChar, 10),
				new OracleParameter(":Name", OracleType.VarChar, 80),
				new OracleParameter(":Type", OracleType.VarChar, 80),
				new OracleParameter(":Price", OracleType.Double, 8),
				new OracleParameter(":CategoryId", OracleType.VarChar, 10),
				new OracleParameter(":ProductId", OracleType.VarChar, 10),
				new OracleParameter(":Quantity", OracleType.Number, 10)};
                parms2[0].Value = uniqueID;
                parms2[1].Value = OracleHelper.OraBit(isShoppingCart);


                OracleConnection conn = new OracleConnection(OracleHelper.ConnectionStringProfile);
                conn.Open();
                OracleTransaction trans = conn.BeginTransaction(IsolationLevel.ReadCommitted);

                try {
                    OracleHelper.ExecuteNonQuery(trans, CommandType.Text, sqlDelete, parms1);

                    foreach (CartItemInfo cartItem in cartItems) {
                        parms2[2].Value = cartItem.ItemId;
                        parms2[3].Value = cartItem.Name;
                        parms2[4].Value = cartItem.Type;
                        parms2[5].Value = cartItem.Price;
                        parms2[6].Value = cartItem.CategoryId;
                        parms2[7].Value = cartItem.ProductId;
                        parms2[8].Value = cartItem.Quantity;
                        OracleHelper.ExecuteNonQuery(trans, CommandType.Text, sqlInsert, parms2);
                    }
                    trans.Commit();
                }
                catch (Exception e) {
                    trans.Rollback();
                    throw new ApplicationException(e.Message);
                }
                finally {
                    conn.Close();
                }
            }
            else {
                // delete cart
                OracleHelper.ExecuteNonQuery(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlDelete, parms1);
            }

		}

        /// <summary>
        /// Update activity dates for current user and application
        /// </summary>
        /// <param name="userName">USer name</param>
        /// <param name="activityOnly">Activity only flag</param>
        /// <param name="appName">Application Name</param>
		public void UpdateActivityDates(string userName, bool activityOnly, string appName) {
			DateTime activityDate = DateTime.Now;

			string sqlUpdate;
			OracleParameter[] parms;

			if(activityOnly) {
				sqlUpdate = "UPDATE Profiles Set LastActivityDate = :LastActivityDate WHERE Username = :Username AND ApplicationName = :ApplicationName";
				parms = new OracleParameter[]{						   
					new OracleParameter(":LastActivityDate", OracleType.DateTime),
					new OracleParameter(":Username", OracleType.VarChar, 256),
					new OracleParameter(":ApplicationName", OracleType.VarChar, 256)};

				parms[0].Value = activityDate;
				parms[1].Value = userName;
				parms[2].Value = appName;

			}
			else {
				sqlUpdate = "UPDATE Profiles SET LastActivityDate = :LastActivityDate, LastUpdatedDate = :LastUpdatedDate WHERE Username = :Username AND ApplicationName = :ApplicationName";
				parms = new OracleParameter[]{
					new OracleParameter(":LastActivityDate", OracleType.DateTime),
					new OracleParameter(":LastUpdatedDate", OracleType.DateTime),
					new OracleParameter(":Username", OracleType.VarChar, 256),
					new OracleParameter(":ApplicationName", OracleType.VarChar, 256)};

				parms[0].Value = activityDate;
				parms[1].Value = activityDate;
				parms[2].Value = userName;
				parms[3].Value = appName;
			}

			OracleHelper.ExecuteNonQuery(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlUpdate, parms);

		}

        /// <summary>
        /// Retrive unique id for current user
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="isAuthenticated">Authentication flag</param>
        /// <param name="ignoreAuthenticationType">Ignore authentication flag</param>
        /// <param name="appName">Application Name</param>
        /// <returns>Unique id for current user</returns>
		public int GetUniqueID(string userName, bool isAuthenticated, bool ignoreAuthenticationType, string appName) {
			string sqlSelect = "SELECT UniqueID FROM Profiles WHERE Username = :Username AND ApplicationName = :ApplicationName";

			OracleParameter[] parms = {
				new OracleParameter(":Username", OracleType.VarChar, 256),
				new OracleParameter(":ApplicationName", OracleType.VarChar, 256)};
			parms[0].Value = userName;
			parms[1].Value = appName;

			if(!ignoreAuthenticationType) {
				sqlSelect += " AND IsAnonymous = :IsAnonymous";
				Array.Resize<OracleParameter>(ref parms, parms.Length + 1);
				parms[2] = new OracleParameter(":IsAnonymous", OracleType.VarChar, 1);
				parms[2].Value = OracleHelper.OraBit(!isAuthenticated);
			}

			int uniqueID = 0;

			object retVal = null;
			retVal = OracleHelper.ExecuteScalar(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlSelect, parms);

			if(retVal == null)
				uniqueID = CreateProfileForUser(userName, isAuthenticated, appName);
			else
				uniqueID = Convert.ToInt32(retVal);

			return uniqueID;
		}

        /// <summary>
        /// Create profile record for current user
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="isAuthenticated">Authentication flag</param>
        /// <param name="appName">Application Name</param>
        /// <returns>Number of records created</returns>
		public int CreateProfileForUser(string userName, bool isAuthenticated, string appName) {

			string sqlInsert = "INSERT INTO Profiles (UniqueId, Username, ApplicationName, LastActivityDate, LastUpdatedDate, IsAnonymous) Values(:UniqueId, :Username, :ApplicationName, :LastActivityDate, :LastUpdatedDate, :IsAnonymous)";
			string sqlUserId = "SELECT UNIQUEID.NEXTVAL FROM DUAL";													   
			int uniqueID = 0;  			
				
			// Create the connection to the database		
			using(OracleConnection conn = new OracleConnection(OracleHelper.ConnectionStringProfile)) {	 

                // Open the database connection
                conn.Open();
					
				// get identity
				int.TryParse(OracleHelper.ExecuteScalar(conn, CommandType.Text, sqlUserId).ToString(), out uniqueID);
	   
				OracleParameter[] parms = {
					new OracleParameter(":UniqueId", OracleType.Number, 10),
					new OracleParameter(":Username", OracleType.VarChar, 256),
					new OracleParameter(":ApplicationName", OracleType.VarChar, 256),
					new OracleParameter(":LastActivityDate", OracleType.DateTime),
					new OracleParameter(":LastUpdatedDate", OracleType.DateTime),
					new OracleParameter(":IsAnonymous", OracleType.VarChar, 1)};

				parms[0].Value = uniqueID;
				parms[1].Value = userName;
				parms[2].Value = appName;
				parms[3].Value = DateTime.Now;
				parms[4].Value = DateTime.Now;
				parms[5].Value = OracleHelper.OraBit(!isAuthenticated);
						   
				// execute the query
				OracleHelper.ExecuteNonQuery(conn, CommandType.Text, sqlInsert, parms);

				// close connection
				conn.Close();
			}

			return uniqueID;
		}

        /// <summary>
        /// Retrieve colection of inactive user id's
        /// </summary>
        /// <param name="authenticationOption">Authentication option</param>
        /// <param name="userInactiveSinceDate">Date to start search from</param>
        /// <param name="appName">Application Name</param>
        /// <returns>Collection of inactive profile id's</returns>
		public IList<string> GetInactiveProfiles(int authenticationOption, DateTime userInactiveSinceDate, string appName) {

			StringBuilder sqlSelect = new StringBuilder("SELECT Username FROM Profiles WHERE ApplicationName = :ApplicationName AND LastActivityDate <= :LastActivityDate");

			OracleParameter[] parms = {
				new OracleParameter(":ApplicationName", OracleType.VarChar, 256),
				new OracleParameter(":LastActivityDate", OracleType.DateTime)};
			parms[0].Value = appName;
			parms[1].Value = userInactiveSinceDate;

			switch(authenticationOption) {
				case AUTH_ANONYMOUS:
					sqlSelect.Append(" AND IsAnonymous = :IsAnonymous");
					Array.Resize<OracleParameter>(ref parms, parms.Length + 1);
					parms[2] = new OracleParameter(":IsAnonymous", OracleType.VarChar, 1);
					parms[2].Value = OracleHelper.OraBit(true);
					break;
				case AUTH_AUTHENTICATED:
					sqlSelect.Append(" AND IsAnonymous = :IsAnonymous");
					Array.Resize<OracleParameter>(ref parms, parms.Length + 1);
					parms[2] = new OracleParameter("@IsAnonymous", OracleType.VarChar, 1);
					parms[2].Value = OracleHelper.OraBit(false);
					break;
				default:
					break;
			}

			IList<string> usernames = new List<string>();

			OracleDataReader dr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlSelect.ToString(), parms);
			while(dr.Read()) {
				usernames.Add(dr.GetString(0));
			}

			dr.Close();
			return usernames;
		}

        /// <summary>
        /// Delete user's profile
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="appName">Application Name</param>
        /// <returns>True, if profile successfully deleted</returns>
		public bool DeleteProfile(string userName, string appName) {

			int uniqueID = GetUniqueID(userName, false, true, appName);

			string sqlDelete = "DELETE FROM Profiles WHERE UniqueID = :UniqueID";
			OracleParameter param = new OracleParameter(":UniqueId", OracleType.Number, 10);
			param.Value = uniqueID;

			int numDeleted = OracleHelper.ExecuteNonQuery(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlDelete, param);

			if(numDeleted <= 0)
				return false;
			else
				return true;
		}

        /// <summary>
        /// Retrieve profile information
        /// </summary>
        /// <param name="authenticationOption">Authentication option</param>
        /// <param name="usernameToMatch">User name</param>
        /// <param name="userInactiveSinceDate">Date to start search from</param>
        /// <param name="appName">Application Name</param>
        /// <param name="totalRecords">Number of records to return</param>
        /// <returns>Collection of profiles</returns>
		public IList<CustomProfileInfo> GetProfileInfo(int authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, string appName, out int totalRecords) {

			// Retrieve the total count.
			StringBuilder sqlSelect1 = new StringBuilder("SELECT COUNT(*) FROM Profiles WHERE ApplicationName = :ApplicationName");
			OracleParameter[] parms1 = {
				new OracleParameter(":ApplicationName", OracleType.VarChar, 256)};
			parms1[0].Value = appName;

			// Retrieve the profile data.
			StringBuilder sqlSelect2 = new StringBuilder("SELECT Username, LastActivityDate, LastUpdatedDate, IsAnonymous FROM Profiles WHERE ApplicationName = :ApplicationName");
			OracleParameter[] parms2 = { new OracleParameter(":ApplicationName", OracleType.VarChar, 256) };
			parms2[0].Value = appName;

			int arraySize;

			// If searching for a user name to match, add the command text and parameters.
			if(usernameToMatch != null) {
				arraySize = parms1.Length;

				sqlSelect1.Append(" AND Username LIKE :Username");
				Array.Resize<OracleParameter>(ref parms1, arraySize + 1);
				parms1[arraySize] = new OracleParameter(":Username", OracleType.VarChar, 256);
				parms1[arraySize].Value = usernameToMatch;

				sqlSelect2.Append(" AND Username LIKE :Username");
				Array.Resize<OracleParameter>(ref parms2, arraySize + 1);
				parms2[arraySize] = new OracleParameter(":Username", OracleType.VarChar, 256);
				parms2[arraySize].Value = usernameToMatch;
			}


			// If searching for inactive profiles, 
			// add the command text and parameters.
			if(userInactiveSinceDate != null) {
				arraySize = parms1.Length;

				sqlSelect1.Append(" AND LastActivityDate >= :LastActivityDate");
				Array.Resize<OracleParameter>(ref parms1, arraySize + 1);
				parms1[arraySize] = new OracleParameter(":LastActivityDate", OracleType.DateTime);
				parms1[arraySize].Value = (DateTime)userInactiveSinceDate;

				sqlSelect2.Append(" AND LastActivityDate >= :LastActivityDate");
				Array.Resize<OracleParameter>(ref parms2, arraySize + 1);
				parms2[arraySize] = new OracleParameter(":LastActivityDate", OracleType.DateTime);
				parms2[arraySize].Value = (DateTime)userInactiveSinceDate;
			}


			// If searching for a anonymous or authenticated profiles,    
			// add the command text and parameters.	   
			if(authenticationOption != AUTH_ALL) {

				arraySize = parms1.Length;

				sqlSelect1.Append(" AND IsAnonymous = :IsAnonymous");
				Array.Resize<OracleParameter>(ref parms1, arraySize + 1);
				parms1[arraySize] = new OracleParameter(":IsAnonymous", OracleType.VarChar, 1);

				sqlSelect2.Append(" AND IsAnonymous = :IsAnonymous");
				Array.Resize<OracleParameter>(ref parms2, arraySize + 1);
				parms2[arraySize] = new OracleParameter(":IsAnonymous", OracleType.VarChar, 1);

				switch(authenticationOption) {
					case AUTH_ANONYMOUS:
						parms1[arraySize].Value = OracleHelper.OraBit(true);
						parms2[arraySize].Value = OracleHelper.OraBit(true);
						break;
					case AUTH_AUTHENTICATED:
						parms1[arraySize].Value = OracleHelper.OraBit(false);
						parms2[arraySize].Value = OracleHelper.OraBit(false);
						break;
					default:
						break;
				}
			}

			IList<CustomProfileInfo> profiles = new List<CustomProfileInfo>();

			// Get the profile count.
			totalRecords = 0;
			int.TryParse(OracleHelper.ExecuteScalar(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlSelect1.ToString(), parms1).ToString(), out totalRecords);

			// No profiles found.
			if(totalRecords <= 0)
				return profiles;

			OracleDataReader dr;
			dr = OracleHelper.ExecuteReader(OracleHelper.ConnectionStringProfile, CommandType.Text, sqlSelect2.ToString(), parms2);
			while(dr.Read()) { 				
				CustomProfileInfo profile = new CustomProfileInfo(dr.GetString(0), dr.GetDateTime(1), dr.GetDateTime(2), OracleHelper.OraBool(dr.GetString(3)));
				profiles.Add(profile);
			}
			dr.Close();

			return profiles;
		}
	}
}
