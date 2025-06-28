public class Npc5AnimController : BaseAnimController
{
    public override void PlayAnim(string animName)
    {
        base.PlayAnim(animName);
        _anim.Play(animName);
    }

    public override void PlayAnim(FullNpcAnimState animState)
    {
        var stateStr = animState.ToString();
        PlayAnim(stateStr);
    }

    public override void PlayAnim(NpcAnimState animState)
    {
        Npc5State state = (Npc5State)animState;
        PlayAnim(state.ToString());
    }

}