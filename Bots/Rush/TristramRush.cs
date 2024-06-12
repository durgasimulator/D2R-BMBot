using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class TristramRush : IBot
{
    GameData gameData;

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position TristramPos = new Position { X = 0, Y = 0 };


    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
    }

    public void DetectCurrentStep()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.StonyField) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.Tristram) CurrentStep = 5;
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

            gameData.townStruc.GoToWPArea(1, 2);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING TRISTRAM");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.StonyField)
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
                TristramPos = gameData.mapAreaStruc.GetPositionOfObject("object", "CairnStoneAlpha", (int)Enums.Area.StonyField, new List<int>());
                if (TristramPos.X != 0 && TristramPos.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(TristramPos);

                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Tristram location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 2)
            {
                gameData.SetGameStatus("Tristram clearing stones");
                if (!gameData.battle.DoBattleScript(25))
                {
                    Position ThisTPPos = new Position { X = TristramPos.X - 10, Y = TristramPos.Y + 5 };
                    gameData.pathFinding.MoveToThisPos(TristramPos);

                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                gameData.SetGameStatus("Tristram waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.StonyField)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                gameData.SetGameStatus("Tristram waiting for Tristram portal");

                if (gameData.objectsStruc.GetObjects("PermanentTownPortal", true, new List<uint>(), 60))
                {
                    gameData.mover.MoveToLocation(gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                    Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                    gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                    gameData.WaitDelay(100);
                }

                if (gameData.playerScan.levelNo == (int)Enums.Area.Tristram)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 5)
            {
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.StonyField)
                {
                    CurrentStep--;
                    return;
                }

                gameData.SetGameStatus("Doing Tristram");

                if (gameData.objectsStruc.GetObjects("CainGibbet", true, new List<uint>()))
                {
                    if (gameData.mover.MoveToLocation(gameData.objectsStruc.itemx, gameData.objectsStruc.itemy))
                    {
                        //repeat clic on tree
                        int tryyy = 0;
                        while (tryyy <= 15)
                        {
                            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                            gameData.WaitDelay(4);
                            tryyy++;
                        }

                        CurrentStep++;
                    }
                }
            }

            if (CurrentStep == 6)
            {
                gameData.SetGameStatus("Clearing Tristram");

                if (!gameData.battle.DoBattleScript(25))
                {
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                }
            }
        }
    }
}
