using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net;
using System.IO;
using System.Threading;

namespace SMS_activator
{

    /// <summary>
    /// Class for getting and manipulating with phone number / Класс для получение и манипулирования с телефонным номером
    /// </summary>
    class Number
    {
        /// <summary>
        /// This property contains the cell number / Данное свойство содержит мобильный номер
        /// </summary>
        public string CellNumber { get; private set; }

        // Flag use in GetNumberNode method for stopping thread when number still not received / Флаг используется в методе GetNumberNode для принудительной остановки потока, когда номер ещё не получен
        private bool STOPE_RECEIVING_NUMBER = false;


        /// <summary>
        /// This property contains reseived sms code / Это свойство содержит СМС код
        /// </summary>
        public string SmsCode { get; private set; }

        /// <summary>
        /// Flag use in GetSmsCodeNode method for stopping thread when Sms still not received / Флаг используется в методе GetSmsCodeNode для принудительной остановки потока, когда смс ещё не получено
        /// </summary>
        private bool STOPE_RECEIVING_SMS = false;

        /// <summary>
        /// In this variable we saving $id, need in moust operation with server / В этой переменной мы храним идентификатор операции, который необходим в большинстве операций с сервером 
        /// </summary>
        private string operationId = null;

        // All threads will be work juast if this flag true
        private bool IS_INTERNET_CONNECTION = true;

        // Flag show is threade get ballans work
        private bool IS_GET_BALLANS_WORK = false;

        // Variable contains value pause between requests in ms / Переменная содержит значение паузы между запросами в мс
        private int PAUSE_REQUEST = 1000;
        private int PAUSE_REQUEST_BALLANSE = 5000;

        /// <summary>
        /// References on view components UI / Ссылки на визуальные компоненты пользовательского интерфейса
        /// </summary>
        private Label labelBallans;
        
        private TextBox textBoxNumber;
        private TextBox textBoxSmsCode;

        private Label labelGo;
        private Label labelMi;
        private Label labelYa;
        private Label labelAo;

        /// <summary>
        /// Parameters for accesing to register / Параметры для доступа в реестр
        /// </summary>
        const string keyName = "HKEY_CURRENT_USER\\SOFTWARE\\SmsRequest";
        const string valueName = "Key";

        /// <summary>
        /// Server responses / Ответы сервера
        /// </summary>
        const string ACCESS_BALANCE = "ACCESS_BALANCE:";
        const string BAD_KEY = "BAD_KEY";
        const string ERROR_SQL = "ERROR_SQL";
        const string NO_NUMBERS = "NO_NUMBERS";
        const string NO_BALANCE = "NO_BALANCE";
        const string BAD_ACTION = "BAD_ACTION";
        const string BAD_SERVICE = "BAD_SERVICE";
        const string ACCESS_READY = "ACCESS_READY";
        const string ACCESS_RETRY_GET = "ACCESS_RETRY_GET";
        const string ACCESS_ACTIVATION = "ACCESS_ACTIVATION";
        const string ACCESS_CANCEL = "ACCESS_CANCEL";
        const string NO_ACTIVATION = "NO_ACTIVATION";
        const string BAD_STATUS = "BAD_STATUS";
        const string STATUS_WAIT_CODE = "STATUS_WAIT_CODE";
        const string STATUS_WAIT_RETRY = "STATUS_WAIT_RETRY:";
        const string STATUS_WAIT_RESEND = "STATUS_WAIT_RESEND";
        const string STATUS_CANCEL = "STATUS_CANCEL";
        const string STATUS_OK = "STATUS_OK:";
        const string ACCESS_NUMBER = "ACCESS_NUMBER:";
        const string WRONG_SERVICE = "WRONG_SERVICE";


        // Evetnts
        public event Action NumberReceived;
        public event Action SmsCodeReceived;

        /// <summary>
        /// Method that initialize compunents, join this class with view components UI / Метод который инициализирует компоненты, соединяя этот класс с компонентами пользовательского интерфейса
        /// </summary>
        public void InitalizeComponentsFromMainForm(ref Label _labelBallans,
                                                           ref TextBox _textBoxNumber,
                                                           ref TextBox _textBoxSmsCode,
                                                           ref Label _labelGo,
                                                           ref Label _labelMi,
                                                           ref Label _labelYa,
                                                           ref Label _labelAo)
        {
            labelBallans = _labelBallans;
            textBoxNumber = _textBoxNumber;
            textBoxSmsCode = _textBoxSmsCode;

            labelGo = _labelGo;
            labelMi = _labelMi;
            labelYa = _labelYa;
            labelAo = _labelAo;
        }

