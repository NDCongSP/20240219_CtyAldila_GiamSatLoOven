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

        public async Task<Result<List<RevoReportStepVm>>> GetReportStepView(RevoFilterModel model)
        {
            try
            {
                var p = ResolveRevoReportRange(model);
                var list = await _context.Set<RevoReportStepVm>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.fn_RevoReport_Step({0}, {1}, {2})",
                        p.from, p.toExclusive, p.revoId)
                    .AsNoTracking()
                    .ToListAsync();
                return await Result<List<RevoReportStepVm>>.SuccessAsync(list);
            }
            catch (Exception ex)
            {
                return await Result<List<RevoReportStepVm>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<RevoReportShaftVm>>> GetReportShaftView(RevoFilterModel model)
        {
            try
            {
                var p = ResolveRevoReportRange(model);
                var list = await _context.Set<RevoReportShaftVm>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.fn_RevoReport_Shaft({0}, {1}, {2})",
                        p.from, p.toExclusive, p.revoId)
                    .AsNoTracking()
                    .ToListAsync();
                return await Result<List<RevoReportShaftVm>>.SuccessAsync(list);
            }
            catch (Exception ex)
            {
                return await Result<List<RevoReportShaftVm>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<RevoReportHourVm>>> GetReportHourView(RevoFilterModel model)
        {
            try
            {
                var p = ResolveRevoReportRange(model);
                var list = await _context.Set<RevoReportHourVm>()
                    .FromSqlRaw(
                        "SELECT * FROM dbo.fn_RevoReport_Hour({0}, {1}, {2})",
                        p.from, p.toExclusive, p.revoId)
                    .AsNoTracking()
                    .ToListAsync();
                return await Result<List<RevoReportHourVm>>.SuccessAsync(list);
            }
            catch (Exception ex)
            {
                return await Result<List<RevoReportHourVm>>.FailAsync(ex.Message);
            }
        }

        public async Task<Result<List<FT09_RevoDatalog>>> GetFilter([Body] RevoFilterModel model)
        {
            try
            {
                var query = _context.FT09_RevoDatalogs.AsQueryable();

                // Filter by date range based on StartedAt and EndedAt
                if (model.FromDate != null && model.ToDate != null)
                {
                    query = query.Where(x => 
                        (x.StartedAt.HasValue && x.StartedAt.Value >= model.FromDate && x.StartedAt.Value <= model.ToDate) ||
                        (x.EndedAt.HasValue && x.EndedAt.Value >= model.FromDate && x.EndedAt.Value <= model.ToDate) ||
                        (x.StartedAt.HasValue && x.EndedAt.HasValue && 
                         x.StartedAt.Value <= model.ToDate && x.EndedAt.Value >= model.FromDate)
                    );
                }

                // Filter by RevoId if specified
                if (!model.GetAll && model.RevoId.HasValue)
                {
                    query = query.Where(x => x.RevoId == model.RevoId);
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
