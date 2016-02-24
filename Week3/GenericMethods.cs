namespace Week3
{
	struct Book
	{
		internal string title;
		internal string author;
		internal int year;

		public Book (string author, string title, int year) {
			this.title = title; this.year = year; this.author = author; }
			
		public override string ToString() {return author + ": " + title + ", " + year;}
	}
	
	public delegate int DComparer<T>(T v1, T v2);
    public delegate bool Func<T,TBoolean>(T myT);
    public delegate U Map<T,U>(T myT);
	
	static class GenericMethods {
	  	private static void Qsort<T>(T[] arr, DComparer<T> cmp, int a, int b) {
    		if (a < b) { 
      			int i = a, j = b;
  				T x = arr[(i+j) / 2];             
      			do {                              
        			while (cmp(arr[i], x) < 0) i++;     
        			while (cmp(x, arr[j]) < 0) j--;     
        			if (i <= j) {
          				T tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;    
          				i++; j--;                     
      				}                             
      			} while (i <= j);                 
      			Qsort<T>(arr, cmp, a, j);                 
      			Qsort<T>(arr, cmp, i, b);                 
    		}                                   
		}

		public static void Quicksort<T>(T[] arr, DComparer<T> cmp) {
			Qsort(arr, cmp, 0, arr.Length-1);
		}
        
        public static T[] Filter<T>(T[] arr, Func<T,bool> p) {
            int arrayLength = 0;
            int currentArrIndex = 0;
            
            foreach(var element in arr)
                if(p(element)) { arrayLength++; }
            var returnArr = new T[arrayLength];
            
            for(int i = 0; i < arr.Length; i++) {
                if(p(arr[i])) {
                    returnArr[currentArrIndex++] = arr[i];
                }
            }
            
            return returnArr;
        }
        
        public static U[] Map<T,U>(T[] arr, Map<T,U> f) {
             var returnArr = new U[arr.Length];
             for(int i = 0; i < arr.Length; i++) {
                 returnArr[i] = f(arr[i]);
             }
             return returnArr;
        }
	}
}

