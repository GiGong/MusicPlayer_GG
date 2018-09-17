# MusicPlayer_GG
**Simple Music Player** for Studying.

### Need Only **Single Executable.**

MusicPlayer_GG.exe (File Size : 1.58MB) in v0.2.1  
Program Version : v0.2.1 on 09.16 2018  
Download Link : <a href="https://drive.google.com/open?id=1QA2t2ONHaDnhSPpr-qJ7PyA3TDH5REG0" target="_blank">Google Drive</a>
<br/><br/>

When you run the program and exit, two file is created.  
One is setting file (setting.gg), the other is default Playlist file (lately.gpl).
<br/><br/>

※ If you want to **Uninstall** program, only need to **Delete Executable**.  
If not necessary, delete setting file, playlist files.
<br/><br/>

Blog Posting is <a href="http://gigong.cf/81" target="_blank">Here</a>

*Support Korean*


# Functions

* Play sound media in Windows OS (ex. *.mp3, *.flac, ...)
* Manage Playlist (Insert or Delete File, Change Order, Save, Load)
* Repeat Play One File
* Shuffle Playlist
* Read Id3v2 Tag (Incomplete)


# Requirements
Windows 7 or Later OS with .NET Framework 4.6 or Later


# Development Information
Tool : Microsoft Visual Studio Community 2017

Framework : Windows Presentation Foundation(WPF)

Target Framework Version : .NET Framework 4.6


# Contact
Blog : <a href="http://gigong.cf" target="_blank">GiGong Blog</a> (Powered by Daum)

Email : <gigong222@gmail.com>


# Changes

 v0.1.1  
-2018.01.04  
Add program information window.  

 v0.1.2  
-2018.01.09  
Fix error when playing file.  
(ex. When file name of playlist is changed.)  

 v0.1.3  
-2018.03.06  
Can change order of playlist with drag and drop.  
Reduce size of executable.  

 v0.2  
-2018.05.15  
Change design of Music Player.  
Mute function added. Mute status is maintained when program exit and re execute.  
Can add file from file explorer to playlist with drag and drop.  
Play file at last play position, when program executed.  
Docking function added. Docking is function that attaches to edge when move program to edge of screen.  
Execute at position where program was last terminated, when program is executed.  
Reduce size of executable by replacing button representation.  
During shuffle play, current file will not be played next.  
Fix other minor errors.  

 v0.2.1  
-2018.09.16  
※ Significantly reduce size of executable by change fontfamily! (nanumgothic -> nanumgothic light) (8.35MB -> 1.58MB)   
Change program icon.  
Change design of top menu, context menu.  
Fix error when playlist failure.  
Fix problem when top menu click.  

