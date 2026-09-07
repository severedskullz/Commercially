using Commercially.Common.Interfaces;
using Commercially.Common.ModSystems;
using Commercially.Common.Registry.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Commercially.Common.Registry
{
    public class OwnableClientRegistry : IOwnableRegistry
    {
        private readonly ILogger Logger;
        private readonly Dictionary<string, Dictionary<long, OwnableRegistration>> OwnablesByOwner = new Dictionary<string, Dictionary<long, OwnableRegistration>>();
        private readonly Dictionary<long, OwnableRegistration> Ownables = new Dictionary<long, OwnableRegistration>();

        Dictionary<string, Dictionary<long, OwnableRegistration>> IOwnableRegistry.OwnablesByOwner => OwnablesByOwner;

        Dictionary<long, OwnableRegistration> IOwnableRegistry.Ownables => Ownables;

        public OwnableClientRegistry(CommerciallyModSystem modSys) { 
            Logger = modSys.Mod.Logger;
        }

        public void Initialize()
        {

        }

        public string[] GetAllOwnableOwners()
        {
            return OwnablesByOwner.Keys.ToArray();
        }

        public List<OwnableRegistration> GetAllOwnables()
        {
            return [.. Ownables.Values];
        }

        public void ClearOwnable(long registerID)
        {
            if (Ownables.ContainsKey(registerID))
            {
                OwnableRegistration Ownable = GetOwnable(registerID);
                OwnablesByOwner[Ownable.OwnerUID].Remove(registerID);
                Ownables.Remove(registerID);
            }
        }
        public void ClearOwnablePos(long id)
        {
            OwnableRegistration Ownable = GetOwnable(id);
            if (Ownable != null)
            {
                Ownable.Position = null;
                Ownable.BroadcastWaypoint = false;
                Ownable.WaypointColor = 0;
                Ownable.WaypointIcon = null;
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
            if (OwnablesByOwner.ContainsKey(owner) && OwnablesByOwner[owner] != null)
            {
                return OwnablesByOwner[owner].Values.ToArray();
            }
            return [];
        }

        public OwnableRegistration AddOwnable(IOwnableReference ownable)
        {
            return AddOwnable(ownable.OwnableType, ownable.OwnerUID, ownable.OwnerName, ownable.Name, ownable.Position, ownable.ID);
        }

        public OwnableRegistration AddOwnable(string type, string owner, string ownerName, string name, BlockPos pos, long ID = -1)
        {
            //Console.WriteLine("Adding new Ownable for " + owner );
            OwnableRegistration register = new OwnableRegistration() {Type = type, OwnerUID = owner, OwnerName = ownerName, Name = name, Position = pos, ID = ID };
            return AddOwnable(register);
        }

        public OwnableRegistration AddOwnable(OwnableRegistration register)
        {

                if (!OwnablesByOwner.ContainsKey(register.OwnerUID) || OwnablesByOwner[register.OwnerUID] == null)
                {
                    OwnablesByOwner[register.OwnerUID] = new Dictionary<long, OwnableRegistration>();
                }

                OwnablesByOwner[register.OwnerUID][register.ID] = register;
                Ownables[register.ID] = register;
                Logger.Audit("Added Register with ID " + register.ID + " and owner " + register.OwnerUID);
 
            return register;
        }

        public int GetCount()
        {
            int i = 0;
            foreach (Dictionary<long, OwnableRegistration> item in OwnablesByOwner.Values)
            {
                if (item == null) 
                    continue;

                i += item.Count; 
            }

            return i;
        }

        //TODO: Old 5.3 logic doesnt really work here. We dont need to do all the checking nonsense and just update to the OwnableRegistration
        public OwnableRegistration UpdateOwnable(OwnableRegistration ownable)
        {
            OwnableRegistration existing = GetOwnable(ownable.ID);
            if (existing != null)
            {

                if (ownable.Name != null)
                {
                    existing.Name = ownable.Name;
                }
                existing.Position = ownable.Position;


                Logger.Audit($"Updating Ownable with ID = {ownable.ID}");
            }
            else
            {
                throw new ArgumentException("Tried to update a non-existant Ownable with ID " + ownable.ID);
            }
            return existing;

        }

        /*
        public OwnableRegistration UpdateOwnableConfig(int id, string description, string shortDescription, string webHook)
        {
            OwnableRegistration register = GetOwnable(id);
            if (register != null)
            {
                //Console.WriteLine("Updating configuration for existing Register with ID " + id);
                register.Description = description;
                register.ShortDescription = shortDescription;
                register.WebHook = webHook;

            }
            else
            {
                throw new ArgumentException("Tried to update a non-existant Ownable with ID " + id);
            }
            return register;
        }
        */

        public OwnableRegistration UpdateOwnable(long id, string name, BlockPos pos)
        {
            OwnableRegistration register = GetOwnable(id);
            if (register != null)
            {
                //Console.WriteLine("Updating existing Register with ID " + id);
                if (name != null) { 
                    register.Name = name;
                }
                register.Position = pos;

            }
            else
            {
                throw new ArgumentException("Tried to update a non-existant Ownable with ID " + id);
            }
            return register;

        }

        public void UpdateOwnableWaypoint(long ID, bool enabled, string icon = null, int color = 0)
        {
            OwnableRegistration Ownable = GetOwnable(ID);
            if (Ownable != null)
            {
                Ownable.BroadcastWaypoint = enabled;
                Ownable.WaypointIcon = icon;
                Ownable.WaypointColor = color;
            }
        }

        public void UpdateOwnableFromServer(OwnableUpdatePacket packet)
        {
            if (packet.IsRemoval)
            {
                ClearOwnable(packet.ID);
                return;
            }



            OwnableRegistration reg = GetOwnable(packet.ID);
            if (reg == null)
            {
                reg = new OwnableRegistration();
                reg.ID = packet.ID;
                Ownables[packet.ID] = reg;
                if (!OwnablesByOwner.ContainsKey(packet.OwnerUID))
                {
                    OwnablesByOwner[packet.OwnerUID] = new Dictionary<long, OwnableRegistration>();
                }
                OwnablesByOwner[packet.OwnerUID][packet.ID] = reg;
            }
            //reg.Permissions = packet.Permissions;
            //reg.StallPermissions = packet.StallPermissions;
            reg.Name = packet.Name;
            reg.OwnerUID = packet.OwnerUID;
            reg.Type = packet.Type;
            reg.X = packet.X;
            reg.Y = packet.Y;
            reg.Z = packet.Z;
            reg.BroadcastWaypoint = packet.BroadcastWaypoint;
            if (reg.BroadcastWaypoint)
            {
                reg.WaypointIcon = packet.WaypointIcon;
                reg.WaypointColor = packet.WaypointColor;
            }
            
            /*
            reg.ShortDescription = packet.ShortDescription;
            reg.Description = packet.Description;
            reg.WebHook = packet.WebHook;
            */
           
            
        }

        public string GetOwnableName(long ID)
        {
            OwnableRegistration Ownable = GetOwnable(ID);
            if (Ownable == null)
            {
                return "Unknown Ownable";
            } else
            {
                return Ownable.Name;
            }
        }

        public string[] GetAllOwners()
        {
            return [.. OwnablesByOwner.Keys];
        }



        public OwnableRegistration[] GetOwnablesForOwner(string owner, string[] types)
        {
            if (OwnablesByOwner.TryGetValue(owner, out Dictionary<long, OwnableRegistration> value) && value != null)
            {
                List<OwnableRegistration> list = new List<OwnableRegistration>();
                foreach (var item in value.Values)
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
    }
}