﻿/*AmpShell : .NET front-end for DOSBox
 * Copyright (C) 2009, 2019 Maximilien Noal
 *This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>.*/
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AmpShell
{
    public partial class GameForm : Form
    {
        private Game _Game;
        private Window _AmpInstance;
        public GameForm(Window AmpWindow)
        {
            InitializeComponent();
            GameInstance = new Game();
            AmpInstance=AmpWindow;
            NoConsoleCheckBox.Checked = AmpInstance.GamesNoConsole;
            FullscreenCheckBox.Checked = AmpInstance.GamesInFullScreen;
            QuitOnExitCheckBox.Checked = AmpInstance.GamesQuitOnExit;
            GameAdditionalCommandsTextBox.Text = AmpInstance.GamesAdditionalCommands;
        }
        //alternate constructor for when a game is edited
        public GameForm(Game EditedGame, Window AmpWindow)
        {
            InitializeComponent();
            //set the property GameInstance to EditedGame
            GameInstance = EditedGame;
            AmpInstance=AmpWindow;
            //fill the form with the Game's data.
            Text = "Editing " + GameInstance.Name + "...";
            if (GameInstance.Icon != String.Empty && GameInstance.Icon!=null)
            {
                String LocationTranslate;
                LocationTranslate = GameInstance.Icon.Replace("AppPath",Application.StartupPath);
                if (File.Exists(LocationTranslate))
                {
                    GameIconPictureBox.Image = Image.FromFile(LocationTranslate).GetThumbnailImage(64, 64, null, IntPtr.Zero);
                    GameIconPictureBox.ImageLocation = LocationTranslate;
                }
            }
            OK.Text = "&Save and apply";
            OK.Width = 102;
            OK.Location = new System.Drawing.Point(Cancel.Location.X-106, Cancel.Location.Y);
            OK.Image = global::AmpShell.Properties.Resources.saveHS;
            Cancel.Text = "&Don't save";
            GameNameTextbox.Text = GameInstance.Name;
            GameLocationTextbox.Text = GameInstance.DOSEXEPath;
            GameDirectoryTextbox.Text = GameInstance.Directory;
            GameCustomConfigurationTextbox.Text = GameInstance.DBConfPath;
            NoConfigCheckBox.Checked = GameInstance.NoConfig;
            GameCDPathTextBox.Text = GameInstance.CDPath;
            GameAdditionalCommandsTextBox.Text = GameInstance.AdditionalCommands;
            NoConsoleCheckBox.Checked = GameInstance.NoConsole;
            QuitOnExitCheckBox.Checked = GameInstance.QuitOnExit;
            FullscreenCheckBox.Checked = GameInstance.InFullScreen;
            GameSetupTextBox.Text = GameInstance.SetupEXEPath;
            UseIOCTLRadioButton.Checked = GameInstance.UseIOCTL;
            IsAFloppyDiskRadioButton.Checked = GameInstance.MountAsFloppy;
            if (UseIOCTLRadioButton.Checked == false && IsAFloppyDiskRadioButton.Checked == false)
                NoneRadioButton.Checked = true;
            GameLocationTextbox.Text = GameLocationTextbox.Text.Replace("AppPath", Application.StartupPath);
            GameDirectoryTextbox.Text=GameDirectoryTextbox.Text.Replace("AppPath", Application.StartupPath);
            GameCustomConfigurationTextbox.Text=GameCustomConfigurationTextbox.Text.Replace("AppPath", Application.StartupPath);
            GameCDPathTextBox.Text=GameCDPathTextBox.Text.Replace("AppPath", Application.StartupPath);
            GameAdditionalCommandsTextBox.Text=GameAdditionalCommandsTextBox.Text.Replace("AppPath", Application.StartupPath);
            GameSetupTextBox.Text=GameSetupTextBox.Text.Replace("AppPath", Application.StartupPath);
        }
        //public property for the private field Game
        public Game GameInstance
        {
            get { return _Game; }
            set { _Game = value; }
        }
        public Window AmpInstance
        {
            get { return _AmpInstance; }
            set { _AmpInstance = value; }
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        //EventHandler for when the user has clicked on "OK"
        private void OK_Click(object sender, EventArgs e)
        {
            //if the game has a name but no executable nor directory mounted as C: specified...
			if (GameNameTextbox.Text != String.Empty)
			{
				if (GameLocationTextbox.Text == String.Empty && GameDirectoryTextbox.Text == String.Empty)
					MessageBox.Show(this,"You must enter the game's executable location or the directory that will be mounted as C:",Text,MessageBoxButtons.OK,MessageBoxIcon.Information);//... show an error.
				//else : the game has a name and a directory mounted has C:
				else
				{
					//close the form
					GameInstance.DOSEXEPath = GameLocationTextbox.Text;
					GameInstance.DBConfPath = GameCustomConfigurationTextbox.Text;
					GameInstance.NoConfig = NoConfigCheckBox.Checked;
					GameInstance.AdditionalCommands = GameAdditionalCommandsTextBox.Text;
					GameInstance.NoConsole = NoConsoleCheckBox.Checked;
					GameInstance.InFullScreen = FullscreenCheckBox.Checked;
					GameInstance.QuitOnExit = QuitOnExitCheckBox.Checked;
					GameInstance.Directory = GameDirectoryTextbox.Text;
					GameInstance.Name = GameNameTextbox.Text;
					GameInstance.CDPath = GameCDPathTextBox.Text;
					GameInstance.SetupEXEPath = GameSetupTextBox.Text;
					if (GameIconPictureBox.ImageLocation != null && GameIconPictureBox.ImageLocation!=String.Empty)
						GameInstance.Icon = GameIconPictureBox.ImageLocation;
					else
						GameInstance.Icon = String.Empty;
					GameInstance.UseIOCTL = UseIOCTLRadioButton.Checked;
					GameInstance.MountAsFloppy = IsAFloppyDiskRadioButton.Checked;
					if (GameCDPathTextBox.Text != String.Empty)
					{
						if (File.Exists(GameCDPathTextBox.Text))
							GameInstance.CDIsAnImage = true;
						else
							GameInstance.CDIsAnImage = false;
					}
					DialogResult = DialogResult.OK;
					Close();
				}
			}
			//if the game has no name
			else
				MessageBox.Show(this, "You must enter the game's name.",Text,MessageBoxButtons.OK,MessageBoxIcon.Information);//show an error
		}
        //EventHandler to choose the Game's executable location
        private void GameLocationBrowseButton_Click(object sender, EventArgs e)
        {
			OpenFileDialog GameEXEFileFD = new OpenFileDialog();
			if (AmpInstance.PortableMode == true)
				GameEXEFileFD.InitialDirectory = Application.StartupPath;
			else if (GameLocationTextbox.Text != String.Empty && Directory.Exists(Directory.GetParent(GameLocationTextbox.Text).FullName))
				GameEXEFileFD.InitialDirectory = Directory.GetParent(GameLocationTextbox.Text).FullName;
			else
				GameEXEFileFD.InitialDirectory = SearchFDInitialDirectory();
			GameEXEFileFD.Title = GameLocationLabel.Text;
			GameEXEFileFD.Filter = "DOS executable files (*.bat;*.cmd;*.com;*.exe)|*.bat;*.cmd;*.com;*.exe;*.BAT;*.CMD;*.COM;*.EXE";
			if (GameEXEFileFD.ShowDialog(this) == DialogResult.OK)
				GameLocationTextbox.Text = GameEXEFileFD.FileName;
        }
        //EventHandler to choose the game .conf (config) file
        private void GameCustomConfigurationBrowseButton_Click(object sender, EventArgs e)
        {
			OpenFileDialog CCFileFD = new OpenFileDialog();
			if (AmpInstance.PortableMode == true)
				CCFileFD.InitialDirectory = Application.StartupPath;
			else if (GameCustomConfigurationTextbox.Text != String.Empty && Directory.Exists(Directory.GetParent(GameCustomConfigurationTextbox.Text).FullName))
				CCFileFD.InitialDirectory = Directory.GetParent(GameCustomConfigurationTextbox.Text).FullName;
			else
				CCFileFD.InitialDirectory = SearchFDInitialDirectory();
			CCFileFD.Title = GameCustomCofigurationLabel.Text;
			CCFileFD.Filter = "DOSBox configuration file (*.conf)|*.conf;*.CONF";
			if (CCFileFD.ShowDialog(this) == DialogResult.OK)
				GameCustomConfigurationTextbox.Text = CCFileFD.FileName;
        }
        //EventHandler for when the "Do not use any config file at all" checbox is (un)checked
        private void NoConfigCheckBox_CheckedChanged(object sender, EventArgs e)
		{                
			if (NoConfigCheckBox.Checked == true)
			{
				GameCustomConfigurationTextbox.Enabled = false;
				GameCustomConfigurationBrowseButton.Enabled = false;
			}
			else
			{
				GameCustomConfigurationTextbox.Enabled = true;
				GameCustomConfigurationBrowseButton.Enabled = true;
			}
		}
        //EventHandler to choose the game's CD image file
        private void GameCDPathBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog CDImageFD = new OpenFileDialog
            {
                Title = GameCDPathLabel.Text,
                Filter = "DOSBox compatible CD images (*.bin;*.cue;*.iso;*.img)|*.bin;*.cue;*.iso;*.img;*.BIN;*.CUE;*.ISO;*.IMG"
            };
            if (AmpInstance.PortableMode == true)
				CDImageFD.InitialDirectory = Application.StartupPath;
			else if (AmpInstance.CDsDefaultDir != String.Empty && Directory.Exists(AmpInstance.CDsDefaultDir))
				CDImageFD.InitialDirectory = AmpInstance.CDsDefaultDir;
			else
				CDImageFD.InitialDirectory = SearchFDInitialDirectory();
			if (CDImageFD.ShowDialog(this) == DialogResult.OK)
				GameCDPathTextBox.Text = CDImageFD.FileName;
		}
        //EventHandler for when the GameLocationTextbox (for the game's executable location) text is changed
        private void GameLocationTextbox_TextChanged(object sender, EventArgs e)
        {
            //if a location for the game's executable has been entered
			if (GameLocationTextbox.Text != String.Empty)
			{
				//then the directory mounted has C: TextBox, BrowseButton, and Labeled are disabled
				//(because DOSBox already mounts the executable's directory path as C: )
				GameDirectoryTextbox.Enabled = false;
				GameDirectoryBrowseButton.Enabled = false;
				GameDirectoryLabel.Enabled = false;
			}
			else
			{
				//if not, they are enabled
				GameDirectoryTextbox.Enabled = true;
				GameDirectoryBrowseButton.Enabled = true;
				GameDirectoryLabel.Enabled = true;
			}
			//if the entered executable does exist
			if (GameLocationTextbox.Text != String.Empty)
			{
				if (File.Exists(GameLocationTextbox.Text)) //the directory mounted has C: is displayed : it is the game's executable full directory path
					GameDirectoryTextbox.Text = Directory.GetParent(GameLocationTextbox.Text).FullName;//(even if the GameDirectory controls are not enabled : it's just to inform the user)
			}
        }
        //EventHandler for when the GameDirectoryTextbox's Text has changed 
        private void GameDirectoryTextbox_TextChanged(object sender, EventArgs e)
        {
			//if the textBox is not empty
			if (GameDirectoryTextbox.Text != String.Empty)
			{
				//if the game location textbox is not empty
				if (GameLocationTextbox.Text != String.Empty)
				{
					//and if the specified directory does not equals to the game executable's directory
					if (Directory.GetParent(GameLocationTextbox.Text).FullName != GameDirectoryTextbox.Text)
					{
						//then this textbox has been entered first by the user
						//so the controls for the game's executable location will be made empty and disabled
						//because DOSBox cannot mount a directory as C: and have an executable specified.
						//(it's one or the other)
						GameLocationTextbox.Text = String.Empty;
						GameLocationTextbox.Enabled = false;
						GameLocationBrowseButton.Enabled = false;
						GameLocationLabel.Enabled = false;
					}
					//if this textbox is empty
					else
					{
						//make the controls for the game's executable location available 
						GameLocationTextbox.Enabled = true;
						GameLocationBrowseButton.Enabled = true;
						GameLocationLabel.Enabled = true;
					}
				}
				else
				{
					GameLocationTextbox.Text = String.Empty;
					GameLocationTextbox.Enabled = false;
					GameLocationBrowseButton.Enabled = false;
					GameLocationLabel.Enabled = false;
				}
			}
			else
			{
				GameLocationTextbox.Enabled = true;
				GameLocationBrowseButton.Enabled = true;
				GameLocationLabel.Enabled = true;
			}
		}
        //EventHandler to choose the directory mounted has C:
        private void GameDirectoryBrowseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog CMountDirectoryFBD = new FolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = GameDirectoryLabel.Text
            };
            if (CMountDirectoryFBD.ShowDialog(this) == DialogResult.OK)
				GameDirectoryTextbox.Text = CMountDirectoryFBD.SelectedPath;
        }
        //EventHandler for when the GameCDPathTextBox's text is changed
        private void GameCDPathTextBox_TextChanged(object sender, EventArgs e)
        {
            if (GameCDPathTextBox.Text == String.Empty)
				MountingOptionsGroupBox.Enabled = false;
			else
			{
				MountingOptionsGroupBox.Enabled = true;
				if (File.Exists(GameCDPathTextBox.Text)==false)
				{
					UseIOCTLRadioButton.Enabled = true;
					IsAFloppyDiskRadioButton.Enabled = false;
				}
				else
				{
					UseIOCTLRadioButton.Enabled = false;
					IsAFloppyDiskRadioButton.Enabled = true;
				}
			}
        }
        //EventHandler to choose the game's setup executable location
        private void GameSetupBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog SetupPathFD = new OpenFileDialog
            {
                Title = GameSetupLabel.Text,
                Filter = "DOS executable files (*.bat;*.com;*.exe)|*.bat;*.com;*.exe;*.BAT;*.COM;*.EXE"
            };
            if (AmpInstance.PortableMode == true)
				SetupPathFD.InitialDirectory = Application.StartupPath;
			else
				SetupPathFD.InitialDirectory = SearchFDInitialDirectory();
			if (SetupPathFD.ShowDialog(this) == DialogResult.OK)
				GameSetupTextBox.Text = SetupPathFD.FileName;
        }
        //EventHandler to choose the directory mounted as D:
        //Because a CD can be a CD Image, or a drive.
        //Or, the user just wants to mount another directory as D:
        private void GameCDDirBrowseButton_Click(object sender, EventArgs e)
        {
		FolderBrowserDialog CDDriveFBD = new FolderBrowserDialog();
			if (CDDriveFBD.ShowDialog(this) == DialogResult.OK)
				GameCDPathTextBox.Text = CDDriveFBD.SelectedPath;
        }
        private void QuitOnExitCheckBox_EnabledChanged(object sender, EventArgs e)
        {
			if (QuitOnExitCheckBox.Enabled == false)
				QuitOnExitCheckBox.Checked = false;
        }
        private void GameIconPictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                OpenFileDialog OpenICOFD = new OpenFileDialog
                {
                    Filter = "Image files (*.bmp;*.exif;*.gif;*.ico;*.jp*;*.png;*.tif*)|*.bmp;*.BMP;*.exif;*.EXIF;*.gif;*.GIF;*.ico;*.ICO;*.jp*;*.JP*;*.png;*.PNG;*.tif*;*.TIF*"
                };
                if (AmpInstance.PortableMode == true)
                    OpenICOFD.InitialDirectory = Application.StartupPath;
                else if (GameIconPictureBox.ImageLocation != null && GameIconPictureBox.ImageLocation != String.Empty && Directory.Exists(Directory.GetParent(GameIconPictureBox.ImageLocation).FullName))
                    OpenICOFD.InitialDirectory = Directory.GetParent(GameIconPictureBox.ImageLocation).FullName;
                else
                    OpenICOFD.InitialDirectory = SearchFDInitialDirectory();
                if (OpenICOFD.ShowDialog(this) == DialogResult.OK)
                {
                    GameIconPictureBox.Image = Image.FromFile(OpenICOFD.FileName).GetThumbnailImage(64, 64, null, IntPtr.Zero);
                    GameIconPictureBox.ImageLocation = OpenICOFD.FileName;
                }
            }
            catch (System.OutOfMemoryException)
            {
                MessageBox.Show(this, "There was an error in the image file, or it's format is not supported. Please check the file.", "Changing the game's icon", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (GameInstance.Icon == String.Empty)
                    GameIconPictureBox.Image = global::AmpShell.Properties.Resources.Generic_Application1;
                else
                    GameIconPictureBox.Image = Image.FromFile(GameInstance.Icon).GetThumbnailImage(64, 64, null, IntPtr.Zero);
            }
        }
        private void ResetIconButton_Click(object sender, EventArgs e)
        {
			GameIconPictureBox.Image = global::AmpShell.Properties.Resources.Generic_Application1;
			GameIconPictureBox.ImageLocation = String.Empty;
        }
        private String SearchFDInitialDirectory()
        {
			String InitialDirectory=String.Empty;
			if (GameInstance.DOSEXEPath != String.Empty && Directory.Exists(Directory.GetParent(GameInstance.DOSEXEPath).FullName))
				InitialDirectory = Directory.GetParent(GameInstance.DOSEXEPath).FullName;
			else if (GameInstance.Directory != String.Empty && Directory.Exists(GameInstance.Directory))
				InitialDirectory = GameInstance.Directory;
			else  if (GameInstance.SetupEXEPath != String.Empty && Directory.Exists(Directory.GetParent(GameInstance.SetupEXEPath).FullName))
				InitialDirectory = Directory.GetParent(GameInstance.SetupEXEPath).FullName;
			else  if (GameInstance.Icon != String.Empty && File.Exists(GameInstance.Icon))
				InitialDirectory = Directory.GetParent(GameInstance.Icon).FullName;
			else if (GameInstance.DBConfPath != String.Empty && Directory.Exists(Directory.GetParent(GameInstance.DBConfPath).FullName))
				InitialDirectory = Directory.GetParent(GameInstance.DBConfPath).FullName;
			else  if (AmpInstance.GamesDefaultDir != String.Empty && Directory.Exists(AmpInstance.GamesDefaultDir))
				InitialDirectory = AmpInstance.GamesDefaultDir;
			if (InitialDirectory == String.Empty)
			{
				if (GameLocationTextbox.Text != String.Empty && Directory.Exists(Directory.GetParent(GameLocationTextbox.Text).FullName))
					InitialDirectory = Directory.GetParent(GameLocationTextbox.Text).FullName;
				else if (GameSetupTextBox.Text != String.Empty && Directory.Exists(Directory.GetParent(GameSetupTextBox.Text).FullName))
					InitialDirectory = Directory.GetParent(GameSetupTextBox.Text).FullName;
				else if (GameCustomConfigurationTextbox.Text != String.Empty && Directory.Exists(Directory.GetParent(GameCustomConfigurationTextbox.Text).FullName))
					InitialDirectory = Directory.GetParent(GameCustomConfigurationTextbox.Text).FullName;
                else if (GameIconPictureBox.ImageLocation != null && GameIconPictureBox.ImageLocation !=String.Empty && Directory.Exists(Directory.GetParent(GameIconPictureBox.ImageLocation).FullName))
                    InitialDirectory = Directory.GetParent(GameIconPictureBox.ImageLocation).FullName;
			}
			return InitialDirectory;
        }
   }
}