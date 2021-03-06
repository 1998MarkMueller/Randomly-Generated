﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace BusinessImplementation
{
    public class FileHandler : GameBase
    {
        public static List<DataEntities.FileLists.FileList> _FileLists = new List<DataEntities.FileLists.FileList>();

        public static string _GamePath = "";
        public static string _GameDirectory = "";
        public static string _FilesDirectory = "";
        public static string _ModPath = "";

        public FileHandler (DataEntities.Game game)
        {
            this.Game = game;
        }

        public static void UpdateGamePaths()
        {
            _GameDirectory = Path.Combine(FileHandler._GamePath, "files/save");
            _FilesDirectory = Path.Combine(FileHandler._GamePath, "files");
            _ModPath = Path.Combine(FileHandler._GamePath, "mods");
        }

        public void UpdateAllFiles()
        {
            bool hasSaveFile = GameFileHandler.DoesSaveExist();

            var gameHandlerBLL = new BusinessImplementation.GameFileHandler(this.Game);
            var modHandlerBLL = new BusinessImplementation.ModHandler(this.Game);

            this.Game = gameHandlerBLL.GetGame();

            //Create all necessary files
            gameHandlerBLL.CreateGameFolder();
            modHandlerBLL.CreateModsFolder();
            CreateDefaultFiles();
            
            //Get File Lists
            var modsFileList = modHandlerBLL.LoadMods();
            var gameFileList = gameHandlerBLL.GetGameFileLists();

            _FileLists.AddRange(modsFileList);
            _FileLists.AddRange(gameFileList);

            //Save first time stuff in game
            if (hasSaveFile == false)
            {
                using (var generateRandomBLL = new BusinessImplementation.GenerateRandomObject(this.Game))
                {
                    this.Game.Races = generateRandomBLL.CreateAllRaces();

                    this.Game.Player = generateRandomBLL.GetPlayer();
                }

                //Testing
                this.Game.Player.Location = new DataEntities.Location()
                {
                    Name = "Morrowind",
                    Destinations = new List<DataEntities.Destination>()
                    {
                        new DataEntities.Destination()
                        {
                            Name = "bar"
                        },
                        new DataEntities.Destination()
                        {
                            Name = "fat man"
                        }
                    }
                };

                SaveGameFile(Game);
            }
        }

        public static void CreateDefaultFiles()
        {
            var enums = Enum.GetValues(typeof(DataEntities.FileLists.FileList.FileTypes)).Cast<DataEntities.FileLists.FileList.FileTypes>().ToList();

            foreach (var enumType in enums)
            {
                if (enumType == DataEntities.FileLists.FileList.FileTypes.Game)
                    continue;

                var path = Path.Combine(_FilesDirectory, GetFileNameFromFileType(enumType));

                var defaultObject = GetClassFromFileType(enumType);

                var defaultValues = Newtonsoft.Json.JsonConvert.SerializeObject(defaultObject.GetDefaultValues(), Newtonsoft.Json.Formatting.Indented);

                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write(defaultValues);
                }
            }
        }

        public static DataEntities.FileLists.FileList GetRandom(DataEntities.FileLists.FileList.FileTypes fileTypes, Dictionary<string, object> options)
        {
            DataEntities.FileLists.FileList ret = null;

            var filesLookingFor = new List<DataEntities.FileLists.FileList>();

            var fileTypeType = GetTypeFromFileType(fileTypes);

            foreach (var file in _FileLists)
            {
                if (file.GetType() == fileTypeType)
                {
                    if (file.AllowedToAdd(options))
                    {
                        filesLookingFor.Add(file);
                    }
                }
            }

            Random rand = new Random();
            var r = rand.Next(0, filesLookingFor.Count);

            if (filesLookingFor.Any()) //Check if any were found
            {
                ret = filesLookingFor[r];
            }
            else //Error
            {

            }

            return ret;
        }

        public static System.Type GetTypeFromFileType(DataEntities.FileLists.FileList.FileTypes fileTypes)
        {
            System.Type ret = null;

            switch (fileTypes)
            {
                case DataEntities.FileLists.FileList.FileTypes.Names:
                    ret = typeof(DataEntities.FileLists.NameFile);
                    break;
                case DataEntities.FileLists.FileList.FileTypes.Genders:
                    ret = typeof(DataEntities.FileLists.GenderFile);
                    break;
                case DataEntities.FileLists.FileList.FileTypes.Races:
                    ret = typeof(DataEntities.FileLists.RaceFile);
                    break;
            }

            return ret;
        }

        public static DataEntities.FileLists.FileList GetClassFromFileType(DataEntities.FileLists.FileList.FileTypes fileTypes)
        {
            DataEntities.FileLists.FileList ret = null;

            switch (fileTypes)
            {
                case DataEntities.FileLists.FileList.FileTypes.Names:
                    ret = new DataEntities.FileLists.NameFile();
                    break;
                case DataEntities.FileLists.FileList.FileTypes.Genders:
                    ret = new DataEntities.FileLists.GenderFile();
                    break;
                case DataEntities.FileLists.FileList.FileTypes.Races:
                    ret = new DataEntities.FileLists.RaceFile();
                    break;
            }

            return ret;
        }

        public static string GetFileNameFromFileType(DataEntities.FileLists.FileList.FileTypes fileTypes)
        {
            switch (fileTypes)
            {
                case DataEntities.FileLists.FileList.FileTypes.Game:
                    return "game.json";
                case DataEntities.FileLists.FileList.FileTypes.Names:
                    return "names.json";
                case DataEntities.FileLists.FileList.FileTypes.Genders:
                    return "genders.json";
                case DataEntities.FileLists.FileList.FileTypes.Races:
                    return "races.json";
            }

            return "ThisShouldntBeHere.txt";
        }

        public void SaveGameFile(DataEntities.Game game)
        {
            string fileContents = "";

            fileContents = Newtonsoft.Json.JsonConvert.SerializeObject(game, Newtonsoft.Json.Formatting.Indented);

            var path = System.IO.Path.Combine(_GameDirectory, GetFileNameFromFileType(DataEntities.FileLists.FileList.FileTypes.Game));

            var gameFileHander = new BusinessImplementation.GameFileHandler(this.Game);
            gameFileHander.CreateGameFolder();

            if (File.Exists(path) == false) //Create game file if it doesn't exist
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.Write("");
                }
            }

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path))
            {
                sw.Write(fileContents);
            }
        }
    }
}
