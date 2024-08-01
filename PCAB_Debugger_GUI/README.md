# PCAB Debugger GUI
Control GUI program.

## Requirements / Supported Platforms
* .NET Framework 4.7.2 or later.
* VNA A.17.20.09 (64 bit)
* Keygisht IO Libraries Suite 18.2.27313.1 or later. @[Keysight](https://www.keysight.com/zz/en/lib/software-detail/computer-software/io-libraries-suite-downloads-2175637.html)
* NI-VISA 18.5 @[National Instruments](https://www.ni.com/ja/support/downloads/drivers/download.ni-visa.html#306043)

## How to use

### Main window
<img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/assets/UI1.png?raw=true" width="600px"><br>
<br>
${\color{Aqua} 1. \space Settings \space before \space connection}$<br>
|No|Item|Description|
|:--|:--|:--|
|1-1|Serial Port			|Select the *Serial Port* coneected to PCAB.
|1-2|Monitor loop inberval	|Specifies the acquisition interval for monitors (temperature, etc.) in milliseconds.|
|1-3|Serial Numbers			|Enter the serial number of the connect target (separate with "," when connecting multiple units).|

${\color{Magenta} 2. \space Control \space and \space Receive}$<br>
|No|Item|Description|
|:--|:--|:--|
|2-1|Serial Number|Select serial numbers to send and receive.|
|2-2|CPU TMP      |CPU internal temperature value [degreeC].|
|2-3|SNS Vin      |Vin voltage value [V].|
|2-4|SNS Pin      |PreAMP Detector voltage value [V].|
|2-5|SNS Id       |Id current value [A].|
|2-6|SNS Vd       |Vd voltage value [V].|
|2-7|Tempurature  |1-wire sensor temperature value [degreeC].|

${\color{Gold} 3. \space CONTROL \space TAB}$<br>
|No|Item|Description|
|:--|:--|:--|
|3-1|Standby AMP			|If checked, it will go into standby mode.|
|3-2|Standby DRA			|If checked, it will go into standby mode.|
|3-3|Low Power Mode			|If checked, it will go into low power mode.|
|3-4|Phase and ATT Config	|Setting the phase delay and attenation.|
|3-5|Standby LNA			|If checked, it will go into standby mode.|
|3-6|Set ATT Config			|Write attenation config.|
|3-7|Set Phase Config		|Write phase delay config.|
|3-8|Set Config				|Write phase delay and attnation config.|
|3-9|Save and Load Target	|Memory number to save and load.|
|3-10|Load Memory			|Load state to memory(ROM).|
|3-11|Save Memory			|Save state to memory(ROM).|
|3-12|Preset Config			|Preset config.|


<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/assets/UI2.png?raw=true" width="600px"><br>
<br>
${\color{LightSkyBlue} 4. \space AUTO \space TAB \space}$ Automatic measurement control<br>
|No|Item|Description|
|:--|:--|:--|
|4-1|VISA Address		|Instrument visa address.|
|4-2|Check Button		|Instrument communication check.|
|4-3|Channel			|Config collection channels.|
|4-4|Mode Settings		|Automatic trigger settings.|
|4-5|Save target		|Select save target.|
|4-7|File Name Header	|Save file name header.|
|4-8|DPS Loop 			|Enable the phase delay step loop.|
|4-9|DPS step			|Select the phase delay step interval.|
|4-0|DPSn				|Select loop target.|
|4-11|DSA Loop 			|Enable the attenation step loop.|
|4-12|DSA step			|Select the attenation step interval.|
|4-13|DSAn				|Select loop target.|
|4-14|Waite Time		|Waite time befor next step.|
|4-15|START Button		|Automatic loop control execution button.|