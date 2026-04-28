#include "rgb_led.h"
#include "wk_system.h"
#include "keyboard_usb.h"
rgb_led_t rgb_led[RGB_LED_MAX];
static uint8_t rgb_refresh_completed = 1;
uint8_t rgb_refresh_mode = 0;

void hsv_to_rgb(uint16_t h, uint8_t v, uint8_t *r, uint8_t *g, uint8_t *b)
{
  uint8_t region = h / 60;
  uint16_t rem = (h % 60) * 255 / 60;
  uint8_t p = 0;
  uint8_t q = (255 - rem) * v / 255;
  uint8_t t = rem * v / 255;

  switch (region) {
    case 0: *r = v; *g = t; *b = p; break;
    case 1: *r = q; *g = v; *b = p; break;
    case 2: *r = p; *g = v; *b = t; break;
    case 3: *r = p; *g = q; *b = v; break;
    case 4: *r = t; *g = p; *b = v; break;
    default:*r = v; *g = p; *b = q; break;
  }
}

uint8_t breathe_set_value(uint16_t phase)
{
  uint16_t x = phase & 0x1FF;
  uint8_t v;
  
  if (x < 256) {
    v = x;
  } else {
    v = 511 - x;
  }
  return (v * v) >> 8;
}

#if WS2812_CONTROL_USE_SPI
#define RGB_RESET_SLOTS    0
static uint8_t rgb_pwm_data[RGB_LED_MAX + RGB_RESET_SLOTS][12];
static uint8_t rgb_logic_0 = 0x8;
static uint8_t rgb_logic_1 = 0xE;

void RGB_SPI_DMA_IRQ_HANDLER(void)
{
  if(dma_interrupt_flag_get(RGB_SPI_DMA_FLAG) != RESET)
  {
    /* handle full data transfer and clear flag */
    dma_flag_clear(RGB_SPI_DMA_FLAG);
    dma_channel_enable(RGB_SPI_DMA_CH, FALSE);
    rgb_refresh_completed = 1;
  }
}

void ws2812_reset(void)
{
  uint8_t i = 0, j = 0;
  for (;i <  RGB_LED_MAX + RGB_RESET_SLOTS; i ++) {
    for(j = 0; j < 12; j ++) {
      rgb_pwm_data[i][j] = 0;
    }
  }
}

void ws2812_set_color(uint8_t led, uint8_t r, uint8_t g, uint8_t b)
{
  uint8_t i;
  uint8_t code = 0;
  uint32_t color = (g << 16) | (r << 8) | b;
  
  if(led > RGB_LED_MAX)
    return;
 
  for (i = 0; i < 24; i ++) {
    if (color & (1 << (23 - i))) {
      code = rgb_logic_1;
    } else {
      code = rgb_logic_0;
    }
    if(i & 1) {
      rgb_pwm_data[led][(i * 4) / 8] |= code << (4 - (( i * 4) % 8));
    } else {
      rgb_pwm_data[led][(i * 4) / 8] = code << (4 - (( i * 4) % 8));
    }
  }
}

void ws2812_rgb_send(void)
{
  rgb_refresh_completed = 0;
  RGB_SPI_DMA_CH->paddr = (uint32_t)&RGB_SPI_DT;
  RGB_SPI_DMA_CH->maddr = (uint32_t)rgb_pwm_data;
  RGB_SPI_DMA_CH->dtcnt = (RGB_LED_MAX + RGB_RESET_SLOTS) * 12;
  dma_channel_enable(RGB_SPI_DMA_CH, TRUE);
}

