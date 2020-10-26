﻿using Salvation.Core.Constants;
using Salvation.Core.Constants.Data;
using Salvation.Core.Interfaces;
using Salvation.Core.Interfaces.Modelling.HolyPriest.Spells;
using Salvation.Core.Interfaces.State;
using Salvation.Core.State;
using System;


namespace Salvation.Core.Modelling.HolyPriest.Spells
{
    public class Chastise : SpellService, IChastiseSpellService
    {
        private readonly ISmiteSpellService _smiteSpellService;
        private readonly IHolyFireSpellService _holyFireSpellService;

        public Chastise(IGameStateService gameStateService,
            ISmiteSpellService smiteSpellService,
            IHolyFireSpellService holyFireSpellService)
            : base(gameStateService)
        {
            SpellId = (int)Spell.Chastise;
            _smiteSpellService = smiteSpellService;
            _holyFireSpellService = holyFireSpellService;
        }

        public override double GetAverageDamage(GameState gameState, BaseSpellData spellData = null)
        {
            if (spellData == null)
                spellData = _gameStateService.GetSpellData(gameState, Spell.Chastise);

            var holyPriestAuraDamageBonus = _gameStateService.GetSpellData(gameState, Spell.HolyPriest)
                .GetEffect(191077).BaseValue / 100 + 1;

            var damageSp = spellData.GetEffect(91044).SpCoefficient;

            double averageDmg = damageSp
                * _gameStateService.GetIntellect(gameState)
                * _gameStateService.GetVersatilityMultiplier(gameState)
                * holyPriestAuraDamageBonus;

            _gameStateService.JournalEntry(gameState, $"[{spellData.Name}] Tooltip: {averageDmg:0.##}");

            averageDmg *= _gameStateService.GetCriticalStrikeMultiplier(gameState);

            return averageDmg * GetNumberOfDamageTargets(gameState, spellData);
        }

        public override double GetMaximumCastsPerMinute(GameState gameState, BaseSpellData spellData = null)
        {
            if (spellData == null)
                spellData = _gameStateService.GetSpellData(gameState, Spell.HolyWordSerenity);

            // Max casts per minute is (60 + Smite * HwCDR) / CD + 1 / (FightLength / 60)
            // HWCDR is 6 base, more with LOTN/other effects
            // 1 from regular CD + reductions from fillers divided by the cooldown to get base CPM
            // Then add the one charge we start with, 1 per fight, into seconds.

            var cpmSmite = _smiteSpellService.GetActualCastsPerMinute(gameState);

            var hastedCD = GetHastedCooldown(gameState, spellData);
            var fightLength = gameState.Profile.FightLengthSeconds;

            var hwCDRSmite= _gameStateService.GetTotalHolyWordCooldownReduction(gameState, Spell.Smite);

            double hwCDR = cpmSmite * hwCDRSmite;

            if (_gameStateService.IsLegendaryActive(gameState, Spell.HarmoniousApparatus))
            {
                var cpmHolyFire = _holyFireSpellService.GetActualCastsPerMinute(gameState);
                var hwCDRPoM = _gameStateService.GetTotalHolyWordCooldownReduction(gameState, Spell.HolyFire);
                hwCDR += cpmHolyFire * hwCDRPoM;
            }

            double maximumPotentialCasts = (60d + hwCDR) / hastedCD
                + 1d / (fightLength / 60d);

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

        public override double GetHastedCooldown(GameState gameState, BaseSpellData spellData = null)
        {
            if (spellData == null)
                spellData = _gameStateService.GetSpellData(gameState, (Spell)SpellId);

            // Cooldown for Chastise is stored in the chargecooldown instead as it has charges
            var cooldown = spellData.ChargeCooldown / 1000;

            return spellData.IsCooldownHasted
                ? cooldown / _gameStateService.GetHasteMultiplier(gameState)
                : cooldown;
        }
    }
}
