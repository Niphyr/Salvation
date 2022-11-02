﻿using Salvation.Core.Constants;
using Salvation.Core.Constants.Data;
using Salvation.Core.Interfaces.Modelling;
using Salvation.Core.Interfaces.Modelling.HolyPriest.Spells;
using Salvation.Core.Interfaces.State;
using Salvation.Core.Modelling.Common;
using Salvation.Core.State;
using System;
using System.Linq;

namespace Salvation.Core.Modelling.HolyPriest.Spells
{
    public class FlashHeal : SpellService, ISpellService<IFlashHealSpellService>
    {
        private readonly ISpellService<ITrailOfLightSpellService> _trailOfLightSpellService;
        private readonly ISpellService<IBindingHealsSpellService> _bindingHealsSpellService;

        public FlashHeal(IGameStateService gameStateService,
            ISpellService<ITrailOfLightSpellService> trailOfLightSpellService,
            ISpellService<IBindingHealsSpellService> bindingHealsSpellService)
            : base(gameStateService)
        {
            Spell = Spell.FlashHeal;
            _trailOfLightSpellService = trailOfLightSpellService;
            _bindingHealsSpellService = bindingHealsSpellService;
        }

        public override double GetAverageRawHealing(GameState gameState, BaseSpellData spellData = null)
        {
            spellData = ValidateSpellData(gameState, spellData);

            var holyPriestAuraHealingBonus = _gameStateService.GetSpellData(gameState, Spell.HolyPriest)
                .GetEffect(179715).BaseValue / 100 + 1;

            var holyPriestAuraFlashHealReduction = _gameStateService.GetSpellData(gameState, Spell.HolyPriest)
                .GetEffect(1040388).BaseValue / 100 + 1;

            // Flash Heal's average heal is:
            // SP% * Intellect * Vers * Hpriest Aura
            var healingSp = spellData.GetEffect(613).SpCoefficient;

            double averageHeal = healingSp
                * _gameStateService.GetIntellect(gameState)
                * _gameStateService.GetVersatilityMultiplier(gameState)
                * holyPriestAuraHealingBonus
                * holyPriestAuraFlashHealReduction;

            _gameStateService.JournalEntry(gameState, $"[{spellData.Name}] Tooltip: {averageHeal:0.##}");

            averageHeal *= (_gameStateService.GetCriticalStrikeMultiplier(gameState) + GetCrisisManagementModifier(gameState))
                * _gameStateService.GetGlobalHealingMultiplier(gameState);

            averageHeal *= GetImprovedFlashHealMultiplier(gameState);

            averageHeal *= GetResonantWordsMulti(gameState, spellData);

            return averageHeal * GetNumberOfHealingTargets(gameState, spellData);
        }

        public override double GetMaximumCastsPerMinute(GameState gameState, BaseSpellData spellData = null)
        {
            spellData = ValidateSpellData(gameState, spellData);

            var hastedCastTime = GetHastedCastTime(gameState, spellData);
            var hastedGcd = GetHastedGcd(gameState, spellData);

            double fillerCastTime = hastedCastTime == 0
                ? hastedGcd
                : hastedCastTime;

            double maximumPotentialCasts = 60d / fillerCastTime;

            return maximumPotentialCasts;
        }

        public override double GetMinimumHealTargets(GameState gameState, BaseSpellData spellData)
        {
            return 1;
        }

        public override double GetMaximumHealTargets(GameState gameState, BaseSpellData spellData)
        {
            return 1;
        }

        public override AveragedSpellCastResult GetCastResults(GameState gameState, BaseSpellData spellData = null)
        {
            spellData = ValidateSpellData(gameState, spellData);

            AveragedSpellCastResult result = base.GetCastResults(gameState, spellData);

            // TODO: Move this somewhere more central rather than Copy/Paste with Heal/FH
            // Calculate ToL if talented.
            if (_gameStateService.GetTalent(gameState, Spell.TrailOfLight).Rank > 0)
            {
                var trailOfLightSpellData = _gameStateService.GetSpellData(gameState, Spell.TrailOfLight);

                trailOfLightSpellData.Overrides[Override.NumberOfHealingTargets] = GetNumberOfHealingTargets(gameState, spellData);
                trailOfLightSpellData.Overrides[Override.CastsPerMinute] = GetActualCastsPerMinute(gameState, spellData);
                trailOfLightSpellData.Overrides[Override.ResultMultiplier] = GetAverageRawHealing(gameState, spellData);

                // grab the result of the spell cast
                var trailOfLightResult = _trailOfLightSpellService.GetCastResults(gameState, trailOfLightSpellData);

                result.AdditionalCasts.Add(trailOfLightResult);
            }

            // TODO: Move this somewhere more central rather than Copy/Paste with Heal/FH
            // Calculate BHs if talented.
            if (_gameStateService.GetTalent(gameState, Spell.BindingHeals).Rank > 0)
            {
                var bindingHealsSpellData = _gameStateService.GetSpellData(gameState, Spell.BindingHeals);

                bindingHealsSpellData.Overrides[Override.NumberOfHealingTargets] = GetNumberOfHealingTargets(gameState, spellData);
                bindingHealsSpellData.Overrides[Override.CastsPerMinute] = GetActualCastsPerMinute(gameState, spellData);
                bindingHealsSpellData.Overrides[Override.ResultMultiplier] = GetAverageRawHealing(gameState, spellData);

                // grab the result of the spell cast
                var bindingHealsResult = _bindingHealsSpellService.GetCastResults(gameState, bindingHealsSpellData);

                result.AdditionalCasts.Add(bindingHealsResult);
            }

            return result;
        }

