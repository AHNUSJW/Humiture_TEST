using System;
using System.Collections.Generic;

namespace HTR
{
    //
    #region RS232 COM Setting
    //

    public enum BAUT : Int32
    {
        B4800 = 4800,
        B9600 = 9600,
        B14400 = 14400,
        B19200 = 19200,
        B38400 = 38400,
        B57600 = 57600,
        B115200 = 115200,
    }

    public enum INTERVAL : Byte //测试间隔时间
    {
        UNIT_SECONDS = 0, //秒
        UNIT_MINUTE = 1, //分
        UNIT_HOUR = 2, //时
        UNIT_DAY = 3, //天
    }

    public enum UNIT : Byte //测试间隔时间
    {
        秒 = 0, //秒
        分 = 1, //分
        时 = 2, //时
        天 = 3, //天
    }

    public enum UNITNAME : Byte //测试间隔时间
    {
        second = 0, //秒
        minute = 1, //分
        hour = 2, //时
        day = 3, //天
    }


    public enum PPRODUCT : Byte     //设备类型
    {
        无线温度验证探头 = 100,
        无线温湿度验证探头 = 101,
        无线压力验证探头 = 102,
        HTN = 103,
        HTQ = 104,
    }

    public enum DEVICE:Byte     //设备类型
    {
        HTT = 100,
        HTH = 101,
        HTP = 102,
        HTN = 103,
        HTQ = 104,
    }

    public enum ACCOUNT : int   //账号信息
    {
        USER = 0,               // 账号名称
        PSWD = 1,               // 密码
        DEPT = 2,               // 部门
        TELE = 3,               // 电话
        DEFUSR = 4,             // 是否默认账号(上次登录的账号自动显示在用户名中)
        REMPSD = 5,             // 是否记住密码
        CATCODE = 6,            // 权限类别编号
        CATNAME = 7,            // 权限类别名称
        CATLIST = 8,            // 权限类别列表
    }

    public enum STEP: Byte      //操作步骤(用户核对对应操作有无权限)
    {
        设备连接 = 0,
        设备设置 = 1,
        设备读取 = 2,
        管理编号 = 3,
        备注信息 = 4,

        数据载入 = 5,
        数据导出 = 6,
        数据曲线 = 7,
        数据修正 = 35,

        报告查看 = 8,
        报告导出 = 9,
        编辑PDF报告 = 34,

        标准器录入 = 10,
        数据检索 = 11,
        校准曲线 = 12,
        校准报告 = 13,

        验证报表 = 14,
        导出报表 = 15,
        查看曲线 = 16,
        验证报告 = 17,

        审计追踪 = 18,
        安全设置 = 19,
        出厂标定 = 20,

        新建账号 = 21,
        注销账号 = 22,
        密码查看 = 23,
        修改密码 = 24,
        权限类别变更 = 25,
        权限管理 = 26,

        设备地址 = 27,
        出厂编号 = 28,
        设备型号 = 29,
        测量范围 = 30,
        校准日期 = 31,
        公司电话 = 32,
        设备名称 = 33,
    }

    public enum INTERFACE : Byte     //界面名称(用于审计追踪)
    {
        进入界面 = 0,
        登录界面 = 1,
        选择界面 = 2,
        主界面 = 3,

        连接界面 = 4,
        编号管理界面 = 5,
        设备设置界面 = 6,

        数据处理界面 = 7,
        数据曲线界面 = 8,

        校准界面 = 9,
        标准器录入界面 = 10,
        校准曲线界面 = 11,
        校准报告界面 = 12,

        验证界面 = 13,
        验证曲线界面 = 14,
        验证报告界面 = 15,

        报告查看界面 = 16,

        系统设置界面 = 17,
        用户管理界面 = 18,
        账号切换界面 = 19,
        权限管理界面 = 20,

        审计追踪界面 = 21,
        软件版本界面 = 22,
        安全设置界面 = 23,

        出厂标定界面 = 24,
        标定曲线界面 = 25,

        出厂设置界面 = 26,
    }

