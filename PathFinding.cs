using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using static Enums;
using static MapAreaStruc;

public class PathFinding
{
    // Define directions (assuming 4-direction movement)
    private int[] dx = { 1, 0, -1, 0, -1, 1, -1, 1 };
    private int[] dy = { 0, 1, 0, -1, -1, -1, 1, 1 };

    public Form1 Form1_0;

    public int TeleportAcceptSize = 20; //not implemented yet
    public int AcceptMoveOffset = 25;
    public int ThisPlayerAreaID = 0;
    public int ThisNextAreaID = 0;

    public bool IsMovingToNextArea = false;

    public Position LastMovingLocation = new Position { X = 0, Y = 0 };
    public Position ThisFinalPosition = new Position { X = 0, Y = 0 };
    public Position ThisOffsetPosition = new Position { X = 0, Y = 0 };

    public bool[,] ThisCollisionGrid = new bool[0, 0];

    public bool IsMovingThruPath = false;
    public int CurrentPathIndex = 0;
    public List<Point> path = new List<Point>();

    public Position PlayerOffsetInCollisiongrid = new Position { X = 0, Y = 0 };
    public Position TargetOffsetInCollisiongrid = new Position { X = 0, Y = 0 };

    public bool CheckingForCloseToTargetPos = true;

    public void SetForm1(Form1 form1_1)
    {
        Form1_0 = form1_1;
    }

    public void CheckForBadMapData(int Thisindex)
    {
        while (Thisindex > GameData.Instance.mapAreaStruc.AllMapData.Count - 1)
        {
            GameData.Instance.mapAreaStruc.GetMapData(GameData.Instance.playerScan.mapSeedValue.ToString(), (Difficulty)GameData.Instance.playerScan.difficulty);
        }
    }

    public void DebugMapCollision()
    {
        Form1_0.ClearDebugCollision();

        ThisCollisionGrid = GameData.Instance.mapAreaStruc.CollisionGrid((Enums.Area)Form1_0.comboBoxCollisionArea.SelectedIndex + 1);
        for (int i = 0; i < ThisCollisionGrid.GetLength(1); i++)
        {
            for (int k = 0; k < ThisCollisionGrid.GetLength(0); k++)
            {
                if (ThisCollisionGrid[k, i]) Form1_0.AppendTextDebugCollision("-");
                if (!ThisCollisionGrid[k, i]) Form1_0.AppendTextDebugCollision("X");
            }
            Form1_0.AppendTextDebugCollision(Environment.NewLine);
        }
    }

    public bool MoveToNextArea(Area ThisID, int AcceptOffset = 4, bool ClearAreaOnMoving = false)
    {
        GameData.Instance.playerScan.GetPositions();
        IsMovingToNextArea = true;
        ThisNextAreaID = (int)ThisID;
        ThisPlayerAreaID = (int)GameData.Instance.playerScan.levelNo;
        //ThisCollisionGrid = ExpandGrid(ThisID);
        ThisCollisionGrid = MergeCollisionGrids(ThisID);
        CheckingForCloseToTargetPos = true;

        //dump data to txt file
        /*string ColisionMapTxt = "";
        //ThisCollisionGrid = GameData.Instance.mapAreaStruc.CollisionGrid((Enums.Area)GameData.Instance.playerScan.levelNo);
        for (int i = 0; i < ThisCollisionGrid.GetLength(1); i++)
        {
            for (int k = 0; k < ThisCollisionGrid.GetLength(0); k++)
            {
                if (ThisCollisionGrid[k, i]) ColisionMapTxt += "-";
                if (!ThisCollisionGrid[k, i]) ColisionMapTxt += "X";
            }
            ColisionMapTxt += Environment.NewLine;
        }
        File.Create(GameData.Instance.ThisEndPath + "CollisionMap.txt").Dispose();
        File.WriteAllText(GameData.Instance.ThisEndPath + "CollisionMap.txt", ColisionMapTxt);*/

        try
        {
            CheckForBadMapData(ThisPlayerAreaID - 1);

            //The Exit or Object find is only for a reference for pathing to the next AreaID, when we enter the next AreaID it will go out of the pathing loop
            //Get any 'exit' object in the next areaID for path reference (ignore object name)
            ThisFinalPosition = GameData.Instance.mapAreaStruc.GetPositionOfObject("exit", GameData.Instance.townStruc.getAreaName((int)ThisID), (int)ThisID, new List<int>() { }, true);

            //Get any 'object' in the next areaID for path reference in case we didn't find any exit (ignore object name)
            if (ThisFinalPosition.X == 0 && ThisFinalPosition.Y == 0) ThisFinalPosition = GameData.Instance.mapAreaStruc.GetPositionOfObject("object", GameData.Instance.townStruc.getAreaName((int)ThisID), (int)ThisID, new List<int>() { }, true);

            //Console.WriteLine("Going to Pos: " + ThisFinalPosition.X + ", " + ThisFinalPosition.Y);
            return GameData.Instance.pathFinding.GetPathFinding(AcceptOffset, ClearAreaOnMoving);
        }
        catch { }

        return false;
    }

