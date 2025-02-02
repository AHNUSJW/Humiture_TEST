
============================================================

HTR_S11(Ziyun)

在HTR_S10基础上新增HTQ型号
新增温度单位（摄氏度、华氏度）切换功能
增加去冷凝功能
修改工作状态为空闲中，准备中，记录中和去冷凝中
更改HTQ的电量显示计算
连接界面设置鼠标悬停显示完全信息
新增HTQ型号的标定、校准和验证
修改文件选择为界面中弹出提示选择文件
新增数据修正功能
更改设备名称、出厂编号为自定义
出pdf中，校准信息添加右面数据的抬头，增加单位℃；%RH；KPa
修改软件版本
对HTP数据标定从原来的14个字节增加到21个字节


============================================================

HTR_S12(Ziyun)

修改生成pdf中，logo文件选择文件类型更改为PNG格式
针对以前的HTT和HTH设备名称不能修改，现在的处理方式是：
读取失败不弹窗提示，设置为默认的设备名称；
之后HTT和HTH再更新的时候，添加上这条命令，使之后的设备做到可以修改名称。


============================================================

HTR_S13(Ziyun)

修复选择文件选择时间筛选中，显示今日时间不更新的Bug
修复选中文件后，生成的表格设备名称有多余字符的问题


============================================================

HTR_S14(Ziyun)

优化选择文件时，只能点击选择框的问题
优化HTQ产品图片变形问题
对数据处理，生成曲线部分进行修改：
1、纵坐标上下限可手动修改
2、有效数据阶段不少于8个阶段的阶段设计
3、对每个阶段新增3个自定义数据：设定温度、纵坐标上限温度、纵坐标下限温度
4、对3个自定义数据，客户不录入，默认为温度最大值和最小值，往上和往下5°
5、对每个阶段，新增自定义阶段名称功能（注：输入名称之后要点击回车键才有效）


============================================================

HTR_S15(Ziyun)

新增删除阶段
将添加阶段、命名、删除阶段放在鼠标右键里
点击切换单位后有弹框提示


============================================================

HTR_S16(Ziyun)

解决设定时间与实际采集时间不符（总时间加上间隔时间）
解决右轴“总下限值”与左轴相关联的BUG
将PDF中的验证人员与报告日期修改为验证人员、验证日期，复核人员、复核日期
去除设置工作时出现的不需要的提示
新增返回主界面关闭串口
解决点击编号管理可能会出现的BUG
修改出厂设置提示显示不完全问题
修改出PDF阶段的名称


============================================================

HTR_S17(Ziyun)

出PDF画曲线图（画一起或者分开画）
选择单独出温度、湿度、压力
更新超级账户，账户号密码


============================================================

HTR_S18(Ziyun)

添加图像放大缩小的功能（滚轮放大、鼠标左键拖动、右键还原）
校准界面输入时间自动补全日期
修改生成pdf中设备型号的参数
重新排版pdf中标准器信息、原始数据的宽度
修改停止按钮中弹框提示
修复HTT读取数据未计算的问题
修改校准界面生成pdf曲线图未显示完全问题


============================================================

HTR_S19(Ziyun)

出厂设置:
修复出厂设置里换板子不能保存的问题
出厂设置里新增列表用于信息记录，方便使用

验证界面:
修复导出报表弹出几次弹框问题
pdf报告删除最后页多余内容,首末页和全页的页脚内容
选择的阶段如果有的探头阶段没有数据，生成纵向汇总表会报错

权限管理：
修复新增权限后登录Bug，出现较多弹框的问题
修复登录界面可能出现的崩溃问题

数据处理界面:
文件新增筛选限制，只筛选出.tmp后缀的文件
文件选择重复文件取消后依旧提示有重复文件

其他：
图片材料要求


============================================================

HTR_S20(Ziyun)

连接界面：
设备读取数据湿度范围限制：0-100%RH
切换单位后更新测量范围

数据处理界面:
阶段不能嵌套
刚开始左轴右轴崩溃
阶段名称的优化
阶段名称编辑后自动更新
鼠标左键点击自动换成当前阶段
全删除后再添加
放大倍数信息隐藏或删除
默认阶段初始化从头到尾
鼠标右键加缩放，鼠标缩放纵坐标有问题
数据处理界面可以选择阶段
日期选择当天日期没有反应

