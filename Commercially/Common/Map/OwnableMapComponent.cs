using Commercially.Common.Registry;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Commercially.Common.Map
{
    public class OwnableMapComponent : MapComponent
    {
        private Vec2f ViewPos = new();
        private readonly Vec4f Color = new();
        private readonly OwnableRegistration Waypoint;
        private readonly Matrixf MvMat = new();
        private readonly OwnableMapLayer WpLayer;
        private bool MouseOver;

        public OwnableMapComponent(int waypointIndex, OwnableRegistration waypoint, OwnableMapLayer wpLayer, ICoreClientAPI capi) : base(capi)
        {
            this.Waypoint = waypoint;
            this.WpLayer = wpLayer;
            ColorUtil.ToRGBAVec4f(this.Waypoint.WaypointColor, ref this.Color);
            Color.A = 1;
        }

        public override void Render(GuiElementMap map, float dt)
        {
            if (Waypoint == null || !Waypoint.BroadcastWaypoint)
            {
                return;
            }

            Vec3d pos = new(this.Waypoint.X, this.Waypoint.Y, this.Waypoint.Z);
            map.TranslateWorldPosToViewPos(pos, ref this.ViewPos);
            if (this.ViewPos.X < -10f || this.ViewPos.Y < -10f || this.ViewPos.X > map.Bounds.OuterWidth + 10.0 || this.ViewPos.Y > map.Bounds.OuterHeight + 10.0)
            {
                return;
            }
            float x = (float)(map.Bounds.renderX + this.ViewPos.X);
            float y = (float)(map.Bounds.renderY + this.ViewPos.Y);
            ICoreClientAPI api = map.Api;
            IShaderProgram prog = api.Render.GetEngineShader(EnumShaderProgram.Gui);
            prog.Uniform("rgbaIn", this.Color);
            prog.Uniform("extraGlow", 0);
            prog.Uniform("applyColor", 0);
            prog.Uniform("noTexture", 0f);
            float hover = (this.MouseOver ? 12 : 0) - 1.5f * Math.Max(1.5f, 1f / map.ZoomLevel);
            LoadedTexture tex;
            if (!this.WpLayer.TexturesByIcon.TryGetValue(this.Waypoint.WaypointIcon, out tex))
            {
                this.Waypoint.WaypointIcon = "genericOwnable";
                this.WpLayer.TexturesByIcon.TryGetValue("genericOwnable", out tex);
            }
            if (tex != null)
            {
                prog.BindTexture2D("tex2d", tex.TextureId, 0);
                prog.UniformMatrix("projectionMatrix", api.Render.CurrentProjectionMatrix);
                MvMat.Set(api.Render.CurrentModelviewMatrix).Translate(x, y, 60f).Scale(tex.Width + hover, tex.Height + hover, 0f).Scale(0.5f * WaypointMapComponent.IconScale, 0.5f * WaypointMapComponent.IconScale, 0f);
                Matrixf shadowMvMat = MvMat.Clone().Scale(1.25f, 1.25f, 1.25f);
                prog.Uniform("rgbaIn", new Vec4f(0f, 0f, 0f, 0.6f));
                prog.UniformMatrix("modelViewMatrix", shadowMvMat.Values);
                api.Render.RenderMesh(this.WpLayer.QuadModel);
                prog.Uniform("rgbaIn", this.Color);
                prog.UniformMatrix("modelViewMatrix", this.MvMat.Values);
                api.Render.RenderMesh(this.WpLayer.QuadModel);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void OnMouseMove(MouseEvent args, GuiElementMap mapElem, StringBuilder hoverText)
        {
            if (this.Waypoint.Position == null)
            {
                return;
            }

            Vec2f viewPos = new Vec2f();
            mapElem.TranslateWorldPosToViewPos(this.Waypoint.Position.ToVec3d(), ref viewPos);
            double x = viewPos.X + mapElem.Bounds.renderX;
            double y = viewPos.Y + mapElem.Bounds.renderY;

            double dX = args.X - x;
            double dY = args.Y - y;
            float size = RuntimeEnv.GUIScale * 8f;
            if (this.MouseOver = (Math.Abs(dX) < size && Math.Abs(dY) < size))
            {
                string text = Lang.Get("commercially:wp-" + Waypoint.WaypointIcon) + ": " + this.Waypoint.Name;

                hoverText.AppendLine(text);
            }
        }

    }
}