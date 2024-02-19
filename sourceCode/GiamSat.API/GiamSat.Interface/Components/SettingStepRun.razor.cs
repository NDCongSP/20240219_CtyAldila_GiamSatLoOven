using GiamSat.Models;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Linq;

namespace GiamSat.Interface.Components
{
    public partial class SettingStepRun
    {
        private string stringValue { get; set; }
        private StepRun enumValue { get; set; } = StepRun.GiaiDoan1;
        public enum StepRun { GiaiDoan1, GiaiDoan2, GiaiDoan3, GiaiDoan4, GiaiDoan5, GiaiDoan6, GiaiDoan7, GiaiDoan8, GiaiDoan9, GiaiDoan10, }
        
        bool success;
        void OnValidSubmit(EditContext args)
        {

        }
        void OnInvalidSubmit(EditContext args)
        {
            
        }
    }
}