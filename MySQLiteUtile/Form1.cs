using MyDescription.MyDescriptionFlod;
using MySQLiteUtile.MySQLiteTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MySQLiteUtile
{
    public partial class Form1 : Form
    {
        private BindingList<MyTestFactory> _preMyTestFactoryItems = new BindingList<MyTestFactory>();
        public Form1()
        {
            InitializeComponent();
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.DataSource = _preMyTestFactoryItems;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MyAttributeManager myAttributeManager = new MyAttributeManager();
            List<string> listDesc = myAttributeManager.GetMyClassObject();
            foreach (string s in listDesc)
            {
                listBox1.Items.Add(s);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach(MyTestFactory mtf in _preMyTestFactoryItems)
            {
                MyTestManager.GetInstance().DbInsertOrUpdate(mtf);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _preMyTestFactoryItems.Clear();
            var source = MyTestManager.GetInstance().QuerySource();
            foreach(MyTestFactory mtf in source)
            {
                _preMyTestFactoryItems.Add(mtf);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MyTestManager.GetInstance().Initialize();
        }
    }
}
