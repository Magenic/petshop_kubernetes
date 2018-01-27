using PetShop.BLL;
using PetShop.IProfileDAL;
using PetShop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ProfileService.Models;

namespace ProfileService.Controllers
{
    public class ProfileController : ApiController
    {
        private static readonly IPetShopProfileProvider dal = new PetShop.OracleProfileDAL.PetShopProfileProvider();

        [HttpPost]
        [Route("profile/getaccountinfo")]
        public AddressInfo GetAccountInfo([FromBody]AccountInfo info)
        {
            return dal.GetAccountInfo(info.FirebaseGuid, info.AppName);
        }
        [HttpPost]
        [Route("profile/getcartitems")]
        public IList<CartItemInfo> GetCartItems(GetCartItems cartitems)
        {
            return dal.GetCartItems(cartitems.Username, cartitems.AppName, cartitems.IsShoppingCart);
        }
        [HttpPost]
        [Route("profile/getuniqueid")]
        public UserProfileId GetUniqueID(GetUniqueId uniqueIdInfo)
        {
            var profileId = new UserProfileId();
            profileId.UniqueId = dal.GetUniqueID(uniqueIdInfo.UserName, uniqueIdInfo.IsAuthenticated, uniqueIdInfo.IgnoreAuthenticationType, uniqueIdInfo.AppName);
            return profileId;
        }
        [HttpPost]
        [Route("profile/create")]
        public int Create([FromBody]CreateProfile createProfileInfo)
        {
            return dal.CreateProfileForUser(createProfileInfo.Username, createProfileInfo.IsAuthenticated, createProfileInfo.AppName);
        }

        [HttpPost]
        [Route("profile/setcartitems")]
        public void SetCartItems(SetCartItems cartItemInfo)
        {
            dal.SetCartItems(cartItemInfo.UniqueID, cartItemInfo.CartItems, cartItemInfo.IsShoppingCart);
        }
        [HttpPost]
        [Route("profile/updateactivitydates")]
        public void UpdateActivityDates(ActivityDates dates)
        {
            dal.UpdateActivityDates(dates.Username, dates.ActivityOnly, dates.AppName);
        }
        [HttpPost]
        [Route("profile/getinactiveprofiles")]
        public IList<string> GetInactiveProfiles(InactiveProfiles inactiveInfo)
        {
            return dal.GetInactiveProfiles(inactiveInfo.AuthenticationOption, inactiveInfo.UserInactiveSinceDate, inactiveInfo.AppName);
        }
        [HttpPost]
        [Route("profile/getprofileinfo")]
        public IList<CustomProfileInfo> GetProfileInfo(ProfileInfo profileInfo)
        {
            int totalRecords;
            return dal.GetProfileInfo(profileInfo.AuthenticationOption, profileInfo.UsernameToMatch, profileInfo.UserInactiveSinceDate, profileInfo.AppName, out totalRecords);
        }

        [HttpPost]
        [Route("profile/delete")]
        public bool DeleteProfile(AccountInfo accountInfo)
        {
            return dal.DeleteProfile(accountInfo.FirebaseGuid, accountInfo.AppName);
        }
        [HttpPost]
        [Route("profile/setaccountinfo")]
        public void SetAccountInfo(SetAccountInfo accountInfo)
        {
            dal.SetAccountInfo(accountInfo.UniqueId, accountInfo.AddressInfo);
        }
    }
}