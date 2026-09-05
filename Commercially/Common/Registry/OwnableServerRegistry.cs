using Commercially.Common.Database;
using Commercially.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Commercially.Common.Registry
{
    public class OwnableServerRegistry : IOwnableRegistry
    {
        //TODO: Better checks / seperation for DB commits
        //TODO: Events for Ownable Loaded/Addition/Removal? Might help with cleaning up other data when an ownable is removed and we have references to it in other places
        private readonly ILogger Logger;
        private readonly CommercialDatabase DB;
        private readonly Dictionary<string, Dictionary<long, OwnableRegistration>> OwnablesByOwner = new Dictionary<string, Dictionary<long, OwnableRegistration>>();
        private readonly Dictionary<long, OwnableRegistration> Ownables = new Dictionary<long, OwnableRegistration>();

        Dictionary<string, Dictionary<long, OwnableRegistration>> IOwnableRegistry.OwnablesByOwner => OwnablesByOwner;

        Dictionary<long, OwnableRegistration> IOwnableRegistry.Ownables => Ownables;

        public OwnableServerRegistry(CommerciallyModSystem modSys, CommercialDatabase db) { 
            DB = db;
            Logger = modSys.Mod.Logger;
        }

        public void Initialize()
        {
            Logger.Debug("+============== Loading Commercially ==============+");

            // TODO: I am still not sure re-doing what I did in 1.X - 5.X where we just load everything into memory is the best idea. At the same time, Im not sure if having to block
            // for I/O and hit the DB is good either. Atleast chunk loading and world is saved off-thread. Im probably severely overthinking it, but we will stick with it for now.
            List<OwnableRegistration> ownables =  DB.LoadAllOwnables();

            foreach (OwnableRegistration item in ownables)
            {
                if (!OwnablesByOwner.ContainsKey(item.OwnerUID))
                {
                    OwnablesByOwner.Add(item.OwnerUID, new Dictionary<long, OwnableRegistration>());
                }

                OwnablesByOwner[item.OwnerUID].Add(item.ID, item);
                Ownables.Add(item.ID, item);

                Logger.VerboseDebug($"Loaded Ownable: {item.Name} with ID: {item.ID} and Owner: {item.OwnerName} ({item.OwnerUID})");
            }
            Logger.Debug($"Loaded {ownables.Count} Ownables" );
            Logger.Debug("=============== Loaded Commercially ================");
        }

        public string[] GetAllOwners()
        {
            return OwnablesByOwner.Keys.ToArray();
        }

        public List<OwnableRegistration> GetAllOwnables()
        {
            return [.. Ownables.Values];
        }

        public List<OwnableRegistration> GetAllOwnablesForType(string type)
        {
            List<OwnableRegistration> results = new List<OwnableRegistration>(Ownables.Values.Count); // Might be a bit of an over optimization
            foreach (OwnableRegistration ownable in Ownables.Values)
            {
                if (ownable.Type == type)
                {
                    results.Add(ownable);
                }
            }
            return results;
        }

        public List<T> GetAllOwnablesForType<T>(string type) where T : OwnableRegistration
        {
            List<T> results = new List<T>(Ownables.Values.Count); // Might be a bit of an over optimization
            foreach (OwnableRegistration ownable in Ownables.Values)
            {
                if (ownable.Type == type)
                {
                    results.Add((T)ownable);
                }
            }
            return results;
        }

        public void ClearOwnable(long registerID)
        {
            if (Ownables.ContainsKey(registerID))
            {
                OwnableRegistration shop = GetOwnable(registerID);
                OwnablesByOwner[shop.OwnerUID].Remove(registerID);
                Ownables.Remove(registerID);
                DB.DeleteOwnableById(registerID);
            }
        }
        public void ClearOwnablePos(long id)
        {
            OwnableRegistration shop = GetOwnable(id);
            if (shop != null)
            {
                shop.Position = null;
                shop.BroadcastWaypoint = false;
                shop.WaypointColor = 0;
                shop.WaypointIcon = null;
                DB.UpdateOwnable(shop);
            }
        }

        public OwnableRegistration GetOwnable(long registerID)
        {
            if (registerID > 0 && Ownables.ContainsKey(registerID))
            {
                return Ownables[registerID];
            }
            return null;
        }

        public OwnableRegistration[] GetOwnablesForOwner(string owner)
        {
            if (OwnablesByOwner.TryGetValue(owner, out Dictionary<long, OwnableRegistration> value) && value != null)
            {
                return value.Values.ToArray();
            }
            return [];
        }

        public OwnableRegistration[] GetOwnablesForOwner(string owner, string[] types)
        {
            if (OwnablesByOwner.TryGetValue(owner, out Dictionary<long, OwnableRegistration> value) && value != null)
            {
                List<OwnableRegistration> list = new List<OwnableRegistration>();
                foreach (var item in list)
                {
                    foreach (var type in types)
                    {
                        if (item.Type == type)
                        {
                            list.Add(item);
                            break;
                        }
                    }
                }
                return list.ToArray();
            }
            return [];
        }

        public OwnableRegistration AddOwnable(IOwnableReference ownable)
        {
            OwnableRegistration reg = AddOwnable(ownable.OwnableType, ownable.OwnerUID, ownable.OwnerName, ownable.Name, ownable.Position, ownable.ID);
            ownable.SetIDInternal(reg.ID);
            return reg;
        }

        private OwnableRegistration AddOwnable(string type, string owner, string ownerName, string name, BlockPos pos, long ID = -1)
        {
            Logger.Audit($"Adding new Shop for {owner}  called {name} at position {pos}" );
            OwnableRegistration register = new OwnableRegistration() {Type = type, OwnerUID = owner, OwnerName = ownerName, Name = name, Position = pos, ID = ID };
            return AddOwnable(register);
        }

        public OwnableRegistration AddOwnable(OwnableRegistration ownable)
        {
            DB.AddOwnable(ownable);
            if (ownable.ID > 0)
            {
                if (!OwnablesByOwner.TryGetValue(ownable.OwnerUID, out Dictionary<long, OwnableRegistration> value))
                {
                    value = [];
                    OwnablesByOwner[ownable.OwnerUID] = value;
                }

                value[ownable.ID] = ownable;
                Ownables[ownable.ID] = ownable;
                Logger.Audit($"Added Ownable {ownable.Type} with ID {ownable.ID} and owner {ownable.OwnerUID}");
            }
            else
            {
                throw new ApplicationException("Failed to persist shop to DB");
            }


            return ownable;
        }

        public int GetCount()
        {
            int i = 0;
            foreach (Dictionary<long, OwnableRegistration> item in OwnablesByOwner.Values)
            {
                if (item != null)
                    i += item.Count;
            }

            return i;
        }

        public OwnableRegistration UpdateOwnable(OwnableRegistration ownable)
        {
            OwnableRegistration existing = GetOwnable(ownable.ID);
            if (existing != null)
            {
                DB.UpdateOwnable(ownable);
                Ownables[existing.ID] = ownable;

                //Only really needed from an Admin perspective to forcibly claim ownership
                if (existing.OwnerUID != ownable.OwnerUID)
                {
                    OwnablesByOwner[existing.OwnerUID].Remove(existing.ID);
                    if (!OwnablesByOwner.TryGetValue(ownable.OwnerUID, out Dictionary<long, OwnableRegistration> value) && value != null)
                    {
                        value = [];
                        OwnablesByOwner[ownable.OwnerUID] = value;
                    }

                    value.Add(existing.ID, ownable);
                }
                
                Logger.Audit($"Updating Ownable with ID = {ownable.ID}");
            }
            else
            {
                throw new ArgumentException("Tried to update a non-existant shop with ID " + ownable.ID);
            }
            return existing;

        }

        public void UpdateOwnableWaypoint(long ID, bool enabled, string icon = null, int color = 0)
        {
            OwnableRegistration shop = GetOwnable(ID);
            if (shop != null)
            {
                shop.BroadcastWaypoint = enabled;
                shop.WaypointIcon = icon;
                shop.WaypointColor = color;
                DB.UpdateOwnable(shop);
            }
        }

        public string GetOwnableName(long ID)
        {
            OwnableRegistration shop = GetOwnable(ID);
            if (shop == null)
            {
                return "Unknown Shop";
            } else
            {
                return shop.Name;
            }
        }

        public void UpdateOwnableWaypoint(int ID, bool enabled, string icon = null, int color = 0)
        {
            OwnableRegistration Ownable = GetOwnable(ID);
            if (Ownable != null)
            {
                Ownable.BroadcastWaypoint = enabled;
                Ownable.WaypointIcon = icon;
                Ownable.WaypointColor = color;
            }
        }


    }
}