    public bool MoveToExit(Area ThisID, int AcceptOffset = 4, bool ClearAreaOnMoving = false)
    {
        GameData.Instance.playerScan.GetPositions();
        IsMovingToNextArea = false;
        CheckingForCloseToTargetPos = false;

        //GameData.Instance.method_1("ToExit " + GameData.Instance.townStruc.getAreaName((int)ThisID), Color.Red);
        try
        {
            ThisPlayerAreaID = (int)GameData.Instance.playerScan.levelNo;
            CheckForBadMapData(ThisPlayerAreaID - 1);

            ThisFinalPosition = GameData.Instance.mapAreaStruc.GetPositionOfObject("exit", GameData.Instance.townStruc.getAreaName((int)ThisID), ThisPlayerAreaID, new List<int>() { });
            ThisOffsetPosition = new Position { X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X, Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y };
            ThisCollisionGrid = GameData.Instance.mapAreaStruc.CollisionGrid((Enums.Area)GameData.Instance.playerScan.levelNo);

            try
            {
                ThisCollisionGrid[ThisFinalPosition.X - ThisOffsetPosition.X, ThisFinalPosition.Y - ThisOffsetPosition.Y] = true; //make sure the exit is walkable
            }
            catch { }

            PlayerOffsetInCollisiongrid = new Position { X = 0, Y = 0 };
            TargetOffsetInCollisiongrid = new Position { X = 0, Y = 0 };

            //Console.WriteLine("Going to Pos: " + ThisFinalPosition.X + ", " + ThisFinalPosition.Y);
            return GameData.Instance.pathFinding.GetPathFinding(AcceptOffset, ClearAreaOnMoving);
        }
        catch { }

        return false;
    }

    public bool MoveToNPC(string NPCName, int AcceptOffset = 4, bool ClearAreaOnMoving = false)
    {
        GameData.Instance.playerScan.GetPositions();
        IsMovingToNextArea = false;
        CheckingForCloseToTargetPos = false;

        //GameData.Instance.method_1("ToNPC " + NPCName, Color.Red);

        try
        {
            ThisPlayerAreaID = (int)GameData.Instance.playerScan.levelNo;
            CheckForBadMapData(ThisPlayerAreaID - 1);

            ThisFinalPosition = GameData.Instance.mapAreaStruc.GetPositionOfObject("npc", NPCName, ThisPlayerAreaID, new List<int>() { });
            ThisOffsetPosition = new Position { X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X, Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y };
            ThisCollisionGrid = GameData.Instance.mapAreaStruc.CollisionGrid((Enums.Area)GameData.Instance.playerScan.levelNo);

            PlayerOffsetInCollisiongrid = new Position { X = 0, Y = 0 };
            TargetOffsetInCollisiongrid = new Position { X = 0, Y = 0 };

            return GameData.Instance.pathFinding.GetPathFinding(AcceptOffset, ClearAreaOnMoving);
        }
        catch { }

        return false;
    }

