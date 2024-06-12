using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Enums;
using static MapAreaStruc;

public class Crypt : IBot
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ColdPlains) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.BurialGrounds) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.Crypt) CurrentStep = 3;

        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.Mausoleum)
        {
            gameData.pathFinding.MoveToExit(Enums.Area.BurialGrounds);
            CurrentStep = 2;
        }
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

            gameData.townStruc.GoToWPArea(1, 1);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING CRYPT");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ColdPlains)
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
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.BurialGrounds)
                {
                    CurrentStep++;
                    return;
                }

                gameData.pathFinding.MoveToNextArea(Enums.Area.BurialGrounds);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.Crypt)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ColdPlains)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.Crypt);
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.BurialGrounds)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.SetGameStatus("CLEARING CRYPT");

                if ((Enums.Area)gameData.battle.AreaIDFullyCleared != Enums.Area.Crypt)
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
