﻿using Salvation.Core.Constants;
using Salvation.Core.Constants.Data;
using Salvation.Core.Interfaces;
using Salvation.Core.Interfaces.Modelling.HolyPriest.Spells;
using Salvation.Core.Interfaces.State;
using Salvation.Core.State;

namespace Salvation.Core.Modelling.HolyPriest.Spells
{
    public class ShadowWordPain : SpellService, IShadowWordPainSpellService
    {
        public ShadowWordPain(IGameStateService gameStateService)
            : base(gameStateService)
        {
            SpellId = (int)Spell.ShadowWordPain;
        }

        public override double GetAverageDamage(GameState gameState, BaseSpellData spellData = null)
        {
            if (spellData == null)
                spellData = _gameStateService.GetSpellData(gameState, Spell.ShadowWordPain);

            BaseSpellData spellDataRank2 = _gameStateService.GetSpellData(gameState, Spell.ShadowWordPainRank2);
            var holyPriestAuraDamagesBonus = _gameStateService.GetSpellData(gameState, Spell.HolyPriest)
                .GetEffect(191077).BaseValue / 100 + 1;

            var holyPriestAuraDamagePeriodicBonus = _gameStateService.GetSpellData(gameState, Spell.HolyPriest)
                .GetEffect(191078).BaseValue / 100 + 1;

            var damageSpInstant = spellData.GetEffect(242).SpCoefficient;
            var damageSpPeriodic = spellData.GetEffect(254257).SpCoefficient;

            var painDirectBonus = _gameStateService.GetSpellData(gameState, Spell.HolyPriest)
                .GetEffect(871790).BaseValue / 100 + 1;
            var painPeriodicBonus = _gameStateService.GetSpellData(gameState, Spell.HolyPriest)
                .GetEffect(871791).BaseValue / 100 + 1;

            // Pains's average dmg is initial + DoT portion:
            double averageDamageFirstTick = damageSpInstant
                * _gameStateService.GetIntellect(gameState)
                * _gameStateService.GetVersatilityMultiplier(gameState)
                * holyPriestAuraDamagesBonus
                * painDirectBonus;

            _gameStateService.JournalEntry(gameState, $"[{spellData.Name}] Tooltip: {averageDamageFirstTick:0.##} (first)");
            averageDamageFirstTick *= _gameStateService.GetCriticalStrikeMultiplier(gameState) * _gameStateService.GetHasteMultiplier(gameState);

            double painDuration = spellData.Duration / 1000;
            double rank2Addition = spellDataRank2.GetEffect(819469).BaseValue / 1000;
            double tickrate = spellData.GetEffect(254257).Amplitude / 1000;

            // DoT is affected by haste
            double averageDmgTicks = damageSpPeriodic
                * _gameStateService.GetIntellect(gameState)
                * _gameStateService.GetVersatilityMultiplier(gameState)
                * _gameStateService.GetHasteMultiplier(gameState)
                * holyPriestAuraDamagePeriodicBonus
                * painPeriodicBonus
                * (painDuration + rank2Addition) / tickrate;

            _gameStateService.JournalEntry(gameState, $"[{spellData.Name}] Tooltip: {averageDmgTicks:0.##} (ticks)");

            averageDmgTicks *= _gameStateService.GetCriticalStrikeMultiplier(gameState);

            return (averageDamageFirstTick + averageDmgTicks) * GetNumberOfDamageTargets(gameState, spellData);
        }

        public override double GetMaximumCastsPerMinute(GameState gameState, BaseSpellData spellData = null)
        {
            if (spellData == null)
                spellData = _gameStateService.GetSpellData(gameState, Spell.ShadowWordPain);

            var hastedCastTime = GetHastedCastTime(gameState, spellData);
            var hastedGcd = GetHastedGcd(gameState, spellData);
            var hastedCd = GetHastedCooldown(gameState, spellData);

            // A fix to the spell being modified to have no cast time and no gcd and no CD
            // This can happen if it's a component in another spell
            if (hastedCastTime == 0 && hastedGcd == 0 && hastedCd == 0)
                return 0;

            double fillerCastTime = hastedCastTime == 0d
                ? hastedGcd
                : hastedCastTime;

            double maximumPotentialCasts = 60d / fillerCastTime;

            return maximumPotentialCasts;
        }

        public override double GetMinimumDamageTargets(GameState gameState, BaseSpellData spellData)
        {
            return 1;
        }

        public override double GetMaximumDamageTargets(GameState gameState, BaseSpellData spellData)
        {
            return 1;
        }
    }
}
