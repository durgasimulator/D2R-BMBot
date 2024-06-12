using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;

public class ChaosLeech
{
    GameData gameData;
    public int CurrentStep = 0;
    public int MaxGameTimeToEnter = CharConfig.MaxTimeEnterGame; //6mins
    public int MaxTimeWaitedForTP = (2 * 60) * 2; //2mins
    public int TimeWaitedForTP = 0;
    public bool PrintedInfos = false;
    public bool ScriptDone { get; set; } = false;

    public bool SearchSameGamesAsLastOne = false;
    public int SameGameRetry = 0;

    public List<uint> IgnoredTPList = new List<uint>();
    public uint LastUsedTP_ID = 0;
    public bool AddingIgnoredTP_ID = false;

    public bool LeechDetectedCorrectly = false;

    //List<Tuple<int, int>> GoodPositions = new List<Tuple<int, int>>();

    public int LastLeechPosX = 0;
    public int LastLeechPosY = 0;

    List<int[]> LastLeechPositions = new List<int[]>();
    public int LastLeechPosXNEW = 0;
    public int LastLeechPosYNEW = 0;

    public bool CastedDefense = false;

    public void RunScriptNOTInGame()
    {
        gameData = GameData.Instance;
        TimeWaitedForTP = 0;
        CurrentStep = 0;
        LastLeechPosX = 0;
        LastLeechPosY = 0;
        LastLeechPosXNEW = 0;
        LastLeechPosYNEW = 0;
        PrintedInfos = false;
        CastedDefense = false;
        //GoodPositions = new List<Tuple<int, int>>();
        LastLeechPositions = new List<int[]>();
        gameData.playerScan.LeechPlayerUnitID = 0;
        gameData.playerScan.LeechPlayerPointer = 0;
        gameData.gameStruc.GetAllGamesNames();
        IgnoredTPList = new List<uint>();
        LastUsedTP_ID = 0;
        bool EnteredGammme = false;
        LeechDetectedCorrectly = false;
        ScriptDone = false;

        string LastGameName = gameData.gameStruc.GameName;
        string SearchSameGame = "";
        if (LastGameName != "" && SameGameRetry < 20 && SearchSameGamesAsLastOne)
        {
            SearchSameGame = LastGameName.Substring(0, LastGameName.Length - 2);
        }

        if (gameData.gameStruc.AllGamesNames.Count > 0)
        {
            List<int> PossibleGamesIndex = new List<int>();

            for (int i = 0; i < gameData.gameStruc.AllGamesNames.Count; i++)
            {
                if (!gameData.Running)
                {
                    break;
                }

                if (SearchSameGame != "")
                {
                    if (gameData.gameStruc.AllGamesNames[i].ToLower().Contains(SearchSameGame.ToLower())
                        && gameData.gameStruc.AllGamesNames[i] != LastGameName)
                    {
                        PossibleGamesIndex.Add(i);
                    }
                }
                else
                {
                    if (gameData.gameStruc.AllGamesNames[i].ToLower().Contains("chaos")
                        && !gameData.gameStruc.IsIncludedInListString(CharConfig.ChaosSearchAvoidWords, gameData.gameStruc.AllGamesNames[i].ToLower())
                        && gameData.gameStruc.AllGamesNames[i] != LastGameName) //not equal last gamename
                    {
                        if (!gameData.gameStruc.TriedThisGame(gameData.gameStruc.AllGamesNames[i]))
                        {
                            PossibleGamesIndex.Add(i);
                        }
                    }
                }
            }

            //##
            if (PossibleGamesIndex.Count > 0)
            {
                for (int i = 0; i < PossibleGamesIndex.Count; i++)
                {
                    if (!gameData.Running)
                    {
                        break;
                    }

                    gameData.SetGameStatus("SEARCHING:" + gameData.gameStruc.AllGamesNames[PossibleGamesIndex[i]]);
                    gameData.method_1("Checking Game: " + PossibleGamesIndex[i], Color.Black);

                    gameData.gameStruc.SelectGame(PossibleGamesIndex[i], false);
                    if (!gameData.gameStruc.SelectedGameName.Contains(gameData.gameStruc.AllGamesNames[PossibleGamesIndex[i]]))
                    {
                        continue;
                    }
                    if (gameData.gameStruc.SelectedGameTime < MaxGameTimeToEnter)
                    {
                        gameData.gameStruc.SelectGame(PossibleGamesIndex[i], true);
                        gameData.WaitDelay(300);
                        EnteredGammme = true;
                        break;
                    }
                    else
                    {
                        gameData.method_1("Game: " + gameData.gameStruc.AllGamesNames[PossibleGamesIndex[i]] + " exceed MaxGameTime of " + (MaxGameTimeToEnter / 60) + "mins", Color.DarkOrange);
                        gameData.gameStruc.AllGamesTriedNames.Add(gameData.gameStruc.AllGamesNames[PossibleGamesIndex[i]]);
                    }
                }
            }

            if (!EnteredGammme)
            {
                if (SearchSameGame != "")
                {
                    SameGameRetry++;
                }
            }
        }
    }

