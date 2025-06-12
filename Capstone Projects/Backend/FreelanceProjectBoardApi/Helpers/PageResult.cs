namespace FreelanceProjectBoardApi.Helpers
{
    public class PageResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public PaginationInfo pagination { get; set; } = new PaginationInfo();
    }

    public class PaginationInfo
    {
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}