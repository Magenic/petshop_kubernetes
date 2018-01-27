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
    public class ItemController : ApiController
    {
        private IItem _dal = new Item();
        [Route("item/byproduct/{productid}")]
        [HttpGet]
        public IList<ItemInfo> GetItemByProduct(string productId)
        {
            return _dal.GetItemsByProduct(productId);
        }

        // GET api/values/5
        [Route("item/{itemId}")]
        public ItemInfo GetItem(string itemId)
        {
            return _dal.GetItem(itemId);
        }
    }
}
