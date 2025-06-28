using static UnityEditor.VersionControl.Asset;

public class BossAnimController : BaseAnimController
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
        BossState state = (BossState)animState;
        PlayAnim(state.ToString());
    }
}