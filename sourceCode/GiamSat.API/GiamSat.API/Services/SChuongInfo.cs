using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SChuongInfo : ISChuongInfo
    {
        readonly ApplicationDbContext _dbContext;
        readonly IHttpContextAccessor _contextAccessor;

        public SChuongInfo(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<List<ChuongInfoModel>>> GetAll()
        {
            try
            {
                return await Result<List<ChuongInfoModel>>.SuccessAsync(await _dbContext.ChuongInfoModel.ToListAsync());
            }
            catch (Exception ex)
            {
                return await Result<List<ChuongInfoModel>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<ChuongInfoModel>> GetById([Path] Guid id)
        {
            try
            {
                return await Result<ChuongInfoModel>.SuccessAsync(await _dbContext.ChuongInfoModel.FindAsync(id));
            }
            catch (Exception ex)
            {
                return await Result<ChuongInfoModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<ChuongInfoModel>> GetByName([Path] string id)
        {
            try
            {
                var res = await _dbContext.ChuongInfoModel.FirstOrDefaultAsync(x => x.TenChuong == id);
                return await Result<ChuongInfoModel>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<ChuongInfoModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<ChuongInfoModel>> Insert([Body] ChuongInfoModel model)
        {
            try
            {
                await _dbContext.ChuongInfoModel.AddAsync(model);
                await _dbContext.SaveChangesAsync();

                return await Result<ChuongInfoModel>.SuccessAsync("Insert successfull.");
            }
            catch (Exception ex)
            {
                return await Result<ChuongInfoModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<ChuongInfoModel>> Update([Body] ChuongInfoModel model)
        {
            try
            {
                _dbContext.ChuongInfoModel.Update(model);
                await _dbContext.SaveChangesAsync();

                return await Result<ChuongInfoModel>.SuccessAsync("Insert successfull.");
            }
            catch (Exception ex)
            {
                return await Result<ChuongInfoModel>.FailAsync(ex.Message);
            }
        }
    }
}
