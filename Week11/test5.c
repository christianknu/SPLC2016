// Micro-C test5.c -- factorial
// Returning result via pointer, recursive procedure
// Should print
// 6

int x;

void main() {
  x = 3;
  int res;
  fac(x, &res);
  write res;
}

void fac(int n, int *res) {
  if (n == 0)
    *res = 1;
  else {
    int tmp;
    fac(n-1, &tmp);
    *res = n * tmp;
  }
}
