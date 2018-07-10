#include <Windows.h>
#include "../TaskMgrCore/mapphlp.h"

int main();
int invoke_main()
{
	int rs = main();
	MAppMainExit(rs);
	return rs;
}
int main()
{
	if (MAppMainLoad()) {
		MAppMainRun();
		MAppMainFree();
	}	
	else {
		MAppMainFree();
		MAppMainSetExitCode(-1);
	}
	return MAppMainGetExitCode();
}