void ws2812_spi_control_init(void)
{
  gpio_init_type gpio_init_struct;
  spi_init_type spi_init_struct;
  dma_init_type dma_init_struct;

  /* configure the spi mosi pin */
  crm_periph_clock_enable(RGB_IN_GPIO_CLK, TRUE);
  gpio_default_para_init(&gpio_init_struct);
  gpio_pin_mux_config(RGB_IN_GPIO, RGB_IN_PINSOURCE, RGB_IN_MUX);
  gpio_init_struct.gpio_pins = RGB_IN_PIN;
  gpio_init_struct.gpio_mode = GPIO_MODE_MUX;
  gpio_init_struct.gpio_out_type = GPIO_OUTPUT_PUSH_PULL;
  gpio_init_struct.gpio_pull = GPIO_PULL_UP;
  gpio_init_struct.gpio_drive_strength = GPIO_DRIVE_STRENGTH_MODERATE;
  gpio_init(RGB_IN_GPIO, &gpio_init_struct);
  
  /* configure spi */
  crm_periph_clock_enable(RGB_SPI_CLK, TRUE);
  spi_init_struct.transmission_mode = SPI_TRANSMIT_FULL_DUPLEX;
  spi_init_struct.master_slave_mode = SPI_MODE_MASTER;
  spi_init_struct.mclk_freq_division = RGB_SPI_MCLK_DIV;
  spi_init_struct.first_bit_transmission = SPI_FIRST_BIT_MSB;
  spi_init_struct.frame_bit_num = SPI_FRAME_8BIT;
  spi_init_struct.clock_polarity = SPI_CLOCK_POLARITY_LOW;
  spi_init_struct.clock_phase = SPI_CLOCK_PHASE_2EDGE;
  spi_init_struct.cs_mode_selection = SPI_CS_SOFTWARE_MODE;
  spi_init(RGB_SPI, &spi_init_struct);
  spi_i2s_dma_transmitter_enable(RGB_SPI, TRUE);
  spi_enable(RGB_SPI, TRUE);
  
  /* spi dma config */
  crm_periph_clock_enable(RGB_SPI_DMA_CLK, TRUE);
  nvic_irq_enable(RGB_SPI_DMA_IRQ, RGB_IRQ_PRIORITY, 0);
  dma_reset(RGB_SPI_DMA_CH);
  dma_default_para_init(&dma_init_struct);
  dma_init_struct.direction = DMA_DIR_MEMORY_TO_PERIPHERAL;
  dma_init_struct.memory_data_width = DMA_MEMORY_DATA_WIDTH_BYTE;
  dma_init_struct.memory_inc_enable = TRUE;
  dma_init_struct.peripheral_data_width = DMA_PERIPHERAL_DATA_WIDTH_BYTE;
  dma_init_struct.peripheral_inc_enable = FALSE;
  dma_init_struct.priority = DMA_PRIORITY_LOW;
  dma_init_struct.loop_mode_enable = FALSE;
  dma_init(RGB_SPI_DMA_CH, &dma_init_struct);
  dma_interrupt_enable(RGB_SPI_DMA_CH, DMA_FDT_INT, TRUE);
  dmamux_enable(RGB_SPI_DMA, TRUE);
  dmamux_init(RGB_SPI_DMACH_MUX, RGB_SPI_DMAREQ_ID);
}
#endif


#if WS2812_CONTROL_USE_TMR
#define RGB_RESET_SLOTS    3
static uint16_t rgb_pwm_data[RGB_LED_MAX + RGB_RESET_SLOTS][24];
static uint16_t rgb_logic_0 = 0;
static uint16_t rgb_logic_1 = 0;

void RGB_TMR_DMA_IRQ_HANDLER(void)
{
  if(dma_interrupt_flag_get(RGB_TMR_DMA_FLAG) != RESET)
  {
    /* handle full data transfer and clear flag */
    dma_flag_clear(RGB_TMR_DMA_FLAG);
    tmr_counter_enable(RGB_TMR, FALSE);
    dma_channel_enable(RGB_TMR_DMA_CH, FALSE);
    rgb_refresh_completed = 1;
  }
}

void ws2812_reset(void)
{
  uint8_t i = 0, j = 0;
  for (;i <  RGB_LED_MAX + RGB_RESET_SLOTS; i ++) {
    for(j = 0; j < 24; j ++) {
      rgb_pwm_data[i][j] = 0;
    }
  }
}

