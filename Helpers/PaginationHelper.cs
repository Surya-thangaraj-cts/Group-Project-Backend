namespace UserApi.Helpers;

using UserApi.DTOs;
using UserApi.Helpers;

public static class PaginationHelper
{
    public static PagedResult<T> CreatePagedResult<T>(
        IEnumerable<T> source,
        int pageNumber,
        int pageSize)
    {
        var totalCount = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}