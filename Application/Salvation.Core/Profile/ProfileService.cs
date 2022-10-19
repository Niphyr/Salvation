﻿using Newtonsoft.Json;
using Salvation.Core.Constants.Data;
using Salvation.Core.Interfaces.Profile;
using Salvation.Core.Profile.Model;
using Salvation.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Talent = Salvation.Core.Profile.Model.Talent;

namespace Salvation.Core.Profile
{
    /// <summary>
    /// This class has methods to interact with a profile at a high level 
    /// i.e. Validating, Cloning the profile. Use GameStateService for general
    /// get/set operations within the profile.
    /// </summary>
    public class ProfileService : IProfileService
    {
        public ProfileService()
        {

        }

        #region Equipment Management

        /// <summary>
        /// Add an item to the profile and optionally equip it (calls EquipItem()).
        /// EquipItem() is automatically called if the item's Equippd property is true.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="item">The item to add</param>
        /// <param name="equip">Set to true to equip it immediately</param>
        public void AddItem(PlayerProfile profile, Item item, bool equip = false)
        {
            // Item can be validated here if needed (effects/spells).
            if (equip || item.Equipped)
                EquipItem(profile, item);

            profile.Items.Add(item);
        }

        /// <summary>
        /// Sets an items state to eqipped
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="item"></param>
        private void EquipItem(PlayerProfile profile, Item item)
        {
            var existingItems = profile.Items
                .Where(i => i.Slot == item.Slot && i.Equipped == true).ToList();

            // If it's not ring/trinket and one already exists, remove
            if (existingItems.Count == 1 && item.Slot != InventorySlot.Finger
                && item.Slot != InventorySlot.Trinket)
            {
                var removeItem = existingItems.First();
                removeItem.Equipped = false;
            }

            // If it is ring or trinket and we already have 2, remove the oldest one
            if (existingItems.Count == 2 &&
                (item.Slot == InventorySlot.Finger || item.Slot == InventorySlot.Trinket))
            {
                var removeItem = existingItems.First();
                removeItem.Equipped = false;
            }

            item.Equipped = true;
        }

        public List<Item> GetEquippedItems(PlayerProfile profile)
        {
            return profile.Items.Where(i => i.Equipped).ToList();
        }

        #endregion Equipment Management

        #region Talent Management

        /// <summary>
        /// Update a talent's rank. If it doesn't exist, add it. 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>The associated talent</returns>
        public Talent UpdateTalent(PlayerProfile profile, Spell spell, int rank)
        {
            var talent = GetTalent(profile, spell);

            talent.Rank = rank;

            return talent;
        }

        /// <summary>
        /// Return a talent if it exists, if not create it and return the new one.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="spell"></param>
        /// <returns></returns>
        public Talent GetTalent(PlayerProfile profile, Spell spell)
        {
            var existingTalent = profile.Talents.Where(t => t.SpellId == (int)spell).FirstOrDefault();

            if (existingTalent != null)
                return existingTalent;

            var newTalent = new Talent(spell, 0);

            profile.Talents.Add(newTalent);

            return newTalent;
        }

        #endregion Talent Management

        /// <summary>
        /// Sets the cast profile for the spellid set inside the provided CastProfile.
        /// Will overwrite an existing CastProfile entry if exists.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="castProfile"></param>
        public void SetSpellCastProfile(PlayerProfile profile, CastProfile castProfile)
        {
            profile.Casts.RemoveAll(c => c.SpellId == castProfile.SpellId);

            profile.Casts.Add(castProfile);
        }

        /// <summary>
        /// Sets the playstyle for the name set inside the provided playstyle.
        /// Will overwrite an existing playstyle entry if exists.
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="playstyle"></param>
        public void SetPlaystyleEntry(PlayerProfile profile, PlaystyleEntry playstyle)
        {
            profile.PlaystyleEntries.RemoveAll(p => p.Name == playstyle.Name);

            profile.PlaystyleEntries.Add(playstyle);
        }

        public void SetProfileName(PlayerProfile profile, string profileName)
        {
            profile.Name = profileName;
        }

        public static List<Talent> GetAllTalents(int specId)
        {
            switch(specId)
            {
                case (int)Spec.HolyPriest:
                    return GetHolyPriestTalents();
                case (int)Spec.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(specId), "Spec ID is not implemented.");
            }
        }

