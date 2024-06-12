using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;

public class ChaosRush : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public bool DetectedBoss = false;

    public Position EntrancePos = new Position { X = 7796, Y = 5561 };
    public Position DiabloSpawnPos = new Position { X = 7800, Y = 5286 };

    public Position CurrentSealPos = new Position { X = 0, Y = 0 };

    public DateTime StartTimeUniqueBossWaiting = DateTime.Now;
    public bool TimeSetForWaitingUniqueBoss = false;
    public int TryCountWaitingUniqueBoss = 0;

    public int BufferPathFindingMoveSize = 0;
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
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if (gameData.playerScan.levelNo == (int)Enums.Area.RiverOfFlame)
                {
                    gameData.townStruc.SpawnTP();
                    gameData.WaitDelay(15);
                    gameData.battle.CastDefense();
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

                gameData.SetGameStatus("Chaos waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(15);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.ChaosSanctuary)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
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
                    else
                    {
                        gameData.SetGameStatus("WAITING VIZIER " + (TryCountWaitingUniqueBoss + 1) + "/3");

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
                            Application.DoEvents();
                        }

                        if (!UniqueDetected)
                        {
                            if (TryCountWaitingUniqueBoss < 3)
                            {
                                TryCountWaitingUniqueBoss++;
                                StartTimeUniqueBossWaiting = DateTime.Now;
                            }
                            else
                            {
                                TimeSetForWaitingUniqueBoss = false;
                                CurrentStep++;
                            }
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
                        }
                    }
                }
                else
                {
                    gameData.pathFinding.MoveToObject("DiabloSeal5", 4, true);
                }
            }

            if (CurrentStep == 5)
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

            if (CurrentStep == 6)
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

                    //######
                    //KILL LORD DE SEIS
                    if (!TimeSetForWaitingUniqueBoss)
                    {
                        StartTimeUniqueBossWaiting = DateTime.Now;
                        TimeSetForWaitingUniqueBoss = true;
                    }
                    else
                    {
                        gameData.SetGameStatus("WAITING LORD DE SEIS " + (TryCountWaitingUniqueBoss + 1) + "/3");

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
                            Application.DoEvents();
                        }

                        if (!UniqueDetected)
                        {
                            if (TryCountWaitingUniqueBoss < 3)
                            {
                                TryCountWaitingUniqueBoss++;
                                StartTimeUniqueBossWaiting = DateTime.Now;
                            }
                            else
                            {
                                TimeSetForWaitingUniqueBoss = false;
                                CurrentStep++;
                            }
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
                                    TimeSetForWaitingUniqueBoss = false;
                                    CurrentStep++;
                                }
                            }
                        }
                    }
                    //######
                }
                else
                {
                    gameData.pathFinding.MoveToObject("DiabloSeal3", 4, true);
                }
            }

            if (CurrentStep == 7)
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

                gameData.townStruc.TPSpawned = false;
            }

            if (CurrentStep == 8)
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
                    else
                    {
                        gameData.SetGameStatus("WAITING INFECTOR " + (TryCountWaitingUniqueBoss + 1) + "/3");

                        bool UniqueDetected = gameData.mobsStruc.GetMobs("getSuperUniqueName", "Infector of Souls", false, 200, new List<long>());

                        while (!UniqueDetected && (DateTime.Now - StartTimeUniqueBossWaiting).TotalSeconds < CharConfig.ChaosWaitingSealBossDelay)
                        {
                            UniqueDetected = gameData.mobsStruc.GetMobs("getSuperUniqueName", "Infector of Souls", false, 200, new List<long>());

                            gameData.playerScan.GetPositions();
                            gameData.overlayForm.UpdateOverlay();
                            gameData.gameStruc.CheckChickenGameTime();
                            gameData.itemsStruc.GetItems(true);
                            gameData.potions.CheckIfWeUsePotion();
                            gameData.battle.DoBattleScript(10);
                            Application.DoEvents();
                        }

                        if (!UniqueDetected)
                        {
                            if (TryCountWaitingUniqueBoss < 3)
                            {
                                TryCountWaitingUniqueBoss++;
                                StartTimeUniqueBossWaiting = DateTime.Now;
                            }
                            else
                            {
                                TimeSetForWaitingUniqueBoss = false;
                                CurrentStep++;
                            }
                        }
                        else
                        {
                            gameData.SetGameStatus("KILLING INFECTOR");

                            if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Infector of Souls", false, 200, new List<long>()))
                            {
                                if (gameData.mobsStruc.MobsHP > 0)
                                {
                                    gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Infector of Souls", new List<long>());
                                }
                                else
                                {
                                    TimeSetForWaitingUniqueBoss = false;
                                    CurrentStep++;
                                }
                            }
                        }
                    }
                    //######
                }
                else
                {
                    gameData.pathFinding.MoveToObject("DiabloSeal1", 4, true);
                }
            }

            if (CurrentStep == 9)
            {
                gameData.SetGameStatus("Chaos waiting on leecher #2");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(15);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.ChaosSanctuary)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 10)
            {
                gameData.pathFinding.MoveToThisPos(DiabloSpawnPos);
                CurrentStep++;
            }

            if (CurrentStep == 11)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING DIABLO");

                //#############
                gameData.mobsStruc.DetectThisMob("getBossName", "Diablo", false, 200, new List<long>());
                bool DetectedDiablo = gameData.mobsStruc.GetMobs("getBossName", "Diablo", false, 200, new List<long>());
                DateTime StartTime = DateTime.Now;
                TimeSpan TimeSinceDetecting = DateTime.Now - StartTime;
                while (!DetectedDiablo && TimeSinceDetecting.TotalSeconds < 12)
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

                if (TimeSinceDetecting.TotalSeconds >= 12)
                {
                    gameData.method_1("Waited too long for Diablo repoping the seals!", Color.Red);
                    CurrentStep = 4;
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
                    gameData.method_1("Diablo not detected!", Color.Red);

                    gameData.battle.DoBattleScript(15);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Diablo", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Diablo", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    gameData.pathFinding.AcceptMoveOffset = BufferPathFindingMoveSize;
                    gameData.townStruc.UseLastTP = false;
                    gameData.townStruc.FastTowning = false;
                    ScriptDone = true;
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