void ws2812_set_color(uint8_t led, uint8_t r, uint8_t g, uint8_t b)
{
  uint8_t i;
  uint32_t color = (g << 16) | (r << 8) | b;
  
  if(led > RGB_LED_MAX)
    return;
  
  for (i = 0; i < 24; i ++) {
    if (color & (1 << (23 - i))) {
      rgb_pwm_data[led][i] = rgb_logic_1;
    } else {
      rgb_pwm_data[led][i] = rgb_logic_0;
    }
  }
}

void ws2812_rgb_send(void)
{
  rgb_refresh_completed = 0;
  RGB_TMR_DMA_CH->paddr = (uint32_t)&RGB_TMR_CxDT;
  RGB_TMR_DMA_CH->maddr = (uint32_t)rgb_pwm_data;
  RGB_TMR_DMA_CH->dtcnt = (RGB_LED_MAX + RGB_RESET_SLOTS) * 24;
  tmr_counter_enable(RGB_TMR, TRUE);
  dma_channel_enable(RGB_TMR_DMA_CH, TRUE);
}

void ws2812_tmr_control_init(void)
{
  uint32_t tmr_cycle_cnt = 0;
  uint32_t tmr_clock_freq = 0;
  crm_clocks_freq_type clock_freq;
  gpio_init_type gpio_init_struct;
  tmr_output_config_type tmr_output_struct;
  dma_init_type dma_init_struct;
  
  /* tmr dma config */
  crm_periph_clock_enable(RGB_TMR_DMA_CLK, TRUE);
  nvic_irq_enable(RGB_TMR_DMA_IRQ, 5, 0);
  dma_reset(RGB_TMR_DMA_CH);
  dma_default_para_init(&dma_init_struct);
  dma_init_struct.direction = DMA_DIR_MEMORY_TO_PERIPHERAL;
  dma_init_struct.memory_data_width = DMA_MEMORY_DATA_WIDTH_HALFWORD;
  dma_init_struct.memory_inc_enable = TRUE;
  dma_init_struct.peripheral_data_width = DMA_PERIPHERAL_DATA_WIDTH_HALFWORD;
  dma_init_struct.peripheral_inc_enable = FALSE;
  dma_init_struct.priority = DMA_PRIORITY_LOW;
  dma_init_struct.loop_mode_enable = FALSE;
  dma_init(RGB_TMR_DMA_CH, &dma_init_struct);
  dma_interrupt_enable(RGB_TMR_DMA_CH, DMA_FDT_INT, TRUE);
  dmamux_enable(RGB_TMR_DMA, TRUE);
  dmamux_init(RGB_TMR_DMACH_MUX, RGB_TMR_DMAREQ_ID);
  
  /* configure the tmr chx pin */
  crm_periph_clock_enable(RGB_IN_GPIO_CLK, TRUE);
  gpio_default_para_init(&gpio_init_struct);
  gpio_pin_mux_config(RGB_IN_GPIO, RGB_IN_PINSOURCE, RGB_IN_MUX);
  gpio_init_struct.gpio_pins = RGB_IN_PIN;
  gpio_init_struct.gpio_mode = GPIO_MODE_MUX;
  gpio_init_struct.gpio_out_type = GPIO_OUTPUT_PUSH_PULL;
  gpio_init_struct.gpio_pull = GPIO_PULL_NONE;
  gpio_init_struct.gpio_drive_strength = GPIO_DRIVE_STRENGTH_MODERATE;
  gpio_init(RGB_IN_GPIO, &gpio_init_struct);

  /* get tmr clock freq */
  crm_clocks_freq_get(&clock_freq);
  if(clock_freq.ahb_freq != RGB_TMR_APB_BUS) {
    tmr_clock_freq = RGB_TMR_APB_BUS * 2;
  } else {
    tmr_clock_freq = RGB_TMR_APB_BUS;
  }
  
  /* timer clock / 800Khz */
  tmr_cycle_cnt = tmr_clock_freq / 800000;
  
  rgb_logic_0 = 35 * tmr_cycle_cnt / 100;
  rgb_logic_1 = 70 * tmr_cycle_cnt / 100;
  
  crm_periph_clock_enable(RGB_TMR_CLK, TRUE);
  /* configure counter settings */
  tmr_cnt_dir_set(RGB_TMR, TMR_COUNT_UP);
  tmr_clock_source_div_set(RGB_TMR, TMR_CLOCK_DIV1);
  tmr_repetition_counter_set(RGB_TMR, 0);
  tmr_period_buffer_enable(RGB_TMR, FALSE);
  tmr_base_init(RGB_TMR, tmr_cycle_cnt, 0);

  /* configure channel 1 output settings */
  tmr_output_struct.oc_mode = TMR_OUTPUT_CONTROL_PWM_MODE_B;
  tmr_output_struct.oc_output_state = TRUE;
  tmr_output_struct.occ_output_state = FALSE;
  tmr_output_struct.oc_polarity = TMR_OUTPUT_ACTIVE_LOW;
  tmr_output_struct.occ_polarity = TMR_OUTPUT_ACTIVE_LOW;
  tmr_output_struct.oc_idle_state = TRUE;
  tmr_output_struct.occ_idle_state = FALSE;
  tmr_output_channel_config(RGB_TMR, RGB_TMR_CHANNLE, 
                            &tmr_output_struct);

  tmr_channel_value_set(RGB_TMR, RGB_TMR_CHANNLE, 0);
  tmr_output_channel_buffer_enable(RGB_TMR, RGB_TMR_CHANNLE, TRUE);
  tmr_output_channel_immediately_set(RGB_TMR, RGB_TMR_CHANNLE, FALSE);

  tmr_dma_request_enable(RGB_TMR, RGB_TMR_DMA_REQUEST, TRUE);
  tmr_output_enable(RGB_TMR, TRUE);
  tmr_counter_enable(RGB_TMR, TRUE);
}
#endif

