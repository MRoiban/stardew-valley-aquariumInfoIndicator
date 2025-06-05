namespace AquariumFishIndicator
{
    public class ModConfig
    {
        public bool ShowDonationStatus { get; set; } = true;
        public bool UseTextIndicator { get; set; } = true;
        public bool UseIconIndicator { get; set; } = false;
        public string DonatedText { get; set; } = "✓ Donated";
        public string NotDonatedText { get; set; } = "! Not Donated";
        
        // Icon positioning options
        public IconPosition IconPosition { get; set; } = IconPosition.TopRight;
        public int IconOffsetX { get; set; } = 0;
        public int IconOffsetY { get; set; } = 0;
        public float IconScale { get; set; } = 2.0f;
        public bool ShowIconBackground { get; set; } = true;
    }
    
    public enum IconPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        FollowCursor
    }
}
