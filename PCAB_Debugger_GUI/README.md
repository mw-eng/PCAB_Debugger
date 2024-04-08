# PCAB Debugger GUI
Control GUI program.

## How to use

### Main window
<img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/assets/UI1.png?raw=true" width="600px"><br>
<br>
${\color{Aqua} 1. \space Settings \space before \space connection}$<br>
1.1.  Serial Port            : Select the *Serial Port* coneected to PCAB.<br>
1.2.  Monitor loop inberval  : Specifies the acquisition interval for monitors (temperature, etc.) in milliseconds.<br>
1.3.  Serial Numbers         : Enter the serial number of the connect target (separate with A when connecting multiple units).<br>

${\color{Magenta} 2. \space Control \space and \space Receive}$<br>
2.1.  Serial Number          : Select serial numbers to send and receive.<br>
2.2.  CPU TMP                : CPU internal temperature value [degreeC].<br>
2.3.  SNS Vin                : Vin voltage value [V].<br>
2.4.  SNS Id                 : Id current value [A].<br>
2.5.  SNS Vd                 : Vd voltage value [V].<br>
2.6.  Tempurature            : 1-wire sensor temperature value [degreeC].<br>

${\color{Gold} 3. \space Control \space TAB}$<br>
3.1.  Standby AMP            : If checked, it will go into standby mode.<br>
3.2.  Standby DRA            : If checked, it will go into standby mode.<br>
3.3.  Low Power Mode         : If checked, it will go into low power mode.<br>
3.4.  Phase Config           : Setting the phase.<br>
3.5.  Standby LNA            : If checked, it will go into standby mode.<br>
3.6.  Save and Load Target   : Memory number to save and load.<br>
3.7.  Load Memory            : Load state to memory(ROM).<br>
3.8.  Save Memory            : Save state to memory(ROM).<br>
3.9.  Preset Config          : Preset config.<br>
3.11. Set Phase Config       : Write phase config.<br>


<br><img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_GUI/assets/UI2.png?raw=true" width="600px"><br>
<br>
${\color{LightSkyBlue} 4. \space VNA \space automatic \space measurement \space control}$<br>
4.1.  VISA Address	         : Instrument visa address.<br>
4.2.  Check Button	         : Instrument communication check.<br>
4.3.  Channel		         : Config collection channels.<br>
4.4.  Mode Settings	         : Automatic trigger settings.<br>
4.5.  Save target	         : What to save.<br>
4.6.  PS{xx}		         : Check loop target.<br>
4.7.  File Name Header       : Save file name header.<br>
4.8.  RUN Button             : Automatic measurement execution button.<br>
