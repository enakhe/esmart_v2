#ifndef __DEFINES_H__
#define __DEFINES_H__
   

// 卡片错误 
enum ERROR_TYPE
{
    OPR_OK              =   1,      // 操作成功
    NO_CARD			    =   -1,     // 没检测到卡片
    NO_RW_MACHINE	    =   -2,     // 没检测到读卡器
    INVALID_CARD	    =   -3,     // 无效卡
    CARD_TYPE_ERROR	    =   -4,     // 卡类型错误
    RDWR_ERROR		    =   -5,     // 读写错误
    PORT_NOT_OPEN	    =   -6,     // 端口未打开
    END_OF_DATA_CARD    =   -7,     // 数据卡结束
    INVALID_PARAMETER   =   -8,     // 无效参数
    INVALID_OPR		    =   -9,     // 无效操作
    OTHER_ERROR		    =   -10,    // 其它错误
    PORT_IN_USED	    =   -11,    // 端口已被占用
    COMM_ERROR		    =   -12,    // 通讯错误
    ERR_RECOVER_CLIENT  =   -13,    // 从发卡器中恢复授权码成功

    ERR_CLIENT          =   -20,    // 客户码错误
    ERR_LOST            =   -21,    // 被挂失了
    ERR_TIME_INVALID    =   -22,    // 时间无效
    ERR_TIME_STOPED     =   -23,    // 宾客卡被顶替了
    ERR_BACK_LOCKED     =   -24,    // 反锁了
    ERR_BUILDING        =   -25,    // 楼栋号错误
    ERR_FLOOR           =   -26,    // 楼层号错误
    ERR_ROOM            =   -27,    // 房号错
    ERR_LOW_BAT         =   -28,    // 电池低压了
    
    ERR_NOT_REGISTERED  =   -29,    // 未注册
    ERR_NO_CLIENT_DATA  =   -30,     // 无授权卡信息
    ERR_ROOMS_CNT_OVER  =   -31,    // 房数超出了可用扇区
};


// iGuestFlags, 各标志可以相加以启用多个选项 (很早期版本)
//enum GUEST_FLAGS 
//{
//    OPTION_1			  =   0x20,      // 选项1, 预留, 根据实际定义
//    OPTION_2			  =   0x10		 // 选项2, 预留, 根据实际定义
//};


// LockSetting：门锁设置选项, 一般LockSetting = LS_MASK (15)即可
enum LOCK_SETTING 
{
// 客房标志1
    LS_REPLACE_EN       =   0x01,      // 宾客卡顶替使能，后一分钟宾客卡顶替前面发的宾客卡
    LS_LEAD_EN          =   0x02,      // 引导使能：使能则刷安装卡、校时卡、升级卡前需要先刷管理卡引导
    LS_VALID_DATE_EN    =   0x04,      // 有效期使能：使能则总卡、应急卡、楼栋卡、楼层卡、退房卡需要检查有效期
    LS_FLOOR_RANGE_EN   =   0x08,      // 检测楼层范围: 使能则楼层卡和应急卡检测楼层范围, 否则检测楼层列表
    LS_CHANEL_MODE_EN   =   0x10,      // 通道模式
	LS_FORBID_TONGUE_ALARM = 0x20,		// 禁止假锁报警
    LS_ALL_REPLACE      =   0x40,       // 各种卡片启动替代功能
    LS_FLAG_BYTE2_EN    =   0x80,       // 启动客房标志字节2使能

// 客房标志2
    LS_LOCK_IMMEDIATE_EN=   0x0100,     // 斜舌缩进后立即上锁, 不等把手松开, 可能是自弹锁体
    LS_NO_BACKLOCK_EN   =   0x0200,     // 置位则所有卡片都不检测反锁
    LS_NO_MUSIC_EN      =   0x0400,     // 去掉开关门的音乐声, 改为一短声
    LS_PROMPT_CLOSE_EN  =   0x0800,     // 提示宾客关门
    LS_NO_BLOCK_LIGHT_EN=   0x1000,     // 反锁时, 不闪灯提示
};


// RoomType: 安装卡的RoomType参数， 客房类型
enum ROOM_TYPE 
{
    RT_COMMON_ROOMS     =   0x1,      // 普通房、套间大门
    RT_SUITE_ROOMS      =   0x2,      // 套房里的小套间
    RT_FLOOR_GATE       =   0x4,      // 楼层通道大门
    RT_BUILDING_GATE    =   0x8,       // 楼栋通道大门
    RT_HOTEL_GATE       =   0x10        // 酒店大门, 只要是本系统的卡都能开.  特别注意楼栋号和楼层号的处理
};


// cardFlags: 安装卡的RoomType参数， 客房类型
enum CARD_FLAGS 
{
    CF_BACK_LOCK_EN     =   0x01,      // 允许开反锁(各种开门卡)
    CF_CHANNEL_MODE     =   0x02,      // 通道模式(各种开门卡)
    CF_PRIVILEGE_EN     =   0x04,      // 可以打开需要特权的门(各种开门卡)
	
