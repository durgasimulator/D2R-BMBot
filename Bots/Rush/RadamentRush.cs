using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class RadamentRush : IBot
{
    private GameData gameData;

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position RadamentPosition = new Position { X = 0, Y = 0 };
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SewersLevel2Act2) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SewersLevel3Act2) CurrentStep = 2;
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

            gameData.townStruc.GoToWPArea(2, 1);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING RADAMENT");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SewersLevel2Act2)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.SewersLevel3Act2)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.SewersLevel3Act2);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SewersLevel2Act2)
                {
                    CurrentStep--;
                    return;
                }
                //####

                RadamentPosition = gameData.mapAreaStruc.GetPositionOfObject("npc", "Radament2", (int)Enums.Area.SewersLevel3Act2, new List<int>());
                if (RadamentPosition.X != 0 && RadamentPosition.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(RadamentPosition);

                    //repeat clic on tree
                    /*int tryyy = 0;
                    while (tryyy <= 25)
                    {
                        Dictionary<string, int> itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, RadamentPosition.X, RadamentPosition.Y);
                        gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"]);
                        gameData.playerScan.GetPositions();
                        tryyy++;
                    }*/

                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Radament location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 3)
            {
                gameData.SetGameStatus("Radament clearing");

                if (!gameData.battle.DoBattleScript(25))
                {
                    gameData.pathFinding.MoveToThisPos(RadamentPosition);

                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                gameData.SetGameStatus("Radament waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.SewersLevel3Act2)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 5)
            {
                gameData.SetGameStatus("Radament waiting on leecher #2");

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
