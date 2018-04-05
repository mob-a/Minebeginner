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
    class CellPoint
    {
        public int ColumnID { get; set; }
        public int RowID { get; set; }
    }

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
            InitializeBoard();
        }

        void InitializeBoard()
        {
            var level = ((ComboBoxItem)level_selection.SelectedItem).Content.ToString();
            if (level.Equals("HIGH"))
            {
                InitializeBoardBySize(20, 25, 50);
            }
            else
            {
                InitializeBoardBySize(15, 20, 20);
            }
        }
        void InitializeBoardBySize(int column_num, int row_num, int bomb_num)
        {
            end = false;
            mineboard = new MineBoard(column_num, row_num, bomb_num);
            datalistitems.DataContext = mineboard.Columns;
            closed_cell_counter.Text = mineboard.ClosedSafeCellNum.ToString();
            this.Width = column_num * 20 + 36;
            this.Height = row_num * 20 + 108;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            System.Windows.Input.Keyboard.ClearFocus();
            if (end)
            {
                return;
            }
            var button = (Button)sender;
            var cell_point = GetCellPoint(button);
            var result = mineboard.Open(cell_point.ColumnID, cell_point.RowID);
            if (result == OpenResult.BOMB)
            {
                end = true;

                // [TODO]              button.GetBindingExpression(Button.ContentProperty).UpdateTarget();
                button.Content = "★";
                var window = new BombMessageWindow();
                window.Show();
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
                    if (btn.Style == this.Resources["ClosedCellButtonStyle"])
                    {
                        var cp = GetCellPoint(btn);
                        var cell = mineboard.GetCell(cp.ColumnID, cp.RowID);
                        if (cell.Opened)
                        {
                            btn.Style = (Style)this.Resources["OpenedCellButtonStyle"];
                            btn.Content = cell.AroundBombs <= 0 ? "" : cell.AroundBombs.ToString();
                        }
                    }
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
            var cp = GetCellPoint(button);
            mineboard.ReverseFlag(cp.ColumnID, cp.RowID);

            var cell = mineboard.GetCell(cp.ColumnID, cp.RowID);
            if (!cell.Opened)
            {
                if (cell.HasFlag)
                {
                    button.Content = "■";
                }
                else
                {
                    button.Content = "";
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitializeBoard();
        }

        private CellPoint GetCellPoint(Button btn)
        {
            var tag = btn.Tag;
            int[] columnid_rowid = tag.ToString().Split('/').Select(numstr => int.Parse(numstr)).ToArray();

            int column_id = columnid_rowid[0];
            int row_id = columnid_rowid[1];
            return new CellPoint
            {
                ColumnID = column_id,
                RowID = row_id
            };
        }
    }

}
