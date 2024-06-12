using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class WPTaker : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public int TriedWPCount = 0;
    public bool HasThisWP = true;
    public Enums.Area DoingThisArea = 0;

    public int CurrentAct = 1;
    public int CurrentWPIndex = 1;
    public bool AdancedIndex = false;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public void ResetVars()
    {
        TriedWPCount = 0;
        CurrentStep = 0;
        ScriptDone = false;
    }

    public void DetectCurrentStep()
    {
        /*if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.LostCity) CurrentStep = 1;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ValleyOfSnakes) CurrentStep = 2;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ClawViperTempleLevel1) CurrentStep = 3;
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ClawViperTempleLevel2) CurrentStep = 4;*/
    }

    public void AdvanceScriptIndex()
    {
        //advance script by 1x index
        if ((CurrentWPIndex < 8 && CurrentAct != 4)
            || (CurrentWPIndex < 2 && CurrentAct == 4))
        {
            CurrentWPIndex++;
        }
        else
        {
            CurrentWPIndex = 1;

            //Go to next Act
            if (CurrentAct < 5)
            {
                CurrentAct++;
            }
            else
            {
                gameData.townStruc.FastTowning = false;
                gameData.townStruc.UseLastTP = false;
                ScriptDone = true;
            }
        }
    }

    public void RunScript()
    {
        gameData.townStruc.ScriptTownAct = CurrentAct;

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;
            AdancedIndex = false;

            if (!HasThisWP)
            {
                gameData.townStruc.GoToWPArea(CurrentAct, CurrentWPIndex);

                TriedWPCount++;
                if (TriedWPCount >= 3)
                {
                    HasThisWP = false;
                    if (CurrentAct == 1 && CurrentWPIndex == 1) DoingThisArea = Enums.Area.ColdPlains;
                    if (CurrentAct == 1 && CurrentWPIndex == 2) DoingThisArea = Enums.Area.StonyField;
                    if (CurrentAct == 1 && CurrentWPIndex == 3) DoingThisArea = Enums.Area.DarkWood;
                    if (CurrentAct == 1 && CurrentWPIndex == 4) DoingThisArea = Enums.Area.BlackMarsh;
                    if (CurrentAct == 1 && CurrentWPIndex == 5) DoingThisArea = Enums.Area.OuterCloister;
                    if (CurrentAct == 1 && CurrentWPIndex == 6) DoingThisArea = Enums.Area.JailLevel1;
                    if (CurrentAct == 1 && CurrentWPIndex == 7) DoingThisArea = Enums.Area.InnerCloister;
                    if (CurrentAct == 1 && CurrentWPIndex == 8) DoingThisArea = Enums.Area.CatacombsLevel2;

                    if (CurrentAct == 2 && CurrentWPIndex == 1) DoingThisArea = Enums.Area.SewersLevel2Act2;
                    if (CurrentAct == 2 && CurrentWPIndex == 2) DoingThisArea = Enums.Area.DryHills;
                    if (CurrentAct == 2 && CurrentWPIndex == 3) DoingThisArea = Enums.Area.HallsOfTheDeadLevel2;
                    if (CurrentAct == 2 && CurrentWPIndex == 4) DoingThisArea = Enums.Area.FarOasis;
                    if (CurrentAct == 2 && CurrentWPIndex == 5) DoingThisArea = Enums.Area.LostCity;
                    if (CurrentAct == 2 && CurrentWPIndex == 6) DoingThisArea = Enums.Area.PalaceCellarLevel1;  //require cube, staff, ammy
                    if (CurrentAct == 2 && CurrentWPIndex == 7) DoingThisArea = Enums.Area.ArcaneSanctuary;
                    if (CurrentAct == 2 && CurrentWPIndex == 8) DoingThisArea = Enums.Area.CanyonOfTheMagi;     //require summoner defeated

                    if (CurrentAct == 3 && CurrentWPIndex == 1) DoingThisArea = Enums.Area.SpiderForest;
                    if (CurrentAct == 3 && CurrentWPIndex == 2) DoingThisArea = Enums.Area.GreatMarsh;
                    if (CurrentAct == 3 && CurrentWPIndex == 3) DoingThisArea = Enums.Area.FlayerJungle;
                    if (CurrentAct == 3 && CurrentWPIndex == 4) DoingThisArea = Enums.Area.LowerKurast;
                    if (CurrentAct == 3 && CurrentWPIndex == 5) DoingThisArea = Enums.Area.KurastBazaar;
                    if (CurrentAct == 3 && CurrentWPIndex == 6) DoingThisArea = Enums.Area.UpperKurast;
                    if (CurrentAct == 3 && CurrentWPIndex == 7) DoingThisArea = Enums.Area.Travincal;
                    if (CurrentAct == 3 && CurrentWPIndex == 8) DoingThisArea = Enums.Area.DuranceOfHateLevel2; //require kahlim flail

                    if (CurrentAct == 4 && CurrentWPIndex == 1) DoingThisArea = Enums.Area.CityOfTheDamned;
                    if (CurrentAct == 4 && CurrentWPIndex == 2) DoingThisArea = Enums.Area.RiverOfFlame;

                    if (CurrentAct == 5 && CurrentWPIndex == 1) DoingThisArea = Enums.Area.FrigidHighlands;
                    if (CurrentAct == 5 && CurrentWPIndex == 2) DoingThisArea = Enums.Area.ArreatPlateau;
                    if (CurrentAct == 5 && CurrentWPIndex == 3) DoingThisArea = Enums.Area.CrystallinePassage;
                    if (CurrentAct == 5 && CurrentWPIndex == 4) DoingThisArea = Enums.Area.HallsOfPain;         //Require Anya saved
                    if (CurrentAct == 5 && CurrentWPIndex == 5) DoingThisArea = Enums.Area.GlacialTrail;
                    if (CurrentAct == 5 && CurrentWPIndex == 6) DoingThisArea = Enums.Area.FrozenTundra;
                    if (CurrentAct == 5 && CurrentWPIndex == 7) DoingThisArea = Enums.Area.TheAncientsWay;
                    if (CurrentAct == 5 && CurrentWPIndex == 8) DoingThisArea = Enums.Area.TheWorldStoneKeepLevel2;

                    TriedWPCount = 0;
                }
            }
            else
            {
                if (HasThisWP)
                {
                    if (!AdancedIndex)
                    {
                        AdvanceScriptIndex();
                        AdancedIndex = true;
                    }

                    Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "WaypointPortal", (int)DoingThisArea, new List<int>() { });
                    if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
                    {
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                        if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                        {
                            gameData.townStruc.SelectTownWP();
                        }
                    }
                    else
                    {
                        gameData.method_1("No WP found nearby", Color.OrangeRed);
                    }
                }

                if (CurrentWPIndex == 1)
                {
                    //start from town
                    if (CurrentAct == 1) gameData.pathFinding.MoveToNextArea(Enums.Area.BloodMoor);
                    if (CurrentAct == 2) gameData.pathFinding.MoveToExit(Enums.Area.SewersLevel1Act2);
                    if (CurrentAct == 3) gameData.pathFinding.MoveToNextArea(Enums.Area.SpiderForest);
                    if (CurrentAct == 4) gameData.pathFinding.MoveToNextArea(Enums.Area.OuterSteppes);
                    if (CurrentAct == 5) gameData.pathFinding.MoveToNextArea(Enums.Area.BloodyFoothills);
                }
                else
                {
                    if (CurrentAct == 2 && CurrentWPIndex == 2) gameData.pathFinding.MoveToNextArea(Enums.Area.RockyWaste);
                    else if (CurrentAct == 2 && CurrentWPIndex == 6) gameData.pathFinding.MoveToExit(Enums.Area.HaremLevel1);
                    else if (CurrentAct == 5 && CurrentWPIndex == 5) gameData.pathFinding.MoveToExit(Enums.Area.NihlathaksTemple);
                    else
                    {
                        //start from the previous wp area
                        gameData.townStruc.GoToWPArea(CurrentAct, CurrentWPIndex - 1);
                    }
                }
            }
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING WP TAKER FOR: " + DoingThisArea);
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                //Act1
                if (CurrentAct == 1 && CurrentWPIndex == 1)
                {
                    gameData.pathFinding.MoveToNextArea(Enums.Area.BloodMoor);
                    gameData.pathFinding.MoveToNextArea(Enums.Area.ColdPlains);
                }
                if (CurrentAct == 1 && CurrentWPIndex == 2) gameData.pathFinding.MoveToNextArea(Enums.Area.StonyField);
                if (CurrentAct == 1 && CurrentWPIndex == 3)
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.UndergroundPassageLevel1);
                    gameData.pathFinding.MoveToExit(Enums.Area.DarkWood);
                }
                if (CurrentAct == 1 && CurrentWPIndex == 4) gameData.pathFinding.MoveToNextArea(Enums.Area.BlackMarsh);
                if (CurrentAct == 1 && CurrentWPIndex == 5)
                {
                    gameData.pathFinding.MoveToNextArea(Enums.Area.TamoeHighland);
                    gameData.pathFinding.MoveToNextArea(Enums.Area.MonasteryGate);
                    gameData.pathFinding.MoveToNextArea(Enums.Area.OuterCloister);
                }
                if (CurrentAct == 1 && CurrentWPIndex == 6) gameData.pathFinding.MoveToExit(Enums.Area.JailLevel1);
                if (CurrentAct == 1 && CurrentWPIndex == 7)
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.JailLevel2);
                    gameData.pathFinding.MoveToExit(Enums.Area.JailLevel3);
                    gameData.pathFinding.MoveToExit(Enums.Area.InnerCloister);
                }
                if (CurrentAct == 1 && CurrentWPIndex == 8)
                {
                    gameData.pathFinding.MoveToNextArea(Enums.Area.Cathedral);
                    gameData.pathFinding.MoveToExit(Enums.Area.CatacombsLevel1);
                    gameData.pathFinding.MoveToExit(Enums.Area.CatacombsLevel2);
                }

                //Act2
                if (CurrentAct == 2 && CurrentWPIndex == 1) gameData.pathFinding.MoveToExit(Enums.Area.SewersLevel2Act2);
                if (CurrentAct == 2 && CurrentWPIndex == 2) gameData.pathFinding.MoveToNextArea(Enums.Area.DryHills);
                if (CurrentAct == 2 && CurrentWPIndex == 3)
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.HallsOfTheDeadLevel1);
                    gameData.pathFinding.MoveToExit(Enums.Area.HallsOfTheDeadLevel2);
                }
                if (CurrentAct == 2 && CurrentWPIndex == 4) gameData.pathFinding.MoveToNextArea(Enums.Area.FarOasis);
                if (CurrentAct == 2 && CurrentWPIndex == 5) gameData.pathFinding.MoveToNextArea(Enums.Area.LostCity);
                if (CurrentAct == 2 && CurrentWPIndex == 6)
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.HaremLevel2);
                    gameData.pathFinding.MoveToExit(Enums.Area.PalaceCellarLevel1);
                }
                if (CurrentAct == 2 && CurrentWPIndex == 7)
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.PalaceCellarLevel2);
                    gameData.pathFinding.MoveToExit(Enums.Area.PalaceCellarLevel3);
                    gameData.pathFinding.MoveToExit(Enums.Area.ArcaneSanctuary);
                }
                if (CurrentAct == 2 && CurrentWPIndex == 8) gameData.pathFinding.MoveToExit(Enums.Area.CanyonOfTheMagi);

                //Act3
                //if (CurrentAct == 3 && CurrentWPIndex == 1) gameData.pathFinding.MoveToNextArea(Enums.Area.SpiderForest);
                if (CurrentAct == 3 && CurrentWPIndex == 2) gameData.pathFinding.MoveToNextArea(Enums.Area.GreatMarsh);
                if (CurrentAct == 3 && CurrentWPIndex == 3) gameData.pathFinding.MoveToNextArea(Enums.Area.FlayerJungle);
                if (CurrentAct == 3 && CurrentWPIndex == 4) gameData.pathFinding.MoveToNextArea(Enums.Area.LowerKurast);
                if (CurrentAct == 3 && CurrentWPIndex == 5) gameData.pathFinding.MoveToNextArea(Enums.Area.KurastBazaar);
                if (CurrentAct == 3 && CurrentWPIndex == 6) gameData.pathFinding.MoveToNextArea(Enums.Area.UpperKurast);
                if (CurrentAct == 3 && CurrentWPIndex == 7)
                {
                    gameData.pathFinding.MoveToNextArea(Enums.Area.KurastCauseway);
                    gameData.pathFinding.MoveToNextArea(Enums.Area.Travincal);
                }
                if (CurrentAct == 3 && CurrentWPIndex == 8)
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.DuranceOfHateLevel1);
                    gameData.pathFinding.MoveToExit(Enums.Area.DuranceOfHateLevel2);
                }

                //Act4
                if (CurrentAct == 4 && CurrentWPIndex == 1)
                {
                    gameData.pathFinding.MoveToNextArea(Enums.Area.PlainsOfDespair);
                    gameData.pathFinding.MoveToNextArea(Enums.Area.CityOfTheDamned);
                }
                if (CurrentAct == 4 && CurrentWPIndex == 2) gameData.pathFinding.MoveToExit(Enums.Area.RiverOfFlame);

                //Act5
                if (CurrentAct == 5 && CurrentWPIndex == 1) gameData.pathFinding.MoveToNextArea(Enums.Area.FrigidHighlands);
                if (CurrentAct == 5 && CurrentWPIndex == 2) gameData.pathFinding.MoveToNextArea(Enums.Area.ArreatPlateau);
                if (CurrentAct == 5 && CurrentWPIndex == 3) gameData.pathFinding.MoveToExit(Enums.Area.CrystallinePassage);
                if (CurrentAct == 5 && CurrentWPIndex == 4) gameData.pathFinding.MoveToExit(Enums.Area.HallsOfPain);
                if (CurrentAct == 5 && CurrentWPIndex == 5) gameData.pathFinding.MoveToExit(Enums.Area.GlacialTrail);
                if (CurrentAct == 5 && CurrentWPIndex == 6) gameData.pathFinding.MoveToExit(Enums.Area.FrozenTundra);
                if (CurrentAct == 5 && CurrentWPIndex == 7) gameData.pathFinding.MoveToExit(Enums.Area.TheAncientsWay);
                if (CurrentAct == 5 && CurrentWPIndex == 8)
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.ArreatSummit);
                    gameData.pathFinding.MoveToExit(Enums.Area.TheWorldStoneKeepLevel1);
                    gameData.pathFinding.MoveToExit(Enums.Area.TheWorldStoneKeepLevel2);
                }

                CurrentStep++;
            }

            if (CurrentStep == 1)
            {
                gameData.pathFinding.MoveToObject("WaypointPortal");

                Position ThisFinalPosition = gameData.mapAreaStruc.GetPositionOfObject("object", "WaypointPortal", (int)DoingThisArea, new List<int>() { });
                if (ThisFinalPosition.X != 0 && ThisFinalPosition.Y != 0)
                {
                    Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisFinalPosition.X, ThisFinalPosition.Y);

                    gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                    if (gameData.uiScan.WaitTilUIOpen("waypointMenu"))
                    {
                        gameData.townStruc.SelectTownWP();
                        CurrentStep++;
                    }
                }
                else
                {
                    gameData.method_1("No WP found nearby", Color.OrangeRed);
                }
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                if (!gameData.townStruc.GetInTown())
                {
                    CurrentStep--;
                    return;
                }

                AdvanceScriptIndex();
            }
        }
    }
}
