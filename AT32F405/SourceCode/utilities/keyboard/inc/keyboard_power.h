#ifndef _KEYBOARD_POWER_H
#define _KEYBOARD_POWER_H

#ifdef __cplusplus
extern "C" {
#endif
#include "platform.h"


#if defined (__CC_ARM)
 #pragma anon_unions
#endif

void suspend_sysclk_switch(crm_sclk_type sclk_type);
void suspend_enter_lower_power(void);
void suspend_exit_lower_power(void);


#ifdef __cplusplus
}
#endif
  
#endif
