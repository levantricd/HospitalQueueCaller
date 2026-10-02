using System;
using System.IO;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;
using Forms = System.Windows.Forms;
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

        private Forms.Screen[] screens =
            Array.Empty<Forms.Screen>();


        private string SaveFile =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "HospitalQueueCaller",
                "queue.json");


        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);


        // =========================================================
        // KHỞI TẠO
        // =========================================================

        public MainWindow()
        {
            InitializeComponent();

            displayWindow = new DisplayWindow();

            LoadState();


            // -----------------------------------------------------
            // SỐ THƯỜNG
            // -----------------------------------------------------

            btnStartNormal.Click +=
                BtnStartNormal_Click;

            btnNextNormal.Click +=
                BtnNextNormal_Click;


            // -----------------------------------------------------
            // SỐ ƯU TIÊN
            // -----------------------------------------------------

            btnStartPriority.Click +=
                BtnStartPriority_Click;

            btnNextPriority.Click +=
                BtnNextPriority_Click;


            // -----------------------------------------------------
            // MỜI ƯU TIÊN TRỰC TIẾP
            // -----------------------------------------------------

            btnPriorityDirect.Click +=
                BtnPriorityDirect_Click;


            // -----------------------------------------------------
            // MÀN HÌNH
            // -----------------------------------------------------

            btnSwitchScreen.Click +=
                BtnSwitchScreen_Click;


            Closing +=
                MainWindow_Closing;


            // -----------------------------------------------------
            // CẬP NHẬT GIAO DIỆN
            // -----------------------------------------------------

            UpdateUI();

            DetectScreens();

            MoveDisplayToPreferredScreen();


            // -----------------------------------------------------
            // KHÔI PHỤC MÀN HÌNH
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    state.ActiveMode))
            {
                UpdateDisplays();
            }
            else
            {
                UpdatePriorityDirectNotice();
            }
        }


        // =========================================================
        // PHÁT HIỆN MÀN HÌNH
        // =========================================================

        private void DetectScreens()
        {
            screens =
                Forms.Screen.AllScreens;

            if (screens.Length == 0)
            {
                txtCurrentScreen.Text =
                    "Không phát hiện màn hình";

                return;
            }

            if (currentScreenIndex >=
                screens.Length)
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


            if (screens.Length > 1)
            {
                for (int i = 0;
                     i < screens.Length;
                     i++)
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


            displayWindow.WindowState =
                WindowState.Normal;

            displayWindow.WindowStyle =
                WindowStyle.None;

            displayWindow.ResizeMode =
                ResizeMode.NoResize;


            // -----------------------------------------------------
            // MÀN HÌNH CHÍNH
            // -----------------------------------------------------

            if (screen.Primary)
            {
                displayWindow.Topmost = false;
                Topmost = true;
            }
            else
            {
                displayWindow.Topmost = true;
                Topmost = false;
            }


            if (!displayWindow.IsVisible)
            {
                displayWindow.Show();
            }


            IntPtr hwnd =
                new WindowInteropHelper(
                    displayWindow).Handle;

            if (hwnd != IntPtr.Zero)
            {
                IntPtr insertAfter =
                    screen.Primary
                        ? new IntPtr(-2)
                        : new IntPtr(-1);

                SetWindowPos(
                    hwnd,
                    insertAfter,
                    screen.Bounds.Left,
                    screen.Bounds.Top,
                    screen.Bounds.Width,
                    screen.Bounds.Height,
                    SWP_NOACTIVATE);
            }


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
                    WpfMessageBoxButton.OK,
                    WpfMessageBoxImage.Information);

                return;
            }


            currentScreenIndex++;

            if (currentScreenIndex >=
                screens.Length)
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
        // ƯU TIÊN - MỜI TRỰC TIẾP
        // =========================================================

        private void BtnPriorityDirect_Click(
            object sender,
            RoutedEventArgs e)
        {
            // -----------------------------------------------------
            // Nếu chưa bật:
            // Bật thông báo mời ưu tiên đến quầy.
            // -----------------------------------------------------

            if (!state.PriorityDirectNotice)
            {
                state.PriorityDirectNotice = true;

                btnPriorityDirect.Content =
                    "KẾT THÚC MỜI ƯU TIÊN";

                txtStatus.Text =
                    "Đang mời đối tượng ưu tiên " +
                    "đến quầy tiếp nhận.";

                SaveState();

                UpdatePriorityDirectNotice();

                return;
            }


            // -----------------------------------------------------
            // Nếu đang bật:
            // Tắt thông báo.
            // -----------------------------------------------------

            state.PriorityDirectNotice = false;

            btnPriorityDirect.Content =
                "MỜI ƯU TIÊN ĐẾN QUẦY";

            txtStatus.Text =
                "Đã kết thúc mời ưu tiên trực tiếp.";

            SaveState();

            UpdatePriorityDirectNotice();
        }


        // =========================================================
        // CẬP NHẬT THÔNG BÁO ƯU TIÊN TRỰC TIẾP
        // =========================================================

        private void UpdatePriorityDirectNotice()
        {
            if (state.PriorityDirectNotice)
            {
                displayWindow.ShowPriorityDirectNotice();

                btnPriorityDirect.Content =
                    "KẾT THÚC MỜI ƯU TIÊN";
            }
            else
            {
                displayWindow.HidePriorityDirectNotice();

                btnPriorityDirect.Content =
                    "MỜI ƯU TIÊN ĐẾN QUẦY";
            }
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
                WpfMessageBox.Show(
                    "Số bắt đầu và bước tăng không hợp lệ.");

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


            // -----------------------------------------------------
            // SỐ THƯỜNG
            // -----------------------------------------------------

            if (state.NormalLastStart > 0)
            {
                txtNormalRange.Text =
                    $"{state.NormalLastStart:D3} - " +
                    $"{state.NormalLastEnd:D3}";
            }
            else
            {
                txtNormalRange.Text =
                    "---";
            }


            // -----------------------------------------------------
            // SỐ ƯU TIÊN
            // -----------------------------------------------------

            if (state.PriorityLastStart > 0)
            {
                txtPriorityRange.Text =
                    $"{state.PriorityLastStart:D3} - " +
                    $"{state.PriorityLastEnd:D3}";
            }
            else
            {
                txtPriorityRange.Text =
                    "---";
            }


            // -----------------------------------------------------
            // CHẾ ĐỘ ĐANG GỌI
            // -----------------------------------------------------

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


            // -----------------------------------------------------
            // NÚT ƯU TIÊN TRỰC TIẾP
            // -----------------------------------------------------

            if (state.PriorityDirectNotice)
            {
                btnPriorityDirect.Content =
                    "KẾT THÚC MỜI ƯU TIÊN";
            }
            else
            {
                btnPriorityDirect.Content =
                    "MỜI ƯU TIÊN ĐẾN QUẦY";
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


            // -----------------------------------------------------
            // XÁC ĐỊNH DÃY ĐANG GỌI
            // -----------------------------------------------------

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
                UpdatePriorityDirectNotice();

                return;
            }


            if (start <= 0)
                return;


            // -----------------------------------------------------
            // HIỂN THỊ DÃY SỐ
            // -----------------------------------------------------

            displayWindow.ShowRange(
                start,
                end,
                mode);


            // -----------------------------------------------------
            // HIỂN THỊ THÔNG BÁO ƯU TIÊN ĐỘC LẬP
            // -----------------------------------------------------

            UpdatePriorityDirectNotice();


            // -----------------------------------------------------
            // ĐẢM BẢO DISPLAY ĐANG HIỂN THỊ
            // -----------------------------------------------------

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

                    ActiveMode = "",

                    PriorityDirectNotice = false
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

                    ActiveMode = "",

                    PriorityDirectNotice = false
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


            System.Windows.Application.Current.Shutdown();
        }


        // =========================================================
        // STATE
        // =========================================================

        private class QueueState
        {
            // -----------------------------------------------------
            // SỐ THƯỜNG
            // -----------------------------------------------------

            public int NormalStart { get; set; } = 1;

            public int NormalStep { get; set; } = 10;

            public int NormalLastStart { get; set; }

            public int NormalLastEnd { get; set; }


            // -----------------------------------------------------
            // SỐ ƯU TIÊN
            // -----------------------------------------------------

            public int PriorityStart { get; set; } = 1;

            public int PriorityStep { get; set; } = 5;

            public int PriorityLastStart { get; set; }

            public int PriorityLastEnd { get; set; }


            // -----------------------------------------------------
            // CHẾ ĐỘ ĐANG GỌI
            // -----------------------------------------------------

            public string ActiveMode { get; set; } = "";


            // -----------------------------------------------------
            // MỜI ƯU TIÊN TRỰC TIẾP
            // -----------------------------------------------------

            public bool PriorityDirectNotice
            {
                get;
                set;
            } = false;
        }
    }
}