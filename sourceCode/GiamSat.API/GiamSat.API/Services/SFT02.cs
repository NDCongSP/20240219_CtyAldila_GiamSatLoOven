using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SFT02 : ISFT02
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT02(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<List<FT02>>> GetAll()
        {
            try
            {
                return await Result<List<FT02>>.SuccessAsync(_context.FT02.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT02>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT02>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT02.FindAsync(id);
                return await Result<FT02>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT02>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT02>> Insert([Body] FT02 model)
        {
            try
            {
                await _context.FT02.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT02>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT02>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT02>> Update([Body] FT02 model)
        {
            try
            {
                _context.FT02.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT02>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT02>.FailAsync(ex.Message);
            }
        }
    }
}
