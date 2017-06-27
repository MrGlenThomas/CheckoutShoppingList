namespace Glen.ShoppingList.Infrastructure.Data
{
    public class PaginationArgs
    {
        public int PageNumber { get; }

        public int PageSize { get; }

        public PaginationArgs(int pageSize, int pageNumber)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
        }
    }
}
