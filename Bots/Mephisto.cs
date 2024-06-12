using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Mephisto : IBot
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.DuranceOfHateLevel2) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.DuranceOfHateLevel3) CurrentStep = 2;
    }

    public void RunScript()
    {
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

            gameData.townStruc.GoToWPArea(3, 8);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING MEPHISTO");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.DuranceOfHateLevel2)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.DuranceOfHateLevel3)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.DuranceOfHateLevel3);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.DuranceOfHateLevel2)
                {
                    CurrentStep = 1;
                    return;
                }
                //####

                /*X: 22561,
                Y: 9553,*/
                if (gameData.mover.MoveToLocation(17568, 8069))
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                //####
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.DuranceOfHateLevel2)
                {
                    CurrentStep = 1;
                    return;
                }
                //####

                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING MEPHISTO");
                gameData.mobsStruc.DetectThisMob("getBossName", "Mephisto", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getBossName", "Mephisto", false, 200, new List<long>()))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.battle.RunBattleScriptOnThisMob("getBossName", "Mephisto", new List<long>());
                    }
                    else
                    {
                        if (gameData.battle.EndBossBattle())
                        {
                            gameData.townStruc.FastTowning = false;
                            gameData.townStruc.UseLastTP = false;
                            ScriptDone = true;

                            //Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "portal", 102 - 1, new List<int>() { });
                            //if (gameData.mover.MoveToLocation(ThisFinalPosition.X, ThisFinalPosition.Y))
                            /*while (gameData.playerScan.levelNo == (int)Enums.Area.DuranceOfHateLevel3)
                            {
                                gameData.itemsStruc.GetItems(true);
                                if (gameData.mover.MoveToLocation(17601, 8070))
                                {
                                    Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 17601, 8070);
                                    //Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                                    gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);

                                    gameData.playerScan.GetPositions();
                                }
                            }

                            gameData.WaitDelay(CharConfig.MephistoRedPortalEnterDelay);

                            gameData.townStruc.FastTowning = false;
                            gameData.townStruc.UseLastTP = false;
                            ScriptDone = true;*/
                            return;
                            //gameData.LeaveGame(true);
                        }
                    }
                }
                else
                {
                    gameData.method_1("Mephisto not detected!", Color.Red);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Mephisto", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Mephisto", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    return;
                }
            }
        }
    }
}
