﻿using RestEase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [BasePath(ApiRoutes.FT04.BasePath)]
    public interface ISFT04 : IRepository<Guid, FT04>
    {
        [Post(ApiRoutes.FT04.GetFilter)]
        Task<Result<List<FT04>>> GetFilter([Body] FilterModel model);
    }
}
