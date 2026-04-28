#ifndef _WS2812_H

#define _WS2812_H

#ifdef __cplusplus
extern "C" {
#endif
#include "platform.h"

#if defined (__CC_ARM)
 #pragma anon_unions
#endif

typedef enum
{
  COLOR_UP = 0,
  COLOR_DOWN,
}RGB_BREATH_DIR;

typedef enum
{
  RGB_RED = 0,
  RGB_GREEN,
  RGB_BLUE,
  RGB_PURPLE,
  RGB_YELLOW,
  RGB_UNKONW
}RGB_COLOR_SELECT;

typedef struct
{
  uint8_t r;
  uint8_t g;
  uint8_t b;

  RGB_BREATH_DIR dir;
  RGB_COLOR_SELECT stat;
}rgb_led_t;

void ws2812_init(void);
void ws2812_send(uint32_t rgb);
void ws2812_breathing(uint8_t id);
void ws2812_led_task(void);

void rgb_led_init(void);
void rgb_led_task(void);

#ifdef __cplusplus
}
#endif

#endif





