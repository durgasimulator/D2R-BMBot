using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Enums;

public class Shop
{
    private GameData gameData;

    public bool FirstShopping = true;
    Dictionary<string, int> LastitemScreenPos = new Dictionary<string, int>();

    public bool ShopForSellingitem = false;
    public bool ShopForHP = false;
    public bool ShopForMana = false;
    public bool ShopForTP = false;
    public bool ShopForKey = false;
    public bool ShopForRegainHP = false;

    public bool ShopForTomeOfPortal = false; //cows level portal making
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public Dictionary<string, int> ConvertShopLocToScreenPos(int ThisX, int ThisY)
    {
        //starting at 1295,580 on screen for first item in inv, increment for 48px
        int xS = 185 + (ThisX * 48);
        int yS = 245 + (ThisY * 48);

        Dictionary<string, int> NewDict = new Dictionary<string, int>();
        NewDict["x"] = xS;
        NewDict["y"] = yS;
        return NewDict;
    }

    public bool ShouldShop()
    {
        if (gameData.inventoryStruc.HasInventoryItemName("Wirt's Leg")
            && !gameData.inventoryStruc.HasInventoryItemName("Tome of Town Portal", true))
        {
            ShopForTomeOfPortal = true;
            return true;
        }

        gameData.itemsStruc.GetItems(false);   //get inventory
        gameData.beltStruc.CheckForMissingPotions();

        ShopForSellingitem = gameData.inventoryStruc.HasInventoryItemsForShop();
        ShopForHP = gameData.beltStruc.MissingHPPot;
        ShopForMana = gameData.beltStruc.MissingManaPot;
        ShopForTP = (gameData.inventoryStruc.HUDItems_tpscrolls <= 2);
        ShopForKey = (gameData.inventoryStruc.HUDItems_keys <= 3) && CharConfig.UseKeys;
        ShopForRegainHP = gameData.playerScan.ShouldSeeShopForHP();

        if (gameData.inventoryStruc.HasInventoryItemsForShop()
            || gameData.beltStruc.MissingHPPot
            || gameData.beltStruc.MissingManaPot
            || gameData.inventoryStruc.HUDItems_tpscrolls <= 2
            || (gameData.inventoryStruc.HUDItems_keys <= 3 && CharConfig.UseKeys)
            || gameData.playerScan.ShouldSeeShopForHP())
        {
            return true;
        }
        return false;
    }

    public bool PlaceItem(int PosX, int PosY, bool ForceBadDetection = false)
    {
        int Tryy = 0;
        gameData.itemsStruc.GetItems(false);
        while (gameData.itemsStruc.ItemOnCursor && Tryy < 15)
        {
            gameData.keyMouse.MouseClicc(PosX, PosY);
            gameData.WaitDelay(10);
            gameData.itemsStruc.GetItems(false);   //get inventory again
            Tryy++;

            if (Tryy == 5 && ForceBadDetection)
            {
                gameData.itemsStruc.GetBadItemsOnCursor();
                Tryy = 10;
            }
        }
        if (Tryy >= 15)
        {
            return false;
        }
        return true;
    }

    public bool PickItem(int PosX, int PosY)
    {
        int Tryy = 0;
        gameData.itemsStruc.GetItems(false);
        while (!gameData.itemsStruc.ItemOnCursor && Tryy < 5)
        {
            gameData.keyMouse.MouseClicc(PosX, PosY);
            gameData.WaitDelay(10);
            gameData.itemsStruc.GetItems(false);   //get inventory again
            Tryy++;
        }
        if (Tryy >= 5)
        {
            return false;
        }
        return true;
    }

    public bool HasUnidItem = false;

