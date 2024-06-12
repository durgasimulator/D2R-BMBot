using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Andariel : IBot
{
    GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public bool DetectedBoss = false;

    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
        DetectedBoss = false;
    }

    public void DetectCurrentStep()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CatacombsLevel2) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CatacombsLevel3) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CatacombsLevel4) CurrentStep = 3;
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

            gameData.townStruc.GoToWPArea(1, 8);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING ANDARIEL");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CatacombsLevel2)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.CatacombsLevel3)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.CatacombsLevel3);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.CatacombsLevel4)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CatacombsLevel2)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.CatacombsLevel4);
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CatacombsLevel3)
                {
                    CurrentStep--;
                    return;
                }
                //####

                /*X: 22561,
                Y: 9553,*/
                if (gameData.mover.MoveToLocation(22556, 9544))
                {
                    DetectedBoss = false;
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING ANDARIEL");

                //#############
                gameData.mobsStruc.DetectThisMob("getBossName", "Andariel", false, 200, new List<long>());
                bool DetectedAndy = gameData.mobsStruc.GetMobs("getBossName", "Andariel", false, 200, new List<long>());
                DateTime StartTime = DateTime.Now;
                TimeSpan TimeSinceDetecting = DateTime.Now - StartTime;
                while (!DetectedAndy && TimeSinceDetecting.TotalSeconds < 5)
                {
                    gameData.SetGameStatus("WAITING DETECTING ANDARIEL");
                    DetectedAndy = gameData.mobsStruc.GetMobs("getBossName", "Andariel", false, 200, new List<long>());
                    TimeSinceDetecting = DateTime.Now - StartTime;

                    //cast attack during this waiting time
                    gameData.battle.SetSkills();
                    gameData.battle.CastSkillsNoMove();
                    gameData.itemsStruc.GetItems(true);      //#############
                    gameData.potions.CheckIfWeUsePotion();

                    if (!gameData.gameStruc.IsInGame() || !gameData.Running)
                    {
                        gameData.overlayForm.ResetMoveToLocation();
                        return;
                    }
                }
                //#############

                if (gameData.mobsStruc.GetMobs("getBossName", "Andariel", false, 200, new List<long>()))
                {
                    gameData.SetGameStatus("KILLING ANDARIEL");
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        DetectedBoss = true;
                        gameData.battle.RunBattleScriptOnThisMob("getBossName", "Andariel", new List<long>());
                    }
                    else
                    {
                        if (!DetectedBoss)
                        {
                            gameData.method_1("Andariel not detected!", Color.Red);
                            gameData.battle.DoBattleScript(15);
                        }

                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        return;
                    }
                }
                else
                {
                    gameData.method_1("Andariel not detected!", Color.Red);

                    gameData.battle.DoBattleScript(15);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Andariel", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Andariel", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    return;
                }
            }
        }
    }
}
