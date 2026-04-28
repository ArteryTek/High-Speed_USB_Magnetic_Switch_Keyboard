#include "keyboard_power.h"
#include "wk_system.h"
#include "keyboard.h"

void suspend_sysclk_switch(crm_sclk_type sclk_type)
{
  /* select pll as system clock source */
  crm_sysclk_switch(sclk_type);

  /* wait till pll is used as system clock source */
  while(crm_sysclk_switch_status_get() != sclk_type)
  {
  }
}

void suspend_enter_lower_power(void)
{
  if(p_key->is_lowpower == FALSE)
  {
    __disable_irq();
    suspend_sysclk_switch(CRM_SCLK_HICK);
    wk_timebase_init();
    __enable_irq();
  }
  p_key->is_lowpower = TRUE;
}

void suspend_exit_lower_power(void)
{
  if(p_key->is_lowpower == TRUE)
  {
    __disable_irq();
    suspend_sysclk_switch(CRM_SCLK_PLL);
    wk_timebase_init();
    __enable_irq();
  }
  p_key->is_lowpower = FALSE;
}


