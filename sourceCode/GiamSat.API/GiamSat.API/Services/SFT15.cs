using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SFT15 : ISFT15
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT15(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public Task<Result<List<FT15_SandingRealtime>>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Result<FT15_SandingRealtime>> GetById([Path] Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<FT15_SandingRealtime>> Insert([Body] FT15_SandingRealtime model)
        {
            throw new NotImplementedException();
        }

        public Task<Result<FT15_SandingRealtime>> Update([Body] FT15_SandingRealtime model)
        {
            throw new NotImplementedException();
        }
    }
}
