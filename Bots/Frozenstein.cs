using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Frozenstein : IBot
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CrystallinePassage) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.FrozenRiver) CurrentStep = 2;
    }

    public void RunScript()
    {
        gameData = GameData.Instance;
        gameData.townStruc.ScriptTownAct = 5; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(5, 3);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING FROZENSTEIN");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CrystallinePassage)
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
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.FrozenRiver)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.FrozenRiver);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo != (int)Enums.Area.FrozenRiver)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToNPC("Frozenstein");
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                if (!gameData.battle.DoBattleScript(10))
                {
                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                }
            }
        }
    }
}
