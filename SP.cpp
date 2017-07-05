#include "stdio.h"  
#include <iostream>

extern "C" __declspec(dllexport) int __stdcall Call(int (*qq)(char  *data,int num),char * str);

using namespace std;
int  (*fpDataReceived)(char  *data  ,int  len); 
int test();
__declspec(dllexport) int __stdcall Call(int (*qq)(char  *data,int num),char * str)
{ 
	printf(str); 
	fpDataReceived = qq;
	test();
	return qq("a",123);
}

int test()
{
	for (int i=0;i<100;i++)
	{
		fpDataReceived("a",1);
	}
	return 1;
}