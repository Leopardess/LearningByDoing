using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoSizeMultipleColumnHeader
{
    public partial class Form1 : Form
    {
        private bool resize = false;
        private int defaultColumnHeadersHeight = 60;
        private DataSet dsHeader = null, dsDetail = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void InvalidateHeader()
        {
            Rectangle rtHeader = this.dataGridView1.DisplayRectangle;
            rtHeader.Height = this.dataGridView1.ColumnHeadersHeight / 2;
            this.dataGridView1.Invalidate(rtHeader);
        }

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            this.InvalidateHeader();
        }

        private void dataGridView1_Resize(object sender, EventArgs e)
        {
            this.InvalidateHeader();
        }

        private void dataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            this.InvalidateHeader();
        }

        private void dataGridView1_Paint(object sender, PaintEventArgs e)
        {
            if (this.dataGridView1.Columns.Count <= 1)   //若無欄位資料跳過繪圖
                return;

            //確認當前可見的最左儲存格的標題位置 
            int startCol = this.dataGridView1.FirstDisplayedCell.ColumnIndex;
            int dgvLastWidth = this.dataGridView1.Width;     //預設最長即為 dgv 寬度
            string mainHeader, subHeader;
            int characterWidth = 7; //設定每一字元預設給予的寬度
            int dGroupCount = 1;    //確認需合併的儲存格數量
            Brush back = new SolidBrush(this.dataGridView1.ColumnHeadersDefaultCellStyle.BackColor);
            Brush fore = new SolidBrush(this.dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor);
            Pen p = new Pen(this.dataGridView1.GridColor);
            StringFormat format;    //Rectangle格式

            for (int i = 0; i < dsHeader.Tables[0].Rows.Count; i++)
            {
                mainHeader = dsHeader.Tables[0].Rows[i][0].ToString();
                subHeader = dsHeader.Tables[0].Rows[i][1].ToString();

                if (resize)     //查詢時才重設表頭寬度，其他時候不特別重設寬度
                {
                    //依主/次表頭內容擇較長的長度給予寬度
                    if (mainHeader.Length >= subHeader.Length)
                    {
                        dataGridView1.Columns[i].Width = characterWidth * mainHeader.Length;   //設定欄寬為主要表頭寬
                    }
                    else
                    {
                        dataGridView1.Columns[i].Width = characterWidth * subHeader.Length;   //設定欄寬為次要表頭寬
                    }
                }

                if (i < dsHeader.Tables[0].Rows.Count - 1
                     && string.Equals(mainHeader, dsHeader.Tables[0].Rows[i + 1][0].ToString()))
                {   //若下一欄值與當前欄值資料重複
                    dGroupCount++;      //註記需合併欄位
                }
                else
                {
                    if (i >= startCol - 1               //左側有在顯示的範圍內的 Column 才繪製 Rectangle
                        && dgvLastWidth > 0)            //由左至右繪製，排除右側不在顯示範圍內的 Column
                    {
                        Rectangle r1 = this.dataGridView1.GetCellDisplayRectangle(i, -1, true);    //取得主表頭繪製範圍
                        int index = 1;  //Rectangle 動態寬度計數
                        while (dGroupCount > 1)
                        {
                            Rectangle r2 = this.dataGridView1.GetCellDisplayRectangle(i - index, -1, true);  //由合併最末欄往左檢查
                            //設定 Rectangle 起始 X 座標
                            if (r1.X > r2.X)        //若 r1 已在顯示範圍內，且左側欄主表頭內容與當前主表頭相同
                                r1.X -= r2.Width;   //左移一欄
                            else
                                r1.X = r2.X;        //若兩個都不在顯示範圍內，皆為 0

                            //設定 Rectangle 寬度
                            if (dgvLastWidth >= r1.Width + r2.Width)    //若主標題尚未佔滿版面
                                r1.Width += r2.Width;                   //以左側欄寬度為主
                            else
                                r1.Width = dgvLastWidth;                //若已滿版，以 dataGridView 剩餘寬度為主

                            index++;
                            dGroupCount -= 1;
                        }

                        r1.Height = defaultColumnHeadersHeight / 2 - 2;  //基本表頭分上下半、扣除上下邊界線
                        r1.Width -= 2;  //考慮左右邊界線
                        r1.X += 1;      //考慮左邊界線
                        r1.Y = 1;       //考慮上邊界線

                        using (format = new StringFormat())
                        {
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Center;

                            e.Graphics.FillRectangle(back, r1);
                            e.Graphics.DrawRectangle(p, r1);
                            e.Graphics.DrawString(
                                dsHeader.Tables[0].Rows[i][0].ToString(),
                                this.dataGridView1.ColumnHeadersDefaultCellStyle.Font,
                                fore,
                                r1,
                                format);
                        }

                        dgvLastWidth -= r1.Width + 2;   //剩餘版面扣除已繪製 Rectangle 寬度
                        dGroupCount = 1;                //重置合併欄計算
                    }
                    //其他狀況不繪製 Rectangle
                }
            }

            resize = false;
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            getFakeData();
            resize = true;
            generateColumns();
        }

        private void getFakeData()
        {
            /* Fake Data Presentation, Normally it should be a SQL Result.
             * Final Result should look like this:
             *                              A Merged Main Header                                  | Main Header | Long Main Header
             * ShortHeader | AReallyLongTitleHeader | ShortHeader2 | AnotherReallyLongTitleHeader |             |
             */
            dsHeader = new DataSet();

            DataTable dt = new DataTable();
            dt.Columns.Add("MainHeader", typeof(string));
            dt.Columns.Add("SubHeader", typeof(string));

            DataRow dr = dt.NewRow();
            dr[0] = "A Merged Main Header";
            dr[1] = "ShortHeader";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr[0] = "A Merged Main Header";
            dr[1] = "AReallyLongTitleHeader";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr[0] = "A Merged Main Header";
            dr[1] = "ShortHeader2";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr[0] = "A Merged Main Header";
            dr[1] = "AnotherReallyLongTitleHeader";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr[0] = "Main Header";
            dr[1] = "";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr[0] = "Long Main Header";
            dr[1] = "";
            dt.Rows.Add(dr);

            dsHeader.Tables.Add(dt);
            /*************************************************************/
            dsDetail = new DataSet();
            dt = new DataTable();
            dt.Columns.Add("0", typeof(string));
            dt.Columns.Add("1", typeof(string));
            dt.Columns.Add("2", typeof(string));
            dt.Columns.Add("3", typeof(string));
            dt.Columns.Add("4", typeof(string));
            dt.Columns.Add("5", typeof(string));
            dr = dt.NewRow();
            dr[0] = "0";
            dr[1] = "1";
            dr[2] = "2";
            dr[3] = "3";
            dr[4] = "4";
            dr[5] = "5";
            dt.Rows.Add(dr);
            dsDetail.Tables.Add(dt);

            return;
        }

        private void generateColumns()
        {
            this.dataGridView1.Rows.Clear();
            this.dataGridView1.Columns.Clear();


            DataGridViewTextBoxColumn col;
            for (int i = 0; i < dsHeader.Tables[0].Rows.Count; i++)
            {
                col = new DataGridViewTextBoxColumn()
                {
                    Name = i + "",
                    HeaderText = dsHeader.Tables[0].Rows[i][1].ToString(),
                    Width = 20,
                    FillWeight = 1
                };
                this.dataGridView1.Columns.Add(col);
            }

            string[] row = new string[dsDetail.Tables[0].Columns.Count];   //動態記錄每列資料
            for (int i = 0; i < dsDetail.Tables[0].Rows.Count; i++)
            {
                for (int j = 0; j < dsDetail.Tables[0].Columns.Count; j++)
                {
                    row[j] = dsDetail.Tables[0].Rows[i][j].ToString();
                }
                this.dataGridView1.Rows.Add(row);
            }

            /*this.dataGridView1.DataSource = dsDetail;
            this.dataGridView1.DataMember = dsDetail.Tables[0].ToString();*/

            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            this.dataGridView1.ColumnHeadersHeight = defaultColumnHeadersHeight;
            this.dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
        }
    }
}