    CF_NO_REPLACE       =   0x08,     // 不顶替前卡(宾客卡)	
	CF_IMPORT_ROOM		=	0x10,     // 宾客卡导入房号
	CF_OPEN_ONCE		=	0x20,     // 只开一次门(一次性宾客卡)
	CF_NEW_CARD			=   0x40,     // DLock早期版本中表示新客人入住。 复制宾客卡时清零。
	CF_CHK_CHECKIN_TIME =	0x80,	  // 检测入住时间
	
	
    CF_REPLACE_EN       =   0x40,     // 顶替使能(员工卡)
    CF_AUTO_LOST_EN     =   0x80,     // 自动挂失卡片(员工卡)

    CF_WHOLE_BUILDING   =   0x40,     // 整个楼栋权限(退房卡)
    CF_WHOLE_HOTEL      =   0x80,     // 整个系统权限(退房卡)

    CF_JUDGE_CHECKIN_TIME = 0x80,     // 判断入住时间使能(宾客卡)   

    CF_CHANGE_FLAGS     = (0x01 << 16),   // 更改锁体标志
    CF_CLEAR_ROOM_INFO  = (0x02 << 16),   // 清空客房信息(锁体设置卡)
    CF_FORBID_CARDS     = (0x04 << 16),   // 封房(锁体设置卡)
    CF_SET_ONE_AREA     = (0x08 << 16),   // 清空客房信息(锁体设置卡)
    CF_CLR_ONE_AREA     = (0x10 << 16),   // 封房(锁体设置卡)
    CF_SET_ALL_AREA     = (0x20 << 16),   // 清空客房信息(锁体设置卡)
    CF_CLR_ALL_AREA     = (0x40 << 16),   // 封房(锁体设置卡)
};





// 制卡类型
enum MAKE_CARD_TYPE 
{
    CLIENT_CARD         =   0,      // 客户卡类型号
    GUEST_CARD	    	=   1,      // 宾客卡类型号
    FLOOR_CARD		    =   2,      // 楼层卡类型号
    BUILDING_CARD	    =   3,      // 楼栋卡类型号
    EMERGENT_CARD	    =   4,      // 应急卡类型号
    CHIEF_CARD		    =   5,      // 总　卡类型号
    INSTALL_CARD	    =   6,      // 安装卡类型号
    LOST_CARD		    =   7,      // 挂失卡类型号
    UN_LOST_CARD	    =   8,      // 取消挂失卡类型号
    TIME_CARD		    =   9,      // 校时卡类型号
    DATA_CARD		    =   10,     // 数据卡类型号
    FACTORY_CARD	    =   11,     // 工厂卡类型号
    TIME_STOP_CARD	    =   12,     // 终止卡类型号
    UPGRADE_CARD	    =   13, 	// 升级卡
    CHECKOUT_CARD       =   14,     // 退房卡
    OFFICE_CARD         =   15,     // 办公卡
    MANAGE_CARD         =   16,     // 清除卡(原管理卡)
    STAFF_CARD          =   17,      // 员工卡
    LOCK_SETTING_CARD   =   18,     // 锁体设置卡
    BACKUP_CARD         =   19,     // 后备卡
    ELEVATOR_CARD       =   20,     // 电梯卡
};


// 门锁类型
enum LOCK_TYPE 
{
    LT_CPU               =   1,      // TM卡锁
    LT_IC       	    =   2,      // 接触式IC卡锁
    LT_256      	    =   3,      // 256卡锁
    LT_T5557    	    =   4,      // T5557卡锁
    LT_MF1  		    =   5,      // MF1卡锁
    LT_MF0      	    =   6       // MF0卡锁
};



// 门锁系统标志1( LOCK_INFO.SysFlags[0] )
enum LOCK_SYS_FLAGS0
{
    LSF_LOW_BAT = 1,                // 低压标志
    LSF_BACK_LOCK = 2,          // 反锁标志
    LSF_DOOR_OPENED = 4        // 门锁已开标志
};

// 卡片数据信息结构体
struct CARD_INFO{
    char    CardNo[17];             // 卡号:            字符串, 16个字符
    int     CardType;               // 卡类型          
    char    BuildingList[30];       // 楼栋号          
    char    FloorList[30];          // 楼层列表 
    char    RoomList[2000];         // 房号列表
    char    RoomMask[13];           // 房号屏蔽码:      字符串, 12个字符
    char    SDateTime[20];          // 起始日期时间:    字符串, 19个字符, 格式 YYYY-MM-DD hh:mm:ss, 
    char    EDateTime[20];          // 结束日期时间:    字符串, 19个字符, 格式 YYYY-MM-DD hh:mm:ss, 
    char    StartTime1[9];          // 起始时间1:       字符串, 8个字符, 格式 hh:mm:ss
    char    EndTime1[9];            // 结束时间1:       字符串, 8个字符, 格式 hh:mm:ss
    char    StartTime2[9];          // 起始时间2:       字符串, 8个字符, 格式 hh:mm:ss
    char    EndTime2[9];            // 结束时间2:       字符串, 8个字符, 格式 hh:mm:ss
    char    StartTime3[9];          // 起始时间3:       字符串, 8个字符, 格式 hh:mm:ss
    char    EndTime3[9];            // 结束时间3:       字符串, 8个字符, 格式 hh:mm:ss

