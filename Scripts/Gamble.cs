using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Gamble
{
    GameData gameData = GameData.Instance;

    public int GambleType = 0;

    public bool CanGamble()
    {
        return (gameData.playerScan.PlayerGoldInStash >= CharConfig.GambleAboveGoldAmount);
    }

    public bool CanStillGamble()
    {
        return (gameData.playerScan.PlayerGoldInStash >= CharConfig.GambleUntilGoldAmount + 75000);

        //GET THE VENDOR PRICE (NOT WORKING)
        /*string GambleThisItem = CharConfig.GambleItems[GambleType];
        if (gameData.itemsStruc.GetShopItem(GambleThisItem, true))
        {
            return (gameData.playerScan.PlayerGoldInStash >= CharConfig.GambleUntilGoldAmount + gameData.itemsStruc.GetValuesFromStats(Enums.Attribute.Value));
        }*/

        //#######
        //if (GambleType == 0) return (gameData.playerScan.PlayerGoldInStash >= CharConfig.GambleUntilGoldAmount + 50000); //gamble ring
        //if (GambleType >= 1) return (gameData.playerScan.PlayerGoldInStash >= CharConfig.GambleUntilGoldAmount + 63000); //gamble ammy
        //return false;
    }

    public void RunGambleScript()
    {
        int tries = 0;
        bool Gambling = true;
        GambleType = 0;
        while (Gambling && tries < 3)
        {
            int ThisStartCount = gameData.itemsStruc.ItemsInInventory;

            string GambleThisItem = CharConfig.GambleItems[GambleType];
            if (gameData.itemsStruc.GetShopItem(GambleThisItem))
            {
                Dictionary<string, int> itemScreenPos = gameData.shop.ConvertShopLocToScreenPos(gameData.itemsStruc.itemx, gameData.itemsStruc.itemy);
                gameData.keyMouse.MouseCliccRight(itemScreenPos["x"], itemScreenPos["y"]);
                gameData.WaitDelay(100);
                gameData.playerScan.GetPositions();
                gameData.itemsStruc.GetItems(false);   //get inventory
            }

            GambleType++;
            if (GambleType > CharConfig.GambleItems.Count - 1) GambleType = 0;

            Gambling = CanStillGamble();

            if (gameData.itemsStruc.ItemsInInventory == ThisStartCount) tries++;
            ThisStartCount = gameData.itemsStruc.ItemsInInventory;
        }
    }
}
