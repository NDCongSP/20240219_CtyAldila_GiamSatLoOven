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
    public class SAlarmSettings : ISAlarmSettings
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SAlarmSettings(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public async Task<Result<List<AlarmSettingsModel>>> GetAll()
        {
            try
            {
                var res = _context.AlarmSettingsModel.ToList();

                foreach (var item in res)
                {
                    item.CredentialPassword = EncodeMD5.DecryptString(item.CredentialPassword, "PTAut0m@t!0n30!)@)20");
                }

                return await Result<List<AlarmSettingsModel>>.SuccessAsync(_context.AlarmSettingsModel.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<AlarmSettingsModel>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<AlarmSettingsModel>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.AlarmSettingsModel.FindAsync(id);
                return await Result<AlarmSettingsModel>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<AlarmSettingsModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<AlarmSettingsModel>> Insert([Body] AlarmSettingsModel model)
        {

            try
            {
                model.CredentialPassword = EncodeMD5.EncryptString(model.CredentialPassword, "PTAut0m@t!0n30!)@)20");                

                await _context.AlarmSettingsModel.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<AlarmSettingsModel>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<AlarmSettingsModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<AlarmSettingsModel>> Update([Body] AlarmSettingsModel model)
        {
            try
            {
                model.CredentialPassword = EncodeMD5.EncryptString(model.CredentialPassword, "PTAut0m@t!0n30!)@)20");                

                _context.AlarmSettingsModel.Update(model);
                await _context.SaveChangesAsync();

                return await Result<AlarmSettingsModel>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<AlarmSettingsModel>.FailAsync(ex.Message);
            }
        }
    }
}
