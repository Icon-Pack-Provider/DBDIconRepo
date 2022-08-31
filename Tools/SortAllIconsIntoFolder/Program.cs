//Directory listing
Dictionary<string, List<string>> PerkFolder = new()
{
    { "Ash", new List<string>() { "iconPerks_buckleUp", "iconPerks_flipFlop", "iconPerks_mettleOfMan" } },
    { "Aurora", new List<string>() { "iconPerks_appraisal", "iconPerks_coupDeGrace", "iconPerks_deception", "iconPerks_Hoarder", "iconPerks_Oppression", "IconPerks_powerStruggle"} },
    { "Cannibal", new List<string>() { "iconPerks_BBQAndChili", "iconPerks_franklinsLoss", "iconPerks_knockOut" } },
    { "Comet", new List<string>() { "iconPerks_FastTrack", "iconPerks_HexCrowdControl", "iconPerks_NoWayOut", "iconPerks_Self-Preservation", "iconPerks_SmashHit", "iconPerks_Starstruck"} },
    { "DLC2", new List<string>() { "iconPerks_decisiveStrike", "iconPerks_dyingLight", "iconPerks_objectOfObsession", "iconPerks_playWithYourFood", "iconPerks_saveTheBestForLast", "iconPerks_soleSurvivor"} },
    { "DLC3", new List<string>() { "iconPerks_aceInTheHole", "iconPerks_devourHope", "iconPerks_openHanded", "iconPerks_ruin", "iconPerks_theThirdSeal", "iconPerks_thrillOfTheHunt", "iconPerks_upTheAnte"} },
    { "DLC4", new List<string>() { "iconPerks_alert", "iconPerks_generatorOvercharge", "iconPerks_lithe", "iconPerks_monitorAndAbuse", "iconPerks_overwelmingPresence", "iconPerks_technician"} },{ "DLC5", new List<string>() { "iconPerks_beastOfPrey", "iconPerks_DeadHard", "iconPerks_HuntressLullaby", "iconPerks_NoMither", "iconPerks_TerritorialImperative", "iconPerks_WereGonnaLiveForever" } },
    { "Eclipse", new List<string>() { "iconPerks_BiteTheBullet", "iconPerks_blastMine", "iconPerks_Counterforce", "iconPerks_eruption", "iconPerks_Flashbang", "iconPerks_hysteria", "iconPerks_lethalPursuer", "iconPerks_Resurgence", "iconPerks_RookieSpirit" } },
    { "England", new List<string>() { "iconPerks_bloodWarden", "iconPerks_fireUp", "iconPerks_pharmacy", "iconPerks_rememberMe", "iconPerks_vigil", "iconPerks_wakeUp" } },
    { "Finland", new List<string>() { "iconPerks_detectivesHunch", "iconPerks_hangmansTrick", "iconPerks_makeYourChoice", "iconPerks_stakeOut", "iconPerks_surveillance", "iconPerks_tenacity" } },
    { "Gemini", new List<string>() { "iconPerks_Deadlock", "iconPerks_HexPlaything", "iconPerks_ScourgeHookGiftOfPain" } },
    { "Guam", new List<string>() { "iconPerks_bamboozle", "iconPerks_coulrophobia", "iconPerks_popGoesTheWeasel" } },
    { "Haiti", new List<string>() { "iconPerks_autodidact", "iconPerks_deliverance", "iconPerks_diversion", "iconPerks_hatred", "iconPerks_hauntedGround", "iconPerks_spiritFury" } },
    { "Hubble", new List<string>() { "iconPerks_BoonCircleOfHealing", "iconPerks_BoonShadowStep", "iconPerks_Clairvoyance" } },
    { "Ion", new List<string>() { "T_iconPerks_BoonExponential", "T_iconPerks_CorrectiveAction",  "T_iconPerks_grimEmbrace", "T_iconPerks_hexPentimento", "T_iconPerks_Overcome", "T_iconPerks_painResonance"} },
    { "Kate", new List<string>() { "iconPerks_boilOver", "iconPerks_danceWithMe", "iconPerks_windowsOfOpportunity" } },
    { "Kenya", new List<string>() { "iconPerks_aftercare", "iconPerks_breakdown", "iconPerks_discordance", "iconPerks_distortion", "iconPerks_ironMaiden", "iconPerks_madGrit" } },
    { "Kepler", new List<string>() { "iconPerks_callOfBrine", "iconPerks_darkTheory", "iconPerks_empathicConnection", "iconPerks_floodOfRage", "iconPerks_mercilessStrom", "iconPerks_parentalGuidance" } },
    { "L4D", new List<string>() { "iconPerks_borrowedTime", "iconPerks_leftBehind", "iconPerks_unbreakable" } },
    { "Mali", new List<string>() { "iconPerks_corruptIntervention", "iconPerks_darkDevotion", "iconPerks_headOn", "iconPerks_infectiousFright", "iconPerks_poised", "iconPerks_solidarity" } },
    { "Meteor", new List<string>() { "iconPerks_BoonDestroyer", "iconPerks_DarknessRevelated", "iconPerks_Dissolution", "iconPerks_InnerFocus", "iconPerks_Overzealous", "iconPerks_ResidualManifest", "iconPerks_SepticTouch" } },
    //{ "NoLicense", new List<string>() { "iconPerks_bloodWarden", "iconPerks_decisiveStrike", "iconPerks_dyingLight", "iconPerks_fireUp", "iconPerks_objectOfObsession", "iconPerks_pharmacy", "iconPerks_playWithYourFood", "iconPerks_rememberMe", "iconPerks_saveTheBestForLast", "iconPerks_soleSurvivor", "iconPerks_vigil", "iconPerks_wakeUp"} },
    { "Oman", new List<string>() { "iconPerks_furtiveChase", "iconPerks_imAllEars", "iconPerks_thrillingTremors" } },
    { "Qater", new List<string>() { "iconPerks_babySitter", "iconPerks_betterTogether", "iconPerks_camaraderie", "iconPerks_cruelConfinement", "iconPerks_fixated", "iconPerks_guardian", "iconPerks_innerStrength", "iconPerks_mindBreaker", "iconPerks_pushThroughIt", "iconPerks_secondWind", "iconPerks_situationalAwareness", "iconPerks_surge", "iconPerks_survivalInstincts" } },
    { "Sweden", new List<string>() { "iconPerks_anyMeansNecessary", "iconPerks_bloodEcho", "iconPerks_breakout", "iconPerks_luckyBreak", "iconPerks_nemesis", "iconPerks_zanshinTactics" } },
    { "Ukraine", new List<string>() { "iconPerks_deadManSwitch", "iconPerks_forThePeople", "iconPerks_gearHead", "iconPerks_hexRetribution", "iconPerks_offTheRecord", "iconPerks_redHerring" } },
    { "Wales", new List<string>() { "iconPerks_bloodPact", "iconPerks_deathbound", "iconPerks_forcedPenance", "iconPerks_repressedAlliance", "iconPerks_soulGuard", "iconPerks_trailOfTorment" } },
    { "Yemen", new List<string>() { "iconPerks_builtToLast", "iconPerks_desperateMeasures", "iconPerks_dragonsGrip", "iconPerks_hexBloodFavor", "iconPerks_hexUndying", "iconPerks_visionary" } },
};

//Moving
string path = $"{Environment.CurrentDirectory}\\Sorting";
foreach (var file in Directory.GetFiles(path))
{
    FileInfo info = new(file);
    if (info.Extension == ".png")
    {
        if (info.Name.ToLower().StartsWith("iconperks") ||
            info.Name.ToLower().StartsWith("t_iconperks"))
        {
            //Perks
            if (!Directory.Exists($"{path}\\Perks"))
                Directory.CreateDirectory($"{path}\\Perks");
            foreach (var keyContent in PerkFolder)
            {
                foreach (var perk in keyContent.Value)
                {
                    string compare = Path.GetFileNameWithoutExtension(info.FullName);
                    if (compare.ToLower().EndsWith(perk.ToLower()))
                    {
                        string toMove = $"{$"{path}\\Perks"}\\{keyContent.Key}";
                        if (!Directory.Exists(toMove))
                            Directory.CreateDirectory(toMove);
                        info.MoveTo($"{$"{path}\\Perks"}\\{keyContent.Key}\\{info.Name}");
                        continue;
                    }
                }
            }
        }
    }
}
