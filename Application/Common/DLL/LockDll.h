#ifndef __MAKE_CARD_H__
#define __MAKE_CARD_H__
    


#ifdef __cplusplus
	extern "C" { 
#endif



/*=============================================================================
函数名：                        LS_SelectDoorLockType
;
功　能：选择卡片类型
输  入：DoorType -- 门锁类型(也就是使用的卡片类型)
输  出: 无
返回值：错误类型
=============================================================================*/
int __stdcall LS_SelectDoorLockType(int DoorType);

/*=============================================================================
函数名：                        LS_OpenPort
;
功　能：打开端口, 连接读卡器
输  入：Port -- 端口号, 串口可设1~4, USB口设为5
输  出: 无
返回值：错误类型
=============================================================================*/
int __stdcall LS_OpenPort(int Port);

/*=============================================================================
函数名：                        LS_ReadSystemPas
;
功　能：读取授权卡中的客户码, 可以读取Mifare卡扇区号
输  入：无
输  出: Password -- 16个字符, 前8个为客户码, 第9~10个为Mifare卡扇区号
返回值：错误类型
=============================================================================*/
int __stdcall	LS_ReadSystemPas(char *funPsw, char *Password, char *cardSnr);


/*=============================================================================
函数名：                        LS_SystemInitialization
;
功　能：系统初始化, 也就是设置客户码和Mifare扇区号等到动态库中
输  入：Password -- 16个字符. 前8个为客户码, 第9~10个为Mifare卡扇区号
输  出: 无
返回值：错误类型
=============================================================================*/
int __stdcall LS_SystemInitialization(char *funPsw, char *Password);

/*=============================================================================
函数名：                        LS_ReadRom
;
功　能：读取卡号(卡片的世界唯一的序列号)
输  入: 无
输  出: cardSnr         --  卡号: 16个字符
返回值：错误类型
=============================================================================*/
int __stdcall LS_ReadRom(char *cardSnr);


/*=============================================================================
函数名：                        LS_MakeStaffCardEx1
;
功　能：制作员工卡
输  入：
        RoomList    --  房号列表: 最多50个客房,  楼栋号.楼层号.房号.子房号  例如:  "001.002.00003.A   001.002.00005"
        AreaList    --  区域列表: 最多8个区域, 例如: "001.002.003.004.005.006.007.008"
        SDateTime   --  起始日期：年月日时分秒
        EDateTime   --  结束日期：年月日时分秒
        Time1       --  时间段1: 起始时间的小时/分钟 + 结束时间的小时/分钟, 4
        Time2       --  时间段2
        Time3       --  时间段3
        FacultyNo   --  持卡人院系 代码, 1~255
        MajorNo     --  持卡人专业 代码, 1~65535
        ClassNo     --  持卡人班级 代码, 1~65535， 预留， 暂时不用
        UserName    -- 用户名, 最多13个英文字母或者6个汉字
        iStaffProhibitedDayOfWeek -- 一个星期进入的天数.  bit0==1表示周一进入, bit6==1表示周日禁入.  (bit0~bit7表示周一至周日)
        cStaffProhibitedDays --禁入的日期. 十六进制字符串, 一个字节占两个字符, 例如"1122AACD", 表示days[]={0x11,0x22,0xAA,0xCD} 等. 从SDateTime的日期开始, days[0].bit0~bit7表示从SDateTime开始的第一~第八天(SDateTime的日期算第一天). 
                        days[1].bit0~bit7表示从SDateTime开始的第九~第十六天. 某bit==1, 则表示该日期禁止进入.  最多366天. 

        options     --  选项: bit0: 常开; 未用到的位请清零
输  出: cardSnr         -- 卡号: 8
返回值：错误类型
注意:   
char    cStaffProhibitedDays[200]; // 员工卡禁入的日期(从SDateTime的日期开始, SDateTime算第一天) 
=============================================================================*/
int __stdcall LS_MakeStaffCardEx1(char *cardSnr, char *RoomList, char *AreaList, char *SDateTime,char *EDateTime, char *STime1, char* ETime1, char *STime2, char* ETime2, char *STime3, char* ETime3, \
                                     int FacultyNo, int MajorNo, int ClassNo, char *UserName, char *LostCardSn, int iStaffFlags, int iReplaceNo, int iStaffProhibitedDayOfWeek, char *cStaffProhibitedDays, int *iRFU);

/*=============================================================================
函数名：                        LS_ReadStaffCard
;
功　能：读取员工卡信息
输  入: 无
输  出：
        RoomList    --  房号列表: 最多50个客房,  楼栋号.楼层号.房号.子房号  例如:  "001.002.00003.A   001.002.00005"
        AreaList    --  区域列表: 最多8个区域, 例如: "001.002.003.004.005.006.007.008"
        SDateTime   --  起始日期：年月日时分秒
        EDateTime   --  结束日期：年月日时分秒
        Time1       --  时间段1: 起始时间的小时/分钟 + 结束时间的小时/分钟, 4
        Time2       --  时间段2
        Time3       --  时间段3
        FacultyNo   --  持卡人院系 代码, 1~255
        MajorNo     --  持卡人专业 代码, 1~65535
        ClassNo     --  持卡人班级 代码, 1~65535， 预留， 暂时不用
        UserName    -- 用户名, 最多13个英文字母或者6个汉字

输  出: cardSnr         -- 卡号: 8
返回值：错误类型
注意:   
=============================================================================*/
int __stdcall LS_ReadStaffCard(char *cardSnr, char *RoomList, char *AreaList, char *SDateTime,char *EDateTime, char *STime1, char* ETime1, char *STime2, char* ETime2, char *STime3, char* ETime3, \
                                     int *FacultyNo, int *MajorNo, int *ClassNo, char *UserName, char *LostCardSn, int *iFlags);

/*=============================================================================
函数名：                        LS_CancelCard
;
功　能：注销卡片/卡片回收/退房
输  入: 无
输  出：

输  出: cardSnr         -- 卡号: 8
返回值：错误类型
注意:   
=============================================================================*/
int __stdcall LS_CancelCard(char *cardSnr);

/*=============================================================================
函数名：                        LS_BeepOk
;
功　能：读卡器发声, 提示操作成功
输  入：无
输  出: 无
返回值：错误类型
=============================================================================*/
void LS_BeepOk();

/*=============================================================================
函数名：                        LS_BeepFailure
;
功　能：读卡器发声, 提示操作失败
输  入：无
输  出: 无
返回值：错误类型
=============================================================================*/
void LS_BeepFailure();


/*=============================================================================
函数名：                        LS_MakeInstallCard
;
功　能：制作安装卡, 可以设定是否实现宾客卡顶替功能.
输  入: Building    --  楼栋号
        Floor       --  楼层号, 1~255
        Room        --  房号, 格式: 楼栋号.楼层号.房号.子房号  例如:  "001.002.00003.A   001.002.00005"
        RoomType    --  客房的类型, 例如普通客房/套房里的小间/楼栋大门等
        RoomDigits  --  本楼栋客房编号的位数(不包括套间号), 常用的为4或者5
        LockSetting --  门锁各种设置和选项，参看Defines.h
输  出: cardSnr         --  卡号: 16个字符
例  子: Building='2', Floor=8, Room="20805", RoomType=1, RoomDigits=5, LockSetting=7
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeInstallCard(char *cardSnr,int Building, int Floor, char *Room,int RoomType, int iAreaNo1, int iAreaNo2, int LockSetting, int iReplaceNo);


/*=============================================================================
函数名：				   LS_MakeLockSettingCardEx1

输  入：
        iAreaNo -- 区域号
        iForbidCardType -- 要禁止或者例外的卡片. 如果是例外的卡片, 则应该是(卡片类型 + 128)
        cForbidDateTime -- 解封日期, 到期卡片自动恢复功能
        SDateTime   --  起始日期：年月日时分秒
        EDateTime   --  结束日期：年月日时分秒
        iFlags      --  卡片标志
        cSTime1     --  时间段1的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime1     --  时间段1的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cSTime2     --  时间段2的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime2     --  时间段2的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cSTime3     --  时间段3的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime3     --  时间段3的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
                  
输  出: cardSnr         -- 卡号: 8
返回值：错误类型
注意:         
=============================================================================*/
int __stdcall LS_MakeLockSettingCardEx1(char *cardSnr, int iAreaNo, int iForbidCardType, char *cForbidDateTime, char *SDateTime, char *EDateTime, int iRFU, char *cRFU, int iFlags, int iReplaceNo,\
                                        char *cSTime1, char *cETime1, char *cSTime2, char *cETime2,  char *cSTime3, char *cETime3);



/*=============================================================================
函数名：				   LS_MakeParaSettingCard
                               制作参数设置卡
输  入：
        sysPara -- 系统参数结构体
        bMaskBits1 -- 位域掩码字节1
        bMaskBits2 -- 位域掩码字节2
        bMaskBits3 -- 位域掩码字节3
        bMaskBits4 -- bit0对应于"门锁模式"
        bMaskBits5 -- bit0对应于"开锁时间"
        iFlags      --  卡片标志
                  
输  出: cardSnr         -- 卡号: 8
返回值：错误类型
注意:         
=============================================================================*/
int __stdcall LS_MakeParaSettingCard(char *cardSnr, SYS_PARA_SETTING sysPara, BYTE bMaskBits1, BYTE bMaskBits2, BYTE bMaskBits3, BYTE bMaskBits4, BYTE bMaskBits5, int iFlags);

/*=============================================================================
函数名：                        LS_MakeTimeCardEx1
;
功　能：制作校时卡扩展1
输  入: DateTime    --  要设置的日期时间, 年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss"
        cSummerTimeStart - 夏令时起始(BCD码): 月|第几个星期几|几点|调整几小时|, 各占2字符, 例如4月份的最后一个星期日的2:00 调快1个小时, 则字符串为"04|57|02|01|",  05表示最后一个星期
        cSummerTimeEnd -   夏令时结束(BCD码): 月|第几个星期几|几点|调整几小时|, 各占2字符, 例如10月份的第二个星期四的2:00 调慢1个小时,  则字符串为"10|24|02|01|"
        iFlags -- 标志字节

        输  出: cardSnr         --  卡号: 16个字符
        例  子: DateTime="2008-06-06 12:30:00", iReplaceNo=0, cSummerTimeStart="04|57|02|01|", cSummerTimeEnd="10|24|02|01|"
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeTimeCardEx1(char *cardSnr,char *DateTime, int iReplaceNo, char *cSummerTimeStart, char *cSummerTimeEnd, int iFlags);

/*=============================================================================
函数名：                        LS_MakeChiefCard
;
功　能：制作总卡
输  入: SDateTime   --  有效期的起始日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 此为预留参数
        EDateTime   --  有效期的结束日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 只使用日期参数
        iFlags  --  卡片标志字节
输  出: cardSnr         --  卡号: 16个字符
例  子: SDateTime="2008-06-06 00:00:00", EDateTime="2009-06-06 00:00:00"
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeChiefCard(char *cardSnr, char *SDateTime, char *EDateTime, int iFlags, int iReplaceNo);

/*=============================================================================
函数名：                        LS_MakeEmergentCard
;
功　能：制作应急卡
输  入: SDateTime   --  有效期的起始日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 此为预留参数
        EDateTime   --  有效期的结束日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 只使用日期参数
        iFlags  --  标志字节
输  出: cardSnr         --  卡号: 16个字符
例  子: SDateTime="2008-06-06 00:00:00", EDateTime="2009-06-06 00:00:00"
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeEmergentCard(char *cardSnr, char *SDateTime, char *EDateTime, int iFlags, int iReplaceNo);

/*=============================================================================
函数名：                        LS_MakeBuildingCard
;
功　能：制作楼栋卡
输  入: BuildingList --  楼栋列表, 例如 "001.002.003.004.005"
        SDateTime   --  有效期的起始日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 此为预留参数
        EDateTime   --  有效期的结束日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 只使用日期参数
        iFlags- 标志字节
输  出: cardSnr         --  卡号: 16个字符
例  子: BuildingList='001.002.003.004.005', SDateTime="2008-06-06 00:00:00", EDateTime="2009-06-06 00:00:00"
返回值：错误类型
=============================================================================*/
int __stdcall	LS_MakeBuildingCard(char *cardSnr, char *BuildingList, char *SDateTime, char *EDateTime, int iFlags, int iReplaceNo);

/*=============================================================================
函数名：                        LS_MakeFloorCard
;
功　能：制作楼层卡
输  入: Building    --  楼栋号
        nStartFloor --  起始楼层号: 1~255
        nEndFloor   --  结束楼层号: 1~255, 与nStartFloor一起决定有效楼层范围
        cSTime1     --  时间段1的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime1     --  时间段1的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cSTime2     --  时间段2的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime2     --  时间段2的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cSTime3     --  时间段3的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime3     --  时间段3的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
        SDateTime   --  有效期的起始日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 此为预留参数
        EDateTime   --  有效期的结束日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 只使用日期参数
        iFlags --  标志字节

输  出: cardSnr         --  卡号: 16个字符

例  子: Building='1', nStartFloor=1, nEndFloor=10, cSTime1="00:00:00", cETime1="23:59:00"
        cSTime2="00:00:00", cETime2="00:00:00", cSTime3="00:00:00", cETime3="00:00:00"
        SDateTime="2008-06-06 00:00:00", EDateTime="2009-06-06 00:00:00"

返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeFloorCard(char *cardSnr, int Building,  char *FloorList, char *cSTime1, char *cETime1, char *cSTime2, char *cETime2,  char *cSTime3, char *cETime3, char *SDateTime, char *EDateTime, int iFlags, int iReplaceNo);


/*=============================================================================
函数名：                        LS_MakeLostCard
;
功　能：制作挂失卡
输  入: LostRom     --  要挂失的卡号: 16个字符
输  出: cardSnr         --  卡号:  16个字符
例  子: LostRom="1122334400000000"
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeLostCard(char *cardSnr,char *LostRom, int iReplaceNo);

/*=============================================================================
函数名：                        LS_MakeUnLostCard
;
功　能：制作取消挂失卡
输  入: UnLostRom   --  要取消挂失的卡号: 16个字符
输  出: cardSnr         --  卡号: 16个字符
例  子: UnLostRom="1122334400000000"
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeUnLostCard(char *cardSnr,char *UnLostRom, int iReplaceNo);

/*=============================================================================
函数名：                        LS_MakeDataCard
;
功　能：制作数据导出卡
输  入: iWantLen        --  希望读取的字节数, 57卡只支持256的整数倍
输  出: cardSnr         --  卡号: 16个字符
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeDataCard(char *cardSnr, int iWantLen, int iReplaceNo);


/*=============================================================================
函数名：                        LS_ReadDataCard
;
功　能：把数据卡的内容读取到内存中, 并提取门锁基本参数. 此后就可以用
        ReadOneOpenDoorRecord从内存中逐条读取开门记录.
输  入: lockInfo    --  门锁信息结构体指针, 如果不关心门锁信息, 则设为0
        datFromFile --  是否从文件读取信息，一般置0（从手持机读取）
        infoPsw     --  验证码，一般置0
        dat         --  开锁记录原始数据指针, 一般设置为0
        iWantLen    --  指定要读取的字节数, 一般设置为0.
输  出: lockInfo    --  门锁信息结构体
        dat         --  开门记录原始数据(16进制数的字符串表示,大小为32K字节)
例  子: lockinfo=0, datFromFile=0, infoPsw=0, dat=0
返回值：错误类型
=============================================================================*/
int __stdcall LS_ReadDataCard(LOCK_INFO *lockInfo, int datFromFile, int infoPsw, char *dat, int iWantLen);


/*=============================================================================
函数名：                        LS_ReadOneOpenDoorRecord
;
功　能：读取一条开门记录
输  入: 无
输  出: Building     --  楼栋号
        Room        --  房号, 普通门5个字符, 套间房后面还有一个字母表示套间号
        OpenKind    --  开门类型
        OpenTime    --  开门时间, 年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss"
        cardSnr         --  卡号: 16个字符
返回值：错误类型
=============================================================================*/
int __stdcall	LS_ReadOneOpenDoorRecord(int *Building, char *Room, int *Kind, char *OpenTime, char *cardSnr);


/*=============================================================================
函数名：                        LS_DownloadRoomDataEx1
;
功　能：下载安装数据到手持机上
输  入: roomCnt -- roomList中的客房个数；
        roomList --  ROOM_INFO型数组，依次存放多个客房数据。
        cSummerTimeStart - 夏令时起始: 月|第几个星期日|几点|调整几小时|, 各占2字符, 例如4月份的最后一个星期日的2:00 调快1个小时, 则字符串为"04|05|02|01|",  05表示最后一个星期
        cSummerTimeEnd -   夏令时结束: 月|第几个星期日|几点|调整几小时|, 各占2字符, 例如10月份的第二个星期日的2:00 调慢1个小时,  则字符串为"10|02|02|01|"
        iFlags -- 标志字节

输  出: 
返回值：错误类型
注意：  下载的客房数据必须按照楼栋号、楼层号、房号排序，然后依次存放在roomList中。
=============================================================================*/
int __stdcall LS_DownloadRoomDataEx1(int roomCnt, ROOM_INFO *roomList, char *cSummerTimeStart, char *cSummerTimeEnd, int iDownloadFlags);


/*=============================================================================
函数名：                        LS_MakeCheckoutCard
;
功　能：制作退房卡
输  入: Building    --  楼栋号
        FloorList   --  楼层列表
        iFlags      --  参见Defines.h
        SDateTime   --  有效期的起始日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 此为预留参数
        EDateTime   --  有效期的结束日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 只使用日期参数
        cStopTime   --  终止时间: 年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 在此时间之前发的宾客卡和员工卡将失效.
                        一般提前办小时, 例如现在是12:00, 则设置为11:30. 
输  出: cardSnr         --  卡号: 16个字符

例  子: Building='1', StartFloor=1, EndFloor=10, WholeSys=0, WholeBuilding=0
        SDateTime="2008-06-06 00:00:00", EDateTime="2009-06-06 00:00:00", cStopTime="2000-00-00 00:00:00"
        返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeCheckoutCard(char *cardSnr, int Building, char *FloorList, char *SDateTime, char *EDateTime, char *cStopTime, int iFlags, int iReplaceNo);


/*************************************************************************
函数名：                LS_GetCardInformation
功　能：读取卡片信息
参　数：datFromFile --  是否从文件中提取卡片信息，一般置0（从卡片中读取）
        infoPsw     --  验证码，一般置0.
        CardInfo    --  卡片数据结构体指针
        dat         --  卡片数据指针, 一般置0

输出：  CardInfo    -- 卡片数据结构体
        dat         -- 卡片数据(16进制数的字符串表示, 100字节)，一般不用理会
返回值：错误类型
描　述：注意要先根据门锁厂家,设置好样锁授权码到动态库.设置好扇区
*************************************************************************/
int __stdcall LS_GetCardInformation(CARD_INFO *CardInfo, int datFromFile, int infoPsw, char *dat);


/*=============================================================================
函数名：                       LS_SaveRegisterCode
;
功　能：发卡器注册
输  入: RegCode      --  注册码, 27个字符, 格式:xxxxxx-xxxxxx-xxxxxx-xxxxxx
输  出: 无
返回值：错误类型
=============================================================================*/
int __stdcall LS_SaveRegisterCode(char *RegCode);


/*=============================================================================
函数名：                       LS_GenMachineCode
;
功　能：生成机器码
输  入: oprType     -- 操作类型, 管理软件注册时设0
输  出: machineCode -- 授权号，27个字符, 格式:xxxxxx-xxxxxx-xxxxxx-xxxxxx
返回值：错误类型
=============================================================================*/
int __stdcall LS_GenMachineCode(char *machineCode, int oprType);


/*=============================================================================
函数名：                        LS_ChkRegisterInfo
;
功　能：注册检查, 如果注册快过期,则提醒注册; 更新使用天数
输  入: 无
输  出: allowedDays -- 剩余授权天数, 65535为无限期使用, 0则不允许再使用
返回值：错误类型
=============================================================================*/
int __stdcall LS_ChkRegisterInfo(int *allowedDays);


/*=============================================================================
函数名：                        LS_GenClientCode
;
功　能：自动产生授权码
输  入：iflags   -- iflags.0==1, 表示产生样锁授权码(安装施工用)
输  出: Password -- 16个字符, 对应8字节, 前4字节为客户码, 第5个字节M1卡扇区号, 
                    第6个字节为替代编号.
返回值：错误类型
=============================================================================*/
int __stdcall LS_GenClientCode(char *funPsw, char *Password, int iflags);


/*=============================================================================
函数名：                        LS_GetClientFromCard
;
功　能：从总卡中恢复授权码
输  入：
输  出: 无
返回值：错误类型
=============================================================================*/
int __stdcall LS_GetClientFromCard(char *funPsw, char *Password, int iflags);


///////////////////////////////////////////////////////////////////////////////////////
int __stdcall LS_ClosePort(int Port);
int __stdcall LS_ReadGuestCard(char *cardSnr,char *Room, char *SDateTime, char *EDateTime);

/*=============================================================================
函数名：                        LS_MakeGuestCard_EX1
;
功　能：制作宾客卡， 可设主宾客卡标志和空调标志
输  入：RoomList    --  房号列表:   字符串, M1卡的最多4个客房，其它的只支持一个客房。例如 "1.2.8203.A  1.2.8205"
        AreaList    --  区域列表
        ElevatorFloorList   --  电梯楼层列表(最多4个), 即额外可以使用的电梯楼层. 例如 "001.002.003.004.005"
        SDateTime   --  入住时间：  年月日时分秒, 字符串格式 "YYYY-MM-DD hh:mm:ss"
        EDateTime   --  预离时间：  年月日时分秒, 字符串格式 "YYYY-MM-DD hh:mm:ss"
        iFlags      --  宾客卡选项, 参见Defines中的GUEST_FLAGS定义
输  出: cardSnr         -- 卡号:        16个字符
例  子: Room="1.2.8203.A", AreaList="001.002.005", ElevatorFloorList="001.002.003.004.005", DateTime="2008-06-06 12:30:59", EDateTime="2008-06-07 12:00:00"
        iFlags=0
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeGuestCard_EX1(char *cardSnr, char *RoomList, char *AreaList, char *ElevatorFloorList, char *SDateTime,char *EDateTime, int iFlags);
int __stdcall LS_MakeGuestCard(char *cardSnr, char *RoomList, char *AreaList, char *SDateTime,char *EDateTime, int iFlags);
/*=============================================================================
函数名：                        LS_MakeBackupCard
;
功　能：制作后备卡
输  入：Room        --  房号:       字符串, 例如 "001.002.00003.A"
        SDateTime   --  入住时间：  年月日时分秒, 字符串格式 "YYYY-MM-DD hh:mm:ss"
        EDateTime   --  预离时间：  年月日时分秒, 字符串格式 "YYYY-MM-DD hh:mm:ss"
        iFlags      --  宾客卡选项, 参见Defines中的定义
输  出: cardSnr         -- 卡号:        16个字符
例  子: Room="001.002.00003.A", SDateTime="2008-06-06 12:30:59", EDateTime="2008-06-07 12:00:00"
        iFlags=0
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeBackupCard(char *cardSnr, char *Room, char *SDateTime,char *EDateTime, int iFlags, int iReplaceNo);

/*=============================================================================
函数名：                       LS_VerifyOperate
;
功　能：验证操作, 用于产生随机数, 产生验证码或者比较验证码, 用于一些需要验证随机数
        的操作.
输  入: mode        -- 操作模式: 1=产生随机数; 2=产生验证码; 3=比较验证码
        verifyCode  -- 验证码 
        funPsw      -- 操作密码

输  出: randomCode  -- 随机数, 5位数
        verifyCode  -- 验证码, 5位数
返回值：错误类型
=============================================================================*/
int __stdcall LS_VerifyOperate(int mode, char *randomCode, char *verifyCode, char *funPsw);


/*=============================================================================
函数名：                        LS_MakeSampleChief
;
功　能：制作样锁总卡
输  入: 无
输  出: 无
返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeSampleChief(char *cardSnr);

/*=============================================================================
函数名：                        LS_HandsimConfig
;
功　能：设置手持机
输  入: roomCnt -- roomList中的客房个数；
        roomList --  ROOM_INFO型数组，依次存放多个客房数据。
		configMask:  掩码, 决定更新手持机上的哪些内容
输  出: 
注意：//configMask 的取值
#define MS_OPERATOR_PSW		(1<<0)		//更新操作员密码
#define MS_MODULE_EN		(1<<1)		//更新功能使能
#define MS_PARA1_UPDATE		(1<<2)		//更新设置参数1
#define MS_PARA2_UPDATE		(1<<3)		//更新设置参数2
=============================================================================*/
int __stdcall LS_HandsimConfig(UINT configMask, HANDSIM_INFO *handsimInfo) ;


/*=============================================================================
函数名：                        LS_GetEnabledModules
;
功　能：检查哪些功能模块已经使能
输  入: 无
输  出: enabledModules -- bit0==1: 启动计次消费;  bit1==1: 启动FIAS接口
返回值：错误类型
=============================================================================*/
int __stdcall LS_GetEnabledModules(int *enabledModules, int *iRFU1, char *cRFU1);

/*=============================================================================
函数名：                        LS_DS2460Read
;
功　能：读取DS2460
输  入: addr -- 起始地址
		len  -- 读取的字节数
输  出: buf  -- 读取的信息在buf中
返回值：错误类型
=============================================================================*/
int __stdcall LS_DS2460Read(int addr, int len, BYTE *buf, char *psw);

/*=============================================================================
函数名：                        LS_DS2460Write
;
功　能：写DS2460
输  入: Addr -- 起始地址， 0~255
		len  -- 写入的字节数，1~64
		buf  -- 发送缓冲区, BYTE型数据
输  出: 无
返回值：错误类型
=============================================================================*/
int __stdcall LS_DS2460Write(int addr, int len, BYTE *buf, char *psw);

/*=============================================================================
函数名：                        LS_MakeElevatorCard
;
功　能：制作电梯卡
输  入: Building    --  楼栋号
        FloorList   --  楼层列表(最多5个), 例如 "001.002.003.004.005"
        AreaList    --  区域列表: 最多8个区域, 例如: "001.002.003.004.005.006.007.008"
        cSTime1     --  时间段1的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime1     --  时间段1的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cSTime2     --  时间段2的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime2     --  时间段2的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cSTime3     --  时间段3的起始时间: 时分秒, 字符串格式 "hh:mm:ss" 
        cETime3     --  时间段3的结束时间: 时分秒, 字符串格式 "hh:mm:ss" 
        SDateTime   --  有效期的起始日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 此为预留参数
        EDateTime   --  有效期的结束日期:  年月日时分秒, 字符串格式"YYYY-MM-DD hh:mm:ss", 只使用日期参数
        iFlags --  标志字节

输  出: cardSnr         --  卡号: 16个字符

例  子: Building='1', FloorList="001.002.003.004.005", AreaList="001.002.003", cSTime1="00:00:00", cETime1="23:59:00"
        cSTime2="00:00:00", cETime2="00:00:00", cSTime3="00:00:00", cETime3="00:00:00"
        SDateTime="2008-06-06 00:00:00", EDateTime="2009-06-06 00:00:00"

返回值：错误类型
=============================================================================*/
int __stdcall LS_MakeElevatorCard(char *cardSnr, int Building, char *FloorList, char *AreaList, char *SDateTime,char *EDateTime, char *cSTime1, char* cETime1, char *cSTime2, char* cETime2, char *cSTime3, char* cETime3, int iFlags, int iReplaceNo);


/*=============================================================================
函数名：				   LS_MakeJoinNetCard

输  入：无
                  
输  出: cardSnr         -- 卡号: 8
返回值：错误类型
注意:         
=============================================================================*/
int __stdcall LS_MakeJoinNetCard(char *cardSnr, char *cPara1, char *cPara2, int iPara3, int iPara4);

/*=============================================================================
函数名：				   M1_MakeExitNetCard

输  入：无
                  
输  出: cardSnr         -- 卡号: 8
返回值：错误类型
注意:         
=============================================================================*/
int __stdcall LS_MakeExitNetCard(char *cardSnr, char *cPara1, char *cPara2, int iPara3, int iPara4);


/*=============================================================================
函数名：				   LS_MakeParaSettingCard
                               制作门禁电梯设置卡
输  入：
        para -- 门禁/电梯参数结构体
        iFlags      --  卡片标志
                  
输  出: cardSnr         -- 卡号: 8
返回值：错误类型
注意:         
=============================================================================*/
int __stdcall LS_MakeAcsElevatorSettingCard(char *cardSnr, ACS_ELEVATOR_SET para, int iFlags);


int __stdcall LS_MakeTestCard(char *cardSnr);
int __stdcall LS_MakeFactoryCard(char *funPsw, char *UserPas,char *cardSnr);
int __stdcall LS_MakeStopCard(char *cardSnr, int iReplaceNo);
int __stdcall LS_MakeClientCard(char *funPsw,char *ClientPsw, int M1ClientCardSector, int M1LockNewSecotr, char *cardSnr);
int __stdcall LS_MakeManageCard(char *cardSn, int RFU1, int RFU2, int RFU3, char *RFU4, char *RFU5, int iReplaceNo);
int __stdcall LS_GetReaderSn(char *ReaderSn);
int __stdcall LS_ReadMem(int addr, int len, char *buf, int psw);
int __stdcall LS_WriteMem(int addr, int len, char *buf, int psw);
int __stdcall LS_GenRegisterCode(char *machineCode, int AllowedDays, int enabledModules, char* pswOptions, OUT char *regCode);
int __stdcall LS_MakeUpdateCard(char *cardSn);
void __stdcall LS_GetBeep();
void __stdcall LS_PCBeep();

#ifdef __cplusplus
   }
#endif

#endif              // __MAKE_CARD_H__