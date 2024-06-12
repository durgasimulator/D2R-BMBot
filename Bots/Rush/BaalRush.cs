using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;

public class BaalRush : IBot
{
    private GameData gameData;

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
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
        DetectedBaal = false;
        Wave5Detected = false;
        Wave5Cleared = false;
        IgnoredMobs = new List<long>();
        CornerClearedIndex = 0;
        TimeSinceLastWaveDone = DateTime.Now;
        TimeSinceLastWaveSet = false;
    }

    public void DetectCurrentStep()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TheWorldStoneKeepLevel2) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TheWorldStoneKeepLevel3) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ThroneOfDestruction) CurrentStep = 3;
    }

    public void RunScript()
    {
        gameData.townStruc.ScriptTownAct = 4; //set to town act 4 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(5, 8);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING BAAL");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if (gameData.playerScan.levelNo == (int)Enums.Area.TheWorldStoneKeepLevel2)
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

                if (!gameData.townStruc.TPSpawned)
                {
                    gameData.pathFinding.MoveToThisPos(PortalPos);
                    gameData.townStruc.SpawnTP();
                }

                gameData.pathFinding.MoveToThisPos(ThronePos);
                CurrentStep++;
            }

            if (CurrentStep == 4)
            {
                //clear throne area of mobs
                if (CornerClearedIndex == 0)
                {
                    gameData.pathFinding.MoveToThisPos(ThroneCorner1Pos, 4, true);
                    CornerClearedIndex++;
                }
                else if (CornerClearedIndex == 1)
                {
                    gameData.pathFinding.MoveToThisPos(ThroneCorner2Pos, 4, true);
                    CornerClearedIndex++;
                }
                else if (CornerClearedIndex == 2)
                {
                    gameData.pathFinding.MoveToThisPos(ThroneCorner4Pos, 4, true);
                    CornerClearedIndex++;
                }
                else if (CornerClearedIndex == 3)
                {
                    gameData.pathFinding.MoveToThisPos(ThroneCorner3Pos, 4, true);
                    CornerClearedIndex++;
                }
                if (CornerClearedIndex == 4)
                {
                    //gameData.pathFinding.MoveToThisPos(ThroneCorner4Pos, 4, true);

                    gameData.pathFinding.MoveToThisPos(ThroneCorner1Pos, 4, true);
                    CurrentStep++;
                }
            }

            if (CurrentStep == 5)
            {
                gameData.SetGameStatus("Baal waiting on leecher");

                gameData.battle.DoBattleScript(15);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.ThroneOfDestruction)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 6)
            {
                //clear waves
                if (gameData.playerScan.xPosFinal < ThronePos.X - 3
                    || gameData.playerScan.xPosFinal > ThronePos.X + 3
                    || gameData.playerScan.yPosFinal < ThronePos.Y - 3
                    || gameData.playerScan.yPosFinal > ThronePos.Y + 3)
                {
                    gameData.pathFinding.MoveToThisPos(ThronePos, 4, true);
                }
                else
                {
                    gameData.battle.DoBattleScript(30);
                }

                if (!Wave5Cleared)
                {
                    //DETECT OTHERS WAVES FOR CASTING
                    if (!TimeSinceLastWaveSet && !gameData.mobsStruc.GetMobs("", "", true, 25, IgnoredMobs))
                    {
                        TimeSinceLastWaveDone = DateTime.Now;
                        TimeSinceLastWaveSet = true;
                    }

                    //START CASTING IN ADVANCE
                    if ((DateTime.Now - TimeSinceLastWaveDone).TotalSeconds > CharConfig.BaalWavesCastDelay)
                    {
                        gameData.battle.SetSkills();
                        gameData.battle.CastSkillsNoMove();
                    }

                    //STOP CASTING
                    if (gameData.mobsStruc.GetMobs("", "", true, 25, IgnoredMobs))
                    {
                        TimeSinceLastWaveDone = DateTime.Now;
                        TimeSinceLastWaveSet = false;
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
                                if (!gameData.mobsStruc.GetMobs("", "", true, 25, IgnoredMobs))
                                {
                                    Wave5Cleared = true;
                                }
                            }
                        }
                    }
                    //####

                    //leecher already in baal chamber.. move to baal chamber then
                    if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.TheWorldstoneChamber)
                    {
                        CurrentStep++;
                    }
                }
                else
                {
                    CurrentStep++;
                }
            }



            if (CurrentStep == 7)
            {
                gameData.SetGameStatus("WAITING PORTAL");

                //move to baal red portal
                if (gameData.playerScan.xPosFinal >= 15170 - 40
                    && gameData.playerScan.xPosFinal <= 15170 + 40
                    && gameData.playerScan.yPosFinal >= 5880 - 40
                    && gameData.playerScan.yPosFinal <= 5880 + 40)
                {
                    gameData.battle.CastDefense();
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
                        if (gameData.mover.MoveToLocation(15095, 5023))
                        {
                            if (gameData.mover.MoveToLocation(15090, 5008))
                            {
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

            if (CurrentStep == 8)
            {
                gameData.SetGameStatus("Baal waiting on leecher #2");

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.TheWorldstoneChamber)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 9)
            {
                gameData.SetGameStatus("MOVING TO BAAL");
                gameData.pathFinding.MoveToThisPos(new Position { X = 15134, Y = 5927 });
                //gameData.WaitDelay(50); //wait a bit to detect baal
                CurrentStep++;
            }

            if (CurrentStep == 10)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING BAAL");
                gameData.mobsStruc.DetectThisMob("getBossName", "Baal", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>()))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        DetectedBaal = true;
                        gameData.battle.RunBattleScriptOnThisMob("getBossName", "Baal", new List<long>());
                    }
                    else
                    {

                        if (gameData.battle.EndBossBattle())
                        {
                            ScriptDone = true;
                        }
                    }
                }
                else
                {
                    gameData.method_1("Baal not detected!", Color.Red);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>())) return; //redetect baal?

                    gameData.potions.CanUseSkillForRegen = true;
                    //gameData.LeaveGame(true);
                    gameData.townStruc.UseLastTP = false;
                    gameData.townStruc.FastTowning = false;
                    ScriptDone = true;
                }
            }

        }
    }

}
