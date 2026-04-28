/**
  **************************************************************************
  * @file     key_code.h
  * @brief    keyboard code define
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

#ifndef _KEY_CODE_H
#define _KEY_CODE_H

#ifdef __cplusplus
extern "C" {
#endif
#include "platform.h"

#if defined (__CC_ARM)
 #pragma anon_unions
#endif

#define HID_KEY_NONE                           0x00    // No key pressed
#define HID_KEY_A                              0x04    // Keyboard a and A
#define HID_KEY_B                              0x05    // Keyboard b and B
#define HID_KEY_C                              0x06    // Keyboard c and C
#define HID_KEY_D                              0x07    // Keyboard d and D
#define HID_KEY_E                              0x08    // Keyboard e and E
#define HID_KEY_F                              0x09    // Keyboard f and F
#define HID_KEY_G                              0x0A    // Keyboard g and G
#define HID_KEY_H                              0x0B    // Keyboard h and H
#define HID_KEY_I                              0x0C    // Keyboard i and I
#define HID_KEY_J                              0x0D    // Keyboard j and J
#define HID_KEY_K                              0x0E    // Keyboard k and K
#define HID_KEY_L                              0x0F    // Keyboard l and L
#define HID_KEY_M                              0x10    // Keyboard m and M
#define HID_KEY_N                              0x11    // Keyboard n and N
#define HID_KEY_O                              0x12    // Keyboard o and O
#define HID_KEY_P                              0x13    // Keyboard p and P
#define HID_KEY_Q                              0x14    // Keyboard q and Q
#define HID_KEY_R                              0x15    // Keyboard r and R
#define HID_KEY_S                              0x16    // Keyboard s and S
#define HID_KEY_T                              0x17    // Keyboard t and T
#define HID_KEY_U                              0x18    // Keyboard u and U
#define HID_KEY_V                              0x19    // Keyboard v and V
#define HID_KEY_W                              0x1A    // Keyboard w and W
#define HID_KEY_X                              0x1B    // Keyboard x and X
#define HID_KEY_Y                              0x1C    // Keyboard y and Y
#define HID_KEY_Z                              0x1D    // Keyboard z and Z

#define HID_KEY_1                              0x1E    // Keyboard 1 and !
#define HID_KEY_2                              0x1F    // Keyboard 2 and @
#define HID_KEY_3                              0x20    // Keyboard 3 and #
#define HID_KEY_4                              0x21    // Keyboard 4 and $
#define HID_KEY_5                              0x22    // Keyboard 5 and %
#define HID_KEY_6                              0x23    // Keyboard 6 and ^
#define HID_KEY_7                              0x24    // Keyboard 7 and &
#define HID_KEY_8                              0x25    // Keyboard 8 and *
#define HID_KEY_9                              0x26    // Keyboard 9 and (
#define HID_KEY_0                              0x27    // Keyboard 0 and )

#define HID_KEY_ENTER                          0x28    // Keyboard Enter
#define HID_KEY_ESCAPE                         0x29    // Keyboard Esc
#define HID_KEY_BACKSPACE                      0x2A    // Keyboard Backspace
#define HID_KEY_TAB                            0x2B    // Keyboard Tab
#define HID_KEY_SPACE                          0x2C    // Keyboard space
#define HID_KEY_MINUS                          0x2D    // Keyboard - and _
#define HID_KEY_EQUAL                          0x2E    // Keyboard = and +
#define HID_KEY_BRACKET_LEFT                   0x2F    // Keyboard [ and {
#define HID_KEY_BRACKET_RIGHT                  0x30    // Keyboard ] and }
#define HID_KEY_BACKSLASH                      0x31    // Keyboard \ and |
#define HID_KEY_EUROPE_1                       0x32    // Keyboard Non-US # and ~
#define HID_KEY_SEMICOLOW                      0x33    // Keyboard ; and :
#define HID_KEY_APOSTROPHE                     0x34    // Keyboard ' and "
#define HID_KEY_GRAVE                          0x35    // Keyboard ` and ~
#define HID_KEY_COMMA                          0x36    // Keyboard , and <
#define HID_KEY_DOT                            0x37    // Keyboard . and >
#define HID_KEY_SLASH                          0x38    // Keyboard / and ?
#define HID_KEY_CAPSLOCK                       0x39    // Keyboard Caps Lock

#define HID_KEY_F1                             0x3A    // Keyboard F1
#define HID_KEY_F2                             0x3B    // Keyboard F2
#define HID_KEY_F3                             0x3C    // Keyboard F3
#define HID_KEY_F4                             0x3D    // Keyboard F4
#define HID_KEY_F5                             0x3E    // Keyboard F5
#define HID_KEY_F6                             0x3F    // Keyboard F6
#define HID_KEY_F7                             0x40    // Keyboard F7
#define HID_KEY_F8                             0x41    // Keyboard F8
#define HID_KEY_F9                             0x42    // Keyboard F9
#define HID_KEY_F10                            0x43    // Keyboard F10
#define HID_KEY_F11                            0x44    // Keyboard F11
#define HID_KEY_F12                            0x45    // Keyboard F12

#define HID_KEY_PRINT_SCRN                     0x46    // Keyboard Print Screen
#define HID_KEY_SCROLL_LOCK                    0x47    // Keyboard Scroll Lock
#define HID_KEY_PAUSE                          0x48    // Keyboard Pause
#define HID_KEY_INSERT                         0x49    // Keyboard Insert
#define HID_KEY_HOME                           0x4A    // Keyboard Home
#define HID_KEY_PAGEUP                         0x4B    // Keyboard Page Up
#define HID_KEY_DELETE                         0x4C    // Keyboard Delete
#define HID_KEY_END                            0x4D    // Keyboard End
#define HID_KEY_PAGEDOWN                       0x4E    // Keyboard Page Down
#define HID_KEY_RIGHT                          0x4F    // Keyboard Right
#define HID_KEY_LEFT                           0x50    // Keyboard Left
#define HID_KEY_DOWN                           0x51    // Keyboard Down
#define HID_KEY_UP                             0x52    // Keyboard Up

#define KEY_LEFT_CTRL                          0xE0    // Keyboard Left Control
#define KEY_LEFT_SHIFT                         0xE1    // Keyboard Left Shift
#define KEY_LEFT_ALT                           0xE2    // Keyboard Left Alt
#define KEY_LEFT_META                          0xE3    // Keyboard Left GUI
#define KEY_RIGHT_CTRL                         0xE4    // Keyboard Left Control
#define KEY_RIGHT_SHIFT                        0xE5    // Keyboard Left Shift
#define KEY_RIGHT_ALT                          0xE6    // Keyboard Left Alt
#define KEY_RIGHT_META                         0xE7    // Keyboard Left GUI


#define HID_KEY_TEST                           0

#define HID_MODIFIER_LEFT_CTRL                0x01
#define HID_MODIFIER_LEFT_SHIFT               0x02
#define HID_MODIFIER_LEFT_ALT                 0x04
#define HID_MODIFIER_LEFT_TGUI                0x08
#define HID_MODIFIER_RIGHT_CTRL               0x10
#define HID_MODIFIER_RIGHT_SHIFT              0x20
#define HID_MODIFIER_RIGHT_ALT                0x40
#define HID_MODIFIER_RIGHT_TGUI               0x80

typedef enum
{
  _KEY_A_ = 43,
  _KEY_B_ = 50,
  _KEY_C_ = 51,
  _KEY_D_ = 42,
  _KEY_E_ = 33,
  _KEY_F_ = 14,
  _KEY_G_ = 41,
  _KEY_H_ = 4,
  _KEY_I_ = 30,
  _KEY_J_ = 40,
  _KEY_K_ = 3,
  _KEY_L_ = 48,
  _KEY_M_ = 12,
  _KEY_N_ = 49,
  _KEY_O_ = 66,
  _KEY_P_ = 29,
  _KEY_Q_ = 70,
  _KEY_R_ = 23,
  _KEY_S_ = 15,
  _KEY_T_ = 68,
  _KEY_U_ = 67,
  _KEY_V_ = 5,
  _KEY_W_ = 24,
  _KEY_X_ = 6,
  _KEY_Y_ = 31,
  _KEY_Z_ = 52,
  
  _KEY_1_ = 25,
  _KEY_2_ = 34,
  _KEY_3_ = 69,
  _KEY_4_ = 60,
  _KEY_5_ = 32,
  _KEY_6_ = 59,
  _KEY_7_ = 22,
  _KEY_8_ = 58,
  _KEY_9_ = 21,
  _KEY_0_ = 57,
  _KEY_ENTER_     = 37,
  _KEY_ESCAPE_    = 26,
  _KEY_BACKSPACE_ = 64,
  _KEY_TAB_       = 35,
  _KEY_SPACE_     = 13,
  _KEY_MINUS_     = 65,
  _KEY_EQUAL_     = 19,
  _KEY_BRACKET_LEFT_  = 56,
  _KEY_BRACKET_RIGHT_ = 28,
  _KEY_BACKSLASH_     = 27,
  _KEY_SEMICOLOW_     = 2,
  _KEY_APOSTROPHE_    = 10,
  _KEY_COMMA_         = 39,
  _KEY_DOT_           = 20,
  _KEY_SLASH_         = 38,
  _KEY_CAPSLOCK_      = 71,
  _KEY_INSERT_        = 63,
  _KEY_PAGEUP_        = 45,
  _KEY_DELETE_        = 54,
  _KEY_PAGEDOWN_      = 36,
  _KEY_RIGHT_         = 0,
  _KEY_LEFT_          = 55,
  _KEY_DOWN_          = 9,
  _KEY_UP_            = 18,
  _LEFT_CTRL_         = 17,
  _LEFT_SHIFT_        = 62,
  _LEFT_ALT_          = 61,
  _LEFT_META_         = 8,
  _RIGHT_CTRL_        = 46,
  _RIGHT_SHIFT_       = 1,
  _RIGHT_ALT_         = 11,
  _RIGHT_META_        = 47,
}key_map_t;


extern const uint8_t adc_key_code[];

#ifdef __cplusplus
}
#endif
  
#endif
