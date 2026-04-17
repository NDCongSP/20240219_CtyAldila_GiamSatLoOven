using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GiamSat.Models
{
    public static class AppPermissions
    {
        // OVEN
        public const string Oven_Home_View = "Oven_Home.View";
        
        public const string Oven_Config_View = "Oven_Config.View";
        public const string Oven_Config_Create = "Oven_Config.Create";
        public const string Oven_Config_Edit = "Oven_Config.Edit";
        public const string Oven_Config_Delete = "Oven_Config.Delete";
        
        public const string Oven_Report_View = "Oven_Report.View";
        public const string Oven_Report_Export = "Oven_Report.Export";
        
        public const string Oven_Settings_View = "Oven_Settings.View";
        public const string Oven_Settings_Edit = "Oven_Settings.Edit";

        // REVO
        public const string Revo_Home_View = "Revo_Home.View";

        public const string Revo_Config_View = "Revo_Config.View";
        public const string Revo_Config_Create = "Revo_Config.Create";
        public const string Revo_Config_Edit = "Revo_Config.Edit";
        public const string Revo_Config_Delete = "Revo_Config.Delete";

        public const string Revo_Report_View = "Revo_Report.View";
        public const string Revo_Report_Export = "Revo_Report.Export";

        // SYSTEM
        public const string System_Config_View = "System_Config.View";
        public const string System_Config_Edit = "System_Config.Edit";

        /// <summary>
        /// Gets all defined permission codes automatically using reflection.
        /// </summary>
        public static IEnumerable<string> GetAll()
        {
            return typeof(AppPermissions)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(x => (string)x.GetRawConstantValue())
                .ToList();
        }
    }
}
