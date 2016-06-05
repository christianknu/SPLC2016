// Micro-C test14.c -- pointer example
// Prints 47

void main() {
  int *a;
  int x; 
  int y; 
  x = 0;
  y = 0;
  a = &x;
  *a = 12;
  *(a+1) = 47;
  write y;
}