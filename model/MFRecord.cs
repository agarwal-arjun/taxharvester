using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZerodhaTaxHarvester.model
{
    public class MFRecord
    {
        public MutualFund MutualFund { get; set; }
        public double PurchaseUnit { get; set; }
        public DateTime Date { get; set; }
        public double PurchaseUnitPrice { get; set; }

        public double Nav { get; set; }

        public int DBId { get; set; }


    }
}
