using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;

public class Battle
{
    private GameData gameData;

    public int AreaX = 0;
    public int AreaY = 0;
    public bool ClearingArea = false;
    public List<long> IgnoredMobsPointer = new List<long>();
    public int ClearingSize = 0;
    public long LastMobAttackedHP = 0;
    public int AttackNotRegisteredCount = 0;
    public int MoveTryCount = 0;

    public int MaxMoveTry = 5;

    public bool FirstAttackCasted = false;
    public bool DoingBattle = false;
    public bool ClearingFullArea = false;

    public int TriedToMoveToMobsCount = 0;

    public string LastMobName = "";
    public string LastMobType = "";

    public List<Room> AllRooms_InArea = new List<Room>();
    public List<int> IgnoredRooms_InArea = new List<int>();
    public int DoingRoomIndex = 0;
    public bool LeftToRight = true;

    public int AreaIDFullyCleared = 0;

    public DateTime TimeSinceLastCast = DateTime.MaxValue;

    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public bool EndBossBattle()
    {
        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);
        if (!gameData.itemsStruc.GetItems(true)) gameData.WaitDelay(CharConfig.EndBattleGrabDelay);

        if (CharConfig.ClearAfterBoss)
        {
            if (gameData.mobsStruc.GetMobs("", "", true, 30, new List<long>()))
            {
                gameData.battle.DoBattleScript(30);
                return false;
            }
        }

        gameData.itemsStruc.GrabAllItemsForGold();

        gameData.battle.ClearingArea = false;
        gameData.battle.DoingBattle = false;
        gameData.potions.CanUseSkillForRegen = true;
        gameData.townStruc.FastTowning = false;
        gameData.townStruc.UseLastTP = false;

