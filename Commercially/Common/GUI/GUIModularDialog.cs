using Vintagestory.API.Client;

namespace Commercially.Common.GUI
{
    public class GUIModularDialog : GuiDialogGeneric
    {
        public const string CODE = "commercially:ModularDialog";
        public GUIModularDialog(string DialogTitle, ICoreClientAPI capi) : base(DialogTitle, capi)
        {

        }
    }
}
