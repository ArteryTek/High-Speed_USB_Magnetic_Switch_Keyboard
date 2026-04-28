/**
  **************************************************************************
  * @file     keyboard_class.c
  * @brief    usb hid keyboard class type
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
#include "usbd_core.h"
#include "keyboard_class.h"
#include "keyboard_desc.h"
#include "keyboard.h"
#include "keyboard_usb.h"

/** @addtogroup AT32F402_405_middlewares_usbd_class
  * @{
  */

/** @defgroup USB_keyboard_class
  * @brief usb device keyboard demo
  * @{
  */

/** @defgroup USB_keyboard_class_private_functions
  * @{
  */

static usb_sts_type class_init_handler(void *udev);
static usb_sts_type class_clear_handler(void *udev);
static usb_sts_type class_setup_handler(void *udev, usb_setup_type *setup);
static usb_sts_type class_ept0_tx_handler(void *udev);
static usb_sts_type class_ept0_rx_handler(void *udev);
static usb_sts_type class_in_handler(void *udev, uint8_t ept_num);
static usb_sts_type class_out_handler(void *udev, uint8_t ept_num);
static usb_sts_type class_sof_handler(void *udev);
static usb_sts_type class_event_handler(void *udev, usbd_event_type event);

static usb_sts_type class_setup_handler_keyboard(void *udev, usb_setup_type *setup);
static usb_sts_type class_setup_handler_iap(void *udev, usb_setup_type *setup);

keyboard_type keyboard_struct;
iap_info_type *iap_info = (iap_info_type *)&keyboard_struct.iap;

/* usb device class handler */
usbd_class_handler keyboard_class_handler =
{
  class_init_handler,
  class_clear_handler,
  class_setup_handler,
  class_ept0_tx_handler,
  class_ept0_rx_handler,
  class_in_handler,
  class_out_handler,
  class_sof_handler,
  class_event_handler,
  &keyboard_struct
};

/**
  * @brief  initialize usb endpoint
  * @param  udev: to the structure of usbd_core_type
  * @retval status of usb_sts_type
  */
static usb_sts_type class_init_handler(void *udev)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;

  /* open hid in endpoint */
  usbd_ept_open(pudev, USBD_KEYBOARD_IN_EPT, EPT_INT_TYPE, USBD_KEYBOARD_IN_MAXPACKET_SIZE);
  
  usbd_ept_open(pudev, USBD_HIDIAP_IN_EPT, EPT_INT_TYPE, USBD_KEYBOARD_IN_MAXPACKET_SIZE);
  usbd_ept_open(pudev, USBD_HIDIAP_OUT_EPT, EPT_INT_TYPE, USBD_KEYBOARD_OUT_MAXPACKET_SIZE);

  pkeyboard->g_u8tx_completed = 1;
  pkeyboard->send_state = 0;
  pkeyboard->start_report = 0;
  
  pkeyboard->iap.send_state = 0;
  
  /* set iap out endpoint to receive status */
  usbd_ept_recv(pudev, USBD_HIDIAP_OUT_EPT, pkeyboard->iap.g_rxhid_buff, USBD_KEYBOARD_OUT_MAXPACKET_SIZE);

  return status;
}

/**
  * @brief  clear endpoint or other state
  * @param  udev: to the structure of usbd_core_type
  * @retval status of usb_sts_type
  */
static usb_sts_type class_clear_handler(void *udev)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;

  /* close hid in endpoint */
  usbd_ept_close(pudev, USBD_KEYBOARD_IN_EPT);
  
  /* close hid iap  in endpoint */
  usbd_ept_close(pudev, USBD_HIDIAP_IN_EPT);

  /* close hid iap  out endpoint */
  usbd_ept_close(pudev, USBD_HIDIAP_OUT_EPT);

  return status;
}

/**
  * @brief  usb device class setup request handler
  * @param  udev: to the structure of usbd_core_type
  * @param  setup: setup packet
  * @retval status of usb_sts_type
  */
