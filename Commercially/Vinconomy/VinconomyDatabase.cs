using Commercially.Common.Database;
using Commercially.Common.Util;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Trading;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using Vinconomy.Network.Packets;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

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
            if (purchaseResult.ProductStacks.StackCount == 0 || purchaseResult.CurrencyStacks.StackCount == 0) {
                throw new ArgumentException("Could not persist purchase with no stock or currency");
            };

            ItemStack product = purchaseResult.ProductStacks[0].Clone();
            ItemStack currency = purchaseResult.CurrencyStacks[0].Clone();

            using (SqliteConnection connection = GetConnection())
            {
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

        public Dictionary<string, List<LedgerEntry>> LoadSales(int shopId, int month, int year)
        {
            return null;
        }

        public void SaveProductListing(IStallComponent shop, int stallSlot, ItemStack product, int productCount, ItemStack currency)
        {
            if (product == null || currency == null)
            {
                ClearStockForSlot(shop, stallSlot);
                return;
            }

            UpdateOrInsertStock(shop, stallSlot, product, productCount, currency);
        }


        public void ClearAllStock(IStallComponent shop)
        {
            if (shop != null)
            {
                using (SqliteConnection connection = GetConnection())
                {
                    BlockPos pos = shop.GetBlockEntity().Pos;

                    connection.Open();
                    SqliteCommand cmd = connection.CreateCommand();
                    cmd.Parameters.Add("@ShopId", SqliteType.Integer).Value = shop.Ownable.ID;
                    cmd.Parameters.Add("@X", SqliteType.Integer).Value = pos.X;
                    cmd.Parameters.Add("@Y", SqliteType.Integer).Value = pos.Y;
                    cmd.Parameters.Add("@Z", SqliteType.Integer).Value = pos.Z;

                    cmd.CommandText = @"DELETE FROM Products 
                                    WHERE ShopId = @ShopId 
                                        AND X = @X
                                        AND Y = @Y
                                        AND Z = @Z";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ClearStockForSlot(IStallComponent shop, int stallSlot)
        {
            if (shop != null)
            {
                using (SqliteConnection connection = GetConnection())
                {
                    BlockPos pos = shop.GetBlockEntity().Pos;

                    connection.Open();
                    SqliteCommand cmd = connection.CreateCommand();
                    cmd.Parameters.Add("@ShopId", SqliteType.Integer).Value = shop.Ownable.ID;
                    cmd.Parameters.Add("@StallSlot", SqliteType.Integer).Value = stallSlot;
                    cmd.Parameters.Add("@X", SqliteType.Integer).Value = pos.X;
                    cmd.Parameters.Add("@Y", SqliteType.Integer).Value = pos.Y;
                    cmd.Parameters.Add("@Z", SqliteType.Integer).Value = pos.Z;

                    cmd.CommandText = @"DELETE FROM Products 
                                    WHERE ShopId = @ShopId 
                                        AND X = @X
                                        AND Y = @Y
                                        AND Z = @Z
                                        AND StallSlot = @StallSlot";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateOrInsertStock(IStallComponent shop, int stallSlot, ItemStack product, int productCount, ItemStack currency)
        {
            using (SqliteConnection connection = GetConnection())
            {

                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();

                BlockPos pos = shop.GetBlockEntity().Pos;


                cmd.Parameters.Add("@ShopId", SqliteType.Integer).Value = shop.Ownable.ID;
                cmd.Parameters.Add("@StallSlot", SqliteType.Integer).Value = stallSlot;
                cmd.Parameters.Add("@X", SqliteType.Integer).Value = pos.X;
                cmd.Parameters.Add("@Y", SqliteType.Integer).Value = pos.Y;
                cmd.Parameters.Add("@Z", SqliteType.Integer).Value = pos.Z;
                cmd.Parameters.Add("@TotalStock", SqliteType.Integer).Value = productCount;

                cmd.Parameters.Add("@ProductName", SqliteType.Text).Value = product.GetName();
                cmd.Parameters.Add("@ProductCode", SqliteType.Text).Value = product.Collectible.Code.ToString();
                cmd.Parameters.Add("@ProductQuantity", SqliteType.Integer).Value = product.StackSize;
                cmd.Parameters.Add("@ProductAttributes", SqliteType.Text).Value = product.Attributes.ToJsonToken(); //TODO: This has CONSISTENTLY failed in the past due to Tyron's poor escape-sequencing for quotes in strings. Serialize to Binary in the future.

                cmd.Parameters.Add("@CurrencyName", SqliteType.Text).Value = currency.GetName();
                cmd.Parameters.Add("@CurrencyCode", SqliteType.Text).Value = currency.Collectible.Code.ToString();
                cmd.Parameters.Add("@CurrencyQuantity", SqliteType.Integer).Value = currency.StackSize;
                cmd.Parameters.Add("@CurrencyAttributes", SqliteType.Text).Value = currency.Attributes.ToJsonToken(); //TODO: This has CONSISTENTLY failed in the past due to Tyron's poor escape-sequencing for quotes in strings. Serialize to Binary in the future.



                cmd.CommandText = @"SELECT Count(*) FROM Products 
                                    WHERE ShopId = @ShopId 
                                        AND X = @X
                                        AND Y = @Y
                                        AND Z = @Z
                                        AND StallSlot = @StallSlot";

                int numRows = Convert.ToInt32(cmd.ExecuteScalar());
                if (numRows == 1)
                {
                    cmd.CommandText = @"UPDATE Products 
                                    SET 
                                        ProductName = @ProductName,
                                        ProductCode = @ProductCode, 
                                        ProductAttributes = @ProductAttributes,
                                        ProductQuantity = @ProductQuantity,
                                        TotalStock = @TotalStock,
                                        CurrencyName = @CurrencyName,
                                        CurrencyCode = @CurrencyCode,
                                        CurrencyAttributes = @CurrencyAttributes,
                                        CurrencyQuantity = @CurrencyQuantity 
                                    WHERE ShopId = @ShopId 
                                        AND X = @X
                                        AND Y = @Y
                                        AND Z = @Z
                                        AND StallSlot = @StallSlot";
                    cmd.ExecuteNonQuery();
                }
                else if (numRows == 0)
                {
                    //X INTEGER, Y INTEGER, Z INTEGER, StallSlot INTEGER, ShopId INTEGER,
                    //ProductName TEXT, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, TotalStock INTEGER,
                    //CurrencyName TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB
                    cmd.CommandText = "INSERT INTO Products VALUES (@X, @Y, @Z, @StallSlot, @ShopId, @ProductName, @ProductCode, @ProductQuantity, @ProductAttributes, @TotalStock, @CurrencyName, @CurrencyCode, @CurrencyQuantity, @CurrencyAttributes);";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Somehow have more than 1 product record for stall");
                }

                connection.Close();
            }
        }
    }
}