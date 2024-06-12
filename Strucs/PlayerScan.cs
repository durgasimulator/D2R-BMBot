﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static Enums;
using static EnumsStates;
using static MapAreaStruc;
using static System.Runtime.InteropServices.ComTypes.IStream;
using static System.Windows.Forms.AxHost;

public class PlayerScan
{
    private GameData gameData;

    public long PlayerPointer = 0;
    public long PlayerNamePointer = 0;
    public long actAddress = 0;
    public long mapSeedAddress = 0;
    public Int64 pathAddress = 0;
    public bool FoundPlayer = false;
    public bool PlayerDead = false;

    public long pAct = 0;
    public uint mapSeedValue = 0;
    public ushort xPos = 0;
    public ushort yPos = 0;
    public ushort xPosOffset = 0;
    public ushort yPosOffset = 0;
    public int xPosOffsetPercent = 0;
    public int yPosOffsetPercent = 0;
    public ushort xPosFinal = 0;
    public ushort yPosFinal = 0;
    public string pName = "";
    public long pRoom1Address = 0;
    public long pRoom2Address = 0;
    public long pLevelAddress = 0;
    public long levelNo = 0;
    public long LastlevelNo = 0;
    public long pStatsListEx = 0;
    public long statPtr = 0;
    public long statCount = 0;

    public Enums.PlayerClass CurrentPlayerClass;
    public Enums.Skill LeftSkill;
    public Enums.Skill RightSkill;

    public long PlayerGoldInventory = 0;
    public long PlayerGoldInStash = 0;
    public long PlayerHP = 0;
    public long PlayerMaxHP = 0;
    public long PlayerMana = 0;
    public long PlayerMaxMana = 0;
    public ushort difficulty = 0;
    public uint lastdwInitSeedHash1 = 0;
    public uint lastdwInitSeedHash2 = 0;
    public string sFile = "";
    public int PlayerSkillLeft = 0;
    public int PlayerSkillRight = 0;

    public int PlayerStrucCount = 0;
    public uint unitId = 0;
    public uint LeechPlayerUnitID = 0;
    public long LeechPlayerPointer = 0;
    public int LeechPosX = 0;
    public int LeechPosY = 0;
    public long LeechlevelNo = 0;

    public ushort xPosFinalOtherP = 0;
    public ushort yPosFinalOtherP = 0;
    public bool IsCorpse = false;
    public string pNameOther = "";
    public uint unitIdOther = 0;

    public int HPFromEquippedItems = 0;
    public int ManaFromEquippedItems = 0;
    public int VitalityFromEquippedItems = 0;
    public int EnergyFromEquippedItems = 0;
    public int HPPercentFromEquippedItems = 0;
    public int ManaPercentFromEquippedItems = 0;

    public long HPFromPlayer = 0;
    public long ManaFromPlayer = 0;
    public long VitalityFromPlayer = 0;
    public long EnergyFromPlayer = 0;
    public long HPPercentFromPlayer = 0;
    public long ManaPercentFromPlayer = 0;

    public bool PrintedLeechFoundInfo = false;
    public bool HasBattleOrderState = false;
    public long MaxHPValueWithBO = 0;
    public long MaxManaValueWithBO = 0;
    List<EnumsStates.State> PlayerStates = new List<EnumsStates.State>();

    public long LastPlayerHP = 0;

    public int[] RoomExit = new int[2];

    public DateTime TimeSinceSameHP = DateTime.MaxValue;
    public bool TimeSinceSameHPSet = false;

