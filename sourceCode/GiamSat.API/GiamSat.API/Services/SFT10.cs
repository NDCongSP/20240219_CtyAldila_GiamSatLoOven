using GiamSat.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public class SFT10 : ISFT10
    {
        readonly ApplicationDbContext _context;

        public SFT10(ApplicationDbContext context)
        {
            _context = context;
            _context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<TemperatureConfigsModel>> GetConfigs()
        {
            try
            {
                var ft10 = await _context.FT10_TemperatureConfigs
                    .FirstOrDefaultAsync(f => f.Actived == true);

                if (ft10 == null || string.IsNullOrEmpty(ft10.C000))
                    return await Result<TemperatureConfigsModel>.SuccessAsync(
                        new TemperatureConfigsModel { LocationsConfig = new List<TemperatureLocationModel>() });

                var config = JsonConvert.DeserializeObject<TemperatureConfigsModel>(ft10.C000);
                return await Result<TemperatureConfigsModel>.SuccessAsync(
                    config ?? new TemperatureConfigsModel { LocationsConfig = new List<TemperatureLocationModel>() });
            }
            catch (Exception ex)
            {
                return await Result<TemperatureConfigsModel>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<bool>> SaveConfigs([Body] TemperatureConfigsModel config)
        {
            try
            {
                var ft10 = await _context.FT10_TemperatureConfigs
                    .FirstOrDefaultAsync(f => f.Actived == true);

                var jsonString = JsonConvert.SerializeObject(config);

                if (ft10 == null)
                {
                    ft10 = new FT10_TemperatureConfig
                    {
                        Id = Guid.NewGuid(),
                        C000 = jsonString,
                        Actived = true,
                        CreatedAt = DateTime.Now
                    };
                    await _context.FT10_TemperatureConfigs.AddAsync(ft10);
                }
                else
                {
                    ft10.C000 = jsonString;
                    _context.FT10_TemperatureConfigs.Update(ft10);
                }

                await _context.SaveChangesAsync();
                return await Result<bool>.SuccessAsync(true);
            }
            catch (Exception ex)
            {
                return await Result<bool>.FailAsync(ex.Message);
            }
        }
    }
}
