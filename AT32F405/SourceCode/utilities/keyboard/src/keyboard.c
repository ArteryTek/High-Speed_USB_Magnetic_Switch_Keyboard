#include "keyboard.h"
#include "keyboard_usb.h"
#include "keyboard_adc.h"
#include "keyboard_filter.h"
#include "keyboard_power.h"
#include "wk_system.h"

static void keyboard_add_key(uint8_t keycode);
static void keyboard_clear_key(uint8_t keycode);
static void key_set_modifier(uint8_t code);
static void key_reset_modifier(uint8_t code);

#if SUPPORT_AUTO_LEARN_RT
static void rt_update(rt_key_t *k);
#endif

static keyboard_state_t keyboard_state;
keyboard_state_t *p_key = &keyboard_state;
key_rt_param_t rt_param[KEY_ADC_KEY_NUM]; 

static void keyboard_add_key(uint8_t keycode)
{
  if(p_key->keyboard_6krp.keynum > 5)
    p_key->keyboard_6krp.keynum = 0;
  p_key->keyboard_6krp.keycode[p_key->keyboard_6krp.keynum ++] = keycode;
  return;
};

static void keyboard_clear_key(uint8_t keycode)
{
  uint8_t i = 0;
  
  for(i = 0; i < 6; i ++)
  {
    if(p_key->keyboard_6krp.keycode[i] == keycode)
      p_key->keyboard_6krp.keycode[i] = 0x00;
  }
  return;
};

static void key_set_modifier(uint8_t code)
{
  key_report *key_r = &p_key->keyboard_6krp;
  switch(code)
  {
    case KEY_LEFT_CTRL:
      key_r->modifier |= HID_MODIFIER_LEFT_CTRL;
      break;
    case KEY_LEFT_SHIFT:
      key_r->modifier |= HID_MODIFIER_LEFT_SHIFT;
      break;
    case KEY_LEFT_ALT:
      key_r->modifier |= HID_MODIFIER_LEFT_ALT;
      break;
    case KEY_LEFT_META:
      key_r->modifier |= HID_MODIFIER_LEFT_TGUI;
      break;
    case KEY_RIGHT_CTRL:
      key_r->modifier |= HID_MODIFIER_RIGHT_CTRL;
      break;
    case KEY_RIGHT_SHIFT:
      key_r->modifier |= HID_MODIFIER_RIGHT_SHIFT;
      break;
    case KEY_RIGHT_ALT:
      key_r->modifier |= HID_MODIFIER_RIGHT_ALT;
      break;
    case KEY_RIGHT_META:
      key_r->modifier |= HID_MODIFIER_RIGHT_TGUI;
      break;
    default:
      break;
  }
}

static void key_reset_modifier(uint8_t code)
{
  key_report *key_r = &p_key->keyboard_6krp;
  switch(code)
  {
    case KEY_LEFT_CTRL:
      key_r->modifier &= ~HID_MODIFIER_LEFT_CTRL;
      break;
    case KEY_LEFT_SHIFT:
      key_r->modifier &= ~HID_MODIFIER_LEFT_SHIFT;
      break;
    case KEY_LEFT_ALT:
      key_r->modifier &= ~HID_MODIFIER_LEFT_ALT;
      break;
    case KEY_LEFT_META:
      key_r->modifier &= ~HID_MODIFIER_LEFT_TGUI;
      break;
    case KEY_RIGHT_CTRL:
      key_r->modifier &= ~HID_MODIFIER_RIGHT_CTRL;
      break;
    case KEY_RIGHT_SHIFT:
      key_r->modifier &= ~HID_MODIFIER_RIGHT_SHIFT;
      break;
    case KEY_RIGHT_ALT:
      key_r->modifier &= ~HID_MODIFIER_RIGHT_ALT;
      break;
    case KEY_RIGHT_META:
      key_r->modifier &= ~HID_MODIFIER_RIGHT_TGUI;
      break;
    default:
      break;
  }
}