    public void RunShopScript()
    {
        if (FirstShopping)
        {
            gameData.beltStruc.ForceHPPotionQty = 0;
            gameData.beltStruc.ForceMANAPotionQty = 0;

            FirstShopping = false;
        }
        HasUnidItem = false;

        if (gameData.inventoryStruc.HasInventoryItemName("Wirt's Leg")) ShopForTomeOfPortal = true;

        LastitemScreenPos = new Dictionary<string, int>();

        if (CharConfig.IDAtShop)
        {
            int tries2 = 0;
            int LastItemIdentified = 999;
            while (gameData.inventoryStruc.HasUnidItemInInventory() && tries2 < 2)
            {
                gameData.SetGameStatus("TOWN-SHOP-ID ITEMS");
                gameData.form.SetProcessingTime();
                if (!gameData.Running || !gameData.gameStruc.IsInGame())
                {
                    break;
                }
                if (gameData.itemsStruc.GetShopItem("Scroll of Identify"))
                {
                    Dictionary<string, int> itemScreenPos = ConvertShopLocToScreenPos(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);

                    gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.WaitDelay(20);
                    gameData.itemsStruc.GetItems(false);   //get inventory
                }

                bool IdentifiedItem = false;
                for (int i = 0; i < 40; i++)
                {
                    if (gameData.inventoryStruc.InventoryItemNames[i] == "Scroll of Identify")
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
                                PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
                                IdentifiedItem = true;
                                gameData.itemsStruc.GetItems(false);   //get inventory again

                                //#########################
                                //try selling this bad item
                                if (k == LastItemIdentified && gameData.inventoryStruc.InventoryHasStashItem[i] == 0)
                                {
                                    gameData.keyMouse.SendCTRL_CLICK(itemScreenPos["x"], itemScreenPos["y"]);
                                    gameData.WaitDelay(5);
                                    gameData.itemsStruc.GetItems(false);   //get inventory again
                                    if (gameData.itemsStruc.ItemOnCursor)
                                    {
                                        PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
                                        IdentifiedItem = false;
                                    }
                                }
                                LastItemIdentified = k;
                                //#########################
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
            if (gameData.inventoryStruc.HasUnidItemInInventory()) HasUnidItem = true;
            gameData.itemsStruc.GetItems(false);   //get inventory
        }

        //sell items
        //if (!gameData.townStruc.FastTowning)
        //{
        if (gameData.inventoryStruc.HasInventoryItems())
        {
            gameData.SetGameStatus("TOWN-SHOP-SELL ITEMS");
            for (int i = 0; i < 40; i++)
            {
                if (CharConfig.InventoryDontCheckItem[i] == 1) continue;
                if (gameData.inventoryStruc.InventoryHasItem[i] == 0) continue;
                if (ShopForTomeOfPortal && gameData.inventoryStruc.InventoryItemNames[i] == "Wirt's Leg") continue;

                //Console.WriteLine("HasStashItem:" + gameData.inventoryStruc.InventoryHasStashItem[i] + ", HasUnidItem:" + gameData.inventoryStruc.InventoryHasUnidItem[i]);

                if (gameData.inventoryStruc.InventoryHasStashItem[i] == 0
                    && gameData.inventoryStruc.InventoryHasUnidItem[i] == 0)
                {
                    //################
                    //GET ITEM SOLD INFOS
                    string SoldTxt = "";
                    Color ThisCol = Color.Black;
                    Dictionary<string, int> itemXYPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                    if (gameData.itemsStruc.GetSpecificItem(0, gameData.inventoryStruc.InventoryItemNames[i], itemXYPos["x"], itemXYPos["y"], gameData.playerScan.unitId, 0, true))
                    {
                        string ItemT = "";// gameData.itemsAlert.GetItemTypeText();
                        if (gameData.itemsAlert.GetItemTypeText() != "") ItemT = " && " + gameData.itemsAlert.GetItemTypeText();
                        SoldTxt = "Sold Item:" + gameData.itemsStruc.ItemNAAME + " (ID:" + gameData.itemsStruc.txtFileNo + ")" + ItemT + " && " + gameData.itemsStruc.GetQualityTextString() + " && " + gameData.itemsStruc.GetAllFlagsFromItem() + " && " + gameData.itemsStruc.GetAllValuesFromStats() + gameData.itemsStruc.GetItemsStashInfosTxt();
                        ThisCol = gameData.itemsStruc.GetColorFromQuality((int)gameData.itemsStruc.itemQuality);
                        if (gameData.itemsAlert.ShouldKeepItem())
                        {
                            continue;
                        }
                    }
                    //gameData.itemsViewer.TakeItemPicture();
                    //################

                    Dictionary<string, int> itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                    itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

                    int Tries = 0;
                    int MaxTries = 1;
                    while (true)
                    {
                        if (!gameData.Running || !gameData.gameStruc.IsInGame())
                        {
                            break;
                        }

                        gameData.itemsStruc.GetItems(false);   //get inventory again

                        //CTRL+Clic to send item into stash
                        //gameData.keyMouse.MouseMoveTo(itemScreenPos["x"], itemScreenPos["y"]);
                        //gameData.itemsViewer.TakeItemPicture();
                        gameData.keyMouse.SendCTRL_CLICK(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.WaitDelay(5);
                        gameData.itemsStruc.GetItems(false);   //get inventory again
                        PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);

                        //item still in inventory
                        if (gameData.inventoryStruc.InventoryHasItem[i] >= 1)
                        {
                            if (Tries > MaxTries)
                            {
                                break;
                            }
                            Tries++;
                        }
                        else
                        {
                            if (SoldTxt != "")
                            {
                                gameData.form.method_1_SoldItems(SoldTxt, ThisCol);
                                //gameData.itemsViewer.AddBufferPicture("Sold");
                            }
                            break;
                        }
                    }

                    LastitemScreenPos = itemScreenPos;

                    if (Tries > MaxTries)
                    {
                        gameData.method_1("Item didn't sell correctly!", Color.OrangeRed);
                        break;
                    }
                }

                if (!gameData.Running || !gameData.gameStruc.IsInGame())
                {
                    break;
                }
            }
        }
        //}

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            return;
        }

        //gameData.method_1("MISS HP: " + gameData.beltStruc.MissingHPPot);
        //gameData.method_1("MISS MANA: " + gameData.beltStruc.MissingManaPot);
        //gameData.method_1("TP QTY: " + gameData.inventoryStruc.HUDItems_tpscrolls);
        //gameData.method_1("ID QTY: " + gameData.inventoryStruc.HUDItems_idscrolls);

        //buy potions
        int tries = 0;
        int StartQty = gameData.beltStruc.HPQuantity;

        string BuyingThisPotion = "Super Healing Potion";
        if (!gameData.itemsStruc.GetShopItem("Super Healing Potion") && BuyingThisPotion == "Super Healing Potion") BuyingThisPotion = "Greater Healing Potion";
        if (!gameData.itemsStruc.GetShopItem("Greater Healing Potion") && BuyingThisPotion == "Greater Healing Potion") BuyingThisPotion = "Healing Potion";
        if (!gameData.itemsStruc.GetShopItem("Healing Potion") && BuyingThisPotion == "Healing Potion") BuyingThisPotion = "Light Healing Potion";
        if (!gameData.itemsStruc.GetShopItem("Light Healing Potion") && BuyingThisPotion == "Light Healing Potion") BuyingThisPotion = "Minor Healing Potion";
        if (!gameData.itemsStruc.GetShopItem("Minor Healing Potion") && BuyingThisPotion == "Minor Healing Potion") BuyingThisPotion = "Potion of Life";

        while (gameData.beltStruc.MissingHPPot && tries < 2)
        {
            gameData.SetGameStatus("TOWN-SHOP-BUY HP POTIONS");
            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                break;
            }
            if (gameData.itemsStruc.GetShopItem(BuyingThisPotion))
            {
                Dictionary<string, int> itemScreenPos = ConvertShopLocToScreenPos(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);

                int ShopCount = 4;
                if (gameData.beltStruc.ForceHPPotionQty > 0)
                {
                    ShopCount = gameData.beltStruc.ForceHPPotionQty - gameData.beltStruc.HPQuantity;
                }
                else
                {
                    ShopCount = 8 - gameData.beltStruc.HPQuantity;
                }

                for (int i = 0; i < ShopCount; i++)
                {
                    //gameData.SendSHIFT_RIGHTCLICK(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.WaitDelay(10);
                }

                gameData.itemsStruc.UsePotionNotInRightSpot = false; //dont use pot if not in correct spot
                gameData.itemsStruc.GetItems(false);   //get inventory

                //####
                if (gameData.inventoryStruc.HasInventoryItemName(BuyingThisPotion) || gameData.beltStruc.HasPotInBadSpot)
                {
                    int IncreaseCount = 0;
                    while (gameData.beltStruc.HPQuantity != gameData.beltStruc.ForceHPPotionQty && IncreaseCount < 15)
                    {
                        gameData.patternsScan.IncreaseV1Scanning();
                        IncreaseCount++;
                        gameData.itemsStruc.GetItems(false);
                    }

                    //gameData.method_1("FORCING HP POT QTY: " + gameData.beltStruc.HPQuantity, Color.Red);
                    gameData.beltStruc.ForceHPPotionQty = gameData.beltStruc.HPQuantity; //reset qty in belt
                    if (gameData.beltStruc.HasPotInBadSpot)
                    {
                        gameData.beltStruc.ForceHPPotionQty -= 1;
                        gameData.itemsStruc.UsePotionNotInRightSpot = true;
                        gameData.itemsStruc.GetItems(false);   //get inventory to use pot in bad spot
                        gameData.itemsStruc.GetItems(false);   //get inventory to use pot in bad spot
                    }
                    //gameData.method_1("FORCING HP POT QTY: " + gameData.beltStruc.ForceHPPotionQty, Color.Red);
                    break;
                }
                //####

                gameData.itemsStruc.UsePotionNotInRightSpot = true;
                gameData.beltStruc.CheckForMissingPotions();
            }

            if (gameData.beltStruc.HPQuantity == StartQty)
            {
                tries++;
            }
            StartQty = gameData.beltStruc.HPQuantity;
        }


        //buy mana
        tries = 0;
        StartQty = gameData.beltStruc.ManyQuantity;

        BuyingThisPotion = "Super Mana Potion";
        if (!gameData.itemsStruc.GetShopItem("Super Mana Potion") && BuyingThisPotion == "Super Mana Potion") BuyingThisPotion = "Greater Mana Potion";
        if (!gameData.itemsStruc.GetShopItem("Greater Mana Potion") && BuyingThisPotion == "Greater Mana Potion") BuyingThisPotion = "Mana Potion";
        if (!gameData.itemsStruc.GetShopItem("Mana Potion") && BuyingThisPotion == "Mana Potion") BuyingThisPotion = "Light Mana Potion";
        if (!gameData.itemsStruc.GetShopItem("Light Mana Potion") && BuyingThisPotion == "Light Mana Potion") BuyingThisPotion = "Minor Mana Potion";

        while (gameData.beltStruc.MissingManaPot && tries < 2)
        {
            gameData.SetGameStatus("TOWN-SHOP-BUY MANA POTIONS");
            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                break;
            }
            if (gameData.itemsStruc.GetShopItem(BuyingThisPotion))
            {
                Dictionary<string, int> itemScreenPos = ConvertShopLocToScreenPos(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);

                int ShopCount = 4;
                if (gameData.beltStruc.ForceHPPotionQty > 0)
                {
                    ShopCount = gameData.beltStruc.ForceMANAPotionQty - gameData.beltStruc.ManyQuantity;
                }
                else
                {
                    ShopCount = 8 - gameData.beltStruc.ManyQuantity;
                }

                for (int i = 0; i < ShopCount; i++)
                {
                    //gameData.SendSHIFT_RIGHTCLICK(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.WaitDelay(10);
                }

                gameData.itemsStruc.UsePotionNotInRightSpot = false; //dont use pot if not in correct spot
                gameData.itemsStruc.GetItems(false);   //get inventory

                //####
                if (gameData.inventoryStruc.HasInventoryItemName(BuyingThisPotion) || gameData.beltStruc.HasPotInBadSpot)
                {
                    int IncreaseCount = 0;
                    while (gameData.beltStruc.ManyQuantity != gameData.beltStruc.ForceMANAPotionQty && IncreaseCount < 10)
                    {
                        gameData.patternsScan.IncreaseV1Scanning();
                        IncreaseCount++;
                        gameData.itemsStruc.GetItems(false);
                    }

                    gameData.beltStruc.ForceMANAPotionQty = gameData.beltStruc.ManyQuantity; //reset qty in belt
                    if (gameData.beltStruc.HasPotInBadSpot)
                    {
                        gameData.beltStruc.ForceMANAPotionQty -= 1;
                        gameData.itemsStruc.UsePotionNotInRightSpot = true;
                        gameData.itemsStruc.GetItems(false);   //get inventory to use pot in bad spot
                        gameData.itemsStruc.GetItems(false);   //get inventory to use pot in bad spot
                    }
                    break;
                }
                //####

                gameData.itemsStruc.UsePotionNotInRightSpot = true;
                gameData.beltStruc.CheckForMissingPotions();
            }

            if (gameData.beltStruc.ManyQuantity == StartQty)
            {
                tries++;
            }
            StartQty = gameData.beltStruc.ManyQuantity;
        }


