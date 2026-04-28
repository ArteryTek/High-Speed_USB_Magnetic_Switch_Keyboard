#include "keyboard_filter.h"

#if USE_KALMAN2_FILTER
int32_t kalman2_update(rt_key_t *k, int32_t z)
{
  const int32_t Qp = 1;
  const int32_t Qv = 4;
  const int32_t R  = 30;
  const int32_t P_MAX = 2000;
  const int32_t V_MAX = 100 << 8;

  if (!k->inited) {
    k->pos = z << 8;
    k->vel = 0;
    k->Pp = 100;
    k->Pv = 100;
    k->inited = 1;
    return z;
  }

  k->pos += k->vel;
  k->Pp += Qp;
  k->Pv += Qv;

  if (k->Pp > P_MAX) k->Pp = P_MAX;
  if (k->Pv > P_MAX) k->Pv = P_MAX;

  int32_t S = k->Pp + R;
  if (S <= 0) S = 1;

  int32_t Kp = (k->Pp << 8) / S;
  int32_t Kv = (k->Pv << 8) / S;
  int32_t e = (z << 8) - k->pos;


  k->pos += ((Kp * e) >> 8);
  k->vel += ((Kv * e) >> 8);

  if (k->vel > V_MAX)  k->vel = V_MAX;
  if (k->vel < -V_MAX) k->vel = -V_MAX;

  k->Pp = ((256 - Kp) * k->Pp) >> 8;
  k->Pv = ((256 - Kv) * k->Pv) >> 8;
  
  if (k->pos < 0) k->pos = 0;
  if (k->pos > (4095 << 8)) k->pos = 4095 << 8;

  return (k->pos >> 8);
}
#endif

#if USE_KALMAN1_FILTER
int32_t kalman1_update(rt_key_t *k, int32_t z)
{
  const int32_t Q = 2;
  const int32_t R = 30;
  const int32_t P_MAX = 1000;

  if (!k->inited) {
    k->pos = z << 8;
    k->Pp = 100;
    k->inited = 1;
    return z;
  }

  k->Pp += Q;
  if (k->Pp > P_MAX) k->Pp = P_MAX;

  int32_t S = k->Pp + R;
  int32_t Kp = (k->Pp << 8) / S;
  int32_t e = (z << 8) - k->pos;

  k->pos += ((Kp * e) >> 8);
  k->Pp = ((256 - Kp) * k->Pp) >> 8;

  if (k->pos < 0) k->pos = 0;
  if (k->pos > (4095 << 8)) k->pos = 4095 << 8;

  return (k->pos >> 8);
}
#endif
