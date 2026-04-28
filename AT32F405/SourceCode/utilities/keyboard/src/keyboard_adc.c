#include "keyboard_adc.h"
#include "keyboard.h"

static inline void key_matrix_cs_switch(uint8_t mux, uint32_t delay);

static uint16_t adc_dma_buffer[KEY_ADC_CHANNEL_MAX * 2];

void KEY_ADC_DMA_IRQ_HANDLER(void) /* function exec time 1us, interrupt time 4.6us*/
{
  uint16_t *p = p_key->adc_key.adc_key_values[p_key->adc_key.id]; 

  /* add multiplexer id */
  p_key->adc_key.cs_mux ++;

  if(p_key->adc_key.cs_mux > 7) {  /* 43.04us 24kHz*/
    /* one scan completed */
    p_key->adc_key.is_done = TRUE;
    
    /* clear multiplexer id to 0 start next scan */
    p_key->adc_key.cs_mux = 0;
    
    p_key->adc_key.p_values = p_key->adc_key.adc_key_values[p_key->adc_key.id];

    /* modify adc key ping pong buffer id */
    p_key->adc_key.id ^= 1;
    
    key_matrix_cs_switch(p_key->adc_key.cs_mux, 1);
#ifdef KEYBOARD_TASK_USE_INTERRUPT
    if(p_key->adc_key.is_calibration == TRUE) {
      exint_software_interrupt_event_generate(EXINT_LINE_15);
    }
#endif
  } else {
    key_matrix_cs_switch(p_key->adc_key.cs_mux, 1);
    key_trigger_scan_start();
  }
  
  /* switch multiplexer id and start next adc convert */
/*
  key_matrix_cs_switch(p_key->adc_key.cs_mux, 1);
  key_trigger_scan_start();
*/

  if(dma_interrupt_flag_get(KEY_ADC_DMA_FULL_FLAG) != RESET)
  {
    memcpy(p + p_key->adc_key.offset, adc_dma_buffer + KEY_ADC_CHANNEL_MAX, KEY_ADC_CHANNEL_MAX * 2);
    p_key->adc_key.offset += KEY_ADC_CHANNEL_MAX;
    dma_flag_clear(KEY_ADC_DMA_FULL_FLAG);
  }
  else if(dma_interrupt_flag_get(KEY_ADC_DMA_HALF_FLAG) != RESET)
  {
    memcpy(p + p_key->adc_key.offset, adc_dma_buffer, KEY_ADC_CHANNEL_MAX * 2);
    p_key->adc_key.offset += KEY_ADC_CHANNEL_MAX;
    dma_flag_clear(KEY_ADC_DMA_HALF_FLAG);
  }
  
  if(p_key->adc_key.cs_mux == 0)
  {
    /* next offset to set 0 */
    p_key->adc_key.offset = 0;
  }
}

static inline void key_matrix_cs_switch(uint8_t mux, uint32_t delay)
{
  __IO uint32_t times = 0;  
  gpio_bits_write(MUX_CS0_GPIO, MUX_CS0, (confirm_state)(mux & 0x1));
  gpio_bits_write(MUX_CS1_GPIO, MUX_CS1, (confirm_state)((mux >> 1) & 0x1));
  gpio_bits_write(MUX_CS2_GPIO, MUX_CS2, (confirm_state)((mux >> 2) & 0x1));
  
  for(times = 0; times < delay; times ++)
  {
  }
}

