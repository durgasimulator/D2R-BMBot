﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class BeltStruc
{
    GameData gameData = GameData.Instance;

    public int[] BeltHaveItems = new int[16];
    public int[] BeltItemsTypes = new int[16];
    public bool MissingHPPot = false;
    public bool MissingManaPot = false;
    public bool MissingRVPot = false;
    public bool IsItemHPPotion = false;
    public bool IsItemManaPotion = false;
    public bool IsItemRVPotion = false;
    public bool IsItemFullRVPotion = false;

    public int HPQuantity = 0;
    public int ManyQuantity = 0;

    public int ForceHPPotionQty = 0;
    public int ForceMANAPotionQty = 0;
    public bool HasPotInBadSpot = false;

    public bool GrabBothRV = false;

    public int GetPotionQuantityInBelt(int ThisPotType)
    {
        int Qty1 = 0;
        for (int i = 0; i < BeltHaveItems.Length; i++)
        {
            if ((i == 0 || i == 4 || i == 8 || i == 12) && BeltHaveItems[i] > 0 && BeltItemsTypes[i] == ThisPotType) Qty1++;
            if ((i == 1 || i == 5 || i == 9 || i == 13) && BeltHaveItems[i] > 0 && BeltItemsTypes[i] == ThisPotType) Qty1++;
            if ((i == 2 || i == 6 || i == 10 || i == 14) && BeltHaveItems[i] > 0 && BeltItemsTypes[i] == ThisPotType) Qty1++;
            if ((i == 3 || i == 7 || i == 11 || i == 15) && BeltHaveItems[i] > 0 && BeltItemsTypes[i] == ThisPotType) Qty1++;
        }

        return Qty1;
    }

    public bool ItemGrabPotion()
    {
        if (CharConfig.RunItemGrabScriptOnly) return false; //item grab only

        //####
        if (gameData.inventoryStruc.HasInventoryItemName("Super Healing Potion"))
        {
            //gameData.method_1("FORCING HP POT QTY: " + HPQuantity, Color.Red);
            ForceHPPotionQty = HPQuantity; //reset qty in belt
            MissingHPPot = false;
        }
        if (gameData.inventoryStruc.HasInventoryItemName("Super Mana Potion"))
        {
            ForceMANAPotionQty = ManyQuantity; //reset qty in belt
            MissingManaPot = false;
        }
        //####

        //Console.WriteLine("" + MissingHPPot + MissingManaPot + MissingRVPot);
        GrabBothRV = false;
        for (int i = 0; i < 4; i++)
        {
            if (CharConfig.BeltPotTypeToHave[i] == 2)
            {
                GrabBothRV = true;
                break;
            }
        }

        if (MissingHPPot || MissingManaPot || MissingRVPot)
        {
            ItemIsPotion();
            if (MissingHPPot && IsItemHPPotion) return true;
            if (MissingManaPot && IsItemManaPotion) return true;
            if (MissingRVPot && IsItemRVPotion && GrabBothRV) return true;
            if (MissingRVPot && IsItemFullRVPotion) return true;
        }
        return false;
    }

    public void CheckForMissingPotions()
    {
        gameData.itemsStruc.GetItems(false);

        MissingHPPot = false;
        MissingManaPot = false;
        MissingRVPot = false;

        for (int i = 0; i < 16; i++)
        {
            if (BeltHaveItems[i] == 0)
            {
                int BeltIndd = ConvertBeltIndexTo4Number(i);
                if (BeltIndd != 5)
                {
                    int PotToHave = CharConfig.BeltPotTypeToHave[BeltIndd];
                    if (PotToHave == 0)
                    {
                        MissingHPPot = true;
                    }
                    if (PotToHave == 1)
                    {
                        MissingManaPot = true;
                    }
                    if (PotToHave == 2 || PotToHave == 3)
                    {
                        MissingRVPot = true;
                    }
                }
            }
        }

        //gameData.method_1("HP POT QTY: " + HPQuantity, Color.Red);

        if (MissingHPPot)
        {
            if (ForceHPPotionQty != 0)
            {
                if (ForceHPPotionQty == HPQuantity)
                {
                    MissingHPPot = false; //not missing pot fix
                }
            }
        }

        if (MissingManaPot)
        {
            if (ForceMANAPotionQty != 0)
            {
                if (ForceMANAPotionQty == ManyQuantity)
                {
                    MissingManaPot = false; //not missing pot fix
                }
            }
        }

        //Console.WriteLine("Missing pots:" + MissingHPPot + "," + MissingManaPot + "," + MissingRVPot);
    }

    public int ConvertBeltIndexTo4Number(int BeltInd)
    {
        if (BeltInd == 0 || BeltInd == 4 || BeltInd == 8 || BeltInd == 12) return 0;
        if (BeltInd == 1 || BeltInd == 5 || BeltInd == 9 || BeltInd == 13) return 1;
        if (BeltInd == 2 || BeltInd == 6 || BeltInd == 10 || BeltInd == 14) return 2;
        if (BeltInd == 3 || BeltInd == 7 || BeltInd == 11 || BeltInd == 15) return 3;
        return 5;
    }

    public void AddBeltItem(bool UsePotNotInSpot)
    {
        int BufferPotType = GetPotType();
        try
        {
            BeltHaveItems[gameData.itemsStruc.itemx] = 1;
            BeltItemsTypes[gameData.itemsStruc.itemx] = BufferPotType;
        }
        catch
        {
            return;
        }

        bool UsedPotion = false;
        HasPotInBadSpot = false;
        int BeltIndd = ConvertBeltIndexTo4Number(gameData.itemsStruc.itemx);
        if (BeltIndd != 5)
        {
            int PotToHave = CharConfig.BeltPotTypeToHave[BeltIndd];
            if (PotToHave != BufferPotType)
            {
                if (UsePotNotInSpot)
                {
                    gameData.potions.PressPotionKey(BeltIndd, false); //use potion, not in right spot
                    UsedPotion = true;
                }
                else
                {
                    HasPotInBadSpot = true;
                }
            }
        }

        if (!UsedPotion)
        {
            if (BufferPotType == 0)
            {
                HPQuantity++;
            }
            if (BufferPotType == 1)
            {
                ManyQuantity++;
            }
        }
        //gameData.method_1("belt pointer" + gameData.itemsStruc.itemx + ": 0x" + gameData.itemsStruc.ItemPointerLocation.ToString("X") + " (diff from player: 0x" + (gameData.playerScan.PlayerPointer - gameData.itemsStruc.ItemPointerLocation).ToString("X") + ")");
    }

    public void ItemIsPotion()
    {
        IsItemHPPotion = false;
        IsItemManaPotion = false;
        IsItemRVPotion = false;
        IsItemFullRVPotion = false;

        string ThisItemName = gameData.itemsStruc.ItemNAAME.Replace(" ", "");

        foreach (var ThisDir in gameData.itemsAlert.PickItemsPotions)
        {
            if (ThisItemName == Regex.Replace(ThisDir.Key.Replace(" ", ""), @"[\d-]", string.Empty) && ThisDir.Value)
            {
                if (ThisItemName.Contains("Healing")) IsItemHPPotion = true;
                if (ThisItemName.Contains("Mana")) IsItemManaPotion = true;
                if (ThisItemName.Contains("Rejuvenation")) IsItemRVPotion = true;
                if (ThisItemName.Contains("FullRejuvenation")) IsItemFullRVPotion = true;
            }
        }

        /*if (gameData.itemsStruc.ItemNAAME == "Super Healing Potion") IsItemHPPotion = true;
        if (gameData.itemsStruc.ItemNAAME == "Super Mana Potion") IsItemManaPotion = true;
        if (gameData.itemsStruc.ItemNAAME == "Rejuvenation Potion") IsItemRVPotion = true;
        if (gameData.itemsStruc.ItemNAAME == "Full Rejuvenation Potion") IsItemFullRVPotion = true;*/
    }

    public int GetPotType()
    {
        if (gameData.itemsStruc.ItemNAAME.Contains("Healing") ||
            gameData.itemsStruc.ItemNAAME == "Potion of Life")
        {
            return 0;
        }
        if (gameData.itemsStruc.ItemNAAME.Contains("Mana"))
        {
            return 1;
        }
        if (gameData.itemsStruc.ItemNAAME == "Rejuvenation Potion")
        {
            return 2;
        }
        if (gameData.itemsStruc.ItemNAAME == "Full Rejuvenation Potion")
        {
            return 3;
        }
        return -1;
    }
}
