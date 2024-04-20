using Microsoft.AspNetCore.Http;
using RestEase;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using GiamSat.Models;
using Microsoft.EntityFrameworkCore;

namespace GiamSat.API
{
    public class SFT05 : ISFT05
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT05(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;

            _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));
        }

        public async Task<Result<List<FT05>>> GetAll()
        {
            try
            {
                return await Result<List<FT05>>.SuccessAsync(_context.FT05.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT05>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT05>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT05.FindAsync(id);
                return await Result<FT05>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT05>.FailAsync(ex.Message);
            }
        }

        public Task<Result<List<FT05>>> GetFilter([Body] FilterModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<FT05>> Insert([Body] FT05 model)
        {
            try
            {
                await _context.FT05.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT05>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT05>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT05>> Update([Body] FT05 model)
        {
            try
            {
                _context.FT05.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT05>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT05>.FailAsync(ex.Message);
            }
        }
    }
}
