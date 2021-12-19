using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinigazStokService.Models
{
    public class AybsModel
    {
        public DateTime IslemTarih { get; set; }
        public decimal Endeks { get; set; }
        public string IslemKod { get; set; }
        public string IslemTip { get; set; }
        public string IslemAciklama { get; set; }
        public string SokulenDepo { get; set; }
        public string TakilanDepo { get; set; }
        public string IslemUser { get; set; }

        // CİHAZ BİLGİLERİ
        public string OlcumCihazTurKod { get; set; }
        public string OlcumCihazNo { get; set; }
        public bool Durum { get; set; }
        public string OlcumCihazTipKod { get; set; }
        public string Marka { get; set; }
        public string Model { get; set; }
        public int Basinc { get; set; }
        public string OlcumCihazCesitKod { get; set; }
        public int Carpan { get; set; }
        public int BasamakSayi { get; set; }
        public int DamgaYil { get; set; }
        public int KalibrasyonSure { get; set; }
        public string KadranTipKod { get; set; }
        public string KadranAd { get; set; }
    }
}
