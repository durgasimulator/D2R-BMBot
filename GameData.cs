using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GameData
{
    private static GameData _instance;
    private static readonly object _lock = new object();
    public Form1 form;
    public string BotVersion = "V3.04";
    public string D2_LOD_113C_Path = "";

    // Form Stuff ?
    public OverlayForm overlayForm;

    // GAME
    public Process process;
    public Dictionary<string, IntPtr> offsets = new Dictionary<string, IntPtr>();
    public IntPtr BaseAddress = (IntPtr)0;
    public IntPtr processHandle = (IntPtr)0;
    public byte[] buffer = new byte[0x3FFFFFF];
    public byte[] bufferRead = new byte[0];
    public System.Timers.Timer LoopTimer;
    public bool HasPointers = false;
    public int UnitStrucOffset = -32;
    public int hWnd = 0;
    public Rectangle D2Rect = new Rectangle();
    public int ScreenX = 1920;
    public int ScreenY = 1080;
    public int CenterX = 0;
    public int CenterY = 0;
    public int D2Width = 0;
    public int D2Height = 0;
    public int ScreenXOffset = 0;
    public int ScreenYOffset = 0;
    public double centerModeScale = 2.262;
    public int renderScale = 3;
    public int ScreenYMenu = 180;

    // STATUS and SETTINGS
    public bool Running = false;
    public bool RunFinished = false;
    public int LoopDone = 0;
    public bool CharDied = false;
    public bool RestartingBot = false;
    public bool BotPaused = false;
    public bool BotResuming = false;
    public bool PrintedGameTime = false;
    public DateTime CheckTime = new DateTime();
    public DateTime GameStartedTime = new DateTime();
    public DateTime TimeSinceSearchingForGames = new DateTime();
    public int CurrentGameNumber = 1;
    public int CurrentGameNumberFullyDone = 0;
    public bool SetGameDone = false;
    public int FoundPlayerPointerTryCount = 0;
    public int FoundPlayerPointerRetryTimes = 0;
    public int TriedToCreateNewGameCount = 0;
    public int CurrentGameNumberSinceStart = 1;
    public bool ForceSwitch2ndPlayer = false;
    public bool BadPlayerPointerFound = false;
    public bool PublicGame = false;
    public int DebugMenuStyle = 0;
    public bool BotJustStarted = true;
    public bool SetDeadCount = false;
    public bool LoadingBot = true;
    public int TotalChickenCount = 0;
    public int TotalDeadCount = 0;
    public int TotalChickenByTimeCount = 0;

    public double FPS = 0;
    public string mS = "";
    public List<double> Averge_FPSList = new List<double>();
    public List<int> Averge_mSList = new List<int>();
    public double Average_FPS = 0;
    public int Average_mS = 0;

    // Public properties for all instances
    public ItemsStruc itemsStruc { get; set; }
    public Mem mem { get; set; }
    public PatternsScan patternsScan { get; set; }
    public GameStruc gameStruc { get; set; }
    public PlayerScan playerScan { get; set; }
    public ItemsAlert itemsAlert { get; set; }
    public UIScan uiScan { get; set; }
    public BeltStruc beltStruc { get; set; }
    public ItemsFlags itemsFlags { get; set; }
    public ItemsNames itemsNames { get; set; }
    public InventoryStruc inventoryStruc { get; set; }
    public MobsStruc mobsStruc { get; set; }
    public NPCStruc npcStruc { get; set; }
    public HoverStruc hoverStruc { get; set; }
    public Town townStruc { get; set; }
    public Potions potions { get; set; }
    public SkillsStruc skillsStruc { get; set; }
    public ObjectsStruc objectsStruc { get; set; }
    public Mover mover { get; set; }
    public Stash stash { get; set; }
    public Shop shop { get; set; }
    public Repair repair { get; set; }
    public Battle battle { get; set; }
    public KeyMouse keyMouse { get; set; }


    public MercStruc mercStruc { get; set; }
    public StashStruc stashStruc { get; set; }
    public Cubing cubing { get; set; }
    public Gamble gamble { get; set; }

    public SettingsLoader settingsLoader { get; set; }
    public MapAreaStruc mapAreaStruc { get; set; }
    public PathFinding pathFinding { get; set; }

    public AreaScript areaScript { get; set; }
    public ItemsViewer itemsViewer { get; set; }
    public List<object> AllClassInstances { get; set; }

    public List<IBot> bots { get; }
    // Bots
    // rushes
    public IBot darkWoodRush { get; set; }
    public IBot tristramRush { get; set; }
    public IBot andarielRush { get; set; }
    public IBot radamentRush { get; set; }
    public IBot hallOfDeadRushCube { get; set; }
    public IBot farOasisRush { get; set; }
    public IBot lostCityRush { get; set; }
    public IBot summonerRush { get; set; }
    public IBot durielRush { get; set; }
    public IBot khalimBrainRush { get; set; }
    public IBot khalimEyeRush { get; set; }
    public IBot khalimHeartRush { get; set; }
    public IBot travincalRush { get; set; }
    public IBot mephistoRush { get; set; }
    public IBot anyaRush { get; set; }
    public IBot ancientsRush { get; set; }
    public IBot chaosRush { get; set; }
    public IBot baalRush { get; set; }

    // farming
    public IBot crypt { get; set; }
    public IBot countess { get; set; }
    public IBot andariel { get; set; }
    public IBot duriel { get; set; }
    public IBot arachnidLair { get; set; }
    public IBot act3Sewers { get; set; }
    public IBot chaos { get; set; }
    public IBot baal { get; set; }
    public IBot cows { get; set; }
    public IBot terrorZones { get; set; }
    public IBot eldritch { get; set; }
    public IBot frozenstein { get; set; }
    public IBot lowerKurast { get; set; }
    public IBot mausoleum { get; set; }
    public IBot mephisto { get; set; }
    public IBot nihlatak { get; set; }
    public IBot pindleskin { get; set; }
    public IBot pit { get; set; }
    public IBot shenk { get; set; }
    public IBot shopBot { get; set; }
    public IBot summoner { get; set; }
    public IBot travincal { get; set; }
    public IBot upperKurast { get; set; }
    public IBot wpTaker { get; set; }


    // leech
    public ChaosLeech chaosLeech { get; set; }
    public BaalLeech baalLeech { get; set; }

    // Private constructor to prevent instantiation from outside
    private GameData()
    {
        itemsStruc = new ItemsStruc();
        mem = new Mem();
        patternsScan = new PatternsScan();
        gameStruc = new GameStruc();
        playerScan = new PlayerScan();
        itemsAlert = new ItemsAlert();
        itemsAlert.Init();
        uiScan = new UIScan();
        beltStruc = new BeltStruc();
        itemsFlags = new ItemsFlags();
        itemsNames = new ItemsNames();
        inventoryStruc = new InventoryStruc();
        mobsStruc = new MobsStruc();
        npcStruc = new NPCStruc();
        hoverStruc = new HoverStruc();
        townStruc = new Town();
        potions = new Potions();
        skillsStruc = new SkillsStruc();
        objectsStruc = new ObjectsStruc();
        mover = new Mover();
        stash = new Stash();
        shop = new Shop();
        repair = new Repair();
        summoner = new Summoner();
        chaosLeech = new ChaosLeech();
        chaos = new Chaos();
        battle = new Battle();
        keyMouse = new KeyMouse();
        keyMouse.Init();
        duriel = new Duriel();
        pindleskin = new Pindleskin();
        baalLeech = new BaalLeech();
        baal = new Baal();
        travincal = new Travincal();
        mephisto = new Mephisto();
        andariel = new Andariel();
        countess = new Countess();
        mercStruc = new MercStruc();
        stashStruc = new StashStruc();
        cubing = new Cubing();
        gamble = new Gamble();
        lowerKurast = new LowerKurast();
        act3Sewers = new Act3Sewers();
        upperKurast = new UpperKurast();
        settingsLoader = new SettingsLoader();
        mapAreaStruc = new MapAreaStruc();
        pathFinding = new PathFinding();
        wpTaker = new WPTaker();
        cows = new Cows();
        eldritch = new Eldritch();
        shenk = new Shenk();
        nihlatak = new Nihlatak();
        frozenstein = new Frozenstein();
        terrorZones = new TerrorZones();
        areaScript = new AreaScript();
        shopBot = new ShopBot();
        mausoleum = new Mausoleum();
        crypt = new Crypt();
        arachnidLair = new ArachnidLair();
        pit = new Pit();
        andarielRush = new AndarielRush();
        darkWoodRush = new DarkWoodRush();
        durielRush = new DurielRush();
        farOasisRush = new FarOasisRush();
        hallOfDeadRushCube = new HallOfDeadRushCube();
        khalimBrainRush = new KhalimBrainRush();
        khalimEyeRush = new KhalimEyeRush();
        khalimHeartRush = new KhalimHeartRush();
        lostCityRush = new LostCityRush();
        mephistoRush = new MephistoRush();
        radamentRush = new RadamentRush();
        summonerRush = new SummonerRush();
        travincalRush = new TravincalRush();
        tristramRush = new TristramRush();
        ancientsRush = new AncientsRush();
        anyaRush = new AnyaRush();
        chaosRush = new ChaosRush();
        baalRush = new BaalRush();
        itemsViewer = new ItemsViewer();
        AllClassInstances = new List<object>();
        settingsLoader.LoadSettings();

        // all IBot scripts
        List<IBot> bots = new List<IBot> {
            darkWoodRush, tristramRush, andarielRush, radamentRush, hallOfDeadRushCube, farOasisRush, lostCityRush, summonerRush, durielRush,
            khalimBrainRush, khalimEyeRush, khalimHeartRush, travincalRush, mephistoRush, anyaRush, ancientsRush, chaosRush, baalRush,
            crypt, countess, andariel, duriel, arachnidLair, act3Sewers, chaos, baal, cows, terrorZones, eldritch, frozenstein,
            lowerKurast, mausoleum, mephisto, nihlatak, pindleskin, pit, shenk, shopBot, summoner, travincal, upperKurast, wpTaker
        };
    }

    // Public method to get the instance of the class
    public static GameData Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new GameData();
                }
                return _instance;
            }
        }
    }

    // Proxy Functions
    public void SetGameStatus(string status)
    {
        form.SetGameStatus(status);
    }

    public void WaitDelay(int delay)
    {
        form.WaitDelay(delay);
    }
    public void method_1(string message, Color color)
    {
        form.method_1(message, color);
    }

    public void LeaveGame(bool finished)
    {
        form.LeaveGame(finished);
    }

    // Helper Functions

    public void ResetRunVars()
    {
        foreach (IBot bot in bots)
        {
            bot.ResetVars();
        }
    }
}


