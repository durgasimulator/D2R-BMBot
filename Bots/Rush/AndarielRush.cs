using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AndarielRush : IBot
{
    GameData gameData;

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public bool DetectedBoss = false;


    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
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
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CatacombsLevel2)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.CatacombsLevel3)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.CatacombsLevel3);
                gameData.townStruc.TPSpawned = false;
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

                gameData.SetGameStatus("Andariel waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.CatacombsLevel4)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                /*X: 22561,
                Y: 9553,*/
                if (gameData.mover.MoveToLocation(22561, 9553))
                {
                    DetectedBoss = false;
                    //gameData.WaitDelay(100);
                    CurrentStep++;
                }
            }

            if (CurrentStep == 5)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING ANDARIEL");
                gameData.mobsStruc.DetectThisMob("getBossName", "Andariel", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getBossName", "Andariel", false, 200, new List<long>()))
                {
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

                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(5);
                        gameData.itemsStruc.GrabAllItemsForGold();
                        gameData.potions.CanUseSkillForRegen = true;

                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.UseLastTP = false;
                        ScriptDone = true;
                        return;
                        //gameData.LeaveGame(true);
                    }
                }
                else
                {
                    gameData.method_1("Andariel not detected!", Color.Red);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Andariel", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Andariel", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                    //gameData.LeaveGame(true);
                }
            }
        }
    }
}
