using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Enums;

public class Cubing
{
    GameData gameData = GameData.Instance;

    public List<string> CubingRecipes = new List<string>();
    public List<bool> CubingRecipesEnabled = new List<bool>();

    public List<string> CurrentRecipe = new List<string>();
    public string CurrentRecipeResult = "";
    public List<int> CurrentRecipeItemInStashNumber = new List<int>();
    public List<int> CurrentRecipeItemLocations = new List<int>();

    public uint[] Cube_ItemTxtNoList = new uint[12];

    public void ResetCubeInventory()
    {
        Cube_ItemTxtNoList = new uint[12];
    }

    public void AddCubeItem(int PosX, int PosY)
    {
        int AtI = ConvertXYToFullCubeIndex(PosX, PosY);
        if (AtI < Cube_ItemTxtNoList.Length) Cube_ItemTxtNoList[AtI] = gameData.itemsStruc.txtFileNo;
    }

    public int ConvertXYToFullCubeIndex(int PosX, int PosY)
    {
        return PosX + (PosY * 3);
    }

    public void GetRecipeAt(int ThisI)
    {
        CurrentRecipeResult = "";
        CurrentRecipe = new List<string>();
        CurrentRecipeItemInStashNumber = new List<int>();
        CurrentRecipeItemLocations = new List<int>();

        string ThisRecipe = CubingRecipes[ThisI];
        if (ThisRecipe.Contains("+"))
        {
            string[] SplittedItemNames = ThisRecipe.Split('+');
            for (int i = 0; i < SplittedItemNames.Length; i++)
            {
                if (SplittedItemNames[i].Contains("="))
                {
                    string[] Splitt = SplittedItemNames[i].Split('=');
                    CurrentRecipe.Add(Splitt[0]);
                    CurrentRecipeResult = Splitt[1];
                }
                else
                {
                    CurrentRecipe.Add(SplittedItemNames[i]);
                }
            }
        }
    }

