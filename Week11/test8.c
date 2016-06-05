// Micro-C test8.c -- arrays and array arguments
// Should print 
// 13

void main() {
  int arr[4];
  arr[0] = 7;
  arr[2] = 13;
  arr[1] = 9;
  arr[3] = 10;
  int res;
  max(arr, 4, &res);
  write res;
}

void max(int ns[], int n, int *out) {
  int i;
  i = 0;
  *out = -2147483647;
  while (i < n) {
    if (ns[i] > *out) 
      *out = ns[i];
    else
      { } 
    i = i+1;
  }
}
