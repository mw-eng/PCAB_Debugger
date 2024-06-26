# PCAB Debugger FW
The main unit firmware. @ RP2040

## Flash ROM area
Firmware is stored at the beginning of the flash ROM. The area used varies depending on the firmware capacity.<br>
The last block is store STATE information and factory settings, the last 16 bytes store the 15-character serial number in ASCII code, and the last byte stores the character count.<br>

## RS485 Serial Communication
Send commands in the order of *#{SERIAL NUMBER}*, *{COMMAND}*, *{ARGUMENTS}*, and *{EXIT CODE}*, separated by space.<br>
or<br>
Send commands in the order of *${ROM ID}*, *{COMMAND}*, *{ARGUMENTS}*, and *{EXIT CODE}*, separated by space.<br>

<details>
<summary>Description</summary>

Specify the serial number of the communication partner in *{SERIAL NUMBER}*. However, if *"\*"* is specified, communication will be performed for all serial numbers.<br>
*{COMMAND}* and *{ARGUMENTS}* refer to *Command Lists*.<br>
*{EXIT CODE}* is *\n(Line Feed Code)* or *\r(Carriage Return Code)* or *\r\n*.<br>
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
GetDSA {0/1/false/true/bf/now} {x} | *Support with v1.2.0 or later*<br>Get digital step attenuator settings.<br>{1/true/now} : Get the currently written binary data.<br>{0/false/bf} : Get the buffer binary data.(Get the binary data written with the WrtDSA command.)<br>{x} : Digital Step attenuator No. ( {0} is gets all data. / {16|IN} is input attenuator No.)
SetDSA {x} {DEC} | *Support with v1.2.0 or later*<br>Set binary data in the buffer.<br>{x} : Digital Step attenuator No. ({16|IN} is input attenuator No.)<br>{DEC} : Decimal binary value.
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
SetLPM {0/1/false/true} | Get low power mode<br>{1/true} : Low Power MODE<br>{0/false} : Full Power MODE

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

## Hardware Switch Configuration
List of settings by onboard hardware switch (SW1) status.  
  
<img src="https://github.com/mw-eng/PCAB_Debugger/blob/master/PCAB_Debugger_RP2040/assets/SW1.png?raw=true" width="100px"> 0 = OFF(H) / 1 = ON(L)  
  
<details open>
<summary>Switch status 0x00 to 0x0F</summary>

Number | SW6 | SW5 | SW4 | SW3 | SW2 | SW1 | HEX | Stateus | Description
:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--
0 | 0 | 0 | 0 | 0 | 0 | 0 | 0x00 | Default | Default Status Boot.<br>*DPS(@All) = 0deg*<br>*DSA(@Input) = 0dB*<br>*DSA(@All except input) = 2dB*<br>*ALL Active Mode*
1 | 0 | 0 | 0 | 0 | 0 | 1 | 0x01 | Auto Load Boot. | Load the state (0) saved in ROM and boot.<br>*Picture Stateus*
2 | 0 | 0 | 0 | 0 | 1 | 0 | 0x02 | Allow settings to be saved. | Settins can be write in ROM.
3 | 0 | 0 | 0 | 0 | 1 | 1 | 0x03 | Auto Load Boot.<br>and<br>Allow settings to be saved. | Allows to start autoload and save settings.<br>*Basic usage conditions *
4 | 0 | 0 | 0 | 1 | 0 | 0 | 0x04 | State4 | Unused.
5 | 0 | 0 | 0 | 1 | 0 | 1 | 0x05 | State5 | Unused.
6 | 0 | 0 | 0 | 1 | 1 | 0 | 0x06 | State6 | Unused.
7 | 0 | 0 | 0 | 1 | 1 | 1 | 0x07 | State7 | Unused.
8 | 0 | 0 | 1 | 0 | 0 | 0 | 0x08 | State8 | Unused.
9 | 0 | 0 | 1 | 0 | 0 | 1 | 0x09 | State9 | Unused.
10 | 0 | 0 | 1 | 0 | 1 | 0 | 0x0A | State10 | Unused.
11 | 0 | 0 | 1 | 0 | 1 | 1 | 0x0B | State11 | Unused.
12 | 0 | 0 | 1 | 1 | 0 | 0 | 0x0C | State12 | Unused.
13 | 0 | 0 | 1 | 1 | 0 | 1 | 0x0D | State13 | Unused.
14 | 0 | 0 | 1 | 1 | 1 | 0 | 0x0E | State14 | Unused.
15 | 0 | 0 | 1 | 1 | 1 | 1 | 0x0F | State15 | Unused.

</details>
<details>
<summary>Switch status 0x10 to 0x1F</summary>

