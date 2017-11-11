## Welcome to Mooege       
       \------.     _           __      ,-.___     _   _        ,---.  
        \ .--. \    \'-,      ,',.'.     \ ._ \    \'-'/       / .-. \ 
        | |  | |     | |     J /__\ L    | |_)(     | |       f /   \ l
        | |  | |     | |     | |  ] ]    | .-. \    | |   ,   t \   / j
        / '--' /    ,'_'.   .'_'. F F    / |__)/    / '--'/    \ '-' / 
       /_,----'      ' '     ' ' /,'    '-----'    '-----'      '---' 
      ('                        ''                    
> mooege (multi-node object oriented educational game emulator) is an open source reference game-server implementation developed with C#. It can be compiled with Microsoft .NET or Mono, which means you can run it on Windows, MacOS, and Linux. Please see the file LICENSE for license details.

Copyright (C) 2011 - 2018 mooege

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


![](mooege.png)

## Tutorial
* Install [.NET Framework 4](https://www.microsoft.com/en-in/download/details.aspx?id=17718) if not installed
* Install [Visual Studio 2017 Community Edition](https://www.visualstudio.com/en/downloads)
* Pick .Net Desktop Development for workload
* Install [GIT](https://git-scm.com/downloads)
* Run CMD
* Choose parent folder for sources
* git clone https://github.com/Sh4d0v/mooege.git
* Open Visual Studio CE, login, if you like, File -> open -> project/solution
* Open mooege/src/Mooege/Mooege-VS2010.sln
* Download [Client](https://yadi.sk/d/g_aoGkXE3PACvC) -> unrar
* Download [patch](https://yadi.sk/d/dfPqlIiM3PAD3z) -> unrar to client
* Download [executable fixer](https://yadi.sk/d/0bMUbmXy3PAD8f) -> unrar to client
* Copy MPQs from client\Data_D3\PC\MPQs\ - all of them to mooege/assets/MPQ
* Now, you can build solution CTRL+Shift+B, it will copy all of MPQs to debug folder
* Right click on Mooege-VS2010 project -> Build -> wait, it will copy all MPQs to build folder
* Now start hit f5 - it will start DEBUG process and should run without errors -> you can use breakpoints!yay (2017 year)
* Start client
* Enter "Test@" login, "123456" as password
* Create char, start the game
* enter !commands in chat for full commands list
* start develop!


## Stay awhile and listen
* Join us: https://discord.gg/ES7G8VV
* Check [RageZone Forums](http://ragezone.com/) for more informations.
* First server is http://d3reflection.com (completly other build of mooege. Its not the same source!)
* Also check our built-in wiki: https://github.com/Sh4d0v/mooege/wiki

## Branches:
* There is two main branches.
* master is for completly working source code.
* beta branch could be bugged or not completed. There is tests mostly.


## Contributing
1. Fork it (<https://github.com/Sh4d0v/mooege/fork>)
2. Create your feature branch (`git checkout -b feature/fooBar`)
3. Commit your changes (`git commit -am 'Add some fooBar'`)
4. Push to the branch (`git push origin feature/fooBar`)
5. Create a new Pull Request


