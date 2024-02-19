using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SFT100 : ISFT100
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT100(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<List<FT100>>> GetAll()
        {
            try
            {
                var res = _context.FT100.Where(x => x.IsActived == 1).ToList();

                return await Result<List<FT100>>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<List<FT100>>.FailAsync(ex.Message);
            }
        }

        public Task<Result<FT100>> GetById([Path] Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<FT100>> Insert([Body] FT100 model)
        {
            throw new NotImplementedException();
        }

        public Task<Result<FT100>> Update([Body] FT100 model)
        {
            throw new NotImplementedException();
        }
    }
}
