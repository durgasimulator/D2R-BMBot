using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class SummonerRush : IBot
{
    GameData gameData;

    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public Position WaitPos = new Position { X = 0, Y = 0 };


    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
    }

    public void RunScript()
    {
        gameData = GameData.Instance;
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
                //gameData.battle.CastDefense();
                //gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ArcaneSanctuary)
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
                gameData.pathFinding.MoveToNPC("Summoner", 25);

                gameData.playerScan.GetPositions();
                WaitPos.X = gameData.playerScan.xPos;
                WaitPos.Y = gameData.playerScan.yPos;
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                if (!gameData.battle.DoBattleScript(25))
                {
                    gameData.pathFinding.MoveToThisPos(WaitPos);

                    gameData.townStruc.TPSpawned = false;
                    CurrentStep++;
                }

                gameData.townStruc.TPSpawned = false;
                CurrentStep++;
            }

            if (CurrentStep == 3)
            {
                gameData.SetGameStatus("Summoner waiting on leecher");

                if (!gameData.townStruc.TPSpawned) gameData.townStruc.SpawnTP();

                gameData.battle.DoBattleScript(25);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo == (int)Enums.Area.ArcaneSanctuary)
                {
                    CurrentStep++;
                }
            }

            if (CurrentStep == 4)
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
                        gameData.potions.CanUseSkillForRegen = true;
                        CurrentStep++;

                        /*gameData.itemsStruc.GetItems(true);
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
                        ScriptDone = true;
                        return;
                        //gameData.LeaveGame(true);*/
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

                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                    //gameData.LeaveGame(true);
                }
            }

            if (CurrentStep == 5)
            {
                gameData.SetGameStatus("Summoner waiting on leecher #2");

                gameData.battle.DoBattleScript(50);

                //get leecher infos
                gameData.playerScan.GetLeechPositions();

                if (gameData.playerScan.LeechlevelNo != (int)Enums.Area.ArcaneSanctuary)
                {
                    gameData.townStruc.FastTowning = false;
                    gameData.townStruc.UseLastTP = false;
                    ScriptDone = true;
                    return;
                }
            }
        }
    }
}
