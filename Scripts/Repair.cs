using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Repair
{
    private GameData gameData;
    public bool ShouldRepair = false;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public void RunRepairScript()
    {
        int tries = 0;
        bool Repairing = true;
        while (Repairing && tries < 3)
        {
            gameData.keyMouse.MouseClicc(585, 775);  //clic full repair button
            gameData.WaitDelay(40);
            Repairing = GetShouldRepair();
            tries++;
        }
    }

    public bool GetShouldRepair()
    {
        gameData.itemsStruc.GetItems(false);   //get inventory
        return ShouldRepair;
    }

    public void GetDurabilityOnThisEquippedItem()
    {
        int durability = 1;
        int Maxdurability = 1;

        if (gameData.itemsStruc.statCount > 0)
        {
            //; get durability
            //gameData.mem.ReadRawMemory(gameData.itemsStruc.statPtr, ref gameData.itemsStruc.statBuffer, (int)(gameData.itemsStruc.statCount * 10));
            for (int i = 0; i < gameData.itemsStruc.statCount; i++)
            {
                int offset = i * 8;
                //short statLayer = BitConverter.ToInt16(gameData.itemsStruc.statBuffer, offset);
                ushort statEnum = BitConverter.ToUInt16(gameData.itemsStruc.statBuffer, offset + 0x2);
                int statValue = BitConverter.ToInt32(gameData.itemsStruc.statBuffer, offset + 0x4);
                //ushort statEnum = BitConverter.ToUInt16(gameData.itemsStruc.statBuffer, offset);
                //int statValue = BitConverter.ToInt32(gameData.itemsStruc.statBuffer, offset + 0x2);

                if (statEnum == (ushort)Enums.Attribute.Durability)
                {
                    durability = statValue;
                }
                if (statEnum == (ushort)Enums.Attribute.MaxDurability)
                {
                    Maxdurability = statValue;
                }
            }
        }

        if (durability == 1 && Maxdurability == 1)
        {
            if (gameData.itemsStruc.statExCount > 0)
            {
                //; get durability
                //gameData.mem.ReadRawMemory(gameData.itemsStruc.statExPtr, ref gameData.itemsStruc.statBuffer, (int)(gameData.itemsStruc.statExCount * 10));
                for (int i = 0; i < gameData.itemsStruc.statExCount; i++)
                {
                    int offset = i * 8;
                    //short statLayer = BitConverter.ToInt16(gameData.itemsStruc.statBufferEx, offset);
                    ushort statEnum = BitConverter.ToUInt16(gameData.itemsStruc.statBufferEx, offset + 0x2);
                    int statValue = BitConverter.ToInt32(gameData.itemsStruc.statBufferEx, offset + 0x4);
                    //ushort statEnum = BitConverter.ToUInt16(gameData.itemsStruc.statBufferEx, offset);
                    //int statValue = BitConverter.ToInt32(gameData.itemsStruc.statBufferEx, offset + 0x2);

                    if (statEnum == (ushort)Enums.Attribute.Durability)
                    {
                        durability = statValue;
                    }
                    if (statEnum == (ushort)Enums.Attribute.MaxDurability)
                    {
                        Maxdurability = statValue;
                    }
                }
            }
        }

        int DurabilityPercent = (durability * 100) / Maxdurability;
        if (DurabilityPercent < 25)
        {
            ShouldRepair = true;
        }
    }
}
