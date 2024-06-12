﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static EnumsMobsNPC;
using static EnumsStates;
using static MapAreaStruc;
using static System.Windows.Forms.AxHost;

public class MercStruc
{
    private GameData gameData;

    public long MercPointerLocation = 0;
    public byte[] Mercdatastruc = new byte[144];
    public uint txtFileNo = 0;
    public long pPathPtr = 0;
    public long pUnitData = 0;
    public uint mode = 0;
    public ushort itemx = 0;
    public ushort itemy = 0;
    public ushort xPosFinal = 0;
    public ushort yPosFinal = 0;
    public byte[] pPath = new byte[144];
    public bool MercAlive = false;
    public int MercHP = 0;
    public int MercMaxHP = 0;

    public uint MercOwnerID = 0; //set within ItemStrus (equipped item on merc)

    public uint statCount = 0;
    public uint statExCount = 0;
    public long statPtr = 0;
    public long statExPtr = 0;
    public byte[] pStatB = new byte[180];
    public byte[] statBuffer = new byte[] { };

    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public bool IsMerc(int MobNameID)
    {
        return MobNameID == (int)EnumsMobsNPC.MobsNPC.Guard || MobNameID == (int)EnumsMobsNPC.MobsNPC.Act5Hireling1Hand || MobNameID == (int)EnumsMobsNPC.MobsNPC.Act5Hireling2Hand || MobNameID == (int)EnumsMobsNPC.MobsNPC.IronWolf || MobNameID == (int)EnumsMobsNPC.MobsNPC.Rogue2;
    }

    public bool GetMercInfos()
    {
        try
        {
            MercAlive = true;
            txtFileNo = 0;
            gameData.patternsScan.scanForUnitsPointer("NPC");

            int MercCount = 1;

            foreach (var ThisCurrentPointer in gameData.patternsScan.AllNPCPointers)
            {
                MercPointerLocation = ThisCurrentPointer.Key;
                if (MercPointerLocation > 0)
                {
                    Mercdatastruc = new byte[144];
                    gameData.mem.ReadRawMemory(MercPointerLocation, ref Mercdatastruc, 144);

                    txtFileNo = BitConverter.ToUInt32(Mercdatastruc, 4);
                    pUnitData = BitConverter.ToInt64(Mercdatastruc, 0x10);
                    mode = BitConverter.ToUInt32(Mercdatastruc, 0x0c);
                    ushort isUnique = gameData.mem.ReadUInt16Raw((IntPtr)pUnitData + 0x18);
                    GetUnitPathData();
                    GetStatsAddr();

                    //Console.WriteLine(gameData.npcStruc.getNPC_ID((int) txtFileNo));
                    //Console.WriteLine(txtFileNo.ToString() + ", isUnique:" + isUnique + ", isPlayerMinion:" + isPlayerMinion + ", mode:" + mode + ", pos:" + xPosFinal + ", " + yPosFinal);

                    //if (IsMerc((int) txtFileNo))
                    if (isUnique == 0 && (txtFileNo == 338) && mode != 0 && mode != 12)
                    //if (isUnique == 0 && (txtFileNo == 338) && mode == 1)
                    {
                        if (xPosFinal != 0 && yPosFinal != 0)
                        {
                            Int64 pUnitDataPtr = BitConverter.ToInt64(Mercdatastruc, 0x10);
                            //uint dwOwnerId = gameData.mem.ReadUInt32Raw((IntPtr)(pUnitDataPtr + 0x0c));
                            //uint dwOwnerId = BitConverter.ToUInt32(Mercdatastruc, 8);

                            if (gameData.mem.ReadByteRaw((IntPtr)(pUnitDataPtr + 0x32)) != 0x0e && gameData.mem.ReadByteRaw((IntPtr)(pUnitDataPtr + 0x33)) != 0x04)
                                //if (dwOwnerId == MercOwnerID && MercOwnerID != 0)
                                //{
                                //SetHPFromStats();
                                /*string SavePathh = gameData.ThisEndPath + "DumpMercStruc" + MercCount;
                                File.Create(SavePathh).Dispose();
                                File.WriteAllBytes(SavePathh, Mercdatastruc);*/

                                /*byte[] buffff = new byte[144];
                                long pStatsListExPtr = BitConverter.ToInt64(Mercdatastruc, 0x10);
                                gameData.mem.ReadRawMemory(pStatsListExPtr, ref buffff, 500);

                                //pStatsListExPtr = BitConverter.ToInt64(buffff, 8);
                                //gameData.mem.ReadRawMemory(pStatsListExPtr, ref buffff, 500);
                                //uint dwOwnerId = BitConverter.ToUInt32(buffff, 0x0c);
                                //uint flags = BitConverter.ToUInt32(buffff, 0x18);

                                string SavePathh2 = gameData.ThisEndPath + "DumpMercStrucBuf" + MercCount;
                                File.Create(SavePathh2).Dispose();
                                File.WriteAllBytes(SavePathh2, buffff);*/

                                //Console.WriteLine(txtFileNo.ToString() + ", isUnique:" + isUnique + ", ownerID:" + dwOwnerId.ToString("X") + ", mode:" + mode + ", pos:" + xPosFinal + ", " + yPosFinal);
                                //Console.WriteLine(flags);
                                //MercCount++;

                                SetHPFromStats();
                            gameData.form.Grid_SetInfos("Merc", MercHP.ToString() + "/" + MercMaxHP.ToString());
                            return true;
                            //}
                        }
                    }
                }
            }
        }
        catch
        {
            gameData.method_1("Couldn't 'GetMercInfos()'", Color.OrangeRed);
        }

        gameData.form.Grid_SetInfos("Merc", "Not alive/detected");
        MercAlive = false;
        return false;
    }

