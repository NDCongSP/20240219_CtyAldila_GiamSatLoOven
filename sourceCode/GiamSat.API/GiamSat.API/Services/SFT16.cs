using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Result<List<FT16_SandingLogData>>> GetAll()
        {
            try
            {
                var list = await _context.FT16_SandingLogDatas.AsNoTracking().ToListAsync();
                return await Result<List<FT16_SandingLogData>>.SuccessAsync(list);
            }
            catch (Exception ex)
            {
                return await Result<List<FT16_SandingLogData>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT16_SandingLogData>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT16_SandingLogDatas.FindAsync(id);
                return await Result<FT16_SandingLogData>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT16_SandingLogData>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT16_SandingLogData>> Insert([Body] FT16_SandingLogData model)
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
                await _context.FT16_SandingLogDatas.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT16_SandingLogData>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT16_SandingLogData>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT16_SandingLogData>> Update([Body] FT16_SandingLogData model)
        {
            try
            {
                _context.FT16_SandingLogDatas.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT16_SandingLogData>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT16_SandingLogData>.FailAsync(ex.Message);
            }
        }
    }
}
