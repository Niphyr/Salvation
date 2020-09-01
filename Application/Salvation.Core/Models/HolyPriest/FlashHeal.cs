﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Salvation.Core.Models.HolyPriest
{
    class FlashHeal 
        : BaseHolyPriestHealingSpell
    {
        public override decimal AverageRawDirectHeal { get => calcAverageRawDirectHeal(); }

        public FlashHeal(HolyPriestModel holyPriestModel, decimal numberOfTargetsHit = 0)
            : base (holyPriestModel, numberOfTargetsHit)
        {
            SpellData = model.GetSpellById((int)HolyPriestModel.SpellIds.FlashHeal);
        }

        private decimal calcAverageRawDirectHeal()
        {
            // Flash Heal's average heal is:
            // SP% * Intellect * Vers * Hpriest Aura
            decimal retVal = SpellData.Coeff1 * model.RawInt * model.GetVersMultiplier(model.RawVers) * NumberOfTargets;

            return retVal;
        }
    }
}
