using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZerodhaTaxHarvester.model
{
    public class MutualFund
    {
        private String name;
        private int schemeCode;

        public string Name { get => name;}
        public int SchemeCode { get => schemeCode; }
        //public double Nav { get => nav; }

        public MutualFund(string name, int schemeCode)
        {
            this.name = name;
            this.schemeCode = schemeCode;
        }
    }
}
