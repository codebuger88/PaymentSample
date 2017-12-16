using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ECPay.Controllers
{
    public class ExpressController : Controller
    {
        private static readonly string hashKey = ConfigurationManager.AppSettings["HashKey"];
        private static readonly string hashIV = ConfigurationManager.AppSettings["HashIV"];
        private static readonly string merchantID = ConfigurationManager.AppSettings["MerchantID"];
        private static readonly string expressMapUrl = ConfigurationManager.AppSettings["ExpressMapUrl"];
        private static readonly string expressCreateUrl = ConfigurationManager.AppSettings["ExpressCreateUrl"];
        private static readonly string expressTradeInfoUrl = ConfigurationManager.AppSettings["ExpressTradeInfoUrl"];
        private static readonly string callbackHost = ConfigurationManager.AppSettings["CallbackHost"];
        private readonly NameValueCollection _orderTransactions;

        public ExpressController()
        {
            _orderTransactions = new NameValueCollection
            {
                { "MerchantID", merchantID },
            };
        }

        public ActionResult Map()
        {
            _orderTransactions.Add("LogisticsType", "CVS");
            _orderTransactions.Add("LogisticsSubType", "UNIMART");
            _orderTransactions.Add("IsCollection", "Y");
            _orderTransactions.Add("ServerReplyURL", callbackHost + "Express/GetStore");

            ViewBag.PayUrl = expressMapUrl;
            ViewBag.OrderTransactions = _orderTransactions;

            return View();
        }

        public ActionResult GetStore(FormCollection forms)
        {
            ViewBag.CVSStoreID = forms["CVSStoreID"];
            ViewBag.CVSStoreName = forms["CVSStoreName"];
            ViewBag.CVSAddress = forms["CVSAddress"];

            return View();
        }

        public ActionResult Create()
        {
            int amount = 200;

            _orderTransactions.Add("MerchantTradeNo", $"ORDER{DateTime.Now:yyyyMMddHHmmss}");
            _orderTransactions.Add("GoodsAmount", amount.ToString());
            _orderTransactions.Add("CollectionAmount", amount.ToString());
            _orderTransactions.Add("ReceiverName", "receiver");
            _orderTransactions.Add("ReceiverCellPhone", "0910123456");
            _orderTransactions.Add("ReceiverEmail", "sample@testing.com");
            _orderTransactions.Add("ReceiverStoreID", "991182");
            _orderTransactions.Add("MerchantTradeDate", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            _orderTransactions.Add("LogisticsType", "CVS");
            _orderTransactions.Add("LogisticsSubType", "UNIMART");
            _orderTransactions.Add("IsCollection", "Y");
            _orderTransactions.Add("GoodsName", "");
            _orderTransactions.Add("SenderName", "testing");
            _orderTransactions.Add("SenderPhone", "");
            _orderTransactions.Add("SenderCellPhone", "");
            _orderTransactions.Add("TradeDesc", "");
            _orderTransactions.Add("ServerReplyURL", callbackHost + "Express/StatusReceive");
            _orderTransactions.Add("ClientReplyURL", "");
            _orderTransactions.Add("LogisticsC2CReplyURL", "");
            _orderTransactions.Add("Remark", "");
            _orderTransactions.Add("PlatformID", "");
            _orderTransactions.Add("CheckMacValue", GetCheckMacValue(_orderTransactions));

            string responseResult = PostRequest(expressCreateUrl, string.Join("&", _orderTransactions.AllKeys
                .Select(key => (key + "=" + _orderTransactions[key])
                )));
            string[] result = responseResult.Split('|');

            ViewBag.Result = responseResult;

            if (result[0] != "1")
            {
                ViewBag.Error = result.ElementAtOrDefault(1) ?? result[0];
            }

            return View();
        }

        public ActionResult StatusReceive(FormCollection forms)
        {
            string checkMacValue = forms["CheckMacValue"];

            forms.Remove("CheckMacValue");
            if (checkMacValue == GetCheckMacValue(forms))
            {
                string merchantTradeNo = forms["MerchantTradeNo"];
                string rtnCode = forms["RtnCode"];
                string rtnMsg = forms["RtnMsg"];
                string allPayLogisticsID = forms["AllPayLogisticsID"];
                string logisticsType = forms["LogisticsType"];
                string logisticsSubType = forms["LogisticsSubType"];
                string goodsAmount = forms["GoodsAmount"];
                string updateStatusDate = forms["UpdateStatusDate"];
                string receiverName = forms["ReceiverName"];
                string receiverPhone = forms["ReceiverPhone"];
                string receiverCellPhone = forms["ReceiverCellPhone"];
                string receiverEmail = forms["ReceiverEmail"];
                string receiverAddress = forms["ReceiverAddress"];
                string cvsPaymentNo = forms["CVSPaymentNo"];
                string cvsValidationNo = forms["CVSValidationNo"];
                string bookingNote = forms["BookingNote"];

                return Content("1|OK");
            }

            return Content("|Error");
        }

        public ActionResult ShipmentNo(string id)
        {
            _orderTransactions.Add("AllPayLogisticsID", id);
            _orderTransactions.Add("TimeStamp", ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString());
            _orderTransactions.Add("CheckMacValue", GetCheckMacValue(_orderTransactions));

            string responseResult = PostRequest(expressTradeInfoUrl, string.Join("&", _orderTransactions.AllKeys
                .Select(key => (key + "=" + _orderTransactions[key])
                )));

            ViewBag.Result = responseResult;
            ViewBag.ShipmentNo = HttpUtility.ParseQueryString(responseResult)["ShipmentNo"] ?? string.Empty;

            return View();
        }

        private static string PostRequest(string url, string data)
        {
            string result = string.Empty;
            byte[] postData = Encoding.GetEncoding(65001).GetBytes(data);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            CookieContainer cCookie = new CookieContainer();
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; zh-TW) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.126 Safari/533.4";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Accept = "application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
            webRequest.Method = "POST";
            webRequest.ContentLength = postData.Length;
            webRequest.CookieContainer = cCookie;

            try
            {
                Stream stream = webRequest.GetRequestStream();
                stream.Write(postData, 0, postData.Length);

                using (var response = webRequest.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var responseReader = new StreamReader(responseStream))
                        {
                            result = responseReader.ReadToEnd();
                        }
                    }
                }

                stream.Close();
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }

        private string GetCheckMacValue(NameValueCollection collections)
        {
            string result = Md5Hash(
                HttpUtility.UrlEncode(("HashKey=" + hashKey + "&" + string.Join("&", collections.AllKeys.OrderBy(key => key)
                                           .Select(key => (key + "=" + collections[key])
                                           )) + "&HashIV=" + hashIV)).ToLower());

            return result.ToUpper();
        }

        private string Md5Hash(string input)
        {
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(input, "MD5");
        }
    }
}