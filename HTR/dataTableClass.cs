using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace HTR
{
    public class dataTableClass
    {
        public DataTable dataTable;

        public int Count
        {
            get { return dataTable.Rows.Count; }
        }


        public dataTableClass ()
        {
            dataTable = new DataTable();
        }

        /// <summary>
        /// 添加列（string类型）
        /// </summary>
        /// <param name="colName">列名称</param>
        /// <param name="isReadOnly">是否只读</param>
        public void addTableColumn(string colName)
        {
            DataColumn column;
            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = colName;
            //column.Unique = true;     //约束列为唯一，不能有重复值

            dataTable.Columns.Add(column);
        }

        /// <summary>
        /// 添加列（string类型）
        /// </summary>
        /// <param name="colName">列名称</param>
        /// <param name="isReadOnly">是否只读</param>
        public void addTableIntColumn(string colName)
        {
            DataColumn column;
            column = new DataColumn();
            column.DataType = typeof(Int32);
            column.ColumnName = colName;
            //column.Unique = true;     //约束列为唯一，不能有重复值

            dataTable.Columns.Add(column);
        }

        /// <summary>
        /// 插入列（string类型）
        /// </summary>
        /// <param name="colName">列名称</param>
        /// <param name="colIdx">设置列索引(在此索引处插入列)</param>
        public void addTableColumn(string colName, int colIdx = -1)
        {
            DataColumn column;
            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = colName;
            //column.SetOrdinal(colIdx);    //设置列索引
            //column.Unique = true;     //约束列为唯一，不能有重复值

            dataTable.Columns.Add(column);
            if (colIdx != -1) dataTable.Columns[colName].SetOrdinal(colIdx);    //设置列索引
        }

        /// <summary>
        /// 添加列
        /// </summary>
        /// <param name="colName">列名称</param>
        /// <param name="colType">列数据类型</param>
        /// <param name="isReadOnly">是否只读</param>
        public void addTableColumn(string colName, Type colType, Boolean isReadOnly = false)
        {
            DataColumn column;
            column = new DataColumn();
            column.DataType = typeof(String);
            column.ColumnName = colName;
            column.ReadOnly = isReadOnly;
            //column.Unique = true;     //约束列为唯一，不能有重复值

            dataTable.Columns.Add(column);
        }

        /// <summary>
        /// 添加列（string数组）
        /// </summary>
        /// <param name="colNames">列名称数组</param>
        /// <param name="isReadOnly">是否只读</param>
        public void addTableColumn(string[] colNames, Boolean isReadOnly = false)
        {
            foreach (string name in colNames)
            {
                DataColumn column;
                column = new DataColumn();
                column.DataType = typeof(String);
                column.ColumnName = name;
                column.ReadOnly = isReadOnly;
                //column.Unique = true;     //约束列为唯一，不能有重复值

                dataTable.Columns.Add(column);
            }
        }

        /// <summary>
        /// 设置列名
        /// </summary>
        /// <param name="mydt"></param>
        public void copyColumnName(DataTable mydt)
        {
            for (int i = 0; i < mydt.Columns.Count; i++) 
            {
                dataTable.Columns[i].ColumnName = mydt.Columns[i].ColumnName;
            }
        }

        /// <summary>
        /// 设置列名
        /// </summary>
        /// <param name="colIdx"></param>
        /// <param name="colName"></param>
        public void setColumnName(int colIdx, string colName)
        {
            dataTable.Columns[colIdx].ColumnName = colName;
        }

        /// <summary>
        /// 获取列名
        /// </summary>
        /// <param name="colIdx"></param>
        /// <param name="colName"></param>
        public string getColumnName(int colIdx)
        {
            return dataTable.Columns[colIdx].ColumnName;
        }

        /// <summary>
        /// 添加空行
        /// </summary>
        /// <returns></returns>
        public void AddTableRow()
        {
            DataRow row = dataTable.NewRow();
            dataTable.Rows.Add(row);
        }

        /// <summary>
        /// 添加n个空行
        /// </summary>
        /// <returns></returns>
        public void AddTableRow(int rownum)
        {
            for (int i = 0; i < rownum; i++)
            {
                DataRow row = dataTable.NewRow();
                dataTable.Rows.Add(row);
            }
        }

        /// <summary>
        /// 添加行
        /// </summary>
        /// <returns></returns>
        public void AddTableRow(string rowText)
        {
            DataRow row = dataTable.NewRow();
            if (row.ItemArray.Length > 0) row[0] = rowText;
            dataTable.Rows.Add(row);
        }

        /// <summary>
        /// 添加行
        /// </summary>
        /// <returns></returns>
        public void AddTableRow(string[] members)
        {
            dataTable.Rows.Add(members);
        }

        /// <summary>
        /// 添加行
        /// </summary>
        /// <param name="myRow"></param>
        public void AddTableRow0(string rowText)
        {
            DataRow row = dataTable.NewRow();
            for (int i = 0; i < row.ItemArray.Length; i++) 
            {
                row[i] = rowText + ",col" + i;
            }
        }

        /// <summary>
        /// 添加行：通过复制dt2表的某一行来创建新行
        /// </summary>
        /// <param name="myRow"></param>
        public void AddTableRow(DataRow myRow)
        {
            dataTable.Rows.Add(myRow.ItemArray);
        }

        /// <summary>
        /// 判断某行是否为空
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        public bool IsRowEmpty(int rowIdx)
        {
            bool IsNull = true;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (!string.IsNullOrEmpty(dataTable.Rows[rowIdx][i].ToString().Trim())) 
                {
                    IsNull = false;
                }
            }
            return IsNull;
        }

        /// <summary>
        /// 判断某行是否为空
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <param name="startCol">从哪一栏开始判断</param>
        public bool IsRowEmpty(int rowIdx, int startCol)
        {
            bool IsNull = true;
            for (int i = startCol; i < dataTable.Columns.Count; i++)
            {
                if (!string.IsNullOrEmpty(dataTable.Rows[rowIdx][i].ToString().Trim()))
                {
                    IsNull = false;
                }
            }
            return IsNull;
        }

        /// <summary>
        /// 判断某格是否为空
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <param name="startCol">列索引</param>
        public bool IsCellEmpty(int rowIdx, int cellIdx)
        {
            bool IsNull = true;
            if (!string.IsNullOrEmpty(dataTable.Rows[rowIdx][cellIdx].ToString().Trim()))
            {
                IsNull = false;
            }
            return IsNull;
        }

        /// <summary>
        /// 将table表某行连接成字符串
        /// </summary>
        /// <returns></returns>
        public string joinToString(int rownum)
        {
            string mystr = string.Empty;
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                if (dataTable.Rows[rownum][j] is DBNull)
                {
                    mystr += " " + ",";         //必须带一个空格，否则后面mystr.Trim(',')会把","都去掉
                }
                else
                {
                    mystr += dataTable.Rows[rownum][j].ToString() + ",";
                }
            }
            mystr = mystr.Trim(',') + Environment.NewLine;

            return mystr;
        }

        /// <summary>
        /// 将整个table表连接成字符串
        /// </summary>
        /// <returns></returns>
        public string joinToString()
        {
            string mystr = string.Empty;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                mystr += dataTable.Columns[i].ColumnName + ",";
            }
            mystr = mystr.Trim(',') + Environment.NewLine;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    if (dataTable.Rows[i][j] is DBNull)
                    {
                        mystr += " " + ",";         //必须带一个空格，否则后面mystr.Trim(',')会把","都去掉
                    }
                    else
                    {
                        mystr += dataTable.Rows[i][j].ToString() + ",";
                    }
                }
                mystr = mystr.Trim(',') + Environment.NewLine;
            }

            return mystr;
        }

        /// <summary>
        /// 将整个table表连接成字符串
        /// </summary>
        /// <returns></returns>
        public string joinToString2()
        {
            string mystr = string.Empty;
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                mystr += dataTable.Columns[i].ColumnName + ",";
            }
            mystr = mystr.Trim(',') + Environment.NewLine;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                mystr += String.Join (",", GetRowArray(i));         //行内容里有空数据的时候会报错
                mystr = mystr.Trim(',') + Environment.NewLine;
            }

            return mystr;
        }

        /// <summary>
        /// 将整个table表连接成字符串
        /// </summary>
        /// <returns></returns>
        public List<String> joinToList()
        {
            string mystr = string.Empty;
            List<String> mylist = new List<string>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                mystr += dataTable.Columns[i].ColumnName + ",";
            }
            mylist.Add(mystr.Trim(','));

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                mystr = string.Empty;
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    if (dataTable.Rows[i][j] is DBNull)
                    {
                        mystr += " " + ",";         //必须带一个空格，否则后面mystr.Trim(',')会把","都去掉
                    }
                    else
                    {
                        mystr += dataTable.Rows[i][j].ToString() + ",";
                    }
                }
                mylist.Add(mystr.Trim(','));
            }

            return mylist;
        }

        /// <summary>
        /// 单元格取值
        /// </summary>
        /// <param name="rowIdx"></param>
        /// <param name="colIdx"></param>
        /// <returns></returns>
        public string GetCellValue(int rowIdx, int colIdx)
        {
            return dataTable.Rows[rowIdx][colIdx].ToString();
        }

        /// <summary>
        /// 单元格赋值
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <param name="colIdx">列索引</param>
        /// <param name="cellText">单元格内容</param>
        public void SetCellValue(int rowIdx, int colIdx, string cellText)
        {
            dataTable.Rows[rowIdx][colIdx] = cellText;
        }

        /// <summary>
        /// 单元格赋值
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <param name="colIdx">列名</param>
        /// <param name="cellText">单元格内容</param>
        public void SetCellValue(int rowIdx, string colname, string cellText)
        {
            dataTable.Rows[rowIdx][colname] = cellText;
        }

        /// <summary>
        /// 行取值
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <returns>返回行的字符串数组</returns>
        public string[] GetRowArray(int rowIdx)
        {
            //不能将对象数组object[]转换为字符串数组string[]，必须转换数组中的每个项目，因为必须检查每个项目是否可以转换。
            //可以使用 Cast方法:
            return dataTable.Rows[rowIdx].ItemArray.Cast<string>().ToArray();
        }

        /// <summary>
        /// 行取值
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <returns>返回行的字符串数组</returns>
        public string[] GetRowArray(int rowIdx, int startidx)
        {
            int rownum = dataTable.Rows.Count;
            int colnum = dataTable.Columns.Count;
            int len = colnum - startidx;
            if (rowIdx >= rownum) return null;
            if (startidx >= colnum) return null;

            String[] myarr = new String[len];
            for (int i = startidx; i < colnum; i++)
            {
                String myval = dataTable.Rows[rowIdx][i].ToString();
                myarr[ i- startidx] = myval;
            }

            return myarr;
        }

        /// <summary>
        /// 列取值
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>返回行的字符串数组</returns>
        public string[] GetColumnArray(string colName)
        {
            string[] colArr = dataTable.AsEnumerable().Select(c => c.Field<string>(colName)).ToArray();
            return colArr;
        }

        /// <summary>
        /// 获得列中的最大值
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>返回列最大值</returns>
        public double GetColumnMaxVal(string colName)
        {
            double max = dataTable.AsEnumerable().Max(s => Convert.ToDouble(s.Field<string>(colName)));
            return max;
        }

        /// <summary>
        /// 获得列中的最大值
        /// </summary>
        /// <param name="colIdx">列索引</param>
        /// <returns>返回列最大值</returns>
        public double GetColumnMaxVal(int colIdx)
        {
            double myVal = 0;
            double max = double.MinValue;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][colIdx].ToString() == "") continue;
                //myVal = Convert.ToDouble(dataTable.Rows[i][colIdx]);
                string mystr = dataTable.Rows[i][colIdx].ToString();
                Double.TryParse(mystr, out myVal);
                if (max < myVal) max = myVal;
            }

            return max;
        }

        /// <summary>
        /// 获得行中的最大值
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <param name="colIdx">开始列的索引</param>
        /// <returns>返回行最大值和最大值所在列(以逗号分隔)</returns>
        public double GetRowMaxVal(int rowIdx, int colIdx)
        {
            double myVal = 0;
            int maxIdx = colIdx;        //最大值所在列
            double max = double.MinValue;

            for (int i = colIdx; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Rows[rowIdx][i].ToString() == "") continue;
                //myVal = Convert.ToDouble(dataTable.Rows[rowIdx][i]);
                string mystr = dataTable.Rows[rowIdx][i].ToString();
                Double.TryParse(mystr, out myVal);
                if (max < myVal)
                {
                    max = myVal;
                    maxIdx = i;     //最大值所在行的索引
                }
            }

            return max;
        }

        /// <summary>
        /// 获得行中的最小值
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <param name="colIdx">开始列的索引</param>
        /// <returns>返回行最小值和最小值所在列(以逗号分隔)</returns>
        public double GetRowMinVal(int rowIdx, int colIdx)
        {
            double myVal = 0;
            int minIdx = colIdx;        //最小值所在列
            double min = double.MaxValue;

            for (int i = colIdx; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Rows[rowIdx][i].ToString() == "") continue;
                //myVal = Convert.ToDouble(dataTable.Rows[rowIdx][i]);
                string mystr = dataTable.Rows[rowIdx][i].ToString();
                Double.TryParse(mystr, out myVal);
                if (min > myVal)
                {
                    min = myVal;        
                    minIdx = i;         //最小值所在行的索引
                }
            }

            return min;
        }

        /// <summary>
        /// 获得行中的平均值
        /// </summary>
        /// <param name="colName">行索引</param>
        /// <param name="colIdx">开始列的索引</param>
        /// <returns>返回行平均值</returns>
        public double GetRowAvrVal(int rowIdx, int colIdx)
        {
            int num = 0;
            double myVal = 0;
            double myAvr = 0;
            for (int i = colIdx; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Rows[rowIdx][i].ToString() == "") continue;

                num++;
                //myVal = Convert.ToDouble(dataTable.Rows[rowIdx][i]);
                string mystr = dataTable.Rows[rowIdx][i].ToString();
                Double.TryParse(mystr, out myVal);
                myAvr += myVal;
            }
            
            if (num > 0) myAvr = myAvr / num;

            return myAvr;
        }

        /// <summary>
        /// 获得列中的最小值
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>返回列最小值</returns>
        public double GetColumnMinVal(string colName)
        {
            double min = dataTable.AsEnumerable().Min(s => Convert.ToDouble(s.Field<string>(colName)));
            return min;
        }

        /// <summary>
        /// 获得列中的最小值
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>返回列最小值</returns>
        public double GetColumnMinVal(int colIdx)
        {
            double myVal = 0;
            double min = double.MaxValue;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][colIdx].ToString() == "") continue;
                //myVal = Convert.ToDouble(dataTable.Rows[i][colIdx]);
                string mystr = dataTable.Rows[i][colIdx].ToString();
                Double.TryParse(mystr, out myVal);
                if (min > myVal) min = myVal;
            }

            return min;
        }

        /// <summary>
        /// 获得列中所有值的和
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>返回列平均值</returns>
        public double GetColumnSumVal(int colIdx)
        {
            double myVal = 0;
            double mySum = 0;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][colIdx].ToString() == "") continue;      //出现空值，忽略
                //mySum += Convert.ToDouble(dataTable.Rows[i][colIdx]);
                string mystr = dataTable.Rows[i][colIdx].ToString();
                Double.TryParse(mystr, out myVal);
                mySum += myVal;
            }


            return mySum;
        }


        /// <summary>
        /// 获得列中的平均值
        /// </summary>
        /// <param name="colName">列名</param>
        /// <returns>返回列平均值</returns>
        public double GetColumnAvrVal(int colIdx)
        {
            int num = 0;
            double myVal = 0;
            double myAvr = 0;

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][colIdx].ToString() == "") continue;      //出现空值，退出循环

                num++;

                //myAvr += Convert.ToDouble(dataTable.Rows[i][colIdx]);
                string mystr = dataTable.Rows[i][colIdx].ToString();
                Double.TryParse(mystr, out myVal);
                myAvr += myVal;
            }

            if (num > 0) myAvr = myAvr/num;

            return myAvr;
        }

        /// <summary>
        /// 行赋值
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <param name="rowArray">行内容数组</param>
        public void SetRowArray(int rowIdx, string[] rowArray)
        {
            dataTable.Rows[rowIdx].ItemArray = rowArray;
        }

        /// <summary>
        /// 行赋值
        /// </summary>
        /// <param name="rowIdx">行索引</param>
        /// <param name="colIdx">列开始索引</param>
        /// <param name="rowArray">行内容数组</param>
        public void SetRowArray(int rowIdx, int colIdx, string[] rowArray)
        {
            for (int icol = colIdx; icol < rowArray.Length; icol++)
            {
                dataTable.Rows[rowIdx][icol] = rowArray[icol];
            }
        }

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="rowIdx"></param>
        public void DeleteTableColumn(int colIdx)
        {
            dataTable.Columns.RemoveAt(colIdx);
        }

        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="rowIdx"></param>
        public void DeleteTableColumn(string colName)
        {
            dataTable.Columns.Remove(colName);
        }

        /// <summary>
        /// 删除行
        /// </summary>
        /// <param name="rowIdx"></param>
        public void DeleteTableRow(int rowIdx)
        {
            dataTable.Rows.RemoveAt(rowIdx);
        }

        //复制DataTable
        public DataTable CopyTable()
        {
            return dataTable.Copy();
        }

        public void AddTable(DataTable dataTableClass)
        {
            dataTable.Merge(dataTableClass);
        }

        //复制DataTable的部分行
        public DataTable CopyTable(int rowidx1,int rowidx2)
        {
            DataTable mydt = dataTable.Copy();
            mydt.Rows.Clear();                      //清除所有行

            for (int irow = 0; irow < dataTable.Rows.Count; irow++)
            {
                if (irow >= rowidx1 && irow <= rowidx2) mydt.Rows.Add(dataTable.Rows[irow].ItemArray);
            }

            return mydt;
        }

        //清空表中的数据(删除所有行)
        public void ClearTableData()
        {
            dataTable.Rows.Clear();
        }

        /// <summary>
        /// 清空列数据(列依然存在，只是清空其中的数据)
        /// </summary>
        /// <param name="col">列索引</param>
        public void ClearTableColumn(int col)
        {
            for (int i = 0; i < dataTable.Rows.Count; i++) 
            {
                SetCellValue(i, col, "");
            }
        }

        public void SelectRow()
        {
            //选择"列1"为空的行

            DataRow[] dr = dataTable.Select("列1=null");

            //选择列1 为5 的行的集合

            DataRow[] dr1 = dataTable.Select("列1=5");

            //选择列1包含'李"的行的集合

            DataRow[] dt2 = dataTable.Select("列1 like '李'");
        }

        private void Temp()
        {
            /*
            //创建一个空表
            DataTable dt = new DataTable();
            //创建一个名为"Table_New"的空表
            DataTable dt = new DataTable("Table_New");


            //1.创建空列
            DataColumn dc = new DataColumn();
            dt.Columns.Add(dc);
            //2.创建带列名和类型名的列(两种方式任选其一)
            dt.Columns.Add("column0", System.Type.GetType("System.String"));
            dt.Columns.Add("column0", typeof(String));
            //3.通过列架构添加列
            DataColumn dc = new DataColumn("column1", System.Type.GetType("System.DateTime"));
            DataColumn dc = new DataColumn("column1", typeof(DateTime));
            dt.Columns.Add(dc);


            //1.创建空行
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);
            //2.创建空行
            dt.Rows.Add();
            //3.通过行框架创建并赋值
            dt.Rows.Add("张三", DateTime.Now);//Add里面参数的数据顺序要和dt中的列的顺序对应 //4.通过复制dt2表的某一行来创建dt.Rows.Add(dt2.Rows[i].ItemArray);


            //新建行的赋值
            DataRow dr = dt.NewRow();
            dr[0] = "张三";//通过索引赋值
            dr["column1"] = DateTime.Now; //通过名称赋值
                                          //对表已有行进行赋值
            dt.Rows[0][0] = "张三"; //通过索引赋值
            dt.Rows[0]["column1"] = DateTime.Now;//通过名称赋值
                                                 //取值
            string name = dt.Rows[0][0].ToString();
            string time = dt.Rows[0]["column1"].ToString();


            //选择column1列值为空的行的集合
            DataRow[] drs = dt.Select("column1 is null");
            //选择column0列值为"李四"的行的集合
            DataRow[] drs = dt.Select("column0 = '李四'");
            //筛选column0列值中有"张"的行的集合(模糊查询)
            DataRow[] drs = dt.Select("column0 like '张%'");//如果的多条件筛选，可以加 and 或 or
                                                           //筛选column0列值中有"张"的行的集合并按column1降序排序
            DataRow[] drs = dt.Select("column0 like '张%'", "column1 DESC");


            //使用DataTable.Rows.Remove(DataRow)方法
            dt.Rows.Remove(dt.Rows[0]);
            //使用DataTable.Rows.RemoveAt(index)方法
            dt.Rows.RemoveAt(0);
            //使用DataRow.Delete()方法
            dt.Row[0].Delete();
            dt.AcceptChanges();

            //-----区别和注意点-----
            //Remove()和RemoveAt()方法是直接删除
            //Delete()方法只是将该行标记为deleted，但是还存在，还可DataTable.RejectChanges()回滚，使该行取消删除。
            //用Rows.Count来获取行数时，还是删除之前的行数，需要使用DataTable.AcceptChanges()方法来提交修改。
            //如果要删除DataTable中的多行，应该采用倒序循环DataTable.Rows，而且不能用foreach进行循环删除，因为正序删除时索引会发生变化，程式发生异常，很难预料后果。
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                dt.Rows.RemoveAt(i);
            }

            //复制表，同时复制了表结构和表中的数据
            DataTable dtNew = new DataTable();
            dtNew = dt.Copy();
            //复制表
            DataTable dtNew = dt.Copy();  //复制dt表数据结构
            dtNew.Clear()  //清空数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (条件语句)
                {
                    dtNew.Rows.Add(dt.Rows[i].ItemArray);  //添加数据行
                }
            }
            //克隆表，只是复制了表结构，不包括数据
            DataTable dtNew = new DataTable();
            dtNew = dt.Clone();
            //如果只需要某个表中的某一行
            DataTable dtNew = new DataTable();
            dtNew = dt.Copy();
            dtNew.Rows.Clear();//清空表数据
            dtNew.ImportRow(dt.Rows[0]);//这是加入的是第一行


            DataTable dt = new DataTable();//创建表
            dt.Columns.Add("ID", typeof(Int32));//添加列
            dt.Columns.Add("Name", typeof(String));
            dt.Columns.Add("Age", typeof(Int32));
            dt.Rows.Add(new object[] { 1, "张三", 20 });//添加行
            dt.Rows.Add(new object[] { 2, "李四", 25 });
            dt.Rows.Add(new object[] { 3, "王五", 30 });
            DataView dv = dt.DefaultView;//获取表视图
            dv.Sort = "ID DESC";//按照ID倒序排序
            dv.ToTable();//转为表

            */
        }

    }
}
