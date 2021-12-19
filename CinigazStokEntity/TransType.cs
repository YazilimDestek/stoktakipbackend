using System;
using System.Collections.Generic;
using System.Text;

namespace CinigazStokEntity
{
    public class TransType : BaseEntity
    {
        public string Name { get; set; }
        public string RefCode { get; set; }
        public bool DocumentRequired { get; set; }
        public bool UseForMobileBarcode { get; set; }
        public bool UseForMobileQrcode { get; set; }
    }
}
