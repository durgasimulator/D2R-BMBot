using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;

public class Baal : IBot
{
    GameData gameData;
    //#####################################################
    //#####################################################
    //Special Run Variable
    public bool KillBaal = true;
    public List<uint> LeaveIfMobsIsPresent_ID = new List<uint>();
    public List<int> LeaveIfMobsIsPresent_Count = new List<int>();
    public int LeaveIfMobsCountIsAbove = 0;
    public bool SafeYoloStrat = false;
    //#####################################################
    //#####################################################

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public bool DetectedBaal = false;

    public List<long> IgnoredMobs = new List<long>();

    public Position ThronePos = new Position { X = 15095, Y = 5029 };
    public Position PortalPos = new Position { X = 15116, Y = 5071 };

    public bool Wave5Detected = false;
    public bool Wave5Cleared = false;

    public Position ThroneCorner1Pos = new Position { X = 15104, Y = 5062 };
    public Position ThroneCorner2Pos = new Position { X = 15082, Y = 5063 };
    public Position ThroneCorner3Pos = new Position { X = 15081, Y = 5016 };
    public Position ThroneCorner4Pos = new Position { X = 15112, Y = 5013 };

    public int CornerClearedIndex = 0;
    public int BufferPathFindingMoveSize = 0;

    public DateTime TimeSinceLastWaveDone = DateTime.Now;
    public bool TimeSinceLastWaveSet = false;

    public int TeleportToBaalTry = 0;
    public bool TryMovingAwayOnLeftSide = true;
    public int MaxMoveAwayTry = 2;

    public int CheckingThroneBackMode = 0;
    public int PortalYOffset = 0;

    public int SafeYoloStratIndex = 0;
    public int DefaultTakeRVPot = 0;

  

    public void ResetVars()
    {
        
        if (DefaultTakeRVPot != 0) CharConfig.TakeRVPotUnder = DefaultTakeRVPot;

        CurrentStep = 0;
        ScriptDone = false;
        DetectedBaal = false;
        Wave5Detected = false;
        Wave5Cleared = false;
        IgnoredMobs = new List<long>();
        CornerClearedIndex = 0;
        TimeSinceLastWaveDone = DateTime.MaxValue;
        TimeSinceLastWaveSet = false;
        TeleportToBaalTry = 0;
        TryMovingAwayOnLeftSide = true;
        CheckingThroneBackMode = 0;
        PortalYOffset = 0;
        SafeYoloStratIndex = 0;
        DefaultTakeRVPot = CharConfig.TakeRVPotUnder;
    }