    public bool IsNotSameLocation(int ThisStash, int ThisLoc)
    {
        if (CurrentRecipeItemLocations.Count > 0)
        {
            for (int i = 0; i < CurrentRecipeItemLocations.Count; i++)
            {
                if (CurrentRecipeItemLocations[i] == ThisLoc
                    && CurrentRecipeItemInStashNumber[i] == ThisStash)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool StashContainRecipeItem(string ItemName)
    {
        for (int i = 0; i < 100; i++)
        {
            if (gameData.stashStruc.Stash1_ItemTxtNoList[i] > 0)
            {
                if (gameData.itemsNames.getItemBaseName(gameData.stashStruc.Stash1_ItemTxtNoList[i]) == ItemName
                    && IsNotSameLocation(1, i))
                {
                    CurrentRecipeItemLocations.Add(i);
                    CurrentRecipeItemInStashNumber.Add(1);
                    return true;
                }
            }
        }
        for (int i = 0; i < 100; i++)
        {
            if (gameData.stashStruc.Stash2_ItemTxtNoList[i] > 0)
            {
                if (gameData.itemsNames.getItemBaseName(gameData.stashStruc.Stash2_ItemTxtNoList[i]) == ItemName
                    && IsNotSameLocation(2, i))
                {
                    CurrentRecipeItemLocations.Add(i);
                    CurrentRecipeItemInStashNumber.Add(2);
                    return true;
                }
            }
        }
        for (int i = 0; i < 100; i++)
        {
            if (gameData.stashStruc.Stash3_ItemTxtNoList[i] > 0)
            {
                if (gameData.itemsNames.getItemBaseName(gameData.stashStruc.Stash3_ItemTxtNoList[i]) == ItemName
                    && IsNotSameLocation(3, i))
                {
                    CurrentRecipeItemLocations.Add(i);
                    CurrentRecipeItemInStashNumber.Add(3);
                    return true;
                }
            }
        }
        for (int i = 0; i < 100; i++)
        {
            if (gameData.stashStruc.Stash4_ItemTxtNoList[i] > 0)
            {
                if (gameData.itemsNames.getItemBaseName(gameData.stashStruc.Stash4_ItemTxtNoList[i]) == ItemName
                    && IsNotSameLocation(4, i))
                {
                    CurrentRecipeItemLocations.Add(i);
                    CurrentRecipeItemInStashNumber.Add(4);
                    return true;
                }
            }
        }

        if (CharConfig.RunCowsScript && !gameData.cows.ScriptDone)
        {
            for (int i = 0; i < 40; i++)
            {
                if (gameData.inventoryStruc.InventoryItemNames[i] == ItemName)
                {
                    CurrentRecipeItemLocations.Add(i);
                    CurrentRecipeItemInStashNumber.Add(5);
                    return true;
                }
            }
        }

        return false;
    }

    public void PerformCubing()
    {
        gameData.uiScan.readUI();
        if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

        //get stash item
        if (gameData.itemsStruc.dwOwnerId_Shared1 == 0 || gameData.itemsStruc.dwOwnerId_Shared2 == 0 || gameData.itemsStruc.dwOwnerId_Shared3 == 0)
        {
            gameData.itemsStruc.GetItems(false);
            gameData.itemsStruc.GetItems(false);

            //still zero return error
            if (gameData.itemsStruc.dwOwnerId_Shared1 == 0 || gameData.itemsStruc.dwOwnerId_Shared2 == 0 || gameData.itemsStruc.dwOwnerId_Shared3 == 0)
            {
                gameData.method_1("Couln't perform Cubing, Shared Stashes aren't Identifed correctly!", Color.OrangeRed);
                return;
            }
        }

        //loop thru all recipes
        for (int i = 0; i < CubingRecipes.Count; i++)
        {
            if (CubingRecipes[i] == "") continue;
            if (!CubingRecipesEnabled[i]) continue;

            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                break;
            }
            gameData.uiScan.readUI();
            if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

            GetRecipeAt(i);

            bool CanCube = true;
            for (int k = 0; k < CurrentRecipe.Count; k++)
            {
                if (!StashContainRecipeItem(CurrentRecipe[k]))
                {
                    CanCube = false;
                    break;
                }
            }

            //perform cubing
            if (CanCube)
            {
                gameData.SetGameStatus("TOWN-STASH-CUBING:" + CurrentRecipeResult);
                SendItemsToCube();
            }
        }
    }

    public void SendItemsToCube()
    {
        for (int i = 0; i < CurrentRecipeItemInStashNumber.Count; i++)
        {
            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                break;
            }
            gameData.uiScan.readUI();
            if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

            //select the stash where the item is located
            if (CurrentRecipeItemInStashNumber[i] == 1) gameData.keyMouse.MouseClicc(200, 200);   //clic stash1
            if (CurrentRecipeItemInStashNumber[i] == 2) gameData.keyMouse.MouseClicc(340, 200);   //clic shared stash1
            if (CurrentRecipeItemInStashNumber[i] == 3) gameData.keyMouse.MouseClicc(450, 200);   //clic shared stash2
            if (CurrentRecipeItemInStashNumber[i] == 4) gameData.keyMouse.MouseClicc(600, 200);   //clic shared stash3
            gameData.WaitDelay(CharConfig.CubeItemPlaceDelay);

            //select the item
            Dictionary<string, int> itemScreenPos = ConvertIndexToXY(CurrentRecipeItemLocations[i]);
            itemScreenPos = ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);
            if (CurrentRecipeItemInStashNumber[i] == 5)
            {
                itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(CurrentRecipeItemLocations[i]);
                itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);
            }
            gameData.stash.PickItem(itemScreenPos["x"], itemScreenPos["y"]);
            //gameData.WaitDelay(10);

            //select the stash where the cube is located
            if (gameData.stashStruc.CubeStashNumber == 1) gameData.keyMouse.MouseClicc(200, 200);   //clic stash1
            if (gameData.stashStruc.CubeStashNumber == 2) gameData.keyMouse.MouseClicc(340, 200);   //clic shared stash1
            if (gameData.stashStruc.CubeStashNumber == 3) gameData.keyMouse.MouseClicc(450, 200);   //clic shared stash2
            if (gameData.stashStruc.CubeStashNumber == 4) gameData.keyMouse.MouseClicc(600, 200);   //clic shared stash3
            gameData.WaitDelay(CharConfig.CubeItemPlaceDelay);

            //clic on cube to send item to cube
            itemScreenPos = ConvertIndexToXY(gameData.stashStruc.CubeIndex);
            itemScreenPos = ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);
            gameData.stash.PlaceItemShift(itemScreenPos["x"], itemScreenPos["y"]);
            gameData.WaitDelay(5);

