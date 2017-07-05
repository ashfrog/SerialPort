using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CSerial : MonoBehaviour {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int RecvData(IntPtr pData, int num);

    [DllImport("CSerialPort", EntryPoint = "Call", CharSet = CharSet.Unicode)]
    public extern static int Call(RecvData mm, [MarshalAs(UnmanagedType.LPStr)]string scom, int baudrate);

    [DllImport("CSerialPort", EntryPoint = "SendData", CharSet = CharSet.Unicode)]
    public extern static int SendData([MarshalAs(UnmanagedType.LPStr)]string data, int num);

    [DllImport("CSerialPort", EntryPoint = "ClosePort", CharSet = CharSet.Unicode)]
    public extern static int ClosePort(bool isruning);

    // Use this for initialization
    void Start () {
        try
        {
            RecvData myd = new RecvData(recvData);
            Call(myd, "com2", 9600);
            Console.ReadKey();
        }

        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    static int recvData(IntPtr pData, int num)
    {
        byte[] data = new byte[num];
        Marshal.Copy(pData, data, 0, num);
        Console.Write(System.Text.Encoding.Default.GetString(data));
        string sdata = System.Text.Encoding.Default.GetString(data);

        //测试发送数据
        SendData(sdata, sdata.Length);
        return num;
    }

    private void OnApplicationQuit()
    {
        ClosePort(false);
        Debug.Log("close");
    }
}
