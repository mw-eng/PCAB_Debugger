# PCAB Debugger FW
The main unit firmware. @ RP2040

## Flash ROM area
Firmware is stored at the beginning of the flash ROM. The area used varies depending on the firmware capacity.<br>
The last block is store STATE information and factory settings, the last 16 bytes store the 15-character serial number in ASCII code, and the last byte stores the character count.<br>

## RS485 Serial Communication Protocol
### ASCII Communication Mode
Send commands in the order of *#{SERIAL NUMBER}*, *{COMMAND}*, *{ARGUMENTS}*, and *{EXIT CODE}*, separated by space.<br>
or<br>
Send commands in the order of *${ROM ID}*, *{COMMAND}*, *{ARGUMENTS}*, and *{EXIT CODE}*, separated by space.<br>
### Binary Communication Mode
*Support with v1.3.3 or later*<br>
Send commands in the order of *#{SERIAL NUMBER}*, *{COMMAND}*, *{ARGUMENT}*, and *{EXIT CODE}*, separated by *0xFF*.<br>
or<br>
Send commands in the order of *${ROM ID}*, *{COMMAND}*, *{ARGUMENT}*, and *{EXIT CODE}*, separated by *0xFF*.<br>

<details>
<summary>Description</summary>

Specify the serial number of the communication partner in *{SERIAL NUMBER}*. However, if *"\*"* is specified, communication will be performed for all serial numbers.<br>
*{COMMAND}* and *{ARGUMENTS}* refer to *Command Lists*.<br>
*{EXIT CODE}* is *\n(Line Feed Code)* or *\r(Carriage Return Code)* or *\r\n*.<br>
However, in SLIP mode (Binary Communication Mode) it can only *0x0D*.
We recommend *\r(CR)* when CUI and echo are enable, and *\n(LF)* when CUI and local echo are enabled or GUI is enabled.<br>
### example
- #0010 WrtDPS
- #0001 SetSTB.AMP true
- #* GetIDN

</details>

## Command Lists
RS485 serial communication command list  

<details>
<summary>Digital Phase Shifter / Digital Phase Attenuator</summary>

Command | Description
:--|:--
WrtDPS | Write binary data to the digital phase sifter.
GetDPS {0/1/false/true/bf/now} {x} | Get digital phase sifter settings.<br>{1/true/now} : Get the currently written binary data.<br>{0/false/bf} : Get the buffer binary data.(Get the binary data written with the WrtDPS command.)<br>{x} : Phase Shifter No. ( {0} is gets all data.)
SetDPS {x} {DEC} | Set binary data in the buffer.<br>{x} : Phase Shifter No.<br>{DEC} : Decimal binary value.
WrtDSA | *Support with v1.2.0 or later*<br>Write binary data to the digital step attenuator.
GetDSA {0/1/false/true/bf/now} {x} | *Support with v1.2.0 or later*<br>Get digital step attenuator settings.<br>{1/true/now} : Get the currently written binary data.<br>{0/false/bf} : Get the buffer binary data.(Get the binary data written with the WrtDSA command.)<br>{x} : Digital Step attenuator No. ( {0} is gets all data. / {16} is input attenuator No.)
SetDSA {x} {DEC} | *Support with v1.2.0 or later*<br>Set binary data in the buffer.<br>{x} : Digital Step attenuator No. ({16} is input attenuator No.)<br>{DEC} : Decimal binary value.
</details>
<details>
<summary>Get and Set the MODE</summary>

Command | Description
:--|:--
GetSTB.AMP | Get AMP STBY.
SetSTB.AMP {0/1/false/true}| Set AMP STBY<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetSTB.DRA | Get DRA STBY.
SetSTB.DRA {0/1/false/true}| Set DRA STBY<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetSTB.LNA | Get LNA STBY.
SetSTB.LNA {0/1/false/true}| Set LNA STBY<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetLPM | Get low power mode.
SetLPM {0/1/false/true} | Set low power mode<br>{1/true} : Low Power MODE<br>{0/false} : Full Power MODE

</details>
<details>
<summary>Get of each sensor information</summary>

Command | Description
:--|:--
GetTMP.ID {x} | Get Temperature sensor ID.<br>{x} : Temp IC No.<br>{0} gets all temperature data.
GetTMP.Val {x} | Get Temperature.<br>{x} : Temp IC No.<br>{0} gets all temperature data.
GetTMP.CPU | Get CPU Temperature.
GetVd | Get Vd Value.
GetId | Get Id Value.
GetVin | *Support with v1.2.0 or later*<br>Get Vin Value.
GetPin | *Support with v1.2.0 or later*<br>Get Pin Value.

