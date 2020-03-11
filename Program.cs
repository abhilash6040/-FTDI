using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTD2XX_NET;
using System.Threading;

namespace application
{
    class Program
    {
        static void Main(string[] args)
        {


            if (args.Count() > 0)
            {
                ExecuteOption(args[0]);
            }
            else
            {
                string option;
                do
                {
                    var menu = new StringBuilder();
                    menu.AppendLine("(1) ListenForData");
                    menu.AppendLine("(x) Exit");

                    System.Console.WriteLine(menu);
                    option = System.Console.ReadKey().KeyChar.ToString();
                    System.Console.WriteLine();
                    ExecuteOption(option);
                    System.Console.WriteLine();
                } while (option.ToLower() != "x");
            }
        }

        private static void ExecuteOption(string option)
        {
            switch (option.ToLower())
            {
                case "1":
                    ListenForData();
                    break;
                case "x":
                    break;
                default:
                    System.Console.WriteLine("That's not an option!");
                    break;
            }
        }

        private static void ListenForData()
        {
            try
            {
                UInt32 ftdiDeviceCount = 0;
                FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

                FTDI myFtdiDevice = new FTDI();

                ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);

                if (ftStatus == FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Number of FTDI devices: " + ftdiDeviceCount.ToString());
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }

                if (ftdiDeviceCount == 0)
                {
                    Console.WriteLine("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }

                FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

                ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);

                if (ftStatus == FTDI.FT_STATUS.FT_OK)
                {
                    for (UInt32 i = 0; i < ftdiDeviceCount; i++)
                    {
                        Console.WriteLine("Device Index: " + i.ToString());
                        Console.WriteLine("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                        Console.WriteLine("Type: " + ftdiDeviceList[i].Type.ToString());
                        Console.WriteLine("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                        Console.WriteLine("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                        Console.WriteLine("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                        Console.WriteLine("Description: " + ftdiDeviceList[i].Description.ToString());
                        Console.WriteLine("");
                    }
                }

                ftStatus = myFtdiDevice.OpenBySerialNumber(ftdiDeviceList[0].SerialNumber);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to open device (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }

                ftStatus = myFtdiDevice.SetBaudRate(115200);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to set Baud rate (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }

                ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);

                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to set data characteristics (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }

                ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x11, 0x13);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to set flow control (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }

                // Set read timeout to 12 seconds, write timeout to infinite
                ftStatus = myFtdiDevice.SetTimeouts(12000, 0);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to set timeouts (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }

                UInt32 numBytesAvailable = 0;

                ftStatus = myFtdiDevice.GetRxBytesAvailable(ref numBytesAvailable);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to get number of bytes available to read (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }
                Thread.Sleep(10);

                string readData = "";
                UInt32 numBytesRead = 0;
                byte[] dataBuffer = new byte[1024];

                ftStatus = myFtdiDevice.Read(out readData, numBytesAvailable, ref numBytesRead);
                while (readData == "")
                {
                    ftStatus = myFtdiDevice.Read(out readData, numBytesAvailable, ref numBytesRead);
                }

                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Failed to read data (error " + ftStatus.ToString() + ")");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine(readData);

                ftStatus = myFtdiDevice.Close();

                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }
            catch
            {
                Console.WriteLine("Something realy bad happened..");


            }
        }
    }
}

