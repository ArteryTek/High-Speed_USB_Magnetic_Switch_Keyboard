#ifndef _KEYBOARD_H
#define _KEYBOARD_H

#ifdef __cplusplus
extern "C" {
#endif
#include "platform.h"
#include "keymap.h"

#if defined (__CC_ARM)
 #pragma anon_unions
#endif

/* usb high-speed sof trigger only 8kHz scan rate */
/* #define KEYBOARD_SCAN_USE_USB_SOF */
#define KEYBOARD_TASK_USE_INTERRUPT

#define USE_EMA_FILTER               0
#define USE_KALMAN1_FILTER           1
#define USE_KALMAN2_FILTER           0

#define CALI_SAMPLE_CNT   16
#define ADC_VALID_CAL     2304

/*
#define ADC_VALID_MAX     2500
#define ADC_VALID_MIN     1000
*/
#ifndef CLAMP
#define CLAMP(x, min, max)   ((x) < (min) ? (min) : ((x) > (max) ? (max) : (x)))
#endif

#define RT_LEARN_MIN_TRAVEL      100

#define RT_PRESS_RATIO           0.15f
#define RT_RELEASE_RATIO         0.10f

#define RT_PRESS_MIN             200
#define RT_PRESS_MAX             800
#define RT_PRESS_DEFAULT         200

#define RT_RELEASE_MIN           150
#define RT_RELEASE_MAX           800
#define RT_RELEASE_DEFAULT       150

#define RT_LEARN_MAX_CNT         16

#define SUPPORT_AUTO_LEARN_RT    0
#define DV_DEADZONE              0

typedef enum {
  SW_SCAN_TRIGGER,
  SOF_SCAN_TRIGGER,
  TMR_SCAN_TRIGGER
}_scan_trigger;

typedef struct
{
  uint8_t hid_code;
  uint8_t rt_set;
  uint16_t rt_press_delta;
  uint16_t rt_release_delta;
}key_rt_param_t;

typedef enum {
  RT_KEY_IDLE = 0,
  RT_KEY_PRESSED,
}rt_state_t;

typedef struct {
  uint16_t adc_filter_value;
  uint16_t adc_last_value;
  uint16_t adc_min_value;
  uint16_t adc_max_value;
#if USE_KALMAN2_FILTER || USE_KALMAN1_FILTER
  int32_t pos;
  int32_t vel;
  int32_t Pp;
  int32_t Pv;
  uint8_t inited;
#endif  
  uint16_t rt_press_delta;
  uint16_t rt_release_delta;
  uint16_t learn_cnt;
  uint16_t noise;
  uint8_t distance;
  rt_state_t state;
  uint8_t hid_code;
}rt_key_t;

typedef struct
{
  uint16_t adc_key_values[2][KEY_ADC_KEY_NUM];
  uint16_t offset;
  uint16_t *p_values;
  uint8_t  id;
  uint8_t  cs_mux;
  __IO confirm_state  is_done;
  __IO confirm_state  is_calibration;
}adc_key_scan_t;

typedef struct
{
  uint8_t modifier;
  uint8_t reserved;
  uint8_t keycode[6];
  uint8_t keynum;
}key_report;

typedef struct
{
  adc_key_scan_t adc_key;
  rt_key_t rt_key[KEY_ADC_KEY_NUM];
  key_report keyboard_6krp;
  __IO confirm_state is_suspend;
  __IO confirm_state is_lowpower;
  _scan_trigger trigger;
}keyboard_state_t;

extern keyboard_state_t *p_key;
extern key_rt_param_t rt_param[];

void key_trigger_scan_start(void);
void keyboard_init(void);
void keyboard_task(void);

#ifdef __cplusplus
}
#endif
  
#endif
