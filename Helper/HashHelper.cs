﻿using NuGet.Common;
using study4_be.Payment.MomoPayment;
using study4_be.PaymentServices.Momo.Config;
using study4_be.services.Request;
using study4_be.Services;
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
