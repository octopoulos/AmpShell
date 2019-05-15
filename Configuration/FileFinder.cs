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
using System.IO;
using System.Windows.Forms;

namespace AmpShell.Configuration
{
    public static class FileFinder
    {
        public static string SearchCommonTextEditor()
        {
            string confEditorPath = string.Empty;
            if (string.IsNullOrWhiteSpace(confEditorPath))
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, Environment.GetFolderPath(Environment.SpecialFolder.System).Length - 8).ToString() + "notepad.exe"))
                {
                    confEditorPath = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, Environment.GetFolderPath(Environment.SpecialFolder.System).Length - 8).ToString() + "notepad.exe";
                }
            }
            return confEditorPath;
        }

        public static string SearchDOSBoxConf(string userConfigDataPath, string DOSBoxExecutablePath)
        {
            string confPath = string.Empty;
            if (userConfigDataPath == Application.StartupPath + "\\AmpShell.xml")
            {
                if (Directory.GetFiles((Application.StartupPath), "*.conf").Length > 0)
                {
                    confPath = Directory.GetFiles((Application.StartupPath), "*.conf")[0];
                }
            }
            if (string.IsNullOrWhiteSpace(confPath))
            {
                //if Local Settings/Application Data/DOSBox exists
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DOSBox"))
                {
                    //then, the DOSBox.conf file inside it becomes the default one. 
                    if (Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DOSBox", "*dosbox*.conf").Length > 0)
                    {
                        confPath = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DOSBox", "*dosbox*.conf")[0];
                    }
                }
                else
                {
                    //if dosbox.conf has been generated by DOSBox in the same directory as dosbox.exe
                    //(behavior of DOSBox versions prior to DOSBox version 0.73)
                    if (string.IsNullOrWhiteSpace(DOSBoxExecutablePath) == false)
                    {
                        if (File.Exists(Directory.GetParent(DOSBoxExecutablePath).FullName + "\\dosbox.conf"))
                        {
                            confPath = DOSBoxExecutablePath + "\\dosbox.conf";
                        }
                    }
                }
            }
            return confPath;
        }

        public static string SearchDOSBoxLanguageFile(string dosboxExecutablePath)
        {
            //returned string
            string langPath = string.Empty;
            //if LangPath is _still_ empty, Windows test cases take place.
            if (string.IsNullOrWhiteSpace(langPath))
            {
                //if Local Settings/Application Data/DOSBox exists
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DOSBox"))
                {
                    //then, the DOSBox.conf file inside it becomes the default one.
                    if (Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DOSBox", "*.lng").Length > 0)
                    {
                        langPath = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\DOSBox", "*.lng")[0];
                    }
                }
                else
                {
                    //if dosbox.conf has been generated by DOSBox in the same directory as dosbox.exe
                    //(behavior of DOSBox versions prior to DOSBox version 0.73)
                    if (string.IsNullOrWhiteSpace(dosboxExecutablePath) == false)
                    {
                        if (Directory.GetFiles(Directory.GetParent(dosboxExecutablePath).FullName, "*.lng").Length > 0)
                        {
                            langPath = Directory.GetFiles(Directory.GetParent(dosboxExecutablePath).FullName, "*.lng")[0];
                        }
                    }
                }
            }
            return langPath;
        }

        public static string SearchDOSBox(string userConfigDataPath, bool portableMode)
        {
            string dosboxPath = string.Empty;
            if (userConfigDataPath == Application.StartupPath + "\\AmpShell.xml" && portableMode)
            {
                if (File.Exists(Application.StartupPath + "\\dosbox.exe"))
                {
                    dosboxPath = Application.StartupPath + "\\dosbox.exe";
                }
            }
            else
            {
                //test if DOSBox is in Program Files/DOSBox-?.?? (Windows x86)
                if (Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DOSBox*", SearchOption.TopDirectoryOnly).GetLength(0) != 0)
                {
                    dosboxPath = Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "DOSBox*", SearchOption.TopDirectoryOnly)[0];
                    if (File.Exists(dosboxPath + "\\dosbox.exe"))
                    {
                        dosboxPath += "\\dosbox.exe";
                    }
                }
                else
                {
                    //test if the user is using Windows x64
                    //in this case, DOSBox's installation directory is most likely in "Program Files (x86)"
                    if (Directory.Exists(Environment.SystemDirectory.Substring(0, 3) + "Program Files (x86)"))
                    {
                        if (Directory.GetDirectories(Environment.SystemDirectory.Substring(0, 3) + "Program Files (x86)", "DOSBox*", SearchOption.TopDirectoryOnly).GetLength(0) != 0)
                        {
                            dosboxPath = Directory.GetDirectories(Environment.SystemDirectory.Substring(0, 3) + "Program Files (x86)", "DOSBox*", SearchOption.TopDirectoryOnly)[0];
                            if (File.Exists(dosboxPath + "\\dosbox.exe"))
                            {
                                dosboxPath += "\\dosbox.exe";
                            }
                        }
                    }
                }
            }
            //if DOSBoxPath is still empty, say to the user that dosbox's executable cannot be found
            if (string.IsNullOrWhiteSpace(dosboxPath))
            {
                switch (MessageBox.Show("AmpShell cannot find DOSBox, do you want to indicate DOSBox's executable location now ? Choose 'Cancel' to quit.", "Cannot find DOSBox", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Cancel:
                        dosboxPath = string.Empty;
                        Environment.Exit(0);
                        break;
                    case DialogResult.Yes:
                        OpenFileDialog dosboxExeFileDialog = new OpenFileDialog
                        {
                            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                            Title = "Please indicate DOSBox's executable location...",
                            Filter = "DOSBox executable (dosbox*)|dosbox*"
                        };
                        if (dosboxExeFileDialog.ShowDialog(WinForms.MainForm.ActiveForm) == DialogResult.OK)
                        {
                            //retrieve the selected dosbox.exe path into Amp.DBPath
                            dosboxPath = dosboxExeFileDialog.FileName;
                        }
                        else
                        {
                            dosboxPath = string.Empty;
                        }

                        break;
                    case DialogResult.No:
                        dosboxPath = string.Empty;
                        break;
                }
            }
            return dosboxPath;
        }
    }
}