        private static List<Talent> GetHolyPriestTalents()
        {
            // This can be generated from TalentStructureUpdateService::MassageTalentOptions
            return new List<Talent>()
            {
                // Class Talents
                new Talent(Spell.Renew, 1),
                new Talent(Spell.DispelMagic, 0),
                new Talent(Spell.Shadowfiend, 0),
                new Talent(Spell.PrayerOfMending, 1),
                new Talent(Spell.ImprovedFlashHeal, 0),
                new Talent(Spell.ImprovedPurify, 0),
                new Talent(Spell.PsychicVoice, 0),
                new Talent(Spell.ShadowWordDeath, 0),
                new Talent(Spell.FocusedMending, 0),
                new Talent(Spell.HolyNova, 0),
                new Talent(Spell.ProtectiveLight, 0),
                new Talent(Spell.FromDarknessComesLight, 0),
                new Talent(Spell.AngelicFeather, 0),
                new Talent(Spell.Phantasm, 0),
                new Talent(Spell.DeathAndMadness, 0),
                new Talent(Spell.SpellWarding, 0),
                new Talent(Spell.BlessedRecovery, 0),
                new Talent(Spell.Rhapsody, 0),
                new Talent(Spell.LeapOfFaith, 0),
                new Talent(Spell.ShackleUndead, 0),
                new Talent(Spell.SheerTerror, 0),
                new Talent(Spell.VoidTendrils, 0),
                new Talent(Spell.MindControl, 0),
                new Talent(Spell.DominateMind, 0),
                new Talent(Spell.WordsOfThePious, 0),
                new Talent(Spell.MassDispel, 0),
                new Talent(Spell.MoveWithGrace, 0),
                new Talent(Spell.PowerInfusion, 0),
                new Talent(Spell.VampiricEmbrace, 0),
                new Talent(Spell.TitheEvasion, 0),
                new Talent(Spell.Inspiration, 0),
                new Talent(Spell.ImprovedMassDispel, 0),
                new Talent(Spell.BodyAndSoul, 0),
                new Talent(Spell.TwinsOfTheSunPriestess, 0),
                new Talent(Spell.VoidShield, 0),
                new Talent(Spell.Sanlayn, 0),
                new Talent(Spell.Apathy, 0),
                new Talent(Spell.UnwaveringWill, 0),
                new Talent(Spell.TwistOfFate, 0),
                new Talent(Spell.ThroesOfPain, 0),
                new Talent(Spell.AngelsMercy, 0),
                new Talent(Spell.BindingHeals, 0),
                new Talent(Spell.DivineStar, 0),
                new Talent(Spell.Halo, 0),
                new Talent(Spell.TranslucentImage, 0),
                new Talent(Spell.Mindgames, 0),
                new Talent(Spell.SurgeOfLight, 0),
                new Talent(Spell.LightsInspiration, 0),
                new Talent(Spell.CrystallineReflection, 0),
                new Talent(Spell.ImprovedFade, 0),
                new Talent(Spell.Manipulation, 0),
                new Talent(Spell.PowerWordLife, 0),
                new Talent(Spell.AngelicBulwark, 0),
                new Talent(Spell.VoidShift, 0),
                new Talent(Spell.ShatteredPerceptions, 0),
                // Spec Talents
                new Talent(Spell.HolyWordSerenity, 0),
                new Talent(Spell.PrayerOfHealing, 0),
                new Talent(Spell.GuardianSpirit, 0),
                new Talent(Spell.HolyWordChastise, 0),
                new Talent(Spell.HolyWordSanctify, 0),
                new Talent(Spell.GuardianAngel, 0),
                new Talent(Spell.GuardiansOfTheLight, 0),
                new Talent(Spell.Censure, 0),
                new Talent(Spell.BurningVehemence, 0),
                new Talent(Spell.CircleOfHealing, 0),
                new Talent(Spell.RevitalizingPrayers, 0),
                new Talent(Spell.SanctifiedPrayers, 0),
                new Talent(Spell.CosmicRipple, 0),
                new Talent(Spell.Afterlife, 0),
                new Talent(Spell.RenewedFaith, 0),
                new Talent(Spell.EmpyrealBlaze, 0),
                new Talent(Spell.PrayerCircle, 0),
                new Talent(Spell.HealingChorus, 0),
                new Talent(Spell.PrayerfulLitany, 0),
                new Talent(Spell.TrailOfLight, 0),
                new Talent(Spell.DivineHymn, 0),
                new Talent(Spell.Enlightenment, 0),
                new Talent(Spell.Benediction, 0),
                new Talent(Spell.HolyMending, 0),
                new Talent(Spell.Orison, 0),
                new Talent(Spell.EverlastingLight, 0),
                new Talent(Spell.GalesOfSong, 0),
                new Talent(Spell.SymbolOfHope, 0),
                new Talent(Spell.DivineService, 0),
                new Talent(Spell.CrisisManagement, 0),
                new Talent(Spell.PrismaticEchoes, 0),
                new Talent(Spell.PrayersOfTheVirtuous, 0),
                new Talent(Spell.Pontifex, 0),
                new Talent(Spell.Apotheosis, 0),
                new Talent(Spell.HolyWordSalvation, 0),
                new Talent(Spell.EmpoweredRenew, 0),
                new Talent(Spell.RapidRecovery, 0),
                new Talent(Spell.SayYourPrayers, 0),
                new Talent(Spell.ResonantWords, 0),
                new Talent(Spell.DesperateTimes, 0),
                new Talent(Spell.LightOfTheNaaru, 0),
                new Talent(Spell.HarmoniousApparatus, 0),
                new Talent(Spell.SearingLight, 0),
                new Talent(Spell.AnsweredPrayers, 0),
                new Talent(Spell.Lightweaver, 0),
                new Talent(Spell.Lightwell, 0),
                new Talent(Spell.DivineImage, 0),
                new Talent(Spell.DivineWord, 0),
                new Talent(Spell.MiracleWorker, 0),
                new Talent(Spell.Restitution, 0),
            };
        }

