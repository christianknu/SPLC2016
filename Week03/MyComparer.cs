namespace Week03
{
    delegate int Sorter(Book a, Book b);
    class MyComparer
    {
        public static int BookCompare(Book a, Book b, params Sorter[] sorters)
        {
            foreach (var sorter in sorters)
                if (sorter(a, b) != 0)
                    return sorter(a, b);
            return 0;
       }
    }
}
