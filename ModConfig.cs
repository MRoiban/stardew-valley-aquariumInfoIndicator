namespace AquariumFishIndicator
{
    public class ModConfig
    {
        public bool ShowDonationStatus { get; set; } = true;
        public bool UseTextIndicator { get; set; } = true;
        public bool UseIconIndicator { get; set; } = false;
        public string DonatedText { get; set; } = "✓ Donated";
        public string NotDonatedText { get; set; } = "! Not Donated";
    }
}
