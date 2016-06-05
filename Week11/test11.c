// Micro-C test11.c -- multiple assignments
// Should print
// 3 3 3 16 4 12 4

void main() {
  int x; int y; int z;
  x = y = z = 1+2;
  write x;
  write y;
  write z;
  x = (y = y + 1) * y;
  write x;
  write y;
  x = z * (z = z + 1);
  write x;
  write z;
}
