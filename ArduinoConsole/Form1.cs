using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Timers;

namespace ArduinoConsole
{
    public partial class Form1 : Form
    {
        private delegate void updateDelegate(string txt);
        public string currentPort;//выбранный порт в listbox
        SerialPort openedPort;//открываемый порт
        System.Timers.Timer aTimer;
        int biteRate = 9600;
        string[] ports = SerialPort.GetPortNames();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                String dataOnwrite = textBox1.Text as String;
                openedPort.Write(dataOnwrite);
                listBox2.Items.Add("                                              " + dataOnwrite);
                textBox1.Clear();
                textBox1.Focus();
            }
            catch { listBox1.Items.Add("Подключение не установленно"); }
        }//отправка данных

        private void OnTimedEvent(object sender, ElapsedEventArgs e)//чтение вошедших данных
        {
            try
            {
                if (openedPort.IsOpen)
                {
                    openedPort.DiscardInBuffer();
                    String dataFromPort = openedPort.ReadLine();
                    listBox2.BeginInvoke(new updateDelegate(updateTextBox), dataFromPort);
                }
                else
                {
                    timer1.Enabled = false;
                }
            }
            catch { }
        }

        private void updateTextBox(string txt)
        {
            listBox2.Items.Add(txt);
        }//обновление чата

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }//меню ПКМ лога

        private void clearChatToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
        }//меню по ПКМ чата

        private void searchPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            if (ports.Length == 0)
            {
                toolStripStatusLabel1.Text = ("Активных COM портов не обнаружено");
                connectButtonBtn.Enabled = false;
                closeConnectionBtn.Enabled = false;
                button1.Enabled = false;
                listBox3.Items.Clear();
            }
            else if (ports.Length == 1)
            {
                toolStripStatusLabel1.Text = ("Найден COM порт, найденный порт выбран по умолчанию");
                connectButtonBtn.Enabled = true;
                button1.Enabled = true;
                closeConnectionBtn.Enabled = false;
                currentPort = ports[0];
                listBox3.Items.Clear();
                listBox3.Items.AddRange(ports);
            }
            else if (ports.Length > 1)
            {
                toolStripStatusLabel1.Text = ("Найдены COM порты");
                listBox3.Items.Clear();
                listBox3.Items.AddRange(ports);
            }
        }//поиск портов

        private void connectButtonBtn_Click(object sender, EventArgs e)
        {
            foreach (string port in ports)
            {
                openedPort = new SerialPort(port, biteRate);
                if (openedPort.IsOpen)
                {
                    MessageBox.Show("Opened port is allready opened", "WTF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            openPortConnection();
        }//подключение к порту, деуствие по кнопке

        private void closeConnectionBtn_Click(object sender, EventArgs e)
        {
            closePortConnection();
        }//закрытие подключения, действие по кнопке

        private void exitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }//кнопка выхода

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("Программа написана djwirtuoz, в 2017 году", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }//о программе

        private void Form1_Load(object sender, EventArgs e)
        {
            connectButtonBtn.Enabled = false;
            closeConnectionBtn.Enabled = false;
            button1.Enabled = false;

            selectSpeedBox.Text = (biteRate.ToString());

            searchPorts();//поиск портов при запуске
        }//установка начальных параметров

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPort = listBox3.SelectedItem.ToString();
            currentPort = selectedPort;
            //MessageBox.Show("" + selectedPort,"WTF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            connectButtonBtn.Enabled = true;
        }//заносит в переменную порта к которому нужно подключиться имя выделенного порта

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            const int WM_DeviceChange = 0x219; //что-то связанное с usb
            const int DBT_DEVICEARRIVAL = 0x8000; //устройство подключено
            const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // устройство отключено

            if (m.Msg == WM_DeviceChange)
            {
                if (m.WParam.ToInt32() == DBT_DEVICEARRIVAL)
                {
                    //новое usb подключено
                    searchPorts();
                }
                if (m.WParam.ToInt32() == DBT_DEVICEREMOVECOMPLETE)
                {
                    // usb отключено
                    searchPorts();
                }

            }
        }//ловит системное событие на подключение устройства и запускает поиск портов

        private void selectSpeedBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
              int biteRateSelected = Convert.ToInt32(selectSpeedBox.SelectedItem);
                biteRate = biteRateSelected;
            }
            catch
            {
                MessageBox.Show("Какая то фигня в коде, по идее все должно работать", "WTF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }//выбор скорости подключения

        private void closePortConnection()
        {
            try
            {
                openedPort.Close();
                listBox1.Items.Add("Подключение закрыто");
                connectButtonBtn.Enabled = true;
            }
            catch
            {
                listBox1.Items.Add("Порт уже отключен");
            }
        }//закрытие подключения

        private void openPortConnection()
        {
            openedPort = new SerialPort(currentPort, biteRate);
            System.Threading.Thread.Sleep(500); // немного подождем

            openedPort.BaudRate = biteRate;
            openedPort.DtrEnable = true;
            openedPort.ReadTimeout = 1000;
            try
            {
                openedPort.Open();
                System.Threading.Thread.Sleep(500);
                listBox1.Items.Add("Подключенно к порту " + currentPort + ", на скорости " + biteRate.ToString());
                aTimer = new System.Timers.Timer(500);
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;
                closeConnectionBtn.Enabled = true;
                button1.Enabled = true;
            }
            catch
            {
                listBox1.Items.Add("Не подключенно к порту" + currentPort);
            }
            connectButtonBtn.Enabled = false;
        }//подключение к порту
    }
}