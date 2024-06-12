using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

public partial class FormSettings : Form
{
    Form1 Form1_0;
    GameData gameData = GameData.Instance;
    public FormSettings(Form1 form1_1)
    {
        Form1_0 = form1_1;

        InitializeComponent();

        textBoxStartKey.Items.Clear();
        comboBoxPauseResume.Items.Clear();
        string[] names = Enum.GetNames(typeof(System.Windows.Forms.Keys));
        for (int i = 0; i < names.Length; i++)
        {
            textBoxStartKey.Items.Add(names[i]);
            comboBoxPauseResume.Items.Add(names[i]);
        }

        groupBoxSearch.Visible = false;
        groupBoxSearch.Location = new Point(groupBox1.Location.X, groupBox1.Location.Y);

        listViewRush.Visible = false;
        listViewRush.Location = new System.Drawing.Point(listViewRunScripts.Location.X, listViewRunScripts.Location.Y);

        panelBaalFeature.Visible = false;
        panelBaalFeature.Location = new System.Drawing.Point(23, 197);

        panelOverlay.Visible = false;
        panelOverlay.Location = new System.Drawing.Point(23, 197);

        panelChaosFeature.Visible = false;
        panelChaosFeature.Location = new System.Drawing.Point(23, 197);

        panelBaalLeech.Visible = false;
        panelBaalLeech.Location = new System.Drawing.Point(23, 197);

        panelShopBot.Visible = false;
        panelShopBot.Location = new System.Drawing.Point(23, 197);

        LoadSettings();
    }

