using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Status;
using System.Diagnostics;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using System.Data;
using System.Collections;
using System.Xml.Linq;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using System.Security.AccessControl;
using static Enums;

public class ItemsAlert
{
    GameData gameData = GameData.Instance;
    public Dictionary<string, bool> PickItemsRunesKeyGems = new Dictionary<string, bool>();
    public Dictionary<string, int> PickItemsRunesKeyGems_Quantity = new Dictionary<string, int>();
    public Dictionary<string, bool> PickItemsPotions = new Dictionary<string, bool>();

    public Dictionary<string, bool> PickItemsNormal_ByName = new Dictionary<string, bool>();
    public Dictionary<string, Dictionary<uint, string>> PickItemsNormal_ByName_Flags = new Dictionary<string, Dictionary<uint, string>>();
    public Dictionary<string, int> PickItemsNormal_ByName_Quality = new Dictionary<string, int>();
    public Dictionary<string, int> PickItemsNormal_ByName_Class = new Dictionary<string, int>();
    public Dictionary<string, Dictionary<string, int>> PickItemsNormal_ByName_Stats = new Dictionary<string, Dictionary<string, int>>();
    public Dictionary<string, Dictionary<string, string>> PickItemsNormal_ByName_Operators = new Dictionary<string, Dictionary<string, string>>();
    public Dictionary<string, string> PickItemsNormal_ByNameDesc = new Dictionary<string, string>();

    public Dictionary<string, bool> PickItemsNormal_ByType = new Dictionary<string, bool>();
    public Dictionary<string, Dictionary<uint, string>> PickItemsNormal_ByType_Flags = new Dictionary<string, Dictionary<uint, string>>();
    public Dictionary<string, int> PickItemsNormal_ByType_Quality = new Dictionary<string, int>();
    public Dictionary<string, int> PickItemsNormal_ByType_Class = new Dictionary<string, int>();
    public Dictionary<string, Dictionary<string, int>> PickItemsNormal_ByType_Stats = new Dictionary<string, Dictionary<string, int>>();
    public Dictionary<string, Dictionary<string, string>> PickItemsNormal_ByType_Operators = new Dictionary<string, Dictionary<string, string>>();
    public Dictionary<string, string> PickItemsNormal_ByTypeDesc = new Dictionary<string, string>();

    public Dictionary<string, List<string>> typeMapping = new Dictionary<string, List<string>>();

