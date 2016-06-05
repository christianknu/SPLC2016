// Micro-C test10.c -- mutually recursive functions
// Should print
// 9 7 5 3 1 42

void main() {
  foo(10);
}

void foo(int n) {
  if (n!=0) 
    goo(n-1);
  else
    write 42;
}

void goo(int m, int z) {
  if (m!=0) {
    write m;
    foo(m-1);
  } else
    write 117;
}
