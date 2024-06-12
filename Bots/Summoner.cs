using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Summoner : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;

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
        gameData.townStruc.ScriptTownAct = 2; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO WP");
            CurrentStep = 0;

            gameData.townStruc.GoToWPArea(2, 7);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING SUMMONER");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ArcaneSanctuary)
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
                gameData.pathFinding.MoveToNPC("Summoner");

                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING SUMMONER");
                gameData.mobsStruc.DetectThisMob("getBossName", "Summoner", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getBossName", "Summoner", false, 200, new List<long>()))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.battle.RunBattleScriptOnThisMob("getBossName", "Summoner", new List<long>());
                    }
                    else
                    {
                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        return;
                    }
                }
                else
                {
                    gameData.method_1("Summoner not detected!", Color.Red);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getBossName", "Summoner", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getBossName", "Summoner", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    return;
                }
            }
        }
    }
}
