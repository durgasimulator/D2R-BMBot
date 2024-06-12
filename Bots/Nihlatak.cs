using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Nihlatak : IBot
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.HallsOfPain) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.HallsOfVaught) CurrentStep = 2;
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

            gameData.townStruc.GoToWPArea(5, 5);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING NIHLATAK");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.HallsOfPain)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.HallsOfVaught)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.HallsOfVaught);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.HallsOfPain)
                {
                    CurrentStep = 1;
                    return;
                }
                //####

                //gameData.pathFinding.MoveToNPC("Nihlathak");
                gameData.pathFinding.MoveToObject("NihlathakWildernessStartPosition");
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING NIHLATAK");
                gameData.mobsStruc.DetectThisMob("getBossName", "Nihlathak", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getBossName", "Nihlathak", false, 200, new List<long>()))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.battle.RunBattleScriptOnThisMob("getBossName", "Nihlathak", new List<long>());
                    }
                    else
                    {
                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        return;
                    }
                }
                else
                {
                    gameData.method_1("Nihlatak not detected!", Color.Red);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Nihlathak", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Nihlathak", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    return;
                }
            }
        }
    }
}
