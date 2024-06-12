using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using System.Net.Http;
using static Enums;
using static Form1;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using static MapAreaStruc;

public partial class Form1 : Form
{
    private GameData gameData;
    // MAIN CONFIG
    public string ThisEndPath = Application.StartupPath + @"\Extracted\";
    public string ThisLogPath = Application.StartupPath + @"\Logs\";




    // REQUIRED CONSTS
    const int PROCESS_QUERY_INFORMATION = 0x0400;
    const int MEM_COMMIT = 0x00001000;
    const int PROCESS_VM_OPERATION = 0x0008;
    const int PROCESS_VM_READ = 0x0010;
    const int PROCESS_VM_WRITE = 0x0020;
    const int SYNCHRONIZE = 0x00100000;

    // REQUIRED METHODS
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

    [DllImport("user32.dll")]
    private static extern int FindWindow(string ClassName, string WindowName);

    [DllImport("user32.dll")]
    private static extern int GetWindowRect(int hwnd, out Rectangle rect);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(int hwnd, out Rectangle lpRect);

    [DllImport("user32.dll")]
    static extern bool ClientToScreen(int hWnd, out Point lpPoint);

    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
    public enum DeviceCap
    {
        VERTRES = 10,
        DESKTOPVERTRES = 117,
    }

    /*public void TestMethod()
    {
        Console.WriteLine("Executing MyMethod from script #3...");
    }*/

    // REQUIRED STRUCTS
    public struct MEMORY_BASIC_INFORMATION
    {
        public int BaseAddress;
        public int AllocationBase;
        public int AllocationProtect;
        public int RegionSize;
        public int State;
        public int Protect;
        public int lType;
    }

    public struct SYSTEM_INFO
    {
        public ushort processorArchitecture;
        ushort reserved;
        public uint pageSize;
        public IntPtr minimumApplicationAddress;
        public IntPtr maximumApplicationAddress;
        public IntPtr activeProcessorMask;
        public uint numberOfProcessors;
        public uint processorType;
        public uint allocationGranularity;
        public ushort processorLevel;
        public ushort processorRevision;
    }

    public Form1()
    {
        InitializeComponent();
        gameData = GameData.Instance;
        this.Text = "D2R - BMBot (" + gameData.BotVersion + ")";
        labelGames.Text = "";//CurrentGameNumber.ToString();
        SetGameStatus("STOPPED");

        richTextBox1.HideSelection = false;//Hide selection so that AppendText will auto scroll to the end
        richTextBox2.HideSelection = false;//Hide selection so that AppendText will auto scroll to the end
                                           //richTextBox2.Visible = false;

        //ModifyMonsterList();

        //overlay graphics
        if (gameData.overlayForm == null || gameData.overlayForm.IsDisposed)
        {
            gameData.overlayForm = new OverlayForm(this);
        }
        gameData.overlayForm.Show();

        comboBoxItemsCategory.SelectedIndex = 0;

        LabelChickenCount.Text = gameData.TotalChickenCount.ToString();
        LabelDeadCount.Text = gameData.TotalDeadCount.ToString();

        SetDebugMenu();

        labelGameName.Text = "";
        labelGameTime.Text = "";

        gameData.LoopTimer = new System.Timers.Timer(1);
        gameData.LoopTimer.Elapsed += new ElapsedEventHandler(timer_Elapsed);

        gameData.ScreenX = Screen.PrimaryScreen.Bounds.Width;
        gameData.ScreenY = Screen.PrimaryScreen.Bounds.Height;

        
        if (gameData.D2_LOD_113C_Path == "" || !Directory.Exists(gameData.D2_LOD_113C_Path))
        {
            bool LoadedPreSettings = false;
            if (CharConfig.PlayerCharName == "CHARNAMEHERE")
            {
                DialogResult result = MessageBox.Show("Do you want to Import all Settings Files from a previous Version?", "ERROR", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    LoadedPreSettings = true;
                    folderBrowserDialog1.Description = "Select the Settings folder where your previous bot version is located";
                    DialogResult result2 = folderBrowserDialog1.ShowDialog();
                    if (result2 == DialogResult.OK)
                    {
                        //gameData.D2_LOD_113C_Path = folderBrowserDialog1.SelectedPath;

                        //load char settings
                        gameData.settingsLoader.ReloadCharSettingsFromThisFile(folderBrowserDialog1.SelectedPath + @"\Char\SorceressBlizzard.txt");
                        gameData.settingsLoader.ReloadCharSettingsFromThisFile(folderBrowserDialog1.SelectedPath + @"\Char\PaladinHammer.txt");
                        gameData.settingsLoader.LoadThisFileSettings(folderBrowserDialog1.SelectedPath + @"\ItemsSettings.txt");
                        gameData.settingsLoader.LoadThisFileSettings(folderBrowserDialog1.SelectedPath + @"\CubingRecipes.txt");
                        gameData.settingsLoader.LoadThisFileSettings(folderBrowserDialog1.SelectedPath + @"\BotSettings.txt");
                        gameData.settingsLoader.LoadThisFileSettings(folderBrowserDialog1.SelectedPath + @"\CharSettings.txt");

                        //Reload Sorc Settings
                        if (CharConfig.RunningOnChar != "PaladinHammer") gameData.settingsLoader.ReloadCharSettingsFromThisFile(folderBrowserDialog1.SelectedPath + @"\Char\SorceressBlizzard.txt");

                        //Reload Settings (D2 Path, RunNumber)
                        gameData.settingsLoader.LoadThisFileSettings(folderBrowserDialog1.SelectedPath + @"\Settings.txt");

                        Application.DoEvents();
                    }
                    else
                    {
                        LoadedPreSettings = false;
                    }
                }
            }

            if (!LoadedPreSettings)
            {
                DialogResult result = MessageBox.Show("Diablo2 LOD 1.13C Path is not set correctly!" + Environment.NewLine + Environment.NewLine + "Do you want to select the Path where it's located?", "ERROR", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    folderBrowserDialog1.Description = "Select the folder where D2 LOD 1.13C is located";
                    DialogResult result2 = folderBrowserDialog1.ShowDialog();
                    if (result2 == DialogResult.OK)
                    {
                        gameData.D2_LOD_113C_Path = folderBrowserDialog1.SelectedPath;
                    }
                    else
                    {
                        method_1("ERROR: Diablo2 LOD 1.13C Path NOT SET CORRECTLY!", Color.Red);
                        method_1("Clic on the settings button and set the path where Diablo2 1.13c (the old legacy diablo2) is located!", Color.Red);
                        method_1("Make sure the path don't contain any whitespace!", Color.Red);
                        buttonD2LOD.Visible = true;
                    }
                }
                else
                {
                    method_1("ERROR: Diablo2 LOD 1.13C Path NOT SET CORRECTLY!", Color.Red);
                    method_1("Clic on the settings button and set the path where Diablo2 1.13c (the old legacy diablo2) is located!", Color.Red);
                    method_1("Make sure the path don't contain any whitespace!", Color.Red);
                    buttonD2LOD.Visible = true;
                }
            }
        }

        gameData.keyMouse.proc = gameData.keyMouse.HookCallback;
        gameData.keyMouse.hookID = gameData.keyMouse.SetHook(gameData.keyMouse.proc);
        //itemsAlert.SetParams();

        dataGridView1.Rows.Add("Processing Time", "Unknown");
        //dataGridView1.Rows.Add("---Player---", "---------");
        dataGridView1.Rows.Add("Pointer", "Unknown");
        dataGridView1.Rows.Add("Cords", "Unknown"); //
        dataGridView1.Rows.Add("LeechCords", "Unknown"); //
        dataGridView1.Rows.Add("Life", "Unknown"); //
        dataGridView1.Rows.Add("Mana", "Unknown"); //
        dataGridView1.Rows.Add("Map Level", "Unknown"); //
                                                        //dataGridView1.Rows.Add("Room Exit", "Unknown"); //
                                                        //dataGridView1.Rows.Add("Difficulty", "Unknown"); //
        dataGridView1.Rows.Add("Merc", "Unknown"); //
                                                   //dataGridView1.Rows.Add("Seed", "Unknown"); //
                                                   //dataGridView1.Rows.Add("Belt qty", "Unknown"); //
                                                   //dataGridView1.Rows.Add("---Items---", "---------");
        dataGridView1.Rows.Add("Scanned", "Unknown");
        dataGridView1.Rows.Add("On ground", "Unknown");
        dataGridView1.Rows.Add("Equipped", "Unknown");
        dataGridView1.Rows.Add("InInventory", "Unknown");
        dataGridView1.Rows.Add("InBelt", "Unknown");
        //dataGridView1.Rows.Add("---Menu---", "---------");
        dataGridView1.Rows.Add("Left Open", "Unknown");
        dataGridView1.Rows.Add("Right Open", "Unknown");
        dataGridView1.Rows.Add("Full Open", "Unknown");

        CheckForUpdates();

        comboBoxCollisionArea.Items.Clear();
        for (int i = 0; i < 136; i++) comboBoxCollisionArea.Items.Add(((Enums.Area) i + 1).ToString());

        gameData.LoadingBot = false;
    }

