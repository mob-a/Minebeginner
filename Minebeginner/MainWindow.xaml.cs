using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            if (level.Equals("Level:HIGH"))
            {
                InitializeBoardBySize(20, 25, 70);
            }
            else
            {
                InitializeBoardBySize(10, 15, 12);
            }
        }
        void InitializeBoardBySize(int column_num, int row_num, int bomb_num)
        {
            end = false;
            mineboard = new MineBoard(column_num, row_num, bomb_num);
            datalistitems.DataContext = mineboard.Columns;
            closed_cell_counter.Text = mineboard.ClosedSafeCellNum.ToString();
            this.Width = column_num * 21 + 37;
            this.Height = row_num * 21 + 101;
        }
        private void ResetCellChild(StackPanel cell_elem, UIElement child)
        {
            cell_elem.Children.Clear();
            if (child != null)
            {
                cell_elem.Children.Add(child);
            }
        }
        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (end)
            {
                return;
            }
            var cell_element = (StackPanel)sender;

            var cell_point = GetCellPoint(cell_element);
            var result = mineboard.Open(cell_point.ColumnID, cell_point.RowID);
            if (result == OpenResult.BOMB)
            {

                cell_element.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffffff");
                ResetCellChild(cell_element, new Image
                {
                    Source = (BitmapImage)this.Resources["BombImage"],
                    MaxWidth = 18,
                    MaxHeight = 18,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
                end = true;
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
                foreach (StackPanel sp in DescendantStackPanels(datalistitems))
                {
                    if (sp.Tag == null)
                    {
                        continue;
                    }
                    var cp = GetCellPoint(sp);
                    var cell = mineboard.GetCell(cp.ColumnID, cp.RowID);
                    if (cell.Opened)
                    {
                        // [TODO] 新しく開いたか判定
                        string[] colormap = { "#000000", "#000000", "#B36B00", "#14B2B2", "#B2A214", "#1423B2", "#B21473", "#14B223", "#B21414" };
                        string color = colormap[cell.AroundBombs];
                        string text = cell.AroundBombs <= 0 ? "" : cell.AroundBombs.ToString();

                        ResetCellChild(sp, new TextBlock
                        {
                            Text = text,
                            Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(color),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                        });
                        sp.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#ffffff");
                    }
                }
            }
        }

        public static List<StackPanel> DescendantStackPanels(DependencyObject obj)
        {
            //http://blog.xin9le.net/entry/2013/10/29/222336
            var l = new List<StackPanel>();
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child == null)
                {
                    continue;
                }
                if (child is StackPanel)
                {
                    l.Add((StackPanel)child);
                }
                var child_count = VisualTreeHelper.GetChildrenCount(child);
                if (child_count > 0)
                {
                    l = l.Concat(DescendantStackPanels(child)).ToList();
                }
            }
            return l;
        }

        private void StackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (end)
            {
                return;
            }
            var cell_element = (StackPanel)sender;
            var cp = GetCellPoint(cell_element);
            mineboard.ReverseFlag(cp.ColumnID, cp.RowID);

            var cell = mineboard.GetCell(cp.ColumnID, cp.RowID);
            if (!cell.Opened)
            {
                if (cell.HasFlag)
                {
                    ResetCellChild(cell_element, new Image
                    {
                        Source = (BitmapImage)this.Resources["FlagImage"],
                        MaxWidth = 18,
                        MaxHeight = 18,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    });
                }
                else
                {
                    ResetCellChild(cell_element, null);
                }
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Keyboard.ClearFocus();
            InitializeBoard();
        }

        private CellPoint GetCellPoint(StackPanel btn)
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

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            StackPanel cell_element = (StackPanel)sender;
            var cp = GetCellPoint(cell_element);
            if (!mineboard.GetCell(cp.ColumnID, cp.RowID).Opened)
            {
                cell_element.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#aaaaaa");
            }
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            StackPanel cell_element = (StackPanel)sender;
            var cp = GetCellPoint(cell_element);
            if (!mineboard.GetCell(cp.ColumnID, cp.RowID).Opened)
            {
                cell_element.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#777777");
            }
        }
    }
}
