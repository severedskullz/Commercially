using Commercially.Common.BlockEntities;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Xml.Linq;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace Commercially.Common
{
    public class CommercialDatabase
    {
        private string ConnStr;
        ICoreServerAPI api;

        public CommercialDatabase(ICoreServerAPI api)
        {
            this.api = api;
            string filePath = Path.Combine(GamePaths.DataPath, "ModData", api.World.SavegameIdentifier);
            string path = Path.Combine(filePath, "commerce.db");

            this.ConnStr = "Data Source=" + path;
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(filePath);
                File.WriteAllBytes(path, new byte[0]);
            }


        }

        public void InitializeDB()
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Ownables (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Owner TEXT, OwnerName TEXT, ParentId INTEGER, X INTEGER, Y INTEGER, Z INTEGER, BroadcastWaypoint INTEGER, WaypointIcon TEXT, WaypointColor INTEGER);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Sales (ShopId INTEGER, Customer TEXT, Month INTEGER, Year INTEGER, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes TEXT);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Products ( X INTEGER, Y INTEGER, Z INTEGER, StallSlot INTEGER, ShopId INTEGER, ProductName TEXT, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, TotalStock INTEGER, CurrencyName TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB, PRIMARY KEY (X,Y,Z, StallSlot));";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Products ( X INTEGER, Y INTEGER, Z INTEGER, StallSlot INTEGER, ShopId INTEGER, ProductName TEXT, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, TotalStock INTEGER, CurrencyName TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB, PRIMARY KEY (X,Y,Z, StallSlot));";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS PendingSales (Id INTEGER PRIMARY KEY AUTOINCREMENT, X INTEGER, Y INTEGER, Z INTEGER, StallSlot INTEGER, ShopId INTEGER, Customer TEXT, ProductName TEXT, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, CurrencyName TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB, Amount INTEGER);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS ShopPermissions (Id INTEGER, PlayerUid TEXT, PlayerName TEXT);";
                cmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        protected SqliteConnection GetConnection()
        {
            if (string.IsNullOrEmpty(this.ConnStr))
            {
                throw new ArgumentNullException("No Connection String Provided");
            }
            return new SqliteConnection(this.ConnStr);
        }

        public void UpdateOwnableEntry(IOwnableReference ownable)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();

                cmd.CommandText = @"UPDATE Ownables SET Name = @Name, Owner = @Owner, OwnerName = @OwnerName, X = @X, Y = @Y, Z = @Z, ParentId = @ParentID WHERE ID = @ID;";
                cmd.Parameters.Add("@Name", SqliteType.Text).Value = ownable.Name != null ? ownable.Name : DBNull.Value;
                cmd.Parameters.Add("@Owner", SqliteType.Text).Value = ownable.OwnerUID != null ? ownable.OwnerUID : DBNull.Value;
                cmd.Parameters.Add("@OwnerName", SqliteType.Text).Value = ownable.OwnerName != null ? ownable.OwnerName : DBNull.Value;

                cmd.Parameters.Add("@ID", SqliteType.Integer).Value = ownable.ID > 0 ? ownable.ID : DBNull.Value;
                cmd.Parameters.Add("@ParentID", SqliteType.Integer).Value = ownable is IOwnableLeaf ? ownable.ID : DBNull.Value;

                if (ownable.Position == null)
                {
                    cmd.Parameters.Add("@X", SqliteType.Integer).Value = DBNull.Value;
                    cmd.Parameters.Add("@Y", SqliteType.Integer).Value = DBNull.Value;
                    cmd.Parameters.Add("@Z", SqliteType.Integer).Value = DBNull.Value;
                }
                else
                {
                    cmd.Parameters.Add("@X", SqliteType.Integer).Value = ownable.Position.X;
                    cmd.Parameters.Add("@Y", SqliteType.Integer).Value = ownable.Position.Y;
                    cmd.Parameters.Add("@Z", SqliteType.Integer).Value = ownable.Position.Z;
                }

                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteOwnable(IOwnableReference entity)
        {
            DeleteOwnableById(entity.ID);
        }

        public void DeleteOwnableById(long ID)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();

                cmd.Parameters.Add("@ID", SqliteType.Integer).Value = ID;

                cmd.CommandText = "DELETE FROM Ownables WHERE Id = @ID;";
                cmd.ExecuteNonQuery();
            }

        }

        public void ClearPositionForOwnable(IOwnableReference entity)
        {
            ClearPositionForOwnableById(entity.ID);
        }

        /// <summary>
        /// Clears the location and parent of this Ownable. This typically happens when the block is broken and picked up.
        /// As the ID of the Ownable should be persisted to ItemStack, when it is placed back down again, the Block Position should be set again via UpdateOwnableEntry(IOwnableReference)
        /// </summary>
        /// <param name="ID"></param>
        public void ClearPositionForOwnableById(long ID)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();

                cmd.Parameters.Add("@ID", SqliteType.Integer).Value = ID;

                cmd.CommandText = @"UPDATE Ownables SET X = NULL, Y = NULL, Z = NULL, ParentId = NULL WHERE ID = @ID;";
                cmd.ExecuteNonQuery();
            }

        }

        public void AddOwnable(IOwnableReference ownable)
        {

            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO Ownables VALUES (@ID, @Name, @Owner, @OwnerName, NULL, @X, @Y, @Z,  false, NULL, NULL);";

                cmd.Parameters.Add("@Name", SqliteType.Text).Value = ownable.Name ?? "";
                cmd.Parameters.Add("@Owner", SqliteType.Text).Value = ownable.OwnerUID ?? "";
                cmd.Parameters.Add("@OwnerName", SqliteType.Text).Value = ownable.OwnerName ?? "";

                if (ownable.ID > 0)
                {
                    cmd.Parameters.Add("@ID", SqliteType.Integer).Value = ownable.ID;
                }
                else
                {
                    cmd.Parameters.Add("@ID", SqliteType.Integer).Value = DBNull.Value;
                }


                if (ownable.Position == null)
                {
                    cmd.Parameters.Add("@X", SqliteType.Integer).Value = DBNull.Value;
                    cmd.Parameters.Add("@Y", SqliteType.Integer).Value = DBNull.Value;
                    cmd.Parameters.Add("@Z", SqliteType.Integer).Value = DBNull.Value;
                }
                else
                {
                    cmd.Parameters.Add("@X", SqliteType.Integer).Value = ownable.Position.X;
                    cmd.Parameters.Add("@Y", SqliteType.Integer).Value = ownable.Position.Y;
                    cmd.Parameters.Add("@Z", SqliteType.Integer).Value = ownable.Position.Z;
                }

                cmd.ExecuteNonQuery();

                // Set the ID on the Ownable object that was generated by the database
                cmd.CommandText = "SELECT last_insert_rowid()";
                long lastId = Convert.ToInt64(cmd.ExecuteScalar());
                ownable.SetIDInternal(lastId);
            }
        }
    }
}
