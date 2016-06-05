// Micro-C Exercise11.c -- code for exercise 1.1
// Should print
// 4

void main() {
    int x;
    foo(3, &x);
    write x;
}

void foo(int a, int *p) {
    int *q;
    q = p;
    int y;
    y = 0;
    *q = a+1;
    q = &y;
}