    public void CheckForUpdates()
    {
        string url = "https://raw.githubusercontent.com/bouletmarc/D2R-BMBot/main/Form1.cs";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;

                if (responseBody.Contains("public string BotVersion = "))
                {
                    string ThisOnlineVString = responseBody.Substring(responseBody.IndexOf('=') + 4, 5);
                    ThisOnlineVString = ThisOnlineVString.Replace("\"", "").Replace(";", "");
                    double ThisVersionOnline = double.Parse(ThisOnlineVString, System.Globalization.CultureInfo.InvariantCulture);
                    double ThisVersionCurrent = double.Parse(gameData.BotVersion.Substring(1).Replace("\"", ""), System.Globalization.CultureInfo.InvariantCulture);

                    if (ThisVersionOnline > ThisVersionCurrent)
                    {
                        method_1("New update V" + ThisVersionOnline.ToString().Replace(",", ".") + " available on github!", Color.Red);
                        buttonUpdate.Visible = true;
                    }
                    else if (ThisVersionOnline == ThisVersionCurrent)
                    {
                        method_1("BMBot is updated!", Color.DarkGreen);
                    }
                    else if (ThisVersionOnline < ThisVersionCurrent)
                    {
                        method_1("BMBot is updated (Development Version)!", Color.DarkGreen);
                    }
                }
                else
                {
                    method_1("Couldn't check for updates!", Color.Red);
                }
            }
            catch (HttpRequestException e)
            {
                method_1("Couldn't check for updates! Error:", Color.Red);
                method_1(e.Message, Color.Red);
            }
        }
    }

    public void LeaveGame(bool BotCompletlyDone)
    {
        if (CharConfig.RunItemGrabScriptOnly) return;

        SetGameStatus("LEAVING");

        if (BotCompletlyDone && !gameData.SetGameDone)
        {
            gameData.CurrentGameNumberFullyDone++;
            gameData.SetGameDone = true;
        }

        if (gameData.uiScan.OpenUIMenu("quitMenu"))
        {
            gameData.keyMouse.MouseClicc(960, 480);
            WaitDelay(5);
            gameData.keyMouse.MouseClicc(960, 480);
            WaitDelay(200);
        }
    }

    void RemovePastDump()
    {
        string[] FileList = Directory.GetFiles(ThisEndPath, "Dump*");
        if (FileList.Length > 0)
        {
            for (int i = 0; i < FileList.Length; i++)
            {
                File.Delete(FileList[i]);
            }
        }
    }

    public void method_1(string string_3, Color ThisColor, bool LogTime = true)
    {
        //try
        //{
        if (richTextBox1.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { method_1(string_3, ThisColor); };
            richTextBox1.Invoke(safeWrite);
        }
        else
        {
            if (LogTime) string_3 = string_3 + " " + gameData.gameStruc.GetTimeNow();
            Console.WriteLine(string_3);
            if (ThisColor == Color.OrangeRed && !CharConfig.LogNotUsefulErrors) return;
            richTextBox1.SelectionColor = ThisColor;
            richTextBox1.AppendText(string_3 + Environment.NewLine);
            gameData.overlayForm.AddLogs(string_3, ThisColor);

            if (ThisColor == Color.Red || ThisColor == Color.Orange || ThisColor == Color.DarkOrange || ThisColor == Color.OrangeRed) AppendTextErrorLogs(string_3, ThisColor);
            if (ThisColor == Color.DarkBlue) AppendTextGameLogs(string_3, ThisColor);
            Application.DoEvents();
        }
        //}
        //catch { }
    }

    public void method_1_Items(string string_3, Color ThisColor)
    {
        //try
        //{
        if (richTextBox2.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { method_1_Items(string_3, ThisColor); };
            richTextBox2.Invoke(safeWrite);
        }
        else
        {
            string LogThis = string_3 + " in " + gameData.townStruc.getAreaName((int)gameData.playerScan.levelNo) + " " + gameData.gameStruc.GetTimeNow();
            richTextBox2.SelectionColor = ThisColor;
            richTextBox2.AppendText(LogThis + Environment.NewLine);
            method_1(LogThis, ThisColor, false);

            if (!Directory.Exists(ThisLogPath)) Directory.CreateDirectory(ThisLogPath);
            if (!File.Exists(ThisLogPath + "ItemsLogs.txt")) File.Create(ThisLogPath + "ItemsLogs.txt").Dispose();
            File.AppendAllText(ThisLogPath + "ItemsLogs.txt", LogThis + Environment.NewLine);
            Application.DoEvents();
        }
        //}
        //catch { }
    }

    public void method_1_SoldItems(string string_3, Color ThisColor)
    {
        //try
        //{
        if (richTextBoSoldLogs.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { method_1_SoldItems(string_3, ThisColor); };
            richTextBoSoldLogs.Invoke(safeWrite);
        }
        else
        {
            string LogThis = string_3 + " " + gameData.gameStruc.GetTimeNow();
            richTextBoSoldLogs.SelectionColor = ThisColor;
            richTextBoSoldLogs.AppendText(LogThis + Environment.NewLine);
            //method_1(LogThis, ThisColor, false);

            if (!Directory.Exists(ThisLogPath)) Directory.CreateDirectory(ThisLogPath);
            if (!File.Exists(ThisLogPath + "ItemsSoldLogs.txt")) File.Create(ThisLogPath + "ItemsSoldLogs.txt").Dispose();
            File.AppendAllText(ThisLogPath + "ItemsSoldLogs.txt", LogThis + Environment.NewLine);
            Application.DoEvents();
        }
        //}
        //catch { }
    }

    public string PreviousStatus = "IDLE";
    public string CurrentStatus = "IDLE";
    

    public void Grid_SetInfos(string RowName, string ThisInfos)
    {
        //try
        //{
        if (dataGridView1.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { Grid_SetInfos(RowName, ThisInfos); };
            dataGridView1.Invoke(safeWrite);
        }
        else
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Value.ToString() == RowName)
                {
                    dataGridView1.Rows[i].Cells[1].Value = ThisInfos;
                    return;
                }
            }
        }
        //}
        //catch { }
    }

    public void SetGamesText()
    {
        /*if (labelGames.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { SetGamesText(); };
            labelGames.Invoke(safeWrite);
        }
        else
        {
            labelGames.Text = CurrentGameNumberSinceStart.ToString() + " entered. " + CurrentGameNumberFullyDone.ToString() + " fully done";
        }*/
        labelGames.Text = gameData.CurrentGameNumberSinceStart.ToString() + " entered. " + gameData.CurrentGameNumberFullyDone.ToString() + " fully done";
    }

    private float getScalingFactor()
    {
        Graphics g = Graphics.FromHwnd(IntPtr.Zero);
        IntPtr desktop = g.GetHdc();
        int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
        int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

        g.ReleaseHdc(desktop);

        float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

        return ScreenScalingFactor; // 1.25 = 125%
    }

    public void Startt()
    {
        try
        {
            SYSTEM_INFO sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);

            if (!Directory.Exists(ThisEndPath)) Directory.CreateDirectory(ThisEndPath);
            RemovePastDump();

            method_1("------------------------------------------", Color.DarkBlue);
            method_1("Extracting Infos...", Color.Black);

            Process[] ProcList = Process.GetProcessesByName("D2R");
            if (!gameData.gameStruc.IsGameRunning())
            {
                method_1("D2R is not running!", Color.Red);
                return;
            }
            else
            {
                SetGameStatus("LOADING");
                method_1("D2R is running...", Color.DarkGreen);

                gameData.hWnd = FindWindow(null, "Diablo II: Resurrected");
                //GetWindowRect(hWnd, out D2Rect);
                GetClientRect(gameData.hWnd, out gameData.D2Rect);
                Point thiP = new Point();
                ClientToScreen(gameData.hWnd, out thiP);

                gameData.D2Width = gameData.D2Rect.Width;
                gameData.D2Height = gameData.D2Rect.Height;
                gameData.ScreenXOffset = thiP.X;
                gameData.ScreenYOffset = thiP.Y;

                gameData.CenterX = (gameData.D2Width / 2) + gameData.ScreenXOffset;
                gameData.CenterY = (gameData.D2Height / 2) + gameData.ScreenYOffset;

                if (IsWindowOnSecondaryMonitor(gameData.D2Rect))
                {
                    int FirstMonitorScreenX = Screen.PrimaryScreen.Bounds.Width;
                    int FirstMonitorScreenY = Screen.PrimaryScreen.Bounds.Height;
                    gameData.ScreenX = Screen.AllScreens[1].Bounds.Width;
                    gameData.ScreenY = Screen.AllScreens[1].Bounds.Height;
                    gameData.ScreenXOffset += FirstMonitorScreenX;
                    gameData.ScreenYOffset += FirstMonitorScreenY;

                    gameData.CenterX = (gameData.D2Width / 2) + gameData.ScreenXOffset;
                    gameData.CenterY = (gameData.D2Height / 2) + gameData.ScreenYOffset;
                    //Console.WriteLine("is on the secondary monitor.");
                }

                method_1("Screen Specs:", Color.DarkBlue);
                method_1("-> Screen size: " + gameData.ScreenX + ", " + gameData.ScreenY, Color.DarkBlue);
                method_1("-> D2R rect Size: " + gameData.D2Width + ", " + gameData.D2Height, Color.DarkBlue);
                method_1("-> D2R rect offset: " + gameData.ScreenXOffset + ", " + gameData.ScreenYOffset, Color.DarkBlue);
                method_1("-> D2R Center Position: " + gameData.CenterX + ", " + gameData.CenterY, Color.DarkBlue);
                if (IsWindowOnSecondaryMonitor(gameData.D2Rect)) method_1("-> **D2R On Secondary Monitor**!", Color.DarkBlue);

                double screenRatio = (double)gameData.D2Width / gameData.D2Height;

                if (Math.Abs(screenRatio - (16.0 / 9.0)) > 0.01)
                {
                    method_1("D2R rect Size ratio is not 16:9!", Color.Red);
                }
                if (gameData.CenterX >= ((gameData.ScreenX / 2) + 15)
                    || gameData.CenterX <= ((gameData.ScreenX / 2) - 15)
                    || gameData.CenterY >= ((gameData.ScreenY / 2) + 15)
                    || gameData.CenterY <= ((gameData.ScreenY / 2) - 15))
                {
                    method_1("D2R rect Position is not in the center of screen, might have some issues!", Color.OrangeRed);
                }

                if (getScalingFactor() != 1f)
                {
                    method_1("Windows scale factor is not 100%, might have some issues!", Color.OrangeRed);
                }

                if (gameData.ScreenX > 1920)
                {
                    method_1("Screen Resolution is bigger than 1920x1080, might have some issues!", Color.OrangeRed);
                }

                if (gameData.D2_LOD_113C_Path.Contains(" "))
                {
                    method_1("The Path to Diablo2 LOD containt a whitespace, make sure the path doesn't containt any spaces in it!", Color.Red);
                }

                gameData.overlayForm.ScaleScreenSize = (float)gameData.D2Width / 1920f;
                gameData.overlayForm.ScaleScreenSizeInverted = 1920f / (float)gameData.D2Width;
                gameData.overlayForm.ResetScaleForDisplay();

                gameData.process = Process.GetProcessesByName("D2R")[0];
                gameData.processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, gameData.process.Id);
                //processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE | SYNCHRONIZE, false, process.Id);

                foreach (ProcessModule module in gameData.process.Modules)
                {
                    if (module.ModuleName == "D2R.exe")
                    {
                        gameData.BaseAddress = module.BaseAddress;
                        method_1("D2R module BaseAddress: 0x" + gameData.BaseAddress.ToString("X"), Color.Black);
                    }
                    //Console.WriteLine("Module: " + module.FileName + ", Name2: " + module.ModuleName + ", BaseAddress: " + module.BaseAddress);
                }

                int bytesRead = 0;
                gameData.buffer = new byte[0x3FFFFFF];
                gameData.mem.ReadMemory(gameData.BaseAddress, ref gameData.buffer, gameData.buffer.Length, ref bytesRead);
                if (bytesRead > 0)
                {
                    string SavePathh = ThisEndPath + "DumpHex1";
                    File.Create(SavePathh).Dispose();
                    File.WriteAllBytes(SavePathh, gameData.buffer);
                }

                gameData.patternsScan.PatternScan();

                gameData.buffer = null;
                gameData.buffer = new byte[0];

                method_1("Starting loop timer!", Color.Black);
                method_1("------------------------------------------", Color.DarkBlue);
                method_1("Bot started for: " + CharConfig.RunningOnChar + " - " + CharConfig.PlayerCharName, Color.DarkBlue);
                SetGameStatus("IDLE");
                gameData.LoopTimer.Start();
            }
        }
        catch (Exception message)
        {
            method_1("Error:" + Environment.NewLine + message, Color.Red);
            return;

        }
    }
    private static bool IsWindowOnSecondaryMonitor(Rectangle windowRect)
    {
        foreach (Screen screen in Screen.AllScreens)
        {
            if (screen.WorkingArea.Contains(windowRect))
            {
                // Check if the window is on a secondary monitor
                if (screen != Screen.PrimaryScreen)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetNewGame()
    {
        if (gameData.BotResuming)
        {
            gameData.BotResuming = false;
            return;
        }

        SetGameStatus("NEW GAME STARTED");

        gameData.inventoryStruc.HUDItems_tpscrolls_locx = -1;
        gameData.inventoryStruc.HUDItems_tpscrolls_locy = -1;

        gameData.PublicGame = (CharConfig.GamePass == "");
        if (!gameData.PublicGame && CharConfig.IsRushing) gameData.PublicGame = true;
        if (!gameData.PublicGame && !CharConfig.RunGameMakerScript) gameData.PublicGame = true;
        if (gameData.PublicGame) gameData.keyMouse.ProcessingDelay = 5;
        else gameData.keyMouse.ProcessingDelay = 2;
        gameData.gameStruc.AlreadyChickening = false;
        gameData.gameStruc.TypedSearchGames = false;
        gameData.patternsScan.StartIndexItem_V2 = long.MaxValue;     //UNITS SCAN V2
        gameData.patternsScan.StartIndexItemLast_V2 = long.MaxValue; //UNITS SCAN V2
        gameData.townStruc.TriedToShopCount = 0;
        gameData.townStruc.TriedToShopCount2 = 0;
        gameData.townStruc.TriedToMercCount = 0;
        gameData.FoundPlayerPointerTryCount = 0;
        gameData.FoundPlayerPointerRetryTimes = 0;
        gameData.TriedToCreateNewGameCount = 0;
        gameData.townStruc.Towning = true;
        gameData.townStruc.IsInTown = true;
        gameData.townStruc.ForcedTowning = false;
        gameData.townStruc.FastTowning = false;
        gameData.playerScan.GetPositions();
        gameData.townStruc.LoadFirstTownAct();
        gameData.overlayForm.ScanningOverlayItems = true;
        gameData.itemsStruc.GetItems(false);
        gameData.playerScan.PlayerHP = gameData.playerScan.PlayerMaxHP;
        if (gameData.playerScan.PlayerHP == 0) gameData.playerScan.PlayerHP = 100;
        gameData.playerScan.SetMaxHPAndMana();
        gameData.shop.FirstShopping = true;
        gameData.itemsStruc.dwOwnerId_Shared1 = 0;
        gameData.itemsStruc.dwOwnerId_Shared2 = 0;
        gameData.itemsStruc.dwOwnerId_Shared3 = 0;
        gameData.potions.CanUseSkillForRegen = true;
        gameData.ResetRunVars();

        gameData.battle.DoingBattle = false;
        gameData.battle.ClearingArea = false;
        gameData.battle.MoveTryCount = 0;

        gameData.townStruc.IgnoredTPList.Clear();
        gameData.townStruc.IgnoredWPList.Clear();
        gameData.baalLeech.IgnoredTPList.Clear();
        gameData.townStruc.FirstTown = true;
        gameData.ForceSwitch2ndPlayer = false;
        gameData.playerScan.PlayerGoldInventory = 0;
        gameData.SetGameDone = false;
        gameData.BadPlayerPointerFound = false;
        gameData.beltStruc.ForceMANAPotionQty = 0;
        gameData.beltStruc.ForceHPPotionQty = 0;
        SetGamesText();
        if (CharConfig.RunGameMakerScript && !gameData.BotJustStarted) gameData.CurrentGameNumber++;
        gameData.CurrentGameNumberSinceStart++;
        gameData.settingsLoader.SaveOthersSettings();
        gameData.itemsStruc.BadItemsOnCursorIDList = new List<long>();
        gameData.itemsStruc.BadItemsOnGroundPointerList = new Dictionary<string, bool>();
        gameData.itemsStruc.AvoidItemsOnGroundPointerList = new Dictionary<string, bool>();
        gameData.SetDeadCount = false;
        gameData.gameStruc.ChickenTry = 0;
        gameData.mercStruc.MercOwnerID = 0;
        gameData.battle.TimeSinceLastCast = DateTime.MaxValue;
        //itemsAlert.CheckItemNames();
        gameData.itemsAlert.RemoveNotPickingItems();

        //##############################
        gameData.mapAreaStruc.ScanMapStruc();


        //pointer fix???
        PointerError = false;
        gameData.patternsScan.GetUnitsScannedCount(1);
        if (gameData.patternsScan.ScannedItemsCount < 100)
        {
            method_1("Detected Pointers Error, Restarting Bot...", Color.Red);
            gameData.patternsScan.ResetV1Scanning();
            gameData.HasPointers = false;
            PointerError = true;

            gameData.patternsScan.AllItemsPointers = new Dictionary<long, bool>();
            gameData.patternsScan.AllObjectsPointers = new Dictionary<long, bool>(); //->refer to all gameobjects
            gameData.patternsScan.AllPlayersPointers = new Dictionary<long, bool>();
            gameData.patternsScan.AllNPCPointers = new Dictionary<long, bool>();

            gameData.patternsScan.AllPossiblePointers = new List<long>();

            gameData.patternsScan.AllPossibleItemsPointers = new List<long>();
            gameData.patternsScan.AllPossiblePlayerPointers = new List<long>();
            gameData.patternsScan.AllPossibleObjectsPointers = new List<long>();
            gameData.patternsScan.AllPossibleNPCPointers = new List<long>();

            gameData.patternsScan.AllScannedPointers = new Dictionary<long, bool>();
            gameData.patternsScan.StartIndexItem = long.MaxValue;
            gameData.patternsScan.StartIndexItemLast = long.MaxValue;
            //patternsScan.PatternScan();
        }
    }

    public bool PointerError = false;

    public void IncreaseDeadCount()
    {
        if (!gameData.SetDeadCount)
        {
            gameData.TotalDeadCount++;
            gameData.form.LabelDeadCount.Text = gameData.TotalDeadCount.ToString();
            gameData.SetDeadCount = true;
        }
    }

    void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        gameData.LoopTimer.Stop();
        gameData.CheckTime = DateTime.Now;

        //gameData.mobsStruc.GetMobs("getBossName", "Andariel", true, 200, new List<long>());
        //overlayForm.DoSomething();
        //if (Running) LoopTimer.Start();
        //return;

        if (gameData.gameStruc.IsGameRunning())
        {
            if (gameData.stash.StashFull)
            {
                StopBot();
                return;
            }

            bool isInGame = gameData.gameStruc.IsInGame();
            if (isInGame)
            {
                if (!gameData.HasPointers)
                {
                    gameData.PrintedGameTime = false;
                    gameData.playerScan.scanForPlayer(true);
                    if (gameData.playerScan.FoundPlayer)
                    {
                        gameData.gameStruc.SetNewGame();
                        SetNewGame();
                        if (PointerError)
                        {
                            Startt();
                            /*SetProcessingTime();
                            if (Running) LoopTimer.Start();*/
                            return;
                        }
                        if (!CharConfig.IsRushing) WaitDelay(CharConfig.MaxDelayNewGame); //wait here because 'loading' menu is not correct
                        if (CharConfig.IsRushing) gameData.playerScan.ScanForLeecher();
                        //if (patternsScan.StartIndexItem_V2 == long.MaxValue) patternsScan.DetectFirstUnitPointer(); //UNITS SCAN V2
                        gameData.townStruc.GetCorpse();
                        gameData.itemsStruc.GetBadItemsOnCursor();
                        gameData.inventoryStruc.CheckInventorySpecialUniqueItems();
                        gameData.HasPointers = true;
                    }
                    else
                    {
                        //didn't found player pointer
                        gameData.playerScan.scanForPlayer(false);
                        if (gameData.playerScan.FoundPlayer)
                        {
                            gameData.gameStruc.SetNewGame();
                            SetNewGame();
                            if (PointerError)
                            {
                                Startt();
                                /*SetProcessingTime();
                                if (Running) LoopTimer.Start();*/
                                return;
                            }
                            if (!CharConfig.IsRushing) WaitDelay(CharConfig.MaxDelayNewGame); //wait here because 'loading' menu is not correct
                            if (CharConfig.IsRushing) gameData.playerScan.ScanForLeecher();
                            //if (patternsScan.StartIndexItem_V2 == long.MaxValue) patternsScan.DetectFirstUnitPointer(); //UNITS SCAN V2
                            gameData.townStruc.GetCorpse();
                            gameData.itemsStruc.GetBadItemsOnCursor();
                            gameData.inventoryStruc.CheckInventorySpecialUniqueItems();
                            gameData.HasPointers = true;
                        }
                        else
                        {
                            gameData.FoundPlayerPointerTryCount++;

                            if (gameData.FoundPlayerPointerTryCount >= 300)
                            {
                                method_1("Leaving Player pointer not found!", Color.Red);
                                gameData.potions.ForceLeave = true;
                                gameData.BadPlayerPointerFound = true;

                                if (gameData.FoundPlayerPointerRetryTimes > 0) gameData.ForceSwitch2ndPlayer = true;
                                gameData.FoundPlayerPointerRetryTimes++;

                                gameData.baalLeech.SearchSameGamesAsLastOne = false;
                                gameData.chaosLeech.SearchSameGamesAsLastOne = false;
                                gameData.LeaveGame(false);

                                SetProcessingTime();
                                if (gameData.Running) gameData.LoopTimer.Start();
                                return;
                            }
                        }
                    }
                }
                if (gameData.HasPointers)
                {
                    gameData.playerScan.GetPositions();
                    gameData.uiScan.readUI();
                    if (!gameData.uiScan.loading)
                    {
                        //mobsStruc.GetMobs("", "", true, 200, new List<long>());
                        //mercStruc.GetMercInfos();
                        //battle.SetSkills();
                        //battle.CastSkills();
                        //itemsStruc.GetItems(false);
                        //playerScan.ScanForLeecher();
                        //itemsStruc.GetItems(true);
                        //overlayForm.SetAllOverlay();
                        //gameStruc.GetTerrorZones();
                        //Running = false;
                        //if (Running) LoopTimer.Start();
                        //keyMouse.SetForm1(gameData);
                        //itemsViewer.ItemViewerDebug();
                        //return;

                        if (CharConfig.RunMapHackOnly)
                        {
                            gameData.itemsStruc.GetItems(false);
                            SetProcessingTime();
                            if (!gameData.Running && gameData.RestartingBot)
                            {
                                StartBot();
                                gameData.RestartingBot = false;
                            }

                            if (gameData.Running) gameData.LoopTimer.Start();
                            if (!gameData.Running) SetBotStopped();
                            return;
                        }

                        if (!gameData.itemsStruc.GetItems(true))
                        {
                            if (CharConfig.RunMapHackPickitOnly)
                            {
                                gameData.itemsStruc.GetItems(false);
                                SetProcessingTime();
                                if (!gameData.Running && gameData.RestartingBot)
                                {
                                    StartBot();
                                    gameData.RestartingBot = false;
                                }

                                if (gameData.Running) gameData.LoopTimer.Start();
                                if (!gameData.Running) SetBotStopped();
                                return;
                            }
                            else if (!CharConfig.RunItemGrabScriptOnly)
                            {
                                if (gameData.townStruc.Towning || (gameData.townStruc.GetInTown() && !gameData.townStruc.TownScriptDone))
                                {
                                    gameData.itemsStruc.GetItems(false);
                                    gameData.townStruc.RunTownScript();
                                }
                                else
                                {
                                    if (gameData.townStruc.TownScriptDone)
                                    {
                                        gameData.itemsStruc.TriesToPickItemCount = 0;
                                        gameData.townStruc.FastTowning = true;
                                        gameData.townStruc.ForcedTowning = false;
                                        //townStruc.FastTowning = false;
                                        gameData.townStruc.UseLastTP = false;
                                        gameData.townStruc.TPSpawned = false;
                                    }
                                    if (!gameData.townStruc.GetInTown())
                                    {
                                        gameData.stash.DeposingGoldCount = 0;
                                        gameData.townStruc.TownScriptDone = false;
                                    }

                                    if (!gameData.townStruc.GetInTown() && gameData.itemsStruc.ItemsEquiped <= 2)
                                    {
                                        method_1("Going to town, body not grabbed!", Color.OrangeRed);
                                        gameData.townStruc.GoToTown();
                                    }
                                    else
                                    {
                                        if (!CharConfig.IsRushing)
                                        {
                                            if (gameData.battle.ClearingArea || gameData.battle.DoingBattle)
                                            {
                                                if (gameData.battle.DoingBattle) gameData.battle.RunBattleScriptOnLastMob(gameData.battle.IgnoredMobsPointer);
                                                else gameData.battle.RunBattleScript();
                                            }
                                            else
                                            {
                                                if (CharConfig.RunWPTaker && gameData.wpTaker.ScriptDone)
                                                {
                                                    gameData.wpTaker.RunScript();
                                                }
                                                else
                                                {
                                                    if (CharConfig.RunShopBotScript && !gameData.shopBot.ScriptDone)
                                                    {
                                                        gameData.shopBot.RunScript();
                                                    }
                                                    else
                                                    {
                                                        if (CharConfig.RunMausoleumScript && !gameData.mausoleum.ScriptDone)
                                                        {
                                                            gameData.mausoleum.RunScript();
                                                        }
                                                        else
                                                        {
                                                            if (CharConfig.RunCryptScript && !gameData.crypt.ScriptDone)
                                                            {
                                                                gameData.crypt.RunScript();
                                                            }
                                                            else
                                                            {
                                                                if (CharConfig.RunPitScript && !gameData.pit.ScriptDone)
                                                                {
                                                                    gameData.pit.RunScript();
                                                                }
                                                                else
                                                                {
                                                                    if (CharConfig.RunCowsScript && !gameData.cows.ScriptDone)
                                                                    {
                                                                        gameData.cows.RunScript();
                                                                    }
                                                                    else
                                                                    {
                                                                        if (CharConfig.RunCountessScript && !gameData.countess.ScriptDone)
                                                                        {
                                                                            gameData.countess.RunScript();
                                                                        }
                                                                        else
                                                                        {
                                                                            if (CharConfig.RunAndarielScript && !gameData.andariel.ScriptDone)
                                                                            {
                                                                                gameData.andariel.RunScript();
                                                                            }
                                                                            else
                                                                            {
                                                                                if (CharConfig.RunSummonerScript && !gameData.summoner.ScriptDone)
                                                                                {
                                                                                    gameData.summoner.RunScript();
                                                                                }
                                                                                else
                                                                                {
                                                                                    if (CharConfig.RunDurielScript && !gameData.duriel.ScriptDone)
                                                                                    {
                                                                                        gameData.duriel.RunScript();
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        if (CharConfig.RunArachnidScript && !gameData.arachnidLair.ScriptDone)
                                                                                        {
                                                                                            gameData.arachnidLair.RunScript();
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            if (CharConfig.RunLowerKurastScript && !gameData.lowerKurast.ScriptDone)
                                                                                            {
                                                                                                gameData.lowerKurast.RunScript();
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                if (CharConfig.RunA3SewersScript && !gameData.act3Sewers.ScriptDone)
                                                                                                {
                                                                                                    gameData.act3Sewers.RunScript();
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    if (CharConfig.RunUpperKurastScript && !gameData.upperKurast.ScriptDone)
                                                                                                    {
                                                                                                        gameData.upperKurast.RunScript();
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        if (CharConfig.RunTravincalScript && !gameData.travincal.ScriptDone)
                                                                                                        {
                                                                                                            gameData.travincal.RunScript();
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            if (CharConfig.RunMephistoScript && !gameData.mephisto.ScriptDone)
                                                                                                            {
                                                                                                                gameData.mephisto.RunScript();
                                                                                                            }
                                                                                                            else
                                                                                                            {
                                                                                                                if (CharConfig.RunChaosScript && !gameData.chaos.ScriptDone)
                                                                                                                {
                                                                                                                    gameData.chaos.RunScript();
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                    if (CharConfig.RunChaosLeechScript && !gameData.chaosLeech.ScriptDone)
                                                                                                                    {
                                                                                                                        gameData.chaosLeech.RunScript();
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                        if (CharConfig.RunEldritchScript && !gameData.eldritch.ScriptDone)
                                                                                                                        {
                                                                                                                            gameData.eldritch.RunScript();
                                                                                                                        }
                                                                                                                        else
                                                                                                                        {
                                                                                                                            if (CharConfig.RunShenkScript && !gameData.shenk.ScriptDone)
                                                                                                                            {
                                                                                                                                gameData.shenk.RunScript();
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                if (CharConfig.RunFrozensteinScript && !gameData.frozenstein.ScriptDone)
                                                                                                                                {
                                                                                                                                    gameData.frozenstein.RunScript();
                                                                                                                                }
                                                                                                                                else
                                                                                                                                {
                                                                                                                                    if (CharConfig.RunPindleskinScript && !gameData.pindleskin.ScriptDone)
                                                                                                                                    {
                                                                                                                                        gameData.pindleskin.RunScript();
                                                                                                                                    }
                                                                                                                                    else
                                                                                                                                    {
                                                                                                                                        if (CharConfig.RunNihlatakScript && !gameData.nihlatak.ScriptDone)
                                                                                                                                        {
                                                                                                                                            gameData.nihlatak.RunScript();
                                                                                                                                        }
                                                                                                                                        else
                                                                                                                                        {
                                                                                                                                            if (CharConfig.RunBaalScript && !gameData.baal.ScriptDone)
                                                                                                                                            {
                                                                                                                                                gameData.baal.RunScript();
                                                                                                                                            }
                                                                                                                                            else
                                                                                                                                            {
                                                                                                                                                if (CharConfig.RunBaalLeechScript && !gameData.baalLeech.ScriptDone)
                                                                                                                                                {
                                                                                                                                                    gameData.baalLeech.RunScript();
                                                                                                                                                }
                                                                                                                                                else
                                                                                                                                                {
                                                                                                                                                    if (CharConfig.RunTerrorZonesScript && !gameData.terrorZones.ScriptDone)
                                                                                                                                                    {
                                                                                                                                                        gameData.terrorZones.RunScript();
                                                                                                                                                    }
                                                                                                                                                    else
                                                                                                                                                    {
                                                                                                                                                        gameData.LeaveGame(true);
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
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (gameData.battle.ClearingArea)
                                            {
                                                gameData.battle.RunBattleScript();
                                            }
                                            else
                                            {
                                                if (CharConfig.RunDarkWoodRush && !gameData.darkWoodRush.ScriptDone)
                                                {
                                                    gameData.darkWoodRush.RunScript();
                                                }
                                                else
                                                {
                                                    if (CharConfig.RunTristramRush && !gameData.tristramRush.ScriptDone)
                                                    {
                                                        gameData.tristramRush.RunScript();
                                                    }
                                                    else
                                                    {
                                                        if (CharConfig.RunAndarielRush && !gameData.andarielRush.ScriptDone)
                                                        {
                                                            gameData.andarielRush.RunScript();
                                                        }
                                                        else
                                                        {
                                                            if (CharConfig.RunRadamentRush && !gameData.radamentRush.ScriptDone)
                                                            {
                                                                gameData.radamentRush.RunScript();
                                                            }
                                                            else
                                                            {
                                                                if (CharConfig.RunHallOfDeadRush && !gameData.hallOfDeadRushCube.ScriptDone)
                                                                {
                                                                    gameData.hallOfDeadRushCube.RunScript();
                                                                }
                                                                else
                                                                {
                                                                    if (CharConfig.RunFarOasisRush && !gameData.farOasisRush.ScriptDone)
                                                                    {
                                                                        gameData.farOasisRush.RunScript();
                                                                    }
                                                                    else
                                                                    {
                                                                        if (CharConfig.RunLostCityRush && !gameData.lostCityRush.ScriptDone)
                                                                        {
                                                                            gameData.lostCityRush.RunScript();
                                                                        }
                                                                        else
                                                                        {
                                                                            if (CharConfig.RunSummonerRush && !gameData.summonerRush.ScriptDone)
                                                                            {
                                                                                gameData.summonerRush.RunScript();
                                                                            }
                                                                            else
                                                                            {
                                                                                if (CharConfig.RunDurielRush && !gameData.durielRush.ScriptDone)
                                                                                {
                                                                                    gameData.durielRush.RunScript();
                                                                                }
                                                                                else
                                                                                {
                                                                                    if (CharConfig.RunKahlimEyeRush && !gameData.khalimEyeRush.ScriptDone)
                                                                                    {
                                                                                        gameData.khalimEyeRush.RunScript();
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        if (CharConfig.RunKahlimBrainRush && !gameData.khalimBrainRush.ScriptDone)
                                                                                        {
                                                                                            gameData.khalimBrainRush.RunScript();
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            if (CharConfig.RunKahlimHeartRush && !gameData.khalimHeartRush.ScriptDone)
                                                                                            {
                                                                                                gameData.khalimHeartRush.RunScript();
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                if (CharConfig.RunTravincalRush && !gameData.travincalRush.ScriptDone)
                                                                                                {
                                                                                                    gameData.travincalRush.RunScript();
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    if (CharConfig.RunMephistoRush && !gameData.mephistoRush.ScriptDone)
                                                                                                    {
                                                                                                        gameData.mephistoRush.RunScript();
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        if (CharConfig.RunChaosRush && !gameData.chaosRush.ScriptDone)
                                                                                                        {
                                                                                                            gameData.chaosRush.RunScript();
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            if (CharConfig.RunAnyaRush && !gameData.anyaRush.ScriptDone)
                                                                                                            {
                                                                                                                gameData.anyaRush.RunScript();
                                                                                                            }
                                                                                                            else
                                                                                                            {
                                                                                                                if (CharConfig.RunAncientsRush && !gameData.ancientsRush.ScriptDone)
                                                                                                                {
                                                                                                                    gameData.ancientsRush.RunScript();
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                    if (CharConfig.RunBaalRush && !gameData.baalRush.ScriptDone)
                                                                                                                    {
                                                                                                                        gameData.baalRush.RunScript();
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
                                    }
                                }
                            }
                        }
                        gameData.potions.CheckIfWeUsePotion();
                        gameData.gameStruc.CheckChickenGameTime();


                        Grid_SetInfos("Scanned", gameData.itemsStruc.ItemsScanned.ToString());
                        Grid_SetInfos("On ground", gameData.itemsStruc.ItemsOnGround.ToString());
                        Grid_SetInfos("Equipped", gameData.itemsStruc.ItemsEquiped.ToString());
                        Grid_SetInfos("InInventory", gameData.itemsStruc.ItemsInInventory.ToString());
                        Grid_SetInfos("InBelt", gameData.itemsStruc.ItemsInBelt.ToString());
                    }
                    else
                    {
                        gameData.overlayForm.ClearAllOverlay();
                    }
                }
            }
            else
            {
                gameData.battle.TimeSinceLastCast = DateTime.MaxValue;
                gameData.gameStruc.method_GameLabel("");
                method_GameTimeLabel("");
                gameData.playerScan.PrintedLeechFoundInfo = false;
                gameData.potions.ForceLeave = false;
                gameData.FoundPlayerPointerTryCount = 0;
                gameData.HasPointers = false;
                gameData.BotJustStarted = false;

                if (!gameData.PrintedGameTime)
                {
                    gameData.patternsScan.ResetV1Scanning();
                    gameData.mapAreaStruc.AllMapData.Clear();
                    gameData.overlayForm.ClearAllOverlay();
                    gameData.gameStruc.LogGameTime();
                    gameData.TimeSinceSearchingForGames = DateTime.Now;
                    gameData.PrintedGameTime = true;
                }

                if (CharConfig.RunSinglePlayerScript)
                {
                    gameData.keyMouse.MouseClicc(970, 970);  //clic 'play'
                    gameData.WaitDelay(100);

                    if (CharConfig.GameDifficulty == 0) gameData.keyMouse.MouseClicc(960, 450);  //clic 'normal'
                    if (CharConfig.GameDifficulty == 0) gameData.keyMouse.MouseClicc(960, 520);  //clic 'nm'
                    if (CharConfig.GameDifficulty == 0) gameData.keyMouse.MouseClicc(960, 585);  //clic 'hell'
                    gameData.WaitDelay(100);
                }
                else
                {
                    if (!gameData.gameStruc.IsPlayerConnectedToBnet())
                    {
                        SetGameStatus("CONNECTING TO BNET!");
                        gameData.gameStruc.ClicCreateNewChar();
                    }
                    //else
                    //{
                    ChangeCharScript();

                    if (CharConfig.IsRushing)
                    {
                        CharConfig.RunGameMakerScript = false;
                        CharConfig.RunItemGrabScriptOnly = false;
                        CharConfig.RunChaosSearchGameScript = false;
                        CharConfig.RunBaalSearchGameScript = false;
                    }

                    if (CharConfig.RunGameMakerScript)
                    {
                        SetGameStatus("CREATING GAME");

                        if (gameData.BadPlayerPointerFound)
                        {
                            gameData.CurrentGameNumber++;
                            gameData.CurrentGameNumberSinceStart++;
                            gameData.BadPlayerPointerFound = false;
                        }
                        if (gameData.TriedToCreateNewGameCount >= 4)
                        {
                            gameData.CurrentGameNumber++;
                            gameData.CurrentGameNumberSinceStart++;
                            gameData.TriedToCreateNewGameCount = 0;
                        }
                        gameData.gameStruc.CreateNewGame(gameData.CurrentGameNumber);
                    }
                    else
                    {
                        if (CharConfig.RunBaalSearchGameScript && !CharConfig.RunItemGrabScriptOnly)
                        {
                            SetGameStatus("SEARCHING GAMES");
                            gameData.baalLeech.RunScriptNOTInGame();

                            TimeSpan ThisTimeCheckk = DateTime.Now - gameData.TimeSinceSearchingForGames;
                            if (ThisTimeCheckk.TotalMinutes > 8)
                            {
                                LeaveGame(false);
                                gameData.TimeSinceSearchingForGames = DateTime.Now;
                            }
                        }
                        else if (CharConfig.RunChaosSearchGameScript && !CharConfig.RunItemGrabScriptOnly)
                        {
                            SetGameStatus("SEARCHING GAMES");
                            gameData.chaosLeech.RunScriptNOTInGame();

                            TimeSpan ThisTimeCheckk = DateTime.Now - gameData.TimeSinceSearchingForGames;
                            if (ThisTimeCheckk.TotalMinutes > 8)
                            {
                                LeaveGame(false);
                                gameData.TimeSinceSearchingForGames = DateTime.Now;
                            }
                        }
                        else
                        {
                            SetGameStatus("IDLE");
                        }
                    }
                    //}
                }
            }
        }

        SetProcessingTime();

        if (!gameData.Running && gameData.RestartingBot)
        {
            StartBot();
            gameData.RestartingBot = false;
        }

        if (gameData.Running) gameData.LoopTimer.Start();
        if (!gameData.Running) SetBotStopped();
    }

    public void SetBotStopped()
    {
        gameData.HasPointers = false;
        method_1("Bot stopped!", Color.DarkGreen);
        gameData.overlayForm.ClearAllLogsExceptLast();
        gameData.overlayForm.SetAllOverlay();
        gameData.overlayForm.ClearAllOverlay();
    }

    public void SetGameStatus(string string_3)
    {
        //try
        //{
        if (this.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { SetGameStatus(string_3); };
            this.Invoke(safeWrite);
        }
        else
        {
            string RunText = "STOPPED";
            if (gameData.Running) RunText = "RUNNING";

            if (string_3 == "STOPPED") string_3 = PreviousStatus;
            else PreviousStatus = string_3;

            CurrentStatus = string_3;

            this.Text = "D2R - BMBot " + gameData.BotVersion + " | " + RunText + " | " + string_3;
            Application.DoEvents();
        }
        //}
        //catch { }
    }

    public void GoToNextScript()
    {
        if (gameData.battle.ClearingFullArea && gameData.battle.ClearingArea)
        {
            gameData.battle.AllRooms_InArea.RemoveAt(gameData.battle.DoingRoomIndex);
        }
        else
        {
            if (!CharConfig.IsRushing && !gameData.townStruc.GetInTown())
            {
                if (CharConfig.RunWPTaker && !gameData.wpTaker.ScriptDone) gameData.wpTaker.ScriptDone = true;
                else if (CharConfig.RunShopBotScript && !gameData.shopBot.ScriptDone) gameData.shopBot.ScriptDone = true;
                else if (CharConfig.RunMausoleumScript && !gameData.mausoleum.ScriptDone) gameData.mausoleum.ScriptDone = true;
                else if (CharConfig.RunCryptScript && !gameData.crypt.ScriptDone) gameData.crypt.ScriptDone = true;
                else if (CharConfig.RunCowsScript && !gameData.cows.ScriptDone) gameData.cows.ScriptDone = true;
                else if (CharConfig.RunAndarielScript && !gameData.andariel.ScriptDone) gameData.andariel.ScriptDone = true;
                else if (CharConfig.RunCountessScript && !gameData.countess.ScriptDone) gameData.countess.ScriptDone = true;
                else if (CharConfig.RunSummonerScript && !gameData.summoner.ScriptDone) gameData.summoner.ScriptDone = true;
                else if (CharConfig.RunDurielScript && !gameData.duriel.ScriptDone) gameData.duriel.ScriptDone = true;
                else if (CharConfig.RunArachnidScript && !gameData.arachnidLair.ScriptDone) gameData.arachnidLair.ScriptDone = true;
                else if (CharConfig.RunLowerKurastScript && !gameData.lowerKurast.ScriptDone) gameData.lowerKurast.ScriptDone = true;
                else if (CharConfig.RunA3SewersScript && !gameData.act3Sewers.ScriptDone) gameData.act3Sewers.ScriptDone = true;
                else if (CharConfig.RunUpperKurastScript && !gameData.upperKurast.ScriptDone) gameData.upperKurast.ScriptDone = true;
                else if (CharConfig.RunTravincalScript && !gameData.travincal.ScriptDone) gameData.travincal.ScriptDone = true;
                else if (CharConfig.RunMephistoScript && !gameData.mephisto.ScriptDone) gameData.mephisto.ScriptDone = true;
                else if (CharConfig.RunChaosScript && !gameData.chaos.ScriptDone) gameData.chaos.ScriptDone = true;
                else if (CharConfig.RunChaosLeechScript && !gameData.chaosLeech.ScriptDone) gameData.chaosLeech.ScriptDone = true;
                else if (CharConfig.RunEldritchScript && !gameData.eldritch.ScriptDone) gameData.eldritch.ScriptDone = true;
                else if (CharConfig.RunShenkScript && !gameData.shenk.ScriptDone) gameData.shenk.ScriptDone = true;
                else if (CharConfig.RunFrozensteinScript && !gameData.frozenstein.ScriptDone) gameData.frozenstein.ScriptDone = true;
                else if (CharConfig.RunPindleskinScript && !gameData.pindleskin.ScriptDone) gameData.pindleskin.ScriptDone = true;
                else if (CharConfig.RunNihlatakScript && !gameData.nihlatak.ScriptDone) gameData.nihlatak.ScriptDone = true;
                else if (CharConfig.RunBaalScript && !gameData.baal.ScriptDone) gameData.baal.ScriptDone = true;
                else if (CharConfig.RunBaalLeechScript && !gameData.baalLeech.ScriptDone) gameData.baalLeech.ScriptDone = true;
                else if (CharConfig.RunTerrorZonesScript && !gameData.terrorZones.ScriptDone) gameData.terrorZones.ScriptDone = true;
            }
            else
            {
                if (CharConfig.RunDarkWoodRush && !gameData.darkWoodRush.ScriptDone) gameData.darkWoodRush.ScriptDone = true;
                else if (CharConfig.RunTristramRush && !gameData.tristramRush.ScriptDone) gameData.tristramRush.ScriptDone = true;
                else if (CharConfig.RunAndarielRush && !gameData.andarielRush.ScriptDone) gameData.andarielRush.ScriptDone = true;
                else if (CharConfig.RunHallOfDeadRush && !gameData.hallOfDeadRushCube.ScriptDone) gameData.hallOfDeadRushCube.ScriptDone = true;
                else if (CharConfig.RunFarOasisRush && !gameData.farOasisRush.ScriptDone) gameData.farOasisRush.ScriptDone = true;
                else if (CharConfig.RunLostCityRush && !gameData.lostCityRush.ScriptDone) gameData.lostCityRush.ScriptDone = true;
                else if (CharConfig.RunSummonerRush && !gameData.summonerRush.ScriptDone) gameData.summonerRush.ScriptDone = true;
                else if (CharConfig.RunDurielRush && !gameData.durielRush.ScriptDone) gameData.durielRush.ScriptDone = true;
                else if (CharConfig.RunKahlimEyeRush && !gameData.khalimEyeRush.ScriptDone) gameData.khalimEyeRush.ScriptDone = true;
                else if (CharConfig.RunKahlimBrainRush && !gameData.khalimBrainRush.ScriptDone) gameData.khalimBrainRush.ScriptDone = true;
                else if (CharConfig.RunKahlimHeartRush && !gameData.khalimHeartRush.ScriptDone) gameData.khalimHeartRush.ScriptDone = true;
                else if (CharConfig.RunTravincalRush && !gameData.travincalRush.ScriptDone) gameData.travincalRush.ScriptDone = true;
                else if (CharConfig.RunMephistoRush && !gameData.mephistoRush.ScriptDone) gameData.mephistoRush.ScriptDone = true;
                else if (CharConfig.RunChaosRush && !gameData.chaosRush.ScriptDone) gameData.chaosRush.ScriptDone = true;
                else if (CharConfig.RunAncientsRush && !gameData.ancientsRush.ScriptDone) gameData.ancientsRush.ScriptDone = true;
                else if (CharConfig.RunAnyaRush && !gameData.anyaRush.ScriptDone) gameData.anyaRush.ScriptDone = true;
                else if (CharConfig.RunBaalRush && !gameData.baalRush.ScriptDone) gameData.baalRush.ScriptDone = true;
            }
        }
    }

    public void ChangeCharScript()
    {
        long baseAddr = (long)gameData.BaseAddress + (long)gameData.offsets["SelectedChar"];
        byte[] buffer = new byte[16];
        gameData.mem.ReadRawMemory(baseAddr, ref buffer, 16);

        string name = "";
        for (int i2 = 0; i2 < 16; i2++)
        {
            if (buffer[i2] != 0x00)
            {
                name += (char)buffer[i2];
            }
        }
        //gameData.method_1("PNAME: " + name, Color.Red);

        if (!name.Contains(CharConfig.PlayerCharName) || gameData.ForceSwitch2ndPlayer)
        {
            method_1("Changing Char...", Color.Red);

            //Esc
            gameData.keyMouse.PressKey(Keys.Escape);
            gameData.WaitDelay(120);
            Application.DoEvents();

            //Select Top Char
            if (!gameData.ForceSwitch2ndPlayer)
            {
                gameData.keyMouse.MouseClicc(1700, 85);
                gameData.WaitDelay(10);
            }
            else
            {
                gameData.keyMouse.MouseClicc(1700, 200);
                gameData.WaitDelay(10);
            }

            gameData.keyMouse.MouseClicc(1190, 990); //clic 'salon' if not in server
            gameData.WaitDelay(10);

            gameData.keyMouse.MouseClicc(1415, 65);  //clic 'join game' if not in game list area
            gameData.WaitDelay(10);

            gameData.keyMouse.MouseClicc(1720, 210); //clic refresh
            gameData.WaitDelay(60);

            gameData.ForceSwitch2ndPlayer = false;
        }
    }

    public void SetProcessingTime()
    {
        //Get processing time (ex: 1.125s)
        DateTime CompareTime = DateTime.Now;
        TimeSpan testtime = (CompareTime - gameData.CheckTime);
        string TimeStr = "";
        if (testtime.Seconds > 0)
        {
            TimeStr += testtime.Seconds + ".";
        }
        if (testtime.Milliseconds > 0)
        {
            //TimeStr += testtime.Milliseconds.ToString("000");
            TimeStr += testtime.Milliseconds.ToString();
        }
        else
        {
            TimeStr += "0";
        }
        TimeStr += "ms";

        //convert to FPS
        long TimeMS = testtime.Milliseconds + (testtime.Seconds * 1000);
        gameData.FPS = 1000.0 / (double)TimeMS;

        SetAverageFPSandMS(testtime.Milliseconds);

        gameData.overlayForm.SetAllOverlay();

        gameData.mS = TimeStr;
        Grid_SetInfos("Processing Time", TimeStr + "-" + gameData.FPS.ToString("00") + "FPS");
        gameData.CheckTime = DateTime.Now;

        if (gameData.gameStruc.IsInGame())
        {
            TimeSpan Checkkt = (DateTime.Now - gameData.GameStartedTime);
            method_GameTimeLabel(Checkkt.Minutes.ToString("00") + ":" + Checkkt.Seconds.ToString("00") + ":" + Checkkt.Milliseconds.ToString("0"));
        }
        /*else
        {
            method_GameTimeLabel("");
        }*/
        Grid_SetInfos("Scanned", gameData.itemsStruc.ItemsScanned.ToString());
        Grid_SetInfos("On ground", gameData.itemsStruc.ItemsOnGround.ToString());
        Grid_SetInfos("Equipped", gameData.itemsStruc.ItemsEquiped.ToString());
        Grid_SetInfos("InInventory", gameData.itemsStruc.ItemsInInventory.ToString());
        Grid_SetInfos("InBelt", gameData.itemsStruc.ItemsInBelt.ToString());
    }

    public void SetAverageFPSandMS(int ThisMSValue)
    {
        //Get averag FPS
        if (gameData.Averge_FPSList.Count >= 50) gameData.Averge_FPSList.RemoveAt(0);
        gameData.Averge_FPSList.Add(gameData.FPS);

        double FullValue = 0.0;
        for (int i = 0; i < gameData.Averge_FPSList.Count; i++) FullValue += gameData.Averge_FPSList[i];
        gameData.Average_FPS = FullValue / gameData.Averge_FPSList.Count;

        //Get averag mS
        if (gameData.Averge_mSList.Count >= 50) gameData.Averge_mSList.RemoveAt(0);
            gameData.Averge_mSList.Add(ThisMSValue);

        int FullMSValue = 0;
        for (int i = 0; i < gameData.Averge_mSList.Count; i++) FullMSValue += gameData.Averge_mSList[i];
        gameData.Average_mS = FullMSValue / gameData.Averge_mSList.Count;
    }

    public string CurrentGameTime = "";

    public void method_GameTimeLabel(string string_3)
    {
        //try
        //{
        /*if (gameData.labelGameTime.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { method_GameTimeLabel(string_3); };
            gameData.labelGameTime.Invoke(safeWrite);
        }
        else
        {
            gameData.labelGameTime.Text = string_3;
            Application.DoEvents();
        }*/
        try
        {
            CurrentGameTime = string_3;
            labelGameTime.Text = string_3;
        }
        catch { }
    }

    public void WaitDelay(int DelayTime)
    {
        DateTime TimeStart = DateTime.Now;
        TimeSpan ThisTime = DateTime.Now - TimeStart;

        int CurrentWait = 0;
        int WaitingDelay = (int) ((DelayTime * 10.0) * CharConfig.OverallDelaysMultiplyer);
        while (ThisTime.TotalMilliseconds < WaitingDelay)
        {
            SetProcessingTime();
            Thread.Sleep(1);
            Application.DoEvents();
            ThisTime = DateTime.Now - TimeStart;
            CurrentWait++;
        }
    }

    public void SetPlayButtonText(string string_3)
    {
        //try
        //{
        if (button1.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { SetPlayButtonText(string_3); };
            button1.Invoke(safeWrite);
        }
        else
        {
            button1.Text = string_3;
            Application.DoEvents();
        }
        //}
        //catch { }
    }

    public void SetSettingButton(bool Enabledd)
    {
        //try
        //{
        if (button3.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { SetSettingButton(Enabledd); };
            button3.Invoke(safeWrite);
        }
        else
        {
            button3.Enabled = Enabledd;
            Application.DoEvents();
        }

        SetItemsButton(Enabledd);
        SetCharButtonEnable(Enabledd);
        //}
        //catch { }
    }

    public void SetItemsButton(bool Enabledd)
    {
        //try
        //{
        if (button4.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { SetItemsButton(Enabledd); };
            button4.Invoke(safeWrite);
        }
        else
        {
            button4.Enabled = Enabledd;
            Application.DoEvents();
        }
        //}
        //catch { }
    }

    public void SetPauseResumeButton(bool Enabledd)
    {
        //try
        //{
        if (buttonPauseResume.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { SetPauseResumeButton(Enabledd); };
            buttonPauseResume.Invoke(safeWrite);
        }
        else
        {
            buttonPauseResume.Enabled = Enabledd;
            Application.DoEvents();
        }
        //}
        //catch { }
    }

    public void StopBot()
    {
        gameData.RestartingBot = false;
        //SetPlayButtonText("START");
        gameData.Running = false;
        gameData.HasPointers = false;
        gameData.playerScan.FoundPlayer = false;
        gameData.LoopDone = 0;
        gameData.stash.StashFull = false;
        SetSettingButton(true);
        gameData.LoopTimer.Stop();
        //mapAreaStruc.AllMapData.Clear();
        gameData.overlayForm.ClearAllOverlay();
        SetGameStatus("STOPPED");
    }

    public void StartBot()
    {
        method_1("Bot started!", Color.DarkGreen);
        SetSettingButton(false);
        SetPauseResumeButton(true);
        //SetPlayButtonText("STOP");
        gameData.Running = true;
        gameData.BotJustStarted = true;
        gameData.GameStartedTime = DateTime.Now;
        gameData.inventoryStruc.DisabledSpecialItems = false;
        Startt();
    }

    public void button1_Click(object sender, EventArgs e)
    {
        if (!gameData.Running && button1.Enabled)
        {
            gameData.BotPaused = false;
            gameData.BotResuming = false;
            method_1("Bot will start!", Color.DarkGreen);
            this.button1.Image = global::app.Properties.Resources.control_stop_square_green;
            this.buttonPauseResume.Image = global::app.Properties.Resources.control_pause;
            StartBot();
        }
        else if (!gameData.Running && !button1.Enabled)
        {
            method_1("Bot will restart!", Color.DarkGreen);
            this.button1.Image = global::app.Properties.Resources.control_stop_square_green;
            this.buttonPauseResume.Image = global::app.Properties.Resources.control_pause;
            gameData.BotPaused = false;
            gameData.BotResuming = false;
            gameData.RestartingBot = true;
        }
        else if (gameData.Running)
        {
            method_1("Bot will stop!", Color.DarkGreen);
            this.button1.Image = global::app.Properties.Resources.control;
            this.buttonPauseResume.Image = global::app.Properties.Resources.control_pause;
            SetPauseResumeButton(false);
            StopBot();
        }
    }

    public void buttonPauseResume_Click(object sender, EventArgs e)
    {
        if (gameData.Running && !gameData.BotPaused)
        {
            if (!gameData.BotPaused)
            {
                method_1("Bot will pause!", Color.DarkGreen);
                this.buttonPauseResume.Image = global::app.Properties.Resources.control;
                StopBot();
                gameData.BotPaused = true;
            }
        }
        else if (!gameData.Running && button1.Enabled)
        {
            method_1("Bot will resume!", Color.DarkGreen);
            this.buttonPauseResume.Image = global::app.Properties.Resources.control_pause;
            gameData.BotPaused = false;
            gameData.BotResuming = true;
            StartBot();
        }
        else if (!gameData.Running && !button1.Enabled)
        {
            method_1("Bot will resume!", Color.DarkGreen);
            this.buttonPauseResume.Image = global::app.Properties.Resources.control_pause;
            gameData.BotPaused = false;
            gameData.BotResuming = true;
            gameData.RestartingBot = true;
        }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        gameData.settingsLoader.SaveCurrentSettings();
        gameData.settingsLoader.SaveOthersSettings();
        KeyMouse.UnhookWindowsHookEx(gameData.keyMouse.hookID);
    }

    private void button3_Click(object sender, EventArgs e)
    {
        FormSettings FormSettings_0 = new FormSettings(this);
        FormSettings_0.ShowDialog();
    }

    private void charSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        FormCharSettings FormCharSettings_0 = new FormCharSettings(this);
        FormCharSettings_0.ShowDialog();
    }

    public bool CanReloadCollision = true;
    private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tabControl2.SelectedIndex == 0) gameData.itemsStruc.DebugItems();
        if (tabControl2.SelectedIndex == 1) { gameData.mobsStruc.DebuggingMobs = true; gameData.overlayForm.ShowMobs = true; }
        if (tabControl2.SelectedIndex == 2) gameData.objectsStruc.DebugObjects();
        if (tabControl2.SelectedIndex == 3) gameData.mapAreaStruc.DebugMapData();
        if (tabControl2.SelectedIndex == 4)
        {
            CanReloadCollision = false;
            comboBoxCollisionArea.SelectedIndex = (int)gameData.playerScan.levelNo - 1;
            gameData.pathFinding.DebugMapCollision();
            CanReloadCollision = true;
        }

        if (tabControl2.SelectedIndex != 1) gameData.mobsStruc.DebuggingMobs = false;
    }

    public void SetDebugMenu()
    {
        if (gameData.DebugMenuStyle == 0)
        {
            this.Size = new System.Drawing.Size(357, 446);
        }
        else if (gameData.DebugMenuStyle == 1)
        {
            this.Size = new System.Drawing.Size(570, 446);
        }
        else if (gameData.DebugMenuStyle == 2)
        {
            this.Size = new System.Drawing.Size(570, 678);
        }
    }

    private void button2_Click(object sender, EventArgs e)
    {
        if (gameData.DebugMenuStyle < 2) gameData.DebugMenuStyle++;
        else gameData.DebugMenuStyle = 0;
        SetDebugMenu();

        if (gameData.DebugMenuStyle == 2) tabControl2_SelectedIndexChanged(null, null);
    }


    public void AppendTextDebugItems(string ThisT)
    {
        if (richTextBoxDebugItems.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { AppendTextDebugItems(ThisT); };
            richTextBoxDebugItems.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugItems.AppendText(ThisT);
            Application.DoEvents();
        }
    }

    public void ClearDebugItems()
    {
        if (richTextBoxDebugItems.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { ClearDebugItems(); };
            richTextBoxDebugItems.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugItems.Text = "";
            Application.DoEvents();
        }
    }

    public void AppendTextDebugObjects(string ThisT)
    {
        if (richTextBoxDebugObjects.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { AppendTextDebugObjects(ThisT); };
            richTextBoxDebugObjects.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugObjects.AppendText(ThisT);
            Application.DoEvents();
        }
    }

    public void ClearDebugobjects()
    {
        if (richTextBoxDebugObjects.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { ClearDebugobjects(); };
            richTextBoxDebugObjects.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugObjects.Text = "";
            Application.DoEvents();
        }
    }

    public void AppendTextDebugMobs(string ThisT)
    {
        if (richTextBoxDebugMobs.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { AppendTextDebugMobs(ThisT); };
            richTextBoxDebugMobs.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugMobs.AppendText(ThisT);
            Application.DoEvents();
        }
    }

    public void ClearDebugMobs()
    {
        if (richTextBoxDebugMobs.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { ClearDebugMobs(); };
            richTextBoxDebugMobs.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugMobs.Text = "";
            Application.DoEvents();
        }
    }

    public void AppendTextDebugMapData(string ThisT)
    {
        if (richTextBoxDebugMapData.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { AppendTextDebugMapData(ThisT); };
            richTextBoxDebugMapData.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugMapData.AppendText(ThisT);
            Application.DoEvents();
        }
    }

    public void ClearDebugMapData()
    {
        if (richTextBoxDebugMapData.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { ClearDebugMapData(); };
            richTextBoxDebugMapData.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugMapData.Text = "";
            Application.DoEvents();
        }
    }

    public void AppendTextDebugCollision(string ThisT)
    {
        if (richTextBoxDebugMapCollision.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { AppendTextDebugCollision(ThisT); };
            richTextBoxDebugMapCollision.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugMapCollision.AppendText(ThisT);
            Application.DoEvents();
        }
    }

    public void ClearDebugCollision()
    {
        if (richTextBoxDebugMapCollision.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { ClearDebugCollision(); };
            richTextBoxDebugMapCollision.Invoke(safeWrite);
        }
        else
        {
            richTextBoxDebugMapCollision.Text = "";
            Application.DoEvents();
        }
    }

    private void button3_Click_1(object sender, EventArgs e)
    {
        FormSettings FormSettings_0 = new FormSettings(this);
        FormSettings_0.ShowDialog();
    }

    public void AppendTextErrorLogs(string ThisT, Color ThisColor)
    {
        if (richTextBoxErrorLogs.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { AppendTextErrorLogs(ThisT, ThisColor); };
            richTextBoxErrorLogs.Invoke(safeWrite);
        }
        else
        {
            richTextBoxErrorLogs.SelectionColor = ThisColor;
            richTextBoxErrorLogs.AppendText(ThisT + Environment.NewLine);
            Application.DoEvents();
        }
    }
    public void AppendTextGameLogs(string ThisT, Color ThisColor)
    {
        if (richTextBoxGamesLogs.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { AppendTextGameLogs(ThisT, ThisColor); };
            richTextBoxGamesLogs.Invoke(safeWrite);
        }
        else
        {
            richTextBoxGamesLogs.SelectionColor = ThisColor;
            richTextBoxGamesLogs.AppendText(ThisT + Environment.NewLine);
            Application.DoEvents();
        }
    }


    public void SetStartButtonEnable(bool Enabled)
    {
        if (button1.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { SetStartButtonEnable(Enabled); };
            button1.Invoke(safeWrite);
        }
        else
        {
            button1.Enabled = Enabled;
            Application.DoEvents();
        }
    }

    private void button4_Click(object sender, EventArgs e)
    {
        FormItems FormItems_0 = new FormItems(this);
        FormItems_0.ShowDialog();
    }

    private void button5_Click(object sender, EventArgs e)
    {
        FormCharSettings FormCharSettings_0 = new FormCharSettings(this);
        FormCharSettings_0.ShowDialog();
    }

    public void SetCharButtonEnable(bool Enabled)
    {
        if (button5.InvokeRequired)
        {
            // Call this same method but append THREAD2 to the text
            Action safeWrite = delegate { SetCharButtonEnable(Enabled); };
            button5.Invoke(safeWrite);
        }
        else
        {
            button5.Enabled = Enabled;
            Application.DoEvents();
        }
    }

    public void ModifyMonsterList()
    {
        string[] AllLines = File.ReadAllLines(Application.StartupPath + @"\List.txt");
        string EndTxt = "";
        EndTxt += "public enum MonsterType" + Environment.NewLine;
        EndTxt += "{" + Environment.NewLine;

        for (int i = 0; i < AllLines.Length; i++)
        {
            if (AllLines[i].Length > 0)
            {
                //EndTxt += AllLines[i].Substring(0, AllLines[i].IndexOf('\t'));
                AllLines[i] = AllLines[i].Substring(AllLines[i].IndexOf('\t') + 1);
                string ThidID = AllLines[i].Substring(0, AllLines[i].IndexOf('\t'));

                AllLines[i] = AllLines[i].Substring(AllLines[i].IndexOf('\t') + 1);
                AllLines[i] = AllLines[i].Substring(AllLines[i].IndexOf('\t') + 1);
                AllLines[i] = AllLines[i].Substring(AllLines[i].IndexOf('\t') + 1);
                AllLines[i] = AllLines[i].Substring(AllLines[i].IndexOf('\t') + 1);
                string ThidName = AllLines[i].Substring(0, AllLines[i].IndexOf('\t'));

                if (ThidName == "dummy" || ThidName == "Dummy" || ThidName == "unused" || ThidName == "Unused" || ThidName == "")
                {
                    AllLines[i] = AllLines[i].Substring(AllLines[i].IndexOf('\t') + 1);
                    ThidName = AllLines[i].Substring(0, AllLines[i].IndexOf('\t'));
                }

                EndTxt += "\t" + ThidName.Replace(" ", "") + " = " + ThidID + "," + Environment.NewLine;
            }
        }
        EndTxt += "}";

        File.Create(Application.StartupPath + @"\List2.txt").Dispose();
        File.WriteAllText(Application.StartupPath + @"\List2.txt", EndTxt);
    }

    private void buttonD2LOD_Click(object sender, EventArgs e)
    {
        FormD2LOD FormD2LOD_0 = new FormD2LOD(this);
        FormD2LOD_0.ShowDialog();
    }

    private void buttonUpdate_Click(object sender, EventArgs e)
    {
        System.Diagnostics.Process.Start("https://github.com/bouletmarc/D2R-BMBot/releases");
    }

    private void comboBoxItemsCategory_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!gameData.LoadingBot) gameData.itemsStruc.DebugItems();
    }

    private void richTextBoSoldLogs_MouseMove(object sender, MouseEventArgs e)
    {
        RichTextBox richTextBox = (RichTextBox)sender;

        // Get the character index and line index from the mouse position
        int charIndex = richTextBox.GetCharIndexFromPosition(e.Location);
        int lineIndex = richTextBox.GetLineFromCharIndex(charIndex);

        // Get the start and end positions of the line
        int lineStartIndex = richTextBox.GetFirstCharIndexFromLine(lineIndex);
        int lineEndIndex = richTextBox.GetFirstCharIndexFromLine(lineIndex + 1);
        if (lineEndIndex == -1) lineEndIndex = richTextBox.Text.Length;

        // Get the text of the hovered line
        string lineText = richTextBox.Text.Substring(lineStartIndex, lineEndIndex - lineStartIndex).Trim();

        gameData.itemsViewer.ShowItemScreenshot(lineIndex, "Sold");
        // Display the line number and text in the console (or any other desired action)
        //Console.WriteLine("Line Number: " + (lineIndex + 1));
        //Console.WriteLine("Line Text: " + lineText);
    }

    private void richTextBoSoldLogs_MouseLeave(object sender, EventArgs e)
    {
        gameData.itemsViewer.UnshowItem();
    }

    private void comboBoxCollisionArea_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (CanReloadCollision) gameData.pathFinding.DebugMapCollision();
    }
}
