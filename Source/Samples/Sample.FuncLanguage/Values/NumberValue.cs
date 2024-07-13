namespace Silverfly.Sample.Func.Values;

public record NumberValue : Value
{
    public double Value { get; set; }
    public NumberValue(double value)
    {
        Value = value;

        Members.Define("'+", Add);
        Members.Define("'-", Sub);
        Members.Define("'*", Mul);
        Members.Define("'/", Div);
    }

    public override bool IsTruthy() => Value != 0;

    public override string ToString() => Value.ToString();

    private static Value Add(Value left, Value right) => ((NumberValue)left).Value + ((NumberValue)right).Value;
    private static Value Sub(Value[] args)
    {
        if (args.Length == 1)
        {
            return -((NumberValue)args[0]).Value;
        }

        return ((NumberValue)args[0]).Value - ((NumberValue)args[1]).Value;
    }

    private static Value Mul(Value left, Value right) => ((NumberValue)left).Value * ((NumberValue)right).Value;
    private static Value Div(Value left, Value right) => ((NumberValue)left).Value / ((NumberValue)right).Value;
}
