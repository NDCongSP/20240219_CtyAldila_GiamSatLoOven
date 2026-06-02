using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Result<List<FT15_SandingRealtime>>> GetAll()
        {
            try
            {
                var list = await _context.FT15_SandingRealtimes.AsNoTracking().ToListAsync();
                return await Result<List<FT15_SandingRealtime>>.SuccessAsync(list);
            }
            catch (Exception ex)
            {
                return await Result<List<FT15_SandingRealtime>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT15_SandingRealtime>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT15_SandingRealtimes.FindAsync(id);
                return await Result<FT15_SandingRealtime>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT15_SandingRealtime>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT15_SandingRealtime>> Insert([Body] FT15_SandingRealtime model)
        {
            try
            {
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }
                await _context.FT15_SandingRealtimes.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT15_SandingRealtime>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT15_SandingRealtime>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT15_SandingRealtime>> Update([Body] FT15_SandingRealtime model)
        {
            try
            {
                _context.FT15_SandingRealtimes.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT15_SandingRealtime>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT15_SandingRealtime>.FailAsync(ex.Message);
            }
        }
    }
}
