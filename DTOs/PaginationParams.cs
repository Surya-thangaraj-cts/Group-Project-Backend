namespace UserApi.DTOs;

public class PaginationParams
{
    private int _pageSize = 10;
    private const int MaxPageSize = 50;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            10 => 10,
            20 => 20,
            50 => 50,
            _ => 10 // Default to 10 if invalid
        };
    }
}