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


        public void ShowRange(
            int start,
            int end,
            string mode)
        {
            txtDisplayStart.Text =
                start.ToString("D3");

            txtDisplayEnd.Text =
                end.ToString("D3");


            // =====================================================
            // SỐ ƯU TIÊN
            // =====================================================

            if (mode == "SỐ ƯU TIÊN")
            {
                txtDisplayMode.Text =
                    "SỐ ƯU TIÊN";

                txtDisplayMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkRed);
            }


            // =====================================================
            // SỐ THƯỜNG
            // =====================================================

            else
            {
                // Không hiển thị chữ "SỐ THƯỜNG"
                txtDisplayMode.Text = "";

                txtDisplayMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkBlue);
            }
        }
    }
}