    int     iCardFlags;             // 卡片的标志字节
    char    LostCardNo[17];          // 挂失卡号:        字符串, 16个字符

    char    CardClientNo[9];        // 卡片加密用授权码:  字符串, 8个字符  
    char    NewClientNo[4];         // 新授权码（用于授权卡）:   字符串, 4个字符(HEX码) 
    int     MF1NewSecotr;           // Mifare 新的扇区号（用于授权卡）
    char    NewKeys[13];            // Mifare 新的密钥（用于工厂卡）: 字符串, 12个字符
    int     ReplaceNo;              // 替代编号
    int     NewReplaceNo;           // 授权卡中的新替代编号
    int     iRoomCnt;
    int     iAreaCnt;
    char    cAreaList[100];         // 区域列表
    int     FacultyNo;              // 院系号
    int     MajorNo;                // 专业号
    int     ClassNo;                // 班级号
    char    UserName[20];           // 姓名

    
    int     iForbidCardType;        // 禁止或者例外的卡片
    char    cForbidDateTime[20];    // 自动解封日期, 当前时间超过解封日期, 则自动解封

    char    cRFU1[200];              // 预留
    char    cRFU2[200];              // 预留
    char    cRFU3[200];              // 预留
    char    cRFU4[200];              // 预留

    int     iRefreshCnt;            // RFU1 预留, 如果本次卡片信息与上次不同, 则iRefreshCnt就会变动, 否则保持不变
    int     RFU2;                   // 预留
    int     RFU3;                   // 预留
    int     RFU4;                   // 预留   
};

// 门锁数据结构体
struct LOCK_INFO{
    int     Building;               // 楼栋号
    int     Floor;                  // 楼层号
    char    RoomNo[20];             // 房号:        字符串, 6个字符
    char    RoomMask[13];           // 房号屏蔽码:  字符串, 12个字符
    int     LockSetting;            // 门锁设置选项
    char    ClientPass[9];          // 客户码:      字符串, 8个字符
    char    StopTime[20];           // 终止时间:    字符串, 19个字符
    char    LockTime[20];           // 锁内时钟:    字符串, 19个字符
    int     RecordAddr;             // 开门记录地址
    char    sysFlags[2];            // 系统标志
    char    TempSn[17];             // 临客卡卡号:  字符串, 16个字符
    char    M1Keys[13];             // 密钥:        字符串, 12个字符
    int     M1Sector;               // Mifare卡使用的扇区号
    char    BlackList[40][17];      // 黑名单:      40个卡号, 每个卡号16个字符
    int     LowBatCnt;              // 低压后还可以开门的次数
    int     iForbidCardType;        // 要禁止或者例外的卡片. 如果是例外的卡片, 则应该是(卡片类型 + 128). 
    char    cForbidDateTime[20];    // 解封日期, 到期卡片自动恢复功能
    char    cAreaList[100];         // 区域号列表
    char    cReplaceNoList[200];     // 各种卡片的替代号列表
    char    HardwareVersion[32];    // 门锁硬件版本号
    char    FirmwareVersion[32];    // 门锁固件版本
    char    DateCode[32];           // 日期代码

    char    cRFU1[200];              // 预留
    char    cRFU2[200];              // 预留
    char    cRFU3[200];              // 预留
    char    cRFU4[200];              // 预留

    int     RFU1;                   // 预留
    int     RFU2;                   // 预留
    int     RFU3;                   // 预留
    int     RFU4;                   // 预留  
};


// 客房数据结构体, 供下载门锁数据使用
struct ROOM_INFO{
    UINT    Building;               // 楼栋号
    UINT    Floor;                  // 楼层号
    char    RoomNo[30];             // 房号:        字符串, 类似 001.002.00003
    UINT    RoomType;
    UINT    iAreaNo;                // 区域号
    UINT    Flags;                  // 
    char    cRFU1[100];             // 预留
    char    cRFU2[100];             // 预留
    UINT    iRFU1;                  // 预留
    UINT    iRFU2;                  // 预留
};



// 手持机信息结构体
struct HANDSIM_INFO{
	UINT    sn;						// 序列号
	char    hardwareVer[64];		// 硬件版本号
	char	firmwareVer[64];		// 固件版本号
	char	releaseDatetime[20];	// 发布时间
	char	deviceDescription[500];	// 设备描述信息
	char	opeatorPassword[20];	// 手持机操作员密码

	UINT	moduleEn;				// 功能使能信息, 反码(要置位某项功能时, 置位)
	UINT    iPara1;					// 设置参数1, 预留
	UINT    iPara2;					// 设置参数1, 预留

    char    cRFU1[100];             // 预留
    char    cRFU2[100];             // 预留
    char    cRFU3[100];             // 预留
    char    cRFU4[100];             // 预留
    UINT    iRFU1;                  // 预留
    UINT    iRFU2;                  // 预留
    UINT    iRFU3;                  // 预留
    UINT    iRFU4;                  // 预留
};


#endif          // __DEFINES_H__