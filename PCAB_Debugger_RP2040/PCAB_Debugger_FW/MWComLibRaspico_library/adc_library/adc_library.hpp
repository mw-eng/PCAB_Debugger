#include "hardware/adc.h"

class adc
{
    private:
    bool adc0;
    bool adc1;
    bool adc2;
    float vref;

    public:
    adc(bool adc0, bool adc1, bool adc2, float vref);
    adc();
    uint16_t readADC0();
    uint16_t readADC1();
    uint16_t readADC2();
    uint16_t readADC3();
    uint16_t readADC4();
    float readVoltageADC0();
    float readVoltageADC1();
    float readVoltageADC2();
    float readVoltageADC3();
    float readVoltageADC4();
    float readVsys();
    float readTempCPU();

};