#if USE_RGB_LED_BREATHING
void rgb_breathing_task(void)
{
  static uint16_t hue = 0;
  static uint16_t phase = 0;

  uint8_t r, g, b;
  uint8_t v;
  uint8_t led;

  static uint32_t rgb_refresh_led_time = 0;
  uint32_t rgb_cur_led_time = wk_timebase_get();

  if(((rgb_refresh_led_time + 10) > rgb_cur_led_time) || rgb_refresh_completed == 0)
  {
    return;
  }
  //207 us
  v = breathe_set_value(phase);
  phase += 2;
  
  hue += 1;
  if (hue >= 360) {
    hue = 0;
  }
  
  hsv_to_rgb(hue, v, &r, &g, &b);

  for (led = 0; led < RGB_LED_MAX; led ++) {
    rgb_led[led].r = r;
    rgb_led[led].g = g;
    rgb_led[led].b = b;
    ws2812_set_color(led, r, g, b);
  }
  ws2812_rgb_send();
  rgb_refresh_led_time = wk_timebase_get();
}

#endif

#if USE_RGB_LED_RUNNING
#define RUNING_LINE  15
static uint8_t running_table[RUNING_LINE][5] = 
{
  {62, 63, 64, 65, 66},
  {61, 34, 33, 0, 67},
  {60, 35, 32, 2, 1},
  {59, 36, 31, 3, 6},
  {58, 37, 30, 4, 10},
  {57, 38, 29, 5, 12},
  {56, 39, 28, 7, 14},
  {55, 40, 27, 8, 16},
  {54, 41, 26, 9, 17},
  {53, 42, 25, 11, 19},
  {52, 43, 24, 13, 255},
  {51, 44, 23, 15, 255},
  {50, 45, 22, 18, 255},
  {49, 46, 21, 20, 255},
  {48, 47, 255, 255, 255}
};

