public class ShitAnimController : BaseAnimController
{
    public void PlayAnim(ShitState animState)
    {
        var stateStr = animState.ToString();

        _anim.Play(stateStr);
    }
}