static usb_sts_type class_setup_handler(void *udev, usb_setup_type *setup)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;
  switch(setup->bmRequestType & USB_REQ_RECIPIENT_MASK)
  {
    case USB_REQ_RECIPIENT_INTERFACE:
      if(setup->wIndex == KEYBOARD_NUM)
      {
        pkeyboard->interface_number = KEYBOARD_NUM;
        class_setup_handler_keyboard(pudev, setup);
      }
      else if(setup->wIndex == IAP_NUM)
      {
        pkeyboard->interface_number = IAP_NUM;
        class_setup_handler_iap(pudev, setup);
      }
      else
      {
        status = USB_NOT_SUPPORT;
        usbd_ctrl_unsupport(pudev);
      }
      break;
    case USB_REQ_RECIPIENT_ENDPOINT:
      break;
    case USB_REQ_RECIPIENT_DEVICE:
      switch(pudev->setup.bRequest)
      {
        case USB_STD_REQ_CLEAR_FEATURE:
        case USB_STD_REQ_SET_FEATURE:
          break;
        default:
          usbd_ctrl_unsupport(pudev);
          break;
      }
      break;
    default:
      usbd_ctrl_unsupport(pudev);
      break;

  }
  return status;
}

/**
  * @brief  usb device class setup request handler
  * @param  udev: to the structure of usbd_core_type
  * @param  setup: setup packet
  * @retval status of usb_sts_type
  */
static usb_sts_type class_setup_handler_keyboard(void *udev, usb_setup_type *setup)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;
  uint16_t len;
  uint8_t *buf;

  switch(setup->bmRequestType & USB_REQ_TYPE_RESERVED)
  {
    /* class request */
    case USB_REQ_TYPE_CLASS:
      switch(setup->bRequest)
      {
        case HID_REQ_SET_PROTOCOL:
          pkeyboard->hid_protocol = (uint8_t)setup->wValue;
          break;
        case HID_REQ_GET_PROTOCOL:
          usbd_ctrl_send(pudev, (uint8_t *)&pkeyboard->hid_protocol, 1);
          break;
        case HID_REQ_SET_IDLE:
          pkeyboard->hid_set_idle = (uint8_t)(setup->wValue >> 8);
          break;
        case HID_REQ_GET_IDLE:
          usbd_ctrl_send(pudev, (uint8_t *)&pkeyboard->hid_set_idle, 1);
          break;
        case HID_REQ_SET_REPORT:
          pkeyboard->hid_state = HID_REQ_SET_REPORT;
          usbd_ctrl_recv(pudev, pkeyboard->hid_set_report, setup->wLength);
          break;
        default:
          usbd_ctrl_unsupport(pudev);
          break;
      }
      break;
    /* standard request */
    case USB_REQ_TYPE_STANDARD:
      switch(setup->bRequest)
      {
        case USB_STD_REQ_GET_DESCRIPTOR:
          if(setup->wValue >> 8 == HID_REPORT_DESC)
          {
            len = MIN(USBD_KEYBOARD_SIZ_REPORT_DESC, setup->wLength);
            buf = (uint8_t *)g_usbd_keyboard_report;
            usbd_ctrl_send(pudev, (uint8_t *)buf, len);
          }
          else if(setup->wValue >> 8 == HID_DESCRIPTOR_TYPE)
          {
            len = MIN(9, setup->wLength);
            buf = (uint8_t *)g_keyboard_usb_desc;
            usbd_ctrl_send(pudev, (uint8_t *)buf, len);
          }
          else
          {
            usbd_ctrl_unsupport(pudev);
          }
          break;
        case USB_STD_REQ_GET_INTERFACE:
          usbd_ctrl_send(pudev, (uint8_t *)&pkeyboard->alt_setting, 1);
          break;
        case USB_STD_REQ_SET_INTERFACE:
          pkeyboard->alt_setting = setup->wValue;
          break;
        case USB_STD_REQ_CLEAR_FEATURE:
          break;
        case USB_STD_REQ_SET_FEATURE:
          break;
        default:
          usbd_ctrl_unsupport(pudev);
          break;
      }
      break;
    default:
      usbd_ctrl_unsupport(pudev);
      break;
  }
  return status;
}