    public void Init()
    {
        typeMapping = new Dictionary<string, List<string>>()
        {
            {"amulet", new List<string>{"Amulet"}},
            {"gold", new List<string>{"Gold"}},
            {"ring", new List<string>{"Ring"}},
            {"jewel", new List<string>{"Jewel"}},
            {"axe", new List<string>{"HandAxe", "Axe", "DoubleAxe", "MilitaryPick", "WarAxe", "LargeAxe", "BroadAxe", "BattleAxe", "GreatAxe", "GiantAxe", "Hatchet", "Cleaver", "TwinAxe", "Crowbill", "Naga", "MilitaryAxe", "BeardedAxe", "Tabar", "GothicAxe", "AncientAxe", "Tomahawk", "SmallCrescent", "EttinAxe", "WarSpike", "BerserkerAxe", "FeralAxe", "SilverEdgedAxe", "Decapitator", "ChampionAxe", "GloriousAxe"}},
            {"wand", new List<string>{"Wand", "YewWand", "BoneWand", "GrimWand", "BurntWand", "PetrifiedWand", "TombWand", "GraveWand", "PolishedWand", "GhostWand", "LichWand", "UnearthedWand"}},
            {"club", new List<string>{"Club", "SpikedClub", "Cudgel", "BarbedClub", "Truncheon", "TyrantClub"}},
            {"scepter", new List<string>{"Scepter", "GrandScepter", "WarScepter", "RuneScepter", "HolyWaterSprinkler", "DivineScepter", "MightyScepter", "SeraphRod", "Caduceus"}},
            {"mace", new List<string>{"Mace", "MorningStar", "Flail", "FlangedMace", "JaggedStar", "Knout", "ReinforcedMace", "DevilStar", "Scourge"}},
            {"hammer", new List<string>{"WarHammer", "Maul", "GreatMaul", "BattleHammer", "WarClub", "MartelDeFer", "LegendaryMallet", "OgreMaul", "ThunderMaul"}},
            {"sword", new List<string>{"ShortSword", "Scimitar", "Sabre", "Falchion", "CrystalSword", "BroadSword", "LongSword", "WarSword", "TwoHandedSword", "Claymore", "GiantSword", "BastardSword", "Flamberge", "GreatSword", "Gladius", "Cutlass", "Shamshir", "Tulwar", "DimensionalBlade", "BattleSword", "RuneSword", "AncientSword", "Espandon", "DacianFalx", "TuskSword", "GothicSword", "Zweihander", "ExecutionerSword", "Falcata", "Ataghan", "ElegantBlade", "HydraEdge", "PhaseBlade", "ConquestSword", "CrypticSword", "MythicalSword", "LegendSword", "HighlandBlade", "BalrogBlade", "ChampionSword", "ColossusSword", "ColossusBlade"}},
            {"knife", new List<string>{"Dagger", "Dirk", "Kris", "Blade", "Poignard", "Rondel", "Cinquedeas", "Stiletto", "BoneKnife", "MithrilPoint", "FangedKnife", "LegendSpike"}},
            {"thrownweapon", new List<string>{"ThrowingKnife", "BalancedKnife", "BattleDart", "WarDart", "FlyingKnife", "WingedKnife"}},
            {"throwingaxe", new List<string>{"ThrowingAxe", "BalancedAxe", "Francisca", "Hurlbat", "FlyingAxe", "WingedAxe"}},
            {"javelin", new List<string>{"Javelin", "Pilum", "ShortSpear", "Glaive", "ThrowingSpear", "WarJavelin", "GreatPilum", "Simbilan", "Spiculum", "Harpoon", "HyperionJavelin", "StygianPilum", "BalrogSpear", "GhostGlaive", "WingedHarpoon"}},
            {"spear", new List<string>{"Spear", "Trident", "Brandistock", "Spetum", "Pike", "WarSpear", "Fuscina", "WarFork", "Yari", "Lance", "HyperionSpear", "StygianPike", "Mancatcher", "GhostSpear", "WarPike"}},
            {"polearm", new List<string>{"Bardiche", "Voulge", "Scythe", "Poleaxe", "Halberd", "WarScythe", "LochaberAxe", "Bill", "BattleScythe", "Partizan", "BecDeCorbin", "GrimScythe", "OgreAxe", "ColossusVoulge", "Thresher", "CrypticAxe", "GreatPoleaxe", "GiantThresher"}},
            {"staff", new List<string>{"ShortStaff", "LongStaff", "GnarledStaff", "BattleStaff", "WarStaff", "JoStaff", "QuarterStaff", "CedarStaff", "GothicStaff", "RuneStaff", "WalkingStick", "Stalagmite", "ElderStaff", "Shillelagh", "ArchonStaff"}},
            {"bow", new List<string>{"ShortBow", "HuntersBow", "LongBow", "CompositeBow", "ShortBattleBow", "LongBattleBow", "ShortWarBow", "LongWarBow", "EdgeBow", "RazorBow", "CedarBow", "DoubleBow", "ShortSiegeBow", "LargeSiegeBow", "RuneBow", "GothicBow", "SpiderBow", "BladeBow", "ShadowBow", "GreatBow", "DiamondBow", "CrusaderBow", "WardBow", "HydraBow"}},
            {"crossbow", new List<string>{"LightCrossbow", "Crossbow", "HeavyCrossbow", "RepeatingCrossbow", "Arbalest", "SiegeCrossbow", "Ballista", "ChuKoNu", "PelletBow", "GorgonCrossbow", "ColossusCrossbow", "DemonCrossBow"}},
            {"helm", new List<string>{"Cap", "SkullCap", "Helm", "FullHelm", "GreatHelm", "Crown", "Mask", "BoneHelm", "WarHat", "Sallet", "Casque", "Basinet", "WingedHelm", "GrandCrown", "DeathMask", "GrimHelm", "Shako", "Hydraskull", "Armet", "GiantConch", "SpiredHelm", "Corona", "DemonHead", "BoneVisage"}},
            {"armor", new List<string>{"QuiltedArmor", "LeatherArmor", "HardLeatherArmor", "StuddedLeather", "RingMail", "ScaleMail", "ChainMail", "BreastPlate", "SplintMail", "PlateMail", "FieldPlate", "GothicPlate", "FullPlateMail", "AncientArmor", "LightPlate", "GhostArmor", "SerpentskinArmor", "DemonhideArmor", "TrellisedArmor", "LinkedMail", "TigulatedMail", "MeshArmor", "Cuirass", "RussetArmor", "TemplarCoat", "SharktoothArmor", "EmbossedPlate", "ChaosArmor", "OrnatePlate", "MagePlate", "DuskShroud", "Wyrmhide", "ScarabHusk", "WireFleece", "DiamondMail", "LoricatedMail", "Boneweave", "GreatHauberk", "BalrogSkin", "HellforgePlate", "KrakenShell", "LacqueredPlate", "ShadowPlate", "SacredArmor", "ArchonPlate"}},
            {"shield", new List<string>{"Buckler", "SmallShield", "LargeShield", "KiteShield", "TowerShield", "GothicShield", "BoneShield", "SpikedShield", "Defender", "RoundShield", "Scutum", "DragonShield", "Pavise", "AncientShield", "GrimShield", "BarbedShield", "Heater", "Luna", "Hyperion", "Monarch", "Aegis", "Ward", "TrollNest", "BladeBarrier"}},
            {"gloves", new List<string>{"LeatherGloves", "HeavyGloves", "ChainGloves", "LightGauntlets", "Gauntlets", "DemonhideGloves", "SharkskinGloves", "HeavyBracers", "BattleGauntlets", "WarGauntlets", "BrambleMitts", "VampireboneGloves", "Vambraces", "CrusaderGauntlets", "OgreGauntlets"}},
            {"boots", new List<string>{"Boots", "HeavyBoots", "ChainBoots", "LightPlatedBoots", "Greaves", "DemonhideBoots", "SharkskinBoots", "MeshBoots", "BattleBoots", "WarBoots", "WyrmhideBoots", "ScarabshellBoots", "BoneweaveBoots", "MirroredBoots", "MyrmidonGreaves"}},
            {"belt", new List<string>{"Sash", "LightBelt", "Belt", "HeavyBelt", "PlatedBelt", "DemonhideSash", "SharkskinBelt", "MeshBelt", "BattleBelt", "WarBelt", "SpiderwebSash", "VampirefangBelt", "MithrilCoil", "TrollBelt", "ColossusGirdle"}},
            {"circlet", new List<string>{"Circlet", "Coronet", "Tiara", "Diadem"}},
            {"assassinclaw", new List<string>{"Katar", "WristBlade", "HatchetHands", "Cestus", "Claws", "BladeTalons", "ScissorsKatar", "Quhab", "WristSpike", "Fascia", "HandScythe", "GreaterClaws", "GreaterTalons", "ScissorsQuhab", "Suwayyah", "WristSword", "WarFist", "BattleCestus", "FeralClaws", "RunicTalons", "ScissorsSuwayyah"}},
            {"handtohand", new List<string>{"Katar", "WristBlade", "HatchetHands", "Cestus", "Claws", "BladeTalons", "ScissorsKatar", "Quhab", "WristSpike", "Fascia", "HandScythe", "GreaterClaws", "GreaterTalons", "ScissorsQuhab", "Suwayyah", "WristSword", "WarFist", "BattleCestus", "FeralClaws", "RunicTalons", "ScissorsSuwayyah"}},
            {"orb", new List<string>{"EagleOrb", "SacredGlobe", "SmokedSphere", "ClaspedOrb", "JaredsStone", "GlowingOrb", "CrystallineGlobe", "CloudySphere", "SparklingBall", "SwirlingCrystal", "HeavenlyStone", "EldritchOrb", "DemonHeart", "VortexOrb", "DimensionalShard"}},
            {"amazonbow", new List<string>{"StagBow", "ReflexBow", "AshwoodBow", "CeremonialBow", "MatriarchalBow", "GrandMatronBow"}},
            {"amazonspear", new List<string>{"MaidenSpear", "MaidenPike", "CeremonialSpear", "CeremonialPike", "MatriarchalSpear", "MatriarchalPike"}},
            {"amazonjavelin", new List<string>{"MaidenJavelin", "CeremonialJavelin", "MatriarchalJavelin"}},
            {"pelt", new List<string>{"WolfHead", "HawkHelm", "Antlers", "FalconMask", "SpiritMask", "AlphaHelm", "GriffonHeaddress", "HuntersGuise", "SacredFeathers", "TotemicMask", "BloodSpirit", "SunSpirit", "EarthSpirit", "SkySpirit", "DreamSpirit"}},
            {"primalhelm", new List<string>{"JawboneCap", "FangedHelm", "HornedHelm", "AssaultHelmet", "AvengerGuard", "JawboneVisor", "LionHelm", "RageMask", "SavageHelmet", "SlayerGuard", "CarnageHelm", "FuryVisor", "DestroyerHelm", "ConquerorCrown", "GuardianCrown"}},
            {"auricshields", new List<string>{"Targe", "Rondache", "HeraldicShield", "AerinShield", "CrownShield", "AkaranTarge", "AkaranRondache", "ProtectorShield", "GildedShield", "RoyalShield", "SacredTarge", "SacredRondache", "KurastShield", "ZakarumShield", "VortexShield"}},
            {"voodooheads", new List<string>{"PreservedHead", "ZombieHead", "UnravellerHead", "GargoyleHead", "DemonHeadShield", "MummifiedTrophy", "FetishTrophy", "SextonTrophy", "CantorTrophy", "HierophantTrophy", "MinionSkull", "HellspawnSkull", "OverseerSkull", "SuccubusSkull", "BloodlordSkull"}},
            {"runes", new List<string>{"ElRune", "EldRune", "TirRune", "NefRune", "EthRune", "IthRune", "TalRune", "RalRune", "OrtRune", "ThulRune", "AmnRune", "SolRune", "ShaelRune", "DolRune", "HelRune", "IoRune", "LumRune", "KoRune", "FalRune", "LemRune", "PulRune", "UmRune", "MalRune", "IstRune", "GulRune", "VexRune", "OhmRune", "LoRune", "SurRune", "BerRune", "JahRune", "ChamRune", "ZodRune"}},
            {"ubers", new List<string>{"KeyOfTerror", "KeyOfHate", "KeyOfDestruction", "DiablosHorn", "BaalsEye", "MephistosBrain"}},
            {"tokens", new List<string>{"TokenofAbsolution", "TwistedEssenceOfSuffering", "ChargedEssenceOfHatred", "BurningEssenceOfTerror", "FesteringEssenceOfDestruction"}},
            {"chippedgems", new List<string>{"ChippedAmethyst", "ChippedDiamond", "ChippedEmerald", "ChippedRuby", "ChippedSapphire", "ChippedSkull", "ChippedTopaz"}},
            {"flawedgems", new List<string>{"FlawedAmethyst", "FlawedDiamond", "FlawedEmerald", "FlawedRuby", "FlawedSapphire", "FlawedSkull", "FlawedTopaz"}},
            {"gems", new List<string>{"Amethyst", "Diamond", "Emerald", "Ruby", "Skull", "Sapphire", "Topaz"}},
            {"flawlessgems", new List<string>{"FlawlessAmethyst", "FlawlessDiamond", "FlawlessEmerald", "FlawlessRuby", "FlawlessSapphire", "FlawlessSkull", "FlawlessTopaz"}},
            {"perfectgems", new List<string>{"PerfectAmethyst", "PerfectDiamond", "PerfectEmerald", "PerfectRuby", "PerfectSapphire", "PerfectSkull", "PerfectTopaz"}}
        };
    }

