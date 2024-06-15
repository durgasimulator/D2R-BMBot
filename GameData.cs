using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

public class GameData
{

    private static readonly Lazy<GameData> _instance = new Lazy<GameData>(() => new GameData());

    public static GameData Instance => _instance.Value;

    private bool _isInitialized = false;
    public Form1 form;
    public string BotVersion = "V3.04";
    public string D2_LOD_113C_Path = "";

    public OverlayForm overlayForm;
    public Process process;
    public Dictionary<string, IntPtr> offsets = new Dictionary<string, IntPtr>();
    public IntPtr BaseAddress = (IntPtr)0;
    public IntPtr processHandle = (IntPtr)0;
    public byte[] buffer;
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

    public ItemsStruc itemsStruc { get; private set; }
    public Mem mem { get; private set; }
    public PatternsScan patternsScan { get; private set; }
    public GameStruc gameStruc { get; private set; }
    public PlayerScan playerScan { get; private set; }
    public ItemsAlert itemsAlert { get; private set; }
    public UIScan uiScan { get; private set; }
    public BeltStruc beltStruc { get; private set; }
    public ItemsFlags itemsFlags { get; private set; }
    public ItemsNames itemsNames { get; private set; }
    public InventoryStruc inventoryStruc { get; private set; }
    public MobsStruc mobsStruc { get; private set; }
    public NPCStruc npcStruc { get; private set; }
    public HoverStruc hoverStruc { get; private set; }
    public Town townStruc { get; private set; }
    public Potions potions { get; private set; }
    public SkillsStruc skillsStruc { get; private set; }
    public ObjectsStruc objectsStruc { get; private set; }
    public Mover mover { get; private set; }
    public Stash stash { get; private set; }
    public Shop shop { get; private set; }
    public Repair repair { get; private set; }
    public Battle battle { get; private set; }
    public KeyMouse keyMouse { get; private set; }

    public MercStruc mercStruc { get; private set; }
    public StashStruc stashStruc { get; private set; }
    public Cubing cubing { get; private set; }
    public Gamble gamble { get; private set; }

    public SettingsLoader settingsLoader { get; private set; }
    public MapAreaStruc mapAreaStruc { get; private set; }
    public PathFinding pathFinding { get; private set; }

    public AreaScript areaScript { get; private set; }
    public ItemsViewer itemsViewer { get; private set; }
    public List<object> AllClassInstances { get; private set; }

    public List<IBot> bots { get; private set; }

    public IBot darkWoodRush { get; private set; }
    public IBot tristramRush { get; private set; }
    public IBot andarielRush { get; private set; }
    public IBot radamentRush { get; private set; }
    public IBot hallOfDeadRushCube { get; private set; }
    public IBot farOasisRush { get; private set; }
    public IBot lostCityRush { get; private set; }
    public IBot summonerRush { get; private set; }
    public IBot durielRush { get; private set; }
    public IBot khalimBrainRush { get; private set; }
    public IBot khalimEyeRush { get; private set; }
    public IBot khalimHeartRush { get; private set; }
    public IBot travincalRush { get; private set; }
    public IBot mephistoRush { get; private set; }
    public IBot anyaRush { get; private set; }
    public IBot ancientsRush { get; private set; }
    public IBot chaosRush { get; private set; }
    public IBot baalRush { get; private set; }

    public IBot crypt { get; private set; }
    public IBot countess { get; private set; }
    public IBot andariel { get; private set; }
    public IBot duriel { get; private set; }
    public IBot arachnidLair { get; private set; }
    public IBot act3Sewers { get; private set; }
    public IBot chaos { get; private set; }
    public IBot baal { get; private set; }
    public IBot cows { get; private set; }
    public IBot terrorZones { get; private set; }
    public IBot eldritch { get; private set; }
    public IBot frozenstein { get; private set; }
    public IBot lowerKurast { get; private set; }
    public IBot mausoleum { get; private set; }
    public IBot mephisto { get; private set; }
    public IBot nihlatak { get; private set; }
    public IBot pindleskin { get; private set; }
    public IBot pit { get; private set; }
    public IBot shenk { get; private set; }
    public IBot shopBot { get; private set; }
    public IBot summoner { get; private set; }
    public IBot travincal { get; private set; }
    public IBot upperKurast { get; private set; }
    public IBot wpTaker { get; private set; }

    public ChaosLeech chaosLeech { get; private set; }
    public BaalLeech baalLeech { get; private set; }

    private GameData()
    {
        // Dont call Initialize here to avoid access to Instance during construction
        
    }

