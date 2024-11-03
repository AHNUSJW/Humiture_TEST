using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace HTR
{
    //用户配置使用
    public partial class XET : SCT
    {
        //User function
        public String userName;
        public String userPassword;
        public String userCFG;
        public String userDAT;
        public String userOUT;
        public String userSAV;
        public String userLOG;
        public String userPIC;
        public String userHEL;
        public String userCFGPATH;
        public String reportPicName;

        //Serial Communication
        public SerialPort mePort = new SerialPort();//串口
        public TASKS meTask = TASKS.disconnected;   //任务状态机
        public Byte[] meTXD = new Byte[SZ.TxSize];  //发送缓冲区
        public Boolean meAutoReceive = true;        //自动接收回复

        public String temUnit = "℃";                 //温度单位时间

        //当前正在通信的设备的地址
        public Byte meActiveAddr = 0x30;            //当前激活产品的地址
        public Int32 meActiveIdx = 0;               //当前正在通讯的设备索引
        public String meActiveJsn = "Blank";        //当前激活产品的出厂编号
        public List<Byte> meDUTAddrArr = new List<Byte>();         //用于保存已连接产品的地址
        public List<String> meDUTJsnArr = new List<String>();         //用于保存已连接产品的出场编号

        public String meTips = null;                //通讯日志
        //public StringBuilder meTips = new StringBuilder(1000);      //通讯日志
        public String meComputer = null;            //电脑名+MAC地址
        public Boolean meFirstByte = false;         //是否接收到第一个字节
        public Boolean meDebugMode = false;         //程序是否为调试模式，是，则显示所有调试框

        //MenuDataPanel&MenuReportPanel
        public Boolean meDataCurveUpdating = false;  //重新加载了文件, 需要更新原始数据曲线信息
        public Boolean meCalCurveUpdating = false;  //重新加载了文件, 需要更新校准、标定曲线信息
        public Boolean meCalTableUpdating = false;  //重新加载了文件, 需要更新校准、标定表
        public dataTableClass meDataTbl;       //测试数据加载数据表
        public dataTableClass meInfoTbl;       //记录数据表参数信息(开始时间、结束时间等)
        public dataTableClass meInfoAllTbl;       //记录数据表参数信息(开始时间、结束时间等)
        public dataTableClass meInfoTmpTbl;       //记录数据表参数信息(开始时间、结束时间等)
        public dataTableClass meInfoHumTbl;       //记录数据表参数信息(开始时间、结束时间等)
        public dataTableClass meInfoPrsTbl;       //记录数据表参数信息(开始时间、结束时间等)
        public dataTableClass meDataAllTbl;    //测试数据加载数据表--全部数据
        public dataTableClass meDataTmpTbl;    //测试数据加载数据表--计算后的温度表
        public dataTableClass meDataHumTbl;    //测试数据加载数据表--计算后的湿度表
        public dataTableClass meDataPrsTbl;    //测试数据加载数据表--计算后的压力表
        public dataTableClass meDataTblAll;   //测试数据加载数据表--全部数据
        public dataTableClass meDataTblTmp;    //测试数据加载数据表--温度表
        public dataTableClass meDataTblHum;    //测试数据加载数据表--湿度表
        public dataTableClass meDataTblPrs;    //测试数据加载数据表--压力表

        public int meActivePn = 0;              //当前正在编辑的阶段Pn
        public int meValidStageNum = 0;        //有效区域数量(不包含没有设置的或只设置了开始或结束的)
        public List<int> meValidIdxList = new List<int>();     //有效开始、有效结束索引列表
        public List<DateTime> meValidTimeList = new List<DateTime>();    //有效开始、有效结束时间列表
        public List<String> meValidNameList = new List<string>();   //设置有效区域的名称
        public List<Double> meValidSetValueList = new List<double>();//设置有效区域的设定值
        public List<Double> meValidUpperList = new List<double>();  //设置有效区域的上限值
        public List<Double> meValidLowerList = new List<double>();  //设置有效区域的下限值
        public List<Double> meTmpSETList = new List<Double>();     //温度设定值列表（P1-P8）
        public List<Double> meHumSETList = new List<Double>();     //湿度设定值列表（P1-P8）
        public List<Double> mePrsSETList = new List<Double>();     //压力设定值列表（P1-P8）
        public List<Double> meLeftMaxMinList = new List<Double>();     //左轴有效区域最大最小值列表（P1-P8）
        public List<Double> meRightMaxMinList = new List<Double>();     //右轴有效区域最大最小值列表（P1-P8）

        public Boolean[] CustomAxes = new Boolean[2];   //是否自定义左右轴
        public Double[] leftLimit = new Double[2];      //左轴自定义上下限
        public Double[] rightLimit = new Double[2];     //右轴自定义上下限
        public double[] recordMaxMin = new Double[4];   //记录数据（温度）最大最小值

        public Boolean drawTemCurve = false;     //是否画温度曲线
        public Boolean drawHumCurve = false;     //是否画湿度曲线
        public Boolean drawPrsCurve = false;    //是否画压力曲线

        //MenuTracePanel
        public String meInterface = "连接界面";      //记录当前所在的界面，方便添加操作日志
        public StringBuilder meRecords = new StringBuilder(1000);      //操作日志
        public dataTableClass meTraceTbl;       //审计追踪日志总表
        public dataTableClass meTraceShowTbl;   //显示在界面上的审计追踪日志表


        //MenuPDFViewPanel
        public dataTableClass mePDFTbl;       //PDF报告总表
        public dataTableClass mePDFShowTbl;   //显示在界面上的PDF报告列表


        //MenuCalPanel0
        public dataTableClass meTblRev;        //后校验表
        public dataTableClass meTblBlank1;     //空数据表(手动输入表)

        //MenuStdPanel
        public dataTableClass meTblSTD;         //标准器信息表

        //MenuCalPanel
        public Int32 meDotNum;                 //需要标定的点数
        public dataTableClass meTblCal1;       //标定数据表1(手动输入表)
        public dataTableClass meTblCal2;       //标定数据表2(自动检索表)
        public dataTableClass meTblPre1;       //前校准表1(手动输入表)
        public dataTableClass meTblPre2;       //前校准表1(自动检索表)
        public dataTableClass meTblCalCurve;   //标定曲线数据表
        public dataTableClass meTblPreCurve;   //校准曲线数据表
        public Boolean meDotComplete = false;  //记录标定是否完成(是否要显示标定后曲线)

        //MenuCalCurvePanel
        public dataTableClass meTblCurve;      //曲线数据表
        public List<String> meCalTypeList = new List<String>(); //数据类型列表TT_T / TH_T / TH_H / TP_P；meCalTypeList用于校准、标定界面画曲线，相比meTypeList其增加了1条或2条标准数据列


        //MenuVerifyPanel
        public dataTableClass meTblVer;        //验证表(输出到界面及报告)
        public dataTableClass meTemVer;
        public dataTableClass meHumVer;
        public dataTableClass mePrsVer;
        public dataTableClass meTblVer1;       //验证报告-报告有效数据汇总表
        public dataTableClass meTemVer1;
        public dataTableClass meHumVer1;
        public dataTableClass mePrsVer1;
        public dataTableClass meTblVer2;       //验证报告-有效数据横向汇总表之温度表
        public dataTableClass meTblVer3;       //验证报告-有效数据横向汇总表之湿度表
        public dataTableClass meTblVer4;       //验证报告-有效数据横向汇总表之压力表
        public dataTableClass meTblVer5;       //验证报告-有效数据纵向汇总表
        public dataTableClass meTemVer5;
        public dataTableClass meHumVer5;
        public dataTableClass mePrsVer5;
        public dataTableClass meTblVer6;       //验证报告-关键参数汇总
        public dataTableClass meTemVer6;
        public dataTableClass meHumVer6;
        public dataTableClass mePrsVer6;
        public dataTableClass meTblVer7;       //验证报告-F0值计算
        public dataTableClass meTblVer8;       //验证报告-报告有效数据汇总表
        public dataTableClass meTemVer8;
        public dataTableClass meHumVer8;
        public dataTableClass mePrsVer8;
        public List<int> meF0ValidList = new List<int>();     //F0计算时，用于统计计算出的有效开始、有效结束索引列表
        public Boolean isF0Checked = false;                   //是否选择计算f0

        //MenuReportPanel
        public string logopath = "";            //页眉上logo图片所在路径
        public string repcode = "";             //PDF报告编号 JDYZ + 年月日 + 流水号(001)
        public string operaMem = "";            //操作人员
        public string calibMem = "";            //校准人员
        public string calibDate = "";           //校准日期
        public string reviewMem = "";           //审核人员
        public string reviewDate = "";          //审核日期
        public string compyPhone = "400-101-0927 www.jdujs.com";          //公司电话
        public string createPdfTime = "";       //pdf创建时间


        //MenuAccountPanel&MenuPermissionPanel
        public String meLoginUser = "";                                  //当前已登录账号信息
        public String mePermission = "";                                 //存放当前账号的所有权限状态(1= 开放权限；0= 无权限)
        public List<String> meListUser = new List<String>();             //储存所有用户名信息
        public List<String> meListPermCat = new List<String>();          //储存所有权限类别信息

        //MenuDataImport
        public int processValue = 0;           //用于记录进度条进度

        //MenuSetPanel
        public Boolean meRemindCail = true;    //用于校准日期不再提醒选择(true= 提醒；false= 不再提醒)

        //=============================================
        //User PC Copyright
        public Int64 myMac = 0;
        public Int64 myVar = 0;
        public Byte myPC = 0;

        //User Event 定义事件
        public event freshHandler myUpdate;

    }
}

//end