    public bool MoveToObject(string ObjectName, int AcceptOffset = 4, bool ClearAreaOnMoving = false)
    {
        GameData.Instance.playerScan.GetPositions();
        IsMovingToNextArea = false;
        CheckingForCloseToTargetPos = false;

        //GameData.Instance.method_1("ToObject " + ObjectName, Color.Red);

        try
        {
            ThisPlayerAreaID = (int)GameData.Instance.playerScan.levelNo;
            CheckForBadMapData(ThisPlayerAreaID - 1);

            ThisFinalPosition = GameData.Instance.mapAreaStruc.GetPositionOfObject("object", ObjectName, ThisPlayerAreaID, new List<int>() { });
            ThisOffsetPosition = new Position { X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X, Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y };
            ThisCollisionGrid = GameData.Instance.mapAreaStruc.CollisionGrid((Enums.Area)GameData.Instance.playerScan.levelNo);

            PlayerOffsetInCollisiongrid = new Position { X = 0, Y = 0 };
            TargetOffsetInCollisiongrid = new Position { X = 0, Y = 0 };

            return GameData.Instance.pathFinding.GetPathFinding(AcceptOffset, ClearAreaOnMoving);
        }
        catch { }

        return false;
    }

    public bool MoveToThisPos(Position ThisPositionn, int AcceptOffset = 4, bool ClearAreaOnMoving = false)
    {
        if (GameData.Instance.playerScan.levelNo == 0) GameData.Instance.playerScan.GetPositions();
        IsMovingToNextArea = false;
        CheckingForCloseToTargetPos = true;

        //GameData.Instance.method_1("ToThisPos", Color.Red);

        try
        {
            ThisPlayerAreaID = (int)GameData.Instance.playerScan.levelNo;
            CheckForBadMapData(ThisPlayerAreaID - 1);

            ThisFinalPosition = ThisPositionn;
            ThisOffsetPosition = new Position { X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X, Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y };
            ThisCollisionGrid = GameData.Instance.mapAreaStruc.CollisionGrid((Enums.Area)GameData.Instance.playerScan.levelNo);

            PlayerOffsetInCollisiongrid = new Position { X = 0, Y = 0 };
            TargetOffsetInCollisiongrid = new Position { X = 0, Y = 0 };

            return GetPathFinding(AcceptOffset, ClearAreaOnMoving);
        }
        catch { }

        return false;
    }