    public void LoadSettings()
    {
        checkBoxShowOverlay.Checked = CharConfig.ShowOverlay;

        textBoxD2Path.Text = GameData.Instance.D2_LOD_113C_Path;
        numericUpDownMaxTime.Value = CharConfig.MaxGameTime;
        checkBoxRush.Checked = CharConfig.IsRushing;
        textBox1LeechName.Text = CharConfig.RushLeecherName;

        checkBoxLogOrangeError.Checked = CharConfig.LogNotUsefulErrors;

        textBoxStartKey.Text = CharConfig.StartStopKey.ToString();
        comboBoxPauseResume.Text = CharConfig.PauseResumeKey.ToString();

        int CurrI = 0;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunMapHackOnly;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunItemGrabScriptOnly;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunMapHackPickitOnly;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunShopBotScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunMausoleumScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunCryptScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunPitScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunCowsScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunCountessScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunAndarielScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunSummonerScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunDurielScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunArachnidScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunLowerKurastScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunA3SewersScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunUpperKurastScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunTravincalScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunMephistoScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunChaosScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunChaosLeechScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunEldritchScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunShenkScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunFrozensteinScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunPindleskinScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunNihlatakScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunBaalScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunBaalLeechScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunTerrorZonesScript;
        listViewRunScripts.Items[CurrI++].Checked = CharConfig.RunWPTaker;

        CurrI = 0;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunDarkWoodRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunTristramRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunAndarielRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunRadamentRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunHallOfDeadRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunFarOasisRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunLostCityRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunSummonerRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunDurielRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunKahlimEyeRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunKahlimBrainRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunKahlimHeartRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunTravincalRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunMephistoRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunChaosRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunAnyaRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunAncientsRush;
        listViewRush.Items[CurrI++].Checked = CharConfig.RunBaalRush;

        if (CharConfig.RunGameMakerScript) comboBoxLobby.SelectedIndex = 0;
        if (CharConfig.RunChaosSearchGameScript) comboBoxLobby.SelectedIndex = 1;
        if (CharConfig.RunBaalSearchGameScript) comboBoxLobby.SelectedIndex = 2;
        if (CharConfig.RunNoLobbyScript) comboBoxLobby.SelectedIndex = 3;
        if (CharConfig.RunSinglePlayerScript) comboBoxLobby.SelectedIndex = 4;

        textBoxGameName.Text = CharConfig.GameName;
        textBoxGamePass.Text = CharConfig.GamePass;

        comboBoxDifficulty.SelectedIndex = CharConfig.GameDifficulty;

        if (GameData.Instance.CurrentGameNumber <= 0) GameData.Instance.CurrentGameNumber = 1;
        numericUpDownRunNumber.Value = GameData.Instance.CurrentGameNumber;

        //###################
        //SPECIALS BAAL FEATURES
        checkBoxKillBaal.Checked = ((Baal)gameData.baal).KillBaal;
        checkBoxBaalSafeHealing.Checked = ((Baal)gameData.baal).SafeYoloStrat;
        numericUpDownBaalLeaveMobsCount.Value = ((Baal)gameData.baal).LeaveIfMobsCountIsAbove;
        for (int i = 0; i < ((Baal)gameData.baal).LeaveIfMobsIsPresent_ID.Count; i++)
        {
            string[] arr = new string[2];
            arr[0] = ((Baal)gameData.baal).LeaveIfMobsIsPresent_ID[i].ToString();
            arr[1] = ((Baal)gameData.baal).LeaveIfMobsIsPresent_Count[i].ToString();
            ListViewItem item = new ListViewItem(arr);

            listViewBaalLeaveOnMobs.Items.Add(item);
        }

        checkBoxBaalLeechFightMobs.Checked = gameData.baalLeech.BaalLeechFight;

        //###################
        //SPECIALS CHAOS FEATURES
        checkBoxFastChaos.Checked = ((Chaos)gameData.chaos).FastChaos;

        //###################
        //SPECIALS OVERLAY FEATURES
        checkBoxOverlayShowMobs.Checked = gameData.overlayForm.ShowMobs;
        checkBoxOverlayShowWP.Checked = gameData.overlayForm.ShowWPs;
        checkBoxOverlayShowGoodChest.Checked = gameData.overlayForm.ShowGoodChests;
        checkBoxOverlayShowLogs.Checked = gameData.overlayForm.ShowLogs;
        checkBoxOverlayShowBotInfos.Checked = gameData.overlayForm.ShowBotInfos;
        checkBoxOverlayShowNPC.Checked = gameData.overlayForm.ShowNPC;
        checkBoxOverlayShowPath.Checked = gameData.overlayForm.ShowPathFinding;
        checkBoxOverlayShowExits.Checked = gameData.overlayForm.ShowExits;
        checkBoxOverlayShowMH.Checked = gameData.overlayForm.ShowMapHackShowLines;
        checkBoxOverlayShowUnitsCount.Checked = gameData.overlayForm.ShowUnitsScanCount;
        //###################
        //SHOP BOT
        numericUpDownMaxShopCount.Value = ((ShopBot)gameData.shopBot).MaxShopCount;
        numericUpDownShopTownAct.Value = ((ShopBot)gameData.shopBot).ShopBotTownAct;
        //###################

        SetCreateGameGroupbox();
        SetRushMenu();
    }

    public void SetRushMenu()
    {
        if (checkBoxRush.Checked)
        {
            groupBox1.Enabled = false;
            comboBoxLobby.Enabled = false;
            listViewRunScripts.Visible = false;

            label1Run.Text = "Select Rush Scripts";

            textBox1LeechName.Visible = true;
            label1LeechName.Visible = true;

            label7MaxTime.Visible = false;
            numericUpDownMaxTime.Visible = false;

            listViewRush.Visible = true;
        }
        else
        {
            groupBox1.Enabled = true;
            comboBoxLobby.Enabled = true;
            listViewRunScripts.Visible = true;

            label1Run.Text = "Select Run Scripts";

            textBox1LeechName.Visible = false;
            label1LeechName.Visible = false;

            label7MaxTime.Visible = true;
            numericUpDownMaxTime.Visible = true;

            listViewRush.Visible = false;
        }
    }

