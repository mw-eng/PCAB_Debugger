# PCAB Debugger ACS
POS linked control GUI program.

## Requirements / Supported Platforms
* .NET Framework 4.7.2 or later.
* POS AV V6 @ PAST2 Binary

## How to use

### Main Window
<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_ACS/assets/UI1_CONFIG.png?raw=true" width="600px"><br>
<br>
|No|Item|Description|
|:--|:--|:--|
|1-1|Serial Port			|Select the *Serial Port* coneected to PCAB.|
|1-2|Baud Rate				|Select the *BAUD RATE* for the main communication to the PCAB.|
|1-3|PTU xx S/N				|Enter the serial number of the connect target.|
|1-4|View					| |
|1-5|Save Log				| |
|1-6|Monitor loop inberval	|Specifies the acquisition interval for monitors (temperature, etc.) in milliseconds.|
|1-7|Coordinate System		| |



<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_ACS/assets/UI2_BOARD.png?raw=true" width="600px"><br>
<br>
|No|Item|Description|
|:--|:--|:--|
|2-1|Standby AMP			|If checked, it will go into standby mode.|
|2-2|Standby DRA			|If checked, it will go into standby mode.|
|2-3|Low Power Mode			|If checked, it will go into low power mode.|
|2-4|Set ATT Config			|Write attenation config.|
|2-5|Set Phase Config		|Write phase delay config.|
|2-6|Set Config				|Write phase delay and attnation config.|
|2-7|Read Config			|Read the currently set state.|
|2-8|Save and Load Target	|Memory number to save and load. <br>0 specifies the autoload region {0xE0 - 0}.|
|2-9|Load Memory			|Load state to memory(ROM).|
|2-10|Save Memory			|Save state to memory(ROM).|
|2-11|Preset Config			|Preset config load.(Load factory defaults.)|
|2-12|Read Port List		| |
|2-13|Offset Reset			| |
|2-14|Frequency				| |
|2-15|Phi / Azimuth [deg]	| |
|2-16|Theta / Elevation [deg]| |
|2-17|Set Phase				| |


<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_ACS/assets/UI2_BOARD_PTU.png?raw=true" width="600px"><br>
<br>
Show by [PCAB_DEBUGGER_GUI README.md](https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/README.md)

<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_ACS/assets/UI3_TRACKING.png?raw=true" width="600px"><br>
<br>
|No|Item|Description|
|:--|:--|:--|
|3-1|Target Position		| |
|3-2|Set Target				| |
|3-3|Target Angle			| |
|3-4|Tracking Start			| |


### Monitor window

<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_ACS/assets/UI4_SENSOR.png?raw=true" width="600px"><br>
<br>

<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_ACS/assets/UI5_POS.png?raw=true" width="600px"><br>
<br>

<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_ACS/assets/UI6_PHASE.png?raw=true" width="600px"><br>
<br>

