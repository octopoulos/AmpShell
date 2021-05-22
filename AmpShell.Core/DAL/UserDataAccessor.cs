﻿/*AmpShell : .NET front-end for DOSBox
 * Copyright (C) 2009, 2021 Maximilien Noal
 *This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>.*/

namespace AmpShell.Core.DAL
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using AmpShell.Core.AutoConfig;
    using AmpShell.Core.Model;
    using AmpShell.Core.Serialization;

    public static class UserDataAccessor
    {
        static UserDataAccessor() => UserData = new Preferences();

        /// <summary>
        /// Gets object to load and save user data through XML (de)serialization.
        /// </summary>
        public static Preferences UserData { get; private set; }

        public static string GetAnUniqueSignature()
        {
            string newSignature;
            do
            {
                Random randNumber = new Random();
                newSignature = randNumber.Next(1048576).ToString(CultureInfo.InvariantCulture);
            }
            while (IsItAnUniqueSignature(newSignature) == false);
            return newSignature;
        }

        public static Category GetCategoryWithSignature(string signature) => UserData.ListChildren.Cast<Category>().FirstOrDefault(x => x.Signature == signature);

        /// <summary>
        /// Returns the path to the user data file (AmpShell.xml).
        /// </summary>
        /// <returns>The absolute path to the user data file.</returns>
        public static string GetDataFilePath()
        {
            var appDataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmpShell\\AmpShell.xml");
            if (StringExt.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AmpShellDebug")) == false)
            {
                return appDataFile;
            }
            if (FileFinder.HasWriteAccessToAssemblyLocationFolder() == false)
            {
                var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AmpShell");
                if (Directory.Exists(appDataDir) == false)
                {
                    Directory.CreateDirectory(appDataDir);
                }
                return appDataFile;
            }
            else
            {
                var portableAppDataFile = Path.Combine(PathFinder.GetStartupPath(), "AmpShell.xml");
                if (File.Exists(portableAppDataFile))
                {
                    return portableAppDataFile;
                }
                else
                {
                    return appDataFile;
                }
            }
        }

        public static Game GetFirstGameWithName(string name)
        {
            if (StringExt.IsNullOrWhiteSpace(name))
            {
                return new Game();
            }
            var game = UserData.ListChildren.Cast<Category>().SelectMany(x => x.ListChildren.Cast<Game>()).FirstOrDefault(x => StringExt.IsNullOrWhiteSpace(x.Name) == false && x.Name.Trim().ToUpperInvariant() == name.Trim().ToUpperInvariant());
            if (game is null)
            {
                return new Game();
            }
            return game;
        }

        public static Game GetGameWithMainExecutable(string executablePath)
        {
            if (StringExt.IsNullOrWhiteSpace(executablePath))
            {
                return new Game();
            }
            var game = UserData.ListChildren.Cast<Category>().SelectMany(x => x.ListChildren.Cast<Game>()).FirstOrDefault(x => StringExt.IsNullOrWhiteSpace(x.DOSEXEPath) == false && x.DOSEXEPath.Trim().ToUpperInvariant() == executablePath.Trim().ToUpperInvariant());
            if (game is null)
            {
                return new Game()
                {
                    DOSEXEPath = executablePath
                };
            }
            return game;
        }

        public static Game GetGameWithSignature(string signature) => UserData.ListChildren.Cast<Category>().SelectMany(x => x.ListChildren.Cast<Game>()).FirstOrDefault(x => x.Signature == signature);

        /// <summary>
        /// Used when a new Category or Game is created : its signature must be unique so AmpShell
        /// can recognize it instantly.
        /// </summary>
        /// <param name="signatureToTest">A Category's or Game's signature..</param>
        /// <returns>Whether the signature equals none of the other ones, or not..</returns>
        public static bool IsItAnUniqueSignature(string signatureToTest)
        {
            for (int i = 0; i < UserData.ListChildren.Count; i++)
            {
                Category otherCat = (Category)UserData.ListChildren[i];
                if (otherCat.Signature != signatureToTest)
                {
                    if (otherCat.ListChildren.Count != 0)
                    {
                        for (int j = 0; j < otherCat.ListChildren.Count; j++)
                        {
                            Game otherGame = (Game)otherCat.ListChildren[j];
                            if (otherGame.Signature == signatureToTest)
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static void LoadUserSettingsAndRunAutoConfig()
        {
            UserData = new Preferences();
            string dataFilePath = GetDataFilePath();
            if (File.Exists(dataFilePath))
            {
                UserData = ObjectSerializer.Deserialize<Preferences>(dataFilePath);
            }
            for (int i = 0; i < UserData.ListChildren.Count; i++)
            {
                Category concernedCategory = (Category)UserData.ListChildren[i];
                for (int j = 0; j < concernedCategory.ListChildren.Count; j++)
                {
                    Game concernedGame = (Game)concernedCategory.ListChildren[j];
                    concernedGame.DOSEXEPath = concernedGame.DOSEXEPath.Replace("AppPath", PathFinder.GetStartupPath());
                    concernedGame.DBConfPath = concernedGame.DBConfPath.Replace("AppPath", PathFinder.GetStartupPath());
                    concernedGame.AdditionalCommands = concernedGame.AdditionalCommands.Replace("AppPath", PathFinder.GetStartupPath());
                    concernedGame.Directory = concernedGame.Directory.Replace("AppPath", PathFinder.GetStartupPath());
                    concernedGame.CDPath = concernedGame.CDPath.Replace("AppPath", PathFinder.GetStartupPath());
                    concernedGame.SetupEXEPath = concernedGame.SetupEXEPath.Replace("AppPath", PathFinder.GetStartupPath());
                    concernedGame.Icon = concernedGame.Icon.Replace("AppPath", PathFinder.GetStartupPath());
                }
            }
            UserData.DBDefaultConfFilePath = UserData.DBDefaultConfFilePath.Replace("AppPath", PathFinder.GetStartupPath());
            UserData.DBDefaultLangFilePath = UserData.DBDefaultLangFilePath.Replace("AppPath", PathFinder.GetStartupPath());
            UserData.DBPath = UserData.DBPath.Replace("AppPath", PathFinder.GetStartupPath());
            UserData.ConfigEditorPath = UserData.ConfigEditorPath.Replace("AppPath", PathFinder.GetStartupPath());
            UserData.ConfigEditorAdditionalParameters = UserData.ConfigEditorAdditionalParameters.Replace("AppPath", PathFinder.GetStartupPath());

            if (StringExt.IsNullOrWhiteSpace(UserData.DBPath))
            {
                UserData.DBPath = FileFinder.SearchDOSBox(dataFilePath, UserData.PortableMode);
            }
            else if (File.Exists(UserData.DBPath) == false)
            {
                UserData.DBPath = FileFinder.SearchDOSBox(dataFilePath, UserData.PortableMode);
            }
            if (StringExt.IsNullOrWhiteSpace(UserData.ConfigEditorPath))
            {
                UserData.ConfigEditorPath = FileFinder.SearchCommonTextEditor();
            }
            else if (File.Exists(UserData.ConfigEditorPath) == false)
            {
                UserData.ConfigEditorPath = FileFinder.SearchCommonTextEditor();
            }

            if (StringExt.IsNullOrWhiteSpace(UserData.DBDefaultConfFilePath))
            {
                UserData.DBDefaultConfFilePath = FileFinder.SearchDOSBoxConf(dataFilePath, UserData.DBPath);
            }
            else if (File.Exists(UserData.DBDefaultConfFilePath) == false)
            {
                UserData.DBDefaultConfFilePath = FileFinder.SearchDOSBoxConf(dataFilePath, UserData.DBPath);
            }

            if (StringExt.IsNullOrWhiteSpace(UserData.DBDefaultLangFilePath) == false)
            {
                UserData.DBDefaultLangFilePath = FileFinder.SearchDOSBoxLanguageFile(dataFilePath, UserData.DBPath);
            }
            else if (File.Exists(UserData.DBDefaultLangFilePath) == false)
            {
                UserData.DBDefaultLangFilePath = FileFinder.SearchDOSBoxLanguageFile(dataFilePath, UserData.DBPath);
            }
        }

        public static void SaveUserSettings()
        {
            //saves the data inside Amp by serializing it in AmpShell.xml
            if (!UserData.PortableMode)
            {
                ObjectSerializer.Serialize(GetDataFilePath(), UserData);
            }
            else
            {
                for (int i = 0; i < UserData.ListChildren.Count; i++)
                {
                    Category category = (Category)UserData.ListChildren[i];
                    for (int j = 0; j < category.ListChildren.Count; j++)
                    {
                        Game game = (Game)category.ListChildren[j];
                        game.DOSEXEPath = game.DOSEXEPath.Replace(PathFinder.GetStartupPath(), "AppPath");
                        game.DBConfPath = game.DBConfPath.Replace(PathFinder.GetStartupPath(), "AppPath");
                        game.AdditionalCommands = game.AdditionalCommands.Replace(PathFinder.GetStartupPath(), "AppPath");
                        game.Directory = game.Directory.Replace(PathFinder.GetStartupPath(), "AppPath");
                        game.CDPath = game.CDPath.Replace(PathFinder.GetStartupPath(), "AppPath");
                        game.SetupEXEPath = game.SetupEXEPath.Replace(PathFinder.GetStartupPath(), "AppPath");
                        game.Icon = game.Icon.Replace(PathFinder.GetStartupPath(), "AppPath");
                    }
                }
                UserData.DBDefaultConfFilePath = UserData.DBDefaultConfFilePath.Replace(PathFinder.GetStartupPath(), "AppPath");
                UserData.DBDefaultLangFilePath = UserData.DBDefaultLangFilePath.Replace(PathFinder.GetStartupPath(), "AppPath");
                UserData.DBPath = UserData.DBPath.Replace(PathFinder.GetStartupPath(), "AppPath");
                UserData.ConfigEditorPath = UserData.ConfigEditorPath.Replace(PathFinder.GetStartupPath(), "AppPath");
                UserData.ConfigEditorAdditionalParameters = UserData.ConfigEditorAdditionalParameters.Replace(PathFinder.GetStartupPath(), "AppPath");
                ObjectSerializer.Serialize(Path.Combine(PathFinder.GetStartupPath(), "AmpShell.xml"), UserData);
            }
        }

        public static bool ImportGamesAndCategories(string fileName)
        {
            if (File.Exists(fileName) == false)
            {
                throw new FileNotFoundException(fileName);
            }
            var dataImported = false;
            var importData = ObjectSerializer.Deserialize<Preferences>(fileName);
            foreach (var category in importData.ListChildren.Cast<Category>())
            {
                UserData.AddChild(category);
            }
            foreach (var category in UserData.ListChildren.Cast<Category>())
            {
                foreach (var game in category.ListChildren.Cast<Game>())
                {
                    game.Signature = GetAnUniqueSignature();
                }
                category.Signature = GetAnUniqueSignature();
                dataImported = true;
            }
            return dataImported;
        }

        public static string GetConfigEditorPath()
        {
            if (StringExt.IsNullOrWhiteSpace(UserData.ConfigEditorPath) == false && Path.Combine(Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.System)), "NOTEPAD.EXE").ToUpperInvariant() == UserData.ConfigEditorPath.ToUpperInvariant())
            {
                return Path.GetFileName(UserData.ConfigEditorPath).ToLowerInvariant();
            }
            return UserData.ConfigEditorPath;
        }
    }
}