#if SUPPORT_AUTO_LEARN_RT
static void rt_update(rt_key_t *k)
{
  uint16_t travel;
  uint16_t target_press;
  uint16_t target_release;
  
  if(k->learn_cnt >= RT_LEARN_MAX_CNT)
    return;    
  
  if(k->adc_max_value <= k->adc_min_value)
    return;

  travel = k->adc_max_value - k->adc_min_value;

  if(travel < RT_LEARN_MIN_TRAVEL)
    return;

  target_press   = (uint16_t)(travel * RT_PRESS_RATIO);
  target_release = (uint16_t)(travel * RT_RELEASE_RATIO);

  target_press   = CLAMP(target_press, RT_PRESS_MIN, RT_PRESS_MAX);
  target_release = CLAMP(target_release, RT_RELEASE_MIN, RT_RELEASE_MAX);

  if(k->learn_cnt == 0)
  {
    k->rt_press_delta   = target_press;
    k->rt_release_delta = target_release;
  }
  else
  {
    k->rt_press_delta = (k->rt_press_delta * 3 + target_press) / 4;

    k->rt_release_delta = (k->rt_release_delta * 3 + target_release) / 4;
  }

  if(k->learn_cnt < RT_LEARN_MAX_CNT)
      k->learn_cnt++;
}
#endif

void key_trigger_scan_start(void)
{
  if(p_key->is_lowpower == FALSE && usbd_is_keyboard_suspend()== TRUE) {
    /* enter lowpower */
    suspend_enter_lower_power();
  }
    
    /* exit lowpower*/
  if(p_key->is_lowpower == TRUE && usbd_is_keyboard_suspend() == FALSE) {
    suspend_exit_lower_power();
  }
  adc_ordinary_software_trigger_enable(KEY_ADC, TRUE);
}

void keyboard_task(void)
{
  uint8_t i; 
  static uint8_t key_report = 0;
  uint16_t value;
  int dv;
  rt_key_t *k;

  if(p_key->adc_key.is_done)
  {
    p_key->adc_key.is_done = FALSE;

#ifdef KEYBOARD_SCAN_USE_USB_SOF /* usb high-speed sof trigger only 8kHz scan rate */
    if(p_key->trigger != SOF_SCAN_TRIGGER) {
      if(is_usbd_keyboard_configure() == TRUE) {
        p_key->trigger = SOF_SCAN_TRIGGER;
      }
    }
#endif
    if(is_usbd_keyboard_started_report() == FALSE) {
      key_report = 0;
      return;
    }
    
    /* trgger scan adc */
    if(p_key->trigger == SW_SCAN_TRIGGER)
      key_trigger_scan_start();

    for(i = 0; i < KEY_ADC_KEY_NUM; i ++)
    {
      if(adc_key_code[i] == 0)
        continue;
      
      k = &p_key->rt_key[i];
      k->hid_code = adc_key_code[i];
#if USE_KALMAN2_FILTER
      k->adc_filter_value = (uint16_t)kalman2_update(k, p_key->adc_key.p_values[i]);/*56.77us, IDLE 730ns*/
#endif
#if USE_KALMAN1_FILTER
      k->adc_filter_value = (uint16_t)kalman1_update(k, p_key->adc_key.p_values[i]); /*37us, IDLE 7.82*/
#endif
#if USE_EMA_FILTER
      k->adc_filter_value = EMA(p_key->adc_key.p_values[i], k->adc_filter_value);   /*23us, IDLE 21.32us*/
#endif
      dv = k->adc_filter_value - k->adc_last_value;
      k->adc_last_value = k->adc_filter_value ;
      
      /* updata rt */
      if(rt_param[i].rt_set && k->state == RT_KEY_IDLE) {
        rt_param[i].rt_set = 0;
        k->rt_press_delta = rt_param[i].rt_press_delta;
        k->rt_release_delta = rt_param[i].rt_release_delta;
      }

      value = k->adc_filter_value;
      switch(k->state) {
        case RT_KEY_IDLE:
          if(dv > DV_DEADZONE && value > k->adc_max_value)
            k->adc_max_value = value;
   
          if(dv <= DV_DEADZONE && k->adc_max_value - value > k->rt_press_delta) {
            k->state = RT_KEY_PRESSED;
            k->adc_min_value = value;
            if(k->hid_code >= KEY_LEFT_CTRL && k->hid_code <= KEY_RIGHT_META) {
              key_report = 1;
              key_set_modifier(k->hid_code);
            } else {
              key_report = 1;
              keyboard_add_key(k->hid_code);
            }
          }
          break;
        case RT_KEY_PRESSED:
          if(dv < DV_DEADZONE && value < k->adc_min_value)
            k->adc_min_value = value;
          
          if(dv >= DV_DEADZONE && (value - k->adc_min_value > k->rt_release_delta)) {
            k->state = RT_KEY_IDLE;
            if(k->hid_code >= KEY_LEFT_CTRL && k->hid_code <= KEY_RIGHT_META) {
              key_report = 1;
              key_reset_modifier(k->hid_code);
            } else {
              keyboard_clear_key(k->hid_code);
              key_report = 1;
            }
  #if SUPPORT_AUTO_LEARN_RT
            rt_update(&p_key->rt_key[i]);
  #endif
            k->adc_max_value = value;
          }
          break;
        default:
          break;
      }
    }
  }

  if(key_report) {
    if(hid_keyboard_report((uint8_t *)&p_key->keyboard_6krp, 8) == 0) {
      key_report = 0;
    }
  }
}

