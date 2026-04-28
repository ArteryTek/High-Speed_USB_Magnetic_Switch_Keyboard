#ifndef _KEYBOARD_USB_H
#define _KEYBOARD_USB_H

#ifdef __cplusplus
extern "C" {
#endif
#include "keyboard_class.h"
#include "usb_core.h"

#define KEYBOARD_CMD             0x6B
#define GET_LED_MODE             0xB1
#define SET_LED_MODE             0xB2
#define GET_RT_MODE              0xB3
#define SET_RT_MODE              0xB4

extern otg_core_type otg_core_struct;

void usbd_keyboard_param_in_complete(void *udev);
void usbd_keyboard_sof_callback(void);
void usbd_set_keyboard_suspend(void);
void usbd_clear_keyboard_suspend(void);
confirm_state usbd_is_keyboard_suspend(void);
uint8_t hid_keyboard_report(uint8_t *data, uint8_t len);
confirm_state is_usbd_keyboard_configure(void);
confirm_state is_usbd_keyboard_started_report(void);

#if defined (__CC_ARM)
 #pragma anon_unions
#endif


#ifdef __cplusplus
}
#endif
  
#endif
