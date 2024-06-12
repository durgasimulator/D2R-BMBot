using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;

public class Chaos : IBot
{
    private GameData gameData;
    //#####################################################
    //#####################################################
    //Special Run Variable
    public bool FastChaos = false;
    //#####################################################
    //#####################################################

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public bool DetectedBoss = false;

    public Position EntrancePos = new Position { X = 7796, Y = 5561 };
    public Position DiabloSpawnPos = new Position { X = 7794, Y = 5294 }; //7800,5286

    public Position TPPos = new Position { X = 7760, Y = 5305 };

    public Position CurrentSealPos = new Position { X = 0, Y = 0 };

    public DateTime StartTimeUniqueBossWaiting = DateTime.Now;
    public bool TimeSetForWaitingUniqueBoss = false;
    public int TryCountWaitingUniqueBoss = 0;

    public int BufferPathFindingMoveSize = 0;
    public int SealType = 0;

    public bool MovedToTPPos = false;
    public bool CastedAtSeis = false;

    public bool FastChaosPopingSeals = false;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
        DetectedBoss = false;
        TimeSetForWaitingUniqueBoss = false;
        TryCountWaitingUniqueBoss = 0;
        StartTimeUniqueBossWaiting = DateTime.Now;
        MovedToTPPos = false;
        CastedAtSeis = false;
        FastChaosPopingSeals = false;
    }

    public void DetectCurrentStep()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.RiverOfFlame) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ChaosSanctuary) CurrentStep = 3;
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

            gameData.townStruc.GoToWPArea(4, 2);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING CHAOS");
                gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if (gameData.playerScan.levelNo == (int)Enums.Area.RiverOfFlame)
                {
                    CurrentStep++;
                }
                else if (gameData.playerScan.levelNo == (int)Enums.Area.ChaosSanctuary)
                {
                    CurrentStep = 3;
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
                if (gameData.playerScan.levelNo == (int)Enums.Area.ChaosSanctuary)
                {
                    CurrentStep++;
                    return;
                }
                //####

                //gameData.pathFinding.MoveToNextArea(Enums.Area.ChaosSanctuary);
                //gameData.pathFinding.MoveToThisPos(EntrancePos);
                //CurrentStep++;

                Position MidPos = new Position { X = 7800, Y = 5761 };
                if (gameData.mover.MoveToLocation(MidPos.X, MidPos.Y))
                {
                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                }
            }

            if (CurrentStep == 2)
            {
                //####
                if (gameData.playerScan.levelNo == (int)Enums.Area.ChaosSanctuary)
                {
                    CurrentStep++;
                    return;
                }
                //####

                if (gameData.mover.MoveToLocation(EntrancePos.X, EntrancePos.Y))
                {
                    if (gameData.PublicGame && !gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();
                    if (!gameData.townStruc.TPSpawned) gameData.battle.CastDefense();
                    gameData.townStruc.TPSpawned = false;

                    BufferPathFindingMoveSize = gameData.pathFinding.AcceptMoveOffset;
                    gameData.pathFinding.AcceptMoveOffset = 15;

                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                if (gameData.playerScan.levelNo != (int)Enums.Area.ChaosSanctuary)
                {
                    CurrentStep--;
                    return;
                }

                CurrentSealPos = gameData.mapAreaStruc.GetPositionOfObject("object", "DiabloSeal5", (int)Enums.Area.ChaosSanctuary, new List<int>());

                if (gameData.playerScan.xPosFinal >= (CurrentSealPos.X - 5)
                    && gameData.playerScan.xPosFinal <= (CurrentSealPos.X + 5)
                    && gameData.playerScan.yPosFinal >= (CurrentSealPos.Y - 5)
                    && gameData.playerScan.yPosFinal <= (CurrentSealPos.Y + 5))
                {
                    int InteractCount = 0;
                    while (InteractCount < 3)
                    {
                        gameData.pathFinding.MoveToObject("DiabloSeal5");
                        gameData.WaitDelay(10);
                        InteractCount++;
                    }

                    //######
                    //KILL VIZIER
                    if (!TimeSetForWaitingUniqueBoss)
                    {
                        StartTimeUniqueBossWaiting = DateTime.Now;
                        TimeSetForWaitingUniqueBoss = true;
                    }

                    if (CurrentSealPos.Y == 5275) SealType = 1;
                    else SealType = 2;

                    if (SealType == 1) gameData.pathFinding.MoveToThisPos(new Position { X = 7691, Y = 5292 }, 4, true);
                    else gameData.pathFinding.MoveToThisPos(new Position { X = 7695, Y = 5316 }, 4, true);

                    gameData.SetGameStatus("WAITING VIZIER " + (TryCountWaitingUniqueBoss + 1) + "/1");

                    bool UniqueDetected = gameData.mobsStruc.GetMobs("getSuperUniqueName", "Grand Vizier of Chaos", false, 200, new List<long>());

                    while (!UniqueDetected && (DateTime.Now - StartTimeUniqueBossWaiting).TotalSeconds < CharConfig.ChaosWaitingSealBossDelay)
                    {
                        UniqueDetected = gameData.mobsStruc.GetMobs("getSuperUniqueName", "Grand Vizier of Chaos", false, 200, new List<long>());

                        gameData.playerScan.GetPositions();
                        gameData.overlayForm.UpdateOverlay();
                        gameData.gameStruc.CheckChickenGameTime();
                        gameData.itemsStruc.GetItems(true);
                        gameData.potions.CheckIfWeUsePotion();
                        gameData.battle.DoBattleScript(10);
                        //###
                        gameData.battle.SetSkills();
                        gameData.battle.CastSkillsNoMove();
                        //###
                        Application.DoEvents();
                    }

                    if (!UniqueDetected)
                    {
                        TimeSetForWaitingUniqueBoss = false;
                        CurrentStep++;
                    }
                    else
                    {
                        gameData.SetGameStatus("KILLING VIZIER");

                        if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Grand Vizier of Chaos", false, 200, new List<long>()))
                        {
                            if (gameData.mobsStruc.MobsHP > 0)
                            {
                                gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Grand Vizier of Chaos", new List<long>());
                            }
                            else
                            {
                                TimeSetForWaitingUniqueBoss = false;
                                CurrentStep++;
                            }
                        }
                        else
                        {
                            TimeSetForWaitingUniqueBoss = false;
                            CurrentStep++;
                        }
                    }
                }
                else
                {
                    if (gameData.playerScan.xPosFinal >= (TPPos.X - 5)
                        && gameData.playerScan.xPosFinal <= (TPPos.X + 5)
                        && gameData.playerScan.yPosFinal >= (TPPos.Y - 5)
                        && gameData.playerScan.yPosFinal <= (TPPos.Y + 5))
                    {
                        if (gameData.PublicGame && !gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();
                        if (!FastChaos && !FastChaosPopingSeals) gameData.battle.CastDefense();
                        gameData.inventoryStruc.DumpBadItemsOnGround();
                        MovedToTPPos = true;
                        gameData.pathFinding.MoveToObject("DiabloSeal5", 4, true);
                    }
                    else
                    {
                        if (!MovedToTPPos) gameData.pathFinding.MoveToThisPos(TPPos, 4, true);
                        else gameData.pathFinding.MoveToObject("DiabloSeal5", 4, true);
                    }
                }
            }

            if (CurrentStep == 4)
            {
                CurrentSealPos = gameData.mapAreaStruc.GetPositionOfObject("object", "DiabloSeal4", (int)Enums.Area.ChaosSanctuary, new List<int>());

                if (gameData.playerScan.xPosFinal >= (CurrentSealPos.X - 5)
                    && gameData.playerScan.xPosFinal <= (CurrentSealPos.X + 5)
                    && gameData.playerScan.yPosFinal >= (CurrentSealPos.Y - 5)
                    && gameData.playerScan.yPosFinal <= (CurrentSealPos.Y + 5))
                {
                    int InteractCount = 0;
                    while (InteractCount < 3)
                    {
                        gameData.pathFinding.MoveToObject("DiabloSeal4");
                        gameData.WaitDelay(10);
                        InteractCount++;
                    }

                    CurrentStep++;
                }
                else
                {
                    gameData.pathFinding.MoveToObject("DiabloSeal4", 4, true);
                }
            }

            if (CurrentStep == 5)
            {
                CurrentSealPos = gameData.mapAreaStruc.GetPositionOfObject("object", "DiabloSeal3", (int)Enums.Area.ChaosSanctuary, new List<int>());

                if (gameData.playerScan.xPosFinal >= (CurrentSealPos.X - 5)
                    && gameData.playerScan.xPosFinal <= (CurrentSealPos.X + 5)
                    && gameData.playerScan.yPosFinal >= (CurrentSealPos.Y - 5)
                    && gameData.playerScan.yPosFinal <= (CurrentSealPos.Y + 5))
                {
                    int InteractCount = 0;
                    while (InteractCount < 3)
                    {
                        gameData.pathFinding.MoveToObject("DiabloSeal3");
                        gameData.WaitDelay(10);
                        InteractCount++;
                    }
                    if (!CastedAtSeis)
                    {
                        CastedAtSeis = true;
                        gameData.battle.CastDefense();
                    }

                    //######
                    //KILL LORD DE SEIS
                    if (!TimeSetForWaitingUniqueBoss)
                    {
                        StartTimeUniqueBossWaiting = DateTime.Now;
                        TimeSetForWaitingUniqueBoss = true;
                    }

                    if (CurrentSealPos.X == 7773) SealType = 1;
                    else SealType = 2;

                    if (SealType == 1)
                    {
                        gameData.pathFinding.MoveToThisPos(new Position { X = 7794, Y = 5227 }, 4, true);
                        //NTM_MoveTo(108, 7797, 5201);
                        //for (int i = 0; i < 3; i += 1) NTM_TeleportTo(7794, 5227);
                    }
                    else gameData.pathFinding.MoveToThisPos(new Position { X = 7798, Y = 5186 }, 4, true);

                    gameData.SetGameStatus("WAITING LORD DE SEIS " + (TryCountWaitingUniqueBoss + 1) + "/1");

                    bool UniqueDetected = gameData.mobsStruc.GetMobs("getSuperUniqueName", "Lord De Seis", false, 200, new List<long>());

                    while (!UniqueDetected && (DateTime.Now - StartTimeUniqueBossWaiting).TotalSeconds < CharConfig.ChaosWaitingSealBossDelay)
                    {
                        UniqueDetected = gameData.mobsStruc.GetMobs("getSuperUniqueName", "Lord De Seis", false, 200, new List<long>());

                        gameData.playerScan.GetPositions();
                        gameData.overlayForm.UpdateOverlay();
                        gameData.gameStruc.CheckChickenGameTime();
                        gameData.itemsStruc.GetItems(true);
                        gameData.potions.CheckIfWeUsePotion();
                        gameData.battle.DoBattleScript(10);
                        //###
                        gameData.battle.SetSkills();
                        gameData.battle.CastSkillsNoMove();
                        //###
                        Application.DoEvents();
                    }

                    if (!UniqueDetected)
                    {
                        //gameData.battle.CastDefense();
                        gameData.inventoryStruc.DumpBadItemsOnGround();
                        TimeSetForWaitingUniqueBoss = false;
                        CurrentStep++;
                    }
                    else
                    {
                        gameData.SetGameStatus("KILLING LORD DE SEIS");

                        if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Lord De Seis", false, 200, new List<long>()))
                        {
                            if (gameData.mobsStruc.MobsHP > 0)
                            {
                                gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Lord De Seis", new List<long>());
                            }
                            else
                            {
                                //gameData.battle.CastDefense();
                                gameData.inventoryStruc.DumpBadItemsOnGround();
                                TimeSetForWaitingUniqueBoss = false;
                                CurrentStep++;
                            }
                        }
                        else
                        {
                            TimeSetForWaitingUniqueBoss = false;
                            CurrentStep++;
                        }
                    }
                    //######
                }
                else
                {
                    gameData.pathFinding.MoveToObject("DiabloSeal3", 4, true);
                }
            }

            if (CurrentStep == 6)
            {
                CurrentSealPos = gameData.mapAreaStruc.GetPositionOfObject("object", "DiabloSeal2", (int)Enums.Area.ChaosSanctuary, new List<int>());

                if (gameData.playerScan.xPosFinal >= (CurrentSealPos.X - 5)
                    && gameData.playerScan.xPosFinal <= (CurrentSealPos.X + 5)
                    && gameData.playerScan.yPosFinal >= (CurrentSealPos.Y - 5)
                    && gameData.playerScan.yPosFinal <= (CurrentSealPos.Y + 5))
                {
                    int InteractCount = 0;
                    while (InteractCount < 3)
                    {
                        gameData.pathFinding.MoveToObject("DiabloSeal2");
                        gameData.WaitDelay(10);
                        InteractCount++;
                    }

                    CurrentStep++;
                }
                else
                {
                    gameData.pathFinding.MoveToObject("DiabloSeal2", 4, true);
                }
            }

            if (CurrentStep == 7)
            {
                CurrentSealPos = gameData.mapAreaStruc.GetPositionOfObject("object", "DiabloSeal1", (int)Enums.Area.ChaosSanctuary, new List<int>());

                if (gameData.playerScan.xPosFinal >= (CurrentSealPos.X - 5)
                    && gameData.playerScan.xPosFinal <= (CurrentSealPos.X + 5)
                    && gameData.playerScan.yPosFinal >= (CurrentSealPos.Y - 5)
                    && gameData.playerScan.yPosFinal <= (CurrentSealPos.Y + 5))
                {
                    int InteractCount = 0;
                    while (InteractCount < 3)
                    {
                        gameData.pathFinding.MoveToObject("DiabloSeal1");
                        gameData.WaitDelay(10);
                        InteractCount++;
                    }

                    //######
                    //KILL INFECTOR
                    if (!TimeSetForWaitingUniqueBoss)
                    {
                        StartTimeUniqueBossWaiting = DateTime.Now;
                        TimeSetForWaitingUniqueBoss = true;
                    }

                    if (CurrentSealPos.X == 7893) SealType = 1;
                    else SealType = 2;

                    if (SealType == 1) SealType = 1; // temp
                    else gameData.pathFinding.MoveToThisPos(new Position { X = 7933, Y = 5299 }, 4, true);

                    gameData.SetGameStatus("WAITING INFECTOR " + (TryCountWaitingUniqueBoss + 1) + "/1");

                    bool UniqueDetected = gameData.mobsStruc.GetMobs("getSuperUniqueName", "Winged Death", false, 200, new List<long>());

                    while (!UniqueDetected && (DateTime.Now - StartTimeUniqueBossWaiting).TotalSeconds < CharConfig.ChaosWaitingSealBossDelay)
                    {
                        UniqueDetected = gameData.mobsStruc.GetMobs("getSuperUniqueName", "Winged Death", false, 200, new List<long>());

                        gameData.playerScan.GetPositions();
                        gameData.overlayForm.UpdateOverlay();
                        gameData.gameStruc.CheckChickenGameTime();
                        gameData.itemsStruc.GetItems(true);
                        gameData.potions.CheckIfWeUsePotion();
                        gameData.battle.DoBattleScript(10);
                        //###
                        gameData.battle.SetSkills();
                        gameData.battle.CastSkillsNoMove();
                        //###
                        Application.DoEvents();
                    }

                    if (!UniqueDetected)
                    {
                        gameData.inventoryStruc.DumpBadItemsOnGround();
                        TimeSetForWaitingUniqueBoss = false;
                        CurrentStep++;
                    }
                    else
                    {
                        gameData.SetGameStatus("KILLING INFECTOR");

                        if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Winged Death", false, 200, new List<long>()))
                        {
                            if (gameData.mobsStruc.MobsHP > 0)
                            {
                                gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Winged Death", new List<long>());
                            }
                            else
                            {
                                gameData.inventoryStruc.DumpBadItemsOnGround();
                                TimeSetForWaitingUniqueBoss = false;
                                CurrentStep++;
                            }
                        }
                        else
                        {
                            gameData.inventoryStruc.DumpBadItemsOnGround();
                            TimeSetForWaitingUniqueBoss = false;
                            CurrentStep++;
                        }
                    }
                    //######
                }
                else
                {
                    gameData.pathFinding.MoveToObject("DiabloSeal1", 4, true);
                }
            }

            if (CurrentStep == 8)
            {
                if (gameData.pathFinding.MoveToThisPos(DiabloSpawnPos, 4, true))
                {
                    //gameData.pathFinding.MoveToThisPos(DiabloSpawnPos, 4, true);
                    if (!FastChaos) gameData.battle.CastDefense();
                    CurrentStep++;
                }
            }

            if (CurrentStep == 9)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING DIABLO");

                //#############
                bool DetectedDiablo = gameData.mobsStruc.GetMobs("getBossName", "Diablo", false, 200, new List<long>());
                DateTime StartTime = DateTime.Now;
                TimeSpan TimeSinceDetecting = DateTime.Now - StartTime;
                while (!DetectedDiablo && TimeSinceDetecting.TotalSeconds < 13)
                {
                    gameData.SetGameStatus("WAITING DETECTING DIABLO");
                    DetectedDiablo = gameData.mobsStruc.GetMobs("getBossName", "Diablo", false, 200, new List<long>());
                    TimeSinceDetecting = DateTime.Now - StartTime;

                    //cast attack during this waiting time
                    /*gameData.battle.SetSkills();
                    gameData.battle.CastSkills();*/
                    gameData.itemsStruc.GetItems(true);      //#############
                    gameData.potions.CheckIfWeUsePotion();

                    if (!gameData.gameStruc.IsInGame() || !gameData.Running)
                    {
                        gameData.overlayForm.ResetMoveToLocation();
                        return;
                    }
                }

                if (TimeSinceDetecting.TotalSeconds >= 13)
                {
                    gameData.mobsStruc.DetectThisMob("getBossName", "Diablo", false, 200, new List<long>());
                    gameData.method_1("Waited too long for Diablo repoping the seals!", Color.OrangeRed);
                    FastChaosPopingSeals = true;
                    CurrentStep = 3;
                    return;
                }
                //#############

                if (gameData.mobsStruc.GetMobs("getBossName", "Diablo", false, 200, new List<long>()))
                {
                    gameData.SetGameStatus("KILLING DIABLO");
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        DetectedBoss = true;
                        gameData.battle.RunBattleScriptOnThisMob("getBossName", "Diablo", new List<long>());
                    }
                    else
                    {
                        if (!DetectedBoss)
                        {
                            gameData.method_1("Diablo not detected!", Color.Red);
                            gameData.battle.DoBattleScript(15);
                        }

                        gameData.pathFinding.AcceptMoveOffset = BufferPathFindingMoveSize;
                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        return;
                        //gameData.LeaveGame(true);
                    }
                }
                else
                {
                    gameData.method_1("Diablo not detected!", Color.Red);

                    gameData.battle.DoBattleScript(15);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Diablo", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Diablo", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    gameData.pathFinding.AcceptMoveOffset = BufferPathFindingMoveSize;
                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    return;
                    //gameData.LeaveGame(true);
                }
            }

        }
    }


    static int[] FindBestPosition(int playerX, int playerY, List<int[]> monsterPositions, int maxDisplacement)
    {
        // Create a list to store all possible positions around the player
        List<int[]> possiblePositions = new List<int[]>();

        // Generate all possible positions within the maximum displacement range
        for (int x = playerX - maxDisplacement; x <= playerX + maxDisplacement; x++)
        {
            for (int y = playerY - maxDisplacement; y <= playerY + maxDisplacement; y++)
            {
                // Calculate the distance between the player and the current position
                double distance = Math.Sqrt(Math.Pow(playerX - x, 2) + Math.Pow(playerY - y, 2));

                // Check if the distance is within the maximum displacement and the position is not occupied by a monster
                if (distance <= maxDisplacement && !IsMonsterPosition(x, y, monsterPositions))
                {
                    //possiblePositions.Add(Tuple.Create(x, y));
                    possiblePositions.Add(new int[2] { x, y });
                }
            }
        }

        // Find the closest position among the possible positions
        //int[] bestPosition = Tuple.Create(playerX, playerY);
        int[] bestPosition = new int[2] { playerX, playerY };
        double closestDistance = double.MaxValue;
        foreach (var position in possiblePositions)
        {
            double distance = Math.Sqrt(Math.Pow(playerX - position[0], 2) + Math.Pow(playerY - position[1], 2));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestPosition = position;
            }
        }

        return bestPosition;
    }

    static bool IsMonsterPosition(int x, int y, List<int[]> monsterPositions)
    {
        foreach (var monsterPosition in monsterPositions)
        {
            if (monsterPosition[0] >= x - 8
                && monsterPosition[0] <= x + 8
                && monsterPosition[1] >= y - 8
                && monsterPosition[1] <= y + 8)
            {
                return true;
            }
        }
        return false;
    }

}
