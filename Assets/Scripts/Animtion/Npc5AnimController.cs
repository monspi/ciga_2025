public class Npc5AnimController : BaseAnimController
{
    public void PlayAnim(Npc5State animState)
    {
        var stateStr = animState.ToString();

        _anim.Play(stateStr);
    }
}