namespace UserApi.Helpers;

public static class IdGenerator
{
    private static readonly Random _random = new();

    /// <summary>
    /// Generates Transaction ID in format: TXN + 4 digits (e.g., TXN0001)
    /// </summary>
    public static string GenerateTransactionId()
    {
        return $"TXN{_random.Next(0, 9999):D4}";
    }

    /// <summary>
    /// Generates Approval ID in format: APP + 4 digits (e.g., APP0001)
    /// </summary>
    public static string GenerateApprovalId()
    {
        return $"APP{_random.Next(0, 9999):D4}";
    }

    /// <summary>
    /// Generates Notification ID in format: NOT + 4 digits (e.g., NOT0001)
    /// </summary>
    public static string GenerateNotificationId()
    {
        return $"NOT{_random.Next(0, 9999):D4}";
    }

    /// <summary>
    /// Generates Report ID in format: RPT + 4 digits (e.g., RPT0001)
    /// </summary>
    public static string GenerateReportId()
    {
        return $"RPT{_random.Next(0, 9999):D4}";
    }
}