    public enum RTCOM : Byte //通讯状态机
    {
        COM_NULL,
        COM_ERR_RST, //BLE复位
        COM_ERR_RPT, //读重传
        COM_ERR_MEM, //无Flash
        COM_SET_LOCK, //
        COM_SET_UNLOCK, //
        COM_SET_RESET, //
        COM_SET_SERI, //设置机器序列号
        COM_SET_DOT, //设置曲线标定点
        COM_SET_MEM, //写记录工作模式
        COM_SET_DEVICE, //设置设备地址
        COM_SET_CALENDAR, //设置机器系统时间
        COM_SET_JOBSET, //设置机器测试时间
        COM_SET_JOBREC, //清空测试数据
        COM_SET_JSN, //设置机器序列号
        COM_SET_USN, //设置机器序列号
        COM_SET_UTXT, //设置备注信息
        COM_READ_DEVICE, //读板子硬件编码、电量和温度
        COM_READ_CALENDAR, //读板子时钟信息
        COM_READ_TIME, //读板子硬件制造日期、更换电池日期、温度标定日期、计量校准日期
        COM_READ_JOBSET, //读用户设定信息：设备开始时间、结束时间、时间间隔等
        COM_READ_JOBREC, //读设备测试数据
        COM_READ_DOT, //读曲线标定点
        COM_READ_JSN, //读板子用户定义序列号
        COM_READ_USN, //读板子用户定义序列号
        COM_READ_UTXT, //读板子用户备注信息
        COM_READ_TMP, //读温度采集当前的结果
        COM_READ_MEM, //读温度采集记录的结果
        COM_READ_NAME,//读设备名称
        COM_READ_SCREEN,//读熄屏状态
    }

    public enum TASKS : Byte //任务状态机
    {
        disconnected = 0, //未连接
        setting = 1, //其它通讯操作
        run = 2, //空闲中
        reading = 3,    //设备读取中
        record = 4, //实时监控工作并且录制中
    }

    public enum TmpMode : Byte
    {
        NULL = 0,         //无
        immediately = 1,  //立即开始记录
        waite = 2,        //延时开始记录
        reach = 3,        //到达温度后开始记录
        threshold = 4,    //温度变化2度后开始记录
    }

    public enum DOT : byte
    {
        DOT0 = 0x80,
        DOT1 = 0x81,
        DOT2 = 0x82,
        DOT3 = 0x83,
        DOT4 = 0x84,
        DOT5 = 0x85,
        DOT6 = 0x86,
    }

    public enum RECT : Byte //任务状态机
    {
        NULL,
        up_45,
        up_50,
        up_90,
        dn_95,
        dn_90,
        dn_50,
    }

    //
    #endregion
    //

    public static class SZ
    {
        public const Byte REC = 64;
        public const Byte CHA = 16;

        public const Int16 RxSize = 2048;
        public const Int16 TxSize = 2048;

        public const Int16 TMAX = 12699;
        public const Int16 TMIN = 0;
    }

    public static class Constants
    {
        //软件版本
        public const String SW_Version = "JD23.12.14";

        public const Byte ERR_RST = 0x4E; //BLE复位
        public const Byte ERR_RPT = 0x4F; //读重传

        public const Byte SET_LOCK = 0x50; //
        public const Byte SET_UNLOCK = 0x51; //
        public const Byte SET_RESET = 0x52; //
        public const Byte SET_SERI = 0x53; //设置机器序列号
        public const Byte SET_DOT = 0x54; //设置曲线标定点
        public const Byte SET_MEM = 0x55; //写记录工作模式

        public const Byte READ_TVS = 0x56; //读板子ID和序列号
        public const Byte READ_PAR = 0x57; //读板子电量和温度
        public const Byte READ_DOT = 0x58; //读曲线标定点
        public const Byte READ_TMP = 0x59; //读温度采集当前的结果
        public const Byte READ_MEM = 0x5A; //读温度采集记录的结果

        //public const Byte START = 0x02; //起始符
        public const Byte STOP = 0x03; //结束符