#ifdef KEYBOARD_TASK_USE_INTERRUPT
void at32_exint_key_task_init(void)
{
  gpio_init_type gpio_init_struct;
  exint_init_type exint_init_struct;

  crm_periph_clock_enable(CRM_GPIOA_PERIPH_CLOCK, TRUE);
  gpio_default_para_init(&gpio_init_struct);

  gpio_init_struct.gpio_drive_strength = GPIO_DRIVE_STRENGTH_STRONGER;
  gpio_init_struct.gpio_out_type  = GPIO_OUTPUT_PUSH_PULL;
  gpio_init_struct.gpio_mode = GPIO_MODE_INPUT;
  gpio_init_struct.gpio_pins = GPIO_PINS_15;
  gpio_init_struct.gpio_pull = GPIO_PULL_DOWN;
  gpio_init(GPIOA, &gpio_init_struct);
  
  crm_periph_clock_enable(CRM_SCFG_PERIPH_CLOCK, TRUE);
  scfg_exint_line_config(SCFG_PORT_SOURCE_GPIOA, SCFG_PINS_SOURCE15);
  
  exint_default_para_init(&exint_init_struct);
  exint_init_struct.line_enable = TRUE;
  exint_init_struct.line_mode = EXINT_LINE_INTERRUPT;
  exint_init_struct.line_select = EXINT_LINE_15;
  exint_init_struct.line_polarity = EXINT_TRIGGER_RISING_EDGE;
  exint_init(&exint_init_struct);
  
  nvic_irq_enable(EXINT15_10_IRQn, TASK_IRQ_PRIORITY, 0);
}

void EXINT15_10_IRQHandler(void)
{
  if(EXINT->intsts & EXINT_LINE_15){
    exint_flag_clear(EXINT_LINE_15);
    keyboard_task();
  }
}
#endif

void keyboard_init(void)
{
  keyboard_adc_init();
#ifdef KEYBOARD_TASK_USE_INTERRUPT
  at32_exint_key_task_init();
#endif
  key_matrix_cs_init();
  
  p_key->is_lowpower = FALSE;
  p_key->trigger = SW_SCAN_TRIGGER;
  keyboard_adc_calibration();
  
  return;
}
