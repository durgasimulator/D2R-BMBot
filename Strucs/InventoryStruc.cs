using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

public class InventoryStruc
{

    private GameData gameData;

    public int[] InventoryHasItem = new int[40];
    public int[] InventoryHasUnidItem = new int[40];
    public int[] InventoryHasStashItem = new int[40];
    public int[] InventoryHasItemToID = new int[40];
    public long[] InventoryItemPointers = new long[40];
    public string[] InventoryItemNames = new string[40];
    public int[] InventoryItemQuality = new int[40];

    public int HUDItems_idscrolls = 0;
    public int HUDItems_tpscrolls = 0;
    public int HUDItems_keys = 0;

    public int HUDItems_tpscrolls_locx = -1;
    public int HUDItems_tpscrolls_locy = -1;
    public int HUDItems_idscrolls_locx = 0;
    public int HUDItems_idscrolls_locy = 0;

    public bool HasIDTome = false;

    public bool DisabledSpecialItems = false;

    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public void CheckInventorySpecialUniqueItems()
    {
        if (DisabledSpecialItems) return;

        //#######
        string ThisNamee = "SmallCharm";
        int ThisIndex = 2;
        bool PickingAnni = false;
        while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
        {
            //Console.WriteLine(gameData.itemsStruc.ItemNAAME + ":" + ThisNamee);
            if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
            {
                if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                {
                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) PickingAnni = true;
                }
                break;
            }
            ThisNamee = "SmallCharm" + ThisIndex;
            ThisIndex++;
        }

