using GiamSat.Models;
using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SRealtimeDisplay : ISRealtimeDisplay
    {
        readonly ApplicationDbContext _dbContext;
        readonly IHttpContextAccessor _contextAccessor;

        public SRealtimeDisplay(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _dbContext = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<List<RealtimeDisplayModel>>> GetAll()
        {
            try
            {
                return await Result<List<RealtimeDisplayModel>>.SuccessAsync(_dbContext.RealTimeDisplayModel.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<RealtimeDisplayModel>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<RealtimeDisplayModel>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _dbContext.RealTimeDisplayModel.FindAsync(id);
                return await Result<RealtimeDisplayModel>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<RealtimeDisplayModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<RealtimeDisplayModel>> GetTop1()
        {
            try
            {
                var res = _dbContext.RealTimeDisplayModel.FirstOrDefault();

                return await Result<RealtimeDisplayModel>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<RealtimeDisplayModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<RealtimeDisplayModel>> Insert([Body] RealtimeDisplayModel model)
        {
            try
            {
                await _dbContext.RealTimeDisplayModel.AddAsync(model);
                await _dbContext.SaveChangesAsync();
                return await Result<RealtimeDisplayModel>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<RealtimeDisplayModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<RealtimeDisplayModel>> Update([Body] RealtimeDisplayModel model)
        {
            try
            {
               _dbContext.RealTimeDisplayModel.Update(model);
                await _dbContext.SaveChangesAsync();

                return await Result<RealtimeDisplayModel>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<RealtimeDisplayModel>.FailAsync($"{ex.Message}");
            }
        }
    }
}
