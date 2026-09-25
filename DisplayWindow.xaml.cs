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
            txtDisplayMode.Text = mode;

            txtDisplayStart.Text =
                start.ToString("D3");

            txtDisplayEnd.Text =
                end.ToString("D3");

            if (mode == "SỐ ƯU TIÊN")
            {
                txtDisplayMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkRed);
            }
            else
            {
                txtDisplayMode.Foreground =
                    new SolidColorBrush(
                        Colors.DarkBlue);
            }
        }
    }
}