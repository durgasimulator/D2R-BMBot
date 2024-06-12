﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StashStruc
{
    private GameData gameData;

    public uint[] Stash1_ItemTxtNoList = new uint[100];
    public uint[] Stash2_ItemTxtNoList = new uint[100];
    public uint[] Stash3_ItemTxtNoList = new uint[100];
    public uint[] Stash4_ItemTxtNoList = new uint[100];
    public int CubeIndex = 0;
    public int CubeStashNumber = 0;
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }

    public void ResetStashInventory()
    {
        Stash1_ItemTxtNoList = new uint[100];
        Stash2_ItemTxtNoList = new uint[100];
        Stash3_ItemTxtNoList = new uint[100];
        Stash4_ItemTxtNoList = new uint[100];
    }

    public int ConvertXYToFullIndex(int PosX, int PosY)
    {
        return PosX + (PosY * 10);
    }

    public int GetStashItemCount(string ThisItemName)
    {
        int ThisCount = 0;

        for (int i = 0; i < Stash1_ItemTxtNoList.Length; i++)
        {
            if (gameData.itemsNames.getItemBaseName(Stash1_ItemTxtNoList[i]) == ThisItemName)
            {
                ThisCount++;
            }
        }
        for (int i = 0; i < Stash2_ItemTxtNoList.Length; i++)
        {
            if (gameData.itemsNames.getItemBaseName(Stash2_ItemTxtNoList[i]) == ThisItemName)
            {
                ThisCount++;
            }
        }
        for (int i = 0; i < Stash3_ItemTxtNoList.Length; i++)
        {
            if (gameData.itemsNames.getItemBaseName(Stash3_ItemTxtNoList[i]) == ThisItemName)
            {
                ThisCount++;
            }
        }
        for (int i = 0; i < Stash4_ItemTxtNoList.Length; i++)
        {
            if (gameData.itemsNames.getItemBaseName(Stash4_ItemTxtNoList[i]) == ThisItemName)
            {
                ThisCount++;
            }
        }

        return ThisCount;
    }

    public void AddStashItem(int PosX, int PosY, int StashNumber)
    {
        try
        {
            int AtI = ConvertXYToFullIndex(PosX, PosY);
            if (StashNumber == 1)
            {
                Stash1_ItemTxtNoList[AtI] = gameData.itemsStruc.txtFileNo;
            }
            if (StashNumber == 2)
            {
                Stash2_ItemTxtNoList[AtI] = gameData.itemsStruc.txtFileNo;
            }
            if (StashNumber == 3)
            {
                Stash3_ItemTxtNoList[AtI] = gameData.itemsStruc.txtFileNo;
            }
            if (StashNumber == 4)
            {
                Stash4_ItemTxtNoList[AtI] = gameData.itemsStruc.txtFileNo;
            }

            if (gameData.itemsStruc.ItemNAAME == "Horadric Cube")
            {
                CubeIndex = AtI;
                CubeStashNumber = StashNumber;
            }
        }
        catch { }
    }
}