        ThisNamee = "LargeCharm";
        ThisIndex = 2;
        bool PickingTorch = false;
        while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
        {
            if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
            {
                if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                {
                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) PickingTorch = true;
                }
                break;
            }
            ThisNamee = "LargeCharm" + ThisIndex;
            ThisIndex++;
        }

        ThisNamee = "GrandCharm";
        ThisIndex = 2;
        bool PickingGC = false;
        while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
        {
            if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
            {
                if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                {
                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) PickingGC = true;
                }
                break;
            }
            ThisNamee = "GrandCharm" + ThisIndex;
            ThisIndex++;
        }
        //#######

        if (PickingAnni || PickingTorch || PickingGC)
        {
            gameData.itemsStruc.GetItems(false);

            //Check inside Inventory
            for (int i = 0; i < 40; i++)
            {
                if (CharConfig.InventoryDontCheckItem[i] == 1)
                {
                    //################
                    //GET ITEM (UNIQUE GC, GHEED, TORCH, ANNI)
                    Dictionary<string, int> itemXYPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                    if (gameData.itemsStruc.GetSpecificItem(0, "", itemXYPos["x"], itemXYPos["y"], gameData.playerScan.unitId, 0))
                    {
                        if (gameData.itemsStruc.ItemNAAME == "Small Charm" && gameData.itemsStruc.itemQuality == 7)
                        {
                            gameData.method_1("Cannot Pick Anni since you have one in your inventory, disabling Anni from pickit!", Color.OrangeRed);

                            ThisNamee = "SmallCharm";
                            ThisIndex = 2;
                            while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
                            {
                                if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
                                {
                                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                                    {
                                        if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee] = false;
                                    }
                                }
                                ThisNamee = "SmallCharm" + ThisIndex;
                                ThisIndex++;
                            }
                        }
                        else if (gameData.itemsStruc.ItemNAAME == "Large Charm" && gameData.itemsStruc.itemQuality == 7)
                        {
                            gameData.method_1("Cannot Pick Torch since you have one in your inventory, disabling Torch from pickit!", Color.OrangeRed);
                            
                            ThisNamee = "LargeCharm";
                            ThisIndex = 2;
                            while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
                            {
                                if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
                                {
                                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                                    {
                                        if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee] = false;
                                    }
                                }
                                ThisNamee = "LargeCharm" + ThisIndex;
                                ThisIndex++;
                            }
                        }
                        else if (gameData.itemsStruc.ItemNAAME == "Grand Charm" && gameData.itemsStruc.itemQuality == 7)
                        {
                            gameData.method_1("Cannot Pick Unique GC's since you have one in your inventory, disabling Unique GC's from pickit!", Color.OrangeRed);

                            ThisNamee = "GrandCharm";
                            ThisIndex = 2;
                            while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
                            {
                                if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
                                {
                                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                                    {
                                        if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee] = false;
                                    }
                                }
                                ThisNamee = "GrandCharm" + ThisIndex;
                                ThisIndex++;
                            }
                        }
                    }
                    //################
                }
            }

            //Check inside stash
            for (int i = 0; i < 100; i++)
            {
                if (gameData.stashStruc.Stash1_ItemTxtNoList[i] >= 603 && gameData.stashStruc.Stash1_ItemTxtNoList[i] <= 605)
                {
                    //################
                    //GET ITEM (UNIQUE GC, GHEED, TORCH, ANNI)
                    Dictionary<string, int> itemXYPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                    if (gameData.itemsStruc.GetSpecificItem(0, "", itemXYPos["x"], itemXYPos["y"], gameData.playerScan.unitId, 4))
                    {
                        if (gameData.itemsStruc.ItemNAAME == "Small Charm" && gameData.itemsStruc.itemQuality == 7)
                        {
                            gameData.method_1("Cannot Pick Anni since you have one in your normal stash, disabling Anni from pickit!", Color.OrangeRed);

                            ThisNamee = "SmallCharm";
                            ThisIndex = 2;
                            while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
                            {
                                if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
                                {
                                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                                    {
                                        if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee] = false;
                                    }
                                }
                                ThisNamee = "SmallCharm" + ThisIndex;
                                ThisIndex++;
                            }
                        }
                        else if (gameData.itemsStruc.ItemNAAME == "Large Charm" && gameData.itemsStruc.itemQuality == 7)
                        {
                            gameData.method_1("Cannot Pick Torch since you have one in your normal stash, disabling Torch from pickit!", Color.OrangeRed);

                            ThisNamee = "LargeCharm";
                            ThisIndex = 2;
                            while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
                            {
                                if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
                                {
                                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                                    {
                                        if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee] = false;
                                    }
                                }
                                ThisNamee = "LargeCharm" + ThisIndex;
                                ThisIndex++;
                            }
                        }
                        else if (gameData.itemsStruc.ItemNAAME == "Grand Charm" && gameData.itemsStruc.itemQuality == 7)
                        {
                            gameData.method_1("Cannot Pick Unique GC's since you have one in your normal stash, disabling Unique GC's from pickit!", Color.OrangeRed);

                            ThisNamee = "GrandCharm";
                            ThisIndex = 2;
                            while (gameData.itemsAlert.PickItemsNormal_ByName.ContainsKey(ThisNamee))
                            {
                                if (gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee])
                                {
                                    if (gameData.itemsAlert.PickItemsNormal_ByName_Quality.ContainsKey(ThisNamee))
                                    {
                                        if (gameData.itemsAlert.PickItemsNormal_ByName_Quality[ThisNamee] == 7) gameData.itemsAlert.PickItemsNormal_ByName[ThisNamee] = false;
                                    }
                                }
                                ThisNamee = "GrandCharm" + ThisIndex;
                                ThisIndex++;
                            }
                        }
                    }
                    //################
                }
            }
        }

        DisabledSpecialItems = true;
    }

    public void UseTP()
    {
        if (HUDItems_tpscrolls_locx != -1 && HUDItems_tpscrolls_locy != -1)
        {
            Dictionary<string, int> itemScreenPos = ConvertInventoryLocToScreenPos(HUDItems_tpscrolls_locx, HUDItems_tpscrolls_locy);
            gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
        }
        else
        {
            gameData.method_1("Tome of Town Portal not found in the Inventory!", Color.Red);
        }
    }

    public Dictionary<string, int> ConvertInventoryLocToScreenPos(int ThisX, int ThisY)
    {
        //starting at 1295,580 on screen for first item in inv, increment for 48px
        int xS = 1300 + (ThisX * 48);
        int yS = 580 + (ThisY * 48);

        Dictionary<string, int> NewDict = new Dictionary<string, int>();
        NewDict["x"] = xS;
        NewDict["y"] = yS;
        return NewDict;
    }

    public int ConvertXYToIndex(int ThisX, int ThisY)
    {
        return ThisX + (ThisY * 10);
    }

    public Dictionary<string, int> ConvertIndexToXY(int Thisndex)
    {
        int yS = (int)Math.Floor((double)Thisndex / 10);
        int xS = Thisndex - (yS * 10);

        Dictionary<string, int> NewDict = new Dictionary<string, int>();
        NewDict["x"] = xS;
        NewDict["y"] = yS;
        return NewDict;
    }

    public bool ContainStashItemInInventory()
    {
        bool ContainStashItem = false;
        gameData.itemsStruc.GetItems(false);

        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] >= 1 && InventoryHasStashItem[i] >= 1)
            {
                ContainStashItem = true;
            }

            /*if (InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] >= 1)
            {
                gameData.itemsStruc.GetItemAtPointer(InventoryItemPointers[i]);
                if (gameData.itemsAlert.ShouldKeepItem())
                {
                    ContainStashItem = true;
                }
            }*/
        }

        return ContainStashItem;
    }

    public void VerifyKeysInventory()
    {
        if (!CharConfig.UseKeys) return;

        int thisindex = CharConfig.KeysLocationInInventory.Item1 + (CharConfig.KeysLocationInInventory.Item2 * 10);

        //if its not a key at the key location, relocate the item
        if (InventoryItemNames[thisindex] != "Key" && InventoryItemNames[thisindex] != "" && InventoryHasItem[thisindex] > 0)
        {
            //Console.WriteLine("here");
            int ThisNewIndex = GetNextFreeSpaceInInventory();
            if (ThisNewIndex > -1)
            {
                //remove item from this slot
                Dictionary<string, int> itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(thisindex);
                itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

                //gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"]);
                gameData.stash.PickItem(itemScreenPos["x"], itemScreenPos["y"]);
                gameData.WaitDelay(5);

                //place to next free space
                itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(ThisNewIndex);
                itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

                gameData.stash.PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
                gameData.stash.PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
            }
        }

        //place all keys together
        thisindex = CharConfig.KeysLocationInInventory.Item1 + (CharConfig.KeysLocationInInventory.Item2 * 10);
        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] >= 1 && InventoryItemNames[i] == "Key")
            {
                //Console.WriteLine("hereIndex:" + i);
                //pick key item from this slot
                Dictionary<string, int> itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

                //gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"]);
                gameData.stash.PickItem(itemScreenPos["x"], itemScreenPos["y"]);
                gameData.WaitDelay(5);

                //place with other key
                itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(thisindex);
                itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

                if (!gameData.stash.PlaceItem(itemScreenPos["x"], itemScreenPos["y"]))
                {
                    itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                    itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

                    gameData.stash.PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.stash.PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
                }
            }
        }
    }

    public void SetHUDItem()
    {
        if (gameData.itemsStruc.statCount > 0)
        {
            //; get quantity
            int quantity = 1;
            //gameData.mem.ReadRawMemory(gameData.itemsStruc.statPtr, ref gameData.itemsStruc.statBuffer, (int)(gameData.itemsStruc.statCount * 10));
            for (int i = 0; i < gameData.itemsStruc.statCount; i++)
            {
                int offset = i * 8;
                ushort statEnum = BitConverter.ToUInt16(gameData.itemsStruc.statBuffer, offset);
                int statValue = BitConverter.ToInt32(gameData.itemsStruc.statBuffer, offset + 0x2);

                //bad verif
                if (statEnum == 70)
                {
                    quantity = statValue;
                }
                //good verif
                if ((statEnum == 0 && gameData.itemsStruc.statCount == 1))
                {
                    quantity = ((statValue >> 8) / 256);

                }
            }

            //; 543 is key
            //; 529 is TP scroll
            //; 530 is ID scroll
            //; 518 is tome of TP
            //; 519 is tome of ID
            if (gameData.itemsStruc.txtFileNo == 543)
            {
                HUDItems_keys = HUDItems_keys + quantity;
            }
            else if (gameData.itemsStruc.txtFileNo == 529)
            {
                HUDItems_tpscrolls = HUDItems_tpscrolls + quantity;
                HUDItems_tpscrolls_locx = gameData.itemsStruc.itemx;
                HUDItems_tpscrolls_locy = gameData.itemsStruc.itemy;
            }
            else if (gameData.itemsStruc.txtFileNo == 530)
            {
                HUDItems_idscrolls = HUDItems_idscrolls + quantity;
                HUDItems_idscrolls_locx = gameData.itemsStruc.itemx;
                HUDItems_idscrolls_locy = gameData.itemsStruc.itemy;
            }
            else if (gameData.itemsStruc.txtFileNo == 518)
            {
                HUDItems_tpscrolls = HUDItems_tpscrolls + quantity;
                HUDItems_tpscrolls_locx = gameData.itemsStruc.itemx;
                HUDItems_tpscrolls_locy = gameData.itemsStruc.itemy;
            }
            else if (gameData.itemsStruc.txtFileNo == 519)
            {
                HasIDTome = true;
                HUDItems_idscrolls = HUDItems_idscrolls + quantity;
                HUDItems_idscrolls_locx = gameData.itemsStruc.itemx;
                HUDItems_idscrolls_locy = gameData.itemsStruc.itemy;
            }
        }
    }

    public void DumpBadItemsOnGround()
    {
        //return;
        //#########################################
        //reset trying to pick item if inventory is free of spots, meaning he try to grab while a mobs is in the way
        bool HasItemItemInInventory = false;
        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] >= 1)
            {
                HasItemItemInInventory = true;
                break;
            }
        }
        if (!HasItemItemInInventory && !gameData.itemsStruc.AlreadyEmptyedInventory)
        {
            gameData.itemsStruc.TriesToPickItemCount = 0;
            gameData.itemsStruc.AlreadyEmptyedInventory = true;
        }
        //#########################################

        bool HasAnyItemToDump = false;
        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] >= 1 && InventoryHasItemToID[i] == 0 && InventoryHasStashItem[i] == 0)
            {
                HasAnyItemToDump = true;
                break;
            }
        }

        if (HasAnyItemToDump)
        {
            gameData.uiScan.OpenUIMenu("invMenu");
            gameData.itemsStruc.GetBadItemsOnCursor();
            //gameData.itemsStruc.GetItems(false);

            //#######################################################
            //#######################################################
            //MANUAL ID THE ITEMS WITH TOME ID
            bool HasTownID = HasInventoryItemName("Tome of Identify");
            if (HasTownID && HUDItems_idscrolls > 0)
            {
                int tries2 = 0;
                while (gameData.inventoryStruc.HasUnidItemInInventory() && tries2 < 2)
                {
                    //gameData.SetGameStatus("INVENTORY-ID ITEMS");
                    gameData.form.SetProcessingTime();
                    if (!gameData.Running || !gameData.gameStruc.IsInGame())
                    {
                        break;
                    }

                    bool IdentifiedItem = false;
                    for (int i = 0; i < 40; i++)
                    {
                        if (gameData.inventoryStruc.InventoryItemNames[i] == "Tome of Identify")
                        {
                            Dictionary<string, int> itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                            itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);
                            gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                            gameData.WaitDelay(20);

                            for (int k = 0; k < 40; k++)
                            {
                                if (gameData.inventoryStruc.InventoryHasUnidItem[k] == 1 && CharConfig.InventoryDontCheckItem[i] == 0)
                                {
                                    itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(k);
                                    itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);
                                    gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"]);
                                    gameData.WaitDelay(100);
                                    gameData.shop.PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
                                    IdentifiedItem = true;
                                    gameData.itemsStruc.GetItems(false);   //get inventory again
                                    break;
                                }
                            }

                            break;
                        }
                    }
                    gameData.itemsStruc.GetItems(false);   //get inventory

                    if (!IdentifiedItem) tries2++;
                    else tries2 = 0;
                }

                gameData.itemsStruc.GetItems(false);   //get inventory
            }
            //#######################################################
            //#######################################################

            //place all bad items on ground
            for (int i = 0; i < 40; i++)
            {
                //Console.WriteLine("toID: " + InventoryHasItemToID[i] + ", tostash: " + InventoryHasStashItem[i]);
                gameData.uiScan.OpenUIMenu("invMenu");
                if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] >= 1 && InventoryHasItemToID[i] == 0 && InventoryHasStashItem[i] == 0)
                {
                    //pick key item from this slot
                    Dictionary<string, int> itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                    itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

                    if (InventoryItemNames[i].Contains("Healing") || InventoryItemNames[i].Contains("Mana") || InventoryItemNames[i].Contains("Rejuvenation"))
                    {
                        gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.WaitDelay(10);
                    }
                    else
                    {
                        /*gameData.stash.PickItem(itemScreenPos["x"], itemScreenPos["y"]);
                        if (!gameData.stash.PlaceItem(gameData.CenterX, gameData.CenterY))
                        {
                            gameData.stash.PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
                        }*/

                        gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.WaitDelay(12);
                        gameData.itemsStruc.GetItems(false);
                        //gameData.keyMouse.MouseClicc_RealPos(gameData.CenterX, gameData.CenterY);
                        gameData.stash.PlaceItem(gameData.CenterX, gameData.CenterY);
                        gameData.stash.PlaceItem(gameData.CenterX, gameData.CenterY);
                        gameData.WaitDelay(10);
                    }
                }
            }

            gameData.uiScan.CloseUIMenu("invMenu");
        }
    }

    public void ResetInventory()
    {
        InventoryHasItem = new int[40];
        InventoryHasItemToID = new int[40];
        InventoryHasUnidItem = new int[40];
        InventoryItemPointers = new long[40];
        InventoryItemNames = new string[40];
        InventoryItemQuality = new int[40];
        InventoryHasStashItem = new int[40];
        HUDItems_idscrolls = 0;
        HUDItems_tpscrolls = 0;
        HUDItems_keys = 0;
    }

    public void SetInventoryItem()
    {
        try
        {
            int FullIndex = ConvertXYToIndex(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);

            if (CharConfig.InventoryDontCheckItem[FullIndex] == 1
                && gameData.itemsStruc.ItemNAAME != "Key"
                && gameData.itemsStruc.ItemNAAME != "Tome of Town Portal"
                && gameData.itemsStruc.ItemNAAME != "Tome of Identify")
            {
                return;
            }

            InventoryHasItem[FullIndex] = 1;
            InventoryItemPointers[FullIndex] = gameData.itemsStruc.ItemPointerLocation;
            InventoryItemNames[FullIndex] = gameData.itemsStruc.ItemNAAME;
            InventoryItemQuality[FullIndex] = (int)gameData.itemsStruc.itemQuality;
            if (gameData.itemsAlert.ShouldKeepItem())
            {
                InventoryHasStashItem[FullIndex] = 1;
            }
            if (gameData.itemsAlert.ShouldPickItem(false))
            {
                InventoryHasItemToID[FullIndex] = 1;
            }

            if (!gameData.itemsStruc.identified)
            {
                InventoryHasUnidItem[FullIndex] = 1;
            }

            //Console.WriteLine(gameData.itemsStruc.ItemNAAME + ", StashItem:" + InventoryHasStashItem[FullIndex] + ", ItemToID:" + InventoryHasItemToID[FullIndex] + ", UnidItem:" + InventoryHasUnidItem[FullIndex]);
        }
        catch { }
    }

    public bool HasUnidItemInInventory()
    {
        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasUnidItem[i] >= 1)
            {
                return true;
            }
        }
        return false;
    }

    public int GetNextFreeSpaceInInventory()
    {
        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] == 0)
            {
                return i;
            }
        }
        return -1;
    }

    public bool HasInventoryItems()
    {
        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] >= 1)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasInventoryItemName(string ItemmN, bool OnlyFreeSpot = false)
    {
        for (int i = 0; i < 40; i++)
        {
            if (InventoryHasItem[i] >= 1)
            {
                if (InventoryItemNames[i] == ItemmN)
                {
                    if (OnlyFreeSpot)
                    {
                        if (CharConfig.InventoryDontCheckItem[i] == 0)
                        {
                            return true;
                        }
                    }
                    else return true;
                }
            }
        }
        return false;
    }

    public bool HasInventoryItemsForShop()
    {
        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.RunCowsScript && !((Cows)gameData.cows).ScriptDone && gameData.inventoryStruc.InventoryItemNames[i] == "Wirt's Leg") continue;
            if (CharConfig.RunCowsScript && !((Cows)gameData.cows).ScriptDone && gameData.inventoryStruc.InventoryItemNames[i] == "Tome of Town Portal") continue;

            if (CharConfig.InventoryDontCheckItem[i] == 0 && InventoryHasItem[i] >= 1 && InventoryHasStashItem[i] == 0)
            {
                return true;
            }
        }
        return false;
    }

    /*public bool HasInventoryItemAt(int AtIndex)
    {
        if (InventoryHasItem[AtIndex] >= 1)
        {
            return true;
        }
        return false;
    }*/
}
