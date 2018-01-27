using PetShop.Model;
using System.Collections.Generic;
using System.Web.Http;
using PetShop.OracleDAL;
using PetShop.IDAL;

namespace OrderService
{
    public class OrderController : ApiController
    {
        private IOrder dal = new Order();

        [HttpGet]
        [Route("order")]
        public OrderInfo GetOrder(int orderId)
        {
            return dal.GetOrder(orderId);
        }

        [HttpPost]
        [Route("order")]
        public void PostOrder([FromBody]OrderInfo orderInfo)
        {
            dal.Insert(orderInfo);
        }
    }
}