        public void GetBallans()
        {
            if (!IS_GET_BALLANS_WORK)
            {
                Thread thread = new Thread(GetBallansNode);
                thread.Start();
            }
        }
        

        /// <summary>
        /// Method reading server Api Key from register / Метод читает Api ключ сурвера из реестра
        /// </summary>
        /// <returns>Returns Key / Возвращает ключ</returns>
        public string GetKey()
        {
            return Convert.ToString(Registry.GetValue(keyName, valueName, ""));
        }


        /// <summary>
        /// Method reading accout ballans from server / Метод читает балланс аккаунта с сервера
        /// </summary>
        private void GetBallansNode()
        {
            do {
                string key = GetKey();

                string responseTxt = string.Empty;

                try {
                    // Send request to the server / Отправляем запрос на сервер
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=getBalance");
                    myReq.Credentials = CredentialCache.DefaultCredentials;

                    IS_INTERNET_CONNECTION = true;
                    IS_GET_BALLANS_WORK = true;

                    Logger.Add("Получение баланса");

                    // Get response / Получаем ответ
                    HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

                    // Create stream and read / Создаём поток и читаем
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    responseTxt = reader.ReadToEnd();

                    response.Close();
                    reader.Close();
                }
                catch(WebException ex)
                {
                    IS_INTERNET_CONNECTION = false;
                    IS_GET_BALLANS_WORK = false;

                    Logger.Add(ex.Message);
                    MessageBox.Show(ex.Message);
                }

                // If response equal server errors - show messageBox / Если ответ соответствует ошибкам - показуем messageBox
                if (responseTxt.Contains(ACCESS_BALANCE))
                {
                    labelBallans.Text = responseTxt.Remove(0, ACCESS_BALANCE.Length);

                    FreeQuantity();
                }
                else
                {
                    Logger.Add("Ошибка сервера: " + responseTxt);
                }

                Thread.Sleep(PAUSE_REQUEST_BALLANSE);
            } while (IS_INTERNET_CONNECTION);
        }

        /// <summary>
        /// Request sum free numbers / Запрашываем колличество свободных номеров
        /// </summary>
        public void FreeQuantity()
        {
            string key = GetKey();

            string responseTxt = string.Empty;

            try {
                //Send request to site / Отправляем запрос на сервер
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=getNumbersStatus");
                myReq.Credentials = CredentialCache.DefaultCredentials;

                IS_INTERNET_CONNECTION = true;

                // Get response / Получаем ответ
                HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

                // Create stream and read / Создаём поток и читаем
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                responseTxt = reader.ReadToEnd();

                response.Close();
                reader.Close();
            } catch (WebException ex)
            {
                IS_INTERNET_CONNECTION = false;

                Logger.Add(ex.Message);
                MessageBox.Show(ex.Message);
            }


            // Parse response from json format / Прасим ответ с json формата
            // Somesing wrong with my visual studio 2015, she does not wont open name space
            // "System.Runtime.Serialization.Json", so I can't use DataContractJsonSerializer
            // I will be create my parser static class;
            // Format response: {"vk_0":"0","ok_0":"0","wa_0":"262","vi_0":"0","tg_0":"0","wb_0":"311","go_0":"0","av_0":"0","av_1":"0","fb_0":"0","tw_0":"0","ot_1":"0","ub_0":"309","qw_0":"289","gt_0":"311","sn_0":"313","ig_0":"228","ss_0":"309","ym_0":"308","ya_0":"301","ma_0":"313","mm_0":"124","uk_0":"310","me_0":"310","mb_0":"310","we_0":"313","ot_0":"239"}
            // http://sms-activate.ru/index.php?act=api

            // Parse Google
            labelGo.Text = Parcer.SubString(responseTxt, "\"go_0\":\"", "\",");

            // Parse Microsoft
            labelMi.Text = Parcer.SubString(responseTxt, "\"mm_0\":\"", "\",");

            // Parse Yahoo
            labelYa.Text = Parcer.SubString(responseTxt, "\"mb_0\":\"", "\",");

            // Parse Aol
            labelAo.Text = Parcer.SubString(responseTxt, "\"we_0\":\"", "\",");
        }

