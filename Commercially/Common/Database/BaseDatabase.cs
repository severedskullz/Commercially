using Microsoft.Data.Sqlite;
using System;
using System.IO;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace Commercially.Common.Database
{
    public abstract class BaseDatabase
    {
        protected string ConnStr;
        protected ICoreServerAPI Api;


        protected SqliteConnection GetConnection()
        {
            if (string.IsNullOrEmpty(this.ConnStr))
            {
                throw new ArgumentNullException("No Connection String Provided");
            }
            return new SqliteConnection(this.ConnStr);
        }

        public BaseDatabase(ICoreServerAPI api, string dbName)
        {
            this.Api = api;
            string filePath = Path.Combine(GamePaths.DataPath, "ModData", api.World.SavegameIdentifier);
            string path = Path.Combine(filePath, dbName + ".db");

            this.ConnStr = "Data Source=" + path;
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(filePath);
                File.WriteAllBytes(path, []);
            }
        }

        public abstract void InitializeDB();
    }
}