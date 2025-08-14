using Microsoft.EntityFrameworkCore;

namespace HRM_BACKEND_VSA.Utilities
{
    public static class Paginator
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public static void SetHttpContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public class PaginatedData<T>
        {
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
            public int CurrentPage { get; set; }
            public int PageSize { get; set; }
            public string? NextPageUrl { get; set; }
            public string? PreviousPageUrl { get; set; }
            public string[] Links { get; set; }
            public string Path { get; set; }
            public List<T> Data { get; set; }
        }
        public static async Task<IEnumerable<T>> PaginateAsync<T>(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            return await Task.Run(() => source.Skip((pageNumber - 1) * pageSize).Take(pageSize));
        }

        public static string getPath()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}";
        }

        private static string GetPageUrl(int pageNumber)
        {

            var currentUrl = getPath();
            return UrlHelper.UpdateQueryStringParameters(currentUrl, new Dictionary<string, string> { { "pageNumber", pageNumber.ToString() } });
        }

        public static string[] getPaginationLinks(int totalPages)
        {
            string[] links = [];
            for (int i = 1; i <= totalPages; i++)
            {
                links = [.. links, GetPageUrl(i)];
            }
            return links;
        }

        public static async Task<PaginatedData<T>> PaginateAsync<T>(IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (_httpContextAccessor == null)
            {
                throw new InvalidOperationException("HttpContextAccessor has not been set. Call SetHttpContextAccessor method before using Paginate.");
            }
            
            var totalCount = await source.CountAsync();
            var paginatedData = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var nextPageUrl = pageNumber < totalPages ? GetPageUrl(pageNumber + 1) : null;
            var previousPageUrl = pageNumber > 1 ? GetPageUrl(pageNumber - 1) : null;

            return new PaginatedData<T>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                NextPageUrl = nextPageUrl,
                PreviousPageUrl = previousPageUrl,
                Path = getPath(),
                Links = getPaginationLinks(totalPages),
                Data = paginatedData
            };
        }


    }
}