static usb_sts_type class_setup_handler_iap(void *udev, usb_setup_type *setup)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;
  iap_info_type *piap = &pkeyboard->iap;
  uint16_t len;
  uint8_t *buf;

  switch(setup->bmRequestType & USB_REQ_TYPE_RESERVED)
  {
    /* class request */
    case USB_REQ_TYPE_CLASS:
      switch(setup->bRequest)
      {
        case HID_REQ_SET_PROTOCOL:
          piap->hid_protocol = (uint8_t)setup->wValue;
          break;
        case HID_REQ_GET_PROTOCOL:
          usbd_ctrl_send(pudev, (uint8_t *)&piap->hid_protocol, 1);
          break;
        case HID_REQ_SET_IDLE:
          piap->hid_set_idle = (uint8_t)(setup->wValue >> 8);
          break;
        case HID_REQ_GET_IDLE:
          usbd_ctrl_send(pudev, (uint8_t *)&piap->hid_set_idle, 1);
          break;
        case HID_REQ_SET_REPORT:
          piap->hid_state = HID_REQ_SET_REPORT;
          usbd_ctrl_recv(pudev, piap->hid_set_report, setup->wLength);
          break;
        default:
          usbd_ctrl_unsupport(pudev);
          break;
      }
      break;
    /* standard request */
    case USB_REQ_TYPE_STANDARD:
      switch(setup->bRequest)
      {
        case USB_STD_REQ_GET_DESCRIPTOR:
          if(setup->wValue >> 8 == HID_REPORT_DESC)
          {
            len = MIN(USBD_HIDIAP_SIZ_REPORT_DESC, setup->wLength);
            buf = (uint8_t *)g_usbd_hidiap_report;
            usbd_ctrl_send(pudev, (uint8_t *)buf, len);
          }
          else if(setup->wValue >> 8 == HID_DESCRIPTOR_TYPE)
          {
            len = MIN(9, setup->wLength);
            buf = (uint8_t *)g_hidiap_usb_desc;
            usbd_ctrl_send(pudev, (uint8_t *)buf, len);
          }
          else
          {
            usbd_ctrl_unsupport(pudev);
          }
          break;
        case USB_STD_REQ_GET_INTERFACE:
          usbd_ctrl_send(pudev, (uint8_t *)&piap->alt_setting, 1);
          break;
        case USB_STD_REQ_SET_INTERFACE:
          piap->alt_setting = setup->wValue;
          break;
        case USB_STD_REQ_CLEAR_FEATURE:
          break;
        case USB_STD_REQ_SET_FEATURE:
          break;
        default:
          usbd_ctrl_unsupport(pudev);
          break;
      }
      break;
    default:
      usbd_ctrl_unsupport(pudev);
      break;
  }
  return status;
}

/**
  * @brief  usb device class endpoint 0 in status stage complete
  * @param  udev: to the structure of usbd_core_type
  * @retval status of usb_sts_type
  */
static usb_sts_type class_ept0_tx_handler(void *udev)
{
  usb_sts_type status = USB_OK;

  /* ...user code... */

  return status;
}

/**
  * @brief  usb device class endpoint 0 out status stage complete
  * @param  udev: to the structure of usbd_core_type
  * @retval status of usb_sts_type
  */
static usb_sts_type class_ept0_rx_handler(void *udev)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;
  uint32_t recv_len = usbd_get_recv_len(pudev, 0);
  /* ...user code... */
  if(pkeyboard->interface_number == KEYBOARD_NUM)
  {
    if( pkeyboard->hid_state == HID_REQ_SET_REPORT)
    {
      pkeyboard->hid_state = 0;
    }
  }
  else if(pkeyboard->interface_number == IAP_NUM)
  {
    if( pkeyboard->iap.hid_state == HID_REQ_SET_REPORT)
    {
      pkeyboard->iap.hid_state = 0;
    }
  }
  
  return status;
}

/**
  * @brief  usb device class transmision complete handler
  * @param  udev: to the structure of usbd_core_type
  * @param  ept_num: endpoint number
  * @retval status of usb_sts_type
  */
static usb_sts_type class_in_handler(void *udev, uint8_t ept_num)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;

  /* ...user code...
    trans next packet data
  */
  if((ept_num & 0x7F) == (USBD_KEYBOARD_IN_EPT & 0x7F))
  {
    pkeyboard->g_u8tx_completed = 1;
    pkeyboard->send_state = 0;
    pkeyboard->start_report = 1;
  }
  
  if((ept_num & 0x7F) == (USBD_HIDIAP_IN_EPT & 0x7F))
  {
    pkeyboard->iap.send_state = 0;
    if(pkeyboard->iap.state == IAP_STS_OTHER) {
      usbd_keyboard_param_in_complete(udev);
    } else {
      usbd_hid_iap_in_complete(udev);
    }
  }
  return status;
}