    public bool GetPathFinding(int AcceptOffset = 4, bool ClearAreaOnMoving = false)
    {
        bool MovedCorrectly = false;

        //Console.WriteLine("player: " + GameData.Instance.playerScan.xPos + ", " + GameData.Instance.playerScan.yPos);
        //Console.WriteLine("offset: " + ThisOffsetPosition.X + ", " + ThisOffsetPosition.Y);
        //Console.WriteLine("final: " + ThisFinalPosition.X + ", " + ThisFinalPosition.Y);

        Point startPos = new Point(GameData.Instance.playerScan.xPos - ThisOffsetPosition.X, GameData.Instance.playerScan.yPos - ThisOffsetPosition.Y);
        Point targetPos = new Point(ThisFinalPosition.X - ThisOffsetPosition.X, ThisFinalPosition.Y - ThisOffsetPosition.Y);
        //Point startPos = new Point(115, 579);
        //Point targetPos = new Point(41, 429);

        startPos.X += PlayerOffsetInCollisiongrid.X;
        startPos.Y += PlayerOffsetInCollisiongrid.Y;

        targetPos.X += TargetOffsetInCollisiongrid.X;
        targetPos.Y += TargetOffsetInCollisiongrid.Y;

        //no need to move we are close already!
        if (CheckingForCloseToTargetPos)
        {
            if (startPos.X >= (targetPos.X - GameData.Instance.mover.MoveAcceptOffset)
                && startPos.X <= (targetPos.X + GameData.Instance.mover.MoveAcceptOffset)
                && startPos.Y >= (targetPos.Y - GameData.Instance.mover.MoveAcceptOffset)
                && startPos.Y <= (targetPos.Y + GameData.Instance.mover.MoveAcceptOffset))
            {
                return true;
            }
        }

        //GameData.Instance.method_1("Start pos: " + startPos.X + ", " + startPos.Y, Color.Red);
        //GameData.Instance.method_1("Target pos: " + targetPos.X + ", " + targetPos.Y, Color.Red);

        /*if (ThisOffsetPosition.X == 0 && ThisOffsetPosition.Y == 0)
        {
            GameData.Instance.method_1("Offsets are bad!", Color.Red);
        }*/
        if (targetPos.X <= 0 || targetPos.Y <= 0)
        {
            GameData.Instance.method_1("Target pos are bad: " + targetPos.X + ", " + targetPos.Y, Color.OrangeRed);
            return false;
        }

        path = FindPath(startPos, targetPos);
        if (path == null)
        {
            GameData.Instance.method_1("No path found.", Color.Red);
            //GameData.Instance.mapAreaStruc.DumpMap();
            Form1_0.GoToNextScript();
            return false;
        }

        //################################################
        //Shorten the path so we don't go at each single unit
        int ThisOffsetToUse = AcceptMoveOffset;
        if (GameData.Instance.townStruc.IsInTown) ThisOffsetToUse = 5;
        else if (!CharConfig.UseTeleport) ThisOffsetToUse = 5;
        else if (CharConfig.UseTeleport && !GameData.Instance.townStruc.GetInTown() && (Enums.Area)GameData.Instance.playerScan.levelNo == Enums.Area.ArcaneSanctuary) ThisOffsetToUse = 1;
        //else if(CharConfig.UseTeleport && !GameData.Instance.townStruc.GetInTown()) ThisOffsetToUse = 1;

        List<Point> pathShortened = new List<Point>();
        int LastPathAdded = 0;
        Point LastPoint = new Point();
        for (int i = 0; i < path.Count; i += ThisOffsetToUse)
        {
            if (ThisOffsetToUse == 1)
            {
                if (path[i].X >= LastPoint.X - 2 && path[i].X <= LastPoint.X + 2
                    && path[i].Y >= LastPoint.Y - 2 && path[i].Y <= LastPoint.Y + 2)
                {
                    LastPoint = path[i];
                    continue;
                }
            }
            LastPoint = path[i];
            pathShortened.Add(path[i]);
            LastPathAdded = i;
        }
        if (LastPathAdded != path.Count - 1) pathShortened.Add(path[path.Count - 1]);
        path = pathShortened;
        //################################################

        IsMovingThruPath = false;
        CurrentPathIndex = 0;
        int BadPathIndexount = 0;
        if (path != null)
        {
            IsMovingThruPath = true;
            while (IsMovingThruPath)
            {
                GameData.Instance.playerScan.GetPositions();
                ThisPlayerAreaID = (int)GameData.Instance.playerScan.levelNo;

                //we are close to accept offset, stop moving to path
                if (IsCloseToLocation(ThisFinalPosition, AcceptOffset))
                {
                    IsMovingThruPath = false;
                    break;
                }

                //we are within the next area, stop moving to path
                if (IsMovingToNextArea)
                {
                    if (ThisPlayerAreaID == ThisNextAreaID)
                    {
                        path = null;
                        CurrentPathIndex = 0;
                        IsMovingThruPath = false;
                        break;
                    }
                }

                if (CurrentPathIndex > path.Count - 1)
                {
                    IsMovingThruPath = false;
                    break;
                }

                DetectCloserPathPositions();

                if (CurrentPathIndex < path.Count - 1)
                {
                    GameData.Instance.mover.MoveAcceptOffset = 7;
                }
                else
                {
                    GameData.Instance.mover.MoveAcceptOffset = 4;
                }

                //Console.WriteLine("Pos test: " + path[CurrentPathIndex].X + ", " + path[CurrentPathIndex].Y);
                if (GameData.Instance.mover.MoveToLocation(path[CurrentPathIndex].X + ThisOffsetPosition.X - PlayerOffsetInCollisiongrid.X, path[CurrentPathIndex].Y + ThisOffsetPosition.Y - PlayerOffsetInCollisiongrid.Y, false, false))
                {
                    BadPathIndexount = 0;
                    CurrentPathIndex++;
                    GameData.Instance.overlayForm.SetPathPoints(path, CurrentPathIndex, ThisOffsetPosition, PlayerOffsetInCollisiongrid);
                    if (CurrentPathIndex >= path.Count - 1)
                    {
                        IsMovingThruPath = false;
                    }
                    else
                    {
                        if (ClearAreaOnMoving)
                        {
                            if (GameData.Instance.battle.ClearingFullArea && GameData.Instance.battle.AllRooms_InArea.Count > 0)
                            {
                                //Remove the Rooms we just done clearing
                                GameData.Instance.battle.RemoveCurrentRoomFromClearing();
                            }

                            //GameData.Instance.method_1("Clearing area of mobs...", Color.Red);
                            if (GameData.Instance.battle.ClearAreaOfMobs(path[CurrentPathIndex].X + ThisOffsetPosition.X - PlayerOffsetInCollisiongrid.X, path[CurrentPathIndex].Y + ThisOffsetPosition.Y - PlayerOffsetInCollisiongrid.Y, AcceptMoveOffset + 2))
                            {
                                IsMovingToNextArea = true;
                                IsMovingThruPath = false;
                            }
                        }
                    }
                }
                else
                {
                    if (path[CurrentPathIndex].X != LastMovingLocation.X || path[CurrentPathIndex].Y != LastMovingLocation.Y)
                    {
                        LastMovingLocation.X = path[CurrentPathIndex].X;
                        LastMovingLocation.Y = path[CurrentPathIndex].Y;
                    }
                    else
                    {
                        BadPathIndexount++;
                        CurrentPathIndex++;
                        GameData.Instance.overlayForm.SetPathPoints(path, CurrentPathIndex, ThisOffsetPosition, PlayerOffsetInCollisiongrid);
                        if (BadPathIndexount >= 3)
                        {
                            IsMovingThruPath = false;
                        }
                    }
                }
            }

            if (!IsMovingToNextArea)
            {
                if (AcceptOffset == 4) GameData.Instance.mover.MoveToLocation(ThisFinalPosition.X - PlayerOffsetInCollisiongrid.X, ThisFinalPosition.Y - PlayerOffsetInCollisiongrid.Y);

                //int tryyy = 0;
                //while (GameData.Instance.playerScan.levelNo == ThisPlayerAreaID && tryyy <= 25)
                //{
                Position itemScreenPos = GameData.Instance.gameStruc.World2Screen(GameData.Instance.playerScan.xPosFinal, GameData.Instance.playerScan.yPosFinal, ThisFinalPosition.X - PlayerOffsetInCollisiongrid.X, ThisFinalPosition.Y - PlayerOffsetInCollisiongrid.Y);
                GameData.Instance.keyMouse.MouseClicc_RealPos(itemScreenPos.X, itemScreenPos.Y);
                GameData.Instance.playerScan.GetPositions();
                //tryyy++;
                //}
            }

            int FinalX = path[path.Count - 1].X + ThisOffsetPosition.X - PlayerOffsetInCollisiongrid.X;
            int FinalY = path[path.Count - 1].Y + ThisOffsetPosition.Y - PlayerOffsetInCollisiongrid.Y;
            if (GameData.Instance.mover.IsPositionNearOf(FinalX, FinalY, AcceptOffset)) MovedCorrectly = true;
        }
        else
        {
            GameData.Instance.method_1("No path found.", Color.Red);
            Form1_0.GoToNextScript();
        }

        return MovedCorrectly;
    }

