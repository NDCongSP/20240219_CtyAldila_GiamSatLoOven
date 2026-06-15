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
    public class SFT09 : ISFT09
    {
        readonly ApplicationDbContext _context;
        readonly IHttpContextAccessor _contextAccessor;

        public SFT09(ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;

            _context.Database.SetCommandTimeout((int)TimeSpan.FromMinutes(60).TotalMilliseconds);
        }

        public async Task<Result<List<FT09_RevoDatalog>>> GetAll()
        {
            try
            {
                return await Result<List<FT09_RevoDatalog>>.SuccessAsync(_context.FT09_RevoDatalogs.ToList());
            }
            catch (Exception ex)
            {
                return await Result<List<FT09_RevoDatalog>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT09_RevoDatalog>> GetById([Path] Guid id)
        {
            try
            {
                var res = await _context.FT09_RevoDatalogs.FindAsync(id);
                return await Result<FT09_RevoDatalog>.SuccessAsync(res);
            }
            catch (Exception ex)
            {
                return await Result<FT09_RevoDatalog>.FailAsync(ex.Message);
            }
        }

        static (DateTime from, DateTime toExclusive, int? revoId) ResolveRevoReportRange(RevoFilterModel model)
        {
            var toExclusive = model.ToDate.AddTicks(1);
            var revoId = (!model.GetAll && model.RevoId.HasValue) ? model.RevoId : null;
            return (model.FromDate, toExclusive, revoId);
        }

        /// <summary>Chuẩn hóa tham số TVF: chỉ "total" hoặc "finished".</summary>
        static string NormalizeShaftScope(string? scope)
        {
            var s = (scope ?? "total").Trim().ToLowerInvariant();
            return s == "finished" ? "finished" : "total";
        }

        public async Task<PagedResult<RevoReportStepVm>> GetReportStepView(RevoFilterModel model)
        {
            try
            {
                var p = ResolveRevoReportRange(model);
                var shaftScope = NormalizeShaftScope(model.ShaftScope);
                var query = _context.Set<RevoReportStepVm>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.fn_RevoReport_Step({0}, {1}, {2}, {3})",
                        p.from, p.toExclusive, p.revoId, shaftScope)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();
                
                // Sorting logic (can be extended using System.Linq.Dynamic.Core later)
                query = query.OrderByDescending(x => x.Hour).ThenBy(x => x.ShaftNo).ThenBy(x => x.StepId);

                var take = model.Take == -1 ? int.MaxValue : model.Take;
                var list = await query
                    .Skip(model.Skip)
                    .Take(take)
                    .ToListAsync();
                    
                return PagedResult<RevoReportStepVm>.Success(list, totalRecords);
            }
            catch (Exception ex)
            {
                var res = new PagedResult<RevoReportStepVm>();
                res.Succeeded = false;
                res.Messages.Add(ex.Message);
                return res;
            }
        }

        public async Task<PagedResult<RevoReportShaftVm>> GetReportShaftView(RevoFilterModel model)
        {
            try
            {
                var p = ResolveRevoReportRange(model);
                var shaftScope = NormalizeShaftScope(model.ShaftScope);
                var query = _context.Set<RevoReportShaftVm>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.fn_RevoReport_Shaft({0}, {1}, {2}, {3})",
                        p.from, p.toExclusive, p.revoId, shaftScope)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                query = query.OrderByDescending(x => x.Hour).ThenBy(x => x.ShaftNo);

                var take = model.Take == -1 ? int.MaxValue : model.Take;
                var list = await query
                    .Skip(model.Skip)
                    .Take(take)
                    .ToListAsync();
                    
                return PagedResult<RevoReportShaftVm>.Success(list, totalRecords);
            }
            catch (Exception ex)
            {
                var res = new PagedResult<RevoReportShaftVm>();
                res.Succeeded = false;
                res.Messages.Add(ex.Message);
                return res;
            }
        }

        public async Task<PagedResult<RevoReportHourVm>> GetReportHourView(RevoFilterModel model)
        {
            try
            {
                var p = ResolveRevoReportRange(model);
                var shaftScope = NormalizeShaftScope(model.ShaftScope);
                var query = _context.Set<RevoReportHourVm>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.fn_RevoReport_Hour({0}, {1}, {2}, {3})",
                        p.from, p.toExclusive, p.revoId, shaftScope)
                    .AsNoTracking();

                var totalRecords = await query.CountAsync();

                query = query.OrderByDescending(x => x.Hour);

                var take = model.Take == -1 ? int.MaxValue : model.Take;
                var list = await query
                    .Skip(model.Skip)
                    .Take(take)
                    .ToListAsync();

                return PagedResult<RevoReportHourVm>.Success(list, totalRecords);
            }
            catch (Exception ex)
            {
                var res = new PagedResult<RevoReportHourVm>();
                res.Succeeded = false;
                res.Messages.Add(ex.Message);
                return res;
            }
        }

        public async Task<Result<List<FT09_RevoDatalog>>> GetFilter([Body] RevoFilterModel model)
        {
            try
            {
                var p = ResolveRevoReportRange(model);
                var shaftScope = NormalizeShaftScope(model.ShaftScope);

                // Đồng bộ logic với các TVF báo cáo:
                // - thời gian theo Started = COALESCE(StartedAt, CreatedAt)
                // - finished: chỉ giữ shaft mà mọi dòng đều có TotalTime > 0
                var query = _context.FT09_RevoDatalogs
                    .AsQueryable()
                    .Where(x => (x.StartedAt ?? x.CreatedAt).HasValue)
                    .Where(x => (x.StartedAt ?? x.CreatedAt) >= p.from && (x.StartedAt ?? x.CreatedAt) < p.toExclusive);

                if (p.revoId.HasValue)
                {
                    query = query.Where(x => x.RevoId == p.revoId.Value);
                }

                if (shaftScope == "finished")
                {
                    var finishedShafts = _context.FT09_RevoDatalogs
                        .Where(x => (x.StartedAt ?? x.CreatedAt).HasValue)
                        .Where(x => (x.StartedAt ?? x.CreatedAt) >= p.from && (x.StartedAt ?? x.CreatedAt) < p.toExclusive)
                        .Where(x => !p.revoId.HasValue || x.RevoId == p.revoId.Value)
                        .Where(x => x.ShaftNum.HasValue)
                        .GroupBy(x => x.ShaftNum!.Value)
                        .Where(g => g.Count() > 0 && g.Count(r => (r.TotalTime ?? 0) > 0) == g.Count())
                        .Select(g => g.Key);

                    query = query.Where(x => !x.ShaftNum.HasValue || finishedShafts.Contains(x.ShaftNum.Value));
                }

                var result = await query
                    .OrderBy(x => x.ShaftNum ?? Guid.Empty)
                    .ThenBy(x => x.StepId ?? 0)
                    .ThenBy(x => x.StartedAt ?? x.CreatedAt ?? DateTime.MinValue)
                    .ToListAsync();
                return await Result<List<FT09_RevoDatalog>>.SuccessAsync(result);
            }
            catch (Exception ex)
            {
                return await Result<List<FT09_RevoDatalog>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT09_RevoDatalog>> Insert([Body] FT09_RevoDatalog model)
        {
            try
            {
                if (model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }
                if (!model.CreatedAt.HasValue)
                {
                    model.CreatedAt = DateTime.Now;
                }
                await _context.FT09_RevoDatalogs.AddAsync(model);
                await _context.SaveChangesAsync();
                return await Result<FT09_RevoDatalog>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT09_RevoDatalog>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<FT09_RevoDatalog>> Update([Body] FT09_RevoDatalog model)
        {
            try
            {
                _context.FT09_RevoDatalogs.Update(model);
                await _context.SaveChangesAsync();
                return await Result<FT09_RevoDatalog>.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                return await Result<FT09_RevoDatalog>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<RevoGetTotalShaftCountDto>>> GetTotalShaft([Query] int? revoId = null)
        {
            try
            {
                List<RevoGetTotalShaftCountDto> list;
                if (revoId.HasValue)
                {
                    list = await _context.RevoTotalShaftCounts
                        .FromSqlRaw("EXEC sp_GetTotalShaft @RevoId = {0}", revoId.Value)
                        .AsNoTracking()
                        .ToListAsync();
                }
                else
                {
                    list = await _context.RevoTotalShaftCounts
                        .FromSqlRaw("EXEC sp_GetTotalShaft")
                        .AsNoTracking()
                        .ToListAsync();
                }
                return await Result<List<RevoGetTotalShaftCountDto>>.SuccessAsync(list);
            }
            catch (Exception ex)
            {
                return await Result<List<RevoGetTotalShaftCountDto>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<bool>> Delete([Path] Guid id)
        {
            try
            {
                var entity = await _context.FT09_RevoDatalogs.FindAsync(id);
                if (entity != null)
                {
                    _context.FT09_RevoDatalogs.Remove(entity);
                    await _context.SaveChangesAsync();
                    return await Result<bool>.SuccessAsync(true);
                }
                return await Result<bool>.FailAsync("Entity not found");
            }
            catch (Exception ex)
            {
                return await Result<bool>.FailAsync(ex.Message);
            }
        }
    }
}
