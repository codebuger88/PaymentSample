using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LINEPay.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string channelId = ConfigurationManager.AppSettings["ChannelId"];
        private static readonly string channelSecret = ConfigurationManager.AppSettings["ChannelSecret"];
        private static readonly string payUrl = ConfigurationManager.AppSettings["PayUrl"];
        private static readonly string callbackHost = ConfigurationManager.AppSettings["CallbackHost"];

        public ActionResult Reserve()
        {
            var param = new
            {
                productName = "Sample Product",
                //productImageUrl = siteUrl + "/img/logo.png",
                amount = 100,
                currency = "TWD",
                confirmUrl = callbackHost + "Home/Confirm",
                orderId = $"ORDER{DateTime.Now:yyyyMMddHHmmss}",
                capture = true
            };

            var responseJson = RequestGateway("request", param);
            dynamic responseObj = JObject.Parse(responseJson);
            ViewBag.ResponseJson = JsonConvert.SerializeObject(responseObj, Formatting.Indented);

            if (responseObj.returnCode == "0000")
            {
                string transactionId = responseObj.info.transactionId.ToString();
                ViewBag.PaymentUrl = responseObj.info.paymentUrl.web;

                //update your order transactionId here, confirm callback will query the order via transactionId
            }
            else
            {
                ViewBag.ErrorMessage = "Faild, please try again later.";
            }

            return View();
        }

        [HttpGet]
        public ActionResult Confirm(string transactionId)
        {
            var param = new
            {
                amount = 100, //get amount via transactionId from db order
                currency = "TWD"
            };

            var responseJson = RequestGateway(transactionId + "/confirm", param);
            dynamic responseObj = JObject.Parse(responseJson);
            ViewBag.ResponseJson = JsonConvert.SerializeObject(responseObj, Formatting.Indented);

            if (responseObj.returnCode == "0000")
            {
                //pay successul
            }
            else
            {
                ViewBag.ErrorMessage = responseObj.returnMessage;
                //pay unsuccessful 
            }

            return View();
        }

        private string RequestGateway(string path, object param)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(payUrl + path);
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";
            webRequest.Headers["X-LINE-ChannelId"] = channelId;
            webRequest.Headers["X-LINE-ChannelSecret"] = channelSecret;

            using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
            {
                streamWriter.Write(JsonConvert.SerializeObject(param));
                streamWriter.Flush();
            }

            var httpResponse = (HttpWebResponse)webRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}