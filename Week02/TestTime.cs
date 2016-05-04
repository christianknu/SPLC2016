using System;

namespace SPLC_Spring_2016
{
    class TestTime
    {
        public static void Main()
        {
            //############## EXERCISE 1 ##############
            /*
            Time myTime = new Time(13, 54);

            //Console.WriteLine("Total minutes " + myTime.ToString());
            Console.WriteLine("Total hours: {0}", myTime.Hour);
            Console.WriteLine("Remaining minutes: {0}", myTime.Minute);
            Console.WriteLine("Time representation: {0}", myTime.ToString());

            Time t1 = new Time(9, 30);
            Console.WriteLine(t1 + new Time(1, 15));
            Console.WriteLine(t1 - new Time(1, 15));

            Time t2 = new Time(9, 30);
            Time t3 = 120; // Two hours
            int m1 = (int)t2;
            Console.WriteLine("t1 = {0} and t2 = {1} and m1 = {2}", t2, t3, m1);
            
            //  My guess to why 't4' is legal, is because the interpreter will see the 't2' and think
            //  'Hey! It's a time!" and say okay, we will add it to something due to the '+'.
            //  As I have made a user-defined conversion operator '+' it will use that, and since that
            //  requires another Time object, it will create it, using the int value of 45. This is
            //  possible because the constructor of a Time struct, accepts just an integer.
            
            Time t4 = t2 + 45;
            Console.WriteLine("t4 = {0}", t4);
            */

            //############## EXERCISE 2.1 ##############
            /*
            Time t5 = new Time(9, 30);
            Time t6 = t5;
            t5.minutes = 100;
            Console.WriteLine("t5={0} and t6={1}", t5, t6);
            */

            //############## EXERCISE 2.2 ##############
            Time t1 = new Time(9, 30);
            Time t2 = new Time(9, 30);
            Time t3 = new Time(9, 30);
            TimeMethods.AddOneHour(t1);
            TimeMethods.AddOneHourByRef(ref t2);
            t3.AddOneHourInstance();
            Console.WriteLine("t1={0}, t2={1} and t3={2}", t1, t2, t3);
        }
    }

    public struct Time
    {
        public int minutes;

        public Time(int hh, int mm)
        {
            this.minutes = 60 * hh + mm;
        }

        public Time(int mm)
        {
            this.minutes = mm;
        }

        public override String ToString()
        {
            return String.Format("{0}:{1}", this.Hour, this.Minute);
        }

        public int Hour { get { return this.minutes / 60; } }

        public int Minute { get { return this.minutes - (60 * this.Hour); } }

        public static Time operator +(Time time1, Time time2)
        {
            return new Time(time1.minutes + time2.minutes);
        }

        public static Time operator -(Time time1, Time time2)
        {
            return new Time(time1.minutes - time2.minutes);
        }

        public static implicit operator Time(int time)
        {
            return new Time(time);
        }

        public static explicit operator int (Time timeStruct)
        {
            return timeStruct.minutes;
        }

        public void AddOneHourInstance()
        {
            this.minutes += 60;
        }
    }

    public class TimeMethods
    {
        public static void AddOneHour(Time t)
        {
            t.minutes += 60;
        }
        public static void AddOneHourByRef(ref Time t)
        {
            t.minutes += 60;
        }
    }
}

