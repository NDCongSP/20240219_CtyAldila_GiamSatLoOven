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
    public class SFT14 : ISFT14
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT14(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;

            _context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<FT14_TipOdFreq>>> GetAll()
        {
            try
            {
                var data = await _context.FT14_TipOdFreqs.AsNoTracking().ToListAsync();
                return await Result<List<FT14_TipOdFreq>>.SuccessAsync(data);
            }
            catch (Exception ex)
            {
                return await Result<List<FT14_TipOdFreq>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT14_TipOdFreq>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT14_TipOdFreqs.FindAsync(id);
                return await Result<FT14_TipOdFreq>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT14_TipOdFreq>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT14_TipOdFreq>> Insert([Body] FT14_TipOdFreq model)
        {
            try
            {
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }
                if (model.CreatedAt == null)
                {
                    model.CreatedAt = DateTime.Now;
                }
                await _context.FT14_TipOdFreqs.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT14_TipOdFreq>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT14_TipOdFreq>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT14_TipOdFreq>> Update([Body] FT14_TipOdFreq model)
        {
            try
            {
                _context.FT14_TipOdFreqs.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT14_TipOdFreq>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT14_TipOdFreq>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<bool>> Delete([Path] Guid id)
        {
            try
            {
                var entity = await _context.FT14_TipOdFreqs.FindAsync(id);
                if (entity != null)
                {
                    _context.FT14_TipOdFreqs.Remove(entity);
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