    public void SetCreateGameGroupbox()
    {
        if (!checkBoxRush.Checked) groupBox1.Enabled = (comboBoxLobby.SelectedIndex == 0);
        else groupBox1.Enabled = false;

        if (comboBoxLobby.SelectedIndex == 1) 
        {
            textBoxSearchGame.Text = CharConfig.ChaosLeechSearch;
            textBoxAvoidWords.Text = "";
            for (int p = 0; p < CharConfig.ChaosSearchAvoidWords.Count; p++)
            {
                textBoxAvoidWords.Text += CharConfig.ChaosSearchAvoidWords[p];
                if (p < CharConfig.ChaosSearchAvoidWords.Count - 2) textBoxAvoidWords.Text += ",";
            }
        }
        else if(comboBoxLobby.SelectedIndex == 2)
        {
            textBoxSearchGame.Text = CharConfig.BaalLeechSearch;
            textBoxAvoidWords.Text = "";
            for (int p = 0; p < CharConfig.BaalSearchAvoidWords.Count; p++)
            {
                textBoxAvoidWords.Text += CharConfig.BaalSearchAvoidWords[p];
                if (p < CharConfig.BaalSearchAvoidWords.Count - 2) textBoxAvoidWords.Text += ",";
            }
        }

        if (comboBoxLobby.SelectedIndex == 1 || comboBoxLobby.SelectedIndex == 2)
        {
            groupBox1.Visible = false;
            groupBoxSearch.Visible = true;
            textBox2LeechName.Text = CharConfig.SearchLeecherName;
        }
        else
        {
            groupBox1.Visible = true;
            groupBoxSearch.Visible = false;
        }

        if (comboBoxLobby.SelectedIndex == 4)
        {
            groupBox1.Visible = true;
            groupBoxSearch.Visible = false;
        }
    }

