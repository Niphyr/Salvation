﻿using Salvation.Core.Models.Common;
using Salvation.Core.Profile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Salvation.Core.Models
{
    public class BaseModelResults
    {
        public List<SpellCastResult> SpellCastResults;

        public BaseProfile Profile { get; set; }

        public decimal TotalActualHPS { get; set; }
        public decimal TotalRawHPS { get; set; }
        public decimal TotalMPS { get; set; }
        public decimal TotalRawHPM { get; set; }
        public decimal TotalActualHPM { get; set; }
        public decimal TimeToOom { get; set; }
        public AveragedSpellCastResult OverallResults { get; set; }

        public BaseModelResults()
        {
            SpellCastResults = new List<SpellCastResult>();
        }
    }
}
