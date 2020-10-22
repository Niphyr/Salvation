﻿using Salvation.Core.Constants;
using Salvation.Core.Constants.Data;
using Salvation.Core.Interfaces;
using Salvation.Core.Interfaces.Modelling.HolyPriest.Spells;
using Salvation.Core.Interfaces.State;
using Salvation.Core.State;

namespace Salvation.Core.Modelling.HolyPriest.Spells
{
    public class DivineHymn : SpellService, IDivineHymnSpellService
    {
        public DivineHymn(IGameStateService gameStateService)
            : base(gameStateService)
        {
            SpellId = (int)Spell.DivineHymn;
        }

        public override double GetAverageRawHealing(GameState gameState, BaseSpellData spellData = null)
        {
            if (spellData == null)
                spellData = _gameStateService.GetSpellData(gameState, Spell.DivineHymn);

            var holyPriestAuraHealingBonus = _gameStateService.GetSpellData(gameState, Spell.HolyPriest)
                .GetEffect(179715).BaseValue / 100 + 1;

            var divineHymnAura = spellData.GetEffect(59162).TriggerSpell.GetEffect(59166).BaseValue;

            // DH's average heal for the first tick is:
            // SP% * Intellect * Vers * Hpriest Aura
            var healingSp = spellData.GetEffect(59162).TriggerSpell.GetEffect(59165).SpCoefficient;

            double firstTickRaid = healingSp
                * _gameStateService.GetIntellect(gameState)
                * _gameStateService.GetVersatilityMultiplier(gameState)
                * holyPriestAuraHealingBonus;

            // double it if we have 5 or less (dungeon group buff)
            double firstTickParty = firstTickRaid * 2;

            _gameStateService.JournalEntry(gameState, $"[{spellData.Name}] Tooltip: {firstTickRaid:0.##} & {firstTickParty:0.##} (first tick)");
            _gameStateService.JournalEntry(gameState, $"[{spellData.Name}] Tooltip: {firstTickRaid * 5:0.##} & {firstTickParty * 5:0.##} (all ticks)");

            // Pick whether we're in part or raid
            double firstTick = GetNumberOfHealingTargets(gameState, spellData) <= 5 ? firstTickParty : firstTickRaid;

            firstTick *= _gameStateService.GetCriticalStrikeMultiplier(gameState);

            // Now the rest of the 4 ticks including the aura:
            double averageHeal = firstTick + (firstTick * 4 * (1 + divineHymnAura));

            return averageHeal * GetNumberOfHealingTargets(gameState, spellData);
        }

        public override double GetMaximumCastsPerMinute(GameState gameState, BaseSpellData spellData = null)
        {
            if (spellData == null)
                spellData = _gameStateService.GetSpellData(gameState, Spell.DivineHymn);

            var hastedCooldown = GetHastedCooldown(gameState, spellData);
            var fightLength = gameState.Profile.FightLengthSeconds;

            // DH is simply 60 / CD + 1 / (FightLength / 60)
            // Number of casts per minute plus one cast at the start of the encounter
            double maximumPotentialCasts = 60d / hastedCooldown
                + 1d / (fightLength / 60d);

            return maximumPotentialCasts;
        }

        public override double GetMaximumHealTargets(GameState gameState, BaseSpellData spellData)
        {
            // TODO: Clamp to raid size?
            return double.MaxValue;
        }
    }
}
