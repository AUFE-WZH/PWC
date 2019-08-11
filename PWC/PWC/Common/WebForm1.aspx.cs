using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace PWC.Common
{
    
    public partial class WebForm1 : System.Web.UI.Page
    {
        

        protected void Page_Load(object sender, EventArgs e)
        {
            string[][] s = new string[5][];

            s[0] = new string[2];
            s[1] = new string[3];
            s[2] = new string[3];
            s[3] = new string[3];
            s[4] = new string[3];

            s[0][0] = "3";
            s[0][1] = "3";

            
            s[1][0] = "id";
            s[1][1] = "name";
            s[1][2] = "age";

            
            s[2][0] = "1";
            s[3][0] = "2";
            s[4][0] = "3";

            
            s[2][1] = "11";
            s[3][1] = "22";
            s[4][1] = "33";

            
            s[2][2] = "111";
            s[3][2] = "222";
            s[4][2] = "333";

            string fileName = "G:myexcle.xlsx";

            Boolean flag = DataToExcel(s, fileName, false);
            if (flag)
            {
                MessageBox.Show("成功！");
            }
            else
            {
                MessageBox.Show("失败！");
            }
            
        }

        /// <summary>
        /// 将数据集中的数据保存到EXCEL文件
        /// </summary>
        /// <param name="dataSet">输入数据集</param>
        /// <param name="fileName">保存EXCEL文件的绝对路径名</param>
        /// <param name="isShowExcle">是否打开EXCEL文件</param>
        /// <returns></returns>
        public bool DataSetToExcel(DataSet dataSet, string fileName, bool isShowExcle)
        {
            
            DataTable dataTable = dataSet.Tables[0];
            int rowNumber = dataTable.Rows.Count;//不包括字段名
            int columnNumber = dataTable.Columns.Count;
            int colIndex = 0;

            if (rowNumber == 0)
            {
                MessageBox.Show("没有任何数据可以导入到Excel文件！");
                return false;
            }

            //建立Excel对象 
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            //excel.Application.Workbooks.Add(true);
            Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];
            excel.Visible = false;
            //Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)excel.Worksheets[1];
            Microsoft.Office.Interop.Excel.Range range;

            //生成字段名称 
            foreach (DataColumn col in dataTable.Columns)
            {
                colIndex++;
                excel.Cells[1, colIndex] = col.ColumnName;
            }

            object[,] objData = new object[rowNumber, columnNumber];

            for (int r = 0; r < rowNumber; r++)
            {
                for (int c = 0; c < columnNumber; c++)
                {
                    objData[r, c] = dataTable.Rows[r][c];
                }
                //Application.DoEvents();
            }

            // 写入Excel 
            range = worksheet.get_Range(excel.Cells[2, 1], excel.Cells[rowNumber + 1, columnNumber]);
            //range.NumberFormat = "@";//设置单元格为文本格式
            range.Value2 = objData;
            worksheet.get_Range(excel.Cells[2, 1], excel.Cells[rowNumber + 1, 1]).NumberFormat = "yyyy-m-d h:mm";

            //string fileName = path + "\\" + DateTime.Now.ToString().Replace(':', '_') + ".xls"; 
            workbook.SaveAs(fileName, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            try
            {
                workbook.Saved = true;
                excel.UserControl = false;
                //excelapp.Quit();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                workbook.Close(Microsoft.Office.Interop.Excel.XlSaveAction.xlSaveChanges, Missing.Value, Missing.Value);
                excel.Quit();
            }

            if (isShowExcle)
            {
                System.Diagnostics.Process.Start(fileName);
            }
            return true;
        }


        /// <summary>
        /// 将数据集中的数据保存到EXCEL文件
        /// </summary>
        /// <param name="s">输入数据集</param>
        /// <param name="fileName">保存EXCEL文件的绝对路径名</param>
        /// <param name="isShowExcle">是否打开EXCEL文件</param>
        /// <returns></returns>
        public bool DataToExcel(string[][] s, string fileName, bool isShowExcle)
        {

            int rowNumber = Convert.ToInt32(s[0][0]);//不包括字段名
            int columnNumber = Convert.ToInt32(s[0][1]);
            int colIndex = 0;

            if (rowNumber == 0)
            {
                MessageBox.Show("没有任何数据可以导入到Excel文件！");
                return false;
            }

            //建立Excel对象 
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            //excel.Application.Workbooks.Add(true);
            Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];
            excel.Visible = false;
            //Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)excel.Worksheets[1];
            Microsoft.Office.Interop.Excel.Range range;

            //生成字段名称 
            foreach (string ss in s[1])
            {
                colIndex++;
                excel.Cells[1, colIndex] = s[1][colIndex-1];
            }

            object[,] objData = new object[rowNumber, columnNumber];

            for (int r = 0; r < rowNumber; r++)
            {
                for (int c = 0; c < columnNumber; c++)
                {
                    objData[r, c] = s[r+2][c];
                }
                //Application.DoEvents();
            }

            // 写入Excel 
            range = worksheet.Range[excel.Cells[2, 1], excel.Cells[rowNumber + 1, columnNumber]];
            //range.NumberFormat = "@";//设置单元格为文本格式
            range.Value2 = objData;
            //worksheet.Range[excel.Cells[2, 1], excel.Cells[rowNumber + 1, 1]].NumberFormat = "yyyy-m-d h:mm";

            //string fileName = path + "\\" + DateTime.Now.ToString().Replace(':', '_') + ".xls"; 
            workbook.SaveAs(fileName, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);


            try
            {
                workbook.Saved = true;
                excel.UserControl = false;
                //excelapp.Quit();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                workbook.Close(Microsoft.Office.Interop.Excel.XlSaveAction.xlSaveChanges, Missing.Value, Missing.Value);
                excel.Quit();
            }

            if (isShowExcle)
            {
                System.Diagnostics.Process.Start(fileName);
            }

            return true;
        }


    }
}