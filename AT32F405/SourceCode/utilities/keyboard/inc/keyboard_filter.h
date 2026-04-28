#ifndef _KEYBOARD_FILTER_H
#define _KEYBOARD_FILTER_H
#ifdef __cplusplus
extern "C" {
#endif
#include "keyboard.h"

#define MATRIX_EMA_ALPHA_EXPONENT    4
#define EMA(x, y) \
  (((uint32_t)(x) + \
    ((uint32_t)(y) * ((1 << MATRIX_EMA_ALPHA_EXPONENT) - 1))) >> \
   MATRIX_EMA_ALPHA_EXPONENT)

#if USE_KALMAN2_FILTER
int32_t kalman2_update(rt_key_t *k, int32_t z);
#endif

#if USE_KALMAN1_FILTER
int32_t kalman1_update(rt_key_t *k, int32_t z);
#endif

#ifdef __cplusplus
}
#endif

#endif
