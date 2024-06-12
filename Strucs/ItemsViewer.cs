using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MapAreaStruc;
using System.Runtime.InteropServices;

public class ItemsViewer
{
    GameData gameData = GameData.Instance;

    public List<Bitmap> SoldItemsScreenshots = new List<Bitmap>();
    public Panel PicturePanel = new Panel();

    public Bitmap BufferItemPicture;

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public void Init()
    {
        PicturePanel.Size = new System.Drawing.Size(1, 1);
        PicturePanel.BackgroundImageLayout = ImageLayout.Stretch;
        PicturePanel.Visible = false;
        gameData.overlayForm.Controls.Add(PicturePanel);
    }

    public void ItemViewerDebug()
    {
        for (int i = 0; i < 40; i++)
        {
            if (CharConfig.InventoryDontCheckItem[i] == 0) continue;

            //Console.WriteLine("HasStashItem:" + gameData.inventoryStruc.InventoryHasStashItem[i] + ", HasUnidItem:" + gameData.inventoryStruc.InventoryHasUnidItem[i]);

            //################
            //GET ITEM SOLD INFOS
            string SoldTxt = "";
            Color ThisCol = Color.Black;
            Dictionary<string, int> itemXYPos = gameData.inventoryStruc.ConvertIndexToXY(i);
            if (gameData.itemsStruc.GetSpecificItem(0, "", itemXYPos["x"], itemXYPos["y"], gameData.playerScan.unitId, 0))
            {
                SoldTxt = "Sold Item:" + gameData.itemsStruc.ItemNAAME + " (ID:" + gameData.itemsStruc.txtFileNo + ")";
                ThisCol = gameData.itemsStruc.GetColorFromQuality((int)gameData.itemsStruc.itemQuality);
            }
            //gameData.itemsViewer.TakeItemPicture();
            //################

            Dictionary<string, int> itemScreenPos = gameData.inventoryStruc.ConvertIndexToXY(i);
            itemScreenPos = gameData.inventoryStruc.ConvertInventoryLocToScreenPos(itemScreenPos["x"], itemScreenPos["y"]);

            gameData.keyMouse.MouseMoveTo(itemScreenPos["x"], itemScreenPos["y"]);
            gameData.itemsViewer.TakeItemPicture();
            gameData.WaitDelay(5);

            if (SoldTxt != "")
            {
                gameData.form.method_1_SoldItems(SoldTxt, ThisCol);
                gameData.itemsViewer.AddBufferPicture("Sold");
            }
        }
    }

    public void ShowItemScreenshot(int ThisIndex, string ThisCategory)
    {
        return;

        Bitmap ThisPicture = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        if (ThisCategory == "Sold") ThisPicture = SoldItemsScreenshots[ThisIndex];

        PicturePanel.Size = new System.Drawing.Size(ThisPicture.Width, ThisPicture.Height);
        PicturePanel.BackgroundImage = ThisPicture;
        PicturePanel.Visible = true;

        int ThisPosX = Cursor.Position.X + 1;
        //if (ThisPosX < 0) ThisPosX = 0;

        int ThisPosY = (Cursor.Position.Y - ThisPicture.Height - 20);
        if (ThisPosY < 0) ThisPosY = 0;
        PicturePanel.Location = new Point(ThisPosX, ThisPosY);
    }

    public void UnshowItem()
    {
        PicturePanel.Visible = false;
    }

    public void TakeItemPicture()
    {
        int StartX = Cursor.Position.X - 335;
        if (StartX < 0) StartX = 0;
        int StartY = 0;
        int SizeX = 670;
        int SizeY = (Cursor.Position.Y - 20) - StartY;

        //AllItemsScreenshots.Add(ItemScreenshot);
        BufferItemPicture = CaptureScreen(StartX, StartY, SizeX, SizeY);
        //BufferItemPicture = CaptureRegion((IntPtr) gameData.hWnd, StartX, StartY, SizeX, SizeY);
    }

    public void AddBufferPicture(string ThisCategory)
    {
        if (ThisCategory == "Sold") SoldItemsScreenshots.Add(BufferItemPicture);
    }


    static Bitmap CaptureScreen(int x, int y, int width, int height)
    {
        // Create a bitmap to store the screenshot
        Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        // Create a graphics object from the bitmap
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            // Capture the screen region and draw it onto the bitmap
            graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
        }

        return bitmap;
    }

    /*static Bitmap CaptureRegion(IntPtr hWnd, int x, int y, int width, int height)
    {
        // Create a bitmap to store the screenshot
        Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        // Get the device context (DC) of the bitmap
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            IntPtr hdcBitmap = graphics.GetHdc();

            // Copy the specified region from the window to the bitmap
            PrintWindow(hWnd, hdcBitmap, 0);

            // Release the device context of the bitmap
            graphics.ReleaseHdc(hdcBitmap);
        }

        return bitmap;
    }*/

    static Bitmap CaptureRegion(IntPtr hWnd, int x, int y, int width, int height)
    {
        // Create a bitmap to store the screenshot
        Bitmap screenshot = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        // Create a graphics object from the bitmap
        using (Graphics graphics = Graphics.FromImage(screenshot))
        {
            // Capture the specified region of the window
            graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
        }

        return screenshot;
    }

}