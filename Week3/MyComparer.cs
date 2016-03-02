namespace Week3
{
    class MyComparer
    {
       public static int BookCompare(Book a, Book b) {
           if(a.year != b.year) {
               return a.year - b.year;
           } else {
               return string.Compare(a.title, b.title);
           }
       }
    }
}
