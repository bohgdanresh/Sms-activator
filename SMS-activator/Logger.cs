using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMS_activator
{
    /// <summary>
    /// This class logging programm work, data writing in richTextBox on UI / Данный класс ведёт логирование работы программы, данные добавляются в компонент richTextBox пользовательского интерфейса
    /// </summary>

    class Logger
    {
        private static RichTextBox richTextBox;

        public static void InitalizeComponentsFromMainForm(ref RichTextBox _richTextBox)
        {
            richTextBox = _richTextBox;
        }

        public static void Add(string _log)
        {
            DateTime dateTime = DateTime.Now;
            string time = dateTime.ToString().Remove(0, dateTime.ToString().IndexOf(" ") + 1);

            richTextBox.AppendText(time + " " + _log + "\n");
        }
    }
}