    // REQUIRED METHODS
    //[DllImport("checkmem.dll")]
    //public static extern uint get_seed(uint InitSeedHash1, uint InitSeedHash2, uint EndSeedHash1);
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public void GetPositions()
    {
        gameData.form.SetProcessingTime();


        long QuestOffset = (long)gameData.BaseAddress + 0x230E9A8;
        long QuestAddress = gameData.mem.ReadInt64Raw((IntPtr)(QuestOffset));
        byte Q1 = gameData.mem.ReadByteRaw((IntPtr)QuestAddress);
        byte Q2 = gameData.mem.ReadByteRaw((IntPtr)(QuestAddress + 1));

        //byte[] Buffer = new byte[2] { 0, 0};
        //gameData.mem.WriteRawMemory((IntPtr)QuestAddress, Buffer, Buffer.Length);

        pathAddress = gameData.mem.ReadInt64Raw((IntPtr)(PlayerPointer + 0x38));
        xPos = gameData.mem.ReadUInt16Raw((IntPtr)(pathAddress + 0x02));
        yPos = gameData.mem.ReadUInt16Raw((IntPtr)(pathAddress + 0x06));
        xPosOffset = gameData.mem.ReadUInt16Raw((IntPtr)(pathAddress + 0x00));
        yPosOffset = gameData.mem.ReadUInt16Raw((IntPtr)(pathAddress + 0x04));
        xPosOffsetPercent = (xPosOffset / 65536); //get percentage
        yPosOffsetPercent = (yPosOffset / 65536); //get percentage
        xPosFinal = (ushort)(xPos + xPosOffsetPercent);
        yPosFinal = (ushort)(yPos + yPosOffsetPercent);

        //long UnitID = gameData.mem.ReadInt64Raw((IntPtr)(PlayerPointer + 8));
        //Console.WriteLine("PlayerID: 0x" + UnitID.ToString("X"));

        //Console.WriteLine("X: " + xPos);
        //Console.WriteLine("Off: " + gameData.mem.ReadByteRaw((IntPtr)(pathAddress + 0x02)).ToString("X2"));
        //Console.WriteLine("Off: " + gameData.mem.ReadByteRaw((IntPtr)(pathAddress + 0x03)).ToString("X2"));
        //Console.WriteLine("Off: " + gameData.mem.ReadByteRaw((IntPtr)(pathAddress + 0x04)).ToString("X2"));
        //Console.WriteLine("Off: " + gameData.mem.ReadByteRaw((IntPtr)(pathAddress + 0x05)).ToString("X2"));
        //byte[] bytee = new byte[2] { 0x76, 0x1e  };
        //gameData.mem.WriteRawMemory((IntPtr)(pathAddress + 0x02), bytee, bytee.Length);

        pStatsListEx = gameData.mem.ReadInt64Raw((IntPtr)(PlayerPointer + 0x88));
        statPtr = gameData.mem.ReadInt64Raw((IntPtr)(pStatsListEx + 0x30));
        statCount = gameData.mem.ReadInt32Raw((IntPtr)(pStatsListEx + 0x38));

        PlayerStates = GetStates(pStatsListEx);
        HasBattleOrderState = HasState(EnumsStates.State.Battleorders, PlayerStates);

        byte[] buffer = new byte[statCount * 8];
        gameData.mem.ReadRawMemory(statPtr + 0x2, ref buffer, (int)(statCount * 8));

        if (gameData.townStruc.IsInTown)
        {
            PlayerHP = PlayerMaxHP;
            if (PlayerHP == 0) PlayerHP = 100;
        }
        else
        {
            //reset player hp
            PlayerHP = 0;

        }

        if (!gameData.gameStruc.IsInGame()) PlayerHP = PlayerMaxHP;

        for (int i = 0; i < statCount; i++)
        {
            int offset = i * 8;
            ushort statEnum = BitConverter.ToUInt16(buffer, offset);
            uint statValue = BitConverter.ToUInt32(buffer, offset + 0x2);

            //Console.WriteLine((Enums.Attribute) statEnum + " - " + statValue);
            /*if (statEnum == (ushort)Enums.Attribute.AttackRate 
                || statEnum == (ushort)Enums.Attribute.OtherAnimRate 
                || statEnum == (ushort)Enums.Attribute.VelocityPercent
                || statEnum == (ushort)Enums.Attribute.FireResist
                || statEnum == (ushort)Enums.Attribute.LightningResist
                || statEnum == (ushort)Enums.Attribute.ColdResist
                || statEnum == (ushort)Enums.Attribute.PoisonResist)
                //|| statEnum == (ushort)Enums.Attribute.SkillPointsRemaining)
            {
                //Console.WriteLine(buffer[offset + 0x2].ToString("X2") + buffer[offset + 0x2 + 1].ToString("X2") + buffer[offset + 0x2 + 2].ToString("X2") + buffer[offset + 0x2 + 3].ToString("X2"));
                //byte[] bytee = new byte[4] { (byte) (buffer[offset + 0x2] + 0x03), buffer[offset + 0x2 + 1], buffer[offset + 0x2 + 2], buffer[offset + 0x2 + 3] };
                byte[] bytee = new byte[4] { 0xff, buffer[offset + 0x2 + 1], buffer[offset + 0x2 + 2], buffer[offset + 0x2 + 3] };
                gameData.mem.WriteRawMemory((IntPtr)(statPtr + 0x2 + offset + 0x02), bytee, bytee.Length);
            }
            if (statEnum == (ushort)Enums.Attribute.Experience)
            //|| statEnum == (ushort)Enums.Attribute.SkillPointsRemaining)
            {
                //Console.WriteLine(buffer[offset + 0x2].ToString("X2") + buffer[offset + 0x2 + 1].ToString("X2") + buffer[offset + 0x2 + 2].ToString("X2") + buffer[offset + 0x2 + 3].ToString("X2"));
                //byte[] bytee = new byte[4] { (byte) (buffer[offset + 0x2] + 0x03), buffer[offset + 0x2 + 1], buffer[offset + 0x2 + 2], buffer[offset + 0x2 + 3] };
                byte[] bytee = new byte[4] { buffer[offset + 0x2], (byte) (buffer[offset + 0x2 + 1] + 0x05), buffer[offset + 0x2 + 2], buffer[offset + 0x2 + 3] };
                gameData.mem.WriteRawMemory((IntPtr)(statPtr + 0x2 + offset + 0x02), bytee, bytee.Length);
            }
            if (statEnum == (ushort)Enums.Attribute.Vitality || statEnum == (ushort)Enums.Attribute.Energy)
            {
                //Console.WriteLine(buffer[offset + 0x2].ToString("X2") + buffer[offset + 0x2 + 1].ToString("X2") + buffer[offset + 0x2 + 2].ToString("X2") + buffer[offset + 0x2 + 3].ToString("X2"));
                //byte[] bytee = new byte[4] { (byte) (buffer[offset + 0x2] + 0x03), buffer[offset + 0x2 + 1], buffer[offset + 0x2 + 2], buffer[offset + 0x2 + 3] };
                byte[] bytee = new byte[4] { 0xff, 0xff, buffer[offset + 0x2 + 2], buffer[offset + 0x2 + 3] };
                gameData.mem.WriteRawMemory((IntPtr)(statPtr + 0x2 + offset + 0x02), bytee, bytee.Length);
            }*/

            if (statEnum == (ushort)Enums.Attribute.Life)
            {
                PlayerHP = statValue >> 8;
            }
            if (statEnum == (ushort)Enums.Attribute.Mana)
            {
                PlayerMana = statValue >> 8;
            }
            if (statEnum == (ushort)Enums.Attribute.GoldInPlayer)
            {
                PlayerGoldInventory = statValue;
            }
            if (statEnum == (ushort)Enums.Attribute.GoldInStash)
            {
                PlayerGoldInStash = statValue;
            }
            //#####
            /*if (statEnum == (ushort)Enums.Attribute.Vitality)
            {
                VitalityFromPlayer = statValue;
                HPFromPlayer = statValue;
            }
            if (statEnum == (ushort)Enums.Attribute.LifeMax)
            {
                HPFromPlayer = (statValue >> 8);
            }
            if (statEnum == (ushort)Enums.Attribute.MaxHPPercent)
            {
                HPPercentFromPlayer = statValue;
            }
            if (statEnum == (ushort)Enums.Attribute.Energy)
            {
                EnergyFromPlayer = statValue;
                ManaFromPlayer = statValue;
            }
            if (statEnum == (ushort)Enums.Attribute.ManaMax)
            {
                ManaFromPlayer = (statValue >> 8);
            }
            if (statEnum == (ushort)Enums.Attribute.MaxManaPercent)
            {
                ManaPercentFromPlayer = statValue;
            }*/
            //#####
        }

        //Console.WriteLine("gold: " + PlayerGoldInventory);

        if (PlayerHP == 0) PlayerDead = true;
        else PlayerDead = false;

        //; get the level number
        pRoom1Address = gameData.mem.ReadInt64Raw((IntPtr)(pathAddress + 0x20));
        pRoom2Address = gameData.mem.ReadInt64Raw((IntPtr)(pRoom1Address + 0x18));
        pLevelAddress = gameData.mem.ReadInt64Raw((IntPtr)(pRoom2Address + 0x90));
        levelNo = gameData.mem.ReadUInt32Raw((IntPtr)(pLevelAddress + 0x1F8));

        RoomExit[0] = gameData.mem.ReadUInt16Raw((IntPtr)(pRoom1Address + 0x10));
        RoomExit[1] = gameData.mem.ReadUInt16Raw((IntPtr)(pRoom1Address + 0x14));

        if (LastlevelNo != levelNo)
        {
            gameData.overlayForm.ScanningOverlayItems = true; //try rescanning overlay if there was too much lags
            LastlevelNo = levelNo;
        }

        //; get/check for bad pointer
        if (levelNo == 0 && xPosFinal == 0 && yPosFinal == 0)
        {
            gameData.HasPointers = false;
        }

        //#####################################################################################################
        //#####################################################################################################
        //#####################################################################################################
        //; get the difficulty
        actAddress = gameData.mem.ReadInt64Raw((IntPtr)(PlayerPointer + 0x20));
        long aActUnk2 = gameData.mem.ReadInt64Raw((IntPtr)(actAddress + 0x78));
        difficulty = gameData.mem.ReadUInt16Raw((IntPtr)(aActUnk2 + 0x830));

        //; get the map seed
        long actMiscAddress = gameData.mem.ReadInt64Raw((IntPtr)(actAddress + 0x78)); //0x0000023a64ed4780; 2449824630656
        uint dwInitSeedHash1 = gameData.mem.ReadUInt32Raw((IntPtr)(actMiscAddress + 0x840));
        uint dwInitSeedHash2 = gameData.mem.ReadUInt32Raw((IntPtr)(actMiscAddress + 0x844));
        uint dwEndSeedHash1 = gameData.mem.ReadUInt32Raw((IntPtr)(actMiscAddress + 0x868));

        /*byte[] buffData = new byte[0x100];
        gameData.mem.ReadRawMemory(actMiscAddress + 0x800, ref buffData, buffData.Length);
        string SavePathh = gameData.ThisEndPath + "DumpHashStruc";
        File.Create(SavePathh).Dispose();
        File.WriteAllBytes(SavePathh, buffData);*/

        var mapSeed = GetMapSeed((uint)dwInitSeedHash1, (uint)dwEndSeedHash1);
        if (!mapSeed.Item2)
        {
            throw new Exception("Error calculating map seed");
        }


        mapSeedValue = mapSeed.Item1;

        //gameData.method_1("SEED: " + mapSeed.Item1.ToString(), Color.Red);
        //gameData.method_1("Difficulty: " + ((Difficulty) difficulty).ToString(), Color.Red);
        //gameData.GetMapData(mapSeed.Item1.ToString(), (Difficulty) difficulty);
        //#####################################################################################################
        //#####################################################################################################
        //#####################################################################################################
        // Skills
        long skillListPtr = gameData.mem.ReadInt64Raw((IntPtr)(PlayerPointer + 0x100));
        //var skills = GetSkills(skillListPtr);

        long leftSkillPtr = gameData.mem.ReadInt64Raw((IntPtr)(skillListPtr + 0x08));
        long leftSkillTxtPtr = gameData.mem.ReadInt64Raw((IntPtr)leftSkillPtr);
        ushort leftSkillId = gameData.mem.ReadUInt16Raw((IntPtr)leftSkillTxtPtr);
        LeftSkill = (Enums.Skill)leftSkillId;

        long rightSkillPtr = gameData.mem.ReadInt64Raw((IntPtr)(skillListPtr + 0x10));
        long rightSkillTxtPtr = gameData.mem.ReadInt64Raw((IntPtr)rightSkillPtr);
        ushort rightSkillId = gameData.mem.ReadUInt16Raw((IntPtr)rightSkillTxtPtr);
        RightSkill = (Enums.Skill)rightSkillId;

        // Class
        uint classValue = gameData.mem.ReadUInt32Raw((IntPtr)(PlayerPointer + 0x174));
        CurrentPlayerClass = (Enums.PlayerClass)classValue;
        //#####################################################################################################
        //#####################################################################################################
        //#####################################################################################################

        //get player name
        PlayerNamePointer = gameData.mem.ReadInt64Raw((IntPtr)(PlayerPointer + 0x10));
        pName = gameData.mem.ReadMemString(PlayerNamePointer);
        //gameData.potions.CheckHPAndManaMax(); //not used here

        SetMaxHPAndMana();

        gameData.form.Grid_SetInfos("Cords", xPosFinal + "," + yPosFinal);
        gameData.form.Grid_SetInfos("Life", PlayerHP + "/" + PlayerMaxHP);
        gameData.form.Grid_SetInfos("Mana", PlayerMana + "/" + PlayerMaxMana);
        gameData.form.Grid_SetInfos("Map Level", levelNo.ToString() + " " + (Enums.Area)levelNo);
        //gameData.Grid_SetInfos("Room Exit", RoomExit[0].ToString() + ", " + RoomExit[1].ToString());
        //gameData.Grid_SetInfos("Seed", mapSeed.ToString());
        //gameData.Grid_SetInfos("Difficulty", difficulty.ToString());

        //gameData.method_1("URL: " + GetImageURL());
        //DownloadImage(GetImageURL());
    }

