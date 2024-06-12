using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Enums;
using static MapAreaStruc;

public class Pit : IBot
{
    GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public List<int> IgnoredChestList = new List<int>();
    public bool HasTakenAnyChest = false;

    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
        IgnoredChestList = new List<int>();
        HasTakenAnyChest = false;
    }

    public void DetectCurrentStep()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.BlackMarsh) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TamoeHighland) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.PitLevel1) CurrentStep = 3;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.PitLevel2) CurrentStep = 4;
    }

    public void RunScript()
    {
        gameData = GameData.Instance;
        gameData.townStruc.ScriptTownAct = 1; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(1, 4);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING CRYPT");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.BlackMarsh)
                {
                    CurrentStep++;
                }
                else
                {
                    DetectCurrentStep();
                    if (CurrentStep == 0)
                    {
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.GoToTown();
                    }
                }
            }

            if (CurrentStep == 1)
            {
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TamoeHighland)
                {
                    CurrentStep++;
                    return;
                }

                gameData.pathFinding.MoveToNextArea(Enums.Area.TamoeHighland);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.PitLevel1)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.BlackMarsh)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.PitLevel1);
                CurrentStep++;
            }

            if (CurrentStep == 3) 
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TamoeHighland)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.SetGameStatus("CLEARING PIT LVL1");
                if ((Enums.Area)gameData.battle.AreaIDFullyCleared != Enums.Area.PitLevel1)
                {
                    gameData.battle.ClearFullAreaOfMobs();

                    if (!gameData.battle.ClearingArea)
                    {
                        gameData.pathFinding.MoveToExit(Enums.Area.PitLevel2);
                        CurrentStep++;
                    }
                }
                else
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.PitLevel2);
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.PitLevel1)
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.PitLevel2);
                    return;
                }
                //####

                gameData.SetGameStatus("CLEARING PIT LVL2");
                if ((Enums.Area)gameData.battle.AreaIDFullyCleared != Enums.Area.PitLevel2)
                {
                    gameData.battle.ClearFullAreaOfMobs();

                    if (!gameData.battle.ClearingArea)
                    {
                        TakeChest((int)(Enums.Area.PitLevel2));
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.UseLastTP = false;
                        ScriptDone = true;
                    }
                }
                else
                {
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                }
            }
        }
    }

    public void TakeChest(int ThisAreaa)
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

        MapAreaStruc.Position ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", ThisAreaa, IgnoredChestList);
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

            ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", ThisAreaa, IgnoredChestList);
            ChestObject = gameData.mapAreaStruc.CurrentObjectIndex;

            Tryy++;
        }

        if (!HasTakenAnyChest) gameData.mapAreaStruc.DumpMap();
    }
}
