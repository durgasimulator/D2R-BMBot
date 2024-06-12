using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Cows : IBot
{
    GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position TristramPos = new Position { X = 0, Y = 0 };

    public bool HasWirtsLeg = false;
    public bool HadWirtsLeg = false;

    public bool HadTomeOfPortal = false;

    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
        HasWirtsLeg = false;
        HadWirtsLeg = false;
        HadTomeOfPortal = false;
    }

    public void DetectCurrentStep()
    {
        if (HadTomeOfPortal)
        {
            if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.MooMooFarm) CurrentStep = 1;
        }
        else
        {
            if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.StonyField) CurrentStep = 1;
            if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.Tristram) CurrentStep = 3;
        }
    }

    public void RunScriptTristam()
    {
        gameData = GameData.Instance;
        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(1, 2);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING TRISTRAM");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.StonyField)
                {
                    CurrentStep++;
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
                TristramPos = gameData.mapAreaStruc.GetPositionOfObject("object", "CairnStoneAlpha", (int)Enums.Area.StonyField, new List<int>());
                if (TristramPos.X != 0 && TristramPos.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(TristramPos);

                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Tristram location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 2)
            {
                gameData.SetGameStatus("Tristram waiting for Tristram portal");

                gameData.battle.DoBattleScript(5);

                if (gameData.objectsStruc.GetObjects("PermanentTownPortal", true, new List<uint>(), 60))
                {
                    gameData.mover.MoveToLocation(gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                    Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                    gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                    gameData.WaitDelay(20);
                }

                if (gameData.playerScan.levelNo == (int)Enums.Area.Tristram)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.StonyField)
                {
                    CurrentStep--;
                    return;
                }

                gameData.SetGameStatus("Doing Tristram");

                gameData.pathFinding.MoveToObject("WirtCorpse");

                if (gameData.objectsStruc.GetObjects("WirtCorpse", true, new List<uint>()))
                {
                    if (gameData.mover.MoveToLocation(gameData.objectsStruc.itemx, gameData.objectsStruc.itemy))
                    {
                        gameData.inventoryStruc.DumpBadItemsOnGround();

                        //repeat clic on WirtCorpse
                        int tryyy = 0;
                        while (tryyy <= 7)
                        {
                            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                            gameData.WaitDelay(4);
                            tryyy++;
                        }

                        CurrentStep++;
                    }
                }
            }

            if (CurrentStep == 4)
            {
                //take leg
                HasWirtsLeg = gameData.inventoryStruc.HasInventoryItemName("Wirt's Leg");
                DateTime TimeSinceTakingLeg = DateTime.Now;
                while (!HasWirtsLeg && (DateTime.Now - TimeSinceTakingLeg).TotalSeconds < 2)
                {
                    gameData.itemsStruc.PickThisItem("Wirt's Leg");
                    gameData.itemsStruc.GetItems(false); //get inventory
                    HasWirtsLeg = gameData.inventoryStruc.HasInventoryItemName("Wirt's Leg");
                }

                if (HasWirtsLeg)
                {
                    HadWirtsLeg = true;
                    CurrentStep = 0; //go to next script for cow
                    gameData.townStruc.UseLastTP = false;
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.Towning = true;
                    gameData.townStruc.GoToTown();
                }
                else
                {
                    CurrentStep--; //return clicking on corpse
                }
            }
        }
    }

    public void RunScriptCow()
    {
        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO SHOP");
            CurrentStep = 0;

            bool HasTownPortal = gameData.inventoryStruc.HasInventoryItemName("Tome of Town Portal", true);
            if (!HasTownPortal && !HadTomeOfPortal)
            {
                //buy tome of portal in store
                gameData.shop.ShopForTomeOfPortal = true;
                gameData.townStruc.MoveToStore();
            }
            else
            {
                HadTomeOfPortal = true;

                if (gameData.objectsStruc.GetObjects("PermanentTownPortal", true, new List<uint>()))
                {
                    Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);

                    gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
                    gameData.WaitDelay(100);
                }
                else
                {
                    //move to stash to create portal by cubing it
                    gameData.townStruc.MoveToStash(true);
                }
            }
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING COWS");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.MooMooFarm)
                {
                    CurrentStep++;
                }
                else
                {
                    DetectCurrentStep();
                    //Console.WriteLine("step shoul be 0: " + CurrentStep);
                    if (CurrentStep == 0)
                    {
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.GoToTown();
                    }
                }
            }

            if (CurrentStep == 1)
            {
                if ((Enums.Area)gameData.battle.AreaIDFullyCleared != Enums.Area.MooMooFarm)
                {
                    gameData.battle.ClearFullAreaOfMobs();

                    if (!gameData.battle.ClearingArea)
                    {
                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.UseLastTP = false;
                        ScriptDone = true;
                    }
                }
                else
                {
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                }
            }
        }
    }

    public void RunScript()
    {
        gameData = GameData.Instance;
        gameData.townStruc.ScriptTownAct = 1; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        gameData.itemsStruc.GetItems(false); //get inventory
        HasWirtsLeg = gameData.inventoryStruc.HasInventoryItemName("Wirt's Leg", true);
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.MooMooFarm) HasWirtsLeg = true;

        if (HasWirtsLeg) HadWirtsLeg = true;

        if (HasWirtsLeg || HadWirtsLeg)
        {
            if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.Tristram) RunScriptTristam();
            else RunScriptCow();
        }
        else
        {
            RunScriptTristam();
        }
    }
}
