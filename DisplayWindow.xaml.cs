using System.Windows;

namespace HospitalQueueCaller
{
    public partial class DisplayWindow : Window
    {
        public DisplayWindow()
        {
            InitializeComponent();
        }

        public void ShowNumber(int number)
        {
            txtDisplayNumber.Text = number.ToString("D3");
        }

        public void ShowRange(int start, int end)
        {
            txtDisplayRange.Text =
                $"XIN MỜI SỐ THỨ TỰ TỪ {start:D3} ĐẾN {end:D3}";
        }
    }
}