    public void DetectCurrentStep()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TheWorldStoneKeepLevel2) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TheWorldStoneKeepLevel3) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ThroneOfDestruction) CurrentStep = 3;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TheWorldstoneChamber) CurrentStep = 7;
    }

    public bool LeaveOnMobs()
    {
        bool IsLeaving = false;

        //Check by mobs count
        if (LeaveIfMobsCountIsAbove > 0)
        {
            if (gameData.mobsStruc.GetMobsCount(0) >= LeaveIfMobsCountIsAbove) IsLeaving = true;
        }

        //Check by mobs ID and count
        if (!IsLeaving)
        {
            if (LeaveIfMobsIsPresent_ID.Count > 0)
            {
                for (int i = 0; i < LeaveIfMobsIsPresent_ID.Count; i++)
                {
                    if (gameData.mobsStruc.GetMobsCount(LeaveIfMobsIsPresent_ID[i]) >= LeaveIfMobsIsPresent_Count[i])
                    {
                        IsLeaving = true;
                        break;
                    }
                }
            }
        }

        if (IsLeaving)
        {
            gameData.method_1("Leaving game (Baal leaving mobs condition)!", Color.Red);
            gameData.LeaveGame(false);
        }

        //Yolo Strat
        if (SafeYoloStrat)
        {
            if (gameData.mobsStruc.GetMobsCount(0) >= 45 && CharConfig.TakeRVPotUnder < 45) CharConfig.TakeRVPotUnder = DefaultTakeRVPot + 10;
            if (gameData.mobsStruc.GetMobsCount(0) >= 25 && gameData.mobsStruc.GetMobsCount(0) < 45 && CharConfig.TakeRVPotUnder < 45) CharConfig.TakeRVPotUnder = DefaultTakeRVPot + 5;
            if (gameData.mobsStruc.GetMobsCount(0) < 25) CharConfig.TakeRVPotUnder = DefaultTakeRVPot;

            int PlayerHPPercent = (int)((gameData.playerScan.PlayerHP * 100) / gameData.playerScan.PlayerMaxHP);
            if (gameData.mobsStruc.GetMobsCount(0) >= 45
                && PlayerHPPercent < DefaultTakeRVPot - 5
                && gameData.beltStruc.GetPotionQuantityInBelt(3) == 0)
            {
                gameData.method_1("Leaving game (Baal Yolo Strat)!", Color.Red);
                gameData.LeaveGame(false);
            }
        }

        return IsLeaving;
    }

    public void RunScript()
    {
        gameData = GameData.Instance;
        gameData.townStruc.ScriptTownAct = 5; //set to town act 4 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            //stop doing baal if we are in town, and we was actually killing baaal and we dont detect the TP,
            //else it will go back in the WP and redo the whole Baal script again. (for Public Games)
            if (CharConfig.RunBaalScript
                && !ScriptDone
                && CurrentStep >= 7
                && gameData.PublicGame)
            {
                gameData.townStruc.FastTowning = false;
                gameData.townStruc.UseLastTP = false;
                ScriptDone = true;
                return;
            }

            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(5, 8);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING BAAL");
                gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if (gameData.playerScan.levelNo == (int)Enums.Area.TheWorldStoneKeepLevel2)
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.TheWorldStoneKeepLevel3)
                {
                    CurrentStep++;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.TheWorldStoneKeepLevel3);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.ThroneOfDestruction)
                {
                    CurrentStep++;
                    return;
                }
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TheWorldStoneKeepLevel2)
                {
                    CurrentStep--;
                    return;
                }
                //####

                gameData.pathFinding.MoveToExit(Enums.Area.ThroneOfDestruction);
                gameData.townStruc.TPSpawned = false;
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.TheWorldStoneKeepLevel3)
                {
                    CurrentStep--;
                    return;
                }
                //####

                if (gameData.PublicGame && !gameData.townStruc.TPSpawned)
                {
                    PortalPos.Y += PortalYOffset;
                    gameData.pathFinding.MoveToThisPos(PortalPos);
                    gameData.townStruc.SpawnTP();

                    PortalYOffset -= 6;
                }

                if (LeaveOnMobs()) return;

                gameData.pathFinding.MoveToThisPos(ThronePos);
                CurrentStep++;
            }

            if (CurrentStep == 4)
            {
                //clear throne area of mobs
                if (CornerClearedIndex == 0)
                {
                    if (LeaveOnMobs()) return;
                    if (gameData.pathFinding.MoveToThisPos(ThroneCorner1Pos, 4, true)) CornerClearedIndex++;
                    if (LeaveOnMobs()) return;
                }
                else if (CornerClearedIndex == 1)
                {
                    if (LeaveOnMobs()) return;
                    if (gameData.pathFinding.MoveToThisPos(ThroneCorner1Pos, 4, true)) CornerClearedIndex++;
                    if (LeaveOnMobs()) return;
                    //CornerClearedIndex++;
                }
                else if (CornerClearedIndex == 2)
                {
                    if (LeaveOnMobs()) return;
                    if (gameData.pathFinding.MoveToThisPos(ThroneCorner1Pos, 4, true)) CornerClearedIndex++;
                    if (LeaveOnMobs()) return;
                    //CornerClearedIndex++;
                }
                else if (CornerClearedIndex == 3)
                {
                    if (LeaveOnMobs()) return;
                    if (gameData.pathFinding.MoveToThisPos(ThroneCorner1Pos, 4, true)) CornerClearedIndex++;
                    if (LeaveOnMobs()) return;
                    //CornerClearedIndex++;
                }
                if (CornerClearedIndex == 4)
                {
                    if (LeaveOnMobs()) return;
                    //gameData.pathFinding.MoveToThisPos(ThroneCorner4Pos, 4, true);
                    CurrentStep++;
                }
            }

            if (CurrentStep == 5)
            {
                //clear waves
                if (gameData.playerScan.xPosFinal < ThronePos.X - 3
                    || gameData.playerScan.xPosFinal > ThronePos.X + 3
                    || gameData.playerScan.yPosFinal < ThronePos.Y - 3
                    || gameData.playerScan.yPosFinal > ThronePos.Y + 3)
                {
                    if (!gameData.battle.ClearingArea && !gameData.battle.DoingBattle)
                    {
                        gameData.pathFinding.MoveToThisPos(ThronePos, 2, true);
                    }
                    else
                    {
                        TimeSinceLastWaveDone = DateTime.MaxValue;
                        TimeSinceLastWaveSet = false;
                        return;
                    }
                }
                else
                {
                    gameData.battle.DoBattleScript(30);

                    if (gameData.battle.ClearingArea || gameData.battle.DoingBattle)
                    {
                        TimeSinceLastWaveDone = DateTime.MaxValue;
                        TimeSinceLastWaveSet = false;
                        return;
                    }
                }

                if (!Wave5Cleared)
                {
                    if (gameData.PublicGame && gameData.playerScan.HasAnyPlayerInArea((int)Enums.Area.TheWorldstoneChamber))
                    {
                        gameData.method_1("People detected in Worldstone chamber, switching to baal script!", Color.OrangeRed);

                        gameData.keyMouse.MouseClicc_RealPos(gameData.CenterX, gameData.ScreenYOffset); //drop possible items on curson to ground
                        CurrentStep++;
                    }

                    //DETECT OTHERS WAVES FOR CASTING
                    if (!TimeSinceLastWaveSet && !gameData.mobsStruc.GetMobs("", "", true, 25, IgnoredMobs))
                    {
                        if (!gameData.PublicGame || !gameData.playerScan.HasBattleOrderState) gameData.battle.CastDefense();
                        TimeSinceLastWaveDone = DateTime.Now;
                        TimeSinceLastWaveSet = true;
                        gameData.inventoryStruc.DumpBadItemsOnGround();
                    }

                    //START CASTING IN ADVANCE
                    if ((DateTime.Now - TimeSinceLastWaveDone).TotalSeconds > CharConfig.BaalWavesCastDelay)
                    {
                        gameData.battle.SetSkills();
                        gameData.battle.CastSkillsNoMove();
                    }

                    //STOP CASTING ERROR DETECTING MOBS/BAAL MOVED
                    if ((DateTime.Now - TimeSinceLastWaveDone).TotalSeconds > 25)
                    {
                        TimeSinceLastWaveDone = DateTime.MaxValue;
                        TimeSinceLastWaveSet = false;

                        if (CheckingThroneBackMode == 0)
                        {
                            gameData.method_1("Mobs undetected, moving back to clear Throne!", Color.OrangeRed);
                            CornerClearedIndex = 0;
                            CurrentStep--;
                            CheckingThroneBackMode = 1;
                            return;
                        }
                        else if (CheckingThroneBackMode == 1)
                        {
                            gameData.method_1("Mobs undetected, moving forward to Kill Baal!", Color.OrangeRed);
                            CurrentStep++;
                            CheckingThroneBackMode = 0;
                            return;
                        }
                    }

                    //STOP CASTING
                    if (gameData.mobsStruc.GetMobs("", "", true, 30, IgnoredMobs))
                    {
                        TimeSinceLastWaveDone = DateTime.MaxValue;
                        TimeSinceLastWaveSet = false;
                        gameData.battle.DoBattleScript(30);
                    }

                    //#### DETECT WAVE 5
                    if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Baal Subject 5", false, 99, IgnoredMobs))
                    {
                        if (gameData.mobsStruc.MobsHP > 0)
                        {
                            Wave5Detected = true;
                        }
                        else
                        {
                            if (Wave5Detected)
                            {
                                if (!gameData.mobsStruc.GetMobs("", "", true, 30, IgnoredMobs))
                                {
                                    Wave5Cleared = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Wave5Detected)
                        {
                            if (!gameData.mobsStruc.GetMobs("", "", true, 30, IgnoredMobs))
                            {
                                Wave5Cleared = true;
                            }
                        }
                    }
                    //####
                }
                else
                {
                    gameData.keyMouse.MouseClicc_RealPos(gameData.CenterX, gameData.ScreenYOffset); //drop possible items on curson to ground
                    CurrentStep++;
                }
            }


            if (CurrentStep == 6)
            {
                if (!KillBaal)
                {

                    gameData.itemsStruc.GrabAllItemsForGold();
                    gameData.battle.ClearingArea = false;
                    gameData.battle.DoingBattle = false;
                    gameData.potions.CanUseSkillForRegen = true;
                    //gameData.LeaveGame(true);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }

                gameData.SetGameStatus("WAITING PORTAL");

                //move to baal red portal
                if (gameData.playerScan.xPosFinal >= 15170 - 40
                    && gameData.playerScan.xPosFinal <= 15170 + 40
                    && gameData.playerScan.yPosFinal >= 5880 - 40
                    && gameData.playerScan.yPosFinal <= 5880 + 40)
                {
                    //gameData.battle.CastDefense();
                    CurrentStep++;
                }
                else
                {
                    if (gameData.playerScan.xPosFinal < 15090 - 3
                        || gameData.playerScan.xPosFinal > 15090 + 3
                        || gameData.playerScan.yPosFinal < 5008 - 3
                        || gameData.playerScan.yPosFinal > 5008 + 3)
                    {
                        if (!CharConfig.UseTeleport)
                        {
                            gameData.mover.MoveAcceptOffset = 1;
                        }
                        else
                        {
                            gameData.mover.MoveAcceptOffset = 3;
                        }
                        //gameData.pathFinding.MoveToThisPos(new Position { X = 15090, Y = 5008 });
                        if (gameData.mover.MoveToLocation(15095, 5023))
                        {
                            if (gameData.mover.MoveToLocation(15090, 5008))
                            {
                                gameData.inventoryStruc.DumpBadItemsOnGround();
                                gameData.battle.CastDefense();
                                gameData.mover.MoveAcceptOffset = 4;
                            }
                        }
                    }
                    else
                    {
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15091, 5005);

                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X - 5, itemScreenPos.Y - 20);
                        gameData.WaitDelay(10);
                    }
                }
            }

            if (CurrentStep == 7)
            {
                gameData.SetGameStatus("MOVING TO BAAL");
                gameData.pathFinding.MoveToThisPos(new Position { X = 15134, Y = 5920 });
                //gameData.WaitDelay(50); //wait a bit to detect baal
                CurrentStep++;
                //15065,5891
            }

            if (CurrentStep == 8)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING BAAL");
                gameData.mobsStruc.DetectThisMob("getBossName", "Baal", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>()))
                {
                    TeleportToBaalTry = 0;
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        DetectedBaal = true;
                        gameData.battle.RunBattleScriptOnThisMob("getBossName", "Baal", new List<long>());
                    }
                    else
                    {
                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    }
                }
                else
                {
                    gameData.method_1("Baal not detected!", Color.Red);

                    if (TeleportToBaalTry < MaxMoveAwayTry && TryMovingAwayOnLeftSide)
                    {
                        if (gameData.mover.MoveToLocation(15062, 5891))
                        {
                            //if (gameData.mover.MoveToLocation(15106, 5901))
                            if (gameData.mover.MoveToLocation(15134, 5920))
                            {
                                TeleportToBaalTry++;
                                if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>())) return; //redetect baal?
                            }
                        }
                    }
                    else if (TeleportToBaalTry >= MaxMoveAwayTry && TryMovingAwayOnLeftSide)
                    {
                        TeleportToBaalTry = 1;
                        TryMovingAwayOnLeftSide = false; //now try moving away to the right to try detect baal
                    }
                    else if (TeleportToBaalTry < MaxMoveAwayTry && !TryMovingAwayOnLeftSide)
                    {
                        if (gameData.mover.MoveToLocation(15214, 5890))
                        {
                            //if (gameData.mover.MoveToLocation(15166, 5908))
                            if (gameData.mover.MoveToLocation(15134, 5920))
                            {
                                TeleportToBaalTry++;
                                if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>())) return; //redetect baal?
                            }
                        }
                    }
                    else if (TeleportToBaalTry >= MaxMoveAwayTry && !TryMovingAwayOnLeftSide)
                    {
                        if (gameData.mover.MoveToLocation(15134, 5920))
                        {
                            for (int i = 0; i < 30; i++) //140
                            {
                                gameData.playerScan.GetPositions();

                                gameData.battle.SetSkills();
                                gameData.battle.CastSkillsNoMove();

                                gameData.itemsStruc.GetItems(true);
                                gameData.potions.CheckIfWeUsePotion();
                                gameData.itemsStruc.GetItems(false);
                                gameData.overlayForm.UpdateOverlay();

                                if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>())) return; //redetect baal?
                            }

                            //baal not detected...
                            gameData.itemsStruc.GetItems(true);
                            if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>())) return; //redetect baal?
                                                                                                                          //if (!gameData.PublicGame) gameData.itemsStruc.GrabAllItemsForGold();
                            gameData.itemsStruc.GrabAllItemsForGold();
                            if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>())) return; //redetect baal?
                                                                                                                          //if (!gameData.PublicGame) gameData.itemsStruc.GrabAllItemsForGold();

                            if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        }
                    }
                }
            }

        }
    }

}