</details>
<details>
<summary>Other commands</summary>

Command | Description
:--|:--
SMEM ({x}) ({y-z}\|{z}) | Save state to memory(ROM).<br>However, whether or not it can be saved depends on the boot mode.<br>To save the default setting, set {z} to 0 or unspecified. ({z} can be specified as 0 to 3.)<br>If {y-z} is specified, it will be written to the specified setting number. ({y} can be specified as 0 to 15.)<br>A sector number can be specified for {x}. The sector numbers available to the user are 0 to 13.<br>*14 is the default setting area when no sector number is specified, and 15 is the data storage area at factory shipment.*<br>*By specifying the sector number, you can save 15×16×4 (=960) settings.*<br>*The Auto Load Boot uses the settings stored in unspecified {z} (sector number 14, setting numbers 0 to 0).*
LMEM ({x}) ({y-z}\|{z}) | Load state from memory(ROM).<br>Arguments are the same as SMEM.
GetMODE | Get boot mode.
GetIDN | Get device identification character.
*IDN? | Same as GetIDN.
GetIDR | Get ROM identification character.
ECHO {0/1/false/true} | Set echo mode.<br>*Do not enable it if you are connected to multiple devices.*<br>{1/true} : With echo.<br>{0/false} : Without echo.
CUI {0/1/false/true} | CUI Control Use<br>{1/true} : CUI MODE<br>{0/false} : GUI MODE<br>Default is CUI MODE.
RST | Restore factory default settings.<br>*PS all 0<br>DSA all 2dB(No,0 = 0dB)<br>STB all 0(RUN MODE)<br>LPM 0(Full Power MODE)*
*RST | Same as RST.
Reboot | Reload setup function.
BCM | Switch to binary communication mode.

</details>
<details>
<summary>Maintenance command</summary>

Command | Description
:--|:--
SetSN {x} | *Can only be changed in maintenance mode.*<br>Set Bord SN.<br>{x} : Serial Number strings.
RROM {x-yz} | Read page data from ROM.<br>{x-yz} : Specify the *block number(x), *sector number(y) + page number(z)* in hexadecimal format, separated by "-".
WROM {x-yz} {HEX} | Write page data to ROM.<br>{x-yz} : Specify the *block number(x), *sector number(y) + page number(z)* in hexadecimal format, separated by "-".<br>{HEX} : HEX data to write.<br>*Data will not be erased.*
EROM {x-y} | Erase page data from ROM.<br>{x} : Specify the *block number(x)* and *sector number(y)* as hexadecimal format separated by "-".
OROM {x-yz} {HEX} | Overwrite sector data to ROM.<br>{x-yz} : Specify the *block number(x)* and *sector number(y) + page number(z)* as hexadecimal format separated by "-".<br>{HEX} : HEX data to write.<br>*Data is written after erasing.*

</details>

<details>
<summary>Binary Communication Mode command</summary>
*Support with v1.3.3 or later*

