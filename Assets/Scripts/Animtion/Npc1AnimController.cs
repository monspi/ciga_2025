public class Npc1AnimController : BaseAnimController
{
    public void PlayAnim(Npc1State animState)
    {
        var stateStr = animState.ToString();

        _anim.Play(stateStr);
    }
}