using Commercially.Common.Database;
using Commercially.Vinconomy.Interfaces;
using Commercially.Vinconomy.Network.Packets;
using Commercially.Vinconomy.Trading;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using Vinconomy.Network.Packets;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace Commercially.Vinconomy
{
    public class VinconomyDatabase : BaseDatabase
    {

        Dictionary<long, ShopProductList> productListCache = new Dictionary<long, ShopProductList>();
        private long EXPIRE_TIME_MILLIS = 1000 * 60 * 10;

        public VinconomyDatabase(ICoreServerAPI api) : base(api, "commerce")
        {
        }

        public override void InitializeDB()
        {
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Products ( Id INTEGER, ShopId INTEGER, X INTEGER, Y INTEGER, Z INTEGER, StallSlot INTEGER, ProductName TEXT, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, TotalStock INTEGER, CurrencyName TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB, PRIMARY KEY (X,Y,Z, StallSlot));";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Sales (ShopId INTEGER, Customer TEXT, Month INTEGER, Year INTEGER, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS PendingSales (Id INTEGER PRIMARY KEY AUTOINCREMENT, X INTEGER, Y INTEGER, Z INTEGER, StallSlot INTEGER, ShopId INTEGER, Customer TEXT, ProductName TEXT, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, CurrencyName TEXT, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB, Amount INTEGER);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS ShopPermissions (Id INTEGER, PlayerUid TEXT, PlayerName TEXT);";
                cmd.ExecuteNonQuery();

                /*
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS CurrencyDefinitions (Id INTEGER PRIMARY KEY AUTOINCREMENT, ShopId INTEGER, CurrencyCode TEXT, CurrencyAttributes BLOB, IgnoreAttributes BOOLEAN, Supply INTEGER, IntervalType INTEGER, IntervalDuration INTEGER, IntervalPeriod INTEGER, IntervalAction INTEGER, IntervalActionValue INTEGER);";
                cmd.ExecuteNonQuery();

                
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS ProductDefinitions (Id INTEGER PRIMARY KEY AUTOINCREMENT, ShopId INTEGER, ProductCode TEXT, ProductQuantity INTEGER, ProductAttributes BLOB, CurrencyCode TEXT, CurrencyQuantity INTEGER, CurrencyAttributes BLOB,  IgnoreAttributes BOOLEAN, Supply INTEGER, IntervalType INTEGER, IntervalDuration INTEGER, IntervalPeriod INTEGER, IntervalAction INTEGER, IntervalActionValue INTEGER, SupplyThreshold INTEGER, ThresholdScale INTEGER, CurrencyLowQuantity INTEGER, CurrencyHighQuantity INTEGER, IdealSupply INTEGER, MaxSupply INTEGER, SalesContribute BOOLEAN, UnlimitedSupply BOOLEAN, UnlimitedDemand BOOLEAN);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS PlayerCooldowns (ShopId INTEGER, PlayerUid TEXT, LastAccessed TIMESTAMP, PRIMARY KEY (ShopId, PlayerUid));";
                cmd.ExecuteNonQuery();
                */
                connection.Close();
            }
        }

                

        public void SavePurchase(TradeResult purchaseResult) {
            TradeRequest req = purchaseResult.Request;
            SavePurchase(
                   req.ParentEntity.Ownable.ID,
                   req.Customer.PlayerUID,
                   req.ProductNeeded,
                   purchaseResult.TotalProductAmount,
                   req.CurrencyNeeded,
                   purchaseResult.TotalCurrencyAmount,
                   req.Api.World.Calendar.Month,
                   req.Api.World.Calendar.Year
               );
        
        }
        public void SavePurchase(PurchaseResult purchaseResult)
        {
            PurchaseRequest req = purchaseResult.Request;
            SavePurchase(
                req.ParentEntity.Ownable.ID,
                req.Customer.PlayerUID,
                req.ProductNeeded,
                purchaseResult.TotalProductAmount,
                req.CurrencyNeeded,
                purchaseResult.TotalCurrencyAmount,
                req.Api.World.Calendar.Month,
                req.Api.World.Calendar.Year
            );

        }

        public void SavePurchase(long ownableId, string customerUID, ItemStack product, int productAmount, ItemStack currency, int currencyAmount, int month, int year)
        {
            if (product == null || currency == null)
            {
                throw new ArgumentException("Could not persist purchase with no stock or currency");
            }

            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.Parameters.Add("@ShopId", SqliteType.Integer).Value = ownableId;
                cmd.Parameters.Add("@Customer", SqliteType.Text).Value = customerUID;
                cmd.Parameters.Add("@Month", SqliteType.Integer).Value = month;
                cmd.Parameters.Add("@Year", SqliteType.Integer).Value = year;
                cmd.Parameters.Add("@ProductCode", SqliteType.Text).Value = product.Collectible.Code.ToString();
                cmd.Parameters.Add("@ProductQuantity", SqliteType.Text).Value = productAmount;
                cmd.Parameters.Add("@ProductAttributes", SqliteType.Text).Value = AttributesToBytes(product);
                cmd.Parameters.Add("@CurrencyCode", SqliteType.Text).Value = currency.Collectible.Code.ToString();
                cmd.Parameters.Add("@CurrencyQuantity", SqliteType.Text).Value = currencyAmount;
                cmd.Parameters.Add("@CurrencyAttributes", SqliteType.Text).Value = AttributesToBytes(currency);

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

                cmd.Parameters.Add("@Id", SqliteType.Integer).Value = shop.Ownable.ID;
                cmd.Parameters.Add("@ShopId", SqliteType.Integer).Value = shop.Ownable.ParentID;
                cmd.Parameters.Add("@X", SqliteType.Integer).Value = pos.X;
                cmd.Parameters.Add("@Y", SqliteType.Integer).Value = pos.Y;
                cmd.Parameters.Add("@Z", SqliteType.Integer).Value = pos.Z;
                cmd.Parameters.Add("@StallSlot", SqliteType.Integer).Value = stallSlot;

                cmd.Parameters.Add("@ProductName", SqliteType.Text).Value = product.GetName();
                cmd.Parameters.Add("@ProductCode", SqliteType.Text).Value = product.Collectible.Code.ToString();
                cmd.Parameters.Add("@ProductQuantity", SqliteType.Integer).Value = product.StackSize;              
                cmd.Parameters.Add("@ProductAttributes", SqliteType.Blob).Value = AttributesToBytes(product);
                cmd.Parameters.Add("@TotalStock", SqliteType.Integer).Value = productCount;

                cmd.Parameters.Add("@CurrencyName", SqliteType.Text).Value = currency.GetName();
                cmd.Parameters.Add("@CurrencyCode", SqliteType.Text).Value = currency.Collectible.Code.ToString();
                cmd.Parameters.Add("@CurrencyQuantity", SqliteType.Integer).Value = currency.StackSize;
                cmd.Parameters.Add("@CurrencyAttributes", SqliteType.Blob).Value = AttributesToBytes(currency);



                cmd.CommandText = @"SELECT Count(*) FROM Products 
                                    WHERE Id = @Id 
                                        AND X = @X
                                        AND Y = @Y
                                        AND Z = @Z
                                        AND StallSlot = @StallSlot";

                int numRows = Convert.ToInt32(cmd.ExecuteScalar());
                if (numRows == 1)
                {
                    cmd.CommandText = @"UPDATE Products 
                                    SET ShopId = @ShopId 
                                        ProductName = @ProductName,
                                        ProductCode = @ProductCode, 
                                        ProductAttributes = @ProductAttributes,
                                        ProductQuantity = @ProductQuantity,
                                        TotalStock = @TotalStock,
                                        CurrencyName = @CurrencyName,
                                        CurrencyCode = @CurrencyCode,
                                        CurrencyAttributes = @CurrencyAttributes,
                                        CurrencyQuantity = @CurrencyQuantity 
                                    WHERE Id = @Id 
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
                    cmd.CommandText = "INSERT INTO Products VALUES (@Id, @ShopId, @X, @Y, @Z, @StallSlot,  @ProductName, @ProductCode, @ProductQuantity, @ProductAttributes, @TotalStock, @CurrencyName, @CurrencyCode, @CurrencyQuantity, @CurrencyAttributes);";
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Somehow have more than 1 product record for stall");
                }

                connection.Close();
            }
        }

        public ShopProductList GetShopProducts(long ID)
        {
            if (productListCache.ContainsKey(ID))
            {
                ShopProductList listing = productListCache[ID];
                // If the expiration timer is in the future, then simply return the cached copy.
                if (listing.ExpiresAt >= DateTime.UtcNow.Ticks)
                {
                    return listing;
                }
            }

            ShopProductList products = new ShopProductList();
            products.ExpiresAt = DateTime.UtcNow.Ticks + EXPIRE_TIME_MILLIS;
            using (SqliteConnection connection = GetConnection())
            {
                connection.Open();
                SqliteCommand cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT * FROM Products WHERE ShopID = @ShopId";
                cmd.Parameters.Add("@ShopId", SqliteType.Integer).Value = ID;
                SqliteDataReader reader = cmd.ExecuteReader();


                while (reader.Read())
                {
                    ShopProduct product = new ShopProduct();
                    product.ProductName = reader.GetString(5);
                    product.ProductCode = reader.GetString(6);
                    product.ProductQuantity = reader.GetInt32(7);
                    product.ProductAttributes = (byte[])reader.GetValue(8);
                    product.TotalStock = reader.GetInt32(9);
                    product.CurrencyName = reader.GetString(10);
                    product.CurrencyCode = reader.GetString(11);
                    product.CurrencyQuantity = reader.GetInt32(12);
                    product.CurrencyAttributes = (byte[])reader.GetValue(13);
                    products.Products.Add(product);
                }

            }

            productListCache[ID] = products;
            return products;
        }

        public static byte[] AttributesToBytes(ItemStack stack)
        {
            // All of this because Anego won't escape strings in their Json Tokenizer code... :/
            byte[] productAttributes;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    stack.Attributes.ToBytes(writer);
                    writer.Flush();
                }
                productAttributes = ms.ToArray();
            }
            return productAttributes;
        }
    }
}