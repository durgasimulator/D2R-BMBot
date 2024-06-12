using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class HallOfDeadRushCube : IBot
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.HallsOfTheDeadLevel2) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.HallsOfTheDeadLevel3) CurrentStep = 2;
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

            gameData.townStruc.GoToWPArea(2, 3);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING HALL OF THE DEAD (CUBE)");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.HallsOfTheDeadLevel2)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.HallsOfTheDeadLevel3)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.HallsOfTheDeadLevel3);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo != (int)Enums.Area.HallsOfTheDeadLevel3)
                {
                    CurrentStep--;
                    return;
                }
                //####

                ChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "HoradricCubeChest", (int)Enums.Area.HallsOfTheDeadLevel3, new List<int>());
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
                    gameData.method_1("Cube location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 3)
            {
                if (!gameData.battle.DoBattleScript(25))
                {
                    Position ThisTPPos = new Position { X = ChestPos.X - 10, Y = ChestPos.Y + 5 };
                    gameData.pathFinding.MoveToThisPos(ThisTPPos);

                    gameData.townStruc.TPSpawned = false;

                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                gameData.SetGameStatus("Cube waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.HallsOfTheDeadLevel3)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 5)
            {
                gameData.SetGameStatus("Cube waiting on leecher #2");

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
