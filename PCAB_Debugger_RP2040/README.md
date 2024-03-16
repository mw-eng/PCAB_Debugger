# PCAB_Debugger
Raspberry Pi Pico


## Command Lists
Command | Description
:--|:--
WrtPS | Write phase shifter binary
GetPS{x} | {x} : Phase Shifter No<br>Get phase shifter settings buffer value<br>*This is not the current setting.*
SetPS{x} {DEC}| {x} : Phase Shifter No<br>{HEX} : state binray 
GetTMP{x} | {x} : Temp IC No<br>Specifying 0 for {0} retrieves all temperature data.
GetId | Get Id Value.<br>
GetVd | Get Vd Value.<br>
GetSTB.AMP | Get AMP STBY<br>
SetSTB.AMP {0/1}| Set AMP STBY<br>1 : Standby MODE<br>0 : Run MODE
GetSTB.DRA | Get DRA STBY<br>
SetSTB.DRA {0/1}| Set DRA STBY<br>1 : Standby MODE<br>0 : Run MODE
GetSTB.LNA | Get LNA STBY<br>
SetSTB.LNA {0/1}| Set LNA STBY<br>1 : Standby MODE<br>0 : Run MODE
GetLPM | Get low power mode<br>
SetLPM {0/1} | Get low power mode<br>1 : Low Power MODE<br>0 : Full Power MODE
SetALD {0/1} | Set auto load date<br>1 : Auto LOAD MODE<br>0 : Non-Auto LOAD MODE
GetALD | Get auto load date<br>1 : Auto LOAD MODE<br>0 : Non-Auto LOAD MODE<br>Non-Auto LOAD MODE if disabled by hardware.
SMEM | Save Memory
LMEM | ReLoad Memory
CUI {0/1} | CUI Control Use<br>0 : CUI MODE<br>1 : GUI MODE
RST | Preset Config.<br>PS all 0<br>STB all 0(RUN MODE)<br>LPM 0(Full Power MODE)<br>ALD 1(Auto LOAD MODE)
