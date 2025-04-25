using ESMART_HMS.Domain.Enum;
using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Text;

namespace ESMART.Presentation.LockSDK
{
    public class LockSDKHeaders
    {
        #region Declear DLL functions
        [DllImport("LockSDK.dll", EntryPoint = "TP_Configuration")]
        public static extern int TP_Configuration(short LockType);


        [DllImport("LockSDK.dll", EntryPoint = "TP_MakeGuestCardEx")]
        public static extern int TP_MakeGuestCardEx(StringBuilder card_snr, string room_no, string checkin_time, string checkout_time, short iflags);


        [DllImport("LockSDK.dll", EntryPoint = "LS_ReadGuestCard")]
        public static extern int LS_ReadGuestCard(char[] card_snr, StringBuilder room_no, StringBuilder checkin_time, StringBuilder checkout_time);


        [DllImport("LockSDK.dll", EntryPoint = "TP_ReadGuestCardEx")]
        public static extern int TP_ReadGuestCardEx(StringBuilder card_snr, StringBuilder room_no, StringBuilder checkin_time, StringBuilder checkout_time, ref int iflags);


        [DllImport("LockSDK.dll", EntryPoint = "LS_MakeGuestCard_EX1")]
        public static extern int LS_MakeGuestCard_EX1(char[] card_snr, string roomno, string areas,
            string floors, string intime, string outtime, CARD_FLAGS iflags);


        [DllImport("LockSDK.dll", EntryPoint = "LS_MakeLockSettingCard")]
        public static extern int LS_MakeLockSettingCard(
            StringBuilder cardSnr,
            uint iAreaNo,
            int iForbidCardType,
            [MarshalAs(UnmanagedType.LPStr)] string cForbidDateTime,
            [MarshalAs(UnmanagedType.LPStr)] string SDateTime,
            [MarshalAs(UnmanagedType.LPStr)] string EDateTime,
            int iRFU,
            int cRFU,
            int iFlags,
            int iReplaceNo,
            [MarshalAs(UnmanagedType.LPStr)] string cSTime1,
            [MarshalAs(UnmanagedType.LPStr)] string cETime1,
            [MarshalAs(UnmanagedType.LPStr)] string cSTime2,
            [MarshalAs(UnmanagedType.LPStr)] string cETime2,
            [MarshalAs(UnmanagedType.LPStr)] string cSTime3,
            [MarshalAs(UnmanagedType.LPStr)] string cETime3
        );

        [DllImport("LockSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "LS_MakeInstallCard")]
        public static extern int LS_MakeInstallCard(StringBuilder cardSnr,
            int Building,
            int Floor,
            string Room,
            ROOM_TYPE RoomType,
            int iAreaNo1,
            int iAreaNo2,
            LOCK_SETTING LockSetting,
            int iReplaceNo
        );

        [DllImport("LockSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "LS_MakeChiefCard")]
        public static extern int LS_MakeChiefCard(char[] cardSnr, string SDateTime, string EDateTime, CARD_FLAGS iFlags, int iReplaceNo);


        [DllImport("LockSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "LS_MakeBuildingCard")]
        public static extern int LS_MakeBuildingCard(char[] cardSnr, string BuildingList, string SDateTime, string EDateTime, int iFlags, int iReplaceNo);


        [DllImport("LockSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "LS_MakeFloorCard")]
        public static extern int LS_MakeFloorCard(char[] cardSnr, int Building, string FloorList, string cSTime1, string cETime1, string cSTime2, string cETime2, string cSTime3, string cETime3, string SDateTime, string EDateTime, CARD_FLAGS iFlags, int iReplaceNo);


        [DllImport("LockSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "LS_SelectDoorLockType")]
        public static extern int LS_SelectDoorLockType(int DoorType);


        [DllImport("LockSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "LS_ReadSystemPas")]
        public static extern int LS_ReadSystemPas(string funPsw, StringBuilder Password, char[] cardSnr);


        [DllImport("LockSDK.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "LS_SystemInitialization")]
        public static extern int LS_SystemInitialization(string funPsw, StringBuilder Password);


        [DllImport("LockSDK.dll", EntryPoint = "TP_CancelCard")]
        public static extern int TP_CancelCard(StringBuilder card_snr);


        [DllImport("LockSDK.dll", EntryPoint = "TP_GetCardSnr")]
        public static extern int TP_GetCardSnr(StringBuilder card_snr);


        [DllImport("LockSDK.dll", EntryPoint = "LS_ReadRom")]
        public static extern int LS_ReadRom(char[] card_snr);


        [DllImport("LockSDK.dll", EntryPoint = "LS_OpenPort")]
        public static extern int LS_OpenPort(int Port);


        [DllImport("LockSDK.dll", EntryPoint = "LS_ClosePort")]
        public static extern int LS_ClosePort(int Port);


        [DllImport("LockSDK.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "LS_GetCardInformation")]
        public static extern int LS_GetCardInformation(ref CARD_INFO cardInfo, int datFromFile, int infoPsw, nint dat);
        #endregion

        #region Public methods
        public static void CheckErr(int iret)
        {
            switch (iret)
            {
                case 1:
                    MessageBox.Show("Card reader/writer found", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case -1:
                    MessageBox.Show("Sorry invalid card", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case -2:
                    MessageBox.Show("Sorry no detected card reader/writer", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case -3:
                    MessageBox.Show("", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case -4:
                    MessageBox.Show("Card type error", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case -5:
                    MessageBox.Show("Read/write error", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case -8:
                    MessageBox.Show("Invalid Parameter", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case -29:
                    MessageBox.Show("Unregistered decoder", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                default:
                    MessageBox.Show("Sorry an error occured", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }

        public static bool PreparedIssue(char[] card_snr)
        {
            int st;
            st = LS_ReadRom(card_snr);
            int[] errors = { 1, 3, 4, 5 };
            if (st != 1)
            {
                CheckErr(st);
                return false;
            }
            return true;
        }
        #endregion


        #region RF DLL functions
        [DllImport("v9RF.dll", EntryPoint = "Buzzer")]
        public static extern int Buzzer(int sc);

        //读DLL版本号
        [DllImport("v9RF.dll", EntryPoint = "GetDLLVersionA")]
        public static extern int GetDLLVersionA(byte[] aType);

        [DllImport("v9RF.dll", EntryPoint = "initializeUSB")]
        public static extern int initializeUSB(int d12);

        //从发卡器中读取酒店标识
        [DllImport("v9RF.dll", EntryPoint = "Getcoid")]
        public static extern int Getcoid(byte[] dlscoid);

        //读卡数据
        [DllImport("v9RF.dll", EntryPoint = "ReadCard")]
        public static extern int ReadCard(byte[] carddata);

        //注销卡片
        [DllImport("v9RF.dll", EntryPoint = "CardEraseA")]
        public static extern int CardEraseA(int coid, byte[] carddata);

        //客人卡
        [DllImport("v9RF.dll", EntryPoint = "WriteGuestCardA")]
        public static extern int WriteGuestCardA(int dlscoid, byte cardno, byte dai, byte llock, char[] EDate, char[] RoomNo, byte[] cardhexstr);


        //获得客人卡信息
        [DllImport("v9RF.dll", EntryPoint = "GetGuestCardinfoA")]
        public static extern int GetGuestCardinfoA(int dlscoid, byte[] carddata, byte[] lockno);
        #endregion

    }

}
