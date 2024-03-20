using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SFT03 : ISFT03
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT03(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<List<FT03>>> GetAll()
        {
            try
            {
                return await Result<List<FT03>>.SuccessAsync(_context.FT03.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT03>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT03>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT03.FindAsync(id);
                return await Result<FT03>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT03>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT03>> Insert([Body] FT03 model)
        {
            try
            {
                await _context.FT03.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT03>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT03>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT03>> Update([Body] FT03 model)
        {
            try
            {
                _context.FT03.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT03>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT03>.FailAsync(ex.Message);
            }
        }
    }
}
