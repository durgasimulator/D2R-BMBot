using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapAreaStruc;

public class Pindleskin : IBot
{
    GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;


    public void ResetVars()
    {
        CurrentStep = 0;
        ScriptDone = false;
    }

    public void RunScript()
    {
        gameData = GameData.Instance;
        gameData.townStruc.ScriptTownAct = 5; //set to town act 5 when running this script

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }

        if (gameData.townStruc.GetInTown())
        {
            gameData.SetGameStatus("GO TO RED PORTAL");
            CurrentStep = 0;

            if (gameData.townStruc.TownAct != 5)
            {
                gameData.townStruc.ScriptTownAct = 5;
                gameData.townStruc.Towning = true;
                return;
            }

            gameData.pathFinding.MoveToThisPos(new Position { X = 5121, Y = 5123 });

            //5119,5121

            //if (gameData.objectsStruc.GetObjects("PermanentTownPortal", true, new List<uint>()))
            //{
            //Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.objectsStruc.itemx, gameData.objectsStruc.itemy);
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, 5117, 5120);

            gameData.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y - 15);
            gameData.WaitDelay(100);
            //}
            //gameData.townStruc.GoToWPArea(3, 8);
        }
        else
        {
            if (CurrentStep == 0)
            {
                gameData.SetGameStatus("DOING PINDLESKIN");
                gameData.battle.CastDefense();
                gameData.WaitDelay(15);

                if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.NihlathaksTemple)
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
                gameData.pathFinding.MoveToThisPos(new Position { X = 10058, Y = 13236 });
                CurrentStep++;
            }

            if (CurrentStep == 2)
            {
                gameData.potions.CanUseSkillForRegen = false;
                gameData.SetGameStatus("KILLING PINDLESKIN");
                gameData.mobsStruc.DetectThisMob("getSuperUniqueName", "Pindleskin", false, 200, new List<long>());
                if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Pindleskin", false, 200, new List<long>()))
                {
                    if (gameData.mobsStruc.MobsHP > 0)
                    {
                        gameData.battle.RunBattleScriptOnThisMob("getSuperUniqueName", "Pindleskin", new List<long>());
                    }
                    else
                    {
                        if (gameData.battle.EndBossBattle()) ScriptDone = true;
                        return;
                    }
                }
                else
                {
                    gameData.method_1("Pindleskin not detected!", Color.Red);

                    //baal not detected...
                    gameData.itemsStruc.GetItems(true);
                    if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Pindleskin", false, 200, new List<long>())) return; //redetect baal?
                    gameData.itemsStruc.GrabAllItemsForGold();
                    if (gameData.mobsStruc.GetMobs("getSuperUniqueName", "Pindleskin", false, 200, new List<long>())) return; //redetect baal?
                    gameData.potions.CanUseSkillForRegen = true;

                    if (gameData.battle.EndBossBattle()) ScriptDone = true;
                    return;
                }
            }
        }
    }
}
