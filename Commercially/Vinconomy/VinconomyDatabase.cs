using Commercially.Common.Database;
using Commercially.Vinconomy.Trading;
using Microsoft.Data.Sqlite;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Commercially.Vinconomy
{
    public class VinconomyDatabase : BaseDatabase
    {
        public VinconomyDatabase(ICoreServerAPI api) : base(api, "commerce")
        {
        }

        public override void InitializeDB()
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Sales (ShopId INTEGER, Customer TEXT, Month INTEGER, Year INTEGER, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes TEXT);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Products ( X INTEGER, Y INTEGER, Z INTEGER, StallSlot INTEGER, ShopId INTEGER, ProductName TEXT, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, TotalStock INTEGER, CurrencyName TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB, PRIMARY KEY (X,Y,Z, StallSlot));";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS PendingSales (Id INTEGER PRIMARY KEY AUTOINCREMENT, X INTEGER, Y INTEGER, Z INTEGER, StallSlot INTEGER, ShopId INTEGER, Customer TEXT, ProductName TEXT, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, CurrencyName TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB, Amount INTEGER);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS ShopPermissions (Id INTEGER, PlayerUid TEXT, PlayerName TEXT);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS CurrencyDefinitions (Id INTEGER PRIMARY KEY AUTOINCREMENT, ShopId INTEGER, CurrencyCode TEXT, CurrencyAttributes BLOB, IgnoreAttributes BOOLEAN, Supply INTEGER, IntervalType INTEGER, IntervalDuration INTEGER, IntervalPeriod INTEGER, IntervalAction INTEGER, IntervalActionValue INTEGER);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS ProductDefinitions (Id INTEGER PRIMARY KEY AUTOINCREMENT, ShopId INTEGER, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB,  IgnoreAttributes BOOLEAN, Supply INTEGER, IntervalType INTEGER, IntervalDuration INTEGER, IntervalPeriod INTEGER, IntervalAction INTEGER, IntervalActionValue INTEGER, SupplyThreshold INTEGER, ThresholdScale INTEGER, CurrencyLowQuantity INTEGER, CurrencyHighQuantity INTEGER, IdealSupply INTEGER, MaxSupply INTEGER, SalesContribute BOOLEAN, UnlimitedSupply BOOLEAN, UnlimitedDemand BOOLEAN);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS PlayerCooldowns (ShopId INTEGER, PlayerUid TEXT, LastAccessed TIMESTAMP, PRIMARY KEY (ShopId, PlayerUid));";
                cmd.ExecuteNonQuery();

                connection.Close();
            }
        }

        public void SavePurchase(TradeResult purchaseResult)
        {
            using (SqliteConnection connection = GetConnection())
            {
                ItemStack product = purchaseResult.ProductStacks[0].Clone();
                ItemStack currency = purchaseResult.CurrencyStacks[0].Clone();

                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.Parameters.Add("@ShopId", SqliteType.Integer).Value = purchaseResult.Request.ParentEntity.Ownable.ID;
                cmd.Parameters.Add("@Customer", SqliteType.Text).Value = purchaseResult.Request.Customer.PlayerUID;
                cmd.Parameters.Add("@Month", SqliteType.Integer).Value = purchaseResult.Request.Api.World.Calendar.Month;
                cmd.Parameters.Add("@Year", SqliteType.Integer).Value = purchaseResult.Request.Api.World.Calendar.Year;
                cmd.Parameters.Add("@ProductCode", SqliteType.Text).Value = product.Collectible.Code.ToString();
                cmd.Parameters.Add("@ProductQuantity", SqliteType.Text).Value = purchaseResult.ProductStacks.TotalCount;
                cmd.Parameters.Add("@ProductAttributes", SqliteType.Text).Value = product.Attributes.ToJsonToken(); //TODO: This has CONSISTENTLY failed in the past due to Tyron's poor escape-sequencing for quotes in strings. Serialize to Binary in the future.
                cmd.Parameters.Add("@CurrencyCode", SqliteType.Text).Value = currency.Collectible.Code.ToString();
                cmd.Parameters.Add("@CurrencyQuantity", SqliteType.Text).Value = purchaseResult.CurrencyStacks.TotalCount;
                cmd.Parameters.Add("@CurrencyAttributes", SqliteType.Text).Value = currency.Attributes.ToJsonToken(); //TODO: This has CONSISTENTLY failed in the past due to Tyron's poor escape-sequencing for quotes in strings. Serialize to Binary in the future.

                cmd.CommandText = @"SELECT Count(*) FROM Sales 
                                    WHERE ShopId = @ShopId 
                                        AND Customer = @Customer
                                        AND Month = @Month
                                        AND Year = @Year
                                        AND ProductCode = @ProductCode
                                        AND CurrencyCode = @CurrencyCode";

                int numRows = Convert.ToInt32(cmd.ExecuteScalar());
                if (numRows == 1)
                {
                    cmd.CommandText = @"UPDATE Sales 
                                    SET ProductQuantity = ProductQuantity + @ProductQuantity,
                                        CurrencyQuantity = CurrencyQuantity + @CurrencyQuantity 
                                    WHERE ShopId = @ShopId 
                                        AND Customer = @Customer
                                        AND Month = @Month
                                        AND Year = @Year
                                        AND ProductCode = @ProductCode
                                        AND CurrencyCode = @CurrencyCode";
                    cmd.ExecuteNonQuery();
                }
                else if (numRows == 0)
                {
                    cmd.CommandText = "INSERT INTO Sales VALUES (@ShopId, @Customer, @Month, @Year, @ProductCode, @ProductQuantity, @ProductAttributes, @CurrencyCode, @CurrencyQuantity, @CurrencyAttributes);";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Somehow have more than 1 sale record for purchase");
                }

                connection.Close();
            }
        }
    }
}