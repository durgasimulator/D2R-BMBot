using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class DarkWoodRush : IBot
{

    GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position InifussTree = new Position { X = 0, Y = 0 };

    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
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

            gameData.townStruc.GoToWPArea(1, 3);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING DARKWOOD");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.DarkWood)
                {
                    gameData.townStruc.SpawnTP();
                    gameData.WaitDelay(15);
                    gameData.battle.CastDefense();
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
                InifussTree = gameData.mapAreaStruc.GetPositionOfObject("object", "InifussTree", (int)Enums.Area.DarkWood, new List<int>());
                if (InifussTree.X != 0 && InifussTree.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(InifussTree);

                    //repeat clic on tree
                    int tryyy = 0;
                    while (tryyy <= 25)
                    {
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, InifussTree.X, InifussTree.Y);

                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
                        gameData.playerScan.GetPositions();
                        tryyy++;
                    }

                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Inifuss Tree location not detected!", Color.Red);
                    gameData.townStruc.UseLastTP = false;
                    gameData.townStruc.FastTowning = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 2)
            {
                if (!gameData.battle.DoBattleScript(25))
                {
                    Position ThisTPPos = new Position { X = InifussTree.X - 10, Y = InifussTree.Y + 5 };
                    gameData.pathFinding.MoveToThisPos(ThisTPPos);

                    gameData.townStruc.TPSpawned = false;

                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                gameData.SetGameStatus("DarkWood waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.DarkWood)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                gameData.SetGameStatus("DarkWood waiting on leecher #2");

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.RogueEncampment)
                {
                    gameData.townStruc.UseLastTP = false;
                    gameData.townStruc.FastTowning = false;
                    ScriptDone = true;
                }
            }
        }
    }
}
