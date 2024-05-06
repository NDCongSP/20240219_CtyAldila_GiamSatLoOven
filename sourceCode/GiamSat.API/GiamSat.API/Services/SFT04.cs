using Microsoft.AspNetCore.Http;
using RestEase;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using GiamSat.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Stl.Async;

namespace GiamSat.API
{
    public class SFT04 : ISFT04
    {
        readonly ApplicationDbContext _context;

        readonly IHttpContextAccessor _contextAccessor;

        public SFT04(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;

            _context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<FT04>>> GetAll()
        {
            try
            {
                return await Result<List<FT04>>.SuccessAsync(_context.FT04.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT04>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT04>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT04.FindAsync(id);
                return await Result<FT04>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT04>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<FT04>>> GetFilter([Body] FilterModel model)
        {
            try
            {
                if (model.GetAll)
                {
                    var d = _context.FT04
                        .Where<FT04>(x => x.CreatedDate >= model.FromDate && x.CreatedDate <= model.ToDate).ToList();
                    return await Result<List<FT04>>.SuccessAsync(d);
                }
                else
                {
                    //using (var con=GlobalVariable.GetDbProvider())
                    //{
                    //    var p = new DynamicParameters();
                    //    p.Add("ovenId",model.OvenId);
                    //    p.Add("fromDate",model.FromDate);
                    //    p.Add("toDate",model.ToDate);

                    //    var d= con.Query<FT04>("[sp_FT04GetAllByDate]", param: p, commandType: CommandType.StoredProcedure).ToList();
                    //    return await Result<List<FT04>>.SuccessAsync(d);
                    //}

                    var d = _context.FT04
                      .Where<FT04>(x => x.OvenId == model.OvenId && x.CreatedDate >= model.FromDate && x.CreatedDate <= model.ToDate).ToList();
                    return await Result<List<FT04>>.SuccessAsync(d);
                }
            }
            catch (Exception ex)
            {
                return await Result<List<FT04>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT04>> Insert([Body] FT04 model)
        {
            try
            {
                await _context.FT04.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT04>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT04>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT04>> Update([Body] FT04 model)
        {
            try
            {
                _context.FT04.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT04>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT04>.FailAsync(ex.Message);
            }
        }
    }
}
