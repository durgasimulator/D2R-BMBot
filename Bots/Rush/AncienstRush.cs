using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class AncientsRush : IBot
{
    GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position AltarPos = new Position { X = 0, Y = 0 };
    public bool KilledAnyMember = false;

    public List<long> IgnoredMembers = new List<long>();


    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
        IgnoredMembers = new List<long>();
    }

    public void RunScript()
    {
        gameData = GameData.Instance;
        gameData.townStruc.ScriptTownAct = 3; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(5, 7);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING ANCIENTS");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TheAncientsWay)
                {
                    gameData.townStruc.SpawnTP();
                    gameData.WaitDelay(15);
                    gameData.battle.CastDefense();
                    CurrentStep++;
                }
                else
                {
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.GoToTown();
                }
            }

            if (CurrentStep == 1)
            {
                gameData.pathFinding.MoveToExit(Enums.Area.ArreatSummit, 10);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                gameData.SetGameStatus("Ancients clearing");

                if (!gameData.battle.DoBattleScript(15))
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.ArreatSummit, 10);

                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                gameData.SetGameStatus("Ancients waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(15);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.TheAncientsWay
                    || gameData.playerScan.LeechlevelNo == (int)Enums.Area.ArreatSummit)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                if (gameData.playerScan.levelNo == (int)Enums.Area.ArreatSummit)
                {
                    CurrentStep++;
                }
                else
                {
                    gameData.pathFinding.MoveToExit(Enums.Area.ArreatSummit);
                    CurrentStep++;
                }
            }

            if (CurrentStep == 5)
            {
                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.TheAncientsWay)
                {
                    CurrentStep--;
                    return;
                }

                AltarPos = gameData.mapAreaStruc.GetPositionOfObject("object", "AncientsAltar", (int)Enums.Area.ArreatSummit, new List<int>());
                if (AltarPos.X != 0 && AltarPos.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(AltarPos);

                    //repeat clic on altar
                    int tryyy = 0;
                    while (tryyy <= 25)
                    {
                        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, AltarPos.X, AltarPos.Y);

                        gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
                        gameData.playerScan.GetPositions();
                        tryyy++;
                    }

                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Ancients Altar location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    ScriptDone = true;
                    gameData.townStruc.UseLastTP = false;
                    return;
                }
            }

            if (CurrentStep == 6)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING ANCIENTS");
                gameData.mobsStruc.DetectThisMob("getSuperUniqueName", "Ancient Barbarian 1", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Ancient Barbarian 1", false, 200, IgnoredMembers))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Ancient Barbarian 1", IgnoredMembers);
                    }
                    else
                    {
                        KilledAnyMember = true;
                        IgnoredMembers.Add(gameData.mobsStruc.MobsPointerLocation);


                        if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Ancient Barbarian 2", false, 200, IgnoredMembers))
                        {
                            if (gameData.mobsStruc.MobsHP > 0)
                            {
                                gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Ancient Barbarian 2", IgnoredMembers);
                            }
                            else
                            {
                                IgnoredMembers.Add(gameData.mobsStruc.MobsPointerLocation);


                                if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Ancient Barbarian 3", false, 200, IgnoredMembers))
                                {
                                    if (gameData.mobsStruc.MobsHP > 0)
                                    {
                                        gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Ancient Barbarian 3", IgnoredMembers);
                                    }
                                    else
                                    {
                                        IgnoredMembers.Add(gameData.mobsStruc.MobsPointerLocation);

                                        //Done all killed!
                                        gameData.potions.CanUseSkillForRegen = true;
                                        gameData.townStruc.UseLastTP = false;
                                        gameData.townStruc.FastTowning = false;
                                        ScriptDone = true;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!KilledAnyMember) gameData.method_1("Ancients Members not detected!", Color.Red);

                    gameData.potions.CanUseSkillForRegen = true;
                    gameData.townStruc.UseLastTP = false;
                    gameData.townStruc.FastTowning = false;
                    ScriptDone = true;
                }
            }
        }
    }
}
