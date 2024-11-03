using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace HTR
{
    class HeaderFooterEvent : PdfPageEventHelper, IPdfPageEvent
    {

        #region 静态字段

        private Font mMyFont;               //页眉页脚字体(字体名称，字体尺寸，字体样式)
        private uint mMyFirstPage = 1;       //定义从哪一页开始添加页眉页脚
        private int mMySignMode = 1;         //定义页脚签字方式：首末页签字、全部签字
        public PdfTemplate tpl = null;       //总页数模板

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="myfont">页眉页脚字体</param>
        /// <param name="firstPage">开始添加页眉页脚的页数</param>
        /// <param name="signMode">1=全部签字；2=首末页签字</param>
        public HeaderFooterEvent(Font myfont, uint firstPage = 1, int signMode = 1)
        {
            mMyFont = myfont;

            if (firstPage <= 0) firstPage = 1;
            mMyFirstPage = firstPage;                //定义开始添加页眉页脚的页数，默认从第一页开始添加
            mMySignMode = signMode;
        }

        #endregion

        #region 生成页眉页脚

        /// <summary>
        /// 新页面创建完成但写入内容之前触发
        /// </summary>
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            string mylogo = MyDefine.myXET.logopath;
            //mylogo = @"D:\原E盘\20210628\一期\界面设计\提交数据\1-界面1-LOGO.png";
            int colwidth = mylogo == "" ? 1 : 8;

            //页眉设置
            PdfPCell cell;
            PdfPTable headerTable = new PdfPTable(4);
            headerTable.TotalWidth = PageSize.A4.Width - 80;                //设置表格总宽
            headerTable.SetWidths(new int[] { colwidth, 50, 50, 10 });      //设置列宽比例
            headerTable.LockedWidth = true;                                 //锁定表格宽度
            headerTable.DefaultCell.FixedHeight = -10;                      //设置单元格高度
            headerTable.DefaultCell.Border = Rectangle.BOTTOM_BORDER;       //设置单元格边框
            headerTable.DefaultCell.BorderWidth = 0.5f;                     //设置单元格线宽
            headerTable.DefaultCell.BorderColor = BaseColor.GRAY;           //设置边框颜色

            if (mylogo == "") headerTable.AddCell(new Paragraph("", mMyFont)); //无logo
            if (mylogo != "") headerTable.AddCell(CreateImage(mylogo, 5, 5));  //添加logo
            //headerTable.AddCell(new Paragraph("报告编号：" + MyDefine.myXET.repcode, mMyFont));
            //headerTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            //headerTable.AddCell(new Paragraph("第 " + (writer.PageNumber) + " 页", mMyFont));

            //报告编号
            cell = new PdfPCell(new Paragraph("报告编号：" + MyDefine.myXET.repcode, mMyFont));
            cell.VerticalAlignment = Element.ALIGN_BOTTOM;                  //下对齐
            cell.HorizontalAlignment = Element.ALIGN_LEFT;                  //左对齐
            cell.Border = Rectangle.BOTTOM_BORDER;
            cell.BorderColor = BaseColor.GRAY;
            headerTable.AddCell(cell);

            //当前页数
            cell = new PdfPCell(new Paragraph("第 " + (writer.PageNumber) + " 页", mMyFont));
            cell.VerticalAlignment = Element.ALIGN_BOTTOM;                  //下对齐
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;                 //右对齐
            cell.Border = Rectangle.BOTTOM_BORDER;
            cell.BorderColor = BaseColor.GRAY;
            headerTable.AddCell(cell);

            //总页数
            cell = new PdfPCell(Image.GetInstance(tpl));
            cell.VerticalAlignment = Element.ALIGN_BOTTOM;                  //下对齐
            cell.Border = Rectangle.BOTTOM_BORDER;
            cell.BorderColor = BaseColor.GRAY;
            headerTable.AddCell(cell);

            //写入页眉 -- 写到指定位置
            headerTable.WriteSelectedRows(0, -1, 40, PageSize.A4.Height - 20, writer.DirectContent);      //写入页眉(位置在上面)

            if (writer.PageNumber < mMyFirstPage && !(mMySignMode == 3) && !(mMySignMode == 4)) return;    //mMyFirstPage页数之前不添加页脚

            //页脚设置
            PdfPTable footerTable = new PdfPTable(3);
            footerTable.TotalWidth = PageSize.A4.Width - 80;            //设置表格总宽
            footerTable.SetWidths(new int[] { 12, 10, 10 });            //设置列宽比例
            footerTable.LockedWidth = true;                             //锁定表格宽度
            footerTable.DefaultCell.FixedHeight = -10;                  //设置单元格高度
            footerTable.DefaultCell.Border = Rectangle.NO_BORDER;       //设置单元格边框
            footerTable.DefaultCell.BorderWidth = 0.5f;                 //设置单元格线宽
            footerTable.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY; //设置边框颜色

            if (mMySignMode == 1 || mMySignMode == 3)                                       //全部签字
            {
                footerTable.AddCell(new Paragraph("晶度技术（北京）有限公司", mMyFont));
                footerTable.AddCell(new Paragraph("验证人：" + MyDefine.myXET.calibMem, mMyFont));
                footerTable.AddCell(new Paragraph("复核人：" + MyDefine.myXET.reviewMem, mMyFont));
                footerTable.AddCell(new Paragraph(MyDefine.myXET.compyPhone, mMyFont));
                footerTable.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, mMyFont));
                footerTable.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, mMyFont));
            }
            else if (mMySignMode == 2 && writer.PageNumber == 2)        //首末页签字
            {
                footerTable.AddCell(new Paragraph("晶度技术（北京）有限公司", mMyFont));
                footerTable.AddCell(new Paragraph("验证人：" + MyDefine.myXET.calibMem, mMyFont));
                footerTable.AddCell(new Paragraph("复核人：" + MyDefine.myXET.reviewMem, mMyFont));
                footerTable.AddCell(new Paragraph(MyDefine.myXET.compyPhone, mMyFont));
                footerTable.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, mMyFont));
                footerTable.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, mMyFont));
            }
            else if (mMySignMode == 2 && writer.PageNumber != 2)        //首末页签字时，中间页不签字并保留页脚其他信息
            {
                footerTable.AddCell(new Paragraph("晶度技术（北京）有限公司", mMyFont));
                footerTable.AddCell(new Paragraph("  ", mMyFont));
                footerTable.AddCell(new Paragraph("  ", mMyFont));
                footerTable.AddCell(new Paragraph(MyDefine.myXET.compyPhone, mMyFont));
                footerTable.AddCell(new Paragraph("  ", mMyFont));
                footerTable.AddCell(new Paragraph("  ", mMyFont));
            }
            else if (mMySignMode == 4 && writer.PageNumber == 1)       //首末页签字时,报告第二部分首页签字
            {
                footerTable.AddCell(new Paragraph("晶度技术（北京）有限公司", mMyFont));
                footerTable.AddCell(new Paragraph("验证人：" + MyDefine.myXET.calibMem, mMyFont));
                footerTable.AddCell(new Paragraph("复核人：" + MyDefine.myXET.reviewMem, mMyFont));
                footerTable.AddCell(new Paragraph(MyDefine.myXET.compyPhone, mMyFont));
                footerTable.AddCell(new Paragraph("验证日期：" + MyDefine.myXET.calibDate, mMyFont));
                footerTable.AddCell(new Paragraph("复核日期：" + MyDefine.myXET.reviewDate, mMyFont));
            }
            else if (mMySignMode == 4 && writer.PageNumber != 1)       //首末页签字时,，中间页不签字并保留页脚其他信息
            {
                footerTable.AddCell(new Paragraph("晶度技术（北京）有限公司", mMyFont));
                footerTable.AddCell(new Paragraph("  ", mMyFont));
                footerTable.AddCell(new Paragraph("  ", mMyFont));
                footerTable.AddCell(new Paragraph(MyDefine.myXET.compyPhone, mMyFont));
                footerTable.AddCell(new Paragraph("  ", mMyFont));
                footerTable.AddCell(new Paragraph("  ", mMyFont));
            }

            footerTable.DefaultCell.Border = Rectangle.NO_BORDER;      //设置单元格边框
            footerTable.AddCell(new Paragraph("软件版本号：" + Constants.SW_Version, mMyFont));
            footerTable.AddCell(new Paragraph("报告打印人：" + MyDefine.myXET.operaMem, mMyFont));
            footerTable.AddCell(new Paragraph("输出时间：" + MyDefine.myXET.createPdfTime, mMyFont));

            //写入页脚 -- 写到指定位置
            footerTable.WriteSelectedRows(0, -1, 40, 60, writer.DirectContent);                           //写入页脚(位置在下面)
        }

        /// <summary>
        /// 打开一个新页面时发生 
        /// </summary>
        public override void OnStartPage(PdfWriter writer, Document document)
        {
            writer.PageCount = writer.PageNumber - 1;
        }

        /// <summary>
        /// 打开文档时，创建一个总页数的模板
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="document"></param>
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            PdfContentByte cb = writer.DirectContent;
            tpl = cb.CreateTemplate(100, 16);            //模版 显示总共页数(调节模版显示的位置)
        }

        /// <summary>
        /// 关闭PDF文档时发生该事件，将总页数写入各页面
        /// </summary>
        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            string pageNum = " 共 " + (writer.PageNumber).ToString() + " 页";
            ColumnText.ShowTextAligned(tpl, Element.ALIGN_LEFT, new Paragraph(pageNum, mMyFont), 1, 2, 0);  //将总页数显示在template的固定位置
        }

        #endregion

        private iTextSharp.text.Image CreateImage(string path, int imgWidth, int imgHeight)
        {
            if (File.Exists(path))
            {
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(path);
                img.ScaleAbsolute(imgWidth, imgHeight);

                return img;
            }

            return null;
        }

    }
}
