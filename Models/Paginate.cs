namespace Dong_Xuan_Market_Online.Models
{
    public class Paginate
    {
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public string ActionName { get; set; }

        public Paginate() { }

        public Paginate(int totalItems, int page, int pageSize = 10)
        {
            TotalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            CurrentPage = Math.Max(1, page);
            CurrentPage = Math.Min(TotalPages, CurrentPage);
            StartPage = CurrentPage - 5;
            EndPage = CurrentPage + 4;

            if (StartPage < 1)
            {
                EndPage = EndPage - (StartPage - 1);
                StartPage = 1;
            }

            if (EndPage > TotalPages)
            {
                EndPage = TotalPages;
                if (EndPage - StartPage < 10)
                {
                    StartPage = Math.Max(1, EndPage - 9);
                }
            }

            TotalItems = totalItems;
            PageSize = pageSize;
        }
    }
}