校准界面:
时间双击,空的话自动填第一个日期时间

验证界面:
生成报表统一温度单位

生成报告：
PDF可编辑权限

其他：
HTQ和HTT统一（鉴于HTT产品召回，后做统一处理，此处未做更改）
检查所有的log


============================================================

HTR_S21(Ziyun)

连接界面完善日历写入后判断
增加设备日历时间显示


============================================================

HTR_S22(Ziyun)

修改设备批量设置读取条数
修改PDF排版
新增部分数据打印
修改保存阶段设置为每次都重新计算设定值、上下限值
修改数据处理界面选择数据类型后只针对该类型数据进行处理
导入数据后，生成全阶段并自动计算设定值、上下限值


============================================================

HTR_S23(Ziyun)

设备日历时间显示普通用户隐藏，超级用户显示


============================================================

HTR_S24(Ziyun)

删除自带的生成安装文件
修改隐藏导入文件后直接生成报表，关键参数汇总数据错误
生成一个exe文件


============================================================

HTR_S25(Ziyun)

修复不能批量设置的Bug


============================================================

HTR_S26(Ziyun)

修复大量HTQ数据同时加载进来分析和出报告速度慢、卡死报错的问题（Lumi）
有多个批次的设备读取时，快速选择对应批次的数据（Ricardo）
新增操作文档说明（Ricardo）
新增能够截取整段数据的部分数据，然后生成新的原始记录包括表格记录（Lumi）
修复修正数据导入后不显示设备编号的问题(Ziyun)
修复修正数据导入后不显示设备编号的问题(Ziyun)
优化PDF报告模板（Lumi）
简化批量设置（Ricardo）
结束时间改成记录持续的时间（Ricardo）
优化表格报错问题(Ziyun)
优化数据载入一个文件出现一个多余的列6(Ziyun)
修复HTT修正数据修正失败的Bug(Ziyun)
修复图像时间和合并图像生成报告报错的问题（Lumi）
重新排版PDF中F0位置（Lumi）
优化EXCEL表格，都保留两位小数(Ziyun)
导入数据，选择单数据表会报错，选择不同（温、湿、压）等单独数据下面添加"面"的最大、最小、最大-最小、平均值（Ricardo）平均值还有问题
优化软件不识别中文括号(Ziyun)
更新首末页签字PDF格式（Lumi）

不点击生成曲线，点击生成报告报错（Lumi）
优化开始时间：设置/批量设置/冷凝均设置为以 xx-xx-00  秒数显示00结尾（Ricardo）
软件权限问题（Lumi）
建议复校时间提示（Lumi）
解决生成曲线界面提示信息不匹配的Bug(Ziyun)
升级支持38400波特率(Ziyun)


============================================================

HTR_S27(Ziyun)

保存阶段数据出错
不能截取多段数据，改变多段数据命名
导入阶段数据出错
更新F0计算，增加一个选择，从有效数据开始计算到有效数据结束
报告总图保留阶段图的截取（Lumi）
能选择文件快捷方式的路径


============================================================

HTR_S28(Ziyun)

右侧输入值修改进入加载
切换界面自动保存图片


============================================================

HTR_S29(Lumi)

报告加最大值最小值时间
报告数据列数调整为12
首末页签字格式修改
增加不拆分PDF的选项


============================================================

HTR_S30(Ricardo)

验证报告pdf中报告信息调整，删除了验证人员、验证日期、审核人员、审核日期
校准时间过期提示弹窗年限限制，超过2005年的弹窗提示

============================================================

HTR_S31(Ricardo)

pdf加密密码更新：

中文账户：JD20191206
英文账户：账户名+JD

F0值计算算法优化，解决了生成报告pdf出现多段数据分析，现仅一次数据分析

============================================================
HTR_S32(Ricardo)

修正数据之后生成pdf中删除 .Cor的标识符



待解决：
文件导入顺序等相关问题
导入的文件时间不连续，数据混乱BUG


账户: JDEGREE
密码: YANG


待解决问题：
校准曲线中生成曲线，如果是一个点，就看不到点位对比；点位上能否标记一下具体温度和湿度数据。
