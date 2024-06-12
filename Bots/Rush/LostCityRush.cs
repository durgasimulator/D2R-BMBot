using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class LostCityRush : IBot
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.LostCity) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ValleyOfSnakes) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ClawViperTempleLevel1) CurrentStep = 3;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ClawViperTempleLevel2) CurrentStep = 4;
    }

    public void RunScript()
    {
        gameData.townStruc.ScriptTownAct = 2; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(2, 5);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING LOST CITY (AMMY)");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.LostCity)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.ValleyOfSnakes)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToNextArea(Enums.Area.ValleyOfSnakes);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.ClawViperTempleLevel1)
                {
                    CurrentStep++;
                    return;
                }
                if (gameData.playerScan.levelNo != (int)Enums.Area.ValleyOfSnakes)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.ClawViperTempleLevel1);
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.ClawViperTempleLevel2)
                {
                    CurrentStep++;
                    return;
                }
                if (gameData.playerScan.levelNo != (int)Enums.Area.ClawViperTempleLevel1)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.ClawViperTempleLevel2);
                CurrentStep++;
            }

            if (CurrentStep == 4)
            {
                //####
                if (gameData.playerScan.levelNo != (int)Enums.Area.ClawViperTempleLevel2)
                {
                    CurrentStep--;
                    return;
                }
                //####

                ChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "TaintedSunAltar", (int)Enums.Area.ClawViperTempleLevel2, new List<int>());
                if (ChestPos.X != 0 && ChestPos.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(ChestPos);

                    //repeat clic on chest
                    /*int tryyy = 0;
                    while (tryyy <= 25)
                    {
                        Dictionary<string, int> itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ChestPos.X, ChestPos.Y);
                        gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.playerScan.GetPositions();
                        tryyy++;
                    }*/


                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Ammy location not detected!", Color.Red);

                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                    //gameData.townStruc.FastTowning = false;
                    //ScriptDone = true;
                    //return;
                }
            }

            if (CurrentStep == 5)
            {
                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                CurrentStep++;
            }

            if (CurrentStep == 6)
            {
                gameData.SetGameStatus("Ammy waiting on leecher");

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.ClawViperTempleLevel2)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 7)
            {
                gameData.SetGameStatus("Ammy waiting on leecher #2");

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.LutGholein)
                {
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                }
            }
        }
    }
}
