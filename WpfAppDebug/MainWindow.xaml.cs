using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using KTerminalLib;

namespace WpfAppDebug
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string str = ('\r').ToString()+((char)5) + "  104013 " + ((char)5) + ((char)16) + "слэш эр" + ((char)26) + " ПростоТекст ";
            kTerminal1.ДобавитьВСписокСтрок(str.ToCharArray());
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string str = ((char)5) + "  0000FF " + ((char)5) + ((char)16) +"Text 22"+ '\n' +
                ((char)5) + "  FF00FF " + ((char)5) + ((char)6) + "Text 3434 fefegf" + ((char)26) + "Просто текст " + '\n';
            kTerminal1.ДобавитьВСписокСтрок(str.ToCharArray());
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            string str = ((char)5) + "  00FF00 " + ((char)0x5) + ((char)16) + "слэш 2 эр 2" + ((char)26) + " ПростоТекст 2 ";
            kTerminal1.ДобавитьВСписокСтрок(str.ToCharArray());
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            kTerminal1.FontSize = 22;
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            string str = ((char)27) + "  2 " + ((char)27);
            kTerminal1.ДобавитьВСписокСтрок(str.ToCharArray());
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            string str = ((char)28).ToString();
            kTerminal1.ДобавитьВСписокСтрок(str.ToCharArray());
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            string str = ((char)5) + "  FF0000 " + ((char)0x5) + ((char)16) + "красный" + ((char)26) + "Другой ";
            kTerminal1.ДобавитьВСписокСтрок(str.ToCharArray());
        }

        private void button8_Click(object sender, RoutedEventArgs e)
        {
            kTerminal1.ОчиститьСписокСтрок();
        }
        Thread threadAdding = null;
        private void button9_Click(object sender, RoutedEventArgs e)
        {
            if (threadAdding != null) { threadAdding.Abort(); threadAdding = null; }
            else
            {
                threadAdding = new Thread(new ParameterizedThreadStart(HandleGotoNumStr));
                threadAdding.Start(this);
            }
        }
        static void HandleGotoNumStr(object obj)
        {
            MainWindow win = obj as MainWindow;
            Random rand = new Random();
            do
            {
                int ind = rand.Next(-1, 27);
                string str = ((char)27) + " " + ind.ToString() + " " + ((char)27);

                win.Dispatcher.Invoke(new Action<string>((s) => win.kTerminal1.ДобавитьВСписокСтрок(str.ToCharArray())), str);
                for (int i = 10000000; i > 0; i--) ;
            }
            while (true);
        }

        private void button10_Click(object sender, RoutedEventArgs e)
        {
            if (null != threadAdding) threadAdding.Abort();
        }
    }
}
