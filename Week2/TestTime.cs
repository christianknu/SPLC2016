using System;

namespace SPLC_Spring_2016
{
    class TestTime
    {
        public static void Main() {
            Time myTime = new Time(13, 54);
            
            //Console.WriteLine("Total minutes " + myTime.ToString());
            Console.WriteLine("Total hours: {0}", myTime.Hour);
            Console.WriteLine("Remaining minutes: {0}", myTime.Minute);
            Console.WriteLine("Time representation: {0}", myTime.ToString());
            
            Time t1 = new Time(9,30);
            Console.WriteLine(t1 + new Time(1, 15));
            Console.WriteLine(t1 - new Time(1, 15));
            
            Time t2 = new Time(9,30);
            Time t3 = 120; // Two hours
            int m1 = (int)t2;
            Console.WriteLine("t1 = {0} and t2 = {1} and m1 = {2}", t2, t3, m1);
            /**
            *
            *
            **/
            Time t4 = t2 + 45;
            Console.WriteLine("t4 = {0}", t4);
        }
    }

    public struct Time {
        private readonly int minutes;
        
        public Time(int hh, int mm) {
            this.minutes = 60 * hh + mm;
        }
        
        public Time(int mm) {
            this.minutes = mm;
        }
        
        public override String ToString() {
            return String.Format("{0}:{1}", this.Hour, this.Minute);
        }
        
        public int Hour { get { return this.minutes / 60; } }
        
        public int Minute { get { return this.minutes - (60 * this.Hour) ;} }
        
        public static Time operator +(Time time1, Time time2){
            return new Time(time1.minutes + time2.minutes);
        }
        
        public static Time operator -(Time time1, Time time2){
            return new Time(time1.minutes - time2.minutes);
        }
        
        public static implicit operator Time(int time) {
            return new Time(time);
        }
        
        public static explicit operator int(Time timeStruct) {
            return timeStruct.minutes;
        }
    }
}

