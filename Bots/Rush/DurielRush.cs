using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class DurielRush : IBot
{
    private GameData gameData;

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position OrificePos = new Position { X = 0, Y = 0 };

    public bool WaitedInDuriel = false;
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
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CanyonOfTheMagi) CurrentStep = 1;
        if (gameData.playerScan.levelNo >= (int)Enums.Area.TalRashasTomb1 && gameData.playerScan.levelNo <= (int)Enums.Area.TalRashasTomb7)
        {
            CurrentStep = 1; //return to step1 anyway!
        }
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.DurielsLair) CurrentStep = 3;
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

            gameData.townStruc.GoToWPArea(2, 8);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING DURIEL");
                gameData.battle.CastDefense();

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.CanyonOfTheMagi)
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
                if (gameData.playerScan.levelNo >= (int)Enums.Area.TalRashasTomb1 && gameData.playerScan.levelNo <= (int)Enums.Area.TalRashasTomb7)
                {
                    CurrentStep++;
                    return;
                }
                //####

                //id":152, "type":"object", "x":453, "y":258, "name":"orifice", "op":25, "class":"quest"}
                //Detect the correct tomb where Duriel hide
                OrificePos = gameData.mapAreaStruc.GetAreaOfObject("object", "HoradricOrifice", new List<int>(), 65, 72);
                if (OrificePos.X != 0 && OrificePos.Y != 0)
                {
                    //"id":71, "type":"exit", "x":214, "y":25, "isGoodExit":true}
                    //gameData.method_1("Moving to: " + ((Enums.Area)(gameData.mapAreaStruc.CurrentObjectAreaIndex + 1)), Color.Red);
                    Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("exit", gameData.townStruc.getAreaName((int)gameData.mapAreaStruc.CurrentObjectAreaIndex + 1), (int)gameData.playerScan.levelNo, new List<int>() { });
                    if (gameData.mover.MoveToLocation(ThisFinalPosition.X, ThisFinalPosition.Y))
                    {
                        int Tryyyy = 0;
                        while (gameData.playerScan.levelNo == (int)Enums.Area.CanyonOfTheMagi && Tryyyy <= 25)
                        {
                            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
                            gameData.playerScan.GetPositions();
                            Tryyyy++;
                        }
                        //didn't clic correctly on tomb door, substract some pixels
                        Tryyyy = 0;
                        while (gameData.playerScan.levelNo == (int)Enums.Area.CanyonOfTheMagi && Tryyyy <= 25)
                        {
                            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X - 70, itemScreenPos.Y);
                            gameData.playerScan.GetPositions();
                            Tryyyy++;
                        }
                        //didn't clic correctly on tomb door, substract some pixels
                        Tryyyy = 0;
                        while (gameData.playerScan.levelNo == (int)Enums.Area.CanyonOfTheMagi && Tryyyy <= 25)
                        {
                            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X + 70, itemScreenPos.Y);
                            gameData.playerScan.GetPositions();
                            Tryyyy++;
                        }

                        gameData.pathFinding.MoveToThisPos(OrificePos); //Move to Orifice

                        CurrentStep++;
                    }


                }
                else
                {
                    gameData.method_1("Horadric Orifice location not detected!", Color.Red);
                    gameData.townStruc.UseLastTP = false;
                    gameData.townStruc.FastTowning = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 2)
            {
                if (!gameData.battle.DoBattleScript(15))
                {
                    gameData.pathFinding.MoveToThisPos(OrificePos);

                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                gameData.SetGameStatus("Duriel waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(15);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo >= (int)Enums.Area.TalRashasTomb1 && gameData.playerScan.LeechlevelNo <= (int)Enums.Area.TalRashasTomb7)
                {
                    gameData.pathFinding.MoveToThisPos(OrificePos); //Move to Orifice
                    CurrentStep++;
                }
                /*else if (gameData.playerScan.LeechlevelNo != (int)Enums.Area.LutGholein)
                {
                    gameData.pathFinding.MoveToThisPos(OrificePos); //Move to Orifice
                    CurrentStep++;
                }*/
            }

            if (CurrentStep == 2)
            {
                int Tryyyy = 0;
                int StartLevel = (int)gameData.playerScan.levelNo;
                while ((int)gameData.playerScan.levelNo == StartLevel && Tryyyy <= 25)
                {
                    Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, OrificePos.X, OrificePos.Y);
                    gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X - 446, itemScreenPos.Y - 268);
                    gameData.playerScan.GetPositions();
                    Tryyyy++;
                }

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.DurielsLair)
                {
                    gameData.WaitDelay(50);  //wait a little bit so duriel can be detected
                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                if (gameData.playerScan.levelNo >= (int)Enums.Area.TalRashasTomb1 && gameData.playerScan.levelNo <= (int)Enums.Area.TalRashasTomb7)
                {
                    CurrentStep--;
                }

                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING DURIEL");
                if (!WaitedInDuriel)
                {
                    //get leecher infos
                    gameData.playerScan.GetLeechPositions();

                    if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.DurielsLair)
                    {
                        WaitedInDuriel = true;
                    }
                    else
                    {
                        return;
                    }
                }
                gameData.mobsStruc.DetectThisMob("getBossName", "Duriel", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getBossName", "Duriel", false, 200, new List<long>()))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.battle.RunBattleScriptOnThisMob("getBossName", "Duriel", new List<long>());
                    }
                    else
                    {

                        if (gameData.battle.EndBossBattle())
                        {
                            ScriptDone = true;
                        }
                        return;
                        //gameData.LeaveGame(true);
                    }
                }
                else
                {
                    gameData.method_1("Duriel not detected!", Color.Red);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Duriel", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Duriel", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    gameData.townStruc.UseLastTP = false;
                    gameData.townStruc.FastTowning = false;
                    ScriptDone = true;
                    return;
                    //gameData.LeaveGame(true);
                }
            }
        }
    }
}
