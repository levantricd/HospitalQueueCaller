using System.Windows;
using System.Windows.Media;

namespace HospitalQueueCaller
{
    public partial class DisplayWindow : Window
    {
        public DisplayWindow()
        {
            InitializeComponent();
        }


        // =====================================================
        // HIỂN THỊ DÃY SỐ ĐANG GỌI
        // =====================================================

        public void ShowRange(
            int start,
            int end,
            string mode)
        {
            txtDisplayStart.Text =
                start.ToString("D3");

            txtDisplayEnd.Text =
                end.ToString("D3");


            // =================================================
            // SỐ ƯU TIÊN
            // =================================================

            if (mode == "SỐ ƯU TIÊN")
            {
                txtDisplayMode.Text =
                    "SỐ ƯU TIÊN";

                txtDisplayMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkRed);
            }


            // =================================================
            // SỐ THƯỜNG
            // =================================================

            else
            {
                txtDisplayMode.Text = "";

                txtDisplayMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkBlue);
            }
        }


        // =====================================================
        // HIỂN THỊ THÔNG BÁO ƯU TIÊN
        // =====================================================

        public void ShowPriorityDirectNotice()
        {
            txtPriorityNoticeTitle.Text =
                "ĐỐI TƯỢNG ƯU TIÊN";

            txtPriorityNotice.Text =
                "MỜI ĐẾN QUẦY TIẾP NHẬN";
        }


        // =====================================================
        // TẮT THÔNG BÁO ƯU TIÊN
        // =====================================================

        public void HidePriorityDirectNotice()
        {
            txtPriorityNoticeTitle.Text = "";

            txtPriorityNotice.Text = "";
        }
    }
}