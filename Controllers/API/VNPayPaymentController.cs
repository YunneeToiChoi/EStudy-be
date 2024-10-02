using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class VNPayPaymentController : ControllerBase
    {
        private readonly ILogger<VNPayPaymentController> _logger;

        // VNPay Configuration
        private readonly string vnp_TmnCode = "";
        private readonly string vnp_HashSecret = "";
        private readonly string vnp_PaymentUrl = "";
        private readonly string vnp_ReturnUrl = ""; // URL trả về cho FE
        private readonly string vnp_IpnUrl = ""; // URL IPN cho BE  

        public VNPayPaymentController(ILogger<VNPayPaymentController> logger)
        {
            _logger = logger;
        }

        [HttpPost("MakePayment")]
        public IActionResult MakePayment([FromBody] VnPayPaymentRequest request)
        {
            try
            {
                string paymentUrl = GetPaymentUrl(request);
                return Ok(new { status = 200, url = paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực hiện yêu cầu thanh toán VNPay");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu thanh toán");
            }
        }

        [HttpGet("PaymentReturn")]
        public IActionResult PaymentReturn([FromQuery] VnPayPaymentReturn request)
        {
            if (request.vnp_ResponseCode == "00")
            {
                return Ok("Payment successful");
            }
            else
            {
                return BadRequest("Payment failed");
            }
        }

        [HttpGet("PaymentIpn")]
        public IActionResult PaymentIpn([FromQuery] VnPayPaymentReturn request)
        {
            if (request.vnp_ResponseCode == "00")
            {
                // Xử lý khi thanh toán thành công
                return Ok("Payment successful");
            }
            else
            {
                // Xử lý khi thanh toán thất bại
                return BadRequest("Payment failed");
            }
        }

        private string GetPaymentUrl(VnPayPaymentRequest request)
        {
            SortedList<string, string> vnPayParams = new SortedList<string, string>
    {
        { "vnp_Version", "2.1.0" },
        { "vnp_Command", "pay" },
        { "vnp_TmnCode", vnp_TmnCode },
        { "vnp_Amount", ((int)(request.Amount * 100)).ToString() }, // Số tiền theo đơn vị VND
        { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
        { "vnp_CurrCode", "VND" },
        { "vnp_IpAddr", request.IpAddr },
        { "vnp_Locale", "vn" },
        { "vnp_OrderInfo", request.OrderInfo },
        { "vnp_OrderType", request.OrderType },
        { "vnp_ReturnUrl", vnp_ReturnUrl },
        { "vnp_IpnUrl", vnp_IpnUrl },
        { "vnp_TxnRef", request.TxnRef },
        { "vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss") } // Đặt thời gian hết hạn
    };

            StringBuilder queryString = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in vnPayParams)
            {
                if (queryString.Length > 0)
                {
                    queryString.Append('&');
                }
                queryString.Append(kv.Key);
                queryString.Append('=');
                queryString.Append(Uri.EscapeDataString(kv.Value));
            }

            string vnpSecureHash = HmacSHA512(vnp_HashSecret, queryString.ToString());
            string paymentUrl = $"{vnp_PaymentUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";

            return paymentUrl;
        }

        private string HmacSHA512(string key, string inputData)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData));
                StringBuilder hex = new StringBuilder(hashValue.Length * 2);
                foreach (byte b in hashValue)
                {
                    hex.AppendFormat("{0:x2}", b);
                }
                return hex.ToString();
            }
        }
    }

    public class VnPayPaymentRequest
    {
        public decimal Amount { get; set; }
        public string IpAddr { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; }
        public string TxnRef { get; set; }
        public string ExpireDate { get; set; }
    }

    public class VnPayPaymentReturn
    {
        public string vnp_ResponseCode { get; set; }
    }
}
