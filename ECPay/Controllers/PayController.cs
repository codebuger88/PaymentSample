using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ECPay.Controllers
{
    public class PayController : Controller
    {
        private static readonly string hashKey = ConfigurationManager.AppSettings["HashKey"];
        private static readonly string hashIV = ConfigurationManager.AppSettings["HashIV"];
        private static readonly string merchantID = ConfigurationManager.AppSettings["MerchantID"];
        private static readonly string payUrl = ConfigurationManager.AppSettings["PayUrl"];
        private static readonly string callbackHost = ConfigurationManager.AppSettings["CallbackHost"];
        private readonly NameValueCollection _orderTransactions;

        public PayController()
        {
            _orderTransactions = new NameValueCollection
            {
                { "MerchantID", merchantID },
                { "EncryptType", "1" }
            };
        }

        public ActionResult ATM()
        {
            _orderTransactions.Add("MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            _orderTransactions.Add("MerchantTradeNo", $"ORDER{DateTime.Now:yyyyMMddHHmmss}");
            _orderTransactions.Add("PaymentType", "aio");
            _orderTransactions.Add("TotalAmount", "100");
            _orderTransactions.Add("TradeDesc", "Sample Pay");
            _orderTransactions.Add("ItemName", "Sample Product");
            _orderTransactions.Add("ReturnURL", callbackHost + "Pay/ATMPaidReceive");
            _orderTransactions.Add("PaymentInfoURL", callbackHost + "Pay/ATMInfoReceive");
            _orderTransactions.Add("ClientRedirectURL", callbackHost + "Pay/ATMInfoReturn");
            _orderTransactions.Add("ExpireDate", "5");
            _orderTransactions.Add("ChoosePayment", "ATM");
            _orderTransactions.Add("CheckMacValue", GetCheckMacValue(_orderTransactions));

            ViewBag.PayUrl = payUrl;
            ViewBag.OrderTransactions = _orderTransactions;

            return View();
        }

        [HttpPost]
        public ActionResult ATMInfoReceive(FormCollection forms)
        {
            string merchantTradeNo = forms["MerchantTradeNo"];
            string rtnCode = forms["RtnCode"];
            string rtnMsg = forms["RtnMsg"];
            string tradeNo = forms["TradeNo"];
            string tradeAmt = forms["TradeAmt"];
            string tradeDate = forms["TradeDate"];
            string paymentType = forms["PaymentType"];
            string bankCode = forms["BankCode"];
            string vAccount = forms["vAccount"];
            string expireDate = forms["ExpireDate"];
            string checkMacValue = forms["CheckMacValue"];

            forms.Remove("CheckMacValue");
            if (checkMacValue == GetCheckMacValue(forms))
            {
                if (rtnCode == "2")
                {
                    //get account successful
                }
                else
                {
                    //get account fail
                }

                return Content("1|OK");
            }
            else
            {
                //error handle
            }

            return Content($"0|{rtnCode} - {rtnMsg}");
        }

        [HttpPost]
        public ActionResult ATMInfoReturn(FormCollection forms)
        {
            ViewBag.OrderTransactions = forms;

            string merchantTradeNo = forms["MerchantTradeNo"];
            string rtnCode = forms["RtnCode"];
            string rtnMsg = forms["RtnMsg"];
            string checkMacValue = forms["CheckMacValue"];

            forms.Remove("CheckMacValue");
            if (checkMacValue == GetCheckMacValue(forms))
            {
                if (rtnCode == "2")
                {
                    //get account successful
                }
                else
                {
                    //get account fail
                }
            }
            else
            {
                //error
            }

            ViewBag.OrderTransactions = new NameValueCollection
            {
                { "MerchantTradeNo", merchantTradeNo },
                { "RtnCode", rtnCode },
                { "RtnMsg", rtnMsg}
            };

            return View();
        }

        [HttpPost]
        public ActionResult ATMPaidReceive(FormCollection forms)
        {
            string merchantTradeNo = forms["MerchantTradeNo"];
            string rtnCode = forms["RtnCode"];
            string rtnMsg = forms["RtnMsg"];
            string tradeNo = forms["TradeNo"];
            string tradeAmt = forms["TradeAmt"];
            string paymentDate = forms["PaymentDate"];
            string paymentType = forms["PaymentType"];
            string tradeDate = forms["TradeDate"];
            string simulatePaid = forms["SimulatePaid"];
            string checkMacValue = forms["CheckMacValue"];

            forms.Remove("CheckMacValue");
            if (checkMacValue == GetCheckMacValue(forms))
            {
                if (simulatePaid == "0")
                {
                    if (rtnCode == "1")
                    {
                        //pay successful
                    }
                    else
                    {
                        //pay fail
                    }
                }
                else
                {
                    //error handle
                }

                return Content("1|OK");
            }
            else
            {
                //error handle
            }

            return Content($"0|{rtnCode} - {rtnMsg}");
        }

        public ActionResult CreditCard()
        {
            _orderTransactions.Add("MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            _orderTransactions.Add("MerchantTradeNo", $"ORDER{DateTime.Now:yyyyMMddHHmmss}");
            _orderTransactions.Add("PaymentType", "aio");
            _orderTransactions.Add("TotalAmount", "100");
            _orderTransactions.Add("TradeDesc", "Sample Pay");
            _orderTransactions.Add("ItemName", "Sample Product");
            _orderTransactions.Add("ReturnURL", callbackHost + "Pay/CreditCardReceive");
            _orderTransactions.Add("OrderResultURL", callbackHost + "Pay/CreditCardReturn");
            _orderTransactions.Add("ChoosePayment", "Credit");
            _orderTransactions.Add("CheckMacValue", GetCheckMacValue(_orderTransactions));

            ViewBag.PayUrl = payUrl;
            ViewBag.OrderTransactions = _orderTransactions;

            return View();
        }

        [HttpPost]
        public ActionResult CreditCardReceive(FormCollection forms)
        {
            string merchantTradeNo = forms["MerchantTradeNo"];
            string rtnCode = forms["RtnCode"];
            string rtnMsg = forms["RtnMsg"];
            string tradeNo = forms["TradeNo"];
            string tradeAmt = forms["TradeAmt"];
            string paymentDate = forms["PaymentDate"];
            string tradeDate = forms["TradeDate"];
            string simulatePaid = forms["SimulatePaid"];
            string checkMacValue = forms["CheckMacValue"];

            forms.Remove("CheckMacValue");
            if (checkMacValue == GetCheckMacValue(forms))
            {
                if (simulatePaid == "0")
                {
                    if (rtnCode == "1")
                    {
                        //pay successful
                    }
                    else
                    {
                        //pay fail
                    }
                }
                else
                {
                    //error handle
                }

                return Content("1|OK");
            }
            else
            {
                //error handle
            }

            return Content($"0|{rtnCode} - {rtnMsg}");
        }

        [HttpPost]
        public ActionResult CreditCardReturn(FormCollection forms)
        {
            string merchantTradeNo = forms["MerchantTradeNo"];
            string rtnCode = forms["RtnCode"];
            string rtnMsg = forms["RtnMsg"];
            string simulatePaid = forms["SimulatePaid"];
            string checkMacValue = forms["CheckMacValue"];

            forms.Remove("CheckMacValue");
            if (checkMacValue == GetCheckMacValue(forms))
            {
                if (simulatePaid == "0")
                {
                    if (rtnCode == "1")
                    {
                        //pay successful
                    }
                    else
                    {
                        //pay fail
                    }
                }
                else
                {
                    //it's simulate paid
                }
            }
            else
            {
                //error
            }

            ViewBag.OrderTransactions = new NameValueCollection
            {
                { "MerchantTradeNo", merchantTradeNo },
                { "RtnCode", rtnCode },
                { "RtnMsg", rtnMsg}
            };

            return View();
        }

        private string GetCheckMacValue(NameValueCollection collections)
        {
            var sorted = collections.AllKeys.OrderBy(key => key)
                .Select(key => new KeyValuePair<string, string>(key, collections[key]));

            string result = Sha256Encrypt(
                HttpUtility.UrlEncode(("HashKey=" + hashKey + "&" + string.Join("&", sorted.Select(s => (s.Key + "=" + s.Value))) + "&HashIV=" + hashIV)).ToLower());

            return result.ToUpper();
        }

        public string Sha256Encrypt(string encrypt)
        {
            byte[] inputByteArray = Encoding.UTF8.GetBytes(encrypt);

            using (SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider())
            {
                return BitConverter.ToString(csp.ComputeHash(inputByteArray)).Replace("-", string.Empty);
            }
        }
    }
}