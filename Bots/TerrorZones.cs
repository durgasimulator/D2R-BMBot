﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;
using static Enums;

public class TerrorZones : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;

    public List<Area> TerrorZonesAreas = new List<Area>();
    public int CurrentTerrorZonesIndex = 0;

    public List<int> IgnoredChestList = new List<int>();
    public bool HasTakenAnyChest = false;

    public bool DoneChestsStep = false;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }


    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
        TerrorZonesAreas = new List<Area>();
        CurrentTerrorZonesIndex = 0;
        IgnoredChestList = new List<int>();
        HasTakenAnyChest = false;
        DoneChestsStep = false;
    }

    public void RunScript()
    {
        if (TerrorZonesAreas.Count == 0) TerrorZonesAreas = gameData.gameStruc.GetTerrorZones();
        if (TerrorZonesAreas.Count == 0)
        {
            gameData.method_1("No Terror Zones Detected!", Color.Red);
            ScriptDone = true;
            return;
        }

        gameData.townStruc.ScriptTownAct = gameData.areaScript.GetActFromArea(TerrorZonesAreas[CurrentTerrorZonesIndex]); //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;
            DoneChestsStep = false;

            //Console.WriteLine(TerrorZonesAreas[CurrentTerrorZonesIndex]);

            if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.BloodMoor) gameData.pathFinding.MoveToNextArea(Enums.Area.BloodMoor);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.SewersLevel1Act2) gameData.pathFinding.MoveToNextArea(Enums.Area.SewersLevel1Act2);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.OuterSteppes) gameData.pathFinding.MoveToNextArea(Enums.Area.OuterSteppes);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.BloodyFoothills) gameData.pathFinding.MoveToNextArea(Enums.Area.BloodyFoothills);
            //######################
            //ACT 1
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.DenOfEvil)
            {
                gameData.pathFinding.MoveToNextArea(Enums.Area.BloodMoor);
                gameData.pathFinding.MoveToExit(Enums.Area.DenOfEvil);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.ColdPlains) gameData.townStruc.GoToWPArea(1, 1);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.CaveLevel1)
            {
                gameData.townStruc.GoToWPArea(1, 1);
                gameData.pathFinding.MoveToExit(Enums.Area.CaveLevel1);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.CaveLevel2)
            {
                gameData.townStruc.GoToWPArea(1, 1);
                gameData.pathFinding.MoveToExit(Enums.Area.CaveLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.CaveLevel2);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.StonyField) gameData.townStruc.GoToWPArea(1, 2);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.DarkWood) gameData.townStruc.GoToWPArea(1, 3);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.UndergroundPassageLevel1)
            {
                gameData.townStruc.GoToWPArea(1, 3);
                gameData.pathFinding.MoveToExit(Enums.Area.UndergroundPassageLevel1);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.UndergroundPassageLevel2)
            {
                gameData.townStruc.GoToWPArea(1, 3);
                gameData.pathFinding.MoveToExit(Enums.Area.UndergroundPassageLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.UndergroundPassageLevel2);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.BlackMarsh) gameData.townStruc.GoToWPArea(1, 4);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.HoleLevel1)
            {
                gameData.townStruc.GoToWPArea(1, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.HoleLevel1);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.HoleLevel2)
            {
                gameData.townStruc.GoToWPArea(1, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.HoleLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.HoleLevel2);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.ForgottenTower)
            {
                gameData.townStruc.GoToWPArea(1, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.ForgottenTower);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.TowerCellarLevel1)
            {
                gameData.townStruc.GoToWPArea(1, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.ForgottenTower);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel1);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.TowerCellarLevel2)
            {
                gameData.townStruc.GoToWPArea(1, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.ForgottenTower);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel2);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.TowerCellarLevel3)
            {
                gameData.townStruc.GoToWPArea(1, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.ForgottenTower);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel2);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel3);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.TowerCellarLevel4)
            {
                gameData.townStruc.GoToWPArea(1, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.ForgottenTower);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel2);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel3);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel4);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.TowerCellarLevel5)
            {
                gameData.townStruc.GoToWPArea(1, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.ForgottenTower);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel2);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel3);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel4);
                gameData.pathFinding.MoveToExit(Enums.Area.TowerCellarLevel5);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.Barracks)
            {
                gameData.townStruc.GoToWPArea(1, 5);
                gameData.pathFinding.MoveToNextArea(Enums.Area.Barracks);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.JailLevel1) gameData.townStruc.GoToWPArea(1, 6);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.JailLevel2)
            {
                gameData.townStruc.GoToWPArea(1, 6);
                gameData.pathFinding.MoveToExit(Enums.Area.JailLevel2);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.JailLevel3)
            {
                gameData.townStruc.GoToWPArea(1, 6);
                gameData.pathFinding.MoveToExit(Enums.Area.JailLevel2);
                gameData.pathFinding.MoveToExit(Enums.Area.JailLevel3);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.Cathedral)
            {
                gameData.townStruc.GoToWPArea(1, 7);
                gameData.pathFinding.MoveToNextArea(Enums.Area.Cathedral);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.InnerCloister) gameData.townStruc.GoToWPArea(1, 7);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.CatacombsLevel1)
            {
                gameData.townStruc.GoToWPArea(1, 7);
                gameData.pathFinding.MoveToNextArea(Enums.Area.Cathedral);
                gameData.pathFinding.MoveToNextArea(Enums.Area.CatacombsLevel1);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.CatacombsLevel2) gameData.townStruc.GoToWPArea(1, 8);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.CatacombsLevel3)
            {
                gameData.townStruc.GoToWPArea(1, 8);
                gameData.pathFinding.MoveToExit(Enums.Area.CatacombsLevel3);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.CatacombsLevel4)
            {
                gameData.townStruc.GoToWPArea(1, 8);
                gameData.pathFinding.MoveToExit(Enums.Area.CatacombsLevel3);
                gameData.pathFinding.MoveToExit(Enums.Area.CatacombsLevel4);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.BurialGrounds)
            {
                gameData.townStruc.GoToWPArea(1, 1);
                gameData.pathFinding.MoveToNextArea(Enums.Area.BurialGrounds);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.Crypt)
            {
                gameData.townStruc.GoToWPArea(1, 1);
                gameData.pathFinding.MoveToNextArea(Enums.Area.BurialGrounds);
                gameData.pathFinding.MoveToExit(Enums.Area.Crypt);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.Mausoleum)
            {
                gameData.townStruc.GoToWPArea(1, 1);
                gameData.pathFinding.MoveToNextArea(Enums.Area.BurialGrounds);
                gameData.pathFinding.MoveToExit(Enums.Area.Mausoleum);
            }
            //######################
            //ACT 2
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.SewersLevel2Act2) gameData.townStruc.GoToWPArea(2, 1);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.SewersLevel3Act2)
            {
                gameData.townStruc.GoToWPArea(2, 1);
                gameData.pathFinding.MoveToExit(Enums.Area.SewersLevel3Act2);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.DryHills) gameData.townStruc.GoToWPArea(2, 2);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.HallsOfTheDeadLevel1)
            {
                gameData.townStruc.GoToWPArea(2, 2);
                gameData.pathFinding.MoveToExit(Enums.Area.HallsOfTheDeadLevel1);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.HallsOfTheDeadLevel2) gameData.townStruc.GoToWPArea(2, 3);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.HallsOfTheDeadLevel3)
            {
                gameData.townStruc.GoToWPArea(2, 3);
                gameData.pathFinding.MoveToExit(Enums.Area.HallsOfTheDeadLevel3);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.FarOasis) gameData.townStruc.GoToWPArea(2, 4);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.LostCity) gameData.townStruc.GoToWPArea(2, 5);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.ValleyOfSnakes)
            {
                gameData.townStruc.GoToWPArea(2, 5);
                gameData.pathFinding.MoveToNextArea(Enums.Area.ValleyOfSnakes);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.ClawViperTempleLevel1)
            {
                gameData.townStruc.GoToWPArea(2, 5);
                gameData.pathFinding.MoveToNextArea(Enums.Area.ValleyOfSnakes);
                gameData.pathFinding.MoveToExit(Enums.Area.ClawViperTempleLevel1);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.ClawViperTempleLevel2)
            {
                gameData.townStruc.GoToWPArea(2, 5);
                gameData.pathFinding.MoveToNextArea(Enums.Area.ValleyOfSnakes);
                gameData.pathFinding.MoveToExit(Enums.Area.ClawViperTempleLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.ClawViperTempleLevel2);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.ArcaneSanctuary) gameData.townStruc.GoToWPArea(2, 7);
            //######################
            //ACT 3
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.SpiderForest) gameData.townStruc.GoToWPArea(3, 1);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.SpiderCavern)
            {
                gameData.townStruc.GoToWPArea(3, 1);
                gameData.pathFinding.MoveToExit(Enums.Area.SpiderCavern);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.GreatMarsh) gameData.townStruc.GoToWPArea(3, 2);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.FlayerJungle) gameData.townStruc.GoToWPArea(3, 3);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.FlayerDungeonLevel1)
            {
                gameData.townStruc.GoToWPArea(3, 3);
                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel1);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.FlayerDungeonLevel2)
            {
                gameData.townStruc.GoToWPArea(3, 3);
                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel2);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.FlayerDungeonLevel3)
            {
                gameData.townStruc.GoToWPArea(3, 3);
                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel1);
                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel2);
                gameData.pathFinding.MoveToExit(Enums.Area.FlayerDungeonLevel3);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.FlayerJungle) gameData.townStruc.GoToWPArea(3, 5);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.KurastBazaar) gameData.townStruc.GoToWPArea(3, 5);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.RuinedTemple)
            {
                gameData.townStruc.GoToWPArea(3, 5);
                gameData.pathFinding.MoveToExit(Enums.Area.RuinedTemple);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.DisusedFane)
            {
                gameData.townStruc.GoToWPArea(3, 5);
                gameData.pathFinding.MoveToExit(Enums.Area.RuinedTemple);
                gameData.pathFinding.MoveToExit(Enums.Area.DisusedFane);
            }
            //######################
            //ACT 4
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.PlainsOfDespair)
            {
                gameData.pathFinding.MoveToNextArea(Enums.Area.OuterSteppes);
                gameData.pathFinding.MoveToNextArea(Enums.Area.PlainsOfDespair);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.RiverOfFlame) gameData.townStruc.GoToWPArea(4, 2);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.CityOfTheDamned) gameData.townStruc.GoToWPArea(4, 1);

            //######################
            //ACT 5
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.FrigidHighlands) gameData.townStruc.GoToWPArea(5, 1);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.AbAddon)
            {
                gameData.townStruc.GoToWPArea(5, 1);
                gameData.pathFinding.MoveToExit(Enums.Area.AbAddon);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.GlacialTrail) gameData.townStruc.GoToWPArea(5, 4);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.DrifterCavern)
            {
                gameData.townStruc.GoToWPArea(5, 4);
                gameData.pathFinding.MoveToExit(Enums.Area.DrifterCavern);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.CrystallinePassage) gameData.townStruc.GoToWPArea(5, 3);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.FrozenRiver)
            {
                gameData.townStruc.GoToWPArea(5, 3);
                gameData.pathFinding.MoveToExit(Enums.Area.FrozenRiver);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.ArreatPlateau) gameData.townStruc.GoToWPArea(5, 2);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.PitOfAcheron)
            {
                gameData.townStruc.GoToWPArea(5, 2);
                gameData.pathFinding.MoveToExit(Enums.Area.PitOfAcheron);
            }
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.TheAncientsWay) gameData.townStruc.GoToWPArea(5, 7);
            else if (TerrorZonesAreas[CurrentTerrorZonesIndex] == Area.IcyCellar)
            {
                //gameData.townStruc.GoToWPArea(5, 6);
                gameData.townStruc.GoToWPArea(5, 7);
                gameData.pathFinding.MoveToExit(Enums.Area.IcyCellar);
            }

        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING TERROR ZONES");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == TerrorZonesAreas[CurrentTerrorZonesIndex])
                {
                    CurrentStep++;
                }
                else
                {
                    if (CurrentStep == 0)
                    {
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.GoToTown();
                    }
                }
            }

            if (CurrentStep == 1)
            {
                if (!DoneChestsStep)
                {
                    TakeChest((int) gameData.playerScan.levelNo);
                    DoneChestsStep = true;
                }
                if ((Enums.Area)gameData.battle.AreaIDFullyCleared != TerrorZonesAreas[CurrentTerrorZonesIndex])
                {
                    gameData.battle.ClearFullAreaOfMobs();

                    if (!gameData.battle.ClearingArea)
                    {
                        CurrentTerrorZonesIndex++;
                        if (CurrentTerrorZonesIndex > TerrorZonesAreas.Count - 1)
                        {
                            gameData.townStruc.FastTowning = false;
                            gameData.townStruc.UseLastTP = false;
                            ScriptDone = true;
                        }
                        else
                        {
                            CurrentStep = 0;
                            gameData.townStruc.FastTowning = false;
                            gameData.townStruc.GoToTown();
                            //gameData.pathFinding.MoveToExit(TerrorZonesAreas[CurrentTerrorZonesIndex], 4, true);
                        }
                    }
                }
                else
                {
                    CurrentTerrorZonesIndex++;
                    if (CurrentTerrorZonesIndex > TerrorZonesAreas.Count - 1)
                    {
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.UseLastTP = false;
                        ScriptDone = true;
                    }
                    else
                    {
                        CurrentStep = 0;
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.GoToTown();
                        //gameData.pathFinding.MoveToExit(TerrorZonesAreas[CurrentTerrorZonesIndex], 4, true);
                    }
                }
            }

        }
    }


    public void TakeChest(int ThisAreaa)
    {
        //JungleStashObject2
        //JungleStashObject3
        //GoodChest
        //NotSoGoodChest
        //DeadVillager1
        //DeadVillager2
        //NotSoGoodChest
        //HollowLog

        //JungleMediumChestLeft ####

        MapAreaStruc.Position ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", ThisAreaa, IgnoredChestList);
        int ChestObject = gameData.mapAreaStruc.CurrentObjectIndex;
        int Tryy = 0;
        while (ThisChestPos.X != 0 && ThisChestPos.Y != 0 && Tryy < 30)
        {
            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                ScriptDone = true;
                return;
            }

            if (gameData.mover.MoveToLocation(ThisChestPos.X, ThisChestPos.Y))
            {
                HasTakenAnyChest = true;

                Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisChestPos.X, ThisChestPos.Y);

                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.WaitDelay(10);
                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.WaitDelay(10);
                gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                gameData.WaitDelay(10);

                int tryy2 = 0;
                while (gameData.itemsStruc.GetItems(true) && tryy2 < 20)
                {
                    gameData.playerScan.GetPositions();
                    gameData.itemsStruc.GetItems(false);
                    gameData.potions.CheckIfWeUsePotion();
                    tryy2++;
                }
                IgnoredChestList.Add(ChestObject);
            }

            ThisChestPos = gameData.mapAreaStruc.GetPositionOfObject("object", "GoodChest", ThisAreaa, IgnoredChestList);
            ChestObject = gameData.mapAreaStruc.CurrentObjectIndex;

            Tryy++;
        }

        if (!HasTakenAnyChest) gameData.mapAreaStruc.DumpMap();
    }




    /*public List<List<Area>> tzAreaChain(Area firstTZ)
    {
        switch (firstTZ)
        {
            // Act 1
            case Area.BloodMoor:
                return new List<List<Area>> { new List<Area> { Area.RogueEncampment, Area.BloodMoor, Area.DenOfEvil } };
            case Area.ColdPlains:
                return new List<List<Area>> { new List<Area> { Area.ColdPlains, Area.CaveLevel1, Area.CaveLevel2 } };
            case Area.BurialGrounds:
                return new List<List<Area>>
                {
                    new List<Area> { Area.ColdPlains, Area.BurialGrounds, Area.Crypt },
                    new List<Area> { Area.ColdPlains, Area.BurialGrounds, Area.Mausoleum }
                };
            case Area.StonyField:
                return new List<List<Area>> { new List<Area> { Area.StonyField } };
            case Area.DarkWood:
                return new List<List<Area>> { new List<Area> { Area.DarkWood, Area.UndergroundPassageLevel1, Area.UndergroundPassageLevel2 } };
            case Area.BlackMarsh:
                return new List<List<Area>> { new List<Area> { Area.BlackMarsh, Area.HoleLevel1, Area.HoleLevel2 } };
            case Area.ForgottenTower:
                return new List<List<Area>> { new List<Area> { Area.BlackMarsh, Area.ForgottenTower, Area.TowerCellarLevel1, Area.TowerCellarLevel2, Area.TowerCellarLevel3, Area.TowerCellarLevel4, Area.TowerCellarLevel5 } };
            case Area.JailLevel1:
                return new List<List<Area>> { new List<Area> { Area.JailLevel1, Area.JailLevel2, Area.JailLevel3 } };
            case Area.Cathedral:
                return new List<List<Area>> { new List<Area> { Area.InnerCloister, Area.Cathedral, Area.CatacombsLevel1, Area.CatacombsLevel2, Area.CatacombsLevel3 } };
            // Act 2
            case Area.SewersLevel1Act2:
                return new List<List<Area>> { new List<Area> { Area.LutGholein, Area.SewersLevel1Act2, Area.SewersLevel2Act2, Area.SewersLevel3Act2 } };
            case Area.DryHills:
                return new List<List<Area>> { new List<Area> { Area.DryHills, Area.HallsOfTheDeadLevel1, Area.HallsOfTheDeadLevel2, Area.HallsOfTheDeadLevel3 } };
            case Area.FarOasis:
                return new List<List<Area>> { new List<Area> { Area.FarOasis } };
            case Area.LostCity:
                return new List<List<Area>> { new List<Area> { Area.LostCity, Area.ValleyOfSnakes, Area.ClawViperTempleLevel1, Area.ClawViperTempleLevel2 } };
            case Area.ArcaneSanctuary:
                return new List<List<Area>> { new List<Area> { Area.ArcaneSanctuary } };
            // Act 3
            case Area.SpiderForest:
                return new List<List<Area>> { new List<Area> { Area.SpiderForest, Area.SpiderCavern } };
            case Area.GreatMarsh:
                return new List<List<Area>> { new List<Area> { Area.GreatMarsh } };
            case Area.FlayerJungle:
                return new List<List<Area>> { new List<Area> { Area.FlayerJungle, Area.FlayerDungeonLevel1, Area.FlayerDungeonLevel2, Area.FlayerDungeonLevel3 } };
            case Area.KurastBazaar:
                return new List<List<Area>> { new List<Area> { Area.KurastBazaar, Area.RuinedTemple, Area.DisusedFane } };
            // Act 4
            case Area.OuterSteppes:
                return new List<List<Area>> { new List<Area> { Area.ThePandemoniumFortress, Area.OuterSteppes, Area.PlainsOfDespair } };
            case Area.RiverOfFlame:
                return new List<List<Area>> { new List<Area> { Area.CityOfTheDamned, Area.RiverOfFlame } };
            // Act 5
            case Area.BloodyFoothills:
                return new List<List<Area>> { new List<Area> { Area.Harrogath, Area.BloodyFoothills, Area.FrigidHighlands, Area.Abaddon } };
            case Area.GlacialTrail:
                return new List<List<Area>> { new List<Area> { Area.GlacialTrail, Area.DrifterCavern } };
            case Area.CrystallinePassage:
                return new List<List<Area>> { new List<Area> { Area.CrystallinePassage, Area.FrozenRiver } };
            case Area.ArreatPlateau:
                return new List<List<Area>> { new List<Area> { Area.ArreatPlateau, Area.PitOfAcheron } };
            case Area.TheAncientsWay:
                return new List<List<Area>> { new List<Area> { Area.TheAncientsWay, Area.IcyCellar } };
        }

        return new List<List<Area>>();
    }*/
}
