namespace study4_be.PaymentServices.Bank.Validator;

public static class BankValidator
{
    private static readonly HashSet<string> SupportedBanks = new HashSet<string>
    {
        "OCB",
        "MBB",
        "BIDV",
        "ACB",
        "VPB"
    };

    public static bool CheckValidBankSupportTingee(string bankName)
    {
        // Kiểm tra ngân hàng có hợp lệ hay không
        return SupportedBanks.Contains(bankName.ToUpper());
    }
}