    public void CheckItemNames()
    {
        foreach (var ThisDir in PickItemsRunesKeyGems)
        {
            if (ThisDir.Value)
            {
                bool FoundItemName = false;
                string CheckName = ThisDir.Key.Replace(" ", "");

                for (int i = 0; i < 659; i++)
                {
                    if (gameData.itemsNames.getItemBaseName(i).Replace(" ", "") == CheckName)
                    {
                        FoundItemName = true;
                        break;
                    }
                }

                if (!FoundItemName)
                {
                    gameData.method_1("Item '" + ThisDir.Key + "' from the pickit doesn't exist", Color.Red);
                }
            }
        }
        foreach (var ThisDir in PickItemsNormal_ByName)
        {
            //if (ThisDir.Value)
            //{
                bool FoundItemName = false;
                string CheckName = Regex.Replace(ThisDir.Key, @"[\d-]", string.Empty);

                for (int i = 0; i < 659; i++)
                {
                    if (gameData.itemsNames.getItemBaseName(i).Replace(" ", "") == CheckName)
                    {
                        FoundItemName = true;
                        break;
                    }
                }

                if (!FoundItemName)
                {
                    gameData.method_1("Item '" + CheckName + "' from the pickit doesn't exist", Color.Red);
                }
                else
                {
                    if (PickItemsNormal_ByName_Stats.ContainsKey(ThisDir.Key))
                    {
                        foreach (var ThisDir2 in PickItemsNormal_ByName_Stats[ThisDir.Key])
                        {
                            //Console.WriteLine(gameData.itemsStruc.ItemNAAME + ":" + ThisDir2.Key + "=" + ThisDir2.Value);
                            gameData.itemsStruc.IsItemHaveSameStatMultiCheck(ThisDir2.Key, ThisDir2.Value, PickItemsNormal_ByName_Operators[ThisDir.Key][ThisDir2.Key]);
                        }
                    }
                }
            //}
        }
    }

