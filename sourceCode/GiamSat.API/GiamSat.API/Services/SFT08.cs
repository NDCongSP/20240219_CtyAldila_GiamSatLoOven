using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SFT08 : ISFT08
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT08(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;

            _context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<FT08_RevoRealtime>>> GetAll()
        {
            try
            {
                return await Result<List<FT08_RevoRealtime>>.SuccessAsync(_context.FT08_RevoRealtimes.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT08_RevoRealtime>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT08_RevoRealtime>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT08_RevoRealtimes.FindAsync(id);
                return await Result<FT08_RevoRealtime>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT08_RevoRealtime>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT08_RevoRealtime>> Insert([Body] FT08_RevoRealtime model)
        {
            try
            {
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }
                await _context.FT08_RevoRealtimes.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT08_RevoRealtime>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT08_RevoRealtime>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT08_RevoRealtime>> Update([Body] FT08_RevoRealtime model)
        {
            try
            {
                _context.FT08_RevoRealtimes.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT08_RevoRealtime>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT08_RevoRealtime>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<bool>> Delete([Path] Guid id)
        {
            try
            {
                var entity = await _context.FT08_RevoRealtimes.FindAsync(id);
                if (entity != null)
                {
                    _context.FT08_RevoRealtimes.Remove(entity);
                    await _context.SaveChangesAsync();
                    return await Result<bool>.SuccessAsync(true);
                }
                return await Result<bool>.FailAsync("Entity not found");
            }
            catch (Exception ex)
            {
                return await Result<bool>.FailAsync(ex.Message);
            }
        }
    }
}