    public void DetectCloserPathPositions()
    {
        if (IsMovingThruPath)
        {
            int DistX = GameData.Instance.playerScan.xPos - path[CurrentPathIndex].X;
            int DistY = GameData.Instance.playerScan.yPos - path[CurrentPathIndex].Y;
            if (DistX < 0) DistX = -DistX;
            if (DistY < 0) DistY = -DistY;

            int CheckIndexxx = CurrentPathIndex + 1;
            int CheckCount = 0;
            while (CheckIndexxx < path.Count - 1 && CheckCount < 1)
            {
                int DistNextX = GameData.Instance.playerScan.xPos - path[CheckIndexxx].X;
                int DistNextY = GameData.Instance.playerScan.yPos - path[CheckIndexxx].Y;
                if (DistNextX < 0) DistNextX = -DistNextX;
                if (DistNextY < 0) DistNextY = -DistNextY;

                if (DistNextX < DistX && DistNextY < DistY)
                {
                    CurrentPathIndex++; //increase pathing index, we are closer to the next destination than the current one!
                    GameData.Instance.overlayForm.SetPathPoints(path, CurrentPathIndex, ThisOffsetPosition, PlayerOffsetInCollisiongrid);
                }
                CheckIndexxx++;
                CheckCount++;
            }
        }
    }

