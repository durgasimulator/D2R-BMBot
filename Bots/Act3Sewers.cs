using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Act3Sewers : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public List<int> IgnoredChestList = new List<int>();
    public bool ScriptDone { get; set; } = false;
    public bool HasTakenAnyChest = false;
    public Position ChestPos = new Position { X = 0, Y = 0 };
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void ResetVars()
    {
        CurrentStep = 0;
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

            gameData.townStruc.GoToWPArea(3, 5);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING A3 SEWERS");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.KurastBazaar)
                {
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
                TakeChest(Enums.Area.KurastBazaar);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.SewersLevel1Act3)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.SewersLevel1Act3);
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.SewersLevel2Act3)
                {
                    CurrentStep++;
                    return;
                }
                if (gameData.playerScan.levelNo != (int)Enums.Area.SewersLevel1Act3)
                {
                    CurrentStep--;
                    return;
                }
                //####
                TakeChest(Enums.Area.SewersLevel1Act3);
                CurrentStep++;
            }

            if (CurrentStep == 4)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.SewersLevel2Act3)
                {
                    CurrentStep++;
                    return;
                }
                if (gameData.playerScan.levelNo != (int)Enums.Area.SewersLevel1Act3)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.SewersLevel2Act3);

                ChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "Act3SewerStairsToLevel3", (int)Enums.Area.SewersLevel1Act3, new List<int>());
                if (ChestPos.X != 0 && ChestPos.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(ChestPos);

                    gameData.battle.SetSkills();
                    gameData.battle.CastSkillsNoMove();

                    //repeat clic on leverfor stair
                    int tryyy = 0;
                    while (tryyy <= 25)
                    {
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ChestPos.X, ChestPos.Y);

                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
                        gameData.playerScan.GetPositions();
                        gameData.WaitDelay(2);
                        tryyy++;
                    }

                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Lever location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }

                gameData.pathFinding.MoveToExit(Enums.Area.SewersLevel2Act3);

                CurrentStep++;
            }

            if (CurrentStep == 5)
            {
                //####
                if (gameData.playerScan.levelNo != (int)Enums.Area.SewersLevel2Act3)
                {
                    CurrentStep--;
                    return;
                }
                //####
                TakeChest(Enums.Area.SewersLevel2Act3);
                CurrentStep++;
            }

            if (CurrentStep == 6)
            {
                gameData.townStruc.Towning = true;
                gameData.townStruc.FastTowning = false;
                gameData.townStruc.UseLastTP = false;
                ScriptDone = true;
            }
        }
    }

    public void TakeChest(Enums.Area ThisArea)
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

        MapAreaStruc.Position ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", (int)ThisArea, IgnoredChestList);
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

            ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", (int)ThisArea, IgnoredChestList);
            ChestObject = gameData.mapAreaStruc.CurrentObjectIndex;

            Tryy++;
        }

        if (!HasTakenAnyChest) gameData.mapAreaStruc.DumpMap();
    }
}
