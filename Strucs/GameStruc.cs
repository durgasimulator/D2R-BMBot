using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.AxHost;
using System.Threading;
using static MapAreaStruc;
using static Enums;

public class GameStruc
{

    GameData gameData = GameData.Instance;
    public string GameName = "";
    public string GameDifficulty = "";
    public string GameOwnerName = "";
    public List<string> AllGamesNames = new List<string>();
    public List<int> AllGamesPlayersCount = new List<int>();

    public List<string> AllPlayersNames = new List<string>();
    public int SelectedGamePlayerCount = 0;
    public int SelectedGameTime = 0;

    public string SelectedGameName = "";

    public List<string> AllGamesTriedNames = new List<string>();
    public bool AlreadyChickening = false;
    public bool TypedSearchGames = false;

    public int CurrentTZAct = 1;

    [DllImport("user32.dll")] static extern short VkKeyScan(char ch);

    [StructLayout(LayoutKind.Explicit)]
    struct Helper
    {
        [FieldOffset(0)] public short Value;
        [FieldOffset(0)] public byte Low;
        [FieldOffset(1)] public byte High;
    }

    public void extract()
    {
        long gameNameOffset = (long)gameData.BaseAddress + (long)gameData.offsets["AllGamesOffset"];
        byte[] unitTableBuffer = new byte[0xfff];
        gameData.mem.ReadRawMemory(gameNameOffset, ref unitTableBuffer, unitTableBuffer.Length);

        string SavePathh = gameData.form.ThisEndPath + "DumpGameStruc";
        File.Create(SavePathh).Dispose();
        File.WriteAllBytes(SavePathh, unitTableBuffer);

        GetAllGamesNames();
    }

    public bool IsPlayerConnectedToBnet()
    {
        long baseAddr = (long)gameData.BaseAddress + (long)gameData.offsets["SelectedChar"] - 16;
        return gameData.mem.ReadByteRaw((IntPtr)baseAddr) != 0x00;
    }

    public void ClicCreateNewChar()
    {
        gameData.method_1("Connecting to BNet/Creating new Char", Color.OrangeRed);
        gameData.method_1("-> If this happens anytime after a game ends, report as bug!", Color.OrangeRed);

        gameData.keyMouse.MouseClicc(1620, 50); //clic online button
        gameData.WaitDelay(200);

        gameData.keyMouse.MouseClicc(1700, 935); //clic create new char
        gameData.WaitDelay(200);
        gameData.keyMouse.PressKey(Keys.Escape); //leave create new char menu
        gameData.WaitDelay(200);
    }

    public void CreateNewGame(int ThisGameNumber)
    {
        if (gameData.TriedToCreateNewGameCount > 0)
        {
            gameData.keyMouse.MouseClicc(960, 580); //clic 'ok' if game already exist
            Thread.Sleep(300);
        }

        gameData.SetGameStatus("CREATING GAME");
        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        gameData.keyMouse.MouseClicc(1190, 990); //clic 'salon' if not in server
        gameData.keyMouse.MouseClicc(1275, 65);  //clic 'create game' if not in game create area

        gameData.keyMouse.MouseClicc(1550, 170);  //clic 'gamename'
                                                   //type game name
        for (int i = 0; i < 16; i++)
        {
            gameData.keyMouse.PressKey(Keys.Back);
            Thread.Sleep(3);
        }
        Thread.Sleep(3);
        string GameName = CharConfig.GameName + ThisGameNumber.ToString("000");
        gameData.method_1("Creating Game: " + GameName, Color.Black);
        for (int i = 0; i < GameName.Length; i++)
        {
            var helper = new Helper { Value = VkKeyScan(GameName[i]) };
            byte virtualKeyCode = helper.Low;
            gameData.keyMouse.PressKey2((Keys)virtualKeyCode);
            Thread.Sleep(5);
        }


        gameData.keyMouse.MouseClicc(1550, 240);  //clic 'gamepass'
                                                   //type game pass
        for (int i = 0; i < 16; i++)
        {
            gameData.keyMouse.PressKey(Keys.Back);
            Thread.Sleep(3);
        }
        Thread.Sleep(3);
        for (int i = 0; i < CharConfig.GamePass.Length; i++)
        {
            var helper = new Helper { Value = VkKeyScan(CharConfig.GamePass[i]) };
            byte virtualKeyCode = helper.Low;
            gameData.keyMouse.PressKey2((Keys)virtualKeyCode);
            Thread.Sleep(5);
        }

        //select difficulty
        gameData.keyMouse.MouseClicc(1360 + (100 * CharConfig.GameDifficulty), 375);

        gameData.keyMouse.MouseClicc(1470, 670);  //clic 'create game'

        gameData.TriedToCreateNewGameCount++;

        gameData.SetGameStatus("LOADING GAME");

        gameData.WaitDelay(CharConfig.CreateGameWaitDelay);

        //###############
        /*GetAllGamesNames();
        SelectGame(0, false);
        SelectGame(0, true);*/
    }

