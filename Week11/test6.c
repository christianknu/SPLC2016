// Micro-C test6.c -- Fibonacci
// Returning result via pointer, recursive procedure
// Should print
// 610

void main() {
  int res;
  fib(14, &res);
  write res;
}

void fib(int n, int *res) {
  if (n < 2)
    *res = 1;
  else {
    int tmp;
    fib(n-1, &tmp);
    *res = tmp;
    fib(n-2, &tmp);
    *res = *res + tmp;
  }
}
