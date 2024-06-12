using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class TravincalRush : IBot
{
    GameData gameData;

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position OrbPos = new Position { X = 0, Y = 0 };
    public List<long> IgnoredCouncilMembers = new List<long>();
    public bool KilledAnyMember = false;

    public Position PortalPosition = new Position { X = 0, Y = 0 };

    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
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

            gameData.townStruc.GoToWPArea(3, 7);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING TRAVINCAL");
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.Travincal)
                {
                    gameData.townStruc.SpawnTP();
                    gameData.WaitDelay(15);
                    gameData.battle.CastDefense();
                    PortalPosition.X = gameData.playerScan.xPos + 85;
                    PortalPosition.Y = gameData.playerScan.yPos - 139;
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
                gameData.pathFinding.MoveToThisPos(PortalPosition);
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                gameData.SetGameStatus("Travincal clearing");

                if (!gameData.battle.DoBattleScript(25))
                {
                    gameData.pathFinding.MoveToThisPos(PortalPosition);

                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                }
            }

            if (CurrentStep == 3)
            {
                gameData.SetGameStatus("Travincal waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.Travincal)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
            {
                OrbPos = gameData.mapAreaStruc.GetPositionOfObject("object", "CompellingOrb", (int)Enums.Area.Travincal, new List<int>());
                if (OrbPos.X != 0 && OrbPos.Y != 0)
                {
                    gameData.pathFinding.MoveToThisPos(OrbPos);

                    CurrentStep++;
                }
                else
                {
                    gameData.method_1("Kahlim Orb location not detected!", Color.Red);
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }
            }

            if (CurrentStep == 5)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING TRAVINCAL COUNCIL");
                gameData.mobsStruc.DetectThisMob("getSuperUniqueName", "Council Member", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Council Member", false, 200, IgnoredCouncilMembers))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Council Member", IgnoredCouncilMembers);
                    }
                    else
                    {
                        KilledAnyMember = true;
                        IgnoredCouncilMembers.Add(gameData.mobsStruc.MobsPointerLocation);
                    }
                }
                else
                {
                    if (!KilledAnyMember)
                    {
                        gameData.method_1("Council Members not detected!", Color.Red);

                        //baal not detected...
                        gameData.itemsStruc.GetItems(true);
                        if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Council Member", false, 200, new List<long>())) return; //redetect baal?
                        gameData.itemsStruc.GrabAllItemsForGold();
                        if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Council Member", false, 200, new List<long>())) return; //redetect baal?
                        gameData.potions.CanUseSkillForRegen = true;

                        gameData.townStruc.FastTowning = false;
                        gameData.townStruc.UseLastTP = false;
                        ScriptDone = true;
                        return;
                    }
                    else
                    {
                        gameData.townStruc.SpawnTP();
                        CurrentStep++;
                    }

                }
            }

            if (CurrentStep == 6)
            {
                gameData.SetGameStatus("Travincal waiting on leecher #2");

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.KurastDocks)
                {
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GetItems(true);
                    gameData.itemsStruc.GrabAllItemsForGold();
                    gameData.potions.CanUseSkillForRegen = true;

                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                }
            }
        }
    }
}
