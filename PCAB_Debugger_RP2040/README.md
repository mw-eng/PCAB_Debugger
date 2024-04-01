# PCAB_Debugger
Raspberry Pi Pico


## Command Lists
RS485 serial communication command list  
  
Command | Description
:--|:--
WrtDPS | Write binary data to the digital phase sifter.
GetDPS {0/1/false/true/bf/now} {x} | Get digital phase sifter settings.<br>{1/true/now} : Get the currently written binary data.<br>{0/false/bf} : Get the buffer binary data.(Get the binary data written with the WrtDPS command.)<br>{x} : Phase Shifter No.
SetDPS {x} {DEC}| Set binary data in the buffer.<br>{x} : Phase Shifter No.<br>{DEC} : Decimal binary value.
WrtDSA | Write binary data to the digital step attenuator.<bf>*Support with v1.2.0 or later*
GetDSA {0/1/false/true/bf/now} {x} | Get digital step attenuator settings.<br>{1/true/now} : Get the currently written binary data.<br>{0/false/bf} : Get the buffer binary data.(Get the binary data written with the WrtDSA command.)<br>{x} : Digital Step attenuator No.<bf>*Support with v1.2.0 or later*
SetDSA {x} {DEC}| Set binary data in the buffer.<br>{x} : Digital Step attenuator No.<br>{DEC} : Decimal binary value.<bf>*Support with v1.2.0 or later*
GetTMP.ID {x} | Get Temperature sensor ID.<br>{x} : Temp IC No.<br>{0} gets all temperature data.
GetTMP.Val {x} | Get Temperature.<br>{x} : Temp IC No.<br>{0} gets all temperature data.
GetId | Get Id Value.
GetVd | Get Vd Value.
GetSTB.AMP | Get AMP STBY.
SetSTB.AMP {0/1/false/true}| Set AMP STBY<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetSTB.DRA | Get DRA STBY.
SetSTB.DRA {0/1/false/true}| Set DRA STBY<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetSTB.LNA | Get LNA STBY.
SetSTB.LNA {0/1/false/true}| Set LNA STBY<br>{1/true} : Standby MODE<br>{0/false} : Run MODE
GetLPM | Get low power mode.
SetLPM {0/1/false/true} | Get low power mode<br>{1/true} : Low Power MODE<br>{0/false} : Full Power MODE
GetALD | Get auto load date<br>1 : Auto LOAD MODE<br>0 : Non-Auto LOAD MODE<br>Non-Auto LOAD MODE if disabled by hardware.
SetALD {0/1/false/true} | Set auto load date<br>{1/true} : Auto LOAD MODE<br>{0/false} : Non-Auto LOAD MODE
SMEM | Save state to memory(ROM).
LMEM | Load state to memory(ROM).
RROM {x} | Read data block from ROM.<br>{x} : Decimal ROM block number.
WROM {x} {HEX} | Write data block to ROM.<br>{x} : Decimal ROM block number.<br>{HEX} : HEX data to write.
EROM {x} | Erase data block from ROM.<br>{x} : Decimal ROM block number.
GetSN | Get Board SN.
SetSN {x} | Set Bord SN.<br>{x}:DEC
RST | Preset Config.<br>PS all 0<br>STB all 0(RUN MODE)<br>LPM 0(Full Power MODE)<br>ALD 1(Auto LOAD MODE)
*RST | Same as RST.
ECHO {0/1/false/true} | Set echo mode.<br>{1/true} : With echo.<br>{0/false} : Without echo.
CUI | Get cui mode.
CUI {0/1/false/true} | CUI Control Use<br>{1/true} : CUI MODE<br>{0/false} : GUI MODE
GetIDN | Get device identification character.
*IDN? | Same as GetIDN.

## Hardware Switch Configuration

