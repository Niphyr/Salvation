﻿using Salvation.Core.Constants;
using Salvation.Core.Constants.Data;
using Salvation.Core.Profile;
using Salvation.Core.State;

namespace Salvation.Core.Interfaces.State
{
    /// <summary>
    /// Goal of this is to provide a way to calculate the required fields using the player profile,
    /// constants and other state specific variables
    /// </summary>
    public interface IGameStateService
    {
        // Global constants
        BaseSpellData GetSpellData(GameState state, SpellIds spellId);
        BaseModifier GetModifier(GameState state, string modifierName);
        CastProfile GetCastProfile(GameState state, int spellId);
        ConduitData GetConduitData(GameState state, Conduit conduitId);
        Covenant GetActiveCovenant(GameState state);
        void OverrideSpellData(GameState state, BaseSpellData newData);
        void OverrideModifier(GameState state, BaseModifier newModifier);

        // Player Stats
        decimal GetIntellect(GameState state);
        decimal GetVersatilityMultiplier(GameState state);
        decimal GetCriticalStrikeMultiplier(GameState state);
        decimal GetMasteryMultiplier(GameState state);
        decimal GetHasteMultiplier(GameState state);
        decimal GetBaseManaAmount(GameState state);

        // Player Configuration
        bool IsConduitActive(GameState state, Conduit conduit);
        int GetConduitRank(GameState state, Conduit conduit);

        // Utility
        GameState CloneGameState(GameState state);
    }
}