using AutoMapper;
using UserApi.DTOs;
using UserApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AccountTrack.Api.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ========== Transaction Mappings ==========
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.ToAccountId, opt => opt.MapFrom(src => src.TargetAccountId));

        CreateMap<CreateTransactionDto, Transaction>()
            .ForMember(dest => dest.TransactionId, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => MapIntToTransactionTypeString(src.TransactionType)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => TransactionStatus.Pending))
            .ForMember(dest => dest.Flag, opt => opt.MapFrom(src => "Normal"))
            .ForMember(dest => dest.TargetAccountId, opt => opt.MapFrom(src => src.ToAccountId))
            .ForMember(dest => dest.Date, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore());

        // ========== Account Mappings ==========
        CreateMap<Account, AccountDto>()
            .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => (int)src.AccountType))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

        CreateMap<CreateAccountDto, Account>()
            .ForMember(dest => dest.AccountId, opt => opt.Ignore())
            .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => (AccountType)src.AccountType))
            .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => 0)) // Default balance to 0
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AccountStatus.Pending));

        CreateMap<UpdateAccountDto, Account>()
            .ForMember(dest => dest.AccountId, opt => opt.Ignore())
            .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => (AccountType)src.AccountType))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (AccountStatus)src.Status))
            .ForMember(dest => dest.Balance, opt => opt.Ignore()); // Balance is not updated via PUT

        // ========== User Mappings ==========
        //CreateMap<User, UserDto>()
        //    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => (int)src.Role));

        // ========== Notification Mappings ==========
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));
    }

    private static string MapIntToTransactionTypeString(int type)
    {
        var transactionType = type switch
        {
            1 => TransactionType.Deposit,
            2 => TransactionType.Withdrawal,
            3 => TransactionType.Transfer,
            _ => throw new ArgumentException($"Invalid transaction type: {type}. Must be 1 (Deposit), 2 (Withdrawal), or 3 (Transfer).")
        };

        return transactionType switch
        {
            TransactionType.Deposit => "Deposit",
            TransactionType.Withdrawal => "Withdrawal",
            TransactionType.Transfer => "Transfer",
            _ => transactionType.ToString()
        };
    }
}