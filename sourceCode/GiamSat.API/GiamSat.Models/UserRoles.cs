using System;

namespace GiamSat.Models
{
    public static class UserRoles
    {
        /// <summary>
        /// Toàn quyền.
        /// </summary>
        public const string Admin = "Admin";
        /// <summary>
        /// Cho phép cài đặt lò Oven.
        /// </summary>
        public const string User = "User";
        /// <summary>
        /// Chỉ xem realtime và report.
        /// </summary>
        public const string Operator = "Operator";
    }
}
