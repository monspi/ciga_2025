public class BossAnimController : BaseAnimController
{
    public void PlayAnim(BossState animState)
    {
        var stateStr = animState.ToString();

        _anim.Play(stateStr);
    }
}