void keyboard_adc_calibration(void)
{
  uint8_t i = 0;
  uint8_t calibration_cnt = 0;
  uint32_t v, avg;
  
  for(i = 0; i < KEY_ADC_KEY_NUM; i ++)
  {
    p_key->rt_key[i].adc_max_value = 0;
  }
  p_key->adc_key.is_calibration = FALSE;
  adc_ordinary_software_trigger_enable(ADC1, TRUE);
  
  while(calibration_cnt < CALI_SAMPLE_CNT)
  {
    if(p_key->adc_key.is_done)
    {
      p_key->adc_key.is_done = FALSE;
      adc_ordinary_software_trigger_enable(KEY_ADC, TRUE);
      for(i = 0; i < KEY_ADC_KEY_NUM; i ++)
      {
        v = p_key->adc_key.p_values[i];
        
#ifdef ADC_CAL_CHECK
        if(v > ADC_VALID_CAL)
          p_key->rt_key[i].adc_max_value += v;
        else
          p_key->rt_key[i].adc_max_value += ADC_VALID_CAL;
          
#else
        p_key->rt_key[i].adc_max_value += v;
#endif

      }
      calibration_cnt ++;
    }
  }

  for(i = 0; i < KEY_ADC_KEY_NUM; i ++)
  {
    avg = p_key->rt_key[i].adc_max_value / CALI_SAMPLE_CNT;
    p_key->rt_key[i].adc_max_value = avg;
    p_key->rt_key[i].adc_filter_value = avg;
    p_key->rt_key[i].adc_min_value = avg;
    p_key->rt_key[i].adc_last_value = avg;
    p_key->rt_key[i].rt_press_delta = RT_PRESS_DEFAULT;
    p_key->rt_key[i].rt_release_delta = RT_RELEASE_DEFAULT;
    p_key->rt_key[i].learn_cnt = 0;
    p_key->rt_key[i].state = RT_KEY_IDLE;
    
    rt_param[i].hid_code = adc_key_code[i];
    rt_param[i].rt_press_delta = RT_PRESS_DEFAULT;
    rt_param[i].rt_release_delta = RT_RELEASE_DEFAULT;
    rt_param[i].rt_set = 0;
  }
  /* wait complete */
  while(p_key->adc_key.is_done != TRUE) {
  }
  /* start key scan */
  p_key->adc_key.is_done = FALSE;

  p_key->adc_key.is_calibration = TRUE;
  
  adc_ordinary_software_trigger_enable(KEY_ADC, TRUE);
  return;
}


void key_matrix_cs_init(void)
{
  gpio_init_type gpio_init_struct;

  crm_periph_clock_enable(MUX_CS0_GPIO_CLK, TRUE);
  crm_periph_clock_enable(MUX_CS1_GPIO_CLK, TRUE);
  crm_periph_clock_enable(MUX_CS2_GPIO_CLK, TRUE);
  gpio_default_para_init(&gpio_init_struct);

  gpio_init_struct.gpio_drive_strength = GPIO_DRIVE_STRENGTH_STRONGER;
  gpio_init_struct.gpio_out_type  = GPIO_OUTPUT_PUSH_PULL;
  gpio_init_struct.gpio_mode = GPIO_MODE_OUTPUT;
  gpio_init_struct.gpio_pull = GPIO_PULL_DOWN;
  
  /* cs0 */
  gpio_init_struct.gpio_pins = MUX_CS0;
  gpio_init(MUX_CS0_GPIO, &gpio_init_struct);
  
  /* cs1 */
  gpio_init_struct.gpio_pins = MUX_CS1;
  gpio_init(MUX_CS1_GPIO, &gpio_init_struct);
  
  /* cs2 */
  gpio_init_struct.gpio_pins = MUX_CS2;
  gpio_init(MUX_CS2_GPIO, &gpio_init_struct);
  
  key_matrix_cs_switch(0, 0);
  p_key->adc_key.cs_mux = 0;
}