            //make sure Cube is not on hands
            gameData.itemsStruc.GetItems(false);
            if (gameData.itemsStruc.ItemOnCursorName == "Horadric Cube") gameData.keyMouse.MouseClicc(itemScreenPos["x"], itemScreenPos["y"]);
        }

        //clic on cube to open the cube
        Dictionary<string, int> itemScreenPos2 = ConvertIndexToXY(gameData.stashStruc.CubeIndex);
        itemScreenPos2 = ConvertInventoryLocToScreenPos(itemScreenPos2["x"], itemScreenPos2["y"]);

        gameData.uiScan.readUI();
        int tryyyy = 0;
        while (!gameData.uiScan.cubeMenu && tryyyy < 7)
        {
            if (!gameData.Running || !gameData.gameStruc.IsInGame())
            {
                break;
            }
            if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

            gameData.keyMouse.MouseCliccRight(itemScreenPos2["x"], itemScreenPos2["y"]);
            gameData.WaitDelay(25);
            gameData.itemsStruc.GetItems(false);
            gameData.stash.PlaceItem(itemScreenPos2["x"], itemScreenPos2["y"]);
            gameData.uiScan.readUI();
            tryyyy++;
        }

        if (gameData.uiScan.cubeMenu)
        {
            //clic transmute button
            gameData.keyMouse.MouseClicc(405, 615);
            gameData.WaitDelay(10);
            gameData.keyMouse.MouseClicc(405, 615);
            gameData.WaitDelay(10);
            gameData.keyMouse.MouseClicc(405, 615);
            gameData.WaitDelay(10);
            gameData.itemsStruc.GetItems(false);   //get inventory again
            gameData.WaitDelay(120);

            //send item to inventory
            gameData.uiScan.readUI();
            for (int i = 0; i < Cube_ItemTxtNoList.Length; i++)
            {
                if (!gameData.Running || !gameData.gameStruc.IsInGame())
                {
                    break;
                }
                if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

                if (Cube_ItemTxtNoList[i] != 0)
                {
                    if (gameData.itemsNames.getItemBaseName(Cube_ItemTxtNoList[i]) == CurrentRecipeResult)
                    {
                        gameData.form.method_1_Items("Cubed: " + gameData.itemsNames.getItemBaseName(Cube_ItemTxtNoList[i]), Color.BlueViolet);
                    }
                }

                int tryyy = 0;
                while (Cube_ItemTxtNoList[i] != 0 && tryyy < 10)
                {
                    if (!gameData.Running || !gameData.gameStruc.IsInGame())
                    {
                        break;
                    }
                    gameData.uiScan.readUI();
                    if (!gameData.uiScan.leftMenu && !gameData.uiScan.rightMenu) return;

                    itemScreenPos2 = ConvertIndexToCubeXY(i);
                    itemScreenPos2 = ConvertCubeLocToScreenPos(itemScreenPos2["x"], itemScreenPos2["y"]);
                    gameData.keyMouse.SendCTRL_CLICK(itemScreenPos2["x"], itemScreenPos2["y"]);
                    gameData.WaitDelay(5);
                    //gameData.keyMouse.MouseClicc(itemScreenPos2["x"], itemScreenPos2["y"]);
                    //gameData.WaitDelay(10);
                    gameData.itemsStruc.GetItems(false);   //get inventory again
                    gameData.stash.PlaceItem(itemScreenPos2["x"], itemScreenPos2["y"]);
                    tryyy++;
                }
            }
        }

        gameData.uiScan.CloseUIMenu("cubeMenu");
    }

    public Dictionary<string, int> ConvertInventoryLocToScreenPos(int ThisX, int ThisY)
    {
        //starting at 1295,580 on screen for first item in inv, increment for 48px
        int xS = 185 + (ThisX * 48);
        int yS = 245 + (ThisY * 48);

        Dictionary<string, int> NewDict = new Dictionary<string, int>();
        NewDict["x"] = xS;
        NewDict["y"] = yS;
        return NewDict;
    }

    public Dictionary<string, int> ConvertCubeLocToScreenPos(int ThisX, int ThisY)
    {
        //starting at 1295,580 on screen for first item in inv, increment for 48px
        int xS = 360 + (ThisX * 48);
        int yS = 395 + (ThisY * 48);

        Dictionary<string, int> NewDict = new Dictionary<string, int>();
        NewDict["x"] = xS;
        NewDict["y"] = yS;
        return NewDict;
    }

    public Dictionary<string, int> ConvertIndexToXY(int Thisndex)
    {
        int yS = (int)Math.Floor((double)Thisndex / 10);
        int xS = Thisndex - (yS * 10);

        Dictionary<string, int> NewDict = new Dictionary<string, int>();
        NewDict["x"] = xS;
        NewDict["y"] = yS;
        return NewDict;
    }

    public Dictionary<string, int> ConvertIndexToCubeXY(int Thisndex)
    {
        int yS = (int)Math.Floor((double)Thisndex / 3);
        int xS = Thisndex - (yS * 3);

        Dictionary<string, int> NewDict = new Dictionary<string, int>();
        NewDict["x"] = xS;
        NewDict["y"] = yS;
        return NewDict;
    }
}
