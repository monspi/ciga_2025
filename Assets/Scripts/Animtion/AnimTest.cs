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
                shitAnimController.PlayAnim(FullNpcAnimState.Start);
            }
            else if (TryGetComponent<Npc1AnimController>(out var npc1AnimController))
            {
                npc1AnimController.PlayAnim(FullNpcAnimState.Speak);
            }
            else if (TryGetComponent<Npc4AnimController>(out var npc4AnimController))
            {
                npc4AnimController.PlayAnim(FullNpcAnimState.Idle);
            }
            else if (TryGetComponent<Npc5AnimController>(out var npc5AnimController))
            {
                npc5AnimController.PlayAnim(FullNpcAnimState.Idle);
            }
            else if (TryGetComponent<BossAnimController>(out var bossAnimController))
            {
                bossAnimController.PlayAnim(FullNpcAnimState.Speak);
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (TryGetComponent<ShitAnimController>(out var shitAnimController))
            {
                shitAnimController.PlayAnim(FullNpcAnimState.Die);
            }
            else if (TryGetComponent<Npc1AnimController>(out var npc1AnimController))
            {
                npc1AnimController.PlayAnim(FullNpcAnimState.Fart);
            }
            else if (TryGetComponent<Npc4AnimController>(out var npc4AnimController))
            {
                npc4AnimController.PlayAnim(FullNpcAnimState.Cry);
            }
            else if (TryGetComponent<Npc5AnimController>(out var npc5AnimController))
            {
                npc5AnimController.PlayAnim(FullNpcAnimState.Frown);
            }
            else if (TryGetComponent<BossAnimController>(out var bossAnimController))
            {
                bossAnimController.PlayAnim(FullNpcAnimState.Walk);
            }
        }
    }
}
