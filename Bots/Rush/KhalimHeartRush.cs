﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class KhalimHeartRush : IBot
{
    private GameData gameData;

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position ChestPos = new Position { X = 0, Y = 0 };
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
    }

    public void DetectCurrentStep()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.KurastBazaar) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SewersLevel1Act3) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SewersLevel2Act3) CurrentStep = 3;
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
                gameData.SetGameStatus("DOING KAHLIM HEART");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.KurastBazaar)
                {
                    gameData.townStruc.SpawnTP();
                    gameData.WaitDelay(15);
                    gameData.battle.CastDefense();
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

            if (CurrentStep == 2)
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

            if (CurrentStep == 3)
            {
                //####
                if (gameData.playerScan.levelNo != (int)Enums.Area.SewersLevel2Act3)
                {
                    CurrentStep--;
                    return;
                }
                //####

                ChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "KhalimChest1", (int)Enums.Area.SewersLevel2Act3, new List<int>());
                if (ChestPos.X != 0 && ChestPos.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(ChestPos);

                    //repeat clic on chest
                    int tryyy = 0;
                    while (tryyy <= 25)
                    {
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ChestPos.X, ChestPos.Y);

                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
                        gameData.playerScan.GetPositions();
                        tryyy++;
                    }

                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Kahlim Heart Chest location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 4)
            {
                if (!gameData.battle.DoBattleScript(10))
                {
                    Position ThisTPPos = new Position { X = ChestPos.X - 10, Y = ChestPos.Y + 5 };
                    gameData.pathFinding.MoveToThisPos(ThisTPPos);

                    gameData.townStruc.TPSpawned = false;

                    CurrentStep++;
                }
            }

            if (CurrentStep == 5)
            {
                gameData.SetGameStatus("Kahlim Heart waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(10);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.SewersLevel2Act3)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 6)
            {
                gameData.SetGameStatus("Kahlim Heart waiting on leecher #2");

                gameData.battle.DoBattleScript(10);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.KurastDocks)
                {
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                }
            }
        }
    }
}