    public void SaveSettings()
    {
        CharConfig.ShowOverlay = checkBoxShowOverlay.Checked;

        gameData.D2_LOD_113C_Path = textBoxD2Path.Text;
        CharConfig.MaxGameTime = (int)numericUpDownMaxTime.Value;
        CharConfig.IsRushing = checkBoxRush.Checked;
        CharConfig.RushLeecherName = textBox1LeechName.Text;
        Enum.TryParse(textBoxStartKey.Text, out CharConfig.StartStopKey);
        Enum.TryParse(comboBoxPauseResume.Text, out CharConfig.PauseResumeKey);

        CharConfig.LogNotUsefulErrors = checkBoxLogOrangeError.Checked;

        int CurrI = 0;
        CharConfig.RunMapHackOnly = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunItemGrabScriptOnly = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunMapHackPickitOnly = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunShopBotScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunMausoleumScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunCryptScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunPitScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunCowsScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunCountessScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunAndarielScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunSummonerScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunDurielScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunArachnidScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunLowerKurastScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunA3SewersScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunUpperKurastScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunTravincalScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunMephistoScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunChaosScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunChaosLeechScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunEldritchScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunShenkScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunFrozensteinScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunPindleskinScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunNihlatakScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunBaalScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunBaalLeechScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunTerrorZonesScript = listViewRunScripts.Items[CurrI++].Checked;
        CharConfig.RunWPTaker = listViewRunScripts.Items[CurrI++].Checked;

        CurrI = 0;
        CharConfig.RunDarkWoodRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunTristramRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunAndarielRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunRadamentRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunHallOfDeadRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunFarOasisRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunLostCityRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunSummonerRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunDurielRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunKahlimEyeRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunKahlimBrainRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunKahlimHeartRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunTravincalRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunMephistoRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunChaosRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunAnyaRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunAncientsRush = listViewRush.Items[CurrI++].Checked;
        CharConfig.RunBaalRush = listViewRush.Items[CurrI++].Checked;

        CharConfig.RunGameMakerScript = (comboBoxLobby.SelectedIndex == 0);
        CharConfig.RunChaosSearchGameScript = (comboBoxLobby.SelectedIndex == 1);
        CharConfig.RunBaalSearchGameScript = (comboBoxLobby.SelectedIndex == 2);
        CharConfig.RunNoLobbyScript = (comboBoxLobby.SelectedIndex == 3);
        CharConfig.RunSinglePlayerScript = (comboBoxLobby.SelectedIndex == 4);

        CharConfig.GameName = textBoxGameName.Text;
        CharConfig.GamePass = textBoxGamePass.Text;

        CharConfig.GameDifficulty = comboBoxDifficulty.SelectedIndex;

        gameData.CurrentGameNumber = (int)numericUpDownRunNumber.Value;

        //###################
        //SPECIALS BAAL FEATURES
        ((Baal)gameData.baal).KillBaal = checkBoxKillBaal.Checked;
        ((Baal)gameData.baal).SafeYoloStrat = checkBoxBaalSafeHealing.Checked;
        ((Baal)gameData.baal).LeaveIfMobsCountIsAbove = (int)numericUpDownBaalLeaveMobsCount.Value;
        ((Baal)gameData.baal).LeaveIfMobsIsPresent_ID.Clear();
        ((Baal)gameData.baal).LeaveIfMobsIsPresent_Count.Clear();
        for (int i = 0; i < listViewBaalLeaveOnMobs.Items.Count; i++)
        {
            ((Baal)gameData.baal).LeaveIfMobsIsPresent_ID.Add(uint.Parse(listViewBaalLeaveOnMobs.Items[i].SubItems[0].Text));
            ((Baal)gameData.baal).LeaveIfMobsIsPresent_Count.Add(int.Parse(listViewBaalLeaveOnMobs.Items[i].SubItems[1].Text));
        }

        gameData.baalLeech.BaalLeechFight = checkBoxBaalLeechFightMobs.Checked;

        //###################
        //SPECIALS CHAOS FEATURES
        ((Chaos)gameData.chaos).FastChaos = checkBoxFastChaos.Checked;

        //###################
        //SPECIALS OVERLAY FEATURES
        gameData.overlayForm.ShowMobs = checkBoxOverlayShowMobs.Checked;
        gameData.overlayForm.ShowWPs = checkBoxOverlayShowWP.Checked;
        gameData.overlayForm.ShowGoodChests = checkBoxOverlayShowGoodChest.Checked;
        gameData.overlayForm.ShowLogs = checkBoxOverlayShowLogs.Checked;
        gameData.overlayForm.ShowBotInfos = checkBoxOverlayShowBotInfos.Checked;
        gameData.overlayForm.ShowNPC = checkBoxOverlayShowNPC.Checked;
        gameData.overlayForm.ShowPathFinding = checkBoxOverlayShowPath.Checked;
        gameData.overlayForm.ShowExits = checkBoxOverlayShowExits.Checked;
        gameData.overlayForm.ShowMapHackShowLines = checkBoxOverlayShowMH.Checked;
        gameData.overlayForm.ShowUnitsScanCount = checkBoxOverlayShowUnitsCount.Checked;
        //###################

        if (comboBoxLobby.SelectedIndex == 1)
        {
            CharConfig.ChaosLeechSearch = textBoxSearchGame.Text;

            CharConfig.ChaosSearchAvoidWords.Clear();
            if (textBoxAvoidWords.Text.Contains(","))
            {
                string[] AllNames = textBoxAvoidWords.Text.Split(',');
                for (int p = 0; p < AllNames.Length; p++) CharConfig.ChaosSearchAvoidWords.Add(AllNames[p]);
            }
            else if (textBoxAvoidWords.Text != "") CharConfig.ChaosSearchAvoidWords.Add(textBoxAvoidWords.Text);
        }
        else if (comboBoxLobby.SelectedIndex == 2)
        {
            CharConfig.BaalLeechSearch = textBoxSearchGame.Text;

            CharConfig.BaalSearchAvoidWords.Clear();
            if (textBoxAvoidWords.Text.Contains(","))
            {
                string[] AllNames = textBoxAvoidWords.Text.Split(',');
                for (int p = 0; p < AllNames.Length; p++) CharConfig.BaalSearchAvoidWords.Add(AllNames[p]);
            }
            else if (textBoxAvoidWords.Text != "") CharConfig.BaalSearchAvoidWords.Add(textBoxAvoidWords.Text);
        }
        CharConfig.SearchLeecherName = textBox2LeechName.Text;

        //###################
        //SHOP BOT
        ((ShopBot)gameData.shopBot).MaxShopCount = int.Parse(numericUpDownMaxShopCount.Value.ToString());
        ((ShopBot)gameData.shopBot).ShopBotTownAct = int.Parse(numericUpDownShopTownAct.Value.ToString());
        //###################
    }

