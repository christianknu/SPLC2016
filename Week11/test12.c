// Micro-C test12.c -- pointer for return value
// Should print 
// 25 390625

void main() {
  int res;
  square(5, &res);
  write res;
  square(res, &res);
  square(res, &res);  
  write res;
}

void square(int x, int *p) { 
  *p = x*x;
}
