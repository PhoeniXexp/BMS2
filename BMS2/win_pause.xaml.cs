using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BMS2
{
    /// <summary>
    /// Логика взаимодействия для win_pause.xaml
    /// </summary>
    public partial class win_pause : Window
    {
        public win_pause(int _p1, int _p2)
        {
            InitializeComponent();
            textBox1.Text = _p1.ToString();
            textBox2.Text = _p2.ToString();
            textBox2.Focus();
        }

        private int conv(string s)
        {
            try { return Convert.ToInt32(s); }
            catch { return 30; }            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        internal int p1
        {
            get { return conv(textBox1.Text); }
        }

        internal int p2
        {
            get { return conv(textBox2.Text); }
        }
    }
}
