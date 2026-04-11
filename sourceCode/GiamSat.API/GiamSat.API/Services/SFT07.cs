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
    public class SFT07 : ISFT07
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT07(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;

            _context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<FT07_RevoConfig>>> GetAll()
        {
            try
            {
                return await Result<List<FT07_RevoConfig>>.SuccessAsync(_context.FT07_RevoConfigs.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT07_RevoConfig>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT07_RevoConfig>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT07_RevoConfigs.FindAsync(id);
                return await Result<FT07_RevoConfig>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT07_RevoConfig>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT07_RevoConfig>> Insert([Body] FT07_RevoConfig model)
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
                await _context.FT07_RevoConfigs.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT07_RevoConfig>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT07_RevoConfig>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT07_RevoConfig>> Update([Body] FT07_RevoConfig model)
        {
            try
            {
                _context.FT07_RevoConfigs.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT07_RevoConfig>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT07_RevoConfig>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<bool>> Delete([Path] Guid id)
        {
            try
            {
                var entity = await _context.FT07_RevoConfigs.FindAsync(id);
                if (entity != null)
                {
                    _context.FT07_RevoConfigs.Remove(entity);
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
