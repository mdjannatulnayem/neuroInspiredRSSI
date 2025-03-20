namespace RSSI_Nuro.Models
{
    public class ArchiveDataModel
    {
        [System.ComponentModel.DataAnnotations.Key]
        public DateTime Timestamp { get; set; }
        public double Bt { get; set; }
        public double BxGSM { get; set; }
        public double ByGSM { get; set; }
        public double BzGSM { get; set; }
        public double Density { get; set; }
        public double Speed { get; set; }
        public double Temperature { get; set; }
        public double Intensity { get; set; }
        public double Declination { get; set; }
        public double Inclination { get; set; }
        public double North { get; set; }
        public double East { get; set; }
        public double Vertical { get; set; }
        public double Horizontal { get; set; }
    }
}