    private const int MapHashDivisor = 1 << 16;

    // Logic stolen from MapAssist, credits to them
    public static (uint, bool) GetMapSeed(uint initHashSeed, uint endHashSeed)
    {
        uint gameSeedXor = 0;
        var (seed, found) = ReverseMapSeedHash(endHashSeed);
        if (found)
        {
            gameSeedXor = initHashSeed ^ seed;
        }

        if (gameSeedXor == 0)
        {
            return (0, false);
        }

        return (seed, true);
    }

    private static (uint, bool) ReverseMapSeedHash(uint hash)
    {
        uint incrementalValue = 1;

        for (uint startValue = 0; startValue < uint.MaxValue; startValue += incrementalValue)
        {
            uint seedResult = (startValue * 0x6AC690C5 + 666) & 0xFFFFFFFF;

            if (seedResult == hash)
            {
                return (startValue, true);
            }

            if (incrementalValue == 1 && (seedResult % MapHashDivisor) == (hash % MapHashDivisor))
            {
                incrementalValue = (uint)MapHashDivisor;
            }
        }

        return (0, false);
    }

    public bool ShouldSeeShopForHP()
    {
        if (((gameData.playerScan.PlayerHP * 100) / gameData.playerScan.PlayerMaxHP) <= 80)
        {
            return true;
        }
        return false;
    }

