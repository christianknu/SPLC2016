using System;

namespace SPLC_Spring_2016
{
    public interface IExpression
    {
        double Eval();
    }

    public abstract class CompositeExpression : IExpression
    {
        public IExpression leftArgument, rightArgument;

        public CompositeExpression(IExpression iExLeft, IExpression iExRight)
        {
            this.leftArgument = iExLeft;
            this.rightArgument = iExRight;
        }

        public abstract double Eval();
    }

    public class Constant : IExpression
    {
        private double value;

        public Constant(double val)
        {
            this.value = val;
        }

        public double Eval()
        {
            return this.value;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }
    }

    public class Sum : CompositeExpression
    {
        public Sum(IExpression iExLeft, IExpression iExRight) : base(iExLeft, iExRight) { }

        public override double Eval()
        {
            return leftArgument.Eval() + rightArgument.Eval();
        }

        public override string ToString()
        {
            return "(" + leftArgument.ToString() + " + " + rightArgument.ToString() + ")";
        }
    }

    public class Product : CompositeExpression
    {
        public Product(IExpression iExLeft, IExpression iExRight) : base(iExLeft, iExRight) { }

        public override double Eval()
        {
            return leftArgument.Eval() * rightArgument.Eval();
        }

        public override string ToString()
        {
            return leftArgument.ToString() + " * " + rightArgument.ToString();
        }
    }

    class TextExpression
    {
        public static void Main()
        {
            IExpression pi = new Constant(3.4);
            Console.WriteLine("Pi = {0}", pi.Eval());


            // Representing the tree on the left: 2*(3+4)
            IExpression threePlusFour = new Sum(new Constant(3), new Constant(4));
            Console.WriteLine("threePlusFour = {0}", threePlusFour.Eval());

            IExpression leftTree = new Product(new Constant(2), threePlusFour);
            Console.WriteLine("leftTree: {1} = {0}", leftTree.Eval(), leftTree.ToString());

            // Representing the tree on the right: 2*3+4
            IExpression twoTimesThree = new Product(new Constant(2), new Constant(3));
            Console.WriteLine("twoTimesThree = {0}", twoTimesThree.Eval());

            IExpression rightTree = new Sum(twoTimesThree, new Constant(4));
            Console.WriteLine("rightTree: {1} = {0}", rightTree.Eval(), rightTree.ToString());
        }
    }
}
