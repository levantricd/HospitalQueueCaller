using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Forms = System.Windows.Forms;
using WpfApplication = System.Windows.Application;
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;

namespace HospitalQueueCaller
{
    public partial class MainWindow : Window
    {
        private readonly DisplayWindow displayWindow;

        private QueueState state = new QueueState();

        private int currentScreenIndex = 0;

        private Forms.Screen[] screens = Array.Empty<Forms.Screen>();


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

            LoadState();

            btnStartNormal.Click += BtnStartNormal_Click;
            btnNextNormal.Click += BtnNextNormal_Click;

            btnStartPriority.Click += BtnStartPriority_Click;
            btnNextPriority.Click += BtnNextPriority_Click;

            btnSwitchScreen.Click += BtnSwitchScreen_Click;

            Closing += MainWindow_Closing;

            UpdateUI();

            DetectScreens();

            MoveDisplayToPreferredScreen();

            if (!string.IsNullOrWhiteSpace(state.ActiveMode))
            {
                UpdateDisplays();
            }
        }


        // =========================================================
        // PHÁT HIỆN MÀN HÌNH
        // =========================================================

        private void DetectScreens()
        {
            screens = Forms.Screen.AllScreens;

            if (screens.Length == 0)
            {
                txtCurrentScreen.Text =
                    "Không phát hiện màn hình";

                return;
            }

            if (currentScreenIndex >= screens.Length)
            {
                currentScreenIndex = 0;
            }
        }


        // =========================================================
        // TỰ ĐỘNG CHỌN MÀN HÌNH PHỤ
        // =========================================================

        private void MoveDisplayToPreferredScreen()
        {
            DetectScreens();

            if (screens.Length == 0)
                return;


            // Có màn hình phụ
            if (screens.Length > 1)
            {
                for (int i = 0; i < screens.Length; i++)
                {
                    if (!screens[i].Primary)
                    {
                        currentScreenIndex = i;
                        break;
                    }
                }
            }
            else
            {
                // Chỉ có một màn hình
                currentScreenIndex = 0;
            }


            MoveDisplayToScreen(
                currentScreenIndex);
        }


        // =========================================================
        // ĐƯA DISPLAY WINDOW SANG MÀN HÌNH
        // =========================================================

        private void MoveDisplayToScreen(
            int screenIndex)
        {
            DetectScreens();

            if (screens.Length == 0)
                return;

            if (screenIndex < 0 ||
                screenIndex >= screens.Length)
            {
                screenIndex = 0;
            }

            currentScreenIndex =
                screenIndex;

            Forms.Screen screen =
                screens[currentScreenIndex];


            // Đóng trạng thái maximize WPF
            displayWindow.WindowState =
                WindowState.Normal;


            // Lấy kích thước màn hình thực tế
            displayWindow.Left =
                screen.Bounds.Left;

            displayWindow.Top =
                screen.Bounds.Top;

            displayWindow.Width =
                screen.Bounds.Width;

            displayWindow.Height =
                screen.Bounds.Height;


            // Full màn hình
            displayWindow.WindowStyle =
                WindowStyle.None;

            displayWindow.ResizeMode =
                ResizeMode.NoResize;

            displayWindow.Topmost = true;


            if (!displayWindow.IsVisible)
            {
                displayWindow.Show();
            }


            displayWindow.Activate();


            txtCurrentScreen.Text =
                $"Màn hình {currentScreenIndex + 1}: " +
                $"{screen.Bounds.Width} × " +
                $"{screen.Bounds.Height}" +
                (screen.Primary
                    ? "  (Màn hình chính)"
                    : "  (Màn hình phụ)");
        }


        // =========================================================
        // CHUYỂN MÀN HÌNH
        // =========================================================

        private void BtnSwitchScreen_Click(
            object sender,
            RoutedEventArgs e)
        {
            DetectScreens();

            if (screens.Length <= 1)
            {
                WpfMessageBox.Show(
                    "Máy hiện chỉ có một màn hình.",
                    "Màn hình",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }


            currentScreenIndex++;

            if (currentScreenIndex >= screens.Length)
            {
                currentScreenIndex = 0;
            }


            MoveDisplayToScreen(
                currentScreenIndex);
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

            if (state.NormalLastStart <= 0)
            {
                BtnStartNormal_Click(
                    sender,
                    e);

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

            if (state.PriorityLastStart <= 0)
            {
                BtnStartPriority_Click(
                    sender,
                    e);

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
        // ĐỌC CẤU HÌNH
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
                WpfMessageBox.Show(
                    "Số bắt đầu và bước tăng không hợp lệ.");

                return false;
            }

            state.NormalStart = start;
            state.NormalStep = step;

            return true;
        }


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
                WpfMessageBox.Show(
                    "Số bắt đầu và bước tăng không hợp lệ.");

                return false;
            }

            state.PriorityStart = start;
            state.PriorityStep = step;

            return true;
        }


        // =========================================================
        // CẬP NHẬT GIAO DIỆN
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
        // CẬP NHẬT MÀN HÌNH HIỂN THỊ
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


            // Đảm bảo display vẫn nằm trên
            // đúng màn hình sau khi cập nhật
            if (!displayWindow.IsVisible)
            {
                MoveDisplayToScreen(
                    currentScreenIndex);
            }
        }


        // =========================================================
        // LƯU
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
            catch
            {
                // Không làm gián đoạn việc gọi số
            }
        }


        // =========================================================
        // LOAD
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
        // ĐÓNG
        // =========================================================

        private void MainWindow_Closing(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            SaveState();

            if (displayWindow != null)
            {
                displayWindow.Close();
            }

            WpfApplication.Current.Shutdown();
        }


        // =========================================================
        // STATE
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