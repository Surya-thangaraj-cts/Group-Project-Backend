namespace UserApi.Helpers;

public static class IdGenerator
{
    private static readonly Random _random = new();

    /// <summary>
    /// Generates Transaction ID in format: TXN + 4 digits (e.g., TXN0001)
    /// </summary>
    public static string GenerateTransactionId()
    {
        return $"TXN{_random.Next(1000, 99999)}";
    }

    /// <summary>
    /// Generates Approval ID in format: APP + 4 digits (e.g., APP0001)
    /// </summary>
    public static string GenerateApprovalId()
    {
        return $"APP{_random.Next(1000, 99999)}";
    }

    /// <summary>
    /// Generates Notification ID in format: NF + 4 digits (e.g., NF1234)
    /// </summary>
    public static string GenerateNotificationId()
    {
        return $"NF{_random.Next(1000, 99999)}";
    }

    /// <summary>
    /// Generates Report ID in format: RP + 4 digits (e.g., RP1234)
    /// </summary>
    public static string GenerateReportId()
    {
        return $"RP{_random.Next(1000, 99999)}";
    }

    /// <summary>
    /// Generates Account ID in format: ACC + 4 digits (e.g., ACC1234)
    /// </summary>
    public static string GenerateAccountId()
    {
        return $"ACC{_random.Next(1000, 99999)}";
    }

    /// <summary>
    /// Generates Audit Log ID in format: AUD + 4 digits (e.g., AUD1234)
    /// </summary>
    public static string GenerateAuditLogId()
    {
        return $"AUD{_random.Next(1000, 99999)}";
    }

    /// <summary>
    /// Generates Account Type ID in format: AT + 4 digits (e.g., AT1234)
    /// </summary>
    public static string GenerateAccountTypeId()
    {
        return $"AT{_random.Next(1000, 99999)}";
    }
}
