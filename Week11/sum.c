
void main() {
    int sump;
    int ns[4];
    ns[0] = 7;
    ns[1] = 13;
    ns[2] = 9;
    ns[3] = 8;
    sum(3, ns, &sump);
    write sump;
}

void sum(int n, int ns[], int *sump) {
    while(n > 0){
        n = n-1;
        *sump = *sump + ns[n];
    }
}
