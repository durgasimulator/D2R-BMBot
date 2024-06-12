using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class KhalimBrainRush : IBot
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.FlayerJungle) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.FlayerDungeonLevel1) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.FlayerDungeonLevel2) CurrentStep = 3;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.FlayerDungeonLevel3) CurrentStep = 4;
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

            gameData.townStruc.GoToWPArea(3, 3);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING KAHLIM BRAIN");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.FlayerJungle)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.FlayerDungeonLevel1)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel1);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.FlayerDungeonLevel2)
                {
                    CurrentStep++;
                    return;
                }
                if (gameData.playerScan.levelNo != (int)Enums.Area.FlayerDungeonLevel1)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel2);
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.FlayerDungeonLevel3)
                {
                    CurrentStep++;
                    return;
                }
                if (gameData.playerScan.levelNo != (int)Enums.Area.FlayerDungeonLevel2)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel3);
                CurrentStep++;
            }

            if (CurrentStep == 4)
            {
                //####
                if (gameData.playerScan.levelNo != (int)Enums.Area.FlayerDungeonLevel3)
                {
                    CurrentStep--;
                    return;
                }
                //####

                ChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "KhalimChest2", (int)Enums.Area.FlayerDungeonLevel3, new List<int>());
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
                    gameData.method_1("Kahlim Brain Chest location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 5)
            {
                if (!gameData.battle.DoBattleScript(15))
                {
                    Position ThisTPPos = new Position { X = ChestPos.X - 10, Y = ChestPos.Y + 5 };
                    gameData.pathFinding.MoveToThisPos(ThisTPPos);

                    gameData.townStruc.TPSpawned = false;

                    CurrentStep++;
                }
            }

            if (CurrentStep == 6)
            {
                gameData.SetGameStatus("Kahlim Brain waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(15);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.FlayerDungeonLevel3)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 7)
            {
                gameData.SetGameStatus("Kahlim Brain waiting on leecher #2");

                gameData.battle.DoBattleScript(15);

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
