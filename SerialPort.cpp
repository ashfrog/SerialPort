#include <iostream>
#include <windows.h>
#include <process.h>  
using namespace std;
HANDLE hMutex;
//////////////////////////////����ӿ�////////////////////////////////
extern "C" __declspec(dllexport) int __stdcall OpenPort(int (*qq)(char  *data,int num),char * scome,int baudrate);
extern "C" __declspec(dllexport) int __stdcall SendData(char  *data,DWORD num);
extern "C" __declspec(dllexport) int __stdcall ClosePort();

using namespace std;
typedef  int  (__stdcall *fpCALLBACK)(char  *data  ,int  len);
static fpCALLBACK fpDataReceive = NULL;
//static int  (__stdcall *fpDataReceive)(char  *data  ,int  len);
HANDLE hCom;
char sCome[255];
int baudRate =0;
bool isRuning;
//////////////////////////////////////////////////////////////////////////
//unsigned int __stdcall Read(PVOID pM)
DWORD WINAPI Read(LPVOID lpParamter)
{
	//�򿪶˿�
	hCom = CreateFileA(sCome, GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, 0);

	//���ó�ʱ
	COMMTIMEOUTS timeouts;
	GetCommTimeouts(hCom, &timeouts);
	timeouts.ReadIntervalTimeout = MAXDWORD;
	timeouts.ReadTotalTimeoutMultiplier = 0;
	timeouts.ReadTotalTimeoutConstant = 0;
	timeouts.WriteTotalTimeoutMultiplier = 50;
	timeouts.WriteTotalTimeoutConstant = 2000;

	if (!SetCommTimeouts(hCom, &timeouts))
	{
		cout<<"��ʱ����ʧ�ܣ�";
		CloseHandle(hCom);
		return 0;
	}

	//ȡ�ò����ö˿�״̬
	DCB dcb;
	GetCommState(hCom, &dcb);
	dcb.DCBlength = sizeof(DCB);
	dcb.BaudRate = baudRate;
	dcb.Parity = 0;  
	dcb.ByteSize = 8;
	dcb.StopBits = ONESTOPBIT;   
	if (!SetCommState(hCom, &dcb))
	{
		cout<<"�˿�״̬����ʧ�ܣ�";
		CloseHandle(hCom);
		return 0;
	}

	char buffer[25];
	DWORD readsize,dwError; 
	BOOL bReadStatus;
	COMSTAT cs;

	WaitForSingleObject(hMutex, INFINITE);
	while(isRuning)
	{   
		readsize = 0;
		ClearCommError(hCom,&dwError,&cs);//ȡ��״̬
		//�����Ƿ������׼��������
		if(cs.cbInQue>sizeof(buffer))
		{
			PurgeComm(hCom,PURGE_RXCLEAR);//���ͨ�Ŷ˿�����
			continue ;
		}
		memset(buffer, 0 , cs.cbInQue);
		bReadStatus = ReadFile(hCom, buffer, cs.cbInQue, &readsize, 0);
		if(readsize>0)
		{
			//cout<<cs.cbInQue;
			buffer[cs.cbInQue]='\0';
			//std::cout<<buffer;
			if (fpDataReceive)
			{
				fpDataReceive(buffer,cs.cbInQue);
			}
		}
		Sleep(10);
	}
	ReleaseMutex(hMutex);
	CloseHandle(hCom);
	return 1;
}
HANDLE hThread;
///����ӿ�
__declspec(dllexport) int __stdcall OpenPort(int (__stdcall *qq)(char  *data,int num),char * scome,int baudrate)
{ 
	memset(sCome,0,sizeof(scome)/sizeof(scome[0]));
	for (int i=0;i<sizeof(scome) / sizeof(scome[0])&&*scome!='\0';i++)
	{
		sCome[i]=*scome;
		scome++;
	}

	baudRate = baudrate;
	isRuning = true;

	HANDLE hThread = CreateThread(NULL, 0, Read, NULL, 0, NULL);
	hMutex = CreateMutex(NULL, FALSE, NULL);
	//CloseHandle(hThread);

	//hThread = (HANDLE)_beginthreadex(NULL, 0, Read, NULL, 0, NULL);

	fpDataReceive = qq;
	return 1;
}

__declspec(dllexport) int __stdcall SendData(char  *data,DWORD num)
{
	WriteFile(hCom, data, num, &num, 0);
	return 1;
}

__declspec(dllexport) int __stdcall ClosePort()
{
	if (isRuning)
	{
		isRuning = false;
		CloseHandle(hThread);
		return 1;
	}
	return 0;
}