    public void RunScript()
    {
        gameData = GameData.Instance;
        gameData.townStruc.ScriptTownAct = 4; //set to town act 4 when running this script
        SameGameRetry = 0;
        GetLeechInfo();
        if (gameData.playerScan.LeechPlayerPointer == 0 || gameData.playerScan.LeechPlayerUnitID == 0)
        {
            ScriptDone = true;
            return;
        }

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("WAITING TP");
            CurrentStep = 0;
            gameData.mover.MoveToLocation(5055, 5039); //move to wp spot

            //use tp
            if (gameData.objectsStruc.GetObjects("TownPortal", true, IgnoredTPList, 999, "", (int)Enums.Area.ChaosSanctuary))
            {
                if (!CastedDefense)
                {
                    gameData.battle.CastDefense();
                    CastedDefense = true;
                    gameData.WaitDelay(30);
                }

                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.WaitDelay(100);
                //gameData.mover.FinishMoving();
            }
            else
            {
                //gameData.method_1("NO TP FOUND NEAR IN TOWN");
                if (TimeWaitedForTP >= MaxTimeWaitedForTP)
                {
                    gameData.method_1("Leaving reason: Waited too long for tp", Color.Black);
                    gameData.LeaveGame(false);
                }
                else
                {
                    gameData.WaitDelay(CharConfig.LeechEnterTPDelay);
                    TimeWaitedForTP++;
                }
            }
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.battle.CastDefense();
                CurrentStep++;
            }
            else
            {
                gameData.SetGameStatus("LEECHING");

                //not correct location check
                if (gameData.playerScan.levelNo != (int)Enums.Area.ChaosSanctuary)
                {

                    if (gameData.objectsStruc.GetObjects("TownPortal", true, IgnoredTPList, 999, "", (int)Enums.Area.ThePandemoniumFortress))
                    {
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                        gameData.WaitDelay(100);
                    }

                    gameData.method_1("Added ignored TP ID: " + LastUsedTP_ID, Color.OrangeRed);

                    IgnoredTPList.Add(LastUsedTP_ID);
                    gameData.townStruc.UseLastTP = false;
                    AddingIgnoredTP_ID = true;
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.GoToTown();
                    return;
                }

                gameData.playerScan.GetLeechPositions();

                while (gameData.playerScan.LeechlevelNo != 108 && !LeechDetectedCorrectly)
                {
                    if (!gameData.Running || !gameData.gameStruc.IsInGame())
                    {
                        ScriptDone = true;
                        return;
                    }

                    int PlayerLeechIndex = 1;

                    while (true)
                    {
                        gameData.gameStruc.GameOwnerName = gameData.gameStruc.AllPlayersNames[PlayerLeechIndex];
                        GetLeechInfo();
                        if (gameData.playerScan.LeechPlayerPointer == 0 || gameData.playerScan.LeechPlayerUnitID == 0) return;
                        gameData.playerScan.GetLeechPositions();

                        if (!gameData.Running || !gameData.gameStruc.IsInGame())
                        {
                            ScriptDone = true;
                            return;
                        }

                        if (gameData.playerScan.LeechlevelNo != 108)
                        {
                            PlayerLeechIndex++;
                            if (PlayerLeechIndex > gameData.gameStruc.AllPlayersNames.Count - 1)
                            {
                                gameData.townStruc.FastTowning = false;
                                gameData.townStruc.GoToTown();
                                break;
                            }
                        }
                        else
                        {
                            gameData.method_1("Leecher Changed to: " + gameData.gameStruc.GameOwnerName, Color.DarkGreen);
                            break;
                        }
                    }
                }

                LeechDetectedCorrectly = true;
                SearchSameGamesAsLastOne = false;

                //gameData.method_1("Leecher: " + gameData.playerScan.LeechPosX + ", " + gameData.playerScan.LeechPosY);
                /*if (gameData.playerScan.LeechPosX == 0 && gameData.playerScan.LeechPosY == 0)
                {
                    //gameData.LeaveGame();
                    return;
                }*/

                List<int[]> monsterPositions = gameData.mobsStruc.GetAllMobsNearby();

                if (IsMonsterPosition(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, monsterPositions))
                {
                    int IndexToCheck = LastLeechPositions.Count - 2;
                    if (IndexToCheck >= 0)
                    {

                        bool MonsterNearby = IsMonsterPosition(LastLeechPositions[IndexToCheck][0], LastLeechPositions[IndexToCheck][1], monsterPositions);
                        while (MonsterNearby)
                        {
                            if (!gameData.Running || !gameData.gameStruc.IsInGame())
                            {
                                ScriptDone = true;
                                return;
                            }

                            IndexToCheck--;

                            if (IndexToCheck >= 0)
                            {
                                //monsterPositions = gameData.mobsStruc.GetAllMobsNearby();
                                MonsterNearby = IsMonsterPosition(LastLeechPositions[IndexToCheck][0], LastLeechPositions[IndexToCheck][1], monsterPositions);
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (!MonsterNearby)
                        {
                            gameData.method_1("Move to Safe Pos: " + LastLeechPositions[IndexToCheck][0] + ", " + LastLeechPositions[IndexToCheck][1], Color.DarkGreen);
                            gameData.mover.MoveToLocation(LastLeechPositions[IndexToCheck][0], LastLeechPositions[IndexToCheck][1], true);
                        }
                    }
                }

                gameData.playerScan.GetLeechPositions();
                if (gameData.playerScan.LeechlevelNo != 108) return;
                if (gameData.playerScan.LeechPosX == 0 && gameData.playerScan.LeechPosY == 0)
                {
                    //gameData.LeaveGame();
                    return;
                }

                //Move to leecher
                if (LastLeechPosXNEW != gameData.playerScan.LeechPosX && LastLeechPosYNEW != gameData.playerScan.LeechPosY)
                {
                    //LastLeechPositions.Add(Tuple.Create(gameData.playerScan.LeechPosX, gameData.playerScan.LeechPosY));
                    LastLeechPositions.Add(new int[2] { gameData.playerScan.LeechPosX, gameData.playerScan.LeechPosY });

                    LastLeechPosXNEW = gameData.playerScan.LeechPosX;
                    LastLeechPosYNEW = gameData.playerScan.LeechPosY;
                }

                /*Tuple<int, int> bestPosition = FindBestPosition(gameData.playerScan.LeechPosX, gameData.playerScan.LeechPosY, monsterPositions, 5);

                if (bestPosition[0] != 0 && bestPosition[1] != 0)
                {
                    if (bestPosition[0] != LastLeechPosX && bestPosition[1] != LastLeechPosY)
                    {
                        if (gameData.playerScan.LeechlevelNo != 108) return;

                        GoodPositions.Add(Tuple.Create(bestPosition[0], bestPosition[1]));
                        gameData.method_1("Move to Leecher Pos: " + bestPosition[0] + ", " + bestPosition[1], Color.DarkGreen);
                        gameData.mover.MoveToLocation(bestPosition[0], bestPosition[1]);

                        LastLeechPosX = bestPosition[0];
                        LastLeechPosY = bestPosition[1];
                    }
                    else
                    {*/
                int ThisCheckIndex = LastLeechPositions.Count - 1;
                while (true)
                {
                    if (!gameData.Running || !gameData.gameStruc.IsInGame())
                    {
                        ScriptDone = true;
                        return;
                    }

                    int[] bestPosition = FindBestPosition(LastLeechPositions[ThisCheckIndex][0], LastLeechPositions[ThisCheckIndex][1], monsterPositions, 4);
                    if (bestPosition[0] != 0 && bestPosition[1] != 0)
                    {
                        if (bestPosition[0] != LastLeechPosX && bestPosition[1] != LastLeechPosY)
                        {
                            if (IsMonsterPosition(bestPosition[0], bestPosition[1], monsterPositions))
                            {
                                ThisCheckIndex--;
                                if (ThisCheckIndex < 0)
                                {
                                    break;
                                }
                                continue;
                            }
                            if (bestPosition[0] - gameData.playerScan.xPosFinal > 300
                                || bestPosition[0] - gameData.playerScan.xPosFinal < -300
                                || bestPosition[1] - gameData.playerScan.yPosFinal > 300
                                || bestPosition[1] - gameData.playerScan.yPosFinal < -300)
                            {
                                break;
                            }

                            if (gameData.playerScan.LeechlevelNo != 108) return;

                            //GoodPositions.Add(Tuple.Create(bestPosition[0], bestPosition[1]));
                            gameData.method_1("Move to Leecher Pos #2: " + bestPosition[0] + ", " + bestPosition[1], Color.DarkGreen);
                            gameData.mover.MoveToLocation(bestPosition[0], bestPosition[1], true);

                            LastLeechPosX = bestPosition[0];
                            LastLeechPosY = bestPosition[1];
                            break;
                        }
                        else
                        {
                            if (IsMonsterPosition(LastLeechPosX, LastLeechPosY, monsterPositions))
                            {
                                ThisCheckIndex--;
                                if (ThisCheckIndex < 0)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                //    }
                //}


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

    public void GetLeechInfo()
    {
        gameData.playerScan.ScanForLeecher();

        if (!PrintedInfos)
        {
            gameData.method_1("Leecher name: " + gameData.gameStruc.GameOwnerName, Color.DarkTurquoise);
            gameData.method_1("Leecher pointer: 0x" + gameData.playerScan.LeechPlayerPointer.ToString("X"), Color.DarkTurquoise);
            gameData.method_1("Leecher unitID: 0x" + gameData.playerScan.LeechPlayerUnitID.ToString("X"), Color.DarkTurquoise);
            PrintedInfos = true;
        }

        //LEECHER NOT IN GAME
        if (gameData.playerScan.LeechPlayerPointer == 0 || gameData.playerScan.LeechPlayerUnitID == 0)
        {
            if (gameData.Running && gameData.gameStruc.IsInGame())
            {
                gameData.itemsStruc.GrabAllItemsForGold();
                SearchSameGamesAsLastOne = true;
                gameData.LeaveGame(true);
            }
        }
    }

    /*public void RunScript()
    {
        if (gameData.townStruc.GetInTown())
        {
            gameData.mover.MoveToLocation(5055, 5039); //move to wp spot

            //use wp
            if (gameData.objectsStruc.GetObjects("PandamoniumFortressWaypoint"))
            {
                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);
                        itemScreenPos = gameData.mover.FixMousePositionWithScreenSize(itemScreenPos);
                gameData.keyMouse.MouseClicc(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.mover.FinishMoving();
                if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                {
                    gameData.keyMouse.MouseClicc(450, 390);
                    gameData.WaitDelay(50);
                    gameData.uiScan.WaitTilUIClose("waypointMenu");
                    gameData.uiScan.WaitTilUIClose("loading");
                }
                else
                {
                    gameData.method_1("WP MENU NOT OPENED");
                }
            }
            else
            {
                gameData.method_1("NO WP FOUND NEAR IN TOWN");
            }
        }
        else
        {
            if (CurrentStep == 0)
            {
                //cast sacred shield
                gameData.keyMouse.PressKey(KeySkillCastDefense);
                gameData.WaitDelay(5);
                gameData.keyMouse.MouseCliccRight(gameData.ScreenX / 2, gameData.ScreenY / 2);
                //start moving to chaos
                if (gameData.mover.MoveToLocation(7794, 5868))
                {
                    CurrentStep++;
                    gameData.playerScan.GetPositions();
                }
            }
            else if (CurrentStep == 1)
            {
                if (gameData.mover.MoveToLocation(7800, 5826))
                {
                    CurrentStep++;
                    gameData.playerScan.GetPositions();
                }
            }
            else if (CurrentStep == 2)
            {
                gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);
                CurrentStep++;
            }
            else if (CurrentStep == 3)
            {
                //7800,5815 - spot1
                if (gameData.mover.MoveToLocation(7800, 5815))
                {
                    CurrentStep++;
                    gameData.playerScan.GetPositions();
                }
            }
            if (CurrentStep == 4)
            {
                //try right
                bool TryingLeft = false;
                if (gameData.mover.MoveToLocation(7820, 5815))
                {
                    gameData.playerScan.GetPositions();
                    gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                    if (gameData.mover.MoveToLocation(7840, 5810))
                    {
                        gameData.playerScan.GetPositions();
                        gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                        if (gameData.mover.MoveToLocation(7840, 5775))
                        {
                            gameData.playerScan.GetPositions();
                            gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                            if (gameData.mover.MoveToLocation(7840, 5740))
                            {
                                gameData.playerScan.GetPositions();
                                gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                                if (gameData.mover.MoveToLocation(7840, 5730))
                                {
                                    gameData.playerScan.GetPositions();
                                    gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                                    CurrentStep++;
                                }
                                else
                                {
                                    gameData.mover.MoveToLocation(7840, 5810); //go back
                                    gameData.mover.MoveToLocation(7820, 5815); //go back
                                    TryingLeft = true;
                                }
                            }
                            else
                            {
                                gameData.mover.MoveToLocation(7840, 5810); //go back
                                gameData.mover.MoveToLocation(7820, 5815); //go back
                                TryingLeft = true;
                            }
                        }
                        else
                        {
                            gameData.mover.MoveToLocation(7840, 5810); //go back
                            gameData.mover.MoveToLocation(7820, 5815); //go back
                            TryingLeft = true;
                        } 
                    }
                    else
                    {
                        gameData.mover.MoveToLocation(7820, 5815); //go back
                        TryingLeft = true;
                    }
                }

                if (TryingLeft)
                {
                    if (gameData.mover.MoveToLocation(7780, 5815))
                    {
                        gameData.playerScan.GetPositions();
                        gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                        if (gameData.mover.MoveToLocation(7780, 5790))
                        {
                            gameData.playerScan.GetPositions();
                            gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                            if (gameData.mover.MoveToLocation(7760, 5790))
                            {
                                gameData.playerScan.GetPositions();
                                gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                                if (gameData.mover.MoveToLocation(7760, 5760))
                                {
                                    gameData.playerScan.GetPositions();
                                    gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                                    if (gameData.mover.MoveToLocation(7760, 5740))
                                    {
                                        gameData.playerScan.GetPositions();
                                        gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                                        if (gameData.mover.MoveToLocation(7780, 5735))
                                        {
                                            gameData.playerScan.GetPositions();
                                            gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);
                                            gameData.mover.MoveToLocation(7780, 5730); //###

                                            if (gameData.mover.MoveToLocation(7800, 5730))
                                            {
                                                gameData.playerScan.GetPositions();
                                                gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);

                                                if (gameData.mover.MoveToLocation(7800, 5705))
                                                {
                                                    gameData.playerScan.GetPositions();
                                                    gameData.battle.ClearAreaOfMobs(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 15);
                                                    CurrentStep++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (CurrentStep == 5)
            {

            }
        }
    }*/


}