/**
  * @brief  usb device class endpoint receive data
  * @param  udev: to the structure of usbd_core_type 
  * @param  ept_num: endpoint number
  * @retval status of usb_sts_type
  */
static usb_sts_type class_out_handler(void *udev, uint8_t ept_num)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;
  uint32_t recv_len;
  
  if((ept_num & 0x7F) == USBD_HIDIAP_OUT_EPT)
  {
    /* get endpoint receive data length  */
    recv_len = usbd_get_recv_len(pudev, ept_num);
    
    if(pkeyboard->iap.g_rxhid_buff[0] == KEYBOARD_CMD) {
      usbd_keyboard_param_process(udev, pkeyboard->iap.g_rxhid_buff, recv_len);
    }
    else {
      /* hid iap process */
      usbd_hid_iap_process(udev, pkeyboard->iap.g_rxhid_buff, recv_len);
    }

    /* start receive next packet */
    usbd_ept_recv(pudev, USBD_HIDIAP_OUT_EPT, pkeyboard->iap.g_rxhid_buff, USBD_KEYBOARD_OUT_MAXPACKET_SIZE);
  }

  return status;
}

/**
  * @brief  usb device class sof handler
  * @param  udev: to the structure of usbd_core_type
  * @retval status of usb_sts_type
  */
static usb_sts_type class_sof_handler(void *udev)
{
  usb_sts_type status = USB_OK;

  /* ...user code... */
  usbd_keyboard_sof_callback();

  return status;
}

/**
  * @brief  usb device class event handler
  * @param  udev: to the structure of usbd_core_type
  * @param  event: usb device event
  * @retval status of usb_sts_type
  */
static usb_sts_type class_event_handler(void *udev, usbd_event_type event)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;
  switch(event)
  {
    case USBD_RESET_EVENT:
      usbd_clear_keyboard_suspend();
      /* ...user code... */

      break;
    case USBD_SUSPEND_EVENT:
      pkeyboard->hid_suspend_flag = 1;
      usbd_set_keyboard_suspend();
      /* ...user code... */

      break;
    case USBD_WAKEUP_EVENT:
      usbd_clear_keyboard_suspend();
      /* ...user code... */

      break;
    default:
      break;
  }
  return status;
}

/**
  * @brief  usb device class send report
  * @param  udev: to the structure of usbd_core_type
  * @param  report: report buffer
  * @param  len: report length
  * @retval status of usb_sts_type
  */
usb_sts_type usb_keyboard_class_send_report(void *udev, uint8_t *report, uint16_t len)
{
  usb_sts_type status = USB_FAIL;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;
  
  if(usbd_connect_state_get(pudev) == USB_CONN_STATE_CONFIGURED && pkeyboard->send_state == 0)
  {
    pkeyboard->send_state = 1;
    usbd_ept_send(pudev, USBD_KEYBOARD_IN_EPT, report, len);
    status = USB_OK;
  }

  return status;
}


/**
  * @brief  usb device class send report
  * @param  udev: to the structure of usbd_core_type
  * @param  report: report buffer
  * @param  len: report length
  * @retval status of usb_sts_type
  */
usb_sts_type usb_iap_class_send_report(void *udev, uint8_t *report, uint16_t len)
{
  usb_sts_type status = USB_OK;
  usbd_core_type *pudev = (usbd_core_type *)udev;
  keyboard_type *pkeyboard = (keyboard_type *)pudev->class_handler->pdata;
  if((usbd_connect_state_get(pudev) == USB_CONN_STATE_CONFIGURED) && (pkeyboard->iap.send_state == 0))
  {
    pkeyboard->iap.send_state = 1;
    usbd_ept_send(pudev, USBD_HIDIAP_IN_EPT, report, len);
  }

  return status;
}


/**
  * @}
  */

/**
  * @}
  */

/**
  * @}
  */