    public void SetMaxHPAndMana()
    {
        byte[] buffer = new byte[statCount * 8];
        gameData.mem.ReadRawMemory(statPtr + 0x2, ref buffer, (int)(statCount * 8));

        for (int i = 0; i < statCount; i++)
        {
            int offset = i * 8;
            ushort statEnum = BitConverter.ToUInt16(buffer, offset);
            uint statValue = BitConverter.ToUInt32(buffer, offset + 0x2);

            if (statEnum == (ushort)Enums.Attribute.LifeMax)
            {
                PlayerMaxHP = (statValue >> 8);
            }
            if (statEnum == (ushort)Enums.Attribute.ManaMax)
            {
                PlayerMaxMana = (statValue >> 8);
            }
        }

        //Console.WriteLine("added mana percent: " + ManaPercentFromEquippedItems);
        double PercentHPAdd = Math.Ceiling(((double)HPPercentFromEquippedItems * (double)PlayerMaxHP) / 100);
        double PercentManaAdd = Math.Ceiling(((double)ManaPercentFromEquippedItems * (double)PlayerMaxMana) / 100);
        PlayerMaxHP = PlayerMaxHP + HPFromEquippedItems + (VitalityFromEquippedItems * 2) + (int)PercentHPAdd;         //might be only for paladin
        PlayerMaxMana = PlayerMaxMana + ManaFromEquippedItems + (EnergyFromEquippedItems / 2) + (int)PercentManaAdd;   //might be only for paladin


        /*double PercentHPAdd2 = Math.Ceiling(((double)HPPercentFromPlayer * (double)PlayerMaxHP) / 100);
        PlayerMaxHP = PlayerMaxHP + (int)PercentHPAdd2;

        double PercentManaAdd2 = Math.Ceiling(((double)ManaPercentFromPlayer * (double)PlayerMaxMana) / 100);
        PlayerMaxMana = PlayerMaxMana + (int)PercentManaAdd2;*/

        if (PlayerHP > PlayerMaxHP) PlayerMaxHP = PlayerHP;
        if (PlayerMana > PlayerMaxMana) PlayerMaxMana = PlayerMana;

        //TimeSinceSameHP = DateTime.MaxValue;
        //Set Max HP With BattleOrder State
        if (HasBattleOrderState)
        {
            //#####
            //Check if Max HP/Mana remain the same, that BO 'power' level didn't changed
            if (PlayerHP == LastPlayerHP)
            {
                if (!TimeSinceSameHPSet)
                {
                    TimeSinceSameHP = DateTime.Now;
                    TimeSinceSameHPSet = true;
                }
                if ((DateTime.Now - TimeSinceSameHP).TotalSeconds > CharConfig.PlayerMaxHPCheckDelay && MaxHPValueWithBO != PlayerMaxHP)
                {
                    MaxHPValueWithBO = PlayerMaxHP;
                    MaxManaValueWithBO = PlayerMaxMana;

                    PlayerMaxHP = MaxHPValueWithBO;
                    PlayerMaxMana = MaxManaValueWithBO;

                    //TimeSinceSameHP = DateTime.Now;
                    TimeSinceSameHP = DateTime.MaxValue;
                    TimeSinceSameHPSet = false;
                }
            }
            else
            {
                TimeSinceSameHP = DateTime.MaxValue;
                TimeSinceSameHPSet = false;
                LastPlayerHP = PlayerHP;
            }
            //#####

            if (MaxHPValueWithBO < PlayerMaxHP)
            {
                MaxHPValueWithBO = PlayerMaxHP;
            }
            else
            {
                PlayerMaxHP = MaxHPValueWithBO;
            }
        }
        else
        {
            MaxHPValueWithBO = 0;
        }

        //Set Max Mana With BattleOrder State
        if (HasBattleOrderState)
        {
            if (MaxManaValueWithBO < PlayerMaxMana)
            {
                MaxManaValueWithBO = PlayerMaxMana;
            }
            else
            {
                PlayerMaxMana = MaxManaValueWithBO;
            }
        }
        else
        {
            MaxManaValueWithBO = 0;
        }
    }