    private void button1_Click(object sender, EventArgs e)
    {
        numericUpDownRunNumber.Value = 1;
    }

    private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveSettings();
        gameData.settingsLoader.SaveCurrentSettings();
    }

    private void comboBoxLobby_SelectedIndexChanged(object sender, EventArgs e)
    {
        SetCreateGameGroupbox();
    }

    private void listViewRunScripts_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    private void FormSettings_Load(object sender, EventArgs e)
    {

    }

    private void checkBoxRush_CheckedChanged(object sender, EventArgs e)
    {
        SetRushMenu();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        //FormCharSettings FormCharSettings_0 = new FormCharSettings(gameData);
        //FormCharSettings_0.ShowDialog();

        SaveSettings();
        gameData.settingsLoader.SaveCurrentSettings();
    }

    private void listViewRunScripts_DoubleClick(object sender, EventArgs e)
    {
        if (listViewRunScripts.SelectedItems.Count == 0) return;

        listViewRunScripts.SelectedItems[0].Checked = !listViewRunScripts.SelectedItems[0].Checked;
        if (listViewRunScripts.SelectedItems[0].Text == "Baal")
        {
            checkBoxKillBaal.Checked = ((Baal)gameData.baal).KillBaal;
            panelBaalFeature.Visible = true;
        }
        else if (listViewRunScripts.SelectedItems[0].Text == "Baal Leech")
        {
            panelBaalLeech.Visible = true;
        }
        else if (listViewRunScripts.SelectedItems[0].Text == "Chaos")
        {
            panelChaosFeature.Visible = true;
        }
        else if (listViewRunScripts.SelectedItems[0].Text == "ShopBot")
        {
            panelShopBot.Visible = true;
        }
        else if (listViewRunScripts.SelectedItems[0].Text == "Maphack ONLY (no script running)")
        {
            panelOverlay.Visible = true;
        }
        else
        {
            MessageBox.Show("No specials features exist for this run!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void buttonBaalApply_Click(object sender, EventArgs e)
    {
        ((Baal)gameData.baal).KillBaal = checkBoxKillBaal.Checked;
        panelBaalFeature.Visible = false;
    }

    private void label10_Click(object sender, EventArgs e)
    {

    }

    private void buttonBaalAddMob_Click(object sender, EventArgs e)
    {
        string[] arr = new string[2];
        arr[0] = numericUpDownBaalMobID.Value.ToString();
        arr[1] = numericUpDownBaalMobCount.Value.ToString();
        ListViewItem item = new ListViewItem(arr);

        listViewBaalLeaveOnMobs.Items.Add(item);
    }

    private void buttonBaalClearMob_Click(object sender, EventArgs e)
    {
        listViewBaalLeaveOnMobs.Items.Clear();
    }

    private void buttonReload_Click(object sender, EventArgs e)
    {
        DialogResult result = openFileDialog1.ShowDialog();
        if (result == DialogResult.OK)
        {
            gameData.settingsLoader.LoadThisFileSettings(openFileDialog1.FileName);
            LoadSettings();
            Application.DoEvents();
        }
    }

    private void button3_Click(object sender, EventArgs e)
    {
        panelOverlay.Visible = false;
    }

    private void buttonOverlaySettings_Click(object sender, EventArgs e)
    {
        panelOverlay.Visible = true;
    }

    private void button4_Click(object sender, EventArgs e)
    {
        panelChaosFeature.Visible = false;
    }

    private void button5_Click(object sender, EventArgs e)
    {
        FormAdvancedSettings FormAdvancedSettings_0 = new FormAdvancedSettings(gameData.form);
        FormAdvancedSettings_0.ShowDialog();
    }

    private void button6_Click(object sender, EventArgs e)
    {
        panelBaalLeech.Visible = false;
    }

    private void buttonApplyShopBot_Click(object sender, EventArgs e)
    {
        panelShopBot.Visible = false;
    }
}