        //buy tp
        tries = 0;
        StartQty = gameData.inventoryStruc.HUDItems_tpscrolls;
        while (gameData.inventoryStruc.HUDItems_tpscrolls < 20 && tries < 1)
        {
            gameData.SetGameStatus("TOWN-SHOP-BUY TP'S");
            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                break;
            }
            if (gameData.itemsStruc.GetShopItem("Scroll of Town Portal"))
            {
                Dictionary<string, int> itemScreenPos = ConvertShopLocToScreenPos(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);

                int ShopCount = 20 - gameData.inventoryStruc.HUDItems_tpscrolls;
                for (int i = 0; i < ShopCount; i++)
                {
                    //gameData.SendSHIFT_RIGHTCLICK(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.WaitDelay(10);
                }
                gameData.itemsStruc.GetItems(false);   //get inventory
            }

            if (gameData.inventoryStruc.HUDItems_tpscrolls == StartQty)
            {
                tries++;
            }
            StartQty = gameData.inventoryStruc.HUDItems_tpscrolls;
        }

        //buy id
        if (gameData.inventoryStruc.HasIDTome)
        {
            tries = 0;
            StartQty = gameData.inventoryStruc.HUDItems_idscrolls;
            while (gameData.inventoryStruc.HUDItems_idscrolls < 20 && tries < 1)
            {
                gameData.SetGameStatus("TOWN-SHOP-BUY ID'S");
                if (!gameData.Running || !gameData.gameStruc.IsInGame())
                {
                    break;
                }
                if (gameData.itemsStruc.GetShopItem("Scroll of Identify"))
                {
                    Dictionary<string, int> itemScreenPos = ConvertShopLocToScreenPos(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);

                    int ShopCount = 20 - gameData.inventoryStruc.HUDItems_idscrolls;
                    for (int i = 0; i < ShopCount; i++)
                    {
                        //gameData.SendSHIFT_RIGHTCLICK(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.WaitDelay(10);
                    }
                    gameData.itemsStruc.GetItems(false);   //get inventory
                }

                if (gameData.inventoryStruc.HUDItems_idscrolls == StartQty)
                {
                    tries++;
                }
                StartQty = gameData.inventoryStruc.HUDItems_idscrolls;
            }
        }

        //buy key
        if (CharConfig.UseKeys)
        {
            gameData.inventoryStruc.VerifyKeysInventory();
            tries = 0;
            StartQty = gameData.inventoryStruc.HUDItems_keys;
            while (gameData.inventoryStruc.HUDItems_keys <= 8 && tries < 1)
            {
                gameData.SetGameStatus("TOWN-SHOP-BUY KEYS");
                if (!gameData.Running || !gameData.gameStruc.IsInGame())
                {
                    break;
                }
                if (gameData.itemsStruc.GetShopItem("Key"))
                {
                    Dictionary<string, int> itemScreenPos = ConvertShopLocToScreenPos(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);

                    gameData.keyMouse.SendSHIFT_RIGHTCLICK(itemScreenPos["x"], itemScreenPos["y"]);
                    //gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.WaitDelay(20);
                    gameData.itemsStruc.GetItems(false);   //get inventory

                    //Buy keys again to fill inventory
                    /*if (StartQty == 0)
                    {
                        gameData.keyMouse.SendSHIFT_RIGHTCLICK(itemScreenPos["x"], itemScreenPos["y"]);
                        //gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.WaitDelay(20);
                        gameData.itemsStruc.GetItems(false);   //get inventory
                    }*/
                }

                if (gameData.inventoryStruc.HUDItems_keys == StartQty)
                {
                    tries++;
                }
                StartQty = gameData.inventoryStruc.HUDItems_keys;
            }
        }

        //buy tome of portal for cows level
        if (ShopForTomeOfPortal)
        {
            bool HasTownPortal = gameData.inventoryStruc.HasInventoryItemName("Tome of Town Portal", true);
            tries = 0;
            while (!HasTownPortal && tries < 1)
            {
                gameData.SetGameStatus("TOWN-SHOP-BUY TOME PORTAL");
                if (!gameData.Running || !gameData.gameStruc.IsInGame())
                {
                    break;
                }
                if (gameData.itemsStruc.GetShopItem("Tome of Town Portal"))
                {
                    Dictionary<string, int> itemScreenPos = ConvertShopLocToScreenPos(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);

                    gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"]);
                    gameData.WaitDelay(20);
                    gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"] + 15);
                    gameData.WaitDelay(10);
                    gameData.itemsStruc.GetItems(false);   //get inventory
                }

                HasTownPortal = gameData.inventoryStruc.HasInventoryItemName("Tome of Town Portal", true);
                if (!HasTownPortal) tries++;
            }

            if (HasTownPortal) ShopForTomeOfPortal = false;
        }

        //ShopBot
        if (CharConfig.RunShopBotScript && !((ShopBot)gameData.shopBot).ScriptDone && ((ShopBot)gameData.shopBot).CurrentStep > 0)
        {
            gameData.itemsStruc.ShopBotGetPurchaseItems();
        }
        //gameData.itemsStruc.ShopBotGetPurchaseItems();
    }
}
