namespace study4_be.PaymentServices.Bank.Request;

public class ConfirmBankLinkRequest
{
    public string BankName { get; set; } // Ngân hàng
    public string ConfirmId { get; set; } // Mã tham chiếu của ngân hàng
    public string OtpNumber { get; set; } // Mã OTP
    public string WalletId { get; set; }
}