        /// <summary>
        /// Method starts in new thread method of receipt number for Google service / Метод запускает в новом потоке метод получения номера для Google сервиса
        /// </summary>
        public void GetGoogleNumber()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(GetNumberNode));
            thread.Start("go");
        }

        /// <summary>
        /// Method starts in new thread method of receipt number for Microsoft service / Метод запускает в новом потоке метод получения номера для Microsoft сервиса
        /// </summary>
        public void GetMicrosoftNumber()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(GetNumberNode));
            thread.Start("mm");
        }

        /// <summary>
        /// Method starts in new thread method of receipt number for Yahoo service / Метод запускает в новом потоке метод получения номера для Yahoo сервиса
        /// </summary>
        public void GetYahooNumber()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(GetNumberNode));
            thread.Start("mb");
        }

        /// <summary>
        /// Method sends a request to the server for receipt phone number / Метод отправляет запрос на сервер для получения номера
        /// </summary>
        /// <param name="_codeService">Code service / Код сервиса (go(Google,youtube,Gmail), mm(Microsoft), mb(Yahoo), we(Aol))</param>
        private void GetNumberNode(object _codeService)
        {
            STOPE_RECEIVING_NUMBER = false;
            bool flagNumberGet = false;
            bool flagGetError = false;

            string key = GetKey();

            string responseTxt = string.Empty;

            do
            {
                try {
                    //Sends request to the server / Отправляем запрос на сервер
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=getNumber&service=" + _codeService);
                    myReq.Credentials = CredentialCache.DefaultCredentials;

                    // Get response / Получаем ответ
                    HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

                    // Create stream and read / Создаём поток и читаем
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    responseTxt = reader.ReadToEnd();

                    response.Close();
                    reader.Close();
                } catch (WebException ex)
                {
                    IS_INTERNET_CONNECTION = false;

                    Logger.Add(ex.Message);
                    MessageBox.Show(ex.Message);
                }


                Logger.Add("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=getNumber&service=" + (string)_codeService);
                // Add server response in log / Добавляем ответ сервера в лог
                Logger.Add("Получение номера. Ответ сервера: " + responseTxt);

                // If response equal server errors - show messageBox / Если ответ соответствует ошибкам - показуем messageBox
                if (!responseTxt.Contains(ACCESS_NUMBER)
                    && !responseTxt.Contains(NO_NUMBERS))
                {
                    MessageBox.Show("Ошибка сервера: " + responseTxt);
                    flagGetError = true;
                } else {
                    // If request contain "ACCESS_NUMBER" start parse number and id (format: ACCESS_NUMBER:$id:$number) / Если запрос содержит последовательность "ACCESS_NUMBER" - начинаем парсить номер и идентификатор сессии
                    if (responseTxt.Contains(ACCESS_NUMBER))
                    {
                        // Id is between two : / Идентификатор находится между двумя :
                        operationId = Parcer.SubString(responseTxt, ":", ":");
                        Logger.Add("Идентификатор сессии: " + operationId);

                        // For parsing with Parcer class, remove first : and add ":end" in the end / Для парсинга с помошью Parce класса, удаляем первое : и добавляем в конец ":end"
                        responseTxt = responseTxt.Remove(responseTxt.IndexOf(":"), 1);
                        responseTxt = responseTxt + ":end";

                        CellNumber = Parcer.SubString(responseTxt, ":", ":end");

                        // Event
                        NumberReceived();

                        // Start thread receiving SMS code / Запускаем поток получения СМС кода
                        GetSmsCode();

                        flagNumberGet = true;
                    }
                }

                Thread.Sleep(PAUSE_REQUEST);

            } while (flagNumberGet == false && flagGetError == false && STOPE_RECEIVING_NUMBER == false);
        }

        /// <summary>
        /// Method stope receving number / Метод останавливает получение номера
        /// </summary>
        public void StopReceivingNumber()
        {
            STOPE_RECEIVING_NUMBER = true;
        }

        private void GetSmsCode()
        {
            Thread thread = new Thread(GetSmsCodeNode);
            thread.Start();
        }

        /// <summary>
        /// Send request to the server for forgive sms code / Отправляет запрос на сервер для получения кода из смс
        /// </summary>
        private void GetSmsCodeNode()
        {
            STOPE_RECEIVING_SMS = false;
            bool flagSmsGet = false;
            bool flagGetError = false;

            string key = GetKey();

            string responseTxt = string.Empty;

            do
            {
                try
                {
                    //Send request to the server  / Отправляем запрос на сервер
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=getStatus&id=" + operationId);
                    myReq.Credentials = CredentialCache.DefaultCredentials;

                    // Get response / Получаем ответ
                    HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

                    // Create stream and read / Создаём поток и читаем
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    responseTxt = reader.ReadToEnd();

                    response.Close();
                    reader.Close();
                }
                catch (WebException ex)
                {
                    IS_INTERNET_CONNECTION = false;

                    Logger.Add(ex.Message);
                    MessageBox.Show(ex.Message);
                }


                Logger.Add("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=getStatus&id=" + operationId);
                // Add server response in log / Добавляем ответ сервера в лог
                Logger.Add("Получение SMS. Ответ сервера: " + responseTxt);

                // If response equal good ansvers of server - parce message / Если ответ соответствует правильным ответам сервера - парсим сообщение
                if (responseTxt.Contains(STATUS_OK)
                    || responseTxt.Contains(STATUS_WAIT_CODE)
                    || responseTxt.Contains(STATUS_WAIT_RETRY)
                    || responseTxt.Contains(STATUS_WAIT_RESEND)
                    || responseTxt.Contains(STATUS_CANCEL))
                {
                    // If message was STATUS_CANCEL then stope thread / Если сообщение было STATUS_CANCEL, тогда останавливаем поток
                    if (responseTxt.Contains(STATUS_CANCEL))
                    {
                        STOPE_RECEIVING_SMS = true;
                    }

                    // If request contain "STATUS_OK" start parse code (format: STATUS_OK:$code) / Если запрос содержит последовательность "STATUS_OK" - начинаем парсить код
                    if (responseTxt.Contains(STATUS_OK))
                    {
                        // For parsing with Parcer class, add ":end" in the end / Для парсинга с помошью Parce класса, добавляем в конец ":end"
                        responseTxt = responseTxt + ":end";

                        SmsCode = Parcer.SubString(responseTxt, ":", ":end");

                        // Event
                        SmsCodeReceived();

                        flagSmsGet = true;
                    }
                } else {

                    MessageBox.Show("Ошибка сервера: " + responseTxt);
                    flagGetError = true;
                }

                Thread.Sleep(PAUSE_REQUEST);

            } while (flagSmsGet == false && flagGetError == false && STOPE_RECEIVING_SMS == false);
        }

        /// <summary>
        /// Method starts in new thread method of receipt number for Aol service / Метод запускает в новом потоке метод получения номера для Aol сервиса
        /// </summary>
        public void GetAolNumber()
        {
            Thread thread = new Thread(new ParameterizedThreadStart(GetNumberNode));
            thread.Start("we");
        }

        public void SendNumberIsUsed()
        {
            if (operationId != null)
            {
                string key = GetKey();

                string responseTxt = string.Empty;

                try {
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=setStatus&status=8&id=" + operationId);
                    myReq.Credentials = CredentialCache.DefaultCredentials;

                    // Get response / Получаем ответ
                    HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

                    // Create stream and read / Создаём поток и читаем
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    responseTxt = reader.ReadToEnd();

                    response.Close();
                    reader.Close();
                } catch (WebException ex)
                {
                    IS_INTERNET_CONNECTION = false;

                    Logger.Add(ex.Message);
                    MessageBox.Show(ex.Message);
                }

                // Add server response in log / Добавляем ответ сервера в лог
                Logger.Add("Отправка запроса, номер использован. Ответ сервера: " + responseTxt);
            }
        }

        public void SendCancelActivation()
        {
            if (operationId != null)
            {
                string key = GetKey();

                string responseTxt = string.Empty;

                try
                {
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=setStatus&status=1&id=" + operationId);
                    myReq.Credentials = CredentialCache.DefaultCredentials;

                    // Get response / Получаем ответ
                    HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();

                    // Create stream and read / Создаём поток и читаем
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    responseTxt = reader.ReadToEnd();

                    // Add server response in log / Добавляем ответ сервера в лог
                    Logger.Add("Отправка запроса 1, отменить активацию. Ответ сервера: " + responseTxt);

                    if (responseTxt.Contains(ACCESS_READY))
                    {
                        HttpWebRequest myReq2 = (HttpWebRequest)WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=" + key + "&action=setStatus&status=-1&id=" + operationId);
                        myReq.Credentials = CredentialCache.DefaultCredentials;

                        response = (HttpWebResponse)myReq2.GetResponse();
                        stream = response.GetResponseStream();
                        reader = new StreamReader(stream);
                        responseTxt = reader.ReadToEnd();

                        // Add server response in log / Добавляем ответ сервера в лог
                        Logger.Add("Отправка запроса -1, отменить активацию. Ответ сервера: " + responseTxt);
                    }




                    response.Close();
                    reader.Close();
                }
                catch (WebException ex)
                {
                    IS_INTERNET_CONNECTION = false;

                    Logger.Add(ex.Message);
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}