    public void RemoveNotPickingItems()
    {
        return;

        //Remove all the items that are disabled in the Pickit to improve Pickit performance
        List<string> KeysToRemove = new List<string>();
        foreach (var ThisDir in PickItemsRunesKeyGems)
        {
            if (!ThisDir.Value)
            {
                KeysToRemove.Add(ThisDir.Key);
            }
        }
        for (int i = 0; i < KeysToRemove.Count; i++)
        {
            PickItemsRunesKeyGems.Remove(KeysToRemove[i]);
            PickItemsRunesKeyGems_Quantity.Remove(KeysToRemove[i]);
        }

        //#######
        KeysToRemove.Clear();
        foreach (var ThisDir in PickItemsPotions)
        {
            if (!ThisDir.Value)
            {
                KeysToRemove.Add(ThisDir.Key);
            }
        }
        for (int i = 0; i < KeysToRemove.Count; i++)
        {
            PickItemsPotions.Remove(KeysToRemove[i]);
        }

        //#######
        KeysToRemove.Clear();
        foreach (var ThisDir in PickItemsNormal_ByName)
        {
            if (!ThisDir.Value)
            {
                KeysToRemove.Add(ThisDir.Key);
            }
        }
        for (int i = 0; i < KeysToRemove.Count; i++)
        {
            PickItemsNormal_ByName.Remove(KeysToRemove[i]);
            PickItemsNormal_ByName_Flags.Remove(KeysToRemove[i]);
            PickItemsNormal_ByName_Quality.Remove(KeysToRemove[i]);
            PickItemsNormal_ByName_Class.Remove(KeysToRemove[i]);
            PickItemsNormal_ByName_Stats.Remove(KeysToRemove[i]);
            PickItemsNormal_ByName_Operators.Remove(KeysToRemove[i]);
            PickItemsNormal_ByNameDesc.Remove(KeysToRemove[i]);
        }

        //#######
        KeysToRemove.Clear();
        foreach (var ThisDir in PickItemsNormal_ByType)
        {
            if (!ThisDir.Value)
            {
                KeysToRemove.Add(ThisDir.Key);
            }
        }
        for (int i = 0; i < KeysToRemove.Count; i++)
        {
            PickItemsNormal_ByType.Remove(KeysToRemove[i]);
            PickItemsNormal_ByType_Flags.Remove(KeysToRemove[i]);
            PickItemsNormal_ByType_Quality.Remove(KeysToRemove[i]);
            PickItemsNormal_ByType_Class.Remove(KeysToRemove[i]);
            PickItemsNormal_ByType_Stats.Remove(KeysToRemove[i]);
            PickItemsNormal_ByType_Operators.Remove(KeysToRemove[i]);
            PickItemsNormal_ByTypeDesc.Remove(KeysToRemove[i]);
        }
    }

