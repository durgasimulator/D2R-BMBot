using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Town
{
    private GameData gameData;

    public int TownAct = 0;
    public bool Towning = true;
    public bool ForcedTowning = false;
    public bool FastTowning = false;
    public bool IsInTown = false;
    public bool TPSpawned = false;
    public bool UseLastTP = false;
    public int ScriptTownAct = 5;       //default should be 0
    public int TriedToCainCount = 0;
    public int TriedToCainCount2 = 0;
    public int TriedToStashCount = 0;
    public int TriedToGambleCount = 0;
    public int TriedToShopCount = 0;
    public int TriedToShopCount2 = 0;
    public int TriedToRepairCount = 0;
    public int TriedToMercCount = 0;
    public int TriedToUseTPCount = 0;
    public int CurrentScript = 0;
    public bool TownScriptDone = false;

    public uint LastUsedTPID = 0;
    public int LastUsedTPCount = 0;
    public List<uint> IgnoredTPList = new List<uint>();
    public List<uint> IgnoredWPList = new List<uint>();
    public bool FirstTown = true;
    public bool CainNotFoundAct1 = false;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void StopTowningReturn()
    {
        CurrentScript = 0;
        TriedToCainCount = 0;
        TriedToCainCount2 = 0;
        TriedToStashCount = 0;
        TriedToGambleCount = 0;
        TriedToShopCount = 0;
        TriedToShopCount2 = 0;
        TriedToRepairCount = 0;
        TriedToMercCount = 0;
        TriedToUseTPCount = 0;
        Towning = false;
        TownScriptDone = true;
        //FastTowning = false;
        //ForcedTowning = false;
        //UseLastTP = true;

        if (gameData.PublicGame)
        {
            DateTime StartWaitingChangeArea = DateTime.Now;
            while (GetInTown() && (DateTime.Now - StartWaitingChangeArea).TotalSeconds < CharConfig.TownSwitchAreaDelay)
            {
                gameData.playerScan.GetPositions();
                gameData.overlayForm.UpdateOverlay();
                gameData.gameStruc.CheckChickenGameTime();
                gameData.itemsStruc.GetItems(false);
            }

            if ((DateTime.Now - StartWaitingChangeArea).TotalSeconds < CharConfig.TownSwitchAreaDelay)
            {
                gameData.WaitDelay(CharConfig.PublicGameTPRespawnDelay);
                gameData.townStruc.SpawnTP();
            }

        }
    }

    public void RunTownScript()
    {
        //Console.WriteLine("Fast town: " + FastTowning);
        if (!ShouldBeInTown())
        {
            if (!FastTowning) return;
            else
            {
                if (MoveToTPOrWPSpot())
                {
                    GetCorpse();
                    gameData.stash.RunningScriptCount = 0;
                    StopTowningReturn();
                }
            }
        }

        gameData.SetGameStatus("TOWN");

        //dead leave game
        if (gameData.playerScan.PlayerDead || gameData.potions.ForceLeave)
        {
            gameData.potions.ForceLeave = true;
            gameData.baalLeech.SearchSameGamesAsLastOne = false;
            gameData.LeaveGame(false);
            gameData.form.IncreaseDeadCount();
            return;
        }

        gameData.gameStruc.CheckChickenGameTime();

        //item grab only -> no town
        if (Towning && CharConfig.RunItemGrabScriptOnly)
        {
            StopTowningReturn();
            return;
        }

        if (!GetInTown())
        {
            gameData.SetGameStatus("TOWN-TP TO TOWN");
            gameData.potions.CheckIfWeUsePotion();

            if (FastTowning) UseLastTP = true;

            if (FastTowning && !gameData.shop.ShouldShop())
            {
                StopTowningReturn();
                return;
            }

            if (TriedToUseTPCount >= 5)
            {
                gameData.method_1("No TP found nearby when trying to Town", Color.Red);
                gameData.LeaveGame(false);
                return;
            }

            //fix for cows script
            if ((Enums.Area) gameData.playerScan.levelNo == Enums.Area.MooMooFarm)
            {
                ((Cows)gameData.cows).HadWirtsLeg = true;
            }

            if (TPSpawned)
            {
                int IncreaseCount = 0;
                while (!gameData.objectsStruc.GetObjects("TownPortal", true, IgnoredTPList, 999, CharConfig.PlayerCharName) && IncreaseCount < 10)
                {
                    gameData.patternsScan.IncreaseV1Scanning();
                    IncreaseCount++;
                }

                //select the spawned TP
                if (gameData.objectsStruc.GetObjects("TownPortal", true, IgnoredTPList, 999, CharConfig.PlayerCharName))
                //if (gameData.objectsStruc.GetObjects("TownPortal", gameData.playerScan.unitId))
                {
                    if (gameData.objectsStruc.itemx != 0 && gameData.objectsStruc.itemy != 0)
                    {
                        //if (gameData.mover.MoveToLocation(gameData.objectsStruc.itemx, gameData.objectsStruc.itemy))
                        //{
                        gameData.method_1("Trying to use TP ID: " + gameData.objectsStruc.ObjectUnitID, Color.OrangeRed);

                        if (LastUsedTPID != gameData.objectsStruc.ObjectUnitID)
                        {
                            LastUsedTPID = gameData.objectsStruc.ObjectUnitID;
                            LastUsedTPCount = 0;
                        }
                        else
                        {
                            gameData.mover.MoveToLocation(gameData.objectsStruc.itemx + (LastUsedTPCount * 2), gameData.objectsStruc.itemy + (LastUsedTPCount * 2));

                            LastUsedTPCount++;
                            if (LastUsedTPCount >= 4)
                            {
                                IgnoredTPList.Add(LastUsedTPID);
                            }
                        }

                        GetCorpse();
                        CurrentScript = 0;
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                        gameData.keyMouse.PressKeyHold(CharConfig.KeyForceMovement);
                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                        gameData.WaitDelay(50);
                        //}
                    }
                    else
                    {
                        TPSpawned = false;
                        TriedToUseTPCount++;
                        if (TriedToUseTPCount >= 3) IgnoredTPList.Clear(); //try to clear again the ignored tp list!
                    }
                }
                else
                {
                    if (TriedToUseTPCount == 3 || TriedToUseTPCount == 4)
                    {
                        gameData.method_1("Trying to use Unkown TP ID!", Color.OrangeRed);

                        CurrentScript = 0;
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.playerScan.xPosFinal - 2, gameData.playerScan.yPosFinal);

                        gameData.keyMouse.PressKeyHold(CharConfig.KeyForceMovement);
                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                        gameData.WaitDelay(50);

                        TPSpawned = false;
                        TriedToUseTPCount++;
                        if (TriedToUseTPCount >= 3) IgnoredTPList.Clear(); //try to clear again the ignored tp list!
                    }
                    else
                    {
                        TPSpawned = false;
                        TriedToUseTPCount++;
                        if (TriedToUseTPCount >= 3) IgnoredTPList.Clear(); //try to clear again the ignored tp list!
                    }
                }
            }
            else
            {
                SpawnTP(true);
                gameData.WaitDelay(CharConfig.TPRespawnDelay);
            }
        }
        else
        {
            gameData.battle.DoingBattle = false;
            gameData.battle.ClearingArea = false;
            gameData.battle.MoveTryCount = 0;

            //Console.WriteLine("town... " + CurrentScript);

            //switch town
            if (CurrentScript == 0)
            {
                if (IsInRightTown())
                {
                    //Grab Corpse
                    if (gameData.itemsStruc.ItemsEquiped <= 2 && FirstTown)
                    {
                        //int Tries = 0;
                        //while (Tries < 5)
                        //{
                        //Console.WriteLine("Corpse found method2");
                        gameData.method_1("Grab corpse #3", Color.Red);
                        gameData.WaitDelay(100);
                        //Clic corpse
                        FirstTown = false;
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal);

                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X - 45, itemScreenPos.Y - 5);
                        //gameData.WaitDelay(100);
                        //Tries++;
                        //}
                    }

                    GetCorpse();
                    CurrentScript++;
                    return;
                }

                if (CurrentScript == 0)
                {
                    if (!IsInRightTown())
                    {
                        gameData.SetGameStatus("TOWN-SWITCH TOWN");
                        GoToWPArea();
                    }
                }
            }

            bool AlreadyGoneToShop = false;

            //ID Items
            if (CurrentScript == 1)
            {
                if (!gameData.inventoryStruc.HasUnidItemInInventory() || (FastTowning && gameData.itemsStruc.TriesToPickItemCount < CharConfig.MaxItemGrabTries))
                {
                    CurrentScript++;
                    return;
                }

                if (TriedToCainCount2 >= CharConfig.MaxItemIDTries)
                {
                    CurrentScript++;
                    return;
                }

                //Go see Cain if we cannot ID at shop
                if (CharConfig.IDAtShop && gameData.shop.HasUnidItem && TriedToCainCount2 < CharConfig.MaxItemIDTries)
                {
                    gameData.SetGameStatus("TOWN-CAIN (" + (TriedToCainCount2 + 1) + "/" + CharConfig.MaxItemIDTries + ")");
                    MoveToCain();
                    AlreadyGoneToShop = false;
                    TriedToCainCount = CharConfig.MaxItemIDTries;
                    TriedToCainCount2++;
                    return;
                }

                if (CurrentScript == 1)
                {
                    if (TriedToCainCount >= CharConfig.MaxItemIDTries)
                    {
                        if (CharConfig.IDAtShop) gameData.shop.HasUnidItem = true;
                        else
                        {
                            TriedToCainCount = CharConfig.MaxItemIDTries;
                            TriedToCainCount2 = CharConfig.MaxItemIDTries;
                            CurrentScript++;
                        }
                        return;
                    }

                    if (gameData.inventoryStruc.HasUnidItemInInventory() && TriedToCainCount < CharConfig.MaxItemIDTries)
                    {
                        if (!CharConfig.IDAtShop)
                        {
                            gameData.SetGameStatus("TOWN-CAIN (" + (TriedToCainCount + 1) + "/" + CharConfig.MaxItemIDTries + ")");
                            MoveToCain();
                            TriedToCainCount++;
                        }
                        else
                        {
                            AlreadyGoneToShop = true;
                            gameData.SetGameStatus("TOWN-SHOP (IDENTIFY ITEMS) (" + (TriedToCainCount + 1) + "/" + CharConfig.MaxItemIDTries + ")");
                            MoveToStore();
                            TriedToCainCount++;
                        }
                    }
                }
            }

            //relive merc
            if (CurrentScript == 2)
            {
                bool ShouldReliveMerc = false;
                if (CharConfig.UsingMerc)
                {
                    gameData.mercStruc.GetMercInfos();
                    ShouldReliveMerc = !gameData.mercStruc.MercAlive;
                }

                if (!ShouldReliveMerc || TriedToMercCount >= CharConfig.MaxMercReliveTries
                    || (gameData.playerScan.PlayerGoldInventory + gameData.playerScan.PlayerGoldInStash) < 75000)
                {
                    CurrentScript++;
                    return;
                }

                if (CurrentScript == 2)
                {
                    if (ShouldReliveMerc && TriedToMercCount < CharConfig.MaxMercReliveTries
                        && (gameData.playerScan.PlayerGoldInventory + gameData.playerScan.PlayerGoldInStash) >= 75000)
                    {
                        gameData.SetGameStatus("TOWN-MERC (" + (TriedToMercCount + 1) + "/" + CharConfig.MaxMercReliveTries + ")");
                        MoveToMerc();
                        TriedToMercCount++;
                    }
                    else
                    {
                        TriedToMercCount++;
                    }
                }

            }

            //stash items
            if (CurrentScript == 3)
            {
                if (gameData.inventoryStruc.HasUnidItemInInventory()
                    && (!FastTowning || (FastTowning && gameData.itemsStruc.TriesToPickItemCount >= CharConfig.MaxItemGrabTries))
                    && TriedToCainCount2 < CharConfig.MaxItemIDTries
                    && TriedToCainCount < CharConfig.MaxItemIDTries)
                {
                    //return to identify script, still contain unid item
                    CurrentScript = 1;
                    gameData.itemsStruc.TriesToPickItemCount = -1;
                    return;
                }

                if ((!gameData.inventoryStruc.ContainStashItemInInventory() && (gameData.playerScan.PlayerGoldInventory < 35000))
                            || TriedToStashCount >= CharConfig.MaxItemStashTries || (FastTowning && gameData.itemsStruc.TriesToPickItemCount < CharConfig.MaxItemGrabTries && gameData.itemsStruc.TriesToPickItemCount >= 0))
                {
                    CurrentScript++;
                    return;
                }

                if (CurrentScript == 3)
                {
                    if ((gameData.inventoryStruc.ContainStashItemInInventory() || (gameData.playerScan.PlayerGoldInventory >= 35000)) && TriedToStashCount < CharConfig.MaxItemStashTries)
                    {
                        string DescTxt = "";
                        if (gameData.inventoryStruc.ContainStashItemInInventory()) DescTxt += " (ITEM)";
                        if ((gameData.playerScan.PlayerGoldInventory >= 35000)) DescTxt += " (GOLD)";
                        gameData.SetGameStatus("TOWN-STASH" + DescTxt + " (" + (TriedToStashCount + 1) + "/" + CharConfig.MaxItemStashTries + ")");
                        MoveToStash(true);
                        TriedToStashCount++;
                    }
                }
            }

            //gamble
            if (CurrentScript == 4)
            {
                if (!CharConfig.GambleGold)
                {
                    CurrentScript++;
                    return;
                }
                if (!gameData.gamble.CanGamble() || TriedToGambleCount >= CharConfig.MaxGambleTries || FastTowning)
                {
                    CurrentScript++;
                    return;
                }

                if (CurrentScript == 4)
                {
                    if (gameData.gamble.CanGamble() && TriedToGambleCount < CharConfig.MaxGambleTries && !FastTowning)
                    {
                        TriedToStashCount = 0;
                        gameData.SetGameStatus("TOWN-GAMBLE (" + (TriedToGambleCount + 1) + "/" + CharConfig.MaxGambleTries + ")");
                        MoveToGamble();
                        TriedToGambleCount++;
                    }
                }
            }

            //buy potions,tp,etc
            if (CurrentScript == 5)
            {
                if (CharConfig.IDAtShop && AlreadyGoneToShop)
                {
                    CurrentScript++;
                    return;
                }

                gameData.itemsStruc.GetItems(false);
                if ((gameData.inventoryStruc.ContainStashItemInInventory())
                    && !FastTowning)
                {
                    //return to stash script, still contain item
                    TriedToStashCount = 0;
                    CurrentScript = 3;
                }
                else
                {
                    if (!gameData.shop.ShouldShop() || TriedToShopCount >= CharConfig.MaxShopTries)
                    {
                        //Console.WriteLine("town shop done");
                        CurrentScript++;
                        return;
                    }

                    if (CurrentScript == 5)
                    {
                        if (gameData.shop.ShouldShop() && TriedToShopCount < CharConfig.MaxShopTries)
                        {
                            string DescTxt = "";
                            if (gameData.shop.ShopForSellingitem) DescTxt += " (SELL)";
                            if (gameData.shop.ShopForHP) DescTxt += " (HP)";
                            if (gameData.shop.ShopForMana) DescTxt += " (MANA)";
                            if (gameData.shop.ShopForTP) DescTxt += " (TP)";
                            if (gameData.shop.ShopForKey) DescTxt += " (KEYS)";
                            if (gameData.shop.ShopForRegainHP) DescTxt += " (REGEN HP)";

                            gameData.SetGameStatus("TOWN-SHOP" + DescTxt + " (" + (TriedToShopCount + 1) + "/" + CharConfig.MaxShopTries + ")");
                            //Console.WriteLine("town moving to shop");
                            MoveToStore();
                            TriedToShopCount++;

                            //if (FastTowning) TriedToShopCount = 6;
                        }
                    }
                }
            }

            //check for repair
            if (CurrentScript == 6)
            {
                if (!gameData.repair.GetShouldRepair() || TriedToRepairCount >= CharConfig.MaxRepairTries || FastTowning)
                {
                    CurrentScript++;
                    return;
                }

                if (CurrentScript == 6)
                {
                    if (gameData.repair.GetShouldRepair() && TriedToRepairCount < CharConfig.MaxRepairTries && !FastTowning)
                    {
                        gameData.SetGameStatus("TOWN-REPAIR" + " (" + (TriedToRepairCount + 1) + "/" + CharConfig.MaxRepairTries + ")");
                        MoveToRepair();
                        TriedToRepairCount++;
                    }
                }
            }

            //end towning script
            if (CurrentScript == 7)
            {
                gameData.SetGameStatus("TOWN-END");

                if (FastTowning)
                {
                    if (MoveToTPOrWPSpot())
                    {
                        GetCorpse();
                        gameData.stash.RunningScriptCount = 0;
                        StopTowningReturn();
                    }
                }
                else
                {
                    GetCorpse();
                    gameData.stash.RunningScriptCount = 0;
                    StopTowningReturn();
                }
            }
        }
    }

    public void LoadFirstTownAct()
    {
        GetInTown();
        ScriptTownAct = TownAct;
    }

    public bool IsInRightTown()
    {
        if (ScriptTownAct == 0)
        {
            return true; //perform current town operation
        }
        if (TownAct != ScriptTownAct)
        {
            return false;
        }
        return true;
    }

    public void GoToWPArea(int SelectActWPIndex = -1, int SelectWPIndex = -1)
    {
        if (TownAct == 1)
        {
            gameData.mover.MovingToInteract = true;
            gameData.pathFinding.MoveToObject("WaypointPortal");
            gameData.mover.MovingToInteract = false;
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "WaypointPortal", (int)Enums.Area.RogueEncampment, new List<int>() { });
            if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
            {
                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                {
                    if (SelectWPIndex == -1)
                    {
                        SelectTownWP();
                    }
                    else
                    {
                        SelectThisWPIndex(SelectActWPIndex, SelectWPIndex);
                    }
                }
            }
            else
            {
                gameData.method_1("No WP found nearby in Town", Color.OrangeRed);
                IgnoredWPList.Clear();
            }
        }
        if (TownAct == 2)
        {
            gameData.mover.MovingToInteract = true;
            gameData.pathFinding.MoveToObject("Act2Waypoint");
            gameData.mover.MovingToInteract = false;
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "Act2Waypoint", (int)Enums.Area.LutGholein, new List<int>() { });
            if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
            {
                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                {
                    if (SelectWPIndex == -1)
                    {
                        SelectTownWP();
                    }
                    else
                    {
                        SelectThisWPIndex(SelectActWPIndex, SelectWPIndex);
                    }
                }
            }
            else
            {
                gameData.method_1("No WP found nearby in Town", Color.OrangeRed);
                IgnoredWPList.Clear();
            }
        }
        if (TownAct == 3)
        {
            gameData.mover.MovingToInteract = true;
            gameData.pathFinding.MoveToObject("Act3TownWaypoint");
            gameData.mover.MovingToInteract = false;
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "Act3TownWaypoint", (int)Enums.Area.KurastDocks, new List<int>() { });
            if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
            {
                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                {
                    if (SelectWPIndex == -1)
                    {
                        SelectTownWP();
                    }
                    else
                    {
                        SelectThisWPIndex(SelectActWPIndex, SelectWPIndex);
                    }
                }
            }
            else
            {
                gameData.method_1("No WP found nearby in Town", Color.OrangeRed);
                IgnoredWPList.Clear();
            }
        }
        if (TownAct == 4)
        {
            gameData.mover.MovingToInteract = true;
            gameData.pathFinding.MoveToObject("PandamoniumFortressWaypoint");
            gameData.mover.MovingToInteract = false;
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "PandamoniumFortressWaypoint", (int)Enums.Area.ThePandemoniumFortress, new List<int>() { });
            if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
            {
                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                {
                    if (SelectWPIndex == -1)
                    {
                        SelectTownWP();
                    }
                    else
                    {
                        SelectThisWPIndex(SelectActWPIndex, SelectWPIndex);
                    }
                }
            }
            else
            {
                gameData.method_1("No WP found nearby in Town", Color.OrangeRed);
                IgnoredWPList.Clear();
            }
        }
        if (TownAct == 5)
        {
            gameData.mover.MovingToInteract = true;
            gameData.pathFinding.MoveToObject("ExpansionWaypoint");
            gameData.mover.MovingToInteract = false;
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "ExpansionWaypoint", (int)Enums.Area.Harrogath, new List<int>() { });
            if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
            {
                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                {
                    if (SelectWPIndex == -1)
                    {
                        SelectTownWP();
                    }
                    else
                    {
                        SelectThisWPIndex(SelectActWPIndex, SelectWPIndex);
                    }
                }
            }
            else
            {
                gameData.method_1("No WP found nearby in Town", Color.OrangeRed);
                IgnoredWPList.Clear();
            }
        }
    }

    public void SelectThisWPIndex(int ThisActIndexx, int ThisIndexx)
    {
        //select town
        if (ThisActIndexx == 1) gameData.keyMouse.MouseClicc(235, 220);
        if (ThisActIndexx == 2) gameData.keyMouse.MouseClicc(325, 220);
        if (ThisActIndexx == 3) gameData.keyMouse.MouseClicc(415, 220);
        if (ThisActIndexx == 4) gameData.keyMouse.MouseClicc(500, 220);
        if (ThisActIndexx == 5) gameData.keyMouse.MouseClicc(585, 220);
        gameData.WaitDelay(50);

        //select WP from index
        gameData.keyMouse.MouseClicc(285, 260 + (ThisIndexx * 60));
        gameData.uiScan.WaitTilUIClose("waypointMenu");
        gameData.uiScan.WaitTilUIClose("loading");
        gameData.WaitDelay(CharConfig.WaypointEnterDelay);
    }

    public void SelectTownWP()
    {
        //select town
        if (ScriptTownAct == 1) gameData.keyMouse.MouseClicc(235, 220);
        if (ScriptTownAct == 2) gameData.keyMouse.MouseClicc(325, 220);
        if (ScriptTownAct == 3) gameData.keyMouse.MouseClicc(415, 220);
        if (ScriptTownAct == 4) gameData.keyMouse.MouseClicc(500, 220);
        if (ScriptTownAct == 5) gameData.keyMouse.MouseClicc(585, 220);
        gameData.WaitDelay(50);

        gameData.keyMouse.MouseClicc(285, 270); //select first wp
        gameData.uiScan.WaitTilUIClose("waypointMenu");
        gameData.uiScan.WaitTilUIClose("loading");
        gameData.WaitDelay(CharConfig.WaypointEnterDelay);
    }

    public bool ShouldBeInTown()
    {
        if (ForcedTowning) return true;
        if (GetInTown() && Towning) return true;

        bool ShouldBe = false;
        if (gameData.inventoryStruc.HasUnidItemInInventory() && !FastTowning) ShouldBe = true;
        if (gameData.inventoryStruc.ContainStashItemInInventory() && !FastTowning) ShouldBe = true;
        if (gameData.itemsStruc.TriesToPickItemCount >= CharConfig.MaxItemGrabTries)
        {
            if (gameData.inventoryStruc.HasUnidItemInInventory()) ShouldBe = true;
            if (gameData.inventoryStruc.ContainStashItemInInventory()) ShouldBe = true;
        }
        if (gameData.shop.ShouldShop()) ShouldBe = true;
        if (gameData.repair.GetShouldRepair()) ShouldBe = true;
        if (gameData.gamble.CanGamble() && !FastTowning && TownAct == 5) ShouldBe = true;

        bool ShouldReliveMerc = false;
        if (CharConfig.UsingMerc)
        {
            gameData.mercStruc.GetMercInfos();
            ShouldReliveMerc = !gameData.mercStruc.MercAlive;
        }
        if (ShouldReliveMerc && (gameData.playerScan.PlayerGoldInventory + gameData.playerScan.PlayerGoldInStash) >= 75000) ShouldBe = true;
        if ((gameData.playerScan.PlayerGoldInventory >= 35000)) ShouldBe = true;


        if (Towning && !ShouldBe)
        {
            Towning = false;
        }
        return ShouldBe;
    }

    public void CheckForNPCValidPos(string ThisNPC)
    {
        if (gameData.npcStruc.GetNPC(ThisNPC))
        {
            FixNPCPos(gameData.npcStruc.xPosFinal, gameData.npcStruc.yPosFinal);
        }
        else
        {
            gameData.method_1(ThisNPC.ToUpper() + " not found nearby", Color.OrangeRed);

            if (ThisNPC == "DeckardCain" && TownAct == 1)
            {
                gameData.pathFinding.MoveToNPC("Akara");
            }
            if (ThisNPC == "DeckardCain" && TownAct == 4)
            {
                gameData.pathFinding.MoveToThisPos(new Position { X = 5092, Y = 5044 });
            }
            if (ThisNPC == "Anya" && TownAct == 5)
            {
                gameData.pathFinding.MoveToThisPos(new Position { X = 5114, Y = 5059 });
            }
            else
            {
                MoveToStash(false);
            }
        }
    }

    public void FixNPCPos(int NPCX, int NPCY)
    {
        //detected bad NPC world position, lets visit stash to fix their pos
        if (NPCX == 0 && NPCY == 0)
        {
            MoveToStash(false);
        }
    }

    public bool MoveToTPOrWPSpot()
    {
        bool MovedCorrectly = false;

        if (TownAct == 1)
        {
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "RogueBonfire", (int)Enums.Area.RogueEncampment, new List<int>() { });
            if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
            {
                ThisFinalPosition.X = ThisFinalPosition.X + 5;
                ThisFinalPosition.Y = ThisFinalPosition.Y - 5;
                gameData.pathFinding.MoveToThisPos(ThisFinalPosition);
                MovedCorrectly = true;
            }
            else
            {
                gameData.method_1("No RogueBonfire found nearby in Town", Color.OrangeRed);
            }
        }
        if (TownAct == 2)
        {
            gameData.pathFinding.MoveToThisPos(new Position { X = 5150, Y = 5056 });
            MovedCorrectly = true;
        }
        if (TownAct == 3)
        {
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "Bank", (int)Enums.Area.KurastDocks, new List<int>() { });
            if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
            {
                gameData.pathFinding.MoveToThisPos(ThisFinalPosition);
                MovedCorrectly = true;
            }
            else
            {
                gameData.method_1("No TP/WP spot (Stash) found nearby in Town", Color.OrangeRed);
            }
        }

        if (TownAct == 4)
        {
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "Bank", (int)Enums.Area.ThePandemoniumFortress, new List<int>() { });
            if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
            {
                gameData.pathFinding.MoveToThisPos(ThisFinalPosition);
                MovedCorrectly = true;
            }
            else
            {
                gameData.method_1("No TP/WP spot (Stash) found nearby in Town", Color.OrangeRed);
            }
        }
        if (TownAct == 5)
        {
            gameData.pathFinding.MoveToThisPos(new Position { X = 5103, Y = 5029 });
            MovedCorrectly = true;
        }

        if (MovedCorrectly)
        {
            if (UseLastTP)
            {
                if (TPSpawned)
                {
                    //use tp
                    if (gameData.objectsStruc.GetObjects("TownPortal", true, new List<uint>(), 999, CharConfig.PlayerCharName))
                    {
                        gameData.pathFinding.MoveToThisPos(new Position { X = gameData.objectsStruc.itemx, Y = gameData.objectsStruc.itemy });
                        gameData.WaitDelay(15);

                        int tries = 0;
                        while (tries < 5)
                        {
                            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                            gameData.WaitDelay(10);
                            gameData.playerScan.GetPositions();
                            tries++;
                        }
                        gameData.WaitDelay(10);
                    }
                    else
                    {
                        gameData.method_1("No TP found nearby in Town", Color.OrangeRed);
                    }
                }
                /*else
                {
                    //use wp
                    if (gameData.objectsStruc.GetObjects("PandamoniumFortressWaypoint"))
                    {
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);
                                    itemScreenPos = gameData.mover.FixMousePositionWithScreenSize(itemScreenPos);
                        gameData.MouseClicc(itemScreenPos.X, itemScreenPos.Y - 15);
                    }
                    else
                    {
                        gameData.method_1("NO TP FOUND NEAR IN TOWN");
                    }
                }*/
            }
        }

        return MovedCorrectly;
    }

    public bool IsPosCloseTo(int TX, int TY, int Offset)
    {
        gameData.playerScan.GetPositions();
        if (gameData.playerScan.xPosFinal >= TX - Offset
                && gameData.playerScan.xPosFinal <= TX + Offset
                && gameData.playerScan.yPosFinal >= TY - Offset
                && gameData.playerScan.yPosFinal <= TY + Offset)
        {
            return true;
        }
        return false;
    }

    public void MoveToGamble()
    {
        bool MovedCorrectly = false;

        //MISSING TOWN ACT HERE -> DONT GAMBLE IN OTHER TOWN ACT
        if (TownAct != 5)
        {
            TriedToGambleCount = CharConfig.MaxGambleTries + 5;
            return;
        }

        if (TownAct == 5)
        {
            CheckForNPCValidPos("Anya");
            //gameData.pathFinding.MoveToNPC("Anya");  //not found
            gameData.pathFinding.MoveToThisPos(new Position { X = 5103, Y = 5115 });
            gameData.npcStruc.GetNPC("Anya");
            MovedCorrectly = true;
        }

        if (MovedCorrectly)
        {
            //Clic store
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.npcStruc.xPosFinal, gameData.npcStruc.yPosFinal);

            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
            if (gameData.uiScan.WaitTilUIOpen("npcInteract"))  //npcShop
            {
                if (TownAct == 5)
                {
                    gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Down); //Anya press down
                    gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Down); //Anya press down
                }
                gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Enter);
                gameData.WaitDelay(50);
                gameData.gamble.RunGambleScript();
                gameData.uiScan.CloseUIMenu("npcInteract");
                gameData.uiScan.CloseUIMenu("npcShop");
            }
        }
    }

    public void MoveToRepair()
    {
        bool MovedCorrectly = false;

        if (TownAct == 1)
        {
            //4985,6108 stash
            //4954,6095 charsi
            Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "Bank", (int)gameData.playerScan.levelNo, new List<int>() { });
            CheckForNPCValidPos("Charsi");
            //gameData.pathFinding.MoveToNPC("Charsi");  //not found
            gameData.pathFinding.MoveToThisPos(new Position { X = ThisFinalPosition.X - 31, Y = ThisFinalPosition.Y - 13 });
            gameData.npcStruc.GetNPC("Charsi");
            MovedCorrectly = true;
        }
        if (TownAct == 2)
        {
            CheckForNPCValidPos("Fara");
            //gameData.pathFinding.MoveToNPC("Fara");  //not found
            gameData.pathFinding.MoveToThisPos(new Position { X = 5115, Y = 5080 });
            gameData.npcStruc.GetNPC("Fara");
            MovedCorrectly = true;
        }
        if (TownAct == 3)
        {
            CheckForNPCValidPos("Hratli");
            //gameData.pathFinding.MoveToNPC("Hratli");  //not found
            gameData.pathFinding.MoveToThisPos(new Position { X = 5219, Y = 5035 });
            gameData.npcStruc.GetNPC("Hratli");
            MovedCorrectly = true;
        }

        if (TownAct == 4)
        {
            CheckForNPCValidPos("Halbu");
            //gameData.pathFinding.MoveToNPC("Halbu");  //not found
            gameData.pathFinding.MoveToThisPos(new Position { X = 5085, Y = 5022 });
            gameData.npcStruc.GetNPC("Halbu");
            MovedCorrectly = true;
        }

        if (TownAct == 5)
        {
            CheckForNPCValidPos("Larzuk");
            //gameData.pathFinding.MoveToNPC("Larzuk");  //not found
            gameData.pathFinding.MoveToThisPos(new Position { X = 5145, Y = 5041 });
            gameData.npcStruc.GetNPC("Larzuk");
            MovedCorrectly = true;
        }

        if (MovedCorrectly)
        {
            //Clic store
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.npcStruc.xPosFinal, gameData.npcStruc.yPosFinal);

            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
            if (gameData.uiScan.WaitTilUIOpen("npcInteract"))  //npcShop
            {
                if (TownAct != 4)
                {
                    gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Down); //Larzuk press down
                }
                gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Enter);
                gameData.WaitDelay(50);
                gameData.repair.RunRepairScript();
                gameData.uiScan.CloseUIMenu("npcInteract");
                gameData.uiScan.CloseUIMenu("npcShop");
            }
        }
    }

    public void MoveToStore()
    {
        bool MovedCorrectly = false;

        if (TownAct == 1)
        {
            CheckForNPCValidPos("Akara");
            gameData.pathFinding.MoveToNPC("Akara");
            gameData.npcStruc.GetNPC("Akara");
            MovedCorrectly = true;
        }
        if (TownAct == 2)
        {
            if (!gameData.shop.ShopForSellingitem
                && !gameData.shop.ShopForHP
                && !gameData.shop.ShopForMana
                && !gameData.shop.ShopForTP
                && !gameData.shop.ShopForKey
                && gameData.shop.ShopForRegainHP)
            {
                //Act2 Drognan doesn't regen HP, if we are going to shop only for regen HP, then go see Atma in Act2
                CheckForNPCValidPos("Atma");
                gameData.pathFinding.MoveToNPC("Atma");
                gameData.npcStruc.GetNPC("Atma");
                MovedCorrectly = true;
            }
            else
            {
                CheckForNPCValidPos("Drognan");
                gameData.pathFinding.MoveToNPC("Drognan");
                gameData.npcStruc.GetNPC("Drognan");
                MovedCorrectly = true;
            }

        }
        if (TownAct == 3)
        {
            CheckForNPCValidPos("Ormus");
            gameData.pathFinding.MoveToNPC("Ormus");
            gameData.npcStruc.GetNPC("Ormus");
            MovedCorrectly = true;
        }

        if (TownAct == 4)
        {
            CheckForNPCValidPos("Jamella");
            gameData.pathFinding.MoveToNPC("Jamella");
            gameData.npcStruc.GetNPC("Jamella");
            MovedCorrectly = true;
        }

        if (TownAct == 5)
        {
            CheckForNPCValidPos("Malah");
            gameData.pathFinding.MoveToNPC("Malah");
            //gameData.npcStruc.GetNPC("Malah");
            MovedCorrectly = true;
        }

        if (MovedCorrectly)
        {
            //Clic store
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.npcStruc.xPosFinal, gameData.npcStruc.yPosFinal);

            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
            if (gameData.uiScan.WaitTilUIOpen("npcInteract"))  //npcShop
            {
                if (TownAct != 4)
                {
                    gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Down);    //press down if not in Act4
                }
                gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Enter);
                gameData.WaitDelay(50);
                gameData.shop.RunShopScript();
                gameData.uiScan.CloseUIMenu("npcInteract");
                gameData.uiScan.CloseUIMenu("npcShop");

                gameData.shop.PlaceItem(gameData.CenterX, gameData.CenterY, true);
            }
        }
    }

    public void MoveToStash(bool RunScript)
    {
        bool MovedCorrectly = false;
        //MISSING TOWN ACT HERE
        if (TownAct == 1)
        {
            gameData.pathFinding.MoveToObject("Bank");
            MovedCorrectly = true;
        }
        if (TownAct == 2)
        {
            gameData.pathFinding.MoveToObject("Bank");
            MovedCorrectly = true;
        }
        if (TownAct == 3)
        {
            gameData.pathFinding.MoveToObject("Bank");
            MovedCorrectly = true;
        }

        if (TownAct == 4)
        {
            gameData.pathFinding.MoveToObject("Bank");
            MovedCorrectly = true;
        }

        if (TownAct == 5)
        {
            gameData.pathFinding.MoveToObject("Bank");
            MovedCorrectly = true;
        }

        if (MovedCorrectly)
        {
            //get stash location
            Position itemScreenPos = new Position { X = 0, Y = 0 };
            bool HasPosForStash = false;
            if (TownAct == 5)
            {
                itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 5124, 5057);

                HasPosForStash = true;
            }
            else
            {
                if (gameData.objectsStruc.GetObjects("Bank", true))
                {
                    gameData.method_1("Changed Stash pos to: " + gameData.objectsStruc.itemx + ", " + gameData.objectsStruc.itemy, Color.BlueViolet);
                    itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                    HasPosForStash = true;
                }
                else
                {
                    gameData.method_1("Stash not found nearby in Town", Color.OrangeRed);
                    if (TownAct == 1)
                    {
                        gameData.pathFinding.MoveToNPC("Akara");
                    }
                    if (TownAct == 2)
                    {
                        gameData.pathFinding.MoveToNPC("Greiz");
                    }
                    if (TownAct == 3)
                    {
                        gameData.pathFinding.MoveToNPC("Asheara");
                    }
                    if (TownAct == 4)
                    {
                        gameData.pathFinding.MoveToThisPos(new Position { X = 5092, Y = 5044 });
                    }
                }

            }
            if (HasPosForStash)
            {
                //Clic stash
                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
                if (gameData.uiScan.WaitTilUIOpen("stash"))
                {
                    if (RunScript)
                    {
                        gameData.stash.RunStashScript();
                    }
                    gameData.uiScan.CloseUIMenu("stash");
                }
            }
        }
    }

    public void MoveToCain()
    {
        CheckForNPCValidPos("DeckardCain");
        bool MovedCorrectly = false;

        if (TownAct == 1)
        {
            if (!CainNotFoundAct1)
            {
                gameData.pathFinding.MoveToNPC("DeckardCain");
                MovedCorrectly = true;
            }
            else
            {
                //go to town act5 for cain
                ScriptTownAct = 5;
            }
        }
        if (TownAct == 2)
        {
            gameData.pathFinding.MoveToNPC("DeckardCain");
            MovedCorrectly = true;
        }
        if (TownAct == 3)
        {
            gameData.pathFinding.MoveToNPC("DeckardCain");
            MovedCorrectly = true;
        }

        if (TownAct == 4)
        {
            gameData.pathFinding.MoveToNPC("DeckardCain");
            MovedCorrectly = true;
        }

        if (TownAct == 5)
        {
            gameData.pathFinding.MoveToNPC("DeckardCain");
            MovedCorrectly = true;
        }

        if (MovedCorrectly)
        {
            if (!gameData.npcStruc.GetNPC("DeckardCain"))
            {
                if (TownAct == 1 && !CainNotFoundAct1)
                {
                    CainNotFoundAct1 = true;

                    //go to town act5 for cain
                    ScriptTownAct = 5;
                }
            }

            //Clic cain
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.npcStruc.xPosFinal, gameData.npcStruc.yPosFinal);

            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
            if (gameData.uiScan.WaitTilUIOpen("npcInteract"))
            {
                //Clic Identify items (get cain pos again) - 227 offset y
                gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Down);
                gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Enter);

                //wait til its done
                if (!gameData.uiScan.WaitTilUIClose("npcInteract"))
                {
                    //gameData.method_1("ITEMS DIDN'T IDENTIFIED, RETRYING...", Color.Black);
                    gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Enter);
                }
                gameData.itemsStruc.GetItems(false);
            }
        }
    }

    public void MoveToMerc()
    {
        bool MovedCorrectly = false;

        if (TownAct == 1)
        {
            CheckForNPCValidPos("Kashya");
            gameData.pathFinding.MoveToNPC("Kashya");
            gameData.npcStruc.GetNPC("Kashya");
            MovedCorrectly = true;
        }
        if (TownAct == 2)
        {
            CheckForNPCValidPos("Greiz");
            gameData.pathFinding.MoveToNPC("Greiz");
            gameData.npcStruc.GetNPC("Greiz");
            MovedCorrectly = true;
        }
        if (TownAct == 3)
        {
            CheckForNPCValidPos("Asheara");
            gameData.pathFinding.MoveToNPC("Asheara");
            gameData.npcStruc.GetNPC("Asheara");
            MovedCorrectly = true;
        }

        if (TownAct == 4)
        {
            CheckForNPCValidPos("Tyrael");
            gameData.pathFinding.MoveToNPC("Tyrael");
            gameData.npcStruc.GetNPC("Tyrael");
            MovedCorrectly = true;
        }

        if (TownAct == 5)
        {
            CheckForNPCValidPos("QualKehk");
            gameData.pathFinding.MoveToNPC("QualKehk");
            gameData.npcStruc.GetNPC("QualKehk");
            MovedCorrectly = true;
        }

        if (MovedCorrectly)
        {
            //Clic merc NPC
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.npcStruc.xPosFinal, gameData.npcStruc.yPosFinal);

            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
            if (gameData.uiScan.WaitTilUIOpen("npcInteract"))
            {
                gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Down);
                if (TownAct == 4)
                {
                    gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Down); //Tyrael press down
                }
                gameData.keyMouse.PressKey(System.Windows.Forms.Keys.Enter);

                //wait til its done
                gameData.uiScan.WaitTilUIClose("npcInteract");
                gameData.uiScan.CloseUIMenu("npcInteract");
            }
        }
    }

    public void FixBaalNearRedPortal()
    {
        //fix when close to RedPortal in Baal
        if (gameData.playerScan.xPosFinal >= (15090 - 4)
            && gameData.playerScan.xPosFinal <= (15090 + 4)
            && gameData.playerScan.yPosFinal >= (5008 - 4)
            && gameData.playerScan.yPosFinal <= (5008 + 4)
            && gameData.playerScan.levelNo == 131)
        {
            gameData.pathFinding.MoveToThisPos(new Position { X = 15090 + 5, Y = 5008 + 15 });
        }
    }

    public void SpawnTP(bool EnterTP = false)
    {
        FixBaalNearRedPortal();

        int IncreaseCount = 0;
        while (gameData.inventoryStruc.HUDItems_tpscrolls == 0 && IncreaseCount < 10)
        {
            gameData.patternsScan.IncreaseV1Scanning();
            IncreaseCount++;
            gameData.itemsStruc.GetItems(false);
        }

        //has tp
        if (gameData.inventoryStruc.HUDItems_tpscrolls > 0)
        {
            // open inv
            gameData.uiScan.OpenUIMenu("invMenu");
            //use tp in inventory
            gameData.inventoryStruc.UseTP();
            TPSpawned = true;
            //close inv
            gameData.uiScan.CloseUIMenu("invMenu");
            gameData.WaitDelay(50); //100 default

            if (EnterTP)
            {
                Towning = true;
                ForcedTowning = true;
            }
        }
        else
        {
            if (EnterTP)
            {
                gameData.method_1("Leaving because TP quantity equal 0, cannot spawn a TP and go to Town!", Color.Red);
                gameData.LeaveGame(false);
            }
        }
    }

    public void GetCorpse()
    {
        if (gameData.itemsStruc.ItemsEquiped > 2) return;

        //method #1
        if (gameData.npcStruc.GetNPC("DeadCorpse"))
        {
            //Console.WriteLine("Corpse found method1");
            gameData.method_1("Grab corpse #1", Color.Red);
            //Clic corpse
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.npcStruc.xPosFinal, gameData.npcStruc.yPosFinal);

            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
        }

        //method #2
        int Tries = 0;
        while (gameData.playerScan.ScanForOthersPlayers(0, CharConfig.PlayerCharName, true) && Tries < 5)
        {
            //Console.WriteLine("Corpse found method2");
            gameData.method_1("Grab corpse #2", Color.Red);
            //Clic corpse
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.playerScan.xPosFinalOtherP, gameData.playerScan.yPosFinalOtherP);

            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
            gameData.WaitDelay(100);
            gameData.playerScan.GetPositions();
            Tries++;
        }
    }

    public void GoToTown()
    {
        //script to spawn tp and move to town quickly (no potion and no hp)
        if (!GetInTown())
        {
            SpawnTP(true);
        }
    }

    public bool GetInTown()
    {
        TownAct = 0;
        if (gameData.playerScan.levelNo >= 1 && gameData.playerScan.levelNo < 40) TownAct = 1;
        if (gameData.playerScan.levelNo >= 40 && gameData.playerScan.levelNo < 75) TownAct = 2;
        if (gameData.playerScan.levelNo >= 75 && gameData.playerScan.levelNo < 103) TownAct = 3;
        if (gameData.playerScan.levelNo >= 103 && gameData.playerScan.levelNo < 109) TownAct = 4;
        if (gameData.playerScan.levelNo >= 109) TownAct = 5;


        IsInTown = false;
        if (gameData.playerScan.levelNo == 1       //act1
            || gameData.playerScan.levelNo == 40   //act2
            || gameData.playerScan.levelNo == 75   //act3
            || gameData.playerScan.levelNo == 103  //act4
            || gameData.playerScan.levelNo == 109) //act5
        {
            IsInTown = true;
        }
        return IsInTown;
    }


    public string getAreaName(int areaNum)
    {
        switch (areaNum)
        {
            case 1: return "Rogue Encampment";
            case 2: return "Blood Moor";
            case 3: return "Cold Plains";
            case 4: return "Stony Field";
            case 5: return "Dark Wood";
            case 6: return "Black Marsh";
            case 7: return "Tamoe Highland";
            case 8: return "Den of Evil";
            case 9: return "Cave Level 1";
            case 10: return "Underground Passage Level 1";
            case 11: return "Hole Level 1";
            case 12: return "Pit Level 1";
            case 13: return "Cave Level 2";
            case 14: return "Underground Passage Level 2";
            case 15: return "Hole Level 2";
            case 16: return "Pit Level 2";
            case 17: return "Burial Grounds";
            case 18: return "Crypt";
            case 19: return "Mausoleum";
            case 20: return "Forgotten Tower";
            case 21: return "Tower Cellar Level 1";
            case 22: return "Tower Cellar Level 2";
            case 23: return "Tower Cellar Level 3";
            case 24: return "Tower Cellar Level 4";
            case 25: return "Tower Cellar Level 5";
            case 26: return "Monastery Gate";
            case 27: return "Outer Cloister";
            case 28: return "Barracks";
            case 29: return "Jail Level 1";
            case 30: return "Jail Level 2";
            case 31: return "Jail Level 3";
            case 32: return "Inner Cloister";
            case 33: return "Cathedral";
            case 34: return "Catacombs Level 1";
            case 35: return "Catacombs Level 2";
            case 36: return "Catacombs Level 3";
            case 37: return "Catacombs Level 4";
            case 38: return "Tristram";
            case 39: return "Moo Moo Farm";
            case 40: return "Lut Gholein";
            case 41: return "Rocky Waste";
            case 42: return "Dry Hills";
            case 43: return "Far Oasis";
            case 44: return "Lost City";
            case 45: return "Valley of Snakes";
            case 46: return "Canyon of the Magi";
            case 47: return "Sewers Level 1";
            case 48: return "Sewers Level 2";
            case 49: return "Sewers Level 3";
            case 50: return "Harem Level 1";
            case 51: return "Harem Level 2";
            case 52: return "Palace Cellar Level 1";
            case 53: return "Palace Cellar Level 2";
            case 54: return "Palace Cellar Level 3";
            case 55: return "Stony Tomb Level 1";
            case 56: return "Halls of the Dead Level 1";
            case 57: return "Halls of the Dead Level 2";
            case 58: return "Claw Viper Temple Level 1";
            case 59: return "Stony Tomb Level 2";
            case 60: return "Halls of the Dead Level 3";
            case 61: return "Claw Viper Temple Level 2";
            case 62: return "Maggot Lair Level 1";
            case 63: return "Maggot Lair Level 2";
            case 64: return "Maggot Lair Level 3";
            case 65: return "Ancient Tunnels";
            case 66: return "Tal Rasha's Tomb #1";
            case 67: return "Tal Rasha's Tomb #2";
            case 68: return "Tal Rasha's Tomb #3";
            case 69: return "Tal Rasha's Tomb #4";
            case 70: return "Tal Rasha's Tomb #5";
            case 71: return "Tal Rasha's Tomb #6";
            case 72: return "Tal Rasha's Tomb #7";
            case 73: return "Duriel's Lair";
            case 74: return "Arcane Sanctuary";
            case 75: return "Kurast Docktown";
            case 76: return "Spider Forest";
            case 77: return "Great Marsh";
            case 78: return "Flayer Jungle";
            case 79: return "Lower Kurast";
            case 80: return "Kurast Bazaar";
            case 81: return "Upper Kurast";
            case 82: return "Kurast Causeway";
            case 83: return "Travincal";
            case 84: return "Arachnid Lair";
            case 85: return "Spider Cavern";
            case 86: return "Swampy Pit Level 1";
            case 87: return "Swampy Pit Level 2";
            case 88: return "Flayer Dungeon Level 1";
            case 89: return "Flayer Dungeon Level 2";
            case 90: return "Swampy Pit Level 3";
            case 91: return "Flayer Dungeon Level 3";
            case 92: return "Sewers Level 1";
            case 93: return "Sewers Level 2";
            case 94: return "Ruined Temple";
            case 95: return "Disused Fane";
            case 96: return "Forgotten Reliquary";
            case 97: return "Forgotten Temple";
            case 98: return "Ruined Fane";
            case 99: return "Disused Reliquary";
            case 100: return "Durance of Hate Level 1";
            case 101: return "Durance of Hate Level 2";
            case 102: return "Durance of Hate Level 3";
            case 103: return "Pandemonium Fortress";
            case 104: return "Outer Steppes";
            case 105: return "Plains of Despair";
            case 106: return "City of the Damned";
            case 107: return "River of Flame";
            case 108: return "Chaos Sanctuary";
            case 109: return "Harrogath";
            case 110: return "Bloody Foothills";
            case 111: return "Frigid Highlands";
            case 112: return "Arreat Plateau";
            case 113: return "Crystalline Passage";
            case 114: return "Frozen River";
            case 115: return "Glacial Trail";
            case 116: return "Drifter Cavern";
            case 117: return "Frozen Tundra";
            case 118: return "Ancients' Way";
            case 119: return "Icy Cellar";
            case 120: return "Arreat Summit";
            case 121: return "Nihlathaks Temple";
            case 122: return "Halls of Anguish";
            case 123: return "Halls of Death's Calling";
            case 124: return "Halls of Vaught";
            case 125: return "Abaddon";
            case 126: return "Pit of Acheron";
            case 127: return "Infernal Pit";
            case 128: return "Worldstone Keep Level 1";
            case 129: return "Worldstone Keep Level 2";
            case 130: return "Worldstone Keep Level 3";
            case 131: return "Throne of Destruction";
            case 132: return "Worldstone Chamber";
            case 133: return "Pandemonium Run 1";
            case 134: return "Pandemonium Run 2";
            case 135: return "Pandemonium Run 3";
            case 136: return "Tristram";
        }

        return "";
    }
}
