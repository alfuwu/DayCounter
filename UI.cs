using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace DayCounterMod {
    public class DayUIText : UIState {
        public UIElement area;
        public UIImage icon;
        public UIText text;

        public override void OnInitialize() {
            Client client = ModContent.GetInstance<Client>();
            area = new();
            AdjustLocation(client.AnchorLocation);
            area.Width.Set(182, 0);
            area.Height.Set(60, 0);
            area.Top.Set(-7, client.AnchorOffset.Y - 0.5f);
            area.Left.Set(10, client.AnchorOffset.X - 0.5f);

            text = new(client.GetServerDays(), 0.8f) {
                HAlign = 0,
                TextOriginX = 0
            };
            text.Width.Set(138, 0);
            text.Height.Set(34, 0);
            text.Top.Set(40, 0);
            text.Left.Set(48, 0);

            if (client.ShouldShowIcon) {
                icon = new(ModContent.Request<Texture2D>("DayCounterMod/sun")) {
                    VAlign = 1
                };
                icon.Width.Set(32, 0);
                icon.Height.Set(32, 0);
                icon.Left.Set(12, 0);
                area.Append(icon);
            }

            area.Append(text);
            Append(area);
        }

        public void AdjustLocation(string anchor) {
            switch (anchor) {
                default:
                case "Bottom Left":
                    area.HAlign = 0.0f;
                    area.VAlign = 1.0f;
                    break;
                case "Left":
                    area.HAlign = 0.0f;
                    area.VAlign = 0.5f;
                    break;
                case "Top Left":
                    area.HAlign = 0.0f;
                    area.VAlign = 0.1f;
                    break;
                case "Bottom Right":
                    area.HAlign = 1.0f;
                    area.VAlign = 1.0f;
                    break;
                case "Right":
                    area.HAlign = 1.0f;
                    area.VAlign = 0.5f;
                    break;
                case "Top Right":
                    area.HAlign = 1.0f;
                    area.VAlign = 0.0f;
                    break;
                case "Bottom":
                    area.HAlign = 0.5f;
                    area.VAlign = 1.0f;
                    break;
                case "Center": // why would you ever use this
                    area.HAlign = 0.5f;
                    area.VAlign = 0.5f;
                    break;
                case "Top":
                    area.HAlign = 0.5f;
                    area.VAlign = 0.0f;
                    break;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch) {
            string currentText = Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().type ?
                ModContent.GetInstance<Client>().GetPlayerDays(DayCounterMod.system.days, DayCounterMod.system.nights, Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().days, Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().nights) :
                ModContent.GetInstance<Client>().GetServerDays(DayCounterMod.system.days, DayCounterMod.system.nights, Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().days, Main.LocalPlayer.GetModPlayer<DayCounterPlayer>().nights);
            text.SetText(currentText);
        }
    }
}