    public bool HasAnyPlayerInArea(int ThisArea)
    {
        long UnitOffset = (long)gameData.BaseAddress + (long)gameData.offsets["rosterOffset"];
        long partyStruct = gameData.mem.ReadInt64Raw((IntPtr)UnitOffset);

        for (int i = 0; i < 8; i++)
        {
            ushort RosterlevelNo = gameData.mem.ReadUInt16Raw((IntPtr)(partyStruct + 0x5C));
            if (RosterlevelNo == ThisArea)
            {
                return true;
            }

            partyStruct = gameData.mem.ReadInt64Raw((IntPtr)(partyStruct + 0x148));
        }

        return false;
    }

    public void GetLeechPositions()
    {
        LeechPosX = (int)gameData.mem.ReadUInt32Raw((IntPtr)(LeechPlayerPointer + 0x60));
        LeechPosY = (int)gameData.mem.ReadUInt32Raw((IntPtr)(LeechPlayerPointer + 0x64));
        LeechlevelNo = gameData.mem.ReadUInt16Raw((IntPtr)(LeechPlayerPointer + 0x5C));

        gameData.form.Grid_SetInfos("LeechCords", LeechPosX + "," + LeechPosY);

        //plevel = d2rprocess.read(partyStruct + 0x58, "UShort");
        //partyId = d2rprocess.read(partyStruct + 0x5A, "UShort");
        //hostilePtr = d2rprocess.read(partyStruct + 0x70, "Int64");
    }

    public void ScanForLeecher()
    {
        long UnitOffset = (long)gameData.BaseAddress + (long)gameData.offsets["rosterOffset"];
        long partyStruct = gameData.mem.ReadInt64Raw((IntPtr)UnitOffset);
        LeechPlayerPointer = 0;
        LeechPlayerUnitID = 0;

        string LeeeechName = gameData.gameStruc.GameOwnerName;
        if (CharConfig.IsRushing) LeeeechName = CharConfig.RushLeecherName;
        else if ((CharConfig.RunBaalLeechScript || CharConfig.RunChaosLeechScript) && CharConfig.SearchLeecherName != "") LeeeechName = CharConfig.SearchLeecherName;

        if (LeeeechName == "")
        {
            gameData.method_1("Leecher name is empty!", Color.Red);
        }

        for (int i = 0; i < 9; i++)
        {
            string name = gameData.mem.ReadMemString(partyStruct);
            if (name.ToLower() == LeeeechName.ToLower())
            {
                if (!PrintedLeechFoundInfo)
                {
                    gameData.method_1("Leecher pointer found!", Color.DarkViolet);
                    PrintedLeechFoundInfo = true;
                }
                LeechPlayerUnitID = gameData.mem.ReadUInt32Raw((IntPtr)(partyStruct + 0x48));
                LeechPlayerPointer = partyStruct;
                break;
            }
            else
            {
                partyStruct = gameData.mem.ReadInt64Raw((IntPtr)(partyStruct + 0x148));
            }
        }
    }

