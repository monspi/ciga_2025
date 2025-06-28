public class Npc4AnimController : BaseAnimController
{
    public void PlayAnim(Npc4State animState)
    {
        var stateStr = animState.ToString();

        _anim.Play(stateStr);
    }
}