        public const int TIMEOUT = 10;  //10*100ms = 1s
        public const int REPEAT = 10;   //通信失败最大重复次数

        //=============================================================
               
        public const Byte START = 0x30;     //通讯地址(起始符)
        public const Byte READ_REG = 0x03;  //读寄存器参数
        public const Byte READ_DATA = 0x06; //写地址后下载数据
        public const Byte SET_REG = 0x09;   //修改寄存器参数

        public const Byte REG_DEVICE = 0x50; //设备寄存器
        public const Byte REG_CALENDAR = 0x51; //日历寄存器
        public const Byte REG_TIME = 0x52; //时间寄存器
        public const Byte REG_BAT_CL = 0x53; //BAT_CL寄存器
        public const Byte REG_CTEMP_CL = 0x54; //CTEMP_CL寄存器
        public const Byte REG_DOT = 0x55; //DOT寄存器
        public const Byte REG_JSN = 0x56; //JSN寄存器
        public const Byte REG_USN = 0x57; //USN寄存器
        public const Byte REG_UTXT = 0x58; //UTXT寄存器
        public const Byte REG_NAME = 0x59;//存储名字
        public const Byte REG_JOBSET = 0x60; //JOBSET寄存器
        public const Byte REG_JOBREC = 0x61; //JOBREC寄存器
        public const Byte REG_TIME_CONDENSE = 0X62;   //设置去冷凝
        public const Byte REG_UNIT = 0x63; //UNIT单位
        public const Byte REG_COR_DATE = 0x64;//修正数据
        public const Byte REG_SCREEN_STATE = 0x65;//熄屏状态

        public const Byte LEN_READ_DEVICE = 0x0F;     //读设备寄存器的数据长度
        public const Byte LEN_READ_CALENDAR = 0x0A;     //读日历寄存器的数据长度
        public const Byte LEN_READ_TIME = 0x10;     //读时间寄存器的数据长度
        public const Byte LEN_READ_JOBSET = 0x19;     //读JOBSET寄存器的数据长度
        public const Byte LEN_READ_DOT1 = 0x28;     //读DOT寄存器的数据长度(温度采集器长度)
        public const Byte LEN_READ_DOT2 = 0x15;     //读DOT寄存器的数据长度(压力采集器长度)
        public const Byte LEN_READ_DOT3 = 0x40;     //读DOT寄存器的数据长度(温湿度采集器长度）
        public const Byte LEN_READ_JSN = 0x80;     //读JSN寄存器的数据长度
        public const Byte LEN_READ_USN = 0x40;     //读USN寄存器的数据长度
        public const Byte LEN_READ_UTXT = 0x80;     //读UTXT寄存器的数据长度
        public const Byte LEN_READ_NAME = 0x40;     //读设备名称
        public const Byte LEN_READ_SCREEN = 0x08;   //读熄屏状态的数据长度

        public const Byte LEN_SET_DEVICE = 0x02;     //设置设备寄存器的数据长度
        public const Byte LEN_SET_CALENDAR = 0x06;     //设置日历寄存器的数据长度
        public const Byte LEN_SET_TIME = 0x10;     //设置时间寄存器的数据长度
        public const Byte LEN_SET_JOBSET = 0x19;     //设置JOBSET寄存器的数据长度
        public const Byte LEN_SET_JOBREC = 0x02;     //设置JOBREC寄存器的数据长度
        public const Byte LEN_SET_JSN = 0x80;     //设置JSN寄存器的数据长度
        public const Byte LEN_SET_USN = 0x80;     //设置USN寄存器的数据长度
        public const Byte LEN_SET_UTXT = 0x80;     //设置USN寄存器的数据长度
        public const Byte LEN_SET_DOT = 0x28;     //设置DOT寄存器的数据长度
        public const Byte LEN_SET_BAT_CL = 0x00;     //设置BAT_CL寄存器的数据长度
        public const Byte LEN_SET_UNIT_CL = 0x01;   //设置REG_BAT_CL寄存器（读单位）的数据
        public const Byte LEN_SET_CTEMP_CL = 0x00;     //设置CTEMP_CL寄存器的数据长度
        public const Byte LEN_SET_SCREEN = 0x08;   //设置熄屏状态的数据长度

