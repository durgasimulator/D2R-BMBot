using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;

public class Mover
{
    GameData gameData = GameData.Instance;
    public int MaxMoveTry = 5; //default is 5
    public int MoveAcceptOffset = 4;
    public long StartAreaBeforeMoving = 0;

    public bool AllowFastMove = false;

    public DateTime LastTimeSinceTeleport = DateTime.Now;

    public bool IsPositionNearOf(int ThisX, int ThisY, int Offset)
    {
        //gameData.playerScan.GetPositions();
        if (gameData.playerScan.xPosFinal >= (ThisX - Offset)
            && gameData.playerScan.xPosFinal <= (ThisX + Offset)
            && gameData.playerScan.yPosFinal >= (ThisY - Offset)
            && gameData.playerScan.yPosFinal <= (ThisY + Offset))
        {
            return true;
        }
        return false;
    }

    public bool MovingToInteract = false;

    //This will move to a direct location -> no pathfinding
    public bool MoveToLocation(int ThisX, int ThisY, bool AllowPickingItem = false, bool AllowMoveSideWay = true)
    {
        if (!gameData.Running || !gameData.gameStruc.IsInGame()) return false;

        gameData.uiScan.readUI();
        if (gameData.uiScan.leftMenu || gameData.uiScan.rightMenu && !MovingToInteract) gameData.uiScan.CloseAllUIMenu();
        //if (gameData.uiScan.leftMenu || gameData.uiScan.rightMenu) gameData.uiScan.CloseAllUIMenu();
        if (gameData.uiScan.quitMenu) gameData.uiScan.CloseUIMenu("quitMenu");

        gameData.playerScan.GetPositions();
        gameData.overlayForm.SetMoveToPoint(new System.Drawing.Point(ThisX, ThisY));
        StartAreaBeforeMoving = gameData.playerScan.levelNo;
        //gameData.gameStruc.CheckChickenGameTime();

        //######
        //moving location is way to far away something might be wrong!
        if (!IsPositionNearOf(ThisX, ThisY, 300)) return false;
        if (ThisX == 0 && ThisY == 0) return false;
        //######

        //no need to move we are close already!
        if (IsPositionNearOf(ThisX, ThisY, MoveAcceptOffset))
        {
            gameData.overlayForm.ResetMoveToLocation();
            return true;
        }

        if (!gameData.gameStruc.IsInGame() || !gameData.Running)
        {
            gameData.overlayForm.ResetMoveToLocation();
            return false;
        }

        //fix town act5 stuck near bolder
        if (gameData.townStruc.GetInTown() && IsPositionNearOf(5093, 5034, 2))
        {
            MoveToLocationAttack(5100, 5021);
        }

        int TryMove = 0;
        int TryMove2 = 0;
        int LastX = gameData.playerScan.xPosFinal;
        int LastY = gameData.playerScan.yPosFinal;
        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisX, ThisY);

        if (gameData.townStruc.GetInTown()) gameData.keyMouse.PressKey(CharConfig.KeySkillfastMoveAtTown);
        else gameData.keyMouse.PressKey(CharConfig.KeySkillfastMoveOutsideTown);

        if (!CharConfig.UseTeleport || (CharConfig.UseTeleport && gameData.townStruc.GetInTown()))
        {
            gameData.keyMouse.MouseMoveTo_RealPos(itemScreenPos.X, itemScreenPos.Y);
            gameData.keyMouse.MouseClicHoldWithoutRelease();
            gameData.keyMouse.PressKeyHold(CharConfig.KeyForceMovement);
        }
        while (true)
        {
            if (!gameData.Running || !gameData.gameStruc.IsInGame()) return false;

            if (gameData.townStruc.GetInTown())
            {
                gameData.keyMouse.PressKey(CharConfig.KeySkillfastMoveAtTown);
                AllowFastMove = false;
            }
            else
            {
                gameData.keyMouse.PressKey(CharConfig.KeySkillfastMoveOutsideTown);
                if (CharConfig.UseTeleport) AllowFastMove = true;
                if (CharConfig.RunBaalLeechScript && !gameData.baalLeech.ScriptDone) AllowFastMove = false;
                if (CharConfig.RunLowerKurastScript && !((LowerKurast)gameData.lowerKurast).ScriptDone) AllowFastMove = true;

                //Check if we are in close range from target destination, if we are, desactivate fast moving (eles it teleport twice)
                if (AllowFastMove)
                {
                    if (IsPositionNearOf(ThisX, ThisY, 21)) AllowFastMove = false;
                }
            }

            if (!CharConfig.UseTeleport || (CharConfig.UseTeleport && gameData.townStruc.GetInTown()))
            {
                gameData.keyMouse.MouseMoveTo_RealPos(itemScreenPos.X, itemScreenPos.Y);
            }
            if (CharConfig.UseTeleport && !gameData.townStruc.GetInTown())
            {
                gameData.keyMouse.MouseCliccRight_RealPos(itemScreenPos.X, itemScreenPos.Y);

                //#######
                if (!AllowFastMove)
                {
                    LastTimeSinceTeleport = DateTime.Now;
                    TimeSpan ThisTimeCheck = DateTime.Now - LastTimeSinceTeleport;
                    while (gameData.playerScan.xPosFinal == LastX && gameData.playerScan.yPosFinal == LastY && ThisTimeCheck.TotalMilliseconds < 200)
                    {
                        if (!gameData.Running || !gameData.gameStruc.IsInGame()) return false;
                        Application.DoEvents();
                        gameData.playerScan.GetPositions();
                        gameData.overlayForm.UpdateOverlay();
                        gameData.potions.CheckIfWeUsePotion();
                        gameData.itemsStruc.GetItems(false);
                        ThisTimeCheck = DateTime.Now - LastTimeSinceTeleport;
                    }
                    if (!gameData.Running || !gameData.gameStruc.IsInGame()) return false;
                }
                else
                {
                    gameData.form.SetProcessingTime();
                }
                //#######
            }
            gameData.playerScan.GetPositions();
            gameData.overlayForm.UpdateOverlay();
            gameData.gameStruc.CheckChickenGameTime();
            if (AllowPickingItem) gameData.itemsStruc.GetItems(true);      //#############
            gameData.potions.CheckIfWeUsePotion();
            itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisX, ThisY);

