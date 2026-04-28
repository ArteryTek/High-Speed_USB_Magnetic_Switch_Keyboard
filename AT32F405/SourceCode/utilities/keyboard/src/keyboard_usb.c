#include "keyboard_usb.h"
#include "keyboard_adc.h"
#include "keyboard_power.h"
#include "keyboard.h"

extern iap_info_type *iap_info;
extern uint8_t rgb_refresh_mode;
#if defined ( __ICCARM__ ) /* iar compiler */
  #pragma data_alignment=4
#endif
ALIGNED_HEAD static uint8_t param_data[64] ALIGNED_TAIL;

uint8_t usbd_keyboard_param_process(void *udev, uint8_t *pdata, uint16_t len)
{
  uint16_t cmd, tmp;
  uint8_t idx = 0, respond = 0, status = 0, code = 0;
  uint8_t i;
  uint16_t rt_press_param = 0;
  uint16_t rt_release_param = 0;

  if(len < 2)
  {
    return 1;
  }
  iap_info->state = IAP_STS_OTHER;
  cmd = pdata[1];
  
  param_data[idx++] = pdata[0];
  param_data[idx++] = pdata[1];

  switch(cmd)
  {
    case GET_LED_MODE:
      param_data[idx++] = 1; /* length */
      param_data[idx++] = rgb_refresh_mode;
      respond = 1;
      break;
    case SET_LED_MODE:
      rgb_refresh_mode = pdata[3];
      break;
    case GET_RT_MODE:
      param_data[idx++] = 12; /* length */
      
      /* press min */
      param_data[idx++] = RT_PRESS_MIN & 0xFF;
      param_data[idx++] = (RT_PRESS_MIN >> 8) & 0xFF;
    
      /* press max */
      param_data[idx++] = RT_PRESS_MAX & 0xFF;
      param_data[idx++] = (RT_PRESS_MAX >> 8) & 0xFF;
    
      /* release min */
      param_data[idx++] = RT_RELEASE_MIN & 0xFF;
      param_data[idx++] = (RT_RELEASE_MIN >> 8) & 0xFF;
    
      /* release max */
      param_data[idx++] = RT_RELEASE_MAX & 0xFF;
      param_data[idx++] = (RT_RELEASE_MAX >> 8) & 0xFF;
      
      param_data[idx++] = rt_press_param & 0xFF;
      param_data[idx++] = (rt_press_param >> 8) & 0xFF;
      param_data[idx++] = rt_release_param & 0xFF;
      param_data[idx++] = (rt_release_param >> 8) & 0xFF;
      respond = 1;
      break;
    case SET_RT_MODE:
      if(pdata[2] == 5) {
        code = pdata[3];
        tmp = pdata[4] | (pdata[5] << 8);
        if(tmp >= RT_PRESS_MIN && tmp <= RT_PRESS_MAX) {
          rt_press_param = tmp;
        }
        
        tmp = pdata[6] | (pdata[7] << 8);
        if(tmp >= RT_RELEASE_MIN && tmp <= RT_PRESS_MAX) {
          rt_release_param = tmp;
        }
        if(rt_press_param != 0 && rt_release_param != 0) {
          if(code == 0) {
            for(i = 0; i < KEY_ADC_KEY_NUM; i ++) {
              rt_param[i].rt_press_delta = rt_press_param;
              rt_param[i].rt_release_delta = rt_release_param;
              rt_param[i].rt_set = 1;
            }
          } else {
            for(i = 0; i < KEY_ADC_KEY_NUM; i ++) {
              if(code == rt_param[i].hid_code) {
                rt_param[i].rt_press_delta = rt_press_param;
                rt_param[i].rt_release_delta = rt_release_param;
                rt_param[i].rt_set = 1;
                break;
              }
            }
          }
        }
      }
      break;
    default:
      status = 1;
      break;
  }

  if(respond) {
    usb_iap_class_send_report(udev, param_data, 64);
  }

  return status;
}



void usbd_keyboard_param_in_complete(void *udev)
{
  return; 
}

void usbd_keyboard_sof_callback(void)
{
#ifdef KEYBOARD_SCAN_USE_USB_SOF
  if(p_key->trigger == SOF_SCAN_TRIGGER)
    key_trigger_scan_start();
#endif
}

void usbd_set_keyboard_suspend(void)
{
#ifdef KEYBOARD_SCAN_USE_USB_SOF
  if(p_key->trigger == SOF_SCAN_TRIGGER) {
    p_key->trigger = SW_SCAN_TRIGGER;
    key_trigger_scan_start();
  }
#endif
  p_key->is_suspend = TRUE;
}

void usbd_clear_keyboard_suspend(void)
{
  p_key->is_suspend = FALSE;
}

confirm_state usbd_is_keyboard_suspend(void)
{
  return p_key->is_suspend;
}

confirm_state is_usbd_keyboard_configure(void)
{
  if(p_key->is_suspend == FALSE && 
    usbd_connect_state_get(&otg_core_struct.dev) == USB_CONN_STATE_CONFIGURED) {
    return TRUE;
  }
  return FALSE;
}

uint8_t hid_keyboard_report(uint8_t *data, uint8_t len)
{
  if(p_key->is_suspend && (otg_core_struct.dev.remote_wakup == 1)) {
    usbd_clear_keyboard_suspend();
    suspend_exit_lower_power();
    usbd_remote_wakeup(&otg_core_struct.dev);
    return 0;
  } else {
    return (uint8_t)usb_keyboard_class_send_report(&otg_core_struct.dev, (uint8_t *)data, 8);
  }
}

confirm_state is_usbd_keyboard_started_report(void)
{
  keyboard_type * pclass = (keyboard_type *)(otg_core_struct.dev.class_handler->pdata);
  if(pclass->start_report == 0)
  {
    memset(&p_key->keyboard_6krp, 0, sizeof(key_report));
    hid_keyboard_report((uint8_t *)&p_key->keyboard_6krp, 8);
    if(p_key->trigger == SW_SCAN_TRIGGER)
      adc_ordinary_software_trigger_enable(KEY_ADC, TRUE);
    return FALSE;
  }
  return TRUE;
}