        #region Profile Management

        public PlayerProfile GetDefaultProfile(Spec spec)
        {
            PlayerProfile profile = spec switch
            {
                Spec.HolyPriest => GenerateHolyPriestProfile(),
                _ => throw new ArgumentOutOfRangeException("Spec", "Spec must be a valid supported spec."),
            };

            return profile;
        }

        /// <summary>
        /// Attempts to re-create the profile following standard rules. 
        /// Should be used after a profile is obtain from an unknown source
        /// such as API endpoints or desserialisation. Errors are ignored
        /// and invalid data is silently dropped. TODO: Capture dropped data
        /// </summary>
        /// <param name="profile">The profile from an unknown source</param>
        /// <returns>A parsed and validated profile</returns>
        public PlayerProfile ValidateProfile(PlayerProfile profile)
        {
            PlayerProfile newProfile = new PlayerProfile()
            {
                // First, apply basic stats and settings
                Spec = profile.Spec,
                Name = profile.Name,
                Server = profile.Server,
                Region = profile.Region,
                Race = profile.Race,
                Class = profile.Class,
                Level = profile.Level,
                FightLengthSeconds = profile.FightLengthSeconds,
            };

            // Casts (spell usage).
            foreach (var cast in profile.Casts)
            {
                SetSpellCastProfile(newProfile, cast);
            }

            // Items
            foreach (var item in profile.Items)
            {
                AddItem(newProfile, item);
            }

            // Talents
            // First get all possible talents, then update the ranks.
            newProfile.Talents = GetAllTalents((int)profile.Spec);

            foreach (var talent in profile.Talents)
            {
                UpdateTalent(newProfile, talent.Spell, talent.Rank);
            }

            // Playstyle entries
            foreach (var playstyle in profile.PlaystyleEntries)
            {
                SetPlaystyleEntry(newProfile, playstyle);
            }

            return newProfile;
        }

