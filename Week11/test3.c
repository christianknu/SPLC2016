// Micro-C test3.c -- basic pointers
// Should print
// 7 -42  then throw exception

int arr[5];
int x;

void main() {
  int *p;
  p = &arr[2];
  read *p;
  write arr[2];
  arr[1+2] = -42;
  write *(p+1);
  bool *q;	// Uninitialized pointer, happens to be address -42
  *q = 1 < 2;   // Index error here
  write *q;
  write arr[0];
}
