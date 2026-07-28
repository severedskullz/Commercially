namespace Commercially.Common
{
    public static class CommerciallyConstants
    {
        public const string COMM_CHANNEL = "Commercially";

        public const int TOGGLE_GUI = 4000;
        public const int OPEN_GUI=4001;
        public const int CLOSE_GUI=4002;
        public const int TAB_UPDATE = 4002;
        public const int GUI_UPDATE = 4012;

        public const int PURCHASE_ITEMS                 = 4100;
        public const int SET_ITEMS_PER_PURCHASE         = 4101;
        public const int SET_PARENT_ID                  = 4102;
        public const int SET_ADMIN_OWNED                = 4107;
        public const int SET_ITEM_PRICE                 = 4108;
        public const int SET_SHOULD_DISCARD_CURRENCY    = 4109;
        public const int SET_NAME                       = 4110;
        public const int SET_WAYPOINT                   = 4111;
        public const int TRANSFER_CONTENTS              = 4114;
        public const int TOGGLE_SLOT                    = 4113;

        // Reserved Logic Constants - Use these for any specific
        // implementation that you might want to share with other
        // mods. Vinconomy might use these for Stall-specific functions
        public const int RLOGIC_1                       = 4201;
        public const int RLOGIC_2                       = 4202;
        public const int RLOGIC_3                       = 4203;
        public const int RLOGIC_4                       = 4204;
        public const int RLOGIC_5                       = 4205;
        public const int RLOGIC_6                       = 4206;
        public const int RLOGIC_7                       = 4207;
        public const int RLOGIC_8                       = 4208;
        public const int RLOGIC_9                       = 4209;
        public const int RLOGIC_10                      = 4210;

        public const int DEFAULT_WAYPOINT_COLOR = 256;
        public const string DEFAULT_WAYPOINT_ICON = "genericOwnable";
    }
}
