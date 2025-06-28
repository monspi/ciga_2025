
using System;
using UnityEngine;

namespace GameLogic.Battle
{
    public class NoteBehaviorGenerator : MonoBehaviour
    {
        public GameObject piClickPF;
        
        public Transform leftEmitTF;
        public Transform rightEmitTF;
        public Transform leftDetectTF;
        public Transform rightDetectTF;

        public NoteBehavior GenerateNoteBehavior(NoteEvent noteEvent, bool isLeft)
        {
            var go = Instantiate(piClickPF, transform);
            var behavior = go.GetComponent<NoteBehavior>();
            if (isLeft)
            {
                behavior.StartMove(leftEmitTF.position, leftDetectTF.position, BattleController.Inst.MoveDuration);
            }
            else
            {
                behavior.StartMove(rightEmitTF.position, rightDetectTF.position, BattleController.Inst.MoveDuration);
            }

            return behavior;
        }
    }
}