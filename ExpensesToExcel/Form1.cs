using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ExpensesToExcel
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
                return;

            var listOfTransactions = new List<Transaction>();

            foreach (var listBox1Item in listBox1.Items)
            {
                var filePath = listBox1Item.ToString();

                if (!File.Exists(filePath))
                    return;


                var lines = File.ReadAllLines(filePath);

                for (var i = 1 /*skip header*/; i < lines.Length; i++)
                {
                    var lineArray = lines[i].Split(',');

                    var transaction = new Transaction();

                    DateTime.TryParse(lineArray[0], out var date);
                    transaction.Date = date;

                    decimal.TryParse(lineArray[4], NumberStyles.Any,
                        new CultureInfo("en-US"), out var number);

                    transaction.Amount = number * -1; //make spending positive
                    if (transaction.Amount <= 0)
                        continue;

                    listOfTransactions.Add(transaction);
                }
            }

            listOfTransactions.Sort((x, y) => DateTime.Compare(x.Date, y.Date));

            var dayOfTheMonthSumList = listOfTransactions.GroupBy(x => new { x.Date })
                    .Select(a => new { Sum = a.Sum(b => b.Amount), DayOfTheMonth = a.Key })
                    .ToList();
            //dayOfTheMonthSumList.Reverse();

            var outputLineList = new List<string>();
            var previousMonth = 0;
            decimal runningMonthSum = 0;
            foreach (var dayOfTheMonth in dayOfTheMonthSumList)
            {

                if (dayOfTheMonth.DayOfTheMonth.Date.Month != previousMonth)
                {
                    previousMonth = dayOfTheMonth.DayOfTheMonth.Date.Month;
                    runningMonthSum = 0;
                    outputLineList.Add($"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dayOfTheMonth.DayOfTheMonth.Date.Month)},Day of the Month,Running Total,Daily Sum");
                }

                runningMonthSum += dayOfTheMonth.Sum;
                outputLineList.Add($"{dayOfTheMonth.DayOfTheMonth.Date.ToShortDateString()},{dayOfTheMonth.DayOfTheMonth.Date.Day},{runningMonthSum},{dayOfTheMonth.Sum}");

            }


            textBox1.Lines = outputLineList.ToArray();
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.All;
            }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0)
                return;

            foreach (var file in files)
            {
                listBox1.Items.Add(file);
            }
        }
    }

    internal class Transaction
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}
