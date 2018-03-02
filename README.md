AcerS3-391FanControl
===================

Allows you to set custom cooling points so your notebook fan is less annoying. 

Based on https://github.com/Gabriel-LG/Acer7551GFanControl but completely rebuilt. Improvements:
 * Drastically reduced CPU Usage. 
   * 300ms CPU time over 15 minutes vs 45 seconds CPU time
 * No longer requires config file, it's now built in. Config file can be created via menu, edited, then reloaded without exiting program. 
 * Added fan hysteresis options so that the fan doesn't constantly change levels. 
 * Added ability to customize hardware address's used so it can support additional notebooks if the fan controller address's are known. 
 * Added Ability to read fan speed when the BIOS is controlling the fan. 
 * Reduced Memory Allocations. 
   * Zero Garbage collections over 15 minutes vs 8 Garbage Collections
   * Uses 13MB RAM vs 16MB

Menu with 3 built in profiles:
![Menu](https://i.imgur.com/AEl8kDu.png)

Reload configuration without restarting the app.
![Reload configuration without restarting the app.](https://i.imgur.com/66YHOzb.png)

Detailed stats on mouse hover.
![Detailed stats on mouse hover.](https://i.imgur.com/WBQYpdh.png)
