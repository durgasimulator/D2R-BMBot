using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;

public class BaalLeech
{
    private GameData gameData;
    //##############
    //EXTRAS FEATURES
    public bool BaalLeechFight = false;
    //##############

    public int CurrentStep = 0;
    public int MaxGameTimeToEnter = CharConfig.MaxTimeEnterGame; //3mins
    public int MaxTimeWaitedForTP = (2 * 60); //2mins
    public int TimeWaitedForTP = 0;
    public bool PrintedInfos = false;
    public int SameGameRetry = 0;
    public bool SearchSameGamesAsLastOne = false;
    public bool KillingManually = false;
    public bool DetectedBaal = false;
    public bool ScriptDone { get; set; } = false;

    public long LastWave4Pointer = 0;
    public long LastWave5Pointer = 0;

    public List<uint> IgnoredTPList = new List<uint>();
    public List<long> IgnoredMobs = new List<long>();
    public uint LastUsedTP_ID = 0;
    public DateTime StartTimeWhenWaiting = DateTime.Now;
    public bool SetStartTime = false;
    public long BaalThronePointer = 0;
    public DateTime TimeSinceInThrone = DateTime.Now;

    public bool WaitedEnteringPortal = false;
    public bool AddingIgnoredTP_ID = false;

    public bool SetNoTPTime = false;
    public DateTime TimeSinceNoTP = DateTime.Now;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void RunScriptNOTInGame()
    {
        TimeWaitedForTP = 0;
        CurrentStep = 0;
        LastWave4Pointer = 0;
        LastWave5Pointer = 0;
        BaalThronePointer = 0;
        PrintedInfos = false;
        KillingManually = false;
        DetectedBaal = false;
        ScriptDone = false;
        SetStartTime = false;
        AddingIgnoredTP_ID = false;
        WaitedEnteringPortal = false;
        SetNoTPTime = false;
        TimeSinceNoTP = DateTime.Now;
        StartTimeWhenWaiting = DateTime.Now;
        gameData.playerScan.LeechPlayerUnitID = 0;
        gameData.playerScan.LeechPlayerPointer = 0;
        gameData.townStruc.ScriptTownAct = 5; //set to town act 5 when running this script
        gameData.gameStruc.GetAllGamesNames();
        IgnoredTPList = new List<uint>();
        LastUsedTP_ID = 0;
        bool EnteredGammme = false;

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
                    if (gameData.gameStruc.AllGamesNames[i].ToLower().Contains("baal")
                        && !gameData.gameStruc.IsIncludedInListString(CharConfig.BaalSearchAvoidWords, gameData.gameStruc.AllGamesNames[i].ToLower())
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
                        gameData.SetGameStatus("LOADING GAME");
                        //gameData.WaitDelay(300);
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
        SearchSameGamesAsLastOne = false;
        SameGameRetry = 0;
        gameData.townStruc.ScriptTownAct = 5; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        GetLeechInfo();
        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("WAITING TP");
            CurrentStep = 0;
            gameData.pathFinding.MoveToThisPos(new Position { X = 5103, Y = 5029 }); //move to tp spot

            if (AddingIgnoredTP_ID)
            {
                IgnoredTPList.Add(gameData.townStruc.LastUsedTPID);
                AddingIgnoredTP_ID = false;
            }

            if (!SetStartTime)
            {
                gameData.townStruc.GetCorpse();
                StartTimeWhenWaiting = DateTime.Now;
                gameData.battle.CastDefense();
                SetStartTime = true;
            }

            //use tp
            if (gameData.objectsStruc.GetObjects("TownPortal", true, IgnoredTPList, 999, "", (int)Enums.Area.ThroneOfDestruction))
            {
                if (!WaitedEnteringPortal)
                {
                    if (IsPortalAtGoodLocation())
                    {
                        gameData.battle.CastDefense();
                        gameData.WaitDelay(CharConfig.LeechEnterTPDelay);
                        WaitedEnteringPortal = true;
                    }
                    else
                    {
                        gameData.method_1("Added ignored TP ID(possible wrong area): " + gameData.objectsStruc.ObjectUnitID, Color.Red);
                        IgnoredTPList.Add(gameData.objectsStruc.ObjectUnitID);
                    }
                }

                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.WaitDelay(100);
                LastUsedTP_ID = gameData.objectsStruc.ObjectUnitID;

                gameData.uiScan.readUI();
                if (gameData.uiScan.tradeMenu) gameData.uiScan.CloseUIMenu("tradeMenu");
                if (gameData.uiScan.quitMenu) gameData.uiScan.CloseUIMenu("quitMenu");
                //gameData.mover.FinishMoving();
            }
            else
            {
                TimeWaitedForTP = 0;
                if (SetStartTime)
                {
                    TimeSpan CheckT = DateTime.Now - StartTimeWhenWaiting;
                    TimeWaitedForTP = (int)CheckT.TotalSeconds;
                }

                //we detected
                if (gameData.playerScan.HasAnyPlayerInArea((int)Enums.Area.ThroneOfDestruction))
                {
                    if (!SetNoTPTime)
                    {
                        TimeSinceNoTP = DateTime.Now;
                        SetNoTPTime = true;
                    }
                    if ((DateTime.Now - TimeSinceNoTP).TotalSeconds > 20)
                    {
                        gameData.method_1("People detected in Throne but TP is undetected!", Color.Red);
                        gameData.pathFinding.MoveToThisPos(new Position { X = 5103, Y = 5115 }); //move to anya to reset TP positions perhaps
                        gameData.pathFinding.MoveToThisPos(new Position { X = 5103, Y = 5029 }); //move back to tp spot
                        TimeSinceNoTP = DateTime.Now;
                    }
                }

                //gameData.method_1("NO TP FOUND NEAR IN TOWN");
                if (TimeWaitedForTP >= MaxTimeWaitedForTP)
                {
                    gameData.method_1("Leaving reason: Waited too long for tp", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    //gameData.LeaveGame(false);
                }
                /*else
                {
                    gameData.WaitDelay(450);
                    //TimeWaitedForTP++;
                }*/

                //check if we are about to do baal atleast when waiting
                if (TimeWaitedForTP >= (MaxTimeWaitedForTP / 3))
                {
                    if (!gameData.playerScan.HasAnyPlayerInArea(129)    //worldstone lvl2
                        && !gameData.playerScan.HasAnyPlayerInArea(130) //worldstone lvl3
                        && !gameData.playerScan.HasAnyPlayerInArea(131))//throne chamber
                    {
                        gameData.method_1("Leaving reason: Nobody seem to baal run", Color.Red);
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.UseLastTP = false;
                        ScriptDone = true;
                        //gameData.LeaveGame(false);
                    }
                }
            }
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.battle.CastDefense();
                //CastDefense();
                //CastDefense();
                //gameData.townStruc.GetCorpse();

                //not correct location check
                if (gameData.playerScan.levelNo != (int)Enums.Area.ThroneOfDestruction)
                {

                    if (gameData.objectsStruc.GetObjects("TownPortal", true, IgnoredTPList, 999, "", (int)Enums.Area.Harrogath))
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
                }
                else
                {
                    TimeSinceNoTP = DateTime.Now;
                    TimeSinceInThrone = DateTime.Now;
                    CurrentStep++;
                }
            }
            if (CurrentStep == 1)
            {
                gameData.SetGameStatus("LEECHING");
                gameData.playerScan.GetLeechPositions();
                TimeSinceNoTP = DateTime.Now;

                //gameData.battle.DoBattleScript();

                if (gameData.playerScan.xPosFinal < 15110 - 8
                    || gameData.playerScan.xPosFinal > 15110 + 8
                    || gameData.playerScan.yPosFinal < 5030 - 8
                    || gameData.playerScan.yPosFinal > 5030 + 8)
                {
                    //gameData.townStruc.GetCorpse();
                    gameData.mover.MoveToLocation(15110, 5030); //move to safe spot
                }
                else
                {
                    //gameData.townStruc.GetCorpse();
                    if (!BaalLeechFight) gameData.battle.DoBattleScript(10);
                    else gameData.battle.DoBattleScript(30);
                }

                //detect last wave
                /*if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Baal Subject 4", false, 99, IgnoredMobs))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.method_1("Wave4 detected, switching to baal script!", Color.OrangeRed);
                        LastWave4Pointer = gameData.mobsStruc.MobsPointerLocation;
                        CurrentStep++;
                    }
                }*/
                if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Baal Subject 5", false, 99, IgnoredMobs))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.method_1("Wave5 detected, switching to baal script!", Color.OrangeRed);
                        LastWave5Pointer = gameData.mobsStruc.MobsPointerLocation;
                        CurrentStep++;
                    }
                }
                //###
                /*if (BaalThronePointer == 0)
                {
                    if (gameData.mobsStruc.GetMobs("getBossName", "BaalThrone", false, 99, new List<long>()))
                    {
                        BaalThronePointer = gameData.mobsStruc.MobsPointerLocation;
                    }
                }
                else
                {
                    gameData.mobsStruc.GetThisMob(BaalThronePointer);

                    //BaalThrone has moved
                    if (gameData.mobsStruc.xPosFinal != 15087 &&
                        gameData.mobsStruc.yPosFinal != 5013)
                    {
                        gameData.method_1("Baal has moved, switching to baal script!", Color.Red);
                        CurrentStep++;
                    }
                }

                if (gameData.FPS == 0) //this is fixed now
                {
                    gameData.method_1("Too low FPS, switching to baal script!", Color.Red);
                    CurrentStep++;
                }*/
                //CurrentStep++;

                if (gameData.playerScan.HasAnyPlayerInArea((int)Enums.Area.TheWorldstoneChamber))
                {
                    gameData.method_1("People detected in Worldstone chamber, switching to baal script!", Color.OrangeRed);
                    CurrentStep++;
                }

                //Automaticly jump to next step (baal killing) after 2mins in throne
                TimeSpan ThisTimeCheckk = DateTime.Now - TimeSinceInThrone;
                if (ThisTimeCheckk.TotalMinutes > 2)
                {
                    gameData.method_1("More than 2min since in Throne, switching to baal script!", Color.OrangeRed);
                    CurrentStep++;
                }
            }

