using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GlobusBank
{
    using static Math;
    struct Rec
    {
        public byte mon_no;
        public decimal remaider_sum;
        public double next_pay_p;
        public double next_pay_all;
        public DateTime date;
    }
    public partial class Form1 : Form
    {      
        const decimal kredit_sum = 563327.5M; 
        PayerKredit PK = new PayerKredit();
        List<Rec> grafic = new List<Rec>();

        double CurProcent
        {
            get
            {
                if (DateTime.Now > DateTime.Parse("15.06.2018"))
                    return 21.9;

                return 13.5;
            }
        }
        decimal GetTotalPaying()
        {
            var totl_sum = from s in PK.Payings
                           select s;

            decimal res = 0;
            foreach (var r in totl_sum) res += r.SumPay;

            return res; 
        }
        decimal GetTotalPayingProcent()
        {
            var totl_sum = from s in PK.Payings
                           where s.TypePay == false
                           select s;

            decimal res = 0;
            foreach (var r in totl_sum)
                res += r.SumPay;

            return res;
        }
        (decimal, decimal) GetTotalPayingBody()
        {
            var totl_sum = from s in PK.Payings
                           where s.TypePay == true
                           select s;

            decimal res = 0;
            foreach (var r in totl_sum)
                res += r.SumPay;
            decimal proc = res / (kredit_sum / 100); 

            return (res, Round(proc, 1));
        }
        decimal GetRemainder()
        {
            return kredit_sum - GetTotalPayingBody().Item1; 
        }
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // перевірки 

            if (!(radioButton1.Checked || radioButton2.Checked))
            {
                MessageBox.Show("Не вибрано призначення платежу!");
                return; 
            }
            decimal sum;
            try
            {
               sum = Decimal.Parse(textBox1.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Введена сума не вірно внесена!");
                return; 
            }

            Paying CurPaying = new Paying();
            CurPaying.DatePay = dateTimePicker1.Value;
            CurPaying.SumPay = sum;
            CurPaying.TypePay = radioButton1.Checked;
            PK.Payings.Add(CurPaying);
            PK.SaveChanges(); 

            MessageBox.Show($"Оплату на суму {sum} було внесено в БД!");
            DisplayInfo(); 

            textBox1.Clear();
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            dateTimePicker1.Value = DateTime.Now; 
        }
        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Загальна сума сплат становить {GetTotalPaying()}");  
        }
        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Загальна сума по тілу кредиту становить {GetTotalPayingBody()}");
        }
        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Загальна сума по відсоткам за кредит становить {GetTotalPayingBody()}");
        }
        void DisplayInfo()
        {
            label4.Text = $"Загальна сума сплат становить {GetTotalPaying():C}";
            label5.Text = $"Загальна сплата по тілу кредиту становить {GetTotalPayingBody().Item1:C} ({GetTotalPayingBody().Item2}%)";
            label6.Text = $"Загальна сплата по відсоткам за кредит становить {GetTotalPayingProcent():C}";
            label7.Text = $"Залишок боргу становить {GetRemainder():C}";
            label8.Text = $"Наступний платіж по відоткам становитиме {GetNextPayProcent():C}";
            label9.Text = $"Щомісячний платіж по тілу кредиту становить {kredit_sum / 60:C}"; 
            label12.Text = $"Разом до сплати наступного місяця {GetNextPayProcent()+(double)(kredit_sum / 60):C}";
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            DisplayInfo();
            dateTimePicker2.Value = new DateTime(2017, 6, 15);
            for (int i = 1; i <= 240; i++) comboBox1.Items.Add(i);
            FillGrafic();
        }

        double GetNextPayProcent()
        {
            return GetTruePayPerMonth(DateTime.Now, GetRemainder(), CurProcent); 
        }

        void LoadAllRecordsInDates()
        {
            var query = from r in PK.Payings
                        where r.DatePay >= dateTimePicker2.Value && r.DatePay <= dateTimePicker3.Value 
                        orderby r.TypePay, r.DatePay
                        select r;
            IQueryable RQuery; 
            if (comboBox2.SelectedIndex == 1)
                RQuery = query.Where(r => r.TypePay == false);
            else 
                if (comboBox2.SelectedIndex == 2)
                RQuery = query.Where(r => r.TypePay == true);
            else RQuery = query;

            listBox1.Items.Clear();
            listBox1.Items.Add(String.Empty);
            foreach (Paying p in RQuery)
            {
                string s = (p.TypePay == true) ? "тіло кредиту" : "за відсотки";
                listBox1.Items.Add($"Дата платежу {p.DatePay.ToShortDateString()}");
                listBox1.Items.Add($"Сума платежу {Round(p.SumPay, 2)} призначення платежу {s}");
                listBox1.Items.Add(new String('=', 25));
                listBox1.Items.Add(String.Empty);
            }
        }
        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage2) LoadAllRecordsInDates();
        }
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            LoadAllRecordsInDates();
        }
        private void FillGrafic(int mcount = 60)
        {
            grafic.Clear(); 
            DateTime mydate = new DateTime(2017, 06, 10);
            decimal remainders = kredit_sum; 
            for (byte i = 1; i <= mcount; i++)
            {
                Rec newRec;
                newRec.mon_no = i;
                newRec.remaider_sum = remainders;
                newRec.next_pay_p = GetTruePayPerMonth(mydate.AddMonths(i), remainders, (i > 12) ? 21.9 : 13.5);
                newRec.next_pay_all = newRec.next_pay_p + (double)kredit_sum / mcount;
                newRec.date = mydate.AddMonths(i); 
                remainders -= kredit_sum / mcount;
                grafic.Add(newRec); 
            }
            FillGrid();
        }
        void FillGrid()
        {
            if (grafic is null)
                return;
            int i = 0;
            dataGridView1.Rows.Clear(); 
            dataGridView1.Rows.Add(grafic.Count -1); 
            foreach (Rec r in grafic)
            {
                dataGridView1.Rows[i].Cells[0].Value = r.mon_no;
                dataGridView1.Rows[i].Cells[1].Value = r.date.ToShortDateString(); 
                dataGridView1.Rows[i].Cells[2].Value = $"{ r.remaider_sum:C}";
                dataGridView1.Rows[i].Cells[3].Value = $"{r.next_pay_p:C}";
                dataGridView1.Rows[i].Cells[4].Value = $"{kredit_sum / grafic.Count:C}"; 
                dataGridView1.Rows[i].Cells[5].Value = $"{r.next_pay_all:C}";

                if (r.date < DateTime.Now)
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.DeepSkyBlue; 
            
                i++;
            }
        }
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            FillGrafic(int.Parse(comboBox1.Text));
        }
        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) FillGrafic(int.Parse(comboBox1.Text));
        }

        double GetTruePayPerMonth(DateTime onDate, decimal sum, double proc)
        {
            DateTime payDate = new DateTime(onDate.Month == 1 ? onDate.Year - 1 : onDate.Year, onDate.Month == 1 ? 12 : onDate.Month - 1, 25);
            DateTime startDay = new DateTime(payDate.Month == 1 ? payDate.Year - 1 : payDate.Year, payDate.Month == 1 ? 12 : payDate.Month - 1, 25);
            int days = (payDate - startDay).Days;
            decimal res = sum / 100 * (decimal)proc;  

            return (double)res/365 * days; 
        }
    }
}