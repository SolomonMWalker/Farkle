using Godot;

public partial class TestDiceFace(DiceFaceValue diceFaceValue, Transform3D transform) : RootDiceFace(diceFaceValue, transform)
{
    public bool overridden = false;
    public int overrideNumber;

    public override int Number {
        get
        {
            return overridden ? overrideNumber : diceFaceValue.numberValue ?? 0;
        }
    }

    public void ToggleOverridden() => overridden = !overridden;
    public void Override(int num)
    {
        overrideNumber = num;
        overridden = true;
        SetLabelText(overrideNumber.ToString());
    }
}