    public bool[,] MergeCollisionGrids(Enums.Area ThisNewArea)
    {
        bool[,] CurrentAreaGrid = GameData.Instance.mapAreaStruc.CollisionGrid((Enums.Area)GameData.Instance.playerScan.levelNo);
        bool[,] NextAreaGrid = GameData.Instance.mapAreaStruc.CollisionGrid(ThisNewArea);
        PlayerOffsetInCollisiongrid = new Position { X = 0, Y = 0 };
        TargetOffsetInCollisiongrid = new Position { X = 0, Y = 0 };

        // Calculate the size of the merged grid
        int minX = Math.Min(GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X, GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X);
        int minY = Math.Min(GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y, GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y);
        int maxX = Math.Max(GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X + GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Size.Width, GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X + GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Size.Width);
        int maxY = Math.Max(GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y + GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Size.Height, GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y + GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Size.Height);
        int width = maxX - minX;
        int height = maxY - minY;

        // Create the merged grid
        bool[,] mergedGrid = new bool[width, height];

        // Copy collision data from map1 to the merged grid
        for (int y = 0; y < GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Size.Height; y++)
        {
            for (int x = 0; x < GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Size.Width; x++)
            {
                mergedGrid[x - minX + GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X, y - minY + GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y] = CurrentAreaGrid[x, y];
            }
        }

        // Copy collision data from map2 to the merged grid
        for (int y = 0; y < GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Size.Height; y++)
        {
            for (int x = 0; x < GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Size.Width; x++)
            {
                mergedGrid[x - minX + GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X, y - minY + GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y] = NextAreaGrid[x, y];
            }
        }

        SetOffsets(ThisNewArea);

        return mergedGrid;
    }

