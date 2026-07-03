using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private enum AsicFamily
        {
            Unknown,
            Vega10,
            Vega12
        }

        private readonly string[] vega10DeviceIDs = new string[]
        {
            "6860",
            "6861",
            "6862",
            "6863",
            "6864",
            "6867",
            "6868",
            "686C",
            "687F"
        };

        private readonly string[] vega12DeviceIDs = new string[]
        {
            "69A0",
            "69A1",
            "69A2",
            "69A3",
            "69AF"
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

        private AsicFamily asicFamily = AsicFamily.Unknown;

        private string TOOL_VERSION = " 1.0.1";

        private string TOOL_EXTRA = "By VASKE";

        private string baseWindowTitle;

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
        private ATOM_Vega10_GFXCLK_Dependency_Record_V2[] atom_vega10_gfxclk_entries;

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
            public ushort usStructureSize;

            public byte ucTableFormatRevision;

            public byte ucTableContentRevision;
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
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ATOM_Vega10_GFXCLK_Dependency_Record_V2
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
            public uint ulMemorySize;

            public uint ulChannelEnable;

            public uint ulMaxMemClk;

            public ushort usReserved0;

            public ushort usReserved1;

            public ushort usReserved2;

            public ushort usMemVoltage;

            public ushort usModuleSize;

            public byte ucExtMemoryID;

            public byte ucMemoryType;

            public byte ucChannelNum;

            public byte ucChannelWidth;

            public byte ucDensity;

            public byte ucTuningSetId;

            public byte ucMemoryVendorRevID;

            public byte ucRefreshRate;

            public byte ucHbmVendorRevID;

            public byte ucVramReserved2;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] strMemPNString;
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

            public ushort usTmrsSeqOffset;

            public ushort usPostUCodeInitOffset;

            public ushort usReserved1;

            public byte ucNumOfVRAMModule;

            public byte ucMemoryClkPatchTblVer;

            public byte ucVramModuleVer;

            public byte ucMcPhyTileNum;
        }

        static byte[] getBytes(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(obj, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return arr;
        }

        static T fromBytes<T>(byte[] arr)
        {
            int size = Marshal.SizeOf(typeof(T));
            if (arr == null || arr.Length < size)
            {
                throw new InvalidDataException("Not enough data to read " + typeof(T).Name + ".");
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.Copy(arr, 0, ptr, size);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
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
            if (dest == null)
            {
                throw new ArgumentNullException("dest");
            }
            if (src == null)
            {
                throw new ArgumentNullException("src");
            }
            if (ptr < 0 || ptr > dest.Length - src.Length)
            {
                throw new InvalidDataException("Write outside BIOS buffer at 0x" + ptr.ToString("X") + ".");
            }

            Buffer.BlockCopy(src, 0, dest, ptr, src.Length);
        }

        private T ReadStruct<T>(int position)
        {
            int size = Marshal.SizeOf(typeof(T));
            EnsureBufferRange(position, size, typeof(T).Name);

            byte[] bytes = new byte[size];
            Buffer.BlockCopy(buffer, position, bytes, 0, size);
            return fromBytes<T>(bytes);
        }

        private void WriteStruct<T>(int position, T value)
        {
            setBytesAtPosition(buffer, position, getBytes(value));
        }

        private int GetGfxclkDependencyRecordSize()
        {
            switch (atom_vega10_gfxclk_table.ucRevId)
            {
                case 0:
                    return Marshal.SizeOf(typeof(ATOM_Vega10_GFXCLK_Dependency_Record));
                case 1:
                    return Marshal.SizeOf(typeof(ATOM_Vega10_GFXCLK_Dependency_Record_V2));
                default:
                    throw new InvalidDataException("Unsupported Vega10 GFXCLK dependency table revision " + atom_vega10_gfxclk_table.ucRevId + ".");
            }
        }

        private int GetGfxclkDependencyRecordOffset(int index)
        {
            return atom_vega10_gfxclk_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_GFXCLK_Dependency_Table)) + GetGfxclkDependencyRecordSize() * index;
        }

        private ATOM_Vega10_GFXCLK_Dependency_Record_V2 ReadGfxclkDependencyRecord(int index)
        {
            int offset = GetGfxclkDependencyRecordOffset(index);
            if (atom_vega10_gfxclk_table.ucRevId == 0)
            {
                ATOM_Vega10_GFXCLK_Dependency_Record record = ReadStruct<ATOM_Vega10_GFXCLK_Dependency_Record>(offset);
                return new ATOM_Vega10_GFXCLK_Dependency_Record_V2
                {
                    ulClk = record.ulClk,
                    ucVddInd = record.ucVddInd,
                    usCKSVOffsetandDisable = record.usCKSVOffsetandDisable,
                    usAVFSOffset = record.usAVFSOffset,
                    ucACGEnable = 0
                };
            }

            return ReadStruct<ATOM_Vega10_GFXCLK_Dependency_Record_V2>(offset);
        }

        private void WriteGfxclkDependencyRecord(int index, ATOM_Vega10_GFXCLK_Dependency_Record_V2 value)
        {
            int offset = GetGfxclkDependencyRecordOffset(index);
            if (atom_vega10_gfxclk_table.ucRevId == 0)
            {
                ATOM_Vega10_GFXCLK_Dependency_Record record = new ATOM_Vega10_GFXCLK_Dependency_Record
                {
                    ulClk = value.ulClk,
                    ucVddInd = value.ucVddInd,
                    usCKSVOffsetandDisable = value.usCKSVOffsetandDisable,
                    usAVFSOffset = value.usAVFSOffset
                };
                WriteStruct(offset, record);
                return;
            }

            WriteStruct(offset, value);
        }

        private void EnsureBufferRange(int position, int length, string name)
        {
            if (!HasBufferRange(position, length))
            {
                throw new InvalidDataException(name + " is outside the BIOS buffer at 0x" + position.ToString("X") + ".");
            }
        }

        private bool HasBufferRange(int position, int length)
        {
            return buffer != null && position >= 0 && length >= 0 && position <= buffer.Length - length;
        }

        private void ClearTables()
        {
            tableROM.Items.Clear();
            tablePOWERPLAY.Items.Clear();
            tablePOWERTUNE.Items.Clear();
            tableFAN.Items.Clear();
            tableGPU.Items.Clear();
            tableMEMORY.Items.Clear();
            tableVRAM.Items.Clear();
            tableVRAM_TIMING.Items.Clear();
            listVRAM.Items.Clear();
            txtRamNotes.Text = "";
        }

        private void SetEditorEnabled(bool enabled)
        {
            bool editablePowerPlay = enabled && asicFamily == AsicFamily.Vega10;

            save.IsEnabled = editablePowerPlay;
            boxROM.IsEnabled = enabled;
            boxPOWERPLAY.IsEnabled = editablePowerPlay;
            boxPOWERTUNE.IsEnabled = editablePowerPlay;
            boxFAN.IsEnabled = editablePowerPlay;
            boxGPU.IsEnabled = editablePowerPlay;
            boxMEM.IsEnabled = editablePowerPlay;
            boxVRAM.IsEnabled = enabled && atom_vram_entries != null && atom_vram_entries.Length > 0;
        }

        private bool IsSupportedDeviceID(string id)
        {
            return vega10DeviceIDs.Contains(id) || vega12DeviceIDs.Contains(id);
        }

        private AsicFamily GetAsicFamily(string id)
        {
            if (vega10DeviceIDs.Contains(id))
            {
                return AsicFamily.Vega10;
            }
            if (vega12DeviceIDs.Contains(id))
            {
                return AsicFamily.Vega12;
            }

            return AsicFamily.Unknown;
        }

        private string GetAsicName()
        {
            switch (asicFamily)
            {
                case AsicFamily.Vega10:
                    return "Vega10";
                case AsicFamily.Vega12:
                    return "Vega12";
                default:
                    return "Unknown";
            }
        }

        private ushort GetVoltageValue(ATOM_Vega10_Voltage_Lookup_Record[] table, int index, string tableName)
        {
            if (table == null || index < 0 || index >= table.Length)
            {
                throw new InvalidDataException(tableName + " voltage index " + index + " is invalid.");
            }

            return table[index].usVdd;
        }

        private void SetVoltageValue(ATOM_Vega10_Voltage_Lookup_Record[] table, int index, ushort value, string tableName)
        {
            if (table == null || index < 0 || index >= table.Length)
            {
                throw new InvalidDataException(tableName + " voltage index " + index + " is invalid.");
            }

            table[index].usVdd = value;
        }

        private string ReadAscii(int position, int length)
        {
            if (position <= 0 || length <= 0 || !HasBufferRange(position, 1))
            {
                return "";
            }

            int safeLength = Math.Min(length, buffer.Length - position);
            return Encoding.ASCII.GetString(buffer, position, safeLength).TrimEnd('\0', ' ', '\r', '\n');
        }

        private string ReadFixedAscii(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return "";
            }

            return Encoding.ASCII.GetString(value).TrimEnd('\0', ' ', '\r', '\n');
        }

        private byte[] MakeFixedAscii(string value, int length)
        {
            byte[] result = new byte[length];
            if (String.IsNullOrEmpty(value))
            {
                return result;
            }

            byte[] text = Encoding.ASCII.GetBytes(value);
            Buffer.BlockCopy(text, 0, result, 0, Math.Min(length, text.Length));
            return result;
        }

        private uint DecodeClockValue(uint rawClock)
        {
            if (rawClock == 0)
            {
                return 0;
            }

            return rawClock >= 10000u ? rawClock / 100u : rawClock;
        }

        private ushort ReadUInt16At(int position, string name)
        {
            EnsureBufferRange(position, 2, name);
            return (ushort)(buffer[position] | (buffer[position + 1] << 8));
        }

        private uint ReadUInt32At(int position, string name)
        {
            EnsureBufferRange(position, 4, name);
            return (uint)(buffer[position] | (buffer[position + 1] << 8) | (buffer[position + 2] << 16) | (buffer[position + 3] << 24));
        }

        private ushort GetVramModuleSize(ATOM_VRAM_ENTRY entry)
        {
            ushort structSize = (ushort)Marshal.SizeOf(typeof(ATOM_VRAM_ENTRY));
            return entry.usModuleSize >= structSize ? entry.usModuleSize : structSize;
        }

        private string GetVramTypeName(byte memoryType)
        {
            switch (memoryType)
            {
                case 0x50:
                    return "GDDR5";
                case 0x60:
                    return "HBM2";
                case 0x61:
                    return "HBM2E";
                case 0x70:
                    return "GDDR6";
                default:
                    return "0x" + memoryType.ToString("X2");
            }
        }

        private string GetMemoryVendorName(ATOM_VRAM_ENTRY entry)
        {
            string partNumber = ReadFixedAscii(entry.strMemPNString);
            foreach (var item in rc)
            {
                if (partNumber.StartsWith(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
                }
            }

            switch (entry.ucMemoryVendorRevID & 0x0F)
            {
                case 0x1:
                    return "SAMSUNG";
                case 0x3:
                    return "ELPIDA";
                case 0x6:
                    return "HYNIX";
                case 0xF:
                    return "MICRON";
                default:
                    return "UNKNOWN";
            }
        }

        private string GetVramModuleName(ATOM_VRAM_ENTRY entry, int index)
        {
            string partNumber = ReadFixedAscii(entry.strMemPNString);
            string vendor = GetMemoryVendorName(entry);
            string type = GetVramTypeName(entry.ucMemoryType);
            string name = String.IsNullOrWhiteSpace(partNumber) ? "Module " + index : partNumber;
            return name + " (" + vendor + ", " + type + ", " + entry.ulMemorySize + " MB)";
        }

        private int GetVramInfoEnd()
        {
            int tableSize = atom_vram_info.sHeader.usStructureSize;
            if (tableSize <= 0 || !HasBufferRange(atom_vram_info_offset, tableSize))
            {
                return buffer == null ? atom_vram_info_offset : buffer.Length;
            }

            return atom_vram_info_offset + tableSize;
        }

        private int GetVramSubTableEnd(ushort subTableOffset)
        {
            int tableEnd = GetVramInfoEnd();
            int subTableEnd = tableEnd;
            ushort[] offsets = new ushort[]
            {
                atom_vram_info.usMemAdjustTblOffset,
                atom_vram_info.usMemClkPatchTblOffset,
                atom_vram_info.usMcAdjustPerTileTblOffset,
                atom_vram_info.usMcPhyInitTableOffset,
                atom_vram_info.usDramDataRemapTblOffset,
                atom_vram_info.usTmrsSeqOffset,
                atom_vram_info.usPostUCodeInitOffset
            };

            foreach (ushort offset in offsets)
            {
                int absolute = atom_vram_info_offset + offset;
                if (offset > subTableOffset && absolute < subTableEnd && absolute <= tableEnd)
                {
                    subTableEnd = absolute;
                }
            }

            return subTableEnd;
        }

        private string FormatUmcRegister(uint rawRegister)
        {
            uint address = rawRegister & 0x00FFFFFFu;
            bool indirect = (rawRegister & 0x01000000u) != 0;
            return "0x" + address.ToString("X6") + (indirect ? "i" : "");
        }

        private void PopulateUmcInitRegBlock(string label, ushort relativeOffset)
        {
            if (relativeOffset == 0)
            {
                return;
            }

            int blockOffset = atom_vram_info_offset + relativeOffset;
            int blockEnd = GetVramSubTableEnd(relativeOffset);
            if (!HasBufferRange(blockOffset, 4) || blockEnd <= blockOffset + 4)
            {
                tableVRAM_TIMING.Items.Add(new
                {
                    MHZ = label,
                    VALUE = "Invalid UMC block @ 0x" + blockOffset.ToString("X")
                });
                return;
            }

            int regCount = ReadUInt16At(blockOffset, label + " UMC register count");
            int regListOffset = blockOffset + 4;
            int settingOffset = regListOffset + regCount * 4;
            if (regCount <= 0 || settingOffset > blockEnd)
            {
                tableVRAM_TIMING.Items.Add(new
                {
                    MHZ = label,
                    VALUE = "No UMC timing registers @ 0x" + blockOffset.ToString("X")
                });
                return;
            }

            uint[] registers = new uint[regCount];
            for (var i = 0; i < registers.Length; i++)
            {
                registers[i] = ReadUInt32At(regListOffset + i * 4, label + " UMC register");
            }

            tableVRAM_TIMING.Items.Add(new
            {
                MHZ = label,
                VALUE = regCount + " regs @ 0x" + blockOffset.ToString("X")
            });

            int settingSize = 4 + regCount * 4;
            int blockIndex = 0;
            while (settingOffset + settingSize <= blockEnd && blockIndex < 128)
            {
                uint settingId = ReadUInt32At(settingOffset, label + " UMC setting id");
                uint memClockRange = settingId & 0x00FFFFFFu;
                uint memBlockId = settingId >> 24;
                List<string> values = new List<string>();

                for (var i = 0; i < registers.Length; i++)
                {
                    uint data = ReadUInt32At(settingOffset + 4 + i * 4, label + " UMC setting data");
                    values.Add(FormatUmcRegister(registers[i]) + "=" + data.ToString("X8"));
                }

                tableVRAM_TIMING.Items.Add(new
                {
                    MHZ = DecodeClockValue(memClockRange) + " blk " + memBlockId,
                    VALUE = String.Join(" ", values)
                });

                settingOffset += settingSize;
                blockIndex++;
            }

            if (blockIndex == 0)
            {
                tableVRAM_TIMING.Items.Add(new
                {
                    MHZ = label,
                    VALUE = "No UMC timing settings @ 0x" + settingOffset.ToString("X")
                });
            }
        }

        private void LoadVramInfo()
        {
            atom_vram_entries = null;
            atom_vram_timing_entries = null;

            atom_vram_info_offset = atom_data_table.VRAM_Info;
            if (atom_vram_info_offset <= 0 || !HasBufferRange(atom_vram_info_offset, Marshal.SizeOf(typeof(ATOM_VRAM_INFO))))
            {
                return;
            }

            atom_vram_info = ReadStruct<ATOM_VRAM_INFO>(atom_vram_info_offset);
            if (atom_vram_info.ucNumOfVRAMModule == 0)
            {
                return;
            }

            int moduleCount = Math.Min((int)atom_vram_info.ucNumOfVRAMModule, 16);
            atom_vram_entries = new ATOM_VRAM_ENTRY[moduleCount];
            int entryOffset = atom_vram_info_offset + Marshal.SizeOf(typeof(ATOM_VRAM_INFO));

            for (var i = 0; i < atom_vram_entries.Length; i++)
            {
                atom_vram_entries[i] = ReadStruct<ATOM_VRAM_ENTRY>(entryOffset);
                entryOffset += GetVramModuleSize(atom_vram_entries[i]);
            }
        }

        private void WriteVramInfo()
        {
            if (atom_vram_entries == null || atom_vram_entries.Length == 0)
            {
                return;
            }

            int entryOffset = atom_vram_info_offset + Marshal.SizeOf(typeof(ATOM_VRAM_INFO));
            for (var i = 0; i < atom_vram_entries.Length; i++)
            {
                WriteStruct(entryOffset, atom_vram_entries[i]);
                entryOffset += GetVramModuleSize(atom_vram_entries[i]);
            }
        }

        private void PopulateRomTable()
        {
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
            tableROM.Items.Add(new
            {
                NAME = "ASIC",
                VALUE = GetAsicName()
            });
        }

        private void PopulateVega12Summary()
        {
            tablePOWERPLAY.Items.Clear();
            tablePOWERPLAY.Items.Add(new
            {
                NAME = "PowerPlay Table",
                VALUE = "Vega12 read-only"
            });
            tablePOWERPLAY.Items.Add(new
            {
                NAME = "Small Power 1 (W)",
                VALUE = atom_vega12_powerplay_table.usSmallPowerLimit1
            });
            tablePOWERPLAY.Items.Add(new
            {
                NAME = "Small Power 2 (W)",
                VALUE = atom_vega12_powerplay_table.usSmallPowerLimit2
            });
            tablePOWERPLAY.Items.Add(new
            {
                NAME = "Boost Power (W)",
                VALUE = atom_vega12_powerplay_table.usBoostPowerLimit
            });
            tablePOWERPLAY.Items.Add(new
            {
                NAME = "Shutdown Temp. (C)",
                VALUE = atom_vega12_powerplay_table.usSoftwareShutdownTemp
            });

            tablePOWERTUNE.Items.Clear();
            tableFAN.Items.Clear();
            tableGPU.Items.Clear();
            tableMEMORY.Items.Clear();
        }

        private void PopulateVramTables()
        {
            tableVRAM.Items.Clear();
            tableVRAM_TIMING.Items.Clear();
            listVRAM.Items.Clear();
            atom_vram_index = -1;

            if (atom_vram_entries == null || atom_vram_entries.Length == 0)
            {
                return;
            }

            for (var i = 0; i < atom_vram_entries.Length; i++)
            {
                listVRAM.Items.Add(GetVramModuleName(atom_vram_entries[i], i));
            }

            listVRAM.SelectedIndex = 0;
            ShowVramEntry(0);
            PopulateVramTimingSummary();
        }

        private void PopulateVramTimingSummary()
        {
            PopulateUmcInitRegBlock("UMC timing", atom_vram_info.usMemClkPatchTblOffset);
        }

        private void ShowVramEntry(int index)
        {
            tableVRAM.Items.Clear();
            if (atom_vram_entries == null || index < 0 || index >= atom_vram_entries.Length)
            {
                return;
            }

            atom_vram_index = index;
            ATOM_VRAM_ENTRY entry = atom_vram_entries[atom_vram_index];
            tableVRAM.Items.Add(new
            {
                NAME = "Vendor/Rev ID",
                VALUE = "0x" + entry.ucMemoryVendorRevID.ToString("X2")
            });
            tableVRAM.Items.Add(new
            {
                NAME = "HBM Vendor ID",
                VALUE = "0x" + entry.ucHbmVendorRevID.ToString("X2")
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Size (MB)",
                VALUE = entry.ulMemorySize
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Max Mem Clock (MHz)",
                VALUE = DecodeClockValue(entry.ulMaxMemClk)
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Mem Voltage (mV)",
                VALUE = entry.usMemVoltage
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Type",
                VALUE = "0x" + entry.ucMemoryType.ToString("X2")
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Density",
                VALUE = "0x" + entry.ucDensity.ToString("X2")
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Channels",
                VALUE = entry.ucChannelNum
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Channel Width",
                VALUE = entry.ucChannelWidth
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Channel Enable",
                VALUE = "0x" + entry.ulChannelEnable.ToString("X")
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Tuning Set",
                VALUE = entry.ucTuningSetId
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Refresh",
                VALUE = entry.ucRefreshRate
            });
            tableVRAM.Items.Add(new
            {
                NAME = "Part Number",
                VALUE = ReadFixedAscii(entry.strMemPNString)
            });
        }

        public MainWindow()
        {
            InitializeComponent();
            baseWindowTitle = Title + TOOL_VERSION + " " + TOOL_EXTRA;
            Title = baseWindowTitle;
            SetEditorEnabled(false);

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
                SetEditorEnabled(false);
                atom_vram_entries = null;
                atom_vram_timing_entries = null;
                asicFamily = AsicFamily.Unknown;
                ClearTables();
                Title = baseWindowTitle + " - [" + openFileDialog.SafeFileName + "]";

                try
                {
                    using (Stream fileStream = openFileDialog.OpenFile())
                    {
                        if ((fileStream.Length != 524288) && (fileStream.Length != 524288 / 2))
                        {
                            MessageBox.Show("This BIOS is non standard size.\nFlashing this BIOS may corrupt your graphics card.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        using (BinaryReader br = new BinaryReader(fileStream))
                        {
                            buffer = br.ReadBytes((int)fileStream.Length);
                        }
                    }

                    atom_rom_header_offset = getValueAtPosition(16, atom_rom_header_ptr);
                    if (atom_rom_header_offset < 0)
                    {
                        throw new InvalidDataException("ATOM ROM header pointer is outside the BIOS buffer.");
                    }

                    atom_rom_header = ReadStruct<ATOM_ROM_HEADER>(atom_rom_header_offset);
                    deviceID = atom_rom_header.usDeviceID.ToString("X4");
                    asicFamily = GetAsicFamily(deviceID);
                    fixChecksum(false);

                    MessageBoxResult msgSuported = MessageBoxResult.Yes;
                    if (!IsSupportedDeviceID(deviceID))
                    {
                        msgSuported = MessageBox.Show("Unsupported DeviceID 0x" + deviceID + " - Continue?", "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                    if (msgSuported == MessageBoxResult.Yes)
                    {
                        atom_data_table = ReadStruct<ATOM_DATA_TABLES>(atom_rom_header.usMasterDataTableOffset);
                        if (asicFamily == AsicFamily.Unknown)
                        {
                            asicFamily = AsicFamily.Vega10;
                        }

                        if (asicFamily == AsicFamily.Vega12)
                        {
                            atom_vega12_powerplay_offset = atom_data_table.PowerPlayInfo;
                            atom_vega12_powerplay_table = ReadStruct<ATOM_VEGA12_POWERPLAYTABLE>(atom_vega12_powerplay_offset);
                            LoadVramInfo();
                            PopulateRomTable();
                            PopulateVega12Summary();
                            PopulateVramTables();
                            SetEditorEnabled(true);
                            txtRamNotes.Text = (ReadAscii(atom_rom_header.usConfigFilenameOffset, 12) + " " + ReadAscii(atom_rom_header.usBIOS_BootupMessageOffset + 2, 64)).Trim();
                            MessageBox.Show("Vega12 BIOS recognized. PowerPlay editing is read-only until the Vega12 SMU table writer is added.", "Vega12", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        atom_vega10_powerplay_offset = atom_data_table.PowerPlayInfo;
                        atom_vega10_powerplay_table = ReadStruct<ATOM_Vega10_POWERPLAYTABLE>(atom_vega10_powerplay_offset);

                        atom_vega10_powertune_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usPowerTuneTableOffset;
                        atom_vega10_powertune_table = ReadStruct<ATOM_Vega10_PowerTune_Table>(atom_vega10_powertune_table_offset);

                        atom_vega10_fan_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usFanTableOffset;
                        atom_vega10_fan_table = ReadStruct<ATOM_Vega10_Fan_Table>(atom_vega10_fan_offset);

                        atom_vega10_mclk_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usMclkDependencyTableOffset;
                        atom_vega10_mclk_table = ReadStruct<ATOM_Vega10_MCLK_Dependency_Table>(atom_vega10_mclk_table_offset);
                        atom_vega10_mclk_entries = new ATOM_Vega10_MCLK_Dependency_Record[atom_vega10_mclk_table.ucNumEntries];
                        for (var i = 0; i < atom_vega10_mclk_entries.Length; i++)
                        {
                            atom_vega10_mclk_entries[i] = ReadStruct<ATOM_Vega10_MCLK_Dependency_Record>(atom_vega10_mclk_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_MCLK_Dependency_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_MCLK_Dependency_Record)) * i);
                        }

                        atom_vega10_gfxclk_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usGfxclkDependencyTableOffset;
                        atom_vega10_gfxclk_table = ReadStruct<ATOM_Vega10_GFXCLK_Dependency_Table>(atom_vega10_gfxclk_table_offset);
                        atom_vega10_gfxclk_entries = new ATOM_Vega10_GFXCLK_Dependency_Record_V2[atom_vega10_gfxclk_table.ucNumEntries];
                        for (var i = 0; i < atom_vega10_gfxclk_entries.Length; i++)
                        {
                            atom_vega10_gfxclk_entries[i] = ReadGfxclkDependencyRecord(i);
                        }

                        atom_vddc_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usVddcLookupTableOffset;
                        atom_vddc_table = ReadStruct<ATOM_Vega10_Voltage_Lookup_Table>(atom_vddc_table_offset);
                        atom_vddc_entries = new ATOM_Vega10_Voltage_Lookup_Record[atom_vddc_table.ucNumEntries];
                        for (var i = 0; i < atom_vddc_table.ucNumEntries; i++)
                        {
                            atom_vddc_entries[i] = ReadStruct<ATOM_Vega10_Voltage_Lookup_Record>(atom_vddc_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Record)) * i);
                        }

                        atom_vega10_memvdd_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usVddmemLookupTableOffset;
                        atom_vega10_memvdd_table = ReadStruct<ATOM_Vega10_Voltage_Lookup_Table>(atom_vega10_memvdd_table_offset);
                        atom_vega10_memvdd_record = new ATOM_Vega10_Voltage_Lookup_Record[atom_vega10_memvdd_table.ucNumEntries];
                        for (var i = 0; i < atom_vega10_memvdd_table.ucNumEntries; i++)
                        {
                            atom_vega10_memvdd_record[i] = ReadStruct<ATOM_Vega10_Voltage_Lookup_Record>(atom_vega10_memvdd_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Record)) * i);
                        }

                        atom_vega10_socclk_table_offset = atom_data_table.PowerPlayInfo + atom_vega10_powerplay_table.usSocclkDependencyTableOffset;
                        atom_vega10_socclk_table = ReadStruct<ATOM_Vega10_SOCCLK_Dependency_Table>(atom_vega10_socclk_table_offset);
                        atom_vega10_clk_entries = new ATOM_Vega10_CLK_Dependency_Record[atom_vega10_socclk_table.ucNumEntries];
                        for (var i = 0; i < atom_vega10_clk_entries.Length; i++)
                        {
                            atom_vega10_clk_entries[i] = ReadStruct<ATOM_Vega10_CLK_Dependency_Record>(atom_vega10_socclk_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_SOCCLK_Dependency_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_CLK_Dependency_Record)) * i);
                        }
                        LoadVramInfo();

                        PopulateRomTable();

                        tablePOWERPLAY.Items.Clear();
                        tablePOWERPLAY.Items.Add(new
                        {
                            NAME = "Max GPU Freq. (MHz)",
                            VALUE = DecodeClockValue(atom_vega10_powerplay_table.ulMaxODEngineClock)
                        });
                        tablePOWERPLAY.Items.Add(new
                        {
                            NAME = "Max Memory Freq. (MHz)",
                            VALUE = DecodeClockValue(atom_vega10_powerplay_table.ulMaxODMemoryClock)
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
                                MHZ = DecodeClockValue(atom_vega10_gfxclk_entries[i].ulClk),
                                MV = GetVoltageValue(atom_vddc_entries, atom_vega10_gfxclk_entries[i].ucVddInd, "VDDC"),
                                TT = ("0x" + GetVoltageValue(atom_vddc_entries, atom_vega10_gfxclk_entries[i].ucVddInd, "VDDC").ToString("X"))
                            });
                        }

                        tableMEMORY.Items.Clear();
                        for (var i = 0; i < atom_vega10_mclk_table.ucNumEntries; i++)
                        {
                            tableMEMORY.Items.Add(new
                            {
                                MHZ = DecodeClockValue(atom_vega10_mclk_entries[i].ulMemClk),
                                MV = GetVoltageValue(atom_vega10_memvdd_record, atom_vega10_mclk_entries[i].ucVddMemInd, "MEMVDD"),
                                TT = ("0x" + GetVoltageValue(atom_vega10_memvdd_record, atom_vega10_mclk_entries[i].ucVddMemInd, "MEMVDD").ToString("X"))
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
                        PopulateVramTables();
                        SetEditorEnabled(true);

                        // Some BIOS attributes to describe the file
                        txtRamNotes.Text = (ReadAscii(atom_rom_header.usConfigFilenameOffset, 12) + " " + ReadAscii(atom_rom_header.usBIOS_BootupMessageOffset + 2, 64)).Trim();
                    }
                }
                catch (Exception ex)
                {
                    buffer = null;
                    ClearTables();
                    SetEditorEnabled(false);
                    Title = baseWindowTitle;
                    MessageBox.Show("Could not open BIOS file:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public Int32 getValueAtPosition(int bits, int position, bool isFrequency = false)
        {
            int value = 0;
            int byteCount = bits / 8;
            if (bits == 24)
            {
                byteCount = 3;
            }

            if (HasBufferRange(position, byteCount))
            {
                switch (bits)
                {
                    case 8:
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
                    default:
                        return -1;
                }
                if (isFrequency) return value / 100;
                return value;
            }
            return -1;
        }

        public bool setValueAtPosition(int value, int bits, int position, bool isFrequency = false)
        {
            if (isFrequency) value *= 100;
            int byteCount = bits / 8;
            if (bits == 24)
            {
                byteCount = 3;
            }

            if (HasBufferRange(position, byteCount))
            {
                uint rawValue = unchecked((uint)value);
                switch (bits)
                {
                    case 8:
                        buffer[position] = (byte)rawValue;
                        break;
                    case 16:
                        buffer[position] = (byte)rawValue;
                        buffer[position + 1] = (byte)(rawValue >> 8);
                        break;
                    case 24:
                        buffer[position] = (byte)rawValue;
                        buffer[position + 1] = (byte)(rawValue >> 8);
                        buffer[position + 2] = (byte)(rawValue >> 16);
                        break;
                    case 32:
                        buffer[position] = (byte)rawValue;
                        buffer[position + 1] = (byte)(rawValue >> 8);
                        buffer[position + 2] = (byte)(rawValue >> 16);
                        buffer[position + 3] = (byte)(rawValue >> 24);
                        break;
                    default:
                        return false;
                }
                return true;
            }
            return false;
        }

        private bool setValueAtPosition(String text, int bits, int position, bool isFrequency = false)
        {
            int value = 0;
            if (!TryParseInt32(text, out value))
            {
                return false;
            }
            return setValueAtPosition(value, bits, position, isFrequency);
        }

        private bool TryParseInt32(string text, out int value)
        {
            uint unsignedValue;
            if (TryParseUInt32(text, out unsignedValue) && unsignedValue <= Int32.MaxValue)
            {
                value = (int)unsignedValue;
                return true;
            }

            value = 0;
            return false;
        }

        private bool TryParseUInt32(string text, out uint value)
        {
            value = 0;
            if (String.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return UInt32.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            return UInt32.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private uint ParseUInt32Value(string text, string name)
        {
            uint value;
            if (!TryParseUInt32(text, out value))
            {
                throw new InvalidDataException(name + " has an invalid numeric value: " + text);
            }

            return value;
        }

        private ushort ParseUInt16Value(string text, string name)
        {
            uint value = ParseUInt32Value(text, name);
            if (value > UInt16.MaxValue)
            {
                throw new InvalidDataException(name + " is larger than 65535.");
            }

            return (ushort)value;
        }

        private byte ParseByteValue(string text, string name)
        {
            uint value = ParseUInt32Value(text, name);
            if (value > Byte.MaxValue)
            {
                throw new InvalidDataException(name + " is larger than 255.");
            }

            return (byte)value;
        }

        private string GetItemText(ListView table, int index, string elementName)
        {
            if (index < 0 || index >= table.Items.Count)
            {
                throw new InvalidDataException(table.Name + " row " + index + " is invalid.");
            }

            table.ScrollIntoView(table.Items[index]);
            table.UpdateLayout();

            var container = table.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
            var element = FindByName(elementName, container);
            if (element is TextBox)
            {
                return ((TextBox)element).Text;
            }
            if (element is TextBlock)
            {
                return ((TextBlock)element).Text;
            }

            throw new InvalidDataException("Cannot read " + elementName + " from " + table.Name + " row " + index + ".");
        }

        private uint ParseClockValue(string text, string name)
        {
            uint mhz = ParseUInt32Value(text, name);
            if (mhz > UInt32.MaxValue / 100u)
            {
                throw new InvalidDataException(name + " is too large.");
            }

            return mhz * 100u;
        }

        private byte ParseRpmHundreds(string text, string name)
        {
            uint rpm = ParseUInt32Value(text, name);
            if (rpm % 100u != 0)
            {
                throw new InvalidDataException(name + " must be a multiple of 100 RPM.");
            }

            uint encoded = rpm / 100u;
            if (encoded > Byte.MaxValue)
            {
                throw new InvalidDataException(name + " is larger than 25500 RPM.");
            }

            return (byte)encoded;
        }

        private void SaveFileDialog_Click(object sender, RoutedEventArgs e)
        {
            if (buffer == null)
            {
                MessageBox.Show("Open a BIOS file before saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (asicFamily != AsicFamily.Vega10)
            {
                MessageBox.Show("Saving is only enabled for Vega10 PowerPlay tables right now.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save As";
            saveFileDialog.Filter = "BIOS (*.rom)|*.rom";

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                ATOM_ROM_HEADER romHeader = atom_rom_header;
                ATOM_Vega10_POWERPLAYTABLE powerplayTable = atom_vega10_powerplay_table;
                ATOM_Vega10_PowerTune_Table powertuneTable = atom_vega10_powertune_table;
                ATOM_Vega10_Fan_Table fanTable = atom_vega10_fan_table;
                ATOM_Vega10_GFXCLK_Dependency_Record_V2[] gfxEntries = (ATOM_Vega10_GFXCLK_Dependency_Record_V2[])atom_vega10_gfxclk_entries.Clone();
                ATOM_Vega10_MCLK_Dependency_Record[] mclkEntries = (ATOM_Vega10_MCLK_Dependency_Record[])atom_vega10_mclk_entries.Clone();
                ATOM_Vega10_Voltage_Lookup_Record[] vddcEntries = (ATOM_Vega10_Voltage_Lookup_Record[])atom_vddc_entries.Clone();
                ATOM_Vega10_Voltage_Lookup_Record[] memvddEntries = (ATOM_Vega10_Voltage_Lookup_Record[])atom_vega10_memvdd_record.Clone();

                for (var i = 0; i < tableROM.Items.Count; i++)
                {
                    var name = GetItemText(tableROM, i, "NAME");
                    var value = GetItemText(tableROM, i, "VALUE");

                    if (name == "VendorID")
                    {
                        romHeader.usVendorID = ParseUInt16Value(value, name);
                    }
                    else if (name == "DeviceID")
                    {
                        romHeader.usDeviceID = ParseUInt16Value(value, name);
                    }
                    else if (name == "Sub ID")
                    {
                        romHeader.usSubsystemID = ParseUInt16Value(value, name);
                    }
                    else if (name == "Sub VendorID")
                    {
                        romHeader.usSubsystemVendorID = ParseUInt16Value(value, name);
                    }
                    else if (name == "Firmware Signature")
                    {
                        romHeader.uaFirmWareSignature = ParseUInt32Value(value, name);
                    }
                }

                for (var i = 0; i < tablePOWERPLAY.Items.Count; i++)
                {
                    var name = GetItemText(tablePOWERPLAY, i, "NAME");
                    var value = GetItemText(tablePOWERPLAY, i, "VALUE");

                    if (name == "Max GPU Freq. (MHz)")
                    {
                        powerplayTable.ulMaxODEngineClock = ParseClockValue(value, name);
                    }
                    else if (name == "Max Memory Freq. (MHz)")
                    {
                        powerplayTable.ulMaxODMemoryClock = ParseClockValue(value, name);
                    }
                    else if (name == "Power Control Limit (%)")
                    {
                        powerplayTable.usPowerControlLimit = ParseUInt16Value(value, name);
                    }
                    else if (name == "ULV VoltageOffset (mV)")
                    {
                        powerplayTable.usUlvVoltageOffset = ParseUInt16Value(value, name);
                    }
                }

                for (var i = 0; i < tablePOWERTUNE.Items.Count; i++)
                {
                    var name = GetItemText(tablePOWERTUNE, i, "NAME");
                    var value = GetItemText(tablePOWERTUNE, i, "VALUE");

                    if (name == "Socket Power (W)")
                    {
                        powertuneTable.usSocketPowerLimit = ParseUInt16Value(value, name);
                    }
                    else if (name == "Battery Power (W)")
                    {
                        powertuneTable.usBatteryPowerLimit = ParseUInt16Value(value, name);
                    }
                    else if (name == "Small Power Limit (W)")
                    {
                        powertuneTable.usSmallPowerLimit = ParseUInt16Value(value, name);
                    }
                    else if (name == "EDC Module Limit")
                    {
                        powertuneTable.usEdcLimit = ParseUInt16Value(value, name);
                    }
                    else if (name == "TDC (A)")
                    {
                        powertuneTable.usTdcLimit = ParseUInt16Value(value, name);
                    }
                    else if (name == "Temp. Limit (C)")
                    {
                        powertuneTable.usTemperatureLimitTedge = ParseUInt16Value(value, name);
                    }
                    else if (name == "Shutdown Temp. (C)")
                    {
                        powertuneTable.usSoftwareShutdownTemp = ParseUInt16Value(value, name);
                    }
                    else if (name == "Hotspot Temp. (C)")
                    {
                        powertuneTable.usTemperatureLimitHotSpot = ParseUInt16Value(value, name);
                    }
                    else if (name == "Liquid 1 Temp. (C)")
                    {
                        powertuneTable.usTemperatureLimitLiquid1 = ParseUInt16Value(value, name);
                    }
                    else if (name == "Liquid 2 Temp. (C)")
                    {
                        powertuneTable.usTemperatureLimitLiquid2 = ParseUInt16Value(value, name);
                    }
                    else if (name == "HBM Temp. (C)")
                    {
                        powertuneTable.usTemperatureLimitHBM = ParseUInt16Value(value, name);
                    }
                    else if (name == "VR Soc Temp. (C)")
                    {
                        powertuneTable.usTemperatureLimitVrSoc = ParseUInt16Value(value, name);
                    }
                    else if (name == "VR Mem Temp. (C)")
                    {
                        powertuneTable.usTemperatureLimitVrMem = ParseUInt16Value(value, name);
                    }
                    else if (name == "PLX Temp. (C)")
                    {
                        powertuneTable.usTemperatureLimitPlx = ParseUInt16Value(value, name);
                    }
                }

                for (var i = 0; i < tableFAN.Items.Count; i++)
                {
                    var name = GetItemText(tableFAN, i, "NAME");
                    var value = GetItemText(tableFAN, i, "VALUE");

                    if (name == "Sensitivity")
                    {
                        fanTable.usFanOutputSensitivity = ParseUInt16Value(value, name);
                    }
                    else if (name == "Target Temp. (C)")
                    {
                        fanTable.usTargetTemperature = ParseUInt16Value(value, name);
                    }
                    else if (name == "Throttling RPM")
                    {
                        fanTable.usThrottlingRPM = ParseUInt16Value(value, name);
                    }
                    else if (name == "Target Gfx Clk")
                    {
                        fanTable.usTargetGfxClk = ParseUInt16Value(value, name);
                    }
                    else if (name == "Fan Gain: Edge")
                    {
                        fanTable.usFanGainEdge = ParseUInt16Value(value, name);
                    }
                    else if (name == "Fan Gain: Hotspot")
                    {
                        fanTable.usFanGainHotspot = ParseUInt16Value(value, name);
                    }
                    else if (name == "Fan Gain: Liquid")
                    {
                        fanTable.usFanGainLiquid = ParseUInt16Value(value, name);
                    }
                    else if (name == "Fan Gain: VrVddc")
                    {
                        fanTable.usFanGainVrVddc = ParseUInt16Value(value, name);
                    }
                    else if (name == "Fan Gain: VrMvdd")
                    {
                        fanTable.usFanGainVrMvdd = ParseUInt16Value(value, name);
                    }
                    else if (name == "Fan Gain: Plx")
                    {
                        fanTable.usFanGainPlx = ParseUInt16Value(value, name);
                    }
                    else if (name == "Fan Gain: HBM")
                    {
                        fanTable.usFanGainHbm = ParseUInt16Value(value, name);
                    }
                    else if (name == "Acoustic Limit (MHz)")
                    {
                        fanTable.usFanAcousticLimitRpm = ParseUInt16Value(value, name);
                    }
                    else if (name == "Fan Parameters")
                    {
                        fanTable.ucFanParameters = ParseByteValue(value, name);
                    }
                    else if (name == "Fan Min RPM")
                    {
                        fanTable.ucFanMinRPM = ParseRpmHundreds(value, name);
                    }
                    else if (name == "Fan Max RPM")
                    {
                        fanTable.ucFanMaxRPM = ParseRpmHundreds(value, name);
                    }
                }

                for (var i = 0; i < tableGPU.Items.Count; i++)
                {
                    if (i >= gfxEntries.Length)
                    {
                        throw new InvalidDataException("GPU table row count does not match the BIOS table.");
                    }

                    gfxEntries[i].ulClk = ParseClockValue(GetItemText(tableGPU, i, "MHZ"), "GPU MHz");
                    SetVoltageValue(vddcEntries, gfxEntries[i].ucVddInd, ParseUInt16Value(GetItemText(tableGPU, i, "MV"), "GPU mV"), "VDDC");
                }

                for (var i = 0; i < tableMEMORY.Items.Count; i++)
                {
                    if (i >= mclkEntries.Length)
                    {
                        throw new InvalidDataException("Memory table row count does not match the BIOS table.");
                    }

                    mclkEntries[i].ulMemClk = ParseClockValue(GetItemText(tableMEMORY, i, "MHZ"), "Memory MHz");
                    SetVoltageValue(memvddEntries, mclkEntries[i].ucVddMemInd, ParseUInt16Value(GetItemText(tableMEMORY, i, "MV"), "Memory mV"), "MEMVDD");
                }

                updateVRAM_entries();

                atom_rom_header = romHeader;
                atom_vega10_powerplay_table = powerplayTable;
                atom_vega10_powertune_table = powertuneTable;
                atom_vega10_fan_table = fanTable;
                atom_vega10_gfxclk_entries = gfxEntries;
                atom_vega10_mclk_entries = mclkEntries;
                atom_vddc_entries = vddcEntries;
                atom_vega10_memvdd_record = memvddEntries;

                WriteStruct(atom_rom_header_offset, atom_rom_header);
                WriteStruct(atom_vega10_powerplay_offset, atom_vega10_powerplay_table);
                WriteStruct(atom_vega10_powertune_table_offset, atom_vega10_powertune_table);
                WriteStruct(atom_vega10_fan_offset, atom_vega10_fan_table);

                for (var i = 0; i < atom_vega10_mclk_table.ucNumEntries; i++)
                {
                    WriteStruct(atom_vega10_mclk_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_MCLK_Dependency_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_MCLK_Dependency_Record)) * i, atom_vega10_mclk_entries[i]);
                }

                for (var i = 0; i < atom_vega10_gfxclk_table.ucNumEntries; i++)
                {
                    WriteGfxclkDependencyRecord(i, atom_vega10_gfxclk_entries[i]);
                }

                for (var i = 0; i < atom_vddc_table.ucNumEntries; i++)
                {
                    WriteStruct(atom_vddc_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Record)) * i, atom_vddc_entries[i]);
                }

                for (var i = 0; i < atom_vega10_memvdd_table.ucNumEntries; i++)
                {
                    WriteStruct(atom_vega10_memvdd_table_offset + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Table)) + Marshal.SizeOf(typeof(ATOM_Vega10_Voltage_Lookup_Record)) * i, atom_vega10_memvdd_record[i]);
                }

                WriteVramInfo();

                fixChecksum(true);
                File.WriteAllBytes(saveFileDialog.FileName, buffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save BIOS file:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void fixChecksum(bool save)
        {
            if (!HasBufferRange(atom_rom_checksum_offset, 1) || !HasBufferRange(0x02, 1))
            {
                throw new InvalidDataException("Cannot calculate checksum because the ROM header is incomplete.");
            }

            byte checksum = buffer[atom_rom_checksum_offset];
            int size = buffer[0x02] * 512;
            if (size <= 0 || size > buffer.Length)
            {
                size = buffer.Length;
            }

            int sum = 0;

            for (int i = 0; i < size; i++)
            {
                sum = (sum + buffer[i]) & 0xFF;
            }

            if (sum == 0)
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
                buffer[atom_rom_checksum_offset] = unchecked((byte)(checksum - sum));
                txtChecksum.Foreground = Brushes.Green;
            }
            txtChecksum.Text = "0x" + buffer[atom_rom_checksum_offset].ToString("X2");
        }

        private FrameworkElement FindByName(string name, FrameworkElement root)
        {
            if (root == null)
            {
                return null;
            }

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
            if (hex == null)
            {
                throw new InvalidDataException();
            }

            hex = hex.Trim().Replace(" ", "").Replace("-", "");
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
            if (atom_vram_entries == null || atom_vram_index < 0 || atom_vram_index >= atom_vram_entries.Length)
            {
                return;
            }

            for (var i = 0; i < tableVRAM.Items.Count; i++)
            {
                var name = GetItemText(tableVRAM, i, "NAME");
                var value = GetItemText(tableVRAM, i, "VALUE");

                if (name == "Vendor/Rev ID")
                {
                    atom_vram_entries[atom_vram_index].ucMemoryVendorRevID = ParseByteValue(value, name);
                }
                else if (name == "HBM Vendor ID")
                {
                    atom_vram_entries[atom_vram_index].ucHbmVendorRevID = ParseByteValue(value, name);
                }
                else if (name == "Size (MB)")
                {
                    atom_vram_entries[atom_vram_index].ulMemorySize = ParseUInt32Value(value, name);
                }
                else if (name == "Max Mem Clock (MHz)")
                {
                    atom_vram_entries[atom_vram_index].ulMaxMemClk = ParseClockValue(value, name);
                }
                else if (name == "Mem Voltage (mV)")
                {
                    atom_vram_entries[atom_vram_index].usMemVoltage = ParseUInt16Value(value, name);
                }
                else if (name == "Density")
                {
                    atom_vram_entries[atom_vram_index].ucDensity = ParseByteValue(value, name);
                }
                else if (name == "Type")
                {
                    atom_vram_entries[atom_vram_index].ucMemoryType = ParseByteValue(value, name);
                }
                else if (name == "Channels")
                {
                    atom_vram_entries[atom_vram_index].ucChannelNum = ParseByteValue(value, name);
                }
                else if (name == "Channel Width")
                {
                    atom_vram_entries[atom_vram_index].ucChannelWidth = ParseByteValue(value, name);
                }
                else if (name == "Channel Enable")
                {
                    atom_vram_entries[atom_vram_index].ulChannelEnable = ParseUInt32Value(value, name);
                }
                else if (name == "Tuning Set")
                {
                    atom_vram_entries[atom_vram_index].ucTuningSetId = ParseByteValue(value, name);
                }
                else if (name == "Refresh")
                {
                    atom_vram_entries[atom_vram_index].ucRefreshRate = ParseByteValue(value, name);
                }
                else if (name == "Part Number")
                {
                    atom_vram_entries[atom_vram_index].strMemPNString = MakeFixedAscii(value, 20);
                }
            }
        }

        private void listVRAM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateVRAM_entries();
            ShowVramEntry(listVRAM.SelectedIndex);
        }

        private void listVRAM_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
