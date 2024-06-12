using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class UpperKurast : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public int WP_X = 0;
    public int WP_Y = 0;
    public List<int> IgnoredChestList = new List<int>();
    public bool ScriptDone { get; set; } = false;
    public bool HasTakenAnyChest = false;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void ResetVars()
    {
        CurrentStep = 0;
        WP_X = 0;
        WP_Y = 0;
        IgnoredChestList = new List<int>();
        ScriptDone = false;
    }

    public void RunScript()
    {
        gameData.townStruc.ScriptTownAct = 3; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(3, 6);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING UPPER KURAST");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.UpperKurast)
                {
                    WP_X = gameData.playerScan.xPos - 3;
                    WP_Y = gameData.playerScan.yPos - 3;

                    CurrentStep++;
                }
                else
                {
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.GoToTown();
                }
            }

            if (CurrentStep == 1)
            {
                TakeChest();

                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //gameData.itemsStruc.GrabAllItemsForGold();

                //gameData.LeaveGame(true);

                if (gameData.mover.MoveToLocation(WP_X, WP_Y))
                {
                    //take back wp
                    //if (gameData.objectsStruc.GetObjects("Act3TownWaypoint", false))
                    //{
                    Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, WP_X, WP_Y);

                    gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                    //gameData.mover.FinishMoving();
                    if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                    {
                        gameData.townStruc.SelectTownWP();
                        gameData.townStruc.Towning = true;
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.UseLastTP = false;
                        ScriptDone = true;

                        //gameData.LeaveGame(true); //#####
                    }
                    //}
                }
            }
        }
    }

    public void TakeChest()
    {
        //JungleStashObject2
        //JungleStashObject3
        //GoodChest
        //NotSoGoodChest
        //DeadVillager1
        //DeadVillager2
        //NotSoGoodChest
        //HollowLog

        //JungleMediumChestLeft ####

        MapAreaStruc.Position ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", (int)Enums.Area.UpperKurast, IgnoredChestList);
        int ChestObject = gameData.mapAreaStruc.CurrentObjectIndex;
        int Tryy = 0;
        while (ThisChestPos.X != 0 && ThisChestPos.Y != 0 && Tryy < 30)
        {
            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                ScriptDone = true;
                return;
            }

            if (gameData.mover.MoveToLocation(ThisChestPos.X, ThisChestPos.Y))
            {
                HasTakenAnyChest = true;

                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisChestPos.X, ThisChestPos.Y);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.WaitDelay(10);
                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.WaitDelay(10);
                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.WaitDelay(10);

                int tryy2 = 0;
                while (gameData.itemsStruc.GetItems(true) && tryy2 < 20)
                {
                    gameData.playerScan.GetPositions();
                    gameData.itemsStruc.GetItems(false);
                    gameData.potions.CheckIfWeUsePotion();
                    tryy2++;
                }
                IgnoredChestList.Add(ChestObject);
            }

            ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", (int)Enums.Area.UpperKurast, IgnoredChestList);
            ChestObject = gameData.mapAreaStruc.CurrentObjectIndex;

            Tryy++;
        }

        if (!HasTakenAnyChest) gameData.mapAreaStruc.DumpMap();
    }
}