    public void SetOffsets(Enums.Area ThisNewArea)
    {
        /*Point startPos = new Point(GameData.Instance.playerScan.xPos - ThisOffsetPosition.X, GameData.Instance.playerScan.yPos - ThisOffsetPosition.Y);
        Point targetPos = new Point(ThisFinalPosition.X - ThisOffsetPosition.X, ThisFinalPosition.Y - ThisOffsetPosition.Y);

        startPos.X += PlayerOffsetInCollisiongrid.X;
        startPos.Y += PlayerOffsetInCollisiongrid.Y;
        targetPos.X += TargetOffsetInCollisiongrid.X;
        targetPos.Y += TargetOffsetInCollisiongrid.Y;*/

        PlayerOffsetInCollisiongrid = new Position { X = 0, Y = 0 };
        TargetOffsetInCollisiongrid = new Position { X = 0, Y = 0 };

        if (GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y == GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y + GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Size.Height)
        {
            //Expend Bottom
            //#####
            /*if (GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X > GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X)
            {
                PlayerOffsetInCollisiongrid.X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X;
                TargetOffsetInCollisiongrid.X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X;

            }
            if (GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X < GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X)
            {
                PlayerOffsetInCollisiongrid.X = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X;
                TargetOffsetInCollisiongrid.X = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X;
            }*/
            //#####
            ThisOffsetPosition = new Position { X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X, Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y };
        }
        if (GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y == GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Size.Height)
        {
            //Expend Up
            //#####
            /*if (GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X > GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X)
            {
                PlayerOffsetInCollisiongrid.X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X;
                TargetOffsetInCollisiongrid.X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X;
            }
            if (GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X < GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X)
            {
                PlayerOffsetInCollisiongrid.X = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X;
                TargetOffsetInCollisiongrid.X = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X;
            }*/
            //#####
            ThisOffsetPosition = new Position { X = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X, Y = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y };
        }
        if (GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X == GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X + GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Size.Width)
        {
            //Expend Right
            //#####
            /*if (GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y > GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y)
            {
                PlayerOffsetInCollisiongrid.Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y;
                //TargetOffsetInCollisiongrid.Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y;

            }
            if (GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y < GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y)
            {
                //PlayerOffsetInCollisiongrid.Y = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y;
                TargetOffsetInCollisiongrid.Y = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y;
            }*/
            //#####
            ThisOffsetPosition = new Position { X = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X, Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y };
        }
        if (GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X == GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.X - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Size.Width)
        {
            //Expend Left
            //#####
            /*if (GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y > GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y)
            {
                PlayerOffsetInCollisiongrid.Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y;
                //TargetOffsetInCollisiongrid.Y = GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y;

            }
            if (GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y < GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y)
            {
                //PlayerOffsetInCollisiongrid.Y = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y;
                TargetOffsetInCollisiongrid.Y = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y - GameData.Instance.mapAreaStruc.AllMapData[ThisPlayerAreaID - 1].Offset.Y;
            }*/
            //#####
            ThisOffsetPosition = new Position { X = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.X, Y = GameData.Instance.mapAreaStruc.AllMapData[(int)ThisNewArea - 1].Offset.Y };
        }
    }