    public bool ShouldKeepItem()
    {
        return ShouldPickItem(true);
        //return PickOrKeepItem(true);
    }

    public bool ShouldPickItem(bool Keeping)
    {
        string ItemName = gameData.itemsStruc.ItemNAAME.Replace(" ", "");
        //foreach (var ThisDir in PickItemsRunesKeyGems)
        if (PickItemsRunesKeyGems.ContainsKey(ItemName))
        {
            //if (ItemName == ThisDir.Key.Replace(" ", "") && ThisDir.Value)
            if (PickItemsRunesKeyGems[ItemName])
            {
                if (PickItemsRunesKeyGems_Quantity[ItemName] == 0 
                    || (PickItemsRunesKeyGems_Quantity[ItemName] > 0 && gameData.stashStruc.GetStashItemCount(gameData.itemsStruc.ItemNAAME) < PickItemsRunesKeyGems_Quantity[ItemName]))
                {
                    return true;
                }
                //break;
            }
        }

        //###############
        //for (int i = 0; i < 20; i++)
        //{
        string ThisNamee = ItemName;
        //if (i > 0) ThisNamee = ItemName + (i + 1);
        //foreach (var ThisDir in PickItemsNormal_ByName)
        int ThisIndex = 2;
        //Console.WriteLine(gameData.itemsStruc.ItemNAAME + ":" + ThisNamee);
        while (PickItemsNormal_ByName.ContainsKey(ThisNamee))
        {
            //if (gameData.itemsStruc.ItemNAAME == "Amulet") Console.WriteLine(gameData.itemsStruc.ItemNAAME + ":" + ThisNamee);
            if (PickItemsNormal_ByName[ThisNamee])
            //if (ItemName == Regex.Replace(ThisDir.Key, @"[\d-]", string.Empty) && ThisDir.Value)
            {
                bool SameQuality = true;
                bool SameFlags = true;
                if (PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                {
                    if (gameData.itemsStruc.quality != gameData.itemsStruc.getQuality(PickItemsNormal_ByName_Quality[ThisNamee])) SameQuality = false;
                }
                if (SameQuality)
                {
                    if (PickItemsNormal_ByName_Flags.ContainsKey(ThisNamee))
                    {
                        uint TotalFlags = 0;
                        foreach (var ThisList in PickItemsNormal_ByName_Flags[ThisNamee])
                        {
                            TotalFlags += ThisList.Key;
                        }
                        foreach (var ThisList in PickItemsNormal_ByName_Flags[ThisNamee])
                        {
                            SameFlags = gameData.itemsFlags.IsItemSameFlags(ThisList.Value, TotalFlags, gameData.itemsStruc.flags);
                        }
                        //Console.WriteLine(gameData.itemsStruc.ItemNAAME + ":" + SameFlags);
                    }

                    if (SameFlags)
                    {
                        if (!gameData.itemsStruc.identified)
                        {
                            //if (!Keeping)
                            //{
                                bool SameStats = true;

                                //Check for sockets stats
                                if (PickItemsNormal_ByName_Stats.ContainsKey(ThisNamee))
                                {
                                    foreach (var ThisDir2 in PickItemsNormal_ByName_Stats[ThisNamee])
                                    {
                                        if (ThisDir2.Key == "Sockets")
                                        {
                                            if (!gameData.itemsStruc.IsItemHaveSameStatMultiCheck(ThisDir2.Key, ThisDir2.Value, PickItemsNormal_ByName_Operators[ThisNamee][ThisDir2.Key])) SameStats = false;
                                            break;
                                        }
                                    }
                                }

                                if (SameStats) return true;
                            //}
                        }
                        else
                        {
                            bool SameStats = true;
                            if (PickItemsNormal_ByName_Stats.ContainsKey(ThisNamee))
                            {
                                foreach (var ThisDir2 in PickItemsNormal_ByName_Stats[ThisNamee])
                                {
                                    //Console.WriteLine(gameData.itemsStruc.ItemNAAME + ":" + ThisDir2.Key + "=" + ThisDir2.Value);
                                    if (!gameData.itemsStruc.IsItemHaveSameStatMultiCheck(ThisDir2.Key, ThisDir2.Value, PickItemsNormal_ByName_Operators[ThisNamee][ThisDir2.Key])) SameStats = false;
                                }
                            }

                            /*if (gameData.itemsStruc.ItemNAAME == "Amulet")
                            {
                                if (PickItemsNormal_ByNameDesc.ContainsKey(ThisNamee))
                                {
                                    if (PickItemsNormal_ByNameDesc[ThisNamee].Contains("Kaleidoscope"))
                                    {
                                        Console.WriteLine(gameData.itemsStruc.ItemNAAME + ":" + SameStats);
                                    }
                                }
                            }*/
                            //Console.WriteLine("---------------------");

                            if (SameStats) return true;
                        }
                    }
                }

                //break;
            }

            ThisNamee = ItemName + ThisIndex;
            ThisIndex++;
        }
        //}
        //###############
        foreach (var ThisDir in PickItemsNormal_ByType)
        {
            if (IsItemThisType(Regex.Replace(ThisDir.Key, @"[\d-]", string.Empty)) && ThisDir.Value)
            {
                bool SameQuality = true;
                bool SameFlags = true;

                bool Checking_identified = false;
                bool Checking_isSocketed = false;
                bool Checking_ethereal = false;
                if (PickItemsNormal_ByType_Quality.ContainsKey(ThisDir.Key))
                {
                    if (gameData.itemsStruc.quality != gameData.itemsStruc.getQuality(PickItemsNormal_ByType_Quality[ThisDir.Key])) SameQuality = false;
                }
                if (SameQuality)
                {
                    if (PickItemsNormal_ByType_Flags.ContainsKey(ThisDir.Key))
                    {
                        uint TotalFlags = 0;
                        foreach (var ThisList in PickItemsNormal_ByType_Flags[ThisDir.Key])
                        {
                            TotalFlags += ThisList.Key;
                            /*if ((0x00000010 & ThisList.Key) != 0) Checking_identified = true;
                            if ((0x00000800 & ThisList.Key) != 0) Checking_isSocketed = true;
                            if ((0x00400000 & ThisList.Key) != 0) Checking_ethereal = true;*/
                        }
                        foreach (var ThisList in PickItemsNormal_ByType_Flags[ThisDir.Key])
                        {
                            SameFlags = gameData.itemsFlags.IsItemSameFlags(ThisList.Value, TotalFlags, gameData.itemsStruc.flags);
                        }
                        //Console.WriteLine(gameData.itemsStruc.ItemNAAME + ":" + SameFlags);
                    }

                    if (SameFlags)
                    //{
                        if (!gameData.itemsStruc.identified)
                        {
                            //if (!Keeping)
                            //{
                                bool SameStats = true;

                                //Check for sockets stats
                                if (PickItemsNormal_ByType_Stats.ContainsKey(ThisNamee))
                                {
                                    foreach (var ThisDir2 in PickItemsNormal_ByType_Stats[ThisNamee])
                                    {
                                        if (ThisDir2.Key == "Sockets")
                                        {
                                            if (!gameData.itemsStruc.IsItemHaveSameStatMultiCheck(ThisDir2.Key, ThisDir2.Value, PickItemsNormal_ByType_Operators[ThisNamee][ThisDir2.Key])) SameStats = false;
                                            break;
                                        }
                                    }
                                }

                                if (SameStats) return true;
                            //}
                        }
                        else
                        {
                            bool SameStats = true;
                            if (PickItemsNormal_ByType_Stats.ContainsKey(ThisDir.Key))
                            {
                                foreach (var ThisDir2 in PickItemsNormal_ByType_Stats[ThisDir.Key])
                                {
                                    if (!gameData.itemsStruc.IsItemHaveSameStatMultiCheck(ThisDir2.Key, ThisDir2.Value, PickItemsNormal_ByType_Operators[ThisDir.Key][ThisDir2.Key])) SameStats = false;
                                }
                            }

                            if (SameStats) return true;
                        }
                    //}
                }

                //break;
            }
        }
        //###############

        return false;
        //return PickOrKeepItem(false);
    }

    public bool IsItemThisType(string ItemTypee)
    {
        if (typeMapping.ContainsKey(ItemTypee))
        {
            for (int i = 0; i < typeMapping[ItemTypee].Count; i++)
            {
                if (typeMapping[ItemTypee][i] == gameData.itemsStruc.ItemNAAME.Replace(" ", ""))
                {
                    return true;
                }
            }
        }
        else
        {
            gameData.method_1("Item '[Type] == " + ItemTypee + "' doesn't exist!", Color.Red);
            return false;
        }

        /*if (ItemTypee == "helm")
        {
            if (gameData.itemsStruc.ItemNAAME.Contains("Mask")
                || gameData.itemsStruc.ItemNAAME.Contains("Helm")
                || gameData.itemsStruc.ItemNAAME.Contains("Crown")
                || gameData.itemsStruc.ItemNAAME == "Demonhead"
                || gameData.itemsStruc.ItemNAAME == "Cap"
                || gameData.itemsStruc.ItemNAAME == "Basinet"
                || gameData.itemsStruc.ItemNAAME == "Bone Visage"
                || gameData.itemsStruc.ItemNAAME == "Shako"
                || gameData.itemsStruc.ItemNAAME == "War Hat"
                || gameData.itemsStruc.ItemNAAME == "Sallet"
                || gameData.itemsStruc.ItemNAAME == "Casque"
                || gameData.itemsStruc.ItemNAAME == "Armet"
                || gameData.itemsStruc.ItemNAAME == "Skull Cap"
                || gameData.itemsStruc.ItemNAAME == "Hydraskull"
                || gameData.itemsStruc.ItemNAAME == "Giant Conch"
                || gameData.itemsStruc.ItemNAAME == "Diadem"
                || gameData.itemsStruc.ItemNAAME == "Tiara"
                || gameData.itemsStruc.ItemNAAME == "Circlet")
            {
                return true;
            }
        }
        if (ItemTypee == "gloves")
        {
            if (gameData.itemsStruc.ItemNAAME.Contains("Gloves")
                || gameData.itemsStruc.ItemNAAME.Contains("Gauntlets")
                || gameData.itemsStruc.ItemNAAME == "Heavy Bracers"
                || gameData.itemsStruc.ItemNAAME == "Vambraces"
                || gameData.itemsStruc.ItemNAAME == "Bramble Mitts")
            {
                return true;
            }
        }
        if (ItemTypee == "boots")
        {
            if (gameData.itemsStruc.ItemNAAME.Contains("Boots")
                || gameData.itemsStruc.ItemNAAME.Contains("Greaves"))
            {
                return true;
            }
        }
        if (ItemTypee == "belt")
        {
            if (gameData.itemsStruc.ItemNAAME.Contains("Belt")
                || gameData.itemsStruc.ItemNAAME.Contains("Sash")
                || gameData.itemsStruc.ItemNAAME == "Mithril Coil"
                || gameData.itemsStruc.ItemNAAME == "Colossus Girdle")
            {
                return true;
            }
        }
        if (ItemTypee == "ring")
        {
            if (gameData.itemsStruc.ItemNAAME == "Ring") return true;
        }
        if (ItemTypee == "amulet")
        {
            if (gameData.itemsStruc.ItemNAAME == "Amulet") return true;
        }
        if (ItemTypee == "armor")
        {
            try
            {
                if (gameData.itemsStruc.ItemNAAME.Contains("Plate")
                    || gameData.itemsStruc.ItemNAAME.Contains("Armor")
                    || gameData.itemsStruc.ItemNAAME.Contains("Skin")
                    || gameData.itemsStruc.ItemNAAME.Contains("Mail")
                    || gameData.itemsStruc.ItemNAAME.Contains("Coat")
                    || gameData.itemsStruc.ItemNAAME.Contains("Shell")
                    || gameData.itemsStruc.ItemNAAME == "Cuirass"
                    || gameData.itemsStruc.ItemNAAME == "Dusk Shroud"
                    || gameData.itemsStruc.ItemNAAME == "Wire Fleece"
                    || gameData.itemsStruc.ItemNAAME == "Studded Leather"
                    || gameData.itemsStruc.ItemNAAME == "Great Hauberk"
                    || gameData.itemsStruc.ItemNAAME == "Boneweave"
                    || gameData.itemsStruc.ItemNAAME == "Wyrmhide"
                    || gameData.itemsStruc.ItemNAAME == "Scarab Husk"
                    || gameData.itemsStruc.ItemNAAME == "Boneweave")
                {
                    return true;
                }
            }
            catch { }
        }
        if (ItemTypee == "circlet")
        {
            if (gameData.itemsStruc.ItemNAAME == "Circlet"
                || gameData.itemsStruc.ItemNAAME == "Tiara"
                || gameData.itemsStruc.ItemNAAME == "Diadem")
            {
                return true;
            }
        }
        if (ItemTypee == "gold")
        {
            if (gameData.itemsStruc.ItemNAAME == "Gold") return true;
        }
        if (ItemTypee == "jewel")
        {
            if (gameData.itemsStruc.ItemNAAME == "Jewel") return true;
        }*/
        return false;
    }

    public string GetItemTypeText()
    {
        if (GetItemType() != "") return "[Type] == " + GetItemType();
        return "";
    }

    public string GetItemType()
    {
        foreach (var ThisTypee in typeMapping)
        {
            for (int i = 0; i < typeMapping[ThisTypee.Key].Count; i++)
            {
                if (typeMapping[ThisTypee.Key][i] == gameData.itemsStruc.ItemNAAME.Replace(" ", ""))
                {
                    return ThisTypee.Key;
                }
            }
        }
        //gameData.method_1("Couldn't get the Item Type for: " + gameData.itemsStruc.ItemNAAME, Color.Red);
        return "";

        /*if (gameData.itemsStruc.ItemNAAME.Contains("Mask")
            || gameData.itemsStruc.ItemNAAME.Contains("Helm")
            || gameData.itemsStruc.ItemNAAME.Contains("Crown")
            || gameData.itemsStruc.ItemNAAME == "Demonhead"
            || gameData.itemsStruc.ItemNAAME == "Cap"
            || gameData.itemsStruc.ItemNAAME == "Basinet"
            || gameData.itemsStruc.ItemNAAME == "Bone Visage"
            || gameData.itemsStruc.ItemNAAME == "Shako"
            || gameData.itemsStruc.ItemNAAME == "War Hat"
            || gameData.itemsStruc.ItemNAAME == "Sallet"
            || gameData.itemsStruc.ItemNAAME == "Casque"
            || gameData.itemsStruc.ItemNAAME == "Armet"
            || gameData.itemsStruc.ItemNAAME == "Skull Cap"
            || gameData.itemsStruc.ItemNAAME == "Hydraskull"
            || gameData.itemsStruc.ItemNAAME == "Giant Conch"
            || gameData.itemsStruc.ItemNAAME == "Diadem"
            || gameData.itemsStruc.ItemNAAME == "Tiara"
            || gameData.itemsStruc.ItemNAAME == "Circlet")
        {
            return "helm";
        }
        if (gameData.itemsStruc.ItemNAAME.Contains("Gloves")
            || gameData.itemsStruc.ItemNAAME.Contains("Gauntlets")
            || gameData.itemsStruc.ItemNAAME == "Heavy Bracers"
            || gameData.itemsStruc.ItemNAAME == "Vambraces"
            || gameData.itemsStruc.ItemNAAME == "Bramble Mitts")
        {
            return "gloves";
        }
        if (gameData.itemsStruc.ItemNAAME.Contains("Boots")
            || gameData.itemsStruc.ItemNAAME.Contains("Greaves"))
        {
            return "boots";
        }
        if (gameData.itemsStruc.ItemNAAME.Contains("Belt")
            || gameData.itemsStruc.ItemNAAME.Contains("Sash")
            || gameData.itemsStruc.ItemNAAME == "Mithril Coil"
            || gameData.itemsStruc.ItemNAAME == "Colossus Girdle")
        {
            return "belt";
        }
        if (gameData.itemsStruc.ItemNAAME == "Ring") return "ring";
        if (gameData.itemsStruc.ItemNAAME == "Amulet") return "amulet";
        try
        {
            if (gameData.itemsStruc.ItemNAAME.Contains("Plate")
                || gameData.itemsStruc.ItemNAAME.Contains("Armor")
                || gameData.itemsStruc.ItemNAAME.Contains("Skin")
                || gameData.itemsStruc.ItemNAAME.Contains("Mail")
                || gameData.itemsStruc.ItemNAAME.Contains("Coat")
                || gameData.itemsStruc.ItemNAAME.Contains("Shell")
                || gameData.itemsStruc.ItemNAAME == "Cuirass"
                || gameData.itemsStruc.ItemNAAME == "Dusk Shroud"
                || gameData.itemsStruc.ItemNAAME == "Wire Fleece"
                || gameData.itemsStruc.ItemNAAME == "Studded Leather"
                || gameData.itemsStruc.ItemNAAME == "Great Hauberk"
                || gameData.itemsStruc.ItemNAAME == "Boneweave"
                || gameData.itemsStruc.ItemNAAME == "Wyrmhide"
                || gameData.itemsStruc.ItemNAAME == "Scarab Husk"
                || gameData.itemsStruc.ItemNAAME == "Boneweave")
            {
                return "armor";
            }
        }
        catch { }
        if (gameData.itemsStruc.ItemNAAME == "Circlet"
            || gameData.itemsStruc.ItemNAAME == "Tiara"
            || gameData.itemsStruc.ItemNAAME == "Diadem")
        {
            return "circlet";
        }
        if (gameData.itemsStruc.ItemNAAME == "Gold") return "gold";
        if (gameData.itemsStruc.ItemNAAME == "Jewel") return "jewel";
        return "";*/
    }
}