            if (CurrentStep == 2)
            {
                gameData.SetGameStatus("WAITING PORTAL");
                TimeSinceNoTP = DateTime.Now;

                //##### detect wave only, not increase script functions
                if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Baal Subject 5", false, 99, IgnoredMobs))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        LastWave5Pointer = gameData.mobsStruc.MobsPointerLocation;
                    }
                }
                //#####

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
            if (CurrentStep == 3)
            {
                gameData.SetGameStatus("MOVING TO BAAL");
                TimeSinceNoTP = DateTime.Now;
                gameData.pathFinding.MoveToThisPos(new Position { X = 15134, Y = 5927 });
                gameData.WaitDelay(50); //wait a bit to detect baal
                CurrentStep++;
                //in throne, move close to baal
                /*if (gameData.mover.MoveToLocation(15166, 5934))
                {
                    if (gameData.mover.MoveToLocation(15140, 5940))
                    {
                        CurrentStep++;
                    }
                }*/
            }
            if (CurrentStep == 4)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING BAAL");
                TimeSinceNoTP = DateTime.Now;
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
                        IgnoredMobs = new List<long>();
                        if (LastWave4Pointer != 0) IgnoredMobs.Add(LastWave4Pointer); //add this killed wave4 to ignoed mob
                        if (LastWave5Pointer != 0) IgnoredMobs.Add(LastWave5Pointer); //add this killed wave5 to ignoed mob
                        if (DetectedBaal) IgnoredMobs.Add(gameData.mobsStruc.MobsPointerLocation); //add this killed baal to ignoed mob

                        if (DetectedBaal && KillingManually)
                        {
                            gameData.method_1("Killed Baal Manually!", Color.DarkMagenta);
                        }

                        SearchSameGamesAsLastOne = true;
                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    }
                }
                else
                {
                    gameData.method_1("Baal not detected!", Color.Red);

                    for (int i = 0; i < 60; i++)
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
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Baal", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;
                    SearchSameGamesAsLastOne = true;
                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                }
            }
        }
    }

    public bool IsPortalAtGoodLocation()
    {
        //Check for all roster member (party peoples) and see if they are in worldstone area

        bool IsCorrectLocation = false;
        string StartLeechName = gameData.gameStruc.GameOwnerName;
        if (CharConfig.SearchLeecherName != "") StartLeechName = CharConfig.SearchLeecherName;

        try
        {
            for (int i = 0; i < gameData.gameStruc.AllPlayersNames.Count; i++)
            {
                gameData.gameStruc.GameOwnerName = gameData.gameStruc.AllPlayersNames[i];
                GetLeechInfo();
                if (gameData.playerScan.LeechPlayerPointer == 0 || gameData.playerScan.LeechPlayerUnitID == 0) continue;
                gameData.playerScan.GetLeechPositions();

                if (!gameData.Running || !gameData.gameStruc.IsInGame())
                {
                    IsCorrectLocation = false;
                    break;
                }

                if (gameData.playerScan.LeechlevelNo == 131)
                {
                    IsCorrectLocation = true;
                    break;
                }
            }
        }
        catch
        {
            IsCorrectLocation = true;
        }

        gameData.gameStruc.GameOwnerName = StartLeechName;
        if (CharConfig.SearchLeecherName != "") gameData.gameStruc.GameOwnerName = CharConfig.SearchLeecherName;

        return IsCorrectLocation;
    }

    public void GetLeechInfo()
    {
        gameData.playerScan.ScanForLeecher();

        if (!PrintedInfos)
        {
            gameData.method_1("Leecher name: " + gameData.gameStruc.GameOwnerName, Color.DarkViolet);
            //gameData.method_1("Leecher pointer: 0x" + gameData.playerScan.LeechPlayerPointer.ToString("X"), Color.DarkViolet);
            //gameData.method_1("Leecher unitID: 0x" + gameData.playerScan.LeechPlayerUnitID.ToString("X"), Color.DarkViolet);
            PrintedInfos = true;
        }

        //LEECHER NOT IN GAME
        if (gameData.playerScan.LeechPlayerPointer == 0 || gameData.playerScan.LeechPlayerUnitID == 0)
        {
            if (CurrentStep < 2) //kill baal manually
            {
                gameData.itemsStruc.GrabAllItemsForGold();
                SearchSameGamesAsLastOne = true;
                gameData.LeaveGame(true);
                ScriptDone = true;
                //gameData.LeaveGame(false);
            }
            else
            {
                if (!KillingManually)
                {
                    //no chance to go alone
                    if (!gameData.mercStruc.MercAlive
                        && gameData.beltStruc.HPQuantity < 6
                        && gameData.beltStruc.ManyQuantity < 3)
                    {
                        gameData.itemsStruc.GrabAllItemsForGold();
                        SearchSameGamesAsLastOne = true;
                        gameData.LeaveGame(true);
                        ScriptDone = true;
                        //gameData.LeaveGame(false);
                    }
                    else
                    {
                        gameData.itemsStruc.GrabAllItemsForGold();
                        SearchSameGamesAsLastOne = true;
                        gameData.LeaveGame(true);
                        ScriptDone = true;
                        //gameData.LeaveGame(false);

                        //KillingManually = true;
                    }
                }
            }
        }
    }
}
