namespace UserApi.Models;

public enum UserRole { Officer, Manager, Admin }
public enum AccountType { Savings, Current }
public enum AccountStatus { Active, Closed, Pending }
public enum TransactionType { Deposit, Withdrawal, Transfer }
public enum TransactionStatus { Completed, Pending, Rejected, Failed }
public enum ApprovalDecision { Pending, Approve, Reject }
public enum ApprovalType { AccountCreation, AccountUpdate, HighValueTransaction }
public enum NotificationStatus { Unread, Read }
public enum NotificationType { ApprovalReminder, SuspiciousActivity }