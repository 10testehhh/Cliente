using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AotForms
{
    internal static class Core
    {
        public static uint GameInstance;
        internal static IntPtr Handle;
        internal static int Width = -1;
        internal static int Height = -1;
        internal static bool HaveMatrix = false;
        internal static Matrix4x4 CameraMatrix;
        internal static ulong LocalPlayer;
        internal static Vector3 LocalMainCamera;
        // Guarda o tempo atual do jogo, para ser usado em animações.
        internal static float CurrentTime;
        public static ConcurrentDictionary<long, Entity> Entities = new();
    }

}
