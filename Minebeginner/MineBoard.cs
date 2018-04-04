using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Minebeginner
{
    enum OpenResult { SUCCESS, BOMB, CANNOTOPEN };
    class MineBoard
    {
        public List<Column> Columns { get; }
        public int ColumnNum { get; }
        public int RowNum { get; }
        public int BombNum { get; }
        public int ClosedSafeCellNum { get; set; }

        public MineBoard(int column_num, int row_num, int bomb_num)
        {
            ColumnNum = column_num;
            RowNum = row_num;
            BombNum = bomb_num;

            ClosedSafeCellNum = ColumnNum * RowNum - BombNum; ;

            int size = RowNum * ColumnNum;
            int[] nums = new int[size];
            for (int i = 0; i < size; i++)
            {
                nums[i] = i;
            }
            int[] shuffled = nums.OrderBy(i => Guid.NewGuid()).ToArray();
            int[] bombIds = new int[BombNum];
            for (int j = 0; j < BombNum; j++)
            {
                bombIds[j] = shuffled[j];
            }

            Columns = new List<Column>();
            for (int column_id = 0; column_id < ColumnNum; column_id++)
            {
                var column = new Column();

                for (int row_id = 0; row_id < RowNum; row_id++)
                {
                    Boolean has_bomb = bombIds.Contains(column_id * RowNum + row_id);
                    column.Cells.Add(new Cell(column_id, row_id, has_bomb));
                }
                Columns.Add(column);
            }

            for (int column_id = 0; column_id < ColumnNum; column_id++)
            {
                for (int row_id = 0; row_id < RowNum; row_id++)
                {
                    var cell = GetCell(column_id, row_id);
                    if (!cell.HasBomb)
                    {
                        cell.AroundBombs = GetAroundBombs(column_id, row_id);
                    }
                }
            }
        }

        public Cell GetCell(int column_id, int row_id)
        {
            return Columns[column_id].Cells[row_id];
        }

        public OpenResult Open(int column_id, int row_id)
        {
            var cell = GetCell(column_id, row_id);
            if (cell.HasFlag || cell.Opened)
            {
                return OpenResult.CANNOTOPEN;
            }

            cell.Open();
            if (cell.HasBomb)
            {
                return OpenResult.BOMB;
            }
            else
            {
                ClosedSafeCellNum--;
                if (cell.AroundBombs == 0)
                {
                    for (int c = column_id - 1; c <= column_id + 1; c++)
                    {
                        if (c < 0 || c >= ColumnNum)
                        {
                            continue;
                        }
                        for (int r = row_id - 1; r <= row_id + 1; r++)
                        {
                            if (c == column_id && r == row_id)
                            {
                                continue;
                            }
                            if (r < 0 || r >= RowNum)
                            {
                                continue;
                            }
                            var acell = this.GetCell(c, r);
                            if (!acell.Opened)
                            {
                                Open(c, r);
                            }
                        }
                    }
                }
                return OpenResult.SUCCESS;
            }
        }
        public void ReverseFlag(int column_id, int row_id)
        {
            var cell = GetCell(column_id, row_id);
            if (!cell.Opened)
            {
                cell.ReverseFlag();
            }
        }
        public int GetAroundBombs(int column_id, int row_id)
        {
            int num = 0;
            for (int c = column_id - 1; c <= column_id + 1; c++)
            {
                if (c < 0 || c >= ColumnNum)
                {
                    continue;
                }
                for (int r = row_id - 1; r <= row_id + 1; r++)
                {
                    if (c == column_id && r == row_id)
                    {
                        continue;
                    }
                    if (r < 0 || r >= RowNum)
                    {
                        continue;
                    }
                    if (GetCell(c, r).HasBomb)
                    {
                        num++;
                    }
                }
            }
            return num;
        }
    }
    class Column
    {
        public List<Cell> Cells { get; set; }
        public Column()
        {
            Cells = new List<Cell>();
        }
    }
    public class Cell : INotifyPropertyChanged
    {
        // http://blog.livedoor.jp/morituri/archives/54652766.html
        public event PropertyChangedEventHandler PropertyChanged;

        public int ColumnID { get; set; }
        public int RowID { get; set; }
        public string ID { get; set; }
        public Boolean HasBomb { get; set; }
        public Boolean Opened { get; set; }
        public string View { get; set; }
        public int AroundBombs { get; set; }
        public Boolean HasFlag { get; set; }
        public Cell(int column_id, int row_id, Boolean has_bomb)
        {
            ColumnID = column_id;
            RowID = row_id;
            HasBomb = has_bomb;
            ID = ColumnID + "/" + RowID;
            Opened = false;
            AroundBombs = -1;
            View = "";
            HasFlag = false;
        }
        public void ReverseFlag()
        {
            if (Opened)
            {
                return;
            }

            if (HasFlag)
            {
                View = "";
                HasFlag = false;
                OnPropertyChanged("View");
            }
            else
            {
                View = "■";
                HasFlag = true;
                OnPropertyChanged("View");
            }
        }

        public void Open()
        {
            if (Opened)
            {
                return;
            }
            Opened = true;
            if (HasBomb)
            {
                View = "★";

            }
            else
            {
                View = AroundBombs.ToString();
            }
            OnPropertyChanged("View");
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
