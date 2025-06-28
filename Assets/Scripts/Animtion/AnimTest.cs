using System;
using UnityEngine;

public class AnimTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (TryGetComponent<ShitAnimController>(out var shitAnimController))
            {
                shitAnimController.PlayAnim(ShitState.Start);
            }
            else if (TryGetComponent<Npc1AnimController>(out var npc1AnimController))
            {
                npc1AnimController.PlayAnim(Npc1State.Speak);
            }
            else if (TryGetComponent<Npc4AnimController>(out var npc4AnimController))
            {
                npc4AnimController.PlayAnim(Npc4State.Idle);
            }
            else if (TryGetComponent<Npc5AnimController>(out var npc5AnimController))
            {
                npc5AnimController.PlayAnim(Npc5State.Idle);
            }
            else if (TryGetComponent<BossAnimController>(out var bossAnimController))
            {
                bossAnimController.PlayAnim(BossState.Speak);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (TryGetComponent<ShitAnimController>(out var shitAnimController))
            {
                shitAnimController.PlayAnim(ShitState.Die);
            }
            else if (TryGetComponent<Npc1AnimController>(out var npc1AnimController))
            {
                npc1AnimController.PlayAnim(Npc1State.Fart);
            }
            else if (TryGetComponent<Npc4AnimController>(out var npc4AnimController))
            {
                npc4AnimController.PlayAnim(Npc4State.Cry);
            }
            else if (TryGetComponent<Npc5AnimController>(out var npc5AnimController))
            {
                npc5AnimController.PlayAnim(Npc5State.Frown);
            }
            else if (TryGetComponent<BossAnimController>(out var bossAnimController))
            {
                bossAnimController.PlayAnim(BossState.Walk);
            }
        }
    }
}
