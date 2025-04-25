using System;
using System.Runtime.InteropServices;

namespace ESMART_HMS.Domain.Enum
{
    [Flags]
    public enum LOCK_SETTING
    {
        LS_REPLACE_EN = 0x01,     // Enable guest card replacement
        LS_LEAD_EN = 0x02,     // Enable lead mode
        LS_VALID_DATE_EN = 0x04,     // Enable validity period
        LS_FLOOR_RANGE_EN = 0x08,     // Check floor range
        LS_CHANEL_MODE_EN = 0x10,     // Channel mode
        LS_FORBID_TONGUE_ALARM = 0x20,  // Disable latch alarm
        LS_ALL_REPLACE = 0x40,     // Enable replacement for various card types
        LS_FLAG_BYTE2_EN = 0x80,     // Enable guest room flag byte 2
        LS_LOCK_IMMEDIATE_EN = 0x0100,   // Immediate lock after retracting the latch
        LS_NO_BACKLOCK_EN = 0x0200,   // Disable backlock check for all cards
        LS_NO_MUSIC_EN = 0x0400,   // Mute door open/close sound
        LS_PROMPT_CLOSE_EN = 0x0800,   // Prompt guest to close the door
        LS_NO_BLOCK_LIGHT_EN = 0x1000,   // No light prompt when backlocking

        LOCK_TYPE = 5
    }

    public enum ROOM_TYPE
    {
        RT_COMMON_ROOMS = 0x1,      // Common rooms and large suites
        RT_SUITE_ROOMS = 0x2,      // Small suites within a larger suite
        RT_FLOOR_GATE = 0x4,      // Floor gate
        RT_BUILDING_GATE = 0x8,      // Building gate
        RT_HOTEL_GATE = 0x10      // Hotel gate
    }

    enum ERROR_TYPE
    {
        OPR_OK = 1,      // Operation successful
        NO_CARD = -1,     // No card detected
        NO_RW_MACHINE = -2,     // No card reader detected
        INVALID_CARD = -3,     // Invalid card
        CARD_TYPE_ERROR = -4,     // Card type error
        RDWR_ERROR = -5,     // Read/write error
        PORT_NOT_OPEN = -6,     // Port not open
        END_OF_DATA_CARD = -7,     // End of data card
        INVALID_PARAMETER = -8,     // Invalid parameter
        INVALID_OPR = -9,     // Invalid operation
        OTHER_ERROR = -10,    // Other error
        PORT_IN_USED = -11,    // Port is already in use
        COMM_ERROR = -12,    // Communication error
        ERR_RECOVER_CLIENT = -13,    // Successfully recovered authorization code from the card reader

        ERR_CLIENT = -20,    // Client code error
        ERR_LOST = -21,    // Card was lost
        ERR_TIME_INVALID = -22,    // Time is invalid
        ERR_TIME_STOPED = -23,    // Card has been deactivated
        ERR_BACK_LOCKED = -24,    // Back locked
        ERR_BUILDING = -25,    // Incorrect building number
        ERR_FLOOR = -26,    // Incorrect floor number
        ERR_ROOM = -27,    // Incorrect room number
        ERR_LOW_BAT = -28,    // Low battery

        ERR_NOT_REGISTERED = -29,    // Not registered
        ERR_NO_CLIENT_DATA = -30,    // No authorization card information
        ERR_ROOMS_CNT_OVER = -31,    // Number of rooms exceeds available sectors
    };

    public enum CARD_FLAGS
    {
        CF_BACK_LOCK_EN = 0x01,      // Enable back lock (various key cards)
        CF_CHANNEL_MODE = 0x02,      // Channel mode (various key cards)
        CF_DOUBLE_MODE = 0x04,      // Double mode in DJE environment; when paired with a guest card, the guest room time mark is merged. On 20191024, the guest card is moved to bit6 !!!!!!
        CF_CHECK_TIMESTAMP = 0x40,      // Update and check timestamp (guest card), used as a master card mark during the validity period	
        CF_NO_REPLACE = 0x08,      // Do not replace the previous card (guest card)	
        CF_IMPORT_ROOM = 0x10,      // Import room number for guest card
        CF_OPEN_ONCE = 0x20,      // Open the door only once (single-use guest card)
        CF_NEW_CARD = 0x40,      // In DLock version, indicates a new guest entry. Clear balance when copying guest card.
        CF_CHK_CHECKIN_TIME = 0x80,   // Check check-in time

