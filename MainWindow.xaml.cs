using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace HospitalQueueCaller
{
    public partial class MainWindow : Window
    {
        private readonly DisplayWindow displayWindow;
        private readonly DisplayWindow displayWindow2;

        private QueueState state = new QueueState();

        private string SaveFile =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "HospitalQueueCaller",
                "queue.json");


        public MainWindow()
        {
            InitializeComponent();

            displayWindow = new DisplayWindow();
            displayWindow2 = new DisplayWindow();

            displayWindow.WindowStartupLocation =
                WindowStartupLocation.CenterScreen;

            displayWindow2.WindowStartupLocation =
                WindowStartupLocation.CenterScreen;

            displayWindow.Show();
            displayWindow2.Show();

            LoadState();

            btnStartNormal.Click += BtnStartNormal_Click;
            btnNextNormal.Click += BtnNextNormal_Click;

            btnStartPriority.Click += BtnStartPriority_Click;
            btnNextPriority.Click += BtnNextPriority_Click;

            Closing += MainWindow_Closing;

            UpdateUI();

            // Nếu đã có dãy đang gọi thì khôi phục lên màn hình
            if (!string.IsNullOrWhiteSpace(state.ActiveMode))
            {
                UpdateDisplays();
            }
        }


        // =========================================================
        // SỐ THƯỜNG - BẮT ĐẦU
        // =========================================================

        private void BtnStartNormal_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!ReadNormalConfig())
                return;

            // BẮT ĐẦU = ĐẶT LẠI DÃY
            state.NormalLastStart =
                state.NormalStart;

            state.NormalLastEnd =
                state.NormalStart +
                state.NormalStep -
                1;

            state.ActiveMode =
                "SỐ THƯỜNG";

            SaveState();
            UpdateUI();
            UpdateDisplays();

            txtStatus.Text =
                $"Số thường: " +
                $"{state.NormalLastStart:D3} - " +
                $"{state.NormalLastEnd:D3}";
        }


        // =========================================================
        // SỐ THƯỜNG - GỌI TIẾP
        // =========================================================

        private void BtnNextNormal_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!ReadNormalConfig())
                return;

            // Chưa có dãy → tự động bắt đầu
            if (state.NormalLastStart <= 0)
            {
                BtnStartNormal_Click(sender, e);
                return;
            }

            int nextStart =
                state.NormalLastEnd + 1;

            state.NormalLastStart =
                nextStart;

            state.NormalLastEnd =
                nextStart +
                state.NormalStep -
                1;

            state.ActiveMode =
                "SỐ THƯỜNG";

            SaveState();
            UpdateUI();
            UpdateDisplays();

            txtStatus.Text =
                $"Số thường: " +
                $"{state.NormalLastStart:D3} - " +
                $"{state.NormalLastEnd:D3}";
        }


        // =========================================================
        // SỐ ƯU TIÊN - BẮT ĐẦU
        // =========================================================

        private void BtnStartPriority_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!ReadPriorityConfig())
                return;

            // BẮT ĐẦU = ĐẶT LẠI DÃY
            state.PriorityLastStart =
                state.PriorityStart;

            state.PriorityLastEnd =
                state.PriorityStart +
                state.PriorityStep -
                1;

            state.ActiveMode =
                "SỐ ƯU TIÊN";

            SaveState();
            UpdateUI();
            UpdateDisplays();

            txtStatus.Text =
                $"Số ưu tiên: " +
                $"{state.PriorityLastStart:D3} - " +
                $"{state.PriorityLastEnd:D3}";
        }


        // =========================================================
        // SỐ ƯU TIÊN - GỌI TIẾP
        // =========================================================

        private void BtnNextPriority_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!ReadPriorityConfig())
                return;

            // Chưa có dãy → tự động bắt đầu
            if (state.PriorityLastStart <= 0)
            {
                BtnStartPriority_Click(sender, e);
                return;
            }

            int nextStart =
                state.PriorityLastEnd + 1;

            state.PriorityLastStart =
                nextStart;

            state.PriorityLastEnd =
                nextStart +
                state.PriorityStep -
                1;

            state.ActiveMode =
                "SỐ ƯU TIÊN";

            SaveState();
            UpdateUI();
            UpdateDisplays();

            txtStatus.Text =
                $"Số ưu tiên: " +
                $"{state.PriorityLastStart:D3} - " +
                $"{state.PriorityLastEnd:D3}";
        }


        // =========================================================
        // ĐỌC CẤU HÌNH SỐ THƯỜNG
        // =========================================================

        private bool ReadNormalConfig()
        {
            if (!int.TryParse(
                    txtNormalStart.Text,
                    out int start) ||
                !int.TryParse(
                    txtNormalStep.Text,
                    out int step) ||
                start <= 0 ||
                step <= 0)
            {
                MessageBox.Show(
                    "Vui lòng nhập số bắt đầu và bước tăng hợp lệ.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            state.NormalStart = start;
            state.NormalStep = step;

            return true;
        }


        // =========================================================
        // ĐỌC CẤU HÌNH SỐ ƯU TIÊN
        // =========================================================

        private bool ReadPriorityConfig()
        {
            if (!int.TryParse(
                    txtPriorityStart.Text,
                    out int start) ||
                !int.TryParse(
                    txtPriorityStep.Text,
                    out int step) ||
                start <= 0 ||
                step <= 0)
            {
                MessageBox.Show(
                    "Vui lòng nhập số bắt đầu và bước tăng hợp lệ.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            state.PriorityStart = start;
            state.PriorityStep = step;

            return true;
        }


        // =========================================================
        // CẬP NHẬT GIAO DIỆN QUẢN LÝ
        // =========================================================

        private void UpdateUI()
        {
            txtNormalStart.Text =
                state.NormalStart.ToString();

            txtNormalStep.Text =
                state.NormalStep.ToString();

            txtPriorityStart.Text =
                state.PriorityStart.ToString();

            txtPriorityStep.Text =
                state.PriorityStep.ToString();


            // SỐ THƯỜNG
            if (state.NormalLastStart > 0)
            {
                txtNormalRange.Text =
                    $"{state.NormalLastStart:D3} - " +
                    $"{state.NormalLastEnd:D3}";
            }
            else
            {
                txtNormalRange.Text = "---";
            }


            // SỐ ƯU TIÊN
            if (state.PriorityLastStart > 0)
            {
                txtPriorityRange.Text =
                    $"{state.PriorityLastStart:D3} - " +
                    $"{state.PriorityLastEnd:D3}";
            }
            else
            {
                txtPriorityRange.Text = "---";
            }


            // CHẾ ĐỘ ĐANG GỌI
            if (state.ActiveMode ==
                "SỐ ƯU TIÊN")
            {
                txtActiveMode.Text =
                    "SỐ ƯU TIÊN";

                txtActiveMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkRed);
            }
            else if (state.ActiveMode ==
                     "SỐ THƯỜNG")
            {
                txtActiveMode.Text =
                    "SỐ THƯỜNG";

                txtActiveMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkBlue);
            }
            else
            {
                txtActiveMode.Text =
                    "CHƯA GỌI SỐ";

                txtActiveMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkGray);
            }
        }


        // =========================================================
        // CẬP NHẬT 2 MÀN HÌNH HIỂN THỊ
        // =========================================================

        private void UpdateDisplays()
        {
            int start;
            int end;
            string mode;

            if (state.ActiveMode ==
                "SỐ ƯU TIÊN")
            {
                start =
                    state.PriorityLastStart;

                end =
                    state.PriorityLastEnd;

                mode =
                    "SỐ ƯU TIÊN";
            }
            else if (state.ActiveMode ==
                     "SỐ THƯỜNG")
            {
                start =
                    state.NormalLastStart;

                end =
                    state.NormalLastEnd;

                mode =
                    "SỐ THƯỜNG";
            }
            else
            {
                return;
            }

            if (start <= 0)
                return;

            displayWindow.ShowRange(
                start,
                end,
                mode);

            displayWindow2.ShowRange(
                start,
                end,
                mode);
        }


        // =========================================================
        // LƯU TRẠNG THÁI
        // =========================================================

        private void SaveState()
        {
            try
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(
                        SaveFile)!);

                string json =
                    JsonSerializer.Serialize(
                        state,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(
                    SaveFile,
                    json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể lưu trạng thái:\n" +
                    ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // ĐỌC TRẠNG THÁI
        // =========================================================

        private void LoadState()
        {
            if (!File.Exists(SaveFile))
            {
                state = new QueueState
                {
                    NormalStart = 1,
                    NormalStep = 10,

                    PriorityStart = 1,
                    PriorityStep = 5,

                    NormalLastStart = 0,
                    NormalLastEnd = 0,

                    PriorityLastStart = 0,
                    PriorityLastEnd = 0,

                    ActiveMode = ""
                };

                return;
            }

            try
            {
                string json =
                    File.ReadAllText(
                        SaveFile);

                QueueState? loaded =
                    JsonSerializer.Deserialize<QueueState>(
                        json);

                if (loaded != null)
                {
                    state = loaded;
                }
            }
            catch
            {
                state = new QueueState
                {
                    NormalStart = 1,
                    NormalStep = 10,

                    PriorityStart = 1,
                    PriorityStep = 5,

                    ActiveMode = ""
                };
            }
        }


        // =========================================================
        // ĐÓNG ỨNG DỤNG
        // =========================================================

        private void MainWindow_Closing(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            SaveState();

            if (displayWindow != null)
                displayWindow.Close();

            if (displayWindow2 != null)
                displayWindow2.Close();

            Application.Current.Shutdown();
        }


        // =========================================================
        // MODEL LƯU TRẠNG THÁI
        // =========================================================

        private class QueueState
        {
            public int NormalStart { get; set; } = 1;

            public int NormalStep { get; set; } = 10;

            public int NormalLastStart { get; set; }

            public int NormalLastEnd { get; set; }


            public int PriorityStart { get; set; } = 1;

            public int PriorityStep { get; set; } = 5;

            public int PriorityLastStart { get; set; }

            public int PriorityLastEnd { get; set; }


            public string ActiveMode { get; set; } = "";
        }
    }
}