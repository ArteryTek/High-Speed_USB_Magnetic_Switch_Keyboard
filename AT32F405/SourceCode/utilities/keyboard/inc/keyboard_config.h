/**
  **************************************************************************
  * @file     key_config.h
  * @brief    keyboard config define
  **************************************************************************
  *                       Copyright notice & Disclaimer
  *
  * The software Board Support Package (BSP) that is made available to
  * download from Artery official website is the copyrighted work of Artery.
  * Artery authorizes customers to use, copy, and distribute the BSP
  * software and its related documentation for the purpose of design and
  * development in conjunction with Artery microcontrollers. Use of the
  * software is governed by this copyright notice and the following disclaimer.
  *
  * THIS SOFTWARE IS PROVIDED ON "AS IS" BASIS WITHOUT WARRANTIES,
  * GUARANTEES OR REPRESENTATIONS OF ANY KIND. ARTERY EXPRESSLY DISCLAIMS,
  * TO THE FULLEST EXTENT PERMITTED BY LAW, ALL EXPRESS, IMPLIED OR
  * STATUTORY OR OTHER WARRANTIES, GUARANTEES OR REPRESENTATIONS,
  * INCLUDING BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.
  *
  **************************************************************************
  */
#ifndef _KEY_CONFIG_H
#define _KEY_CONFIG_H

#ifdef __cplusplus
extern "C" {
#endif
#include "platform.h"
#include "key_code.h"

#if defined (__CC_ARM)
 #pragma anon_unions
#endif


/* mux control pin */
#define MUX_CS0_GPIO_CLK     CRM_GPIOC_PERIPH_CLOCK
#define MUX_CS0_GPIO         GPIOC
#define MUX_CS0              GPIO_PINS_13

#define MUX_CS1_GPIO_CLK     CRM_GPIOC_PERIPH_CLOCK
#define MUX_CS1_GPIO         GPIOC
#define MUX_CS1              GPIO_PINS_14

#define MUX_CS2_GPIO_CLK     CRM_GPIOC_PERIPH_CLOCK
#define MUX_CS2_GPIO         GPIOC
#define MUX_CS2              GPIO_PINS_15

#define RGB_CTRL_GPIO_CLK    CRM_GPIOC_PERIPH_CLOCK
#define RGB_CTRL_GPIO        GPIOC
#define RGB_CTRL_PIN         GPIO_PINS_11

#define RGB_ON()             gpio_bits_set(RGB_CTRL_GPIO, RGB_CTRL_PIN)
#define RGB_OFF()            gpio_bits_reset(RGB_CTRL_GPIO, RGB_CTRL_PIN)

#define ADC_CHNANEL_NUM      9
#define MAX_ANALOG_KEY       72

#define RANGE                0xA0
#define UP_RANGE             (RANGE / 2)
#define LEFT_SHIFT_OFFSET    9


#ifdef __cplusplus
}
#endif
  
#endif
