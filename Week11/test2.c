// Micro-C test2.c -- global and local variables
// Should print
// 7 9 13 0 3 0 6 2

int arr[5];
int x;

void main() {
  int i;
  i = 0;
  read x;
  write x;
  while (x != 0) {
    i = i + 1;
    read x;
    write x;
  }
  write i;
  write x;
  write &x;
  write &arr[2];
}
