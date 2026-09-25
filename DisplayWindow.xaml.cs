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
    }
}