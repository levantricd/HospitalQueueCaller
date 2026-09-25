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
            // Hiện tại chưa dùng ở phần bố cục này
        }

        public void ShowRange(int start, int end)
        {
            txtDisplayStart.Text = start.ToString("D3");
            txtDisplayEnd.Text = end.ToString("D3");
        }
    }
}