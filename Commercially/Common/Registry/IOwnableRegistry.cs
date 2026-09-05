using Commercially.Common.Interfaces;
using System.Collections.Generic;

namespace Commercially.Common.Registry
{
    public interface IOwnableRegistry
    {
        protected Dictionary<string, Dictionary<long, OwnableRegistration>> OwnablesByOwner { get; }
        protected Dictionary<long, OwnableRegistration> Ownables { get; }

        public void Initialize();

        public string[] GetAllOwners();

        public List<OwnableRegistration> GetAllOwnables();
        public List<OwnableRegistration> GetAllOwnablesForType(string type);
        public List<T> GetAllOwnablesForType<T>(string type) where T : OwnableRegistration;

        public void ClearOwnable(long id);
        public void ClearOwnablePos(long id);

        public OwnableRegistration GetOwnable(long registerID);

        public OwnableRegistration[] GetOwnablesForOwner(string owner);
        public OwnableRegistration[] GetOwnablesForOwner(string owner, string[] type);

        public OwnableRegistration AddOwnable(IOwnableReference ownable);

        //public OwnableRegistration AddOwnable(string type, string owner, string ownerName, string name, BlockPos pos, long ID = -1);

        public OwnableRegistration AddOwnable(OwnableRegistration register);

        public int GetCount();

        //TODO: Old 5.3 logic doesnt really work here. We dont need to do all the checking nonsense and just update to the OwnableRegistration
        public OwnableRegistration UpdateOwnable(OwnableRegistration ownable);

        public void UpdateOwnableWaypoint(long ID, bool enabled, string icon = null, int color = 0);

        public string GetOwnableName(long ID);
    }
}
