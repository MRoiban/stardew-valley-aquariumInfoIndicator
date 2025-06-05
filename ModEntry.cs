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
        private Point _lastMousePosition;

        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            this.config = helper.ReadConfig<ModConfig>();

            // Load the curator emoji texture with error handling
            try
            {
                this.curatorEmojiTexture = helper.ModContent.Load<Texture2D>("assets/tilesheet/curator_emoji.png");
                if (this.curatorEmojiTexture == null)
                {
                    this.Monitor.Log("Failed to load curator emoji texture - texture is null", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Error loading curator emoji texture: {ex.Message}", LogLevel.Error);
                this.curatorEmojiTexture = null; // Ensure it's explicitly null
            }

            helper.Events.Display.RenderedHud += OnRenderedHud;
            helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            helper.Events.Display.Rendering += OnRendering; // Track hovered items
        }

        private void OnRendering(object sender, EventArgs e)
        {
            Point currentMouse = new Point(Game1.getMouseX(), Game1.getMouseY());
            if (currentMouse != _lastMousePosition)
            {
                _hoverItem = GetHoveredItem();
                _lastMousePosition = currentMouse;
            }
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (Game1.activeClickableMenu == null && config.ShowDonationStatus && config.UseIconIndicator)
            {
                DrawFishDonationIndicator(e.SpriteBatch);
            }
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (config.ShowDonationStatus && config.UseIconIndicator)
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
            // Check if texture is available before drawing
            if (this.curatorEmojiTexture == null)
            {
                return; // Gracefully skip drawing if texture is not available
            }

            // Calculate tooltip dimensions like UIInfoSuite2 does
            string hoverText = hoveredItem.getDescription();
            string hoverTitle = hoveredItem.DisplayName;

            // Get the mouse position for tooltip calculation
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            // Calculate tooltip text dimensions
            Vector2 titleSize = Game1.smallFont.MeasureString(hoverTitle);
            Vector2 textSize = Game1.smallFont.MeasureString(hoverText);

            // Calculate window width and height like the game does for tooltips
            int windowWidth = Math.Max((int)titleSize.X, (int)textSize.X) + 32; // Add padding
            int windowHeight = (int)(titleSize.Y + textSize.Y) + 32; // Add padding for title and description

            // Calculate tooltip position (same logic as IClickableMenu.drawHoverText)
            int x = mouseX + 32;
            int y = mouseY + 32;

            // Adjust if tooltip would go off screen
            if (x + windowWidth > Game1.uiViewport.Width)
            {
                x = Game1.uiViewport.Width - windowWidth;
            }
            if (y + windowHeight > Game1.uiViewport.Height)
            {
                y = mouseY - windowHeight - 16;
            }
            // Position icon in upper-right corner of tooltip window
            Vector2 iconPosition = new Vector2(x + windowWidth - 25, y - 4); // 30px from right edge (accounting for icon size), 2px from top

            // Draw the custom curator emoji icon
            spriteBatch.Draw(
                curatorEmojiTexture,
                iconPosition,
                null, // Use entire texture
                Color.White,
                0f,
                Vector2.Zero,
                2.5f, // Scale (make it 2x size)
                SpriteEffects.None,
                0.86f // Same layer depth as UIInfoSuite2 uses
            );
        }
    }
}