        private PlayerProfile GenerateHolyPriestProfile()
        {
            var basicProfile = new PlayerProfile()
            {
                Name = "Holy Priest Default Profile",
                Spec = Spec.HolyPriest,
                Race = Race.NoRace,
                Class = Class.Priest,
                Casts = new List<CastProfile>()
                {
                    new CastProfile((int)Spell.LeechHeal, 0.0d, 0.3475d, 1, 0, "Leech"),

                    // Base Spells (SpellId, Efficiency, Overheal, HealTargets, DamageTargets, NameAndKeywords)
                    new CastProfile((int)Spell.FlashHeal, 0.1103d, 0.1084d, 1, 0, "Flash Heal, FH"),
                    new CastProfile((int)Spell.Heal, 0.1564d, 0.3054d, 1, 0, "Heal"),
                    new CastProfile((int)Spell.Renew, 0.0364d, 0.3643d, 1, 0, "Renew"),
                    new CastProfile((int)Spell.Mindgames, 1.0d, 0.01d, 1, 1, "Mindgames"),
                    new CastProfile((int)Spell.PrayerOfMending, 0.9056d, 0.0219d, 1, 0, "Prayer of Mending, PoM"),
                    new CastProfile((int)Spell.PrayerOfHealing, 0.2931d, 0.2715d, 5, 0, "Prayer of Healing, PoH"),
                    new CastProfile((int)Spell.HolyNova, 0.0034d, 0.15d, 20, 1, "Holy Nova"),
                    new CastProfile((int)Spell.HolyWordSerenity, 0.677d, 0.1515d, 1, 0, "Holy Word: Serenity, HW"),
                    new CastProfile((int)Spell.HolyWordSanctify, 0.7822d, 0.3234d, 6, 0, "Holy Word: Sanctify, HW"),
                    new CastProfile((int)Spell.DivineHymn, 0.8005d, 0.314d, 20, 0, "Divine Hymn"),
                    new CastProfile((int)Spell.CircleOfHealing, 0.8653d, 0.1417d, 5, 0, "CoH, Circle of Healing"),
                    new CastProfile((int)Spell.DivineStar, 0.81d, 0.44d, 10, 10, "Divine Star, Divstar"),
                    new CastProfile((int)Spell.Halo, 0.7596d, 0.3658d, 20, 1, "Halo"),
                    new CastProfile((int)Spell.HolyWordSalvation, 0.804d, 0.3142d, 20, 0, "Holy Word: Salvation"),
                    new CastProfile((int)Spell.CosmicRipple, 1d, 0.2332d, 5, 0, "Cosmic Ripple, CR"),
                    new CastProfile((int)Spell.PowerWordShield, 0.01d, 0.38d, 1, 0, "Power Word: Shield, PW:S"),
                    new CastProfile((int)Spell.EchoOfLight, 0d, 0.4224d, 1, 0, "Echo of Light, EoL"),
                    new CastProfile((int)Spell.GuardianSpirit, 0.36d, 0d, 1, 0, "Guardian Spirit, GS, Angel"),
                    new CastProfile((int)Spell.TrailOfLight, 1, 0.329d, 1, 0, "Trail of Light, ToL"),
                    new CastProfile((int)Spell.BindingHeals, 1, 0.411d, 1, 0, "Binding Heals, BH"),
                    new CastProfile((int)Spell.Lightwell, 1, 0.243d, 15, 0, "Lightwell, LW"),
                    new CastProfile((int)Spell.EmpoweredRenew, 1, 0.196d, 1, 0, "Empowered Renew"),
                    new CastProfile((int)Spell.HolyMending, 1, 0.189d, 1, 0, "Empowered Renew"),
                    new CastProfile((int)Spell.DivineImageBlessedLight, 0.0d, 0.10d, 5, 0, "Divine Image, DI"),
                    new CastProfile((int)Spell.DivineImageDazzlingLight, 0.0d, 0.10d, 5, 0, "Divine Image, DI"),
                    new CastProfile((int)Spell.DivineImageHealingLight, 0.0d, 0.05d, 1, 0, "Divine Image, DI"),
                    new CastProfile((int)Spell.DivineImageLightEruption, 0.0d, 0.25d, 1, 0, "Divine Image, DI"),
                    new CastProfile((int)Spell.DivineImageSearingLight, 0.0d, 0.25d, 1, 0, "Divine Image, DI"),
                    new CastProfile((int)Spell.DivineImageTranquilLight, 0.0d, 0.20d, 1, 0, "Divine Image, DI"),

                    // DPS Spells
                    new CastProfile((int)Spell.Smite, 0.12d, 0, 0, 1, "Smite"),
                    new CastProfile((int)Spell.ShadowWordPain, 0.04d, 0, 0, 1, "Shadow Word: Pain, SW:P"),
                    new CastProfile((int)Spell.ShadowWordDeath, 0.01d, 0, 0, 1, "Shadow Word: Death, SW:D"),
                    new CastProfile((int)Spell.HolyWordChastise, 0.01d, 0, 0, 1, "Holy Word: Chastise"),
                    new CastProfile((int)Spell.HolyFire, 0.01d, 0, 0, 1, "Holy Fire, HF"),
                },
                Talents = GetAllTalents((int)Spec.HolyPriest),
                FightLengthSeconds = 397,
                PlaystyleEntries = new List<PlaystyleEntry>()
                {
                    new PlaystyleEntry("GroupSize", 20, (uint)Spell.HolyPriest),
                    // #### Base Overrides ####
                    // Overrides the stat value to be set directly rather than from items/race/class
                    new PlaystyleEntry("OverrideStatIntellect", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("OverrideStatHaste", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("OverrideStatCriticalStrike", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("OverrideStatVersatility", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("OverrideStatMastery", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("OverrideStatLeech", 0, (uint)Spell.HolyPriest),

                    // Adds additional stats (for use with stat weights)
                    new PlaystyleEntry("GrantAdditionalStatIntellect", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("GrantAdditionalStatMastery", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("GrantAdditionalStatHaste", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("GrantAdditionalStatVersatility", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("GrantAdditionalStatCriticalStrike", 0, (uint)Spell.HolyPriest),
                    new PlaystyleEntry("GrantAdditionalStatLeech", 0, (uint)Spell.HolyPriest),

                    new PlaystyleEntry("LeechSelfHealPercent", 0.20, (uint)Spell.HolyPriest),

                    // Force the cloth armor bonus
                    new PlaystyleEntry("ForceClothBonus", 0, (uint)Spell.HolyPriest),
                    // Amount of average damage taken per second over the course of a fight
                    new PlaystyleEntry("DamageTakenPerSecond", 1500, (uint)Spell.HolyPriest),
                    
                    // #### Priest ####
                    // ## Talent overrides
                    new PlaystyleEntry("TwistOfFateUptime", 0.3354, (uint)Spell.TwistOfFate),

                    // #### Holy Priest ####
                    // ## Talent overrides
                    new PlaystyleEntry("EverlastingLightAverageMana", 0.55, (uint)Spell.EverlastingLight),
                    new PlaystyleEntry("PontifexAverageSalvationStacks", 0.2, (uint)Spell.Pontifex),
                    new PlaystyleEntry("PontifexPercentUsageSerenity", 0.25, (uint)Spell.Pontifex),
                    new PlaystyleEntry("UnwaveringWillUptime", 0.9, (uint)Spell.UnwaveringWill),
                    new PlaystyleEntry("SanctifiedPrayersUptime", 0.8, (uint)Spell.SanctifiedPrayers),
                    new PlaystyleEntry("PrayerCircleUptime", 0.8, (uint)Spell.PrayerCircle),
                    new PlaystyleEntry("BindingHealsSelfCastPercentage", 0.15, (uint)Spell.BindingHeals),
                    new PlaystyleEntry("PoMPercentageStacksExpired", 0.01, (uint)Spell.PrayerOfMending),
                    new PlaystyleEntry("HealingChorusStacksWastedPerMinute", 3.29, (uint)Spell.HealingChorus),
                    new PlaystyleEntry("LightweaverAverageBuffedCasts", .95, (uint)Spell.Lightweaver),

                    // ## Damage & Healing overrides
                    new PlaystyleEntry("ShadowWordDeathPercentExecute", 0.2, (uint)Spell.ShadowWordDeath),

                    // ## Items overrides
                },
                Items = new List<Item>()
                {

                }
            };

            UpdateTalent(basicProfile, Spell.DispelMagic, 1);
            UpdateTalent(basicProfile, Spell.Shadowfiend, 1);
            UpdateTalent(basicProfile, Spell.ImprovedFlashHeal, 1);
            UpdateTalent(basicProfile, Spell.ImprovedPurify, 1);
            UpdateTalent(basicProfile, Spell.FocusedMending, 1);
            UpdateTalent(basicProfile, Spell.HolyNova, 1);
            UpdateTalent(basicProfile, Spell.ProtectiveLight, 1);
            UpdateTalent(basicProfile, Spell.AngelicFeather, 1);
            UpdateTalent(basicProfile, Spell.SpellWarding, 1);
            UpdateTalent(basicProfile, Spell.Rhapsody, 1);
            UpdateTalent(basicProfile, Spell.LeapOfFaith, 1);
            UpdateTalent(basicProfile, Spell.MassDispel, 1);
            UpdateTalent(basicProfile, Spell.PowerInfusion, 1);
            UpdateTalent(basicProfile, Spell.ImprovedMassDispel, 1);
            UpdateTalent(basicProfile, Spell.BodyAndSoul, 1);
            UpdateTalent(basicProfile, Spell.TwinsOfTheSunPriestess, 1);
            UpdateTalent(basicProfile, Spell.UnwaveringWill, 2);
            UpdateTalent(basicProfile, Spell.TwistOfFate, 2);
            UpdateTalent(basicProfile, Spell.AngelsMercy, 1);
            UpdateTalent(basicProfile, Spell.Halo, 1);
            UpdateTalent(basicProfile, Spell.TranslucentImage, 1);
            UpdateTalent(basicProfile, Spell.SurgeOfLight, 2);
            UpdateTalent(basicProfile, Spell.LightsInspiration, 2);
            UpdateTalent(basicProfile, Spell.PowerWordLife, 1);
            UpdateTalent(basicProfile, Spell.AngelicBulwark, 1);

            UpdateTalent(basicProfile, Spell.HolyWordSerenity, 1);
            UpdateTalent(basicProfile, Spell.PrayerOfHealing, 1);
            UpdateTalent(basicProfile, Spell.GuardianSpirit, 1);
            UpdateTalent(basicProfile, Spell.HolyWordSanctify, 1);
            UpdateTalent(basicProfile, Spell.GuardianAngel, 1);
            UpdateTalent(basicProfile, Spell.CircleOfHealing, 1);
            UpdateTalent(basicProfile, Spell.SanctifiedPrayers, 1);
            UpdateTalent(basicProfile, Spell.CosmicRipple, 1);
            UpdateTalent(basicProfile, Spell.RenewedFaith, 1);
            UpdateTalent(basicProfile, Spell.PrayerCircle, 1);
            UpdateTalent(basicProfile, Spell.PrayerfulLitany, 1);
            UpdateTalent(basicProfile, Spell.DivineHymn, 1);
            UpdateTalent(basicProfile, Spell.Enlightenment, 1);
            UpdateTalent(basicProfile, Spell.Benediction, 1);
            UpdateTalent(basicProfile, Spell.Orison, 1);
            UpdateTalent(basicProfile, Spell.GalesOfSong, 2);
            UpdateTalent(basicProfile, Spell.SymbolOfHope, 1);
            UpdateTalent(basicProfile, Spell.PrismaticEchoes, 2);
            UpdateTalent(basicProfile, Spell.PrayersOfTheVirtuous, 2);
            UpdateTalent(basicProfile, Spell.HolyWordSalvation, 1);
            UpdateTalent(basicProfile, Spell.SayYourPrayers, 1);
            UpdateTalent(basicProfile, Spell.LightOfTheNaaru, 2);
            UpdateTalent(basicProfile, Spell.AnsweredPrayers, 2);
            UpdateTalent(basicProfile, Spell.DivineImage, 1);
            UpdateTalent(basicProfile, Spell.MiracleWorker, 1);

            return basicProfile;
        }

        /// <summary>
        /// Deep clone a profile by serialising and deserialising it as JSON.
        /// </summary>
        /// <param name="profile">The profile to be cloned</param>
        /// <returns>A fresh instance of the profile</returns>
        public PlayerProfile CloneProfile(PlayerProfile profile)
        {
            var profileString = JsonConvert.SerializeObject(profile);

            var newProfile = JsonConvert.DeserializeObject<PlayerProfile>(profileString);

            return newProfile;
        }

        #endregion
    }
}
