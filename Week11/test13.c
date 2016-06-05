// Micro-C test13.c -- pointer example

void main() {
  int *p;
  int *q;
  int x;
  int y;
  x = 0;
  y = 1;
  p = &x;
  q = &y;
  *p = y;
  y = 2;
  write *p; // writes 1
  write *q; // writes 2
}