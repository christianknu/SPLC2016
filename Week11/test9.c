// Micro-C test9.c -- mixing arrays and pointers

void main() {
  int n;		// An integer	
  int ia[3];		// Array of 3 integers
  int *p;		// Pointer to integer
  int *ipa[4];		// Array of 4 pointers to integers
  int (*iap)[3];	// Pointer to array of 3 integers
  int *(*ipap)[4];	// Pointer to array of 4 pointers to integers
  n = 42;
  ia[0] = 33;
  ia[1] = n;
  p = &ia[0];		// Pointer to ia[0]
  write p[1];		// Write ia[1], that is, 42

  ipa[0] = p+1;		// Pointer to ia[1]
  ipa[1] = &p[1];	// Same
  ipa[2] = p;		// Pointer to ia[0]
  ipa[3] = &n;          // Pointer to n

  n=0; 
  while (n<4) { 
    write *ipa[n];
    n=n+1;
  }
  iap = &ia;		// Pointer to the ia array
  n=0; 
  while (n<3) { 
    write (*iap)[n];
    n=n+1;
  }
  ipap = &ipa;		// Pointer to the ipa array 
  n=0; 
  while (n<4) { 
    write *(*ipap)[n];
    n=n+1;
  }
}
