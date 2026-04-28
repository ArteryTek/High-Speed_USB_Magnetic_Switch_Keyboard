#ifndef _PLATFORM_H
#define _PLATFORM_H
#ifdef __cplusplus
extern "C" {
#endif
#include "at32f402_405.h"
#include "stdio.h"
#include "string.h"

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

/* adc config */
#define KEY_ADC_CLK          CRM_ADC1_PERIPH_CLOCK
#define KEY_ADC              ADC1
#define KEY_ADC_DIV          ADC_DIV_8
#define KEY_ADC_IRQ          ADC1_IRQn
#define KEY_ADC_SAMPLE_RATE  ADC_SAMPLETIME_1_5
#define KEY_ADC_CHANNEL_MAX   9
#define KEY_ADC_KEY_NUM      (KEY_ADC_CHANNEL_MAX * 8)

/* adc dma config */
#define KEY_ADC_DT           ADC1->odt
#define KEY_ADC_DMA_CLK      CRM_DMA1_PERIPH_CLOCK
#define KEY_ADC_DMA          DMA1
#define KEY_ADC_DMA_CH       DMA1_CHANNEL1
#define KEY_ADC_DMA_IRQ      DMA1_Channel1_IRQn
#define KEY_ADC_DMACH_MUX    DMA1MUX_CHANNEL1
#define KEY_ADC_DMAREQ_ID    DMAMUX_DMAREQ_ID_ADC1
#define KEY_ADC_DMA_IRQ_HANDLER DMA1_Channel1_IRQHandler
#define KEY_ADC_DMA_FULL_FLAG     DMA1_FDT1_FLAG
#define KEY_ADC_DMA_HALF_FLAG     DMA1_HDT1_FLAG


/*adc pin config*/
#define ADC_CH1_GPIO_CLK      CRM_GPIOA_PERIPH_CLOCK
#define ADC_CH1_GPIO          GPIOA
#define ADC_CH1_PIN           GPIO_PINS_0

#define ADC_CH2_GPIO_CLK      CRM_GPIOA_PERIPH_CLOCK
#define ADC_CH2_GPIO          GPIOA
#define ADC_CH2_PIN           GPIO_PINS_1

#define ADC_CH3_GPIO_CLK      CRM_GPIOA_PERIPH_CLOCK
#define ADC_CH3_GPIO          GPIOA
#define ADC_CH3_PIN           GPIO_PINS_2

#define ADC_CH4_GPIO_CLK      CRM_GPIOA_PERIPH_CLOCK
#define ADC_CH4_GPIO          GPIOA
#define ADC_CH4_PIN           GPIO_PINS_3

#define ADC_CH5_GPIO_CLK      CRM_GPIOA_PERIPH_CLOCK
#define ADC_CH5_GPIO          GPIOA
#define ADC_CH5_PIN           GPIO_PINS_4

#define ADC_CH6_GPIO_CLK      CRM_GPIOA_PERIPH_CLOCK
#define ADC_CH6_GPIO          GPIOA
#define ADC_CH6_PIN           GPIO_PINS_5

#define ADC_CH7_GPIO_CLK      CRM_GPIOA_PERIPH_CLOCK
#define ADC_CH7_GPIO          GPIOA
#define ADC_CH7_PIN           GPIO_PINS_6

#define ADC_CH8_GPIO_CLK      CRM_GPIOA_PERIPH_CLOCK
#define ADC_CH8_GPIO          GPIOA
#define ADC_CH8_PIN           GPIO_PINS_7

#define ADC_CH9_GPIO_CLK      CRM_GPIOB_PERIPH_CLOCK
#define ADC_CH9_GPIO          GPIOB
#define ADC_CH9_PIN           GPIO_PINS_0

/* rgb led config */
#define RGB_LED_MAX           68
#define USE_RGB_LED_BREATHING 1
#define USE_RGB_LED_RUNNING   1

#define WS2812_CONTROL_USE_TMR        0
#define WS2812_CONTROL_USE_SPI        1

/* rgb control on/off pin */
#define RGB_CTRL_GPIO_CLK    CRM_GPIOC_PERIPH_CLOCK
#define RGB_CTRL_GPIO        GPIOC
#define RGB_CTRL_PIN         GPIO_PINS_11
#define LED_RGB_ON()         gpio_bits_reset(RGB_CTRL_GPIO, RGB_CTRL_PIN)
#define LED_RGB_OFF()        gpio_bits_set(RGB_CTRL_GPIO, RGB_CTRL_PIN)

/* rgb input */
#define RGB_IN_GPIO_CLK      CRM_GPIOC_PERIPH_CLOCK
#define RGB_IN_GPIO          GPIOC
#define RGB_IN_PIN           GPIO_PINS_12
#define RGB_IN_PINSOURCE     GPIO_PINS_SOURCE12

/* use timer pwm control ws2812 */
#if WS2812_CONTROL_USE_TMR
/* rgb input tmr mux */
#define RGB_IN_MUX           GPIO_MUX_3

/* ws2812 rgb timer control define */
#define RGB_TMR_CLK          CRM_TMR11_PERIPH_CLOCK
#define RGB_TMR              TMR11
#define RGB_TMR_CHANNLE      TMR_SELECT_CHANNEL_1
#define RGB_TMR_CxDT         TMR11->c1dt
#define RGB_TMR_DMA_REQUEST  TMR_C1_DMA_REQUEST
#define RGB_TMR_APB_BUS      clock_freq.apb2_freq

#define RGB_TMR_DMA_CLK      CRM_DMA1_PERIPH_CLOCK
#define RGB_TMR_DMA          DMA1
#define RGB_TMR_DMA_CH       DMA1_CHANNEL2
#define RGB_TMR_DMA_IRQ      DMA1_Channel2_IRQn
#define RGB_TMR_DMACH_MUX    DMA1MUX_CHANNEL2
#define RGB_TMR_DMAREQ_ID    DMAMUX_DMAREQ_ID_TMR11_CH1
#define RGB_TMR_DMA_IRQ_HANDLER DMA1_Channel2_IRQHandler
#define RGB_TMR_DMA_FLAG     DMA1_FDT2_FLAG
#endif

/* use spi control ws2812 */
#if WS2812_CONTROL_USE_SPI
/* rgb input spi mux */
#define RGB_IN_MUX           GPIO_MUX_6

/* ws2812 rgb timer control define */
#define RGB_SPI_CLK          CRM_SPI3_PERIPH_CLOCK
#define RGB_SPI              SPI3
#define RGB_SPI_DT           SPI3->dt
#define RGB_SPI_DMA_REQUEST  TMR_C1_DMA_REQUEST
#define RGB_SPI_MCLK_DIV     SPI_MCLK_DIV_32
#define RGB_SPI_APB_BUS      clock_freq.apb1_freq

#define RGB_SPI_DMA_CLK      CRM_DMA1_PERIPH_CLOCK
#define RGB_SPI_DMA          DMA1
#define RGB_SPI_DMA_CH       DMA1_CHANNEL2
#define RGB_SPI_DMA_IRQ      DMA1_Channel2_IRQn
#define RGB_SPI_DMACH_MUX    DMA1MUX_CHANNEL2
#define RGB_SPI_DMAREQ_ID    DMAMUX_DMAREQ_ID_SPI3_TX
#define RGB_SPI_DMA_IRQ_HANDLER DMA1_Channel2_IRQHandler
#define RGB_SPI_DMA_FLAG     DMA1_FDT2_FLAG

#endif

#if (WS2812_CONTROL_USE_SPI & WS2812_CONTROL_USE_TMR) || (!WS2812_CONTROL_USE_SPI & !WS2812_CONTROL_USE_TMR)
    #error "Please select one method SPI or timer to control ws2812 rgb led!"
#endif

#define USB_IRQ_PRIORITY     2
#define DMA_IRQ_PRIORITY     1
#define TASK_IRQ_PRIORITY    4
#define RGB_IRQ_PRIORITY     5

#ifdef __cplusplus
}
#endif

#endif