        return true;
    }

    public int[] FindBestPositionNoMobsArround(int playerX, int playerY, List<int[]> monsterPositions, int maxDisplacement)
    {
        // Create a list to store all possible positions around the player
        List<int[]> possiblePositions = new List<int[]>();

        // Generate all possible positions within the maximum displacement range
        for (int x = playerX - maxDisplacement; x <= playerX + maxDisplacement; x++)
        {
            for (int y = playerY - maxDisplacement; y <= playerY + maxDisplacement; y++)
            {
                // Calculate the distance between the player and the current position
                double distance = Math.Sqrt(Math.Pow(playerX - x, 2) + Math.Pow(playerY - y, 2));

                // Check if the distance is within the maximum displacement and the position is not occupied by a monster
                if (distance <= maxDisplacement && !IsMonsterNearPosition(x, y, monsterPositions))
                {
                    //possiblePositions.Add(Tuple.Create(x, y));
                    possiblePositions.Add(new int[2] { x, y });
                }
            }
        }

        // Find the closest position among the possible positions
        //int[] bestPosition = Tuple.Create(playerX, playerY);
        int[] bestPosition = new int[2] { playerX, playerY };
        double closestDistance = double.MaxValue;
        foreach (var position in possiblePositions)
        {
            double distance = Math.Sqrt(Math.Pow(playerX - position[0], 2) + Math.Pow(playerY - position[1], 2));
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestPosition = position;
            }
        }

        return bestPosition;
    }

    static bool IsMonsterNearPosition(int x, int y, List<int[]> monsterPositions)
    {
        foreach (var monsterPosition in monsterPositions)
        {
            if (monsterPosition[0] >= x - 6
                && monsterPosition[0] <= x + 6
                && monsterPosition[1] >= y - 6
                && monsterPosition[1] <= y + 6)
            {
                return true;
            }
        }
        return false;
    }

    public Position GetBestAttackLocation(Position ThisAttackPos)
    {
        Position ReturnPos = new Position { X = ThisAttackPos.X, Y = ThisAttackPos.Y };
        int ChoosenAttackLocation = 0; //0=Down, 1=Right, 2=Left, 3=Up

        bool[,] ThisCollisionGrid = gameData.mapAreaStruc.CollisionGrid((Enums.Area)gameData.playerScan.levelNo);

        if (ThisCollisionGrid.GetLength(0) == 0 || ThisCollisionGrid.GetLength(1) == 0) return ReturnPos;
        if (gameData.mapAreaStruc.AllMapData.Count == 0) return ReturnPos;

        int ThisX = ThisAttackPos.X - gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.X;
        int ThisY = ThisAttackPos.Y - gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.Y;

        if (ThisX < 0) return ReturnPos;
        if (ThisY < 0) return ReturnPos;
        if (ThisX > ThisCollisionGrid.GetLength(0) - 1) return ReturnPos;
        if (ThisY > ThisCollisionGrid.GetLength(1) - 1) return ReturnPos;

        try
        {
            bool AttackPosFound = false;
            while (!AttackPosFound)
            {
                //check boundary for attacking the mobs from down position
                if (ChoosenAttackLocation == 0)
                {
                    //#####
                    //Check Validity
                    bool IsValid = true;
                    if (ThisX < 2) IsValid = false;
                    if (ThisY < 2) IsValid = false;
                    if (ThisX > ThisCollisionGrid.GetLength(0) - 1) IsValid = false;
                    if (ThisY > ThisCollisionGrid.GetLength(1) - 1) IsValid = false;
                    //#####

                    if (ThisCollisionGrid[ThisX, ThisY]
                        && ThisCollisionGrid[ThisX - 1, ThisY]
                        && ThisCollisionGrid[ThisX - 2, ThisY]
                        && ThisCollisionGrid[ThisX - 1, ThisY - 1]
                        && ThisCollisionGrid[ThisX - 2, ThisY - 1]
                        && ThisCollisionGrid[ThisX - 1, ThisY - 2]
                        && ThisCollisionGrid[ThisX - 1, ThisY - 3]
                        && ThisCollisionGrid[ThisX - 1, ThisY - 4]
                        && IsValid)
                    {
                        //gameData.method_1("Attack from Bottom!", Color.OrangeRed);
                        AttackPosFound = true;
                        ChoosenAttackLocation = 0; //Attack from Bottom
                        ReturnPos = new Position { X = ThisX + gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.X, Y = ThisY + gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.Y };
                    }
                    else
                    {
                        //change attack location to right
                        ThisX += 4;
                        ThisY -= 2;

                        ChoosenAttackLocation++;
                    }
                }

                //check boundary for attacking the mobs from Right position
                if (ChoosenAttackLocation == 1)
                {
                    //#####
                    //Check Validity
                    bool IsValid = true;
                    if (ThisX < 2) IsValid = false;
                    if (ThisY < 0) IsValid = false;
                    if (ThisX > ThisCollisionGrid.GetLength(0) - 1) IsValid = false;
                    if (ThisY > ThisCollisionGrid.GetLength(1) - 1) IsValid = false;
                    //#####

                    if (ThisCollisionGrid[ThisX, ThisY]
                        && ThisCollisionGrid[ThisX - 1, ThisY]
                        && ThisCollisionGrid[ThisX - 2, ThisY]
                        && IsValid)
                    {
                        //gameData.method_1("Attack from Right!", Color.OrangeRed);
                        AttackPosFound = true;
                        ChoosenAttackLocation = 1; //Attack from Right
                        ReturnPos = new Position { X = ThisX + gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.X, Y = ThisY + gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.Y };
                    }
                    else
                    {
                        //change attack location to left
                        ThisX -= 7;

                        ChoosenAttackLocation++;
                    }
                }

                //check boundary for attacking the mobs from Left position
                if (ChoosenAttackLocation == 2)
                {
                    //#####
                    //Check Validity
                    bool IsValid = true;
                    if (ThisX < 1) IsValid = false;
                    if (ThisY < 1) IsValid = false;
                    if (ThisX > ThisCollisionGrid.GetLength(0) - 3) IsValid = false;
                    if (ThisY > ThisCollisionGrid.GetLength(1) - 1) IsValid = false;
                    //#####

                    if (ThisCollisionGrid[ThisX, ThisY]
                        && ThisCollisionGrid[ThisX - 1, ThisY]
                        && ThisCollisionGrid[ThisX + 1, ThisY]
                        && ThisCollisionGrid[ThisX + 2, ThisY]
                        && ThisCollisionGrid[ThisX, ThisY - 1]
                        && ThisCollisionGrid[ThisX + 1, ThisY - 1]
                        && IsValid)
                    {
                        //gameData.method_1("Attack from Left!", Color.OrangeRed);
                        AttackPosFound = true;
                        ChoosenAttackLocation = 2; //Attack from Left
                        ReturnPos = new Position { X = ThisX + gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.X, Y = ThisY + gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.Y };
                    }
                    else
                    {
                        //change attack location to top
                        ThisX += 3;
                        ThisY -= 5;

                        ChoosenAttackLocation++;
                    }
                }

                //check boundary for attacking the mobs from Up position (NOT RECOMMENDED FOR HAMMER)
                if (ChoosenAttackLocation == 3)
                {
                    //#####
                    //Check Validity
                    bool IsValid = true;
                    if (ThisX < 1) IsValid = false;
                    if (ThisY < 1) IsValid = false;
                    if (ThisX > ThisCollisionGrid.GetLength(0) - 2) IsValid = false;
                    if (ThisY > ThisCollisionGrid.GetLength(1) - 2) IsValid = false;
                    //#####

                    if (ThisCollisionGrid[ThisX, ThisY]
                        && ThisCollisionGrid[ThisX - 1, ThisY]
                        && ThisCollisionGrid[ThisX + 1, ThisY]
                        && ThisCollisionGrid[ThisX, ThisY - 1]
                        && ThisCollisionGrid[ThisX, ThisY + 1]
                        && IsValid)
                    {
                        //gameData.method_1("Attack from Top!", Color.OrangeRed);
                        AttackPosFound = true;
                        ChoosenAttackLocation = 3; //Attack from Top
                        ReturnPos = new Position { X = ThisX + gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.X, Y = ThisY + gameData.mapAreaStruc.AllMapData[(int)gameData.playerScan.levelNo - 1].Offset.Y };
                    }
                    else
                    {
                        gameData.method_1("No Attack pos found!", Color.Red);
                        //no atack pos found??
                        AttackPosFound = true;
                        ChoosenAttackLocation++; //return attack pos = 4 (for error)
                        ReturnPos = new Position { X = ThisAttackPos.X, Y = ThisAttackPos.Y };
                        //ReturnPos = new Position { X = 0, Y = 0 };
                    }
                }
            }

        }
        catch { }

        return ReturnPos;
    }

    public void CastDefense()
    {
        if (CharConfig.UseBO && !gameData.townStruc.GetInTown())
        {
            gameData.potions.CheckIfWeUsePotion();

            gameData.keyMouse.PressKey(CharConfig.KeySwapWeapon);
            gameData.WaitDelay(15);
            //gameData.keyMouse.PressKey(CharConfig.KeySkillBattleOrder);
            gameData.keyMouse.PressKey(CharConfig.KeySkillBattleCommand);
            gameData.WaitDelay(10);
            /*gameData.keyMouse.MouseClicc(1025, 1025);
            gameData.WaitDelay(5);
            gameData.keyMouse.MouseClicc(1095, 610);
            gameData.WaitDelay(5);*/
            gameData.playerScan.GetPositions();

            //press W again to switch weapon again
            //if (gameData.playerScan.RightSkill != Enums.Skill.BattleOrders)
            if (gameData.playerScan.RightSkill != Enums.Skill.BattleCommand)
            {
                gameData.keyMouse.PressKey(CharConfig.KeySwapWeapon);
                gameData.WaitDelay(15);
                //gameData.keyMouse.PressKey(CharConfig.KeySkillBattleOrder);
                gameData.keyMouse.PressKey(CharConfig.KeySkillBattleCommand);
                gameData.WaitDelay(10);
                /*gameData.keyMouse.MouseClicc(1025, 1025);
                gameData.WaitDelay(5);
                gameData.keyMouse.MouseClicc(1095, 610);
                gameData.WaitDelay(5);*/
                gameData.playerScan.GetPositions();
            }

            gameData.keyMouse.MouseCliccRight_RealPos(gameData.CenterX, gameData.CenterY);
            gameData.WaitDelay(35);

            //select battle command
            //gameData.keyMouse.PressKey(CharConfig.KeySkillBattleCommand);
            gameData.keyMouse.PressKey(CharConfig.KeySkillBattleOrder);
            gameData.WaitDelay(10);
            /*gameData.keyMouse.MouseClicc(1025, 1025);
            gameData.WaitDelay(5);
            gameData.keyMouse.MouseClicc(1025, 610);
            gameData.WaitDelay(5);*/
            gameData.keyMouse.MouseCliccRight_RealPos(gameData.CenterX, gameData.CenterY);
            gameData.WaitDelay(35); //60 <-
            gameData.potions.CheckIfWeUsePotion();

            //select battle cry
            gameData.keyMouse.PressKey(CharConfig.KeySkillBattleCry);
            gameData.WaitDelay(10);
            /*gameData.keyMouse.MouseClicc(1025, 1025);
            gameData.WaitDelay(5);
            gameData.keyMouse.MouseClicc(1165, 610);
            gameData.WaitDelay(5);*/
            gameData.keyMouse.MouseCliccRight_RealPos(gameData.CenterX, gameData.CenterY);
            gameData.WaitDelay(60);

            gameData.keyMouse.PressKey(CharConfig.KeySwapWeapon);
            gameData.WaitDelay(15);
            gameData.playerScan.GetPositions();
        }

        //press W again to switch weapon again
        if (gameData.playerScan.RightSkill == Enums.Skill.BattleCry
            || gameData.playerScan.RightSkill == Enums.Skill.BattleOrders
            || gameData.playerScan.RightSkill == Enums.Skill.BattleCommand)
        {
            gameData.keyMouse.PressKey(CharConfig.KeySwapWeapon);
            gameData.WaitDelay(15);
            gameData.playerScan.GetPositions();
        }

        //cast sacred shield
        gameData.keyMouse.PressKey(CharConfig.KeySkillCastDefense);
        gameData.WaitDelay(5);
        gameData.keyMouse.MouseCliccRight_RealPos(gameData.CenterX, gameData.CenterY);
        gameData.WaitDelay(35);

        //cast sacred shield
        gameData.keyMouse.PressKey(CharConfig.KeySkillLifeAura);
        gameData.WaitDelay(5);
        gameData.keyMouse.MouseCliccRight_RealPos(gameData.CenterX, gameData.CenterY);
        gameData.WaitDelay(5);

        TimeSinceLastCast = DateTime.Now;
    }

    public bool ClearAreaOfMobs(int ThisX, int ThisY, int ClearSize)
    {
        AreaX = ThisX;
        AreaY = ThisY;
        IgnoredMobsPointer = new List<long>();
        ClearingSize = ClearSize;
        AttackNotRegisteredCount = 0;
        MoveTryCount = 0;
        //ClearingFullArea = false;

        //ClearingArea = true;
        if (gameData.mobsStruc.GetMobs("", "", true, ClearingSize, IgnoredMobsPointer))
        {
            ClearingArea = true;
            return true;
        }
        return false;
    }

    public void ClearFullAreaOfMobs()
    {
        IgnoredMobsPointer = new List<long>();
        AttackNotRegisteredCount = 0;
        MoveTryCount = 0;
        ClearingSize = 500;
        ClearingFullArea = true;
        DoingRoomIndex = 0;

        AllRooms_InArea = gameData.mapAreaStruc.AllMapData[(int)(gameData.playerScan.levelNo - 1)].Rooms;

        //if (gameData.mobsStruc.GetMobs("", "", true, ClearingSize, IgnoredMobsPointer)) ClearingArea = true;
        ClearingArea = true;
    }

    public void SetBattleMoveAcceptOffset()
    {
        //if (CharConfig.RunningOnChar.ToLower().Contains("sorc")) gameData.mover.MoveAcceptOffset = 10;
        //else gameData.mover.MoveAcceptOffset = 4; //default
    }

    public void ResetBattleMoveAcceptOffset()
    {
        //gameData.mover.MoveAcceptOffset = 4; //default
    }

    public bool IsIncludedInList(List<int> IgnoredIDList, int ThisID)
    {
        if (IgnoredIDList != null)
        {
            if (IgnoredIDList.Count > 0)
            {
                for (int i = 0; i < IgnoredIDList.Count; i++)
                {
                    if (IgnoredIDList[i] == ThisID)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public void RemoveCurrentRoomFromClearing()
    {
        //List<Room> AllRooms = gameData.mapAreaStruc.AllMapData[(int)(gameData.playerScan.levelNo - 1)].Rooms;
        int LastRoomXIndex = 0;
        for (int i = 1; i < AllRooms_InArea.Count; i++)
        {
            if (AllRooms_InArea[i].X == AllRooms_InArea[0].X) break;
            LastRoomXIndex++;
        }

        //Remove the Rooms we just done clearing
        List<int> RemovingRoomsAt = new List<int>();
        for (int i = 0; i < AllRooms_InArea.Count; i++)
        {
            if (gameData.playerScan.xPosFinal >= AllRooms_InArea[i].X && gameData.playerScan.xPosFinal <= AllRooms_InArea[i].X + AllRooms_InArea[i].Width
                && gameData.playerScan.yPosFinal >= AllRooms_InArea[i].Y && gameData.playerScan.yPosFinal <= AllRooms_InArea[i].Y + AllRooms_InArea[i].Height)
            {
                DoingRoomIndex = i;
                RemovingRoomsAt.Add(i + LastRoomXIndex);
                RemovingRoomsAt.Add(i + 1);
                RemovingRoomsAt.Add(i);
                RemovingRoomsAt.Add(i - 1);
                RemovingRoomsAt.Add(i - LastRoomXIndex);
                break;
            }
        }

        for (int i = 0; i < RemovingRoomsAt.Count; i++)
        {
            if (RemovingRoomsAt[i] < AllRooms_InArea.Count)
            {
                try
                {
                    if (!IsIncludedInList(IgnoredRooms_InArea, RemovingRoomsAt[i]))
                    {
                        IgnoredRooms_InArea.Add(RemovingRoomsAt[i]);
                        //gameData.method_1("Removed Room: " + RemovingRoomsAt[i] + ", remaining: " + (AllRooms_InArea.Count - IgnoredRooms_InArea.Count), Color.Red);
                    }
                }
                catch { }
            }
        }
    }

    public void RunBattleScript()
    {
        if ((Enums.Area)gameData.playerScan.levelNo == Enums.Area.ThroneOfDestruction)
        {
            //15096,5096
            if (gameData.playerScan.yPosFinal > 5096)
            {
                DoingBattle = false;
                FirstAttackCasted = false;
                ResetBattleMoveAcceptOffset();
                if (!ClearingFullArea) gameData.pathFinding.MoveToThisPos(new Position { X = AreaX, Y = AreaY });
                //gameData.mover.MoveToLocation(AreaX, AreaY);
                ClearingArea = false;
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                return;
            }
        }

        if (gameData.mobsStruc.GetMobs("", "", true, ClearingSize, IgnoredMobsPointer))
        {
            if (CharConfig.RunBaalScript && !((Baal)gameData.baal).ScriptDone && gameData.mobsStruc.MobsName == "BaalSubject5") ((Baal)gameData.baal).Wave5Detected = true;
            if (CharConfig.RunBaalScript && !((Baal)gameData.baal).ScriptDone && (Enums.Area) gameData.playerScan.levelNo == Enums.Area.ThroneOfDestruction) ((Baal)gameData.baal).TimeSinceLastWaveDone = DateTime.MaxValue;

            DoingBattle = true;
            SetBattleMoveAcceptOffset();
            gameData.mover.MoveAcceptOffset = 2;
            Position ThisAttackPos = GetBestAttackLocation(new Position { X = gameData.mobsStruc.xPosFinal + 1, Y = gameData.mobsStruc.yPosFinal + 5 });
            if (ThisAttackPos.X != 0 && ThisAttackPos.Y != 0)
            {
                if (!gameData.mover.MoveToLocationAttack(ThisAttackPos.X, ThisAttackPos.Y))
                {
                    TriedToMoveToMobsCount++;
                    if (TriedToMoveToMobsCount >= 2)
                    {
                        ThisAttackPos = ResetMovePostionInBetween(ThisAttackPos);
                        gameData.mover.MoveToLocationAttack(ThisAttackPos.X, ThisAttackPos.Y);
                        TriedToMoveToMobsCount = 0;
                    }
                }
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            }
            //gameData.mover.MoveToLocationAttack(gameData.mobsStruc.xPosFinal - 1, gameData.mobsStruc.yPosFinal + 2);
            gameData.mover.MoveAcceptOffset = 4;
            ResetBattleMoveAcceptOffset();

            FirstAttackCasting();
            SetSkills();
            CastSkills();
            if (CharConfig.RunningOnChar == "PaladinHammer")
            {
                CastSkills();
                CastSkills();
            }
            AttackTryCheck();

            if (ClearingFullArea && IgnoredRooms_InArea.Count < AllRooms_InArea.Count)
            {
                //Remove the Rooms we just done clearing
                RemoveCurrentRoomFromClearing();
            }
        }
        else
        {
            if (ClearingFullArea && (AllRooms_InArea.Count - IgnoredRooms_InArea.Count) > 0)
            {
                if ((DateTime.Now - TimeSinceLastCast).TotalSeconds > CharConfig.RecastBODelay)
                {
                    CastDefense();
                }
                //"x":25320, "y":6100, "width":40, "height":40

                //Remove the Rooms we just done clearing
                RemoveCurrentRoomFromClearing();

                if (DoingRoomIndex > 0) DoingRoomIndex--;
                while (IsIncludedInList(IgnoredRooms_InArea, DoingRoomIndex)) DoingRoomIndex--;
                if (DoingRoomIndex < 0)
                {
                    DoingRoomIndex = 0;
                    while (IsIncludedInList(IgnoredRooms_InArea, DoingRoomIndex)) DoingRoomIndex++;
                }
                //if (DoingRoomIndex > AllRooms_InArea.Count - 1) DoingRoomIndex = AllRooms_InArea.Count - 1;
                if (DoingRoomIndex > AllRooms_InArea.Count - 1)
                {
                    gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                    gameData.mobsStruc.xPosFinal = 0;
                    gameData.mobsStruc.yPosFinal = 0;
                    //if (CharConfig.RunBaalScript && !gameData.baal.ScriptDone && gameData.baal.Wave5Detected) gameData.baal.Wave5Cleared = true;
                    TriedToMoveToMobsCount = 0;
                    DoingBattle = false;
                    FirstAttackCasted = false;
                    ResetBattleMoveAcceptOffset();
                    if (!ClearingFullArea) gameData.pathFinding.MoveToThisPos(new Position { X = AreaX, Y = AreaY });
                    //gameData.mover.MoveToLocation(AreaX, AreaY);
                    ClearingArea = false;
                    AreaIDFullyCleared = (int) gameData.playerScan.levelNo;
                    return;
                }

                //Go to next room
                bool[,] ThisCollisionGrid = gameData.mapAreaStruc.CollisionGrid((Enums.Area)gameData.playerScan.levelNo);
                int RoomStartX = AllRooms_InArea[DoingRoomIndex].X - gameData.mapAreaStruc.AllMapData[(int)(gameData.playerScan.levelNo - 1)].Offset.X;
                int RoomStartY = AllRooms_InArea[DoingRoomIndex].Y - gameData.mapAreaStruc.AllMapData[(int)(gameData.playerScan.levelNo - 1)].Offset.Y;
                int RoomSizeX = AllRooms_InArea[DoingRoomIndex].Width;
                int RoomSizeY = AllRooms_InArea[DoingRoomIndex].Height;

                Position MovingToPos = new Position { X = AllRooms_InArea[DoingRoomIndex].X, Y = AllRooms_InArea[DoingRoomIndex].Y };
                bool FoundWalkablePath = false;
                //gameData.method_1("Check:" + RoomStartX + ", " + RoomStartY, Color.Red);
                //gameData.method_1("Check size:" + RoomSizeX + ", " + RoomSizeY, Color.Red);
                for (int i = RoomStartX; i < RoomStartX + RoomSizeX; i++)
                {
                    for (int k = RoomStartY; k < RoomStartY + RoomSizeY; k++)
                    {
                        if (ThisCollisionGrid[i, k])
                        {
                            FoundWalkablePath = true;
                            MovingToPos = new Position { X = i + gameData.mapAreaStruc.AllMapData[(int)(gameData.playerScan.levelNo - 1)].Offset.X, Y = k + gameData.mapAreaStruc.AllMapData[(int)(gameData.playerScan.levelNo - 1)].Offset.Y };
                        }
                    }
                }
                if (FoundWalkablePath)
                {
                    //gameData.pathFinding.MoveToThisPos(MovingToPos);
                    gameData.pathFinding.MoveToThisPos(MovingToPos, 4, true);
                }
                else
                {
                    if (!IsIncludedInList(IgnoredRooms_InArea, DoingRoomIndex)) IgnoredRooms_InArea.Add(DoingRoomIndex);
                }
            }
            else
            {
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                gameData.mobsStruc.xPosFinal = 0;
                gameData.mobsStruc.yPosFinal = 0;
                //if (CharConfig.RunBaalScript && !gameData.baal.ScriptDone && gameData.baal.Wave5Detected) gameData.baal.Wave5Cleared = true;
                TriedToMoveToMobsCount = 0;
                DoingBattle = false;
                FirstAttackCasted = false;
                ResetBattleMoveAcceptOffset();
                if (!ClearingFullArea) gameData.pathFinding.MoveToThisPos(new Position { X = AreaX, Y = AreaY });
                //gameData.mover.MoveToLocation(AreaX, AreaY);
                ClearingArea = false;
                AreaIDFullyCleared = (int)gameData.playerScan.levelNo;
            }
        }
    }

    public bool DoBattleScript(int MaxDistance)
    {
        if (gameData.mobsStruc.GetMobs("", "", true, MaxDistance, new List<long>()))
        {
            if (CharConfig.RunBaalScript && !((Baal)gameData.baal).ScriptDone && gameData.mobsStruc.MobsName == "BaalSubject5") ((Baal)gameData.baal).Wave5Detected = true;
            if (CharConfig.RunBaalScript && !((Baal)gameData.baal).ScriptDone && (Enums.Area)gameData.playerScan.levelNo == Enums.Area.ThroneOfDestruction) ((Baal)gameData.baal).TimeSinceLastWaveDone = DateTime.MaxValue;
            DoingBattle = true;
            SetBattleMoveAcceptOffset();
            gameData.mover.MoveAcceptOffset = 2;
            Position ThisAttackPos = GetBestAttackLocation(new Position { X = gameData.mobsStruc.xPosFinal + 1, Y = gameData.mobsStruc.yPosFinal + 5 });
            if (ThisAttackPos.X != 0 && ThisAttackPos.Y != 0)
            {
                if (!gameData.mover.MoveToLocationAttack(ThisAttackPos.X, ThisAttackPos.Y))
                {
                    TriedToMoveToMobsCount++;
                    if (TriedToMoveToMobsCount >= 2)
                    {
                        ThisAttackPos = ResetMovePostionInBetween(ThisAttackPos);
                        gameData.mover.MoveToLocationAttack(ThisAttackPos.X, ThisAttackPos.Y);
                        TriedToMoveToMobsCount = 0;
                    }
                }
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            }
            //gameData.mover.MoveToLocationAttack(gameData.mobsStruc.xPosFinal - 1, gameData.mobsStruc.yPosFinal + 2);
            gameData.mover.MoveAcceptOffset = 4;
            ResetBattleMoveAcceptOffset();

            FirstAttackCasting();
            SetSkills();
            CastSkills();
            if (CharConfig.RunningOnChar == "PaladinHammer")
            {
                CastSkills();
                CastSkills();
            }
            AttackTryCheck();
            return true;
        }

        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        gameData.mobsStruc.xPosFinal = 0;
        gameData.mobsStruc.yPosFinal = 0;
        //if (CharConfig.RunBaalScript && !gameData.baal.ScriptDone && gameData.baal.Wave5Detected) gameData.baal.Wave5Cleared = true;
        TriedToMoveToMobsCount = 0;
        DoingBattle = false;
        FirstAttackCasted = false;
        return false;
    }

    public void RunBattleScriptOnLastMob(List<long> IgnoredIDList)
    {
        IgnoredMobsPointer = IgnoredIDList;
        if (gameData.mobsStruc.GetMobs(LastMobType, LastMobName, false, 200, IgnoredIDList))
        {
            if (CharConfig.RunBaalScript && !((Baal)gameData.baal).ScriptDone && gameData.mobsStruc.MobsName == "BaalSubject5") ((Baal)gameData.baal).Wave5Detected = true;
            if (CharConfig.RunBaalScript && !((Baal)gameData.baal).ScriptDone && (Enums.Area)gameData.playerScan.levelNo == Enums.Area.ThroneOfDestruction) ((Baal)gameData.baal).TimeSinceLastWaveDone = DateTime.MaxValue;
            if (gameData.mobsStruc.MobsHP > 0)
            {
                DoingBattle = true;
                SetBattleMoveAcceptOffset();
                gameData.mover.MoveAcceptOffset = 2;
                Position ThisAttackPos = GetBestAttackLocation(new Position { X = gameData.mobsStruc.xPosFinal + 1, Y = gameData.mobsStruc.yPosFinal + 5 });
                if (ThisAttackPos.X != 0 && ThisAttackPos.Y != 0)
                {
                    if (!gameData.mover.MoveToLocationAttack(ThisAttackPos.X, ThisAttackPos.Y))
                    {
                        TriedToMoveToMobsCount++;
                        if (TriedToMoveToMobsCount >= 2)
                        {
                            ThisAttackPos = ResetMovePostionInBetween(ThisAttackPos);
                            gameData.mover.MoveToLocationAttack(ThisAttackPos.X, ThisAttackPos.Y);
                            TriedToMoveToMobsCount = 0;
                        }
                    }
                    gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                }
                //gameData.mover.MoveToLocationAttack(gameData.mobsStruc.xPosFinal - 1, gameData.mobsStruc.yPosFinal + 2);
                gameData.mover.MoveAcceptOffset = 4;
                ResetBattleMoveAcceptOffset();


                FirstAttackCasting();
                SetSkills();
                CastSkills();
                if (CharConfig.RunningOnChar == "PaladinHammer")
                {
                    CastSkills();
                    CastSkills();
                }
                AttackTryCheck();
            }
            else
            {
                //LastMobType = "";
                //LastMobName = "";
                gameData.mobsStruc.xPosFinal = 0;
                gameData.mobsStruc.yPosFinal = 0;
                //if (CharConfig.RunBaalScript && !gameData.baal.ScriptDone && gameData.baal.Wave5Detected) gameData.baal.Wave5Cleared = true;
                TriedToMoveToMobsCount = 0;
                DoingBattle = false;
                FirstAttackCasted = false;
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            }
        }
        else
        {
            //LastMobType = "";
            //LastMobName = "";
            gameData.mobsStruc.xPosFinal = 0;
            gameData.mobsStruc.yPosFinal = 0;
            //if (CharConfig.RunBaalScript && !gameData.baal.ScriptDone && gameData.baal.Wave5Detected) gameData.baal.Wave5Cleared = true;
            TriedToMoveToMobsCount = 0;
            DoingBattle = false;
            FirstAttackCasted = false;
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        }
    }

    public void RunBattleScriptOnThisMob(string MobType, string MobName, List<long> IgnoredIDList)
    {
        LastMobType = MobType;
        LastMobName = MobName;
        RunBattleScriptOnLastMob(IgnoredIDList);
    }

    public Position ResetMovePostionInBetween(Position ThisPos)
    {
        Position ReturnPos = new Position { };
        ReturnPos.X = 0;
        ReturnPos.Y = 0;

        if (ThisPos.X >= gameData.playerScan.xPosFinal) ReturnPos.X = ThisPos.X - ((ThisPos.X - gameData.playerScan.xPosFinal) / 2);
        if (ThisPos.Y >= gameData.playerScan.yPosFinal) ReturnPos.Y = ThisPos.Y - ((ThisPos.Y - gameData.playerScan.yPosFinal) / 2);
        if (ThisPos.X < gameData.playerScan.xPosFinal) ReturnPos.X = ThisPos.X + ((gameData.playerScan.xPosFinal - ThisPos.X) / 2);
        if (ThisPos.Y < gameData.playerScan.yPosFinal) ReturnPos.Y = ThisPos.Y + ((gameData.playerScan.yPosFinal - ThisPos.Y) / 2);

        return ReturnPos;
    }

    public void MoveAway()
    {
        int MoveDistance = 5;
        //gameData.WaitDelay(5); //wait a little bit, we just casted attack
        if (MoveTryCount == 1)
        {
            gameData.mover.MoveAcceptOffset = 2;
            gameData.mover.MoveToLocationAttack(gameData.playerScan.xPosFinal + MoveDistance, gameData.playerScan.yPosFinal + MoveDistance);
            gameData.mover.MoveAcceptOffset = 4;
        }
        if (MoveTryCount == 2)
        {
            gameData.mover.MoveAcceptOffset = 2;
            gameData.mover.MoveToLocationAttack(gameData.playerScan.xPosFinal - MoveDistance, gameData.playerScan.yPosFinal + MoveDistance);
            gameData.mover.MoveAcceptOffset = 4;
        }
        if (MoveTryCount == 3)
        {
            gameData.mover.MoveAcceptOffset = 2;
            gameData.mover.MoveToLocationAttack(gameData.playerScan.xPosFinal + MoveDistance, gameData.playerScan.yPosFinal - MoveDistance);
            gameData.mover.MoveAcceptOffset = 4;
        }
        if (MoveTryCount == 4)
        {
            gameData.mover.MoveAcceptOffset = 2;
            gameData.mover.MoveToLocationAttack(gameData.playerScan.xPosFinal - MoveDistance, gameData.playerScan.yPosFinal - MoveDistance);
            gameData.mover.MoveAcceptOffset = 4;
        }
    }

    public void AttackTryCheck()
    {
        gameData.potions.CheckIfWeUsePotion();
        gameData.mobsStruc.GetLastMobs();
        //long AttackedThisPointer = gameData.mobsStruc.LastMobsPointerLocation;

        //if (AttackedThisPointer == LastMobAttackedPointer)
        //{
        if (gameData.mobsStruc.MobsHP >= LastMobAttackedHP)
        {
            AttackNotRegisteredCount++;
            //gameData.method_1("Attack not registered! " + AttackNotRegisteredCount + "/" + MaxAttackTry, Color.OrangeRed);

            if (AttackNotRegisteredCount >= CharConfig.MaxBattleAttackTries)
            {
                AttackNotRegisteredCount = 0;
                MoveTryCount++;
                gameData.method_1("Attack not registered, moving away! " + MoveTryCount + "/" + MaxMoveTry, Color.OrangeRed);
                MoveAway();

                if (MoveTryCount >= MaxMoveTry)
                {
                    MoveTryCount = 0;
                    IgnoredMobsPointer.Add(gameData.mobsStruc.LastMobsPointerLocation);
                }
            }
        }
        else
        {
            //gameData.method_1("Attack registered! " + AttackNotRegisteredCount + "/" + MaxAttackTry, Color.DarkGreen);
            AttackNotRegisteredCount = 0;
            MoveTryCount = 0;
        }
        /*}
        else
        {
            AttackNotRegisteredCount = 0;
            MoveTryCount = 0;
        }*/

        //LastMobAttackedPointer = gameData.mobsStruc.LastMobsPointerLocation;
        LastMobAttackedHP = gameData.mobsStruc.MobsHP;
    }

    public void SetSkills()
    {
        gameData.keyMouse.PressKey(CharConfig.KeySkillAttack);
        gameData.keyMouse.PressKey(CharConfig.KeySkillAura);
    }

    public void CastSkills()
    {
        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        if (gameData.mobsStruc.xPosFinal != 0 && gameData.mobsStruc.yPosFinal != 0)
        {
            gameData.playerScan.GetPositions();
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.mobsStruc.xPosFinal, gameData.mobsStruc.yPosFinal);
            if (!CharConfig.PlayerAttackWithRightHand)
            {
                gameData.keyMouse.SendSHIFT_CLICK_ATTACK(itemScreenPos.X, itemScreenPos.Y - 30);
            }
            else
            {
                gameData.keyMouse.MouseCliccRightAttackMove(itemScreenPos.X, itemScreenPos.Y - 30);
            }
        }
        else
        {
            if (!CharConfig.PlayerAttackWithRightHand)
            {
                gameData.keyMouse.SendSHIFT_CLICK_ATTACK(gameData.CenterX, gameData.CenterY - 1);
            }
            else
            {
                gameData.keyMouse.MouseCliccRightAttackMove(gameData.CenterX, gameData.CenterY - 1);
            }
        }
        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        //gameData.WaitDelay(5);
        //gameData.WaitDelay(1);
    }

    public void CastSkillsNoMove()
    {
        if (gameData.mobsStruc.xPosFinal != 0 && gameData.mobsStruc.yPosFinal != 0)
        {
            gameData.playerScan.GetPositions();
            Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, gameData.mobsStruc.xPosFinal, gameData.mobsStruc.yPosFinal);
            if (!CharConfig.PlayerAttackWithRightHand)
            {
                //gameData.keyMouse.SendSHIFT_CLICK_ATTACK(itemScreenPos.X, itemScreenPos.Y - 30);
                gameData.keyMouse.SendSHIFT_CLICK_ATTACK_CAST_NO_MOVE(itemScreenPos.X, itemScreenPos.Y - 30);
            }
            else
            {
                gameData.keyMouse.MouseCliccRightAttackMove(itemScreenPos.X, itemScreenPos.Y - 30);
            }
        }
        else
        {
            if (!CharConfig.PlayerAttackWithRightHand)
            {
                //gameData.keyMouse.SendSHIFT_CLICK_ATTACK(gameData.CenterX, gameData.CenterY - 1);
                gameData.keyMouse.SendSHIFT_CLICK_ATTACK_CAST_NO_MOVE(gameData.CenterX, gameData.CenterY - 1);
            }
            else
            {
                gameData.keyMouse.MouseCliccRightAttackMove(gameData.CenterX, gameData.CenterY - 1);
            }
        }
        //gameData.WaitDelay(5);
        //gameData.WaitDelay(1);
    }

    public void FirstAttackCasting()
    {
        if (!FirstAttackCasted)
        {
            if (CharConfig.RunningOnChar == "SorceressBlizzard")
            {
                gameData.keyMouse.PressKey(CharConfig.KeySkillAttack); //select static

                int tryes = 0;
                while (tryes < 6)
                {
                    CastSkills();
                    gameData.WaitDelay(35);
                    tryes++;
                }
            }

            FirstAttackCasted = true;
        }
    }
}