        public override double GetHastedCastTime(GameState gameState, BaseSpellData spellData = null)
        {
            var hastedCT = base.GetHastedCastTime(gameState, spellData);

            hastedCT *= this.GetUnwaveringWillMultiplier(gameState);

            return hastedCT;
        }

        internal double GetImprovedFlashHealMultiplier(GameState gameState)
        {
            var multi = 1d;

            var talent = _gameStateService.GetTalent(gameState, Spell.ImprovedFlashHeal);

            if (talent != null && talent.Rank > 0)
            {
                var talentSpellData = _gameStateService.GetSpellData(gameState, Spell.ImprovedFlashHeal);

                multi += talentSpellData.GetEffect(1033063).BaseValue / 100;
            }

            return multi;
        }

        internal double GetCrisisManagementModifier(GameState gameState)
        {
            // TODO: Move this somewhere more neutral rather than copy/paste with FH/Heal.
            var modifier = 0d;

            var talent = _gameStateService.GetTalent(gameState, Spell.CrisisManagement);

            if (talent != null && talent.Rank > 0)
            {
                var talentSpellData = _gameStateService.GetSpellData(gameState, Spell.CrisisManagement);

                modifier += talentSpellData.GetEffect(1028125).BaseValue / 100 * talent.Rank;
            }

            return modifier;
        }

        internal double GetResonantWordsMulti(GameState gameState, BaseSpellData spellData)
        {
            spellData = ValidateSpellData(gameState, spellData);
            // TODO: Move this to its own location rather than copy/pasted in Heal & FH
            var multi = 1d;

            var talent = _gameStateService.GetTalent(gameState, Spell.ResonantWords);

            if (talent != null && talent.Rank > 0)
            {
                // Injecting these spells directly causes a circular dependency error. 
                var serenity = _gameStateService.GetRegisteredSpells(gameState).Where(s => s.Spell == Spell.HolyWordSerenity).FirstOrDefault();
                var sanc = _gameStateService.GetRegisteredSpells(gameState).Where(s => s.Spell == Spell.HolyWordSanctify).FirstOrDefault();
                var chastise = _gameStateService.GetRegisteredSpells(gameState).Where(s => s.Spell == Spell.HolyWordChastise).FirstOrDefault();

                var hwCasts = serenity.SpellService.GetActualCastsPerMinute(gameState, serenity.SpellData)
                    + sanc.SpellService.GetActualCastsPerMinute(gameState, sanc.SpellData)
                    + chastise.SpellService.GetActualCastsPerMinute(gameState, chastise.SpellData);

                var numberBuffsUsed = _gameStateService.GetPlaystyle(gameState, "ResonantWordsPercentageBuffsUsed");

                if (numberBuffsUsed == null)
                    throw new ArgumentOutOfRangeException("ResonantWordsPercentageBuffsUsed", $"ResonantWordsPercentageBuffsUsed needs to be set.");

                var percentageBuffsForHeal = _gameStateService.GetPlaystyle(gameState, "ResonantWordsPercentageBuffsHeal");

                if (percentageBuffsForHeal == null)
                    throw new ArgumentOutOfRangeException("ResonantWordsPercentageBuffsHeal", $"ResonantWordsPercentageBuffsHeal needs to be set.");

                var talentSpellData = _gameStateService.GetSpellData(gameState, Spell.ResonantWords);

                var numBuffedSpellsTotal = hwCasts * numberBuffsUsed.Value;

                // Max number of Flash Heal spells that can be buffed: hwCasts * (1 - Heal_percent)
                // Actual number of buffed casts: lowest of either max spells cast or actual casts per minute
                var numBuffedCasts = Math.Min(numBuffedSpellsTotal * (1 - percentageBuffsForHeal.Value), GetActualCastsPerMinute(gameState, spellData));

                // This is the extra healing on all the buffed casts
                var resonantWordsModifier = talentSpellData.GetEffect(996850).BaseValue / 100 * talent.Rank;
                var extraHealing = numBuffedCasts * resonantWordsModifier;

                // Now divide this by all casts
                var increase = extraHealing / GetActualCastsPerMinute(gameState, spellData);

                multi += increase;
            }

            return multi;
        }
    }
}
