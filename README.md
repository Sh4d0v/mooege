###Current Status:
* You can Login, create characters and enter world.
* The Fallen Star quest working in 99%.
* Legacy of Cain quest working in 20%.
* A Shattered Crown quest working in 0%.
* Reign of the Black King quest working in 5%.
* Skills working in 55%.
* Drop is still bugged. Decrease of gold drop.
* Working with 1.0.3.10485 (soon newer with new Patcher).
* Shared Stash fixed. Now you can buy more rows and tabs.
* Working commands: !tp, !save, !changename, !changegender, !speed, !addgold
* Rates of server in Server.conf file.
* some small updates and fixes (Necrosummon)

***

# Stay awhile and listen

* Join us: https://discord.gg/ES7G8VV
* Check [RageZone Forums](http://ragezone.com/) for more informations.
* First server is http://d3reflection.com
* Community board soon (i hope)...

# Tutorial
* Install [.NET Framework 4](https://www.microsoft.com/en-in/download/details.aspx?id=17718) if not installed
* Install [Visual Studio 2017 Community Edition](https://www.visualstudio.com/en/downloads)
* Pick .Net Desktop Development for workload
* Install [GIT](https://git-scm.com/downloads) - next next next
* CMD
* Choose parent folder for sources
* git clone https://github.com/Sh4d0v/mooege.git
* Open Visual Studio CE, login, if you like, File -> open -> project/solution
* Open mooege/src/Mooege/Mooege-VS2010.sln
* Somehow this solution not full, right click on solution in Solution Explorer -> Add -> Existing Project -> pick \LibMooNet\LibMooNet.csproj
* In Solution Explorer unfold the References link of Mooege-VS2010 project
* Remove LibMooNet reference
* Right click on references -> Add Reference -> Pick Projects -> Pick LibMooNet checkbox -> OK
* Download [Client](https://yadi.sk/d/g_aoGkXE3PACvC) -> unrar
* Download [downgrade patch](https://yadi.sk/d/dfPqlIiM3PAD3z) -> unrar to client
* Download [executable fixer](https://yadi.sk/d/0bMUbmXy3PAD8f) -> unrar to client
* Copy MPQs from client\Data_D3\PC\MPQs\ - all of them to mooege/assets/MPQ
* Now, you can build solution CTRL+Shift+B, it will copy all of MPQs to debug folder
* It should build without errors
* Now start hit f5 - it will start DEBUG process and should run without errors -> you can use breakpoints!yay (2017 year)
* Start client
* Enter @test login, 123456 as password
* ...
* PROFIT!!!

# Welcome to mooege

mooege (multi-node object oriented educational game emulator) is an open source reference game-server implementation
developed with C#. It can be compiled with Microsoft .NET or Mono, which means you can run it on Windows, MacOS, 
and Linux. Please see the file LICENSE for license details.

**Copyright (C) 2011 - 2014 mooege project**

**Copyright (C) 2016 - 2017 D3Emu project by Necrosummon (updated mooege)**

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
