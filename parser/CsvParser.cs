using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using ZerodhaTaxHarvester.model;

namespace ZerodhaTaxHarvester.parser
{
    class CsvParser
    {
        public static List<MFRecord> parseZerodhaTradeBookCSV(String path, Dictionary<String, MutualFund> mutualFundMap)
        {
            List<MFRecord> mfRecords = new List<MFRecord>();
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields().Skip(1);
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    String mfName = fields[0].Split('-')[0].Trim(' ');
                    String[] mfNameKeys = mfName.Split(' ');
                    String tradeType = fields[6];

                    if (tradeType.ToLower().Equals("buy"))
                    {

                        MutualFund fundObj = null;
                        foreach (String key in mutualFundMap.Keys)
                        {

                            String tKey = key.ToLower().Replace('-', ' ');
                            Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                            tKey = regex.Replace(tKey, " ");
                            /*foreach (String mfKey in mfNameKeys)
                            {
                                if (!tKey.Contains(mfKey.ToLower()+" "))
                                {
                                    found = false;
                                    break;
                                }
                            }*/
                            if (tKey.Contains(mfName.ToLower()))
                            {
                                fundObj = mutualFundMap[key];
                                break;
                            }

                        }
                        if (fundObj == null)
                        {
                            continue;
                        }

                        MFRecord mfR = new MFRecord();
                        mfR.MutualFund = fundObj;
                        mfR.PurchaseUnit = double.Parse(fields[7]);
                        mfR.PurchaseUnitPrice = double.Parse(fields[8]);
                        DateTime buyDate = DateTime.ParseExact(fields[2], "m/dd/yyyy", CultureInfo.InvariantCulture);
                        mfR.Date = buyDate;
                        mfRecords.Add(mfR);
                    }

                }
            }

            return mfRecords;
        }
    }
}
