// Micro-C test15.c -- an example with a 2-dim array

void main() {
  int (a[3])[2];
  int x;
  (a[0])[1] = 2;
  (a[1])[0] = 3;
  write (a[0])[1];
}