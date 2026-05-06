using Commercially.Common.BlockEntities;
using Commercially.Common.BlockEntityBehaviors;
using Commercially.Common.Interfaces;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace Commercially.Common.GUI.Tabs
{
    public class GuiBlockEntityDebugTab : ModularTab
    {
        public const string CODE = "commercially.BlockEntityDebug";
        public override string Code => CODE;
        public override string TabName => Lang.Get("commercially:tabname-debug");

        public override void Initialize(IModularGui gui, BlockEntity entity = null)
        {
            base.Initialize(gui, entity);
            JsonObject config = GetConfiguration();
            string rstring = config["RandomString"].AsString();
        }

        public override void Compose(GuiComposer composer, ElementBounds rootBounds)
        {
            BECommercialBase commercialBase = BlockEntity as BECommercialBase;
            IOwnable ownableBehavior = commercialBase?.Ownable;
            BEBehaviorTextureSwappable behaviorTextureSwappable = commercialBase?.GetBehavior<BEBehaviorTextureSwappable>();

            ElementBounds nameLabel = ElementBounds.FixedSize(400, 30).WithFixedOffset(0.0,GuiStyle.TitleBarHeight);
            ElementBounds ownableTypeLabel = ElementBounds.FixedSize(400, 30).FixedUnder(nameLabel);
            ElementBounds isAdminOwnedLabel = ElementBounds.FixedSize(400, 30).FixedUnder(ownableTypeLabel);
            ElementBounds ownerNameLabel = ElementBounds.FixedSize(400, 30).FixedUnder(isAdminOwnedLabel);
            ElementBounds ownerUIDLabel = ElementBounds.FixedSize(400, 30).FixedUnder(ownerNameLabel);
            ElementBounds idLabel = ElementBounds.FixedSize(400, 30).FixedUnder(ownerUIDLabel);
            ElementBounds parentLabel = ElementBounds.FixedSize(400, 30).FixedUnder(idLabel);
            rootBounds.WithChildren(nameLabel, ownableTypeLabel, isAdminOwnedLabel, ownerNameLabel, ownerUIDLabel, idLabel, parentLabel);

            ElementBounds primaryMatLabel = ElementBounds.FixedSize(400, 30).FixedUnder(parentLabel);
            ElementBounds secondaryMatLabel = ElementBounds.FixedSize(400, 30).FixedUnder(primaryMatLabel);
            ElementBounds decoMatLabel = ElementBounds.FixedSize(400, 30).FixedUnder(secondaryMatLabel);
            rootBounds.WithChildren(primaryMatLabel, secondaryMatLabel, decoMatLabel);

            CairoFont debugFont = CairoFont.WhiteSmallishText();

            composer.AddDynamicText($"Name: {ownableBehavior?.Name ?? "N/A"}", debugFont,  nameLabel);
            composer.AddDynamicText($"Ownable Type: {ownableBehavior?.OwnableType ?? "N/A"}", debugFont, ownableTypeLabel);
            composer.AddDynamicText($"Admin Owned: {ownableBehavior?.IsAdminOwned.ToString() ?? "N/A"}", debugFont, isAdminOwnedLabel);
            composer.AddDynamicText($"Owner Name: {ownableBehavior?.OwnerName ?? "N/A"}", debugFont, ownerNameLabel);
            composer.AddDynamicText($"Owner UID: {ownableBehavior?.OwnerUID ?? "N/A"}", debugFont, ownerUIDLabel);
            composer.AddDynamicText($"Ownership ID: {(ownableBehavior as IOwnableReference)?.ID.ToString() ?? "N/A"}", debugFont, idLabel);
            composer.AddDynamicText($"Parent ID: {(ownableBehavior as IOwnableChild)?.ParentID.ToString() ?? "N/A"}", debugFont, parentLabel);
            composer.AddDynamicText($"Primary Material: {behaviorTextureSwappable?.PrimaryMaterial ?? "N/A"}", debugFont, primaryMatLabel);
            composer.AddDynamicText($"Secondary Material: {behaviorTextureSwappable?.SecondaryMaterial ?? "N/A"}", debugFont, secondaryMatLabel);
            composer.AddDynamicText($"Deco Material: {behaviorTextureSwappable?.DecoMaterial ?? "N/A"}", debugFont, decoMatLabel);

        }

        public override bool IsVisible(GuiDialog gui)
        {
            return true;
        }

        public override void OnGuiClosed()
        {
            
        }

        public override void OnGuiOpened()
        {
            
        }
    }
}
