using System.Collections.Generic;

namespace GiamSat.Models
{
    public class PagedResult<T> : Result<List<T>>
    {
        public int TotalRecords { get; set; }

        public PagedResult() : base()
        {
        }

        public static PagedResult<T> Success(List<T> data, int totalRecords)
        {
            return new PagedResult<T>
            {
                Succeeded = true,
                Data = data,
                TotalRecords = totalRecords
            };
        }
    }
}
