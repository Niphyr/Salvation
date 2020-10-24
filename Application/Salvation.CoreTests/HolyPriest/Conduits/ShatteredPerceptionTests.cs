﻿using Newtonsoft.Json;
using NUnit.Framework;
using Salvation.Core.Constants;
using Salvation.Core.Constants.Data;
using Salvation.Core.Interfaces.Constants;
using Salvation.Core.Interfaces.State;
using Salvation.Core.Modelling.HolyPriest.Spells;
using Salvation.Core.Profile;
using Salvation.Core.State;
using System.IO;
using System.Linq;

namespace Salvation.CoreTests.HolyPriest.Conduits
{
    [TestFixture]
    class ShatteredPerceptionTests
    {
        private GameState _gameState;

        [OneTimeSetUp]
        public void InitOnce()
        {
            IConstantsService constantsService = new ConstantsService();

            // Load this from somewhere that doesn't change
            var basePath = @"HolyPriest" + Path.DirectorySeparatorChar + "TestData";
            var constants = constantsService.ParseConstants(
                File.ReadAllText(Path.Combine(basePath, "SpellServiceTests_constants.json")));
            var profile = JsonConvert.DeserializeObject<PlayerProfile>(
                File.ReadAllText(Path.Combine(basePath, "SpellServiceTests_profile.json")));

            _gameState = new GameState(profile, constants);
        }

        [Test]
        public void SP_Increases_GetAverageDamage()
        {
            // Arrange
            IGameStateService gameStateService = new GameStateService();
            var spellService = new Mindgames(gameStateService);
            var gamestate1 = gameStateService.CloneGameState(_gameState);
            var gamestate2 = gameStateService.CloneGameState(_gameState);

            gamestate1.Profile.Conduits.Add(Conduit.ShatteredPerceptions, 0);

            // Act
            var resultWith = spellService.GetAverageDamage(gamestate1, null);
            var resultWithout = spellService.GetAverageDamage(gamestate2, null);

            // Assert
            Assert.AreEqual(3768.879375d, resultWithout);
            Assert.AreEqual(4258.8336937499998d, resultWith);
        }

        [Test]
        public void SP_Increases_GetDuration()
        {
            // Arrange
            IGameStateService gameStateService = new GameStateService();
            var spellService = new Mindgames(gameStateService);
            var gamestate1 = gameStateService.CloneGameState(_gameState);
            var gamestate2 = gameStateService.CloneGameState(_gameState);

            gamestate1.Profile.Conduits.Add(Conduit.ShatteredPerceptions, 0);

            // Act
            var resultWith = spellService.GetDuration(gamestate1, null);
            var resultWithout = spellService.GetDuration(gamestate2, null);

            // Assert
            Assert.AreEqual(5.0d, resultWithout);
            Assert.AreEqual(7.0d, resultWith);
        }
    }
}
