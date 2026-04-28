#ifndef _KEYBOARD_ADC_H
#define _KEYBOARD_ADC_H
#ifdef __cplusplus
extern "C" {
#endif
#include "platform.h"

void key_matrix_cs_init(void);
void keyboard_adc_init(void);
void keyboard_adc_calibration(void);

#ifdef __cplusplus
}
#endif

#endif
