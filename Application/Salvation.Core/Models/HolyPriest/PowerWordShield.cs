﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Salvation.Core.Models.HolyPriest
{
    class PowerWordShield 
        : BaseHolyPriestHealingSpell
    {
        public PowerWordShield(HolyPriestModel holyPriestModel, decimal numberOfTargetsHit = 0)
            : base (holyPriestModel, numberOfTargetsHit)
        {
            SpellData = model.GetSpecSpellDataById((int)HolyPriestModel.SpellIds.PowerWordShield);
        }

        protected override decimal calcAverageRawDirectHeal()
        {
            decimal averageHeal = SpellData.Coeff1 
                * model.RawInt
                * model.GetVersMultiplier(model.RawVers)
                * model.GetCritMultiplier(model.RawCrit)
                * holyPriestAuraHealingBonus
                * 2; // PW:S has a x2 multiplier built in to it

            return averageHeal * NumberOfTargets;
        }

        protected override decimal calcCastsPerMinute()
        {
            decimal castsPerMinute = CastProfile.Efficiency * MaximumCastsPerMinute;

            return castsPerMinute;
        }

        protected override decimal calcMaximumCastsPerMinute()
        {
            // If it's instant cast, instead use the hasted GCD as the limiting factor
            decimal fillerCastTime = HastedCastTime == 0
                ? HastedGcd
                : HastedCastTime;

            decimal maximumPotentialCasts = 60m / fillerCastTime;

            return maximumPotentialCasts;
        }
    }
}
