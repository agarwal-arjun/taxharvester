using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZerodhaTaxHarvester.model;
using ZerodhaTaxHarvester.parser;

namespace ZerodhaTaxHarvester
{
    public partial class Form1 : Form
    {
        
        public Dictionary<string, MutualFund> MFMap = new Dictionary<string, MutualFund>();
        public Dictionary<int, MFRecord> MFSchemeCodeMap = new Dictionary<int, MFRecord>();

        private DatabaseHelper databaseHelper;

        public Form1()
        {
            //comment lines 28 to 36 and set the variable appDataPath as your local source code folder to work in local
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.mdf");
            string programDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ZerodhaTaxHarvester");
            string programDataPath = Path.Combine(programDataDir,"Database.mdf");

            if (!File.Exists(programDataPath)) {
                System.IO.Directory.CreateDirectory(programDataDir);
                System.IO.File.Copy(dbPath,programDataPath);
            }

            string connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={programDataPath};Integrated Security=True";
            Console.WriteLine(connectionString);
            databaseHelper = new DatabaseHelper(connectionString);

            InitializeComponent();
            List<MFRecord> mfData = DataLoader.importMFData(DateTime.Now);          

            foreach (MFRecord mf in mfData)
            {
                MFMap.Add(mf.MutualFund.Name, mf.MutualFund);
                MFSchemeCodeMap.Add(mf.MutualFund.SchemeCode, mf);
                this.comboBox1.Items.Add(mf.MutualFund.Name);
            }
            this.UpdateDataGrid();

            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {        
            var schemeName = this.comboBox1.Text;
            var schemeId = this.MFMap[schemeName].SchemeCode;
            var purchaseQuantity = int.Parse(this.textBox4.Text);
            var date = this.dateTimePicker1.Value;

            var mutualFund = new MutualFund(schemeName, schemeId);
            var mfRecord = new MFRecord { MutualFund = mutualFund, PurchaseUnit = purchaseQuantity, Date = date, PurchaseUnitPrice = DataLoader.GetMFPriceForDate(mutualFund, date) };
            databaseHelper.AddEntry(mfRecord);
            UpdateDataGrid();                
        }

        private void UpdateDataGrid()
        {           
            this.dataGridView1.Rows.Clear();
            this.dataGridView1.Refresh();
            var mFRecords = databaseHelper.ListAll();
            var mfProfitAndLoss = DataLoader.computePnL(mFRecords,MFSchemeCodeMap);
            int count = 0;
            foreach (var pnlEntry in mfProfitAndLoss)
            {
                this.dataGridView1.Rows.Add();
                this.dataGridView1[0, count].Value = pnlEntry.MFRecord.MutualFund.Name;
                this.dataGridView1[1, count].Value = pnlEntry.MFRecord.PurchaseUnit;
                this.dataGridView1[2, count].Value = pnlEntry.MFRecord.Date;                            
                this.dataGridView1[3, count].Value = pnlEntry.PnL;
                this.dataGridView1[4, count].Value = pnlEntry.Harvest;
                this.dataGridView1[5, count].Value = pnlEntry.MFRecord.DBId;
                
                count += 1;
            }

            
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateDataGrid();
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            var row = e.Row.Index;
            var dbId = int.Parse(this.dataGridView1[5, row].Value.ToString());
            this.databaseHelper.DeleteEntry(dbId);
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //To where your opendialog box get starting location. My initial directory location is desktop.
            openFileDialog1.InitialDirectory = "C://Desktop";
            //Your opendialog box title name.
            openFileDialog1.Title = "Select file to be upload.";
            //which type file format you want to upload in database. just add them.
            openFileDialog1.Filter = "Select Valid Document(*.csv)|*.csv";
            //FilterIndex property represents the index of the filter currently selected in the file dialog box.
            openFileDialog1.FilterIndex = 1;
            try
            {
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog1.CheckFileExists)
                    {
                        string path = System.IO.Path.GetFullPath(openFileDialog1.FileName);
                        label4.Text = "uploaded file " + path;
                        var records = CsvParser.parseZerodhaTradeBookCSV(path,MFMap);
                        foreach(var record in records)
                        {
                            this.databaseHelper.AddEntry(record);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please Upload document.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