Number | SW6 | SW5 | SW4 | SW3 | SW2 | SW1 | HEX | Stateus | Description
:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--
0 | 0 | 0 | 0 | 0 | 0 | 0 | 0x00 | Default | Default Status
1 | 0 | 0 | 0 | 0 | 0 | 1 | 0x01 | State1 | Picture Stateus
2 | 0 | 0 | 0 | 0 | 1 | 0 | 0x02 | State1 | Picture Stateus
3 | 0 | 0 | 0 | 0 | 1 | 1 | 0x03 | State1 | Picture Stateus
4 | 0 | 0 | 0 | 1 | 0 | 0 | 0x04 | State1 | Picture Stateus
5 | 0 | 0 | 0 | 1 | 0 | 1 | 0x05 | State1 | Picture Stateus
6 | 0 | 0 | 0 | 1 | 1 | 0 | 0x06 | State1 | Picture Stateus
7 | 0 | 0 | 0 | 1 | 1 | 1 | 0x07 | State1 | Picture Stateus
8 | 0 | 0 | 1 | 0 | 0 | 0 | 0x08 | State1 | Picture Stateus
9 | 0 | 0 | 1 | 0 | 0 | 1 | 0x09 | State1 | Picture Stateus
10 | 0 | 0 | 1 | 0 | 1 | 0 | 0x1A | State1 | Picture Stateus
11 | 0 | 0 | 1 | 0 | 1 | 1 | 0x1B | State1 | Picture Stateus
12 | 0 | 0 | 1 | 1 | 0 | 0 | 0x1C | State1 | Picture Stateus
13 | 0 | 0 | 1 | 1 | 0 | 1 | 0x1D | State1 | Picture Stateus
14 | 0 | 0 | 1 | 1 | 1 | 0 | 0x1E | State1 | Picture Stateus
15 | 0 | 0 | 1 | 1 | 1 | 1 | 0x1F | State1 | Picture Stateus
16 | 0 | 1 | 0 | 0 | 0 | 0 | 0x10 | State1 | Picture Stateus
17 | 0 | 1 | 0 | 0 | 0 | 1 | 0x11 | State1 | Picture Stateus
18 | 0 | 1 | 0 | 0 | 1 | 0 | 0x12 | State1 | Picture Stateus
19 | 0 | 1 | 0 | 0 | 1 | 1 | 0x13 | State1 | Picture Stateus
20 | 0 | 1 | 0 | 1 | 0 | 0 | 0x14 | State1 | Picture Stateus
21 | 0 | 1 | 0 | 1 | 0 | 1 | 0x15 | State1 | Picture Stateus
22 | 0 | 1 | 0 | 1 | 1 | 0 | 0x16 | State1 | Picture Stateus
23 | 0 | 1 | 0 | 1 | 1 | 1 | 0x17 | State1 | Picture Stateus
24 | 0 | 1 | 1 | 0 | 0 | 0 | 0x18 | State1 | Picture Stateus
25 | 0 | 1 | 1 | 0 | 0 | 1 | 0x19 | State1 | Picture Stateus
26 | 0 | 1 | 1 | 0 | 1 | 0 | 0x1A | State1 | Picture Stateus
27 | 0 | 1 | 1 | 0 | 1 | 1 | 0x1B | State1 | Picture Stateus
28 | 0 | 1 | 1 | 1 | 0 | 0 | 0x1C | State1 | Picture Stateus
29 | 0 | 1 | 1 | 1 | 0 | 1 | 0x1D | State1 | Picture Stateus
30 | 0 | 1 | 1 | 1 | 1 | 0 | 0x1E | State1 | Picture Stateus
31 | 0 | 1 | 1 | 1 | 1 | 1 | 0x1F | State1 | Picture Stateus
32 | 1 | 0 | 0 | 0 | 0 | 0 | 0x20 | State1 | Picture Stateus
33 | 1 | 0 | 0 | 0 | 0 | 1 | 0x21 | State1 | Picture Stateus
34 | 1 | 0 | 0 | 0 | 1 | 0 | 0x22 | State1 | Picture Stateus
35 | 1 | 0 | 0 | 0 | 1 | 1 | 0x23 | State1 | Picture Stateus
36 | 1 | 0 | 0 | 1 | 0 | 0 | 0x24 | State1 | Picture Stateus
37 | 1 | 0 | 0 | 1 | 0 | 1 | 0x25 | State1 | Picture Stateus
38 | 1 | 0 | 0 | 1 | 1 | 0 | 0x26 | State1 | Picture Stateus
39 | 1 | 0 | 0 | 1 | 1 | 1 | 0x27 | State1 | Picture Stateus
40 | 1 | 0 | 1 | 0 | 0 | 0 | 0x28 | State1 | Picture Stateus
41 | 1 | 0 | 1 | 0 | 0 | 1 | 0x29 | State1 | Picture Stateus
42 | 1 | 0 | 1 | 0 | 1 | 0 | 0x2A | State1 | Picture Stateus
43 | 1 | 0 | 1 | 0 | 1 | 1 | 0x2B | State1 | Picture Stateus
44 | 1 | 0 | 1 | 1 | 0 | 0 | 0x2C | State1 | Picture Stateus
45 | 1 | 0 | 1 | 1 | 0 | 1 | 0x2D | State1 | Picture Stateus
46 | 1 | 0 | 1 | 1 | 1 | 0 | 0x2E | State1 | Picture Stateus
47 | 1 | 0 | 1 | 1 | 1 | 1 | 0x2F | State1 | Picture Stateus
48 | 1 | 1 | 0 | 0 | 0 | 0 | 0x30 | State1 | Picture Stateus
49 | 1 | 1 | 0 | 0 | 0 | 1 | 0x31 | State1 | Picture Stateus
50 | 1 | 1 | 0 | 0 | 1 | 0 | 0x32 | State1 | Picture Stateus
51 | 1 | 1 | 0 | 0 | 1 | 1 | 0x33 | State1 | Picture Stateus
52 | 1 | 1 | 0 | 1 | 0 | 0 | 0x34 | State1 | Picture Stateus
53 | 1 | 1 | 0 | 1 | 0 | 1 | 0x35 | State1 | Picture Stateus
54 | 1 | 1 | 0 | 1 | 1 | 0 | 0x36 | State1 | Picture Stateus
55 | 1 | 1 | 0 | 1 | 1 | 1 | 0x37 | State1 | Picture Stateus
56 | 1 | 1 | 1 | 0 | 0 | 0 | 0x38 | State1 | Picture Stateus
57 | 1 | 1 | 1 | 0 | 0 | 1 | 0x39 | State1 | Picture Stateus
58 | 1 | 1 | 1 | 0 | 1 | 0 | 0x3A | State1 | Picture Stateus
59 | 1 | 1 | 1 | 0 | 1 | 1 | 0x3B | State1 | Picture Stateus
60 | 1 | 1 | 1 | 1 | 0 | 0 | 0x3C | State1 | Picture Stateus
61 | 1 | 1 | 1 | 1 | 0 | 1 | 0x3D | State1 | Picture Stateus
62 | 1 | 1 | 1 | 1 | 1 | 0 | 0x3E | State1 | Picture Stateus
63 | 1 | 1 | 1 | 1 | 1 | 1 | 0x3F | State1 | Picture Stateus