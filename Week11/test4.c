// Micro-C test4.c -- scopes
// Should print
// 12 13 12

int x;

void main() {
  x = 12;
  write x;
  {
    int x;
    x = 13;
    write x;
  }
  write x;
}
