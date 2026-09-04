using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BDSender.cls
{
    class Class1
    {
        public static SerialPort _serialPort = new SerialPort();
        public static string data { get; set; } = "";
        public static bool PortOpenflag { get; set; }
        public static Byte[] data_in = new Byte[400];
        public static int data_addr { get; set; }
        public static void Main_Prog()
        {
            try
            {
                if (_serialPort != null)
                {
                    if (PortOpenflag && !_serialPort.IsOpen)
                    {
                        string[] availablePorts = SerialPort.GetPortNames();
                        if (availablePorts.Contains(_serialPort.PortName))
                        {
                            _serialPort.Open();
                        }
                        else
                        {
                            Console.WriteLine("Port " + _serialPort.PortName + " does not exist.");
                        }
                    }

                    if (_serialPort.IsOpen)
                    {
                        int num = _serialPort.BytesToRead;
                        byte[] buff = new byte[num];
                        _serialPort.Read(buff, 0, num);

                        for (int i = 0; i < num; i++)
                        {

                            if (buff[i] == (byte)'#')
                            {
                                data_addr = 0;
                                data_in[data_addr] = buff[i];
                            }
                            else if (buff[i] == (byte)'$')
                            {
                                data_in[data_addr] = buff[i];
                                data_in[data_addr + 1] = 0;
                                byte[] buffCmd = new byte[data_addr + 1];
                                Array.Copy(data_in, 0, buffCmd, 0, buffCmd.Length);
                                DecodeString(System.Text.Encoding.ASCII.GetString(buffCmd, 0, buffCmd.Length - 1));
                            }
                            else
                            {
                                data_in[data_addr] = buff[i];
                            }

                            data_addr = (data_addr + 1) % 400;
                        }


                    }
                    else
                    {

                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show(ex.Message);

            }
        }
        public static string DecodeString(string command)
        {
            //    log.WriterLog("R->" + command);
            // #|U|boardID|address|parm|$
            Console.WriteLine(command);
            string[] strtext;
            string rfid = "";
            char[] delim = { '|' };

            strtext = command.Split(delim);

            if (strtext.Length > 2)
            {
                data = strtext[2];
            }

            return data;
        }
        public virtual void Button_event(string Row, string Addr, string QTY)
        {
            // Override in derived class if needed
        }

        public virtual void RFID_event(string RFID_Code)
        {
            // Override in derived class if needed
        }



    }
}
