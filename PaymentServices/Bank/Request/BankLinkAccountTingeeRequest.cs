namespace study4_be.PaymentServices.Bank.Request
{
    public class BankLinkAccountTingeeRequest
    {
        public string AccountType { get; set; } = "personal-account"; // Hoặc "business-account"
        public string BankName { get; set; } // Ví dụ: OCB, MBB, BIDV, ACB, VPB
        public string AccountNumber { get; set; } 
        public string AccountName { get; set; } 
        public string Identity { get; set; }
        public string Mobile { get; set; } 
        public string Email { get; set; }
        public string RequestType { get; set; }
        public string UserId { get; set; }
    }
}