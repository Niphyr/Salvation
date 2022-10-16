﻿using NUnit.Framework;
using Salvation.Core.Constants;
using Salvation.Core.Constants.Data;
using Salvation.Core.Interfaces.State;
using Salvation.Core.Modelling.HolyPriest.Spells;
using Salvation.Core.State;
using System;

namespace Salvation.CoreTests.HolyPriest.Spells
{
    [TestFixture]
    public class FocusedMendingTests : BaseTest
    {
        private GameState _gameState;
        [OneTimeSetUp]
        public void InitOnce()
        {
            _gameState = GetGameState();
        }

        [Test]
        public void FM_GetAverageRawHealing_Calculates_Rank1()
        {
            // Arrange
            IGameStateService gameStateService = new GameStateService();
            var spellService = new PrayerOfMending(gameStateService);

            // Act
            gameStateService.SetTalentRank(_gameState, Spell.FocusedMending, 0);
            var resultDefault = spellService.GetAverageRawHealing(_gameState, null);

            gameStateService.SetTalentRank(_gameState, Spell.FocusedMending, 1);
            var resultRank1 = spellService.GetAverageRawHealing(_gameState, null);

            // Assert
            Assert.AreEqual(10999.137937736617d, resultDefault);
            Assert.AreEqual(13448.945932959772d, resultRank1);
        }
    }
}
