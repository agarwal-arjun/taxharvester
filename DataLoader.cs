using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ZerodhaTaxHarvester.model;

namespace ZerodhaTaxHarvester
{
    class DataLoader
    {
        public static double HARVEST_LIMIT = 100000;
        //Captial letter start
        //REturn type to map of Mfund and Price?
        public static List<MFRecord> importMFData(DateTime dateTime)
        {
            dateTime = dateTime.AddDays(-1);
            List<MFRecord> mfData = new List<MFRecord>();
            //TODO - code to pull and parse data from
            var dateStr=dateTime.ToString("dd-MMM-yyyy");
            //Move to constants or settings file
            String url = $"http://portal.amfiindia.com/DownloadNAVHistoryReport_Po.aspx?tp=1&frmdt={dateStr}&todt={dateStr}";
            String result =  FetchDataAsync(url);
            List<List<String>> parsedResult = parse(result);
            foreach (List<String> lStr in parsedResult)
            {   if (lStr.ElementAt(1).ToLower().Contains("regular") || lStr.ElementAt(1).ToLower().Contains("retail"))
                {
                    continue;
                }
                MutualFund mf = new MutualFund(name: lStr.ElementAt(1), schemeCode: Int32.Parse(lStr.ElementAt(0)));
                MFRecord mFRecord = new MFRecord();
                mFRecord.MutualFund = mf;
                mFRecord.Date = dateTime;
                mFRecord.Nav = Double.Parse(lStr.ElementAt(4));

                mfData.Add(mFRecord);
            }
            
            return mfData;
        }


        //Use a different class with interface so we can switch implementation later
        public static List<MFProfitAndLoss> computePnL(List<MFRecord> mFRecords, Dictionary<int, MFRecord> mfWithCurrentNavMap)
        {
            List<MFProfitAndLoss> list= new List<MFProfitAndLoss>();
            foreach (MFRecord mf in mFRecords)
            {
                MFProfitAndLoss pnl = new MFProfitAndLoss();
                double pnlAmt = mf.PurchaseUnit * (mfWithCurrentNavMap[mf.MutualFund.SchemeCode].Nav - mf.PurchaseUnitPrice);
                pnl.MFRecord = mf;
                pnl.PnL = pnlAmt;
                list.Add(pnl);
            }
            //TODO list sort on pnl
            list=list.OrderBy(p => -p.PnL).ToList();
            double pnlSoFar = 0;
            foreach(MFProfitAndLoss pnl in list)
            {
                if (pnl.PnL <= 0)
                {
                    break;
                }

                if (pnl.PnL + pnlSoFar <= HARVEST_LIMIT)
                {
                    pnl.Harvest = pnl.PnL;
                    pnlSoFar += pnl.PnL;
                }
                else
                {
                    pnl.Harvest = HARVEST_LIMIT - pnlSoFar;
                }
            }
            return list;
        }


        private static string FetchDataAsync(String url)
        {
            HttpClient httpClient = new HttpClient();

           String result =   httpClient.GetStringAsync(url).Result;
            return result;
        }

        //Get list of an object or dictionary instead
        // Should use https://joshclose.github.io/CsvHelper/examples/reading/get-class-records/
        private static List<List<String>> parse(String result)
        {
            List<List<String>> mfData = new List<List<String>>();

            String resultClean = result.Replace("/\r ?\n / g", "\n");
            String[] resultArr = resultClean.Split('\n');
           
            foreach (String mfScheme in resultArr)
            {
                List<String> list = new List<string>();
                String[] mfSchemeElement=mfScheme.Split(';');
                if (mfSchemeElement.Length != 8 || mfSchemeElement[0]=="Scheme Code")
                {
                    continue;
                }
           
                foreach(String subStr in mfSchemeElement)
                {
                    list.Add(subStr);
                }
                mfData.Add(list);
            }
          

            return mfData;
        }

        public static double GetMFPriceForDate(MutualFund mf, DateTime dateTime)
        {
            List<MFRecord> mFunds = importMFData(dateTime);
            foreach(var mFund in mFunds)
            {
                if (mf.SchemeCode == mFund.MutualFund.SchemeCode)
                {
                    return mFund.Nav;
                }
            }
            //make specific
            throw new Exception("Mutual Fund not found");
        }

    }
}
