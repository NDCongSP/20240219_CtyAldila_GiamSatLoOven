using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SDisplayRealtime : ISDisplayRealtime
    {
        readonly ApplicationDbContext _dbContext;
        readonly IHttpContextAccessor _contextAccessor;//de lay cac thong tin cua client khi truy cap API.

        public SDisplayRealtime(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<DisplayRealTimeModel>> Insert([Body] DisplayRealTimeModel model)
        {
            try
            {
                await _dbContext.DisplayRealTimeModel.AddAsync(model);
                await _dbContext.SaveChangesAsync();
                return await Result<DisplayRealTimeModel>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<DisplayRealTimeModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<DisplayRealTimeModel>>> GetAll()
        {
            try
            {
                return await Result<List<DisplayRealTimeModel>>.SuccessAsync(_dbContext.DisplayRealTimeModel.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<DisplayRealTimeModel>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<DisplayRealTimeModel>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _dbContext.DisplayRealTimeModel.FindAsync(id);
                return await Result<DisplayRealTimeModel>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<DisplayRealTimeModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<DisplayRealTimeModel>> Update([Body] DisplayRealTimeModel model)
        {
            try
            {
                _dbContext.DisplayRealTimeModel.Update(model);
                await _dbContext.SaveChangesAsync();

                return await Result<DisplayRealTimeModel>.SuccessAsync("Update successfull");
            }
            catch (Exception ex)
            {
                return await Result<DisplayRealTimeModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<DisplayRealTimeModel>>> UpdateTenChuong()
        {
            try
            {
                return await Result<List<DisplayRealTimeModel>>.SuccessAsync(_dbContext.DisplayRealTimeModel.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<DisplayRealTimeModel>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<DisplayRealTimeModel>> UpdateTenChuong(DisplayRealTimeModel model)
        {
            try
            {
                var itemUpdate = await _dbContext.DisplayRealTimeModel.FindAsync(model.ChuongId);

                if (itemUpdate == null)
                {
                    return await Result<DisplayRealTimeModel>.FailAsync("Loi ko co data");
                }

                _dbContext.Entry(itemUpdate).Property(x => x.TenChuong).CurrentValue = model.TenChuong;
                _dbContext.Entry(itemUpdate).Property(x => x.NumIndex).CurrentValue = model.NumIndex;
                _dbContext.Entry(itemUpdate).Property(x => x.HightTemperature).CurrentValue = model.HightTemperature;
                _dbContext.Entry(itemUpdate).Property(x => x.Lowtemperature).CurrentValue = model.Lowtemperature;
                await _dbContext.SaveChangesAsync();

                return await Result<DisplayRealTimeModel>.SuccessAsync("Success");
            }
            catch (Exception ex)
            {
                return await Result<DisplayRealTimeModel>.FailAsync(ex.Message);
            }
        }
    }
}
