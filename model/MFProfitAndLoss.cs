using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZerodhaTaxHarvester.model
{
    class MFProfitAndLoss
    {
        public Double CurrentNav { get; set; }
        public MFRecord MFRecord { get; set; }

        public Double PnL { get; set; }
        public Double Harvest { get; set; }
    }
}
