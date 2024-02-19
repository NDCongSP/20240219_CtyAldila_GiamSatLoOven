using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GiamSat.API
{
    public class SFT101 : ISFT101
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT101(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<List<FT101>>> GetAll()
        {
            try
            {
                var res= await _context.FT101.ToListAsync();

                return await Result<List<FT101>>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<List<FT101>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT101>> GetById([Path] Guid id)
        {
            try
            {
                var res= await _context.FT101.FindAsync(id);

                if (res==null)
                {
                    return await Result<FT101>.FailAsync("Data empty");
                }

                return await Result<FT101>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT101>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT101>> Insert([Body] FT101 model)
        {
            try
            {
                await _context.FT101.AddAsync(model);
                await _context.SaveChangesAsync();

                return await Result<FT101>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT101>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT101>> Update([Body] FT101 model)
        {
            try
            {
                _context.FT101.Update(model);
                await _context.SaveChangesAsync();

                return await Result<FT101>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT101>.FailAsync(ex.Message);
            }
        }
    }
}
