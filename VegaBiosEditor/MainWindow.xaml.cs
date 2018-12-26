using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VegaBiosEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MAX_VRAM_ENTRIES = 36;

        private byte[] buffer;

        private Int32Converter int32 = new Int32Converter();

        private UInt32Converter uint32 = new UInt32Converter();

        private string[] supportedDeviceID = new string[6]
        {
        "67DF",
        "1002",
        "4350",
        "5249",
        "687F",
        "6863"
        };

        string[] manufacturers = new string[4]
        {
            "SAMSUNG",
            "ELPIDA",
            "HYNIX",
            "MICRON"
        };

        string[] timings = new string[]
        {
    	    // for timings
       
        };

        Dictionary<string, string> rc = new Dictionary<string, string>();

        private string deviceID = "";

        private string TOOL_VERSION = " 1.0";

        private string TOOL_EXTRA = "By VASKE";

        private int atom_rom_checksum_offset = 33;

        private int atom_rom_header_ptr = 72;

        private int atom_rom_header_offset;

        private ATOM_ROM_HEADER atom_rom_header;

        private ATOM_DATA_TABLES atom_data_table;

        int atom_vega10_powerplay_offset;
        private ATOM_Vega10_POWERPLAYTABLE atom_vega10_powerplay_table;

        int atom_vega12_powerplay_offset;
        private ATOM_VEGA12_POWERPLAYTABLE atom_vega12_powerplay_table;

        int atom_vega10_state_array_offset;
        private ATOM_Vega10_State_Array atom_vega10_state_array;

        //int ATOM_Vega10_GFXCLK_Dependency_Table_offset;
        int atom_vega10_gfxclk_table_offset;
        private ATOM_Vega10_GFXCLK_Dependency_Table atom_vega10_gfxclk_table;
        private ATOM_Vega10_GFXCLK_Dependency_Record[] atom_vega10_gfxclk_entries;

        int atom_vega10_mclk_table_offset;
        private ATOM_Vega10_MCLK_Dependency_Table atom_vega10_mclk_table;
        private ATOM_Vega10_MCLK_Dependency_Record[] atom_vega10_mclk_entries;

        int atom_vega10_socclk_table_offset;
        private ATOM_Vega10_SOCCLK_Dependency_Table atom_vega10_socclk_table;
        private ATOM_Vega10_CLK_Dependency_Record[] atom_vega10_clk_entries;

        int atom_vega10_gfxvdd_table_offset;
        private ATOM_Vega10_Voltage_Lookup_Table atom_vega10_gfxvdd_table;
        private ATOM_Vega10_Voltage_Lookup_Record[] atom_vega10_gfxvdd_record;

        int atom_vega10_memvdd_table_offset;
        private ATOM_Vega10_Voltage_Lookup_Table atom_vega10_memvdd_table;
        private ATOM_Vega10_Voltage_Lookup_Record[] atom_vega10_memvdd_record;

        int atom_vddc_table_offset;
        private ATOM_Vega10_Voltage_Lookup_Table atom_vddc_table;
        private ATOM_Vega10_Voltage_Lookup_Record[] atom_vddc_entries;

        int atom_vega10_fan_offset;
        private ATOM_Vega10_Fan_Table atom_vega10_fan_table;
        /*
        int atom_vega10_powertune_table_v1_offset;
        ATOM_Vega10_PowerTune_Table_V1 atom_vega10_powertune_table_v1;

        int atom_vega10_powertune_table_v2_offset;
        ATOM_Vega10_PowerTune_Table_V2 atom_vega10_powertune_table_v2;
        */
        int atom_vega10_powertune_table_offset;
        private ATOM_Vega10_PowerTune_Table atom_vega10_powertune_table;

        private int atom_vram_info_offset;

        private ATOM_VRAM_INFO atom_vram_info;

        private ATOM_VRAM_ENTRY[] atom_vram_entries;

        private ATOM_VRAM_TIMING_ENTRY[] atom_vram_timing_entries;

        private int atom_vram_index;

        private int atom_vram_timing_offset;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ATOM_COMMON_TABLE_HEADER
        {
            private short usStructureSize;

            private byte ucTableFormatRevision;

            private byte ucTableContentRevision;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ATOM_ROM_HEADER
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;

            public uint uaFirmWareSignature;

            public ushort usBiosRuntimeSegmentAddress;

            public ushort usProtectedModeInfoOffset;

            public ushort usConfigFilenameOffset;

            public ushort usCRC_BlockOffset;

            public ushort usBIOS_BootupMessageOffset;

            public ushort usInt10Offset;

            public ushort usPciBusDevInitCode;

            public ushort usIoBaseAddress;

            public ushort usSubsystemVendorID;

            public ushort usSubsystemID;

            public ushort usPCI_InfoOffset;

            public ushort usMasterCommandTableOffset;

            public ushort usMasterDataTableOffset;

            public byte ucExtendedFunctionCode;

            public byte ucReserved;

            public uint ulPSPDirTableOffset;

            public ushort usVendorID;

            public ushort usDeviceID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ATOM_DATA_TABLES
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;

            public ushort UtilityPipeLine;

            public ushort MultimediaCapabilityInfo;

            public ushort MultimediaConfigInfo;

            public ushort StandardVESA_Timing;

            public ushort FirmwareInfo;

            public ushort PaletteData;

            public ushort LCD_Info;

            public ushort DIGTransmitterInfo;

            public ushort SMU_Info;

            public ushort SupportedDevicesInfo;

            public ushort GPIO_I2C_Info;

            public ushort VRAM_UsageByFirmware;

            public ushort GPIO_Pin_LUT;

            public ushort VESA_ToInternalModeLUT;

            public ushort GFX_Info;

            public ushort PowerPlayInfo;

            public ushort GPUVirtualizationInfo;

            public ushort SaveRestoreInfo;

            public ushort PPLL_SS_Info;

            public ushort OemInfo;

            public ushort XTMDS_Info;

            public ushort MclkSS_Info;

            public ushort Object_Header;

            public ushort IndirectIOAccess;

            public ushort MC_InitParameter;

            public ushort ASIC_VDDC_Info;

            public ushort ASIC_InternalSS_Info;

            public ushort TV_VideoMode;

            public ushort VRAM_Info;

            public ushort MemoryTrainingInfo;

            public ushort IntegratedSystemInfo;

            public ushort ASIC_ProfilingInfo;

            public ushort VoltageObjectInfo;

            public ushort PowerSourceInfo;

            public ushort ServiceInfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_POWERPLAYTABLE
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;
            public Byte ucTableRevision;
            public UInt16 usTableSize;                        /* the size of header structure */
            public UInt32 ulGoldenPPID;                       /* PPGen use only */
            public UInt32 ulGoldenRevision;                   /* PPGen use only */
            public UInt16 usFormatID;                         /* PPGen use only */
            public UInt32 ulPlatformCaps;                     /* See ATOM_Vega10_CAPS_* */
            public UInt32 ulMaxODEngineClock;                 /* For Overdrive. */
            public UInt32 ulMaxODMemoryClock;                 /* For Overdrive. */
            public UInt16 usPowerControlLimit;
            public UInt16 usUlvVoltageOffset;                 /* in mv units */
            public UInt16 usUlvSmnclkDid;
            public UInt16 usUlvMp1clkDid;
            public UInt16 usUlvGfxclkBypass;
            public UInt16 usGfxclkSlewRate;
            public Byte ucGfxVoltageMode;
            public Byte ucSocVoltageMode;
            public Byte ucUclkVoltageMode;
            public Byte ucUvdVoltageMode;
            public Byte ucVceVoltageMode;
            public Byte ucMp0VoltageMode;
            public Byte ucDcefVoltageMode;
            public UInt16 usStateArrayOffset;                 /* points to ATOM_Vega10_State_Array */
            public UInt16 usFanTableOffset;                   /* points to ATOM_Vega10_Fan_Table */
            public UInt16 usThermalControllerOffset;          /* points to ATOM_Vega10_Thermal_Controller */
            public UInt16 usSocclkDependencyTableOffset;      /* points to ATOM_Vega10_SOCCLK_Dependency_Table */
            public UInt16 usMclkDependencyTableOffset;        /* points to ATOM_Vega10_MCLK_Dependency_Table */
            public UInt16 usGfxclkDependencyTableOffset;      /* points to ATOM_Vega10_GFXCLK_Dependency_Table */
            public UInt16 usDcefclkDependencyTableOffset;     /* points to ATOM_Vega10_DCEFCLK_Dependency_Table */
            public UInt16 usVddcLookupTableOffset;            /* points to ATOM_Vega10_Voltage_Lookup_Table */
            public UInt16 usVddmemLookupTableOffset;          /* points to ATOM_Vega10_Voltage_Lookup_Table */
            public UInt16 usMMDependencyTableOffset;          /* points to ATOM_Vega10_MM_Dependency_Table */
            public UInt16 usVCEStateTableOffset;              /* points to ATOM_Vega10_VCE_State_Table */
            public UInt16 usReserve;                          /* No PPM Support for Vega10 */
            public UInt16 usPowerTuneTableOffset;             /* points to ATOM_Vega10_PowerTune_Table */
            public UInt16 usHardLimitTableOffset;             /* points to ATOM_Vega10_Hard_Limit_Table */
            public UInt16 usVddciLookupTableOffset;           /* points to ATOM_Vega10_Voltage_Lookup_Table */
            public UInt16 usPCIETableOffset;                  /* points to ATOM_Vega10_PCIE_Table */
            public UInt16 usPixclkDependencyTableOffset;      /* points to ATOM_Vega10_PIXCLK_Dependency_Table */
            public UInt16 usDispClkDependencyTableOffset;     /* points to ATOM_Vega10_DISPCLK_Dependency_Table */
            public UInt16 usPhyClkDependencyTableOffset;      /* points to ATOM_Vega10_PHYCLK_Dependency_Table */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_VEGA12_POWERPLAYTABLE
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;
            public Byte ucTableRevision;
            public UInt16 usTableSize;
            public UInt32 ulGoldenPPID;
            public UInt32 ulGoldenRevision;
            public UInt16 usFormatID;

            public UInt32 ulPlatformCaps;

            public Byte ucThermalControllerType;

            public UInt16 usSmallPowerLimit1;
            public UInt16 usSmallPowerLimit2;
            public UInt16 usBoostPowerLimit;
            public UInt16 usODTurboPowerLimit;
            public UInt16 usODPowerSavePowerLimit;
            public UInt16 usSoftwareShutdownTemp;

            // public UInt32 PowerSavingClockMax[ATOM_VEGA12_PPCLOCK_COUNT];
            //  public UInt32 PowerSavingClockMin[ATOM_VEGA12_PPCLOCK_COUNT];

            //  public UInt32 ODSettingsMax[ATOM_VEGA12_ODSETTING_COUNT];
            //  public UInt32 ODSettingsMin[ATOM_VEGA12_ODSETTING_COUNT];

            public unsafe fixed UInt16 usReserve[5];

            //PPTable_t smcPPTable;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_State
        {
            public Byte ucSocClockIndexHigh;
            public Byte ucSocClockIndexLow;
            public Byte ucGfxClockIndexHigh;
            public Byte ucGfxClockIndexLow;
            public Byte ucMemClockIndexHigh;
            public Byte ucMemClockIndexLow;
            public UInt16 usClassification;
            public UInt32 ulCapsAndSettings;
            public UInt16 usClassification2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_State_Array
        {
            public Byte ucRevId;
            public Byte ucNumEntries;                                         /* Number of entries. */
            // ATOM_Vega10_State states[1];                             /* Dynamically allocate entries. */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_GFXCLK_Dependency_Record
        {
            public UInt32 ulClk;                                               /* Clock Frequency */
            public Byte ucVddInd;                                            /* SOC_VDD index */
            public UInt16 usCKSVOffsetandDisable;                              /* Bits 0~30: Voltage offset for CKS, Bit 31: Disable/enable for the GFXCLK level. */
            public UInt16 usAVFSOffset;                                        /* AVFS Voltage offset */
            public Byte ucACGEnable;
            public unsafe fixed Byte ucReserved[3];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_GFXCLK_Dependency_Table
        {
            public Byte ucRevId;
            public Byte ucNumEntries;                                         /* Number of entries. */
            //ATOM_Vega10_GFXCLK_Dependency_Record entries[1];            /* Dynamically allocate entries. */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_MCLK_Dependency_Record
        {
            public UInt32 ulMemClk;                                            /* Clock Frequency */
            public Byte ucVddInd;                                            /* SOC_VDD index */
            public Byte ucVddMemInd;                                         /* MEM_VDD - only non zero for MCLK record */
            public Byte ucVddciInd;                                          /* VDDCI   = only non zero for MCLK record */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_MCLK_Dependency_Table
        {
            public Byte ucRevId;
            public Byte ucNumEntries;                                         /* Number of entries. */
                                                                              // ATOM_Vega10_MCLK_Dependency_Record entries[1];                   /* Dynamically allocate entries. */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_CLK_Dependency_Record
        {
            public UInt32 ulClk;                                               /* Frequency of Clock */
            public Byte ucVddInd;                                            /* Base voltage */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_SOCCLK_Dependency_Table
        {
            public Byte ucRevId;
            public Byte ucNumEntries;                                         /* Number of entries. */
                                                                              // ATOM_Vega10_CLK_Dependency_Record entries[1];            /* Dynamically allocate entries. */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_Voltage_Lookup_Record
        {
            public UInt16 usVdd;                                               /* Base voltage */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_Voltage_Lookup_Table
        {
            public Byte ucRevId;
            public Byte ucNumEntries;                                          /* Number of entries */
            //ATOM_Vega10_Voltage_Lookup_Record entries[1];             /* Dynamically allocate entries */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_Fan_Table
        {
            public Byte ucRevId;
            public UInt16 usFanOutputSensitivity;
            public UInt16 usFanAcousticLimitRpm;
            public UInt16 usThrottlingRPM;
            public UInt16 usTargetTemperature;
            public UInt16 usMinimumPWMLimit;
            public UInt16 usTargetGfxClk;
            public UInt16 usFanGainEdge;
            public UInt16 usFanGainHotspot;
            public UInt16 usFanGainLiquid;
            public UInt16 usFanGainVrVddc;
            public UInt16 usFanGainVrMvdd;
            public UInt16 usFanGainPlx;
            public UInt16 usFanGainHbm;
            public Byte ucEnableZeroRPM;
            public UInt16 usFanStopTemperature;
            public UInt16 usFanStartTemperature;
            public Byte ucFanParameters;
            public Byte ucFanMinRPM;
            public Byte ucFanMaxRPM;
        }
        /*
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_PowerTune_Table_V1
        {
            public Byte ucRevId;
            public UInt16 usSocketPowerLimit;
            public UInt16 usBatteryPowerLimit;
            public UInt16 usSmallPowerLimit;
            public UInt16 usTdcLimit;
            public UInt16 usEdcLimit;
            public UInt16 usSoftwareShutdownTemp;
            public UInt16 usTemperatureLimitHotSpot;
            public UInt16 usTemperatureLimitLiquid1;
            public UInt16 usTemperatureLimitLiquid2;
            public UInt16 usTemperatureLimitHBM;
            public UInt16 usTemperatureLimitVrSoc;
            public UInt16 usTemperatureLimitVrMem;
            public UInt16 usTemperatureLimitPlx;
            public UInt16 usLoadLineResistance;
            public Byte ucLiquid1_I2C_address;
            public Byte ucLiquid2_I2C_address;
            public Byte ucVr_I2C_address;
            public Byte ucPlx_I2C_address;
            public Byte ucLiquid_I2C_LineSCL;
            public Byte ucLiquid_I2C_LineSDA;
            public Byte ucVr_I2C_LineSCL;
            public Byte ucVr_I2C_LineSDA;
            public Byte ucPlx_I2C_LineSCL;
            public Byte ucPlx_I2C_LineSDA;
            public UInt16 usTemperatureLimitTedge;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_PowerTune_Table_V2
        {
            public Byte ucRevId;
            public UInt16 usSocketPowerLimit;
            public UInt16 usBatteryPowerLimit;
            public UInt16 usSmallPowerLimit;
            public UInt16 usTdcLimit;
            public UInt16 usEdcLimit;
            public UInt16 usSoftwareShutdownTemp;
            public UInt16 usTemperatureLimitHotSpot;
            public UInt16 usTemperatureLimitLiquid1;
            public UInt16 usTemperatureLimitLiquid2;
            public UInt16 usTemperatureLimitHBM;
            public UInt16 usTemperatureLimitVrSoc;
            public UInt16 usTemperatureLimitVrMem;
            public UInt16 usTemperatureLimitPlx;
            public UInt16 usLoadLineResistance;
            public Byte ucLiquid1_I2C_address;
            public Byte ucLiquid2_I2C_address;
            public Byte ucLiquid_I2C_Line;
            public Byte ucVr_I2C_address;
            public Byte ucVr_I2C_Line;
            public Byte ucPlx_I2C_address;
            public Byte ucPlx_I2C_Line;
            public UInt16 usTemperatureLimitTedge;
        }
        */
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_PowerTune_Table
        {
            public Byte ucRevId;
            public UInt16 usSocketPowerLimit;
            public UInt16 usBatteryPowerLimit;
            public UInt16 usSmallPowerLimit;
            public UInt16 usTdcLimit;
            public UInt16 usEdcLimit;
            public UInt16 usSoftwareShutdownTemp;
            public UInt16 usTemperatureLimitHotSpot;
            public UInt16 usTemperatureLimitLiquid1;
            public UInt16 usTemperatureLimitLiquid2;
            public UInt16 usTemperatureLimitHBM;
            public UInt16 usTemperatureLimitVrSoc;
            public UInt16 usTemperatureLimitVrMem;
            public UInt16 usTemperatureLimitPlx;
            public UInt16 usLoadLineResistance;
            public Byte ucLiquid1_I2C_address;
            public Byte ucLiquid2_I2C_address;
            public Byte ucLiquid_I2C_Line;
            public Byte ucVr_I2C_address;
            public Byte ucVr_I2C_Line;
            public Byte ucPlx_I2C_address;
            public Byte ucPlx_I2C_Line;
            public UInt16 usTemperatureLimitTedge;
            public UInt16 usBoostStartTemperature;
            public UInt16 usBoostStopTemperature;
            public UInt32 ulBoostClock;
            public unsafe fixed UInt32 Reserved[2];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ATOM_VRAM_TIMING_ENTRY
        {
            public uint ulClkRange;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] ucLatency;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ATOM_VRAM_ENTRY
        {
            public uint ulChannelMapCfg;

            public ushort usModuleSize;

            public ushort usMcRamCfg;

            public ushort usEnableChannels;

            public byte ucExtMemoryID;

            public byte ucMemoryType;

            public byte ucChannelNum;

            public byte ucChannelWidth;

            public byte ucDensity;

            public byte ucBankCol;

            public byte ucMisc;

            public byte ucVREFI;

            public ushort usReserved;

            public ushort usMemorySize;

            public byte ucMcTunningSetId;

            public byte ucRowNum;

            public ushort usEMRS2Value;

            public ushort usEMRS3Value;

            public byte ucMemoryVenderID;

            public byte ucRefreshRateFactor;

            public byte ucFIFODepth;

            public byte ucCDR_Bandwidth;

            public uint ulChannelMapCfg1;

            public uint ulBankMapCfg;

            public uint ulReserved;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] strMemPNString;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] strMemPNXT;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ATOM_VRAM_INFO
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;

            public ushort usMemAdjustTblOffset;

            public ushort usMemClkPatchTblOffset;

            public ushort usMcAdjustPerTileTblOffset;

            public ushort usMcPhyInitTableOffset;

            public ushort usDramDataRemapTblOffset;

            public ushort usReserved1;

            public byte ucNumOfVRAMModule;

            public byte ucMemoryClkPatchTblVer;

            public byte ucVramModuleVer;

            public byte ucMcPhyTileNum;
        }

        static byte[] getBytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        static T fromBytes<T>(byte[] arr)
        {
            T obj = default(T);
            int size = Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);
            obj = (T)Marshal.PtrToStructure(ptr, obj.GetType());
            Marshal.FreeHGlobal(ptr);

            return obj;
        }

        public string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;
            if (me == null)
            {
                throw new ArgumentException();
            }
            return me.Member.Name;
        }

        public void setBytesAtPosition(byte[] dest, int ptr, byte[] src)
        {
            for (var i = 0; i < src.Length; i++)
            {
                dest[ptr + i] = src[i];
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.GetWindow(this).Title += TOOL_VERSION + " " + TOOL_EXTRA;

            save.IsEnabled = false;
            boxROM.IsEnabled = false;
            boxPOWERPLAY.IsEnabled = false;
            boxPOWERTUNE.IsEnabled = false;
            boxFAN.IsEnabled = false;
            boxGPU.IsEnabled = false;
            boxMEM.IsEnabled = false;
            boxVRAM.IsEnabled = false;

            rc.Add("MT51J256M3", "MICRON");
            rc.Add("EDW4032BAB", "ELPIDA");
            rc.Add("H5GC4H24AJ", "HYNIX_1");
            rc.Add("H5GQ4H24AJ", "HYNIX_2");
            rc.Add("H5GQ8H24MJ", "HYNIX_2");
            rc.Add("H5GC8H24MJ", "HYNIX_3");
            rc.Add("H5GC8H24AJ", "HYNIX_4");
            rc.Add("K4G80325FB", "SAMSUNG");
            rc.Add("K4G41325FE", "SAMSUNG");
            rc.Add("K4G41325FC", "SAMSUNG");
            rc.Add("K4G41325FS", "SAMSUNG");
        }

        private void OpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BIOS (.rom)|*.rom|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                save.IsEnabled = false;

                tableROM.Items.Clear();
                tablePOWERPLAY.Items.Clear();
                tablePOWERTUNE.Items.Clear();
                tableFAN.Items.Clear();
                tableGPU.Items.Clear();
                tableMEMORY.Items.Clear();
                tableVRAM.Items.Clear();
                tableVRAM_TIMING.Items.Clear();

                MainWindow.GetWindow(this).Title += " " + "-" + " " + "[" + openFileDialog.SafeFileName + "]";

                System.IO.Stream fileStream = openFileDialog.OpenFile();
                if ((fileStream.Length != 524288) && (fileStream.Length != 524288 / 2))
                {
                    MessageBox.Show("This BIOS is non standard size.\nFlashing this BIOS may corrupt your graphics card.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                using (BinaryReader br = new BinaryReader(fileStream))
                {
                    buffer = br.ReadBytes((int)fileStream.Length);

                    atom_rom_header_offset = getValueAtPosition(16, atom_rom_header_ptr);
                    atom_rom_header = fromBytes<ATOM_ROM_HEADER>(buffer.Skip(atom_rom_header_offset).ToArray());
                    deviceID = atom_rom_header.usDeviceID.ToString("X");
                    fixChecksum(false);

                    MessageBoxResult msgSuported = MessageBoxResult.Yes;
                    if (!supportedDeviceID.Contains(deviceID))
                    {
                        msgSuported = MessageBox.Show("Unsupported DeviceID 0x" + deviceID + " - Continue?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                    if (msgSuported == MessageBoxResult.Yes)
                    {
                        atom_data_table = fromBytes<ATOM_DATA_TABLES>(buffer.Skip(atom_rom_header.usMasterDataTableOffset).ToArray());
                        atom_vega10_powerplay_offset = atom_data_table.PowerPlayInfo;
                        atom_vega10_powerplay_table = fromBytes<ATOM_Vega10_POWERPLAYTABLE>(buffer.Skip(atom_vega10_powerplay_offset).ToArray());

                        atom_vega10_powertune_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usPowerTuneTableOffset;
                        atom_vega10_powertune_table = fromBytes<ATOM_Vega10_PowerTune_Table>(buffer.Skip(atom_vega10_powertune_table_offset).ToArray());

                        atom_vega10_fan_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usFanTableOffset;
                        atom_vega10_fan_table = fromBytes<ATOM_Vega10_Fan_Table>(buffer.Skip(atom_vega10_fan_offset).ToArray());

                        atom_vega10_mclk_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usMclkDependencyTableOffset;
                        atom_vega10_mclk_table = fromBytes<ATOM_Vega10_MCLK_Dependency_Table>(buffer.Skip(atom_vega10_mclk_table_offset).ToArray());
                        atom_vega10_mclk_entries = new ATOM_Vega10_MCLK_Dependency_Record[atom_vega10_mclk_table.ucNumEntries];
                        for (var i = 0; i < atom_vega10_mclk_entries.Length; i++)
                        {
                            atom_vega10_mclk_entries[i] = fromBytes<ATOM_Vega10_MCLK_Dependency_Record>(buffer.Skip(atom_vega10_mclk_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_MCLK_Dependency_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_MCLK_Dependency_Record)) * i).ToArray());
                        }

                        atom_vega10_gfxclk_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usGfxclkDependencyTableOffset;
                        atom_vega10_gfxclk_table = fromBytes<ATOM_Vega10_GFXCLK_Dependency_Table>(buffer.Skip(atom_vega10_gfxclk_table_offset).ToArray());
                        atom_vega10_gfxclk_entries = new ATOM_Vega10_GFXCLK_Dependency_Record[atom_vega10_gfxclk_table.ucNumEntries];
                        for (var i = 0; i < atom_vega10_gfxclk_entries.Length; i++)
                        {
                            atom_vega10_gfxclk_entries[i] = fromBytes<ATOM_Vega10_GFXCLK_Dependency_Record>(buffer.Skip(atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usGfxclkDependencyTableOffset + Marshal.SizeOf(typeof(ATOM_Vega10_GFXCLK_Dependency_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_GFXCLK_Dependency_Record)) * i).ToArray());
                        }

                        atom_vddc_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usVddcLookupTableOffset;
                        atom_vddc_table = fromBytes<ATOM_Vega10_Voltage_Lookup_Table>(buffer.Skip(atom_vddc_table_offset).ToArray());
                        atom_vddc_entries = new ATOM_Vega10_Voltage_Lookup_Record[atom_vddc_table.ucNumEntries];
                        for (var i = 0; i < atom_vddc_table.ucNumEntries; i++)
                        {
                            atom_vddc_entries[i] = fromBytes<ATOM_Vega10_Voltage_Lookup_Record>(buffer.Skip(atom_vddc_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Record)) * i).ToArray());
                        }

                        atom_vega10_memvdd_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usVddmemLookupTableOffset;
                        atom_vega10_memvdd_table = fromBytes<ATOM_Vega10_Voltage_Lookup_Table>(buffer.Skip(atom_vega10_memvdd_table_offset).ToArray());
                        atom_vega10_memvdd_record = new ATOM_Vega10_Voltage_Lookup_Record[atom_vega10_memvdd_table.ucNumEntries];
                        for (var i = 0; i < atom_vega10_memvdd_table.ucNumEntries; i++)
                        {
                            atom_vega10_memvdd_record[i] = fromBytes<ATOM_Vega10_Voltage_Lookup_Record>(buffer.Skip(atom_vega10_memvdd_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Record)) * i).ToArray());
                        }

                        atom_vega10_socclk_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usSocclkDependencyTableOffset;
                        atom_vega10_socclk_table = fromBytes<ATOM_Vega10_SOCCLK_Dependency_Table>(buffer.Skip(atom_vega10_socclk_table_offset).ToArray());
                        atom_vega10_clk_entries = new ATOM_Vega10_CLK_Dependency_Record[atom_vega10_socclk_table.ucNumEntries];
                        for (var i = 0; i < atom_vega10_clk_entries.Length; i++)
                        {
                            atom_vega10_clk_entries[i] = fromBytes<ATOM_Vega10_CLK_Dependency_Record>(buffer.Skip(atom_vega10_socclk_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_SOCCLK_Dependency_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_CLK_Dependency_Record)) * i).ToArray());
                        }

                        atom_vram_info_offset = atom_data_table.VRAM_Info;
                        atom_vram_info = fromBytes<ATOM_VRAM_INFO>(buffer.Skip(atom_vram_info_offset).ToArray());
                        atom_vram_entries = new ATOM_VRAM_ENTRY[atom_vram_info.ucNumOfVRAMModule];
                        var atom_vram_entry_offset = atom_vram_info_offset + Marshal.SizeOf(typeof(ATOM_VRAM_INFO));
                        for (var i = 0; i < atom_vram_info.ucNumOfVRAMModule; i++)
                        {
                            atom_vram_entries[i] = fromBytes<ATOM_VRAM_ENTRY>(buffer.Skip(atom_vram_entry_offset).ToArray());
                            atom_vram_entry_offset += atom_vram_entries[i].usModuleSize;
                        }

                        atom_vram_timing_offset = atom_vram_info_offset + atom_vram_info.usMemClkPatchTblOffset + 0x9302;
                        atom_vram_timing_entries = new ATOM_VRAM_TIMING_ENTRY[MAX_VRAM_ENTRIES];
                        for (var i = 0; i < MAX_VRAM_ENTRIES; i++)
                        {
                            atom_vram_timing_entries[i] = fromBytes<ATOM_VRAM_TIMING_ENTRY>(buffer.Skip(atom_vram_timing_offset + Marshal.SizeOf(typeof(ATOM_VRAM_TIMING_ENTRY)) * i).ToArray());

                            // atom_vram_timing_entries have an undetermined length
                            // attempt to determine the last entry in the array
                            if (atom_vram_timing_entries[i].ulClkRange == 0)
                            {
                                Array.Resize(ref atom_vram_timing_entries, i);
                                break;
                            }
                        }

                        tableROM.Items.Clear();
                        tableROM.Items.Add(new
                        {
                            NAME = "BootupMessage",
                            VALUE = "0x" + atom_rom_header.usBIOS_BootupMessageOffset.ToString("X")
                        });
                        tableROM.Items.Add(new
                        {
                            NAME = "VendorID",
                            VALUE = "0x" + atom_rom_header.usVendorID.ToString("X")
                        });
                        tableROM.Items.Add(new
                        {
                            NAME = "DeviceID",
                            VALUE = "0x" + atom_rom_header.usDeviceID.ToString("X")
                        });
                        tableROM.Items.Add(new
                        {
                            NAME = "Sub ID",
                            VALUE = "0x" + atom_rom_header.usSubsystemID.ToString("X")
                        });
                        tableROM.Items.Add(new
                        {
                            NAME = "Sub VendorID",
                            VALUE = "0x" + atom_rom_header.usSubsystemVendorID.ToString("X")
                        });
                        tableROM.Items.Add(new
                        {
                            NAME = "Firmware Signature",
                            VALUE = "0x" + atom_rom_header.uaFirmWareSignature.ToString("X")
                        });

                        tablePOWERPLAY.Items.Clear();
                        tablePOWERPLAY.Items.Add(new
                        {
                            NAME = "Max GPU Freq. (MHz)",
                            VALUE = atom_vega10_powerplay_table.ulMaxODEngineClock / 100
                        });
                        tablePOWERPLAY.Items.Add(new
                        {
                            NAME = "Max Memory Freq. (MHz)",
                            VALUE = atom_vega10_powerplay_table.ulMaxODMemoryClock / 100
                        });
                        tablePOWERPLAY.Items.Add(new
                        {
                            NAME = "Power Control Limit (%)",
                            VALUE = atom_vega10_powerplay_table.usPowerControlLimit
                        });
                        tablePOWERPLAY.Items.Add(new
                        {
                            NAME = "ULV VoltageOffset (mV)",
                            VALUE = atom_vega10_powerplay_table.usUlvVoltageOffset
                        });

                        tablePOWERTUNE.Items.Clear();
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "Socket Power (W)",
                            VALUE = atom_vega10_powertune_table.usSocketPowerLimit
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "Battery Power (W)",
                            VALUE = atom_vega10_powertune_table.usBatteryPowerLimit
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "Small Power Limit (W)",
                            VALUE = atom_vega10_powertune_table.usSmallPowerLimit
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "EDC Module Limit",
                            VALUE = atom_vega10_powertune_table.usEdcLimit
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "TDC (A)",
                            VALUE = atom_vega10_powertune_table.usTdcLimit
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "Temp. Limit (C)",
                            VALUE = atom_vega10_powertune_table.usTemperatureLimitTedge
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "Shutdown Temp. (C)",
                            VALUE = atom_vega10_powertune_table.usSoftwareShutdownTemp
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "Hotspot Temp. (C)",
                            VALUE = atom_vega10_powertune_table.usTemperatureLimitHotSpot
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "Liquid 1 Temp. (C)",
                            VALUE = atom_vega10_powertune_table.usTemperatureLimitLiquid1
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "Liquid 2 Temp. (C)",
                            VALUE = atom_vega10_powertune_table.usTemperatureLimitLiquid2
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "HBM Temp. (C)",
                            VALUE = atom_vega10_powertune_table.usTemperatureLimitHBM
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "VR Soc Temp. (C)",
                            VALUE = atom_vega10_powertune_table.usTemperatureLimitVrSoc
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "VR Mem Temp. (C)",
                            VALUE = atom_vega10_powertune_table.usTemperatureLimitVrMem
                        });
                        tablePOWERTUNE.Items.Add(new
                        {
                            NAME = "PLX Temp. (C)",
                            VALUE = atom_vega10_powertune_table.usTemperatureLimitPlx
                        });


                        tableFAN.Items.Clear();
                        tableFAN.Items.Add(new
                        {
                            NAME = "Sensitivity",
                            VALUE = atom_vega10_fan_table.usFanOutputSensitivity
                        });
                        tableFAN.Items.Clear();
                        tableFAN.Items.Add(new
                        {
                            NAME = "Target Temp. (C)",
                            VALUE = atom_vega10_fan_table.usTargetTemperature
                        });

                        tableFAN.Items.Add(new
                        {
                            NAME = "Throttling RPM",
                            VALUE = atom_vega10_fan_table.usThrottlingRPM
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Target Temp. (C)",
                            VALUE = atom_vega10_fan_table.usTargetTemperature
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Target Gfx Clk",
                            VALUE = atom_vega10_fan_table.usTargetGfxClk
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Gain: Edge",
                            VALUE = atom_vega10_fan_table.usFanGainEdge
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Gain: Hotspot",
                            VALUE = atom_vega10_fan_table.usFanGainHotspot
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Gain: Liquid",
                            VALUE = atom_vega10_fan_table.usFanGainLiquid
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Gain: VrVddc",
                            VALUE = atom_vega10_fan_table.usFanGainVrVddc
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Gain: VrMvdd",
                            VALUE = atom_vega10_fan_table.usFanGainVrMvdd
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Gain: Plx",
                            VALUE = atom_vega10_fan_table.usFanGainPlx
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Gain: HBM",
                            VALUE = atom_vega10_fan_table.usFanGainHbm
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Acoustic Limit (MHz)",
                            VALUE = atom_vega10_fan_table.usFanAcousticLimitRpm
                        });/*
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Stop Temperature",
                            VALUE = atom_vega10_fan_table.usFanStopTemperature
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Start Temperature",
                            VALUE = atom_vega10_fan_table.usFanStartTemperature 
                        });*/
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Parameters",
                            VALUE = atom_vega10_fan_table.ucFanParameters
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Min RPM",
                            VALUE = atom_vega10_fan_table.ucFanMinRPM * 100
                        });
                        tableFAN.Items.Add(new
                        {
                            NAME = "Fan Max RPM",
                            VALUE = atom_vega10_fan_table.ucFanMaxRPM * 100
                        });

                        tableGPU.Items.Clear();
                        for (var i = 0; i < atom_vega10_gfxclk_table.ucNumEntries; i++)
                        {
                            tableGPU.Items.Add(new
                            {
                                MHZ = atom_vega10_gfxclk_entries[i].ulClk / 100u,
                                MV = atom_vddc_entries[atom_vega10_gfxclk_entries[i].ucVddInd].usVdd,
                                TT = ("0x" + atom_vddc_entries[atom_vega10_gfxclk_entries[i].ucVddInd].usVdd.ToString("X"))
                            });
                        }

                        tableMEMORY.Items.Clear();
                        for (var i = 0; i < atom_vega10_mclk_table.ucNumEntries; i++)
                        {
                            tableMEMORY.Items.Add(new
                            {
                                MHZ = atom_vega10_mclk_entries[i].ulMemClk / 100u,
                                MV = atom_vddc_entries[i].usVdd,
                                TT = ("0x" + atom_vddc_entries[i].usVdd.ToString("X"))
                            });
                        }
                        /*
                        tableSOC.Items.Clear();
                        for (var i = 0; i < atom_vega10_socclk_table.ucNumEntries; i++)
                        {
                            tableSOC.Items.Add(new
                            {
                                MHZ = atom_vega10_clk_entries[i].ulClk / 100u,
                                MV = atom_vddc_entries[i].usVdd,
                                TT = ("0x" + atom_vddc_entries[i].usVdd.ToString("X"))
                            });
                        }*/

                        listVRAM.Items.Clear();
                        for (var i = 0; i < atom_vram_info.ucNumOfVRAMModule; i++)
                        {
                            if (atom_vram_entries[i].strMemPNString[0] != 0)
                            {
                                var mem_id = Encoding.UTF8.GetString(atom_vram_entries[i].strMemPNString).Substring(0, 10);
                                string mem_vendor;
                                if (rc.ContainsKey(mem_id))
                                {
                                    mem_vendor = rc[mem_id];
                                }
                                else
                                {
                                    mem_vendor = "UNKNOWN";
                                }

                                listVRAM.Items.Add(mem_id + " (" + mem_vendor + ")");
                            }
                        }
                        listVRAM.SelectedIndex = 0;
                        atom_vram_index = listVRAM.SelectedIndex;

                        tableVRAM_TIMING.Items.Clear();
                        for (var i = 0; i < atom_vram_timing_entries.Length; i++)
                        {
                            uint tbl = atom_vram_timing_entries[i].ulClkRange >> 0x4F00;
                            tableVRAM_TIMING.Items.Add(new
                            {
                                MHZ = tbl.ToString() + ":" + (atom_vram_timing_entries[i].ulClkRange & 0x4300) / 100,
                                VALUE = ByteArrayToString(atom_vram_timing_entries[i].ucLatency)
                            });
                        }

                        save.IsEnabled = true;
                        boxROM.IsEnabled = true;
                        boxPOWERPLAY.IsEnabled = true;
                        boxPOWERTUNE.IsEnabled = true;
                        boxFAN.IsEnabled = true;
                        boxGPU.IsEnabled = true;
                        boxMEM.IsEnabled = true;
                        boxVRAM.IsEnabled = true;

                        // Some BIOS attributes to describe the file
                        txtRamNotes.Text = "";
                        if (atom_rom_header.usConfigFilenameOffset > 0)
                        {
                            byte[] bfn = new byte[12];
                            for (int i = 0; i < 12; i++) bfn[i] = buffer[atom_rom_header.usConfigFilenameOffset + i];
                            txtRamNotes.Text += " " + System.Text.Encoding.ASCII.GetString(bfn);
                        }
                        if (atom_rom_header.usBIOS_BootupMessageOffset > 0)
                        {
                            byte[] bbm = new byte[64];
                            for (int i = 0; i < 64; i++) bbm[i] = buffer[atom_rom_header.usBIOS_BootupMessageOffset + 2 + i];
                            txtRamNotes.Text += " " + System.Text.Encoding.ASCII.GetString(bbm);
                        }
                    }
                    fileStream.Close();
                }
            }
        }

        public Int32 getValueAtPosition(int bits, int position, bool isFrequency = false)
        {
            int value = 0;
            if (position <= buffer.Length - 4)
            {
                switch (bits)
                {
                    case 8:
                    default:
                        value = buffer[position];
                        break;
                    case 16:
                        value = (buffer[position + 1] << 8) | buffer[position];
                        break;
                    case 24:
                        value = (buffer[position + 2] << 16) | (buffer[position + 1] << 8) | buffer[position];
                        break;
                    case 32:
                        value = (buffer[position + 3] << 24) | (buffer[position + 2] << 16) | (buffer[position + 1] << 8) | buffer[position];
                        break;
                }
                if (isFrequency) return value / 100;
                return value;
            }
            return -1;
        }

        public bool setValueAtPosition(int value, int bits, int position, bool isFrequency = false)
        {
            if (isFrequency) value *= 100;
            if (position <= buffer.Length - 4)
            {
                switch (bits)
                {
                    case 8:
                    default:
                        buffer[position] = (byte)value;
                        break;
                    case 16:
                        buffer[position] = (byte)value;
                        buffer[position + 1] = (byte)(value >> 8);
                        break;
                    case 24:
                        buffer[position] = (byte)value;
                        buffer[position + 1] = (byte)(value >> 8);
                        buffer[position + 2] = (byte)(value >> 16);
                        break;
                    case 32:
                        buffer[position] = (byte)value;
                        buffer[position + 1] = (byte)(value >> 8);
                        buffer[position + 2] = (byte)(value >> 16);
                        buffer[position + 3] = (byte)(value >> 32);
                        break;
                }
                return true;
            }
            return false;
        }

        private bool setValueAtPosition(String text, int bits, int position, bool isFrequency = false)
        {
            int value = 0;
            if (!int.TryParse(text, out value))
            {
                return false;
            }
            return setValueAtPosition(value, bits, position, isFrequency);
        }

        private void SaveFileDialog_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SaveFileDialog = new SaveFileDialog();
            SaveFileDialog.Title = "Save As";
            SaveFileDialog.Filter = "BIOS (*.rom)|*.rom";

            if (SaveFileDialog.ShowDialog() == true)
            {
                FileStream fs = new FileStream(SaveFileDialog.FileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                for (var i = 0; i < tableROM.Items.Count; i++)
                {
                    var container = tableROM.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("NAME", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var num = (int)int32.ConvertFromString(value);

                    if (name == "VendorID")
                    {
                        atom_rom_header.usVendorID = (UInt16)num;
                    }
                    else if (name == "DeviceID")
                    {
                        atom_rom_header.usDeviceID = (UInt16)num;
                    }
                    else if (name == "Sub ID")
                    {
                        atom_rom_header.usSubsystemID = (UInt16)num;
                    }
                    else if (name == "Sub VendorID")
                    {
                        atom_rom_header.usSubsystemVendorID = (UInt16)num;
                    }
                    else if (name == "Firmware Signature")
                    {
                        atom_rom_header.uaFirmWareSignature = (UInt32)num;
                    }
                }

                for (var i = 0; i < tablePOWERPLAY.Items.Count; i++)
                {
                    var container = tablePOWERPLAY.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("NAME", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var num = (int)int32.ConvertFromString(value);

                    if (name == "Max GPU Freq. (MHz)")
                    {
                        atom_vega10_powerplay_table.ulMaxODEngineClock = (UInt32)(num * 100);
                    }
                    else if (name == "Max Memory Freq. (MHz)")
                    {
                        atom_vega10_powerplay_table.ulMaxODMemoryClock = (UInt32)(num * 100);
                    }
                    else if (name == "Power Control Limit (%)")
                    {
                        atom_vega10_powerplay_table.usPowerControlLimit = (UInt16)num;
                    }
                    else if (name == "ULV VoltageOffset (mV)")
                    {
                        atom_vega10_powerplay_table.usUlvVoltageOffset = (UInt16)num;
                    }
                }

                for (var i = 0; i < tablePOWERTUNE.Items.Count; i++)
                {
                    var container = tablePOWERTUNE.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("NAME", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var num = (int)int32.ConvertFromString(value);

                    if (name == "Socket Power (W)")
                    {
                        atom_vega10_powertune_table.usSocketPowerLimit = (UInt16)num;
                    }
                    else if (name == "Battery Power (W)")
                    {
                        atom_vega10_powertune_table.usBatteryPowerLimit = (UInt16)num;
                    }
                    else if (name == "Small Power Limit (W)")
                    {
                        atom_vega10_powertune_table.usSmallPowerLimit = (UInt16)num;
                    }
                    else if (name == "EDC Module Limit")
                    {
                        atom_vega10_powertune_table.usEdcLimit = (UInt16)num;
                    }
                    else if (name == "TDC (A)")
                    {
                        atom_vega10_powertune_table.usTdcLimit = (UInt16)num;
                    }
                    else if (name == "Temp. Limit (C)")
                    {
                        atom_vega10_powertune_table.usTemperatureLimitTedge = (UInt16)num;
                    }
                    else if (name == "Shutdown Temp. (C)")
                    {
                        atom_vega10_powertune_table.usSoftwareShutdownTemp = (UInt16)num;
                    }
                    else if (name == "Hotspot Temp. (C)")
                    {
                        atom_vega10_powertune_table.usTemperatureLimitHotSpot = (UInt16)num;
                    }
                    else if (name == "Liquid 1 Temp. (C)")
                    {
                        atom_vega10_powertune_table.usTemperatureLimitLiquid1 = (UInt16)num;
                    }
                    else if (name == "Liquid 2 Temp. (C)")
                    {
                        atom_vega10_powertune_table.usTemperatureLimitLiquid2 = (UInt16)num;
                    }
                    else if (name == "HBM Temp. (C)")
                    {
                        atom_vega10_powertune_table.usTemperatureLimitHBM = (UInt16)num;
                    }
                    else if (name == "VR Soc Temp. (C)")
                    {
                        atom_vega10_powertune_table.usTemperatureLimitVrSoc = (UInt16)num;
                    }
                    else if (name == "VR Mem Temp. (C)")
                    {
                        atom_vega10_powertune_table.usTemperatureLimitVrMem = (UInt16)num;
                    }
                    else if (name == "PLX Temp. (C)")
                    {
                        atom_vega10_powertune_table.usTemperatureLimitPlx = (UInt16)num;
                    }
                }

                for (var i = 0; i < tableFAN.Items.Count; i++)
                {
                    var container = tableFAN.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("NAME", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var num = (int)int32.ConvertFromString(value);

                    if (name == "Sensitivity")
                    {
                        atom_vega10_fan_table.usFanOutputSensitivity = (UInt16)num;
                    }
                    else if (name == "Target Temp. (C)")
                    {
                        atom_vega10_fan_table.usTargetTemperature = (UInt16)num;
                    }
                    else if (name == "Throttling RPM")
                    {
                        atom_vega10_fan_table.usThrottlingRPM = (UInt16)num;
                    }
                    else if (name == "Target Gfx Clk")
                    {
                        atom_vega10_fan_table.usTargetGfxClk = (UInt16)num;
                    }
                    else if (name == "Fan Gain: Edge")
                    {
                        atom_vega10_fan_table.usFanGainEdge = (UInt16)num;
                    }
                    else if (name == "Fan Gain: Hotspot")
                    {
                        atom_vega10_fan_table.usFanGainHotspot = (UInt16)num;
                    }
                    else if (name == "Fan Gain: Liquid")
                    {
                        atom_vega10_fan_table.usFanGainLiquid = (UInt16)num;
                    }
                    else if (name == "Fan Gain: VrVddc")
                    {
                        atom_vega10_fan_table.usFanGainVrVddc = (UInt16)num;//(num * 100)
                    }
                    else if (name == "Fan Gain: VrMvdd")
                    {
                        atom_vega10_fan_table.usFanGainVrMvdd = (UInt16)num;
                    }
                    else if (name == "Fan Gain: Plx")
                    {
                        atom_vega10_fan_table.usFanGainPlx = (UInt16)num;
                    }
                    else if (name == "Fan Gain: HBM")
                    {
                        atom_vega10_fan_table.usFanGainHbm = (UInt16)num;
                    }
                    else if (name == "Acoustic Limit (MHz)")
                    {
                        atom_vega10_fan_table.usFanAcousticLimitRpm = (UInt16)num;
                    }/*
                    else if (name == "Fan Stop Temperature")
                    {
                        atom_vega10_fan_table.usFanStopTemperature = (UInt16)num;
                    }
                    else if (name == "Fan Start Temperature")
                    {
                        atom_vega10_fan_table.usFanStartTemperature = (UInt16)num;
                    }*/
                    else if (name == "Fan Parameters")
                    {
                        atom_vega10_fan_table.ucFanParameters = (Byte)num;
                    }
                    else if (name == "Fan Min RPM")
                    {
                        atom_vega10_fan_table.ucFanMinRPM = (Byte)(num * 100);
                    }
                    else if (name == "Fan Max RPM")
                    {
                        atom_vega10_fan_table.ucFanMaxRPM = (Byte)(num * 100);
                    }
                }

                for (var i = 0; i < tableGPU.Items.Count; i++)
                {
                    var container = tableGPU.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var mhz = (int)int32.ConvertFromString(((TextBox)FindByName("MHZ", container)).Text) * 100;
                    var mv = (int)int32.ConvertFromString(((TextBox)FindByName("MV", container)).Text);

                    atom_vega10_gfxclk_entries[i].ulClk = (UInt32)mhz;
                    atom_vddc_entries[atom_vega10_gfxclk_entries[i].ucVddInd].usVdd = (UInt16)mv;
                    /*
                    if (mv < 0xFF00)
                    {
                        atom_vega10_gfxclk_entries[i].usVddcOffset = 0;
                    }*/
                }

                for (var i = 0; i < tableMEMORY.Items.Count; i++)
                {
                    var container = tableMEMORY.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var mhz = (int)int32.ConvertFromString(((TextBox)FindByName("MHZ", container)).Text) * 100;
                    var mv = (int)int32.ConvertFromString(((TextBox)FindByName("MV", container)).Text);

                    atom_vega10_mclk_entries[i].ulMemClk = (UInt32)mhz;
                    atom_vddc_entries[i].usVdd = (UInt16)mv;
                }
                /*
                for (var i = 0; i < tableSOC.Items.Count; i++)
                {
                    var container = tableSOC.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var mhz = (int)int32.ConvertFromString(((TextBox)FindByName("MHZ", container)).Text) * 100;
                    var mv = (int)int32.ConvertFromString(((TextBox)FindByName("MV", container)).Text);

                    atom_vega10_clk_entries[i].ulClk = (UInt32)mhz;
                    atom_vddc_entries[i].usVdd = (UInt16)mv;
                }*/

                updateVRAM_entries();
                for (var i = 0; i < tableVRAM_TIMING.Items.Count; i++)
                {
                    var container = tableVRAM_TIMING.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("MHZ", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var arr = StringToByteArray(value);
                    UInt32 mhz;
                    if (name.IndexOf(':') > 0)
                    {
                        mhz = (UInt32)uint32.ConvertFromString(name.Substring(name.IndexOf(':') + 1)) * 100;
                        mhz += (UInt32)uint32.ConvertFromString(name.Substring(0, name.IndexOf(':'))) << 24; // table id
                    }
                    else
                    {
                        mhz = (UInt32)uint32.ConvertFromString(name) * 100;
                    }
                    atom_vram_timing_entries[i].ulClkRange = mhz;
                    atom_vram_timing_entries[i].ucLatency = arr;
                }

                setBytesAtPosition(buffer, atom_rom_header_offset, getBytes(atom_rom_header));
                setBytesAtPosition(buffer, atom_vega10_powerplay_offset, getBytes(atom_vega10_powerplay_offset));
                setBytesAtPosition(buffer, atom_vega10_powertune_table_offset, getBytes(atom_vega10_powertune_table_offset));
                setBytesAtPosition(buffer, atom_vega10_fan_offset, getBytes(atom_vega10_fan_table));

                for (var i = 0; i < atom_vega10_mclk_table.ucNumEntries; i++)
                {
                    setBytesAtPosition(buffer, atom_vega10_mclk_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_MCLK_Dependency_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_MCLK_Dependency_Record)) * i, getBytes(atom_vega10_mclk_entries[i]));
                }

                for (var i = 0; i < atom_vega10_gfxclk_table.ucNumEntries; i++)
                {
                    setBytesAtPosition(buffer, atom_vega10_gfxclk_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_GFXCLK_Dependency_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_GFXCLK_Dependency_Record)) * i, getBytes(atom_vega10_gfxclk_entries[i]));
                }

                for (var i = 0; i < atom_vddc_table.ucNumEntries; i++)
                {
                    setBytesAtPosition(buffer, atom_vddc_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Record)) * i, getBytes(atom_vddc_entries[i]));
                }

                var atom_vram_entry_offset = atom_vram_info_offset + Marshal.SizeOf(typeof(ATOM_VRAM_INFO));
                for (var i = 0; i < atom_vram_info.ucNumOfVRAMModule; i++)
                {
                    setBytesAtPosition(buffer, atom_vram_entry_offset, getBytes(atom_vram_entries[i]));
                    atom_vram_entry_offset += atom_vram_entries[i].usModuleSize;
                }

                atom_vram_timing_offset = atom_vram_info_offset + atom_vram_info.usMemClkPatchTblOffset + 0x2E;
                for (var i = 0; i < atom_vram_timing_entries.Length; i++)
                {
                    setBytesAtPosition(buffer, atom_vram_timing_offset + Marshal.SizeOf(typeof(ATOM_VRAM_TIMING_ENTRY)) * i, getBytes(atom_vram_timing_entries[i]));
                }

                fixChecksum(true);
                bw.Write(buffer);

                fs.Close();
                bw.Close();
            }
        }

        private void fixChecksum(bool save)
        {
            Byte checksum = buffer[atom_rom_checksum_offset];
            int size = buffer[0x02] * 512;
            Byte offset = 0;

            for (int i = 0; i < size; i++)
            {
                offset += buffer[i];
            }
            if (checksum == (buffer[atom_rom_checksum_offset] - offset))
            {
                txtChecksum.Foreground = Brushes.Green;
            }
            else if (!save)
            {
                txtChecksum.Foreground = Brushes.Red;
                MessageBox.Show("Invalid checksum - Save to fix!", "WARNING!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (save)
            {
                buffer[atom_rom_checksum_offset] -= offset;
                txtChecksum.Foreground = Brushes.Green;
            }
            txtChecksum.Text = "0x" + buffer[atom_rom_checksum_offset].ToString("X");
        }

        private FrameworkElement FindByName(string name, FrameworkElement root)
        {
            Stack<FrameworkElement> tree = new Stack<FrameworkElement>();
            tree.Push(root);

            while (tree.Count > 0)
            {
                FrameworkElement current = tree.Pop();
                if (current.Name == name)
                    return current;

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(current, i);
                    if (child is FrameworkElement)
                        tree.Push((FrameworkElement)child);
                }
            }
            return null;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public static byte[] StringToByteArray(String hex)
        {
            if (hex.Length % 2 != 0)
            {
                MessageBox.Show("Invalid hex string", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new InvalidDataException();
            }
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public void updateVRAM_entries()
        {
            for (var i = 0; i < tableVRAM.Items.Count; i++)
            {
                var container = tableVRAM.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                var name = (FindByName("NAME", container) as TextBlock).Text;
                var value = (FindByName("VALUE", container) as TextBox).Text;
                var num = (int)int32.ConvertFromString(value);

                if (name == "VendorID")
                {
                    atom_vram_entries[atom_vram_index].ucMemoryVenderID = (Byte)num;
                }
                else if (name == "Size (MB)")
                {
                    atom_vram_entries[atom_vram_index].usMemorySize = (UInt16)num;
                }
                else if (name == "Density")
                {
                    atom_vram_entries[atom_vram_index].ucDensity = (Byte)num;
                }
                else if (name == "Type")
                {
                    atom_vram_entries[atom_vram_index].ucMemoryType = (Byte)num;
                }
            }
        }

        private void listVRAM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateVRAM_entries();
            tableVRAM.Items.Clear();
            if (listVRAM.SelectedIndex >= 0 && listVRAM.SelectedIndex < listVRAM.Items.Count)
            {
                atom_vram_index = listVRAM.SelectedIndex;
                tableVRAM.Items.Add(new
                {
                    NAME = "VendorID",
                    VALUE = "0x" + atom_vram_entries[atom_vram_index].ucMemoryVenderID.ToString("X")
                });
                tableVRAM.Items.Add(new
                {
                    NAME = "Size (MB)",
                    VALUE = atom_vram_entries[atom_vram_index].usMemorySize
                });
                tableVRAM.Items.Add(new
                {
                    NAME = "Density",
                    VALUE = "0x" + atom_vram_entries[atom_vram_index].ucDensity.ToString("X")
                });
                tableVRAM.Items.Add(new
                {
                    NAME = "Type",
                    VALUE = "0x" + atom_vram_entries[atom_vram_index].ucMemoryType.ToString("X")
                });
            }
        }

        private void listVRAM_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void apply_timings(int vendor_index, int timing_index)
        {
            for (int i = 0; i < this.tableVRAM_TIMING.Items.Count; i++)
            {
                FrameworkElement root = this.tableVRAM_TIMING.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                string text = (this.FindByName("MHZ", root) as TextBlock).Text;
                int num;
                if (text.IndexOf(':') > 0)
                {
                    num = (int)this.int32.ConvertFromString(text.Substring(0, 1));
                }
                else
                {
                    num = 32768;
                }
                if ((uint)this.uint32.ConvertFromString(text.Substring(text.IndexOf(':') + 1)) >= 1500u && (num == vendor_index || num == 32768))
                {
                    TextBox textBox = this.FindByName("VALUE", root) as TextBox;
                    string text2 = textBox.Text = this.timings[timing_index];
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int samsung_index = -1;
            int micron_index = -1;
            int elpida_index = -1;
            int hynix_1_index = -1;
            int hynix_2_index = -1;
            int hynix_3_index = -1;
            int hynix_4_index = -1;
            for (var i = 0; i < atom_vram_info.ucNumOfVRAMModule; i++)
            {
                string mem_vendor;
                if (atom_vram_entries[i].strMemPNString[0] != 0)
                {
                    var mem_id = Encoding.UTF8.GetString(atom_vram_entries[i].strMemPNString).Substring(0, 10);

                    if (rc.ContainsKey(mem_id))
                    {
                        mem_vendor = rc[mem_id];
                    }
                    else
                    {
                        mem_vendor = "UNKNOWN";
                    }

                    switch (mem_vendor)
                    {
                        case "SAMSUNG":
                            samsung_index = i;
                            break;
                        case "MICRON":
                            micron_index = i;
                            break;
                        case "ELPIDA":
                            elpida_index = i;
                            break;
                        case "HYNIX_1":
                            hynix_1_index = i;
                            break;
                        case "HYNIX_2":
                            hynix_2_index = i;
                            break;
                        case "HYNIX_3":
                            hynix_3_index = i;
                            break;
                        case "HYNIX_4":
                            hynix_4_index = i;
                            break;
                    }
                }
            }

            if (samsung_index != -1)
            {
                if (MessageBox.Show("Do you want faster Uber-mix 3.1?", "Important Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    int num = (int)MessageBox.Show("Samsung Memory found at index #" + (object)samsung_index + ", now applying UBERMIX 3.1 timings to 1500+ strap(s)");
                    this.apply_timings(samsung_index, 0);
                }
                else
                {
                    int num = (int)MessageBox.Show("Samsung Memory found at index #" + (object)samsung_index + ", now applying UBERMIX 3.2 timings to 1500+ strap(s)");
                    this.apply_timings(samsung_index, 1);
                }
            }

            if (hynix_3_index != -1)
            {
                if (MessageBox.Show("Do you want Universal Hynix Timing?", "Important Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Hynix (3) Memory found at index #" + hynix_3_index + ", now applying Universal HYNIX MINING timings to 1500+ strap(s)");
                    apply_timings(hynix_3_index, 8);
                }
                else
                {
                    MessageBox.Show("Hynix (3) Memory found at index #" + hynix_3_index + ", now applying GOOD HYNIX MINING timings to 1500+ strap(s)");
                    apply_timings(hynix_3_index, 2);
                }
            }

            if (hynix_2_index != -1)
            {
                if (MessageBox.Show("Do you want Universal Hynix Timing?", "Important Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Hynix (2) Memory found at index #" + hynix_2_index + ", now applying Universal HYNIX MINING timings to 1500+ strap(s)");
                    apply_timings(hynix_2_index, 8);
                }
                else
                {
                    int num = (int)MessageBox.Show("Hynix (2) Memory found at index #" + (object)micron_index + ", now applying GOOD Hynix timings to 1500+ strap(s)");
                    this.apply_timings(hynix_2_index, 3);
                }
            }

            if (micron_index != -1)
            {
                if (MessageBox.Show("Do you want Good Micron Timing?", "Important Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    int num = (int)MessageBox.Show("Micron Memory found at index #" + (object)micron_index + ", now applying Good Micron timings to 1500+ strap(s)");
                    this.apply_timings(micron_index, 4);
                }
                else
                {
                    int num = (int)MessageBox.Show("Micron Memory found at index #" + (object)micron_index + ", now applying S Micron timings to 1500+ strap(s)");
                    this.apply_timings(micron_index, 5);
                }
            }

            if (hynix_4_index != -1)
            {
                MessageBox.Show("Hynix (4) Memory found at index #" + hynix_4_index + ", now applying HYNIX MINING timings to 1500+ strap(s)");
                apply_timings(hynix_4_index, 9);
            }

            if (hynix_1_index != -1)
            {
                MessageBox.Show("Hynix (1) Memory found at index #" + hynix_1_index + ", now applying GOOD HYNIX MINING timings to 1500+ strap(s)");
                apply_timings(hynix_1_index, 6);
            }

            if (elpida_index != -1)
            {
                MessageBox.Show("Elpida Memory found at index #" + elpida_index + ", now applying GOOD ELPIDA MINING timings to 1500+ strap(s)");
                apply_timings(elpida_index, 7);
            }

            if (samsung_index == -1 && hynix_2_index == -1 && hynix_3_index == -1 && hynix_1_index == -1 && elpida_index == -1 && micron_index == -1)
            {
                MessageBox.Show("Sorry, no supported memory found. If you think this is an error, please file a bugreport @ github.com/vvaske/VegaBiosEditor");
            }
        }

    }
}