    public bool IsIncludedInListString(List<string> IgnoredIDList, string ThisID)
    {
        if (IgnoredIDList != null)
        {
            if (IgnoredIDList.Count > 0)
            {
                for (int i = 0; i < IgnoredIDList.Count; i++)
                {
                    if (IgnoredIDList[i] == ThisID)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void GetAllGamesNames()
    {
        AllGamesNames = new List<string>();
        AllGamesPlayersCount = new List<int>();
        ClicTopRow();
        gameData.keyMouse.MouseClicc(1190, 990); //clic 'salon' if not in server
        gameData.keyMouse.MouseClicc(1415, 65);  //clic 'join game' if not in game list area

        //#####
        if (!TypedSearchGames)
        {
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            gameData.keyMouse.MouseClicc(1450, 210); //clic search bar
                                                      //type 'search' type games
            for (int i = 0; i < 16; i++)
            {
                gameData.keyMouse.PressKey(Keys.Back);
                Thread.Sleep(3);
            }
            Thread.Sleep(3);
            string GameName = "";
            if (CharConfig.RunBaalSearchGameScript && !CharConfig.RunItemGrabScriptOnly) GameName = CharConfig.BaalLeechSearch;
            if (CharConfig.RunChaosSearchGameScript && !CharConfig.RunItemGrabScriptOnly) GameName = CharConfig.ChaosLeechSearch;
            for (int i = 0; i < GameName.Length; i++)
            {
                var helper = new Helper { Value = VkKeyScan(GameName[i]) };
                byte virtualKeyCode = helper.Low;
                gameData.keyMouse.PressKey2((Keys)virtualKeyCode);
                Thread.Sleep(3);
            }
            TypedSearchGames = true;
        }
        //#####

        gameData.keyMouse.MouseClicc(1720, 210); //clic refresh
        gameData.WaitDelay(60);

        long gameNameOffset = (long)gameData.BaseAddress + (long)gameData.offsets["AllGamesOffset"];

        for (int i = 0; i < 40; i++)
        {
            long NameOffet = gameNameOffset + 0x08 + (i * 0x128);
            long CountOffet = gameNameOffset + 0xf8 + (i * 0x128);
            string TestName = gameData.mem.ReadMemString(NameOffet);

            if (TestName != "")
            {
                if (HasGameNameInList(TestName))
                {
                    break;
                }
                AllGamesNames.Add(TestName);
                AllGamesPlayersCount.Add((int)gameData.mem.ReadByteRaw((IntPtr)CountOffet));

                //gameData.method_1("Game: " + TestName + " - Players: " + ((int)gameData.mem.ReadByteRaw((IntPtr)CountOffet)), Color.Red);
            }
            else
            {
                break;
            }
        }

        //GetSelectedGameInfo();
    }

    public bool TriedThisGame(string TestN)
    {
        if (AllGamesTriedNames.Count > 0)
        {
            for (int i = 0; i < AllGamesTriedNames.Count; i++)
            {
                if (AllGamesTriedNames[i] == TestN)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasGameNameInList(string TestN)
    {
        if (AllGamesNames.Count > 0)
        {
            for (int i = 0; i < AllGamesNames.Count; i++)
            {
                if (AllGamesNames[i] == TestN)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void GetSelectedGameInfo()
    {
        if ((int)gameData.offsets["GameSelectedOffset"] <= 64)
        {
            gameData.patternsScan.PatternScan();
        }

        gameData.method_1("------------------------------------------", Color.Black);

        //0x53F or 0x540 size
        long gameOffset = (long)gameData.BaseAddress + (long)gameData.offsets["GameSelectedOffset"];
        long PlayersNamesOffset = gameOffset + 0x138; //then 0x78 offset each others names
        AllPlayersNames = new List<string>();

        SelectedGamePlayerCount = (int)gameData.mem.ReadByteRaw((IntPtr)(gameOffset + 0x108));
        SelectedGameTime = gameData.mem.ReadInt32Raw((IntPtr)(gameOffset + 0xf0));
        gameData.method_1("Player Count: " + SelectedGamePlayerCount, Color.OrangeRed);
        for (int i = 0; i < SelectedGamePlayerCount; i++)
        {
            long NameOffet = PlayersNamesOffset + (i * 0x78);
            string TestName = gameData.mem.ReadMemString(NameOffet);

            if (TestName != "")
            {
                AllPlayersNames.Add(TestName);
                gameData.method_1("Player Name: " + TestName, Color.OrangeRed);
            }
        }

        SelectedGameName = "";
        byte[] buffer = new byte[16];
        gameData.mem.ReadRawMemory(gameOffset + 0x08, ref buffer, 16);

        for (int i2 = 0; i2 < 16; i2++)
        {
            if (buffer[i2] != 0x00)
            {
                SelectedGameName += (char)buffer[i2];
            }
        }

        //LogGameTime();
    }

    public void SelectGame(int ThisIndex, bool EnterGame)
    {
        if (EnterGame)
        {
            if (AllPlayersNames.Count > 0)
            {
                GameOwnerName = AllPlayersNames[0];
            }
            else
            {
                return;
            }
            if (SelectedGamePlayerCount == 8)
            {
                return;
            }
        }

        //gameData.method_1("Selecting game: " + ThisIndex + ", ENTER: " + EnterGame);


        if (ThisIndex >= 0 && ThisIndex <= 13)
        {
            if (!EnterGame) ClicTopRow();
            ClicGameIndex(ThisIndex);
            if (EnterGame)
            {
                Thread.Sleep(10);
                ClicGameIndex(ThisIndex);
                Thread.Sleep(10);
                ClicGameIndex(ThisIndex);
            }
        }
        //dont select any game greater than this index as 'refreshing' the search can make a bug where there no more games listed
        /*if (ThisIndex >= 14 && ThisIndex <= 26)
        {
            if (!EnterGame) ClicMidRow();
            ClicGameIndex(ThisIndex - 13);
            if (EnterGame)
            {
                Thread.Sleep(10);
                ClicGameIndex(ThisIndex);
                Thread.Sleep(10);
                ClicGameIndex(ThisIndex);
            }
        }
        if (ThisIndex >= 27 && ThisIndex <= 39)
        {
            if (!EnterGame) ClicBottomRow();
            ClicGameIndex(ThisIndex - 26);
            if (EnterGame)
            {
                Thread.Sleep(10);
                ClicGameIndex(ThisIndex);
                Thread.Sleep(10);
                ClicGameIndex(ThisIndex);
            }
        }*/

        gameData.WaitDelay(40);
        GetSelectedGameInfo();
    }

    public void ClicGameIndex(int ThisIndex)
    {
        //1345, 260 (+25px each games)
        gameData.keyMouse.MouseClicc(1345, (int)(260 + (ThisIndex * 27.3)));
    }

    public void ClicTopRow()
    {
        gameData.keyMouse.MouseClicc(1510, 270);
        gameData.WaitDelay(10);
    }

    public void ClicMidRow()
    {
        gameData.keyMouse.MouseClicc(1510, 465);
        gameData.WaitDelay(10);
    }

    public void ClicBottomRow()
    {
        gameData.keyMouse.MouseClicc(1510, 605);
        gameData.WaitDelay(10);
    }

    public int ChickenTry = 0;

    public void CheckChickenGameTime()
    {
        if (!CharConfig.RunItemGrabScriptOnly)
        {
            if (!CharConfig.IsRushing)
            {
                if (!AlreadyChickening)
                {
                    TimeSpan Checkkt = (DateTime.Now - gameData.GameStartedTime);

                    gameData.form.method_GameTimeLabel(Checkkt.Minutes.ToString("00") + ":" + Checkkt.Seconds.ToString("00") + ":" + Checkkt.Milliseconds.ToString("0"));
                    if (Checkkt.TotalMinutes > CharConfig.MaxGameTime)
                    {
                        gameData.method_1("Leaving reason: Chicken time", Color.Red);
                        gameData.LeaveGame(false);
                        AlreadyChickening = true;
                        gameData.TotalChickenByTimeCount++;
                    }
                }
                else
                {
                    ChickenTry++;
                    if (ChickenTry > 10)
                    {
                        AlreadyChickening = false;
                        ChickenTry = 0;
                    }
                }
            }
        }
    }

    public string GetTimeNow()
    {
        DateTime ThisTimee = DateTime.Now;
        string HourTime = " (" + DateTime.Now.ToString("hh:mm:ss") + ")";
        return HourTime;

        //string MonthTime = ThisTimee.Day + "-" + ThisTimee.Month + "-" + ThisTimee.Year;
        //return HourTime + " (" + MonthTime + ")";
    }

    public void SetNewGame()
    {
        //gameData.method_1("------------------------------------------", Color.DarkBlue);
        gameData.method_1("New game started: " + GetTimeNow(), Color.DarkBlue);

        gameData.GameStartedTime = DateTime.Now;

        long gameNameOffset = (long)gameData.offsets["gameDataOffset"] + 0x40;
        long gameNameAddress = (long)gameData.BaseAddress + gameNameOffset;
        GameName = gameData.mem.ReadMemString(gameNameAddress);

        method_GameLabel(GameName);

        gameData.method_1("Entered game: " + GameName, Color.DarkBlue);

        AllGamesTriedNames = new List<string>();
    }

    public void method_GameLabel(string string_3)
    {
        //try
        //{
        /*if (gameData.labelGameName.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { method_GameLabel(string_3); };
            gameData.labelGameName.Invoke(safeWrite);
        }
        else
        {
            gameData.labelGameName.Text = string_3;
            Application.DoEvents();
        }*/
        gameData.form.labelGameName.Text = string_3;
        //}
        //catch { }
    }

    public void LogGameTime()
    {
        TimeSpan ThisTimee = DateTime.Now - gameData.GameStartedTime;
        gameData.method_1("Game Time: " + ThisTimee.Minutes.ToString("00") + ":" + ThisTimee.Seconds.ToString("00") + ":" + ThisTimee.Milliseconds.ToString("0"), Color.DarkBlue);
    }

    public Position World2Screen(long playerX, long playerY, long targetx, long targety)
    {
        //; scale = 27
        //double scale = gameData.centerModeScale * gameData.renderScale * 100;
        double scale = 40.8;
        long xdiff = targetx - playerX;
        long ydiff = targety - playerY;

        double ThisScales = (double)gameData.D2Width / 1920.0;
        //double ThisScalesInv = 1920.0 / (double)gameData.D2Width;
        //if (ThisScales != 1) scale = scale / (ThisScales * 2);
        if (ThisScales != 1) scale = scale * ThisScales;

        double angle = 0.785398; //45 deg
        double x = xdiff * Math.Cos(angle) - ydiff * Math.Sin(angle);
        double y = xdiff * Math.Sin(angle) + ydiff * Math.Cos(angle);

        int xS = (int)(gameData.CenterX + (x * scale));
        //int yS = (int) (gameData.CenterY + (y * scale * 0.5) - 10);
        int yS = (int)(gameData.CenterY + ((y * scale * 0.5) - 30));

        return FixMouseYPosition(new Position { X = xS, Y = yS });
    }

    public Position World2ScreenDisplay(long playerX, long playerY, long targetx, long targety)
    {
        //; scale = 27
        //double scale = gameData.centerModeScale * gameData.renderScale * 100;
        double scale = 40.8;
        long xdiff = targetx - playerX;
        long ydiff = targety - playerY;

        double ThisScales = (double)gameData.D2Width / 1920.0;
        //double ThisScalesInv = 1920.0 / (double)gameData.D2Width;
        if (ThisScales != 1) scale = scale / (ThisScales * 2);
        //if (ThisScales != 1) scale = scale * ThisScales;

        double angle = 0.785398; //45 deg
        double x = xdiff * Math.Cos(angle) - ydiff * Math.Sin(angle);
        double y = xdiff * Math.Sin(angle) + ydiff * Math.Cos(angle);

        int xS = (int)(gameData.CenterX + (x * scale));
        //int yS = (int) (gameData.CenterY + (y * scale * 0.5) - 10);
        int yS = (int)(gameData.CenterY + ((y * scale * 0.5) - 30));

        //return FixMouseYPosition(new Position { X = xS, Y = yS });
        return new Position { X = xS, Y = yS };
    }

    public Position FixMouseYPosition(Position itemScreenPos)
    {
        Position itemScreenPos2 = new Position { X = itemScreenPos.X, Y = itemScreenPos.Y };

        //calculate new Y clicking offset, else it will clic on bottom menu items
        if (itemScreenPos2.Y >= (gameData.D2Height + gameData.ScreenYOffset - gameData.ScreenYMenu))
        {
            int DiffX = gameData.CenterX - itemScreenPos2.X;
            itemScreenPos2.X = (int)(itemScreenPos2.X + (DiffX / 6));
            itemScreenPos2.Y = (gameData.D2Height + gameData.ScreenYOffset - gameData.ScreenYMenu);
            //Console.WriteLine("corrected pos from: " + Sx + "," + Sy + " to: " + itemScreenPos2.X + "," + itemScreenPos2.Y);
        }

        return itemScreenPos2;
    }


    public bool IsGameRunning()
    {
        Process[] ProcList = Process.GetProcessesByName("D2R");
        if (ProcList.Length == 0)
        {
            return false;
        }
        return true;
    }

    public bool IsInGame()
    {
        try
        {
            long baseAddress = (long)gameData.BaseAddress + (long)gameData.offsets["unitTable"] - 56;
            byte[] unitTableBuffer = new byte[1];
            gameData.mem.ReadRawMemory(baseAddress, ref unitTableBuffer, 1);

            //Console.WriteLine(unitTableBuffer[0]);
            if (unitTableBuffer[0] == 0x01)
            {
                return true;
            }
        }
        catch { }

        return false;
    }

    public List<Area> GetTerrorZones()
    {
        List<Area> areas = new List<Area>();
        bool SetActArea = false;
        for (int i = 0; i < 7; i++)
        {
            //uint tzArea = gameData.mem.ReadUInt32Raw((IntPtr) ((long)gameData.BaseAddress + (0x299E2D8 + (i * 4))));
            uint tzArea = gameData.mem.ReadUInt32Raw((IntPtr)((long)gameData.BaseAddress + (0x29E9558 + (i * 4))));
            if (tzArea != 0)
            {
                if (!SetActArea)
                {
                    CurrentTZAct = gameData.areaScript.GetActFromArea((Area)tzArea);
                    SetActArea = true;
                }

                if (gameData.areaScript.IsThisTZAreaInSameAct(CurrentTZAct, ((Area)tzArea)))
                {
                    Console.WriteLine("Added TZ: " + ((Area)tzArea));
                    areas.Add((Area)tzArea);
                }
            }
        }

        return areas;
    }
}
