// MicroC test1.c -- a loop
// Should print 
// 142 10132

int x;

void main() {
  read x;
  int i;
  while (x < 10000) {
    i = i + 1;
    x = x + i;
  }
  write i;
  write x;
}
