using GiamSat.Models;
using Microsoft.AspNetCore.Http;
using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SAlarmLog : ISAlarmLog
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SAlarmLog(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<List<AlarmLogModel>>> GetAll()
        {
            try
            {
                return await Result<List<AlarmLogModel>>.SuccessAsync(_context.AlarmLogModel.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<AlarmLogModel>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<AlarmLogModel>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.AlarmLogModel.FindAsync(id);
                return await Result<AlarmLogModel>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<AlarmLogModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<AlarmLogModel>> Insert([Body] AlarmLogModel model)
        {
            try
            {
                await _context.AlarmLogModel.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<AlarmLogModel>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<AlarmLogModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<AlarmLogModel>> Update([Body] AlarmLogModel model)
        {
            try
            {
                _context.AlarmLogModel.Update(model);
                await _context.SaveChangesAsync();
                return await Result<AlarmLogModel>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<AlarmLogModel>.FailAsync(ex.Message);
            }
        }
    }
}