Number | SW6 | SW5 | SW4 | SW3 | SW2 | SW1 | HEX | Stateus | Description
:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--
16 | 0 | 1 | 0 | 0 | 0 | 0 | 0x10 | State16 | Unused.
17 | 0 | 1 | 0 | 0 | 0 | 1 | 0x11 | State17 | Unused.
18 | 0 | 1 | 0 | 0 | 1 | 0 | 0x12 | State18 | Unused.
19 | 0 | 1 | 0 | 0 | 1 | 1 | 0x13 | State19 | Unused.
20 | 0 | 1 | 0 | 1 | 0 | 0 | 0x14 | State20 | Unused.
21 | 0 | 1 | 0 | 1 | 0 | 1 | 0x15 | State21 | Unused.
22 | 0 | 1 | 0 | 1 | 1 | 0 | 0x16 | State22 | Unused.
23 | 0 | 1 | 0 | 1 | 1 | 1 | 0x17 | State23 | Unused.
24 | 0 | 1 | 1 | 0 | 0 | 0 | 0x18 | State24 | Unused.
25 | 0 | 1 | 1 | 0 | 0 | 1 | 0x19 | State25 | Unused.
26 | 0 | 1 | 1 | 0 | 1 | 0 | 0x1A | State26 | Unused.
27 | 0 | 1 | 1 | 0 | 1 | 1 | 0x1B | State27 | Unused.
28 | 0 | 1 | 1 | 1 | 0 | 0 | 0x1C | State28 | Unused.
29 | 0 | 1 | 1 | 1 | 0 | 1 | 0x1D | State29 | Unused.
30 | 0 | 1 | 1 | 1 | 1 | 0 | 0x1E | State30 | Unused.
31 | 0 | 1 | 1 | 1 | 1 | 1 | 0x1F | State31 | Unused.

</details>
<details open>
<summary>Switch status 0x20 to 0x2F</summary>

Number | SW6 | SW5 | SW4 | SW3 | SW2 | SW1 | HEX | Stateus | Description
:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--
32 | 1 | 0 | 0 | 0 | 0 | 0 | 0x20 | Factory reset on boot. | If the switch is in this state at startup, the system boots to factory defaults and restore the autoload settings to their initial state.<br>*Settings outside the default settings area will not be changed.*
33 | 1 | 0 | 0 | 0 | 0 | 1 | 0x21 | State33 | Unused.
34 | 1 | 0 | 0 | 0 | 1 | 0 | 0x22 | State34 | Unused.
35 | 1 | 0 | 0 | 0 | 1 | 1 | 0x23 | State35 | Unused.
36 | 1 | 0 | 0 | 1 | 0 | 0 | 0x24 | State36 | Unused.
37 | 1 | 0 | 0 | 1 | 0 | 1 | 0x25 | State37 | Unused.
38 | 1 | 0 | 0 | 1 | 1 | 0 | 0x26 | State38 | Unused.
39 | 1 | 0 | 0 | 1 | 1 | 1 | 0x27 | State39 | Unused.
40 | 1 | 0 | 1 | 0 | 0 | 0 | 0x28 | State40 | Unused.
41 | 1 | 0 | 1 | 0 | 0 | 1 | 0x29 | State41 | Unused.
42 | 1 | 0 | 1 | 0 | 1 | 0 | 0x2A | Boot in the maintenance mode. | If the switch is in this state at startup, it boots in the administrator mode.<br>As general rule, do not use it as it may overwrite the ROM area where factory settings and serial numbers are stored.
43 | 1 | 0 | 1 | 0 | 1 | 1 | 0x2B | State43 | Unused.
44 | 1 | 0 | 1 | 1 | 0 | 0 | 0x2C | State44 | Unused.
45 | 1 | 0 | 1 | 1 | 0 | 1 | 0x2D | State45 | Unused.
46 | 1 | 0 | 1 | 1 | 1 | 0 | 0x2E | State46 | Unused.
47 | 1 | 0 | 1 | 1 | 1 | 1 | 0x2F | State47 | Unused.

</details>
<details>
<summary>Switch status 0x30 to 0x3F</summary>

Number | SW6 | SW5 | SW4 | SW3 | SW2 | SW1 | HEX | Stateus | Description
:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--
48 | 1 | 1 | 0 | 0 | 0 | 0 | 0x30 | State48 | Unused.
49 | 1 | 1 | 0 | 0 | 0 | 1 | 0x31 | State49 | Unused.
50 | 1 | 1 | 0 | 0 | 1 | 0 | 0x32 | State50 | Unused.
51 | 1 | 1 | 0 | 0 | 1 | 1 | 0x33 | State51 | Unused.
52 | 1 | 1 | 0 | 1 | 0 | 0 | 0x34 | State52 | Unused.
53 | 1 | 1 | 0 | 1 | 0 | 1 | 0x35 | State53 | Unused.
54 | 1 | 1 | 0 | 1 | 1 | 0 | 0x36 | State54 | Unused.
55 | 1 | 1 | 0 | 1 | 1 | 1 | 0x37 | State55 | Unused.
56 | 1 | 1 | 1 | 0 | 0 | 0 | 0x38 | State56 | Unused.
57 | 1 | 1 | 1 | 0 | 0 | 1 | 0x39 | State57 | Unused.
58 | 1 | 1 | 1 | 0 | 1 | 0 | 0x3A | State58 | Unused.
59 | 1 | 1 | 1 | 0 | 1 | 1 | 0x3B | State59 | Unused.
60 | 1 | 1 | 1 | 1 | 0 | 0 | 0x3C | State60 | Unused.
61 | 1 | 1 | 1 | 1 | 0 | 1 | 0x3D | State61 | Unused.
62 | 1 | 1 | 1 | 1 | 1 | 0 | 0x3E | State62 | Unused.
63 | 1 | 1 | 1 | 1 | 1 | 1 | 0x3F | State63 | Unused.

</details>