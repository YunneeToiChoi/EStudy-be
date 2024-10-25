using NuGet.Common;
using Org.BouncyCastle.Asn1.X9;
using study4_be.Controllers.API;
using study4_be.Payment.MomoPayment;
using study4_be.PaymentServices.Momo.Config;
using study4_be.PaymentServices.Momo.Request;
using study4_be.Services;
using study4_be.Services.Payment;
using System.Security.Cryptography;
using System.Text;

namespace study4_be.Helper
{
    public class HashHelper
    {
        public string GenerateSignature(MomoPaymentRequest request,MomoConfig config)
        {
            // Dữ liệu cần tạo chữ ký
            var data = $"accessKey={config.AccessKey}&amount={request.Amount}&extraData={request.ExtraData}&ipnUrl={request.IpnUrl}&orderId={request.OrderId}&orderInfo={request.OrderInfo}&partnerCode={config.PartnerCode}&redirectUrl={request.RedirectUrl}&requestId={request.RequestId}&requestType={request.RequestType}";

            // Sử dụng khóa bí mật để tạo chữ ký
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }    
        public string GenerateSignature(BankLinkRequest request,MomoConfig config)
        {
            var rawData = $"accessKey={config.AccessKey}&" +
               $"amount={request.Amount}&" +
               $"extraData={request.ExtraData}&" +
               $"ipnUrl={request.IpnUrl}&" +
               $"orderId={request.OrderId}&" +
               $"orderInfo={request.OrderInfo}&" +
               $"partnerClientId={request.partnerClientId}&" +
               $"partnerCode={config.PartnerCode}&" +
               $"redirectUrl={request.RedirectUrl}&" +
               $"requestId={request.RequestId}&" +
               $"requestType={request.RequestType}";
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        public string GenerateSignature(DecryptAesTokenRequest request, MomoConfig config)
        {
            var rawData = $"accessKey={config.AccessKey}&" +
               $"callbackToken={request.CallbackToken}&" +
               $"orderId={request.OrderId}&" +
               $"partnerClientId={request.PartnerClientId}&" +
               $"partnerCode={config.PartnerCode}&" +
               $"requestId={request.RequestId}&";
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        public string GenerateDisbursementSignature(DisbursementRequest request, MomoConfig config,string disbursementMethod)
        {
            // Dữ liệu cần tạo chữ ký
            var data = $"accessKey={config.AccessKey}&" +
                $"amount={request.Amount}&" +
                $"disbursementMethod={disbursementMethod}&" +
                $"extraData={request.ExtraData}&" +
                $"orderId={request.OrderId}&" +
                $"orderInfo={request.OrderInfo}&" +
                $"partnerCode={config.PartnerCode}&" +
                $"requestId={request.RequestId}&" +
                $"requestType={request.RequestType}";

            // Sử dụng khóa bí mật để tạo chữ ký
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }     
        public string GenerateDisbursementSignature(DisbursementRequest request, MomoTestConfig config,string disbursementMethod)
        {
            // Dữ liệu cần tạo chữ ký
            var data = $"accessKey={config.AccessKey}&" +
                $"amount={request.Amount}&" +
                $"disbursementMethod={disbursementMethod}&" +
                $"extraData={request.ExtraData}&" +
                $"orderId={request.OrderId}&" +
                $"orderInfo={request.OrderInfo}&" +
                $"partnerCode={config.PartnerCode}&" +
                $"requestId={request.RequestId}&" +
                $"requestType={request.RequestType}";

            // Sử dụng khóa bí mật để tạo chữ ký
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        public string GenerateSignatureToCheckingStatus(RequestTrackingStatusMomo request,MomoConfig config)
        {
            // Dữ liệu cần tạo chữ ký
            var data = $"accessKey={config.AccessKey}&orderId={request.orderId}&partnerCode={config.PartnerCode}&requestId={request.requestId}";

            // Sử dụng khóa bí mật để tạo chữ ký
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