    public void scanForPlayer(bool QuickScan)   //scanning for self
    {
        FoundPlayer = false;

        try
        {
            int SizeArray = 0;
            int SizeIncrement = 0;
            byte[] unitTableBufferT = new byte[] { };
            if (QuickScan)
            {
                SizeArray = (128 + 516) * 8;
                SizeIncrement = 8;
                unitTableBufferT = new byte[SizeArray];
                long UnitOffset = (long)gameData.BaseAddress + (long)gameData.offsets["unitTable"] + gameData.UnitStrucOffset;
                gameData.mem.ReadRawMemory(UnitOffset, ref unitTableBufferT, SizeArray);

                PlayerStrucCount = 0;
                for (int i = 0; i < SizeArray; i += SizeIncrement)
                {
                    long UnitPointerLocation = BitConverter.ToInt64(unitTableBufferT, i);

                    if (UnitPointerLocation > 0)
                    {
                        byte[] itemdatastruc = new byte[144];
                        gameData.mem.ReadRawMemory(UnitPointerLocation, ref itemdatastruc, 144);

                        // Do ONLY UnitType:0 && TxtFileNo:3
                        //if (BitConverter.ToUInt32(itemdatastruc, 0) == 0 && BitConverter.ToUInt32(itemdatastruc, 4) == 3)
                        if (BitConverter.ToUInt32(itemdatastruc, 0) == 0)
                        {
                            PlayerStrucCount++;
                            //gameData.method_1("PPointerLocation: 0x" + (UnitPointerLocation).ToString("X"));

                            long pUnitDataPtr = BitConverter.ToInt64(itemdatastruc, 0x10);
                            byte[] pUnitData = new byte[144];
                            gameData.mem.ReadRawMemory(pUnitDataPtr, ref pUnitData, 144);

                            string name = "";
                            for (int i2 = 0; i2 < 16; i2++)
                            {
                                if (pUnitData[i2] != 0x00)
                                {
                                    name += (char)pUnitData[i2];
                                }
                            }
                            //name = name.Replace("?", "");
                            //gameData.method_1("PNAME: " + name, Color.Red);

                            //Console.WriteLine(BitConverter.ToUInt32(itemdatastruc, 0));
                            //Console.WriteLine(BitConverter.ToUInt32(itemdatastruc, 4));

                            long ppath = BitConverter.ToInt64(itemdatastruc, 0x38);
                            byte[] ppathData = new byte[144];
                            gameData.mem.ReadRawMemory(ppath, ref ppathData, 144);

                            //if posX equal not zero
                            if (BitConverter.ToInt16(ppathData, 2) != 0 && name == CharConfig.PlayerCharName)
                            {
                                gameData.method_1("------------------------------------------", Color.DarkBlue);
                                PlayerPointer = UnitPointerLocation;
                                gameData.form.Grid_SetInfos("Pointer", "0x" + PlayerPointer.ToString("X"));
                                FoundPlayer = true;
                                unitId = BitConverter.ToUInt32(itemdatastruc, 0x08);
                                gameData.method_1("Player ID: 0x" + unitId.ToString("X"), Color.DarkBlue);

                                /*string SavePathh = gameData.ThisEndPath + "DumpPlayerStruc";
                                File.Create(SavePathh).Dispose();
                                File.WriteAllBytes(SavePathh, itemdatastruc);
                                SavePathh = gameData.ThisEndPath + "DumpPlayerUnitData";
                                File.Create(SavePathh).Dispose();
                                File.WriteAllBytes(SavePathh, pUnitData);
                                SavePathh = gameData.ThisEndPath + "DumpPlayerPath";
                                File.Create(SavePathh).Dispose();
                                File.WriteAllBytes(SavePathh, ppathData);*/

                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                gameData.patternsScan.scanForUnitsPointer("player");

                PlayerStrucCount = 0;
                foreach (var ThisCurrentPointer in gameData.patternsScan.AllPlayersPointers)
                {
                    long UnitPointerLocation = ThisCurrentPointer.Key;

                    if (UnitPointerLocation > 0)
                    {
                        byte[] itemdatastruc = new byte[144];
                        gameData.mem.ReadRawMemory(UnitPointerLocation, ref itemdatastruc, 144);

                        // Do ONLY UnitType:0 && TxtFileNo:3
                        //if (BitConverter.ToUInt32(itemdatastruc, 0) == 0 && BitConverter.ToUInt32(itemdatastruc, 4) == 3)
                        if (BitConverter.ToUInt32(itemdatastruc, 0) == 0)
                        {
                            PlayerStrucCount++;
                            //gameData.method_1("PPointerLocation: 0x" + (UnitPointerLocation).ToString("X"));

                            long pUnitDataPtr = BitConverter.ToInt64(itemdatastruc, 0x10);
                            byte[] pUnitData = new byte[144];
                            gameData.mem.ReadRawMemory(pUnitDataPtr, ref pUnitData, 144);

                            string name = "";
                            for (int i2 = 0; i2 < 16; i2++)
                            {
                                if (pUnitData[i2] != 0x00)
                                {
                                    name += (char)pUnitData[i2];
                                }
                            }
                            //name = name.Replace("?", "");
                            //gameData.method_1("PNAME: " + name, Color.Red);

                            //Console.WriteLine(BitConverter.ToUInt32(itemdatastruc, 0));
                            //Console.WriteLine(BitConverter.ToUInt32(itemdatastruc, 4));

                            long ppath = BitConverter.ToInt64(itemdatastruc, 0x38);
                            byte[] ppathData = new byte[144];
                            gameData.mem.ReadRawMemory(ppath, ref ppathData, 144);

                            //if posX equal not zero
                            if (BitConverter.ToInt16(ppathData, 2) != 0 && name == CharConfig.PlayerCharName)
                            {
                                gameData.method_1("------------------------------------------", Color.DarkBlue);
                                PlayerPointer = UnitPointerLocation;
                                gameData.form.Grid_SetInfos("Pointer", "0x" + PlayerPointer.ToString("X"));
                                FoundPlayer = true;
                                unitId = BitConverter.ToUInt32(itemdatastruc, 0x08);
                                gameData.method_1("Player ID: 0x" + unitId.ToString("X"), Color.DarkBlue);

                                /*string SavePathh = gameData.ThisEndPath + "DumpPlayerStruc";
                                File.Create(SavePathh).Dispose();
                                File.WriteAllBytes(SavePathh, itemdatastruc);
                                SavePathh = gameData.ThisEndPath + "DumpPlayerUnitData";
                                File.Create(SavePathh).Dispose();
                                File.WriteAllBytes(SavePathh, pUnitData);
                                SavePathh = gameData.ThisEndPath + "DumpPlayerPath";
                                File.Create(SavePathh).Dispose();
                                File.WriteAllBytes(SavePathh, ppathData);*/

                                return;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            gameData.method_1("Couldn't 'scanForPlayer()'", Color.OrangeRed);
        }
    }

    public bool ScanForOthersPlayers(long ThisUnitID, string ThisPlayerName, bool GetCorpseOnly)
    {
        try
        {
            //this can be used to get self corpse??

            gameData.patternsScan.scanForUnitsPointer("player");

            foreach (var ThisCurrentPointer in gameData.patternsScan.AllPlayersPointers)
            {
                long UnitPointerLocation = ThisCurrentPointer.Key;
                if (UnitPointerLocation > 0)
                {
                    byte[] itemdatastruc = new byte[0x98];
                    gameData.mem.ReadRawMemory(UnitPointerLocation, ref itemdatastruc, itemdatastruc.Length);


                    long pInventory = BitConverter.ToInt64(itemdatastruc, 0x90);
                    if (pInventory > 0)
                    {
                        unitIdOther = BitConverter.ToUInt32(itemdatastruc, 0x08);

                        long OtherpathAddress = gameData.mem.ReadInt64Raw((IntPtr)(PlayerPointer + 0x38));
                        long OtherxPos = gameData.mem.ReadUInt16Raw((IntPtr)(OtherpathAddress + 0x02));
                        long OtheryPos = gameData.mem.ReadUInt16Raw((IntPtr)(OtherpathAddress + 0x06));
                        long OtherxPosOffset = gameData.mem.ReadUInt16Raw((IntPtr)(OtherpathAddress + 0x00));
                        long OtheryPosOffset = gameData.mem.ReadUInt16Raw((IntPtr)(OtherpathAddress + 0x04));
                        long OtherxPosOffsetPercent = (OtherxPosOffset / 65536); //get percentage
                        long OtheryPosOffsetPercent = (OtheryPosOffset / 65536); //get percentage
                        xPosFinalOtherP = (ushort)(OtherxPos + OtherxPosOffsetPercent);
                        yPosFinalOtherP = (ushort)(OtheryPos + OtheryPosOffsetPercent);

                        long pUnitDataPtr = BitConverter.ToInt64(itemdatastruc, 0x10);
                        byte[] pUnitData = new byte[144];
                        gameData.mem.ReadRawMemory(pUnitDataPtr, ref pUnitData, 144);

                        string name = "";
                        for (int i2 = 0; i2 < 16; i2++)
                        {
                            if (pUnitData[i2] != 0x00)
                            {
                                name += (char)pUnitData[i2];
                            }
                            else
                            {
                                break;
                            }
                        }
                        pNameOther = name;

                        IsCorpse = false;
                        if (gameData.mem.ReadByteRaw((IntPtr)(PlayerPointer + 0x1A6)) == 1)
                        {
                            IsCorpse = true;
                        }
                        if (xPosFinalOtherP > 1 && yPosFinalOtherP > 1)
                        {
                            if (unitIdOther != 0)
                            {
                                if (unitIdOther == ThisUnitID)
                                {
                                    if (!GetCorpseOnly || (GetCorpseOnly && IsCorpse))
                                    {
                                        return true;
                                    }
                                }
                            }
                            if (ThisPlayerName != "")
                            {
                                //gameData.method_1("TEST player corpse scan name: " + ThisPlayerName + "|" + IsCorpse, Color.OrangeRed);

                                if (pNameOther == ThisPlayerName)
                                {
                                    if (!GetCorpseOnly || (GetCorpseOnly && IsCorpse))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            gameData.method_1("Couldn't 'ScanForOthersPlayers()'", Color.OrangeRed);
        }

        return false;
    }

    public void GetHPAndManaOnThisEquippedItem()
    {
        int AddedHP = 0;
        int AddedMana = 0;
        int AddedVit = 0;
        int AddedEnergy = 0;
        int AddedHPPercent = 0;
        int AddedManaPercent = 0;

        //item_maxmana_percent	ln34	item_maxhp_percent	ln34	skill_staminapercent

        if (gameData.itemsStruc.statCount > 0)
        {
            for (int i = 0; i < gameData.itemsStruc.statCount; i++)
            {
                int offset = i * 8;
                //short statLayer = BitConverter.ToInt16(gameData.itemsStruc.statBuffer, offset);
                ushort statEnum = BitConverter.ToUInt16(gameData.itemsStruc.statBuffer, offset + 0x2);
                int statValue = BitConverter.ToInt32(gameData.itemsStruc.statBuffer, offset + 0x4);

                if (statEnum == (ushort)Enums.Attribute.Vitality)
                {
                    AddedVit += statValue;
                    AddedHP += statValue;
                }
                if (statEnum == (ushort)Enums.Attribute.LifeMax)
                {
                    AddedHP += (statValue >> 8);
                }
                if (statEnum == (ushort)Enums.Attribute.MaxHPPercent)
                {
                    AddedHPPercent += statValue;
                }
                if (statEnum == (ushort)Enums.Attribute.Energy)
                {
                    AddedEnergy += statValue;
                    AddedMana += statValue;
                }
                if (statEnum == (ushort)Enums.Attribute.ManaMax)
                {
                    AddedMana += (statValue >> 8);
                }
                if (statEnum == (ushort)Enums.Attribute.MaxManaPercent)
                {
                    AddedManaPercent += statValue;
                }
            }
        }

        if (gameData.itemsStruc.statExCount > 0)
        {
            for (int i = 0; i < gameData.itemsStruc.statExCount; i++)
            {
                int offset = i * 8;
                //short statLayer = BitConverter.ToInt16(gameData.itemsStruc.statBufferEx, offset);
                ushort statEnum = BitConverter.ToUInt16(gameData.itemsStruc.statBufferEx, offset + 0x2);
                int statValue = BitConverter.ToInt32(gameData.itemsStruc.statBufferEx, offset + 0x4);

                if (statEnum == (ushort)Enums.Attribute.Vitality)
                {
                    AddedVit += statValue;
                    AddedHP += statValue;
                }
                if (statEnum == (ushort)Enums.Attribute.LifeMax)
                {
                    AddedHP += (statValue >> 8);
                }
                if (statEnum == (ushort)Enums.Attribute.MaxHPPercent)
                {
                    AddedHPPercent += statValue;
                }
                if (statEnum == (ushort)Enums.Attribute.Energy)
                {
                    AddedEnergy += statValue;
                    AddedMana += statValue;
                }
                if (statEnum == (ushort)Enums.Attribute.ManaMax)
                {
                    AddedMana += (statValue >> 8);
                }
                if (statEnum == (ushort)Enums.Attribute.MaxManaPercent)
                {
                    AddedManaPercent += statValue;
                }
            }
        }

        HPFromEquippedItems += AddedHP;
        ManaFromEquippedItems += AddedMana;
        VitalityFromEquippedItems += AddedVit;
        EnergyFromEquippedItems += AddedEnergy;
        HPPercentFromEquippedItems += AddedHPPercent;
        ManaPercentFromEquippedItems += AddedManaPercent;
    }

    public bool HasState(EnumsStates.State state, List<EnumsStates.State> ThisStatesList)
    {
        foreach (EnumsStates.State st in ThisStatesList)
        {
            if (st == state)
            {
                return true;
            }
        }
        return false;
    }

    public List<EnumsStates.State> GetStates(long statsListExPtr)
    {
        List<EnumsStates.State> states = new List<EnumsStates.State>();
        for (int i = 0; i < 6; i++)
        {
            int offset = i * 4;
            byte stateByte = gameData.mem.ReadByteRaw((IntPtr)(statsListExPtr + 0xAD0 + (uint)offset));

            offset = (32 * i) - 1;
            states.AddRange(CalculateStates(stateByte, (uint)offset));
        }

        return states;
    }

    // Assuming you have a Process class with a ReadUInt method
    private Process Process = new Process();

    private List<EnumsStates.State> CalculateStates(byte stateByte, uint offset)
    {
        List<EnumsStates.State> states = new List<EnumsStates.State>();
        for (int i = 0; i < 8; i++)
        {
            if ((stateByte & (1 << i)) != 0)
            {
                states.Add((EnumsStates.State)(offset + i + 1));
            }
        }
        return states;
    }
}
