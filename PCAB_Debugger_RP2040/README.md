# PCAB_Debugger
Raspberry Pi Pico


## Command Lists
Command | Description
:--|:--
WrtDPS | Write binary data to the digital phase sifter.
GetDSP {0/1/false/true/bf/now} {x} | Get digital phase sifter settings.<br>{1/true/now} : Get the currently written binary data.<br>{0/false/bf} : Get the buffer binary data.(Get the binary data written with the WrtDPS command.)<br>{x} : Phase Shifter No.
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
GetID | Get Board ID.
SetID {x} | Set Bord ID.<br>{x}:DEC
RST | Preset Config.<br>PS all 0<br>STB all 0(RUN MODE)<br>LPM 0(Full Power MODE)<br>ALD 1(Auto LOAD MODE)
ECHO {0/1/false/true} | Set echo mode.<br>{1/true} : With echo.<br>{0/false} : Without echo.
CUI | Get cui mode.
CUI {0/1/false/true} | CUI Control Use<br>{1/true} : CUI MODE<br>{0/false} : GUI MODE
GetIDN | Get device identification character.
*IDN? | Same as GetIDN.