            Application.DoEvents();

            gameData.itemsStruc.AvoidItemsOnGroundPointerList.Clear();

            //######
            //moving location is way to far away something might be wrong!
            if (!IsPositionNearOf(ThisX, ThisY, 300))
            {
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                return false;
            }
            if (ThisX == 0 && ThisY == 0)
            {
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                return false;
            }
            //######

            //not in suposed area, may have taken unwanted tp
            if (gameData.playerScan.levelNo < StartAreaBeforeMoving - 1
                || gameData.playerScan.levelNo > StartAreaBeforeMoving + 1)
            {
                gameData.overlayForm.ScanningOverlayItems = true; //try rescanning overlay if there was too much lags
                gameData.overlayForm.ResetMoveToLocation();
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                return false;
            }

            //detect is moving
            if (gameData.playerScan.xPosFinal != LastX
            || gameData.playerScan.yPosFinal != LastY)
            {
                TryMove = 0;
            }
            else
            {
                TryMove++;
            }

            //break moving loop
            if (IsPositionNearOf(ThisX, ThisY, MoveAcceptOffset)) break;

            //teleport again
            /*gameData.playerScan.GetPositions();
            if (AllowFastMove && !IsPositionNearOf(ThisX, ThisY, MoveAcceptOffset))
            {
                if (gameData.townStruc.GetInTown())
                {
                    gameData.keyMouse.PressKey(CharConfig.KeySkillfastMoveAtTown);
                    AllowFastMove = false;
                }
                else
                {
                    gameData.keyMouse.PressKey(CharConfig.KeySkillfastMoveOutsideTown);
                    if (CharConfig.UseTeleport) AllowFastMove = true;
                }

                itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisX, ThisY);

                gameData.keyMouse.MouseCliccRight_RealPos(itemScreenPos.X, itemScreenPos.Y);
            }*/

            if (TryMove >= MaxMoveTry && (!CharConfig.UseTeleport || (CharConfig.UseTeleport && gameData.townStruc.GetInTown())))
            {
                if (!AllowMoveSideWay)
                {
                    gameData.itemsStruc.AvoidItemsOnGroundPointerList.Clear();
                    gameData.overlayForm.ResetMoveToLocation();
                    gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                    return false;
                }

                if (!gameData.gameStruc.IsInGame() || !gameData.Running)
                {
                    gameData.overlayForm.ResetMoveToLocation();
                    gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                    return false;
                }
                if (AllowPickingItem) gameData.itemsStruc.GetItems(true);      //#############
                gameData.potions.CheckIfWeUsePotion();
                /*if (TryMove2 == 0) gameData.keyMouse.MouseMoveTo(gameData.ScreenX / 2, gameData.ScreenY / 2);
                if (TryMove2 == 1) gameData.keyMouse.MouseMoveTo(gameData.ScreenX / 2 - 250, gameData.ScreenY / 2);
                if (TryMove2 == 2) gameData.keyMouse.MouseMoveTo(gameData.ScreenX / 2 + 250, gameData.ScreenY / 2);
                if (TryMove2 == 3) gameData.keyMouse.MouseMoveTo(gameData.ScreenX / 2, gameData.ScreenY / 2 - 250);
                if (TryMove2 == 4) gameData.keyMouse.MouseMoveTo(gameData.ScreenX / 2, gameData.ScreenY / 2 + 250);*/

                gameData.keyMouse.MouseClicRelease();
                gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
                gameData.keyMouse.MouseClicHoldWithoutRelease();
                gameData.keyMouse.PressKeyHold(CharConfig.KeyForceMovement);

                gameData.WaitDelay(4);

                TryMove2++;
                if (TryMove2 >= MaxMoveTry)
                {
                    break;
                }
            }

