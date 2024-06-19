using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace HRM_BACKEND_VSA.Utilities
{
    public class QueryBuilder<T>
    {
        private IQueryable<T> _query;
        private string _searchKey;
        private List<string> _searchColumns = new List<string>();
        private string _sortBy;
        private string _sortDirection = "desc";
        private int? _pageSize = null;
        private int? _pageNumber = 1;
        public QueryBuilder(IQueryable<T> query)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
        }

        public QueryBuilder<T> WithSearch(string? searchKey, params string[] searchColumns)
        {

            if (string.IsNullOrWhiteSpace(searchKey))
                return this;

            _searchKey = searchKey;

            if (searchColumns != null)
                _searchColumns.AddRange(searchColumns);
            return this;
        }

        public QueryBuilder<T> WithSort(string? sortBy)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return this;

            var parts = sortBy.Split('_');
            if (parts.Length != 2)
                throw new ArgumentException("SortBy parameter must be in the format 'fieldName_direction'.");

            var fieldName = parts[0];
            var direction = parts[1].ToLower() == "desc" ? "desc" : "asc";


            var property = typeof(T).GetProperties().FirstOrDefault(p => p.Name.ToLower() == fieldName.ToLower());
            if (property == null)
                throw new ArgumentException($"Property '{fieldName}' not found in entity '{typeof(T).Name}'.");

            _sortBy = property.Name;
            _sortDirection = direction;
            return this;
        }


        public async Task<object> BuildAsync()
        {
            var result = _query;

            // Applying search
            if (!string.IsNullOrWhiteSpace(_searchKey) && _searchColumns.Any())
            {
                var searchKeyLower = _searchKey.ToLower();
                foreach (var column in _searchColumns)
                {
                    result = result.Where(x => EF.Property<string>(x, column).ToLower().Contains(searchKeyLower));
                }
            }

            // Applying sorting
            if (!string.IsNullOrWhiteSpace(_sortBy))
            {
                result = _sortDirection.ToLower() == "asc" ? result.OrderBy(x => EF.Property<string>(x, _sortBy)) : result.OrderByDescending(x => EF.Property<string>(x, _sortBy));
            }


            if (_pageSize != null)
            {
                var paginatedData = await Paginator.PaginateAsync(result, (int)_pageNumber, (int)_pageSize);
                return paginatedData;
            }

            return await result.ToListAsync();
        }

        public QueryBuilder<T> Paginate(int? pageNumber, int? pageSize = null)
        {
            if (pageSize is not null)
            {
                _pageSize = pageSize;
                _pageNumber = pageNumber ?? 1;
            }
            return this;
        }
    }
}
