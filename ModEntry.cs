using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace AquariumFishIndicator
{
    public static class Utils
    {
        /// <summary>
        /// Checks if a fish item has not been donated to the aquarium yet.
        /// This uses the same logic as the StardewAquarium mod.
        /// </summary>
        public static bool IsUnDonatedFish(Item item)
        {
            if (item == null || item.Category != -4)
            {
                return false;
            }

            try
            {
                // Check if the fish donation flag exists in master player's mail
                string donationFlag = $"AquariumDonated:{item.Name.Replace(" ", string.Empty)}";
                return !Game1.MasterPlayer.mailReceived.Contains(donationFlag);
            }
            catch
            {
                // If there's any error, assume it's not donatable
                return false;
            }
        }
    }
    public class ModEntry : Mod
    {
        private IModHelper helper;
        private ModConfig config;
        private Texture2D curatorEmojiTexture;        // Add a rendering event to track hovered items like UIInfoSuite2 does
        private Item _hoverItem;

        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            this.config = helper.ReadConfig<ModConfig>();

            // Load the curator emoji texture
            this.curatorEmojiTexture = helper.ModContent.Load<Texture2D>("assets/tilesheet/curator_emoji.png");

            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Display.Rendering += OnRendering; // Track hovered items
        }

        private void OnRendering(object sender, EventArgs e)
        {
            _hoverItem = GetHoveredItem();
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu == null && config.ShowDonationStatus)
            {
                DrawFishDonationIndicator(e.SpriteBatch);
            }
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (config.ShowDonationStatus)
            {
                DrawFishDonationIndicator(e.SpriteBatch);
            }
        }
        private void DrawFishDonationIndicator(SpriteBatch spriteBatch)
        {
            Item hoveredItem = _hoverItem;

            if (hoveredItem != null && IsFish(hoveredItem))
            {
                bool isUndonated = Utils.IsUnDonatedFish(hoveredItem);

                // Only show indicator if fish is not donated yet
                if (isUndonated)
                {
                    DrawCuratorIcon(spriteBatch, hoveredItem);
                }
            }
        }

        private bool IsFish(Item item)
        {
            // Method 1: Check if it's a fish object
            if (item is StardewValley.Object obj)
            {
                // Fish category is -4
                return obj.Category == StardewValley.Object.FishCategory;
            }

            // Method 2: Alternative check using item type
            return item.Category == -4;
        }

        public static Item GetHoveredItem()
        {
            Item hoverItem = null;

            // Check toolbar hover when no menu is active
            if (Game1.activeClickableMenu == null && Game1.onScreenMenus != null)
            {
                hoverItem = Game1.onScreenMenus
                    .OfType<Toolbar>()
                    .Select(tb => tb.hoverItem)
                    .FirstOrDefault(hi => hi is not null);
            }

            // Check inventory page in game menu
            if (Game1.activeClickableMenu is GameMenu gameMenu &&
                gameMenu.GetCurrentPage() is InventoryPage inventory)
            {
                hoverItem = inventory.hoveredItem;
            }

            // Check item grab menu (chests, etc.)
            if (Game1.activeClickableMenu is ItemGrabMenu itemMenu)
            {
                hoverItem = itemMenu.hoveredItem;
            }

            // Check shops and other menus
            if (Game1.activeClickableMenu is ShopMenu shopMenu)
            {
                // ShopMenu.hoveredItem is ISalable, need to cast
                if (shopMenu.hoveredItem is Item item)
                    hoverItem = item;
            }

            return hoverItem;
        }
        private void DrawCuratorIcon(SpriteBatch spriteBatch, Item hoveredItem)
        {
            // Only draw if there's actually a tooltip being shown
            if (Game1.activeClickableMenu != null)
            {
                // For menu contexts, position relative to the menu
                var menu = Game1.activeClickableMenu;
                if (menu is ItemGrabMenu || menu is GameMenu)
                {
                    DrawIconForMenu(spriteBatch, hoveredItem, menu);
                }
            }
            else
            {
                // For HUD/toolbar context, position relative to cursor
                DrawIconForHud(spriteBatch, hoveredItem);
            }
        }

        private void DrawIconForMenu(SpriteBatch spriteBatch, Item hoveredItem, IClickableMenu menu)
        {
            // Calculate tooltip dimensions
            string hoverText = hoveredItem.getDescription();
            string hoverTitle = hoveredItem.DisplayName;
            
            Vector2 titleSize = Game1.smallFont.MeasureString(hoverTitle);
            Vector2 textSize = Game1.smallFont.MeasureString(hoverText);
            
            int windowWidth = Math.Max((int)titleSize.X, (int)textSize.X) + 32;
            int windowHeight = (int)(titleSize.Y + textSize.Y) + 32;
            
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();
            
            // Calculate tooltip position
            int tooltipX = mouseX + 32;
            int tooltipY = mouseY + 32;
            
            // Adjust if tooltip would go off screen
            if (tooltipX + windowWidth > Game1.uiViewport.Width)
            {
                tooltipX = Game1.uiViewport.Width - windowWidth;
            }
            if (tooltipY + windowHeight > Game1.uiViewport.Height)
            {
                tooltipY = mouseY - windowHeight - 16;
            }
            
            // Calculate icon position based on configuration
            Vector2 iconPosition = CalculateIconPosition(tooltipX, tooltipY, windowWidth, windowHeight);
            
            DrawIcon(spriteBatch, iconPosition);
        }

        private void DrawIconForHud(SpriteBatch spriteBatch, Item hoveredItem)
        {
            // For HUD context, position icon near the cursor but offset to avoid overlap
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();
            
            Vector2 iconPosition;
            
            if (config.IconPosition == IconPosition.FollowCursor)
            {
                // Position icon to the upper-right of cursor, with safe margins
                iconPosition = new Vector2(
                    mouseX + 48 + config.IconOffsetX, 
                    mouseY - 32 + config.IconOffsetY
                );
                
                // Ensure icon stays within screen bounds
                int iconSize = (int)(32 * config.IconScale);
                if (iconPosition.X + iconSize > Game1.uiViewport.Width)
                {
                    iconPosition.X = mouseX - 48 + config.IconOffsetX; // Move to left side of cursor
                }
                if (iconPosition.Y < 0)
                {
                    iconPosition.Y = mouseY + 32 + config.IconOffsetY; // Move below cursor
                }
            }
            else
            {
                // Use fixed positioning based on config
                iconPosition = new Vector2(mouseX + 48 + config.IconOffsetX, mouseY - 32 + config.IconOffsetY);
            }
            
            DrawIcon(spriteBatch, iconPosition);
        }

        private Vector2 CalculateIconPosition(int tooltipX, int tooltipY, int windowWidth, int windowHeight)
        {
            Vector2 position;
            int iconSize = (int)(32 * config.IconScale);
            
            switch (config.IconPosition)
            {
                case IconPosition.TopLeft:
                    position = new Vector2(tooltipX + 8, tooltipY + 8);
                    break;
                case IconPosition.TopRight:
                    position = new Vector2(tooltipX + windowWidth - iconSize - 8, tooltipY + 8);
                    break;
                case IconPosition.BottomLeft:
                    position = new Vector2(tooltipX + 8, tooltipY + windowHeight - iconSize - 8);
                    break;
                case IconPosition.BottomRight:
                    position = new Vector2(tooltipX + windowWidth - iconSize - 8, tooltipY + windowHeight - iconSize - 8);
                    break;
                default: // TopRight
                    position = new Vector2(tooltipX + windowWidth - iconSize - 8, tooltipY + 8);
                    break;
            }
            
            // Apply custom offsets
            position.X += config.IconOffsetX;
            position.Y += config.IconOffsetY;
            
            return position;
        }

        private void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
        {
            // Draw a subtle background circle for better visibility if enabled
            if (config.ShowIconBackground)
            {
                int iconSize = (int)(32 * config.IconScale);
                spriteBatch.Draw(
                    Game1.fadeToBlackRect,
                    new Rectangle((int)position.X - 2, (int)position.Y - 2, iconSize + 4, iconSize + 4),
                    Color.Black * 0.5f
                );
            }
            
            // Draw the curator emoji icon
            spriteBatch.Draw(
                curatorEmojiTexture,
                position,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                config.IconScale,
                SpriteEffects.None,
                0.86f
            );
        }
    }
}