using Commercially.Common.Interfaces;
using Commercially.Common.Registry;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace Commercially.Common.Database
{
    public class CommercialDatabase : BaseDatabase
    {

        public CommercialDatabase(ICoreServerAPI api) : base(api, "commerce")
        {

        }

        public override void InitializeDB()
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Ownables (Id INTEGER PRIMARY KEY AUTOINCREMENT, Type TEXT, Name TEXT, Owner TEXT, OwnerName TEXT, ParentId INTEGER, X INTEGER, Y INTEGER, Z INTEGER, BroadcastWaypoint INTEGER, WaypointIcon TEXT, WaypointColor INTEGER);";
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



        public void UpdateOwnable(OwnableRegistration ownable)
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
                cmd.Parameters.Add("@ParentID", SqliteType.Integer).Value = ownable.ParentId > 0 ? ownable.ParentId : DBNull.Value;

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

        public void AddOwnable(OwnableRegistration ownable)
        {

            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO Ownables VALUES (@ID, @Type, @Name, @Owner, @OwnerName, NULL, @X, @Y, @Z,  false, NULL, NULL);";

                cmd.Parameters.Add("@Name", SqliteType.Text).Value = ownable.Name ?? "";
                cmd.Parameters.Add("@Type", SqliteType.Text).Value = ownable.Type ?? "";
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
                ownable.ID = lastId;
                //ownable.SetIDInternal(lastId);
            }
        }

        public List<OwnableRegistration> GetAllOwnablesForOwner(string ownerUID)
        {
            List<OwnableRegistration> ownables = new List<OwnableRegistration>();
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM Ownables WHERE Owner = @Owner;";

                cmd.Parameters.Add("@Owner", SqliteType.Text).Value = ownerUID;

                SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    OwnableRegistration reg = ReadOwnable(reader);
                    ownables.Add(reg);
                }
            }

            return ownables;
        }

        public OwnableRegistration GetOwnable(string ownerUID, long? parentID)
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM Ownables WHERE Id = @ID AND Owner = @Owner;";

                cmd.Parameters.Add("@Owner", SqliteType.Text).Value = ownerUID;
                cmd.Parameters.Add("@ID", SqliteType.Integer).Value = parentID;

                SqliteDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return ReadOwnable(reader);
                }
            }

            return null;
        }

        public List<OwnableRegistration> LoadAllOwnables()
        {
            List<OwnableRegistration> ownables = new List<OwnableRegistration>();
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM Ownables";

                SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    OwnableRegistration reg = ReadOwnable(reader);
                    ownables.Add(reg);
                }
            }

            return ownables;
        }

        private OwnableRegistration ReadOwnable(SqliteDataReader reader)
        {
            OwnableRegistration reg = new OwnableRegistration();
            reg.ID = reader.GetInt32(0);
            reg.Type = reader.IsDBNull(1) ? null : reader.GetString(1);
            reg.Name = reader.IsDBNull(2) ? null : reader.GetString(2);
            reg.OwnerUID = reader.GetString(3);
            reg.OwnerName = reader.GetString(4);
            reg.ParentId = reader.IsDBNull(5) ? null : reader.GetInt32(5);
            if (reader.IsDBNull(6))
            {
                reg.Position = null;
            }
            else
            {
                reg.X = reader.GetInt32(6);
                reg.Y = reader.GetInt32(7);
                reg.Z = reader.GetInt32(8);
            }
            reg.BroadcastWaypoint = reader.GetBoolean(9);
            reg.WaypointIcon = reader.IsDBNull(10) ? null : reader.GetString(10); ;
            reg.WaypointColor = reader.IsDBNull(11) ? 0 : reader.GetInt32(11);

            return reg;
        }

    }
}
