using System.Numerics;

namespace AotForms
{
    internal class Entity
    {
 

        internal bool IsKnown;
        internal Bool3 IsTeam;
        internal Vector3 Head;
        internal Vector3 LeftWrist;
        internal Vector3 RightWrist;
        internal Vector3 Spine;
        internal Vector3 Root;
        internal Vector3 Hip;
        internal Vector3 RightCalf;
        internal Vector3 LeftCalf;
        internal Vector3 RightFoot;
        internal Vector3 LeftFoot;
        internal Vector3 RighFoot;
        internal Vector3 LeftHand;
        internal Vector3 LeftSholder;
        internal Vector3 RightSholder;
        internal Vector3 RightWristJoint;
        internal Vector3 LeftWristJoint;
        internal Vector3 RightElbow;
        internal Vector3 LeftElbow;
        internal short Health;
        internal bool IsDead;
        internal bool IsKnocked;
        internal string Name;
        internal float Distance;
        internal bool IsFiring;
        internal bool isVisible;
        // --- LINHAS ADICIONADAS DE VOLTA ---
        // Estas propriedades são necessárias para a animação da barra de vida.
        internal float VidaAnterior { get; set; }
        internal float TempoUltimoDano { get; set; }
    }
}
