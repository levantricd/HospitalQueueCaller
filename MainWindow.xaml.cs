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


            // SỐ THƯỜNG

            btnStartNormal.Click +=
                BtnStartNormal_Click;

            btnNextNormal.Click +=
                BtnNextNormal_Click;


            // SỐ ƯU TIÊN

            btnStartPriority.Click +=
                BtnStartPriority_Click;

            btnNextPriority.Click +=
                BtnNextPriority_Click;


            // ƯU TIÊN TRỰC TIẾP

            btnPriorityDirect.Click +=
                BtnPriorityDirect_Click;


            // CHUYỂN MÀN HÌNH

            btnSwitchScreen.Click +=
                BtnSwitchScreen_Click;


            Closing +=
                MainWindow_Closing;


            UpdateUI();

            DetectScreens();

            MoveDisplayToPreferredScreen();


            // Khôi phục màn hình sau khi mở chương trình

            if (!string.IsNullOrWhiteSpace(
                    state.ActiveMode))
            {
                UpdateDisplays();
            }
            else
            {
                UpdatePriorityDirectNotice();

                UpdatePreview();
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
                return;


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


            // Màn hình chính

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
        }


        // =========================================================
        // ƯU TIÊN - MỜI TRỰC TIẾP
        // =========================================================

        private void BtnPriorityDirect_Click(
            object sender,
            RoutedEventArgs e)
        {
            state.PriorityDirectNotice =
                !state.PriorityDirectNotice;


            SaveState();

            UpdateUI();

            UpdatePriorityDirectNotice();

            UpdatePreview();
        }


        // =========================================================
        // CẬP NHẬT THÔNG BÁO ƯU TIÊN
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


            state.NormalStart =
                start;

            state.NormalStep =
                step;

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


            state.PriorityStart =
                start;

            state.PriorityStep =
                step;

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
                txtNormalRange.Text =
                    "---";
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
                txtPriorityRange.Text =
                    "---";
            }


            // NÚT ƯU TIÊN

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


            UpdatePreview();
        }


        // =========================================================
        // CẬP NHẬT MÀN HÌNH THẬT
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
                UpdatePriorityDirectNotice();

                UpdatePreview();

                return;
            }


            if (start <= 0)
                return;


            displayWindow.ShowRange(
                start,
                end,
                mode);


            UpdatePriorityDirectNotice();


            UpdatePreview();


            if (!displayWindow.IsVisible)
            {
                MoveDisplayToScreen(
                    currentScreenIndex);
            }
        }


        // =========================================================
        // CẬP NHẬT PREVIEW
        // =========================================================

        private void UpdatePreview()
        {
            int start = 1;
            int end = 10;


            // -----------------------------------------------------
            // Xác định dãy số đang hiển thị
            // -----------------------------------------------------

            if (state.ActiveMode ==
                "SỐ ƯU TIÊN")
            {
                start =
                    state.PriorityLastStart;

                end =
                    state.PriorityLastEnd;

                txtPreviewMode.Text =
                    "SỐ ƯU TIÊN";

                txtPreviewMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkRed);
            }
            else if (state.ActiveMode ==
                     "SỐ THƯỜNG")
            {
                start =
                    state.NormalLastStart;

                end =
                    state.NormalLastEnd;

                txtPreviewMode.Text =
                    "";

                txtPreviewMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkBlue);
            }
            else
            {
                txtPreviewMode.Text =
                    "";

                txtPreviewMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkBlue);
            }


            if (start <= 0)
            {
                start = 1;
                end = 10;
            }


            txtPreviewStart.Text =
                start.ToString("D3");


            txtPreviewEnd.Text =
                end.ToString("D3");


            // -----------------------------------------------------
            // THÔNG BÁO ƯU TIÊN
            // -----------------------------------------------------

            if (state.PriorityDirectNotice)
            {
                txtPreviewPriorityTitle.Text =
                    "ĐỐI TƯỢNG ƯU TIÊN";

                txtPreviewPriorityNotice.Text =
                    "MỜI ĐẾN QUẦY TIẾP NHẬN";
            }
            else
            {
                txtPreviewPriorityTitle.Text =
                    "";

                txtPreviewPriorityNotice.Text =
                    "";
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
            // SỐ THƯỜNG

            public int NormalStart { get; set; } = 1;

            public int NormalStep { get; set; } = 10;

            public int NormalLastStart { get; set; }

            public int NormalLastEnd { get; set; }


            // SỐ ƯU TIÊN

            public int PriorityStart { get; set; } = 1;

            public int PriorityStep { get; set; } = 5;

            public int PriorityLastStart { get; set; }

            public int PriorityLastEnd { get; set; }


            // CHẾ ĐỘ ĐANG HIỂN THỊ

            public string ActiveMode { get; set; } = "";


            // MỜI ƯU TIÊN TRỰC TIẾP

            public bool PriorityDirectNotice
            {
                get;
                set;
            } = false;
        }
    }
}