        CF_PROHIBIT_DATA_EN = 0x10,      // Prohibit entry data for staff card	
        CF_REPLACE_EN = 0x40,      // Enable replacement (staff card)
        CF_AUTO_LOST_EN = 0x80,      // Auto-loss card (staff card)

        CF_CHECKOUT_GUEST_ONLY = 0x10,      // Only check out guest card
        CF_CHECKOUT_STAFF_ONLY = 0x20,      // Only check out staff card
        CF_WHOLE_BUILDING = 0x40,      // Whole building permission (room card)
        CF_WHOLE_HOTEL = 0x80,      // Whole system permission (room card)

        CF_JUDGE_CHECKIN_TIME = 0x80,      // Enable check-in time judgement (guest card)   

        CF_CHANGE_FLAGS = (0x01 << 16),   // Change lock flag
        CF_CLEAR_ROOM_INFO = (0x02 << 16),   // Clear guest room information (lock configuration card)
        CF_FORBID_CARDS = (0x04 << 16),   // Block room (lock configuration card)
        CF_SET_ONE_AREA = (0x08 << 16),   // Clear guest room information (lock configuration card)
        CF_CLR_ONE_AREA = (0x10 << 16),   // Block room (lock configuration card)
        CF_SET_ALL_AREA = (0x20 << 16),   // Clear guest room information (lock configuration card)
        CF_CLR_ALL_AREA = (0x40 << 16),   // Block room (lock configuration card)
        CF_SET_CHANNEL_TIME = (0x80 << 16),   // Set channel door time
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CARD_INFO
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public byte[] CardNo;

        public int CardType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public byte[] BuildingList;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public byte[] FloorList;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2000)]
        public byte[] RoomList;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public byte[] RoomMask;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] SDateTime;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] EDateTime;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] StartTime1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] EndTime1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] StartTime2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] EndTime2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] StartTime3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] EndTime3;

        public int iCardFlags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public byte[] LostCardNo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] CardClientNo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] NewClientNo;

        public int MF1NewSecotr;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
        public byte[] NewKeys;

        public int ReplaceNo;

        public int NewReplaceNo;

        public int iRoomCnt;

        public int iAreaCnt;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public byte[] cAreaList;

        public int FacultyNo;

        public int MajorNo;

        public int ClassNo;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] UserName;

        public int iForbidCardType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] cForbidDateTime;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] cSummerTimeStart;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] cSummerTimeEnd;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public byte[] cStaffProhibitedDays;

        public byte ClrReplaceOfCardType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 59)]
        public byte[] cRFU2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public byte[] cRFU3;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        public byte[] cRFU4;

        public int iRefreshCnt;

        public int iStaffProhibitedDayOfWeek;

        public int RFU3;

        public int RFU4;
    }

    public enum MakeCardType
    {
        ClientCard = 0,      // Client Card Type
        GuestCard = 1,      // Guest Card Type
        FloorCard = 2,      // Floor Card Type
        BuildingCard = 3,      // Building Card Type
        EmergentCard = 4,      // Emergent Card Type
        MasterCard = 5,      // Chief Card Type
        RoomSettingsCard = 6,      // Install Card Type
        LostCard = 7,      // Lost Card Type
        UnLostCard = 8,      // Unlost Lost Card Type
        TimeCard = 9,      // Time Card Type
        DataCard = 10,     // Data Card Type
        FactoryCard = 11,     // Factory Card Type
        TimeStopCard = 12,     // Time Stop Card Type
        UpgradeCard = 13,     // Upgrade Card
        CheckoutCard = 14,     // Checkout Card
        OfficeCard = 15,     // Office Card
        ClearCard = 16,     // Clear Card (formerly Manage Card) 20200526:MANAGE_CARD-->CLEAR_CARD
        StaffCard = 17,     // Staff Card
        LockSettingCard = 18,     // Lock Setting Card
        BackupCard = 19,     // Backup Card
        ElevatorCard = 20,     // Elevator Card
        JoinNetCard = 21,     // Join Network Card
        ExitNetCard = 22,     // Exit Network Card
        ParaSettingCard = 23,     // Parameter Setting Card
        AcsElevatorSetCard = 24      // Access Control System Elevator Setting Card
    }

}