    public void SetHPFromStats()
    {
        try
        {
            MercHP = 32768;
            MercMaxHP = 32768;

            gameData.mem.ReadRawMemory(this.statPtr, ref statBuffer, (int)(this.statCount * 10));
            for (int i = (int)this.statCount - 1; i >= 0; i--)
            {
                int offset = i * 8;
                //short statLayer = BitConverter.ToInt16(statBuffer, offset);
                ushort statEnum = BitConverter.ToUInt16(statBuffer, offset + 0x2);
                int statValue = BitConverter.ToInt32(statBuffer, offset + 0x4);
                if (statEnum == (ushort)Enums.Attribute.Life)
                {
                    MercHP = statValue;
                    if (MercHP > 32768) MercHP = statValue >> 8;

                }
                /*if (statEnum == (ushort)Enums.Attribute.LifeMax)
                {
                    MercMaxHP = statValue >> 8;
                }*/

                //Console.WriteLine(((Enums.Attribute) statEnum).ToString() + " = " + statValue);
            }

            //if (MercMaxHP < MercHP) MercMaxHP = MercHP;
            /*if (ThisHPStat <= 32768)
            {
                MercHP = ThisHPStat / 32768 * MercMaxHP;
            }
            else
            {
                MercHP = ThisHPStat >> 8;
            }*/

            //Console.WriteLine("HP:" + MercHP);
        }
        catch { }
    }

    public void GetStatsAddr()
    {
        long pStatsListExPtr = BitConverter.ToInt64(Mercdatastruc, 0x88);

        /*pStatB = new byte[180];
        gameData.mem.ReadRawMemory(pStatsListExPtr, ref pStatB, 180);
        statPtr = BitConverter.ToInt64(pStatB, 0x30);
        statCount = BitConverter.ToUInt32(pStatB, 0x38);
        statExPtr = BitConverter.ToInt64(pStatB, 0x88);
        statExCount = BitConverter.ToUInt32(pStatB, 0x90);*/

        statPtr = gameData.mem.ReadInt64Raw((IntPtr)(pStatsListExPtr + 0x30));
        statCount = gameData.mem.ReadUInt32Raw((IntPtr)(pStatsListExPtr + 0x38));
        statExPtr = gameData.mem.ReadInt64Raw((IntPtr)(pStatsListExPtr + 0x88));
        statExCount = gameData.mem.ReadUInt32Raw((IntPtr)(pStatsListExPtr + 0x90));
    }

    public void GetUnitPathData()
    {
        pPathPtr = BitConverter.ToInt64(Mercdatastruc, 0x38);
        //pPath = new byte[144];
        pPath = new byte[0x08];
        gameData.mem.ReadRawMemory(pPathPtr, ref pPath, pPath.Length);

        itemx = BitConverter.ToUInt16(pPath, 0x02);
        itemy = BitConverter.ToUInt16(pPath, 0x06);
        ushort xPosOffset = BitConverter.ToUInt16(pPath, 0x00);
        ushort yPosOffset = BitConverter.ToUInt16(pPath, 0x04);
        int xPosOffsetPercent = (xPosOffset / 65536); //get percentage
        int yPosOffsetPercent = (yPosOffset / 65536); //get percentage
        xPosFinal = (ushort)(itemx + xPosOffsetPercent);
        yPosFinal = (ushort)(itemy + yPosOffsetPercent);
    }
}
