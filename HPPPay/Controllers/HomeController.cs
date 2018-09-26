using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace HPPPay.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string merchantId = ConfigurationManager.AppSettings["MerchantId"];
        private static readonly string terminalId = ConfigurationManager.AppSettings["TerminalId"];
        private static readonly string macKey = ConfigurationManager.AppSettings["MacKey"];
        private static readonly string payUrl = ConfigurationManager.AppSettings["PayUrl"];
        private static readonly string callbackHost = ConfigurationManager.AppSettings["CallbackHost"];

        public ActionResult Pay()
        {
            string orderId = $"ORDER{DateTime.Now:yyyyMMddHHmmss}";
            string notifyURL = callbackHost + "/PayReceive";
            int install = 0; //分期期數
            string signature = "MerchantID=" + merchantId +
                               "&TerminalID=" + terminalId +
                               "&OrderID=" + orderId +
                               "&TransAmt=" + 100 +
                               "&TransMode=" + (install > 0 ? "1" : "0") +
                               "&Install=" + (install > 0 ? install.ToString() : "") +
                               "&NotifyURL=" + notifyURL +
                               "&CSS_URL=" +
                               "&BankNo=" +
                               "&TEMPLATE=BOTH" +
                               "&TravelLocCode=" +
                               "&TravelStartDate=" +
                               "&TravelEndDate=" +
                               "&" + SHA256Hashed(macKey).ToLower();

            NameValueCollection _orderTransactions = new NameValueCollection
            {
                { "MerchantID", merchantId },
                { "TerminalID", terminalId },
                { "OrderID", orderId },
                { "TransAmt", "100" },
                { "TransMode", (install > 0 ? "1" : "0") },
                { "NotifyURL", notifyURL },
                { "TEMPLATE", "BOTH" },
                { "Signature", SHA256Hashed(signature).ToLower() }
            };
            if (install > 0)
            {
                _orderTransactions.Add("Install", install.ToString());
            }

            ViewBag.PayUrl = payUrl;
            ViewBag.OrderTransactions = _orderTransactions;

            return View();
        }

        [HttpPost]
        public ActionResult PayReceive(FormCollection forms)
        {
            string orderId = forms["OrderID"];
            string responseCode = forms["ResponseCode"];
            string responseMsg = forms["ResponseMsg"];

            ViewBag.OrderTransactions = new NameValueCollection
            {
                { "OrderId", orderId },
                { "ResponseCode", responseCode },
                { "ResponseMsg", responseMsg}
            };

            return View();
        }


        private string SHA256Hashed(string val)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(val);
            using (SHA256CryptoServiceProvider provider8 = new SHA256CryptoServiceProvider())
            {
                return BitConverter.ToString(provider8.ComputeHash(bytes)).Replace("-", string.Empty);
            }
        }
    }
}