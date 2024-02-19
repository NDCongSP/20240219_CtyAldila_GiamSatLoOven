 using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public interface IRepository<TId, T> where T : class
    {
        [Get(ApiRoutes.GetAll)]
        Task<Result<List<T>>> GetAll();

        [Get(ApiRoutes.GetById)]
        Task<Result<T>> GetById([Path] TId id);

        [Post(ApiRoutes.Insert)]
        Task<Result<T>> Insert([Body] T model);

        [Post(ApiRoutes.Update)]
        Task<Result<T>> Update([Body] T model);
    }
}