void keyboard_adc_init(void)
{
  adc_base_config_type adc_base_struct;
  dma_init_type dma_init_struct;

  gpio_init_type gpio_initstructure;
  crm_periph_clock_enable(CRM_GPIOA_PERIPH_CLOCK, TRUE);
  crm_periph_clock_enable(CRM_GPIOB_PERIPH_CLOCK, TRUE);

  gpio_default_para_init(&gpio_initstructure);

  /* config adc pin as analog input mode */
  gpio_initstructure.gpio_mode = GPIO_MODE_ANALOG;
  
  crm_periph_clock_enable(ADC_CH1_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH1_PIN;
  gpio_init(ADC_CH1_GPIO, &gpio_initstructure);
  
  crm_periph_clock_enable(ADC_CH2_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH2_PIN;
  gpio_init(ADC_CH2_GPIO, &gpio_initstructure);
  
  crm_periph_clock_enable(ADC_CH3_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH3_PIN;
  gpio_init(ADC_CH3_GPIO, &gpio_initstructure);
  
  crm_periph_clock_enable(ADC_CH4_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH4_PIN;
  gpio_init(ADC_CH4_GPIO, &gpio_initstructure);
  
  crm_periph_clock_enable(ADC_CH5_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH5_PIN;
  gpio_init(ADC_CH5_GPIO, &gpio_initstructure);
  
  crm_periph_clock_enable(ADC_CH6_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH6_PIN;
  gpio_init(ADC_CH6_GPIO, &gpio_initstructure);
  
  crm_periph_clock_enable(ADC_CH7_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH7_PIN;
  gpio_init(ADC_CH7_GPIO, &gpio_initstructure);
  
  crm_periph_clock_enable(ADC_CH8_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH8_PIN;
  gpio_init(ADC_CH8_GPIO, &gpio_initstructure);
  
  crm_periph_clock_enable(ADC_CH9_GPIO_CLK, TRUE);
  gpio_initstructure.gpio_pins = ADC_CH9_PIN;
  gpio_init(ADC_CH9_GPIO, &gpio_initstructure);
  
  
  crm_periph_clock_enable(KEY_ADC_CLK, TRUE);
  adc_reset(KEY_ADC);
  adc_clock_div_set(KEY_ADC_DIV);
  nvic_irq_enable(KEY_ADC_IRQ, 1, 0);
  adc_base_default_para_init(&adc_base_struct);
  
  adc_base_struct.sequence_mode = TRUE;
  adc_base_struct.repeat_mode = FALSE;
  adc_base_struct.data_align = ADC_RIGHT_ALIGNMENT;
  adc_base_struct.ordinary_channel_length = KEY_ADC_CHANNEL_MAX;
  adc_base_config(KEY_ADC, &adc_base_struct);
  
  /* config ordinary channel */
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_0, 1, KEY_ADC_SAMPLE_RATE);
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_1, 2, KEY_ADC_SAMPLE_RATE);
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_2, 3, KEY_ADC_SAMPLE_RATE);
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_3, 4, KEY_ADC_SAMPLE_RATE);
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_4, 5, KEY_ADC_SAMPLE_RATE);
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_5, 6, KEY_ADC_SAMPLE_RATE);
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_6, 7, KEY_ADC_SAMPLE_RATE);
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_7, 8, KEY_ADC_SAMPLE_RATE);
  adc_ordinary_channel_set(KEY_ADC, ADC_CHANNEL_8, 9, KEY_ADC_SAMPLE_RATE);
  
  /* config ordinary trigger source and trigger edge */
  adc_ordinary_conversion_trigger_set(KEY_ADC, ADC12_ORDINARY_TRIG_SOFTWARE, TRUE);
  
  /* config dma mode,it's not useful when common dma mode is use */
  adc_dma_mode_enable(KEY_ADC, TRUE);

  /* adc enable */
  adc_enable(KEY_ADC, TRUE);

  /* adc calibration */
  adc_calibration_init(KEY_ADC);
  while(adc_calibration_init_status_get(KEY_ADC));
  adc_calibration_start(KEY_ADC);
  while(adc_calibration_status_get(KEY_ADC));
  
  crm_periph_clock_enable(KEY_ADC_DMA_CLK, TRUE);
  nvic_irq_enable(DMA1_Channel1_IRQn, DMA_IRQ_PRIORITY, 0);

  dma_reset(KEY_ADC_DMA_CH);
  dma_default_para_init(&dma_init_struct);
  dma_init_struct.buffer_size = KEY_ADC_CHANNEL_MAX * 2;
  dma_init_struct.direction = DMA_DIR_PERIPHERAL_TO_MEMORY;
  dma_init_struct.memory_base_addr = (uint32_t)adc_dma_buffer;
  dma_init_struct.memory_data_width = DMA_MEMORY_DATA_WIDTH_HALFWORD;
  dma_init_struct.memory_inc_enable = TRUE;
  dma_init_struct.peripheral_base_addr = (uint32_t)&KEY_ADC_DT;
  dma_init_struct.peripheral_data_width = DMA_PERIPHERAL_DATA_WIDTH_HALFWORD;
  dma_init_struct.peripheral_inc_enable = FALSE;
  dma_init_struct.priority = DMA_PRIORITY_HIGH;
  dma_init_struct.loop_mode_enable = TRUE;
  dma_init(KEY_ADC_DMA_CH, &dma_init_struct);

  dmamux_enable(KEY_ADC_DMA, TRUE);
  dmamux_init(KEY_ADC_DMACH_MUX, KEY_ADC_DMAREQ_ID);

  /* enable dma transfer complete interrupt */
  dma_interrupt_enable(KEY_ADC_DMA_CH, DMA_HDT_INT, TRUE);
  dma_interrupt_enable(KEY_ADC_DMA_CH, DMA_FDT_INT, TRUE);
  dma_channel_enable(DMA1_CHANNEL1, TRUE);
}
