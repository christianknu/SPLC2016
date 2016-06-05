// Micro-C test7.c -- argument passing
// Should print 
// 42 142 242 342 442 1210 1210

void main() {
  int res;
  int x;
  x = 42;
  foo(x, 100+x, x+200, &res, 300+x, x+400);
  write res;
}

void foo(int n1, int n2, int n3, int *p, int n4, int n5) {
  write n1;
  write n2;
  write n3;
  write n4;
  write n5;
  *p = n1+n2+n3+n4+n5;
  write *p;
}
