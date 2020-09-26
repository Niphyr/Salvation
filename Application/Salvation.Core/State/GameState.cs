﻿using Salvation.Core.Constants;
using Salvation.Core.Profile;

namespace Salvation.Core.State
{
    public class GameState
    {
        public PlayerProfile Profile { get; set; }
        public GlobalConstants Constants { get; set; }

        public GameState()
        {

        }

        public GameState(PlayerProfile profile, GlobalConstants constants)
        {
            Profile = profile;
            Constants = constants;
        }
    }
}