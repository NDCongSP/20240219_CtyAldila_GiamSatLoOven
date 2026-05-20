using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SFT16 : ISFT16
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT16(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public Task<Result<List<FT016_SandingLogData>>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Result<FT016_SandingLogData>> GetById([Path] Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<FT016_SandingLogData>> Insert([Body] FT016_SandingLogData model)
        {
            throw new NotImplementedException();
        }

        public Task<Result<FT016_SandingLogData>> Update([Body] FT016_SandingLogData model)
        {
            throw new NotImplementedException();
        }
    }
}
