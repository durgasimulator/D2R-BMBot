using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HoverStruc
{
    private GameData gameData;

    public bool isHovered = false;
    public uint lastHoveredType = 0;
    public uint lastHoveredUnitId = 0;
    public byte[] hoverBuffer = new byte[12];
    public void Initialize(GameData gameData)
    {
        this.gameData = gameData;
    }
    public bool IsHoveringItem(uint ThissType, uint ThissUnitId)
    {
        if (lastHoveredType == ThissType && lastHoveredUnitId == ThissUnitId)
        {
            return true;
        }
        return false;
    }

    public void GetHovering()
    {
        long hoverAddress = (long)gameData.BaseAddress + (long)gameData.offsets["hoverOffset"];
        gameData.mem.ReadRawMemory(hoverAddress, ref hoverBuffer, 12);
        ushort TeB = BitConverter.ToUInt16(hoverBuffer, 0);
        if (TeB > 0)
        {
            isHovered = true;
        }
        else
        {
            isHovered = false;
        }
        if (isHovered)
        {
            lastHoveredType = BitConverter.ToUInt32(hoverBuffer, 0x04);
            lastHoveredUnitId = BitConverter.ToUInt32(hoverBuffer, 0x08);
        }
    }
}
