PCMgr [![License](https://img.shields.io/badge/Licence-MIT-blue.svg)]()
=======================
 Windows 任务管理器重制版 Rebulid of Windows task manager
===
这是一个 Windows 任务管理器重制版，添加了任务管理器不具有的功能，并且与 Windows 任务管理器非常相似。<br/>
使用WinAPI以及C#开发，可以进行查看进程、进程信息、结束进程等。<br/>
目前只完成部分功能。<br/>

This is an rebulid version of the Windows Task Manager that adds features that the Task Manager does not have, And it is very similar to the Windows task manager. <br/>
Use WinAPI and C# development, you can view the process, process information, end the process, etc. <br/>
At present, only part of the functions are completed.

作者的说明
---
** 作者已经弃坑。<br/>
这是本人在高二暑假心血来潮制作的一个小软件，纯粹是为了玩玩而已，那个时候才刚刚接触编程，编程水平不足，所以写的代码也很烂😓，大家不要学我。<br/>
现在想想这个项目已经没有太多的意义，毕竟微软在 Win10 的更新非常快，这个任务管理器功能已经比不上 Win10 的自带任务管理器，靠自己去猜测系统不公开的功能不仅费时费力，而且随时可能失效，所以我决定不再维护此项目，因此此项目已经非常落后了（不过在 Win7 上运行还勉勉强强），不建议学习。<br/>

Note
---
This is a small software that I made in senior high school. It's just for fun. At that time, I just came into contact with programming. The programming level was not enough, so the code I wrote was also very bad 😓

Now think about this project, the update of Microsoft in win10 is very fast, and the task manager function is not as good as win10's task manager. It's not only time-consuming and laborious to guess the system's undisclosed functions by myself, but also may fail at any time, so I decided not to maintain this project.

特点
---
- 完全的进程查看以及结束、暂停运行、继续运行进程
- 查看进程线程、模块、窗口、句柄
- 类似Win10任务管理器的性能图表
- 管理系统服务
- 与自带任务管理器非常相似
- 最低支持 Windows7
- 一些任务管理器没有的功能
- 内核还未完全开发完成，完成以后可以获取更多更强的权限
- 这是完全开源的软件，你可以学习其中的原理

[下载已编译程序(x86)](https://github.com/imengyu/PCMgr/raw/master/Release/Release_x86_1.3.2.6.zip) | 
[下载已编译程序(x64)](https://github.com/imengyu/PCMgr/raw/master/Release_64/Release_64_1.3.2.6.zip)

Features
---
- Process view and terminate, suspend, resuseme process.
- View threads, modules, handles, windows of process.
- Performance chart like windows task manager.
- Manage system services.
- Very similar to the windows task manager.
- Support Windows7.
- Kernel driver.
- Full open source.

[Download compiled program(x86)](https://github.com/imengyu/PCMgr/raw/master/Release/Release_x86_1.3.2.6.zip) | 
[Download compiled program(x64)](https://github.com/imengyu/PCMgr/raw/master/Release_64/Release_64_1.3.2.6.zip)

编译 Bulid
---
你至少需要使用 Visual Studio 2017 进行编译，需要C#和C++的生成工具。其中内核驱动最好使用WDK10<br/>
You need to use Visual Studio 2017 to compile at least, and must install C# and C++ in Visual studio. The kernel driver is best to use WDK10.

#### 步骤
* 使用 Visual Studio 2017+打开项目。
* 设置启动项目为 PCMgrLoader，配置设置为x86 或x64，Debug或Release模式。
* 在PCMgrLoader右键点击生成。
* x86下拷贝 `ThirdPart\AeroWizard\AeroWizard32.dll` 至 `Debug` 或 `Release` 目录，x64下拷贝 `ThirdPart\AeroWizard\AeroWizard64.dll 至` `Debug_64` 或 `Release_64` 目录。
* x86下拷贝 `ThirdPart\capstone\lib\x86\capstone.dll` 至 `Debug` 或 `Release` 目录，x64下拷贝 `ThirdPart\capstone\lib\x64\capstone.dll` 至 `Debug_64` 或 `Release_64` 目录。
* PCMgrKernel 是驱动项目，您可以不生成。如果要生成，需要安装WDK10并配置项目头文件路径为您的WDK路径。
* PCMgrNetMon 项目没有功能，可以不生成。
* PCMgrRegedit 项目没有功能，可以不生成。
* PCMgrUpdate 项目是更新器，作者服务器已经过期，更新功能没有用了，这个项目可以不生成。
* F5调试项目，或者运行生成好的 PCMgr32.exe。

#### Step
* Open project with Visual Studio 2017+.
* Set PCMgrLoader as a startup project, set solution platforms to x86 or x64, set solution configurations to Debug or Release.
* Right click PCMgrLoader in solution explrer then click Build to build project.
* If you set solution platforms to x86 then copy `ThirdPart\AeroWizard\AeroWizard32.dll` to `Debug` or `Release`, x64 copy `ThirdPart\AeroWizard\AeroWizard64.dll` to `Debug_64` or `Release_64`.
* If you set solution platforms to x86 then copy `ThirdPart\capstone\lib\x86\capstone.dll` to `Debug` or `Release`, x64 copy `ThirdPart\capstone\lib\x64\capstone.dll` to `Debug_64` or `Release_64`.
* PCMgrKernel is driver project, you don't need to build it. If you want to build, you need to install wdk10 and configure the project header file path to your WDK path.
* Project PCMgrNetMon has no function and can not be build.
* Project PCMgrRegedit has no function and can not be build.
* Project PCMgrUpdate is an updater. The author server has expired. The update function is useless. This project can not be build.
* Press F5 in visual studio to debug the program, or run the generated pcmgr32.exe.

需求 Requirement
---
至少需要Windows7，并且需要.Net Framework 4.0<br/>
At least in Windows7 and installed .Net Framework 4.0<br/>

截图 ScreenShot
---
![Image1](https://raw.githubusercontent.com/717021/PCMgr/master/image1.png)<br/>
![Image11](https://raw.githubusercontent.com/717021/PCMgr/master/image11.png)<br/>
![Image6](https://raw.githubusercontent.com/717021/PCMgr/master/image6.png)<br/>
![Image10](https://raw.githubusercontent.com/717021/PCMgr/master/image10.png)<br/>
![Image2](https://raw.githubusercontent.com/717021/PCMgr/master/image2.png)<br/>
![Image3](https://raw.githubusercontent.com/717021/PCMgr/master/image3.png)<br/>
![Image4](https://raw.githubusercontent.com/717021/PCMgr/master/image4.png)<br/>
![Image5](https://raw.githubusercontent.com/717021/PCMgr/master/image5.png)<br/>

![Image7](https://raw.githubusercontent.com/717021/PCMgr/master/image7.png)<br/>
![Image8](https://raw.githubusercontent.com/717021/PCMgr/master/image8.png)<br/>
![Image9](https://raw.githubusercontent.com/717021/PCMgr/master/image9.png)<br/>

许可 License
---
本程序遵循 MIT 协议
Theme PCMgr is covered with MIT license
