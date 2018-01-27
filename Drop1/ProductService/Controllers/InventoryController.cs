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
    public class InventoryController : ApiController
    {
        private IInventory _dal = new Inventory();
        [HttpGet]
        [Route("inventory")]
        public ItemInfo Get(string itemId)
        {
            var itemQty = _dal.CurrentQtyInStock(itemId);
            ItemInfo info = new ItemInfo();
            info.Id = itemId;
            info.Quantity = itemQty;
            return info;
        }
        [HttpPost]
        [Route("inventory")]
        public void Post(LineItemInfo[] info)
        {
            _dal.TakeStock(info);
        }
    }
}
