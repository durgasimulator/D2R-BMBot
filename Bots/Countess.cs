using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Countess : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ForgottenTower) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel1) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel2) CurrentStep = 3;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel3) CurrentStep = 4;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel4) CurrentStep = 5;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel5) CurrentStep = 6;
    }

    public void RunScript()
    {
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

            gameData.townStruc.GoToWPArea(1, 4);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING COUNTESS");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.BlackMarsh)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.ForgottenTower)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.ForgottenTower);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.TowerCellarLevel1)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.BlackMarsh)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel1);
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.TowerCellarLevel2)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ForgottenTower)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel2);
                CurrentStep++;
            }

            if (CurrentStep == 4)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.TowerCellarLevel3)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel1)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel3);
                CurrentStep++;
            }

            if (CurrentStep == 5)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.TowerCellarLevel4)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel2)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel4);
                CurrentStep++;
            }

            if (CurrentStep == 6)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.TowerCellarLevel5)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel3)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel5);
                CurrentStep++;
            }

            if (CurrentStep == 7)
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TowerCellarLevel4)
                {
                    CurrentStep--;
                    return;
                }
                //####

                MapAreaStruc.Position ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", (int)Enums.Area.TowerCellarLevel5, new List<int>());

                //gameData.itemsStruc.GrabAllItemsForGold();
                if (gameData.mover.MoveToLocation(ThisChestPos.X, ThisChestPos.Y))
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 8)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData. SetGameStatus("KILLING COUNTESS");
                gameData.mobsStruc.DetectThisMob("getSuperUniqueName", "The Countess", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "The Countess", false, 200, new List<long>()))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "The Countess", new List<long>());
                    }
                    else
                    {
                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        return;
                    }
                }
                else
                {
                    gameData.method_1("Countess not detected!", Color.Red);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "The Countess", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "The Countess", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;
                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    return;
                }
            }
        }
    }
}
