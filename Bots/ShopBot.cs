using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ShopBot : IBot
{
    private GameData gameData;
    public int CurrentStep = 0;
    public bool ScriptDone { get; set; } = false;
    public int MaxShopCount = -1;
    public int CurrentShopCount = 0;
    public int ShopBotTownAct = 5;
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
        gameData.townStruc.ScriptTownAct = ShopBotTownAct;

        if (!gameData.Running || !gameData.gameStruc.IsInGame())
        {
            ScriptDone = true;
            return;
        }
        
        if (CurrentStep == 0)
        {
            gameData.SetGameStatus("DOING SHOPBOT");
            gameData.battle.CastDefense();
            gameData.WaitDelay(15);

            if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.Harrogath)
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
            gameData.SetGameStatus("TOWN-SHOPBOT");
            //Console.WriteLine("town moving to shop");
            gameData.townStruc.MoveToStore();
            CurrentStep++;
        }

        if (CurrentStep == 2)
        {
            gameData.townStruc.GoToWPArea(5, 1);
            CurrentStep++;
        }

        if (CurrentStep == 2)
        {
            gameData.townStruc.GoToWPArea(5, 0);

            if (MaxShopCount > 0)
            {
                CurrentShopCount++;
                if (CurrentShopCount >= MaxShopCount)
                {
                    ScriptDone = true;
                }
            }
            CurrentStep = 1;
        }
    }
}