            LastX = gameData.playerScan.xPosFinal;
            LastY = gameData.playerScan.yPosFinal;
        }
        if (!gameData.Running || !gameData.gameStruc.IsInGame()) return false;

        bool MovedCorrectly = false;
        if (IsPositionNearOf(ThisX, ThisY, MoveAcceptOffset)) MovedCorrectly = true;

        if (!CharConfig.UseTeleport || (CharConfig.UseTeleport && gameData.townStruc.GetInTown()))
        {
            gameData.keyMouse.MouseClicRelease();
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        }
        gameData.keyMouse.PressKey(CharConfig.KeySkillDefenseAura);

        gameData.itemsStruc.AvoidItemsOnGroundPointerList.Clear();
        gameData.overlayForm.ResetMoveToLocation();
        return MovedCorrectly;
    }

    //This will FAST move to a direct location -> no pathfinding (used for attacking mobs)
    public bool MoveToLocationAttack(int ThisX, int ThisY)
    {
        if (!gameData.Running || !gameData.gameStruc.IsInGame()) return false;

        gameData.uiScan.readUI();
        if (gameData.uiScan.leftMenu || gameData.uiScan.rightMenu) gameData.uiScan.CloseAllUIMenu();

        gameData.playerScan.GetPositions();
        gameData.overlayForm.SetMoveToPoint(new System.Drawing.Point(ThisX, ThisY));
        StartAreaBeforeMoving = gameData.playerScan.levelNo;
        //gameData.gameStruc.CheckChickenGameTime();

        //######
        //moving location is way to far away something might be wrong!
        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        if (!IsPositionNearOf(ThisX, ThisY, 300)) return false;
        if (ThisX == 0 && ThisY == 0) return false;
        //######

        //no need to move we are close already!
        if (IsPositionNearOf(ThisX, ThisY, MoveAcceptOffset))
        {
            gameData.overlayForm.ResetMoveToLocation();
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            return true;
        }

        if (!gameData.gameStruc.IsInGame() || !gameData.Running)
        {
            gameData.overlayForm.ResetMoveToLocation();
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            return false;
        }

        Position itemScreenPos = gameData.gameStruc.World2Screen(gameData.playerScan.xPosFinal, gameData.playerScan.yPosFinal, ThisX, ThisY);


        //if (!CharConfig.UseTeleport || (CharConfig.UseTeleport && gameData.townStruc.GetInTown()))
        //{
        gameData.keyMouse.MouseMoveTo_RealPos(itemScreenPos.X, itemScreenPos.Y);
        gameData.keyMouse.MouseClicHoldWithoutRelease();
        gameData.keyMouse.PressKeyHold(CharConfig.KeyForceMovement);
        //}
        if (gameData.townStruc.GetInTown()) gameData.keyMouse.PressKey(CharConfig.KeySkillfastMoveAtTown);
        else gameData.keyMouse.PressKey(CharConfig.KeySkillfastMoveOutsideTown);


        gameData.WaitDelay(5); //wait a little bit, we just casted attack

        if (!CharConfig.UseTeleport || (CharConfig.UseTeleport && gameData.townStruc.GetInTown())) gameData.keyMouse.MouseMoveTo_RealPos(itemScreenPos.X, itemScreenPos.Y);
        if (CharConfig.UseTeleport && !gameData.townStruc.GetInTown()) gameData.keyMouse.MouseCliccRightAttackMove(itemScreenPos.X, itemScreenPos.Y);

        //#######
        gameData.playerScan.GetPositions();
        gameData.overlayForm.UpdateOverlay();
        gameData.gameStruc.CheckChickenGameTime();
        gameData.potions.CheckIfWeUsePotion();
        gameData.itemsStruc.GetItems(true);
        gameData.form.SetProcessingTime();
        //Application.DoEvents();
        //#######

        //######
        //moving location is way to far away something might be wrong!
        if (!IsPositionNearOf(ThisX, ThisY, 300))
        {
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            return false;
        }
        if (ThisX == 0 && ThisY == 0)
        {
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            return false;
        }
        //######

        //not in suposed area, may have taken unwanted tp
        if (gameData.playerScan.levelNo < StartAreaBeforeMoving - 1
            || gameData.playerScan.levelNo > StartAreaBeforeMoving + 1)
        {
            gameData.itemsStruc.AvoidItemsOnGroundPointerList.Clear();
            gameData.overlayForm.ScanningOverlayItems = true; //try rescanning overlay if there was too much lags
            gameData.overlayForm.ResetMoveToLocation();
            gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
            return false;
        }

        bool MovedCorrectly = false;
        if (IsPositionNearOf(ThisX, ThisY, MoveAcceptOffset)) MovedCorrectly = true;

        //if (!CharConfig.UseTeleport || (CharConfig.UseTeleport && gameData.townStruc.GetInTown()))
        //{
        gameData.keyMouse.MouseClicRelease();
        gameData.keyMouse.ReleaseKey(CharConfig.KeyForceMovement);
        //}
        //gameData.keyMouse.PressKey(CharConfig.KeySkillDefenseAura);

        gameData.itemsStruc.AvoidItemsOnGroundPointerList.Clear();
        gameData.overlayForm.ResetMoveToLocation();
        return MovedCorrectly;
    }
}
