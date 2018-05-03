using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    /// <summary>处理游戏大厅中每个房间的玩家</summary>
    public class GameTables
    {
        private const int max = 16; //棋盘网格最大的行列数
        public const int None = -1; //无棋子
        public const int Black = 0; //黑棋
        public const int White = 1; //白棋

        /// <summary>保存同一房间的两个玩家</summary>
        public User[] players { get; set; }

        /// <summary>棋盘，-1：无棋子，0：黑棋，1：白棋</summary>
        private int[,] grid = new int[max, max];

        /// <summary>下一步棋子颜色号（0：黑棋,1：白棋）</summary>
        private int nextColor = 0;

        public GameTables()
        {
            players = new User[2];
            ResetGrid();
        }

        /// <summary>重置棋盘</summary>
        public void ResetGrid()
        {
            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                    grid[i, j] = None;
                }
            }
        }

        /// <summary>
        /// 获取棋子落下后是否获胜
        /// </summary>
        public bool IsWin(int row, int col)
        {
            int x = 0, y = 0;
            //与方格的第i,j交叉点向四个方向的连子数，依次是水平，垂直，左上右下，左下右上
            int[] n = new int[4];
            #region 检查水平同色棋子个数
            n[0] = 1; //连子个数
            x = row + 1; //向右检查，前方棋子与row,col点不同时跳出循环
            while (x < max)
            {
                if (grid[x, col] == grid[row, col]) { n[0]++; x++; } else { break; }
            }
            x = row - 1; //向左检查，前方棋子与row,col点不同时跳出循环
            while (x >= 0)
            {
                if (grid[x, col] == grid[row, col]) { n[0]++; x--; } else { break; }
            }
            #endregion
            #region 检查垂直同色棋子个数
            n[1] = 1; //连子个数
            y = col + 1; //向右检查，前方棋子与row,col点不同时跳出循环
            while (y < max)
            {
                if (grid[row, y] == grid[row, col]) { n[1]++; y++; } else { break; }
            }
            y = col - 1; //向上检查，前方棋子与row,col点不同时跳出循环
            while (y >= 0)
            {
                if (grid[row, y] == grid[row, col]) { n[1]++; y--; } else { break; }
            }
            #endregion
            #region 检查左上到右下同色棋子个数
            n[2] = 1; //连子个数
            x = row + 1;
            y = col + 1;
            while (x<max && y < max)
            {
                if (grid[x, y] == grid[row, col]) { n[2]++; x++; y++; } else { break; }
            }
            x = row - 1;
            y = col - 1;
            while (x>=0 && y >= 0)
            {
                if (grid[x, y] == grid[row, col]) { n[2]++; x--; y--; } else { break; }
            }
            #endregion
            #region 检查左下到右上同色棋子个数
            n[3] = 1; //连子个数
            x = row - 1;
            y = col + 1;
            while (x >=0 && y < max)
            {
                if (grid[x, y] == grid[row, col]) { n[3]++; x--; y++; } else { break; }
            }
            x = row + 1;
            y = col - 1;
            while (x < max && y >= 0)
            {
                if (grid[x, y] == grid[row, col]) { n[3]++; x++; y--; } else { break; }
            }
            #endregion
            //检查是否获胜
            for (int k = 0; k < n.Length; k++)
            {
                if (Math.Abs(n[k]) == 5) return true;
            }
            return false;
        }

        /// <summary>放置棋子。参数：行，列</summary>
        public void SetGridDot(int i, int j)
        {
            grid[i, j] = nextColor;
            players[0].callback.ShowSetDot(i, j, nextColor);
            players[1].callback.ShowSetDot(i, j, nextColor);
            
            if (IsWin(i, j))
            {
                players[0].IsStarted = false;
                players[1].IsStarted = false;
                string message = nextColor == 0 ? "红方胜" : "蓝方胜";
                players[0].callback.GameWin(message);
                players[1].callback.GameWin(message);
                this.ResetGrid();
            }
            else
            {
               nextColor = (nextColor + 1) % 2;
            }
        }

        public void SetDoubleGridDot(int table,int i, int j, int nextX,  int nextY)
        {
            grid[i, j] = nextColor;
            players[0].callback.ShowSetDoubleDot(i, j, nextColor, nextX, nextY);
            players[1].callback.ShowSetDoubleDot(i, j, nextColor, nextX, nextY);

            nextColor = (nextColor + 1) % 2;
        }
        //public void SetNextGrid(int i, int j)
        //{
        //    players[0].callback.ShowSetGetNext(i, j);
        //    players[1].callback.ShowSetGetNext( i, j);
        //}
    }
}