    public List<Point> FindPath(Point startPos, Point targetPos)
    {
        int width = ThisCollisionGrid.GetLength(0);
        int height = ThisCollisionGrid.GetLength(1);

        bool[,] closedSet = new bool[width, height];
        List<Node> openSet = new List<Node>();

        openSet.Add(new Node(startPos, null, 0, Heuristic(startPos, targetPos)));

        try
        {
            while (openSet.Any())
            {
                Node current = openSet.OrderBy(node => node.F).First();

                if (current.Position.Equals(targetPos)) return ReconstructPath(current);

                openSet.Remove(current);
                closedSet[current.Position.X, current.Position.Y] = true;

                for (int p = 0; p < 8; p++) // Assuming 4-direction movement
                {
                    int nx = current.Position.X + dx[p];
                    int ny = current.Position.Y + dy[p];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        if (nx < ThisCollisionGrid.GetLength(0) && ny < ThisCollisionGrid.GetLength(1)
                            && nx < closedSet.GetLength(0) && ny < closedSet.GetLength(1))
                        {
                            if (ThisCollisionGrid[nx, ny])
                            {
                                Point neighborPos = new Point(nx, ny);

                                if (closedSet[nx, ny]) continue;

                                double tentativeG = current.G + 1;
                                if (p > 3) tentativeG += 1;

                                Node neighbor = openSet.FirstOrDefault(node => node.Position.Equals(neighborPos));
                                if (neighbor == null || tentativeG < neighbor.G)
                                {
                                    if (neighbor == null)
                                    {
                                        neighbor = new Node(neighborPos);
                                        openSet.Add(neighbor);
                                    }

                                    neighbor.Parent = current;
                                    neighbor.G = tentativeG;
                                    neighbor.H = Heuristic(neighborPos, targetPos);
                                }
                            }
                        }
                    }

                    if (CharConfig.UseTeleport && !GameData.Instance.townStruc.GetInTown() && (Enums.Area)GameData.Instance.playerScan.levelNo == Enums.Area.ArcaneSanctuary)
                    //if (CharConfig.UseTeleport && !GameData.Instance.townStruc.GetInTown())
                    {
                        nx = current.Position.X + (dx[p] * AcceptMoveOffset);
                        ny = current.Position.Y + (dy[p] * AcceptMoveOffset);

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            if (nx < ThisCollisionGrid.GetLength(0) && ny < ThisCollisionGrid.GetLength(1)
                                && nx < closedSet.GetLength(0) && ny < closedSet.GetLength(1))
                            {
                                if (ThisCollisionGrid[nx, ny])
                                {
                                    Point neighborPos = new Point(nx, ny);

                                    if (closedSet[nx, ny]) continue;

                                    double tentativeG = current.G + AcceptMoveOffset + 1;
                                    //double tentativeG = current.G + AcceptMoveOffset;
                                    //double tentativeG = current.G + 2;
                                    //if (p > 3) tentativeG += 1;

                                    Node neighbor = openSet.FirstOrDefault(node => node.Position.Equals(neighborPos));
                                    if (neighbor == null || tentativeG < neighbor.G)
                                    {
                                        if (neighbor == null)
                                        {
                                            neighbor = new Node(neighborPos);
                                            openSet.Add(neighbor);
                                        }

                                        neighbor.Parent = current;
                                        neighbor.G = tentativeG;
                                        neighbor.H = Heuristic(neighborPos, targetPos);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            GameData.Instance.method_1("Issue with PathFinding CollisionGrid 'Indexes'!", Color.Red);
        }

        // No path found
        return null;
    }

    public bool IsCloseToLocation(Position ThissP, int AcceptOffset)
    {
        bool MovedCorrectly = false;
        if (GameData.Instance.playerScan.xPosFinal >= (ThissP.X - AcceptOffset)
                && GameData.Instance.playerScan.xPosFinal <= (ThissP.X + AcceptOffset)
                && GameData.Instance.playerScan.yPosFinal >= (ThissP.Y - AcceptOffset)
                && GameData.Instance.playerScan.yPosFinal <= (ThissP.Y + AcceptOffset))
        {
            MovedCorrectly = true;
        }
        return MovedCorrectly;
    }

    public Point TeleportPoint = new Point(0, 0);

    private bool IsWalkable(Point point)
    {
        return point.X >= 0 && point.X < ThisCollisionGrid.GetLength(0) && point.Y >= 0 && point.Y < ThisCollisionGrid.GetLength(1) && ThisCollisionGrid[point.X, point.Y];
    }

    private double Heuristic(Point a, Point b)
    {
        return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }

    private List<Point> ReconstructPath(Node node)
    {
        List<Point> path = new List<Point>();

        while (node != null)
        {
            path.Add(node.Position);
            node = node.Parent;
        }

        path.Reverse();
        return path;
    }

    private class Node
    {
        public Point Position { get; }
        public Node Parent { get; set; }
        public double G { get; set; } // g score: cost from start to current
        public double H { get; set; } // h score: estimated cost from current to target
        public double F => G + H; // f score: total estimated cost

        public Node(Point position, Node parent = null, double g = 0, double h = 0)
        {
            Position = position;
            Parent = parent;
            G = g;
            H = h;
        }
    }

    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;

            var other = (Point)obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}
