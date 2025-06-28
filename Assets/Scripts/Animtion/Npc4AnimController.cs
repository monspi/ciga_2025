public class Npc4AnimController : BaseAnimController
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
        Npc4State state = (Npc4State)animState;
        PlayAnim(state.ToString());
    }
}