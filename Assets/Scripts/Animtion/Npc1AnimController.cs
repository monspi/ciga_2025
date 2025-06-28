public class Npc1AnimController : BaseAnimController
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
        Npc1State state = (Npc1State)animState;
        PlayAnim(state.ToString());
    }
}