using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Enums;
using static MapAreaStruc;

public class ArachnidLair : IBot
{

    GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;


    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
    }

    public void DetectCurrentStep()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SpiderForest) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SpiderCavern) CurrentStep = 2;
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

            gameData.townStruc.GoToWPArea(3, 1);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING ARACHNID LAIR");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SpiderForest)
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
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SpiderCavern)
                {
                    CurrentStep++;
                    return;
                }

                gameData.pathFinding.MoveToExit(Enums.Area.SpiderCavern);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.SpiderForest)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.SetGameStatus("CLEARING ARACHNID LAIR");

                if ((Enums.Area)gameData.battle.AreaIDFullyCleared != Enums.Area.SpiderCavern)
                {
                    gameData.battle.ClearFullAreaOfMobs();

                    if (!gameData.battle.ClearingArea)
                    {
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
}
