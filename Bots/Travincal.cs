using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Travincal : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position OrbPos = new Position { X = 0, Y = 0 };
    public List<long> IgnoredCouncilMembers = new List<long>();
    public bool KilledAnyMember = false;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
    }

    public void RunScript()
    {
        gameData.townStruc.ScriptTownAct = 5; //set to town act 5 when running this script

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
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.Travincal)
                {
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

            if (CurrentStep == 2)
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

                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        return;
                    }
                    else
                    {
                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        return;
                    }

                }
            }
        }
    }
}
