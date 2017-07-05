using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Text;
public class USBtoSerialPortDeal : MonoBehaviour
{
    /// <summary>
    /// 端口配置参数
    /// </summary>
    [SerializeField]
    private string portName = "COM4";
    [SerializeField]
    private int baudRate = 9600;
    [SerializeField]
    private Parity parity = Parity.None;
    [SerializeField]
    private int dataBits = 8;
    [SerializeField]
    private StopBits stopBits = StopBits.One;
    [SerializeField]
    private int delay = 20;//接收数据延时
    private bool brecv = false;
    private SerialPort sp = null;//端口
    //private List<byte> list = new List<byte>();//接收数据
    [SerializeField]
    private DataType datatype = DataType.Hex;

    //接收字符数据
    string svalue;

    //接收数据类型
    public enum DataType
    {
        ASKII = 0,
        Hex = 1,  
    }

    /// <summary>
    /// 打开端口
    /// </summary>
    public void OpenPort()
    {
        sp = new SerialPort("\\\\.\\" + portName, baudRate, parity, dataBits, stopBits);
        sp.ReadTimeout = 20;//端口读取速度
        try
        {
            //打开端口启动携程循环接收数据
            sp.Open();
            brecv = true;
            
            Thread recv_thread = new Thread(RecvData);
            recv_thread.IsBackground = true;
            recv_thread.Start();
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
    /// <summary>
    /// 关闭端口
    /// </summary>
    public void ClosePort()
    {
        try
        {
            brecv = false;
            sp.Close();
            Debug.Log("CloseSerialPort");
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void RecvData()
    {
        byte[] dataBytes = new byte[1];//存储数据
        //int bytesToRead = 0;//记录获取的数据长度
        while (brecv)
        {
            Thread.Sleep(delay);
            if (sp != null && sp.IsOpen)
            {
                try
                {
                    //bytesToRead = sp.Read(dataBytes, 0, dataBytes.Length);
                    sp.Read(dataBytes, 0, dataBytes.Length);
                    if (dataBytes.Length < 1) break;
                    sp.Write(dataBytes,0,dataBytes.Length);
                    switch(datatype)
                    {
                        case DataType.ASKII:
                            svalue = System.Text.Encoding.Default.GetString(dataBytes);
                            Debug.Log(svalue);
                            break;
                        case DataType.Hex:
                            svalue = dataBytes[0].ToString("X2");
                            int dvalue = Convert.ToInt16(svalue, 16); 
                            Debug.Log(dvalue);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //sp中没有数据读取
                    //Debug.Log(ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// 退出
    /// </summary>
    void OnApplicationQuit()
    {
        ClosePort();
    }

    // Use this for initialization
    void Start()
    {
        OpenPort();
    }

    ///////////////////////////////////////////测试平移 svalue处理后清空
    private void Update()
    {
        switch(svalue)
        {
            case "a": this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z - 1) ;svalue = ""; if(this.gameObject.transform.position.z<-50) this.gameObject.transform.position=new Vector3(0,0,0); break;
            case "b": this.gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + 1); svalue = ""; if (this.gameObject.transform.position.z>50) this.gameObject.transform.position = new Vector3(0, 0, 0); break;
            default:    break;
        }
    }
    /////////////////////////////////////////////
}