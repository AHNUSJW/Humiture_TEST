using System;
using System.Collections.Generic;

namespace HTR 
{
    //EEPROM数据块使用
    public partial class SCT
    {
        //读取到的设备参数
        public Byte meRTC = 0;                 //为1表示更换电池重新上电后需要重设RTC时钟
        public Byte meHTT = 0;                 //为1表示更换电池重新上电后需要重设设备工作
        public Byte meWDT = 0;
        public Byte meLUK = 0;
        public Int16 meTMax = 0;              //最高温度
        public Int16 meTMin = 0;              //最低温度
        public UInt16 meBat = 0;              //电池电压
        public String meUID = "";             //硬件编号
        public String meHWVer = "";           //硬件版本
        public String meSWVer = "";           //程序版本

        //读取到的设备工作参数
        public DateTime meDateCalendar = DateTime.MinValue;   //设备日历信息
        public DateTime meDateCondense = DateTime.MinValue;   //去冷凝结束时间
        public DateTime meDateHW = DateTime.MinValue;         //硬件制造日期
        public DateTime meDateBat = DateTime.MinValue;        //更换电池日期
        public DateTime meDateDot = DateTime.MinValue;        //温度标定日期
        public DateTime meDateCal = DateTime.MinValue;        //计量校准日期
        public DateTime meDateWake = DateTime.MinValue;       //PC端计算好的醒来日期
        public DateTime meDateStart = DateTime.MinValue;      //测试起始日期
        public DateTime meDateEnd = DateTime.MinValue;        //测试结束日期
        public Byte condenseTime = 0x00;                      //去冷凝时长
        public UInt16 meSpan = 0;                             //测试间隔时间(单位：秒、分、时、天)
        public UInt16 meUnit = 0;                             //间隔时间单位
        public Boolean meScreenStatus;                        //屏幕状态：false 熄屏 True 不熄屏
        public UInt32 meDuration = 0;                         //测试持续时间(秒)
        public UInt32[] meArrUnit = { 1, 60, 3600, 86400};    //不同时间间隔单位对应的单位间隔时间{秒，分，时，天}

        public UInt16 meSelectDur = 2;                        //选择持续时间
        public UInt16 meDurUnit = 1;                          //持续时间单位

        //读取到的设备编号
        public DEVICE meType = DEVICE.HTT;                    //设备类型HTT、HTP、HTH、HTN、HTQ
        public String meName = "";                            //设备名称
        public String meJSN = "";                             //出厂编号
        public String meUSN = "";                             //用户定义的编号
        public String meUTXT = "";                            //用户备注信息
        public String meModel = "";                           //设备型号(YZLT30、YZLT150等)
        public String meRange = "";                           //测量范围(-80~150℃等)

        //加载文件的参数(计算得出)
        public Int32 meStartIdx = 0;                            //总数据开始的索引
        public Int32 meStopIdx = 0;                             //总数据结束的索引
        public DateTime meStartTime = DateTime.MinValue;        //总数据开始时间
        public DateTime meStopTime = DateTime.MinValue;         //总效数据结束时间
        public Int32 meSpanTime = 0;                            //测试间隔时间(单位：秒)
        public Int32 meValidStartIdx = 0;                       //有效数据开始的索引
        public Int32 meValidStopIdx = 0;                        //有效数据结束的索引
        public DateTime meValidStartTime = DateTime.MinValue;   //有效数据开始时间
        public DateTime meValidStopTime = DateTime.MinValue;    //有效数据结束时间
        public Double meTMPMax = 0;         //加载数据后计算的温度最大值(所有数据中的最大值)
        public Double meTMPMin = 0;         //加载数据后计算的温度最小值(所有数据中的最大值)
        public Double meHUMMax = 0;         //加载数据后计算的湿度最大值(所有数据中的最大值)
        public Double meHUMMin = 0;         //加载数据后计算的湿度最小值(所有数据中的最大值)
        public Double mePRSMax = 0;         //加载数据后计算的压力最大值(所有数据中的最大值)
        public Double mePRSMin = 0;         //加载数据后计算的压力最小值(所有数据中的最大值)

        //加载文件信息汇总
        public int meTMPNum = 0;                 //温度列数量
        public int meHUMNum = 0;                 //湿度列数量
        public int mePRSNum = 0;                 //压力列数量
        public int meDUTNum = 0;                 //产品总数
        public List<String> meJSNList = new List<String>(); //出厂编号列表
        public List<String> meJSNListAll = new List<String>(); //出厂编号列表
        public List<String> meJSNListPart = new List<String>(); //出厂编号列表
        public List<String> meTypeList = new List<String>(); //数据类型列表TT_T / TH_T / TH_H / TP_P
        public List<String> meAllList = new List<String>();
        public List<String> meTmpList = new List<String>();
        public List<String> meHumList = new List<String>();
        public List<String> mePrsList = new List<String>();
        public List<int> codeNumlist = new List<int>();    //数量编号列表，指明当前产品是第几个产品(HTH产品有两列，但两列是一个产品) -- 应该用不到

        //读取并保存测试数据后，以下变量更新为当前设备最新设置
        public TMP mtp = new TMP();
        public List<TMP> myMem = new List<TMP>();//导出TVS的flash
        public List<TMP> corMem = new List<TMP>();//修正TVS
        public TMP corMtp = new TMP();
      
        //加载文件数据后，以下变量更新为当前文件最新设置
        public List<TMP> myHom = new List<TMP>();//导入硬盘的数据
        public String homFileName;      //文件名
        public String homsave;          //保存时间
        public String homdate = DateTime.MinValue.ToString("yyyy-MM-dd"); //工作日期
        public String homstart = DateTime.MinValue.ToString("yy-MM-dd HH:mm:ss"); //工作起始时间
        public String homstop = DateTime.MinValue.ToString("yy-MM-dd HH:mm:ss"); //工作停止时间
        public String homspan = "0"; //测试间隔,秒
        public String homsamp = "0"; //采用条数
        public String homrun = "0"; //工作用时,秒
        public String hom_UID = "FFFFFFFF";   //从文件加载的设备ID
        public String hom_Model = "YZLT30"; //从文件加载的设备类型(YZLT30等)
        public String hom_JSN = ""; //从文件加载的设备编号(TT20210101等)
        public String hom_Range = "";//从文件加载的测量范围(-80~150℃等)
        public String hom_Cal = "00010101";   //从文件加载的设备校准日期
        public String hom_Rec = "00010101";   //从文件加载的设备复校日期
        public String hom_Bat = "0";   //从文件加载的设备电池电量
        public String homunit = "℃"; //数据单位℃/%RH/kPa(用于出报告)
        public String hom_USN = "";   //直接从设备里读到的管理编号
        public String hom_UTXT = "";   //直接从设备里读到的管理编号
        public String hom_Type = "HTT"; //直接从设备里读到的设备类型(HTT等)

        //标定点
        public Int32[] meTemp_CalPoints = new Int32[8];  //标定温度点
        public Int32[] meHum_CalPoints = new int[8];     //标定湿度点
        public Int32[] meADC_CalPoints = new Int32[8];   //标定ADC值
        public Int32[] meADC1_CalPoints = new Int32[8];  //标定ADC值（湿度）
        public float[] meV_Slope = new float[8];         //标定点斜率
        public float[] meV1_Slope = new float[8];        //标定点斜率（湿度）

        //数据修正
        public Int32[] meData_Cor = new Int32[32];      //数据修正
        public Int32 tempCount = 0; //温度修正条数
        public Int32 humCount = 0;  //湿度修正条数

        //
        public List<TMP> mySyn = new List<TMP>();//实时读出

    }
}

//end