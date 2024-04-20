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
    public class SFT06 : ISFT06
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT06(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;

            _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(30));
        }

        public async Task<Result<List<FT06>>> GetAll()
        {
            try
            {
                return await Result<List<FT06>>.SuccessAsync(_context.FT06.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT06>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT06>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT06.FindAsync(id);
                return await Result<FT06>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT06>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT06>> Insert([Body] FT06 model)
        {
            try
            {
                await _context.FT06.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT06>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT06>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT06>> Update([Body] FT06 model)
        {
            try
            {
                _context.FT06.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT06>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT06>.FailAsync(ex.Message);
            }
        }
    }
}
