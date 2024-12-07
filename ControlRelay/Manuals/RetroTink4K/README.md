# RetronTink 4K Remote Control Emulation Over Serial

## Serial Port Parameters
* Baud Rate: **115200**
* Data Bits: **8**
* Parity: **None**
* Stop Bits: **1**

## String Termination
All commands should be terminated with ```\r``` or ```\n``` or a combination of both.

## Issuing Commands
Issue commands in the following way.
```
remote <command>
```

Possible commands are defined below.
### Command Table
| Command  | Behaviour      |
| -------- |----------------|
| pwr      | BUTTON PWR     |
| menu     | BUTTON MENU    |
| up       | BUTTON UP      |
| down     | BUTTON DOWN    |
| left     | BUTTON LEFT    |
| right    | BUTTON RIGHT   |
| ok       | BUTTON OK      |
| back     | BUTTON BACK    |
| diag     | BUTTON DIAG    |
| stat     | BUTTON STAT    |
| input    | BUTTON INPUT   |
| output   | BUTTON OUTPUT  |
| scaler   | BUTTON SCALER  |
| sfx      | BUTTON SFX     |
| adc      | BUTTON ADC     |
| prof     | BUTTON PROF    |
| prof1    | BUTTON 1       |
| prof2    | BUTTON 2       |
| prof3    | BUTTON 3       |
| prof4    | BUTTON 4       |
| prof5    | BUTTON 5       |
| prof6    | BUTTON 6       |
| prof7    | BUTTON 7       |
| prof8    | BUTTON 8       |
| prof9    | BUTTON 9       |
| prof10   | BUTTON 10      |
| prof11   | BUTTON 11      |
| prof12   | BUTTON 12      |
| gain     | BUTTON GAIN    |
| phase    | BUTTON PHASE   |
| pause    | BUTTON PAUSE   |
| safe     | BUTTON SAFE    |
| genlock  | BUTTON GENLOCK |
| buffer   | BUTTON BUFFER  |
| res4k    | BUTTON 4K      |
| res1080p | BUTTON 1080P   |
| res1440p | BUTTON 1440P   |
| res480p  | BUTTON 480P    |
| res1     | BUTTON RES1    |
| res2     | BUTTON RES2    |
| res3     | BUTTON RES3    |
| res4     | BUTTON RES4    |
| aux1     | BUTTON AUX1    |
| aux2     | BUTTON AUX2    |
| aux3     | BUTTON AUX3    |
| aux4     | BUTTON AUX4    |
| aux5     | BUTTON AUX5    |
| axu6     | BUTTON AXU6    |
| aux7     | BUTTON AUX7    |
| aux8     | BUTTON AUX8    |


## Power On Command While Sleeping
Issuing the following command turns the RT4K on, all other commands do nothing.
```
pwr on
```

## Support for [SVS Switch](https://arthrimus.com/support/scalable-video-switch/) Auto-Loading
* Similar function to DV1 (auto-loading enabled in the 'Profile' menu)
* When SVS signals new input, RT4K checks the `/profile/SVS` subfolder for a matching profile
* Profiles need to be named: ``S<input number>_<user defined>.rt4``
    * For example, ```SVS input 2``` would look for a profile that is named `S2_SNES.rt4`
    * If there's more than one profile that fits the pattern, the first match is used
* Other devices may also use this system by sending SVS commands through either serial port
* ```SVS NEW INPUT=<input number>``` triggers an auto profile load
* ```SVS CURRENT INPUT=<input number>``` is a keep alive signal that tells the RT4K a switch is connected. This should be sent ~1-2 seconds
