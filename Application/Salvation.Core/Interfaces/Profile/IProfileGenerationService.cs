﻿using Salvation.Core.Constants.Data;
using Salvation.Core.Profile;

namespace Salvation.Core.Interfaces.Profile
{
    public interface IProfileGenerationService
    {
        PlayerProfile GetDefaultProfile(Spec spec);
        PlayerProfile CloneProfile(PlayerProfile profile);


        void AddConduit(PlayerProfile profile, Conduit conduit, int rank);
        void RemoveConduit(PlayerProfile profile, Conduit conduit);
        void AddTalent(PlayerProfile profile, Talent talent);
        void RemoveTalent(PlayerProfile profile, Talent talent);
        void SetCovenant(PlayerProfile profile, Covenant covenant, bool cleanupCovenantData = true);
        void RemoveCovenantData(PlayerProfile profile);
        void SetSpellCastProfile(PlayerProfile profile, CastProfile castProfile);
        void SetProfileName(PlayerProfile profile, string profileName);
    }
}