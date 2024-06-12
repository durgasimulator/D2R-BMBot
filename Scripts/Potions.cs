using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Potions
{
    private GameData gameData;
    public int SameHPCount = 0;
    public int GainingHPCount = 0;
    public long PlayerHPLast = 0;

    public int SameMANACount = 0;
    public int GainingMANACount = 0;
    public long PlayerMANALast = 0;

    public int MercGainingHPCount = 0;
    public long MercHPLast = 0;
    public int MercSameHPCount = 0;
    public bool CanUseSkillForRegen = true;

    public bool ForceLeave = false;

    public DateTime LastTimeSinceUsedHPPot = DateTime.Now;
    public DateTime LastTimeSinceUsedManaPot = DateTime.Now;

    public List<int> MercHPList = new List<int>();

    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void CheckIfWeUsePotion()
    {
        if (gameData.playerScan.PlayerMaxHP == 0 || gameData.playerScan.PlayerMaxMana == 0)
        {
            return;
        }

        if (gameData.townStruc.Towning && gameData.townStruc.IsInTown)
        {
            return;
        }

        if (!gameData.gameStruc.IsInGame()) return;

        gameData.playerScan.GetPositions();
        CheckHPAndManaMax();


        //dead leave game
        if (gameData.playerScan.PlayerDead || ForceLeave || gameData.playerScan.PlayerHP == 0)
        {
            ForceLeave = true;
            gameData.baalLeech.SearchSameGamesAsLastOne = false;
            gameData.LeaveGame(false);
            gameData.form.IncreaseDeadCount();
            return;
            //Chicken();
        }

        //take hp
        if (((gameData.playerScan.PlayerHP * 100) / gameData.playerScan.PlayerMaxHP) <= CharConfig.TakeHPPotUnder)
        {
            if (((gameData.playerScan.PlayerHP * 100) / gameData.playerScan.PlayerMaxHP) <= CharConfig.TakeRVPotUnder)
            {
                TakePotion(2);
            }
            else
            {
                //take hp pot only if we are not gaining hp already/haven't taken potion yet
                if (GainingHPCount == 0)
                {
                    TakePotion(0);
                }
                //TakePotion(0);
            }
        }

        gameData.playerScan.GetPositions();

        //dead leave game
        if (gameData.playerScan.PlayerDead || ForceLeave)
        {
            ForceLeave = true;
            gameData.baalLeech.SearchSameGamesAsLastOne = false;
            gameData.LeaveGame(false);
            gameData.form.IncreaseDeadCount();
            return;
            //Chicken();
        }

        //chicken
        if (((gameData.playerScan.PlayerHP * 100) / gameData.playerScan.PlayerMaxHP) < CharConfig.ChickenHP)
        {
            if (!gameData.townStruc.GetInTown())
            {
                Chicken();
                return;
            }
        }

        //take mana
        if (((gameData.playerScan.PlayerMana * 100) / gameData.playerScan.PlayerMaxMana) <= CharConfig.TakeManaPotUnder)
        {
            //take mana pot only if we are not gaining mana already/haven't taken potion yet
            if (GainingMANACount == 0)
            {
                TakePotion(1);
            }
            //TakePotion(1);
        }

        //Check Merc
        if (CharConfig.UsingMerc)
        {
            gameData.mercStruc.GetMercInfos();
            if (gameData.mercStruc.MercAlive)
            {
                CheckHPMerc();
                int ThisMercHP = (int) ((gameData.mercStruc.MercHP * 100.0) / gameData.mercStruc.MercMaxHP);
                MercHPList.Add(ThisMercHP);
                if (MercHPList.Count > 10) MercHPList.RemoveAt(0);
                int MercHPAverage = 0;
                for (int i = 0; i < MercHPList.Count; i++) MercHPAverage += MercHPList[i];
                MercHPAverage = MercHPAverage / MercHPList.Count;

                if (MercHPAverage <= CharConfig.MercTakeHPPotUnder)
                {
                    //take hp pot only if we are not gaining hp already/haven't taken potion yet
                    if (MercGainingHPCount == 0)
                    {
                        TakePotion(1, true);  //merc send potion
                    }
                }
            }
            else
            {
                if (!gameData.townStruc.GetInTown() && CharConfig.TownIfMercDead && (gameData.playerScan.PlayerGoldInventory + gameData.playerScan.PlayerGoldInStash) >= 75000)
                {
                    gameData.townStruc.FastTowning = true;
                    gameData.townStruc.GoToTown();
                }
            }
        }
    }

    public void TakePotion(int PotToTake, bool SendToMerc = false)
    {
        //Take HP Pot
        if (PotToTake == 0)
        {
            bool UsedPot = false;
            TimeSpan ThisTimeCheck = DateTime.Now - LastTimeSinceUsedHPPot;
            if (ThisTimeCheck.TotalMilliseconds > CharConfig.TakeHPPotionDelay)
            {
                for (int i = 0; i < CharConfig.BeltPotTypeToHave.Length; i++)
                {
                    if (CharConfig.BeltPotTypeToHave[i] == 0) //Type equal 0
                    {
                        if (gameData.beltStruc.BeltHaveItems[i] == 1)
                        {
                            PressPotionKey(i, SendToMerc);
                            UsedPot = true;
                            LastTimeSinceUsedHPPot = DateTime.Now;
                            gameData.beltStruc.CheckForMissingPotions();
                            gameData.playerScan.GetPositions();
                            i = CharConfig.BeltPotTypeToHave.Length;
                        }
                    }
                }
            }
            else
            {
                UsedPot = true;
            }
            if (!UsedPot)
            {
                gameData.townStruc.FastTowning = true;
                gameData.townStruc.GoToTown();
                return;
            }
        }
        //Take Mana Pot
        if (PotToTake == 1)
        {
            bool UsedPot = false;
            TimeSpan ThisTimeCheck = DateTime.Now - LastTimeSinceUsedManaPot;
            if (ThisTimeCheck.TotalMilliseconds > CharConfig.TakeManaPotionDelay)
            {
                for (int i = 0; i < CharConfig.BeltPotTypeToHave.Length; i++)
                {
                    if (CharConfig.BeltPotTypeToHave[i] == 1) //Type equal 1
                    {
                        if (gameData.beltStruc.BeltHaveItems[i] == 1)
                        {
                            PressPotionKey(i, SendToMerc);
                            UsedPot = true;
                            LastTimeSinceUsedManaPot = DateTime.Now;
                            gameData.beltStruc.CheckForMissingPotions();
                            gameData.playerScan.GetPositions();
                            i = CharConfig.BeltPotTypeToHave.Length;
                        }
                    }
                }
            }
            else
            {
                UsedPot = true;
            }
            if (!UsedPot)
            {
                gameData.townStruc.FastTowning = true;
                gameData.townStruc.GoToTown();
                return;
            }
        }
        //Take RV Pot
        if (PotToTake == 2)
        {
            bool UsedPot = false;
            for (int i = 0; i < CharConfig.BeltPotTypeToHave.Length; i++)
            {
                if (CharConfig.BeltPotTypeToHave[i] == 2 || CharConfig.BeltPotTypeToHave[i] == 3) //Type equal 2 or 3
                {
                    if (gameData.beltStruc.BeltHaveItems[i] == 1)
                    {
                        PressPotionKey(i, SendToMerc);
                        UsedPot = true;
                        gameData.beltStruc.CheckForMissingPotions();
                        gameData.playerScan.GetPositions();
                        i = CharConfig.BeltPotTypeToHave.Length;
                    }
                }
            }
            if (!UsedPot)
            {
                gameData.townStruc.FastTowning = true;
                gameData.townStruc.GoToTown();
                return;
            }
        }

        gameData.itemsStruc.GetItems(false);
    }

    public void Chicken()
    {
        gameData.method_1("Leaving reason: Chicken HP", Color.Red);
        gameData.LeaveGame(false);

        gameData.TotalChickenCount++;
        gameData.form.LabelChickenCount.Text = gameData.TotalChickenCount.ToString();
    }

    public void PressPotionKey(int i, bool SendToMerc)
    {
        if (i == 0)
        {
            if (SendToMerc) gameData.keyMouse.PressPotionKeyMerc(CharConfig.KeyPotion1);
            else gameData.keyMouse.PressPotionKey(CharConfig.KeyPotion1);
        }
        if (i == 1)
        {
            if (SendToMerc) gameData.keyMouse.PressPotionKeyMerc(CharConfig.KeyPotion2);
            else gameData.keyMouse.PressPotionKey(CharConfig.KeyPotion2);
        }
        if (i == 2)
        {
            if (SendToMerc) gameData.keyMouse.PressPotionKeyMerc(CharConfig.KeyPotion3);
            else gameData.keyMouse.PressPotionKey(CharConfig.KeyPotion3);
        }
        if (i == 3)
        {
            if (SendToMerc) gameData.keyMouse.PressPotionKeyMerc(CharConfig.KeyPotion4);
            else gameData.keyMouse.PressPotionKey(CharConfig.KeyPotion4);
        }
    }

    public void CheckHPMerc()
    {
        if (gameData.mercStruc.MercHP < MercHPLast)
        {
            MercGainingHPCount = 0;
            MercSameHPCount = 0;
        }
        if (gameData.mercStruc.MercHP >= MercHPLast)
        {
            if (MercGainingHPCount < 250)
            {
                MercGainingHPCount++;
            }
            if (gameData.mercStruc.MercHP == MercHPLast)
            {
                if (MercSameHPCount < 45)
                {
                    MercSameHPCount++;
                }
            }
            else
            {
                MercSameHPCount = 0;
            }

            //Set Higher HP
            if (gameData.mercStruc.MercHP > gameData.mercStruc.MercMaxHP)
            {
                gameData.mercStruc.MercMaxHP = gameData.mercStruc.MercHP;
            }
        }
        //Set Lower HP
        if (gameData.mercStruc.MercHP == MercHPLast && MercSameHPCount >= 45)
        {
            if (gameData.mercStruc.MercHP < gameData.mercStruc.MercMaxHP)
            {
                gameData.mercStruc.MercMaxHP = gameData.mercStruc.MercHP;
            }
        }
    }

    public void CheckHPAndManaMax()
    {
        //if (PlayerHP > PlayerMaxHP) PlayerMaxHP = PlayerHP;
        //if (PlayerMana > PlayerMaxMana) PlayerMaxMana = PlayerMana;

        //############################
        if (gameData.playerScan.PlayerHP < PlayerHPLast)
        {
            if (CanUseSkillForRegen && !CharConfig.RunItemGrabScriptOnly)
            {
                gameData.keyMouse.PressKey(CharConfig.KeySkillLifeAura);
            }
            GainingHPCount = 0;
            SameHPCount = 0;
        }
        if (gameData.playerScan.PlayerHP >= PlayerHPLast)
        {
            if (GainingHPCount < 250)
            {
                GainingHPCount++;
            }
            if (gameData.playerScan.PlayerHP == PlayerHPLast)
            {
                if (SameHPCount < 45)
                {
                    SameHPCount++;
                }
            }
            else
            {
                if (CanUseSkillForRegen && !CharConfig.RunItemGrabScriptOnly)
                {
                    gameData.keyMouse.PressKey(CharConfig.KeySkillLifeAura);
                }
                SameHPCount = 0;
            }

            //Set Higher HP
            if (gameData.playerScan.PlayerHP > gameData.playerScan.PlayerMaxHP)
            {
                gameData.playerScan.PlayerMaxHP = gameData.playerScan.PlayerHP;
            }
        }
        //Set Lower HP
        if (gameData.playerScan.PlayerHP == PlayerHPLast && SameHPCount >= 45)
        {
            if (!CharConfig.RunItemGrabScriptOnly && !gameData.battle.DoingBattle) gameData.keyMouse.PressKey(CharConfig.KeySkillDefenseAura);
            if (gameData.playerScan.PlayerHP < gameData.playerScan.PlayerMaxHP)
            {
                gameData.playerScan.PlayerMaxHP = gameData.playerScan.PlayerHP;
            }
        }
        //############################
        if (gameData.playerScan.PlayerMana < PlayerMANALast)
        {
            GainingMANACount = 0;
            SameMANACount = 0;
        }
        if (gameData.playerScan.PlayerMana >= PlayerMANALast)
        {
            if (GainingMANACount < 250)
            {
                GainingMANACount++;
            }
            if (gameData.playerScan.PlayerMana == PlayerMANALast)
            {
                if (SameMANACount < 55)
                {
                    SameMANACount++;
                }
            }
            else
            {
                SameMANACount = 0;
            }

            //Set Higher Mana
            if (gameData.playerScan.PlayerMana > gameData.playerScan.PlayerMaxMana)
            {
                gameData.playerScan.PlayerMaxMana = gameData.playerScan.PlayerMana;
            }
        }
        //Set Lower Mana
        if (gameData.playerScan.PlayerMana == PlayerMANALast && SameMANACount >= 55)
        {
            if (gameData.playerScan.PlayerMana < gameData.playerScan.PlayerMaxMana)
            {
                gameData.playerScan.PlayerMaxMana = gameData.playerScan.PlayerMana;
            }
        }
        //############################

        PlayerHPLast = gameData.playerScan.PlayerHP;
        PlayerMANALast = gameData.playerScan.PlayerMana;
    }
}