Command Code | Description
:--|:--
0x0D | Frame end code.
0xFE | Switch to ASCII communication mode.
0xFF | Command separator code.
0xC0 0xFF {Byte} | Write Byte data to the input attenuator.
0xC1 0xFF {Binary} | Write binary data to the digital step attenuator.<br>Binary data is the same as DPS.
0xC2 0xFF {Binary} | Write binary data to the digital phase sifter.<br>The binary data must be specified in the order of DPS numbers 1 to 15, and each DPS setting must be specified in 8 bits ( i.e. 15 bytes of data ).
0xC3 0xFF {0x00/0x01} | Set AMP STBY.<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC4 0xFF {0x00/0x01} | Set DRA STBY.<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC5 0xFF {0x00/0x01} | Set LNA STBY.<br>{0x00} : Run MODE<br>{0x01} : Standby MODE
0xC6 0xFF {0x00/0x01} | Set low power mode.<br>{0x00} : Full Power MODE<br>{0x01} : Low Power MODE
0xD0 | Get input attenuator settings.<br>The response data is in the same binary format as it was written.
0xD1 | Get digital step attenuator settings.<br>The response data is in the same binary format as it was written.
0xD2 | Get digital phase sifter settins.<br>The response data is in the same binary format as it was written.
0xD3 | Get AMP STBY.<br>The response data is in the same binary format as it was written.
0xD4 | Get DRA STBY.<br>The response data is in the same binary format as it was written.
0xD5 | Get LNA STBY.<br>The response data is in the same binary format as it was written.
0xD6 | Get low power mode.<br>The response data is in the same binary format as it was written.
0xE1 | Get all temperature sensor IDs.<br> 8byte * 15
0xE2 | Get all temperature data.<br>The response data is 2 bytes of raw data.
0xE3 | Get CPU Temperature.<br>The response data is 2 bytes of raw data.
0xE4 | Get Vd Value.<br>The response data is 2 bytes of raw data.
0xE5 | Get Id Value.<br>The response data is 2 bytes of raw data.
0xF6 | Get Vin Value.<br>The response data is 2 bytes of raw data.
0xF7 | Get Pin Value.<br>The response data is 2 bytes of raw data.
0xFA | Restore factory default settings.<br>PS all 0<br>DSA all 2dB(No,0 = 0dB)<br>STB all 0(RUN MODE)<br>LPM 0(Full Power MODE)
0xFB 0xFF {0x00/0x01/0x02/0x03}| Save state to memory(ROM).<br>However, whether or not it can be saved depends on the boot mode.<br>To save the default setting, specify 0x00.
0xFC 0xFF {0x00/0x01/0x02/0x03}| Load state from memory(ROM).<br>Argument are the same as 0xFA.

Return Code | Description
:--|:--
0x0D | Frame end code.
0xFF | Command separator code.
0x00 | Successfull code.
0x00 0xFF {binary} | Successfull code and binary data.
0xF1 | Command not found error code.
0xF2 | Data length error code.
0xFE | Other errors code.


</details>

## Hardware Switch Configuration
List of settings by onboard hardware switch (SW1) status.<br>
*In v1.3.0 and later, sw1 and sw2 are assigned to output pins and must be fixed in the OFF state.*

<img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_RP2040/assets/SW1.png?raw=true" width="100px"> 0 = OFF(H) / 1 = ON(L)  
  
<details open>
<summary>Switch status 0x00 to 0x0F</summary>

Number | SW6 | SW5 | SW4 | SW3 | HEX | Stateus | Description
:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--
0 | 0 | 0 | 0 | 0 | 0x00 | Default | Default Status Boot.<br>*DPS(@All) = 0deg*<br>*DSA(@Input) = 0dB*<br>*DSA(@All except input) = 2dB*<br>*ALL Active Mode*
1 | 0 | 0 | 0 | 1 | 0x01 | Auto Load Boot. | Load the state (0) saved in ROM and boot.<br>*Picture Stateus*
2 | 0 | 0 | 1 | 0 | 0x02 | Allow settings to be saved. | Settins can be write in ROM.
3 | 0 | 0 | 1 | 1 | 0x03 | Auto Load Boot.<br>and<br>Allow settings to be saved. | Allows to start autoload and save settings.<br>*Basic usage conditions *
4 | 0 | 1 | 0 | 0 | 0x04 | State4 | Unused.
5 | 0 | 1 | 0 | 1 | 0x05 | State5 | Unused.
6 | 0 | 1 | 1 | 0 | 0x06 | State6 | Unused.
7 | 0 | 1 | 1 | 1 | 0x07 | State7 | Unused.
8 | 1 | 0 | 0 | 0 | 0x08 | State8 | Unused.
9 | 1 | 0 | 0 | 1 | 0x09 | State9 | Unused.
10 | 1 | 0 | 1 | 0 | 0x0A | State10 | Factory reset on boot. | If the switch is in this state at startup, the system boots to factory defaults and restore the autoload settings to their initial state.<br>*Settings outside the default settings area will not be changed.*
11 | 1 | 0 | 1 | 1 | 0x0B | State11 | Unused.
12 | 1 | 1 | 0 | 0 | 0x0C | State12 | Unused.
13 | 1 | 1 | 0 | 1 | 0x0D | State13 | Unused.
14 | 1 | 1 | 1 | 0 | 0x0E | State14 | Unused.
15 | 1 | 1 | 1 | 1 | 0x0F | State15 | Boot in the maintenance mode. | If the switch is in this state at startup, it boots in the administrator mode.<br>As general rule, do not use it as it may overwrite the ROM area where factory settings and serial numbers are stored.

</details>