void rgb_running_light_task(void)
{
  static uint32_t rgb_refresh_led_time = 0;
  static uint16_t run_hue = 0;
  static uint8_t run_pos = 0;

  uint8_t r, g, b;
  uint8_t led;
  uint32_t rgb_cur_led_time = wk_timebase_get();
  
  /* 300 ms */
  if(((rgb_refresh_led_time + 300) > rgb_cur_led_time) || rgb_refresh_completed == 0)
  {
    return;
  }
  
  //240us
  for (led = 0; led < RGB_LED_MAX; led ++) {
    ws2812_set_color(led, 55, 0, 55);
  }
 
  hsv_to_rgb(run_hue, 255, &r, &g, &b);

  ws2812_set_color(running_table[run_pos][0], r, g, b);
  ws2812_set_color(running_table[run_pos][1], r, g, b);
  ws2812_set_color(running_table[run_pos][2], r, g, b);
  ws2812_set_color(running_table[run_pos][3], r, g, b);
  ws2812_set_color(running_table[run_pos][4], r, g, b);
  
  ws2812_set_color(running_table[run_pos+1][0], r, g, b);
  ws2812_set_color(running_table[run_pos+1][1], r, g, b);
  ws2812_set_color(running_table[run_pos+1][2], r, g, b);
  ws2812_set_color(running_table[run_pos+1][3], r, g, b);
  ws2812_set_color(running_table[run_pos+1][4], r, g, b);
  
  ws2812_set_color(running_table[run_pos+2][0], r, g, b);
  ws2812_set_color(running_table[run_pos+2][1], r, g, b);
  ws2812_set_color(running_table[run_pos+2][2], r, g, b);
  ws2812_set_color(running_table[run_pos+2][3], r, g, b);
  ws2812_set_color(running_table[run_pos+2][4], r, g, b);
  
  run_pos = (run_pos + 3) % RUNING_LINE;
  run_hue = (run_hue + 8) % 360;
  
  ws2812_rgb_send();
  rgb_refresh_led_time = wk_timebase_get();
}
#endif

void rgb_test(void)
{
  uint8_t led;
  
  LED_RGB_ON();
  
  for (led = 0; led < RGB_LED_MAX; led ++) {
    ws2812_set_color(led, 255, 0, 0);
  }
  ws2812_rgb_send();
  
  for (led = 0; led < RGB_LED_MAX; led ++) {
    ws2812_set_color(led, 0, 255, 0);
  }
  ws2812_rgb_send();
  
  for (led = 0; led < RGB_LED_MAX; led ++) {
    ws2812_set_color(led, 0, 0, 255);
  }
  ws2812_rgb_send();
  
  for (led = 0; led < RGB_LED_MAX; led ++) {
    ws2812_set_color(led, 0, 0, 0);
  }
}

void rgb_led_init(void)
{
  gpio_init_type gpio_init_struct;
  
  crm_periph_clock_enable(RGB_CTRL_GPIO_CLK, TRUE);
  
  /* led rgb on/off control init */
  gpio_default_para_init(&gpio_init_struct);
  gpio_init_struct.gpio_pins = RGB_CTRL_PIN;
  gpio_init_struct.gpio_mode = GPIO_MODE_OUTPUT;
  gpio_init_struct.gpio_out_type = GPIO_OUTPUT_OPEN_DRAIN;
  gpio_init_struct.gpio_pull = GPIO_PULL_NONE;
  gpio_init_struct.gpio_drive_strength = GPIO_DRIVE_STRENGTH_MODERATE;
  gpio_init(RGB_CTRL_GPIO, &gpio_init_struct);
  
  /* default rgb off */
  LED_RGB_OFF();
  
  /* ws2812 rgb in config*/
#if WS2812_CONTROL_USE_TMR
  ws2812_tmr_control_init();
  ws2812_reset();
#endif

#if WS2812_CONTROL_USE_SPI
  ws2812_spi_control_init();
  ws2812_reset();
#endif

#if 0
  /* rgb test */
  rgb_test();
#endif
}


void rgb_led_task(void)
{
  static uint8_t rgb_led_suspend = 1;
 
  if(usbd_is_keyboard_suspend())
  {
    if(rgb_led_suspend == 0)
    {
      rgb_led_suspend = 1;
      LED_RGB_OFF();
    }
    return;
  }
  if (rgb_led_suspend) {
    LED_RGB_ON();
    rgb_led_suspend = 0;
  }

#if USE_RGB_LED_BREATHING
  if(rgb_refresh_mode == 0)
    rgb_breathing_task();
#endif

#if USE_RGB_LED_RUNNING
  if(rgb_refresh_mode == 1)
    rgb_running_light_task();
#endif
  return;
}




