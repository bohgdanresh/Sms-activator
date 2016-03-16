using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SMS_activator
{
    public partial class Form2_Settings : Form
    {
        /// <summary>
        /// Parameters for accesing to register / Параметры для доступа в реестр
        /// </summary>
        const string keyName = "HKEY_CURRENT_USER\\SOFTWARE\\SmsRequest";
        const string valueName = "Key";

        public Form2_Settings()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Registry.SetValue(keyName, valueName, textBox1.Text);
        }

        private void Form2_Settings_Load(object sender, EventArgs e)
        {
            textBox1.Text = Convert.ToString(Registry.GetValue(keyName, valueName, ""));
        }
    }
}
