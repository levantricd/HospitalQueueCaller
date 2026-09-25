using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace HospitalQueueCaller
{
    public partial class MainWindow : Window
    {
        private int startNumber;
        private int endNumber;
        private int step;
        private int currentNumber;

        private DisplayWindow displayWindow;

        public MainWindow()
        {
            InitializeComponent();

            displayWindow = new DisplayWindow();

            var screens = System.Windows.Forms.Screen.AllScreens;

            displayWindow.Left = screens[0].WorkingArea.Left;
            displayWindow.Top = screens[0].WorkingArea.Top;
            displayWindow.Show();

            LoadState();

            btnStart.Click += BtnStart_Click;
            btnNext.Click += BtnNext_Click;
            btnReset.Click += BtnReset_Click;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtStartNumber.Text, out startNumber) ||
                !int.TryParse(txtEndNumber.Text, out endNumber) ||
                !int.TryParse(txtStep.Text, out step) ||
                step <= 0 ||
                startNumber > endNumber)
            {
                System.Windows.MessageBox.Show(
                    "Vui lòng nhập thông tin hợp lệ.");

                return;
            }

            currentNumber = startNumber;

            SaveState();
            UpdateDisplay();

            txtStatus.Text =
                $"Trạng thái: Đang chạy từ {startNumber:D3} " +
                $"đến {endNumber:D3}, bước {step}";
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (currentNumber == 0)
            {
                System.Windows.MessageBox.Show(
                    "Vui lòng bấm BẮT ĐẦU DÃY trước.");

                return;
            }

            int nextNumber;

            if (currentNumber == startNumber)
            {
                nextNumber = currentNumber;
            }
            else
            {
                nextNumber = currentNumber + step;
            }

            if (nextNumber > endNumber)
            {
                System.Windows.MessageBox.Show(
                    "Đã hết số trong dãy.");

                return;
            }

            currentNumber = nextNumber;

            UpdateDisplay();
            SaveState();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            currentNumber = 0;

            SaveState();

            txtCurrentNumber.Text = "000";
            displayWindow.ShowNumber(0);

            txtStatus.Text = "Trạng thái: Đã reset";
        }

        private void UpdateDisplay()
        {
            txtCurrentNumber.Text =
                currentNumber.ToString("D3");

            displayWindow.ShowNumber(currentNumber);
        }

        private string SaveFile =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "HospitalQueueCaller",
                "queue.json");

        private void SaveState()
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(SaveFile)!);

            var state = new QueueState
            {
                startNumber = startNumber,
                endNumber = endNumber,
                step = step,
                currentNumber = currentNumber
            };

            File.WriteAllText(
                SaveFile,
                JsonSerializer.Serialize(state));
        }

        private void LoadState()
        {
            if (!File.Exists(SaveFile))
                return;

            try
            {
                var json = File.ReadAllText(SaveFile);

                var state =
                    JsonSerializer.Deserialize<QueueState>(json);

                if (state == null)
                    return;

                startNumber = state.startNumber;
                endNumber = state.endNumber;
                step = state.step;
                currentNumber = state.currentNumber;

                txtStartNumber.Text =
                    startNumber.ToString("D3");

                txtEndNumber.Text =
                    endNumber.ToString("D3");

                txtStep.Text =
                    step.ToString();

                txtCurrentNumber.Text =
                    currentNumber.ToString("D3");

                displayWindow.ShowNumber(currentNumber);
            }
            catch
            {
                currentNumber = 0;
            }
        }

        private class QueueState
        {
            public int startNumber { get; set; }
            public int endNumber { get; set; }
            public int step { get; set; }
            public int currentNumber { get; set; }
        }
    }
}