#pragma once
#include "stdafx.h"



class M_API MMonitor
{
public:
	MMonitor();
	~MMonitor();

	virtual bool Update();
	virtual bool Start();
	virtual bool Stop();

};

