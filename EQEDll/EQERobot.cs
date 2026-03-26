using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace HMTesterStation.EQE
{
    public delegate void UpdateUIMSiteIDEvent(uint siteID);

    public class EQERobot
    {
        private static EQERobot Instance = null;
        private static object locker = new object();
        private static string mPort;
        private static uint XSiteID;
        private static uint YSiteID;
        private static UInt16 PreviousPos = 1;

        private EQERobot()
        {
            //串口参数
            mPort = "9";
            InitPara();
        }

        ~EQERobot()
        {
            //Close();
        }

        public static EQERobot GetInstance()
        {
            //先检查Instance是否为null，防止每次调用都锁定locker，影响性能
            if (Instance == null)
            {
                lock (locker)
                {
                    if (Instance == null)
                    {
                        Instance = new EQERobot();
                    }
                }
            }
            return Instance;
        }
        public event UpdateUIMSiteIDEvent OnUpdateUIMSiteIDEvent;

        #region
        const int MAX_GATEWAY_COUNT = 5;
        const int MAX_SUBCAN_NUM = 126;

        const int UIDEV_RS232CAN = 0x01;
        const int UIDEV_WIFI2RS232CAN = 0x02;
        const int UIDEV_USBCAN = 0x04;
        const int UIDEV_PCICAN = 0x08;
        const int UIDEV_ETHCAN = 0x10;
        const int THIRD_PARTY_DEV = 0x20;
        const int UIDEV_ALL = (UIDEV_RS232CAN | UIDEV_WIFI2RS232CAN | UIDEV_USBCAN | UIDEV_PCICAN | UIDEV_ETHCAN | THIRD_PARTY_DEV);
        public struct DEV_INFO_OBJ
        {
            public uint dwDevType;		        //0x11代表RS232CAN 0x20代表PCICAN 
            public uint dwDevIndex;		        //软件启动时为设备分配的ID
            public uint uiComIndex;               //系统分配的COM编号
            public uint uiBaudRate;				//COM口对应的波特率
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string pszDevName;  //设备型号和名字
            public uint Protocol;                 //是字符串传输还是数据传输
        }
        public struct GW_SEARCH_PARA
        {
            public uint dwComIndex;
            public uint dwBtr;
        }

        public enum SValue
        {
            NoActionNoNotify = 0,//无动作，无状态通知
            NoCationNotifyByIE = 1,//无动作,状态取决于(MCFG<SXIE>)
            StartandRunReversely = 2,//反向连续运行
            StartandRunForwardly = 10,//正向连续运行
            DecelerateuntilStop = 3,//减速直到停止
            ResetAndDecelerateuntilStop = 11,//绝对位置清零+减速直到停止
            ResetAndDisplacementControl = 7,//绝对位置清零+位移控制
            EmergencyStop = 4,//紧急停止
            ResetAndEmergencyStop = 12,//绝对位置清零+紧急停止
            ReverseDisplacementControl = 5,//反向先对位移控制
            ForwardDisplacementControl = 13,//正向相对位移控制
            ZeroAbsolutePosition = 6, //绝对位置清零
            ChangedirectionDisplacementControl = 9,//换向(DIR=~DIR)相对位移控制
            ChangedirectionAndRun = 14,//换向(DIR=~DIR)连续运行
            MotorOFF = 15 //脱机
        }

        public enum STLValue
        {
            NoActionNoNotify = 0,//无动作，无状态通知
            NoCationNotifyByIE = 1,//无动作,状态取决于(MCFG<SXIE>)
            EmergencyStop = 4,//紧急停止
            Reset = 6,//设置零位
            ChangedirectionDisplacementControl = 9,//换向(DIR=~DIR)相对位移控制
            ResetAndEmergencyStop = 12,//绝对位置清零+紧急停止
            ChangedirectionAndRun = 14,//换向(DIR=~DIR)连续运行
            MotorOFF = 15 //脱机
        }

        //电机型号和固件号信息
        public struct MDL_INFO_OBJ
        {
            public uint uiCANNodeID;					//驱动器标识码
            public uint uiCANNodeType;				//驱动器型号
            public uint uiCurrent;						//电流
            public uint bIntegrationEncode;		//内置编码器
            public uint bEnCode;						//闭环控制
            public uint bMotion;						//高级运动控制
            public uint b2Sensor;						//"-SP"
            public uint b4Sensor;                       //"-S"
            public uint uiFirewareVersion;			//驱动器固件版本
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string szModelName;		//驱动器型号
        }

        //Basic Instruction Acknowledgment
        public struct BASIC_ACK_OBJ
        {
            public uint uiReserv;         //0
            public byte bENA;             //电机使能状态
            public byte bDIR;             //电机方向
            public byte bACR;             //电流减半
            public uint uiMCS;             //电机细分
            public uint uiCUR;             //电流
            public uint uiSPD;             //当前速度
            public uint uiSTP;             //当前步长
        };

        //Basic Instruction Feedback
        public struct BASIC_FBK_OBJ
        {
            public uint uiReserv;         //0
            public byte bENA;             //电机使能状态
            public byte bDIR;             //电机方向
            public byte bACR;             //电流减半
            public uint uiMCS;             //电机细分
            public uint uiCUR;             //电流
            public uint uiSPD;             //当前速度
            public uint uiSTP;             //当前步长
        };

        //传感器返回信息结构
        public struct SFBK_INFO_OBJ
        {
            public uint bSensor1;      //S1传感器的电平逻辑值
            public uint bSensor2;      //S2传感器的电平逻辑值
            public uint bSensor3;      //S3传感器的电平逻辑值
            public float fAnaInput;       //The converted value for analog input (12 bit)
        };

        public struct P_S12CON
        {
            public uint uiS2RACT;
            public uint uiS2FACT;
            public uint uiS1RACT;
            public uint uiS1FACT;
        };

        public struct P_S34CON
        {
            public uint bP4LVL;
            public uint uiP4EVENT;
            public uint uiS3RACT;
            public uint uiS3FACT;
            public uint uiSTLValue;
        };

        public struct UIM_MCFG_INFO_OBJ
        {
            public uint uiMcfgVal;
        };

        public struct UIM_ICFG_INFO_OBJ
        {
            public uint uiIcfgVal;
        };

        public struct GW_DIGITAL_IO_STATUS_OBJ
        {
            public uint uiIOStatus;
        };
        public struct GW_DIGITAL_OUTPUT_OBJ
        {
            public uint uiOutPut;
        };
        public struct CAN_MSG_OBJ
        {
            public int ID;                    //报文 ID  = SID(11位) | EID (18位)
            public int Reserved0;         //保留, 赋值0
            public byte Reserved1;         //保留, 赋值0
            public byte SendType;		    //0：正常发送，1：自发自收
            public byte IDE;	                //0：标准帧，  1：扩展帧
            public byte RTR;	                //0：数据帧，  1：远程帧
            public byte DataLen;		    //表明Data[8]数组内的的字节数，长度不能超过8；
            //CAN数据包原为8个字节，为了支持RS232，此数据的长度增加为128
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data;
            //系统保留
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserved;
        };


        //UID828 AVA结构
        public struct AVA_INFO_OBJ
        {
            public int objAVAPort;
            public int nP1Analog;
            public int nP2Analog;
            public int nP3Analog;
            public int nP4Analog;
            public int nP5Analog;
            public int nP6Analog;
            public int nP7Analog;
            public int nP8Analog;
        }


        //函数导入
        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SearchGateway", CharSet = CharSet.Ansi)]
        private static extern int SearchGateway(int dwGatewayType, ref GW_SEARCH_PARA pGWSearchPara, IntPtr pDevInfoObj, int iLen);
        
        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "OpenGateway", CharSet = CharSet.Ansi)]
        private static extern int OpenGateway(uint dwDevIndex, Int32[] pDevIndexList, int ilen, ref int canBtr);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "CloseGateway", CharSet = CharSet.Ansi)]
        private static extern int CloseGateway(uint dwDevIndex);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SearchDevice", CharSet = CharSet.Ansi)]
        private static extern int SearchDevice(uint dwDevType, GW_SEARCH_PARA[] pGWSearchPara);

        [DllImport("UISimCanFunc.dll", EntryPoint = "GetUimDevIdList", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetUimDevIdList(uint dwDevType, uint[] pDevIndexList);

        [DllImport("UISimCanFunc.dll", EntryPoint = "GetUimDevInfo", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetUimDevInfo(uint dwDevType, ref DEV_INFO_OBJ devInfoObj);

        [DllImport("UISimCanFunc.dll", EntryPoint = "OpenUimDev", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int OpenUimDev(uint dwDevType);

        [DllImport("UISimCanFunc.dll", EntryPoint = "CloseUimDev", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseUimDev(uint dwDevType);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetUimDevIdList", CharSet = CharSet.Ansi)]
        private static extern int GetUimDevIdList(uint dwDevType, Int32[] pDevIndexList);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UimGrobReg", CharSet = CharSet.Ansi)]
        private static extern int UimGrobReg(uint dwDevType, uint[] pCanNodeIdList, uint ilen);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetMDL", CharSet = CharSet.Ansi)]
        private static extern int GetMDL(uint dwDevType, uint dwCanNodeId, ref MDL_INFO_OBJ pMDLInfoObj);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UimENA", CharSet = CharSet.Ansi)]
        private static extern int UimENA(uint dwDevType, uint dwCanNodeId, bool bAckEna, ref BASIC_ACK_OBJ pBasicAckobj);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UimOFF", CharSet = CharSet.Ansi)]
        private static extern int UimOFF(uint dwDevType, uint dwCanNodeId, bool bAckEna, ref BASIC_ACK_OBJ pBasicAckobj);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetORG", CharSet = CharSet.Ansi)]
        private static extern int SetORG(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UimFBK", CharSet = CharSet.Ansi)]
        private static extern int UimFBK(uint dwDevType, uint dwCanNodeId, ref BASIC_FBK_OBJ pFBKInfo);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UimSFBK", CharSet = CharSet.Ansi)]
        private static extern int UimSFBK(uint dwDevType, uint dwCanNodeId, ref SFBK_INFO_OBJ pSFBKInfo);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSPD", CharSet = CharSet.Ansi)]
        private static extern int SetSPD(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetSTP", CharSet = CharSet.Ansi)]
        private static extern int SetSTP(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetPOS", CharSet = CharSet.Ansi)]
        private static extern int SetPOS(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetQEC", CharSet = CharSet.Ansi)]
        private static extern int SetQEC(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetSPD", CharSet = CharSet.Ansi)]
        private static extern int GetSPD(uint dwDevType, uint dwCanNodeId, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetSTP", CharSet = CharSet.Ansi)]
        private static extern int GetSTP(uint dwDevType, uint dwCanNodeId, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetPOS", CharSet = CharSet.Ansi)]
        private static extern int GetPOS(uint dwDevType, uint dwCanNodeId, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetQEC", CharSet = CharSet.Ansi)]
        private static extern int GetQEC(uint dwDevType, uint dwCanNodeId, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "UIMRegRtcnCallBack", CharSet = CharSet.Ansi)]
        private static extern int UIMRegRtcnCallBack(uint dwDevType, uint dwDevIndex, ProcessDelegate pFunc);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetUimMCFG", CharSet = CharSet.Ansi)]
        private static extern int SetUimMCFG(uint dwDevType, uint dwCanNodeId, ref UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_IN, bool bAckEna, ref UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_OUT);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetUimMCFG", CharSet = CharSet.Ansi)]
        private static extern int GetUimMCFG(uint dwDevType, uint dwCanNodeId, ref UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_OUT);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetUimICFG", CharSet = CharSet.Ansi)]
        private static extern int SetUimICFG(uint dwDevType, uint dwCanNodeId, ref UIM_ICFG_INFO_OBJ pUIM_ICFG_INFO_OBJ_IN, bool bAckEna, ref UIM_ICFG_INFO_OBJ pUIM_ICFG_INFO_OBJ_OUT);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetUimICFG", CharSet = CharSet.Ansi)]
        private static extern int GetUimICFG(uint dwDevType, uint dwCanNodeId, ref UIM_ICFG_INFO_OBJ pUIM_ICFG_INFO_OBJ_OUT);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetS12CON", CharSet = CharSet.Ansi)]
        private static extern int SetS12CON(uint dwDevType, uint dwCanNodeId, ref P_S12CON pS12CON_IN, bool bAckEna, ref P_S12CON pS12CON_OUT);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetS12CON", CharSet = CharSet.Ansi)]
        private static extern int GetS12CON(uint dwDevType, uint dwCanNodeId, ref P_S12CON pS12CON);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetS34CON", CharSet = CharSet.Ansi)]
        private static extern int SetS34CON(uint dwDevType, uint dwCanNodeId, ref P_S34CON pS34CON_IN, bool bAckEna, ref P_S34CON pS34CON_OUT);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetS34CON", CharSet = CharSet.Ansi)]
        private static extern int GetS34CON(uint dwDevType, uint dwCanNodeId, ref P_S34CON pS34CON);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetIOOutput", CharSet = CharSet.Ansi)]
        private static extern int GetIOOutput(uint dwDevIndex, uint dwCanSlaveId, ref GW_DIGITAL_IO_STATUS_OBJ pDigitalValueIn);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetIOOutput", CharSet = CharSet.Ansi)]
        private static extern int SetIOOutput(uint dwDevIndex, uint dwCanSlaveId, ref GW_DIGITAL_OUTPUT_OBJ pDigitalValueIn, bool bAckEna, ref GW_DIGITAL_IO_STATUS_OBJ pDigitalValueOut);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Uim900SetBtr", CharSet = CharSet.Ansi)]
        private static extern int Uim900SetBtr(uint dwDevIndex, uint dwCanSlaveId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Uim900Send", CharSet = CharSet.Ansi)]
        private static extern int Uim900Send(uint dwDevIndex, uint dwCanSlaveId, char[] pSendDate, int len);
        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Uim900Recv", CharSet = CharSet.Ansi)]
        private static extern int Uim900Recv(uint dwDevIndex, uint dwCanSlaveId, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pSendDate, int len);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetmACC", CharSet = CharSet.Ansi)]
        private static extern int SetmACC(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetmACC", CharSet = CharSet.Ansi)]
        private static extern int GetmACC(uint dwDevType, uint dwCanNodeId, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetmDEC", CharSet = CharSet.Ansi)]
        private static extern int SetmDEC(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetmDEC", CharSet = CharSet.Ansi)]
        private static extern int GetmDEC(uint dwDevType, uint dwCanNodeId, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetmMDS", CharSet = CharSet.Ansi)]
        private static extern int SetmMDS(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetmMDS", CharSet = CharSet.Ansi)]
        private static extern int GetmMDS(uint dwDevType, uint dwCanNodeId, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SetmMSS", CharSet = CharSet.Ansi)]
        private static extern int SetmMSS(uint dwDevType, uint dwCanNodeId, int iValue, bool bAckEna, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetmMSS", CharSet = CharSet.Ansi)]
        private static extern int GetmMSS(uint dwDevType, uint dwCanNodeId, ref int pRtnValue);

        [DllImport("UISimCanFunc.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetAVAInfo", CharSet = CharSet.Ansi)]
        private static extern int GetAVAInfo(uint dwDevType, uint dwCanNodeId, ref AVA_INFO_OBJ pAvaInfoIn, bool bAckEna, ref AVA_INFO_OBJ pAvaInfoOut);


        bool m_bConnectDevice = false;
        uint[] m_DevIDList;
        uint m_uiDevIndex = 0;
        uint m_dwDevType = 0;
        int m_uiSubCANCount = -1;
        GW_SEARCH_PARA[] m_pSearchPara;
        bool m_bDevCnectFlg;
        // DEV_INFO_OBJ[] m_DevInfo;
        IntPtr m_DevInfo;
        DEV_INFO_OBJ[] gatewayDevInfo;
        public static uint m_nSTPIE = 0;
        public static uint m_nS1L = 0;
        public static uint m_nS2L = 0;

        public uint nSiteID;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ProcessDelegate(uint dwDevIndex, ref CAN_MSG_OBJ can_msg_obj, int dwMsgLen);
        ProcessDelegate pd = new ProcessDelegate(ProcessSensor);
        //public ProcessDelegate m_delRtcnProcess = null;
        //处理传感器事件
        public static void ProcessSensor(uint dwDevIndex, ref CAN_MSG_OBJ can_msg_obj, int dwMsgLen)
        {

            string strSensorInfo = string.Empty;
            int dwCanNodeId = ((can_msg_obj.ID >> 3) & 0x060) + ((can_msg_obj.ID >> 19) & 0x01F);
            int iRCW = can_msg_obj.ID & 0xFF;
            int iData = can_msg_obj.Data[0] & 0xFF;

            switch (iRCW)
            {
                case 62:
                    strSensorInfo = string.Format("Site{0}:900 Data  {1}, {2}, {3}, {4}\r\n", dwCanNodeId, can_msg_obj.Data[0], can_msg_obj.Data[1], can_msg_obj.Data[2], can_msg_obj.Data[3]);
                    break;
                case 113:
                    //m_nS1L = 1;
                    strSensorInfo = string.Format("Site{0}:S1 faileding edge\r\n", dwCanNodeId);
                    break;
                case 113 + (1 << 7):
                    strSensorInfo = string.Format("Site{0}:S1 rising edge\r\n", dwCanNodeId);
                    break;
                case 114:
                    m_nS2L = 1;
                    strSensorInfo = string.Format("Site{0}:S2 faileding edge\r\n", dwCanNodeId);
                    break;
                case 114 + (1 << 7):
                    strSensorInfo = string.Format("Site{0}:S2 rising edge\r\n", dwCanNodeId);
                    break;
                case 115:
                    strSensorInfo = string.Format("Site{0}:S3 faileding edge\r\n", dwCanNodeId);
                    break;
                case 115 + (1 << 7):
                    strSensorInfo = string.Format("Site{0}:S3 rising edge\r\n", dwCanNodeId);
                    break;
                case 116:
                    strSensorInfo = string.Format("Site{0}:TTL output P4 low\r\n", dwCanNodeId);
                    break;
                case 116 + (1 << 7):
                    strSensorInfo = string.Format("Site{0}:TTL output P4 high\r\n", dwCanNodeId);
                    break;
                case 117:
                    m_nSTPIE = 1;
                    strSensorInfo = string.Format("Site{0}:displacement control complete(Open Loop)\r\n", dwCanNodeId);
                    break;
                case 117 + (1 << 7):
                    m_nSTPIE = 1;
                    strSensorInfo = string.Format("Site{0}:displacement control complete(Close Loop)\r\n", dwCanNodeId);
                    break;
                case 118:
                    strSensorInfo = string.Format("Site{0}:zero position\r\n", dwCanNodeId);
                    break;
                case 119:
                    strSensorInfo = string.Format("Site{0}: Stalling \r\n", dwCanNodeId);
                    break;
                case 112:
                    strSensorInfo = string.Format("Site{0}: 828 Channel{1} faileding edge\r\n", dwCanNodeId, iData);
                    break;
                case 112 + (1 << 7):
                    strSensorInfo = string.Format("Site{0}: 828 Channel{1} rising edge\r\n", dwCanNodeId, iData);
                    break;
                default:
                    break;
            }

            //ShowRTCN(strSensorInfo);
        }

        //    public delegate void DelegateRTCN(string strRTCN);
        //    DelegateRTCN delShowRTCN;
        public void ThreadStart(Action m)
        {
            System.Threading.ThreadStart threadMethod;
            threadMethod = new System.Threading.ThreadStart(m);
            System.Threading.Thread thread = new System.Threading.Thread(threadMethod);
            thread.Start();
        }
        #endregion

        public void Enable()
        {
            BASIC_ACK_OBJ objBasicAck = new BASIC_ACK_OBJ();
            UimENA(m_uiDevIndex, nSiteID, true, ref objBasicAck);
        }

        public void Disable()
        {
            BASIC_ACK_OBJ objBasicAck = new BASIC_ACK_OBJ();
            UimOFF(m_uiDevIndex, nSiteID, true, ref objBasicAck);
        }

        public bool SetORG()
        {
            int nRtnValue = -1;
            return -1 == SetORG(m_uiDevIndex, nSiteID, 0, true, ref nRtnValue);
        }

        public void FBK(ref string info)
        {
            BASIC_FBK_OBJ objFBKBasic = new BASIC_FBK_OBJ();
            if (-1 == UimFBK(m_uiDevIndex, nSiteID, ref objFBKBasic))
            {
                info = "Execute failed!";
                return;
            }
            else
            {
                info = string.Format("ENA {0}, DIR {1}, ACR {2}, MCS{3}, CUR {4}, SPD {5}, STP {6}",
                    objFBKBasic.bENA,
                    objFBKBasic.bDIR,
                    objFBKBasic.bACR,
                    objFBKBasic.uiMCS,
                    objFBKBasic.uiCUR,
                    objFBKBasic.uiSPD,
                    objFBKBasic.uiSTP);
            }
        }

        public bool SetSPD(Int32 speed)
        {
            UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_IN = new UIM_MCFG_INFO_OBJ();
            UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_OUT = new UIM_MCFG_INFO_OBJ();

            //BASIC_ACK_OBJ objBasicAck = new BASIC_ACK_OBJ();
            P_S12CON pS12CON_OUT = new P_S12CON();
            P_S12CON pS12CON_IN = new P_S12CON();
            pS12CON_IN.uiS1FACT = 0x0;
            pS12CON_IN.uiS1RACT = 0x0;
            pS12CON_IN.uiS1FACT = 0x0;
            pS12CON_IN.uiS1RACT = 0x3;

            //P_S34CON pS34CON_OUT = new P_S34CON();
            //P_S34CON pS34CON_IN = new P_S34CON();


            SetS12CON(m_uiDevIndex, nSiteID, ref pS12CON_IN, true, ref pS12CON_OUT);
            //SetS34CON(m_uiDevIndex, nSiteID, ref pS34CON_IN, true, ref pS34CON_OUT);

            GetUimMCFG(m_uiDevIndex, nSiteID, ref pUIM_MCFG_INFO_OBJ_IN);
            if ((pUIM_MCFG_INFO_OBJ_IN.uiMcfgVal & 0x01) != 0x01)
            {
                pUIM_MCFG_INFO_OBJ_IN.uiMcfgVal |= 0x01;
                SetUimMCFG(m_uiDevIndex, nSiteID, ref pUIM_MCFG_INFO_OBJ_IN, true, ref pUIM_MCFG_INFO_OBJ_OUT);
            }

            int nRtnValue = -1;
            if (-1 == SetSPD(m_uiDevIndex, nSiteID, speed, true, ref nRtnValue))
            {
                return false;
            }
            else
            {
                //判断是否返回值和设置是否相同
                if (speed != nRtnValue)
                {
                    return false;
                }
            }
            return true;
        }

        public bool QuerySPD(ref int nRtnValue)
        {
            return (-1 == GetSPD(m_uiDevIndex, nSiteID, ref nRtnValue));
        }

        public bool SetSTP(Int32 stp)
        {
            int nRtnValue = -1;
            if (-1 == SetSTP(m_uiDevIndex, nSiteID, stp, true, ref nRtnValue))
            {
                return false;
            }
            else
            {
                //判断是否返回值和设置是否相同
                if (stp != nRtnValue)
                {
                    return false;
                }
            }
            return true;
        }

        public bool QuerySTP(ref int nRtnValue)
        {
            return (-1 == GetSTP(m_uiDevIndex, nSiteID, ref nRtnValue));
        }

        public bool SetPOS(Int32 pos)
        {
            int nRtnValue = -1;
            if (-1 == SetPOS(m_uiDevIndex, nSiteID, pos, true, ref nRtnValue))
            {
                return false;
            }
            else
            {
                //判断是否返回值和设置是否相同
                if (pos != nRtnValue)
                {
                    return false;
                }
            }
            return true;
        }

        public bool QueryPOS(ref int nRtnValue)
        {
            return (-1 == GetPOS(m_uiDevIndex, nSiteID, ref nRtnValue));
        }

        public bool SetQEC(Int32 qec)
        {
            int nRtnValue = -1;
            if (-1 == SetQEC(m_uiDevIndex, nSiteID, qec, true, ref nRtnValue))
            {
                return false;
            }
            else
            {
                //判断是否返回值和设置是否相同
                if (qec != nRtnValue)
                {
                    return false;
                }
            }
            return true;
        }

        public bool QueryQEC(ref int nRtnValue)
        {
            return (-1 == GetQEC(m_uiDevIndex, nSiteID, ref nRtnValue));
        }

        public void IOTest(ref string info)
        {
            //以下代码展示，如果通过SDK函数设置绑定传感器S1的
            UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_IN = new UIM_MCFG_INFO_OBJ();
            UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_OUT = new UIM_MCFG_INFO_OBJ();

            if (-1 == GetUimMCFG(m_uiDevIndex, nSiteID, ref pUIM_MCFG_INFO_OBJ_IN))
            {
                info = "GetUimMCFG failed!";
                return;
            }
            //判断是否开启了S1IE,开启S1后，当发生S12事件后，可通过回调函数RTCN获取到S1的触发消息
            if ((pUIM_MCFG_INFO_OBJ_IN.uiMcfgVal & 0x01) != 1)//S1在MCFG寄存器里处于第一个bit位,参考SDK手册
            {
                //未开启S1IE，则进行开启
                pUIM_MCFG_INFO_OBJ_IN.uiMcfgVal |= 1;
                if (-1 == SetUimMCFG(m_uiDevIndex, nSiteID, ref pUIM_MCFG_INFO_OBJ_IN, true, ref pUIM_MCFG_INFO_OBJ_OUT))
                {
                    info = "SetUimMCFG failed!";
                    return;
                }
            }

            P_S12CON s12ConIn = new P_S12CON();
            P_S12CON s12ConOut = new P_S12CON();
            //绑定S1下降沿为紧急停止加清零,实际情况根据需要设置动作代码
            s12ConIn.uiS1FACT = (uint)SValue.ResetAndEmergencyStop;
            s12ConIn.uiS1RACT = (uint)SValue.NoActionNoNotify;
            s12ConIn.uiS2FACT = (uint)SValue.NoActionNoNotify;
            s12ConIn.uiS2RACT = (uint)SValue.NoActionNoNotify;

            if (-1 == SetS12CON(m_uiDevIndex, nSiteID, ref s12ConIn, true, ref s12ConOut))
            {
                info = "SetS12CON failed!";
            }
            else
            {
                //正确，则进行设置值和返回值比较
                if (s12ConIn.uiS1FACT != s12ConOut.uiS1FACT
                    || s12ConIn.uiS1RACT != s12ConOut.uiS1RACT
                    || s12ConIn.uiS2FACT != s12ConOut.uiS2FACT
                    || s12ConIn.uiS2RACT != s12ConOut.uiS2RACT)
                {
                    info = "UIM data check failed!";
                }
            }

            if (-1 == GetUimMCFG(m_uiDevIndex, nSiteID, ref pUIM_MCFG_INFO_OBJ_IN))
            {
                info = "GetUimMCFG failed!";
                return;
            }

            //判断是否开启了STLIE,开启STLIE后，当发生堵状事件后，可通过回调函数RTCN获取到堵状的触发消息
            if (((pUIM_MCFG_INFO_OBJ_IN.uiMcfgVal & 0x40) >> 6) != 1)//STLIE在MCFG寄存器里处于第七个bit位，参考SDK手册
            {
                //未开启STLIE，则进行开启
                pUIM_MCFG_INFO_OBJ_IN.uiMcfgVal |= (1 << 6);
                if (-1 == SetUimMCFG(m_uiDevIndex, nSiteID, ref pUIM_MCFG_INFO_OBJ_IN, true, ref pUIM_MCFG_INFO_OBJ_OUT))
                {
                    info = "SetUimMCFG failed!";
                    return;
                }
            }

            P_S34CON s34ConIn = new P_S34CON();
            P_S34CON s34ConOut = new P_S34CON();
            //获取S34Con配置,先获取，再修改相对应的字段
            if (-1 == GetS34CON(m_uiDevIndex, nSiteID, ref s34ConIn))
            {
                info = "UIM GetS34CON failed.";
                return;
            }
            //设置堵状绑定动作
            s34ConIn.uiSTLValue = (uint)STLValue.EmergencyStop;
            if (-1 == SetS34CON(m_uiDevIndex, nSiteID, ref s34ConIn, true, ref s34ConOut))
            {
                info = "UIM SetS34CON failed.";
                return;
            }
            else
            {
                //正确，则进行设置值和返回值比较
                if (s34ConIn.bP4LVL != s34ConOut.bP4LVL
                    || s34ConIn.uiP4EVENT != s34ConOut.uiP4EVENT
                    || s34ConIn.uiS3FACT != s34ConOut.uiS3FACT
                    || s34ConIn.uiS3RACT != s34ConOut.uiS3RACT
                    || s34ConIn.uiSTLValue != s34ConOut.uiSTLValue)
                {
                    info = "UIM data check failed!";
                }
            }
            info = "Test Success Finished!";
        }

        public ObservableCollection<string> SearchDev()
        {
            ObservableCollection<string> list_dev = new ObservableCollection<string>();
            //显示所有设备
            //清空CAN站点列表显示框
            if (m_bDevCnectFlg)
            {
                //设备已经连接，断开
                if (-1 == CloseGateway(m_uiDevIndex))
                {

                }//
                m_bDevCnectFlg = false;
            }
            GW_SEARCH_PARA spara = new GW_SEARCH_PARA();
            //指定串口COM3
            spara.dwComIndex = Convert.ToUInt16(mPort);
            //指定波特率
            spara.dwBtr = 9600;
            //查找网关
            int dwGateWayLen = SearchGateway(UIDEV_RS232CAN, ref spara, m_DevInfo, 3);

            if (dwGateWayLen > 0)
            {
                for (int i = 0; i < dwGateWayLen; i++)
                {
                    gatewayDevInfo[i] = (DEV_INFO_OBJ)Marshal.PtrToStructure((IntPtr)(m_DevInfo.ToInt64() + i * Marshal.SizeOf(typeof(DEV_INFO_OBJ))), typeof(DEV_INFO_OBJ));
                    if (gatewayDevInfo[i].pszDevName != null)
                        list_dev.Add(gatewayDevInfo[i].pszDevName);
                }
            }
            return list_dev;
        }

        public ObservableCollection<EQEDevInfo> OpenDev(int selected_uim_dev_index)
        {
            ObservableCollection<EQEDevInfo> dev_list = new ObservableCollection<EQEDevInfo>();
            int dwCanBtr = 0;
            int uiSubCANCount = 0;
            Int32[] mSubCanNodeId = new Int32[MAX_SUBCAN_NUM];
            m_uiDevIndex = gatewayDevInfo[selected_uim_dev_index].dwDevIndex;
            uiSubCANCount = OpenGateway(m_uiDevIndex, mSubCanNodeId, MAX_SUBCAN_NUM, ref dwCanBtr);
            if (uiSubCANCount == -1)
            {
                return dev_list;
            }
            else
            {
                IntPtr pMDL = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MDL_INFO_OBJ)));
                for (int i = 0; i < uiSubCANCount; i++)
                {
                    MDL_INFO_OBJ mdl = new MDL_INFO_OBJ();
                    uint iNodeId = (uint)mSubCanNodeId[i];
                    GetMDL(m_uiDevIndex, iNodeId, ref mdl);
                    EQEDevInfo dev_info = new EQEDevInfo() { SiteID = (uint)mSubCanNodeId[i], Model = mdl.szModelName, Firmware = mdl.uiFirewareVersion.ToString() };
                    dev_list.Add(dev_info);
                }
                UIMRegRtcnCallBack(m_dwDevType, m_uiDevIndex, pd);
                return dev_list;
            }
        }

        public bool CloseDev()
        {
            return -1 == CloseGateway(m_uiDevIndex);
        }

        public bool GoHome(ref string info)
        {
            bool ret;
            UpdateSiteID(XSiteID);
            ret = Go2InitPos(ref info);
            if (!ret)
                return false;
            UpdateSiteID(YSiteID);
            ret = Go2InitPos(ref info);
            PreviousPos = 1;
            Move21Pos(ref info);         
            return ret;
        }

        // 1. 增加公共属性，并赋予默认初始值
        public int PulseX_Pos1 { get; set; } = 38000;
        public int PulseY_Pos1 { get; set; } = -14800;

        public bool Move21Pos(ref string info)
        {
            int velocity = 6000;
            //位置修改
            int pulse_x = this.PulseX_Pos1;//38000    前 后+
            int pulse_y = this.PulseY_Pos1;//-14800   左 右+
            bool ret;

            if (pulse_x != 0)
            {
                UpdateSiteID(XSiteID);
                ret = EQERobot.GetInstance().SetSTP(pulse_x);
                if (!ret)
                {
                    info = "UIM X-axis moves failed!";
                    return false;
                }
                System.Threading.Thread.Sleep(Math.Abs(pulse_x / (velocity-500)* 1000));
            }
            if (pulse_y != 0)
            {
                UpdateSiteID(YSiteID);
                ret = EQERobot.GetInstance().SetSTP(pulse_y);
                if (!ret)
                {
                    info = "UIM Y-axis moves failed!";
                    return false;
                }
                System.Threading.Thread.Sleep(Math.Abs(pulse_y / (velocity-500) * 1000));
            }
            info = "X-Y Stage move to  Pos 1";
            return true;
        }

        private bool Go2InitPos(ref string info)
        {
            //第一次使用，先使能电机
            BASIC_ACK_OBJ objBasicAck = new BASIC_ACK_OBJ();
            if (-1 == UimENA(m_uiDevIndex, nSiteID, true, ref objBasicAck))
            {
                info = "UimENA failed!";
                return false;
            }

            UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_IN = new UIM_MCFG_INFO_OBJ();

            if (-1 == GetUimMCFG(m_uiDevIndex, nSiteID, ref pUIM_MCFG_INFO_OBJ_IN))
            {
                info = "GetUimMCFG failed!";
                return false;
            }

            // 使能 STPIE（到达指令位置通知），参考MCFG指令手册，STPIE在第5位
            pUIM_MCFG_INFO_OBJ_IN.uiMcfgVal |= 0x1 << 4;
            // 使能 S1（到达指令位置通知），参考MCFG指令手册，S1IE在第1位
            pUIM_MCFG_INFO_OBJ_IN.uiMcfgVal |= 0x2;


            UIM_MCFG_INFO_OBJ pUIM_MCFG_INFO_OBJ_OUT = new UIM_MCFG_INFO_OBJ();
            if (-1 == SetUimMCFG(m_uiDevIndex, nSiteID, ref pUIM_MCFG_INFO_OBJ_IN, true, ref pUIM_MCFG_INFO_OBJ_OUT))
            {
                info = "SetUimMCFG failed!";
                return false;
            }

            //  先查询当前电机是否在原点(S1)，通过SFBK查询传感器S1。
            SFBK_INFO_OBJ objSFBK = new SFBK_INFO_OBJ();
            if (-1 == UimSFBK(m_uiDevIndex, nSiteID, ref objSFBK))
            {
                info = "UimSFBK failed!";
                return false;
            }

            uint timeout;
            uint cnt = 0;
            int nRtnValue = -1;
            if (objSFBK.bSensor2 == 0)
            {
                //运动标志位置0
                m_nSTPIE = 0;
                //设定一个固定速度1000,可根据要求更改。
                //      err = csUIM.SetSPD(GtwyHandle, m_nTestCANID, 1000, ref iRtnVal); if (err != 0) { Console.WriteLine("SetSPD失败!"); return; }

                //设定一个固定位移1000，需保证位移保证电机脱机传感器区域，可根据要求更改。
                //if (-1 == SetSPD(m_uiDevIndex, nSiteID, 1000, true, ref nRtnValue))
                //{
                //    info = "UIM Set SPD failed!";
                //    return false;
                //}
                if (-1 == SetSPD(m_uiDevIndex, nSiteID, -6000, true, ref nRtnValue))
                {
                    info = "UIM Set SPD failed 2!";
                    return false;
                }

                if (nSiteID == XSiteID)
                {
                    //设定一个固定位移1000，需保证位移保证电机脱机传感器区域，可根据要求更改。
                    if (-1 == SetSTP(m_uiDevIndex, nSiteID, 1000, true, ref nRtnValue))
                    {
                        info = "UIM Set STP failed!";
                        return false;
                    }
                }
                else if (nSiteID == YSiteID)
                {
                    //设定一个固定位移1000，需保证位移保证电机脱机传感器区域，可根据要求更改。
                    if (-1 == SetSTP(m_uiDevIndex, nSiteID, -1000, true, ref nRtnValue))
                    {
                        info = "UIM Set STP failed!";
                        return false;
                    }
                }


                //等待电机运动到位消息,该标志位为RTCNprocess函数置1，可配置超时时间
                timeout = 2000;
                cnt = 0;
                while (m_nSTPIE == 0)
                {
                    System.Threading.Thread.Sleep(100);
                    if (cnt++ * 100 >= timeout)
                    {
                        info = "UIM timeout";
                        return false;
                    }
                }
            }

            //电机已经脱机原点位置，则低速回原点流程。

            P_S12CON pS12Con_IN = new P_S12CON();
            P_S12CON pS12Con_Out = new P_S12CON();
            //S1FACT 配置为绝对位置清零+减速直到停止
            //传感器触发代码参考UIM242手册第50页
            /*
            * 0001 无
            * 0010 负向连续运行
            * 0011 减速直到停止
            * 0100 紧急停止
            * 0101 负向相对位移控制
            * 0110 绝对位置清零
            * 0111 绝对位置清零+相对位移控制
            * 1000 执行用户预设中断程序*
            * 1001 换向相对位移控制
            * 1010 正向连续运行
            * 1011 绝对位置清零+减速直到停止
            * 1100 绝对位置清零+紧急停止
            * 1101 正向相对位移控制
            * 1110 换向连续运行
            * 1111 脱机
            */
            pS12Con_IN.uiS2RACT = 0;
            pS12Con_IN.uiS2FACT = 12;
            pS12Con_IN.uiS1RACT = 0;
            pS12Con_IN.uiS1FACT = 0;
            if (-1 == SetS12CON(m_uiDevIndex, nSiteID, ref pS12Con_IN, true, ref pS12Con_Out))
            {
                info = "UIM Set S12CON failed!";
                return false;
            }
            //S1低电平置位0
            m_nS2L = 0;
            //然后用指定一个低速度位移运动到原点方向，数值为负。
            //设定一个固定速度1000,可根据要求更改。
            //设定一个固定位移1000，需保证位移保证电机脱机传感器区域，可根据要求更改。
            //设定一个固定位移-50000，需保证位移保证电机走足够的位置到原点，可根据要求更改。
            int pulse;
            if (nSiteID == XSiteID)//X axis
            {
                timeout = 20000;
                pulse = -80000;
            }
            else if (nSiteID == YSiteID)//Y axis
            {
                timeout = 15000;
                pulse = 60000;
            }
            else
            {
                timeout = 10000;
                pulse = 40000;
            }
            if (-1 == SetSPD(m_uiDevIndex, nSiteID, 6000, true, ref nRtnValue))
            {
                info = "UIM Set SPD failed 2!";
                return false;
            }
            if (-1 == SetSTP(m_uiDevIndex, nSiteID, pulse, true, ref nRtnValue))
            {
                info = "UIM Set STP failed 2!";
                return false;
            }
            //if (nSiteID == YSiteID)
            //{
            //    System.Threading.Thread.Sleep(23500 * 1000 / 6000);
            //}

            //等待传感器到位消息,该标志位为RTCNprocess函数置1，可配置超时时间

            cnt = 0;
            while (m_nS2L == 0)
            {
                System.Threading.Thread.Sleep(100);
                if (cnt++ * 100 >= timeout)
                {
                    info = "UIM timeout 2.";
                    return false;
                }
            }

            //到位消息S1L成功，电机也完成的清零功能，并进入减速停止状态
            //进入减速停止需要时间，这里加入延时等待
            //System.Threading.Thread.Sleep(300);

            //清零后关闭传感器触发事件为空：避免在原点周围反复误触发。

            pS12Con_IN.uiS2RACT = 0;
            pS12Con_IN.uiS2FACT = 0;
            pS12Con_IN.uiS1RACT = 0;
            pS12Con_IN.uiS1FACT = 0;

            if (-1 == SetS12CON(m_uiDevIndex, nSiteID, ref pS12Con_IN, true, ref pS12Con_Out))
            {
                info = "UIM Set S12CON failed 2!";
                return false;
            }
          
            return true;
        }

        public void InitPara()
        {
            m_pSearchPara = new GW_SEARCH_PARA[MAX_GATEWAY_COUNT];
            m_DevInfo = Marshal.AllocHGlobal(MAX_GATEWAY_COUNT * Marshal.SizeOf(typeof(DEV_INFO_OBJ)));//new DEV_INFO_OBJ[MAX_GATEWAY_COUNT];
            gatewayDevInfo = new DEV_INFO_OBJ[MAX_GATEWAY_COUNT];
            //delShowRTCN = new DelegateRTCN(ShowRTCN);
        }

        public bool Move2Pos(int NextPos, ref string info)
        {
            Console.WriteLine( PreviousPos);
            Console.WriteLine(NextPos);
            /// sample carrier index position
            /// 6 5 4
            /// 3 2 1
            int velocity = 6000;
            bool ret;
            //bool ret=QuerySPD(ref velocity);
            //if (ret)
            //    return false; 

            //位置调节
            Int32 pulse_factor_x = 8025;     //x:8025
            Int32 pulse_factor_y = -10000;   //y:-10000
            Int32 pulse_x;
            Int32 pulse_y;
            Int32 times_x;
            Int32 times_y;

            if ((ushort)NextPos < 4)
            {
                if (PreviousPos < 4)
                    times_y = 0;
                else
                    times_y = -1;
            }
            else
            {
                if (PreviousPos < 4)
                    times_y = 1;
                else
                    times_y = 0;
            }
            times_x = ((((ushort)NextPos - 1) % 3) - ((PreviousPos - 1) % 3));

            pulse_x = pulse_factor_x * times_x;
            pulse_y = pulse_factor_y * times_y;

            info = String.Format("X-Y Stage move to  Pos {0:D1}", (ushort)NextPos);
            if (pulse_x != 0)
            {
                UpdateSiteID(XSiteID);
                ret = EQERobot.GetInstance().SetSTP(pulse_x);
                if (!ret)
                {
                    info = "UIM X-axis moves failed!";
                    return false;
                }
                System.Threading.Thread.Sleep(Math.Abs(pulse_x / velocity * 1000));
            }
            if (pulse_y != 0)
            {
                UpdateSiteID(YSiteID);
                ret = EQERobot.GetInstance().SetSTP(pulse_y);
                if (!ret)
                {
                    info = "UIM Y-axis moves failed!";
                    return false;
                }
                System.Threading.Thread.Sleep(Math.Abs(pulse_y / velocity * 1000));
            }
            PreviousPos = (ushort)NextPos;
            Console.WriteLine(PreviousPos);
            return true;
        }

        public void UpdateSiteID(uint siteID)
        {
            nSiteID = siteID;
            //OnUpdateUIMSiteIDEvent(siteID);
            //this.OnUpdateUIMSiteIDEvent(siteID);
        }

        public void SetSiteID(uint siteID_x, uint siteID_y)
        {
            XSiteID = siteID_x;
            YSiteID = siteID_y;
        }

        public void ResetSiteID()
        {
            XSiteID = 0;
            YSiteID = 0;
        }

        public static void Main(string[] args)
        {

        }

    }
}
