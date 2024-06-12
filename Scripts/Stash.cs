using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Enums;

public class Stash
{
    GameData gameData = GameData.Instance;
    public bool StashFull = false;
    Dictionary<string, int> LastitemScreenPos = new Dictionary<string, int>();

    public int RunningScriptCount = 0;

    public bool MakingCowPortal = false;
    public int DeposingGoldCount = 0;


    public void RunStashScript()
    {
        if (StashFull) return;

        gameData.uiScan.readUI();
        if (gameData.uiScan.GetMenuActive("npcInteract")) gameData.uiScan.CloseThisMenu("npcInteract");
        //if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

        gameData.WaitDelay(35);
        LastitemScreenPos = new Dictionary<string, int>();

        //move inventory into stash
        for (int i = 0; i < 40; i++)
        {
            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                break;
            }

            if ((CharConfig.InventoryDontCheckItem[i] == 0 && gameData.inventoryStruc.InventoryHasStashItem[i] >= 1)
                || (MakingCowPortal && gameData.inventoryStruc.InventoryItemNames[i] == "Tome of Town Portal"))
            {
                //################
                //GET ITEM (UNIQUE GC, GHEED, TORCH, ANNI)
                bool IsUniqueSpecial = false;
                Dictionary<string, int> itemXYPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                if (gameData.itemsStruc.GetSpecificItem(0, gameData.inventoryStruc.InventoryItemNames[i], itemXYPos["x"], itemXYPos["y"], gameData.playerScan.unitId, 0))
                {
                    if ((gameData.itemsStruc.ItemNAAME == "Small Charm"
                        || gameData.itemsStruc.ItemNAAME == "Large Charm"
                        || gameData.itemsStruc.ItemNAAME == "Grand Charm")
                        && gameData.itemsStruc.itemQuality == 7) //Unique
                    {
                        IsUniqueSpecial = true;
                    }
                }
                //################
                gameData.SetGameStatus("TOWN-STASH-ITEM:" + gameData.inventoryStruc.InventoryItemNames[i]);
                gameData.form.method_1_Items("Stashed: " + gameData.inventoryStruc.InventoryItemNames[i], gameData.itemsStruc.GetColorFromQuality(gameData.inventoryStruc.InventoryItemQuality[i]));

                Dictionary<string, int> itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(i);
                itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

                int TryStashCount = 0;
                if (IsUniqueSpecial)
                {
                    TryStashCount = 1;
                    gameData.keyMouse.MouseClicc(340, 200);   //clic shared stash1
                }
                while (true)
                {
                    int Tries = 0;
                    int MaxTries = 1;
                    while (true)
                    {
                        if (!gameData.Running || !gameData.gameStruc.IsInGame())
                        {
                            break;
                        }
                        gameData.uiScan.readUI();
                        if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

                        //CTRL+Clic to send item into stash
                        gameData.keyMouse.SendCTRL_CLICK(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.WaitDelay(5);
                        gameData.itemsStruc.GetItems(false);   //get inventory again
                        gameData.SetGameStatus("TOWN-STASH-ITEM:" + gameData.inventoryStruc.InventoryItemNames[i] + " (" + (Tries + 1) + "/" + MaxTries + ")");
                        PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
                        //PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);

                        //item still in inventory
                        if (gameData.inventoryStruc.InventoryHasStashItem[i] >= 1)
                        {
                            if (Tries > MaxTries)
                            {
                                break;
                            }
                            Tries++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    LastitemScreenPos = itemScreenPos;

                    if (!gameData.Running || !gameData.gameStruc.IsInGame())
                    {
                        break;
                    }
                    gameData.uiScan.readUI();
                    if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

                    //swap stash
                    if (Tries > MaxTries)
                    {
                        PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);

                        //200-340-450-600
                        if (TryStashCount == 0)
                        {
                            gameData.keyMouse.MouseClicc(200, 200);   //clic stash1
                        }
                        if (TryStashCount == 1)
                        {
                            gameData.keyMouse.MouseClicc(340, 200);   //clic shared stash1
                        }
                        if (TryStashCount == 2)
                        {
                            gameData.keyMouse.MouseClicc(450, 200);   //clic shared stash2
                        }
                        if (TryStashCount == 3)
                        {
                            gameData.keyMouse.MouseClicc(600, 200);   //clic shared stash3
                        }
                        if (TryStashCount >= 4)
                        {
                            RunningScriptCount++;
                            if (RunningScriptCount >= CharConfig.StashFullTries) StashFull = true;
                            //StashFull = true; //##################################################
                            i = 40; //stash is full, dont try others items to stash
                            break;
                        }
                        TryStashCount++;
                        Tries = 0;
                    }
                    else
                    {
                        break;
                    }
                }


                PlaceItem(itemScreenPos["x"], itemScreenPos["y"]);
            }
        }

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            return;
        }
        gameData.uiScan.readUI();
        if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

        //deposit gold
        if (DeposingGoldCount == 0) gameData.keyMouse.MouseClicc(200, 200);   //clic stash1
        if (DeposingGoldCount == 1) gameData.keyMouse.MouseClicc(340, 200);   //clic shared stash1
        if (DeposingGoldCount == 2) gameData.keyMouse.MouseClicc(450, 200);   //clic shared stash2
        if (DeposingGoldCount == 3) gameData.keyMouse.MouseClicc(600, 200);   //clic shared stash3
        DeposingGoldCount++;
        if (DeposingGoldCount > 3) DeposingGoldCount = 0;

        if (gameData.playerScan.PlayerGoldInventory > 0)
        {
            gameData.SetGameStatus("TOWN-STASH-DEPOSIT GOLD");
            gameData.keyMouse.MouseClicc(1450, 790);  //clic deposit
            gameData.WaitDelay(25);
            gameData.keyMouse.MouseClicc(820, 580);  //clic ok on deposit
            gameData.WaitDelay(25);
            gameData.playerScan.PlayerGoldInventory = 0;
        }

        //craft/cube item script here ###
        gameData.playerScan.GetPositions();
        gameData.itemsStruc.GetItems(false);
        gameData.cubing.PerformCubing();

        gameData.inventoryStruc.VerifyKeysInventory();

    }

    //Place item to cube
    public bool PlaceItemShift(int PosX, int PosY)
    {
        int Tryy = 0;
        while (gameData.itemsStruc.ItemOnCursor && Tryy < 5)
        {
            gameData.keyMouse.SendSHIFT_CLICK(PosX, PosY);
            gameData.WaitDelay(5);
            gameData.itemsStruc.GetItems(false);   //get inventory again
            Tryy++;
        }
        if (Tryy >= 5)
        {
            return false;
        }
        return true;
    }

    public bool PlaceItem(int PosX, int PosY)
    {
        int Tryy = 0;
        while (gameData.itemsStruc.ItemOnCursor && Tryy < 5)
        {
            gameData.keyMouse.MouseClicc(PosX, PosY);
            gameData.WaitDelay(5);
            gameData.itemsStruc.GetItems(false);   //get inventory again
            Tryy++;
        }
        if (Tryy >= 5)
        {
            return false;
        }
        return true;
    }

    public bool PickItem(int PosX, int PosY)
    {
        gameData.itemsStruc.GetBadItemsOnCursor();

        int Tryy = 0;
        while (!gameData.itemsStruc.ItemOnCursor && Tryy < 5)
        {
            gameData.keyMouse.MouseClicc(PosX, PosY);
            gameData.WaitDelay(5);
            gameData.itemsStruc.GetItems(false);   //get inventory again
            Tryy++;
        }
        if (Tryy >= 5)
        {
            return false;
        }
        return true;
    }
}
