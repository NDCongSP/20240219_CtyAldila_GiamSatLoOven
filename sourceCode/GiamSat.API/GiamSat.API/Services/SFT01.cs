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
    public class SFT01 : ISFT01
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _httpContextAccessor;

        public SFT01(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;

            _context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);

            ////Get thông tin claim của user login
            //var e=_httpContextAccessor.HttpContext.User.Identity.Name;

            //var r = _httpContextAccessor.HttpContext.User.FindFirst("testabc");
        }

        public async Task<Result<List<FT01>>> GetAll()
        {
            try
            {
                return await Result<List<FT01>>.SuccessAsync( _context.FT01.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT01>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT01>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT01.FindAsync(id);
                return await Result<FT01>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT01>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT01>> Insert([Body] FT01 model)
        {
            try
            {
                await _context.FT01.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT01>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT01>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT01>> Update([Body] FT01 model)
        {
            try
            {
                _context.FT01.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT01>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT01>.FailAsync(ex.Message);
            }
        }
    }
}