    public void Initialize(Form1 form)
    {
        if (_isInitialized) return;
        this.form = form;
        overlayForm = new OverlayForm(this);
        itemsStruc = new ItemsStruc();
        itemsStruc.Initialize(this);

        mem = new Mem();
        mem.Initialize(this);

        patternsScan = new PatternsScan();
        patternsScan.Initialize(this);

        gameStruc = new GameStruc();
        gameStruc.Initialize(this);

        playerScan = new PlayerScan();
        playerScan.Initialize(this);

        itemsAlert = new ItemsAlert();
        itemsAlert.Initialize(this);

        uiScan = new UIScan();
        uiScan.Initialize(this);

        beltStruc = new BeltStruc();
        beltStruc.Initialize(this);

        itemsFlags = new ItemsFlags();
        itemsFlags.Initialize(this);

        itemsNames = new ItemsNames();

        inventoryStruc = new InventoryStruc();
        inventoryStruc.Initialize(this);

        mobsStruc = new MobsStruc();
        mobsStruc.Initialize(this);

        npcStruc = new NPCStruc();
        npcStruc.Initialize(this);

        hoverStruc = new HoverStruc();
        hoverStruc.Initialize(this);

        townStruc = new Town();
        townStruc.Initialize(this);

        potions = new Potions();
        potions.Initialize(this);

        skillsStruc = new SkillsStruc();

        objectsStruc = new ObjectsStruc();
        objectsStruc.Initialize(this);

        mover = new Mover();
        mover.Initialize(this);

        stash = new Stash();
        stash.Initialize(this);

        shop = new Shop();
        shop.Initialize(this);

        repair = new Repair();
        repair.Initialize(this);

        summoner = new Summoner();
        summoner.Initialize(this);

        chaosLeech = new ChaosLeech();
        chaosLeech.Initialize(this);

        chaos = new Chaos();
        chaos.Initialize(this);

        battle = new Battle();
        battle.Initialize(this);

        keyMouse = new KeyMouse();
        keyMouse.Initialize(this);

        duriel = new Duriel();
        duriel.Initialize(this);

        pindleskin = new Pindleskin();
        pindleskin.Initialize(this);

        baalLeech = new BaalLeech();
        baalLeech.Initialize(this);

        baal = new Baal();
        baal.Initialize(this);

        travincal = new Travincal();
        travincal.Initialize(this);

        mephisto = new Mephisto();
        mephisto.Initialize(this);

        andariel = new Andariel();
        andariel.Initialize(this);

        countess = new Countess();
        countess.Initialize(this);

        mercStruc = new MercStruc();
        mercStruc.Initialize(this);

        stashStruc = new StashStruc();
        stashStruc.Initialize(this);

        cubing = new Cubing();
        cubing.Initialize(this);

        gamble = new Gamble();
        gamble.Initialize(this);

        lowerKurast = new LowerKurast();
        lowerKurast.Initialize(this);

        act3Sewers = new Act3Sewers();
        act3Sewers.Initialize(this);

        upperKurast = new UpperKurast();
        upperKurast.Initialize(this);

        settingsLoader = new SettingsLoader();
        settingsLoader.Initialize(this);

        mapAreaStruc = new MapAreaStruc();
        mapAreaStruc.Initialize(this);

        pathFinding = new PathFinding();
        pathFinding.Initialize(this);

        wpTaker = new WPTaker();
        wpTaker.Initialize(this);

        cows = new Cows();
        cows.Initialize(this);

        eldritch = new Eldritch();
        eldritch.Initialize(this);

        shenk = new Shenk();
        shenk.Initialize(this);

        nihlatak = new Nihlatak();
        nihlatak.Initialize(this);

        frozenstein = new Frozenstein();
        frozenstein.Initialize(this);

        terrorZones = new TerrorZones();
        terrorZones.Initialize(this);

        areaScript = new AreaScript();

        shopBot = new ShopBot();
        shopBot.Initialize(this);

        mausoleum = new Mausoleum();
        mausoleum.Initialize(this);

        crypt = new Crypt();
        crypt.Initialize(this);

        arachnidLair = new ArachnidLair();
        arachnidLair.Initialize(this);

        pit = new Pit();
        pit.Initialize(this);

        andarielRush = new AndarielRush();
        andarielRush.Initialize(this);

        darkWoodRush = new DarkWoodRush();
        darkWoodRush.Initialize(this);

        durielRush = new DurielRush();
        durielRush.Initialize(this);

        farOasisRush = new FarOasisRush();
        farOasisRush.Initialize(this);

        hallOfDeadRushCube = new HallOfDeadRushCube();
        hallOfDeadRushCube.Initialize(this);

        khalimBrainRush = new KhalimBrainRush();
        khalimBrainRush.Initialize(this);

        khalimEyeRush = new KhalimEyeRush();
        khalimEyeRush.Initialize(this);

        khalimHeartRush = new KhalimHeartRush();
        khalimHeartRush.Initialize(this);

        lostCityRush = new LostCityRush();
        lostCityRush.Initialize(this);

        mephistoRush = new MephistoRush();
        mephistoRush.Initialize(this);

        radamentRush = new RadamentRush();
        radamentRush.Initialize(this);

        summonerRush = new SummonerRush();
        summonerRush.Initialize(this);

        travincalRush = new TravincalRush();
        travincalRush.Initialize(this);

        tristramRush = new TristramRush();
        tristramRush.Initialize(this);

        ancientsRush = new AncientsRush();
        ancientsRush.Initialize(this);

        anyaRush = new AnyaRush();
        anyaRush.Initialize(this);

        chaosRush = new ChaosRush();
        chaosRush.Initialize(this);

        baalRush = new BaalRush();
        baalRush.Initialize(this);

        itemsViewer = new ItemsViewer();
        itemsViewer.Initialize(this);

        AllClassInstances = new List<object>();

        bots = new List<IBot> {
            darkWoodRush, tristramRush, andarielRush, radamentRush, hallOfDeadRushCube, farOasisRush, lostCityRush, summonerRush, durielRush,
            khalimBrainRush, khalimEyeRush, khalimHeartRush, travincalRush, mephistoRush, anyaRush, ancientsRush, chaosRush, baalRush,
            crypt, countess, andariel, duriel, arachnidLair, act3Sewers, chaos, baal, cows, terrorZones, eldritch, frozenstein,
            lowerKurast, mausoleum, mephisto, nihlatak, pindleskin, pit, shenk, shopBot, summoner, travincal, upperKurast, wpTaker
        };

        settingsLoader.LoadSettings();
        _isInitialized = true;
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
