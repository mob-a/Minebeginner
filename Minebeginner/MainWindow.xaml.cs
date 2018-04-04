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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;


namespace Minebeginner
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MineBoard mineboard;
        private Boolean end;

        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard(15, 20, 10);
        }

        void InitializeBoard(int column_num, int row_num, int bomb_num)
        {
            end = false;
            mineboard = new MineBoard(column_num, row_num, bomb_num);
            datalistitems.DataContext = mineboard.Columns;
            closed_cell_counter.Text = mineboard.ClosedSafeCellNum.ToString();
            this.Width = column_num * 20 + 36;
            this.Height = row_num * 20 + 102;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (end)
            {
                return;
            }
            var button = (Button)sender;
            var tag = button.Tag;
            var columnid_rowid = tag.ToString().Split('/').Select(numstr => int.Parse(numstr)).ToArray();

            var column_id = columnid_rowid[0];
            var row_id = columnid_rowid[1];
            var result = mineboard.Open(column_id, row_id);
            if (result == OpenResult.BOMB)
            {
                end = true;
                var window = new BombMessageWindow();
                window.Show();
                button.GetBindingExpression(Button.ContentProperty).UpdateTarget();
            }
            else if (result == OpenResult.SUCCESS)
            {
                closed_cell_counter.Text = mineboard.ClosedSafeCellNum.ToString();
                if (mineboard.ClosedSafeCellNum == 0)
                {
                    var cwindow = new ClearMessageWindow();
                    cwindow.Show();
                    end = true;
                }
                foreach (Button btn in DescendantButtons(datalistitems))
                {
                    // [TODO] 全部updateするのはかなり効率悪い
                    btn.GetBindingExpression(Button.ContentProperty).UpdateTarget();
                }
            }
        }

        public static List<Button> DescendantButtons(DependencyObject obj)
        {
            //http://blog.xin9le.net/entry/2013/10/29/222336
            var l = new List<Button>();
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child == null)
                {
                    continue;
                }
                if (child is Button)
                {
                    l.Add((Button)child);
                }
                var child_count = VisualTreeHelper.GetChildrenCount(child);
                if (child_count > 0)
                {
                    l = l.Concat(DescendantButtons(child)).ToList();
                }
            }
            return l;
        }

        private void Button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (end)
            {
                return;
            }
            var button = (Button)sender;
            var tag = button.Tag;
            var columnid_rowid = tag.ToString().Split('/').Select(numstr => int.Parse(numstr)).ToArray();

            var column_id = columnid_rowid[0];
            var row_id = columnid_rowid[1];

            mineboard.ReverseFlag(column_id, row_id);
            button.GetBindingExpression(Button.ContentProperty).UpdateTarget();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitializeBoard(5, 4, 3);
        }
    }

}
