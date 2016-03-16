using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Serialization;
using System.Media;

namespace SMS_activator
{
    public partial class Form1 : Form
    {
        Number number;

        public Form1()
        {
            InitializeComponent();

            number = new Number();

            number.NumberReceived += NumberReceived_Handler;
            number.NumberReceived += PlaySoundWhenNumberGet_Handler;

            number.SmsCodeReceived += SmsCodeReceived_Handler;
            number.SmsCodeReceived += PlaySoundWhenSmsCodeReceived_Handler;

            number.InitalizeComponentsFromMainForm(ref this.label4, ref this.textBox1, ref this.textBox2, ref this.labelGo, ref this.labelMi, ref this.labelYa, ref this.labelAo);
            Logger.InitalizeComponentsFromMainForm(ref this.richTextBox1);

            // Very bad crutch
            Control.CheckForIllegalCrossThreadCalls = false;

            formSettings = new Form2_Settings();

        }

        Form2_Settings formSettings;


        /// <summary>
        /// Method is the handler event of receiving a number / Метод является обработчиком события получения номера
        /// </summary>
        private void NumberReceived_Handler()
        {
            textBox1.Text = number.CellNumber;
        }

        /// <summary>
        /// Method is the handler event of receiving a Sms code / Метод является обработчиком события получения Смс кода
        /// </summary>
        private void SmsCodeReceived_Handler()
        {
            textBox2.Text = number.SmsCode;
        }

        /// <summary>
        /// Method create sound effect when number was received / Метод создает звуковой эффект, когда получен номер
        /// </summary>
        private void PlaySoundWhenNumberGet_Handler()
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "AlarmNumber.wav";

            try{
                player.Load();
                player.Play();
            }catch (Exception ex){
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Method create sound effect when number was received / Метод создает звуковой эффект, когда получен номер
        /// </summary>
        private void PlaySoundWhenSmsCodeReceived_Handler()
        {
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "AlarmSmsCode.wav";

            try
            {
                player.Load();
                player.Play();
            }catch (Exception ex){
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = true;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            radioButton3.Checked = true;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            radioButton4.Checked = true;
        }

        private void программаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void label1_DoubleClick(object sender, EventArgs e)
        {
            if (label1.Text == "Открыть Лог")
            {
                label1.Text = "Закрыть Лог";
                Form1.ActiveForm.Height = 510;
            }
            else
            {
                label1.Text = "Открыть Лог";
                Form1.ActiveForm.Height = 285;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (radioButton1.Checked)
                number.GetGoogleNumber();

            if (radioButton2.Checked)
                number.GetMicrosoftNumber();

            if (radioButton3.Checked)
                number.GetYahooNumber();

            if (radioButton4.Checked)
                number.GetAolNumber();

            progressBar1.Visible = true;
            pictureBox5.Visible = true;

            number.GetBallans();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            number.StopReceivingNumber();

            progressBar1.Visible = false;
            pictureBox5.Visible = false;
        }

        private void pictureBox5_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_TextChanged(object sender, EventArgs e)
        {
            if(Convert.ToInt32(label4.Text) > 9)
                label5.Left =  292;

            if (Convert.ToInt32(label4.Text) > 99)
                label5.Left = 299;

            if (Convert.ToInt32(label4.Text) > 999)
                label5.Left = 306;

            if (Convert.ToInt32(label4.Text) > 9999)
                label5.Left = 313;
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formSettings.ShowDialog();
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            number.FreeQuantity();
            number.GetBallans();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            number.GetBallans();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            number.GetBallans();
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            progressBar1.Visible = false;
            pictureBox5.Visible = false;

            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            number.SendNumberIsUsed();

            button2.Enabled = false;
            button3.Enabled = false;

            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            number.SendCancelActivation();

            button2.Enabled = false;
            button3.Enabled = false;

            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
        
        }
    }
}