        public const UInt32 REC_ONEPAGE = 0x80;           //JOBREC寄存器每页存储的数据量(128字节)
        public const UInt32 REC_ADDRESS = 0x00000000;     //JOBREC的初始地址

        //定义文件中参数的字段名，注意其排序与其在meInfoTbl中的排序是相同的
        public const Int32 InfoTblRowNum = 33; //数据文件中包含的参数个数
        public const String CON_date = "date";          //测试开始日期
        public const String CON_start = "start";        //测试开始时间 -- 设定值
        public const String CON_stop = "stop";          //测试结束时间 -- 设定值
        public const String CON_span = "span";          //测试间隔时间(秒) -- 设定值
        public const String CON_duration = "duration";  //测试持续时间(秒) -- 设定值
        public const String CON_UID = "UID";            //设备ID
        public const String CON_mode = "mode";          //设备模式(YZLT30)
        public const String CON_JSN = "JSN";            //出厂编号
        public const String CON_range = "range";        //设备范围
        public const String CON_cal = "cal";            //校准日期
        public const String CON_rec = "rec";            //复校日期
        public const String CON_battery = "battery";    //电池电量
        public const String CON_TYPE = "TYPE";          //设备类型(HTT)
        public const String CON_USN = "USN";            //管理编号
        public const String CON_UTXT = "UTXT";          //备注信息

        //后来加的参数
        public const String CON_VERHW = "VERHW";
        public const String CON_VERSW = "VERSW";
        public const String CON_ADDR = "ADDR";
        public const String CON_maxcore = "maxcore";
        public const String CON_mincore = "mincore";
        public const String CON_rtc = "rtc";
        public const String CON_htr = "htr";
        public const String CON_wdt = "wdt";
        public const String CON_luk = "luk";
        public const String CON_mantime = "mantime";
        public const String CON_battime = "battime";
        public const String CON_caltime = "caltime";
        public const String CON_jintime = "jintime";
        public const String CON_read = "read";
        public const String CON_wake = "wake";
        public const String CON_interval = "interval";
        public const String CON_unit = "unit";
        public const String CON_Dstart = "Dstart";          //测试开始时间 -- 实际值
        public const String CON_Dstop = "Dstop";            //测试结束时间 -- 实际值
        public const String CON_Dduration = "Dduration";    //测试持续时间 -- 实际值
        public const String CON_hash = "hash";
    }

    public class TMP
    {
        public int[] OUT = new int[SZ.CHA]; //16点温度值
        public UInt16[] HUM = new UInt16[SZ.CHA]; //16点湿度值
        public int[] SOP = new int[SZ.CHA]; //16点温度变化率,前后12次采样相关

        public int outAvg; //15点平均温度
        public int sopAvg; //15点平均温度变化率

        public TMP()
        {
            for (Byte i = 0; i < SZ.CHA; i++)
            {
                OUT[i] = 0;
                HUM[i] = 0;
            }
        }

        public TMP(TMP mP)
        {
            for (Byte i = 0; i < SZ.CHA; i++)
            {
                OUT[i] = mP.OUT[i];
                HUM[i] = mP.HUM[i];
            }
        }
    }

    public class tmpoint
    {
        public int temperature;
        public int time;

        public tmpoint()
        {
            temperature = 0;
            time = 0;
        }

        public tmpoint(int a, int b)
        {
            temperature = a;
            time = b;
        }
    }
    public class PRM
    {
        //名称
        public String name;

        //step = 集合下标+1
        public List<tmpoint> myTP = new List<tmpoint>();

        public PRM()
        {
            name = "";
        }

        public PRM(String str)
        {
            name = str;
        }
    }

    public static class MyDefine
    {
        public static XET myXET = new XET();//参数数据使用
    }

    public delegate void freshHandler();//定义委托
}

//end

